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
    public class VisitableBlock : VisitableBase, IVisitable
    {
        public VisitableBlock()
        {
            this.ExchangeList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == "block" && x.Exchange == true).Select(x => x.Name).ToArray();
            this.QueueList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == "block" && x.Exchange == false).Select(x => x.Name).ToArray();
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

            this.obj = jsonObj;

            Search(this.obj, "block", searchJson);

            return true;
        }

    }
}
