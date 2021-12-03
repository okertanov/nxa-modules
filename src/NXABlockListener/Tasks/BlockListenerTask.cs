using Neo;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract.Native;
using Nxa.Plugins.Pattern.Visitors;
using Nxa.Plugins.Persistence;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Nxa.Plugins.Tasks
{
    public class BlockListenerTask : IDisposable
    {
        private TaskObject taskObject;
        private readonly NeoSystem neoSystem;

        private readonly Guid taskId;
        private readonly CancellationToken cancellationToken;
        private readonly Func<Guid, bool> cleanUpTask;

        private readonly RabbitMQ.RabbitMQ rabbitMQ;
        private readonly Visitor visitor;

        private static ConcurrentDictionary<Guid, BlockingCollection<Block>> IncomingBlocks = new ConcurrentDictionary<Guid, BlockingCollection<Block>>();

        public BlockListenerTask(TaskObject taskObject, NeoSystem neoSystem, CancellationToken cancellationToken, Func<Guid, bool> cleanUpTask)
        {
            this.taskObject = taskObject;
            this.neoSystem = neoSystem;
            this.taskId = taskObject.Id;
            this.cancellationToken = cancellationToken;
            this.cleanUpTask = cleanUpTask;

            this.rabbitMQ = new RabbitMQ.RabbitMQ();
            this.visitor = new Visitor(this.rabbitMQ, neoSystem.Settings
                , taskObject.TaskType == TaskType.Search ? taskObject.Id.ToString() : ""
                , taskObject.TaskType == TaskType.Search ? taskObject.TaskParameters.SearchJSON : null);

        }

        public static void AddBlock(Block block)
        {
            foreach (var col in IncomingBlocks)
                col.Value.Add(block);
        }

        public void CreateTask()
        {
            //add blocking collection for new incomming blocks
            IncomingBlocks.TryAdd(this.taskId, new BlockingCollection<Block>());

            //update task state
            taskObject.TaskState = TaskState.Active;
            StorageManager.Manager.UpdateTaskState(taskObject);

            //create new queue in RabbitMQ for search task
            if (this.taskObject.TaskType == TaskType.Search)
            {
                this.rabbitMQ.DeclareExchangeQueue("", this.taskId.ToString());
            }

            SnapshotProcessing();
        }

        public TaskObject GetTaskObject()
        {
            return taskObject;
        }

        private void SnapshotProcessing()
        {
            uint activeBlockIndex = taskObject.ActiveBlock;

            using (SnapshotCache snapshot = neoSystem.GetSnapshot())
            {
                var currentBlockIndex = NativeContract.Ledger.CurrentIndex(snapshot);

                if (activeBlockIndex < currentBlockIndex)
                {
                    List<Block> unsentBlocks = new();
                    while (activeBlockIndex < currentBlockIndex)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            TaskCancelled();

                        var block = NativeContract.Ledger.GetBlock(snapshot, activeBlockIndex);
                        foreach (var item in visitor.Parse(block.ToJson(neoSystem.Settings)))
                        {
                            //token passed to visitable to cancel operation if rabbitMQ is dead
                            item.Accept(visitor, cancellationToken);
                        }

                        //update storage
                        taskObject.ActiveBlock = block.Index;
                        StorageManager.Manager.UpdateTaskActiveBlock(taskObject);

                        if (CheckEndTaskConditions(block.Index))
                        {
                            EndTask();
                            return;
                        }

                        activeBlockIndex++;
                    }

                }
            }
            NewBlockProcessing();
        }

        private void NewBlockProcessing()
        {
            while (true)
            {
                Block block = null;
                try
                {
                    block = IncomingBlocks[taskId].Take(cancellationToken);
                }
                catch
                {
                    if (cancellationToken.IsCancellationRequested)
                        TaskCancelled();
                }


                if (cancellationToken.IsCancellationRequested)
                    TaskCancelled();

                foreach (var item in visitor.Parse(block.ToJson(neoSystem.Settings)))
                {
                    //token passed to visitable to cancel operation if rabbitMQ is dead
                    item.Accept(visitor, cancellationToken);
                }

                //update storage
                taskObject.ActiveBlock = block.Index;
                StorageManager.Manager.UpdateTaskActiveBlock(taskObject);

                //check for end condition
                if (CheckEndTaskConditions(block.Index))
                {
                    EndTask();
                    return;
                }

            }
        }

        private bool CheckEndTaskConditions(uint blockIndex)
        {
            if (taskObject.TaskType != TaskType.BlockListener)
            {
                if (taskObject.TaskParameters.ToBlock <= blockIndex)
                    return true;


            }
            return false;
        }

        private void TaskCancelled()
        {
            if (taskObject.TaskState != TaskState.Canceled)
            {
                taskObject.TaskState = TaskState.Canceled;
            }
            StorageManager.Manager.UpdateTaskState(taskObject);

            //dispose then throw
            Dispose();
            cancellationToken.ThrowIfCancellationRequested();
        }

        private void EndTask()
        {
            //update task state in db
            taskObject.TaskState = TaskState.Finished;
            StorageManager.Manager.UpdateTaskState(taskObject);

            //send finish message for search task (So there is queue even if there is no messages)
            if (this.taskObject.TaskType == TaskType.Search)
            {
                this.rabbitMQ.Send("Finished", "", this.taskId.ToString());
            }

            Dispose();
        }


        #region Dispose

        private bool _disposedValue;

        ~BlockListenerTask() => Dispose(false);

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
                    IncomingBlocks.Remove(this.taskId, out BlockingCollection<Block> value);
                    value?.Dispose();
                    cleanUpTask(taskId);
                }
                _disposedValue = true;
            }
        }

        #endregion
    }
}
