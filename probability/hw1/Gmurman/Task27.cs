namespace Gmurman;

internal class Task27
{
    internal static void Run()
    {
        const double length = 1.0;
        const double L = length / 3.0;
        const int experiments = 1_000_000;

        var hits = 0;
        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var x = rnd.NextDouble() * length;
            if (x > L && x < length - L) hits++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) hits / experiments}");
    }
}