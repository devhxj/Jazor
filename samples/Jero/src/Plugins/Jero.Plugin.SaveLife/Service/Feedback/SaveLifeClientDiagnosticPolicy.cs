using System.Text.RegularExpressions;

namespace Jero.Plugin.SaveLife.Service;

public sealed class SaveLifeClientDiagnostic
{
    public string Route { get; init; } = "/unknown";

    public int DurationMs { get; init; }

    public int StatusCode { get; init; }

    public string Version { get; init; } = "unknown";

    public string Environment { get; init; } = "unknown";

    public string NetworkType { get; init; } = "unknown";
}

public static class SaveLifeClientDiagnosticPolicy
{
    private static readonly Regex NumericPathSegmentPattern = new(
        @"/\d+(?=/|$)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex SafeRoutePattern = new(
        @"^[A-Za-z0-9_/:.\-]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex VersionPattern = new(
        @"^\d{1,4}(?:\.\d{1,4}){1,3}(?:[-+][A-Za-z0-9.\-]{1,16})?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static SaveLifeClientDiagnostic Sanitize(SubmitSaveLifeClientDiagnosticInput input)
    {
        return new SaveLifeClientDiagnostic
        {
            Route = NormalizeRoute(input.Route),
            DurationMs = Math.Clamp(input.DurationMs, 0, 120000),
            StatusCode = Math.Clamp(input.StatusCode, 0, 599),
            Version = NormalizeText(input.Version, 32),
            Environment = NormalizeEnvironment(input.Environment),
            NetworkType = NormalizeNetworkType(input.NetworkType)
        };
    }

    private static string NormalizeRoute(string? value)
    {
        var route = value?.Trim() ?? string.Empty;
        var separatorIndex = route.IndexOfAny(['?', '#']);
        if (separatorIndex >= 0)
            route = route[..separatorIndex];

        if (!route.StartsWith("/save-life/", StringComparison.Ordinal) &&
            !route.StartsWith("/agent/", StringComparison.Ordinal))
            return "/unknown";
        if (!SafeRoutePattern.IsMatch(route))
            return "/unknown";

        route = NumericPathSegmentPattern.Replace(route, "/:id");
        return route.Length <= 128 ? route : route[..128];
    }

    private static string NormalizeEnvironment(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "production" => "production",
            "development" => "development",
            "trial" => "trial",
            "test" => "test",
            _ => "unknown"
        };
    }

    private static string NormalizeNetworkType(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "wifi" => "wifi",
            "2g" => "2g",
            "3g" => "3g",
            "4g" => "4g",
            "5g" => "5g",
            "none" => "none",
            _ => "unknown"
        };
    }

    private static string NormalizeText(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return "unknown";

        if (normalized is "development" or "unknown")
            return normalized;
        if (!VersionPattern.IsMatch(normalized))
            return "unknown";

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
