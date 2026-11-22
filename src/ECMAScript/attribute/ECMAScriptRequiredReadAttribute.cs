namespace ECMAScript;

/// <summary>Throws when a JavaScript-backed property reads as undefined.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class ECMAScriptRequiredReadAttribute : Attribute;
