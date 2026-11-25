using Jero.Core;
using Xunit;

namespace Jero.Test.Utils;

public class DefaultHomePathUtilTests
{
    [Fact]
    public void TryNormalize_ValidInternalPath_ReturnsTrimmedPath()
    {
        var result = DefaultHomePathUtil.TryNormalize("  /dashboard/workbench?tab=todo  ", out var normalized);

        Assert.True(result);
        Assert.Equal("/dashboard/workbench?tab=todo", normalized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("dashboard/home")]
    [InlineData("//example.com")]
    [InlineData("https://example.com/dashboard")]
    [InlineData("/dashboard home")]
    [InlineData("/dashboard\\home")]
    [InlineData("/dashboard/\n/home")]
    public void TryNormalize_InvalidPath_ReturnsFalse(string? value)
    {
        var result = DefaultHomePathUtil.TryNormalize(value, out var normalized);

        Assert.False(result);
        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void TryNormalize_TooLongPath_ReturnsFalse()
    {
        var value = "/" + new string('a', DefaultHomePathUtil.MaxLength);

        var result = DefaultHomePathUtil.TryNormalize(value, out var normalized);

        Assert.False(result);
        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void NormalizeOrFallback_InvalidPath_ReturnsFallbackHomePath()
    {
        var result = DefaultHomePathUtil.NormalizeOrFallback("https://example.com");

        Assert.Equal(DefaultHomePathUtil.FallbackHomePath, result);
    }
}
