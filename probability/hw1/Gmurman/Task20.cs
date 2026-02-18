namespace Gmurman;

internal class Task20
{
    internal static void Run()
    {
        const int total = 12;
        const int excellent = 8;
        const int taken = 9;
        const int experiments = 1_000_000;

        var success = 0;
        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var students = new bool[total];
            for (var j = 0; j < excellent; j++) students[j] = true;

            var selected = new bool[total];
            var count = 0;
            while (count < taken)
            {
                var index = rnd.Next(total);
                if (selected[index]) continue;
                
                selected[index] = true;
                count++;
            }

            var excellentCount = 0;
            for (var j = 0; j < total; j++)
            {
                if (selected[j] && students[j]) excellentCount++;
            }

            if (excellentCount == 5) success++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) success / experiments}");
    }
}