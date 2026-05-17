namespace ECMAScript.Vben;

internal static class VbenNavigationKeyHelper
{
    public static string? Normalize(string? key)
        => string.IsNullOrWhiteSpace(key)
            ? null
            : key.Trim();
}
