using Neo;
using Neo.ConsoleService;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    public class NXABlockListener : Plugin, IPersistencePlugin
    {

        public override string Name => "NXABlockListener";
        public override string Description => "NXABlockListener informs abount new blocks";

        private BlockListenerManager blockListenerManager;

        protected override void Configure()
        {
            try
            {
                Settings.Load(GetConfiguration());
                if (!Settings.Default.Active)
                {
                    Console.Error.WriteLine("NXABlockListener turned off. Check config file.");
                    return;
                }
                Console.WriteLine($"Load plugin NXABlockListener configuration; Network: {Settings.Default.Network};");
            }
            catch (Exception e)
            {
                Settings.Load();
                Console.Error.WriteLine($"NXABlockListener configuration cannot be loaded. Error: {e.Message}");
            }
        }

        protected override void OnSystemLoaded(NeoSystem system)
        {
            if (!Settings.Default.Active) return;
            if (system.Settings.Network != Settings.Default.Network) return;

            try
            {
                blockListenerManager = new BlockListenerManager(system);
                Console.WriteLine("NXABlockListener loaded");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"NXABlockListener cannot load. Error: {e.Message}");
                blockListenerManager?.Dispose();
            }
        }

        void IPersistencePlugin.OnCommit(NeoSystem system, Block block, DataCache snapshot)
        {
            if (!Settings.Default.Active) return;
            if (system.Settings.Network != Settings.Default.Network) return;

            if (blockListenerManager != null && blockListenerManager.Active)
                BlockListenerManager.AddBlock(block);
        }


        [ConsoleCommand("stop blocklistener", Category = "BlockListener", Description = "Stop block listener service (NXABlockListener)")]
        public void StopBlockListener()
        {
            if (blockListenerManager != null)
            {
                Console.WriteLine("Stopping block listener");
                blockListenerManager.StopBlockListener();
            }
            Console.WriteLine("Block listener stopped");
        }


        [ConsoleCommand("start blocklistener", Category = "BlockListener", Description = "Start block listener service (NXABlockListener)")]
        public void StartBlockListener()
        {
            if (!Settings.Default.Active)
            {
                Console.WriteLine("NXABlockListener turned off. Check config file.");
                return;
            }

            if (blockListenerManager != null)
            {
                Console.WriteLine("Starting block listener");
                blockListenerManager.StartBlockListener();
                Console.WriteLine("Block listener started");
            }
            else
            {
                Console.WriteLine("NXABlockListener cannot start.");
            }
        }


        [ConsoleCommand("show blocklistener", Category = "BlockListener", Description = "Show blocklistener service state (NXABlockListener)")]
        public void ShowBlockListenerState()
        {
            try
            {
                ConsoleWriter.ShowState();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"NXABlockListener cannot show state. Error: {e.Message}");
            }
        }


        #region dispose
        public override void Dispose()
        {
            ConsoleWriter.Dispose();
            if (blockListenerManager != null)
                blockListenerManager.Dispose();
            //GC.SuppressFinalize(this);
        }
        #endregion
    }
}
