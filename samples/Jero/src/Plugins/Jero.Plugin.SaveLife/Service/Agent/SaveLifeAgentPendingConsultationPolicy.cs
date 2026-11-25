namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentPendingConsultationPolicy
{
    public static SaveLifeAgentPendingConsultationTransition StartOrReplace(
        SaveLifeAgentPendingConsultationState? current,
        SaveLifeAgentPendingConsultationState proposed)
    {
        ArgumentNullException.ThrowIfNull(proposed);
        Validate(proposed, requirePending: true);

        if (current == null)
        {
            return new SaveLifeAgentPendingConsultationTransition(
                SaveLifeAgentPendingConsultationTransitionKinds.Created,
                Clone(proposed),
                null);
        }

        Validate(current, requirePending: false);
        if (current.Id == proposed.Id)
        {
            if (!HasSameIdentity(current, proposed))
                throw new ArgumentException("待继续咨询 Id 已用于其他目标。", nameof(proposed));

            var kind = current.Status == SaveLifeAgentPendingConsultationStatuses.Pending
                ? SaveLifeAgentPendingConsultationTransitionKinds.AlreadyApplied
                : SaveLifeAgentPendingConsultationTransitionKinds.AlreadyProcessed;
            return new SaveLifeAgentPendingConsultationTransition(kind, Clone(current), null);
        }

        var previous = Clone(current);
        if (previous.Status == SaveLifeAgentPendingConsultationStatuses.Pending)
            previous.Status = SaveLifeAgentPendingConsultationStatuses.Superseded;

        return new SaveLifeAgentPendingConsultationTransition(
            SaveLifeAgentPendingConsultationTransitionKinds.Replaced,
            Clone(proposed),
            previous);
    }

    public static SaveLifeAgentPendingConsultationTransition Preserve(
        SaveLifeAgentPendingConsultationState? current)
    {
        if (current == null)
        {
            return new SaveLifeAgentPendingConsultationTransition(
                SaveLifeAgentPendingConsultationTransitionKinds.NotCurrent,
                null,
                null);
        }

        Validate(current, requirePending: false);
        return new SaveLifeAgentPendingConsultationTransition(
            SaveLifeAgentPendingConsultationTransitionKinds.Preserved,
            Clone(current),
            null);
    }

    public static SaveLifeAgentPendingConsultationTransition Cancel(
        SaveLifeAgentPendingConsultationState? current,
        long pendingConsultationId)
    {
        return Complete(
            current,
            pendingConsultationId,
            SaveLifeAgentPendingConsultationStatuses.Cancelled,
            SaveLifeAgentPendingConsultationTransitionKinds.Cancelled);
    }

    public static SaveLifeAgentPendingConsultationTransition Consume(
        SaveLifeAgentPendingConsultationState? current,
        long pendingConsultationId)
    {
        return Complete(
            current,
            pendingConsultationId,
            SaveLifeAgentPendingConsultationStatuses.Consumed,
            SaveLifeAgentPendingConsultationTransitionKinds.Consumed);
    }

    private static SaveLifeAgentPendingConsultationTransition Complete(
        SaveLifeAgentPendingConsultationState? current,
        long pendingConsultationId,
        string completedStatus,
        string transitionKind)
    {
        if (pendingConsultationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(pendingConsultationId));

        if (current == null || current.Id != pendingConsultationId)
        {
            return new SaveLifeAgentPendingConsultationTransition(
                SaveLifeAgentPendingConsultationTransitionKinds.NotCurrent,
                current == null ? null : Clone(current),
                null);
        }

        Validate(current, requirePending: false);
        if (current.Status != SaveLifeAgentPendingConsultationStatuses.Pending)
        {
            return new SaveLifeAgentPendingConsultationTransition(
                SaveLifeAgentPendingConsultationTransitionKinds.AlreadyProcessed,
                Clone(current),
                null);
        }

        var completed = Clone(current);
        completed.Status = completedStatus;
        return new SaveLifeAgentPendingConsultationTransition(transitionKind, completed, null);
    }

    private static bool HasSameIdentity(
        SaveLifeAgentPendingConsultationState left,
        SaveLifeAgentPendingConsultationState right)
    {
        return left.SourceUserMessageId == right.SourceUserMessageId &&
               string.Equals(left.GoalType, right.GoalType, StringComparison.Ordinal) &&
               string.Equals(left.SubjectHint, right.SubjectHint, StringComparison.Ordinal);
    }

    private static void Validate(
        SaveLifeAgentPendingConsultationState state,
        bool requirePending)
    {
        if (state.Id <= 0)
            throw new ArgumentException("待继续咨询 Id 必须为正数。", nameof(state));
        if (state.SourceUserMessageId <= 0)
            throw new ArgumentException("来源消息 Id 必须为正数。", nameof(state));
        if (state.GoalType != SaveLifeAgentPendingConsultationGoals.IllnessHelp)
            throw new ArgumentException("待继续咨询目标类型无效。", nameof(state));
        if (!SaveLifeAgentSubjectScopes.All.Contains(state.SubjectHint))
            throw new ArgumentException("待继续咨询主体范围无效。", nameof(state));
        if (state.CreatedTime == default)
            throw new ArgumentException("待继续咨询创建时间不能为空。", nameof(state));

        var knownStatus = state.Status is
            SaveLifeAgentPendingConsultationStatuses.Pending or
            SaveLifeAgentPendingConsultationStatuses.Consumed or
            SaveLifeAgentPendingConsultationStatuses.Cancelled or
            SaveLifeAgentPendingConsultationStatuses.Superseded;
        if (!knownStatus || (requirePending && state.Status != SaveLifeAgentPendingConsultationStatuses.Pending))
            throw new ArgumentException("待继续咨询状态无效。", nameof(state));
    }

    private static SaveLifeAgentPendingConsultationState Clone(
        SaveLifeAgentPendingConsultationState state)
    {
        return new SaveLifeAgentPendingConsultationState
        {
            Id = state.Id,
            SourceUserMessageId = state.SourceUserMessageId,
            GoalType = state.GoalType,
            SubjectHint = state.SubjectHint,
            CreatedTime = state.CreatedTime,
            Status = state.Status
        };
    }
}

public sealed record SaveLifeAgentPendingConsultationTransition(
    string Kind,
    SaveLifeAgentPendingConsultationState? Current,
    SaveLifeAgentPendingConsultationState? Previous);

public static class SaveLifeAgentPendingConsultationTransitionKinds
{
    public const string Created = "created";
    public const string AlreadyApplied = "already_applied";
    public const string Replaced = "replaced";
    public const string Preserved = "preserved";
    public const string Cancelled = "cancelled";
    public const string Consumed = "consumed";
    public const string AlreadyProcessed = "already_processed";
    public const string NotCurrent = "not_current";
}
