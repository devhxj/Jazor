namespace Jero.Plugin.SaveLife.Service;

public class CreateSaveLifeAgentConversationInput
{
    public string? ConversationId { get; set; }

    public string? UserId { get; set; }

    public string? ContextType { get; set; }

    public long? ContextId { get; set; }

    public long? TrialId { get; set; }

    public long? KnowledgeId { get; set; }

    public long? EnrollmentId { get; set; }
}

public class QuerySaveLifeAgentConversationInput
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public class QuerySaveLifeAgentMessagePageInput
{
    public string? BeforeMessageId { get; set; }

    public int PageSize { get; set; } = 20;
}

public class SubmitSaveLifeAgentIntentionInput
{
    [Required(ErrorMessage = "会话Id不能为空")]
    public string ConversationId { get; set; } = string.Empty;

    [Required(ErrorMessage = "客户端命令Id不能为空")]
    public string ClientCommandId { get; set; } = string.Empty;

    [Range(1, long.MaxValue, ErrorMessage = "会话状态修订号无效")]
    public long StateRevision { get; set; }

    public string? UserId { get; set; }

    [Required(ErrorMessage = "意向不能为空")]
    public string Intention { get; set; } = string.Empty;

    public string? ContextType { get; set; }

    public long? ContextId { get; set; }

    public long? TrialId { get; set; }

    [Required(ErrorMessage = "患者档案Id不能为空")]
    public long? PatientProfileId { get; set; }

    [MaxLength(64, ErrorMessage = "联系人姓名不能超过64个字符")]
    public string? ContactName { get; set; }

    [MaxLength(32, ErrorMessage = "联系电话不能超过32个字符")]
    public string? ContactPhone { get; set; }
}

public class SaveLifeAgentConversationOutput
{
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Phase { get; set; }

    public string? LastMessage { get; set; }

    public string? ContextType { get; set; }

    public string? ContextId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public sealed class SaveLifeAgentClearConversationsOutput
{
    public int ClearedConversationCount { get; set; }

    public int ClearedMessageCount { get; set; }
}

public class SaveLifeAgentMessageOutput
{
    public string Id { get; set; } = string.Empty;

    public string ConversationId { get; set; } = string.Empty;

    public string? PatientProfileId { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? ClientMessageId { get; set; }

    public string? ReplyToMessageId { get; set; }

    public string? GenerationStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<SaveLifeAgentMatchedTrialOutput> MatchedTrials { get; set; } = new();

    public List<SaveLifeAgentKnowledgeOutput> MatchedKnowledge { get; set; } = new();

    public List<SaveLifeAgentMaterialOutput> MatchedMaterials { get; set; } = new();

    public List<SaveLifePatientProfileProposalOutput> ProfileProposals { get; set; } = new();

    public SaveLifeAgentBusinessContextOutput? BusinessContext { get; set; }

    public List<SaveLifeAgentSuggestedActionOutput> SuggestedActions { get; set; } = new();

    public string? GuidanceText { get; set; }

    public List<string> SuggestedFollowUps { get; set; } = new();

    public string? ModelProvider { get; set; }

    public string? UsedModel { get; set; }

    public string? RunStatus { get; set; }

    public string? FallbackReason { get; set; }

    public string? ErrorSummary { get; set; }

    public SaveLifeAgentRetrievalDiagnostics? RetrievalDiagnostics { get; set; }

    public SaveLifeAgentRetrievalHitCountsOutput RetrievalHitCounts { get; set; } = new();

    public SaveLifeAgentStageTimingsOutput StageTimings { get; set; } = new();
}

public class SaveLifeAgentMessagePageOutput
{
    public List<SaveLifeAgentMessageOutput> Items { get; set; } = [];

    public string? NextCursor { get; set; }

    public bool HasMore { get; set; }
}

public class SaveLifeAgentConversationStateOutput
{
    public int Version { get; set; }

    public SaveLifeAgentPendingQuestionOutput? PendingQuestion { get; set; }

    public SaveLifeAgentPendingConsultationOutput? PendingConsultation { get; set; }

    public bool IsGeneralGuidanceDeferred { get; set; }

    public bool GeneralTrialOptOut { get; set; }

    public List<string> TrialOptOutPatientProfileIds { get; set; } = [];

    public bool TrialOptOut { get; set; }

    public string? SelectedTrialId { get; set; }

    public string? PendingPatientSwitchProfileId { get; set; }

    public SaveLifePatientProfileOutput? CurrentPatient { get; set; }
}

public class SaveLifeAgentPendingQuestionOutput
{
    public string Key { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;
}

public class SaveLifeAgentPendingConsultationOutput
{
    public string Id { get; set; } = string.Empty;

    public string SourceUserMessageId { get; set; } = string.Empty;

    public string GoalType { get; set; } = string.Empty;

    public string SubjectHint { get; set; } = string.Empty;

    public DateTime CreatedTime { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class SaveLifeAgentBusinessContextOutput
{
    public string ContextType { get; set; } = string.Empty;

    public string ContextId { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Summary { get; set; }
}

public class SaveLifeAgentMatchedTrialOutput
{
    public string Id { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? DiseaseArea { get; set; }

    public string? Indication { get; set; }

    public string? ClinicalPhase { get; set; }

    public string? Sponsor { get; set; }

    public string? Location { get; set; }

    public string? Status { get; set; }

    public string? Requirements { get; set; }

    public string? Description { get; set; }

    public string? Benefits { get; set; }

    public string? ResultLevel { get; set; }

    public bool IsPinned { get; set; }

    public double? Score { get; set; }
}

public static class SaveLifeAgentTrialResultLevels
{
    public const string Discovery = "discovery";
    public const string InitialMatch = "initial_match";
}

public class SaveLifeAgentKnowledgeOutput
{
    public string Id { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? DiseaseName { get; set; }

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public string? Tags { get; set; }

    public double? Score { get; set; }
}

public class SaveLifeAgentMaterialOutput
{
    public string Id { get; set; } = string.Empty;

    public string MaterialId { get; set; } = string.Empty;

    public string? EnrollmentRecordId { get; set; }

    public bool IsPersonalMaterial { get; set; }

    public string? Title { get; set; }

    public string? MaterialType { get; set; }

    public string? MaterialTypeText { get; set; }

    public string? FileName { get; set; }

    public string? Status { get; set; }

    public string? StatusText { get; set; }

    public bool CanSatisfyRequiredMaterial { get; set; }

    public bool RequiresSupplement { get; set; }

    public string? Summary { get; set; }

    public double? Score { get; set; }
}

public class SaveLifeAgentIntentionOutput
{
    public string ConversationId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? EnrollmentRecordId { get; set; }

    public string Content { get; set; } = string.Empty;

    public List<SaveLifeAgentSuggestedActionOutput> SuggestedActions { get; set; } = new();
}

public class SaveLifeAgentSuggestedActionOutput
{
    public string Type { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string? TargetType { get; set; }

    public string? TargetId { get; set; }

    public Dictionary<string, string> Params { get; set; } = new();
}

public sealed class SaveLifeAgentClarificationOptionOutput
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public class SaveLifeAgentPatientOnboardingOutput
{
    public string PendingConsultationId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public List<SaveLifeAgentSuggestedActionOutput> AllowedActions { get; set; } = [];
}

public static class SaveLifeAgentPatientOnboardingStatuses
{
    public const string AwaitingPatientScope = "awaiting_patient_scope";
}

public class SaveLifeAgentRetrievalHitCountsOutput
{
    public int Knowledge { get; set; }

    public int Trials { get; set; }

    public int Materials { get; set; }
}

public class SaveLifeAgentStageTimingsOutput
{
    public long ContextMs { get; set; }

    public long HistoryMs { get; set; }

    public long RoutingMs { get; set; }

    public long RetrievalMs { get; set; }

    public long UserContextMs { get; set; }

    public long ModelMs { get; set; }

    public long PersistMs { get; set; }

    public long TotalMs { get; set; }
}

internal class SaveLifeAgentMessageMetadata
{
    public string? Phase { get; set; }

    public bool ShowIntentionOptions { get; set; }

    public List<SaveLifeAgentMatchedTrialOutput> MatchedTrials { get; set; } = new();

    public List<SaveLifeAgentKnowledgeOutput> MatchedKnowledge { get; set; } = new();

    public List<SaveLifeAgentMaterialOutput> MatchedMaterials { get; set; } = new();

    public SaveLifeAgentBusinessContextOutput? BusinessContext { get; set; }

    public List<SaveLifeAgentSuggestedActionOutput> SuggestedActions { get; set; } = new();

    public string? GuidanceText { get; set; }

    public List<SaveLifeAgentClarificationOptionOutput> ClarificationOptions { get; set; } = new();

    public List<SaveLifePatientProfileProposalOutput> ProfileProposals { get; set; } = new();

    public SaveLifeAgentPatientOnboardingOutput? PatientOnboarding { get; set; }

    public List<string> SuggestedFollowUps { get; set; } = new();

    public string? ModelProvider { get; set; }

    public string? UsedModel { get; set; }

    public string? RunStatus { get; set; }

    public string? FallbackReason { get; set; }

    public string? ErrorSummary { get; set; }

    public string? FailureCode { get; set; }

    public string? FailureStage { get; set; }

    public bool? FailureRetryable { get; set; }

    public string? FailureTraceId { get; set; }

    public SaveLifeAgentRetrievalDiagnostics? RetrievalDiagnostics { get; set; }

    public SaveLifeAgentRetrievalHitCountsOutput RetrievalHitCounts { get; set; } = new();

    public SaveLifeAgentStageTimingsOutput StageTimings { get; set; } = new();
}
