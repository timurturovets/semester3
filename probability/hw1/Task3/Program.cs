namespace Task3;

// ReSharper disable file InconsistentNaming
public class Task3
{
    public static void Run(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Необходимо ввести параметр <N> в командную строку при запуске");
            return;
        }

        int N;
        if (!int.TryParse(args[0], out N))
        {
            Console.WriteLine("Некорректный параметр <N>");
            return;
        }

        var i = 1;
        var j = 2;
        if (N < 2)
        {
            Console.WriteLine("Параметр <N> должен быть больше 2");
            return;
        }
        
        var perms = GetPermutations(Enumerable.Range(1, N).ToList()).ToList();

        var Ai = new List<int[]>();
        var Aj = new List<int[]>();
        var AiAj = new List<int[]>();
        var AiUnionAj = new List<int[]>();

        foreach (var p in perms)
        {
            var inAi = p[i - 1] == i;
            var inAj = p[j - 1] == j;

            if (inAi) Ai.Add(p.ToArray());
            if (inAj) Aj.Add(p.ToArray());
            if (inAi && inAj) AiAj.Add(p.ToArray());
            if (inAi || inAj) AiUnionAj.Add(p.ToArray());
        }
        
        Console.WriteLine($"Всего перестановок: {perms.Count}");
        Console.WriteLine($"Событие A{i} (элемент {i} на позиции {i}): {Ai.Count} вариантов, " 
                          + $"вероятность: {(double)Ai.Count / perms.Count}");
        Console.WriteLine($"Событие A{j} (элемент {j} на позиции {j}): {Aj.Count} вариантов, " 
                          + $"вероятность: {(double)Aj.Count / perms.Count}");
        Console.WriteLine($"Событие A{i} & A{j}: {AiAj.Count} вариантов, " 
                          + $"вероятность: {(double)AiAj.Count / perms.Count}");
        Console.WriteLine($"Событие A{i} | A{j}: {AiUnionAj.Count} вариантов, " 
                          + $"вероятность: {(double)AiUnionAj.Count / perms.Count}");
        
        Console.WriteLine($"Примеры благоприятных перестановок для A{i} | A{j}:");
        foreach (var p in AiUnionAj.Take(10))
        {
            Console.WriteLine(string.Join(" ", p));
        }
    }

    private static IEnumerable<int[]> GetPermutations(List<int> array)
    {   
        var n = array.Count;
        var c = new int[n];
        var a = array.ToArray();

        yield return (int[])a.Clone();

        var i = 0;
        while (i < n)
        {
            if (c[i] < i)
            {
                if (i % 2 == 0) (a[0], a[i]) = (a[i], a[0]);
                else (a[c[i]], a[i]) = (a[i], a[c[i]]);
                yield return (int[])a.Clone();
                c[i]++;
                i = 0;
            }
            else
            {
                c[i] = 0;
                i++;
            }
        }
    }
}