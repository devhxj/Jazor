namespace Jazor.AspNetCore.Dev;

/// <summary>Detects the environment markers emitted by ASP.NET Core browser refresh tooling.</summary>
internal static class ExternalBrowserRefresh
{
    private const string BrowserToolsVariableName = "__ASPNETCORE_BROWSER_TOOLS";
    private const string AutoReloadEndpointVariableName = "ASPNETCORE_AUTO_RELOAD_WS_ENDPOINT";
    private const string AutoReloadKeyVariableName = "ASPNETCORE_AUTO_RELOAD_WS_KEY";

    /// <summary>Returns whether the supplied environment lookup indicates an active external refresh transport.</summary>
    public static bool IsActive(Func<string, string?> getEnvironmentVariable)
    {
        ArgumentNullException.ThrowIfNull(getEnvironmentVariable);

        if (!IsTruthy(getEnvironmentVariable(BrowserToolsVariableName)))
            return false;

        return HasValue(getEnvironmentVariable(AutoReloadEndpointVariableName))
            || HasValue(getEnvironmentVariable(AutoReloadKeyVariableName));
    }

    private static bool HasValue(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static bool IsTruthy(string? value)
        => string.Equals(value, "1", StringComparison.Ordinal)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
}
