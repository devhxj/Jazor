using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NET.Sdk.Razor.SourceGenerators;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorBridge
{
    private const string RazorHostOutputsPropertyName = "build_property.EnableRazorHostOutputs";

    public static bool TryCreateCarrier(
        Compilation compilation,
        string documentPath,
        string documentText,
        string projectDirectory,
        out BridgedCarrier? carrier,
        out string? failure)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (string.IsNullOrWhiteSpace(documentPath))
            throw new ArgumentException("Document path cannot be empty.", nameof(documentPath));
        if (documentText is null)
            throw new ArgumentNullException(nameof(documentText));

        carrier = null;
        failure = null;

        if (!TryCreateCodeDocument(compilation, documentPath, documentText, projectDirectory, out var codeDocument, out failure))
            return false;

        if (!TryReadCarrier(codeDocument!, out carrier, out failure))
            return false;

        return true;
    }

    internal static bool TryCreateCodeDocument(
        Compilation compilation,
        string documentPath,
        string documentText,
        string projectDirectory,
        out object? codeDocument,
        out string? failure)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (string.IsNullOrWhiteSpace(documentPath))
            throw new ArgumentException("Document path cannot be empty.", nameof(documentPath));
        if (documentText is null)
            throw new ArgumentNullException(nameof(documentText));

        codeDocument = null;
        failure = null;

        var parseOptions = GetParseOptions(compilation);
        var additionalText = new InMemoryAdditionalText(documentPath, documentText);
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_property.RazorLangVersion"] = "9.0",
                ["build_property.RootNamespace"] = GetRootNamespace(compilation),
                ["build_property.SupportLocalizedComponentNames"] = "true",
                ["build_property.GenerateRazorMetadataSourceChecksumAttributes"] = "false",
                ["build_property.MSBuildProjectDirectory"] = projectDirectory,
                [RazorHostOutputsPropertyName] = "true"
            },
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                [documentPath] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(GetRelativePath(projectDirectory, documentPath)))
                }
            });

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new RazorSourceGenerator().AsSourceGenerator()],
            additionalTexts: [additionalText],
            parseOptions: parseOptions,
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
        if (diagnostics.Length > 0)
        {
            failure = string.Join("\n", diagnostics.Select(static item => item.ToString()));
            return false;
        }

        var runResult = driver.GetRunResult();
        if (runResult.Results.Length == 0)
        {
            failure = "The SDK Razor source generator did not produce a run result.";
            return false;
        }

        var generatorResult = runResult.Results[0];
        var hostOutput = GetHostOutputValue(generatorResult, "RazorGeneratorResult");
        if (hostOutput is null)
        {
            failure = "The SDK Razor source generator did not publish RazorGeneratorResult into HostOutputs.";
            return false;
        }

        var method = hostOutput.GetType().GetMethod(
            "GetCodeDocument",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(string)],
            modifiers: null);
        if (method is null)
        {
            failure = "RazorGeneratorResult.GetCodeDocument(string) was not available.";
            return false;
        }

        codeDocument = method.Invoke(hostOutput, [documentPath]);
        if (codeDocument is null)
        {
            failure = "RazorGeneratorResult.GetCodeDocument(string) returned null.";
            return false;
        }

        return true;
    }

    private static bool TryReadCarrier(
        object codeDocument,
        out BridgedCarrier? carrier,
        out string? failure)
    {
        carrier = null;
        failure = null;

        var source = codeDocument.GetType().GetProperty("Source", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(codeDocument);
        if (source is null)
        {
            failure = "RazorCodeDocument.Source was not available.";
            return false;
        }

        var documentPath = source.GetType().GetProperty("FilePath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(source) as string;
        if (string.IsNullOrWhiteSpace(documentPath))
        {
            failure = "RazorCodeDocument.Source.FilePath was not available.";
            return false;
        }

        var documentText = ReadRazorSourceText(source);
        var importsProperty = codeDocument.GetType().GetProperty("Imports", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var imports = ImmutableArray<BridgedImport>.Empty;
        if (importsProperty?.GetValue(codeDocument) is IEnumerable importEntries)
        {
            var builder = ImmutableArray.CreateBuilder<BridgedImport>();
            foreach (var import in importEntries)
            {
                if (import is null)
                    continue;

                var importPath = import.GetType().GetProperty("FilePath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(import) as string;
                if (string.IsNullOrWhiteSpace(importPath))
                    continue;

                builder.Add(new BridgedImport(importPath!, ReadRazorSourceText(import)));
            }

            imports = builder.ToImmutable();
        }

        carrier = new BridgedCarrier(documentPath!, documentText, imports);
        return true;
    }

    private static string ReadRazorSourceText(object sourceDocument)
    {
        var textProperty = sourceDocument.GetType().GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var textValue = textProperty?.GetValue(sourceDocument);
        if (textValue is not null)
        {
            return textValue.ToString() ?? string.Empty;
        }

        var lengthValue = sourceDocument.GetType().GetProperty("Length", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(sourceDocument);
        if (lengthValue is not int length || length <= 0)
            return string.Empty;

        var buffer = new char[length];
        var copyTo = sourceDocument.GetType().GetMethod(
            "CopyTo",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(int), typeof(char[]), typeof(int), typeof(int)],
            modifiers: null);
        copyTo?.Invoke(sourceDocument, [0, buffer, 0, length]);
        return new string(buffer);
    }

    private static object? GetHostOutputValue(object generatorResult, string key)
    {
        var property = generatorResult.GetType().GetProperty("HostOutputs");
        if (property?.GetValue(generatorResult) is not IEnumerable entries)
            return null;

        foreach (var entry in entries)
        {
            if (entry is null)
                continue;

            var entryType = entry.GetType();
            var entryKey = entryType.GetProperty("Key")?.GetValue(entry) as string;
            if (!string.Equals(entryKey, key, StringComparison.Ordinal))
                continue;

            return entryType.GetProperty("Value")?.GetValue(entry);
        }

        return null;
    }

    private static CSharpParseOptions GetParseOptions(Compilation compilation)
        => compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
           ?? CSharpParseOptions.Default;

    private static string GetRootNamespace(Compilation compilation)
        => compilation.AssemblyName ?? "Jazor.Assembly";

    private static string GetRelativePath(string projectDirectory, string documentPath)
    {
        if (string.IsNullOrWhiteSpace(projectDirectory))
            return Path.GetFileName(documentPath);

        var normalizedProjectDirectory = Path.GetFullPath(projectDirectory);
        var normalizedDocumentPath = Path.GetFullPath(documentPath);
        if (!normalizedDocumentPath.StartsWith(normalizedProjectDirectory, StringComparison.OrdinalIgnoreCase))
            return Path.GetFileName(documentPath);

        var relativePath = normalizedDocumentPath.Substring(normalizedProjectDirectory.Length);
        return relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);

        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => _text;
    }

    private sealed class TestAnalyzerConfigOptionsProvider(
        IReadOnlyDictionary<string, string> globalOptions,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> additionalFileOptions) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _additionalFileOptions = additionalFileOptions;
        private static readonly AnalyzerConfigOptions EmptyOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>(StringComparer.Ordinal));

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            => EmptyOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => _additionalFileOptions.TryGetValue(textFile.Path, out var values)
                ? new TestAnalyzerConfigOptions(values)
                : EmptyOptions;
    }

    private sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> _values = values;

        public override bool TryGetValue(string key, out string value)
            => _values.TryGetValue(key, out value!);
    }

    internal sealed record BridgedCarrier(
        string DocumentPath,
        string DocumentText,
        ImmutableArray<BridgedImport> Imports);

    internal sealed record BridgedImport(
        string Path,
        string Text);
}
