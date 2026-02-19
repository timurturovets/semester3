namespace Task2;

// ReSharper disable file InconsistentNaming
public class Task2
{
    public static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Необходимо ввести параметры <N> и <C> в командную строку при запуске");
            return;
        }

        int N, C;
        
        if (!int.TryParse(args[0], out N))
        {
            Console.WriteLine("Некорректный параметр <N>");
            return;
        };

        if (!int.TryParse(args[1], out C))
        {
            Console.WriteLine("Некорректный параметр <C>");
            return;
        }
        
        var evenCount = N / 2;

        var firstEven = 0;
        var secondEvenNoShift = 0;
        var secondEvenShift = 0;
        var bothEvenNoShift = 0;
        var bothEvenShift = 0;

        var rnd = new Random();

        for (var i = 0; i < C; i++)
        {
            var alphabet = Enumerable.Range(1, N).ToList();
            
            var index1 = rnd.Next(alphabet.Count);
            var first = alphabet[index1];
            alphabet.RemoveAt(index1);
            
            var index2NoShift = rnd.Next(alphabet.Count);
            var secondNoShift = alphabet[index2NoShift];
            
            var secondShiftIndex = index2NoShift + 1;

            if (first % 2 == 0) firstEven++;
            if (secondNoShift % 2 == 0) secondEvenNoShift++;
            if (secondShiftIndex % 2 == 0) secondEvenShift++;
            if (first % 2 == 0 && secondNoShift % 2 == 0) bothEvenNoShift++;
            if (first % 2 == 0 && secondShiftIndex % 2 == 0) bothEvenShift++;
        }
        
        var pFirstEmp = (double)firstEven / C;
        var pSecondNoShiftEmp = (double)secondEvenNoShift / C;
        var pSecondShiftEmp = (double)secondEvenShift / C;
        var pBothNoShiftEmp = (double)bothEvenNoShift / C;
        var pBothShiftEmp = (double)bothEvenShift / C;

        var pFirstTheor = (double)evenCount / N;
        var pBothTheor = (double)evenCount / N * ((double)(evenCount - 1) / (N - 1));

        Console.WriteLine("Эмпирические вероятности:");
        Console.WriteLine($"Первый символ чётный: {pFirstEmp}");
        Console.WriteLine($"Второй символ без изменения индексов: {pSecondNoShiftEmp}");
        Console.WriteLine($"Второй символ с изменением индексов: {pSecondShiftEmp}");
        Console.WriteLine($"Оба символа без изменения индексов: {pBothNoShiftEmp},");
        Console.WriteLine($"Оба символа с изменением индексов: {pBothShiftEmp}");
        Console.WriteLine();
        Console.WriteLine("Теоретические вероятности:");
        Console.WriteLine($"Первый символ чётный: {pFirstTheor}");
        Console.WriteLine($"Оба символа чётные: {pBothTheor}");
    }
}