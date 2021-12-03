using Neo;
using Neo.IO.Json;
using Neo.SmartContract;
using Neo.Wallets;
using Neo.Wallets.NEP6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins.HelperObjects
{
    internal class OperationWallet : Wallet
    {
        private readonly Dictionary<UInt160, OperationAccount> accounts;

        /// <summary>
        /// The parameters of the SCrypt algorithm used for encrypting and decrypting the private keys in the wallet.
        /// </summary>
        //public readonly ScryptParameters Scrypt;

        public override string Name { get; }

        /// <summary>
        /// The version of the wallet standard. It is currently fixed at 1.0 and will be used for functional upgrades in the future.
        /// </summary>
        public override Version Version { get; }

        public OperationWallet(ProtocolSettings settings, OperationAccount[] accounts) : base(String.Empty, settings)
        {
            this.Version = Version.Parse("1.0");
            //this.Scrypt = ScryptParameters.Default;
            this.accounts = accounts.ToDictionary(p => p.ScriptHash);
        }

        public override bool ChangePassword(string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(UInt160 scriptHash)
        {
            lock (accounts)
            {
                return accounts.ContainsKey(scriptHash);
            }
        }

        public override WalletAccount CreateAccount(byte[] privateKey)
        {
            throw new NotImplementedException();
        }

        public override WalletAccount CreateAccount(Contract contract, KeyPair key = null)
        {
            throw new NotImplementedException();
        }

        public override WalletAccount CreateAccount(UInt160 scriptHash)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteAccount(UInt160 scriptHash)
        {
            throw new NotImplementedException();
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override WalletAccount GetAccount(UInt160 scriptHash)
        {
            accounts.TryGetValue(scriptHash, out OperationAccount account);
            return account;
        }

        public override IEnumerable<WalletAccount> GetAccounts()
        {
            foreach (OperationAccount account in accounts.Values)
                yield return account;
        }

        public override bool VerifyPassword(string password)
        {
            throw new NotImplementedException();
        }
    }

}
