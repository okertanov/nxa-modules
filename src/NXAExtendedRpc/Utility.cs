using Neo;
using Neo.ConsoleService;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    static class Utility
    {
        public static string ToBase64String(this byte[] input) => System.Convert.ToBase64String(input);

        //rework to return JObject
        public static void PrintExecutionOutput(ApplicationEngine engine, bool showStack = true)
        {
            ConsoleHelper.Info("VM State: ", engine.State.ToString());
            ConsoleHelper.Info("Gas Consumed: ", new BigDecimal((BigInteger)engine.GasConsumed, NativeContract.GAS.Decimals).ToString());

            if (showStack)
                ConsoleHelper.Info("Result Stack: ", new JArray(engine.ResultStack.Select(p => p.ToJson())).ToString());

            if (engine.State == VMState.FAULT)
                ConsoleHelper.Error(GetExceptionMessage(engine.FaultException));
        }

        public static string GetExceptionMessage(Exception exception)
        {
            if (exception == null) return "Engine faulted.";

            if (exception.InnerException != null)
            {
                return GetExceptionMessage(exception.InnerException);
            }

            return exception.Message;
        }

        public static JObject GetExecutionOutput(ApplicationEngine engine, bool showStack = true, byte[] script = null)
        {
            JObject result = new JObject();
            result["vmstate"] = engine.State.ToString();
            result["gasconsumed"] = new BigDecimal((BigInteger)engine.GasConsumed, NativeContract.GAS.Decimals).ToString();

            if (showStack)
                result["resultstack"] = new JArray(engine.ResultStack.Select(p => p.ToJson())).ToString();

            if (engine.State == VMState.FAULT)
                result["error"] = GetExceptionMessage(engine.FaultException);

            if (script != null)
                result["script"] = script.ToBase64String();

            return result;
        }



        //public static JObject BlockToJson(Block block, ProtocolSettings settings)
        //{
        //    JObject json = block.ToJson(settings);
        //    json["tx"] = block.Transactions.Select(p => TransactionToJson(p, settings)).ToArray();
        //    return json;
        //}

        //public static JObject TransactionToJson(Transaction tx, ProtocolSettings settings)
        //{
        //    JObject json = tx.ToJson(settings);
        //    json["sysfee"] = tx.SystemFee.ToString();
        //    json["netfee"] = tx.NetworkFee.ToString();
        //    return json;
        //}

        //public static JObject NativeContractToJson(this NativeContract contract, ProtocolSettings settings)
        //{
        //    return new JObject
        //    {
        //        ["id"] = contract.Id,
        //        ["hash"] = contract.Hash.ToString(),
        //        ["nef"] = contract.Nef.ToJson(),
        //        ["manifest"] = contract.Manifest.ToJson(),
        //        ["updatehistory"] = settings.NativeUpdateHistory[contract.Name].Select(p => (JObject)p).ToArray()
        //    };
        //}
    }
}
