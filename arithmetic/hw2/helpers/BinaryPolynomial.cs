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
}
