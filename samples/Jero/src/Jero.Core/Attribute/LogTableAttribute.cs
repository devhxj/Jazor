namespace Jero.Core;

/// <summary>
/// 日志表特性
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class LogTableAttribute : Attribute
{
}