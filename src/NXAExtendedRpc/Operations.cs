using Akka.Actor;
using Neo;
using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.IO;
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
            Signer[] signers = System.Array.Empty<Signer>();
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
                Transaction tx = wallet.MakeTransaction(snapshot, script, account, signers, maxGas: gas);

                using (ApplicationEngine engine = ApplicationEngine.Run(tx.Script, snapshot, container: tx, settings: system.Settings, gas: gas))
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
            Signer[] signers = System.Array.Empty<Signer>();
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
                Transaction tx = wallet.MakeTransaction(snapshot, script, account, signers, maxGas: gas);

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
            bool fSuccess = false;
            foreach (UInt160 scriptHash in context.ScriptHashes)
            {
                WalletAccount account = wallet.GetAccount(scriptHash);

                if (account != null)
                {
                    // Try to sign self-contained multiSig
                    Contract multiSigContract = account.Contract;

                    if (multiSigContract != null &&
                        multiSigContract.Script.IsMultiSigContract(out int m, out ECPoint[] points))
                    {
                        foreach (var point in points)
                        {
                            account = wallet.GetAccount(point);
                            if (account?.HasKey != true) continue;
                            KeyPair key = account.GetKey();
                            byte[] signature = context.Verifiable.Sign(key, context.Network);
                            fSuccess |= context.AddSignature(multiSigContract, key.PublicKey, signature);
                            if (fSuccess) m--;
                            if (context.Completed || m <= 0) break;
                        }
                        continue;
                    }
                    else if (account.HasKey)
                    {
                        // Try to sign with regular accounts
                        KeyPair key = account.GetKey();
                        byte[] signature = context.Verifiable.Sign(key, context.Network);
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
            List<ContractParameter> parameters = new List<ContractParameter>();

            if (contractParameters != null)
            {
                foreach (var contractParameter in contractParameters)
                {
                    parameters.Add(ContractParameter.FromJson(contractParameter));
                }
            }

            ContractState contract = NativeContract.ContractManagement.GetContract(system.StoreView, scriptHash);
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

            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(scriptHash, operation, parameters.ToArray());
                script = scriptBuilder.ToArray();
            }

            if (verificable is Transaction tx)
            {
                tx.Script = script;
            }

            using ApplicationEngine engine = ApplicationEngine.Run(script, system.StoreView, container: verificable, settings: system.Settings, gas: gas);
            result = engine.State == VMState.FAULT ? null : engine.ResultStack.Peek();

            if (engine.State == VMState.FAULT)
            {
                throw new RpcException(-500, Utility.GetExecutionOutput(engine, true, script).AsString());
            }
        }




    }
}
