﻿using Neo;
using Neo.IO.Json;
using Nxa.Plugins.Pattern.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins.Pattern.Visitables
{
    public abstract class VisitableBase 
    {
        public JObject obj { get; set; }
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

    }
}
