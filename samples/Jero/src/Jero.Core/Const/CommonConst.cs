namespace Jero.Core;

/// <summary>
/// 通用常量
/// </summary>
[Const("平台配置")]
public class CommonConst
{
    /// <summary>
    /// 日志分组名称
    /// </summary>
    public const string SysLogCategoryName = "System.Logging.LoggingMonitor";

    /// <summary>
    /// 事件-增加异常日志
    /// </summary>
    public const string AddExLog = "Add:ExLog";

    /// <summary>
    /// 事件-发送异常邮件
    /// </summary>
    public const string SendErrorMail = "Send:ErrorMail";

    /// <summary>
    /// 默认基本角色名称
    /// </summary>
    public const string DefaultBaseRoleName = "默认基本角色";

    /// <summary>
    /// 默认基本角色编码
    /// </summary>
    public const string DefaultBaseRoleCode = "default_base_role";
}