using Jero.Core.Update;
using Xunit;

namespace Jero.Test.Utils;

public class AutoVersionUpdateVersionComparerTests
{
    [Fact]
    public void Compare_SemanticVersionWithDoubleDigitMinor_ReturnsCorrectOrder()
    {
        var result = AutoVersionUpdateVersionComparer.Compare("1.10.0", "1.2.0");

        Assert.True(result > 0);
    }

    [Fact]
    public void Compare_EmptyVersion_IsLowerThanNonEmptyVersion()
    {
        var result = AutoVersionUpdateVersionComparer.Compare(string.Empty, "1.0.0");

        Assert.True(result < 0);
    }

    [Fact]
    public void Compare_MissingVersionParts_NormalizesBeforeComparison()
    {
        var result = AutoVersionUpdateVersionComparer.Compare("1.0", "1.0.0");

        Assert.Equal(0, result);
    }

    [Fact]
    public void Compare_InvalidVersion_FallsBackToOrdinalComparison()
    {
        var result = AutoVersionUpdateVersionComparer.Compare("release-b", "release-a");

        Assert.True(result > 0);
    }
}
