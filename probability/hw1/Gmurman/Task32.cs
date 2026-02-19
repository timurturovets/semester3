namespace Gmurman;

internal class Task32
{
    internal static void Run()
    {
        const double R = 10.0;
        const double r = 5.0;
        const int experiments = 1_000_000;

        var hits = 0;
        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var radius = Math.Sqrt(rnd.NextDouble()) * R;

            if (radius > r) hits++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) hits / experiments}");
    }
}