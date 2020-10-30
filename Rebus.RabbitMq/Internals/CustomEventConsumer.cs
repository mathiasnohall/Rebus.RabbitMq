using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace Rebus.Internals
{
    class CustomEventConsumer : EventingBasicConsumer
    {
        public ConcurrentQueue<BasicDeliverEventArgs> Queue { get; } = new ConcurrentQueue<BasicDeliverEventArgs>();

        public CustomEventConsumer(IModel model) : base(model)
        {
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (ca, ea) => HandleReceive(ca, ea);
        }

        internal void HandleReceive(object o, BasicDeliverEventArgs ea)
        {
            Queue.Enqueue(new BasicDeliverEventArgs
            {
                ConsumerTag = ea.ConsumerTag,
                DeliveryTag = ea.DeliveryTag,
                Redelivered = ea.Redelivered,
                Exchange = ea.Exchange,
                RoutingKey = ea.RoutingKey,
                BasicProperties = ea.BasicProperties,
                Body = ea.Body.ToArray()
            });
        }

        public void Dispose()
        {
            try
            {
                // it's so fucked up that these can throw exceptions
                Model?.Close();
                Model?.Dispose();
            }
            catch
            {
            }
        }
    }
}