using Neo;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Nxa.Plugins.Pattern.Visitors;
using System.Linq;
using System.Threading;

namespace Nxa.Plugins.Pattern.Visitables
{
    public class VisitableTransaction : VisitableBase, IVisitable
    {
      
        public VisitableTransaction()
        {
            name = "transaction";
            this.ExchangeList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == name && x.Exchange == true).Select(x => x.Name).ToArray();
            this.QueueList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == name && x.Exchange == false).Select(x => x.Name).ToArray();
        }
        public Transaction transaction { get; private set; }

        public override void Accept(IVisitor visitor, CancellationToken cancellationToken)
        {
            visitor.Visit(this, cancellationToken);
        }

        public override bool Parse(JObject jsonObj, ProtocolSettings protocolSettings, JObject searchJson = null)
        {
            this.transaction = Utility.TransactionFromJson(jsonObj, protocolSettings);
            if (this.transaction == null)
                return false;

            Obj = jsonObj;

            Search(Obj, name, searchJson);

            return true;
        }

    }

}
