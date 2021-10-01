using Neo;
using Neo.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    public class NXAExtendedRpcPlugin : Plugin
    {
        public override string Name => "ExtenedRpcServer";
        public override string Description => "Enables Extened RPC for the node";


        private Settings settings;
        private static readonly Dictionary<uint, NXAExtendedRpc> servers = new();
        private static readonly Dictionary<uint, List<object>> handlers = new();

        protected override void Configure()
        {
            settings = new Settings(GetConfiguration());
        }

        public override void Dispose()
        {
            foreach (var (_, server) in servers)
                server.Dispose();
            base.Dispose();
        }

        protected override void OnSystemLoaded(NeoSystem system)
        {
            if (settings.Active)
            {
                RpcServerSettings s = settings.Servers.FirstOrDefault(p => p.Network == system.Settings.Network);
                if (s is null) return;

                NXAExtendedRpc server = new(system, s);

                if (handlers.Remove(s.Network, out var list))
                {
                    foreach (var handler in list)
                    {
                        server.RegisterMethods(handler);
                    }
                }

                server.StartRpcServer();
                servers.TryAdd(s.Network, server);
            }
        }

        public static void RegisterMethods(object handler, uint network)
        {
            if (servers.TryGetValue(network, out NXAExtendedRpc server))
            {
                server.RegisterMethods(handler);
                return;
            }
            if (!handlers.TryGetValue(network, out var list))
            {
                list = new List<object>();
                handlers.Add(network, list);
            }
            list.Add(handler);
        }

    }
}
