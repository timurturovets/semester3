namespace Gmurman;

internal class Task21
{
    internal static void Run()
    {
        const int total = 5;
        const int painted = 3;
        const int taken = 2;
        const int experiments = 1_000_000;

        var onePainted = 0;
        var twoPainted = 0;
        var atLeastOne = 0;

        var rnd = new Random();

        for (var i = 0; i < experiments; i++)
        {
            var items = new bool[total];
            for (var j = 0; j < painted; j++) items[j] = true;

            var selected = new bool[total];

            var count = 0;
            while (count < taken)
            {
                var index = rnd.Next(total);
                if (selected[index]) continue;
                
                selected[index] = true;
                count++;
            }

            var paintedCount = 0;
            for (var j = 0; j < total; j++)
            {
                if (selected[j] && items[j]) paintedCount++;
            }

            switch (paintedCount)
            {
                case 1: onePainted++; break;
                case 2: twoPainted++; break;
            }
            
            if (paintedCount >= 1) atLeastOne++;
        }
        
        Console.WriteLine("Вычисленные вероятности:");
        Console.WriteLine($"Одно окрашенное: {(double) onePainted / experiments}");
        Console.WriteLine($"Два окрашенных: {(double) twoPainted / experiments}");
        Console.WriteLine($"Хотя бы одно: {(double) atLeastOne / experiments}");
        
    }
}