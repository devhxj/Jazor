namespace Jazor.RazorVue.Generation;

/// <summary>
/// Installs the final-compilation hook once when the generator assembly is loaded.
/// 它是生成器装载期的最小 bootstrap，不参与组件扫描或 Vue artifact 生成。
/// </summary>
internal static class Bootstrap
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        _ = InitializeHookInstaller.TryInstall();
    }
}
