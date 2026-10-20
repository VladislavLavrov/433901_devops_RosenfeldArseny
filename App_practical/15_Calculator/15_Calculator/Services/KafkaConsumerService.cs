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
                GroupId = config["Kafka:ConsumerSettings:GroupId"] ?? "rozenfeld_15_group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                SocketTimeoutMs = 5000
            };
            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _topic = config["Kafka:TopicName"] ?? "rozenfeld_15";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _kafkaConsumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _kafkaConsumer.Consume(stoppingToken);
                    if (cr?.Message?.Value == null) continue;

                    var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);

                    if (inputData != null)
                    {
                        var result = CalculatorLibrary.CalculateOperation(inputData.Operand_1, inputData.Operand_2, inputData.Type_operation);
                        inputData.Result = result.ToString();

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<CalculatorContext>();
                            context.DataInputVariants.Add(inputData);
                            await context.SaveChangesAsync();
                        }
                        Console.WriteLine($"Saved: {inputData.Operand_1} {inputData.Type_operation} {inputData.Operand_2} = {inputData.Result}");
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (ConsumeException e) { if (e.Error.IsFatal) break; }
                catch (Exception e) { Console.WriteLine($"Error: {e.Message}"); }
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