using Neo;
using Neo.ConsoleService;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using Nxa.Plugins.Persistence;
using Nxa.Plugins.Tasks;
using System;

namespace Nxa.Plugins
{
    public partial class NXABlockListenerPlugin : Plugin, IPersistencePlugin
    {

        public override string Name => "NXABlockListener";
        public override string Description => "NXABlockListener informs abount new blocks";

        private TaskManager taskManager;
        protected override void Configure()
        {
            try
            {
                Settings.Load(GetConfiguration());
                if (!Settings.Default.TurnedOn)
                {
                    Console.Error.WriteLine("NXABlockListener turned off. Check config file.");
                    return;
                }
                StorageManager.Load();
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
            Settings.Default.ProtocolSettings = system.Settings;

            if (!Settings.Default.TurnedOn) return;
            if (system.Settings.Network != Settings.Default.Network) return;

            RpcServerPlugin.RegisterMethods(this, system.Settings.Network);

            try
            {
                taskManager = new TaskManager(system);
                ConsoleWriter.SetUpState(ref taskManager);
                Console.WriteLine("NXABlockListener loaded");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"NXABlockListener cannot load. Error: {e.Message}");
                taskManager?.Dispose();
            }

        }

        void IPersistencePlugin.OnCommit(NeoSystem system, Block block, DataCache snapshot)
        {
            if (!Settings.Default.TurnedOn) return;
            if (system.Settings.Network != Settings.Default.Network) return;

            if (taskManager != null)
                taskManager.AddBlock(block);
        }

        [ConsoleCommand("stop blocklistener", Category = "BlockListener", Description = "Stop block listener service (NXABlockListener)")]
        public void StopBlockListener()
        {
            if (taskManager != null)
            {
                Console.WriteLine("Stopping block listener");
                taskManager.StopBlockListener();
            }
            Console.WriteLine("Block listener stopped");
        }


        [ConsoleCommand("start blocklistener", Category = "BlockListener", Description = "Start block listener service (NXABlockListener)")]
        public void StartBlockListener()
        {
            if (!Settings.Default.TurnedOn)
            {
                Console.WriteLine("NXABlockListener turned off. Check config file.");
                return;
            }

            if (taskManager != null)
            {
                Console.WriteLine("Starting block listener");
                taskManager.StartBlockListener();
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

        public override void Dispose()
        {
            ConsoleWriter.Release();
            taskManager?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
