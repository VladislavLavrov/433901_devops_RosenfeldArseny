using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace _15_Calculator.Services
{
    public class KafkaProducerService<K, V>
    {
        IProducer<K, V> kafkaHandle;
        public KafkaProducerService(KafkaProducerHandler handle)
        {
            kafkaHandle = new DependentProducerBuilder<K, V>(handle.Handle).Build();
        }
        public Task ProduceAsync(string topic, Message<K, V> message)
            => kafkaHandle.ProduceAsync(topic, message);
    }
}
