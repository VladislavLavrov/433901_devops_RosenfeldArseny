using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;

namespace _15_Calculator.Services
{
    public class KafkaProducerHandler : IDisposable
    {
        private readonly IProducer<byte[], byte[]> kafkaProducer;

        public KafkaProducerHandler(IConfiguration config)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:ProducerSettings:BootstrapServers"]
            };
            kafkaProducer = new ProducerBuilder<byte[], byte[]>(producerConfig).Build();
        }

        public Task ProduceAsync(string topic, Message<byte[], byte[]> message)
            => kafkaProducer.ProduceAsync(topic, message);

        public void Flush(TimeSpan timeout) => kafkaProducer.Flush(timeout);
        public void Dispose() => kafkaProducer.Dispose();
    }
}