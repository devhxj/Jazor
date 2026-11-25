using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentConversationPolicyTests
{
    [Fact]
    public void ShouldReuseLatest_WithSameBusinessContext_ReturnsTrue()
    {
        var latest = new SaveLifeAgentConversation
        {
            Status = "active",
            ContextType = "trial",
            ContextId = 1001
        };
        var requested = new SaveLifeAgentBusinessContextOutput
        {
            ContextType = "trial",
            ContextId = "1001"
        };

        var shouldReuse = SaveLifeAgentConversationPolicy.ShouldReuseLatest(latest, requested);

        Assert.True(shouldReuse);
    }

    [Fact]
    public void ShouldReuseLatest_WithDifferentBusinessContext_ReturnsTrue()
    {
        var latest = new SaveLifeAgentConversation
        {
            Status = "active",
            ContextType = "trial",
            ContextId = 1001
        };
        var requested = new SaveLifeAgentBusinessContextOutput
        {
            ContextType = "knowledge",
            ContextId = "1001"
        };

        var shouldReuse = SaveLifeAgentConversationPolicy.ShouldReuseLatest(latest, requested);

        Assert.True(shouldReuse);
    }

    [Fact]
    public void ShouldReuseLatest_WithClosedConversation_ReturnsFalse()
    {
        var latest = new SaveLifeAgentConversation
        {
            Status = "closed",
            ContextType = "trial",
            ContextId = 1001
        };
        var requested = new SaveLifeAgentBusinessContextOutput
        {
            ContextType = "trial",
            ContextId = "1001"
        };

        var shouldReuse = SaveLifeAgentConversationPolicy.ShouldReuseLatest(latest, requested);

        Assert.False(shouldReuse);
    }

    [Fact]
    public void ShouldReuseLatest_WithoutRequestedContext_ReusesContextualActiveConversation()
    {
        var contextless = new SaveLifeAgentConversation { Status = "active" };
        var contextual = new SaveLifeAgentConversation
        {
            Status = "active",
            ContextType = "trial",
            ContextId = 1001
        };

        Assert.True(SaveLifeAgentConversationPolicy.ShouldReuseLatest(contextless, null));
        Assert.True(SaveLifeAgentConversationPolicy.ShouldReuseLatest(contextual, null));
    }

    [Theory]
    [InlineData("closed")]
    [InlineData("draft")]
    [InlineData("pending")]
    public void ShouldReuseLatest_WithoutActiveStatus_ReturnsFalse(string status)
    {
        var latest = new SaveLifeAgentConversation { Status = status };

        Assert.False(SaveLifeAgentConversationPolicy.ShouldReuseLatest(latest, null));
    }

    [Fact]
    public void GetTurnEnrollmentRecordId_UsesOnlyExplicitEnrollmentContext()
    {
        var enrollment = new SaveLifeAgentBusinessContextOutput
        {
            ContextType = "enrollment",
            ContextId = "701"
        };
        var trial = new SaveLifeAgentBusinessContextOutput
        {
            ContextType = "trial",
            ContextId = "701"
        };

        Assert.Equal(701, SaveLifeAgentConversationPolicy.GetTurnEnrollmentRecordId(enrollment));
        Assert.Null(SaveLifeAgentConversationPolicy.GetTurnEnrollmentRecordId(trial));
        Assert.Null(SaveLifeAgentConversationPolicy.GetTurnEnrollmentRecordId(null));
    }

    [Fact]
    public void BuildConversationState_DerivesEffectiveSuppressionFromCurrentScope()
    {
        var conversation = new SaveLifeAgentConversation
        {
            Id = 101,
            ExternalUserId = "openid-1",
            CurrentPatientProfileId = 502,
            FlowStateJson = SaveLifeAgentConversationStateCodec.Serialize(
                new SaveLifeAgentConversationState
                {
                    GeneralTrialOptOut = true,
                    TrialOptOutPatientProfileIds = [501, 502]
                })
        };
        var patient = new SaveLifePatientProfile
        {
            Id = 502,
            OwnerExternalUserId = "openid-1",
            AccountPatientKey = SaveLifePatientProfileKeys.ForOther(502),
            RelationshipToAccount = SaveLifePatientRelationships.Parent,
            RelationshipDescription = "母亲",
            DisplayName = "母亲"
        };

        var patientState = SaveLifeAgentHistoryPolicy.BuildConversationState(conversation, patient);
        conversation.CurrentPatientProfileId = null;
        var generalState = SaveLifeAgentHistoryPolicy.BuildConversationState(conversation, null);

        Assert.True(patientState.TrialOptOut);
        Assert.True(generalState.TrialOptOut);
        Assert.True(generalState.GeneralTrialOptOut);
    }

}
