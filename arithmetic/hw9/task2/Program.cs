using System.Numerics;
using System.Text;

namespace task2;

public static class Task2
{
    public static void Run(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        var x = Polynomial.Variable(0);
        var y = Polynomial.Variable(1);
        var z = Polynomial.Variable(2);

        var f = x.Pow(3) * z - 2 * y.Pow(2);
        var g1 = x * z - y;
        var g2 = x * y + 2 * z.Pow(2);
        var g3 = y - z;

        var generators = new List<Polynomial> { g1, g2, g3 };
        var groebnerBasis = BuildGroebnerBasis(generators);
        var remainder = f.ReduceBy(groebnerBasis);

        Console.WriteLine("Проверка принадлежности полинома f идеалу I = < xz - y, xy + 2z^2, y - z >");
        Console.WriteLine($"f = {f}");
        Console.WriteLine();
        Console.WriteLine("Порождающие идеала:");
        Console.WriteLine($"g1 = {g1}");
        Console.WriteLine($"g2 = {g2}");
        Console.WriteLine($"g3 = {g3}");
        Console.WriteLine();
        Console.WriteLine("Базис Грёбнера (лексикографический порядок x > y > z):");

        for (var i = 0; i < groebnerBasis.Count; i++)
        {
            Console.WriteLine($"G{i + 1} = {groebnerBasis[i]}");
        }

        Console.WriteLine();
        Console.WriteLine($"Остаток от деления f на G: {remainder}");
        Console.WriteLine(remainder.IsZero
            ? "Вывод: f принадлежит идеалу I."
            : "Вывод: f не принадлежит идеалу I.");
    }

    private static List<Polynomial> BuildGroebnerBasis(IEnumerable<Polynomial> generators)
    {
        var basis = generators
            .Where(polynomial => !polynomial.IsZero)
            .Select(polynomial => polynomial.NormalizeLeadingCoefficient())
            .ToList();

        var pairs = new Queue<(int Left, int Right)>();
        for (var i = 0; i < basis.Count; i++)
        {
            for (var j = i + 1; j < basis.Count; j++)
            {
                pairs.Enqueue((i, j));
            }
        }

        while (pairs.Count > 0)
        {
            var (left, right) = pairs.Dequeue();
            var reduced = SPolynomial(basis[left], basis[right]).ReduceBy(basis);

            if (reduced.IsZero)
            {
                continue;
            }

            reduced = reduced.NormalizeLeadingCoefficient();
            var newIndex = basis.Count;
            basis.Add(reduced);

            for (var i = 0; i < newIndex; i++)
            {
                pairs.Enqueue((i, newIndex));
            }
        }

        return basis;
    }

    private static Polynomial SPolynomial(Polynomial left, Polynomial right)
    {
        var leftLeading = left.LeadingTerm;
        var rightLeading = right.LeadingTerm;
        var lcm = Monomial.Lcm(leftLeading.Monomial, rightLeading.Monomial);

        var leftMultiplier = lcm.DivideBy(leftLeading.Monomial);
        var rightMultiplier = lcm.DivideBy(rightLeading.Monomial);

        return left.MultiplyByMonomial(leftMultiplier) - right.MultiplyByMonomial(rightMultiplier);
    }

    private readonly record struct Monomial(int X, int Y, int Z) : IComparable<Monomial>
    {
        public static readonly Monomial One = new(0, 0, 0);

        public int CompareTo(Monomial other)
        {
            var xCompare = X.CompareTo(other.X);
            if (xCompare != 0)
            {
                return xCompare;
            }

            var yCompare = Y.CompareTo(other.Y);
            if (yCompare != 0)
            {
                return yCompare;
            }

            return Z.CompareTo(other.Z);
        }

        public static Monomial operator +(Monomial left, Monomial right) =>
            new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        public bool Divides(Monomial other) =>
            X <= other.X && Y <= other.Y && Z <= other.Z;

        public Monomial DivideBy(Monomial divisor) =>
            new(X - divisor.X, Y - divisor.Y, Z - divisor.Z);

        public static Monomial Lcm(Monomial left, Monomial right) =>
            new(Math.Max(left.X, right.X), Math.Max(left.Y, right.Y), Math.Max(left.Z, right.Z));

        public override string ToString()
        {
            var parts = new List<string>();
            Append(parts, "x", X);
            Append(parts, "y", Y);
            Append(parts, "z", Z);
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
                0 => FromTerm(1, new Monomial(1, 0, 0)),
                1 => FromTerm(1, new Monomial(0, 1, 0)),
                2 => FromTerm(1, new Monomial(0, 0, 1)),
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

        public Polynomial NormalizeLeadingCoefficient()
        {
            if (IsZero)
            {
                return this;
            }

            return LeadingTerm.Coefficient.Sign < 0 ? -this : this;
        }

        public Polynomial MultiplyByMonomial(Monomial monomial)
        {
            var result = new Dictionary<Monomial, BigInteger>();
            foreach (var (currentMonomial, coefficient) in _terms)
            {
                result[currentMonomial + monomial] = coefficient;
            }

            return new Polynomial(result);
        }

        public Polynomial ReduceBy(IReadOnlyList<Polynomial> basis)
        {
            var current = this;
            var remainder = FromConstant(0);

            while (!current.IsZero)
            {
                var leading = current.LeadingTerm;
                var wasReduced = false;

                foreach (var divisor in basis)
                {
                    if (divisor.IsZero)
                    {
                        continue;
                    }

                    var divisorLeading = divisor.LeadingTerm;
                    if (!divisorLeading.Monomial.Divides(leading.Monomial))
                    {
                        continue;
                    }

                    var quotientMonomial = leading.Monomial.DivideBy(divisorLeading.Monomial);
                    var quotientCoefficient = leading.Coefficient / divisorLeading.Coefficient;
                    current -= divisor.MultiplyByMonomial(quotientMonomial) * quotientCoefficient;
                    wasReduced = true;
                    break;
                }

                if (!wasReduced)
                {
                    var leadingPolynomial = FromTerm(leading.Coefficient, leading.Monomial);
                    remainder += leadingPolynomial;
                    current -= leadingPolynomial;
                }
            }

            return remainder;
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
