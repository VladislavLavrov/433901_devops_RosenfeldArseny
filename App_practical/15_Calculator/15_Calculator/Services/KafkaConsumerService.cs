using Confluent.Kafka;
using _15_Calculator.Data;
using _15_Calculator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;
using System.Text.Json;

namespace _15_Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly IHttpClientFactory _clientFactory;

        public KafkaConsumerService(
            IConfiguration config,
            IHttpClientFactory clientFactory)
        {
            var consumerConfig = new ConsumerConfig();
            config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);

            _topic = config["Kafka:TopicName"]
                ?? throw new InvalidOperationException("Kafka:TopicName is not configured.");

            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _clientFactory = clientFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            try
            {
                _kafkaConsumer.Subscribe(_topic);
                Console.WriteLine($"Kafka Consumer подписан на топик: {_topic}");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _kafkaConsumer.Consume(cancellationToken);
                        if (string.IsNullOrWhiteSpace(cr?.Message?.Value))
                            continue;

                        var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);
                        if (inputData == null)
                        {
                            Console.WriteLine("Kafka: получено пустое или некорректное сообщение.");
                            continue;
                        }

                        var result = CalculatorLibrary.CalculateOperation(
                            inputData.Operand_1,
                            inputData.Operand_2,
                            inputData.Type_operation);

                        inputData.Result = result.ToString(
                            System.Globalization.CultureInfo.InvariantCulture);

                        var httpClient = _clientFactory.CreateClient();
                        using var response = await httpClient.PostAsJsonAsync(
                            "http://127.0.0.1:5015/Calculator/Callback",
                            inputData,
                            cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(
                                $"УСПЕХ: {inputData.Operand_1} {inputData.Type_operation} " +
                                $"{inputData.Operand_2} = {inputData.Result}. Запись отправлена в MariaDB.");
                        }
                        else
                        {
                            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                            Console.WriteLine(
                                $"Ошибка Callback: HTTP {(int)response.StatusCode}. {responseText}");
                        }

                        Console.WriteLine($"Kafka topic: {cr.Topic}, partition: {cr.Partition}, offset: {cr.Offset}");
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ConsumeException ex)
                    {
                        Console.WriteLine($"Ошибка Kafka Consumer: {ex.Error.Reason}");
                        if (ex.Error.IsFatal)
                            break;

                        await Task.Delay(1000, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обработки Kafka-сообщения: {ex.Message}");
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальная остановка фоновой службы.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка Kafka Consumer: {ex}");
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
