using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nxa.Plugins.RabbitMQ
{
    public class RabbitMQ : IDisposable
    {
        private IConnection _connection;

        private TimeSpan _confirmWaitTime = new TimeSpan(0, 0, 5);
        public RabbitMQ()
        {
        }

        public bool Send(string block)
        {
            if (connectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        channel.ConfirmSelect();
                    }
                    channel.ExchangeDeclare(exchange: Plugins.Settings.Default.RMQ.Exchange, type: ExchangeType.Fanout, durable: false, autoDelete: false, arguments: null);

                    var body = Encoding.UTF8.GetBytes(block);
                    channel.BasicPublish(exchange: Plugins.Settings.Default.RMQ.Exchange, routingKey: Plugins.Settings.Default.RMQ.Queue, basicProperties: null, body: body);

                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        var result = channel.WaitForConfirms(_confirmWaitTime);
                        return result;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool SendBatch(List<string> blockJsonList)
        {
            if (connectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        channel.ConfirmSelect();
                    }
                    channel.ExchangeDeclare(exchange: Plugins.Settings.Default.RMQ.Exchange, type: ExchangeType.Fanout, durable: false, autoDelete: false, arguments: null);

                    var basicPublishBatch = channel.CreateBasicPublishBatch();
                    foreach (var block in blockJsonList)
                    {
                        var body = Encoding.UTF8.GetBytes(block);
                        basicPublishBatch.Add(exchange: Plugins.Settings.Default.RMQ.Exchange, routingKey: Plugins.Settings.Default.RMQ.Queue, mandatory: true, properties: null, new ReadOnlyMemory<byte>(body));
                    }
                    basicPublishBatch.Publish();

                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        var result = channel.WaitForConfirms(_confirmWaitTime);
                        return result;
                    }
                    return true;
                }
            }
            return false;
        }

        private void createConnection()
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

        private bool connectionExists()
        {
            if (_connection != null && _connection.IsOpen)
                return true;
            else
                _connection = null;

            createConnection();
            return _connection != null;
        }

        #region dispose
        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                }
            }
        }

        #endregion
    }

}
