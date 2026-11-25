namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeEnrollmentRecordInput : BasePageInput
{
    public long? ProjectId { get; set; }

    /// <summary>
    /// 项目状态。
    /// </summary>
    public string? ProjectStatus { get; set; }

    /// <summary>
    /// 项目疾病领域。
    /// </summary>
    public string? DiseaseArea { get; set; }

    /// <summary>
    /// 项目临床阶段。
    /// </summary>
    public string? ClinicalPhase { get; set; }

    /// <summary>
    /// 报名审核状态。
    /// </summary>
    public string? AuditStatus { get; set; }

    /// <summary>
    /// 兼容既有调用的报名状态筛选。
    /// </summary>
    public string? Status { get; set; }
}

public class SaveLifeEnrollmentRecordBaseInput
{
    public virtual long? UserProfileId { get; set; }

    public virtual string? ExternalUserId { get; set; }

    [Required(ErrorMessage = "项目Id不能为空")]
    public virtual long ProjectId { get; set; }

    public virtual string? ProjectTitle { get; set; }

    public virtual string? ContactName { get; set; }

    public virtual string? ContactPhone { get; set; }

    public virtual string Status { get; set; } = "pending";

    public virtual DateTime? SubmittedTime { get; set; }

    public virtual DateTime? AuditTime { get; set; }

    public virtual string? AuditRemark { get; set; }

    public virtual string? DiseaseDescription { get; set; }

    public virtual string? MedicalHistory { get; set; }

    public virtual string? DiagnosisReport { get; set; }

    public virtual string? MedicationStatus { get; set; }

    public virtual string? OtherInfo { get; set; }

    public virtual string? Notes { get; set; }
}

public class AddSaveLifeEnrollmentRecordInput : SaveLifeEnrollmentRecordBaseInput
{
}

public class UpdateSaveLifeEnrollmentRecordInput : SaveLifeEnrollmentRecordBaseInput
{
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}

public class DeleteSaveLifeEnrollmentRecordInput : BaseIdInput
{
}

public class QueryByIdSaveLifeEnrollmentRecordInput : DeleteSaveLifeEnrollmentRecordInput
{
}

public class SaveLifeEnrollmentRecordDetailOutput
{
    public SaveLifeEnrollmentRecordOutput Record { get; set; } = default!;

    public List<SaveLifeEnrollmentMaterialOutput> Materials { get; set; } = new();
}

/// <summary>
/// 后台报名记录输出，附带提交时关联的用户和患者档案摘要。
/// </summary>
public class SaveLifeEnrollmentRecordOutput : SaveLifeEnrollmentRecord
{
    public string? ProjectCdeRegistrationNumber { get; set; }

    public string? ProjectDiseaseArea { get; set; }

    public string? ProjectClinicalPhase { get; set; }

    public SaveLifeEnrollmentUserProfileOutput? UserProfile { get; set; }

    public SaveLifeEnrollmentPatientProfileOutput? PatientProfile { get; set; }
}

public class SaveLifeEnrollmentUserProfileOutput
{
    public long Id { get; set; }

    public string? ExternalUserId { get; set; }

    public string? RealName { get; set; }

    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Ethnicity { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }
}

public class SaveLifeEnrollmentPatientProfileOutput
{
    public long Id { get; set; }

    public string? RelationshipToAccount { get; set; }

    public string? RelationshipDescription { get; set; }

    public string? DisplayName { get; set; }

    public bool IsDefault { get; set; }

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

public class AuditSaveLifeEnrollmentRecordInput : BaseIdInput
{
    [Required(ErrorMessage = "报名状态不能为空")]
    public string Status { get; set; } = "pending";

    public string? AuditRemark { get; set; }
}

public class ContactSaveLifeEnrollmentRecordInput : BaseIdInput
{
    public string? ContactRemark { get; set; }
}

public class RequestSupplementSaveLifeEnrollmentRecordInput : BaseIdInput
{
    [Required(ErrorMessage = "补充资料要求不能为空")]
    public string SupplementRequest { get; set; } = string.Empty;
}
