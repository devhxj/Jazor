namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeAnnouncementInput : BasePageInput
{
    public bool? IsPublished { get; set; }

    public string? AnnouncementType { get; set; }
}

public class SaveLifeAnnouncementBaseInput
{
    [Required(ErrorMessage = "公告标题不能为空")]
    public virtual string Title { get; set; } = string.Empty;

    public virtual string? Content { get; set; }

    public virtual string AnnouncementType { get; set; } = "system";

    public virtual string? BusinessType { get; set; }

    public virtual long? BusinessId { get; set; }

    public virtual bool IsPublished { get; set; } = true;

    public virtual int SortOrder { get; set; } = 100;
}

public class AddSaveLifeAnnouncementInput : SaveLifeAnnouncementBaseInput
{
}

public class UpdateSaveLifeAnnouncementInput : SaveLifeAnnouncementBaseInput
{
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}

public class DeleteSaveLifeAnnouncementInput : BaseIdInput
{
}

public class QueryByIdSaveLifeAnnouncementInput : DeleteSaveLifeAnnouncementInput
{
}
