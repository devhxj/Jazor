namespace Jero.Core;

/// <summary>
/// 种子数据特性
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class SeedDataAttribute : Attribute
{
    /// <summary>
    /// 排序（越大越后执行）
    /// </summary>
    public int Order { get; set; } = 0;

    public SeedDataAttribute(int orderNo)
    {
        Order = orderNo;
    }
}