namespace ECMAScript.Vben;

internal static class VbenDisplayTextHelper
{
    public static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
