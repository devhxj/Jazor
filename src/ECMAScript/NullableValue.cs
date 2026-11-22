namespace ECMAScript;

/// <summary>Exposes a JavaScript value that can also be null for any payload type.</summary>
[ECMAScript]
public sealed class NullableValue<T>
{
    private NullableValue() { }

    /// <summary>Preserves the original JavaScript payload when assigning to a nullable host field.</summary>
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(value))]
    public static extern implicit operator NullableValue<T>?(T value);

    /// <summary>Reads the payload after the caller has checked for null. Zero and false retain their values.</summary>
    public extern T Value
    {
        [ECMAScriptInline("__arg1")]
        get;
    }
}
