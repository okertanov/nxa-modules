using Neo;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using System;
using System.Collections.Generic;


namespace Nxa.Plugins
{
    public class NXABlockListener : Plugin, IPersistencePlugin
    {

        public override string Name => "NXABlockListener";
        public override string Description => "NXABlockListener informs abount new blocks";

        private RabbitMQ.RabbitMQ rabbitMQ;

        protected override void Configure()
        {
            Settings.Load(GetConfiguration());

            if (!Settings.Default.Active)
            {
                ConsoleWriter.WriteLine(string.Format("NXABlockListener inactive"));
                return;
            }

            rabbitMQ = new RabbitMQ.RabbitMQ(Settings.Default.RMQ);

            ConsoleWriter.WriteLine(String.Format("Load plugin NXABlockListener configuration; Network: {0};", Settings.Default.Network));
        }

        void IPersistencePlugin.OnCommit(NeoSystem system, Block block, DataCache snapshot)
        {
            if (!Settings.Default.Active) return;
            if (system.Settings.Network != Settings.Default.Network) return;

            rabbitMQ.send(block.ToJson(ProtocolSettings.Default).AsString());
            //ConsoleWriter.WriteLine(String.Format("Block hash: {0}; Block index: {1}; Block json: {2};", block.Hash, block.Index, block.ToJson(ProtocolSettings.Default).AsString()));
        }


        static string GetExceptionMessage(Exception exception)
        {
            return exception?.GetBaseException().Message;
        }

    }
}
