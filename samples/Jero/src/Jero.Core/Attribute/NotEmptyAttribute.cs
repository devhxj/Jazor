namespace Jero.Core;

/// <summary>
/// 校验集合不能为空
/// </summary>
[SuppressSniffer]
public class NotEmptyAttribute : ValidationAttribute
{
    /// <summary>
    /// 校验集合不能为空
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool IsValid(object? value) => (value as IEnumerable)?.GetEnumerator().MoveNext() ?? false;

    /// <summary>
    /// 错误信息
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override string FormatErrorMessage(string name) => base.FormatErrorMessage(name);
}
