using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using Jazor.RazorVue;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorFallbackOutput
{
    internal const string FallbackRequiredFailureMarker = "fallback Razor SG host-output generation is required";
    private const string RazorCompilerAssemblyName = "Microsoft.CodeAnalysis.Razor.Compiler";
    private const string RazorSourceGeneratorTypeName = "Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator";
    private const string RazorGeneratorResultTypeName = "Microsoft.NET.Sdk.Razor.SourceGenerators.RazorGeneratorResult";
    internal static void Register(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AnalyzerConfigOptionsProvider
            .Select(static (optionsProvider, _) => RazorSourceGeneratorHostOutputHookOptions.CreateTailOutputOptions(optionsProvider));
        var additionalTexts = context.AdditionalTextsProvider.Collect();
        var metadataReferences = context.MetadataReferencesProvider.Collect();
        var input = context.CompilationProvider
            .Combine(context.ParseOptionsProvider)
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Combine(additionalTexts)
            .Combine(metadataReferences)
            .Combine(options);

        context.RegisterSourceOutput(input, static (outputContext, source) =>
        {
            var (withReferences, tailOptions) = source;
            var (withAdditionalTexts, metadataReferenceValues) = withReferences;
            var (withAnalyzerOptions, additionalTextValues) = withAdditionalTexts;
            var (withParseOptions, analyzerOptions) = withAnalyzerOptions;
            var (compilation, parseOptions) = withParseOptions;
            Emit(
                outputContext,
                compilation,
                parseOptions,
                analyzerOptions,
                additionalTextValues,
                metadataReferenceValues,
                tailOptions);
        });
    }

    internal static bool IsFallbackRequiredFailure(string? failure)
    {
        if (string.IsNullOrWhiteSpace(failure))
            return false;

        return failure!.IndexOf(FallbackRequiredFailureMarker, StringComparison.Ordinal) >= 0;
    }

    private static void Emit(
        SourceProductionContext context,
        Compilation compilation,
        ParseOptions parseOptions,
        AnalyzerConfigOptionsProvider analyzerOptions,
        ImmutableArray<AdditionalText> additionalTexts,
        ImmutableArray<MetadataReference> metadataReferences,
        RazorSourceGeneratorTailOutputOptions options)
    {
        if (!options.Enabled)
            return;

        if (!RequiresFallbackTailOutput(compilation))
        {
            EmitTraceIfEnabled(context, options, 0, "fallback-not-required", null);
            return;
        }

        try
        {
            if (!TryCollectDocuments(
                    compilation,
                    parseOptions,
                    analyzerOptions,
                    additionalTexts,
                    metadataReferences,
                    out var documents,
                    out var failure))
            {
                ReportFailure(context, failure ?? "RazorVue could not collect Razor SG fallback documents.");
                EmitTraceIfEnabled(context, options, 0, "fallback-failed", failure);
                return;
            }

            RazorSourceGeneratorTailOutput.EmitDocuments(context, compilation, documents, options);
            EmitTraceIfEnabled(context, options, documents.Length, "fallback-collected", null);
        }
        catch (Exception ex)
        {
            ReportFailure(context, ex.GetType().Name + ": " + ex.Message);
            EmitTraceIfEnabled(context, options, 0, "fallback-exception", ex.GetType().FullName + ": " + ex.Message);
        }
    }

    private static bool RequiresFallbackTailOutput(Compilation compilation)
    {
        var razorVueContext = RazorVueCompilationContext.TryCreate(compilation);
        if (razorVueContext is null)
            return false;

        var snapshots = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(razorVueContext);
        return snapshots.Any(static snapshot =>
            snapshot.RazorIrCarrier is null &&
            snapshot.RazorSourceGeneratorDocument is null &&
            (snapshot.BuildRenderTreeMethod is null ||
             !RazorVueBuildRenderTreeAuthoringClassifier.IsHandwrittenBuildRenderTree(snapshot)));
    }

    private static bool TryCollectDocuments(
        Compilation compilation,
        ParseOptions parseOptions,
        AnalyzerConfigOptionsProvider analyzerOptions,
        ImmutableArray<AdditionalText> additionalTexts,
        ImmutableArray<MetadataReference> metadataReferences,
        out ImmutableArray<RazorSourceGeneratorDocumentOutput> documents,
        out string? failure)
    {
        documents = ImmutableArray<RazorSourceGeneratorDocumentOutput>.Empty;
        failure = null;

        if (compilation is null)
        {
            failure = "Compilation was null.";
            return false;
        }

        if (parseOptions is not CSharpParseOptions csharpParseOptions)
        {
            failure = "Razor SG fallback requires C# parse options.";
            return false;
        }

        if (analyzerOptions is null)
        {
            failure = "Analyzer config options were unavailable.";
            return false;
        }

        if (additionalTexts.IsDefaultOrEmpty)
            return true;

        if (!TryCreateRazorSourceGenerator(out var generator, out failure))
            return false;

        var driverCompilation = CloneCompilationWithReferences(compilation, metadataReferences);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: additionalTexts,
            parseOptions: csharpParseOptions,
            optionsProvider: analyzerOptions);

        driver = driver.RunGenerators(driverCompilation);
        var runResult = driver.GetRunResult();
        var firstFailure = runResult.Diagnostics
            .FirstOrDefault(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        if (firstFailure is not null)
        {
            failure = firstFailure.ToString();
            return false;
        }

        var builder = ImmutableArray.CreateBuilder<RazorSourceGeneratorDocumentOutput>();
        foreach (var result in runResult.Results)
        {
            if (!TryGetRazorGeneratorResult(result, out var razorGeneratorResult))
                continue;

            foreach (var additionalText in additionalTexts)
            {
                if (!TryReadRazorDocument(
                        razorGeneratorResult!,
                        additionalText.Path,
                        out var document,
                        out var documentFailure))
                {
                    if (!IsRazorFile(additionalText.Path))
                        continue;

                    failure = documentFailure;
                    return false;
                }

                builder.Add(document);
            }
        }

        documents = builder.ToImmutable();
        return true;
    }

    private static Compilation CloneCompilationWithReferences(
        Compilation compilation,
        ImmutableArray<MetadataReference> metadataReferences)
    {
        if (metadataReferences.IsDefaultOrEmpty)
            return compilation;

        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var reference in compilation.References)
            AddReferenceKey(existing, reference);

        var missingReferences = ImmutableArray.CreateBuilder<MetadataReference>();
        foreach (var reference in metadataReferences)
        {
            var key = GetReferenceKey(reference);
            if (string.IsNullOrWhiteSpace(key) || !existing.Add(key))
                continue;

            missingReferences.Add(reference);
        }

        return missingReferences.Count == 0
            ? compilation
            : compilation.AddReferences(missingReferences);
    }

    private static void AddReferenceKey(HashSet<string> keys, MetadataReference reference)
    {
        var key = GetReferenceKey(reference);
        if (!string.IsNullOrWhiteSpace(key))
            keys.Add(key);
    }

    private static string GetReferenceKey(MetadataReference reference)
        => reference switch
        {
            PortableExecutableReference peReference when !string.IsNullOrWhiteSpace(peReference.FilePath) => peReference.FilePath!,
            _ => reference.Display ?? string.Empty
        };

    private static bool TryCreateRazorSourceGenerator(
        out IIncrementalGenerator generator,
        out string? failure)
    {
        generator = default!;
        failure = null;

        var assembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(static item => string.Equals(
                item.GetName().Name,
                RazorCompilerAssemblyName,
                StringComparison.Ordinal));
        if (assembly is null)
        {
            failure = RazorCompilerAssemblyName + " is not loaded in the current analyzer process.";
            return false;
        }

        var type = assembly.GetType(RazorSourceGeneratorTypeName, throwOnError: false);
        if (type is null)
        {
            failure = RazorSourceGeneratorTypeName + " was not found.";
            return false;
        }

        if (!typeof(IIncrementalGenerator).IsAssignableFrom(type))
        {
            failure = RazorSourceGeneratorTypeName + " does not implement IIncrementalGenerator.";
            return false;
        }

        try
        {
            generator = (IIncrementalGenerator)Activator.CreateInstance(type)!;
            return true;
        }
        catch (Exception ex)
        {
            failure = "Could not create " + RazorSourceGeneratorTypeName + ": " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    private static bool TryGetRazorGeneratorResult(
        GeneratorRunResult generatorRunResult,
        out object? razorGeneratorResult)
    {
        razorGeneratorResult = null;
        var property = generatorRunResult.GetType().GetProperty("HostOutputs");
        if (property?.GetValue(generatorRunResult) is not IEnumerable entries)
            return false;

        foreach (var entry in entries)
        {
            if (entry is null)
                continue;

            var entryType = entry.GetType();
            var key = entryType.GetProperty("Key")?.GetValue(entry) as string;
            var value = entryType.GetProperty("Value")?.GetValue(entry);
            if (string.Equals(key, "RazorGeneratorResult", StringComparison.Ordinal) &&
                string.Equals(value?.GetType().FullName, RazorGeneratorResultTypeName, StringComparison.Ordinal))
            {
                razorGeneratorResult = value;
                return true;
            }
        }

        return false;
    }

    private static bool TryReadRazorDocument(
        object razorGeneratorResult,
        string documentPath,
        out RazorSourceGeneratorDocumentOutput document,
        out string? failure)
    {
        document = default;
        failure = null;

        var getCodeDocument = razorGeneratorResult.GetType().GetMethod(
            "GetCodeDocument",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(string)],
            modifiers: null);
        if (getCodeDocument is null)
        {
            failure = "RazorGeneratorResult.GetCodeDocument(string) was not found.";
            return false;
        }

        var codeDocument = getCodeDocument.Invoke(razorGeneratorResult, [documentPath]);
        if (codeDocument is null)
        {
            failure = "RazorGeneratorResult did not return a RazorCodeDocument for '" + documentPath + "'.";
            return false;
        }

        var getRequiredCSharpDocument = codeDocument.GetType().GetMethod(
            "GetRequiredCSharpDocument",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);
        if (getRequiredCSharpDocument is null)
        {
            failure = "RazorCodeDocument.GetRequiredCSharpDocument() was not found.";
            return false;
        }

        var csharpDocument = getRequiredCSharpDocument.Invoke(codeDocument, null);
        if (csharpDocument is null)
        {
            failure = "RazorCodeDocument.GetRequiredCSharpDocument() returned null for '" + documentPath + "'.";
            return false;
        }

        document = new RazorSourceGeneratorDocumentOutput(
            GetHintName(razorGeneratorResult, documentPath),
            codeDocument,
            csharpDocument);
        return true;
    }

    private static string GetHintName(object razorGeneratorResult, string documentPath)
    {
        var method = razorGeneratorResult.GetType().GetMethod(
            "GetHintName",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(string)],
            modifiers: null);
        return method?.Invoke(razorGeneratorResult, [documentPath]) as string
               ?? Path.GetFileNameWithoutExtension(documentPath) + "_razor.g.cs";
    }

    private static bool IsRazorFile(string path)
        => path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase);

    private static void ReportFailure(SourceProductionContext context, string failure)
        => context.ReportDiagnostic(Diagnostic.Create(
            RazorSourceGeneratorDiagnostics.RazorSgTailOutputFailed,
            Location.None,
            string.IsNullOrWhiteSpace(failure)
                ? "Fallback output failed with an unknown failure."
                : "Fallback output failed: " + failure));

    private static void EmitTraceIfEnabled(
        SourceProductionContext context,
        RazorSourceGeneratorTailOutputOptions options,
        int documentCount,
        string state,
        string? detail)
    {
        if (!options.TestHookEnabled)
            return;

        context.AddSource(
            "Jazor.RazorVue.RazorSgFallbackTrace.g.cs",
            BuildTraceSource(documentCount, state, detail));
    }

    private static string BuildTraceSource(int documentCount, string state, string? detail)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static class RazorSgFallbackTrace");
        builder.AppendLine("    {");
        builder.Append("        internal const int DocumentCount = ").Append(documentCount).AppendLine(";");
        builder.Append("        internal const string State = ").Append(EscapeCSharpString(state)).AppendLine(";");
        builder.Append("        internal const string Detail = ").Append(EscapeCSharpString(detail ?? string.Empty)).AppendLine(";");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string EscapeCSharpString(string value)
    {
        var builder = new System.Text.StringBuilder((value ?? string.Empty).Length + 2);
        builder.Append('"');
        foreach (var ch in value ?? string.Empty)
        {
            builder.Append(ch switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\0' => "\\0",
                '\a' => "\\a",
                '\b' => "\\b",
                '\f' => "\\f",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\v' => "\\v",
                _ => ch.ToString()
            });
        }

        builder.Append('"');
        return builder.ToString();
    }
}
