using Microsoft.AspNetCore.Mvc;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentHistoryPolicyTests
{
    [Fact]
    public void SelectModelHistory_UsesCompletedReplyPairsForCurrentPatientOnly()
    {
        var messages = new List<SaveLifeAgentMessage>
        {
            Message(10, "user", 501, "患者一问题 1"),
            Message(11, "assistant", 501, "患者一回答 1", 10, "completed"),
            Message(20, "user", 502, "患者二问题"),
            Message(21, "assistant", 502, "患者二回答", 20, "completed"),
            Message(30, "user", 501, "失败问题"),
            Message(31, "assistant", 501, "失败回答", 30, "failed"),
            Message(32, "user", 501, "未知状态问题"),
            Message(33, "assistant", 501, "未知状态回答", 32, "cancelled"),
            Message(40, "user", 501, "患者一问题 2"),
            Message(41, "assistant", 501, "患者一回答 2", 40, "completed"),
            Message(50, "user", 501, "当前问题"),
            Message(51, "assistant", 501, "不应存在的当前回复", 50, "completed")
        };

        var history = SaveLifeAgentHistoryPolicy.SelectModelHistory(
            messages,
            patientProfileId: 501,
            currentUserMessageId: 50,
            maxRounds: 10);

        Assert.Equal([10L, 11L, 40L, 41L], history.Select(u => u.Id));
    }

    [Fact]
    public void SelectModelHistory_TakesMostRecentConfiguredRoundsInChronologicalOrder()
    {
        var messages = Enumerable.Range(1, 5)
            .SelectMany(index => new[]
            {
                Message(index * 10, "user", 501, $"问题 {index}"),
                Message(index * 10 + 1, "assistant", 501, $"回答 {index}", index * 10, null)
            })
            .Reverse()
            .ToList();

        var history = SaveLifeAgentHistoryPolicy.SelectModelHistory(
            messages,
            patientProfileId: 501,
            currentUserMessageId: 99,
            maxRounds: 2);

        Assert.Equal([40L, 41L, 50L, 51L], history.Select(u => u.Id));
    }

    [Fact]
    public void SelectModelHistory_WithoutPatient_UsesOnlyUnscopedCompletedPairs()
    {
        var messages = new List<SaveLifeAgentMessage>
        {
            Message(10, "user", null, "通用问题"),
            Message(11, "assistant", null, "通用回答", 10, "completed"),
            Message(20, "user", 501, "患者问题"),
            Message(21, "assistant", 501, "患者回答", 20, "completed"),
            Message(30, "user", null, "当前通用问题")
        };

        var history = SaveLifeAgentHistoryPolicy.SelectModelHistory(
            messages,
            patientProfileId: null,
            currentUserMessageId: 30,
            maxRounds: 10);

        Assert.Equal([10L, 11L], history.Select(u => u.Id));
    }

    [Fact]
    public void BuildConversationState_UsesPersistedStateAndConfirmedPatientProfile()
    {
        var conversation = new SaveLifeAgentConversation
        {
            CurrentPatientProfileId = 501,
            ExternalUserId = "openid-a",
            FlowStateJson = SaveLifeAgentConversationStateCodec.Serialize(
                new SaveLifeAgentConversationState
                {
                    GeneralTrialOptOut = true,
                    IsGeneralGuidanceDeferred = true,
                    PendingConsultation = new SaveLifeAgentPendingConsultationState
                    {
                        Id = 801,
                        SourceUserMessageId = 601,
                        GoalType = SaveLifeAgentPendingConsultationGoals.IllnessHelp,
                        SubjectHint = SaveLifeAgentSubjectScopes.Other,
                        CreatedTime = new DateTime(2026, 7, 15, 10, 30, 0),
                        Status = SaveLifeAgentPendingConsultationStatuses.Pending
                    },
                    PendingQuestion = new SaveLifeAgentPendingQuestionState
                    {
                        Key = "city",
                        Prompt = "患者目前在哪个城市？"
                    },
                    SelectedTrialId = 701,
                    TrialOptOut = true,
                    TrialOptOutPatientProfileIds = [501, 502]
                })
        };
        var patient = new SaveLifePatientProfile
        {
            Id = 501,
            DisplayName = "母亲",
            OwnerExternalUserId = "openid-a",
            RelationshipDescription = "母亲",
            RelationshipToAccount = SaveLifePatientRelationships.Parent
        };

        var state = SaveLifeAgentHistoryPolicy.BuildConversationState(conversation, patient);

        Assert.Equal("city", state.PendingQuestion?.Key);
        Assert.Equal("801", state.PendingConsultation?.Id);
        Assert.Equal("601", state.PendingConsultation?.SourceUserMessageId);
        Assert.Equal(SaveLifeAgentPendingConsultationGoals.IllnessHelp, state.PendingConsultation?.GoalType);
        Assert.Equal(SaveLifeAgentSubjectScopes.Other, state.PendingConsultation?.SubjectHint);
        Assert.Equal(new DateTime(2026, 7, 15, 10, 30, 0), state.PendingConsultation?.CreatedTime);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Pending, state.PendingConsultation?.Status);
        Assert.True(state.IsGeneralGuidanceDeferred);
        Assert.True(state.GeneralTrialOptOut);
        Assert.Equal(["501", "502"], state.TrialOptOutPatientProfileIds);
        Assert.Equal("701", state.SelectedTrialId);
        Assert.True(state.TrialOptOut);
        Assert.Equal("501", state.CurrentPatient?.Id);
        Assert.Equal("母亲", state.CurrentPatient?.DisplayName);
    }

    [Fact]
    public void MessagePageContract_UsesCursorWithoutLegacyFlowState()
    {
        var endpoint = typeof(SaveLifeAgentService).GetMethod(
            "GetConversationMessagesPage");

        Assert.NotNull(endpoint);
        Assert.NotNull(endpoint!.GetCustomAttributes(typeof(HttpGetAttribute), false).Single());
        Assert.NotNull(typeof(QuerySaveLifeAgentMessagePageInput).GetProperty("BeforeMessageId"));
        Assert.NotNull(typeof(QuerySaveLifeAgentMessagePageInput).GetProperty("PageSize"));
        Assert.NotNull(typeof(SaveLifeAgentMessagePageOutput).GetProperty("Items"));
        Assert.NotNull(typeof(SaveLifeAgentMessagePageOutput).GetProperty("NextCursor"));
        Assert.NotNull(typeof(SaveLifeAgentMessagePageOutput).GetProperty("HasMore"));
        Assert.Null(typeof(SaveLifeAgentMessagePageOutput).GetProperty("State"));
        Assert.NotNull(typeof(SaveLifeAgentMessageOutput).GetProperty("PatientProfileId"));
    }

    private static SaveLifeAgentMessage Message(
        long id,
        string role,
        long? patientProfileId,
        string content,
        long? replyToMessageId = null,
        string? generationStatus = null)
    {
        return new SaveLifeAgentMessage
        {
            Id = id,
            ConversationId = 100,
            Content = content,
            CreateTime = new DateTime(2026, 7, 14).AddMinutes(id),
            ExternalUserId = "openid-a",
            GenerationStatus = generationStatus,
            PatientProfileId = patientProfileId,
            ReplyToMessageId = replyToMessageId,
            Role = role
        };
    }
}
