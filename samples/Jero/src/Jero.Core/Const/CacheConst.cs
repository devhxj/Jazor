namespace Jero.Core;

/// <summary>
/// 缓存相关常量
/// </summary>
public class CacheConst
{
    /// <summary>
    /// 用户权限缓存（按钮集合）
    /// </summary>
    public const string KeyUserButton = "sys_user_button:";

    /// <summary>
    /// 用户机构缓存
    /// </summary>
    public const string KeyUserOrg = "sys_user_org:";

    /// <summary>
    /// 角色最大数据范围缓存
    /// </summary>
    public const string KeyRoleMaxDataScope = "sys_role_maxDataScope:";

    /// <summary>
    /// 在线用户缓存
    /// </summary>
    public const string KeyUserOnline = "sys_user_online:";

    /// <summary>
    /// 图形验证码缓存
    /// </summary>
    public const string KeyVerCode = "sys_verCode:";

    /// <summary>
    /// 手机验证码缓存
    /// </summary>
    public const string KeyPhoneVerCode = "sys_phoneVerCode:";

    /// <summary>
    /// 密码错误次数缓存
    /// </summary>
    public const string KeyPasswordErrorTimes = "sys_password_error_times:";

    /// <summary>
    /// 租户缓存
    /// </summary>
    public const string KeyTenant = "sys_tenant";

    /// <summary>
    /// 常量下拉框
    /// </summary>
    public const string KeyConst = "sys_const:";

    /// <summary>
    /// 所有缓存关键字集合
    /// </summary>
    public const string KeyAll = "sys_keys";

    /// <summary>
    /// SqlSugar二级缓存
    /// </summary>
    public const string SqlSugar = "sys_sqlSugar:";

    /// <summary>
    /// 开放接口身份缓存
    /// </summary>
    public const string KeyOpenAccess = "sys_open_access:";

    /// <summary>
    /// 开放接口身份随机数缓存
    /// </summary>
    public const string KeyOpenAccessNonce = "sys_open_access_nonce:";

    /// <summary>
    /// 登录黑名单
    /// </summary>
    public const string KeyBlacklist = "sys_blacklist:";

    /// <summary>
    /// 系统配置缓存
    /// </summary>
    public const string KeyConfig = "sys_config:";

    /// <summary>
    /// 系统租户配置缓存
    /// </summary>
    public const string KeyTenantConfig = "sys_tenant_config:";

    /// <summary>
    /// 系统用户配置缓存
    /// </summary>
    public const string KeyUserConfig = "sys_user_config:";

    /// <summary>
    /// 系统字典缓存
    /// </summary>
    public const string KeyDict = "sys_dict:";

    /// <summary>
    /// 系统租户字典缓存
    /// </summary>
    public const string KeyTenantDict = "sys_tenant_dict:";

    /// <summary>
    /// 重复请求(幂等)字典缓存
    /// </summary>
    public const string KeyIdempotent = "sys_idempotent:";

    /// <summary>
    /// Excel临时文件缓存
    /// </summary>
    public const string KeyExcelTemp = "sys_excel_temp:";

    /// <summary>
    /// 系统更新命令日志缓存
    /// </summary>
    public const string KeySysUpdateLog = "sys_update_log";

    /// <summary>
    /// 系统更新间隔标记缓存
    /// </summary>
    public const string KeySysUpdateInterval = "sys_update_interval";
}