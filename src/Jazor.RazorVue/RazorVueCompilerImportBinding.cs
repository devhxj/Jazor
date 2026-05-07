namespace Jazor.RazorVue;

internal enum RazorVueCompilerImportKind
{
    Default,
    Namespace,
    Named
}

internal sealed record RazorVueCompilerImportBinding(
    string ModulePath,
    RazorVueCompilerImportKind Kind,
    string LocalName,
    string? ImportedName);
