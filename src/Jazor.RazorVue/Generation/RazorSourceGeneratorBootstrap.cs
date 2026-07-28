namespace Jazor.RazorVue.Generation;

internal static class RazorSourceGeneratorBootstrap
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        _ = RazorSourceGeneratorInitializeHookInstaller.TryInstall();
    }
}
