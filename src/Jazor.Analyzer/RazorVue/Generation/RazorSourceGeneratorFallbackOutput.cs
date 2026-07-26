using System.Collections.Immutable;
using Jazor.RazorVue.Analysis;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorFallbackOutput
{
    internal const string TailOutputRequiredFailureMarker = "official Razor SG tail output is required";
    internal const string FallbackRequiredFailureMarker = TailOutputRequiredFailureMarker;

    internal static void Register(IncrementalGeneratorInitializationContext context)
    {
        var contextKey = RazorSourceGeneratorInitializationContextState.GetContextKey(context);
        var tailOutputRegistrationVersionBeforeInitialize = RazorSourceGeneratorBootstrapState.GetTailOutputRegistrationVersion();
        var options = context.AnalyzerConfigOptionsProvider
            .Select(static (optionsProvider, _) => RazorSourceGeneratorHostOutputHookOptions.CreateTailOutputOptions(optionsProvider));
        var input = context.CompilationProvider.Combine(options);

        context.RegisterSourceOutput(input, (outputContext, source) =>
        {
            var (compilation, tailOptions) = source;
            Emit(
                outputContext,
                compilation,
                tailOptions,
                contextKey,
                tailOutputRegistrationVersionBeforeInitialize);
        });
    }

    internal static bool IsFallbackRequiredFailure(string? failure)
    {
        if (string.IsNullOrWhiteSpace(failure))
            return false;

        return failure!.IndexOf(TailOutputRequiredFailureMarker, StringComparison.Ordinal) >= 0 ||
               failure.IndexOf("fallback Razor SG host-output generation is required", StringComparison.Ordinal) >= 0;
    }

    private static void Emit(
        SourceProductionContext context,
        Compilation compilation,
        RazorSourceGeneratorTailOutputOptions options,
        object? contextKey,
        int tailOutputRegistrationVersionBeforeInitialize)
    {
        if (!options.Enabled)
            return;

        if (!RequiresTailOutput(compilation, out var componentSummary))
        {
            EmitTraceIfEnabled(context, options, "not-required", string.Empty);
            return;
        }

        var trace = RazorSourceGeneratorBootstrapState.CreateTrace(
            contextKey,
            tailOutputRegistrationVersionBeforeInitialize);
        if (trace.PatchFailed || trace.PatchUnavailable)
        {
            EmitTraceIfEnabled(context, options, "bootstrap-unavailable", trace.Failure ?? string.Empty);
            return;
        }

        if (trace.TailOutputRegisteredForCurrentContext)
        {
            EmitTraceIfEnabled(context, options, "tail-registered", string.Empty);
            return;
        }

        var detail = "RazorVue does not run a private Razor source generator fallback inside the analyzer. " +
                     "RazorVue .razor component processing must be triggered after the official Razor source generator has produced final generated C# through the Razor SG tail output route. " +
                     "Components requiring official Razor SG tail output: " + componentSummary + ".";
        context.ReportDiagnostic(Diagnostic.Create(
            RazorSourceGeneratorDiagnostics.RazorSgTailOutputFailed,
            Location.None,
            detail));
        EmitTraceIfEnabled(context, options, "forbidden", detail);
    }

    private static bool RequiresTailOutput(Compilation compilation, out string componentSummary)
    {
        componentSummary = string.Empty;
        var requiredComponents = RazorSgComponentCandidateSelector
            .DiscoverTailRequiredComponents(compilation)
            .Select(static component => component.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat))
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToImmutableArray();
        if (requiredComponents.IsDefaultOrEmpty)
            return false;

        componentSummary = string.Join(", ", requiredComponents);
        return true;
    }

    private static void EmitTraceIfEnabled(
        SourceProductionContext context,
        RazorSourceGeneratorTailOutputOptions options,
        string state,
        string detail)
    {
        if (!options.TestHookEnabled)
            return;

        context.AddSource(
            "Jazor.RazorVue.RazorSgFallbackTrace.g.cs",
            BuildTraceSource(state, detail));
    }

    private static string BuildTraceSource(string state, string detail)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static class RazorSgFallbackTrace");
        builder.AppendLine("    {");
        builder.AppendLine("        internal const int DocumentCount = 0;");
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
