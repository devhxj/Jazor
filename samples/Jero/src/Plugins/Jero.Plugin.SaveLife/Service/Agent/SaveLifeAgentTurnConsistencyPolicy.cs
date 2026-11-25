namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentTurnConsistencyPolicy
{
    public const string PublicMessage =
        "当前咨询的患者范围已发生变化，您刚才发送的内容已保留，请确认当前患者后重试。";

    public static void EnsureCurrent(
        SaveLifeAgentConversation conversation,
        SaveLifeAgentMessage userMessage)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(userMessage);

        if (conversation.Id != userMessage.ConversationId ||
            !string.Equals(
                conversation.ExternalUserId,
                userMessage.ExternalUserId,
                StringComparison.Ordinal) ||
            conversation.CurrentPatientProfileId != userMessage.PatientProfileId ||
            conversation.PatientContextVersion <= 0 ||
            userMessage.PatientContextVersion <= 0 ||
            conversation.PatientContextVersion != userMessage.PatientContextVersion)
        {
            throw CreateException();
        }
    }

    internal static SaveLifeAgentUnavailableException CreateException()
    {
        return new SaveLifeAgentUnavailableException(
            SaveLifeAgentFailureCodes.PatientContextChanged,
            SaveLifeAgentFailureStages.Persistence,
            retryable: true,
            publicMessage: PublicMessage);
    }

    internal static SaveLifeAgentUnavailableException CreateTurnSupersededException()
    {
        return new SaveLifeAgentUnavailableException(
            SaveLifeAgentFailureCodes.TurnSuperseded,
            SaveLifeAgentFailureStages.Persistence,
            retryable: false,
            publicMessage: "该轮咨询已被后续状态取代，请作为新消息重新发送。");
    }
}
