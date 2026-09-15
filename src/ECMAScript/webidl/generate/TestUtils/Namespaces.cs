namespace ECMAScript.TestUtils;

/// <summary>
/// WebIDL namespace TestUtils。定义于 Test Utils Standard。
/// </summary>
/// <remarks>
/// <see href="https://testutils.spec.whatwg.org/#namespacedef-testutils">Test Utils Standard: 4 The TestUtils Namespace</see>
/// </remarks>
[ECMAScript]
[Description("@#TestUtils")]
public static class TestUtils
{
    /// <summary>
    /// JavaScript TestUtils.gc() 的强类型绑定，WebIDL 返回类型为 Promise&lt;undefined&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://testutils.spec.whatwg.org/#dom-testutils-gc">Test Utils Standard: 4 The TestUtils Namespace</see>
    /// </remarks>
    [Description("@#gc")]
    public static extern PromiseResult Gc();
}