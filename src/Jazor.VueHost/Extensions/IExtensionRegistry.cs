namespace Jazor.VueHost.Extensions;

internal interface IExtensionRegistry
{
    void RegisterExtension(IExtension extension);

    void RegisterLspDiagnosticProvider(ILspDiagnosticProvider provider);

    void RegisterLspCodeActionProvider(ILspCodeActionProvider provider);

    IReadOnlyDictionary<string, IExtension> GetExtensions();

    IReadOnlyList<ILspDiagnosticProvider> GetLspDiagnosticProviders();

    IReadOnlyList<ILspCodeActionProvider> GetLspCodeActionProviders();
}
