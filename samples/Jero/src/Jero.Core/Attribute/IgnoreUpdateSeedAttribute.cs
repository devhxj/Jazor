namespace Jero.Core;

/// <summary>
/// 忽略更新种子特性（标记在种子类）
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class IgnoreUpdateSeedAttribute : Attribute
{
}