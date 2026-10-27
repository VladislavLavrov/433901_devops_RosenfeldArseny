using _15_Calculator.Data;
using _15_Calculator.Models;
using _15_Calculator.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace _15_Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly CalculatorContext _context;
        private readonly KafkaProducerHandler _producer;

        public CalculatorController(CalculatorContext context, KafkaProducerHandler producer)
        {
            _context = context;
            _producer = producer;
        }

        public IActionResult Index()
        {
            var data = _context.DataInputVariants.OrderByDescending(x => x.ID_DataInputVariant).ToList();
            return View(data);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(double num1, double num2, Operation operation)
        {
            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = num1,
                Operand_2 = num2,
                Type_operation = operation,
            };

            var json = JsonSerializer.Serialize(dataInputVariant);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            await _producer.ProduceAsync("rozenfeld_15", new Message<byte[], byte[]> { Value = bytes });

            return RedirectToAction(nameof(Index));
        }
    }
}