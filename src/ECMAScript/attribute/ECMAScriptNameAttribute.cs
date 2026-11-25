namespace ECMAScript;

/// <summary>
/// Overrides the emitted ECMAScript runtime name for a declaration.
/// 指定声明在发射 ECMAScript 时使用的运行时名称。
/// </summary>
/// <remarks>Supply a nonempty alias. A field on a <see cref="StringAttribute"/> enum
/// may specify the empty string as its literal value. Use <see cref="BoundaryAttribute"/>
/// on a binding container to stop host-name qualification.</remarks>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class ECMAScriptNameAttribute(string name) : Attribute
{
	/// <summary>Gets the exact ECMAScript runtime name. 获取精确的 ECMAScript 运行时名称。</summary>
	public string Name { get; } = name;
}
