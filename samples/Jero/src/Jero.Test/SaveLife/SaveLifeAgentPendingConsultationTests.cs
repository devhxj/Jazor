using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentPendingConsultationTests
{
    [Fact]
    public void StartOrReplace_WithoutCurrentGoal_CreatesPendingGoal()
    {
        var proposed = Consultation(7001, 6001, SaveLifeAgentSubjectScopes.Self);

        var result = SaveLifeAgentPendingConsultationPolicy.StartOrReplace(null, proposed);

        Assert.Equal(SaveLifeAgentPendingConsultationTransitionKinds.Created, result.Kind);
        Assert.Equal(7001, result.Current?.Id);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Pending, result.Current?.Status);
        Assert.Null(result.Previous);
    }

    [Fact]
    public void StartOrReplace_WithSameCommand_IsIdempotent()
    {
        var current = Consultation(7001, 6001, SaveLifeAgentSubjectScopes.Self);
        var replay = Consultation(7001, 6001, SaveLifeAgentSubjectScopes.Self);

        var result = SaveLifeAgentPendingConsultationPolicy.StartOrReplace(current, replay);

        Assert.Equal(SaveLifeAgentPendingConsultationTransitionKinds.AlreadyApplied, result.Kind);
        Assert.Equal(7001, result.Current?.Id);
        Assert.Null(result.Previous);
        Assert.NotSame(current, result.Current);
    }

    [Fact]
    public void StartOrReplace_WithNewIndividualizedGoal_SupersedesPreviousGoal()
    {
        var current = Consultation(7001, 6001, SaveLifeAgentSubjectScopes.Self);
        var proposed = Consultation(7002, 6002, SaveLifeAgentSubjectScopes.Other);

        var result = SaveLifeAgentPendingConsultationPolicy.StartOrReplace(current, proposed);

        Assert.Equal(SaveLifeAgentPendingConsultationTransitionKinds.Replaced, result.Kind);
        Assert.Equal(7002, result.Current?.Id);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Pending, result.Current?.Status);
        Assert.Equal(7001, result.Previous?.Id);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Superseded, result.Previous?.Status);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Pending, current.Status);
    }

    [Fact]
    public void Preserve_DuringPublicInformationConversation_KeepsPendingGoal()
    {
        var current = Consultation(7001, 6001, SaveLifeAgentSubjectScopes.Unknown);

        var result = SaveLifeAgentPendingConsultationPolicy.Preserve(current);

        Assert.Equal(SaveLifeAgentPendingConsultationTransitionKinds.Preserved, result.Kind);
        Assert.Equal(7001, result.Current?.Id);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Pending, result.Current?.Status);
        Assert.Null(result.Previous);
    }

    [Fact]
    public void Cancel_WithMatchingPendingGoal_CancelsWithoutRemovingAuditState()
    {
        var current = Consultation(7001, 6001, SaveLifeAgentSubjectScopes.Self);

        var result = SaveLifeAgentPendingConsultationPolicy.Cancel(current, 7001);

        Assert.Equal(SaveLifeAgentPendingConsultationTransitionKinds.Cancelled, result.Kind);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Cancelled, result.Current?.Status);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Pending, current.Status);
    }

    [Fact]
    public void Consume_RepeatedCommand_ReturnsAlreadyProcessedWithoutSecondTransition()
    {
        var current = Consultation(7001, 6001, SaveLifeAgentSubjectScopes.Self);

        var consumed = SaveLifeAgentPendingConsultationPolicy.Consume(current, 7001);
        var replay = SaveLifeAgentPendingConsultationPolicy.Consume(consumed.Current, 7001);

        Assert.Equal(SaveLifeAgentPendingConsultationTransitionKinds.Consumed, consumed.Kind);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Consumed, consumed.Current?.Status);
        Assert.Equal(SaveLifeAgentPendingConsultationTransitionKinds.AlreadyProcessed, replay.Kind);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Consumed, replay.Current?.Status);
    }

    [Fact]
    public void Consume_WithStaleGoalId_DoesNotConsumeCurrentGoal()
    {
        var current = Consultation(7002, 6002, SaveLifeAgentSubjectScopes.Other);

        var result = SaveLifeAgentPendingConsultationPolicy.Consume(current, 7001);

        Assert.Equal(SaveLifeAgentPendingConsultationTransitionKinds.NotCurrent, result.Kind);
        Assert.Equal(7002, result.Current?.Id);
        Assert.Equal(SaveLifeAgentPendingConsultationStatuses.Pending, result.Current?.Status);
    }

    [Fact]
    public void StartOrReplace_WithReusedIdAndDifferentSource_RejectsConflict()
    {
        var current = Consultation(7001, 6001, SaveLifeAgentSubjectScopes.Self);
        var conflicting = Consultation(7001, 6002, SaveLifeAgentSubjectScopes.Self);

        Assert.Throws<ArgumentException>(() =>
            SaveLifeAgentPendingConsultationPolicy.StartOrReplace(current, conflicting));
    }

    private static SaveLifeAgentPendingConsultationState Consultation(
        long id,
        long sourceMessageId,
        string subjectHint)
    {
        return new SaveLifeAgentPendingConsultationState
        {
            Id = id,
            SourceUserMessageId = sourceMessageId,
            GoalType = SaveLifeAgentPendingConsultationGoals.IllnessHelp,
            SubjectHint = subjectHint,
            CreatedTime = new DateTime(2026, 7, 15, 10, 30, 0, DateTimeKind.Utc),
            Status = SaveLifeAgentPendingConsultationStatuses.Pending
        };
    }
}
