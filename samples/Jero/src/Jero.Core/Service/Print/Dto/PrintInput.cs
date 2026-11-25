namespace Jero.Core.Service;

public class PagePrintInput : BasePageInput
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

public class AddPrintInput : SysPrint
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "模板名称不能为空")]
    public override string Name { get; set; } = string.Empty;
}

public class UpdatePrintInput : AddPrintInput
{
}

public class DeletePrintInput : BaseIdInput
{
}