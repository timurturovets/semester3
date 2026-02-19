namespace Task4;

public class Task4
{
    public static void Run(string[] args)
    {
        const int experiments = 1_000_000;
        
        if (args.Length < 1)
        {
            Console.WriteLine("Необходимо ввести параметр <k> в командную строку при запуске");
            return;
        }

        if (!int.TryParse(args[0], out var k))
        {
            Console.WriteLine("Некорректный параметр <k>");
            return;
        }

        var successBeforeK = 0;
        var evenCount = 0;

        var rnd = new Random();
        for (var i = 0; i < experiments; i++)
        {
            var throws = 0;
            var prevSide = rnd.Next(2);
            int side1, side2;
            do
            {
                side1 = prevSide;
                side2 = rnd.Next(2);
                throws++;

                prevSide = side2;
            } while (side1 != side2);
            
            if (throws < k) successBeforeK++;
            if (throws % 2 == 0) evenCount++;
        }
        
        Console.WriteLine("Вычисленные вероятности:");
        Console.WriteLine($"Опыт закончится до k-го бросания монеты: {(double) successBeforeK / experiments}");
        Console.WriteLine($"Потребовалось чётное число бросаний монеты: {(double) evenCount / experiments}");
    }
}