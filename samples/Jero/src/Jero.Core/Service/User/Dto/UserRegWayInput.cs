namespace Jero.Core.Service;

/// <summary>
/// 注册方案分页查询输入参数
/// </summary>
public class PageUserRegWayInput : BasePageInput
{
    /// <summary>
    /// 方案名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}

/// <summary>
/// 注册方案增加输入参数
/// </summary>
public class AddUserRegWayInput : SysUserRegWay
{
    /// <summary>
    /// 方案名称
    /// </summary>
    [Required(ErrorMessage = "方案名称不能为空")]
    [MaxLength(32, ErrorMessage = "方案名称字符长度不能超过32")]
    public override string Name { get; set; } = string.Empty;
    /// <summary>
    /// 账号类型
    /// </summary>
    [Enum(ErrorMessage = "账号类型不正确")]
    public override AccountTypeEnum AccountType { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    [Required(ErrorMessage = "角色不能为空")]
    public override long RoleId { get; set; }

    /// <summary>
    /// 机构
    /// </summary>
    [Required(ErrorMessage = "机构不能为空")]
    public override long OrgId { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    [Required(ErrorMessage = "职位不能为空")]
    public override long PosId { get; set; }
}

/// <summary>
/// 注册方案更新输入参数
/// </summary>
public class UpdateUserRegWayInput : AddUserRegWayInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public override long Id { get; set; }
}