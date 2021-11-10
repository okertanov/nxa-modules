using Neo;
using Neo.IO.Json;
using Nxa.Plugins.Pattern.Visitables;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Nxa.Plugins.Pattern.Visitors
{
    public class Visitor : IVisitor, IDisposable
    {

        private readonly ProtocolSettings settings;
        private RabbitMQ.RabbitMQ rabbitMQ;
        private JsonVisitableParser visitableParser;
        private string rmqSearchQueue;
        public Visitor(RabbitMQ.RabbitMQ rabbitMQ, ProtocolSettings settings, string rmqSearchQueue = "", JObject searchJson = null)
        {
            this.settings = settings;
            this.rabbitMQ = rabbitMQ;
            this.visitableParser = new JsonVisitableParser(settings, searchJson);
            this.rmqSearchQueue = rmqSearchQueue;
        }
        public bool Visit(VisitableBlock block, CancellationToken cancellationToken)
        {
            Announce(block, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return false;

            //get block tx
            foreach (var txJson in (JArray)block.Obj["tx"])
            {
                foreach (var item in Parse(txJson))
                {
                    item.Accept(this, cancellationToken);
                }
            }

            return true;
        }

        public bool Visit(VisitableTransaction tx, CancellationToken cancellationToken)
        {
            Announce(tx, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return false;

            //get transaction types
            foreach (var item in Parse(tx.Obj["script"]))
            {
                item.Accept(this, cancellationToken);
            }

            return true;
        }

        public bool Visit(VisitableTransfer transfer, CancellationToken cancellationToken)
        {
            Announce(transfer, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return false;
            return true;
        }

        public bool Visit(VisitableSCDeployment scdeployment, CancellationToken cancellationToken)
        {
            Announce(scdeployment, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return false;
            return true;
        }

        public IEnumerable<IVisitable> Parse(JObject obj)
        {
            return this.visitableParser.Parse(obj, this.settings);
        }

        private void Announce(VisitableBase visitableBase, CancellationToken cancellationToken)
        {
            if (!String.IsNullOrEmpty(rmqSearchQueue))
            {
                if (visitableBase.AnnounceThis)
                {
                    //send serch content
                    this.rabbitMQ.SendToRabbitMQ(visitableBase.AnnounceObject(), cancellationToken, "", rmqSearchQueue);
                }
            }
            else
            {
                //send listener contetnt
                foreach (var exchange in visitableBase.ExchangeList)
                {
                    this.rabbitMQ.SendToRabbitMQ(visitableBase.AnnounceObject(), cancellationToken, exchange);
                }

                foreach (var queue in visitableBase.QueueList)
                {
                    this.rabbitMQ.SendToRabbitMQ(visitableBase.AnnounceObject(), cancellationToken, "", queue);
                }
            }

        }

        #region Dispose

        private bool _disposedValue;

        ~Visitor() => Dispose(false);

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
                    rabbitMQ?.Dispose();
                }
                _disposedValue = true;
            }
        }

        #endregion
    }
}
