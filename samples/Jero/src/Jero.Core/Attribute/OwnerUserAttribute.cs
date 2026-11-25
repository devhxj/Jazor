namespace Jero.Core;

/// <summary>
/// 所属用户数据权限
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class OwnerUserAttribute : Attribute
{
}