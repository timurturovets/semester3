using Helpers;

namespace Knuth3;

public class Task12
{
    public static void Run(string[] args)
    {
        Polynomial polynomial;
        
        Console.Write("Введите через пробел коэффициенты многочлена от меньшей степени к большей: ");
        while (!Polynomial.TryParse(Console.ReadLine(), out polynomial!))
        {
            Console.Write("Некорректный ввод. Введите заново: ");
        }

        double x0;
        Console.Write("Введите x0: ");
        while(!double.TryParse(Console.ReadLine(), out x0)) Console.Write("Некорректный ввод. Введите заново: ");
        
        double x;
        Console.Write("Введите x: ");
        while(!double.TryParse(Console.ReadLine(), out x)) Console.Write("Некорректный ввод. Введите заново: ");
        
        var shifted = Shift(polynomial, x0);
        
        Console.WriteLine($"u(x+{x0}) = ");
        for (var i = 0; i <= shifted.Degree; i++)
        {
            Console.WriteLine($"x^{i}: {shifted[i]}");
        }
        
        Console.WriteLine($"Исходный многочлен u({x}+{x0}) = {polynomial.Evaluate(x + x0)}");
        Console.WriteLine($"Смещённый многочлен shifted({x}) = {shifted.Evaluate(x)}");
    }

    private static Polynomial Shift(Polynomial u, double x0)
    {
        var n = u.Degree;

        var v = new List<Polynomial>();

        for (var j = 0; j <= n; j++)
        {
            v.Add(new Polynomial([u[j]]));
        }

        var result = v[n];
        var xPlusX0 = new Polynomial([x0, 1]);
        
        for (var j = n - 1; j >= 0; j--)
        {
            result = Polynomial.MultiplyKaratsuba(result, xPlusX0) + v[j];
        }

        return result;
    }
}