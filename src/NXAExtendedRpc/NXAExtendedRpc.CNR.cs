using Neo;
using Neo.IO.Json;
using Neo.Persistence;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.VM.Types;
using Neo.Wallets;
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
