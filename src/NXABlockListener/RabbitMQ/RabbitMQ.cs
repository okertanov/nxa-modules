using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nxa.Plugins.RabbitMQ
{
    public class RabbitMQ : IDisposable
    {
        private IConnection connection;

        private readonly TimeSpan confirmWaitTimeSec = new(0, 0, 5);
        public RabbitMQ()
        {
        }

        public bool Send(string msg)
        {
            try
            {
                using (var channel = SetUpConnection())
                {
                    if (channel == null) { return false; }

                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        channel.ConfirmSelect();
                    }
                    channel.ExchangeDeclare(exchange: Plugins.Settings.Default.RMQ.Exchange, type: ExchangeType.Fanout, durable: false, autoDelete: false, arguments: null);

                    var body = Encoding.UTF8.GetBytes(msg);
                    channel.BasicPublish(exchange: Plugins.Settings.Default.RMQ.Exchange, routingKey: Plugins.Settings.Default.RMQ.Queue, basicProperties: null, body: body);

                    if (Plugins.Settings.Default.RMQ.ConfirmSelect)
                    {
                        var result = channel.WaitForConfirms(confirmWaitTimeSec);
                        return result;
                    }
                    return true;
                }
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
                using (var channel = SetUpConnection())
                {
                    if (channel == null) { return false; }

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
                        var result = channel.WaitForConfirms(confirmWaitTimeSec);
                        return result;
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                ConsoleWriter.UpdateRmqConnection("Error");
                ConsoleWriter.WriteLine($"Error establishing connection to RMQ: {e.Message}");
                CloseConnection();
                return false;
            }
        }

        private IModel SetUpConnection()
        {
            CreateConnection();

            if (connection != null && connection.IsOpen)
            {
                return connection.CreateModel();
            }
            return null;
        }

        private void CreateConnection()
        {
            if (connection != null && connection.IsOpen)
            {
                return;
            }

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
                connection = factory.CreateConnection(endpoints);
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
            if (connection != null)
            {
                if (connection.IsOpen)
                {
                    try { connection.Close(); }
                    catch { }
                }
                connection.Dispose();
                connection = null;
            }
        }

        public void Dispose()
        {
            CloseConnection();
            ConsoleWriter.UpdateRmqConnection("Closed");
            GC.SuppressFinalize(this);
        }

    }

}
