namespace ECMAScript;

/// <summary>Reads JavaScript undefined as C# null, or passes a null argument as JavaScript undefined.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false)]
public sealed class ECMAScriptOptionalAttribute : Attribute;
