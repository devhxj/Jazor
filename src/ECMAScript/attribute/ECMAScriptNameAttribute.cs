namespace ECMAScript;

/// <summary>
/// 用于指定 ECMAScript 符号的运行时名称。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class ECMAScriptNameAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}
