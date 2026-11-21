namespace ECMAScript;

/// <summary>Passes JavaScript undefined if a later argument is supplied and this parameter is omitted.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class ECMAScriptUndefinedDefaultAttribute : Attribute;
