namespace Jero.Core;

/// <summary>
/// 事件类型-系统用户操作枚举
/// </summary>
[SuppressSniffer]
[Description("事件类型-系统用户操作枚举")]
public enum SysUserEventTypeEnum
{
    /// <summary>
    /// 增加用户
    /// </summary>
    [Description("增加用户")]
    Add = 111,

    /// <summary>
    /// 更新用户
    /// </summary>
    [Description("更新用户")]
    Update = 222,

    /// <summary>
    /// 授权用户角色
    /// </summary>
    [Description("授权用户角色")]
    UpdateRole = 333,

    /// <summary>
    /// 删除用户
    /// </summary>
    [Description("删除用户")]
    Delete = 444,

    /// <summary>
    /// 设置用户状态
    /// </summary>
    [Description("设置用户状态")]
    SetStatus = 555,

    /// <summary>
    /// 修改密码
    /// </summary>
    [Description("修改密码")]
    ChangePwd = 666,

    /// <summary>
    /// 重置密码
    /// </summary>
    [Description("重置密码")]
    ResetPwd = 777,

    /// <summary>
    /// 解除登录锁定
    /// </summary>
    [Description("解除登录锁定")]
    UnlockLogin = 888,

    /// <summary>
    /// 注册用户
    /// </summary>
    [Description("注册用户")]
    Register = 999,

    /// <summary>
    /// 用户登录
    /// </summary>
    [Description("用户登录")]
    Login = 1000,

    /// <summary>
    /// 用户退出
    /// </summary>
    [Description("用户退出")]
    LoginOut = 1001,

    /// <summary>
    /// RefreshToken
    /// </summary>
    [Description("刷新Token")]
    RefreshToken = 1002,
}