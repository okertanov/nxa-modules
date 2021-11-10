using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nxa.Plugins.RabbitMQ
{
    public class RabbitMQ : IDisposable
    {
        private IConnection connection;

        private readonly TimeSpan confirmWaitTimeSec = new(0, 0, 5);
        private readonly TimeSpan retryWaitTimeSec = new(0, 0, 5);
        public RabbitMQ()
        {
        }

        public void SendToRabbitMQ(string msg, CancellationToken cancellationToken, string exchange = "", string queue = "")
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (!Send(msg, exchange, queue))
                {
                    ConsoleWriter.WriteLine(String.Format("Failed to send to RMQ. Wait {0}sec and try again.", retryWaitTimeSec.TotalSeconds));
                    Thread.Sleep(retryWaitTimeSec);
                }
                else
                {
                    return;
                }
            }
        }

        public bool DeclareExchangeQueue(string exchange = "", string queue = "")
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

                    if (!String.IsNullOrEmpty(exchange))
                    {
                        channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout, durable: false, autoDelete: false, arguments: null);
                    }
                    else if (!String.IsNullOrEmpty(queue))
                    {
                        channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: true);
                    }

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
                ConsoleWriter.WriteLine($"Error establishing connection to RMQ: {e.Message}");
                CloseConnection();
                return false;
            }
        }

        public bool Send(string msg, string exchange = "", string queue = "")
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

                    if (!String.IsNullOrEmpty(exchange))
                    {
                        channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout, durable: false, autoDelete: false, arguments: null);
                    }
                    else if (!String.IsNullOrEmpty(queue))
                    {
                        channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: true);
                    }

                    var body = Encoding.UTF8.GetBytes(msg);
                    channel.BasicPublish(exchange: exchange, routingKey: queue, basicProperties: null, body: body);

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
                ConsoleWriter.WriteLine($"Error establishing connection to RMQ: {e.Message}");
                CloseConnection();
                return false;
            }
        }

        public bool SendBatch(List<string> msgList, string exchange = "", string queue = "")
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
                    if (!String.IsNullOrEmpty(exchange))
                    {
                        channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout, durable: false, autoDelete: false, arguments: null);
                    }
                    else if (!String.IsNullOrEmpty(queue))
                    {
                        channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: true);
                    }

                    var basicPublishBatch = channel.CreateBasicPublishBatch();
                    foreach (var block in msgList)
                    {
                        var body = Encoding.UTF8.GetBytes(block);
                        basicPublishBatch.Add(exchange: exchange, routingKey: queue, mandatory: true, properties: null, new ReadOnlyMemory<byte>(body));
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
            }
            catch (Exception ex)
            {
                ConsoleWriter.WriteLine($"Could not create RMQ connection: {ex.Message}");
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

        #region Dispose

        private bool _disposedValue;

        ~RabbitMQ() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CloseConnection();
                    ConsoleWriter.WriteLine("RMQ connection closed");
                }

                _disposedValue = true;
            }
        }

        #endregion

    }

}
