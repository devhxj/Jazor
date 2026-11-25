namespace Jero.Core.Service;

/// <summary>
/// 设置用户状态输入参数
/// </summary>
public class UserInput : BaseStatusInput
{
}

/// <summary>
/// 获取用户分页列表输入参数
/// </summary>
public class PageUserInput : BasePageInput
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; } = string.Empty;
    /// <summary>
    /// 姓名
    /// </summary>
    public string RealName { get; set; } = string.Empty;
    /// <summary>
    /// 职位名称
    /// </summary>
    public string PosName { get; set; } = string.Empty;
    /// <summary>
    /// 手机号
    /// </summary>
    public string Phone { get; set; } = string.Empty;
    /// <summary>
    /// 查询时所选机构Id
    /// </summary>
    public long OrgId { get; set; }
}

/// <summary>
/// 增加用户输入参数
/// </summary>
public class AddUserInput : SysUser
{
    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "账号不能为空")]
    public override string Account { get; set; } = string.Empty;
    /// <summary>
    /// 真实姓名
    /// </summary>
    [Required(ErrorMessage = "真实姓名不能为空")]
    public override string RealName { get; set; } = string.Empty;
    /// <summary>
    /// 域用户
    /// </summary>
    public string DomainAccount { get; set; } = string.Empty;
    /// <summary>
    /// 角色集合
    /// </summary>
    public List<long> RoleIdList { get; set; } = [];
    /// <summary>
    /// 扩展机构集合
    /// </summary>
    public List<SysUserExtOrg> ExtOrgIdList { get; set; } = [];
}

/// <summary>
/// 更新用户输入参数
/// </summary>
public class UpdateUserInput : AddUserInput
{
}

/// <summary>
/// 删除用户输入参数
/// </summary>
public class DeleteUserInput : BaseIdInput
{
    /// <summary>
    /// 机构Id
    /// </summary>
    public long OrgId { get; set; }
}

/// <summary>
/// 重置用户密码输入参数
/// </summary>
public class ResetPwdUserInput : BaseIdInput
{
}

/// <summary>
/// 修改用户密码输入参数
/// </summary>
public class ChangePwdInput
{
    /// <summary>
    /// 当前密码
    /// </summary>
    [Required(ErrorMessage = "当前密码不能为空")]
    public string PasswordOld { get; set; } = string.Empty;
    /// <summary>
    /// 新密码
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空"), MinLength(5, ErrorMessage = "密码需要大于5个字符")]
    public string PasswordNew { get; set; } = string.Empty;
}

/// <summary>
/// 解除登录锁定输入参数
/// </summary>
public class UnlockLoginInput : BaseIdInput
{
}