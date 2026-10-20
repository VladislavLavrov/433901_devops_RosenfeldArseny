using _15_Calculator.Models;

namespace _15_Calculator.Models
{
    public class CalculatorLibrary
    {
        public static double CalculateOperation(double num1, double num2, Operation operation)
        {
            return operation switch
            {
                Operation.Add => num1 + num2,
                Operation.Subtract => num1 - num2,
                Operation.Multiply => num1 * num2,
                Operation.Divide => num1 / num2,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), "Invalid operation"),
            };
        }
    }
}