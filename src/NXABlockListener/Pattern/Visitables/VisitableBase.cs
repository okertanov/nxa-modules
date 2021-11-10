using Neo;
using Neo.IO.Json;
using Nxa.Plugins.Pattern.Visitors;
using System.Threading;

namespace Nxa.Plugins.Pattern.Visitables
{
    public abstract class VisitableBase 
    {
        protected string name { get; set; }
        public JObject Obj { get; set; }

        public string[] ExchangeList { get; set; }
        public string[] QueueList { get; set; }
        public bool AnnounceThis { get; set; }


        public abstract void Accept(IVisitor visitor, CancellationToken cancellationToken);
        public abstract bool Parse(JObject jsonObj, ProtocolSettings protocolSettings, JObject searchJson = null);
        public virtual void Search(JObject jsonObj, string searchType, JObject searchJson = null)
        {
            if (searchJson == null)
            {
                AnnounceThis = true;
                return;
            }

            if (!searchJson.ContainsProperty(searchType))
            {
                AnnounceThis = false;
                return;
            }

            foreach (var child in searchJson[searchType].Properties)
            {
                if (!Pattern.Utility.HasValueDeep(jsonObj, child.Key, child.Value))
                {
                    AnnounceThis = false;
                    return;
                }
            }

            AnnounceThis = true;
        }

        public string AnnounceObject()
        {
            JObject jObject = new JObject();
            jObject[name] = Obj;
            return jObject.AsString();
        }
    }
}
