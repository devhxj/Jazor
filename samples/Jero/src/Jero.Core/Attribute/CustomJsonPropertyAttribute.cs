namespace Jero.Core;

/// <summary>
/// 自定义Json转换字段名
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CustomJsonPropertyAttribute : Attribute
{
    public string Name { get; }

    public CustomJsonPropertyAttribute(string name)
    {
        Name = name;
    }
}