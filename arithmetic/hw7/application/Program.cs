namespace Application;

public class Program
{
    private delegate void Runner(string[] args);
    internal static void Main(string[] args)
    {
        var runners = new List<Runner>()
        {
            
        };
        
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Эта программа решает задачи с применением двоичных многочленов.");

            Console.WriteLine("0. Выход");
            Console.WriteLine();
            
            int choice;
            do Console.Write("Выберите задачу: ");
            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > runners.Count);

            if (choice == 0) return;

            Console.WriteLine();
            
            runners[choice - 1](args);
            
            Console.WriteLine();
            Console.Write("Нажмите любую кнопку, чтобы продолжить...");
            Console.ReadKey();
        }
    }
}