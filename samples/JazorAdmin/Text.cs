namespace JazorAdmin;

[ECMAScriptModule("components/text.mjs")]
public static class Text
{
    public static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
