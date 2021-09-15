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
        public bool Active { get { return _active; } }
        private bool _active = false;

        private const int loadAmount = 1000;
        private static BlockingCollection<Block> incomingBlocks = new();
        private ConcurrentBag<Task> tasks;
        private CancellationTokenSource tokenSource;

        private readonly NeoSystem system;
        private RabbitMQ.RabbitMQ rabbitMQ;
        private LevelDbManager levelDbManager;

        public BlockListenerManager(NeoSystem newSystem)
        {
            system = newSystem;
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

            if (rabbitMQ != null)
                rabbitMQ.Dispose();
            if (levelDbManager != null)
                levelDbManager.Dispose();

            rabbitMQ = new();
            levelDbManager = new();

            incomingBlocks = new();

            tasks.Add(Task.Run(() => UnsentBlockProcessing(tokenSource.Token), tokenSource.Token));
            _active = true;
        }


        private void UnsentBlockProcessing(CancellationToken token)
        {
            uint rmqBlockIndex = levelDbManager.GetRMQBlockIndex();
            if (Settings.Default.StartBlock > rmqBlockIndex)
                rmqBlockIndex = Settings.Default.StartBlock;

            //send unsent blocks as batch
            uint activeBlockIndex = rmqBlockIndex;
            using (SnapshotCache snapshot = system.GetSnapshot())
            {
                var currentBlockIndex = NativeContract.Ledger.CurrentIndex(snapshot);
                if (rmqBlockIndex < currentBlockIndex)
                {
                    ConsoleWriter.WriteLine(String.Format("Load and send unsent blocs to RMQ. Load by {0} and send", loadAmount));
                    ConsoleWriter.WriteLine(String.Format("RMQ block index {0} and current block index {1}", rmqBlockIndex, currentBlockIndex));

                    int i = 0;
                    List<Block> unsentBlocks = new();
                    while (activeBlockIndex < currentBlockIndex)
                    {
                        CheckCancellationToken(token);

                        var block = NativeContract.Ledger.GetBlock(snapshot, activeBlockIndex);
                        unsentBlocks.Add(block);

                        i++;
                        if (i == loadAmount)
                        {
                            rmqBlockIndex = TrySendRMQ(rmqBlockIndex, unsentBlocks, token);
                            ConsoleWriter.WriteLine(String.Format("RMQ block index {0} and current block index {1}", rmqBlockIndex, currentBlockIndex));

                            i = 0;
                            unsentBlocks.Clear();
                        }
                        activeBlockIndex++;
                    }
                    if (unsentBlocks.Count > 0)
                    {
                        rmqBlockIndex = TrySendRMQ( rmqBlockIndex, unsentBlocks, token);
                        ConsoleWriter.WriteLine(String.Format("RMQ block index {0} and current block index {1}", rmqBlockIndex, currentBlockIndex));
                        unsentBlocks.Clear();
                    }
                }
            }

            //start block listener
            tasks.Add(Task.Run(() => BlockListner(levelDbManager, rabbitMQ, token), token));
        }

        private void BlockListner(LevelDbManager levelDbManager, RabbitMQ.RabbitMQ rabbitMQ, CancellationToken token)
        {
            while (true)
            {
                CheckCancellationToken(token);

                var block = incomingBlocks.Take(CancellationToken.None);

                bool blocksSent = false;
                while (!blocksSent)
                {
                    CheckCancellationToken(token);

                    uint rmqBlockIndex = block.Index;
                    blocksSent = rabbitMQ.Send(block.ToJson(ProtocolSettings.Default).AsString());
                    if (!blocksSent)
                    {
                        CheckCancellationToken(token);
                        ConsoleWriter.WriteLine(String.Format("Failed to send blocks to RMQ. Wait 5sec and try again."));
                        Thread.Sleep(new TimeSpan(0, 0, 5));
                    }
                    else
                    {
                        ConsoleWriter.WriteLine(String.Format("Successfully sent blocks to RMQ. Updating RMQ block index to {0}", rmqBlockIndex));
                        levelDbManager.SetRMQBlockIndex(rmqBlockIndex);
                    }
                }
            }
        }

        private uint TrySendRMQ(uint rmqBlockIndex, List<Block> unsentBlocks, CancellationToken token)
        {
            ConsoleWriter.WriteLine(String.Format("Try sending {0} blocks to RMQ", unsentBlocks.Count));

            List<string> blocks = new();
            foreach (var block in unsentBlocks)
            {
                rmqBlockIndex = block.Index;
                blocks.Add(block.ToJson(ProtocolSettings.Default).AsString());
            }

            bool blocksSent = false;
            while (!blocksSent)
            {
                CheckCancellationToken( token);

                blocksSent = rabbitMQ.SendBatch(blocks);
                if (!blocksSent)
                {
                    CheckCancellationToken(token);

                    ConsoleWriter.WriteLine(String.Format("Failed to send blocks to RMQ. Wait 5sec and try again."));
                    Thread.Sleep(new TimeSpan(0, 0, 5));
                }
                else
                {
                    ConsoleWriter.WriteLine(String.Format("Successfully sent blocks to RMQ. Updating RMQ block index to {0}", rmqBlockIndex));
                    levelDbManager.SetRMQBlockIndex(rmqBlockIndex);
                }
            }

            return rmqBlockIndex;
        }

        #region dispose
        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
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
                _active = false;
            }
        }
        private void CheckCancellationToken(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                if (levelDbManager != null)
                    levelDbManager.Dispose();
                if (rabbitMQ != null)
                    rabbitMQ.Dispose();
                if (incomingBlocks != null)
                    incomingBlocks.Dispose();
                token.ThrowIfCancellationRequested();
            }
        }
        #endregion
    }
}
