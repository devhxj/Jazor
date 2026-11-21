namespace ECMAScript;

/// <summary>Projects an undefined JavaScript property value or method result as C# null.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = false)]
public sealed class ECMAScriptOptionalAttribute : Attribute;
