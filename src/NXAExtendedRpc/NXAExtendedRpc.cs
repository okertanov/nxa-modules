using Neo;
using Neo.IO.Json;
using Neo.Plugins;

namespace Nxa.Plugins
{
    public partial class NXAExtendedRpc : Plugin
    {
        public override string Name => "ExtenedRpcServer";
        public override string Description => "Enables Extened RPC for the node";
        public const long TestModeGas = 20_00000000;

        private NeoSystem system;
        private Settings settings;


        protected override void Configure()
        {
            settings = new Settings(GetConfiguration());
        }

        protected override void OnSystemLoaded(NeoSystem system)
        {
            if (settings.Active)
            {
                this.system = system;
                RpcServerPlugin.RegisterMethods(this, settings.Network);
            }
        }

        [RpcMethod]
        protected virtual JObject HealthCheck(JArray _params)
        {
            JObject result = new JObject();
            result["success"] = true;
            return result;
        }

    }
}
