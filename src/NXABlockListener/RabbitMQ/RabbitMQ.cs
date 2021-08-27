using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nxa.Plugins.RabbitMQ
{
    public class RabbitMQ
    {
        private IConnection _connection;
        private Settings _settings;
        public RabbitMQ(Settings settings)
        {
            _settings = settings;

            if (!_settings.Active)
            {
                ConsoleWriter.WriteLine(string.Format("NXABlockListener RMQ disabled"));
                return;
            }

            CreateConnection();
        }

        public void send(string json)
        {

            if (ConnectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.QueueDeclare(queue: _settings.BlockQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var body = Encoding.UTF8.GetBytes(json);

                    channel.BasicPublish(exchange: "", routingKey: _settings.BlockQueue, basicProperties: null, body: body);
                }
            }

        }



        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    UserName = _settings.Username,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost
                };

                var endpoints = new List<AmqpTcpEndpoint>();
                foreach (var endpoint in _settings.RMQHost)
                {
                    endpoints.Add(new AmqpTcpEndpoint(endpoint.Host, endpoint.Port));
                }
                _connection = factory.CreateConnection(endpoints);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create RMQ connection: {ex.Message}");
                //exit in this case???
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
                return true;
            CreateConnection();
            return _connection != null;
        }
    }

}
