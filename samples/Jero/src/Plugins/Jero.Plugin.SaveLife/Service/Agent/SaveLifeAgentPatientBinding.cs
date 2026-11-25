namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentPatientBinding
{
    public static void InitializeConversation(
        SaveLifeAgentConversation conversation,
        SaveLifePatientProfile? preferredPatient)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        conversation.FlowStateVersion ??= SaveLifeAgentConversationState.CurrentVersion;
        conversation.PatientContextVersion = Math.Max(1, conversation.PatientContextVersion);
        if (conversation.CurrentPatientProfileId.HasValue || preferredPatient == null)
            return;

        if (preferredPatient.Id <= 0 ||
            preferredPatient.IsDelete ||
            !string.Equals(
                preferredPatient.OwnerExternalUserId,
                conversation.ExternalUserId,
                StringComparison.Ordinal))
        {
            return;
        }

        conversation.CurrentPatientProfileId = preferredPatient.Id;
    }

    public static void BindNewMessage(
        SaveLifeAgentConversation conversation,
        SaveLifeAgentMessage message)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(message);

        if (message.ConversationId != conversation.Id ||
            message.ExternalUserId != conversation.ExternalUserId)
        {
            throw new InvalidOperationException("消息与会话归属不一致");
        }

        conversation.PatientContextVersion = Math.Max(1, conversation.PatientContextVersion);
        message.PatientProfileId = conversation.CurrentPatientProfileId;
        message.PatientContextVersion = conversation.PatientContextVersion;
    }

    public static void BindReply(
        SaveLifeAgentMessage userMessage,
        SaveLifeAgentMessage assistantMessage)
    {
        ArgumentNullException.ThrowIfNull(userMessage);
        ArgumentNullException.ThrowIfNull(assistantMessage);

        if (assistantMessage.ConversationId != userMessage.ConversationId ||
            assistantMessage.ExternalUserId != userMessage.ExternalUserId)
        {
            throw new InvalidOperationException("回复与用户消息归属不一致");
        }

        assistantMessage.PatientProfileId = userMessage.PatientProfileId;
        assistantMessage.PatientContextVersion = Math.Max(1, userMessage.PatientContextVersion);
        assistantMessage.ContextSourceMessageId = userMessage.ContextSourceMessageId;
    }
}
