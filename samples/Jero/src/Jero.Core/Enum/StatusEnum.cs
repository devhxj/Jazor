namespace Jero.Core;

/// <summary>
/// 通用状态枚举
/// </summary>
[Description("通用状态枚举")]
public enum StatusEnum
{
    /// <summary>
    /// 启用
    /// </summary>
    [Description("启用"), Theme("success")]
    Enable = 1,

    /// <summary>
    /// 停用
    /// </summary>
    [Description("停用"), Theme("danger")]
    Disable = 2,
}