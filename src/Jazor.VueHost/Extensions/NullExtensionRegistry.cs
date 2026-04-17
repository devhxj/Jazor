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

    public void RegisterLspHoverProvider(ILspHoverProvider provider)
    {
    }

    public void RegisterLspCompletionProvider(ILspCompletionProvider provider)
    {
    }

    public void RegisterLspDocumentSymbolProvider(ILspDocumentSymbolProvider provider)
    {
    }

    public void RegisterLspReferenceProvider(ILspReferenceProvider provider)
    {
    }

    public void RegisterLspRenameProvider(ILspRenameProvider provider)
    {
    }

    public IReadOnlyDictionary<string, IExtension> GetExtensions()
        => new Dictionary<string, IExtension>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ILspDiagnosticProvider> GetLspDiagnosticProviders()
        => Array.Empty<ILspDiagnosticProvider>();

    public IReadOnlyList<ILspCodeActionProvider> GetLspCodeActionProviders()
        => Array.Empty<ILspCodeActionProvider>();

    public IReadOnlyList<ILspHoverProvider> GetLspHoverProviders()
        => Array.Empty<ILspHoverProvider>();

    public IReadOnlyList<ILspCompletionProvider> GetLspCompletionProviders()
        => Array.Empty<ILspCompletionProvider>();

    public IReadOnlyList<ILspDocumentSymbolProvider> GetLspDocumentSymbolProviders()
        => Array.Empty<ILspDocumentSymbolProvider>();

    public IReadOnlyList<ILspReferenceProvider> GetLspReferenceProviders()
        => Array.Empty<ILspReferenceProvider>();

    public IReadOnlyList<ILspRenameProvider> GetLspRenameProviders()
        => Array.Empty<ILspRenameProvider>();

    public void ReportProviderInvocation(ExtensionProviderInvocation invocation)
    {
    }

    public IReadOnlyList<ExtensionProviderHealth> GetProviderHealth()
        => Array.Empty<ExtensionProviderHealth>();
}
