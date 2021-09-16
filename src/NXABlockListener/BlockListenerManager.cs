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
        public bool Active => _active;
        private bool _active = false;

        private readonly TimeSpan _retryWaitTimeSec = new(0, 0, 5);
        private const int loadAmount = 1000;
        private static BlockingCollection<Block> incomingBlocks = new();
        private ConcurrentBag<Task> tasks;
        private CancellationTokenSource tokenSource;

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
            incomingBlocks.Add(block);
        }

        public void StartBlockListener()
        {
            if (_active)
                return;
            SetUpBlockListener();
        }

        public void StopBlockListener()
        {
            if (!_active)
                return;
            Stop();
        }

        private void SetUpBlockListener()
        {
            tokenSource = new CancellationTokenSource();
            tasks = new ConcurrentBag<Task>();

            rabbitMQ?.Dispose();
            levelDbManager?.Dispose();

            rabbitMQ = new();
            levelDbManager = new();

            incomingBlocks = new();

            tasks.Add(Task.Run(() => UnsentBlockProcessing(tokenSource.Token), tokenSource.Token));
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
            if (systemRef.TryGetTarget(out NeoSystem system))
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
                    ConsoleWriter.WriteLine($"Load and send unsent blocs to RMQ. Load by {loadAmount} and send");
                    ConsoleWriter.WriteLine($"RMQ block index {rmqBlockIndex} and current block index {currentBlockIndex}");

                    int i = 0;
                    List<Block> unsentBlocks = new();
                    while (activeBlockIndex < currentBlockIndex)
                    {
                        CheckDisposeCancellationToken(token);

                        var block = NativeContract.Ledger.GetBlock(snapshot, activeBlockIndex);
                        unsentBlocks.Add(block);

                        i++;
                        if (i == loadAmount)
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
            tasks.Add(Task.Run(() => BlockListner(token), token));
        }

        private void BlockListner(CancellationToken token)
        {
            while (true)
            {
                //CheckCancellationToken(token);
                //var block = incomingBlocks.Take(CancellationToken.None);

                Block block = null;
                try { block = incomingBlocks.Take(token); }
                catch { CheckDisposeCancellationToken(token); }

                bool blocksSent = false;
                while (!blocksSent)
                {
                    CheckDisposeCancellationToken(token);

                    uint rmqBlockIndex = block.Index;
                    blocksSent = rabbitMQ.Send(block.ToJson(ProtocolSettings.Default).AsString());
                    if (!blocksSent)
                    {
                        CheckDisposeCancellationToken(token);
                        ConsoleWriter.WriteLine("Failed to send blocks to RMQ. Wait 5sec and try again.");
                        Thread.Sleep(_retryWaitTimeSec);
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
                CheckDisposeCancellationToken(token);

                blocksSent = rabbitMQ.SendBatch(blocks);
                if (!blocksSent)
                {
                    CheckDisposeCancellationToken(token);

                    ConsoleWriter.WriteLine("Failed to send blocks to RMQ. Wait 5sec and try again.");
                    Thread.Sleep(_retryWaitTimeSec);
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
            _active = active;
            ConsoleWriter.UpdateBlockListenerState(active);
        }

        #region dispose
        public void Dispose()
        {
            Stop();
            //GC.SuppressFinalize(this);
        }
        private void Stop()
        {
            if (_active)
            {
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
        private void CheckDisposeCancellationToken(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                levelDbManager?.Dispose();
                rabbitMQ?.Dispose();
                incomingBlocks?.Dispose();
                token.ThrowIfCancellationRequested();
            }
        }
        #endregion
    }
}
