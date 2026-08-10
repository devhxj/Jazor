using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Generation;

/// <summary>
/// Registers RazorVue generation against the completed compilation exposed by the analyzer hook.
/// Incremental generators cannot otherwise observe source produced by another generator in the same pass.
/// </summary>
[Generator]
public sealed class RazorVueGenerator : IIncrementalGenerator
{
    public RazorVueGenerator()
    {
        // GeneratorDriver invokes Initialize from inside RunGeneratorsAndUpdateCompilation.
        // Revalidate here so a tiered-JIT replacement is repaired before that driver call.
        Bootstrap.Initialize();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Bootstrap.Initialize();

        var razorSources = context.AdditionalTextsProvider
            .Select(static (source, cancellationToken) => RazorSourceTextRegistry.TryCreate(source, cancellationToken))
            .Where(static source => source is not null)
            .Select(static (source, _) => source!.Value)
            .Collect();
        context.RegisterImplementationSourceOutput(
            razorSources,
            static (output, sources) =>
            {
                if (sources.IsDefaultOrEmpty)
                    return;

                output.AddSource(
                    RazorSourceTextRegistry.CarrierHintName,
                    Microsoft.CodeAnalysis.Text.SourceText.From(
                        RazorSourceTextRegistry.BuildCarrierSource(sources),
                        System.Text.Encoding.UTF8));
            });

        var failure = InitializeHookInstaller.GetInstallFailure();
        if (string.IsNullOrEmpty(failure))
            return;

        context.RegisterSourceOutput(
            context.CompilationProvider,
            (output, _) => output.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.TailOutputFailed,
                Location.None,
                failure)));
    }
}
