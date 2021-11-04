using Neo;
using Neo.IO.Json;
using Nxa.Plugins.Pattern.Visitors;
using System;
using System.Linq;
using System.Threading;

namespace Nxa.Plugins.Pattern.Visitables
{
    public class VisitableTransfer : VisitableBase, IVisitable
    {
        public VisitableTransfer()
        {
            this.ExchangeList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == "transfer" && x.Exchange == true).Select(x => x.Name).ToArray();
            this.QueueList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == "transfer" && x.Exchange == false).Select(x => x.Name).ToArray();
        }

        public override void Accept(IVisitor visitor, CancellationToken cancellationToken)
        {
            visitor.Visit(this, cancellationToken);
        }

        public override bool Parse(JObject jsonObj, ProtocolSettings protocolSettings, JObject searchJson = null)
        {
            if (jsonObj is not JString)
                return false;

            JString script = (JString)jsonObj;
            var byteArr = Convert.FromBase64String(script.AsString());

            var result = Utility.ParseScript(byteArr);

            if (result["method"] == null || result["method"].AsString() != "transfer")
                return false;

            this.obj = new JObject();
            this.obj["method"] = result["method"];
            this.obj["assetid"] = result["assetid"];
            JObject transferOutput = new JObject();
            transferOutput["data"] = result["action"][0];
            transferOutput["value"] = result["action"][1];
            transferOutput["addressto"] = result["action"][2];
            transferOutput["addressfrom"] = result["action"][3];
            this.obj["transferoutput"] = transferOutput;

            Search(this.obj, "transfer", searchJson);

            return true;
        }

    }
}
