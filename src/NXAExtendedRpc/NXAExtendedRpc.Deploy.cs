using Neo.Cryptography.ECC;
using Neo.IO.Json;
using Neo.Plugins;
using Neo.SmartContract.Manifest;
using Neo.SmartContract.Native;
using Neo.VM;
using Nxa.Plugins.HelperObjects;
using System;
using System.Numerics;
using System.Text;

namespace Nxa.Plugins
{
    public partial class NXAExtendedRpc
    {



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
            if (manifestStr.IsBase64String())
            {
                manifestStr = Encoding.UTF8.GetString(Convert.FromBase64String(manifestStr));
            }

            var manifestJson = JObject.Parse(manifestStr);
            var manifest = ContractManifest.FromJson(manifestJson);

            return Operations.DeploySmartContract(system: system, wallet: wallet, keyPair: keyPair, nefImage: nefImage, manifest: manifest);
        }

        [RpcMethod]
        protected virtual JObject CreateDeployContract(JArray _params)
        {
            string publicKey = _params[0].AsString();
            ECPoint pubKey = ECPoint.Parse(publicKey, ECCurve.Secp256r1);

            OperationAccount account = new OperationAccount(pubKey, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            var nefImageBase64Str = _params[1].AsString();
            var nefImage = Convert.FromBase64String(nefImageBase64Str);

            var manifestStr = _params[2].AsString();
            if (manifestStr.IsBase64String())
            {
                manifestStr = Encoding.UTF8.GetString(Convert.FromBase64String(manifestStr));
            }

            var manifestJson = JObject.Parse(manifestStr);
            var manifest = ContractManifest.FromJson(manifestJson);

            return Operations.CreateDeploySmartContractTransaction(system: system, wallet: wallet, account: account.ScriptHash, nefImage: nefImage, manifest: manifest);

        }
    }
}
