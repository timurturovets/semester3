using System.Globalization;

namespace task7;

public static class Task7
{
    public static void Run(string[] args)
    {
        var n = ReadN();
        var maxValue = 10 * n;

        var setA = ReadSet($"Введите {n} различных целых для множества A (диапазон [0, {maxValue}]): ", n, maxValue);
        var setB = ReadSet($"Введите {n} различных целых для множества B (диапазон [0, {maxValue}]): ", n, maxValue);

        var aCoefficients = BuildIndicatorPolynomial(setA, maxValue);
        var bCoefficients = BuildIndicatorPolynomial(setB, maxValue);
        var sumCounts = Convolve(aCoefficients, bCoefficients);

        Console.WriteLine();
        Console.WriteLine($"A = {{{string.Join(", ", setA.OrderBy(x => x))}}}");
        Console.WriteLine($"B = {{{string.Join(", ", setB.OrderBy(x => x))}}}");

        var cartesianSum = new List<int>();
        for (var value = 0; value < sumCounts.Length; value++)
        {
            if (sumCounts[value] > 0)
            {
                cartesianSum.Add(value);
            }
        }

        Console.WriteLine($"C = A (+) B = {{{string.Join(", ", cartesianSum)}}}");
        Console.WriteLine("Кратности (сколько пар (x, y) дают каждую сумму):");

        foreach (var value in cartesianSum)
        {
            Console.WriteLine($"{value}: {sumCounts[value]}");
        }
    }

    private static int ReadN()
    {
        while (true)
        {
            Console.Write("Введите n (размер множеств A и B): ");
            var input = Console.ReadLine();
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ||
                int.TryParse(input, NumberStyles.Integer, CultureInfo.CurrentCulture, out n))
            {
                if (n > 0)
                {
                    return n;
                }
            }

            Console.WriteLine("Ошибка: n должно быть положительным целым числом.");
        }
    }

    private static HashSet<int> ReadSet(string prompt, int expectedSize, int maxValue)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine() ?? string.Empty;
            var parts = input.Split([' ', '\t', ',', ';'], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != expectedSize)
            {
                Console.WriteLine($"Ошибка: нужно ввести ровно {expectedSize} чисел.");
                continue;
            }

            var values = new HashSet<int>();
            var ok = true;
            foreach (var part in parts)
            {
                if (!(int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ||
                      int.TryParse(part, NumberStyles.Integer, CultureInfo.CurrentCulture, out value)))
                {
                    Console.WriteLine($"Ошибка: '{part}' не является целым числом.");
                    ok = false;
                    break;
                }

                if (value < 0 || value > maxValue)
                {
                    Console.WriteLine($"Ошибка: значение {value} вне диапазона [0, {maxValue}].");
                    ok = false;
                    break;
                }

                if (!values.Add(value))
                {
                    Console.WriteLine($"Ошибка: значение {value} повторяется. Требуется множество из различных элементов.");
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                return values;
            }
        }
    }

    private static int[] BuildIndicatorPolynomial(IEnumerable<int> set, int maxValue)
    {
        var coefficients = new int[maxValue + 1];
        foreach (var value in set)
        {
            coefficients[value] = 1;
        }

        return coefficients;
    }

    private static long[] Convolve(int[] left, int[] right)
    {
        var result = new long[left.Length + right.Length - 1];

        for (var i = 0; i < left.Length; i++)
        {
            if (left[i] == 0)
            {
                continue;
            }

            for (var j = 0; j < right.Length; j++)
            {
                if (right[j] == 0)
                {
                    continue;
                }

                result[i + j] += (long)left[i] * right[j];
            }
        }

        return result;
    }
}
