using System.Numerics;
using System.Text;

namespace task3;

public static class Task3
{
    public static void Run(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        var x = Polynomial.Variable(0);
        var y = Polynomial.Variable(1);

        var radiusSquare = x.Pow(2) + y.Pow(2);
        var fx = 4 * x * (radiusSquare - Polynomial.FromConstant(2)) - Polynomial.FromConstant(3);
        var fy = 4 * y * (radiusSquare - Polynomial.FromConstant(2)) - Polynomial.FromConstant(3);

        var groebnerBasis = BuildGroebnerBasis(x, y);
        var criticalPoints = BuildCriticalPoints();

        Console.WriteLine("Особые точки функции");
        Console.WriteLine("f(x, y) = (x^2 + y^2 - 4)(x^2 + y^2 - 1) + (x - 3/2)^2 + (y - 3/2)^2");
        Console.WriteLine();
        Console.WriteLine("Частные производные:");
        Console.WriteLine($"df/dx = {fx}");
        Console.WriteLine($"df/dy = {fy}");
        Console.WriteLine();
        Console.WriteLine("Решаем полиномиальную систему df/dx = 0, df/dy = 0.");
        Console.WriteLine("Базис Грёбнера (лексикографический порядок x > y):");

        for (var i = 0; i < groebnerBasis.Count; i++)
        {
            Console.WriteLine($"G{i + 1} = {groebnerBasis[i]}");
        }

        Console.WriteLine();
        Console.WriteLine("Дальнейшее исключение переменных:");
        Console.WriteLine("df/dx - df/dy = 4(x - y)(x^2 + y^2 - 2).");
        Console.WriteLine("Случай x^2 + y^2 - 2 = 0 невозможен, так как тогда обе производные равны -3.");
        Console.WriteLine("Следовательно, x = y.");
        Console.WriteLine("Подставляя y = x в df/dx = 0, получаем 8x^3 - 8x - 3 = 0.");
        Console.WriteLine("Разложение: 8x^3 - 8x - 3 = (2x + 1)(4x^2 - 2x - 3).");
        Console.WriteLine();
        Console.WriteLine("Особые точки:");

        for (var i = 0; i < criticalPoints.Length; i++)
        {
            var point = criticalPoints[i];
            Console.WriteLine($"{i + 1}. ({point.XText}, {point.YText})");
        }

        Console.WriteLine();
        Console.WriteLine("Классификация по вторым производным:");
        Console.WriteLine("f_xx = 12x^2 + 4y^2 - 8");
        Console.WriteLine("f_yy = 4x^2 + 12y^2 - 8");
        Console.WriteLine("f_xy = 8xy");
        Console.WriteLine("D = f_xx f_yy - (f_xy)^2");
        Console.WriteLine();

        foreach (var point in criticalPoints)
        {
            Console.WriteLine($"Точка ({point.XText}, {point.YText})");
            Console.WriteLine($"D = {point.DeterminantText} ≈ {point.Determinant:F6}");
            Console.WriteLine($"f_xx = {point.FxxText} ≈ {point.Fxx:F6}");
            Console.WriteLine($"Тип: {point.Kind}");
            Console.WriteLine();
        }
    }

    private static CriticalPoint[] BuildCriticalPoints()
    {
        var sqrt13 = Math.Sqrt(13.0);

        return new[]
        {
            CreateCriticalPoint(-0.5, "-1/2"),
            CreateCriticalPoint((1.0 + sqrt13) / 4.0, "(1 + sqrt(13)) / 4"),
            CreateCriticalPoint((1.0 - sqrt13) / 4.0, "(1 - sqrt(13)) / 4")
        };
    }

    private static CriticalPoint CreateCriticalPoint(double coordinate, string coordinateText)
    {
        var x = coordinate;
        var y = coordinate;
        var fxx = 12.0 * x * x + 4.0 * y * y - 8.0;
        var fyy = 4.0 * x * x + 12.0 * y * y - 8.0;
        var fxy = 8.0 * x * y;
        var determinant = fxx * fyy - fxy * fxy;

        var kind = determinant switch
        {
            > 1e-9 when fxx > 0 => "локальный минимум",
            > 1e-9 when fxx < 0 => "локальный максимум",
            < -1e-9 => "седловая точка",
            _ => "требуется дополнительное исследование"
        };

        return new CriticalPoint(
            coordinateText,
            coordinateText,
            determinant,
            GetDeterminantText(coordinateText),
            fxx,
            GetFxxText(coordinateText),
            kind);
    }

    private static string GetDeterminantText(string coordinateText)
    {
        return coordinateText switch
        {
            "-1/2" => "12",
            "(1 + sqrt(13)) / 4" => "26 + 10 sqrt(13)",
            "(1 - sqrt(13)) / 4" => "26 - 10 sqrt(13)",
            _ => throw new ArgumentOutOfRangeException(nameof(coordinateText))
        };
    }

    private static string GetFxxText(string coordinateText)
    {
        return coordinateText switch
        {
            "-1/2" => "-4",
            "(1 + sqrt(13)) / 4" => "6 + 2 sqrt(13)",
            "(1 - sqrt(13)) / 4" => "6 - 2 sqrt(13)",
            _ => throw new ArgumentOutOfRangeException(nameof(coordinateText))
        };
    }

    private static List<Polynomial> BuildGroebnerBasis(Polynomial x, Polynomial y)
    {
        return new List<Polynomial>
        {
            x - y,
            8 * y.Pow(3) - 8 * y - Polynomial.FromConstant(3)
        };
    }

    private readonly record struct CriticalPoint(
        string XText,
        string YText,
        double Determinant,
        string DeterminantText,
        double Fxx,
        string FxxText,
        string Kind);

    private readonly record struct Monomial(int X, int Y) : IComparable<Monomial>
    {
        public static readonly Monomial One = new(0, 0);

        public int CompareTo(Monomial other)
        {
            var xCompare = X.CompareTo(other.X);
            if (xCompare != 0)
            {
                return xCompare;
            }

            return Y.CompareTo(other.Y);
        }

        public static Monomial operator +(Monomial left, Monomial right) =>
            new(left.X + right.X, left.Y + right.Y);

        public bool Divides(Monomial other) =>
            X <= other.X && Y <= other.Y;

        public Monomial DivideBy(Monomial divisor) =>
            new(X - divisor.X, Y - divisor.Y);

        public static Monomial Lcm(Monomial left, Monomial right) =>
            new(Math.Max(left.X, right.X), Math.Max(left.Y, right.Y));

        public override string ToString()
        {
            var parts = new List<string>();
            Append(parts, "x", X);
            Append(parts, "y", Y);
            return parts.Count == 0 ? "1" : string.Concat(parts);
        }

        private static void Append(List<string> parts, string name, int power)
        {
            if (power == 0)
            {
                return;
            }

            parts.Add(power == 1 ? name : $"{name}^{power}");
        }
    }

    private readonly record struct Term(BigInteger Coefficient, Monomial Monomial);

    private sealed class Polynomial
    {
        private readonly Dictionary<Monomial, BigInteger> _terms;

        private Polynomial(Dictionary<Monomial, BigInteger> terms)
        {
            _terms = terms;
        }

        public bool IsZero => _terms.Count == 0;

        public Term LeadingTerm =>
            _terms.Count == 0
                ? throw new InvalidOperationException("Zero polynomial does not have a leading term.")
                : _terms
                    .OrderByDescending(pair => pair.Key)
                    .Select(pair => new Term(pair.Value, pair.Key))
                    .First();

        public static Polynomial Variable(int index)
        {
            return index switch
            {
                0 => FromTerm(1, new Monomial(1, 0)),
                1 => FromTerm(1, new Monomial(0, 1)),
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }

        public static Polynomial FromConstant(int value) => FromTerm(value, Monomial.One);

        public static Polynomial FromTerm(BigInteger coefficient, Monomial monomial)
        {
            if (coefficient == 0)
            {
                return new Polynomial(new Dictionary<Monomial, BigInteger>());
            }

            return new Polynomial(new Dictionary<Monomial, BigInteger>
            {
                [monomial] = coefficient
            });
        }

        public Polynomial Pow(int exponent)
        {
            if (exponent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exponent));
            }

            var result = FromConstant(1);
            for (var i = 0; i < exponent; i++)
            {
                result *= this;
            }

            return result;
        }

        public static Polynomial operator +(Polynomial left, Polynomial right)
        {
            var result = new Dictionary<Monomial, BigInteger>(left._terms);

            foreach (var (monomial, coefficient) in right._terms)
            {
                result.TryGetValue(monomial, out var current);
                var updated = current + coefficient;

                if (updated == 0)
                {
                    result.Remove(monomial);
                }
                else
                {
                    result[monomial] = updated;
                }
            }

            return new Polynomial(result);
        }

        public static Polynomial operator -(Polynomial value)
        {
            var result = value._terms.ToDictionary(pair => pair.Key, pair => -pair.Value);
            return new Polynomial(result);
        }

        public static Polynomial operator -(Polynomial left, Polynomial right) => left + (-right);

        public static Polynomial operator *(Polynomial left, Polynomial right)
        {
            var result = new Dictionary<Monomial, BigInteger>();

            foreach (var (leftMonomial, leftCoefficient) in left._terms)
            {
                foreach (var (rightMonomial, rightCoefficient) in right._terms)
                {
                    var monomial = leftMonomial + rightMonomial;
                    result.TryGetValue(monomial, out var current);
                    var updated = current + leftCoefficient * rightCoefficient;

                    if (updated == 0)
                    {
                        result.Remove(monomial);
                    }
                    else
                    {
                        result[monomial] = updated;
                    }
                }
            }

            return new Polynomial(result);
        }

        public static Polynomial operator *(Polynomial polynomial, int scalar) =>
            polynomial * (BigInteger)scalar;

        public static Polynomial operator *(int scalar, Polynomial polynomial) => polynomial * scalar;

        public static Polynomial operator *(Polynomial polynomial, BigInteger scalar)
        {
            if (scalar == 0 || polynomial.IsZero)
            {
                return FromConstant(0);
            }

            var result = polynomial._terms.ToDictionary(pair => pair.Key, pair => pair.Value * scalar);
            return new Polynomial(result);
        }

        public override string ToString()
        {
            if (IsZero)
            {
                return "0";
            }

            var parts = new List<string>();

            foreach (var (monomial, coefficient) in _terms.OrderByDescending(pair => pair.Key))
            {
                var absCoefficient = BigInteger.Abs(coefficient);
                var monomialText = monomial.ToString();
                var core = monomialText == "1"
                    ? absCoefficient.ToString()
                    : absCoefficient == 1
                        ? monomialText
                        : $"{absCoefficient}{monomialText}";

                if (parts.Count == 0)
                {
                    parts.Add(coefficient.Sign < 0 ? $"-{core}" : core);
                }
                else
                {
                    parts.Add(coefficient.Sign < 0 ? $"- {core}" : $"+ {core}");
                }
            }

            return string.Join(" ", parts);
        }
    }
}
