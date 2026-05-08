using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Jazor.AspNetCore.Dev;

internal static class JazorDevelopmentHtmlInjector
{
    private static readonly Regex ScriptNoncePattern = new(
        @"'nonce-(?<value>[^']+)'",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string InjectClientScript(
        string html,
        string clientScriptPath,
        string pathBaseAttributeName,
        string pathBase,
        string? nonce)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientScriptPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(pathBaseAttributeName);
        ArgumentNullException.ThrowIfNull(pathBase);

        if (html.Contains(clientScriptPath, StringComparison.Ordinal))
            return html;

        var htmlWithPathBase = EnsureHtmlPathBaseAttribute(html, pathBaseAttributeName, pathBase);
        var scriptTag = BuildScriptTag(BuildClientScriptPath(clientScriptPath, pathBase), nonce);
        var headIndex = htmlWithPathBase.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (headIndex >= 0)
            return htmlWithPathBase.Insert(headIndex, scriptTag);

        var bodyIndex = htmlWithPathBase.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (bodyIndex >= 0)
            return htmlWithPathBase.Insert(bodyIndex, scriptTag);

        return scriptTag + htmlWithPathBase;
    }

    public static string? TryExtractScriptNonce(StringValues policyValues)
    {
        foreach (var policy in policyValues)
        {
            if (string.IsNullOrWhiteSpace(policy))
                continue;

            var match = ScriptNoncePattern.Match(policy);
            if (match.Success)
                return match.Groups["value"].Value;
        }

        return null;
    }

    public static StringValues AugmentContentSecurityPolicy(StringValues policyValues)
    {
        if (policyValues.Count == 0)
            return policyValues;

        var transformed = new string[policyValues.Count];
        for (var index = 0; index < policyValues.Count; index++)
        {
            transformed[index] = AugmentSinglePolicy(policyValues[index] ?? string.Empty);
        }

        return new StringValues(transformed);
    }

    public static Encoding ResolveEncoding(string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType)
            && MediaTypeHeaderValue.TryParse(contentType, out var mediaType)
            && !StringSegment.IsNullOrEmpty(mediaType.Charset))
        {
            try
            {
                return Encoding.GetEncoding(mediaType.Charset.ToString());
            }
            catch (ArgumentException)
            {
            }
        }

        return Encoding.UTF8;
    }

    private static string BuildScriptTag(string clientScriptPath, string? nonce)
    {
        var builder = new StringBuilder();
        builder.Append("<script type=\"module\" src=\"")
            .Append(clientScriptPath)
            .Append('"');

        if (!string.IsNullOrWhiteSpace(nonce))
        {
            builder.Append(" nonce=\"")
                .Append(nonce)
                .Append('"');
        }

        builder.Append("></script>");
        return builder.ToString();
    }

    private static string BuildClientScriptPath(string clientScriptPath, string pathBase)
    {
        var normalizedPathBase = NormalizePathBase(pathBase);
        return normalizedPathBase.Length == 0
            ? clientScriptPath
            : normalizedPathBase + clientScriptPath;
    }

    private static string EnsureHtmlPathBaseAttribute(
        string html,
        string pathBaseAttributeName,
        string pathBase)
    {
        var normalizedPathBase = NormalizePathBase(pathBase);
        var htmlTagIndex = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
        if (htmlTagIndex < 0)
            return html;

        var htmlTagEndIndex = html.IndexOf('>', htmlTagIndex);
        if (htmlTagEndIndex < 0)
            return html;

        var htmlTag = html.Substring(htmlTagIndex, htmlTagEndIndex - htmlTagIndex + 1);
        var attributeValue = pathBaseAttributeName + "=\"" + normalizedPathBase + "\"";

        if (htmlTag.Contains(pathBaseAttributeName + "=", StringComparison.OrdinalIgnoreCase))
            return html;

        var updatedHtmlTag = htmlTag.Insert(htmlTag.Length - 1, " " + attributeValue);
        return html[..htmlTagIndex] + updatedHtmlTag + html[(htmlTagEndIndex + 1)..];
    }

    private static string NormalizePathBase(string pathBase)
    {
        if (string.IsNullOrWhiteSpace(pathBase) || pathBase == "/")
            return string.Empty;

        return pathBase.EndsWith("/", StringComparison.Ordinal)
            ? pathBase[..^1]
            : pathBase;
    }

    private static string AugmentSinglePolicy(string policy)
    {
        if (string.IsNullOrWhiteSpace(policy))
            return policy;

        var directives = policy
            .Split(';', StringSplitOptions.TrimEntries)
            .Where(static directive => directive.Length > 0)
            .ToList();

        for (var index = 0; index < directives.Count; index++)
        {
            if (!directives[index].StartsWith("connect-src", StringComparison.OrdinalIgnoreCase))
                continue;

            directives[index] = AugmentConnectSourceDirective(directives[index]);
            return string.Join("; ", directives) + ";";
        }

        directives.Add("connect-src 'self' ws: wss:");
        return string.Join("; ", directives) + ";";
    }

    private static string AugmentConnectSourceDirective(string directive)
    {
        var parts = directive.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (parts.Count == 0)
            return "connect-src 'self' ws: wss:";

        var values = parts.Skip(1).ToList();
        if (values.Contains("'none'", StringComparer.Ordinal))
        {
            values.Clear();
            values.Add("'self'");
        }

        if (!values.Contains("'self'", StringComparer.Ordinal))
            values.Add("'self'");
        if (!values.Contains("ws:", StringComparer.OrdinalIgnoreCase))
            values.Add("ws:");
        if (!values.Contains("wss:", StringComparer.OrdinalIgnoreCase))
            values.Add("wss:");

        return "connect-src " + string.Join(' ', values);
    }
}
