using Microsoft.Extensions.Configuration;

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
