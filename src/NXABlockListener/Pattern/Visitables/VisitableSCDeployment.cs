using Neo;
using Neo.IO.Json;
using Nxa.Plugins.Pattern.Visitors;
using System;
using System.Linq;
using System.Threading;

namespace Nxa.Plugins.Pattern.Visitables
{
    public class VisitableSCDeployment : VisitableBase, IVisitable
    {
        public VisitableSCDeployment()
        {
            name = "scdeployment";
            this.ExchangeList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == name && x.Exchange == true).Select(x => x.Name).ToArray();
            this.QueueList = Settings.Default.RMQ.Exchanges.Where(x => x.Type == name && x.Exchange == false).Select(x => x.Name).ToArray();
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

            Obj = new JObject();
            Obj["method"] = result["method"];
            Obj["assetid"] = result["assetid"];

            Obj["manifest"] = JObject.Parse(result["action"][0].AsString());
            Obj["nef"] = JObject.Parse(result["action"][1].AsString());

            Search(Obj, name, searchJson);

            return true;
        }

    }
}
