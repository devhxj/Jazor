namespace ECMAScript;

/// <summary>The JavaScript literal <c>false</c> in closed host value domains.</summary>
[ECMAScript]
public sealed class False
{
    private False() { }

    /// <summary>Emits the JavaScript literal <c>false</c>.</summary>
    public static extern False Value { [ECMAScriptInline("false")] get; }
}
