using Microsoft.Extensions.Configuration;

namespace Nxa.Plugins
{
    internal class Settings
    {
        public bool Active { get; }
        public uint Network { get; }
        public RabbitMQ.Settings RMQ { get; }
        public static Settings Default { get; private set; }

        public Settings(IConfigurationSection section)
        {
            Active = section.GetValue("Active", false);
            Network = section.GetValue("Network", 5195086u);
            RMQ = new RabbitMQ.Settings(section);
        }

        public static void Load(IConfigurationSection section)
        {
            Default = new Settings(section);
        }
    }
}
