namespace Helpers;

public readonly struct DivisionResult(int[] quotient, int remainder)
{
    public int[] Quotient { get; } = quotient ?? throw new ArgumentNullException(nameof(quotient));
    public int Remainder { get; } = remainder;

    public string ToNumberString()
    {
        var firstNonZero = Array.FindIndex(Quotient, d => d != 0);
        return firstNonZero == -1 ? "0" : string.Concat(Quotient.Skip(firstNonZero));
    }
    
    public override string ToString()
    {
        return $"частное: {ToNumberString()}, остаток: {Remainder}";
    }
}

public static class Division
{
    public static DivisionResult Divide(int[] u, int v, int b)
    {
        if (v <= 0 || v >= b) throw new ArgumentException("Должно выполняться условие 0 < v < b");

        if (u == null || u.Length == 0) throw new ArgumentException("Массив цифр пуст");

        var n = u.Length;
        var w = new int[n];
        var r = 0;

        for (var i = 0; i < n; i++)
        {
            if (u[i] < 0 || u[i] >= b) throw new ArgumentException("Цифры должны быть в диапазоне [0, b)");

            var t = r * b + u[i];
            w[i] = t / v;
            r = t % v;
        }

        return new DivisionResult(w, r);
    }
}