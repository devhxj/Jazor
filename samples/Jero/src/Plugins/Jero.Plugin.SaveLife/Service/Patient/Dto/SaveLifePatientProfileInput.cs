namespace Jero.Plugin.SaveLife.Service;

public class QuerySaveLifePatientProfilesInput
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public class CreateSaveLifePatientProfileInput
{
    [Required, MaxLength(32)]
    public string RelationshipToAccount { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string RelationshipDescription { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Ethnicity { get; set; }

    public string? City { get; set; }

    public string? Residence { get; set; }

    public string? Diagnosis { get; set; }

    public string? DiseaseStage { get; set; }

    public string? DiseaseStatus { get; set; }

    public string? TreatmentHistory { get; set; }

    public string? CurrentTreatment { get; set; }

    public string? Pathology { get; set; }

    public string? Biomarkers { get; set; }

    public string? Comorbidities { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }
}

public class UpdateSaveLifePatientProfileInput
{
    public string? RelationshipToAccount { get; set; }

    public string? RelationshipDescription { get; set; }

    public string? DisplayName { get; set; }

    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Ethnicity { get; set; }

    public string? City { get; set; }

    public string? Residence { get; set; }

    public string? Diagnosis { get; set; }

    public string? DiseaseStage { get; set; }

    public string? DiseaseStatus { get; set; }

    public string? TreatmentHistory { get; set; }

    public string? CurrentTreatment { get; set; }

    public string? Pathology { get; set; }

    public string? Biomarkers { get; set; }

    public string? Comorbidities { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }
}

public class SwitchSaveLifeCurrentPatientInput
{
    [Required, MaxLength(32)]
    public string PatientProfileId { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? PendingConsultationId { get; set; }
}

public class CreateAndSelectSaveLifePatientInput
{
    [Required, MaxLength(64)]
    public string ClientCommandId { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string RelationshipToAccount { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? PendingConsultationId { get; set; }
}

public class DeferSaveLifePatientOnboardingInput
{
    [Required, MaxLength(32)]
    public string PendingConsultationId { get; set; } = string.Empty;
}

public class SaveLifePatientOnboardingDeferralOutput
{
    public string PendingConsultationId { get; set; } = string.Empty;

    public bool IsDeferred { get; set; }
}

public class ConfirmSaveLifePatientProfileProposalInput
{
    [Required, MaxLength(32)]
    public string ConversationId { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string PatientProfileId { get; set; } = string.Empty;
}

public sealed class DecideSaveLifePatientProfileProposalInput
{
    [Required, MaxLength(16)]
    public string Decision { get; set; } = string.Empty;
}

public class SaveLifePatientProfileProposalOutput
{
    public string Id { get; set; } = string.Empty;

    public string ConversationId { get; set; } = string.Empty;

    public string PatientProfileId { get; set; } = string.Empty;

    public string SourceUserMessageId { get; set; } = string.Empty;

    public string SourceAssistantMessageId { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string? SourceMaterialId { get; set; }

    public string Field { get; set; } = string.Empty;

    public string FieldLabel { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string ProposedValue { get; set; } = string.Empty;

    public string Evidence { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? ResolvedAt { get; set; }
}

public class SaveLifePatientProfileProposalConfirmationOutput
{
    public SaveLifePatientProfileProposalOutput Proposal { get; set; } = new();

    public SaveLifePatientProfileOutput Patient { get; set; } = new();

    public SaveLifeAgentPendingQuestionOutput? PendingQuestion { get; set; }

    public List<SaveLifeAgentSuggestedActionOutput> SuggestedActions { get; set; } = [];
}

public class SaveLifePatientProfileOutput
{
    public string Id { get; set; } = string.Empty;

    public string RelationshipToAccount { get; set; } = string.Empty;

    public string RelationshipDescription { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsCurrent { get; set; }

    public int ProfileCompleteness { get; set; }

    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Ethnicity { get; set; }

    public string? City { get; set; }

    public string? Residence { get; set; }

    public string? Diagnosis { get; set; }

    public string? DiseaseStage { get; set; }

    public string? DiseaseStatus { get; set; }

    public string? TreatmentHistory { get; set; }

    public string? CurrentTreatment { get; set; }

    public string? Pathology { get; set; }

    public string? Biomarkers { get; set; }

    public string? Comorbidities { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class SaveLifePatientProfileListOutput
{
    public List<SaveLifePatientProfileOutput> Items { get; set; } = [];

    public int Total { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
