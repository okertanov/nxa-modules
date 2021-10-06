using Neo;
using Neo.Cryptography.ECC;
using Neo.IO.Json;
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

            KeyPair key = Utility.GetKeyPair(privateKey);

            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            var testGas = NativeContract.DVITA.GetRegisterPrice(system.StoreView) + (BigInteger)Math.Pow(10, NativeContract.GAS.Decimals) * 10;

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "registerCandidate", key.PublicKey);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateSendTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash, gas: (long)testGas);
        }

        [RpcMethod]
        protected virtual JObject CreateRegisterCandidateTx(JArray _params)
        {
            string publicKey = _params[0].AsString();

            ECPoint pubKey = ECPoint.Parse(publicKey, ECCurve.Secp256r1);

            OperationAccount account = new OperationAccount(pubKey, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            var testGas = NativeContract.DVITA.GetRegisterPrice(system.StoreView) + (BigInteger)Math.Pow(10, NativeContract.GAS.Decimals) * 10;

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "registerCandidate", pubKey);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash, gas: (long)testGas);
        }

        [RpcMethod]
        protected virtual JObject UnregisterCandidate(JArray _params)
        {
            string privateKey = _params[0].AsString();

            KeyPair key = Utility.GetKeyPair(privateKey);

            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "unregisterCandidate", key.PublicKey);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateSendTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash);
        }

        [RpcMethod]
        protected virtual JObject CreateUnregisterCandidateTx(JArray _params)
        {
            string publicKey = _params[0].AsString();

            ECPoint pubKey = ECPoint.Parse(publicKey, ECCurve.Secp256r1);

            OperationAccount account = new OperationAccount(pubKey, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "unregisterCandidate", pubKey);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash);
        }


        [RpcMethod]
        protected virtual JObject Vote(JArray _params)
        {
            string privateKey = _params[0].AsString();
            string votePubKey = _params[1].AsString();

            KeyPair key = Utility.GetKeyPair(privateKey);
            ECPoint pubKey = ECPoint.Parse(votePubKey, ECCurve.Secp256r1);

            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "vote", account.ScriptHash, pubKey);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateSendTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash);
        }

        [RpcMethod]
        protected virtual JObject CreateVoteTx(JArray _params)
        {
            string publicKey = _params[0].AsString();
            string voteForPubKeyStr = _params[1].AsString();

            ECPoint pubKey = ECPoint.Parse(publicKey, ECCurve.Secp256r1);
            ECPoint voteForPubKey = ECPoint.Parse(voteForPubKeyStr, ECCurve.Secp256r1);

            OperationAccount account = new OperationAccount(pubKey, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "vote", account.ScriptHash, voteForPubKey);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash);
        }

        [RpcMethod]
        protected virtual JObject Unvote(JArray _params)
        {
            string privateKey = _params[0].AsString();

            KeyPair key = Utility.GetKeyPair(privateKey);

            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "vote", account.ScriptHash, null);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateSendTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash);
        }

        [RpcMethod]
        protected virtual JObject CreateUnvoteTx(JArray _params)
        {
            string publicKey = _params[0].AsString();

            ECPoint pubKey = ECPoint.Parse(publicKey, ECCurve.Secp256r1);

            OperationAccount account = new OperationAccount(pubKey, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.DVITA.Hash, "vote", account.ScriptHash, null);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash);

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

            var resJArray = (Neo.VM.Types.Array)resultStack;
            foreach (StackItem value in resJArray)
            {
                if (value.IsNull)
                {
                    throw new RpcException(-500, notice);
                }
            }
            var publickey = ECPoint.Parse(((ByteString)resJArray?[2])?.GetSpan().ToHexString(), ECCurve.Secp256r1);

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

            var resJArray = (Neo.VM.Types.Array)resultStack;

            JArray candidates = new JArray();
            if (resJArray.Count > 0)
            {
                foreach (var item in resJArray)
                {
                    var value = (Neo.VM.Types.Array)item;
                    JObject candidate = new JObject();
                    candidate["DVITA"] = ((Integer)value?[1]).GetInteger().ToString();
                    candidate["PubKey"] = (((ByteString)value?[0])?.GetSpan().ToHexString()).ToString();
                    candidates.Add(candidate);

                }
            }
            JObject result = new JObject();
            result["candidates"] = candidates;
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
