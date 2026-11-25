


using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;
namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeKnowledgeContentSanitizer
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "a",
        "b",
        "blockquote",
        "br",
        "code",
        "div",
        "em",
        "h1",
        "h2",
        "h3",
        "h4",
        "hr",
        "i",
        "img",
        "li",
        "ol",
        "p",
        "pre",
        "s",
        "span",
        "strong",
        "sub",
        "sup",
        "u",
        "ul",
        "video"
    };

    private static readonly HashSet<string> DangerousTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "base",
        "embed",
        "form",
        "iframe",
        "input",
        "link",
        "meta",
        "object",
        "script",
        "style",
        "svg",
        "textarea"
    };

    private static readonly HashSet<string> GlobalAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "align",
        "class",
        "style"
    };

    private static readonly HashSet<string> AnchorAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "href",
        "target",
        "title"
    };

    private static readonly HashSet<string> ImageAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "alt",
        "height",
        "src",
        "title",
        "width"
    };

    private static readonly HashSet<string> VideoAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "controls",
        "height",
        "loop",
        "muted",
        "poster",
        "preload",
        "src",
        "width"
    };

    private static readonly HashSet<string> TextBlockTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "blockquote",
        "br",
        "div",
        "h1",
        "h2",
        "h3",
        "h4",
        "hr",
        "li",
        "ol",
        "p",
        "pre",
        "ul"
    };

    private static readonly HashSet<string> AllowedStyleProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "height",
        "max-height",
        "max-width",
        "text-align",
        "width"
    };

    public static string? Normalize(string? content, string? mediaOrigin = null)
    {
        var normalized = content?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        var parser = new HtmlParser();
        var document = parser.ParseDocument(normalized);
        SanitizeChildren(document.Body);
        if (!string.IsNullOrWhiteSpace(mediaOrigin) && document.Body != null)
            NormalizeMediaSources(document.Body, mediaOrigin);

        var html = document.Body?.InnerHtml.Trim();
        return string.IsNullOrWhiteSpace(html) ? null : html;
    }

    private static void NormalizeMediaSources(IElement root, string mediaOrigin)
    {
        foreach (var element in root.QuerySelectorAll("img, video"))
        {
            foreach (var attributeName in element.TagName.Equals("video", StringComparison.OrdinalIgnoreCase)
                         ? new[] { "src", "poster" }
                         : new[] { "src" })
            {
                var value = element.GetAttribute(attributeName);
                var normalized = NormalizeMediaSourceUrl(value);
                if (!string.IsNullOrWhiteSpace(normalized))
                    element.SetAttribute(attributeName, ToAbsoluteMediaUrl(normalized, mediaOrigin));
            }
        }
    }

    private static string ToAbsoluteMediaUrl(string value, string mediaOrigin)
    {
        if (value.StartsWith("//", StringComparison.Ordinal))
            return $"https:{value}";

        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri) &&
            absoluteUri.Scheme is "http" or "https")
        {
            return value;
        }

        return $"{mediaOrigin.TrimEnd('/')}/{value.TrimStart('/')}";
    }

    public static string NormalizeMediaSourceUrl(string? value, string? mediaOrigin = null)
    {
        var normalized = NormalizeMediaSourcePath(value);
        if (string.IsNullOrWhiteSpace(normalized) || string.IsNullOrWhiteSpace(mediaOrigin))
            return normalized;

        return ToAbsoluteMediaUrl(normalized, mediaOrigin);
    }

    private static string NormalizeMediaSourcePath(string? value)
    {
        var url = value?.Trim();
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        var normalized = Regex.Replace(url.Replace('\\', '/'), @"[\u0000-\u001F\u007F\s]+", string.Empty);
        if (string.IsNullOrWhiteSpace(normalized))
            return string.Empty;

        if (normalized.StartsWith("//", StringComparison.Ordinal))
            return IsSafeProtocolRelativeUrl(normalized) ? normalized : string.Empty;

        var schemeSeparatorIndex = normalized.IndexOf(':', StringComparison.Ordinal);
        var pathBoundaryIndex = normalized.IndexOfAny(['/', '?', '#']);
        if (schemeSeparatorIndex >= 0 && (pathBoundaryIndex < 0 || schemeSeparatorIndex < pathBoundaryIndex))
        {
            if (!Uri.TryCreate(normalized, UriKind.Absolute, out var compactUri))
                return string.Empty;

            return compactUri.Scheme is "http" or "https" ? normalized : string.Empty;
        }

        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        if (normalized.StartsWith("~/", StringComparison.Ordinal))
            normalized = normalized[2..];

        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            return uri.Scheme is "http" or "https" ? normalized : string.Empty;

        if (normalized.StartsWith("/", StringComparison.Ordinal))
            return normalized;

        if (normalized.StartsWith("../", StringComparison.Ordinal) ||
            normalized.Equals("..", StringComparison.Ordinal) ||
            normalized.StartsWith("#", StringComparison.Ordinal) ||
            normalized.StartsWith("?", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return $"/{normalized}";
    }

    public static string? ExtractPlainText(string? content)
    {
        var html = Normalize(content);
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var parser = new HtmlParser();
        var document = parser.ParseDocument(html);
        var builder = new StringBuilder();
        AppendPlainText(document.Body, builder);

        var text = NormalizePlainText(builder.ToString());
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static void SanitizeChildren(INode? parent)
    {
        if (parent == null)
            return;

        foreach (var child in parent.ChildNodes.ToList())
        {
            switch (child)
            {
                case IElement element:
                    SanitizeElement(element);
                    break;
                case IText:
                    break;
                default:
                    parent.RemoveChild(child);
                    break;
            }
        }
    }

    private static void SanitizeElement(IElement element)
    {
        if (!AllowedTags.Contains(element.TagName))
        {
            if (DangerousTags.Contains(element.TagName))
            {
                element.Parent?.RemoveChild(element);
                return;
            }

            Unwrap(element);
            return;
        }

        SanitizeAttributes(element);
        SanitizeChildren(element);
    }

    private static void SanitizeAttributes(IElement element)
    {
        foreach (var attribute in element.Attributes.ToList())
        {
            var name = attribute.Name;
            var value = attribute.Value?.Trim() ?? string.Empty;
            if (!IsAllowedAttribute(element, name, value))
            {
                element.RemoveAttribute(name);
                continue;
            }

            if (name.Equals("style", StringComparison.OrdinalIgnoreCase))
            {
                var style = SanitizeStyle(value);
                if (string.IsNullOrWhiteSpace(style))
                    element.RemoveAttribute(name);
                else
                    element.SetAttribute(name, style);
            }
            else if (IsMediaSourceAttribute(element, name))
            {
                var normalizedUrl = NormalizeMediaSourceUrl(value);
                if (string.IsNullOrWhiteSpace(normalizedUrl))
                    element.RemoveAttribute(name);
                else
                    element.SetAttribute(name, normalizedUrl);
            }
        }

        if (element.TagName.Equals("a", StringComparison.OrdinalIgnoreCase) &&
            element.GetAttribute("target")?.Equals("_blank", StringComparison.OrdinalIgnoreCase) == true)
        {
            element.SetAttribute("rel", "noopener noreferrer");
        }
    }

    private static bool IsAllowedAttribute(IElement element, string name, string value)
    {
        if (name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            return false;

        if (name.Equals("style", StringComparison.OrdinalIgnoreCase))
            return GlobalAttributes.Contains(name);

        if (GlobalAttributes.Contains(name))
            return IsSafePlainAttribute(value);

        if (element.TagName.Equals("a", StringComparison.OrdinalIgnoreCase) && AnchorAttributes.Contains(name))
            return name.Equals("href", StringComparison.OrdinalIgnoreCase)
                ? IsSafeUrl(value, allowRelative: true)
                : IsSafePlainAttribute(value);

        if (element.TagName.Equals("img", StringComparison.OrdinalIgnoreCase) && ImageAttributes.Contains(name))
            return name.Equals("src", StringComparison.OrdinalIgnoreCase)
                ? !string.IsNullOrWhiteSpace(NormalizeMediaSourceUrl(value))
                : IsSafePlainAttribute(value);

        if (element.TagName.Equals("video", StringComparison.OrdinalIgnoreCase) && VideoAttributes.Contains(name))
            return name.Equals("src", StringComparison.OrdinalIgnoreCase) || name.Equals("poster", StringComparison.OrdinalIgnoreCase)
                ? !string.IsNullOrWhiteSpace(NormalizeMediaSourceUrl(value))
                : IsSafePlainAttribute(value);

        return false;
    }

    private static bool IsMediaSourceAttribute(IElement element, string name)
    {
        if (element.TagName.Equals("img", StringComparison.OrdinalIgnoreCase))
            return name.Equals("src", StringComparison.OrdinalIgnoreCase);

        if (element.TagName.Equals("video", StringComparison.OrdinalIgnoreCase))
        {
            return name.Equals("src", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("poster", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsSafeProtocolRelativeUrl(string value)
    {
        return Uri.TryCreate($"https:{value}", UriKind.Absolute, out var uri) &&
            uri.Scheme is "http" or "https";
    }

    private static bool IsSafeUrl(string value, bool allowRelative)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var compact = Regex.Replace(value.Trim(), @"[\u0000-\u001F\u007F\s]+", string.Empty);
        if (compact.StartsWith("//", StringComparison.Ordinal))
            return true;

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return uri.Scheme is "http" or "https";

        var schemeSeparatorIndex = compact.IndexOf(':', StringComparison.Ordinal);
        var pathBoundaryIndex = compact.IndexOfAny(['/', '?', '#']);
        if (schemeSeparatorIndex >= 0 && (pathBoundaryIndex < 0 || schemeSeparatorIndex < pathBoundaryIndex))
            return false;

        return allowRelative &&
            !compact.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) &&
            !compact.StartsWith("data:", StringComparison.OrdinalIgnoreCase) &&
            !compact.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSafePlainAttribute(string value)
    {
        return !value.Contains('<') && !value.Contains('>');
    }

    private static string? SanitizeStyle(string value)
    {
        var declarations = value
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(u => u.Trim())
            .Where(IsSafeStyleDeclaration)
            .ToList();

        return declarations.Count == 0 ? null : string.Join("; ", declarations);
    }

    private static bool IsSafeStyleDeclaration(string declaration)
    {
        var separatorIndex = declaration.IndexOf(':', StringComparison.Ordinal);
        if (separatorIndex <= 0)
            return false;

        var property = declaration[..separatorIndex].Trim();
        var value = declaration[(separatorIndex + 1)..].Trim();
        if (!AllowedStyleProperties.Contains(property) || string.IsNullOrWhiteSpace(value))
            return false;

        return !value.Contains("url(", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("expression", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("javascript:", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains('<') &&
            !value.Contains('>');
    }

    private static void Unwrap(IElement element)
    {
        var parent = element.Parent;
        if (parent == null)
            return;

        var movedChildren = element.ChildNodes.ToList();

        foreach (var child in movedChildren)
        {
            parent.InsertBefore(child, element);
        }
        parent.RemoveChild(element);

        foreach (var child in movedChildren)
        {
            switch (child)
            {
                case IElement movedElement:
                    SanitizeElement(movedElement);
                    break;
                case IText:
                    break;
                default:
                    parent.RemoveChild(child);
                    break;
            }
        }
    }

    private static void AppendPlainText(INode? node, StringBuilder builder)
    {
        if (node == null)
            return;

        foreach (var child in node.ChildNodes)
        {
            if (child is IText text)
            {
                builder.Append(text.Data);
                continue;
            }

            if (child is not IElement element)
                continue;

            if (element.TagName.Equals("img", StringComparison.OrdinalIgnoreCase) ||
                element.TagName.Equals("video", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var isBlock = TextBlockTags.Contains(element.TagName);
            if (isBlock)
                AppendLineBreak(builder);

            AppendPlainText(element, builder);

            if (isBlock)
                AppendLineBreak(builder);
        }
    }

    private static void AppendLineBreak(StringBuilder builder)
    {
        if (builder.Length == 0 || builder[^1] == '\n')
            return;

        builder.AppendLine();
    }

    private static string NormalizePlainText(string text)
    {
        var normalized = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        normalized = Regex.Replace(normalized, @"[ \t\f\v]+", " ");
        normalized = Regex.Replace(normalized, @" *\n *", "\n");
        normalized = Regex.Replace(normalized, @"\n{3,}", "\n\n");
        return normalized.Trim();
    }
}
