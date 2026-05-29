using System.Text.RegularExpressions;
using application.Models;

namespace application.Services;

public static class MonomialParser
{
    private static readonly Regex TokenRegex = new(
        @"([xy])(?:\^(\d+))?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<MonomialGenerator> ParseMany(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new FormatException("Введите хотя бы одну мономиальную образующую.");
        }

        var separators = new[] { ',', ';', '\n', '\r' };
        var tokens = input
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();

        if (tokens.Length == 0)
        {
            throw new FormatException("Введите хотя бы одну мономиальную образующую.");
        }

        return tokens.Select(ParseSingle).ToArray();
    }

    public static MonomialGenerator ParseSingle(string monomialText)
    {
        var normalized = monomialText.Replace(" ", string.Empty);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new FormatException("Моном не может быть пустым.");
        }

        if (normalized == "1")
        {
            return new MonomialGenerator(0, 0);
        }

        var matches = TokenRegex.Matches(normalized);
        if (matches.Count == 0)
        {
            throw new FormatException($"Некорректный моном: '{monomialText}'.");
        }

        var consumed = string.Concat(matches.Select(match => match.Value));
        if (!string.Equals(consumed, normalized, StringComparison.Ordinal))
        {
            throw new FormatException($"Некорректный моном: '{monomialText}'.");
        }

        var xExponent = 0;
        var yExponent = 0;

        foreach (Match match in matches)
        {
            var variable = match.Groups[1].Value;
            var exponent = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;

            if (variable == "x")
            {
                xExponent += exponent;
            }
            else
            {
                yExponent += exponent;
            }
        }

        return new MonomialGenerator(xExponent, yExponent);
    }

    public static string ToDisplayText(IEnumerable<MonomialGenerator> generators)
    {
        return string.Join(", ", generators.Select(generator => generator.ToString()));
    }
}
