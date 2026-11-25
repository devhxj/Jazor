namespace Jero.Core;

/// <summary>
/// 最大值校验
/// </summary>
[SuppressSniffer]
public class MaxValueAttribute : ValidationAttribute
{
    private double MaxValue { get; }

    /// <summary>
    /// 最大值
    /// </summary>
    /// <param name="value"></param>
    public MaxValueAttribute(double value) => this.MaxValue = value;

    /// <summary>
    /// 最大值校验
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool IsValid(object? value)
    {
        return value == null || Convert.ToDouble(value) <= this.MaxValue;
    }

    /// <summary>
    /// 错误信息
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override string FormatErrorMessage(string name) => base.FormatErrorMessage(name);
}
