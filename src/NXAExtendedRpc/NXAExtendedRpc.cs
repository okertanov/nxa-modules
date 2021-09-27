using Neo;
using Neo.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    public partial class NXAExtendedRpc : RpcServer
    {
        private readonly NeoSystem system;

        public NXAExtendedRpc(NeoSystem system, RpcServerSettings settings) : base(system, settings)
        {
            this.system = system;
        }

    }
}
