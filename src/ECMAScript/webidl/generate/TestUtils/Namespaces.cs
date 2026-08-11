namespace ECMAScript.TestUtils;

/// <summary>
/// <see href="https://testutils.spec.whatwg.org/#namespacedef-testutils">Test Utils Standard: 4 The TestUtils Namespace</see>
/// </summary>
[ECMAScript]
[Description("@#TestUtils")]
public static class TestUtils
{
    /// <summary>
    /// <see href="https://testutils.spec.whatwg.org/#dom-testutils-gc">Test Utils Standard: 4 The TestUtils Namespace</see>
    /// </summary>
    [Description("@#gc")]
    public static extern PromiseResult Gc();
}
