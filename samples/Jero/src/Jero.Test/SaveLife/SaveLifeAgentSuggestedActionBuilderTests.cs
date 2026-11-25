using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentSuggestedActionBuilderTests
{
    [Fact]
    public void BuildForIntention_WithDraftEnrollment_ReturnsRelevantBusinessActions()
    {
        var actions = SaveLifeAgentSuggestedActionBuilder.BuildForIntention(
            "draft",
            "701",
            "801",
            requiresProfileCompletion: true);

        Assert.Equal(
            ["view_enrollment", "complete_profile", "upload_material", "view_trial"],
            actions.Select(action => action.Type));
    }

    [Fact]
    public void BuildForIntention_WithoutEnrollment_DoesNotInventEnrollmentActions()
    {
        var actions = SaveLifeAgentSuggestedActionBuilder.BuildForIntention(
            "considering",
            null,
            "801",
            requiresProfileCompletion: false);

        var action = Assert.Single(actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.ViewTrial, action.Type);
        Assert.Equal("801", action.TargetId);
    }
}
