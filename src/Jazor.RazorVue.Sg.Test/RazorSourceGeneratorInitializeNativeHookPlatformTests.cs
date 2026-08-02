using System.Runtime.InteropServices;
using Jazor.RazorVue.Generation;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSourceGeneratorInitializeNativeHookPlatformTests
{
    public static IEnumerable<TestDataRow<NativeHookPlatformCase>> Cases
        => NativeHookPlatformCase.All.Select(static testCase => new TestDataRow<NativeHookPlatformCase>(testCase)
        {
            DisplayName = "NativeHook_" + testCase.Id
        });

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void IsSupportedPlatform_EnforcesValidatedRuntimeMatrix(NativeHookPlatformCase testCase)
    {
        var supported = RazorSourceGeneratorInitializeNativeHook.IsSupportedPlatform(
            (RazorSourceGeneratorInitializeNativeHookOperatingSystem)testCase.OperatingSystemValue,
            (Architecture)testCase.ArchitectureValue,
            out var failure);

        Assert.AreEqual(testCase.IsSupported, supported);
        if (testCase.IsSupported)
        {
            Assert.AreEqual(string.Empty, failure);
            return;
        }

        StringAssert.Contains(failure, testCase.FailureFragment, StringComparison.Ordinal);
        StringAssert.Contains(
            failure,
            ((RazorSourceGeneratorInitializeNativeHookOperatingSystem)testCase.OperatingSystemValue).ToString(),
            StringComparison.Ordinal);
        StringAssert.Contains(failure, ((Architecture)testCase.ArchitectureValue).ToString(), StringComparison.Ordinal);
    }
}

public sealed record NativeHookPlatformCase(
    string Id,
    int OperatingSystemValue,
    int ArchitectureValue,
    bool IsSupported,
    string FailureFragment)
{
    public static IReadOnlyList<NativeHookPlatformCase> All { get; } =
    [
        new("windows_x64", (int)RazorSourceGeneratorInitializeNativeHookOperatingSystem.Windows, (int)Architecture.X64, true, string.Empty),
        new("windows_arm64", (int)RazorSourceGeneratorInitializeNativeHookOperatingSystem.Windows, (int)Architecture.Arm64, true, string.Empty),
        new("linux_x64", (int)RazorSourceGeneratorInitializeNativeHookOperatingSystem.Linux, (int)Architecture.X64, true, string.Empty),
        new("macos_x64", (int)RazorSourceGeneratorInitializeNativeHookOperatingSystem.MacOS, (int)Architecture.X64, true, string.Empty),
        new("macos_arm64", (int)RazorSourceGeneratorInitializeNativeHookOperatingSystem.MacOS, (int)Architecture.Arm64, true, string.Empty),
        new("linux_arm64", (int)RazorSourceGeneratorInitializeNativeHookOperatingSystem.Linux, (int)Architecture.Arm64, false, "does not enable Linux arm64"),
        new("other_x64", (int)RazorSourceGeneratorInitializeNativeHookOperatingSystem.Other, (int)Architecture.X64, false, "supports Windows, Linux, and macOS only"),
        new("windows_x86", (int)RazorSourceGeneratorInitializeNativeHookOperatingSystem.Windows, (int)Architecture.X86, false, "supports x64 and arm64 processes only")
    ];
}
