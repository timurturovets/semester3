using task1;
using task2;
using task3;
using task4;
using task5;
using task6;
using task7;
using task8;

namespace Application;

public class Program
{
    private delegate void Runner(string[] args);
    internal static void Main(string[] args)
    {
        var runners = new List<Runner>()
        {
            Task1.Run,
            Task2.Run,
            Task3.Run,
            Task4.Run,
            Task5.Run,
            Task6.Run,
            Task7.Run,
            Task8.Run,
        };
        
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Эта программа решает задачи с применением двоичных многочленов.");
            Console.WriteLine("1. Представление элемента из GF(2^n) в полиномиальной форме и наоборот");
            Console.WriteLine("2. Умножение двух произвольных двоичных многочленов степени не выше 32");
            Console.WriteLine("3. Умножение двух элементов GF(2^n) для разных неприводимых многочленов");
            Console.WriteLine("4. Расширенный алгоритм Евклида для GF(2^n)");
            Console.WriteLine("5. Поиск мультипликативно обратного элемента в GF(2^n)");
            Console.WriteLine("6. (фон цур Гатен) Вычисление z1 / z2 за не более чем 7 умножений/делений");
            Console.WriteLine("7. (Кормен) Вычисление декартовой суммы множеств A и B");
            Console.WriteLine("8. Многомерное Быстрое преобразование Фурье");
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
