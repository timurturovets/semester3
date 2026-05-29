using System;

namespace SummationApp.Models
{
    public static class SummationMethods
    {
        public static double DirectSummation(int n, int p, out long operations)
        {
            operations = 0;
            double sum = 0.0;
            
            for (int k = 1; k <= n; k++)
            {
                sum += Math.Pow(k, p);
                operations += 2;
            }
            
            return sum;
        }
        
        public static double AsymptoticSummation(int n, int p, out long operations)
        {
            operations = 0;
            double term1 = Math.Pow(n, p + 1) / (p + 1);
            double term2 = Math.Pow(n, p) / 2;
            operations += 4;
            
            return term1 + term2;
        }
        
        public static double ExactSummation(int n, int p, out long operations)
        {
            operations = 0;
            double sum = 0.0;
            
            for (int j = 0; j <= p; j++)
            {
                double binomialCoeff = BernoulliNumbers.BinomialCoefficient(p + 1, j);
                double bernoulliNumber = BernoulliNumbers.GetBernoulliNumber(j);
                double term = binomialCoeff * bernoulliNumber * Math.Pow(n, p + 1 - j);
                sum += term;
                operations += 5;
            }
            
            sum /= p + 1;
            operations += 1;
            
            return sum;
        }
    }
}