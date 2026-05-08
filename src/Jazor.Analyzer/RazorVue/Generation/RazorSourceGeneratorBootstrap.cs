namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorBootstrap
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        RazorSourceGeneratorBootstrapState.MarkAttempted();
        _ = RazorSourceGeneratorInitializeHookInstaller.TryInstall();
    }

    internal static bool HasAttemptedBootstrap()
        => RazorSourceGeneratorBootstrapState.HasAttempted();
}
