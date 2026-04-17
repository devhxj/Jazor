namespace Jazor.VueHost.Extensions;

internal sealed class NullExtensionRegistry : IExtensionRegistry
{
    public static NullExtensionRegistry Instance { get; } = new();

    private NullExtensionRegistry()
    {
    }

    public void RegisterExtension(IExtension extension)
    {
    }

    public void RegisterLspDiagnosticProvider(ILspDiagnosticProvider provider)
    {
    }

    public void RegisterLspCodeActionProvider(ILspCodeActionProvider provider)
    {
    }

    public IReadOnlyDictionary<string, IExtension> GetExtensions()
        => new Dictionary<string, IExtension>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ILspDiagnosticProvider> GetLspDiagnosticProviders()
        => Array.Empty<ILspDiagnosticProvider>();

    public IReadOnlyList<ILspCodeActionProvider> GetLspCodeActionProviders()
        => Array.Empty<ILspCodeActionProvider>();
}
