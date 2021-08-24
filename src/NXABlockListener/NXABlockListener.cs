using Neo;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using System;
using System.Collections.Generic;


namespace NXABlockListener
{
    public class NXABlockListener : Plugin, IPersistencePlugin
    {

        public override string Name => "NXABlockListener";
        public override string Description => "Test: Informs abount new blocks";

        protected override void Configure()
        {
            Settings.Load(GetConfiguration());
            System.Console.WriteLine(String.Format("NXABlockListener configuration; Network: {0}; Test: {1}", Settings.Default.Network, Settings.Default.Test));
        }

        void IPersistencePlugin.OnPersist(NeoSystem system, Block block, DataCache snapshot, IReadOnlyList<Blockchain.ApplicationExecuted> applicationExecutedList)
        {
            System.Console.WriteLine(String.Format("OnPersist block (System network: {0};)", system.Settings.Network));
            if (system.Settings.Network != Settings.Default.Network) return;

            System.Console.WriteLine(String.Format("Block hash: {0}; Block index: {1}; Block json: {2};", block.Hash, block.Index, block.ToJson(ProtocolSettings.Default).AsString()));
        }

        void IPersistencePlugin.OnCommit(NeoSystem system, Block block, DataCache snapshot)
        {
            System.Console.WriteLine(String.Format("OnCommit block (System network: {0};)", system.Settings.Network));
            if (system.Settings.Network != Settings.Default.Network) return;

            System.Console.WriteLine(String.Format("Block hash: {0}; Block index: {1}; Block json: {2};", block.Hash, block.Index, block.ToJson(ProtocolSettings.Default).AsString()));
        }

        static string GetExceptionMessage(Exception exception)
        {
            return exception?.GetBaseException().Message;
        }
    }
}
