namespace ECMAScript;

/// <summary>Supplies a fixed boolean at a JavaScript argument position omitted from the C# signature.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class ECMAScriptFixedBooleanArgumentAttribute(int index, bool value) : Attribute
{
    /// <summary>The zero-based JavaScript argument index.</summary>
    public int Index { get; } = index;

    /// <summary>The argument value.</summary>
    public bool Value { get; } = value;
}
