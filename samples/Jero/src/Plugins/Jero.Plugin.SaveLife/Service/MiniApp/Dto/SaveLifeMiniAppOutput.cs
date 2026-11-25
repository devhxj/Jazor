namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeMiniAppClinicalTrialOutput
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? ProtocolNumber { get; set; }

    public string CdeRegistrationNumber { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Sponsor { get; set; }

    public string? ClinicalPhase { get; set; }

    public string? TrialPhase { get; set; }

    public string? DiseaseArea { get; set; }

    public string? Indication { get; set; }

    public string Status { get; set; } = "recruiting";

    public int? TargetCount { get; set; }

    public int? EnrolledCount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? ParticipantType { get; set; }

    public string? InclusionCriteria { get; set; }

    public string? ExclusionCriteria { get; set; }

    public string? TrialDuration { get; set; }

    public string? Benefits { get; set; }

    public string? EthicsCommitteeInfo { get; set; }

    public string? Sites { get; set; }

    public string? Location { get; set; }

    public string? Requirements { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? UpdateTime { get; set; }
}

public class SaveLifeMiniAppKnowledgeOutput
{
    public string Id { get; set; } = string.Empty;

    public string? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string ContentCategory { get; set; } = SaveLifeKnowledgeContentCategory.Science;

    public string Title { get; set; } = string.Empty;

    public string? Author { get; set; }

    public string? DiseaseName { get; set; }

    public string? Content { get; set; }

    public string? Summary { get; set; }

    public string? ImageUrl { get; set; }

    public string? Tags { get; set; }

    public int ViewCount { get; set; }

    public int LikeCount { get; set; }

    public bool IsLiked { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public List<SaveLifeMiniAppKnowledgeOutput> Related { get; set; } = new();
}

public class SaveLifeMiniAppKnowledgeCategoryOutput
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Icon { get; set; }
}

public class SaveLifeMiniAppKnowledgeReadHistoryOutput
{
    public string Id { get; set; } = string.Empty;

    public string KnowledgeId { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? DiseaseName { get; set; }

    public string? Summary { get; set; }

    public string? ImageUrl { get; set; }

    public int ReadCount { get; set; }

    public DateTime LastReadTime { get; set; }
}

public class SaveLifeMiniAppAnnouncementOutput
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string AnnouncementType { get; set; } = string.Empty;

    public string? BusinessType { get; set; }

    public string? BusinessId { get; set; }

    public DateTime CreateTime { get; set; }
}

public class SaveLifeMiniAppHomeOutput
{
    public List<SaveLifeMiniAppClinicalTrialOutput> RecommendedTrials { get; set; } = new();

    public List<SaveLifeMiniAppKnowledgeOutput> RecommendedKnowledge { get; set; } = new();

    public List<SaveLifeMiniAppAnnouncementOutput> Announcements { get; set; } = new();

    public SaveLifeMiniAppHomeStatsOutput Stats { get; set; } = new();
}

public class SaveLifeMiniAppHomeStatsOutput
{
    public int TrialCount { get; set; }

    public int KnowledgeCount { get; set; }

    public int AnnouncementCount { get; set; }

    public int CategoryCount { get; set; }
}

public class SaveLifeMiniAppTrialFilterOptionsOutput
{
    public List<SaveLifeMiniAppTrialFilterOptionItem> DiseaseAreas { get; set; } = new();

    public List<SaveLifeMiniAppTrialFilterOptionItem> ClinicalPhases { get; set; } = new();

    public List<SaveLifeMiniAppTrialFilterOptionItem> Statuses { get; set; } = new();
}

public class SaveLifeMiniAppTrialFilterOptionItem
{
    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

public class SaveLifeMiniAppEnrollmentOutput
{
    public string Id { get; set; } = string.Empty;

    public string? ExternalUserId { get; set; }

    public string? PatientProfileId { get; set; }

    public string ProjectId { get; set; } = string.Empty;

    public string? ProjectTitle { get; set; }

    public string? InstitutionName { get; set; }

    public string? ContactName { get; set; }

    public string? ContactPhone { get; set; }

    public string Status { get; set; } = "pending";

    public DateTime? SubmittedTime { get; set; }

    public DateTime? AuditTime { get; set; }

    public string? AuditRemark { get; set; }

    public string? DiseaseDescription { get; set; }

    public string? MedicalHistory { get; set; }

    public string? DiagnosisReport { get; set; }

    public string? MedicationStatus { get; set; }

    public string? OtherInfo { get; set; }

    public string? Notes { get; set; }

    public int ProfileCompleteness { get; set; }

    public bool HasRequiredProfile { get; set; }

    public List<string> MissingProfileFields { get; set; } = new();

    public int MaterialCount { get; set; }

    public bool HasRequiredMaterials { get; set; }

    public List<SaveLifeMiniAppMaterialRequirementOutput> MaterialRequirements { get; set; } = new();

    public List<SaveLifeEnrollmentMaterialOutput>? Materials { get; set; }

    public string? NextStepHint { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? UpdateTime { get; set; }
}

public class SaveLifeMiniAppEnrollmentStatusCountsOutput
{
    public int Total { get; set; }

    public int Pending { get; set; }

    public int Approved { get; set; }

    public int Contacted { get; set; }

    public int Rejected { get; set; }

    public int NeedSupplement { get; set; }

    public int Draft { get; set; }

    public int Cancelled { get; set; }

    public int Closed { get; set; }
}

public class SaveLifeMiniAppMaterialRequirementOutput
{
    public string MaterialType { get; set; } = string.Empty;

    public string MaterialTypeText { get; set; } = string.Empty;

    public bool Required { get; set; }

    public bool Uploaded { get; set; }
}

public class SubmitSaveLifeMiniAppEnrollmentInput
{
    public string? UserId { get; set; }

    public long? PatientProfileId { get; set; }

    [Required(ErrorMessage = "项目Id不能为空")]
    public long ProjectId { get; set; }

    public string? ContactName { get; set; }

    public string? ContactPhone { get; set; }

    public string? DiseaseDescription { get; set; }

    public string? MedicalHistory { get; set; }

    public string? DiagnosisReport { get; set; }

    public string? MedicationStatus { get; set; }

    public string? OtherInfo { get; set; }

    public string? Notes { get; set; }
}

public class CancelSaveLifeMiniAppEnrollmentInput
{
    public string? UserId { get; set; }
}
