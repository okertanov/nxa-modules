using Neo;
using Neo.ConsoleService;
using Neo.Cryptography.ECC;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.VM.Types;
using Neo.Wallets;
using Nxa.Plugins.HelperObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nxa.Plugins
{
    public partial class NXAExtendedRpc
    {

        [RpcMethod]
        protected virtual JObject RegisterCandidate(JArray _params)
        {
            string privateKey = _params[0].AsString();
            KeyPair key = new(OperationWallet.GetPrivateKeyFromWIF(privateKey));

            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            var testGas = NativeContract.DVITA.GetRegisterPrice(system.StoreView) + (BigInteger)Math.Pow(10, NativeContract.GAS.Decimals) * 10;

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "registerCandidate", key.PublicKey);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateSendTransaction(system, script, wallet, true, account.ScriptHash, (long)testGas);
        }

        [RpcMethod]
        protected virtual JObject UnregisterCandidate(JArray _params)
        {
            string privateKey = _params[0].AsString();
            KeyPair key = new(OperationWallet.GetPrivateKeyFromWIF(privateKey));

            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "unregisterCandidate", key.PublicKey);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateSendTransaction(system, script, wallet, true, account.ScriptHash);
        }

        [RpcMethod]
        protected virtual JObject Vote(JArray _params)
        {
            string privateKey = _params[0].AsString();
            KeyPair key = new(OperationWallet.GetPrivateKeyFromWIF(privateKey));

            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            string votePubKey = _params[1].AsString();

            if (!ECPoint.TryParse(votePubKey, ECCurve.Secp256r1, out ECPoint pubKey))
            {
                //error
            }

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "vote", account.ScriptHash, pubKey);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateSendTransaction(system, script, wallet, true, account.ScriptHash);
        }

        //[RpcMethod]
        //protected virtual JObject Vote2(JArray _params)
        //{
        //    string privateKey = _params[0].AsString();
        //    KeyPair key = new(OperationWallet.GetPrivateKeyFromWIF(privateKey));

        //    OperationAccount account = new OperationAccount(key, system.Settings);
        //    OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

        //    string votePubKey = _params[1].AsString();

        //    if (!ECPoint.TryParse(votePubKey, ECCurve.Secp256r1, out ECPoint pubKey))
        //    {
        //        //error
        //    }

        //    byte[] script;
        //    using (ScriptBuilder scriptBuilder = new ScriptBuilder())
        //    {
        //        scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "vote", account.ScriptHash, pubKey);
        //        script = scriptBuilder.ToArray();
        //    }

        //    return Operations.CreateSendTransaction(system: system, script: script, wallet: wallet, signAndSend: false, account: account.ScriptHash);
        //}


        [RpcMethod]
        protected virtual JObject Unvote(JArray _params)
        {
            string privateKey = _params[0].AsString();
            KeyPair key = new(OperationWallet.GetPrivateKeyFromWIF(privateKey));

            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "vote", account.ScriptHash, null);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateSendTransaction(system, script, wallet, true, account.ScriptHash);

        }




        [RpcMethod]
        protected virtual JObject GetAccountState(JArray _params)
        {
            string address = _params[0].AsString();

            string notice = "No vote record!";
            var arg = new JObject();
            arg["type"] = "Hash160";
            arg["value"] = address.ToScriptHash(system.Settings.AddressVersion).ToString();

            Operations.OnInvokeWithResult(system, NativeContract.DVITA.Hash, "getAccountState", out StackItem resultStack, null, new JArray(arg));
            //if (!Operations.OnInvokeWithResult(system, NativeContract.DVITA.Hash, "getAccountState", out StackItem result, null, new JArray(arg))) 
            //return new JObject();
            //Console.WriteLine();
            //if (result.IsNull)
            //{
            //    ConsoleHelper.Warning(notice);
            //    return new JObject();
            //}
            var resJArray = (Neo.VM.Types.Array)resultStack;
            foreach (StackItem value in resJArray)
            {
                if (value.IsNull)
                {
                    //ConsoleHelper.Warning(notice);
                    //return new JObject();
                    throw new RpcException(-500, notice);
                }
            }
            var publickey = ECPoint.Parse(((ByteString)resJArray?[2])?.GetSpan().ToHexString(), ECCurve.Secp256r1);
            //ConsoleHelper.Info("Voted: ", Contract.CreateSignatureRedeemScript(publickey).ToScriptHash().ToAddress(system.Settings.AddressVersion));
            //ConsoleHelper.Info("Amount: ", new BigDecimal(((Integer)resJArray?[0]).GetInteger(), NativeContract.DVITA.Decimals).ToString());
            //ConsoleHelper.Info("Block: ", ((Integer)resJArray?[1]).GetInteger().ToString());

            JObject result = new JObject();
            result["voted"] = Contract.CreateSignatureRedeemScript(publickey).ToScriptHash().ToAddress(system.Settings.AddressVersion);
            result["amount"] = new BigDecimal(((Integer)resJArray?[0]).GetInteger(), NativeContract.DVITA.Decimals).ToString();
            result["block"] = ((Integer)resJArray?[1]).GetInteger().ToString();

            return result;
        }

        [RpcMethod]
        protected virtual JObject GetCandidates(JArray _params)
        {
            Operations.OnInvokeWithResult(system, NativeContract.DVITA.Hash, "getCandidates", out StackItem resultStack, null, null, false);

            //if (!Operations.OnInvokeWithResult(system, NativeContract.DVITA.Hash, "getCandidates", out StackItem result, null, null, false)) return new JObject();

            var resJArray = (Neo.VM.Types.Array)resultStack;


            List<JString> candidates = new List<JString>();
            if (resJArray.Count > 0)
            {
                //Console.WriteLine();
                //ConsoleHelper.Info("Candidates:");

                foreach (var item in resJArray)
                {
                    var value = (Neo.VM.Types.Array)item;

                    candidates.Add((JString)(((ByteString)value?[0])?.GetSpan().ToHexString() + " : " + ((Integer)value?[1]).GetInteger()));
                    //Console.Write(((ByteString)value?[0])?.GetSpan().ToHexString() + "\t");
                    //Console.WriteLine(((Integer)value?[1]).GetInteger());
                }
            }
            JObject result = new JObject();
            result["candidates"] = candidates.ToArray();
            return result;
        }

        [RpcMethod]
        protected virtual JObject GetFundation(JArray _params)
        {
            JObject result = new JObject();
            result["fundation"] = system.Settings.StandbyValidators.Select(x => (JString)(x.ToString())).ToArray();
            return result;
        }

    }
}
