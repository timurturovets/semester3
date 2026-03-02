using System.Globalization;

namespace task6;

public static class Task6
{
    public static void Run(string[] args)
    {
        Console.WriteLine("z1 = a0 + a1*i, z2 = b0 + b1*i");

        var a0 = ReadDouble("Введите a0: ");
        var a1 = ReadDouble("Введите a1: ");

        double b0;
        double b1;
        while (true)
        {
            b0 = ReadDouble("Введите b0: ");
            b1 = ReadDouble("Введите b1: ");
            if (b0 != 0.0 || b1 != 0.0)
            {
                break;
            }

            Console.WriteLine("Ошибка: z2 не должно быть нулевым комплексным числом. Повторите ввод b0 и b1.");
        }

        var (realPart, imaginaryPart) = DivideWithAtMostSevenMulDiv(a0, a1, b0, b1);

        Console.WriteLine();
        Console.WriteLine($"z1 = {FormatComplex(a0, a1)}");
        Console.WriteLine($"z2 = {FormatComplex(b0, b1)}");
        Console.WriteLine($"z1 / z2 = {FormatComplex(realPart, imaginaryPart)}");
        Console.WriteLine("Использовано 5 умножений и 2 деления = 7 операций умножения/деления.");
    }

    private static (double realPart, double imaginaryPart) DivideWithAtMostSevenMulDiv(
        double a0,
        double a1,
        double b0,
        double b1)
    {
        var p = a0 * b0;
        var q = a1 * b1;
        var r = (a0 + a1) * (b0 - b1);
        var s = b0 * b0;
        var t = b1 * b1;

        var denominator = s + t;
        var realPart = (p + q) / denominator;
        var imaginaryPart = (r - p + q) / denominator;

        return (realPart, imaginaryPart);
    }

    private static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (double.TryParse(input, out var value))
            {
                return value;
            }

            Console.WriteLine("Некорректное число. Повторите ввод.");
        }
    }

    private static string FormatComplex(double realPart, double imaginaryPart)
    {
        var sign = imaginaryPart < 0 ? "-" : "+";
        return $"{realPart} {sign} {Math.Abs(imaginaryPart)}i";
    }
}
