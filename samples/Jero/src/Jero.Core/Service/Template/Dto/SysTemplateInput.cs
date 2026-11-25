namespace Jero.Core.Service;

public class PageTemplateInput : BasePageInput
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// 分组名称
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    /// <summary>
    /// 模板类型
    /// </summary>
    public TemplateTypeEnum? Type { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}

/// <summary>
/// 新增模板输入参数
/// </summary>
public class AddTemplateInput : SysTemplate
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public override string Name { get; set; } = string.Empty;
    /// <summary>
    /// 模板类型
    /// </summary>
    [Enum]
    public override TemplateTypeEnum Type { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    [Required(ErrorMessage = "编码不能为空")]
    public override string Code { get; set; } = string.Empty;
    /// <summary>
    /// 分组名称
    /// </summary>
    [Required(ErrorMessage = "分组名称不能为空")]
    public override string GroupName { get; set; } = string.Empty;
    /// <summary>
    /// 模板内容
    /// </summary>
    [Required(ErrorMessage = "内容名称不能为空")]
    public override string Content { get; set; } = string.Empty;
}

/// <summary>
/// 更新模板输入参数
/// </summary>
public class UpdateTemplateInput : AddTemplateInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    [DataValidation(ValidationTypes.Numeric)]
    public override long Id { get; set; }
}

/// <summary>
/// 预览模板输入参数
/// </summary>
public class ProViewTemplateInput : BaseIdInput
{
    /// <summary>
    /// 渲染参数
    /// </summary>
    [Required(ErrorMessage = "渲染参数不能为空")]
    public object? Data { get; set; }
}

/// <summary>
/// 模板渲染输入参数
/// </summary>
public class RenderTemplateInput
{
    /// <summary>
    /// 模板内容
    /// </summary>
    [Required(ErrorMessage = "内容名称不能为空")]
    public string Content { get; set; } = string.Empty;
    /// <summary>
    /// 渲染参数
    /// </summary>
    [Required(ErrorMessage = "渲染参数不能为空")]
    public object? Data { get; set; }
}
