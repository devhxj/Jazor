using System.Runtime.InteropServices;

namespace Jazor.NodeHost;

internal static class LinuxLibNodeLoader
{
    private const int Now = 0x2;
    private const int DeepBind = 0x8;
    private const int Global = 0x100;

    public static string Load()
    {
        // Use the native search paths resolved by the .NET host from the app's deps.json.
        var searchPaths = (string?)AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") ?? "";
        var path = searchPaths.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(directory => Path.Combine(directory, "libnode.so"))
            .FirstOrDefault(File.Exists)
            ?? throw new DllNotFoundException("Could not find the selected libnode.so in the .NET native asset paths.");

        // Addons need global napi_* symbols. Register system OpenSSL first for
        // .NET's TLS calls, and bind libnode's relocations to its bundled OpenSSL.
        // Keep both handles alive for the lifetime of the embedded platform.
        Open("libssl.so.3", Now | Global);
        Open(path, Now | DeepBind | Global);
        return path;
    }

    private static void Open(string path, int flags)
    {
        if (DlOpen(path, flags) == 0)
            throw new DllNotFoundException($"Cannot load '{path}': {Marshal.PtrToStringUTF8(DlError())}");
    }

    [DllImport("libdl.so.2", EntryPoint = "dlopen")]
    private static extern nint DlOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags);

    [DllImport("libdl.so.2", EntryPoint = "dlerror")]
    private static extern nint DlError();
}
