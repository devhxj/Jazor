namespace Jazor.VueHost.LanguageServers;

internal sealed class ExternalProcessOptions
{
    public required string Name { get; init; }

    public required string FileName { get; init; }

    public string[] Arguments { get; init; } = [];

    public string? WorkingDirectory { get; init; }

    public IReadOnlyDictionary<string, string?> EnvironmentVariables { get; init; }
        = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}
