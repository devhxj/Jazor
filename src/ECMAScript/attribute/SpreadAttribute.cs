namespace ECMAScript;

/// <summary>
/// Declares that a record property should be spread into its containing structural object literal
/// instead of being emitted as a nested property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class SpreadAttribute : Attribute
{
}
