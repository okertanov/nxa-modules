using Neo;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Nxa.Plugins.Pattern.Visitors;
using System.Linq;
using System.Threading;

namespace Nxa.Plugins.Pattern.Visitables
{
    public class VisitableBlock : VisitableBase, IVisitable
    {
        public VisitableBlock()
        {
            name = "block";
            this.ExchangeList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == name && x.Exchange == true).Select(x => x.Name).ToArray();
            this.QueueList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == name && x.Exchange == false).Select(x => x.Name).ToArray();
        }

        public Block block { get; private set; }

        public override void Accept(IVisitor visitor, CancellationToken cancellationToken)
        {
            visitor.Visit(this, cancellationToken);
        }

        public override bool Parse(JObject jsonObj, ProtocolSettings protocolSettings, JObject searchJson = null)
        {
            this.block = Utility.BlockFromJson(jsonObj, protocolSettings);
            if (this.block == null)
                return false;

            Obj = jsonObj;

            Search(Obj, name, searchJson);

            return true;
        }

    }
}
