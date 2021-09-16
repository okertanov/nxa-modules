using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nxa.Plugins.RabbitMQ
{
    public class RabbitMQ : IDisposable
    {
        private IConnection _connection;

        private readonly TimeSpan _confirmWaitTimeSec = new(0, 0, 5);
        public RabbitMQ()
        {
        }

        public bool Send(string msg)
        {
            try
            {
                if (SetUpConnection())
                {
                    using (var channel = _connection.CreateModel())
                    {
                        if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                        {
                            channel.ConfirmSelect();
                        }
                        channel.ExchangeDeclare(exchange: Plugins.Settings.Default.RMQ.Exchange, type: ExchangeType.Fanout, durable: false, autoDelete: false, arguments: null);

                        var body = Encoding.UTF8.GetBytes(msg);
                        channel.BasicPublish(exchange: Plugins.Settings.Default.RMQ.Exchange, routingKey: Plugins.Settings.Default.RMQ.Queue, basicProperties: null, body: body);

                        if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                        {
                            var result = channel.WaitForConfirms(_confirmWaitTimeSec);
                            return result;
                        }
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                ConsoleWriter.UpdateRmqConnection("Error");
                ConsoleWriter.WriteLine($"Error establishing connection to RMQ: {e.Message}");
                CloseConnection();
                return false;
            }
        }

        public bool SendBatch(List<string> msgList)
        {
            try
            {
                if (SetUpConnection())
                {
                    using (var channel = _connection.CreateModel())
                    {
                        if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                        {
                            channel.ConfirmSelect();
                        }
                        channel.ExchangeDeclare(exchange: Plugins.Settings.Default.RMQ.Exchange, type: ExchangeType.Fanout, durable: false, autoDelete: false, arguments: null);

                        var basicPublishBatch = channel.CreateBasicPublishBatch();
                        foreach (var block in msgList)
                        {
                            var body = Encoding.UTF8.GetBytes(block);
                            basicPublishBatch.Add(exchange: Plugins.Settings.Default.RMQ.Exchange, routingKey: Plugins.Settings.Default.RMQ.Queue, mandatory: true, properties: null, new ReadOnlyMemory<byte>(body));
                        }
                        basicPublishBatch.Publish();

                        if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                        {
                            var result = channel.WaitForConfirms(_confirmWaitTimeSec);
                            return result;
                        }
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                ConsoleWriter.UpdateRmqConnection("Error");
                ConsoleWriter.WriteLine($"Error establishing connection to RMQ: {e.Message}");
                CloseConnection();
                return false;
            }
        }

        private bool SetUpConnection()
        {
            if (_connection != null && _connection.IsOpen)
                return true;

            CreateConnection();
            return _connection != null;
        }

        private void CreateConnection()
        {
            try
            {
                CloseConnection();

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
                ConsoleWriter.UpdateRmqConnection("Active");
            }
            catch (Exception ex)
            {
                ConsoleWriter.WriteLine($"Could not create RMQ connection: {ex.Message}");
                ConsoleWriter.UpdateRmqConnection($"Error: {ex.Message}");
            }
        }

        private void CloseConnection()
        {
            if (_connection != null)
            {
                if (_connection.IsOpen)
                {
                    try { _connection.Close(); }
                    catch { }
                }
                _connection.Dispose();
                _connection = null;
            }
        }

        #region dispose
        public void Dispose()
        {
            CloseConnection();
            ConsoleWriter.UpdateRmqConnection("Closed");
            //GC.SuppressFinalize(this);
        }

        #endregion
    }

}
