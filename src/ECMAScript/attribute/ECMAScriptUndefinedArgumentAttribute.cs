namespace ECMAScript;

/// <summary>Supplies undefined at a JavaScript argument position omitted from the C# signature.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class ECMAScriptUndefinedArgumentAttribute(int index) : Attribute
{
    /// <summary>The zero-based JavaScript argument index.</summary>
    public int Index { get; } = index;
}
