namespace Gmurman;

internal class Task22
{
    internal static void Run()
    {
        const int disks = 4;
        const int sectors = 5;
        const int experiments = 1_000_000;

        var success = 0;
        var rnd = new Random();

        var correct = new [] { 0, 1, 2, 3 };

        for (var i = 0; i < experiments; i++)
        {
            var attempt = new int[disks];
            for (var j = 0; j < disks; j++) attempt[j] = rnd.Next(sectors);

            var match = true;
            for (var j = 0; j < disks; j++)
            {
                if (attempt[j] == correct[j]) continue;
                
                match = false;
                break;
            }
            
            if (match) success++;
        }
        
        Console.WriteLine($"Вычисленная вероятность: {(double) success / experiments}");
    }
}