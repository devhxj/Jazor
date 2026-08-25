using System.Runtime.InteropServices;

namespace Jazor.RazorVue.Sg.Test;

internal static class RazorSgDenoRuntime
{
    public static string ResolveExecutable()
    {
        // Runtime tests own their native Deno asset. Do not depend on another project's
        // incidental bin output, which is absent in a clean or isolated CI build.
        var runtimeIdentifier = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "win-x64"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "linux-x64"
                : RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                    ? "osx-arm64"
                    : "osx-x64";
        var executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "deno.exe" : "deno";
        var path = Path.Combine(AppContext.BaseDirectory, "runtimes", runtimeIdentifier, "native", executableName);
        return File.Exists(path)
            ? path
            : throw new FileNotFoundException("Bundled Deno runtime was not found in the Razor SG test output.", path);
    }
}
