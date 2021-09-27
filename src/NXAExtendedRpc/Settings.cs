using Microsoft.Extensions.Configuration;
using Neo;
using Neo.Plugins;
using Neo.SmartContract.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;


namespace Nxa.Plugins
{

    internal sealed class Settings
    {
        public IReadOnlyList<RpcServerSettings> Servers { get; }

        public Settings(IConfigurationSection section)
        {
            Servers = section.GetSection(nameof(Servers)).GetChildren().Select(p => RpcServerSettings.Load(p)).ToArray();
        }
    }

}
