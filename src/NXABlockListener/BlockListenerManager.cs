using Neo;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract.Native;
using Nxa.Plugins.Db;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    public class BlockListenerManager
    {
        private const int loadAmount = 1000;
        private static BlockingCollection<Block> incomingBlocks = new BlockingCollection<Block>();
        public BlockListenerManager(NeoSystem system)
        {
            setUpBlockListener(system);
        }
        public void AddBlock(Block block)
        {
            incomingBlocks.Add(block);
        }

        private void setUpBlockListener(NeoSystem system)
        {
            RabbitMQ.RabbitMQ rabbitMQ = new RabbitMQ.RabbitMQ();
            LevelDbManager levelDbManager = new LevelDbManager();

            uint rmqBlockIndex = levelDbManager.GetRMQBlockIndex();
            if (Settings.Default.StartBlock > rmqBlockIndex)
                rmqBlockIndex = Settings.Default.StartBlock;

            uint activeBlockIndex = rmqBlockIndex;
            using (SnapshotCache snapshot = system.GetSnapshot())
            {
                var currentBlockIndex = NativeContract.Ledger.CurrentIndex(snapshot);
                ConsoleWriter.WriteLine(String.Format("Load and send unsent blocs to RMQ. Load by {0} and send", loadAmount));
                ConsoleWriter.WriteLine(String.Format("RMQ block index {0} and current block index {1}", rmqBlockIndex, currentBlockIndex));

                int i = 0;
                List<Block> unsentBlocks = new List<Block>();
                while (activeBlockIndex <= currentBlockIndex)
                {
                    var block = NativeContract.Ledger.GetBlock(snapshot, activeBlockIndex);
                    unsentBlocks.Add(block);

                    i++;
                    if (i == loadAmount)
                    {
                        rmqBlockIndex = trySendRMQ(levelDbManager, rabbitMQ, rmqBlockIndex, unsentBlocks);

                        ConsoleWriter.WriteLine(String.Format("RMQ block index {0} and current block index {1}", rmqBlockIndex, currentBlockIndex));

                        i = 0;
                        unsentBlocks.Clear();
                    }
                    activeBlockIndex++;
                }
                if (unsentBlocks.Count > 0)
                {
                    rmqBlockIndex = trySendRMQ(levelDbManager, rabbitMQ, rmqBlockIndex, unsentBlocks);

                    ConsoleWriter.WriteLine(String.Format("RMQ block index {0} and current block index {1}", rmqBlockIndex, currentBlockIndex));
                    unsentBlocks.Clear();
                }

            }
            //start active rmq thread
            Task.Run(() => blockListner(levelDbManager, rabbitMQ, rmqBlockIndex));
        }

        private uint trySendRMQ(LevelDbManager levelDbManager, RabbitMQ.RabbitMQ rabbitMQ, uint rmqBlockIndex, List<Block> unsentBlocks)
        {
            ConsoleWriter.WriteLine(String.Format("Try sending {0} blocks to RMQ", unsentBlocks.Count));

            List<string> blocks = new List<string>();
            foreach (var block in unsentBlocks)
            {
                rmqBlockIndex = block.Index;
                blocks.Add(block.ToJson(ProtocolSettings.Default).AsString());
            }

            bool blocksSent = false;
            while (!blocksSent)
            {
                blocksSent = rabbitMQ.SendBatch(blocks);
                if (!blocksSent)
                {
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


        private void blockListner(LevelDbManager levelDbManager, RabbitMQ.RabbitMQ rabbitMQ, uint rmqBlockIndex)
        {
            while (true)
            {
                //take block
                var block = incomingBlocks.Take();

                bool blocksSent = false;
                while (!blocksSent)
                {
                    rmqBlockIndex = block.Index;
                    blocksSent = rabbitMQ.Send(block.ToJson(ProtocolSettings.Default).AsString());
                    if (!blocksSent)
                    {
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

    }
}
