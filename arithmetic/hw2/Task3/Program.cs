using helpers;

namespace task3;

public static class Task3
{
    public static void Run(string[] args)
    {
        const int n = 4;
        Console.WriteLine($"Демонстрация умножения элементов в GF(2^{n}) с разными неприводимыми многочленами.");

        var left = BinaryPolynomial.ReadPolynomial(
            "Введите первый элемент A(x) степени не выше 3 (по умолчанию: x^3 + x + 1): ",
            maxDegree: n - 1,
            defaultValue: "x^3 + x + 1");
        var right = BinaryPolynomial.ReadPolynomial(
            "Введите второй элемент B(x) степени не выше 3 (по умолчанию: x^3 + x^2 + 1): ",
            maxDegree: n - 1,
            defaultValue: "x^3 + x^2 + 1");

        var leftElement = left.Coefficients;
        var rightElement = right.Coefficients;

        var irreducibleModuli = new[]
        {
            BinaryPolynomial.Parse("x^4 + x + 1"),
            BinaryPolynomial.Parse("x^4 + x^3 + 1")
        };

        Console.WriteLine();
        Console.WriteLine($"A(x) = {left} (биты: {ToBinaryString(leftElement)})");
        Console.WriteLine($"B(x) = {right} (биты: {ToBinaryString(rightElement)})");
        Console.WriteLine("Результаты умножения для разных неприводимых многочленов:");

        foreach (var modulus in irreducibleModuli)
        {
            var field = new Gf2Field(n, modulus);
            var product = field.MultiplyElements(leftElement, rightElement);
            var productPolynomial = Gf2Field.ElementToPolynomial(product);

            Console.WriteLine();
            Console.WriteLine($"f(x) = {modulus}");
            Console.WriteLine($"A(x) * B(x) mod f(x) = {productPolynomial}");
            Console.WriteLine($"Битовая форма результата: {ToBinaryString(product)}");
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
