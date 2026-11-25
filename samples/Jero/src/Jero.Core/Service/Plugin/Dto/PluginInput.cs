namespace Jero.Core.Service;

public class PagePluginInput : BasePageInput
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
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}

public class AddPluginInput : SysPlugin
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "功能名称不能为空")]
    public override string Name { get; set; } = string.Empty;
}

public class UpdatePluginInput : AddPluginInput
{
}

public class DeletePluginInput : BaseIdInput
{
}