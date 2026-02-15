namespace Helpers;

public class OperationCounter
{
    public static long Additions { get; private set; }
    public static long Multiplications { get; private set; }

    public static void Reset()
    {
        Additions = 0;
        Multiplications = 0;
    }
    
    public static void Add() => Additions++;
    public static void Multiply() => Multiplications++;
    public static long Total() => Additions + Multiplications;
}