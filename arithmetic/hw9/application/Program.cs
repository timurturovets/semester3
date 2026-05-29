using System.Text;
using task1;
using task2;
using task3;

namespace Application;

internal static class Program
{
    private delegate void Runner(string[] args);

    internal static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        var runners = new List<Runner>
        {
            Task1.Run,
            Task2.Run,
            Task3.Run,
        };

        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Определение принадлежности полинома идеалу 1");
            Console.WriteLine("2. Определение принадлежности полинома идеалу 2");
            Console.WriteLine("3. Особые точки функции двух переменных");
            Console.WriteLine("0. Выход");
            Console.WriteLine();

            int choice;
            do Console.Write("Выберите задачу: ");
            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > runners.Count);

            if (choice == 0)
            {
                return;
            }

            Console.WriteLine();
            runners[choice - 1](args);
            Console.WriteLine();
            Console.Write("Нажмите любую кнопку, чтобы продолжить...");
            Console.ReadKey();
        }
    }
}
