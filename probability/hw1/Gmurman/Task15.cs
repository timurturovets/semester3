namespace Gmurman;

internal class Task15
{
    internal static void Run()
    {
        const int total = 5;
        const int worn = 2; // 0 и 1 сломаны
        const int experiments = 1_000_000;

        var success = 0;
        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var first = rnd.Next(total);
            int second;

            do second = rnd.Next(total);
            while (second == first);

            if (first >= worn && second >= worn) success++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) success / experiments}");
    }
}