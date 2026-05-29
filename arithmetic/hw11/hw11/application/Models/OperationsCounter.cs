namespace SummationApp.Models
{
    public static class OperationsCounter
    {
        public static long CountDirectOperations(int n, int p)
        {
            return 2 * n;
        }
        
        public static long CountAsymptoticOperations(int n, int p)
        {
            return 4;
        }
        
        public static long CountExactOperations(int n, int p)
        {
            return 5 * (p + 1) + 1;
        }
    }
}