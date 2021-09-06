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
            Settings.Load(GetConfiguration());

            if (!Settings.Default.Active)
            {
                Console.WriteLine(string.Format("NXABlockListener turned off. Check config file."));
                return;
            }
            Console.WriteLine(String.Format("Load plugin NXABlockListener configuration; Network: {0};", Settings.Default.Network));
        }

        protected override void OnSystemLoaded(NeoSystem system)
        {
            if (!Settings.Default.Active) return;
            if (system.Settings.Network != Settings.Default.Network) return;

            blockListenerManager = new BlockListenerManager(system);
        }

        void IPersistencePlugin.OnCommit(NeoSystem system, Block block, DataCache snapshot)
        {
            if (!Settings.Default.Active) return;
            if (system.Settings.Network != Settings.Default.Network) return;

            blockListenerManager.AddBlock(block);
        }

        [ConsoleCommand("stop blocklistener", Category = "BlockListener", Description = "Stop block listener service (NXABlockListener)")]
        public void StopBlockListener()
        {
            Console.WriteLine("Stopping block listener");
            blockListenerManager.StopBlockListener();
            Console.WriteLine("Block listener stopped");
        }

        [ConsoleCommand("start blocklistener", Category = "BlockListener", Description = "Stop block listener service (NXABlockListener)")]
        public void StartBlockListener()
        {
            if (!Settings.Default.Active)
            {
                Console.WriteLine("NXABlockListener turned off. Check config file.");
                return;
            }

            Console.WriteLine("Starting block listener");
            blockListenerManager.StartBlockListener();
            Console.WriteLine("Block listener started");
        }


        [ConsoleCommand("show blocklistener", Category = "BlockListener", Description = "Stop block listener console (NXABlockListener)")]
        public void ShowBlockListenerState()
        {
            Console.CursorVisible = false;
            Console.Clear();
            ConsoleWriter.StartWriting();
            if (!Settings.Default.Active)
            {
                ConsoleWriter.WriteLine(string.Format("NXABlockListener turned off. Check config file."));
            }
            else
            {
                ConsoleWriter.WriteLine(String.Format("Plugin NXABlockListener Active: {0}", blockListenerManager.Active));
                ReadLine();
            }
            ConsoleWriter.StopWriting();
            Console.WriteLine();
            Console.CursorVisible = true;
        }


        private readonly CancellationTokenSource _shutdownToken = new CancellationTokenSource();
        protected string ReadLine()
        {
            Task<string> readLineTask = Task.Run(() => Console.ReadLine());

            try
            {
                readLineTask.Wait(_shutdownToken.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            return readLineTask.Result;
        }

        #region dispose
        public override void Dispose()
        {
            _shutdownToken.Cancel();
            if (blockListenerManager != null)
                blockListenerManager.Dispose();
        }
        #endregion
    }
}
