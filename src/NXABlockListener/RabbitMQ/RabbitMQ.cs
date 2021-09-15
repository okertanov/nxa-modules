using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nxa.Plugins.RabbitMQ
{
    public class RabbitMQ : IDisposable
    {
        private IConnection _connection;

        private readonly TimeSpan _confirmWaitTime = new (0, 0, 5);
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
            if (ConnectionExists())
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

        #region dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connection != null)
                {
                    if (_connection.IsOpen)
                        _connection.Close();
                    _connection.Dispose();
                }
            }
        }

        #endregion
    }

}
