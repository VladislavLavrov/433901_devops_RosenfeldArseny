using System.ComponentModel.DataAnnotations;

namespace Calculator.Models;

public class CalculatorViewModel
{
    [Display(Name = "Первое число")]
    public double? FirstNumber { get; set; }

    [Display(Name = "Второе число")]
    public double? SecondNumber { get; set; }

    [Display(Name = "Операция")]
    public string Operation { get; set; } = "+";

    public double? Result { get; set; }

    public string? Error { get; set; }
}
