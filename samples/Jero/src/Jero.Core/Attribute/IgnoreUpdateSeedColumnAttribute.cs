namespace Jero.Core;

/// <summary>
/// 忽略更新种子列特性（标记在实体属性）
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class IgnoreUpdateSeedColumnAttribute : Attribute
{
}