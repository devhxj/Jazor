namespace Jazor.VueHost.Extensions.Builtin;

internal static class BuiltinExtensionCatalog
{
    public static IReadOnlyList<IExtension> Create()
    {
        return
        [
            new StructureDiagnosticExtension(),
            new DirectiveCompletionExtension(),
            new ComponentCodeActionExtension(),
            new WorkspaceSymbolExtension()
        ];
    }
}
