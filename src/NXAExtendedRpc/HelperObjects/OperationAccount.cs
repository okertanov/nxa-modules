using Neo;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo.SmartContract;
using Neo.Cryptography.ECC;

namespace Nxa.Plugins.HelperObjects
{
    internal class OperationAccount : WalletAccount
    {
        private readonly KeyPair key;

        public override bool HasKey => key != null;

        public OperationAccount(KeyPair key, ProtocolSettings settings)
            : base(Contract.CreateSignatureRedeemScript(key.PublicKey).ToScriptHash(), settings)
        {
            this.key = key;
            this.Contract = new()
            {
                Script = Contract.CreateSignatureRedeemScript(key.PublicKey),
                ParameterList = new[] { ContractParameterType.Signature },
            };
        }

        public OperationAccount(ECPoint pubKey, ProtocolSettings settings) : base(Contract.CreateSignatureRedeemScript(pubKey).ToScriptHash(), settings)
        {
            this.Contract = new()
            {
                Script = Contract.CreateSignatureRedeemScript(pubKey),
                ParameterList = new[] { ContractParameterType.Signature },
            };
        }

        public override KeyPair GetKey()
        {
            return key;
        }
    }
}
