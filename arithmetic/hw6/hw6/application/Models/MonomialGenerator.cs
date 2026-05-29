namespace application.Models;

public sealed record MonomialGenerator(int XExponent, int YExponent)
{
    public override string ToString()
    {
        return FormatMonomial(XExponent, YExponent);
    }

    public static string FormatMonomial(int xExponent, int yExponent)
    {
        if (xExponent == 0 && yExponent == 0)
        {
            return "1";
        }

        var parts = new List<string>(2);

        if (xExponent > 0)
        {
            parts.Add(xExponent == 1 ? "x" : $"x^{xExponent}");
        }

        if (yExponent > 0)
        {
            parts.Add(yExponent == 1 ? "y" : $"y^{yExponent}");
        }

        return string.Concat(parts);
    }
}
