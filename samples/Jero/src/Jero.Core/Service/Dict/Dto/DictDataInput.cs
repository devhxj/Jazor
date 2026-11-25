namespace Jero.Core.Service;

public class DictDataInput : BaseStatusInput
{
}

public class PageDictDataInput : BasePageInput
{
    /// <summary>
    /// 字典类型Id
    /// </summary>
    public long DictTypeId { get; set; }

    /// <summary>
    /// 文本
    /// </summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
}

public class AddDictDataInput : SysDictData
{
}

public class UpdateDictDataInput : AddDictDataInput
{
}

public class DeleteDictDataInput : BaseIdInput
{
}

public class GetDataDictDataInput
{
    /// <summary>
    /// 字典类型Id
    /// </summary>
    [Required(ErrorMessage = "字典类型Id不能为空"), DataValidation(ValidationTypes.Numeric)]
    public long DictTypeId { get; set; }
}

public class QueryDictDataInput
{
    /// <summary>
    /// 字典值
    /// </summary>
    [Required(ErrorMessage = "字典值不能为空")]
    public string Value { get; set; } = string.Empty;
    /// <summary>
    /// 状态
    /// </summary>
    public int? Status { get; set; }
}