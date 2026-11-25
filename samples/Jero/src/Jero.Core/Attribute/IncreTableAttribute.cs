namespace Jero.Core;

/// <summary>
/// 增量表特性
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class IncreTableAttribute : Attribute
{
}