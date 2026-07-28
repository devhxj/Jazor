namespace Jazor.Admin;

[ECMAScriptModule("components/jazor-admin-display-text-helper.mjs")]
internal static class AdminDisplayTextHelper
{
    public static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
