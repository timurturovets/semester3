using System.Diagnostics;

namespace Gmurman;

internal class Task13
{
    internal static void Run()
    {
        const int total = 100;
        const int taken = 10;
        const int experiments = 10_000_000;

        var success = 0;
        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var target = rnd.Next(total);

            var selected = new bool[total];
            var count = 0;

            while (count < taken)
            {
                var index = rnd.Next(total);
                if (selected[index]) continue;
                
                selected[index] = true;
                count++;
            }

            if (selected[target]) success++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) success / experiments}");
    }
}