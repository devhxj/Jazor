using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Generation;

// The analyzer assembly installs the driver-completion hook. RazorVue must
// consume the completed Compilation, because incremental generators cannot see
// source emitted by another generator in the same driver pass.
[Generator]
public sealed class RazorVueGenerator : IIncrementalGenerator
{
    public RazorVueGenerator()
    {
        // GeneratorDriver invokes Initialize from inside RunGeneratorsAndUpdateCompilation.
        // Revalidate here so a tiered-JIT replacement is repaired before that driver call.
        RazorSourceGeneratorBootstrap.Initialize();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        RazorSourceGeneratorBootstrap.Initialize();
        var failure = RazorSourceGeneratorInitializeHookInstaller.GetInstallFailure();
        if (string.IsNullOrEmpty(failure))
            return;

        context.RegisterSourceOutput(
            context.CompilationProvider,
            (output, _) => output.ReportDiagnostic(Diagnostic.Create(
                RazorSourceGeneratorDiagnostics.RazorSgTailOutputFailed,
                Location.None,
                failure)));
    }
}
