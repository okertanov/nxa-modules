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
    public interface IVisitable
    {
        void Accept(IVisitor visitor, CancellationToken cancellationToken);
        bool Parse(JObject jsonObj, ProtocolSettings protocolSettings, JObject searchJson = null);
        void Search(JObject jsonObj, string searchType, JObject searchJson = null);
    }

}
