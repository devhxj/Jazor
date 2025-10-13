namespace ECMAScript;

[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class ECMAScriptNameAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}
