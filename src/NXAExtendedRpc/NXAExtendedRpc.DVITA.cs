using Neo;
using Neo.IO.Json;
using Neo.Persistence;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Numerics;
using System.Security.Cryptography;

namespace Nxa.Plugins
{
    public partial class NXAExtendedRpc
    {

        [RpcMethod]
        protected virtual JObject GetBalance(JArray _params)
        {
            UInt160 address = _params[0].ToScriptHash(system.Settings);
            UInt160 asset_id;
            switch (_params[1].AsString())
            {
                case "DVITA":
                    asset_id = NativeContract.DVITA.Hash;
                    break;
                case "GAS":
                    asset_id = NativeContract.GAS.Hash;
                    break;
                default:
                    asset_id = UInt160.Parse(_params[1].AsString());
                    break;
            }

            using (var snapshot = system.GetSnapshot())
            {
                var balance = this.GetBalance(snapshot, asset_id, new UInt160[] { address });
                var result = new JObject();
                result["address"] = _params[0].AsString();
                result["token"] = _params[1].AsString();
                result["balance"] = balance.ToString();
                return result;
            }
        }

        [RpcMethod]
        protected virtual JObject NewAddress(JArray _params)
        {
            byte[] privateKey = new byte[32];
        generate:
            try
            {
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(privateKey);
                }
                return CreateAddress(privateKey);
            }
            catch (ArgumentException)
            {
                goto generate;
            }
            finally
            {
                Array.Clear(privateKey, 0, privateKey.Length);
            }
        }


        /// <summary>
        /// Get new address
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns>Address, public key, private key, script</returns>
        private JObject CreateAddress(byte[] privateKey)
        {
            if (privateKey is null) throw new ArgumentNullException(nameof(privateKey));
            KeyPair key = new(privateKey);
            if (key.PublicKey.IsInfinity) throw new ArgumentException(null, nameof(privateKey));

            var scriptHash = Contract.CreateSignatureRedeemScript(key.PublicKey).ToScriptHash();

            var result = new JObject();
            result["address"] = scriptHash.ToAddress(system.Settings.AddressVersion);
            result["pubkey"] = key.PublicKey.ToString();
            result["privkey"] = key.Export();
            result["scripthash"] = scriptHash.ToString();

            return result;
        }

        /// <summary>
        /// Gets the balance for the specified asset.
        /// </summary>
        /// <param name="snapshot">The snapshot used to read data.</param>
        /// <param name="asset_id">The id of the asset.</param>
        /// <param name="accounts">The accounts to be counted.</param>
        /// <returns>The balance for the specified asset.</returns>
        private BigDecimal GetBalance(DataCache snapshot, UInt160 asset_id, params UInt160[] accounts)
        {
            byte[] script;
            using (ScriptBuilder sb = new())
            {
                sb.EmitPush(0);
                foreach (UInt160 account in accounts)
                {
                    sb.EmitDynamicCall(asset_id, "balanceOf", CallFlags.ReadOnly, account);
                    sb.Emit(OpCode.ADD);
                }
                sb.EmitDynamicCall(asset_id, "decimals", CallFlags.ReadOnly);
                script = sb.ToArray();
            }
            using ApplicationEngine engine = ApplicationEngine.Run(script, snapshot, settings: system.Settings, gas: 0_60000000L * accounts.Length);
            if (engine.State == VMState.FAULT)
                return new BigDecimal(BigInteger.Zero, 0);
            byte decimals = (byte)engine.ResultStack.Pop().GetInteger();
            BigInteger amount = engine.ResultStack.Pop().GetInteger();
            return new BigDecimal(amount, decimals);
        }
    }
}
