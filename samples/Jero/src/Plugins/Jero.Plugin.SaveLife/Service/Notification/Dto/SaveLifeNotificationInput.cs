namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeNotificationInput : BasePageInput
{
    public string? ExternalUserId { get; set; }

    public string? NotificationType { get; set; }

    public string? BusinessType { get; set; }

    public bool? IsRead { get; set; }
}

public class AddSaveLifeNotificationInput
{
    [Required(ErrorMessage = "外部用户Id不能为空")]
    public string ExternalUserId { get; set; } = string.Empty;

    public string NotificationType { get; set; } = "system";

    [Required(ErrorMessage = "标题不能为空")]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? BusinessType { get; set; }

    public long? BusinessId { get; set; }
}

public class DeleteSaveLifeNotificationInput : BaseIdInput
{
}

public class QueryByIdSaveLifeNotificationInput : DeleteSaveLifeNotificationInput
{
}

public class SaveLifeMiniAppNotificationOutput
{
    public string Id { get; set; } = string.Empty;

    public string NotificationType { get; set; } = "system";

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? BusinessType { get; set; }

    public string? BusinessId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadTime { get; set; }

    public DateTime CreateTime { get; set; }
}
