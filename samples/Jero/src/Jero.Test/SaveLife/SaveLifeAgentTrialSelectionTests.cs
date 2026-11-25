using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentTrialSelectionTests
{
    [Fact]
    public void ResolveCandidateTrialId_WithExplicitTrialId_PrefersInputTrial()
    {
        var matchedTrials = new List<SaveLifeAgentMatchedTrialOutput>
        {
            new() { Id = "2002", Title = "匹配项目" }
        };

        var trialId = SaveLifeAgentTrialSelection.ResolveCandidateTrialId(1001, matchedTrials);

        Assert.Equal(1001, trialId);
    }

    [Fact]
    public void ResolveCandidateTrialId_WithoutExplicitTrialId_UsesFirstMatchedTrial()
    {
        var matchedTrials = new List<SaveLifeAgentMatchedTrialOutput>
        {
            new() { Id = "2002", Title = "第一匹配项目" },
            new() { Id = "3003", Title = "第二匹配项目" }
        };

        var trialId = SaveLifeAgentTrialSelection.ResolveCandidateTrialId(null, matchedTrials);

        Assert.Equal(2002, trialId);
    }

    [Fact]
    public void ResolveCandidateTrialId_WithoutAnyCandidate_ReturnsNull()
    {
        var trialId = SaveLifeAgentTrialSelection.ResolveCandidateTrialId(null, new List<SaveLifeAgentMatchedTrialOutput>());

        Assert.Null(trialId);
    }

    [Theory]
    [InlineData("recruiting", true)]
    [InlineData("Recruiting", true)]
    [InlineData("paused", false)]
    [InlineData("closed", false)]
    public void IsRecruiting_WithTrialStatus_ReturnsExpectedResult(string status, bool expected)
    {
        var trial = new SaveLifeClinicalTrial { Status = status };

        var result = SaveLifeAgentTrialSelection.IsRecruiting(trial);

        Assert.Equal(expected, result);
    }
}
