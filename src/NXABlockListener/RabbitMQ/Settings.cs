using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins.RabbitMQ
{
    public class Settings
    {
        public class HostAddress
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
        //public bool Active { get; }
        public bool ConfirmSelect { get; }
        public string VirtualHost { get; }
        public HostAddress[] RMQHost { get; }
        public string BlockQueue { get; }
        public string Username { get; }
        public string Password { get; }

        public Settings(IConfigurationSection section)
        {
            var rmqSection = section.GetSection("RMQ");
            //Active = rmqSection.GetValue("Active", false);
            ConfirmSelect = rmqSection.GetValue("ConfirmSelect", false);
            VirtualHost = rmqSection.GetValue("VirtualHost", "localhost");
            BlockQueue = rmqSection.GetValue("BlockQueue", "default");
            Username = rmqSection.GetValue("Username", "guest");
            Password = rmqSection.GetValue("Password", "guest");

            var hostSection = rmqSection.GetSection("RMQHost");
            RMQHost = hostSection.GetChildren().Select(c => new HostAddress(c.Value)).ToArray();
        }

    }



}
