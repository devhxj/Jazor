using System;

namespace ECMAScript;

/// <summary>
/// Emits a fixed Boolean property when constructing a structural record.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class ECMAScriptFixedBooleanAttribute(bool value) : Attribute
{
    /// <summary>The Boolean value emitted for the property.</summary>
    public bool Value { get; } = value;
}
