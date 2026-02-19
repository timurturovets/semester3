namespace Gmurman;

internal class Task45
{
    internal static void Run()
    {
        const int experiments = 1_000_000;
        var hits = 0;
        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var x = rnd.NextDouble();
            var y = rnd.NextDouble();

            if (x + y <= 1 && x * y >= 0.09) hits++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) hits / experiments}");
    }
}