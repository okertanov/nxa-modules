using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins.Persistence
{
    internal sealed class Settings
    {
        public string Path { get; }
        public Settings(IConfigurationSection section)
        {
            var dbSection = section.GetSection("DB");
            this.Path = dbSection.GetValue("Path", "RMQData_{0}");
        }

    }
}
