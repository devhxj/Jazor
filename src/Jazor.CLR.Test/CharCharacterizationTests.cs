namespace Jazor.CLR.Test;

[TestClass]
public sealed class CharCharacterizationTests
{
    [TestMethod]
    public void Char_IsWhiteSpace_CoversRepresentativeUnicodeWhitespaceCodePoints()
    {
        Assert.IsTrue(char.IsWhiteSpace('\u0009'));
        Assert.IsTrue(char.IsWhiteSpace('\u000B'));
        Assert.IsTrue(char.IsWhiteSpace('\u0085'));
        Assert.IsTrue(char.IsWhiteSpace('\u00A0'));
        Assert.IsTrue(char.IsWhiteSpace('\u1680'));
        Assert.IsTrue(char.IsWhiteSpace('\u2000'));
        Assert.IsTrue(char.IsWhiteSpace('\u200A'));
        Assert.IsTrue(char.IsWhiteSpace('\u2028'));
        Assert.IsTrue(char.IsWhiteSpace('\u2029'));
        Assert.IsTrue(char.IsWhiteSpace('\u202F'));
        Assert.IsTrue(char.IsWhiteSpace('\u205F'));
        Assert.IsTrue(char.IsWhiteSpace('\u3000'));

        Assert.IsFalse(char.IsWhiteSpace('\u200B'));
        Assert.IsFalse(char.IsWhiteSpace('\uFEFF'));
    }
}
