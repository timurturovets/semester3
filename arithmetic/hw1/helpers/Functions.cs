namespace helpers;

public static class Functions
{
    public static uint Factorial(uint n)
    {
        if (n == 0) return 1;
        return n * Factorial(n - 1);
    }
}