using Neo;
using Neo.IO.Json;
using Neo.SmartContract;
using Neo.VM;
using Nxa.Plugins.Pattern.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins.Pattern.Visitables
{
    public class VisitableSCDeployment : VisitableBase, IVisitable
    {
        public VisitableSCDeployment()
        {
            this.ExchangeList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == "scdeployment" && x.Exchange == true).Select(x => x.Name).ToArray();
            this.QueueList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == "scdeployment" && x.Exchange == false).Select(x => x.Name).ToArray();
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

            if (result["method"] == null || result["method"].AsString() != "deploy")
                return false;

            this.obj = new JObject();
            this.obj["method"] = result["method"];
            this.obj["assetid"] = result["assetid"];

            this.obj["manifest"] = JObject.Parse(result["action"][0].AsString());
            this.obj["nef"] = JObject.Parse(result["action"][1].AsString());

            Search(this.obj, "scdeployment", searchJson);

            return true;
        }

    }
}
