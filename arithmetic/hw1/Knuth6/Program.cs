namespace Knuth6;

public class Task15
{
    public static void Run(string[] args)
    {
        int n;
        Console.Write("Введите степень n (целое >= 0): ");
        while (!int.TryParse(Console.ReadLine(), out n) || n < 0) Console.Write("Некорректный ввод. Введите заново: ");
        

        var coefficients = new double[n + 1];
        for (var k = 0; k <= n; k++)
        {
            Console.Write($"Введите коэффициент u_{k}: ");
            while (!double.TryParse(Console.ReadLine(), out coefficients[k]))
            {
                Console.Write($"Некорректный ввод. Введите заново: ");
            }
        }

        Console.Write("Введите значение x: ");
        double x;
        while (!double.TryParse(Console.ReadLine(), out x))
        {
            Console.Write("Некорректный ввод. Введите заново: ");
        }

        var (result, multiplications, additions) = EvaluateFactorialPolynomial(coefficients, x);

        Console.WriteLine("Значение многочлена:");
        Console.WriteLine($"P(x) = {result}");

        Console.WriteLine($"Количество умножений: {multiplications}");
        Console.WriteLine($"Количество сложений: {additions}");
    }

    private static (double result, int multiplications, int additions)
        EvaluateFactorialPolynomial(double[] coefficients, double x)
    {
        var n = coefficients.Length - 1;
        var result = coefficients[n];

        var multiplications = 0;
        var additions = 0;

        for (var k = n - 1; k >= 0; k--)
        {
            result = coefficients[k] + (x - k) * result;
            multiplications++;
            additions += 2;
        }

        additions--;

        return (result, multiplications, additions);
    }
}