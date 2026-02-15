using Helpers;

namespace task8;

// ReSharper disable file InconsistentNaming
public class Task8
{
    public static void Run(string[] args)
    {
        int n;
        Console.Write("Введите степень по x: ");
        while (!int.TryParse(Console.ReadLine(), out n))
        {
            Console.Write("Некорректный ввод. Введите заново: ");
        }
        
        int m;
        Console.Write("Введите степень по y: ");
        while (!int.TryParse(Console.ReadLine(), out m))
        {
            Console.Write("Некорректный ввод. Введите заново: ");
        }

        var f = GenerateRandomPolynomial2D(n, m);
        var g = GenerateRandomPolynomial2D(n, m);

        OperationCounter.Reset();
        Polynomial2D.MultiplyClassic2D(f, g);
        var classicOperations = OperationCounter.Total();
        
        OperationCounter.Reset();
        Polynomial2D.MultiplyKaratsuba2D(f, g);
        var karatsubaOperations = OperationCounter.Total();
        
        Console.WriteLine($"Количество операций при перемножении многочленов классическим методом: {classicOperations}");
        Console.WriteLine($"Количество операций при перемножении многочленов методом Карацубы: {karatsubaOperations}");
        Console.WriteLine($"Методом Карацубы использовано на {classicOperations - karatsubaOperations} меньше операций");
        
        Console.WriteLine($"Классический метод: ~ O(n^2 m^2) = {Math.Pow(n, 2) * Math.Pow(m, 2)}");
        Console.WriteLine($"Метод Карацубы: ~ O(n^1.585 m^1.585) = {Math.Pow(n, 1.585) * Math.Pow(m, 1.585):F3}");
    }

    private static Polynomial2D GenerateRandomPolynomial2D(int n, int m)
    {
        var rand = new Random();
        var coefficients = new Polynomial[n];

        for (var i = 0; i < n; i++)
        {
            var inner = new double[m];
            for (var j = 0; j < m; j++)
            {
                inner[j] = rand.Next(1, 5);
            }

            coefficients[i] = new Polynomial(inner);
        }
        
        return new Polynomial2D(coefficients);
    }
}