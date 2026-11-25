using Jero.Core.Service;
using Xunit;

namespace Jero.Test.Utils;

public class SysServerInfoProviderTests
{
    [Fact]
    public void GetRuntimeInfo_ReturnsCurrentProcessAndRuntimeMetadata()
    {
        var runtimeInfo = SysServerInfoProvider.GetRuntimeInfo();

        Assert.False(string.IsNullOrWhiteSpace(runtimeInfo.OsName));
        Assert.False(string.IsNullOrWhiteSpace(runtimeInfo.FrameworkDescription));
        Assert.True(runtimeInfo.ProcessorCount > 0);
        Assert.True(runtimeInfo.ProcessId > 0);
        Assert.NotNull(runtimeInfo.CommandLineArgs);
    }

    [Fact]
    public void GetNuGetPackages_EmptyPrefix_ReturnsEmptyList()
    {
        var packages = SysServerInfoProvider.GetNuGetPackages(string.Empty);

        Assert.Empty(packages);
    }

    [Fact]
    public void NuGetPackage_ToString_ReturnsPackageIdentifier()
    {
        var package = new NuGetPackage
        {
            PackageName = "SqlSugarCore",
            PackageVersion = "5.1.4.215"
        };

        Assert.Equal("SqlSugarCore@5.1.4.215", package.ToString());
    }
}
