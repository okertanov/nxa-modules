using Neo;
using Neo.IO.Json;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.VM;
using Neo.VM.Types;
using Neo.Wallets;
using Nxa.Plugins.HelperObjects;
using System;
using Neo.Network.P2P.Payloads;
using System.Linq;
using Neo.Cryptography.ECC;

namespace Nxa.Plugins
{
    public partial class NXAExtendedRpc
    {
        private const string scriptHashStr = "NVhCWHzmB4pRKsLzaBSyU4uxddgsvUsX9V";

        /// <summary>
        /// Process "resolve" command
        /// </summary>
        [RpcMethod]
        protected virtual JObject Resolve(JArray _params)
        {
            var cname = _params[0].AsString();
            var result = new JObject();
            result["cname"] = cname;

            if (Utility.IsCNRAddress(cname))
            {
                var resolved = ResolveAddressByCNR(system, cname);
                var address = resolved?.ToAddress(system.Settings.AddressVersion) ?? "<unknown address>";
                result["address"] = address;
            }
            else
            {
                result["error"] = "Input value is invalid - it should be either email or alphanumeric value ending with .id.dvita.com";
            }
            return result;
        }

        /// <summary>
        /// Process "register" command
        /// </summary>
        [RpcMethod]
        protected virtual JObject Register(JArray _params)
        {
            var cname = _params[0].AsString();
            var address = _params[1].ToScriptHash(system.Settings);
            var privateKey = _params[2].AsString();
            var result = new JObject();
            result["cname"] = cname;
            result["address"] = address.ToString();

            if (Utility.IsCNRAddress(cname))
            {
                RegisterAddressByCNR(cname, address, privateKey, system, ref result);
            }
            else
            {
                result["error"] = "CNR name input is invalid - it should be either email or alphanumeric value ending with .id.dvita.com";
            }

            return result;
        }

        internal static bool RegisterAddressByCNR(string cname, UInt160 address, string signer, NeoSystem system, ref JObject result)
        {
            var keyPair = Utility.GetKeyPair(signer);
            var account = new OperationAccount(keyPair, system.Settings);
            var wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            try
            {
                byte[] script;
                using (var scriptBuilder = new ScriptBuilder())
                {
                    var scriptHash = scriptHashStr.ToScriptHash(system.Settings.AddressVersion);
                    scriptBuilder.EmitDynamicCall(scriptHash, "register", cname, address);
                    script = scriptBuilder.ToArray();
                }

                var sender = Contract.CreateSignatureRedeemScript(keyPair.PublicKey).ToScriptHash();
                var signers = new[] { new Signer { Scopes = WitnessScope.CalledByEntry, Account = sender } };

                var snapshot = system.StoreView;
                var tx = wallet.MakeTransaction(snapshot, script, sender, signers, maxGas: TestModeGas);

                var txHash = Operations.SignAndSendTx(system, snapshot, tx, wallet, true);
                result["txHash"] = txHash;

                return true;
            }
            catch (Exception e)
            {
                result["error"] = $"Error: {e.ToFlattenString()}: {e.StackTrace}";
            }

            return false;
        }

        /// <summary>
        /// Process "unregister" command
        /// </summary>
        [RpcMethod]
        protected virtual JObject Unregister(JArray _params)
        {
            var cname = _params[0].AsString();
            var privateKey = _params[1].AsString();
            var result = new JObject();
            result["cname"] = cname;

            if (Utility.IsCNRAddress(cname))
            {
                UnregisterAddressByCNR(cname, privateKey, system, ref result);
            }
            else
            {
                result["error"] = "CNR name input is invalid - it should be either email or alphanumeric value ending with .id.dvita.com";
            }

            return result;
        }

        internal static void UnregisterAddressByCNR(string cname, string signer, NeoSystem system, ref JObject result)
        {
            var keyPair = Utility.GetKeyPair(signer);
            var account = new OperationAccount(keyPair, system.Settings);
            var wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            try
            {
                byte[] script;
                using (var scriptBuilder = new ScriptBuilder())
                {
                    var scriptHash = scriptHashStr.ToScriptHash(system.Settings.AddressVersion);
                    scriptBuilder.EmitDynamicCall(scriptHash, "unregister", cname);
                    script = scriptBuilder.ToArray();
                }

                var sender = Contract.CreateSignatureRedeemScript(keyPair.PublicKey).ToScriptHash();
                var signers = new[] { new Signer { Scopes = WitnessScope.CalledByEntry, Account = sender } };

                var snapshot = system.StoreView;
                var tx = wallet.MakeTransaction(snapshot, script, sender, signers, maxGas: TestModeGas);
                var txHash = Operations.SignAndSendTx(system, snapshot, tx, wallet, true);
                result["txHash"] = txHash;
            }
            catch (Exception e)
            {
                result["error"] = $"Error: {e.ToFlattenString()}: {e.StackTrace}";
            }
        }

        private UInt160 ResolveAddressByCNR(NeoSystem neoSystem, string cname)
        {
            try
            {
                using (var scriptBuilder = new ScriptBuilder())
                {
                    var scriptHash = scriptHashStr.ToScriptHash(neoSystem.Settings.AddressVersion);
                    scriptBuilder.EmitDynamicCall(scriptHash, "resolve", cname);
                    var script = scriptBuilder.ToArray();
                    using (var engine = ApplicationEngine.Run(script, neoSystem.StoreView, container: null, settings: neoSystem.Settings, gas: TestModeGas))
                    {
                        var result = engine.State == VMState.FAULT ? null : engine.ResultStack.Peek();
                        if (result is not null && !result.IsNull)
                        {
                            var resultSpan = ((ByteString)result).GetSpan();
                            var resultScriptHash = new UInt160(resultSpan);
                            return resultScriptHash;
                        }
                        else
                        {
                            Console.Error.WriteLine($"CNR for: {cname} wrong result: {result}, state: {engine.State}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error: {e.ToFlattenString()}: {e.StackTrace}");
            }

            Console.Error.WriteLine($"CNR for: {cname} failed.");
            return null;
        }

        /// <summary>
        /// Create register transaction
        /// </summary>
        [RpcMethod]
        protected virtual JObject CreateRegisterTx(JArray _params)
        {
            var cname = _params[0].AsString();
            var address = _params[1].ToScriptHash(system.Settings);
            var publicKey = _params[2].AsString();

            var pubKey = ECPoint.Parse(publicKey, ECCurve.Secp256r1);
            var account = new OperationAccount(pubKey, system.Settings);
            var wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });
            byte[] script;

            using (var scriptBuilder = new ScriptBuilder())
            {
                var scriptHash = scriptHashStr.ToScriptHash(system.Settings.AddressVersion);
                scriptBuilder.EmitDynamicCall(scriptHash, "register", cname, address);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash);
        }

        /// <summary>
        /// Create unregister transaction
        /// </summary>
        [RpcMethod]
        protected virtual JObject CreateUnregisterTx(JArray _params)
        {
            var cname = _params[0].AsString();
            var publicKey = _params[1].AsString();

            var pubKey = ECPoint.Parse(publicKey, ECCurve.Secp256r1);
            var account = new OperationAccount(pubKey, system.Settings);
            var wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });
            byte[] script;

            using (var scriptBuilder = new ScriptBuilder())
            {
                var scriptHash = scriptHashStr.ToScriptHash(system.Settings.AddressVersion);
                scriptBuilder.EmitDynamicCall(scriptHash, "unregister", cname);
                script = scriptBuilder.ToArray();
            }

            return Operations.CreateTransaction(system: system, script: script, wallet: wallet, account: account.ScriptHash);
        }
    }
}
