namespace Gmurman;

internal class Task29
{
    internal static void Run()
    {
        const double a = 1.0;
        const double r = 0.5;
        const int experiments = 1_000_000;

        var hits = 0;
        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var x = rnd.NextDouble() * 2 * a;

            if (x >= r && x <= 2 * a - r) hits++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) hits / experiments}");
    }
}