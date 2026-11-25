namespace Jero.Core.Service;

/// <summary>
/// 用户登录信息
/// </summary>
public class LoginUserOutput
{
    /// <summary>
    /// 用户id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 账号名称
    /// </summary>
    public string Account { get; set; } = string.Empty;
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; } = string.Empty;
    /// <summary>
    /// 电话
    /// </summary>
    public string Phone { get; set; } = string.Empty;
    /// <summary>
    /// 身份证
    /// </summary>
    public string IdCardNum { get; set; } = string.Empty;
    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// 账号类型
    /// </summary>
    public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.NormalUser;

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; } = string.Empty;
    /// <summary>
    /// 个人简介
    /// </summary>
    public string Introduction { get; set; } = string.Empty;
    /// <summary>
    /// 地址
    /// </summary>
    public string Address { get; set; } = string.Empty;
    /// <summary>
    /// 电子签名
    /// </summary>
    public string Signature { get; set; } = string.Empty;
    /// <summary>
    /// 机构Id
    /// </summary>
    public long OrgId { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    public string OrgName { get; set; } = string.Empty;
    /// <summary>
    /// 机构类型
    /// </summary>
    public string OrgType { get; set; } = string.Empty;
    /// <summary>
    /// 职位名称
    /// </summary>
    public string PosName { get; set; } = string.Empty;
    /// <summary>
    /// 按钮权限集合
    /// </summary>
    public List<string> Buttons { get; set; } = [];
    /// <summary>
    /// 角色集合
    /// </summary>
    public List<long> RoleIds { get; set; } = [];
    /// <summary>
    /// 水印文字
    /// </summary>
    public string WatermarkText { get; set; } = string.Empty;
    /// <summary>
    /// 租户Id
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 当前切换到的租户Id
    /// </summary>
    public long? CurrentTenantId { get; set; }

    /// <summary>
    /// 语言代码
    /// </summary>
    public string LangCode { get; internal set; } = string.Empty;

    /// <summary>
    /// 登录后首页地址
    /// </summary>
    public string HomePath { get; set; } = DefaultHomePathUtil.FallbackHomePath;
}
