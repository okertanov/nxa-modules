using Neo;
using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Nxa.Plugins.Pattern
{
    public enum ParseType
    {
        Block,
        Transaction,
        Transfer,
        SCDeployment
    }
    public static class Utility
    {
        public static UInt160 ToScriptHash(this JObject value, ProtocolSettings protocolSettings = null)
        {
            var addressOrScriptHash = value.AsString();

            return addressOrScriptHash.Length < 40 ?
                addressOrScriptHash.ToScriptHash(protocolSettings == null ? Settings.Default.ProtocolSettings.AddressVersion : protocolSettings.AddressVersion)
                : UInt160.Parse(addressOrScriptHash);
        }
        public static Block BlockFromJson(JObject json, ProtocolSettings protocolSettings = null)
        {
            if (json["version"] == null)
                return null;
            if (json["previousblockhash"] == null)
                return null;
            if (json["merkleroot"] == null)
                return null;
            if (json["time"] == null)
                return null;
            if (json["nonce"] == null)
                return null;
            if (json["index"] == null)
                return null;
            if (json["primary"] == null)
                return null;
            if (json["nextconsensus"] == null)
                return null;
            if (json["witnesses"] == null)
                return null;

            return new Block()
            {
                Header = HeaderFromJson(json, protocolSettings),
                Transactions = ((JArray)json["tx"]).Select(p => TransactionFromJson(p, protocolSettings)).ToArray()
            };
        }
        public static Header HeaderFromJson(JObject json, ProtocolSettings protocolSettings = null)
        {
            return new Header
            {
                Version = (uint)json["version"].AsNumber(),
                PrevHash = UInt256.Parse(json["previousblockhash"].AsString()),
                MerkleRoot = UInt256.Parse(json["merkleroot"].AsString()),
                Timestamp = (ulong)json["time"].AsNumber(),
                Nonce = Convert.ToUInt64(json["nonce"].AsString(), 16),
                Index = (uint)json["index"].AsNumber(),
                PrimaryIndex = (byte)json["primary"].AsNumber(),
                NextConsensus = json["nextconsensus"].ToScriptHash(protocolSettings),
                Witness = ((JArray)json["witnesses"]).Select(p => WitnessFromJson(p)).FirstOrDefault()
            };
        }
        public static Transaction TransactionFromJson(JObject json, ProtocolSettings protocolSettings = null)
        {
            if (json["version"] == null)
                return null;
            if (json["nonce"] == null)
                return null;
            if (json["signers"] == null)
                return null;
            if (json["sysfee"] == null)
                return null;
            if (json["netfee"] == null)
                return null;
            if (json["validuntilblock"] == null)
                return null;
            if (json["attributes"] == null)
                return null;
            if (json["script"] == null)
                return null;
            if (json["witnesses"] == null)
                return null;

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
        public static Signer SignerFromJson(JObject json, ProtocolSettings protocolSettings = null)
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

        public static JObject ParseScript(byte[] array, ParseType? parseType = null)
        {
            JObject jObject = new JObject();
            JArray jArray = new JArray();
            for (int i = 0; i < array.Length; i++)
            {
                OpCode opCode = (OpCode)array[i];

                switch (opCode)
                {
                    case OpCode.NEWARRAY:
                        jObject["action"] = jArray;
                        continue;

                    case OpCode.PACK:
                        //remove last that is length
                        jArray.Remove(jArray.Last());
                        jObject["action"] = jArray;
                        continue;
                }

                i = array.ParseOpCode(opCode, i, out object result, parseType);

                if (jObject["action"] == null)
                {
                    jArray.Add(result == null ? null : result.ToString());
                    continue;
                }

                if (jObject["flag"] == null)
                {
                    jObject["flag"] = (CallFlags)(int)result;
                    continue;
                }

                if (jObject["method"] == null)
                {
                    jObject["method"] = result.ToString();
                    continue;
                }
                if (jObject["assetid"] == null)
                {
                    jObject["assetid"] = result.ToString();
                    continue;
                }
            }

            return jObject;
        }

        public static int ParseOpCode(this byte[] array, OpCode opCode, int index, out object result, ParseType? parseType = null)
        {
            result = null;
            int length;
            byte[] arr;
            switch (opCode)
            {
                #region Constants

                case OpCode.PUSHDATA1:
                    length = (int)array[index + 1];
                    index = index + 1;

                    arr = new byte[length];
                    System.Array.Copy(array, index + 1, arr, 0, length);
                    index = index + length;

                    result = ParseByteArray(arr);

                    break;
                case OpCode.PUSHDATA2:
                    length = BitConverter.ToInt16(array, index + 1);
                    index = index + 2;

                    arr = new byte[length];
                    System.Array.Copy(array, index + 1, arr, 0, length);
                    index = index + length;

                    result = ParseByteArray(arr);

                    break;
                case OpCode.PUSHDATA4:
                    length = BitConverter.ToInt32(array, index + 1);
                    index = index + 4;

                    arr = new byte[length];
                    System.Array.Copy(array, index + 1, arr, 0, length);
                    index = index + length;

                    result = ParseByteArray(arr);
                    break;

                case OpCode.PUSHNULL:
                    break;

                case OpCode.PUSHM1:
                    result = -1;
                    break;
                case OpCode.PUSH0:
                    result = 0;
                    break;
                case OpCode.PUSH1:
                    result = 1;
                    break;
                case OpCode.PUSH2:
                    result = 2;
                    break;
                case OpCode.PUSH3:
                    result = 3;
                    break;
                case OpCode.PUSH4:
                    result = 4;
                    break;
                case OpCode.PUSH5:
                    result = 5;
                    break;
                case OpCode.PUSH6:
                    result = 6;
                    break;
                case OpCode.PUSH7:
                    result = 7;
                    break;
                case OpCode.PUSH8:
                    result = 8;
                    break;
                case OpCode.PUSH9:
                    result = 9;
                    break;
                case OpCode.PUSH10:
                    result = 10;
                    break;
                case OpCode.PUSH11:
                    result = 11;
                    break;
                case OpCode.PUSH12:
                    result = 12;
                    break;
                case OpCode.PUSH13:
                    result = 13;
                    break;
                case OpCode.PUSH14:
                    result = 14;
                    break;
                case OpCode.PUSH15:
                    result = 15;
                    break;
                case OpCode.PUSH16:
                    result = 16;
                    break;

                case OpCode.PUSHINT8:
                    index = index + 1;

                    arr = new byte[1];
                    System.Array.Copy(array, index, arr, 0, 1);

                    result = new BigInteger(arr);
                    break;

                case OpCode.PUSHINT16:
                    index = index + 1;

                    arr = new byte[2];
                    System.Array.Copy(array, index, arr, 0, 2);
                    index = index + 1;

                    result = new BigInteger(arr);
                    break;

                case OpCode.PUSHINT32:
                    index = index + 1;

                    arr = new byte[4];
                    System.Array.Copy(array, index, arr, 0, 4);
                    index = index + 3;

                    result = new BigInteger(arr);
                    break;

                case OpCode.PUSHINT64:
                    index = index + 1;

                    arr = new byte[8];
                    System.Array.Copy(array, index, arr, 0, 8);
                    index = index + 7;

                    result = new BigInteger(arr);
                    break;

                case OpCode.PUSHINT128:
                    index = index + 1;

                    arr = new byte[16];
                    System.Array.Copy(array, index, arr, 0, 16);
                    index = index + 15;

                    result = new BigInteger(arr);
                    break;

                case OpCode.PUSHINT256:
                    index = index + 1;

                    arr = new byte[32];
                    System.Array.Copy(array, index, arr, 0, 32);
                    index = index + 31;

                    result = new BigInteger(arr);
                    break;

                #endregion

                #region Flow Control
                case OpCode.SYSCALL:
                    int maxLength = array.Length;
                    length = maxLength - 2 - index;

                    arr = new byte[length];
                    System.Array.Copy(array, index + 1, arr, 0, length);
                    index = index + length;

                    //result = ParseByteArray(arr);

                    break;

                case OpCode.JMPGT_L:
                    //index = index + 1;
                    result = opCode;
                    break;
                #endregion

                #region Logical operations
                case OpCode.OR:
                    //index = index + 1;  //int 2
                    result = opCode;
                    break;
                case OpCode.CLEAR:
                    //index = index + 1;
                    result = opCode;
                    break;

                #endregion

                #region Stack operations
                case OpCode.TUCK:
                    //index = index + 1;
                    result = opCode;
                    break;

                case OpCode.NIP:
                    //index = index + 1;
                    result = opCode;
                    break;

                #endregion

                #region Slot
                case OpCode.STARG:
                    //index = index + 1;
                    result = opCode;
                    break;

                case OpCode.STARG0:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STARG1:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STARG2:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STARG3:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STARG4:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STARG5:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STARG6:
                    //index = index + 1;
                    result = opCode;
                    break;


                case OpCode.STSFLD:
                    //index = index + 1;
                    result = opCode;
                    break;

                case OpCode.STSFLD0:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STSFLD1:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STSFLD2:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STSFLD3:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STSFLD4:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STSFLD5:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.STSFLD6:
                    //index = index + 1;
                    result = opCode;
                    break;


                #endregion


                #region Advanced data structures
                case OpCode.PACK:
                    result = opCode;
                    break;

                case OpCode.NEWARRAY:
                    result = opCode;
                    break;
                #endregion


                #region Type
                case OpCode.ISNULL:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.ISTYPE:
                    //index = index + 1;
                    result = opCode;
                    break;
                case OpCode.CONVERT:
                    //index = index + 1;
                    result = opCode;
                    break;

                #endregion

                case OpCode.ASSERT:
                    result = opCode;
                    break;

                case OpCode.ROLL:
                    result = opCode;
                    break;

                default:
                    result = null;
                    break;

            }
            return index;
        }

        public static object ParseByteArray(byte[] array, ParseType? parseType = null)
        {
            if (array.Length == 20)
            {
                try
                {
                    UInt160 uInt160 = new UInt160(array);
                    return uInt160;
                }
                catch
                {
                }
            }
            if (array.Length == 33)
            {
                try
                {
                    ECPoint pubKey = ECPoint.DecodePoint(array, ECCurve.Secp256r1);
                    return pubKey;
                }
                catch
                {
                }
            }

            if (!parseType.HasValue || parseType == ParseType.SCDeployment)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream(array))
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        NefFile nefFile = new NefFile();
                        nefFile.Deserialize(reader);
                        return nefFile.ToJson();
                    }
                }
                catch
                {
                }
            }
            return Encoding.UTF8.GetString(array);
        }

        public static bool HasValueDeep(JObject jObject, string property, JObject value)
        {
            if (jObject.ContainsProperty(property))
            {
                return jObject[property].AsString() == value.AsString();
            }
            else
            {
                foreach (var prop in jObject.Properties)
                {
                    if (HasValueDeep(prop.Value, property, value))
                        return true;
                }
            }

            return false;
        }

    }

    public class ParseNTF
    {
        //type

        //stage


    }
}
