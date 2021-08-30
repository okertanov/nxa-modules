using Neo;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using Nxa.Plugins.Db;
using System;
using System.Collections.Generic;


namespace Nxa.Plugins
{
    public class NXABlockListener : Plugin, IPersistencePlugin
    {

        public override string Name => "NXABlockListener";
        public override string Description => "NXABlockListener informs abount new blocks";

        private RabbitMQ.RabbitMQ rabbitMQ;
        private LevelDbManager levelDbManager;
        private BlockListenerManager blockListenerManager;

        protected override void Configure()
        {
            Settings.Load(GetConfiguration());

            if (!Settings.Default.Active)
            {
                ConsoleWriter.WriteLine(string.Format("NXABlockListener inactive"));
                return;
            }
                        ConsoleWriter.WriteLine(String.Format("Load plugin NXABlockListener configuration; Network: {0};", Settings.Default.Network));
        }

        protected override void OnSystemLoaded(NeoSystem system)
        {
            if (system.Settings.Network != Settings.Default.Network) return;
            blockListenerManager = new BlockListenerManager(system);
        }

        void IPersistencePlugin.OnCommit(NeoSystem system, Block block, DataCache snapshot)
        {
            if (!Settings.Default.Active) return;
            if (system.Settings.Network != Settings.Default.Network) return;

            blockListenerManager.AddBlock(block);
        }

    }
}
