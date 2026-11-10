using _15_Calculator.Data;
using _15_Calculator.Models;
using _15_Calculator.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace _15_Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly CalculatorContext _context;
        private readonly KafkaProducerService<Null, string> _producer;
        private readonly string _topic;

        public CalculatorController(
            CalculatorContext context,
            KafkaProducerService<Null, string> producer,
            IConfiguration configuration)
        {
            _context = context;
            _producer = producer;
            _topic = configuration["Kafka:TopicName"]
                ?? throw new InvalidOperationException("Kafka:TopicName is not configured.");
        }

        public IActionResult Index()
        {
            var data = _context.DataInputVariants
                .OrderByDescending(x => x.ID_DataInputVariant)
                .ToList();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(double num1, double num2, Operation operation)
        {
            if (operation == Operation.Divide && num2 == 0)
            {
                TempData["Error"] = "Делить на ноль нельзя.";
                return RedirectToAction(nameof(Index));
            }

            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = num1,
                Operand_2 = num2,
                Type_operation = operation
            };

            var json = JsonSerializer.Serialize(dataInputVariant);

            await _producer.ProduceAsync(
                _topic,
                new Message<Null, string> { Value = json });

            TempData["Info"] = $"Запрос отправлен в Kafka (топик {_topic}). Результат появится после обработки сообщения.";
            return RedirectToAction(nameof(Index));
        }

        // Kafka Consumer вызывает этот метод после вычисления результата.
        [HttpPost]
        public async Task<IActionResult> Callback([FromBody] DataInputVariant inputData)
        {
            if (inputData == null)
                return BadRequest();

            _context.DataInputVariants.Add(inputData);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
