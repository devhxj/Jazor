namespace Jazor.Vue.Analysis.Internal;

internal enum JazorImportKind
{
    JSImport,
    VueImport
}

internal enum JazorImportBindingKind
{
    Default,
    Named,
    Namespace
}

internal enum ExternalExportKind
{
    Default,
    Named,
    Namespace
}

internal enum ExternalSymbolKind
{
    Function,
    Value,
    Object,
    Component,
    Composable,
    TypeOnly,
    Namespace
}

internal enum ExternalTypeQuality
{
    Exact,
    Structural,
    Opaque
}

internal sealed class JazorImportBinding
{
    public JazorImportBinding(string localName, string? importedName, JazorImportBindingKind bindingKind)
    {
        LocalName = localName ?? throw new ArgumentNullException(nameof(localName));
        ImportedName = importedName;
        BindingKind = bindingKind;
    }

    public string LocalName { get; }

    public string? ImportedName { get; }

    public JazorImportBindingKind BindingKind { get; }
}

internal sealed class JazorImportDirective
{
    public JazorImportDirective(
        JazorImportKind kind,
        string source,
        IReadOnlyList<JazorImportBinding> bindings,
        string rawText)
    {
        Kind = kind;
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
        RawText = rawText ?? throw new ArgumentNullException(nameof(rawText));
    }

    public JazorImportKind Kind { get; }

    public string Source { get; }

    public IReadOnlyList<JazorImportBinding> Bindings { get; }

    public string RawText { get; }
}

internal sealed class ExternalSymbolDescriptor
{
    public ExternalSymbolDescriptor(
        string publicName,
        string importSource,
        ExternalExportKind exportKind,
        ExternalSymbolKind symbolKind,
        string runtimeImportName,
        bool templateVisible,
        ExternalTypeQuality typeQuality)
    {
        PublicName = publicName ?? throw new ArgumentNullException(nameof(publicName));
        ImportSource = importSource ?? throw new ArgumentNullException(nameof(importSource));
        ExportKind = exportKind;
        SymbolKind = symbolKind;
        RuntimeImportName = runtimeImportName ?? throw new ArgumentNullException(nameof(runtimeImportName));
        TemplateVisible = templateVisible;
        TypeQuality = typeQuality;
    }

    public string PublicName { get; }

    public string ImportSource { get; }

    public ExternalExportKind ExportKind { get; }

    public ExternalSymbolKind SymbolKind { get; }

    public string RuntimeImportName { get; }

    public bool TemplateVisible { get; }

    public ExternalTypeQuality TypeQuality { get; }
}

internal sealed class VirtualExternalSymbolTable
{
    public VirtualExternalSymbolTable(IReadOnlyList<ExternalSymbolDescriptor> symbols)
    {
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
    }

    public IReadOnlyList<ExternalSymbolDescriptor> Symbols { get; }

    public static VirtualExternalSymbolTable FromImports(IReadOnlyList<JazorImportDirective> imports)
    {
        if (imports is null)
        {
            throw new ArgumentNullException(nameof(imports));
        }

        var symbols = new List<ExternalSymbolDescriptor>();
        foreach (var import in imports)
        {
            foreach (var binding in import.Bindings)
            {
                var exportKind = binding.BindingKind switch
                {
                    JazorImportBindingKind.Default => ExternalExportKind.Default,
                    JazorImportBindingKind.Named => ExternalExportKind.Named,
                    JazorImportBindingKind.Namespace => ExternalExportKind.Namespace,
                    _ => ExternalExportKind.Named
                };

                var symbolKind = import.Kind switch
                {
                    JazorImportKind.VueImport => ExternalSymbolKind.Component,
                    _ when binding.BindingKind == JazorImportBindingKind.Namespace => ExternalSymbolKind.Namespace,
                    _ when binding.LocalName.StartsWith("use", StringComparison.Ordinal) => ExternalSymbolKind.Composable,
                    _ when binding.BindingKind == JazorImportBindingKind.Named => ExternalSymbolKind.Function,
                    _ => ExternalSymbolKind.Value
                };

                symbols.Add(new ExternalSymbolDescriptor(
                    binding.LocalName,
                    import.Source,
                    exportKind,
                    symbolKind,
                    binding.LocalName,
                    templateVisible: import.Kind == JazorImportKind.VueImport,
                    ExternalTypeQuality.Opaque));
            }
        }

        return new VirtualExternalSymbolTable(symbols);
    }
}

internal sealed class JazorVueDocument
{
    public JazorVueDocument(
        string filePath,
        string sourceText,
        IReadOnlyList<JazorImportDirective> imports,
        string template,
        string code,
        int codeStartIndex)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        SourceText = sourceText ?? throw new ArgumentNullException(nameof(sourceText));
        Imports = imports ?? throw new ArgumentNullException(nameof(imports));
        Template = template ?? throw new ArgumentNullException(nameof(template));
        Code = code ?? throw new ArgumentNullException(nameof(code));
        CodeStartIndex = codeStartIndex;
    }

    public string FilePath { get; }

    public string SourceText { get; }

    public IReadOnlyList<JazorImportDirective> Imports { get; }

    public string Template { get; }

    public string Code { get; }

    public int CodeStartIndex { get; }
}

internal sealed class JazorVueCompilationResult
{
    public JazorVueCompilationResult(
        JazorVueDocument document,
        VirtualExternalSymbolTable externalSymbols,
        string generatedVueText,
        IReadOnlyList<string> diagnostics)
        : this(
            document,
            externalSymbols,
            generatedVueText,
            CreateGeneratedExternalDeclarationsText(document, externalSymbols),
            diagnostics)
    {
    }

    public JazorVueCompilationResult(
        JazorVueDocument document,
        VirtualExternalSymbolTable externalSymbols,
        string generatedVueText,
        string generatedExternalDeclarationsText,
        IReadOnlyList<string> diagnostics)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        ExternalSymbols = externalSymbols ?? throw new ArgumentNullException(nameof(externalSymbols));
        GeneratedVueText = generatedVueText ?? throw new ArgumentNullException(nameof(generatedVueText));
        GeneratedExternalDeclarationsText = generatedExternalDeclarationsText ?? throw new ArgumentNullException(nameof(generatedExternalDeclarationsText));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
    }

    public JazorVueDocument Document { get; }

    public VirtualExternalSymbolTable ExternalSymbols { get; }

    public string GeneratedVueText { get; }

    public string GeneratedExternalDeclarationsText { get; }

    public IReadOnlyList<string> Diagnostics { get; }

    private static string CreateGeneratedExternalDeclarationsText(
        JazorVueDocument document,
        VirtualExternalSymbolTable externalSymbols)
    {
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (externalSymbols is null)
        {
            throw new ArgumentNullException(nameof(externalSymbols));
        }

        return JazorVueExternalDeclarationEmitter.Emit(
            externalSymbols,
            JazorVueExternalDeclarationEmitter.DefaultNamespace,
            JazorVueExternalDeclarationEmitter.CreateContainerName(document.FilePath));
    }
}

internal static class JazorVueNaming
{
    public static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (name.Length == 1)
        {
            return name.ToLowerInvariant();
        }

        return char.ToLowerInvariant(name[0]).ToString() + name.Substring(1);
    }
}
