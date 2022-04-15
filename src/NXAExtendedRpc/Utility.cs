using Neo;
using Neo.ConsoleService;
using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
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

        public static KeyPair GetKeyPair(string key)
        {
            if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException(nameof(key)); }
            if (key.StartsWith("0x")) { key = key[2..]; }

            if (key.Length == 52)
            {
                return new KeyPair(Wallet.GetPrivateKeyFromWIF(key));
            }
            else if (key.Length == 64)
            {
                return new KeyPair(key.HexToBytes());
            }

            throw new FormatException();
        }

        public static JObject TransactionAndContextToJson(Transaction tx, ProtocolSettings protocolSettings, ContractParametersContext context = null)
        {
            JObject json = new JObject();
            json["tx"] = TransactionToJson(tx, protocolSettings);
            json["base64txjson"] = JsonToBase64(json["tx"]);
            if (context != null)
                json["context"] = context.ToJson();
            return json;
        }

        public static JObject TransactionToJson(Transaction tx, ProtocolSettings protocolSettings)
        {
            if (tx.Witnesses == null)
                tx.Witnesses = new Witness[] { };

            JObject json = tx.ToJson(protocolSettings);
            json["sysfee"] = tx.SystemFee.ToString();
            json["netfee"] = tx.NetworkFee.ToString();
            return json;
        }

        public static JString JsonToBase64(JObject tx)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(tx.AsString()));
        }

        public static Transaction TransactionFromJson(JObject json, ProtocolSettings protocolSettings)
        {
            return new Transaction
            {
                Version = byte.Parse(json["version"].AsString()),
                Nonce = uint.Parse(json["nonce"].AsString()),
                Signers = ((JArray)json["signers"]).Select(p => SignerFromJson(p, protocolSettings)).ToArray(),
                SystemFee = long.Parse(json["sysfee"].AsString()),
                NetworkFee = long.Parse(json["netfee"].AsString()),
                ValidUntilBlock = uint.Parse(json["validuntilblock"].AsString()),
                Attributes = ((JArray)json["attributes"]).Select(p => TransactionAttributeFromJson(p)).ToArray(),
                Script = Convert.FromBase64String(json["script"].AsString()),
                Witnesses = ((JArray)json["witnesses"]).Select(p => WitnessFromJson(p)).ToArray()
            };
        }

        public static Signer SignerFromJson(JObject json, ProtocolSettings protocolSettings)
        {
            return new Signer
            {
                Account = json["account"].ToScriptHash(protocolSettings),
                Scopes = (WitnessScope)Enum.Parse(typeof(WitnessScope), json["scopes"].AsString()),
                AllowedContracts = ((JArray)json["allowedcontracts"])?.Select(p => p.ToScriptHash(protocolSettings)).ToArray(),
                AllowedGroups = ((JArray)json["allowedgroups"])?.Select(p => ECPoint.Parse(p.AsString(), ECCurve.Secp256r1)).ToArray()
            };
        }

        public static TransactionAttribute TransactionAttributeFromJson(JObject json)
        {
            TransactionAttributeType usage = Enum.Parse<TransactionAttributeType>(json["type"].AsString());
            return usage switch
            {
                TransactionAttributeType.HighPriority => new HighPriorityAttribute(),
                TransactionAttributeType.OracleResponse => new OracleResponse()
                {
                    Id = (ulong)json["id"].AsNumber(),
                    Code = Enum.Parse<OracleResponseCode>(json["code"].AsString()),
                    Result = Convert.FromBase64String(json["result"].AsString()),
                },
                _ => throw new FormatException(),
            };
        }

        public static Witness WitnessFromJson(JObject json)
        {
            return new Witness
            {
                InvocationScript = Convert.FromBase64String(json["invocation"].AsString()),
                VerificationScript = Convert.FromBase64String(json["verification"].AsString())
            };
        }

        public static UInt160 ToScriptHash(this JObject value, ProtocolSettings protocolSettings)
        {
            var addressOrScriptHash = value.AsString();

            return addressOrScriptHash.Length < 40 ?
                addressOrScriptHash.ToScriptHash(protocolSettings.AddressVersion) : UInt160.Parse(addressOrScriptHash);
        }

        public static bool IsBase64String(this string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }

        public static bool IsCNRAddress(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            bool isCNRAddress =
                Regex.Match(input, @"^[a-zA-Z0-9_\.-]*\.(id.dvita.com)$").Success ||
                input.StartsWith('@') ||
                (input.Contains('@') && input.Contains('.'));

            return isCNRAddress;
        }

        public static IEnumerable<Exception> Flatten(this Exception ex)
        {
            var innerException = ex;
            do
            {
                yield return innerException;
                innerException = innerException.InnerException;
            }
            while (innerException != null);
        }

        public static string ToFlattenString(this Exception ex)
        {
            return String.Join(' ', ex.Flatten().Select(em => em.Message));
        }
    }
}
