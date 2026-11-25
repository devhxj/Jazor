namespace Jero.Core;

/// <summary>
/// 最小值校验
/// </summary>
[SuppressSniffer]
public class MinValueAttribute : ValidationAttribute
{
    private double MinValue { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    /// <param name="value"></param>
    public MinValueAttribute(double value) => this.MinValue = value;

    /// <summary>
    /// 最小值校验
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool IsValid(object? value)
    {
        return value == null || Convert.ToDouble(value) > this.MinValue;
    }

    /// <summary>
    /// 错误信息
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override string FormatErrorMessage(string name) => base.FormatErrorMessage(name);
}
