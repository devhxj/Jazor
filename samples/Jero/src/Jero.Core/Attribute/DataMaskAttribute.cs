namespace Jero.Core;

/// <summary>
/// 数据脱敏特性（支持自定义脱敏位置和脱敏字符）
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DataMaskAttribute : Attribute
{
    /// <summary>
    /// 脱敏起始位置（从0开始）
    /// </summary>
    private int StartIndex { get; }

    /// <summary>
    /// 脱敏长度
    /// </summary>
    private int Length { get; }

    /// <summary>
    /// 脱敏字符（默认*）
    /// </summary>
    private char MaskChar { get; set; } = '*';

    /// <summary>
    /// 是否保留原始长度（默认true）
    /// </summary>
    private bool KeepLength { get; set; } = true;

    public DataMaskAttribute(int startIndex, int length)
    {
        if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

        StartIndex = startIndex;
        Length = length;
    }

    /// <summary>
    /// 执行脱敏处理
    /// </summary>
    public string Mask(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= StartIndex)
            return input;

        var maskedLength = Math.Min(Length, input.Length - StartIndex);
        var maskStr = new string(MaskChar, KeepLength ? maskedLength : Math.Min(4, maskedLength));

        return input.Substring(0, StartIndex) + maskStr +
               (StartIndex + maskedLength < input.Length ?
                   input.Substring(StartIndex + maskedLength) : "");
    }
}