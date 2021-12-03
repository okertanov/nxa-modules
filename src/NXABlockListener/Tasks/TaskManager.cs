using Neo;
using Neo.Network.P2P.Payloads;
using Nxa.Plugins.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins.Tasks
{

    public class TaskManager : IDisposable
    {
        private class TaskListItem
        {
            public BlockListenerTask BlockListenerTask { get; set; }
            public Task Task { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }
        private Dictionary<Guid, TaskListItem> TaskList = new Dictionary<Guid, TaskListItem>();

        private readonly NeoSystem neoSystem;


        //block listener on or off
        public bool Active { get; private set; }
        public TaskManager(NeoSystem neoSystem)
        {
            this.neoSystem = neoSystem;
            if (Settings.Default.AutoStart)
            {
                Load();
            }
        }

        public void AddBlock(Block block)
        {
            BlockListenerTask.AddBlock(block);
        }

        public void StartBlockListener()
        {
            if (Active)
                return;
            Load();
        }

        public void StopBlockListener()
        {
            if (!Active)
                return;
            Stop();
        }

        public TaskObject StartTask(Guid guid)
        {
            if (!Active)
                return null;

            if (TaskList.ContainsKey(guid))
            {
                //task allready active
                return TaskList[guid].BlockListenerTask.GetTaskObject();
            }

            var taskObj = StorageManager.Manager.GetTaskObject(guid);
            if (taskObj != null)
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                BlockListenerTask blockListenerTask = new BlockListenerTask(taskObj, neoSystem, cancellationTokenSource.Token, cleanUpTask);
                TaskList[taskObj.Id] = new TaskListItem()
                {
                    CancellationTokenSource = cancellationTokenSource,
                    BlockListenerTask = blockListenerTask,
                    Task = Task.Run(() => blockListenerTask.CreateTask(), cancellationTokenSource.Token)
                };
                return taskObj;
            }
            else
            {
                //no such task exist
                return null;
            }
        }

        public bool CreateTask(TaskObject taskObj)
        {
            if (!Active)
                return false;

            if (TaskList.ContainsKey(taskObj.Id))
            {
                return false;
            }

            if (StorageManager.Manager.CheckIfKeyExist(taskObj))
            {
                return false;
            }

            //create task in db
            StorageManager.Manager.AddTaskObject(taskObj);

            //run task
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            BlockListenerTask blockListenerTask = new BlockListenerTask(taskObj, neoSystem, cancellationTokenSource.Token, cleanUpTask);
            TaskList[taskObj.Id] = new TaskListItem()
            {
                CancellationTokenSource = cancellationTokenSource,
                BlockListenerTask = blockListenerTask,
                Task = Task.Run(() => blockListenerTask.CreateTask(), cancellationTokenSource.Token)
            };
            return true;
        }

        public TaskObject StopTask(Guid guid)
        {
            if (!Active)
                return null;

            //set task status to canceled
            if (TaskList.ContainsKey(guid))
            {
                var taskObject = TaskList[guid].BlockListenerTask.GetTaskObject();
                taskObject.TaskState = TaskState.Canceled;
                TaskList[guid].CancellationTokenSource.Cancel();

                try
                {
                    Task.WaitAll(TaskList[guid].Task);
                }
                catch (Exception)
                {
                    //stopped
                }
                return taskObject;
            }
            return null;
        }

        private void Load()
        {
            //get active tasks
            var taskList = StorageManager.Manager.GetTasks(new TaskState[] { TaskState.Active }).ToList();

            //check for blocklistener tasks
            if (taskList.FirstOrDefault(x => x.TaskType == TaskType.BlockListener) == null)
            {
                //if none create one from config
                TaskObject taskObject = new TaskObject()
                {
                    ActiveBlock = Settings.Default.StartBlock,
                    TaskState = TaskState.None,
                    TaskType = TaskType.BlockListener,
                };
                taskList.Add(taskObject);
                StorageManager.Manager.AddTaskObject(taskObject);
            }

            //start tasks
            foreach (var taskObj in taskList)
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                BlockListenerTask blockListenerTask = new BlockListenerTask(taskObj, neoSystem, cancellationTokenSource.Token, cleanUpTask);
                TaskList[taskObj.Id] = new TaskListItem()
                {
                    CancellationTokenSource = cancellationTokenSource,
                    BlockListenerTask = blockListenerTask,
                    Task = Task.Run(() => blockListenerTask.CreateTask(), cancellationTokenSource.Token)
                };
            }
            Active = true;
        }

        private void Stop()
        {
            foreach (var task in TaskList)
            {
                task.Value.CancellationTokenSource.Cancel();
            }

            try
            {
                Task.WaitAll(TaskList.Select(x => x.Value.Task).ToArray());
            }
            catch (Exception)
            {
                //stopped
            }

            Active = false;
            Dispose();
        }

        private bool cleanUpTask(Guid guid)
        {
            if (TaskList.ContainsKey(guid))
            {
                TaskList[guid].CancellationTokenSource.Dispose();
                TaskList.Remove(guid);
            }
            return true;
        }

        #region Dispose

        private bool _disposedValue;

        ~TaskManager() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var task in TaskList)
                    {
                        task.Value.CancellationTokenSource.Dispose();
                    }
                }
                TaskList.Clear();
                _disposedValue = true;
            }
        }

        #endregion
    }
}
