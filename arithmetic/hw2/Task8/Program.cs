using System.Globalization;
using System.Numerics;

namespace task8;

public static class Task8
{
    public static void Run(string[] args)
    {
        Console.WriteLine("Многомерное ДПФ через последовательные одномерные БПФ.");
        Console.WriteLine("Каждая размерность n_i должна быть степенью двойки.");

        var dimensions = ReadDimensions();
        var d = dimensions.Length;
        var n = ComputeTotalSize(dimensions);

        var input = GenerateSampleData(n);
        Console.WriteLine();
        Console.WriteLine($"Размерности: ({string.Join(", ", dimensions)})");
        Console.WriteLine($"Общее число элементов n = {n}");

        var naturalOrder = Enumerable.Range(0, d).ToArray();
        var reverseOrder = naturalOrder.Reverse().ToArray();

        var naturalResult = MultiDimensionalFft(input, dimensions, naturalOrder);
        var reverseResult = MultiDimensionalFft(input, dimensions, reverseOrder);

        Console.WriteLine();
        Console.WriteLine("а) Последовательные 1D БПФ по измерениям:");
        for (var axis = 0; axis < d; axis++)
        {
            var lineCount = n / dimensions[axis];
            Console.WriteLine($"Ось {axis + 1}: {lineCount} отдельных 1D БПФ длины {dimensions[axis]}");
        }

        Console.WriteLine();
        Console.WriteLine("б) Проверка независимости от порядка измерений:");
        Console.WriteLine($"Порядок 1: ({string.Join(", ", naturalOrder.Select(x => x + 1))})");
        Console.WriteLine($"Порядок 2: ({string.Join(", ", reverseOrder.Select(x => x + 1))})");
        var orderDifference = MaxDifference(naturalResult, reverseResult);
        Console.WriteLine($"Максимальное |Y1 - Y2| = {orderDifference:E6}");

        if (n <= 256)
        {
            var direct = DirectMultiDimensionalDft(input, dimensions);
            var directDiff = MaxDifference(naturalResult, direct);
            Console.WriteLine($"Проверка с прямым d-мерным ДПФ: max |Yfft - Ydirect| = {directDiff:E6}");
        }

        Console.WriteLine();
        Console.WriteLine("в) Оценка сложности при использовании БПФ:");
        var logSum = 0.0;
        for (var axis = 0; axis < d; axis++)
        {
            var logNi = Math.Log2(dimensions[axis]);
            logSum += logNi;
            Console.WriteLine($"Ось {axis + 1}: O(n log n_{axis + 1}) = O({n} * {logNi:G6})");
        }

        var logN = Math.Log2(n);
        Console.WriteLine($"sum log2(n_i) = {logSum:G6}, log2(n) = {logN:G6}");
        Console.WriteLine("Следовательно sum O(n log n_i) = O(n sum log n_i) = O(n log n), независимо от d.");
    }

    private static int[] ReadDimensions()
    {
        while (true)
        {
            Console.Write("Введите число измерений d: ");
            var dInput = Console.ReadLine();
            if (!int.TryParse(dInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out var d) &&
                !int.TryParse(dInput, NumberStyles.Integer, CultureInfo.CurrentCulture, out d))
            {
                Console.WriteLine("Ошибка: d должно быть целым числом.");
                continue;
            }

            if (d <= 0)
            {
                Console.WriteLine("Ошибка: d должно быть положительным.");
                continue;
            }

            Console.Write($"Введите {d} размерностей n_i через пробел: ");
            var dimsInput = Console.ReadLine() ?? string.Empty;
            var parts = dimsInput.Split([' ', '\t', ',', ';'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != d)
            {
                Console.WriteLine($"Ошибка: нужно ввести ровно {d} чисел.");
                continue;
            }

            var dims = new int[d];
            var ok = true;
            for (var i = 0; i < d; i++)
            {
                if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) &&
                    !int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
                {
                    Console.WriteLine($"Ошибка: '{parts[i]}' не является целым числом.");
                    ok = false;
                    break;
                }

                if (value <= 0)
                {
                    Console.WriteLine("Ошибка: все n_i должны быть положительными.");
                    ok = false;
                    break;
                }

                if (!IsPowerOfTwo(value))
                {
                    Console.WriteLine($"Ошибка: n_{i + 1} = {value} не является степенью двойки.");
                    ok = false;
                    break;
                }

                dims[i] = value;
            }

            if (!ok)
            {
                continue;
            }

            try
            {
                var n = ComputeTotalSize(dims);
                if (n > 1_000_000)
                {
                    Console.WriteLine("Ошибка: n слишком велико для демонстрационного режима (n <= 1 000 000).");
                    continue;
                }
            }
            catch (OverflowException)
            {
                Console.WriteLine("Ошибка: произведение n_i переполняет допустимый диапазон int.");
                continue;
            }

            return dims;
        }
    }

    private static int ComputeTotalSize(IReadOnlyList<int> dimensions)
    {
        var result = 1;
        for (var i = 0; i < dimensions.Count; i++)
        {
            result = checked(result * dimensions[i]);
        }

        return result;
    }

    private static bool IsPowerOfTwo(int value)
    {
        return (value & (value - 1)) == 0;
    }

    private static Complex[] GenerateSampleData(int size)
    {
        var data = new Complex[size];
        for (var i = 0; i < size; i++)
        {
            var real = Math.Sin(0.31 * (i + 1)) + ((i % 5) - 2);
            var imag = Math.Cos(0.17 * (i + 1)) + ((i % 7) - 3) * 0.1;
            data[i] = new Complex(real, imag);
        }

        return data;
    }

    private static Complex[] MultiDimensionalFft(Complex[] input, int[] dimensions, int[] order)
    {
        var result = new Complex[input.Length];
        Array.Copy(input, result, input.Length);

        for (var i = 0; i < order.Length; i++)
        {
            ApplyFftAlongAxis(result, dimensions, order[i]);
        }

        return result;
    }

    private static void ApplyFftAlongAxis(Complex[] data, int[] dimensions, int axis)
    {
        var axisSize = dimensions[axis];
        var axisStride = 1;
        for (var i = axis + 1; i < dimensions.Length; i++)
        {
            axisStride *= dimensions[i];
        }

        var blockSize = axisSize * axisStride;
        var line = new Complex[axisSize];

        for (var blockStart = 0; blockStart < data.Length; blockStart += blockSize)
        {
            for (var offset = 0; offset < axisStride; offset++)
            {
                var start = blockStart + offset;
                for (var j = 0; j < axisSize; j++)
                {
                    line[j] = data[start + j * axisStride];
                }

                Fft1DInPlace(line);

                for (var j = 0; j < axisSize; j++)
                {
                    data[start + j * axisStride] = line[j];
                }
            }
        }
    }

    private static void Fft1DInPlace(Complex[] values)
    {
        var n = values.Length;
        if (!IsPowerOfTwo(n))
        {
            throw new ArgumentException("Длина одномерного преобразования должна быть степенью двойки.");
        }

        var bitCount = (int)Math.Log2(n);
        for (var i = 0; i < n; i++)
        {
            var j = ReverseBits(i, bitCount);
            if (j > i)
            {
                (values[i], values[j]) = (values[j], values[i]);
            }
        }

        for (var len = 2; len <= n; len <<= 1)
        {
            var angle = -2.0 * Math.PI / len;
            var wLen = new Complex(Math.Cos(angle), Math.Sin(angle));
            var half = len / 2;

            for (var i = 0; i < n; i += len)
            {
                var w = Complex.One;
                for (var j = 0; j < half; j++)
                {
                    var u = values[i + j];
                    var v = values[i + j + half] * w;
                    values[i + j] = u + v;
                    values[i + j + half] = u - v;
                    w *= wLen;
                }
            }
        }
    }

    private static int ReverseBits(int value, int bitCount)
    {
        var result = 0;
        for (var i = 0; i < bitCount; i++)
        {
            result = (result << 1) | (value & 1);
            value >>= 1;
        }

        return result;
    }

    private static Complex[] DirectMultiDimensionalDft(Complex[] input, int[] dimensions)
    {
        var n = input.Length;
        var d = dimensions.Length;
        var coords = PrecomputeCoordinates(n, dimensions);
        var result = new Complex[n];

        for (var k = 0; k < n; k++)
        {
            var kCoords = coords[k];
            var sum = Complex.Zero;
            for (var j = 0; j < n; j++)
            {
                var jCoords = coords[j];
                var phase = 0.0;
                for (var axis = 0; axis < d; axis++)
                {
                    phase += (double)jCoords[axis] * kCoords[axis] / dimensions[axis];
                }

                var angle = -2.0 * Math.PI * phase;
                var twiddle = new Complex(Math.Cos(angle), Math.Sin(angle));
                sum += input[j] * twiddle;
            }

            result[k] = sum;
        }

        return result;
    }

    private static int[][] PrecomputeCoordinates(int n, int[] dimensions)
    {
        var d = dimensions.Length;
        var strides = new int[d];
        var stride = 1;
        for (var axis = d - 1; axis >= 0; axis--)
        {
            strides[axis] = stride;
            stride *= dimensions[axis];
        }

        var coords = new int[n][];
        for (var index = 0; index < n; index++)
        {
            var tuple = new int[d];
            for (var axis = 0; axis < d; axis++)
            {
                tuple[axis] = (index / strides[axis]) % dimensions[axis];
            }

            coords[index] = tuple;
        }

        return coords;
    }

    private static double MaxDifference(Complex[] left, Complex[] right)
    {
        var max = 0.0;
        for (var i = 0; i < left.Length; i++)
        {
            var diff = (left[i] - right[i]).Magnitude;
            if (diff > max)
            {
                max = diff;
            }
        }

        return max;
    }
}
