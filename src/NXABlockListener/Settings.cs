using Microsoft.Extensions.Configuration;

namespace Nxa.Plugins
{
    internal class Settings
    {
        public string Test { get; }
        public uint Network { get; }

        public static Settings Default { get; private set; }

        public Settings(IConfigurationSection section)
        {
            Test = section.GetValue("Test", "Nothing");
            Network = section.GetValue("Network", 5195086u);
        }

        public static void Load(IConfigurationSection section)
        {
            Default = new Settings(section);
        }
    }
}
