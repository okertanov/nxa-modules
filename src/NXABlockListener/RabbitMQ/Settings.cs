using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Nxa.Plugins.RabbitMQ
{
    internal sealed class Settings
    {
        internal sealed class HostAddress
        {
            public HostAddress(string value)
            {
                var res = value.Split(":");
                Host = res[0];
                Port = int.Parse(res[1]);
            }
            public string Host { get; }
            public int Port { get; }
        }

        internal sealed class Exchange_New
        {
            public Exchange_New(IConfigurationSection section)
            {
                Type = section.GetValue("type", "");
                Name = section.GetValue("name", "");
                Exchange = section.GetValue("exchange", true);
            }
            public string Type { get; }
            public string Name { get; }
            public bool Exchange { get; }
        }

        public bool ConfirmSelect { get; }
        public string VirtualHost { get; }
        public HostAddress[] RMQHost { get; }
        //public string Queue { get; }
        //public string Exchange { get; }
        public string Username { get; }
        public string Password { get; }
        public Exchange_New[] Exchanges { get; }

        public Settings(IConfigurationSection section)
        {
            var rmqSection = section.GetSection("RMQ");

            ConfirmSelect = rmqSection.GetValue("ConfirmSelect", false);

            VirtualHost = rmqSection.GetValue("VirtualHost", "/");
            //Queue = rmqSection.GetValue("Queue", "");
            //Exchange = rmqSection.GetValue("Exchange", "");

            Username = rmqSection.GetValue("Username", "");
            Password = rmqSection.GetValue("Password", "");

            var hostSection = rmqSection.GetSection("RMQHost");
            RMQHost = hostSection.GetChildren().Select(c => new HostAddress(c.Value)).ToArray();

            var exchanges = rmqSection.GetSection("Exchanges");
            Exchanges = exchanges.GetChildren().Select(c => new Exchange_New(c)).ToArray();

        }

    }



}
