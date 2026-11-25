namespace Jero.Core;

/// <summary>
/// 枚举拓展主题样式
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class ThemeAttribute : Attribute
{
    public string Theme { get; private set; }

    public ThemeAttribute(string theme)
    {
        this.Theme = theme;
    }
}