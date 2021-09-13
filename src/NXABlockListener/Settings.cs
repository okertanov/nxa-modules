using Microsoft.Extensions.Configuration;

namespace Nxa.Plugins
{
    internal class Settings
    {
        public bool Active { get; }
        public bool AutoStart { get; }
        public uint Network { get; }
        public uint StartBlock { get; }
        public RabbitMQ.Settings RMQ { get; }
        public Db.Settings Db { get; }
        public static Settings Default { get; private set; }

        public Settings(IConfigurationSection section)
        {
            Active = section.GetValue("Active", false);
            AutoStart = section.GetValue("AutoStart", false);
            Network = section.GetValue("Network", 5195086u);
            StartBlock = section.GetValue("StartBlock", 0u);
            RMQ = new RabbitMQ.Settings(section);
            Db = new Db.Settings(section);
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
