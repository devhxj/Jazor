using Acornima;
using Jazor.Compiler;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class JavaScriptAstFactoryTests
{
    [TestMethod]
    public void CreateStringLiteral_NullValue_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => JavaScriptAstFactory.CreateStringLiteral(null!));

        Assert.AreEqual("value", exception.ParamName);
    }

    [TestMethod]
    public void CreateStringLiteral_EscapesAllJavaScriptSourceHazards()
    {
        var value = "\"\\\0" + "1\a\b\f\n\r\t\v\u001F\u2028\u2029";

        var literal = JavaScriptAstFactory.CreateStringLiteral(value);

        Assert.AreEqual(value, literal.Value);
        Assert.AreEqual(
            "\"\\\"\\\\\\x001\\u0007\\b\\f\\n\\r\\t\\v\\u001F\\u2028\\u2029\"",
            literal.ToKnRECMAScript());
    }

    [TestMethod]
    public void CreateStringLiteral_NullNotFollowedByDigit_UsesShortEscape()
    {
        var literal = JavaScriptAstFactory.CreateStringLiteral("\0x");
        var beforeDecimalRange = JavaScriptAstFactory.CreateStringLiteral("\0/");

        Assert.AreEqual("\"\\0x\"", literal.ToKnRECMAScript());
        Assert.AreEqual("\"\\0/\"", beforeDecimalRange.ToKnRECMAScript());
    }

    [TestMethod]
    public void CreateStringLiteral_UnpairedSurrogates_UsesUnicodeEscapes()
    {
        var value = new string(['\uD800', 'x', '\uDC00']);

        var literal = JavaScriptAstFactory.CreateStringLiteral(value);

        Assert.AreEqual(value, literal.Value);
        Assert.AreEqual("\"\\uD800x\\uDC00\"", literal.ToKnRECMAScript());
    }

    [TestMethod]
    public void CreateStringLiteral_TerminalSurrogates_UseUnicodeEscapes()
    {
        var leadingLow = JavaScriptAstFactory.CreateStringLiteral(new string(['\uDC00', 'x']));
        var trailingHigh = JavaScriptAstFactory.CreateStringLiteral(new string(['x', '\uD800']));

        Assert.AreEqual("\"\\uDC00x\"", leadingLow.ToKnRECMAScript());
        Assert.AreEqual("\"x\\uD800\"", trailingHigh.ToKnRECMAScript());
    }
}
