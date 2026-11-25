namespace Jero.Core;

/// <summary>
/// 过滤逻辑运算符
/// </summary>
[Description("过滤逻辑运算符")]
public enum FilterOperatorEnum
{
    /// <summary>
    /// 等于(=)
    /// </summary>
    [Description("等于")]
    EQ,

    /// <summary>
    /// 不等于(!=)
    /// </summary>
    [Description("不等于")]
    NEQ,

    /// <summary>
    /// 小于<![CDATA[ < ]]>
    /// </summary>
    [Description("小于")]
    LT,

    /// <summary>
    /// 小于等于<![CDATA[ <= ]]>
    /// </summary>
    [Description("小于等于")]
    LTE,

    /// <summary>
    /// 大于(>)
    /// </summary>
    [Description("大于")]
    GT,

    /// <summary>
    /// 大于等于(>=)
    /// </summary>
    [Description("大于等于")]
    GTE,

    /// <summary>
    /// 开始包含
    /// </summary>
    [Description("开始包含")]
    StartsWith,

    /// <summary>
    /// 末尾包含
    /// </summary>
    [Description("末尾包含")]
    EndsWith,

    /// <summary>
    /// 包含
    /// </summary>
    [Description("包含")]
    Contains
}