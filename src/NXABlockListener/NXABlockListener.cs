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
        public override string Description => "Test: Informs abount new blocks";

        protected override void Configure()
        {
            Settings.Load(GetConfiguration());
            ConsoleWriter.WriteLine(String.Format("NXABlockListener configuration; Network: {0}; Test: {1}", Settings.Default.Network, Settings.Default.Test));
        }

        void IPersistencePlugin.OnPersist(NeoSystem system, Block block, DataCache snapshot, IReadOnlyList<Blockchain.ApplicationExecuted> applicationExecutedList)
        {
            if (system.Settings.Network != Settings.Default.Network) return;

            ConsoleWriter.WriteLine(String.Format("Block hash: {0}; Block index: {1}; Block json: {2};", block.Hash, block.Index, block.ToJson(ProtocolSettings.Default).AsString()));
        }

        void IPersistencePlugin.OnCommit(NeoSystem system, Block block, DataCache snapshot)
        {
            if (system.Settings.Network != Settings.Default.Network) return;

            ConsoleWriter.WriteLine(String.Format("Block hash: {0}; Block index: {1}; Block json: {2};", block.Hash, block.Index, block.ToJson(ProtocolSettings.Default).AsString()));
        }

        static string GetExceptionMessage(Exception exception)
        {
            return exception?.GetBaseException().Message;
        }
    }
}
