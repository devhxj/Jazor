namespace Jazor.VueHost.Extensions;

internal interface IExtensionRegistry
{
    void RegisterExtension(IExtension extension);

    void UnregisterExtension(IExtension extension);

    void RegisterLspDiagnosticProvider(ILspDiagnosticProvider provider);

    void RegisterLspCodeActionProvider(ILspCodeActionProvider provider);

    void RegisterLspHoverProvider(ILspHoverProvider provider);

    void RegisterLspCompletionProvider(ILspCompletionProvider provider);

    void RegisterLspDocumentSymbolProvider(ILspDocumentSymbolProvider provider);

    void RegisterLspSignatureHelpProvider(ILspSignatureHelpProvider provider);

    void RegisterLspInlayHintProvider(ILspInlayHintProvider provider);

    void RegisterLspWorkspaceSymbolProvider(ILspWorkspaceSymbolProvider provider);

    void RegisterLspFoldingRangeProvider(ILspFoldingRangeProvider provider);

    void RegisterLspReferenceProvider(ILspReferenceProvider provider);

    void RegisterLspRenameProvider(ILspRenameProvider provider);

    IReadOnlyDictionary<string, IExtension> GetExtensions();

    IReadOnlyList<ILspDiagnosticProvider> GetLspDiagnosticProviders();

    IReadOnlyList<ILspCodeActionProvider> GetLspCodeActionProviders();

    IReadOnlyList<ILspHoverProvider> GetLspHoverProviders();

    IReadOnlyList<ILspCompletionProvider> GetLspCompletionProviders();

    IReadOnlyList<ILspDocumentSymbolProvider> GetLspDocumentSymbolProviders();

    IReadOnlyList<ILspSignatureHelpProvider> GetLspSignatureHelpProviders();

    IReadOnlyList<ILspInlayHintProvider> GetLspInlayHintProviders();

    IReadOnlyList<ILspWorkspaceSymbolProvider> GetLspWorkspaceSymbolProviders();

    IReadOnlyList<ILspFoldingRangeProvider> GetLspFoldingRangeProviders();

    IReadOnlyList<ILspReferenceProvider> GetLspReferenceProviders();

    IReadOnlyList<ILspRenameProvider> GetLspRenameProviders();

    void ReportExtensionLoad(ExtensionLoadInvocation invocation);

    IReadOnlyList<ExtensionLoadHealth> GetExtensionLoadHealth();

    IReadOnlyList<ExtensionLoadInvocation> GetRecentExtensionLoadInvocations(int maxCount = 100);

    void ReportProviderInvocation(ExtensionProviderInvocation invocation);

    IReadOnlyList<ExtensionProviderHealth> GetProviderHealth();

    IReadOnlyList<ExtensionProviderInvocationSnapshot> GetRecentProviderInvocations(int maxCount = 200);
}
