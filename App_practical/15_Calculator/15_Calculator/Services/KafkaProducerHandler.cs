using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;

namespace _15_Calculator.Services
{
    public class KafkaProducerHandler : IDisposable
    {
        IProducer<byte[], byte[]> kafkaProducer;
        public KafkaProducerHandler(IConfiguration config)
        {
            var conf = new ProducerConfig();
            config.GetSection("Kafka:ProducerSettings").Bind(conf);
            kafkaProducer = new ProducerBuilder<byte[], byte[]>(conf).Build();
        }
        public Handle Handle { get => kafkaProducer.Handle; }
        public void Dispose()
        {
            kafkaProducer.Flush();
            kafkaProducer.Dispose();
        }
    }
}
