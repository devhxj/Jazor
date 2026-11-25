namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentHistoryPolicy
{
    public static List<SaveLifeAgentMessage> SelectModelHistory(
        IReadOnlyList<SaveLifeAgentMessage> messages,
        long? patientProfileId,
        long currentUserMessageId,
        int maxRounds)
    {
        var users = messages
            .Where(u =>
                u.Id < currentUserMessageId &&
                u.PatientProfileId == patientProfileId &&
                string.Equals(u.Role, "user", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(u.Content))
            .GroupBy(u => u.Id)
            .ToDictionary(u => u.Key, u => u.First());

        var roundLimit = Math.Clamp(
            maxRounds,
            1,
            SaveLifeAgentOptions.MaximumHistoryRounds);
        var rounds = messages
            .Where(u =>
                u.PatientProfileId == patientProfileId &&
                string.Equals(u.Role, "assistant", StringComparison.OrdinalIgnoreCase) &&
                u.ReplyToMessageId.HasValue &&
                (string.IsNullOrWhiteSpace(u.GenerationStatus) ||
                 string.Equals(u.GenerationStatus, "completed", StringComparison.OrdinalIgnoreCase)) &&
                !string.IsNullOrWhiteSpace(u.Content))
            .Select(assistant => new
            {
                Assistant = assistant,
                User = users.GetValueOrDefault(assistant.ReplyToMessageId!.Value)
            })
            .Where(round =>
                round.User != null &&
                round.User.ConversationId == round.Assistant.ConversationId &&
                string.Equals(
                    round.User.ExternalUserId,
                    round.Assistant.ExternalUserId,
                    StringComparison.Ordinal))
            .OrderByDescending(round => round.User!.CreateTime)
            .ThenByDescending(round => round.User!.Id)
            .Take(roundLimit)
            .OrderBy(round => round.User!.CreateTime)
            .ThenBy(round => round.User!.Id)
            .SelectMany(round => new[] { round.User!, round.Assistant })
            .ToList();

        return rounds;
    }

    public static SaveLifeAgentConversationStateOutput BuildConversationState(
        SaveLifeAgentConversation conversation,
        SaveLifePatientProfile? currentPatient)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        var state = SaveLifeAgentConversationStateCodec.Deserialize(conversation.FlowStateJson);
        var confirmedPatient = currentPatient != null &&
            conversation.CurrentPatientProfileId == currentPatient.Id &&
            !currentPatient.IsDelete &&
            string.Equals(
                conversation.ExternalUserId,
                currentPatient.OwnerExternalUserId,
                StringComparison.Ordinal)
                ? SaveLifePatientProfileMapper.ToOutput(
                    currentPatient,
                    conversation.CurrentPatientProfileId)
                : null;
        SaveLifeAgentTrialOptOutPolicy.NormalizePatientScopes(state);
        var effectiveTrialOptOut = SaveLifeAgentTrialOptOutPolicy.IsSuppressed(
            state,
            confirmedPatient == null ? null : currentPatient!.Id);

        return new SaveLifeAgentConversationStateOutput
        {
            CurrentPatient = confirmedPatient,
            GeneralTrialOptOut = state.GeneralTrialOptOut,
            IsGeneralGuidanceDeferred = state.IsGeneralGuidanceDeferred,
            PendingConsultation = state.PendingConsultation == null
                ? null
                : new SaveLifeAgentPendingConsultationOutput
                {
                    CreatedTime = state.PendingConsultation.CreatedTime,
                    GoalType = state.PendingConsultation.GoalType,
                    Id = state.PendingConsultation.Id.ToString(),
                    SourceUserMessageId = state.PendingConsultation.SourceUserMessageId.ToString(),
                    Status = state.PendingConsultation.Status,
                    SubjectHint = state.PendingConsultation.SubjectHint
                },
            PendingPatientSwitchProfileId = state.PendingPatientSwitchProfileId?.ToString(),
            PendingQuestion = state.PendingQuestion == null
                ? null
                : new SaveLifeAgentPendingQuestionOutput
                {
                    Key = state.PendingQuestion.Key,
                    Prompt = state.PendingQuestion.Prompt
                },
            SelectedTrialId = state.SelectedTrialId?.ToString(),
            TrialOptOutPatientProfileIds = state.TrialOptOutPatientProfileIds
                .Select(u => u.ToString())
                .ToList(),
            TrialOptOut = effectiveTrialOptOut,
            Version = state.Version
        };
    }
}
