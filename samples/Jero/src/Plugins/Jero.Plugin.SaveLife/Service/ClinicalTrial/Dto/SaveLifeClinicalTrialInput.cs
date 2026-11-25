namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeClinicalTrialInput : BasePageInput
{
    public string? Status { get; set; }

    public string? DiseaseArea { get; set; }

    public string? ClinicalPhase { get; set; }

    public string? Sponsor { get; set; }
}

public class SaveLifeClinicalTrialListOutput
{
    public long Id { get; set; }

    public string? Title { get; set; }

    public string? Sponsor { get; set; }

    public string? ClinicalPhase { get; set; }

    public string? DiseaseArea { get; set; }

    public string? Indication { get; set; }

    public string? Status { get; set; }

    public bool IsPinned { get; set; }

    public int SortOrder { get; set; }
}

public class SaveLifeClinicalTrialBaseInput
{
    [Required(ErrorMessage = "试验题目不能为空")]
    public virtual string Title { get; set; } = string.Empty;

    public virtual string? ProtocolNumber { get; set; }

    [Required(ErrorMessage = "CDE 登记号不能为空")]
    [MaxLength(64, ErrorMessage = "CDE 登记号不能超过 64 个字符")]
    public virtual string CdeRegistrationNumber { get; set; } = string.Empty;

    public virtual string? Description { get; set; }

    public virtual string? Sponsor { get; set; }

    public virtual string? ClinicalPhase { get; set; }

    public virtual string? DiseaseArea { get; set; }

    public virtual string? Indication { get; set; }

    public virtual string Status { get; set; } = "recruiting";

    public virtual int? TargetCount { get; set; }

    public virtual int? EnrolledCount { get; set; }

    public virtual DateTime? StartDate { get; set; }

    public virtual DateTime? EndDate { get; set; }

    public virtual string? ParticipantType { get; set; }

    public virtual string? InclusionCriteria { get; set; }

    public virtual string? ExclusionCriteria { get; set; }

    public virtual string? TrialDuration { get; set; }

    public virtual string? Benefits { get; set; }

    public virtual string? EthicsCommitteeInfo { get; set; }

    public virtual string? Sites { get; set; }

    public virtual string? Location { get; set; }

    public virtual string? Requirements { get; set; }

    public virtual int SortOrder { get; set; } = 100;
}

public class AddSaveLifeClinicalTrialInput : SaveLifeClinicalTrialBaseInput
{
}

public class UpdateSaveLifeClinicalTrialInput : SaveLifeClinicalTrialBaseInput
{
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}

public class DeleteSaveLifeClinicalTrialInput : BaseIdInput
{
}

public class PinSaveLifeClinicalTrialInput : BaseIdInput
{
    public bool IsPinned { get; set; }
}

public class TestSaveLifeClinicalTrialKnowledgeBaseInput
{
    [Required(ErrorMessage = "测试关键词不能为空")]
    [MaxLength(200, ErrorMessage = "测试关键词不能超过 200 个字符")]
    public string Keyword { get; set; } = string.Empty;
}

public class SaveLifeClinicalTrialKnowledgeBaseTestOutput
{
    public string Keyword { get; set; } = string.Empty;

    public int CandidateCount { get; set; }

    public int HitCount { get; set; }

    public string? RetrievalStatus { get; set; }

    public string? TextEmbeddingStatus { get; set; }

    public string? ErrorSummary { get; set; }

    public List<SaveLifeClinicalTrialKnowledgeBaseTestProjectOutput> Projects { get; set; } = new();
}

public class SaveLifeClinicalTrialKnowledgeBaseTestProjectOutput
{
    public long Id { get; set; }

    public string? CdeRegistrationNumber { get; set; }

    public string? Title { get; set; }

    public string? DiseaseArea { get; set; }

    public string? ClinicalPhase { get; set; }

    public string? Status { get; set; }

    public bool IsInKnowledgeBase { get; set; }

    public double Score { get; set; }

    public string? Description { get; set; }

    public string? Benefits { get; set; }
}

public class QueryByIdSaveLifeClinicalTrialInput : DeleteSaveLifeClinicalTrialInput
{
}
