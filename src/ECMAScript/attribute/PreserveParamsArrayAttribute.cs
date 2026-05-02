namespace ECMAScript;

/// <summary>
/// Marks a <c>params</c> parameter that should stay as one emitted array argument
/// instead of being expanded into JavaScript varargs by compiler conveniences.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class PreserveParamsArrayAttribute : Attribute
{
}
