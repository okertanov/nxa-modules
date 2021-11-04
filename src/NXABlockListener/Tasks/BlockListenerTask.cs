using Neo;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract.Native;
using Nxa.Plugins.Pattern.Visitors;
using Nxa.Plugins.Persistence;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins.Tasks
{
    public class BlockListenerTask : IDisposable
    {
        private TaskObject taskObject;
        private readonly NeoSystem neoSystem;
        private readonly Guid taskId;
        private readonly Visitor visitor;
        private readonly CancellationToken cancellationToken;

        private static ConcurrentDictionary<Guid, BlockingCollection<Block>> IncomingBlocks = new ConcurrentDictionary<Guid, BlockingCollection<Block>>();

        public BlockListenerTask(TaskObject taskObject, NeoSystem neoSystem, CancellationToken cancellationToken)
        {
            this.taskObject = taskObject;
            this.neoSystem = neoSystem;
            this.taskId = taskObject.Id;
            this.cancellationToken = cancellationToken;

            this.visitor = new Visitor(new RabbitMQ.RabbitMQ(), neoSystem.Settings
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
            if (taskObject.TaskState != TaskState.Active)
            {
                taskObject.TaskState = TaskState.Active;
                StorageManager.Manager.UpdateTaskState(taskObject);
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
                    int i = 0;
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
            cancellationToken.ThrowIfCancellationRequested();
        }

        private void EndTask()
        {
            //update task state in db
            taskObject.TaskState = TaskState.Finished;
            StorageManager.Manager.UpdateTaskState(taskObject);

        }

        public void Dispose()
        {
            //remove blocking collection from concurency dictionary
            IncomingBlocks.Remove(this.taskId, out BlockingCollection<Block> value);
            value?.Dispose();

            throw new NotImplementedException();
        }
    }
}
