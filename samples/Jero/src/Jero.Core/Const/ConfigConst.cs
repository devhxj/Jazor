namespace Jero.Core;

/// <summary>
/// 配置常量
/// </summary>
public class ConfigConst
{
    /// <summary>
    /// 演示环境
    /// </summary>
    public const string SysDemoEnv = "sys_demo";

    /// <summary>
    /// 默认密码
    /// </summary>
    public const string SysPassword = "sys_password";

    /// <summary>
    /// 密码最大错误次数
    /// </summary>
    public const string SysPasswordMaxErrorTimes = "sys_password_max_error_times";

    /// <summary>
    /// 日志保留天数
    /// </summary>
    public const string SysLogRetentionDays = "sys_log_retention_days";

    /// <summary>
    /// 记录操作日志
    /// </summary>
    public const string SysOpLog = "sys_oplog";

    /// <summary>
    /// 单设备登录
    /// </summary>
    public const string SysSingleLogin = "sys_single_login";

    /// <summary>
    /// 登入登出提醒
    /// </summary>
    public const string SysLoginOutReminder = "sys_login_out_reminder";

    /// <summary>
    /// 登陆时隐藏租户
    /// </summary>
    public const string SysHideTenantLogin = "sys_hide_tenant_login";

    /// <summary>
    /// 登录二次验证
    /// </summary>
    public const string SysSecondVer = "sys_second_ver";

    /// <summary>
    /// 图形验证码
    /// </summary>
    public const string SysCaptcha = "sys_captcha";

    /// <summary>
    /// Token过期时间
    /// </summary>
    public const string SysTokenExpire = "sys_token_expire";

    /// <summary>
    /// RefreshToken过期时间
    /// </summary>
    public const string SysRefreshTokenExpire = "sys_refresh_token_expire";

    /// <summary>
    /// 发送异常日志邮件
    /// </summary>
    public const string SysErrorMail = "sys_error_mail";

    /// <summary>
    /// 域登录验证
    /// </summary>
    public const string SysDomainLogin = "sys_domain_login";

    // /// <summary>
    // /// 租户域名隔离登录验证
    // /// </summary>
    // public const string SysTenantHostLogin = "sys_tenant_host_login";

    /// <summary>
    /// 数据校验日志
    /// </summary>
    public const string SysValidationLog = "sys_validation_log";

    /// <summary>
    /// 行政区域同步层级 1-省级,2-市级,3-区县级,4-街道级,5-村级
    /// </summary>
    public const string SysRegionSyncLevel = "sys_region_sync_level";

    /// <summary>
    /// 后台默认首页地址
    /// </summary>
    public const string SysDefaultHomePath = "sys_default_home_path";

    /// <summary>
    /// Default 分组
    /// </summary>
    public const string SysDefaultGroup = "Default";

    /// <summary>
    /// 支付宝授权页面地址
    /// </summary>
    public const string AlipayAuthPageUrl = "alipay_auth_page_url_";

    // 系统图标
    // public const string SysWebLogo = "sys_web_logo";
    // 系统主标题
    // public const string SysWebTitle = "sys_web_title";
    // 系统副标题
    // public const string SysWebViceTitle = "sys_web_viceTitle";
    // 系统描述
    // public const string SysWebViceDesc = "sys_web_viceDesc";
    // 水印内容
    // public const string SysWebWatermark = "sys_web_watermark";
    // 版权说明
    // public const string SysWebCopyright = "sys_web_copyright";
    // ICP备案号
    // public const string SysWebIcp = "sys_web_icp";
    // ICP地址
    // public const string SysWebIcpUrl = "sys_web_icpUrl";
}
