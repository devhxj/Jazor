using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeEnrollmentStatusPolicyTests
{
    [Theory]
    [InlineData("pending", "approved", true)]
    [InlineData("pending", "rejected", true)]
    [InlineData("need_supplement", "approved", true)]
    [InlineData("approved", "rejected", false)]
    [InlineData("rejected", "approved", false)]
    [InlineData("contacted", "approved", false)]
    public void CanAudit_OnlyAllowsReviewStatuses(
        string currentStatus,
        string nextStatus,
        bool expected)
    {
        Assert.Equal(expected, SaveLifeEnrollmentStatusPolicy.CanAudit(currentStatus, nextStatus));
    }

    [Theory]
    [InlineData("approved", true)]
    [InlineData("pending", false)]
    [InlineData("contacted", false)]
    [InlineData("rejected", false)]
    public void CanContact_OnlyAllowsApprovedRecords(string currentStatus, bool expected)
    {
        Assert.Equal(expected, SaveLifeEnrollmentStatusPolicy.CanContact(currentStatus));
    }

    [Fact]
    public void Normalize_TrimsAndUsesLowerCase()
    {
        Assert.Equal("approved", SaveLifeEnrollmentStatusPolicy.Normalize("  APPROVED "));
        Assert.Equal(string.Empty, SaveLifeEnrollmentStatusPolicy.Normalize(null));
    }
}
