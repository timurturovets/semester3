using System.Globalization;
using System.Text;

namespace helpers;

public class BinaryPolynomial(ulong coefficients)
{

    public ulong Coefficients { get; } = coefficients;

    public int Degree
    {
        get
        {
            if (Coefficients == 0UL)
            {
                return -1;
            }

            for (var i = 63; i >= 0; i--)
            {
                if (((Coefficients >> i) & 1UL) == 1UL)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    public bool HasTerm(int power)
    {
        if (power is < 0 or > 63)
        {
            throw new ArgumentOutOfRangeException(nameof(power), "Степень должна быть в диапазоне [0, 63].");
        }

        return ((Coefficients >> power) & 1UL) == 1UL;
    }

    public override string ToString()
    {
        if (Coefficients == 0UL)
        {
            return "0";
        }

        var sb = new StringBuilder();
        var first = true;

        for (var power = 63; power >= 0; power--)
        {
            if (!HasTerm(power))
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

    public static BinaryPolynomial Parse(string polynomial)
    {
        if (string.IsNullOrWhiteSpace(polynomial))
        {
            throw new ArgumentException("Строка многочлена пуста.", nameof(polynomial));
        }

        var normalized = polynomial.Replace(" ", string.Empty);
        var parts = normalized.Split('+', StringSplitOptions.RemoveEmptyEntries);

        var value = 0UL;
        foreach (var part in parts)
        {
            var power = part switch
            {
                "1" => 0,
                "x" or "X" => 1,
                _ when part.StartsWith("x^", StringComparison.OrdinalIgnoreCase)
                    => int.Parse(part[2..], CultureInfo.InvariantCulture),
                _ => throw new FormatException()
            };

            if (power is < 0 or > 63)
            {
                throw new FormatException("Степень должна быть в диапазоне [0, 63].");
            }

            value ^= 1UL << power;
        }

        return new BinaryPolynomial(value);
    }

    public static BinaryPolynomial ReadPolynomial(
        string prompt,
        int? maxDegree = null,
        int? expectedDegree = null,
        string? defaultValue = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                input = defaultValue;
            }

            try
            {
                var polynomial = Parse(input ?? string.Empty);
                if (expectedDegree.HasValue && polynomial.Degree != expectedDegree.Value)
                {
                    Console.WriteLine($"Ошибка: степень многочлена должна быть равна {expectedDegree.Value}.");
                    continue;
                }

                if (maxDegree.HasValue && polynomial.Degree > maxDegree.Value)
                {
                    Console.WriteLine($"Ошибка: степень многочлена должна быть не выше {maxDegree.Value}.");
                    continue;
                }

                return polynomial;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Некорректный ввод многочлена: {e.Message}");
            }
        }
    }

    public static UInt128 MultiplyUpTo32(BinaryPolynomial left, BinaryPolynomial right)
    {
        if (left.Degree > 32 || right.Degree > 32)
        {
            throw new ArgumentException("Степень каждого множителя должна быть не выше 32.");
        }

        UInt128 result = 0;
        var leftValue = left.Coefficients;
        var rightValue = (UInt128)right.Coefficients;
        for (var i = 0; i <= 32; i++)
        {
            if (((leftValue >> i) & 1UL) == 0UL)
            {
                continue;
            }

            result ^= rightValue << i;
        }

        return result;
    }
}
