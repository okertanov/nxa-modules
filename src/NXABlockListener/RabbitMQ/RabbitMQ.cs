using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nxa.Plugins.RabbitMQ
{
    public class RabbitMQ
    {
        private IConnection _connection;
        public RabbitMQ()
        {
        }

        public bool Send(string block)
        {
            if (ConnectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        channel.ConfirmSelect();
                    }
                    channel.QueueDeclare(queue: Plugins.Settings.Default.RMQ.BlockQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var body = Encoding.UTF8.GetBytes(block);
                    channel.BasicPublish(exchange: "", routingKey: Nxa.Plugins.Settings.Default.RMQ.BlockQueue, basicProperties: null, body: body);

                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        var result = channel.WaitForConfirms(new TimeSpan(0, 0, 5));
                        return result;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool SendBatch(List<string> blockJsonList)
        {
            if (ConnectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        channel.ConfirmSelect();
                    }
                    channel.QueueDeclare(queue: Plugins.Settings.Default.RMQ.BlockQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var basicPublishBatch = channel.CreateBasicPublishBatch();
                    foreach (var block in blockJsonList)
                    {
                        var body = Encoding.UTF8.GetBytes(block);
                        basicPublishBatch.Add(exchange: "", routingKey: Plugins.Settings.Default.RMQ.BlockQueue, mandatory: true, properties: null, new ReadOnlyMemory<byte>(body));
                    }
                    basicPublishBatch.Publish();

                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        var result = channel.WaitForConfirms(new TimeSpan(0, 0, 5));
                        return result;
                    }
                    return true;
                }
            }
            return false;
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    UserName = Plugins.Settings.Default.RMQ.Username,
                    Password = Plugins.Settings.Default.RMQ.Password,
                    VirtualHost = Plugins.Settings.Default.RMQ.VirtualHost
                };

                var endpoints = new List<AmqpTcpEndpoint>();
                foreach (var endpoint in Plugins.Settings.Default.RMQ.RMQHost)
                {
                    endpoints.Add(new AmqpTcpEndpoint(endpoint.Host, endpoint.Port));
                }
                _connection = factory.CreateConnection(endpoints);
            }
            catch (Exception ex)
            {
                ConsoleWriter.WriteLine($"Could not create RMQ connection: {ex.Message}");
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null && _connection.IsOpen)
                return true;
            else
                _connection = null;

            CreateConnection();
            return _connection != null;
        }
    }

}
