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
        public bool Active { get; }
        public uint Network { get; }
        public Settings(IConfigurationSection section)
        {
            Active = section.GetValue("Active", false);
            Network = uint.Parse(section.GetValue("Network", "5195086"));
        }
    }

}
