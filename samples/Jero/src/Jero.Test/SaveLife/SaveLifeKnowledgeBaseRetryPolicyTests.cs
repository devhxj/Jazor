using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeKnowledgeBaseRetryPolicyTests
{
    [Fact]
    public void CalculateNextRetryTime_WithTransientErrors_UsesCappedExponentialBackoff()
    {
        var now = new DateTime(2026, 7, 2, 10, 0, 0);

        Assert.Equal(now.AddDays(1), SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(1, "timeout", now));
        Assert.Equal(now.AddDays(2), SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(2, "timeout", now));
        Assert.Equal(now.AddDays(4), SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(3, "timeout", now));
        Assert.Equal(now.AddDays(7), SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(10, "timeout", now));
    }

    [Fact]
    public void CalculateNextRetryTime_WithConfigurationErrors_UsesLongRetryWindow()
    {
        var now = new DateTime(2026, 7, 2, 10, 0, 0);

        var nextRetry = SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(1, "Embedding API Key 未配置", now);

        Assert.Equal(now.AddDays(7), nextRetry);
    }

    [Fact]
    public void IsRetryDue_WithMissingOrPastTime_ReturnsTrue()
    {
        var now = new DateTime(2026, 7, 2, 10, 0, 0);

        Assert.True(SaveLifeKnowledgeBaseRetryPolicy.IsRetryDue(null, now));
        Assert.True(SaveLifeKnowledgeBaseRetryPolicy.IsRetryDue(now.AddMinutes(-1), now));
        Assert.False(SaveLifeKnowledgeBaseRetryPolicy.IsRetryDue(now.AddMinutes(1), now));
    }
}
