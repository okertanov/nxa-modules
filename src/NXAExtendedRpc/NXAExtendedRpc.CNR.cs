﻿using Neo;
using Neo.IO.Json;
using Neo.Persistence;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.VM.Types;
using Neo.Wallets;
using Nxa.Plugins.HelperObjects;
using System;
using System.Numerics;
using System.Security.Cryptography;

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
            string cname = _params[0].ToString();
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
            string cname = _params[0].ToString();
            UInt160 address = _params[1].ToScriptHash(system.Settings);
            string privateKey = _params[2].AsString();
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
            KeyPair key = Utility.GetKeyPair(signer);
            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            try
            {
                byte[] script;
                using (ScriptBuilder scriptBuilder = new ScriptBuilder())
                {
                    var scriptHash = scriptHashStr.ToScriptHash(system.Settings.AddressVersion);
                    scriptBuilder.EmitDynamicCall(scriptHash, "register", cname, address);
                    script = scriptBuilder.ToArray();
                }
                var transactionResult= Operations.CreateSendTransaction(system: system, script: script, wallet: wallet, account: key.PublicKeyHash);
                result["result"] = transactionResult;
                return true;
            }
            catch (Exception e)
            {
                result["error"] = $"Error: {e.Message}: {e.StackTrace}.";
            }

            return false;
        }

        /// <summary>
        /// Process "unregister" command
        /// </summary>
        [RpcMethod]
        protected virtual JObject Unregister(JArray _params)
        {
            string cname = _params[0].ToString();
            string privateKey = _params[1].AsString();
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

        internal static bool UnregisterAddressByCNR(string cname, string signer, NeoSystem system, ref JObject result)
        {
            KeyPair key = Utility.GetKeyPair(signer);
            OperationAccount account = new OperationAccount(key, system.Settings);
            OperationWallet wallet = new OperationWallet(system.Settings, new OperationAccount[] { account });

            try
            {
                byte[] script;
                using (ScriptBuilder scriptBuilder = new ScriptBuilder())
                {
                    var scriptHash = scriptHashStr.ToScriptHash(system.Settings.AddressVersion);
                    scriptBuilder.EmitDynamicCall(scriptHash, "unregister", cname);
                    script = scriptBuilder.ToArray();
                }
                var transactionResult = Operations.CreateSendTransaction(system: system, script: script, wallet: wallet, account: key.PublicKeyHash);
                result["result"] = transactionResult;
                return true;
            }
            catch (Exception e)
            {
                result["error"] = $"Error: {e.Message}: {e.StackTrace}.";
            }

            return false;
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
                            Console.WriteLine($"CNR for: {cname} is RESULT: {result.Type} {resultScriptHash}");
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
                Console.Error.WriteLine($"Error: {e.Message}: {e.StackTrace}.");
            }

            Console.Error.WriteLine($"CNR for: {cname} failed.");
            return null;
        }
    }
}
