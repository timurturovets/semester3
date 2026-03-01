using helpers;

namespace task1;

public static class Task1
{
    public static void Run(string[] args)
    {
        const int n = 8;
        var irreducible = ReadPolynomial(
            $"Введите модульный многочлен степени {n}: ", expectedDegree: n);
        var field = new Gf2Field(n, irreducible);

        var element = ReadElement(
            $"Введите элемент в битовой форме (до {n} бит, например 10110110): ",
            n);
        var polynomialForm = Gf2Field.ElementToPolynomial(element);
        var restoredElement = field.PolynomialToElement(polynomialForm);

        Console.WriteLine();
        Console.WriteLine($"Поле GF(2^{n})");
        Console.WriteLine($"Модульный многочлен f(x): {field.Modulus}");
        Console.WriteLine($"Элемент (битовая форма): {ToBinaryString(element)}");
        Console.WriteLine($"Элемент (полиномиальная форма): {polynomialForm}");
        Console.WriteLine($"Обратное преобразование в биты: {ToBinaryString(restoredElement)}");

        var fromString = ReadPolynomial(
            "Введите многочлен элемента для перевода в биты (например: x^7 + x^5 + x^3 + x^2 + x): ");
        var fromStringValue = field.PolynomialToElement(fromString);
        Console.WriteLine($"Многочлен '{fromString}' в битовой форме (по модулю f): {ToBinaryString(fromStringValue)}");
    }

    private static BinaryPolynomial ReadPolynomial(
        string prompt,
        int? expectedDegree = null,
        string? defaultValue = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) input = defaultValue;

            try
            {
                var polynomial = BinaryPolynomial.Parse(input ?? string.Empty);
                if (!expectedDegree.HasValue || polynomial.Degree == expectedDegree.Value) return polynomial;
                Console.WriteLine($"Ошибка: степень многочлена должна быть равна {expectedDegree.Value}.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Некорректный ввод многочлена: {e.Message}");
            }
        }
    }

    private static ulong ReadElement(string prompt, int maxBits)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = (Console.ReadLine() ?? string.Empty).Replace(" ", string.Empty);

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Ошибка: введите непустую битовую строку.");
                continue;
            }

            if (input.Length > maxBits)
            {
                Console.WriteLine($"Ошибка: длина битовой строки не должна превышать {maxBits}.");
                continue;
            }

            if (input.Any(c => c is not ('0' or '1')))
            {
                Console.WriteLine("Ошибка: используйте только символы 0 и 1.");
                continue;
            }

            var value = 0UL;
            foreach (var bit in input)
            {
                value <<= 1;
                if (bit == '1')
                {
                    value |= 1UL;
                }
            }

            return value;
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
