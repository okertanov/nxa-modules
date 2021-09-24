using Neo;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract.Native;
using Nxa.Plugins.Db;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    public class BlockListenerManager : IDisposable
    {
        public bool Active { get; private set; }

        private const int LoadAmount = 1000;
        private static BlockingCollection<Block> IncomingBlocks = new();

        private readonly TimeSpan retryWaitTimeSec = new(0, 0, 5);
        private CancellationTokenSource tokenSource;
        private Task unsentBlocksTask;
        private Task blockListenerTask;

        private readonly WeakReference<NeoSystem> systemRef;
        private RabbitMQ.RabbitMQ rabbitMQ;
        private LevelDbManager levelDbManager;


        public BlockListenerManager(NeoSystem newSystem)
        {
            systemRef = new WeakReference<NeoSystem>(newSystem, false);
            if (Plugins.Settings.Default.AutoStart)
            {
                SetUpBlockListener();
            }
        }

        public static void AddBlock(Block block)
        {
            IncomingBlocks.Add(block);
        }

        public void StartBlockListener()
        {
            if (Active)
                return;
            SetUpBlockListener();
        }

        public void StopBlockListener()
        {
            if (!Active)
                return;
            Stop();
        }

        private void SetUpBlockListener()
        {
            tokenSource = new CancellationTokenSource();

            rabbitMQ?.Dispose();
            levelDbManager?.Dispose();

            rabbitMQ = new();
            levelDbManager = new();

            IncomingBlocks = new();

            unsentBlocksTask = Task.Run(() => UnsentBlockProcessing(tokenSource.Token), tokenSource.Token);
            SetBlockListenerState(true);
        }

        private void UnsentBlockProcessing(CancellationToken token)
        {
            uint rmqBlockIndex = levelDbManager.GetRMQBlockIndex();
            ConsoleWriter.UpdateRmqBlock(rmqBlockIndex);

            if (Settings.Default.StartBlock > rmqBlockIndex)
                rmqBlockIndex = Settings.Default.StartBlock;

            //send unsent blocks as batch
            uint activeBlockIndex = rmqBlockIndex;
            if (!systemRef.TryGetTarget(out NeoSystem system))
            {
                ConsoleWriter.WriteLine("Cannot sync unsent blocks. Cannot access NeoSystem");
                SetBlockListenerState(false);
                return;
            }

            using (SnapshotCache snapshot = system.GetSnapshot())
            {
                var currentBlockIndex = NativeContract.Ledger.CurrentIndex(snapshot);
                if (rmqBlockIndex < currentBlockIndex)
                {
                    ConsoleWriter.WriteLine($"Load and send unsent blocs to RMQ. Load by {LoadAmount} and send");
                    ConsoleWriter.WriteLine($"RMQ block index {rmqBlockIndex} and current block index {currentBlockIndex}");

                    int i = 0;
                    List<Block> unsentBlocks = new();
                    while (activeBlockIndex < currentBlockIndex)
                    {
                        CheckAndDisposeCancellationToken(token);

                        var block = NativeContract.Ledger.GetBlock(snapshot, activeBlockIndex);
                        unsentBlocks.Add(block);

                        i++;
                        if (i == LoadAmount)
                        {
                            rmqBlockIndex = TrySendRMQ(rmqBlockIndex, unsentBlocks, token);
                            ConsoleWriter.WriteLine($"RMQ block index {rmqBlockIndex} and current block index {currentBlockIndex}");

                            i = 0;
                            unsentBlocks.Clear();
                        }
                        activeBlockIndex++;
                    }
                    if (unsentBlocks.Count > 0)
                    {
                        rmqBlockIndex = TrySendRMQ(rmqBlockIndex, unsentBlocks, token);
                        ConsoleWriter.WriteLine($"RMQ block index {rmqBlockIndex} and current block index {currentBlockIndex}");
                        unsentBlocks.Clear();
                    }
                }
            }

            //start block listener
            blockListenerTask = Task.Run(() => BlockListner(token), token);
        }

        private void BlockListner(CancellationToken token)
        {
            while (true)
            {
                Block block = null;
                try { block = IncomingBlocks.Take(token); }
                catch { CheckAndDisposeCancellationToken(token); }

                bool blocksSent = false;
                while (!blocksSent)
                {
                    CheckAndDisposeCancellationToken(token);

                    uint rmqBlockIndex = block.Index;
                    blocksSent = rabbitMQ.Send(block.ToJson(ProtocolSettings.Default).AsString());
                    if (!blocksSent)
                    {
                        CheckAndDisposeCancellationToken(token);
                        ConsoleWriter.WriteLine("Failed to send blocks to RMQ. Wait 5sec and try again.");
                        Thread.Sleep(retryWaitTimeSec);
                    }
                    else
                    {
                        ConsoleWriter.WriteLine($"Successfully sent blocks to RMQ. Updating RMQ block index to {rmqBlockIndex}");
                        levelDbManager.SetRMQBlockIndex(rmqBlockIndex);
                        ConsoleWriter.UpdateRmqBlock(rmqBlockIndex);
                    }
                }
            }
        }

        private uint TrySendRMQ(uint rmqBlockIndex, List<Block> unsentBlocks, CancellationToken token)
        {
            ConsoleWriter.WriteLine($"Try sending {unsentBlocks.Count} blocks to RMQ");

            List<string> blocks = new();
            foreach (var block in unsentBlocks)
            {
                rmqBlockIndex = block.Index;
                blocks.Add(block.ToJson(ProtocolSettings.Default).AsString());
            }

            bool blocksSent = false;
            while (!blocksSent)
            {
                CheckAndDisposeCancellationToken(token);

                blocksSent = rabbitMQ.SendBatch(blocks);
                if (!blocksSent)
                {
                    CheckAndDisposeCancellationToken(token);

                    ConsoleWriter.WriteLine("Failed to send blocks to RMQ. Wait 5sec and try again.");
                    Thread.Sleep(retryWaitTimeSec);
                }
                else
                {
                    ConsoleWriter.WriteLine($"Successfully sent blocks to RMQ. Updating RMQ block index to {rmqBlockIndex}");
                    levelDbManager.SetRMQBlockIndex(rmqBlockIndex);
                    ConsoleWriter.UpdateRmqBlock(rmqBlockIndex);
                }
            }

            return rmqBlockIndex;
        }

        private void SetBlockListenerState(bool active)
        {
            Active = active;
            ConsoleWriter.UpdateBlockListenerState(active);
        }

        private void CheckAndDisposeCancellationToken(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                levelDbManager?.Dispose();
                rabbitMQ?.Dispose();
                IncomingBlocks?.Dispose();
                token.ThrowIfCancellationRequested();
            }
        }

        private void Stop()
        {
            if (Active)
            {
                List<Task> tasks = new List<Task>();
                if (unsentBlocksTask?.IsCompleted == false)
                {
                    tasks.Add(unsentBlocksTask);
                }
                if (blockListenerTask?.IsCompleted == false)
                {
                    tasks.Add(blockListenerTask);
                }

                tokenSource.Cancel();
                try
                {
                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception)
                {
                    ConsoleWriter.WriteLine("BlockListener stopped");
                }
                finally
                {
                    tokenSource.Dispose();
                }
                SetBlockListenerState(false);
            }
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

    }
}
