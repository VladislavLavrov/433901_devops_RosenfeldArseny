using _15_Calculator.Data;
using _15_Calculator.Models;
using Microsoft.AspNetCore.Mvc;

namespace _15_Calculator.Controllers
{
    public enum Operation { Add, Subtract, Multiply, Divide }

    public class CalculatorController : Controller
    {
        private readonly CalculatorContext _context;

        public CalculatorController(CalculatorContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calculate(double num1, double num2, Operation operation)
        {
            double result = 0;
            switch (operation)
            {
                case Operation.Add: result = num1 + num2; break;
                case Operation.Subtract: result = num1 - num2; break;
                case Operation.Multiply: result = num1 * num2; break;
                case Operation.Divide: result = num1 / num2; break;
            }

            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = num1.ToString(),
                Operand_2 = num2.ToString(),
                Type_operation = operation.ToString(),
                Result = result.ToString()
            };

            _context.DataInputVariants.Add(dataInputVariant);
            _context.SaveChanges();

            ViewBag.Result = result;
            ViewBag.Num1 = num1;
            ViewBag.Num2 = num2;
            ViewBag.Operation = operation;

            return View("Index");
        }
    }
}