using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentTurnConsistencyTests
{
    public static TheoryData<long?, long> ChangedSnapshots => new()
    {
        { 502L, 7L },
        { 501L, 8L },
        { null, 8L }
    };

    [Fact]
    public void EnsureCurrent_WithMatchingPatientSnapshot_AllowsTurn()
    {
        var conversation = Conversation(patientProfileId: 501, patientContextVersion: 7);
        var message = Message(patientProfileId: 501, patientContextVersion: 7);

        SaveLifeAgentTurnConsistencyPolicy.EnsureCurrent(conversation, message);
    }

    [Theory]
    [MemberData(nameof(ChangedSnapshots))]
    public void EnsureCurrent_WithChangedPatientSnapshot_FailsClosed(
        long? currentPatientProfileId,
        long currentPatientContextVersion)
    {
        var conversation = Conversation(currentPatientProfileId, currentPatientContextVersion);
        var message = Message(patientProfileId: 501, patientContextVersion: 7);

        var error = Assert.Throws<SaveLifeAgentUnavailableException>(() =>
            SaveLifeAgentTurnConsistencyPolicy.EnsureCurrent(conversation, message));

        Assert.Equal(SaveLifeAgentFailureCodes.PatientContextChanged, error.Code);
        Assert.Equal(SaveLifeAgentFailureStages.Persistence, error.Stage);
        Assert.True(error.Retryable);
        Assert.Contains("患者", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsureCurrent_WithUnscopedMatchingSnapshot_AllowsTurn()
    {
        var conversation = Conversation(patientProfileId: null, patientContextVersion: 3);
        var message = Message(patientProfileId: null, patientContextVersion: 3);

        SaveLifeAgentTurnConsistencyPolicy.EnsureCurrent(conversation, message);
    }

    [Fact]
    public void EnsureCurrent_WithDifferentOwner_FailsClosed()
    {
        var conversation = Conversation(patientProfileId: 501, patientContextVersion: 7);
        var message = Message(patientProfileId: 501, patientContextVersion: 7);
        message.ExternalUserId = "openid-2";

        var error = Assert.Throws<SaveLifeAgentUnavailableException>(() =>
            SaveLifeAgentTurnConsistencyPolicy.EnsureCurrent(conversation, message));

        Assert.Equal(SaveLifeAgentFailureCodes.PatientContextChanged, error.Code);
    }

    private static SaveLifeAgentConversation Conversation(
        long? patientProfileId,
        long patientContextVersion)
    {
        return new SaveLifeAgentConversation
        {
            Id = 101,
            ExternalUserId = "openid-1",
            CurrentPatientProfileId = patientProfileId,
            PatientContextVersion = patientContextVersion,
            Status = "active"
        };
    }

    private static SaveLifeAgentMessage Message(
        long? patientProfileId,
        long patientContextVersion)
    {
        return new SaveLifeAgentMessage
        {
            Id = 1001,
            ConversationId = 101,
            ExternalUserId = "openid-1",
            PatientProfileId = patientProfileId,
            PatientContextVersion = patientContextVersion,
            Role = "user",
            Content = "合成测试消息"
        };
    }
}
