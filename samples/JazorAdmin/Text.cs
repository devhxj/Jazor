namespace JazorAdmin;

[ECMAScriptModule("components/text.js")]
public static class Text
{
    public static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
