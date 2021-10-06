using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.Plugins;
using Neo.Wallets;
using Nxa.Plugins.HelperObjects;

namespace Nxa.Plugins
{
    public partial class NXAExtendedRpc
    {

        [RpcMethod]
        protected virtual JObject SignTx(JArray _params)
        {
            string privateKey = _params[0].AsString();
            string transactionString = _params[1].AsString();

            KeyPair key = Utility.GetKeyPair(privateKey);
            JObject transactionJson = JObject.Parse(transactionString);
            Transaction tx = Utility.TransactionFromJson(transactionJson, system.Settings);

            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            return Operations.SignAndSendTx(system: system, snapshot: system.StoreView, tx: tx, wallet: wallet, send: false);

        }

        [RpcMethod]
        protected virtual JObject RelayTx(JArray _params)
        {
            string transactionString = _params[0].AsString();
            JObject transactionJson = JObject.Parse(transactionString);
            Transaction tx = Utility.TransactionFromJson(transactionJson, system.Settings);

            return Operations.SignAndSendTx(system: system, snapshot: system.StoreView, tx: tx, wallet: null, send: true);

        }


    }
}
