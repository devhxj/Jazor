namespace ECMAScript;

/// <summary>Adapts a host callback from (name, ...args) to (name, args), converting nullish arguments to null.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class ECMAScriptRestCallbackAttribute : Attribute;
