using System.Globalization;
using System.Text;
using Jazor.Analyzer.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Analysis;

// The generator owns Razor SG hook registration only. The tail output is the
// sole production route from official generated C# to future render artifacts.
[Generator]
public sealed class RazorVueGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        RazorSourceGeneratorBootstrap.Initialize();
        var registrationVersion = RazorSourceGeneratorBootstrapState.GetTailOutputRegistrationVersion();
        RazorSourceGeneratorFallbackOutput.Register(context);

        var contextKey = RazorSourceGeneratorInitializationContextState.GetContextKey(context);
        var testHookEnabled = context.AnalyzerConfigOptionsProvider.Select(
            static (optionsProvider, _) => RazorSourceGeneratorHostOutputHookOptions.IsTestHookEnabled(optionsProvider));
        var bootstrapTrace = context.CompilationProvider.Select(
            (_, _) => CreateBootstrapTrace(contextKey, registrationVersion));

        context.RegisterSourceOutput(
            testHookEnabled.Combine(bootstrapTrace),
            static (outputContext, input) =>
            {
                var (enabled, trace) = input;
                if (!enabled)
                    return;

                outputContext.AddSource(
                    "Jazor.RazorVue.RazorSgBootstrapTrace.g.cs",
                    BuildRazorSgBootstrapTraceSource(trace));
            });
    }

    private static RazorSourceGeneratorBootstrapTrace CreateBootstrapTrace(
        object? contextKey,
        int registrationVersion)
    {
        var trace = RazorSourceGeneratorBootstrapState.CreateTrace(contextKey);
        if (!RazorSourceGeneratorBootstrapState.HasTailOutputRegistrationAfter(registrationVersion))
            return trace;

        return trace with
        {
            TailOutputRegistered = true,
            TailOutputRegisteredForCurrentContext = true
        };
    }

    private static string BuildRazorSgBootstrapTraceSource(RazorSourceGeneratorBootstrapTrace trace)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static class RazorSgBootstrapTrace");
        builder.AppendLine("    {");
        builder.Append("        internal const bool HasAttempted = ").Append(ToCSharpBool(trace.HasAttempted)).AppendLine(";");
        builder.Append("        internal const bool IsInstalled = ").Append(ToCSharpBool(trace.IsInstalled)).AppendLine(";");
        builder.Append("        internal const bool RazorAssemblyObserved = ").Append(ToCSharpBool(trace.RazorAssemblyObserved)).AppendLine(";");
        builder.Append("        internal const bool PatchAttempted = ").Append(ToCSharpBool(trace.PatchAttempted)).AppendLine(";");
        builder.Append("        internal const bool GeneratorTypeFound = ").Append(ToCSharpBool(trace.GeneratorTypeFound)).AppendLine(";");
        builder.Append("        internal const bool InitializeMethodFound = ").Append(ToCSharpBool(trace.InitializeMethodFound)).AppendLine(";");
        builder.Append("        internal const bool PostfixMethodFound = ").Append(ToCSharpBool(trace.PostfixMethodFound)).AppendLine(";");
        builder.Append("        internal const bool PatchSucceeded = ").Append(ToCSharpBool(trace.PatchSucceeded)).AppendLine(";");
        builder.Append("        internal const bool PatchFailed = ").Append(ToCSharpBool(trace.PatchFailed)).AppendLine(";");
        builder.Append("        internal const bool PatchUnavailable = ").Append(ToCSharpBool(trace.PatchUnavailable)).AppendLine(";");
        builder.Append("        internal const string RazorSourceGeneratorAssemblyVersion = ").Append(EscapeCSharpString(trace.RazorSourceGeneratorAssemblyVersion)).AppendLine(";");
        builder.Append("        internal const string RazorSourceGeneratorModuleVersionId = ").Append(EscapeCSharpString(trace.RazorSourceGeneratorModuleVersionId)).AppendLine(";");
        builder.Append("        internal const int RazorSourceGeneratorInitializeMethodIlLength = ").Append(trace.RazorSourceGeneratorInitializeMethodIlLength.ToString(CultureInfo.InvariantCulture)).AppendLine(";");
        builder.Append("        internal const string RazorSourceGeneratorInitializeMethodIlSha256 = ").Append(EscapeCSharpString(trace.RazorSourceGeneratorInitializeMethodIlSha256)).AppendLine(";");
        builder.Append("        internal const bool PostfixInvoked = ").Append(ToCSharpBool(trace.PostfixInvoked)).AppendLine(";");
        builder.Append("        internal const bool ImplementationSourceOutputHookInstalled = ").Append(ToCSharpBool(trace.ImplementationSourceOutputHookInstalled)).AppendLine(";");
        builder.Append("        internal const bool ImplementationSourceOutputObserved = ").Append(ToCSharpBool(trace.ImplementationSourceOutputObserved)).AppendLine(";");
        builder.Append("        internal const bool TailOutputRegistered = ").Append(ToCSharpBool(trace.TailOutputRegistered)).AppendLine(";");
        builder.Append("        internal const bool CurrentContextKeyAvailable = ").Append(ToCSharpBool(trace.CurrentContextKeyAvailable)).AppendLine(";");
        builder.Append("        internal const bool TailOutputRegisteredForCurrentContext = ").Append(ToCSharpBool(trace.TailOutputRegisteredForCurrentContext)).AppendLine(";");
        builder.Append("        internal const string TailOutputRegistrationKind = ").Append(EscapeCSharpString(trace.TailOutputRegistrationKind)).AppendLine(";");
        builder.Append("        internal const bool TestHookObserved = ").Append(ToCSharpBool(trace.TestHookObserved)).AppendLine(";");
        builder.Append("        internal const string? Failure = ").Append(EscapeNullableCSharpString(trace.Failure)).AppendLine(";");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string ToCSharpBool(bool value)
        => value ? "true" : "false";

    private static string EscapeNullableCSharpString(string? value)
        => value is null ? "null" : EscapeCSharpString(value);

    private static string EscapeCSharpString(string value)
    {
        var builder = new StringBuilder((value ?? string.Empty).Length + 2);
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
