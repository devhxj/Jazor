namespace Jero.Core;

/// <summary>
/// 完成状态枚举
/// </summary>
[Description("完成状态枚举")]
public enum FinishStatusEnum
{
    /// <summary>
    /// 已完成
    /// </summary>
    [Description("已完成"), Theme("success")]
    Finish = 1,

    /// <summary>
    /// 未完成
    /// </summary>
    [Description("未完成"), Theme("danger")]
    UnFinish = 0,
}