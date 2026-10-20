using Confluent.Kafka;
using _15_Calculator.Data;
using _15_Calculator.Models;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace _15_Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly string _topic;
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerService(IConfiguration config, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:ConsumerSettings:BootstrapServers"],
                GroupId = config["Kafka:ConsumerSettings:GroupId"] ?? "lavrov_15",
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.Plaintext,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _topic = config["Kafka:TopicName"] ?? "lavrov";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _kafkaConsumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _kafkaConsumer.Consume(stoppingToken);
                    var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);

                    if (inputData != null)
                    {
                        // Считаем
                        var result = CalculatorLibrary.CalculateOperation(inputData.Operand_1, inputData.Operand_2, inputData.Type_operation);
                        inputData.Result = result.ToString();

                        // Сохраняем в БД
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<CalculatorContext>();
                            context.DataInputVariants.Add(inputData);
                            await context.SaveChangesAsync();
                        }

                        Console.WriteLine($"Message key: {cr.Message.Key}, value: {cr.Message.Value}");
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (ConsumeException e)
                {
                    if (e.Error.IsFatal) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }

        public override void Dispose()
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
            base.Dispose();
        }
    }
}