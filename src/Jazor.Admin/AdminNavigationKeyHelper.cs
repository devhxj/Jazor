namespace Jazor.Admin;

[ECMAScriptModule("components/jazor-admin-navigation-key-helper.mjs")]
internal static class AdminNavigationKeyHelper
{
    public static string? Normalize(string? key)
        => string.IsNullOrWhiteSpace(key)
            ? null
            : key.Trim();
}
