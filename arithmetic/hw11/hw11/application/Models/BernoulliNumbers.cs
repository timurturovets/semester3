using System;

namespace SummationApp.Models
{
    public static class BernoulliNumbers
    {
        private static readonly double[] BernoulliCache = new double[100];
        
        static BernoulliNumbers()
        {
            BernoulliCache[0] = 1.0;
        }
        
        public static double GetBernoulliNumber(int j)
        {
            if (j < 0)
                throw new ArgumentException("Индекс числа Бернулли не может быть отрицательным.");
            
            if (j == 0)
                return BernoulliCache[0];
            
            if (BernoulliCache[j] != 0)
                return BernoulliCache[j];
            
            double sum = 0.0;
            for (int k = 0; k < j; k++)
            {
                double binomialCoeff = BinomialCoefficient(j + 1, k);
                sum += binomialCoeff * GetBernoulliNumber(k);
            }
            
            BernoulliCache[j] = -sum / (j + 1);
            return BernoulliCache[j];
        }
        
        public static double BinomialCoefficient(int n, int k)
        {
            if (k < 0 || k > n)
                return 0;
            
            if (k == 0 || k == n)
                return 1;
            
            double result = 1.0;
            for (int i = 1; i <= k; i++)
            {
                result *= (double)(n - k + i) / i;
            }
            
            return result;
        }
    }
}