using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.Wallets;
using Nxa.Plugins.HelperObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


            //KeyPair key = Utility.GetKeyPair("L3SRadqAWNhee9jMwa4a77eAR8vSvZ6xPHVTB4q2podYLwryRWN7");
            //OperationAccount account = new OperationAccount(key, system.Settings);
            //OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });


            //string c = "{\u0022type\u0022:\u0022Neo.Network.P2P.Payloads.Transaction\u0022,\u0022data\u0022:\u0022AFqNSUpI8y0AAAAAAIhREgAAAAAAdCICAAFlctTN0Uwtt6TulzCc0cTZhTGjQAEAOwsMFGVy1M3RTC23pO6XMJzRxNmFMaNAEsAfDAR2b3RlDBRfngMbwo5HEd8xgpE6lR45JRBOs0FifVtS\u0022,\u0022items\u0022:{\u00220x40a33185d9c4d19c3097eea4b72d4cd1cdd47265\u0022:{\u0022script\u0022:\u0022DCEDWZfqo2gsq0ovcBqQhauJGtl\\u002BhSsrowvbVxP\\u002BYoVmZNdBVuezJw ==\u0022,\u0022parameters\u0022:[{\u0022type\u0022:\u0022Signature\u0022,\u0022value\u0022:\u0022x28a / znb2DkODnZhjev2ix / vnmquAoVLYxbXm80k08 / rNxwf9D6XOGJQtQcd72Zcggnqqil24piD3g4d6KdaJg ==\u0022}],\u0022signatures\u0022:{\u0022035997eaa3682cab4a2f701a9085ab891ad97e852b2ba30bdb5713fe62856664d7\u0022:\u0022x28a / znb2DkODnZhjev2ix / vnmquAoVLYxbXm80k08 / rNxwf9D6XOGJQtQcd72Zcggnqqil24piD3g4d6KdaJg ==\u0022} } },\u0022network\u0022:199}";

            //ContractParametersContext context;
            //ContractParametersContext context2;
            //try
            //{
            //     context = ContractParametersContext.Parse(c.ToString(), system.StoreView);
            //    context2 = new ContractParametersContext(system.StoreView, tx, system.Settings.Network);

            //    if (!(context.Verifiable is Transaction tx2))
            //    {
            //        return new JObject();
            //    }
            //    tx2.Witnesses = context.GetWitnesses();

            //    Console.WriteLine($"Data relay success, the hash is shown as follows: {Environment.NewLine}{tx.Hash}");
            //}
            //catch (Exception e)
            //{

            //}

        }


    }
}
