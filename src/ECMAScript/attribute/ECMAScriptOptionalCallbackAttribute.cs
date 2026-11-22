namespace ECMAScript;

/// <summary>Adapts a unary host callback whose missing input and result are both represented by JavaScript undefined.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class ECMAScriptOptionalCallbackAttribute : Attribute;
