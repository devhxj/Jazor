namespace Jazor.RazorVue.Generator.Generation;

internal static class RazorSourceGeneratorBootstrap
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        _ = RazorSourceGeneratorInitializeHookInstaller.TryInstall();
    }
}
