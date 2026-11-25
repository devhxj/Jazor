using System.ClientModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentDiagnostics
{
    private const int DefaultMaxLength = 300;
    private const int DefaultUserMessageLogMaxLength = 500;
    private const int ServiceErrorMaxLength = 180;
    private const int MaxServiceErrorPayloadLength = 64 * 1024;

    internal static string BuildCorrelationId(
        string kind,
        string correlationScope,
        string entityId)
    {
        if (string.IsNullOrWhiteSpace(correlationScope))
            throw new ArgumentException("关联范围不能为空", nameof(correlationScope));
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("关联实体不能为空", nameof(entityId));

        var prefix = kind switch
        {
            "patient" => "pat",
            "message" => "msg",
            _ => "ref"
        };
        var value = $"save-life-agent-v1\n{kind}\n{correlationScope.Trim()}\n{entityId.Trim()}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return $"{prefix}_{Convert.ToHexString(hash)[..24].ToLowerInvariant()}";
    }

    public static string? BuildErrorSummary(Exception? exception, int maxLength = DefaultMaxLength)
    {
        if (exception == null)
            return null;

        var clientException = FindClientResultException(exception);
        var root = exception.GetBaseException();
        var source = IsEmbeddingRequestFailure(exception)
            ? exception
            : clientException ?? root;
        var summary = $"{source.GetType().Name}: {source.Message}";
        var serviceError = clientException == null ? null : TryReadServiceError(clientException);

        if (!string.IsNullOrWhiteSpace(serviceError) &&
            !summary.Contains(serviceError, StringComparison.OrdinalIgnoreCase))
            summary = $"{summary}; provider: {serviceError}";

        return SanitizeText(summary, maxLength);
    }

    public static string BuildEmbeddingRequestErrorMessage(
        Exception exception,
        string? model,
        int inputLength,
        Uri? endpoint = null,
        int maxLength = 512)
    {
        var modelText = SanitizeText(model, 120) ?? "未提供";
        var endpointText = endpoint == null
            ? null
            : SanitizeText($"{endpoint.Scheme}://{endpoint.Authority}{endpoint.AbsolutePath}", 180);
        var detail = BuildErrorSummary(exception, 300) ?? "调用失败";
        var summary = $"文本嵌入请求失败。model={modelText}; inputChars={Math.Max(0, inputLength)}";
        if (!string.IsNullOrWhiteSpace(endpointText))
            summary += $"; endpoint={endpointText}";

        summary += $"; {detail}";
        if (IsLikelyShortContextBge(model) && inputLength > 450)
        {
            summary += "；提示：BGE v1.5 常见上下文上限为 512 tokens，请核对供应商限制或改用长上下文模型。";
        }

        return SanitizeText(summary, maxLength) ?? "文本嵌入请求失败";
    }

    public static string? ExtractServiceError(string? content, int maxLength = ServiceErrorMaxLength)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        if (content.Length > MaxServiceErrorPayloadLength)
            return null;

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var error = FindProperty(root, "error");
            var message = ReadProperty(error, "message")
                ?? ReadProperty(root, "message")
                ?? ReadProperty(root, "msg")
                ?? (error.HasValue ? ReadScalar(error.Value) : null);
            var type = ReadProperty(error, "type") ?? ReadProperty(root, "type");
            var code = ReadProperty(error, "code") ?? ReadProperty(root, "code");
            var parts = new[]
            {
                message,
                type is null ? null : $"type={type}",
                code is null ? null : $"code={code}"
            }.Where(u => !string.IsNullOrWhiteSpace(u));

            return SanitizeText(string.Join("; ", parts), maxLength);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ClientResultException? FindClientResultException(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is ClientResultException clientException)
                return clientException;
        }

        return null;
    }

    private static bool IsLikelyShortContextBge(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
            return false;

        var normalized = model.Trim();
        var modelName = normalized[(normalized.LastIndexOf('/') + 1)..];
        return modelName.Contains("bge-", StringComparison.OrdinalIgnoreCase) &&
            modelName.Contains("v1.5", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEmbeddingRequestFailure(Exception exception)
    {
        return exception.Message.StartsWith("文本嵌入请求失败。", StringComparison.Ordinal);
    }

    private static string? TryReadServiceError(ClientResultException exception)
    {
        try
        {
            var response = exception.GetRawResponse();
            return response == null ? null : ExtractServiceError(response.Content.ToString());
        }
        catch
        {
            return null;
        }
    }

    private static JsonElement? FindProperty(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                return property.Value;
        }

        return null;
    }

    private static string? ReadProperty(JsonElement? element, string name)
    {
        return element.HasValue
            ? ReadScalar(FindProperty(element.Value, name))
            : null;
    }

    private static string? ReadScalar(JsonElement? element)
    {
        if (!element.HasValue)
            return null;

        var value = element.Value;
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()?.Trim(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    public static string? SanitizeText(string? value, int maxLength = DefaultMaxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var sanitized = value.Trim();
        sanitized = Regex.Replace(
            sanitized,
            @"(?i)\b(api[-_ ]?key|token|secret|authorization)\s*[:=]\s*(bearer\s+)?[A-Za-z0-9_\-\.\/\+=]{8,}",
            "$1=[redacted]");
        sanitized = Regex.Replace(
            sanitized,
            @"(?i)\bbearer\s+[A-Za-z0-9_\-\.\/\+=]{8,}",
            "Bearer [redacted]");
        sanitized = Regex.Replace(
            sanitized,
            @"\bsk-[A-Za-z0-9_\-]{8,}",
            "sk-[redacted]");
        sanitized = Regex.Replace(
            sanitized,
            @"\b[A-Za-z0-9_\-]{24,}\.[A-Za-z0-9_\-]{6,}\.[A-Za-z0-9_\-]{6,}\b",
            "[redacted-token]");

        return sanitized.Length <= maxLength ? sanitized : sanitized[..maxLength];
    }

    internal static string BuildUserMessageLogText(
        string? value,
        int maxLength = DefaultUserMessageLogMaxLength)
    {
        var sanitized = SanitizeText(value, int.MaxValue);
        if (string.IsNullOrWhiteSpace(sanitized))
            return "(empty)";

        sanitized = Regex.Replace(
            sanitized,
            @"(?i)(?<![A-Z0-9._%+\-])([A-Z0-9])[A-Z0-9._%+\-]*@([A-Z0-9.\-]+\.[A-Z]{2,})(?![A-Z0-9.\-])",
            "$1***@$2");
        sanitized = Regex.Replace(
            sanitized,
            @"(?<!\d)(\d{6})\d{8}(\d{3}[0-9Xx])(?![0-9A-Za-z])",
            "$1********$2");
        sanitized = Regex.Replace(
            sanitized,
            @"(?<!\d)(\d{6})\d{6}(\d{3})(?!\d)",
            "$1******$2");
        sanitized = Regex.Replace(
            sanitized,
            @"(?<!\d)(1[3-9]\d)\d{4}(\d{4})(?!\d)",
            "$1****$2");
        sanitized = Regex.Replace(
            sanitized,
            @"(?<!\d)(0\d{2,3})-?\d{3,4}(\d{4})(?!\d)",
            "$1****$2");
        sanitized = Regex.Replace(
            sanitized,
            @"(?<!\d)(\d{4})\d{8,11}(\d{4})(?!\d)",
            "$1********$2");
        sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();

        var safeMaxLength = Math.Clamp(maxLength, 1, 2000);
        return sanitized.Length <= safeMaxLength
            ? sanitized
            : sanitized[..safeMaxLength];
    }

    internal static string BuildLogValueList(
        IEnumerable<string?> values,
        int maxLength = DefaultUserMessageLogMaxLength)
    {
        ArgumentNullException.ThrowIfNull(values);
        var text = string.Join(
            " | ",
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));
        return BuildUserMessageLogText(text, maxLength);
    }
}
