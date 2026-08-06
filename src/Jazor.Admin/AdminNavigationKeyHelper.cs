namespace Jazor.Admin;

[ECMAScriptModule("components/admin/navigation-key.mjs")]
internal static class AdminNavigationKeyHelper
{
    public static string? Normalize(string? key)
        => string.IsNullOrWhiteSpace(key)
            ? null
            : key.Trim();
}
