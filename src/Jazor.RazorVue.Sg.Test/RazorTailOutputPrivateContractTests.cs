using System.Reflection;
using Jazor.RazorVue.Generation;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorTailOutputPrivateContractTests
{
    [TestMethod]
    public void EscapeCSharpString_EncodesAllControlAndDelimiterCharacters()
    {
        var escaped = InvokeEscape("plain\\\"\0\a\b\f\n\r\t\v");

        Assert.AreEqual("\"plain\\\\\\\"\\0\\a\\b\\f\\n\\r\\t\\v\"", escaped);
    }

    private static string InvokeEscape(string value)
    {
        var method = typeof(RazorTailOutput).GetMethod(
            "EscapeCSharpString",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (string)method.Invoke(null, [value])!;
    }
}
