using Calculator.Models;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Controllers;

public class CalculatorController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new CalculatorViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(CalculatorViewModel model)
    {
        if (!ModelState.IsValid || model.FirstNumber is null || model.SecondNumber is null)
        {
            model.Error = "Введите оба числа.";
            return View(model);
        }

        var first = model.FirstNumber.Value;
        var second = model.SecondNumber.Value;

        switch (model.Operation)
        {
            case "+":
                model.Result = first + second;
                break;
            case "-":
                model.Result = first - second;
                break;
            case "*":
                model.Result = first * second;
                break;
            case "/" when second != 0:
                model.Result = first / second;
                break;
            case "/":
                model.Error = "На ноль делить нельзя.";
                break;
            default:
                model.Error = "Выбрана неизвестная операция.";
                break;
        }

        return View(model);
    }
}
