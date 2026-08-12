namespace ECMAScript;

/// <summary>
/// Marks a <c>params</c> parameter that should stay as one emitted array argument
/// instead of being expanded into JavaScript varargs by compiler conveniences.
/// 标记一个 params 参数，使其在编译时保留为单个数组参数发出，而不会被编译器的便利机制展开为 JavaScript 可变参数。
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class PreserveAttribute : Attribute
{
}
