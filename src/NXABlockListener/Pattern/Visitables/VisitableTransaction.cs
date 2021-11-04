using Neo;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Nxa.Plugins.Pattern.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins.Pattern.Visitables
{
    public class VisitableTransaction : VisitableBase, IVisitable
    {
        public Transaction transaction { get; private set; }

        public VisitableTransaction()
        {
            this.ExchangeList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == "transaction" && x.Exchange == true).Select(x => x.Name).ToArray();
            this.QueueList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == "transaction" && x.Exchange == false).Select(x => x.Name).ToArray();
        }

        public override void Accept(IVisitor visitor, CancellationToken cancellationToken)
        {
            visitor.Visit(this, cancellationToken);
        }

        public override bool Parse(JObject jsonObj, ProtocolSettings protocolSettings, JObject searchJson = null)
        {
            this.transaction = Utility.TransactionFromJson(jsonObj, protocolSettings);
            if (this.transaction == null)
                return false;
            this.obj = jsonObj;

            Search(this.obj, "transaction", searchJson);

            return true;
        }

    }

}
