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
}
