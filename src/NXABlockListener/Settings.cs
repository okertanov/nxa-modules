using Microsoft.Extensions.Configuration;
using Neo;

namespace Nxa.Plugins
{
    internal sealed class Settings
    {
        public bool TurnedOn { get; }
        public bool AutoStart { get; }
        public uint Network { get; }
        public uint StartBlock { get; }
        public RabbitMQ.Settings RMQ { get; }
        public Persistence.Settings Db { get; }
        public ProtocolSettings ProtocolSettings { get; set; }
        public static Settings Default { get; private set; }

        public Settings(IConfigurationSection section)
        {
            TurnedOn = section.GetValue("TurnedOn", false);
            AutoStart = section.GetValue("AutoStart", false);
            Network = section.GetValue("Network", 5195086u);
            StartBlock = section.GetValue("StartBlock", 0u);
            RMQ = new RabbitMQ.Settings(section);
            Db = new Persistence.Settings(section);
        }


        public static void Load(IConfigurationSection section = null)
        {
            if (section == null)
            {
                section = new ConfigurationBuilder().Build().GetSection("");
            }
            Default = new Settings(section);
        }

    }
}
