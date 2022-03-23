using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Akka.Actor;
using Neo;
using Neo.IO;
using Neo.IO.Json;
using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.SmartContract.Manifest;
using Neo.VM;
using Neo.VM.Types;
using Neo.Wallets;
using Nxa.Plugins.HelperObjects;

namespace Nxa.Plugins
{
    internal static class Operations
    {
        public const long TestModeGas = 20_00000000;

        /// <summary>
        /// Make and send transaction with script, sender
        /// </summary>
        /// <param name="system">NeoSystem</param>
        /// <param name="script">script</param>
        /// <param name="account">sender</param>
        /// <param name="gas">Max fee for running the script</param>
        public static JObject CreateSendTransaction(NeoSystem system, byte[] script, OperationWallet wallet, UInt160 account = null, long gas = TestModeGas)
        {
            var signers = System.Array.Empty<Signer>();
            var snapshot = system.StoreView;

            if (account != null)
            {
                signers = wallet.GetAccounts()
                .Where(p => !p.Lock && !p.WatchOnly && p.ScriptHash == account && NativeContract.GAS.BalanceOf(snapshot, p.ScriptHash).Sign > 0)
                .Select(p => new Signer() { Account = p.ScriptHash, Scopes = WitnessScope.CalledByEntry })
                .ToArray();
            }

            try
            {
                var tx = wallet.MakeTransaction(snapshot, script, account, signers, maxGas: gas);

                using (var engine = ApplicationEngine.Run(tx.Script, snapshot, container: tx, settings: system.Settings, gas: gas))
                {
                    if (engine.State == VMState.FAULT)
                    {
                        throw new RpcException(-500, Utility.GetExecutionOutput(engine, true, tx.Script).AsString());
                    }
                }
                return SignAndSendTx(system, system.StoreView, tx, wallet, true);
            }
            catch (InvalidOperationException e)
            {
                //-300 should be 'Insufficient funds'
                throw new RpcException(-500, Utility.GetExceptionMessage(e));
            }

        }

        /// <summary>
        /// Make transaction with script, sender
        /// </summary>
        /// <param name="system">NeoSystem</param>
        /// <param name="script">script</param>
        /// <param name="account">sender</param>
        /// <param name="gas">Max fee for running the script</param>
        public static JObject CreateTransaction(NeoSystem system, byte[] script, OperationWallet wallet, UInt160 account = null, long gas = TestModeGas)
        {
            var signers = System.Array.Empty<Signer>();
            var snapshot = system.StoreView;

            if (account != null)
            {
                signers = wallet.GetAccounts()
                .Where(p => !p.Lock && !p.WatchOnly && p.ScriptHash == account && NativeContract.GAS.BalanceOf(snapshot, p.ScriptHash).Sign > 0)
                .Select(p => new Signer() { Account = p.ScriptHash, Scopes = WitnessScope.CalledByEntry })
                .ToArray();
            }

            try
            {
                var tx = wallet.MakeTransaction(snapshot, script, account, signers, maxGas: gas);

                return Utility.TransactionAndContextToJson(tx, system.Settings);
            }
            catch (InvalidOperationException e)
            {
                //-300 should be 'Insufficient funds'
                throw new RpcException(-500, Utility.GetExceptionMessage(e));
            }
        }

        /// <summary>
        /// Calculates the network fee for the specified transaction.
        /// </summary>
        /// <param name="system">NeoSystem</param>
        /// <param name="snapshot">The snapshot used to read data.</param>
        /// <param name="tx">The transaction to calculate.</param>
        /// <returns>The network fee of the transaction.</returns>
        public static JObject SignAndSendTx(NeoSystem system, DataCache snapshot, Transaction tx, OperationWallet wallet, bool send = false)
        {
            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(snapshot, tx, system.Settings.Network);

                //if (tx.Witnesses != null && tx.Witnesses.Length > 0)
                //{
                // missing contextItem => myby can create and add 
                //    //context.AddSignature(context,tx.Witnesses[0], tx.Signers)
                //}
            }
            catch (InvalidOperationException e)
            {
                throw new RpcException(-500, "Failed creating contract params: " + Utility.GetExceptionMessage(e));
            }

            if (wallet != null)
            {
                Sign(system, context, wallet);
                tx.Witnesses = context.GetWitnesses();
            }

            if (send)
            {
                //for now missing context item in contract
                if (wallet == null)
                {
                    system.Blockchain.Tell(tx);
                    return (JString)($"{tx.Hash}");
                }

                if (context.Completed)
                {
                    system.Blockchain.Tell(tx);
                    return (JString)($"{tx.Hash}");
                }
                else
                {
                    throw new RpcException(-500, "Incomplete signature: " + $"{context}");
                    //return (JString)("Incomplete signature:" + $"{context}");
                }
            }
            else
            {
                return Utility.TransactionAndContextToJson(tx, system.Settings, context);
            }
        }

        /// <summary>
        /// Signs the <see cref="IVerifiable"/> in the specified <see cref="ContractParametersContext"/> with the wallet.
        /// </summary>
        /// <param name="system">NeoSystem</param>
        /// <param name="context">The <see cref="ContractParametersContext"/> to be used.</param>
        /// <returns><see langword="true"/> if the signature is successfully added to the context; otherwise, <see langword="false"/>.</returns>
        /// 
        public static bool Sign(NeoSystem system, ContractParametersContext context, OperationWallet wallet)
        {
            if (context.Network != system.Settings.Network) return false;
            var fSuccess = false;
            foreach (var scriptHash in context.ScriptHashes)
            {
                WalletAccount account = wallet.GetAccount(scriptHash);

                if (account != null)
                {
                    // Try to sign self-contained multiSig
                    var multiSigContract = account.Contract;

                    if (multiSigContract != null &&
                        multiSigContract.Script.IsMultiSigContract(out int m, out ECPoint[] points))
                    {
                        foreach (var point in points)
                        {
                            account = wallet.GetAccount(point);
                            if (account?.HasKey != true) continue;
                            var key = account.GetKey();
                            var signature = context.Verifiable.Sign(key, context.Network);
                            fSuccess |= context.AddSignature(multiSigContract, key.PublicKey, signature);
                            if (fSuccess) m--;
                            if (context.Completed || m <= 0) break;
                        }
                        continue;
                    }
                    else if (account.HasKey)
                    {
                        // Try to sign with regular accounts
                        var key = account.GetKey();
                        var signature = context.Verifiable.Sign(key, context.Network);
                        fSuccess |= context.AddSignature(account.Contract, key.PublicKey, signature);
                        continue;
                    }
                }

                // Try Smart contract verification
                var contract = NativeContract.ContractManagement.GetContract(context.Snapshot, scriptHash);

                if (contract != null)
                {
                    var deployed = new DeployedContract(contract);

                    // Only works with verify without parameters
                    if (deployed.ParameterList.Length == 0)
                    {
                        fSuccess |= context.Add(deployed);
                    }
                }
            }

            return fSuccess;
        }


        /// <summary>
        /// Process "invoke" command
        /// </summary>
        /// <param name="system">NeoSystem</param>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="operation">Operation</param>
        /// <param name="result">Result</param>
        /// <param name="verificable">Transaction</param>
        /// <param name="contractParameters">Contract parameters</param>
        /// <param name="showStack">Show result stack if it is true</param>
        /// <param name="gas">Max fee for running the script</param>
        /// <returns>Return true if it was successful</returns>
        public static void OnInvokeWithResult(NeoSystem system, UInt160 scriptHash, string operation, out StackItem result, IVerifiable verificable = null, JArray contractParameters = null, bool showStack = true, long gas = TestModeGas)
        {
            var parameters = new List<ContractParameter>();

            if (contractParameters != null)
            {
                foreach (var contractParameter in contractParameters)
                {
                    parameters.Add(ContractParameter.FromJson(contractParameter));
                }
            }

            var contract = NativeContract.ContractManagement.GetContract(system.StoreView, scriptHash);
            if (contract == null)
            {
                throw new RpcException(-500, "Contract does not exist.");
            }
            else
            {
                if (contract.Manifest.Abi.GetMethod(operation, parameters.Count) == null)
                {
                    throw new RpcException(-500, "This method does not not exist in this contract.");
                }
            }

            byte[] script;

            using (var scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(scriptHash, operation, parameters.ToArray());
                script = scriptBuilder.ToArray();
            }

            if (verificable is Transaction tx)
            {
                tx.Script = script;
            }

            using var engine = ApplicationEngine.Run(script, system.StoreView, container: verificable, settings: system.Settings, gas: gas);
            result = engine.State == VMState.FAULT ? null : engine.ResultStack.Peek();

            if (engine.State == VMState.FAULT)
            {
                throw new RpcException(-500, Utility.GetExecutionOutput(engine, true, script).AsString());
            }
        }


        /// <summary>
        /// Process "invoke" command
        /// </summary>
        /// <param name="system">NeoSystem</param>
        /// <param name="wallet">wallet</param>
        /// <param name="keyPair">key pair </param>
        /// <param name="nefImage">nef image</param>
        /// <param name="manifest">manifest</param>
        /// <returns>Return jobject with SC script hash</returns>
        public static JObject DeploySmartContract(NeoSystem system, OperationWallet wallet, KeyPair keyPair, byte[] nefImage, ContractManifest manifest)
        {
            try
            {
                var stream = new MemoryStream(nefImage);
                var reader = new BinaryReader(stream);
                var nefFile = new NefFile();
                nefFile.Deserialize(reader);

                Console.WriteLine($"Deploying Smart Contract: '{manifest.Name}' compiled: '{nefFile.Compiler}'");

                byte[] script;
                using (var sb = new ScriptBuilder())
                {
                    sb.EmitDynamicCall(NativeContract.ContractManagement.Hash, "deploy", nefFile.ToArray(), manifest.ToJson().ToString());
                    script = sb.ToArray();
                }
                var sender = Contract.CreateSignatureRedeemScript(keyPair.PublicKey).ToScriptHash();
                var signers = new[] { new Signer { Scopes = WitnessScope.CalledByEntry, Account = sender } };

                var snapshot = system.StoreView;
                var tx = wallet.MakeTransaction(snapshot, script, sender, signers, maxGas: TestModeGas);

                var scriptHash = Neo.SmartContract.Helper.GetContractHash(tx.Sender, nefFile.CheckSum, manifest.Name);

                var txHash = SignAndSendTx(system, snapshot, tx, wallet, true);

                var res = new JObject();
                res["scriptHash"] = $"{scriptHash}";
                res["address"] = scriptHash.ToAddress(system.Settings.AddressVersion);
                res["txHash"] = txHash;

                Console.WriteLine($"Deployed Smart Contract '{scriptHash}'.");

                return res;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Deployed Smart Contract Error: {e.Message} : {e.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// Process "invoke" command
        /// </summary>
        /// <param name="system">NeoSystem</param>
        /// <param name="wallet">wallet</param>
        /// <param name="account">public key </param>
        /// <param name="nefImage">nef image</param>
        /// <param name="manifest">manifest</param>
        /// <returns>Return jobject with SC script hash</returns>
        public static JObject CreateDeploySmartContractTransaction(NeoSystem system, OperationWallet wallet, UInt160 account, byte[] nefImage, ContractManifest manifest
            , long gas = TestModeGas)
        {
            try
            {
                var stream = new MemoryStream(nefImage);
                var reader = new BinaryReader(stream);
                var nefFile = new NefFile();
                nefFile.Deserialize(reader);

                byte[] script;
                using (var sb = new ScriptBuilder())
                {
                    sb.EmitDynamicCall(NativeContract.ContractManagement.Hash, "deploy", nefFile.ToArray(), manifest.ToJson().ToString());
                    script = sb.ToArray();
                }

                var signers = System.Array.Empty<Signer>();
                var snapshot = system.StoreView;
                if (account != null)
                {
                    signers = wallet.GetAccounts()
                    .Where(p => !p.Lock && !p.WatchOnly && p.ScriptHash == account && NativeContract.GAS.BalanceOf(snapshot, p.ScriptHash).Sign > 0)
                    .Select(p => new Signer() { Account = p.ScriptHash, Scopes = WitnessScope.CalledByEntry })
                    .ToArray();
                }
                var tx = wallet.MakeTransaction(snapshot, script, account, signers, maxGas: gas);

                return Utility.TransactionAndContextToJson(tx, system.Settings);

            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Create Deploy Smart Contract transaction Error: {e.Message} : {e.ToString()}");
                throw;
            }
        }
    }
}
