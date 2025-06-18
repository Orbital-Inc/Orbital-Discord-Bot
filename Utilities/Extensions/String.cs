using System.Text.RegularExpressions;

namespace MainBot.Utilities.Extensions;
internal static partial class String
{
    public static string RemoveSpecialCharacters(this string str) => RemoveSpecialCharacters().Replace(str, "");
    public static bool ContainsSpecialCharacters(this string str) => ContainsSpecialCharacters().IsMatch(str) is false;
    public static string FormatEnum(this Enum enumValue) => FormatEnumWithSpacing().Replace(enumValue.ToString(), "$1 $2");

    [GeneratedRegex("^[a-zA-Z0-9_ ]*$", RegexOptions.Compiled)]
    private static partial Regex ContainsSpecialCharacters();

    [GeneratedRegex("[^a-zA-Z0-9_ ]+", RegexOptions.Compiled)]
    private static partial Regex RemoveSpecialCharacters();

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex FormatEnumWithSpacing();
}