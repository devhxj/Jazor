namespace Jero.Core.Service;

public class DictTypeInput : BaseStatusInput
{
}

public class PageDictTypeInput : BasePageInput
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
}

public class AddDictTypeInput : SysDictType
{
    /// <summary>
    /// 是否是租户字典（Y-是，N-否）
    /// </summary>
    public override YesNoEnum IsTenant { get; set; } = YesNoEnum.Y;

    /// <summary>
    /// 是否是内置字典（Y-是，N-否）
    /// </summary>
    public override YesNoEnum SysFlag { get; set; } = YesNoEnum.N;
}

public class UpdateDictTypeInput : AddDictTypeInput
{
}

public class DeleteDictTypeInput : BaseIdInput
{
}

public class GetDataDictTypeInput
{
    /// <summary>
    /// 编码
    /// </summary>
    [Required(ErrorMessage = "字典类型编码不能为空")]
    public string Code { get; set; } = string.Empty;
}