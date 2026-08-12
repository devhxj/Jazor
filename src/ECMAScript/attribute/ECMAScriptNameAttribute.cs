namespace ECMAScript;

/// <summary>
/// Overrides the emitted ECMAScript runtime name for a declaration.
/// 指定声明在发射 ECMAScript 时使用的运行时名称。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class ECMAScriptNameAttribute(string name) : Attribute
{
	/// <summary>Gets the exact ECMAScript runtime name. 获取精确的 ECMAScript 运行时名称。</summary>
	public string Name { get; } = name;
}
