namespace Gmurman;

internal class Task30
{
    internal static void Run()
    {
        const double a = 1.0;
        const double r = 0.4;
        const int experiments = 1_000_000;

        var hits = 0;
        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var x = rnd.NextDouble() * a;
            var y = rnd.NextDouble() * a;

            if (x >= r && x <= a - r && y >= r && y <= a - r) hits++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) hits / experiments}");
    }
}