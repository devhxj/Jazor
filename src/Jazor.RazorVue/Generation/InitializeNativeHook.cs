using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Jazor.RazorVue.Generation;

/// <summary>
/// Applies and validates the architecture-specific jump used by the initialization hook.
/// 该对象封装进程内原生代码 patch 的安装与回滚，隔离于正常 RazorVue lowering 生命周期之外。
/// </summary>
internal sealed class InitializeNativeHook : IDisposable
{
    private const int X64JumpLength = 12;
    private const int Arm64JumpLength = 16;
    private const uint WindowsPageExecuteReadWrite = 0x40;
    private const int UnixProtRead = 0x1;
    private const int UnixProtWrite = 0x2;
    private const int UnixProtExecute = 0x4;

    private readonly MethodInfo _target;
    private readonly IntPtr _patchAddress;
    private readonly int _patchLength;
    private readonly byte[] _original;
    private readonly byte[] _jump;
    private bool _applied;
    private static int _selfTestReplacementCount;

    private InitializeNativeHook(MethodInfo target, MethodInfo replacement)
    {
        if (!IsCurrentPlatformSupported(out var unsupportedReason))
            throw new PlatformNotSupportedException(unsupportedReason);

        if (target is null)
            throw new ArgumentNullException(nameof(target));
        if (replacement is null)
            throw new ArgumentNullException(nameof(replacement));

        _target = target;
        RuntimeHelpers.PrepareMethod(target.MethodHandle);
        RuntimeHelpers.PrepareMethod(replacement.MethodHandle);

        var targetPointer = target.MethodHandle.GetFunctionPointer();
        var replacementPointer = replacement.MethodHandle.GetFunctionPointer();
        _jump = BuildJump(replacementPointer);
        _patchLength = _jump.Length;
        _patchAddress = ResolvePatchAddress(targetPointer, _patchLength);
        _original = ReadBytes(_patchAddress, _patchLength);
    }

    public static InitializeNativeHook Install(MethodInfo target, MethodInfo replacement)
    {
        var hook = new InitializeNativeHook(target, replacement);
        hook.Apply();
        return hook;
    }

    internal static bool IsCurrentPlatformSupported(out string unsupportedReason)
        => IsSupportedPlatform(
            GetCurrentOperatingSystem(),
            RuntimeInformation.ProcessArchitecture,
            out unsupportedReason);

    internal static bool IsSupportedPlatform(
        NativePlatform operatingSystem,
        Architecture architecture,
        out string unsupportedReason)
    {
        if (architecture is not Architecture.X64 and not Architecture.Arm64)
        {
            unsupportedReason = CreateUnsupportedPlatformReason(
                operatingSystem,
                architecture,
                "RazorVue Razor SG hook supports x64 and arm64 processes only.");
            return false;
        }

        if (operatingSystem is not NativePlatform.Windows and
            not NativePlatform.Linux and
            not NativePlatform.MacOS)
        {
            unsupportedReason = CreateUnsupportedPlatformReason(
                operatingSystem,
                architecture,
                "RazorVue Razor SG hook supports Windows, Linux, and macOS only.");
            return false;
        }

        if (operatingSystem == NativePlatform.Linux &&
            architecture == Architecture.Arm64)
        {
            unsupportedReason = CreateUnsupportedPlatformReason(
                operatingSystem,
                architecture,
                "RazorVue Razor SG hook does not enable Linux arm64 yet because the analyzer cannot reliably flush the instruction cache on that platform without an additional validated runtime shim.");
            return false;
        }

        unsupportedReason = string.Empty;
        return true;
    }

    internal static bool TryValidateCurrentPlatform(out string failure)
    {
        if (!IsCurrentPlatformSupported(out failure))
            return false;

        try
        {
            // Verify both patch and restoration on the live runtime before touching Roslyn's
            // GeneratorDriver. Unsupported runtimes fail as a normal RazorVue diagnostic.
            // 在 patch Roslyn 前先做本地自检，避免平台指令差异导致 generator host 不可恢复。
            Interlocked.Exchange(ref _selfTestReplacementCount, 0);
            var target = typeof(InitializeNativeHook).GetMethod(
                nameof(SelfTestTarget),
                BindingFlags.Static | BindingFlags.NonPublic);
            var replacement = typeof(InitializeNativeHook).GetMethod(
                nameof(SelfTestReplacement),
                BindingFlags.Static | BindingFlags.NonPublic);
            if (target is null || replacement is null)
            {
                failure = "RazorVue Razor SG hook backend self-test methods could not be resolved.";
                return false;
            }

            using (Install(target, replacement))
            {
                var patchedResult = SelfTestTarget(41);
                if (patchedResult != 43 || Volatile.Read(ref _selfTestReplacementCount) != 1)
                {
                    failure = "RazorVue Razor SG hook backend self-test failed after patching. " +
                              "Expected replacement result 43 and one replacement invocation, got result " +
                              patchedResult.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                              " and " +
                              Volatile.Read(ref _selfTestReplacementCount).ToString(System.Globalization.CultureInfo.InvariantCulture) +
                              " replacement invocations.";
                    return false;
                }
            }

            var restoredResult = SelfTestTarget(41);
            if (restoredResult != 42)
            {
                failure = "RazorVue Razor SG hook backend self-test failed after unpatching. Expected original result 42, got " +
                          restoredResult.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                          ".";
                return false;
            }

            failure = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            failure = "RazorVue Razor SG hook backend self-test failed on " +
                      DescribeCurrentPlatform() +
                      ": " +
                      ex.GetType().FullName +
                      ": " +
                      ex.Message;
            return false;
        }
    }

    public void Dispose()
        => Unapply();

    // Tiered JIT can replace the native body after the initial install. Keep the
    // process-wide driver hook bound to the method's current executable entry.
    internal bool IsCurrentTargetPatched()
    {
        RuntimeHelpers.PrepareMethod(_target.MethodHandle);
        var currentPatchAddress = ResolvePatchAddress(_target.MethodHandle.GetFunctionPointer(), _patchLength);
        return currentPatchAddress == _patchAddress &&
               ReadBytes(currentPatchAddress, _patchLength).AsSpan().SequenceEqual(_jump);
    }

    private void Apply()
    {
        if (_applied)
            return;

        // The original bytes were captured at construction and are restored by Dispose. The
        // installer serializes ownership; this type intentionally has no independent global lock.
        // 原始指令与跳转长度成对保存，进程级同步由 installer 负责。
        WriteBytes(_patchAddress, _jump);
        _applied = true;
    }

    private void Unapply()
    {
        if (!_applied)
            return;

        WriteBytes(_patchAddress, _original);
        _applied = false;
    }

    private static NativePlatform GetCurrentOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return NativePlatform.Windows;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return NativePlatform.Linux;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return NativePlatform.MacOS;

        return NativePlatform.Other;
    }

    private static IntPtr ResolvePatchAddress(IntPtr targetPointer, int patchLength)
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            var entryBytes = ReadBytes(targetPointer, Math.Max(patchLength, 16));
            if (entryBytes.Length >= 14 &&
                entryBytes[0] == 0xFF &&
                entryBytes[1] == 0x25)
            {
                var relative = BitConverter.ToInt32(entryBytes, 2);
                var cellAddress = IntPtr.Add(targetPointer, 6 + relative);
                var nativeCodeAddress = ReadIntPtr(cellAddress);
                if (nativeCodeAddress != IntPtr.Zero)
                    return nativeCodeAddress;
            }
        }

        return targetPointer;
    }

    private static byte[] BuildJump(IntPtr destination)
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        if (architecture == Architecture.X64)
        {
            var bytes = new byte[X64JumpLength];
            bytes[0] = 0x48;
            bytes[1] = 0xB8;
            BitConverter.GetBytes(destination.ToInt64()).CopyTo(bytes, 2);
            bytes[10] = 0xFF;
            bytes[11] = 0xE0;
            return bytes;
        }

        if (architecture == Architecture.Arm64)
        {
            var bytes = new byte[Arm64JumpLength];
            BitConverter.GetBytes(0x58000050u).CopyTo(bytes, 0); // ldr x16, #8
            BitConverter.GetBytes(0xD61F0200u).CopyTo(bytes, 4); // br x16
            BitConverter.GetBytes(destination.ToInt64()).CopyTo(bytes, 8);
            return bytes;
        }

        throw new PlatformNotSupportedException("Unsupported architecture: " + architecture + ".");
    }

    private static byte[] ReadBytes(IntPtr address, int length)
    {
        var bytes = new byte[length];
        Marshal.Copy(address, bytes, 0, length);
        return bytes;
    }

    private static IntPtr ReadIntPtr(IntPtr address)
    {
        var bytes = ReadBytes(address, IntPtr.Size);
        return IntPtr.Size == 8
            ? new IntPtr(BitConverter.ToInt64(bytes, 0))
            : new IntPtr(BitConverter.ToInt32(bytes, 0));
    }

    private static void WriteBytes(IntPtr address, byte[] bytes)
    {
        if (bytes.Length == 0)
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WriteBytesWindows(address, bytes);
            return;
        }

        WriteBytesUnix(address, bytes);
    }

    private static void WriteBytesWindows(IntPtr address, byte[] bytes)
    {
        if (!VirtualProtect(address, (UIntPtr)bytes.Length, WindowsPageExecuteReadWrite, out var oldProtect))
        {
            throw new InvalidOperationException(
                "VirtualProtect failed while installing RazorVue Razor SG hook. Win32 error: " +
                Marshal.GetLastWin32Error().ToString(System.Globalization.CultureInfo.InvariantCulture) +
                ".");
        }

        try
        {
            Marshal.Copy(bytes, 0, address, bytes.Length);
            _ = FlushInstructionCache(GetCurrentProcess(), address, (UIntPtr)bytes.Length);
        }
        finally
        {
            _ = VirtualProtect(address, (UIntPtr)bytes.Length, oldProtect, out _);
        }
    }

    private static void WriteBytesUnix(IntPtr address, byte[] bytes)
    {
        var pageSize = GetPageSize();
        var start = address.ToInt64();
        var pageStart = start - (start % pageSize);
        var end = start + bytes.Length;
        var pageEnd = ((end + pageSize - 1) / pageSize) * pageSize;
        var pageAddress = new IntPtr(pageStart);
        var pageLength = new UIntPtr((ulong)(pageEnd - pageStart));

        if (MProtect(pageAddress, pageLength, UnixProtRead | UnixProtWrite | UnixProtExecute) != 0)
        {
            throw new InvalidOperationException(
                "mprotect(RWX) failed while installing RazorVue Razor SG hook. errno: " +
                Marshal.GetLastWin32Error().ToString(System.Globalization.CultureInfo.InvariantCulture) +
                ".");
        }

        try
        {
            Marshal.Copy(bytes, 0, address, bytes.Length);
            FlushInstructionCacheUnix(address, bytes.Length);
        }
        finally
        {
            _ = MProtect(pageAddress, pageLength, UnixProtRead | UnixProtExecute);
        }
    }

    private static int MProtect(IntPtr address, UIntPtr length, int protection)
        => RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? MProtectMacOS(address, length, protection)
            : MProtectLinux(address, length, protection);

    private static void FlushInstructionCacheUnix(IntPtr address, int length)
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SysICacheInvalidate(address, (UIntPtr)length);
        }
    }

    private static int GetPageSize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return GetPageSizeMacOS();

        return GetPageSizeLinux();
    }

    private static string DescribeCurrentPlatform()
        => "OS: " +
           GetCurrentOperatingSystem() +
           "; Architecture: " +
           RuntimeInformation.ProcessArchitecture +
           ".";

    private static string CreateUnsupportedPlatformReason(
        NativePlatform operatingSystem,
        Architecture architecture,
        string reason)
        => reason +
           " Current platform: OS: " +
           operatingSystem +
           "; Architecture: " +
           architecture +
           ".";

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static int SelfTestTarget(int value)
        => value + 1;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static int SelfTestReplacement(int value)
    {
        Interlocked.Increment(ref _selfTestReplacementCount);
        return value + 2;
    }

    [DllImport("kernel32", SetLastError = true)]
    private static extern bool VirtualProtect(
        IntPtr lpAddress,
        UIntPtr dwSize,
        uint flNewProtect,
        out uint lpflOldProtect);

    [DllImport("kernel32")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32", SetLastError = true)]
    private static extern bool FlushInstructionCache(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        UIntPtr dwSize);

    [DllImport("libc", EntryPoint = "mprotect", SetLastError = true)]
    private static extern int MProtectLinux(
        IntPtr addr,
        UIntPtr len,
        int prot);

    [DllImport("libSystem.dylib", EntryPoint = "mprotect", SetLastError = true)]
    private static extern int MProtectMacOS(
        IntPtr addr,
        UIntPtr len,
        int prot);

    [DllImport("libc", EntryPoint = "getpagesize")]
    private static extern int GetPageSizeLinux();

    [DllImport("libSystem.dylib", EntryPoint = "getpagesize")]
    private static extern int GetPageSizeMacOS();

    [DllImport("libSystem.dylib", EntryPoint = "sys_icache_invalidate")]
    private static extern void SysICacheInvalidate(
        IntPtr start,
        UIntPtr len);
}

/// <summary>Platforms distinguished by the native code-patching implementation. 枚举决定跳转指令编码。</summary>
internal enum NativePlatform
{
    Windows,
    Linux,
    MacOS,
    Other
}
