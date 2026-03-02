using helpers;

namespace task4;

public static class Task4
{
    public static void Run(string[] args)
    {
        const int n = 8;
        Console.WriteLine("Расширенный алгоритм Евклида для GF(2^n).");

        var modulus = BinaryPolynomial.ReadPolynomial(
            $"Введите неприводимый многочлен f(x) степени {n} (по умолчанию: x^8 + x^4 + x^3 + x + 1): ",
            expectedDegree: n,
            defaultValue: "x^8 + x^4 + x^3 + x + 1");
        var field = new Gf2Field(n, modulus);

        var element = ReadNonZeroElement(n);

        try
        {
            var inverse = field.InverseElementExtendedEuclid(element.Coefficients);
            var inversePolynomial = Gf2Field.ElementToPolynomial(inverse);
            var check = field.MultiplyElements(element.Coefficients, inverse);

            Console.WriteLine();
            Console.WriteLine($"f(x) = {field.Modulus}");
            Console.WriteLine($"a(x) = {element} (биты: {ToBinaryString(element.Coefficients)})");
            Console.WriteLine($"a^(-1)(x) = {inversePolynomial} (биты: {ToBinaryString(inverse)})");
            Console.WriteLine($"Проверка: a(x) * a^(-1)(x) mod f(x) = {Gf2Field.ElementToPolynomial(check)} (биты: {ToBinaryString(check)})");
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine($"Не удалось найти обратный элемент: {e.Message}");
        }
    }

    private static BinaryPolynomial ReadNonZeroElement(int n)
    {
        while (true)
        {
            var element = BinaryPolynomial.ReadPolynomial(
                $"Введите ненулевой элемент a(x) степени не выше {n - 1} (по умолчанию: x^6 + x + 1): ",
                maxDegree: n - 1,
                defaultValue: "x^6 + x + 1");

            if (element.Coefficients != 0UL)
            {
                return element;
            }

            Console.WriteLine("Ошибка: нулевой элемент не имеет мультипликативной обратной.");
        }
    }

    private static string ToBinaryString(ulong value)
    {
        if (value == 0UL)
        {
            return "0";
        }

        var chars = new char[64];
        var index = 64;
        while (value > 0UL)
        {
            chars[--index] = (value & 1UL) == 1UL ? '1' : '0';
            value >>= 1;
        }

        return new string(chars, index, 64 - index);
    }
}
