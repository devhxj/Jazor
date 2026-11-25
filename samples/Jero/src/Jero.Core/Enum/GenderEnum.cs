namespace Jero.Core;

/// <summary>
/// 性别枚举（GB/T 2261.1-2003）
/// </summary>
[Description("性别枚举")]
public enum GenderEnum
{
    /// <summary>
    /// 未知
    /// </summary>
    [Description("未知"), Theme("info")]
    Unknown = 0,

    /// <summary>
    /// 男性
    /// </summary>
    [Description("男性"), Theme("success")]
    Male = 1,

    /// <summary>
    /// 女性
    /// </summary>
    [Description("女性"), Theme("danger")]
    Female = 2,

    ///// <summary>
    ///// 未说明的性别
    ///// </summary>
    //[Description("未说明的性别"), Theme("warning")]
    //Unspecified = 9
}