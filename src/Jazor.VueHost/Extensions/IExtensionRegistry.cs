namespace Jazor.VueHost.Extensions;

internal interface IExtensionRegistry
{
    void RegisterExtension(IExtension extension);

    void RegisterLspDiagnosticProvider(ILspDiagnosticProvider provider);

    void RegisterLspCodeActionProvider(ILspCodeActionProvider provider);

    void RegisterLspHoverProvider(ILspHoverProvider provider);

    void RegisterLspCompletionProvider(ILspCompletionProvider provider);

    void RegisterLspDocumentSymbolProvider(ILspDocumentSymbolProvider provider);

    void RegisterLspReferenceProvider(ILspReferenceProvider provider);

    void RegisterLspRenameProvider(ILspRenameProvider provider);

    IReadOnlyDictionary<string, IExtension> GetExtensions();

    IReadOnlyList<ILspDiagnosticProvider> GetLspDiagnosticProviders();

    IReadOnlyList<ILspCodeActionProvider> GetLspCodeActionProviders();

    IReadOnlyList<ILspHoverProvider> GetLspHoverProviders();

    IReadOnlyList<ILspCompletionProvider> GetLspCompletionProviders();

    IReadOnlyList<ILspDocumentSymbolProvider> GetLspDocumentSymbolProviders();

    IReadOnlyList<ILspReferenceProvider> GetLspReferenceProviders();

    IReadOnlyList<ILspRenameProvider> GetLspRenameProviders();

    void ReportProviderInvocation(ExtensionProviderInvocation invocation);

    IReadOnlyList<ExtensionProviderHealth> GetProviderHealth();
}
