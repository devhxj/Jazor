using Jero.Plugin.SaveLife;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentConversationStateTests
{
    [Fact]
    public void Codec_RoundTripsVersionTwoProgressiveConsultationState()
    {
        var state = new SaveLifeAgentConversationState
        {
            PendingConsultation = new SaveLifeAgentPendingConsultationState
            {
                Id = 7001,
                SourceUserMessageId = 6001,
                GoalType = SaveLifeAgentPendingConsultationGoals.IllnessHelp,
                SubjectHint = SaveLifeAgentSubjectScopes.Self,
                CreatedTime = new DateTime(2026, 7, 15, 10, 30, 0),
                Status = SaveLifeAgentPendingConsultationStatuses.Pending
            },
            IsGeneralGuidanceDeferred = true,
            GeneralTrialOptOut = true,
            TrialOptOutPatientProfileIds = [501, 502]
        };

        var serialized = SaveLifeAgentConversationStateCodec.Serialize(state);
        var restored = SaveLifeAgentConversationStateCodec.Deserialize(serialized);

        Assert.Equal(2, SaveLifeAgentConversationState.CurrentVersion);
        Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, restored.Version);
        Assert.Equal(7001, restored.PendingConsultation?.Id);
        Assert.Equal(6001, restored.PendingConsultation?.SourceUserMessageId);
        Assert.Equal(SaveLifeAgentPendingConsultationGoals.IllnessHelp, restored.PendingConsultation?.GoalType);
        Assert.Equal(SaveLifeAgentSubjectScopes.Self, restored.PendingConsultation?.SubjectHint);
        Assert.Equal(new DateTime(2026, 7, 15, 10, 30, 0), restored.PendingConsultation?.CreatedTime);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Pending, restored.PendingConsultation?.Status);
        Assert.True(restored.IsGeneralGuidanceDeferred);
        Assert.True(restored.GeneralTrialOptOut);
        Assert.Equal([501L, 502L], restored.TrialOptOutPatientProfileIds);
        Assert.DoesNotContain("expires", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Codec_DeserializesVersionOneStateWithoutLosingLegacyValues()
    {
        const string serialized = """
            {
              "version": 1,
              "pendingQuestion": { "key": "city", "prompt": "患者目前在哪个城市？" },
              "trialOptOut": true,
              "selectedTrialId": 9001,
              "pendingPatientSwitchProfileId": 502
            }
            """;

        var restored = SaveLifeAgentConversationStateCodec.Deserialize(serialized);

        Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, restored.Version);
        Assert.Equal("city", restored.PendingQuestion?.Key);
        Assert.True(restored.TrialOptOut);
        Assert.Equal(9001, restored.SelectedTrialId);
        Assert.Equal(502, restored.PendingPatientSwitchProfileId);
        Assert.Null(restored.PendingConsultation);
        Assert.False(restored.IsGeneralGuidanceDeferred);
        Assert.False(restored.GeneralTrialOptOut);
        Assert.Empty(restored.TrialOptOutPatientProfileIds);
    }

    [Fact]
    public void Codec_WithFutureVersion_RejectsUnsupportedState()
    {
        var error = Assert.Throws<NotSupportedException>(() =>
            SaveLifeAgentConversationStateCodec.Deserialize("{\"version\":3}"));

        Assert.Contains("不支持的助手会话状态版本", error.Message, StringComparison.Ordinal);
    }
}
