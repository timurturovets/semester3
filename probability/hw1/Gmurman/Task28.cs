namespace Gmurman;

internal class Task28
{
    internal static void Run()
    {
        const double R = 1.0;
        const double r = 0.5;
        const double experiments = 1_000_000;

        var hits = 0;
        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var theta = rnd.NextDouble() * 2 * Math.PI;
            var radius = Math.Sqrt(rnd.NextDouble()) * R;
            var x = radius * Math.Cos(theta);
            var y = radius * Math.Sin(theta);

            if (Math.Sqrt(x * x + y * y) <= r) hits++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) hits / experiments}");
    }
}