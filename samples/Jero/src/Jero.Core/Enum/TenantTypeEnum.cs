namespace Jero.Core;

/// <summary>
/// 租户类型枚举
/// </summary>
[Description("租户类型枚举")]
public enum TenantTypeEnum
{
    /// <summary>
    /// ID隔离
    /// </summary>
    [Description("ID隔离")]
    Id = 0,

    /// <summary>
    /// 库隔离
    /// </summary>
    [Description("库隔离")]
    Db = 1,
}