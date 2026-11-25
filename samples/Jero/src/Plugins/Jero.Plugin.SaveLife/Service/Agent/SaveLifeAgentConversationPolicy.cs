namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentConversationPolicy
{
    public static bool ShouldReuseLatest(
        SaveLifeAgentConversation? latest,
        SaveLifeAgentBusinessContextOutput? requestedContext)
    {
        return latest != null &&
            string.Equals(latest.Status, "active", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsSameContext(
        string? conversationContextType,
        long? conversationContextId,
        SaveLifeAgentBusinessContextOutput? requestedContext)
    {
        if (requestedContext == null)
            return !conversationContextId.HasValue && string.IsNullOrWhiteSpace(conversationContextType);

        if (!conversationContextId.HasValue)
            return false;

        return string.Equals(
                conversationContextType?.Trim(),
                requestedContext.ContextType?.Trim(),
                StringComparison.OrdinalIgnoreCase) &&
            long.TryParse(requestedContext.ContextId, out var requestedContextId) &&
            requestedContextId == conversationContextId.Value;
    }

    public static long? GetTurnEnrollmentRecordId(
        SaveLifeAgentBusinessContextOutput? requestedContext)
    {
        if (requestedContext == null ||
            !string.Equals(
                requestedContext.ContextType,
                "enrollment",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return long.TryParse(requestedContext.ContextId, out var enrollmentRecordId) &&
            enrollmentRecordId > 0
                ? enrollmentRecordId
                : null;
    }
}

public static class SaveLifeAgentTrialOptOutPolicy
{
    public static bool IsSuppressed(
        SaveLifeAgentConversationState state,
        long? patientProfileId)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (HasLegacyOnlyState(state))
            return state.TrialOptOut;

        return patientProfileId is > 0
            ? state.TrialOptOutPatientProfileIds.Contains(patientProfileId.Value)
            : state.GeneralTrialOptOut;
    }

    public static void MigrateLegacyScope(
        SaveLifeAgentConversationState state,
        long? patientProfileId)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (!HasLegacyOnlyState(state))
            return;

        SetScope(state, patientProfileId, suppressed: true);
    }

    public static void NormalizePatientScopes(SaveLifeAgentConversationState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.TrialOptOutPatientProfileIds = state.TrialOptOutPatientProfileIds
            .Where(id => id > 0)
            .Distinct()
            .Order()
            .ToList();
    }

    private static bool HasLegacyOnlyState(SaveLifeAgentConversationState state)
    {
        return state.TrialOptOut &&
               !state.GeneralTrialOptOut &&
               state.TrialOptOutPatientProfileIds.Count == 0;
    }

    private static void SetScope(
        SaveLifeAgentConversationState state,
        long? patientProfileId,
        bool suppressed)
    {
        if (patientProfileId is not > 0)
        {
            state.GeneralTrialOptOut = suppressed;
            return;
        }

        if (suppressed)
            state.TrialOptOutPatientProfileIds.Add(patientProfileId.Value);
        else
            state.TrialOptOutPatientProfileIds.RemoveAll(id => id == patientProfileId.Value);
        NormalizePatientScopes(state);
    }
}
