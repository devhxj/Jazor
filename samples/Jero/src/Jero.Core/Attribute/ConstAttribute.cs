namespace Jero.Core;

/// <summary>
/// 常量特性
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ConstAttribute : Attribute
{
    public string Name { get; set; } = string.Empty;
    public ConstAttribute(string name)
    {
        Name = name;
    }
}