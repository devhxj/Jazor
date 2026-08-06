namespace Jazor.RazorVue.Generation;

/// <summary>Installs the final-compilation hook once when the generator assembly is loaded.</summary>
internal static class Bootstrap
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        _ = InitializeHookInstaller.TryInstall();
    }
}
