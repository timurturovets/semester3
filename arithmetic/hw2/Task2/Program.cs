using System.Globalization;
using System.Text;
using helpers;

namespace task2;

public static class Task2
{
    public static void Run(string[] args)
    {
        var left = BinaryPolynomial.ReadPolynomial(
            "Введите первый многочлен A(x): ",
            maxDegree: 32);
        var right = BinaryPolynomial.ReadPolynomial(
            "Введите второй многочлен B(x): ",
            maxDegree: 32);

        var product = BinaryPolynomial.MultiplyUpTo32(left, right);

        Console.WriteLine();
        Console.WriteLine($"A(x) = {left}");
        Console.WriteLine($"B(x) = {right}");
        Console.WriteLine($"A(x) * B(x) = {ToPolynomialString(product)}");
        Console.WriteLine($"Битовая форма результата: {ToBinaryString(product)}");
    }

    private static string ToBinaryString(UInt128 value)
    {
        if (value == (UInt128)0)
        {
            return "0";
        }

        var chars = new char[128];
        var index = 128;
        while (value > (UInt128)0)
        {
            chars[--index] = (value & (UInt128)1) == (UInt128)1 ? '1' : '0';
            value >>= 1;
        }

        return new string(chars, index, 128 - index);
    }

    private static string ToPolynomialString(UInt128 coefficients)
    {
        if (coefficients == (UInt128)0)
        {
            return "0";
        }

        var sb = new StringBuilder();
        var first = true;
        for (var power = 127; power >= 0; power--)
        {
            if (((coefficients >> power) & (UInt128)1) == (UInt128)0)
            {
                continue;
            }

            if (!first)
            {
                sb.Append(" + ");
            }

            first = false;
            if (power == 0)
            {
                sb.Append('1');
            }
            else if (power == 1)
            {
                sb.Append('x');
            }
            else
            {
                sb.Append("x^");
                sb.Append(power.ToString(CultureInfo.InvariantCulture));
            }
        }

        return sb.ToString();
    }
}
