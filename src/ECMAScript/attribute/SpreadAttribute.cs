namespace ECMAScript;

/// <summary>
/// Declares that a record property should be spread into its containing structural object literal
/// instead of being emitted as a nested property.
/// 声明记录属性应展开到其包含的结构化对象字面量中，而不是作为嵌套属性发出。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class SpreadAttribute : Attribute
{
}
