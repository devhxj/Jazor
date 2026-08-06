namespace Jazor.Admin;

[ECMAScriptModule("components/admin/display-text.mjs")]
internal static class AdminDisplayTextHelper
{
    public static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
