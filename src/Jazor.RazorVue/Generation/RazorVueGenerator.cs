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
