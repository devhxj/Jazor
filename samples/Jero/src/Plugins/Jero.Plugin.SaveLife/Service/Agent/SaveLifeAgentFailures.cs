namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentFailureContract
{
    public const string PublicMessage =
        "当前智能助手服务暂时不可用，您刚才发送的内容已保留，请稍后重试。";
}

public static class SaveLifeAgentFailureCodes
{
    public const string RetrievalUnavailable = "AGENT_RETRIEVAL_UNAVAILABLE";
    public const string VectorIncompatible = "AGENT_VECTOR_INCOMPATIBLE";
    public const string MaterialNotReady = "AGENT_MATERIAL_NOT_READY";
    public const string PatientContextChanged = "AGENT_PATIENT_CONTEXT_CHANGED";
    public const string TurnSuperseded = "AGENT_TURN_SUPERSEDED";
}

public static class SaveLifeAgentFailureStages
{
    public const string Retrieval = "retrieval";
    public const string Material = "material";
    public const string Persistence = "persistence";
}

public sealed class SaveLifeAgentUnavailableException : Exception
{
    public SaveLifeAgentUnavailableException(
        string code,
        string stage,
        Exception? innerException = null,
        bool retryable = true,
        SaveLifeAgentRetrievalDiagnostics? retrievalDiagnostics = null,
        string? publicMessage = null)
        : base(
            string.IsNullOrWhiteSpace(publicMessage)
                ? SaveLifeAgentFailureContract.PublicMessage
                : publicMessage.Trim(),
            innerException)
    {
        Code = code;
        Stage = stage;
        Retryable = retryable;
        RetrievalDiagnostics = retrievalDiagnostics;
    }

    public string Code { get; }

    public string Stage { get; }

    public bool Retryable { get; }

    public SaveLifeAgentRetrievalDiagnostics? RetrievalDiagnostics { get; }
}

public sealed class SaveLifeAgentProfileProposalValidationException(string message)
    : Exception(message);
