namespace Jero.Core;

/// <summary>
/// 过滤条件
/// </summary>
[Description("过滤条件")]
public enum FilterLogicEnum
{
    /// <summary>
    /// 并且
    /// </summary>
    [Description("并且")]
    And,

    /// <summary>
    /// 或者
    /// </summary>
    [Description("或者")]
    Or,

    /// <summary>
    /// 异或
    /// </summary>
    [Description("异或")]
    Xor
}