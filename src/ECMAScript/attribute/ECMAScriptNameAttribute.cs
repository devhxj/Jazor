namespace ECMAScript;

/// <summary>
/// Attribute to specify the name of a class or member in ECMAScript.
/// </summary>
/// <param name="name"></param>
/// <param name="isSpecial"></param>
/// <param name="specialKey"></param>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class ECMAScriptNameAttribute(string name, bool isSpecial = false, string specialKey = "") : Attribute
{
	public string Name { get; } = name;

	public bool IsSpecial { get; } = isSpecial;

	public string SpecialKey { get; } = specialKey;
}
