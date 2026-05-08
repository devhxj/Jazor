using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorHostOutputHookOptions
{
    private const string TestHookPropertyName = "build_property.JazorRazorVueTestHook";
    private const string EnableRazorSgIntegrationPropertyName = "build_property.JazorRazorVueEnableRazorSgIntegration";

    internal static bool IsTestHookEnabled(AnalyzerConfigOptionsProvider optionsProvider)
    {
        if (optionsProvider is null)
            throw new ArgumentNullException(nameof(optionsProvider));

        return optionsProvider.GlobalOptions.TryGetValue(TestHookPropertyName, out var value) &&
               bool.TryParse(value, out var parsed) &&
               parsed;
    }

    internal static bool IsRazorSgIntegrationEnabled(AnalyzerConfigOptionsProvider optionsProvider)
    {
        if (optionsProvider is null)
            throw new ArgumentNullException(nameof(optionsProvider));

        return optionsProvider.GlobalOptions.TryGetValue(EnableRazorSgIntegrationPropertyName, out var value) &&
               bool.TryParse(value, out var parsed) &&
               parsed;
    }

    internal static RazorSourceGeneratorTailOutputOptions CreateTailOutputOptions(AnalyzerConfigOptionsProvider optionsProvider)
    {
        if (optionsProvider is null)
            throw new ArgumentNullException(nameof(optionsProvider));

        return new RazorSourceGeneratorTailOutputOptions(
            IsRazorSgIntegrationEnabled(optionsProvider),
            IsTestHookEnabled(optionsProvider));
    }
}

internal sealed record RazorSourceGeneratorTailOutputOptions(
    bool Enabled,
    bool TestHookEnabled);
