namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeDiseaseCategoryInput : BasePageInput
{
    public StatusEnum? Status { get; set; }
}

public class SaveLifeDiseaseCategoryBaseInput
{
    [Required(ErrorMessage = "分类名称不能为空")]
    public virtual string Name { get; set; } = string.Empty;

    public virtual string? Description { get; set; }

    public virtual string? Icon { get; set; }

    public virtual int SortOrder { get; set; } = 100;

    public virtual StatusEnum Status { get; set; } = StatusEnum.Enable;
}

public class AddSaveLifeDiseaseCategoryInput : SaveLifeDiseaseCategoryBaseInput
{
}

public class UpdateSaveLifeDiseaseCategoryInput : SaveLifeDiseaseCategoryBaseInput
{
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}

public class DeleteSaveLifeDiseaseCategoryInput : BaseIdInput
{
}

public class QueryByIdSaveLifeDiseaseCategoryInput : DeleteSaveLifeDiseaseCategoryInput
{
}
