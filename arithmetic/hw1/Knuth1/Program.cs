using Helpers;

namespace Knuth1;

public class Task10
{
    public static void Run(string[] args)
    {
        Console.Write("Введите число в нужной системе счисления: ");
        var input = Console.ReadLine() ?? "";

        int b;
        Console.Write("Введите основание системы счисления b: ");
        while(!int.TryParse(Console.ReadLine(), out b)) Console.Write("Некорректный ввод. Введите заново: ");
        
        int v;
        Console.Write("Введите делитель (0 < v < b): ");
        while(!int.TryParse(Console.ReadLine(), out v)) Console.Write("Некорректный ввод. Введите заново: ");

        var invalidDigit = false;
        var digits = input.Select(c =>
        {
            var d = char.IsDigit(c) ? c - '0' : char.ToUpper(c) - 'A' + 10;
            if (d < b) return d;
            
            Console.WriteLine($"Цифра {c} вне системы счисления {b}. Перезапустите программу.");
            invalidDigit = true;
            return 0;
        }).ToArray();

        if (invalidDigit) return;

        var result = Division.Divide(digits, v, b);
        
        Console.WriteLine($"Результат деления: {result.ToString()}");
    }
}