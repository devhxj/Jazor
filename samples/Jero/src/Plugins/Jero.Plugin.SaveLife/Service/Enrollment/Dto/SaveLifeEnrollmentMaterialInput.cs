namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeEnrollmentMaterialInput : BasePageInput
{
    public long? EnrollmentRecordId { get; set; }

    public string? ExternalUserId { get; set; }

    public string? MaterialType { get; set; }

    public string? Status { get; set; }

    public string? ExtractStatus { get; set; }
}

public class UploadSaveLifeMiniAppEnrollmentMaterialInput
{
    [Required(ErrorMessage = "客户端命令Id不能为空")]
    [MaxLength(128, ErrorMessage = "客户端命令Id不能超过128个字符")]
    public string ClientCommandId { get; set; } = string.Empty;

    [Required(ErrorMessage = "材料文件不能为空")]
    public IFormFile File { get; set; } = default!;

    [MaxLength(64, ErrorMessage = "材料类型不能超过64个字符")]
    public string? MaterialType { get; set; }

    [MaxLength(512, ErrorMessage = "材料备注不能超过512个字符")]
    public string? Remark { get; set; }

    [MaxLength(255, ErrorMessage = "原始文件名不能超过255个字符")]
    public string? OriginalFileName { get; set; }

    [MaxLength(32, ErrorMessage = "患者档案Id不能超过32个字符")]
    public string? PatientProfileId { get; set; }
}

public class AuditSaveLifeEnrollmentMaterialInput : BaseIdInput
{
    [Required(ErrorMessage = "材料状态不能为空")]
    public string Status { get; set; } = "submitted";

    public string? ReviewRemark { get; set; }

    public string? SupplementRequest { get; set; }
}

public class SaveLifeEnrollmentMaterialOutput
{
    public string Id { get; set; } = string.Empty;

    public string? PatientProfileId { get; set; }

    public string EnrollmentRecordId { get; set; } = string.Empty;

    public bool IsPersonalMaterial { get; set; }

    public string? ExternalUserId { get; set; }

    public string ProjectId { get; set; } = string.Empty;

    public string MaterialType { get; set; } = "other";

    public string MaterialTypeText { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? FileSuffix { get; set; }

    public string? ContentType { get; set; }

    public long SizeKb { get; set; }

    public string? SizeInfo { get; set; }

    public string Status { get; set; } = "submitted";

    public string StatusText { get; set; } = string.Empty;

    public string? Remark { get; set; }

    public string? ReviewRemark { get; set; }

    public string? SupplementRequest { get; set; }

    public string ExtractStatus { get; set; } = "pending";

    public string? ExtractStatusText { get; set; }

    public string? ExtractError { get; set; }

    public DateTime? ExtractedTime { get; set; }

    public string? ExtractModel { get; set; }

    public string IndexStatus { get; set; } = "pending";

    public string? IndexStatusText { get; set; }

    public string? IndexError { get; set; }

    public DateTime? IndexedTime { get; set; }

    public int ProcessRetryCount { get; set; }

    public DateTime? NextProcessRetryTime { get; set; }

    public DateTime? ReviewedTime { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public bool CanDelete { get; set; }
}

public class RetrySaveLifeEnrollmentMaterialExtractInput : BaseIdInput
{
}
