namespace Jero.Core;

/// <summary>
/// 忽略枚举类型转字典特性（标记在枚举类型）
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = true, Inherited = true)]
public class IgnoreEnumToDictAttribute : Attribute
{
}