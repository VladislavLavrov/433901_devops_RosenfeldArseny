using System.ComponentModel.DataAnnotations;

namespace _15_Calculator.Models
{
    public enum Operation
    {
        [Display(Name = "Сложение")]
        Add = 1,
        [Display(Name = "Вычитание")]
        Subtract = 2,
        [Display(Name = "Умножение")]
        Multiply = 3,
        [Display(Name = "Деление")]
        Divide = 4
    }
}