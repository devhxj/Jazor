namespace Jazor.RazorVue.Lowering;

internal static class RazorVueCaptureUnmatchedAttributePolicy
{
    private static readonly HashSet<string> GlobalAttributeNames = new(StringComparer.Ordinal)
    {
        "accesskey",
        "autocapitalize",
        "autofocus",
        "class",
        "contenteditable",
        "dir",
        "draggable",
        "enterkeyhint",
        "exportparts",
        "hidden",
        "id",
        "inert",
        "inputmode",
        "is",
        "itemid",
        "itemprop",
        "itemref",
        "itemscope",
        "itemtype",
        "lang",
        "nonce",
        "part",
        "popover",
        "role",
        "slot",
        "spellcheck",
        "style",
        "tabindex",
        "title",
        "translate",
        "virtualkeyboardpolicy"
    };

    public static bool CanCaptureExplicitAttribute(string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
            return false;

        if (GlobalAttributeNames.Contains(attributeName))
            return true;

        if (attributeName.StartsWith("data-", StringComparison.Ordinal) ||
            attributeName.StartsWith("aria-", StringComparison.Ordinal))
        {
            return true;
        }

        return IsVueDirectiveLikeAttribute(attributeName) ||
               IsLowerCamelRawAttributeName(attributeName) ||
               IsKebabCaseRawAttributeName(attributeName);
    }

    private static bool IsVueDirectiveLikeAttribute(string attributeName)
        => attributeName.StartsWith("v-", StringComparison.Ordinal) ||
           attributeName.StartsWith(":", StringComparison.Ordinal) ||
           attributeName.StartsWith("@", StringComparison.Ordinal) ||
           attributeName.StartsWith("#", StringComparison.Ordinal);

    private static bool IsLowerCamelRawAttributeName(string attributeName)
    {
        if (attributeName[0] is not (>= 'a' and <= 'z'))
            return false;

        for (var index = 1; index < attributeName.Length; index++)
        {
            var character = attributeName[index];
            if (character is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
                continue;

            if (character is >= '0' and <= '9' or '_' or '$' or ':')
                continue;

            return false;
        }

        return true;
    }

    private static bool IsKebabCaseRawAttributeName(string attributeName)
    {
        if (!attributeName.Contains("-"))
            return false;

        var hasAsciiLetter = false;
        foreach (var character in attributeName)
        {
            if (character is >= 'a' and <= 'z')
            {
                hasAsciiLetter = true;
                continue;
            }

            if (character is >= '0' and <= '9' or '-' or '_' or ':' or '.')
                continue;

            return false;
        }

        return hasAsciiLetter;
    }
}
