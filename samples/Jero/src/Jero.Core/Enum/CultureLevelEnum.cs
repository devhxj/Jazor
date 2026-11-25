namespace Jero.Core;

/// <summary>
/// 文化程度枚举
/// </summary>
[Description("文化程度枚举")]
public enum CultureLevelEnum
{
    /// <summary>
    /// 其他
    /// </summary>
    [Description("其他"), Theme("info")]
    Level0 = 0,

    /// <summary>
    /// 文盲
    /// </summary>
    [Description("文盲")]
    Level1 = 1,

    /// <summary>
    /// 小学
    /// </summary>
    [Description("小学")]
    Level2 = 2,

    /// <summary>
    /// 初中
    /// </summary>
    [Description("初中")]
    Level3 = 3,

    /// <summary>
    /// 普通高中
    /// </summary>
    [Description("普通高中")]
    Level4 = 4,

    /// <summary>
    /// 技工学校
    /// </summary>
    [Description("技工学校")]
    Level5 = 5,

    /// <summary>
    /// 职业教育
    /// </summary>
    [Description("职业教育")]
    Level6 = 6,

    /// <summary>
    /// 职业高中
    /// </summary>
    [Description("职业高中")]
    Level7 = 7,

    /// <summary>
    /// 中等专科
    /// </summary>
    [Description("中等专科")]
    Level8 = 8,

    /// <summary>
    /// 大学专科
    /// </summary>
    [Description("大学专科")]
    Level9 = 9,

    /// <summary>
    /// 大学本科
    /// </summary>
    [Description("大学本科")]
    Level10 = 10,

    /// <summary>
    /// 硕士研究生
    /// </summary>
    [Description("硕士研究生")]
    Level11 = 11,

    /// <summary>
    /// 博士研究生
    /// </summary>
    [Description("博士研究生")]
    Level12 = 12,
}