using System.Collections;
using System.Collections.Immutable;
using Jazor.RazorVue.Analysis;
using Jazor.RazorVue;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorTailOutput
{
    private const string RazorCodeDocumentTypeName = "Microsoft.AspNetCore.Razor.Language.RazorCodeDocument";
    private const string RazorCSharpDocumentTypeName = "Microsoft.AspNetCore.Razor.Language.RazorCSharpDocument";
    internal static void Emit(SourceProductionContext context, Compilation compilation, object source)
        => Emit(
            context,
            compilation,
            source,
            new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: false));

    internal static void Emit(
        SourceProductionContext context,
        Compilation compilation,
        object source,
        RazorSourceGeneratorTailOutputOptions options)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        var requiresTailOutput = CompilationRequiresRazorVueTailOutput(compilation);
        if (!TryReadInput(source, out var documents, out var suppressed))
        {
            ReportTailFailureIfRequired(
                context,
                requiresTailOutput,
                "RazorVue could not read the Razor SG tail output input.");
            EmitTraceIfEnabled(context, options, 0, 0, 0, "input-unreadable");
            return;
        }

        EmitCore(context, compilation, documents, suppressed, options, requiresTailOutput);
    }

    internal static void Emit<TDocument>(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<TDocument> source,
        RazorSourceGeneratorTailOutputOptions options)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        var requiresTailOutput = CompilationRequiresRazorVueTailOutput(compilation);
        if (source.IsDefaultOrEmpty)
        {
            ReportTailFailureIfRequired(
                context,
                requiresTailOutput,
                "RazorVue tail output did not receive any Razor source generator documents.");
            EmitTraceIfEnabled(
                context,
                options,
                hostDocumentCount: 0,
                generatorDocumentCount: 0,
                artifactCount: 0,
                state: "no-generator-documents",
                sourceItemCount: source.IsDefault ? -1 : 0,
                sourceTypeName: typeof(TDocument).FullName ?? typeof(TDocument).Name);
            return;
        }

        var documents = ImmutableArray.CreateBuilder<ReflectedRazorSourceGeneratorDocument>();
        var suppressedCount = 0;
        var firstItemFieldNames = string.Empty;
        var firstNestedFieldNames = string.Empty;
        var firstNestedValueTypes = string.Empty;
        foreach (var item in source)
        {
            if (item is null)
                continue;

            if (string.IsNullOrEmpty(firstItemFieldNames))
            {
                firstItemFieldNames = GetFieldNameSummary(item);
                if (TryGetTupleItem(item, "Item1", out var nestedValue) && nestedValue is not null)
                {
                    firstNestedFieldNames = GetFieldNameSummary(nestedValue);
                    firstNestedValueTypes = GetTupleValueTypeSummary(nestedValue);
                }
            }

            if (TryReadDocumentTuple(item, out var document))
            {
                documents.Add(document);
                continue;
            }

            if (!TryReadSuppressedDocumentTuple(item, out document, out var suppressed))
                continue;

            if (suppressed)
            {
                suppressedCount++;
                continue;
            }

            documents.Add(document);
        }

        var collectedDocuments = documents.ToImmutable();
        if (collectedDocuments.IsDefaultOrEmpty)
        {
            ReportTailFailureIfRequired(
                context,
                requiresTailOutput,
                suppressedCount > 0
                    ? "RazorVue tail output received only suppressed Razor source generator documents."
                    : "RazorVue tail output did not receive any readable Razor source generator documents.");
            EmitTraceIfEnabled(
                context,
                options,
                hostDocumentCount: suppressedCount,
                generatorDocumentCount: 0,
                artifactCount: 0,
                state: suppressedCount > 0 ? "suppressed" : "no-generator-documents",
                sourceItemCount: source.Length,
                sourceTypeName: (typeof(TDocument).FullName ?? typeof(TDocument).Name)
                                + "|fields=" + firstItemFieldNames
                                + "|nested=" + firstNestedFieldNames
                                + "|values=" + firstNestedValueTypes);
            return;
        }

        EmitCore(
            context,
            compilation,
            collectedDocuments,
            suppressed: false,
            options,
            requiresTailOutput);
    }

    internal static void EmitDocuments(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<RazorSourceGeneratorDocumentOutput> documents,
        RazorSourceGeneratorTailOutputOptions options)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        var convertedDocuments = documents.IsDefaultOrEmpty
            ? ImmutableArray<ReflectedRazorSourceGeneratorDocument>.Empty
            : documents
                .Select(static item => new ReflectedRazorSourceGeneratorDocument(
                    item.HintName,
                    item.CodeDocument,
                    item.CSharpDocument))
                .ToImmutableArray();
        EmitCore(
            context,
            compilation,
            convertedDocuments,
            suppressed: false,
            options,
            CompilationRequiresRazorVueTailOutput(compilation));
    }

    private static void EmitCore(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<ReflectedRazorSourceGeneratorDocument> documents,
        bool suppressed,
        RazorSourceGeneratorTailOutputOptions options,
        bool requiresTailOutput)
    {
        if (suppressed)
        {
            ReportTailFailureIfRequired(
                context,
                requiresTailOutput,
                "RazorVue tail output received a suppressed Razor source generator document set.");
            EmitTraceIfEnabled(context, options, documents.Length, 0, 0, "suppressed");
            return;
        }

        if (documents.IsDefaultOrEmpty)
        {
            ReportTailFailureIfRequired(
                context,
                requiresTailOutput,
                "RazorVue tail output did not receive any Razor source generator documents.");
            EmitTraceIfEnabled(context, options, 0, 0, 0, "no-generator-documents");
            return;
        }

        if (!requiresTailOutput)
        {
            EmitTraceIfEnabled(
                context,
                options,
                documents.Length,
                0,
                0,
                "no-razorvue-candidates");
            return;
        }

        var bridgeInputs = documents
            .Select(static item => new RazorVueRazorSourceGeneratorDocumentInput(
                item.HintName,
                item.CodeDocument,
                item.CSharpDocument))
            .ToImmutableArray();
        RazorVueRazorSourceGeneratorTailBridgeResult result;
        try
        {
            result = RazorVueRazorSourceGeneratorTailBridge.ExecuteSfcPipeline(compilation, bridgeInputs);
        }
        catch (Exception ex)
        {
            ReportTailFailure(context, ex.GetType().Name + ": " + ex.Message);
            EmitTraceIfEnabled(
                context,
                options,
                documents.Length,
                0,
                0,
                "bridge-exception",
                sourceTypeName: ex.GetType().FullName + ": " + ex.Message);
            return;
        }

        if (!result.Success)
        {
            ReportTailFailure(context, result.Failure ?? "Unknown RazorVue Razor SG tail bridge failure.");
            EmitTraceIfEnabled(
                context,
                options,
                documents.Length,
                result.GeneratorDocumentCount,
                0,
                "bridge-failed",
                sourceTypeName: result.Failure);
            return;
        }

        var catalog = result.Catalog;
        if (catalog.Artifacts.IsDefaultOrEmpty)
        {
            EmitTraceIfEnabled(
                context,
                options,
                documents.Length,
                result.GeneratorDocumentCount,
                0,
                "no-artifacts");
            return;
        }

        foreach (var artifact in catalog.Artifacts)
        {
            context.AddSource(
                RazorVueGenerator.CreateRazorVueSfcArtifactHintName(artifact),
                RazorVueGenerator.BuildRazorVueSfcArtifactSource(artifact));
        }

        context.AddSource(
            "Jazor.Generated.RazorVueCatalog.g.cs",
            RazorVueGenerator.BuildRazorVueSfcCatalogSource(catalog));
        EmitTraceIfEnabled(
            context,
            options,
            documents.Length,
            result.GeneratorDocumentCount,
            catalog.Artifacts.Length,
            "emitted");
    }

    private static bool CompilationRequiresRazorVueTailOutput(Compilation compilation)
    {
        var razorVueContext = RazorVueCompilationContext.TryCreate(compilation);
        return razorVueContext is not null &&
               !razorVueContext.DiscoverComponentCandidates().IsDefaultOrEmpty;
    }

    private static void ReportTailFailureIfRequired(
        SourceProductionContext context,
        bool required,
        string failure)
    {
        if (required)
            ReportTailFailure(context, failure);
    }

    private static void ReportTailFailure(SourceProductionContext context, string failure)
        => context.ReportDiagnostic(Diagnostic.Create(
            RazorSourceGeneratorDiagnostics.RazorSgTailOutputFailed,
            Location.None,
            string.IsNullOrWhiteSpace(failure) ? "Unknown failure." : failure));

    private static void EmitTraceIfEnabled(
        SourceProductionContext context,
        RazorSourceGeneratorTailOutputOptions options,
        int hostDocumentCount,
        int generatorDocumentCount,
        int artifactCount,
        string state,
        int? sourceItemCount = null,
        string? sourceTypeName = null)
    {
        if (!options.TestHookEnabled)
            return;

        context.AddSource(
            "Jazor.RazorVue.RazorSgTailTrace.g.cs",
            BuildTraceSource(hostDocumentCount, generatorDocumentCount, artifactCount, state, sourceItemCount, sourceTypeName));
    }

    private static string BuildTraceSource(
        int hostDocumentCount,
        int generatorDocumentCount,
        int artifactCount,
        string state,
        int? sourceItemCount,
        string? sourceTypeName)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static class RazorSgTailTrace");
        builder.AppendLine("    {");
        builder.Append("        internal const int HostDocumentCount = ").Append(hostDocumentCount).AppendLine(";");
        builder.Append("        internal const int GeneratorDocumentCount = ").Append(generatorDocumentCount).AppendLine(";");
        builder.Append("        internal const int ArtifactCount = ").Append(artifactCount).AppendLine(";");
        builder.Append("        internal const string State = ").Append(EscapeCSharpString(state)).AppendLine(";");
        builder.Append("        internal const int SourceItemCount = ").Append(sourceItemCount ?? -2).AppendLine(";");
        builder.Append("        internal const string SourceTypeName = ").Append(EscapeCSharpString(sourceTypeName ?? string.Empty)).AppendLine(";");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static bool TryReadInput(
        object source,
        out ImmutableArray<ReflectedRazorSourceGeneratorDocument> documents,
        out bool suppressed)
    {
        documents = ImmutableArray<ReflectedRazorSourceGeneratorDocument>.Empty;
        suppressed = false;

        if (!TryGetTupleItem(source, "Item1", out var outerLeft) ||
            !TryGetTupleItem(source, "Item2", out var outerRight))
        {
            return false;
        }

        suppressed = outerRight is bool value && value;
        if (!TryGetTupleItem(outerLeft!, "Item1", out var documentsValue))
            return false;

        if (documentsValue is not IEnumerable entries)
            return false;

        var builder = ImmutableArray.CreateBuilder<ReflectedRazorSourceGeneratorDocument>();
        foreach (var entry in entries)
        {
            if (entry is not null && TryReadDocumentTuple(entry, out var document))
            {
                builder.Add(document);
            }
        }

        documents = builder.ToImmutable();
        return true;
    }

    private static bool TryReadDocumentTuple(
        object value,
        out ReflectedRazorSourceGeneratorDocument document)
    {
        document = default;
        if (!TryGetTupleItem(value, "Item1", out var hintNameValue) ||
            !TryGetTupleItem(value, "Item2", out var codeDocumentValue) ||
            !TryGetTupleItem(value, "Item3", out var csharpDocumentValue))
        {
            return false;
        }

        if (hintNameValue is not string hintName ||
            !HasFullName(codeDocumentValue, RazorCodeDocumentTypeName) ||
            !HasFullName(csharpDocumentValue, RazorCSharpDocumentTypeName))
        {
            return false;
        }

        document = new ReflectedRazorSourceGeneratorDocument(
            hintName,
            codeDocumentValue!,
            csharpDocumentValue!);
        return true;
    }

    private static bool TryReadSuppressedDocumentTuple(
        object value,
        out ReflectedRazorSourceGeneratorDocument document,
        out bool suppressed)
    {
        document = default;
        suppressed = false;
        if (!TryGetTupleItem(value, "Item1", out var documentValue) ||
            !TryGetTupleItem(value, "Item2", out var suppressedValue))
        {
            return false;
        }

        suppressed = suppressedValue is bool boolValue && boolValue;
        return documentValue is not null &&
               TryReadDocumentTuple(documentValue, out document);
    }

    private static bool TryGetTupleItem(object value, string name, out object? item)
    {
        item = null;
        var hasOrdinal = TryGetTupleItemOrdinal(name, out var ordinal);

        var field = value.GetType().GetField(
            name,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);
        if (field is null && hasOrdinal)
        {
            field = value.GetType()
                .GetFields(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic)
                .FirstOrDefault(candidate => string.Equals(candidate.Name, "Item" + ordinal, StringComparison.Ordinal));
        }

        if (field is null)
            return false;

        item = field.GetValue(value);
        return true;
    }

    private static bool TryGetTupleItemOrdinal(string name, out int ordinal)
    {
        ordinal = 0;
        if (!name.StartsWith("Item", StringComparison.Ordinal))
            return false;

        return int.TryParse(name.Substring("Item".Length), out ordinal) && ordinal > 0;
    }

    private static bool HasFullName(object? value, string fullName)
        => string.Equals(value?.GetType().FullName, fullName, StringComparison.Ordinal);

    private static string GetFieldNameSummary(object value)
        => string.Join(
            ",",
            value.GetType()
                .GetFields(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic)
                .Select(static field => field.Name));

    private static string GetTupleValueTypeSummary(object value)
    {
        var parts = new List<string>();
        for (var i = 1; i <= 3; i++)
        {
            if (!TryGetTupleItem(value, "Item" + i, out var item))
                continue;

            var type = item?.GetType();
            parts.Add(
                "Item" + i + "=" +
                (type?.FullName ?? "<null>") +
                "|asm=" +
                (type?.Assembly.GetName().Name ?? "<null>") +
                "|path=" +
                (type?.Assembly.Location ?? "<null>"));
        }

        return string.Join(";", parts);
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

    private readonly record struct ReflectedRazorSourceGeneratorDocument(
        string HintName,
        object CodeDocument,
        object CSharpDocument);
}
