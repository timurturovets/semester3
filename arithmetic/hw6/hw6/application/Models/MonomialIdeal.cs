namespace application.Models;

public sealed class MonomialIdeal
{
    public MonomialIdeal(IEnumerable<MonomialGenerator> generators)
    {
        Generators = generators
            .Distinct()
            .OrderBy(generator => generator.XExponent)
            .ThenBy(generator => generator.YExponent)
            .ToArray();
    }

    public IReadOnlyList<MonomialGenerator> Generators { get; }

    public bool IsInIdeal(int xExponent, int yExponent)
    {
        return Generators.Any(generator =>
            xExponent >= generator.XExponent &&
            yExponent >= generator.YExponent);
    }

    public IEnumerable<GridPoint> EnumerateIdealPoints(int maxX, int maxY)
    {
        return EnumeratePoints(maxX, maxY, expectedMembership: true);
    }

    public IEnumerable<GridPoint> EnumerateRemainderPoints(int maxX, int maxY)
    {
        return EnumeratePoints(maxX, maxY, expectedMembership: false);
    }

    public (int MaxX, int MaxY) SuggestBounds(int padding = 6, int minimumSize = 10)
    {
        if (Generators.Count == 0)
        {
            return (minimumSize, minimumSize);
        }

        var maxX = Math.Max(minimumSize, Generators.Max(generator => generator.XExponent) + padding);
        var maxY = Math.Max(minimumSize, Generators.Max(generator => generator.YExponent) + padding);

        return (maxX, maxY);
    }

    private IEnumerable<GridPoint> EnumeratePoints(int maxX, int maxY, bool expectedMembership)
    {
        for (var y = 0; y <= maxY; y++)
        {
            for (var x = 0; x <= maxX; x++)
            {
                if (IsInIdeal(x, y) == expectedMembership)
                {
                    yield return new GridPoint(x, y);
                }
            }
        }
    }
}
