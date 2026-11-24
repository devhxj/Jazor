namespace ECMAScript;

/// <summary>The JavaScript literal <c>true</c> in closed host value domains.</summary>
[ECMAScript]
public sealed class True
{
    private True() { }

    /// <summary>Emits the JavaScript literal <c>true</c>.</summary>
    public static extern True Value { [ECMAScriptInline("true")] get; }
}
