namespace Jero.Core.Service;

public class TenantInput : BaseIdInput
{
    /// <summary>
    /// 状态
    /// </summary>
    public StatusEnum Status { get; set; }
}

public class PageTenantInput : BasePageInput
{
    /// <summary>
    /// 名称
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;
    /// <summary>
    /// 电话
    /// </summary>
    public virtual string Phone { get; set; } = string.Empty;
}

public class AddTenantInput : TenantOutput
{
    /// <summary>
    /// 租户名称
    /// </summary>
    [Required(ErrorMessage = "租户名称不能为空"), MinLength(2, ErrorMessage = "租户名称不能少于2个字符")]
    public override string Name { get; set; } = string.Empty;
    /// <summary>
    /// 租管账号
    /// </summary>
    [Required(ErrorMessage = "租管账号不能为空"), MinLength(3, ErrorMessage = "租管账号不能少于3个字符")]
    public override string AdminAccount { get; set; } = string.Empty;
    /// <summary>
    /// 系统主标题
    /// </summary>
    [CommonValidation("!string.IsNullOrWhiteSpace(Host) && string.IsNullOrWhiteSpace(Title)", "系统主标题不能为空")]
    public override string? Title { get; set; } = string.Empty;
    /// <summary>
    /// 系统副标题
    /// </summary>
    [CommonValidation("!string.IsNullOrWhiteSpace(Host) && string.IsNullOrWhiteSpace(ViceTitle)", "系统副标题不能为空")]
    public override string? ViceTitle { get; set; } = string.Empty;
    /// <summary>
    /// 系统描述
    /// </summary>
    [CommonValidation("!string.IsNullOrWhiteSpace(Host) && string.IsNullOrWhiteSpace(ViceDesc)", "系统描述不能为空")]
    public override string? ViceDesc { get; set; } = string.Empty;
    /// <summary>
    /// 版权说明
    /// </summary>
    [CommonValidation("!string.IsNullOrWhiteSpace(Host) && string.IsNullOrWhiteSpace(Copyright)", "版权说明不能为空")]
    public override string? Copyright { get; set; } = string.Empty;
    /// <summary>
    /// ICP备案号
    /// </summary>
    public override string? Icp { get; set; } = string.Empty;
    /// <summary>
    /// ICP地址
    /// </summary>
    [CommonValidation("!string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Icp) && string.IsNullOrWhiteSpace(IcpUrl)", "ICP地址不能为空")]
    public override string? IcpUrl { get; set; } = string.Empty;
    /// <summary>
    /// Logo图片Base64码
    /// </summary>
    [CommonValidation("!string.IsNullOrWhiteSpace(Host) && string.IsNullOrWhiteSpace(Logo) && string.IsNullOrWhiteSpace(LogoBase64)", "图标不能为空")]
    public virtual string LogoBase64 { get; set; } = string.Empty;
    /// <summary>
    /// Logo文件名
    /// </summary>
    public virtual string LogoFileName { get; set; } = string.Empty;
}

public class UpdateTenantInput : AddTenantInput
{
}

public class DeleteTenantInput : BaseIdInput
{
}

/// <summary>
/// 租户菜单
/// </summary>
public class TenantMenuInput : BaseIdInput
{
    /// <summary>
    /// 同步租户Id集合
    /// </summary>
    public List<long> TenantIdList { get; set; } = [];
    /// <summary>
    /// 菜单Id集合
    /// </summary>
    public List<long> MenuIdList { get; set; } = [];
}

public class TenantUserInput
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public long UserId { get; set; }
}

public class TenantIdInput
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}
