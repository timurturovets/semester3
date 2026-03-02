using System.Numerics;

namespace helpers;

public sealed class Gf2Field
{
    public int Degree { get; }
    public BinaryPolynomial Modulus { get; }

    public Gf2Field(int degree, BinaryPolynomial modulus)
    {
        if (degree is < 2 or > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(degree), "Степень должна быть в диапазоне [2, 64].");
        }

        if (modulus.Degree != degree)
        {
            throw new ArgumentException("Степень модульного многочлена должна совпадать со степенью поля.", nameof(modulus));
        }

        Degree = degree;
        Modulus = modulus;
    }

    public static BinaryPolynomial ElementToPolynomial(ulong element)
    {
        return new BinaryPolynomial(element);
    }

    public ulong PolynomialToElement(BinaryPolynomial polynomial)
    {
        var reduced = Reduce(polynomial.Coefficients);
        return reduced;
    }

    public ulong MultiplyElements(ulong left, ulong right)
    {
        if (Degree == 64)
        {
            throw new NotSupportedException("Умножение для степени 64 не поддерживается текущим представлением коэффициентов.");
        }

        var modulusValue = Modulus.Coefficients;
        var mask = (1UL << Degree) - 1UL;

        var a = Reduce(left);
        var b = Reduce(right);
        ulong result = 0UL;

        for (var i = 0; i < Degree; i++)
        {
            if ((b & 1UL) == 1UL)
            {
                result ^= a;
            }

            b >>= 1;
            var carry = ((a >> (Degree - 1)) & 1UL) == 1UL;
            a <<= 1;
            if (carry)
            {
                a ^= modulusValue;
            }

            a &= mask;
        }

        return result & mask;
    }

    public ulong InverseElementExtendedEuclid(ulong element)
    {
        var reducedElement = Reduce(element);
        if (reducedElement == 0UL)
        {
            throw new ArgumentException("Нулевой элемент не имеет мультипликативной обратной.", nameof(element));
        }

        var modulus = new BigInteger(Modulus.Coefficients);
        var a = new BigInteger(reducedElement);

        var (gcd, _, coefficientForElement) = ExtendedEuclid(modulus, a);
        if (gcd != BigInteger.One)
        {
            throw new InvalidOperationException("Элемент не имеет обратного в данном поле.");
        }

        var inverse = ModuloPolynomial(coefficientForElement, modulus);
        return (ulong)inverse;
    }

    private ulong Reduce(ulong polynomial)
    {
        if (Degree == 64)
        {
            return polynomial;
        }

        var modulusValue = Modulus.Coefficients;
        for (var i = 63; i >= Degree; i--)
        {
            if (((polynomial >> i) & 1UL) == 0UL)
            {
                continue;
            }

            var shift = i - Degree;
            polynomial ^= modulusValue << shift;
        }

        var mask = (1UL << Degree) - 1UL;
        return polynomial & mask;
    }

    private static (BigInteger gcd, BigInteger coefficientForModulus, BigInteger coefficientForElement) ExtendedEuclid(
        BigInteger modulus,
        BigInteger element)
    {
        var r0 = modulus;
        var r1 = element;
        var s0 = BigInteger.One;
        var s1 = BigInteger.Zero;
        var t0 = BigInteger.Zero;
        var t1 = BigInteger.One;

        while (r1 != BigInteger.Zero)
        {
            var quotient = DividePolynomials(r0, r1, out var remainder);

            var nextR = remainder;
            var nextS = s0 ^ MultiplyPolynomials(quotient, s1);
            var nextT = t0 ^ MultiplyPolynomials(quotient, t1);

            r0 = r1;
            r1 = nextR;
            s0 = s1;
            s1 = nextS;
            t0 = t1;
            t1 = nextT;
        }

        return (r0, s0, t0);
    }

    private static BigInteger MultiplyPolynomials(BigInteger left, BigInteger right)
    {
        var a = left;
        var b = right;
        var result = BigInteger.Zero;

        while (b != BigInteger.Zero)
        {
            if (!b.IsEven)
            {
                result ^= a;
            }

            a <<= 1;
            b >>= 1;
        }

        return result;
    }

    private static BigInteger DividePolynomials(BigInteger dividend, BigInteger divisor, out BigInteger remainder)
    {
        if (divisor == BigInteger.Zero)
        {
            throw new DivideByZeroException("Деление многочлена на ноль невозможно.");
        }

        remainder = dividend;
        var quotient = BigInteger.Zero;
        var divisorDegree = GetDegree(divisor);

        while (remainder != BigInteger.Zero && GetDegree(remainder) >= divisorDegree)
        {
            var shift = GetDegree(remainder) - divisorDegree;
            quotient ^= BigInteger.One << shift;
            remainder ^= divisor << shift;
        }

        return quotient;
    }

    private static BigInteger ModuloPolynomial(BigInteger value, BigInteger modulus)
    {
        _ = DividePolynomials(value, modulus, out var remainder);
        return remainder;
    }

    private static int GetDegree(BigInteger polynomial)
    {
        if (polynomial == BigInteger.Zero)
        {
            return -1;
        }

        var degree = -1;
        var value = polynomial;
        while (value != BigInteger.Zero)
        {
            value >>= 1;
            degree++;
        }

        return degree;
    }
}
