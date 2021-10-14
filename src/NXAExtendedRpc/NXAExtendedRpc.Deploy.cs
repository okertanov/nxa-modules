using Neo.IO;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.Plugins;
using Neo.Wallets;
using Nxa.Plugins.HelperObjects;
using System;
using System.Text;

namespace Nxa.Plugins {
    public partial class NXAExtendedRpc {
        [RpcMethod]
        protected virtual JObject DeployContract(JArray _params)
        {
            var privateKey = _params[0].AsString();
            var keyPair = Utility.GetKeyPair(privateKey);

            var account = new OperationAccount(keyPair, system.Settings);
            var wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            var nefImageBase64Str = _params[1].AsString();
            var nefImage = Convert.FromBase64String(nefImageBase64Str);

            var manifestStr = _params[2].AsString();
            if (manifestStr.IsBase64String()) {
                manifestStr = Encoding.UTF8.GetString(Convert.FromBase64String(manifestStr));
            }

            var manifestJson = JObject.Parse(manifestStr);

            return Operations.DeploySmartContract(system: system, wallet: wallet, nefImage: nefImage, manifest: manifestJson);
        }
    }
}
