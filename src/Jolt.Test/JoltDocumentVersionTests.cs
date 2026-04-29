using Jazor.Common.VueContracts.Documents;

namespace Jolt.Test;

[TestClass]
public sealed class JoltDocumentVersionTests
{
    [TestMethod]
    public void DocumentVersion_Constructor_RejectsBlankValues()
    {
        AssertThrows<ArgumentException>(() => new DocumentVersion(""));
        AssertThrows<ArgumentException>(() => new DocumentVersion("   "));
    }

    [TestMethod]
    public void DocumentVersion_Create_RejectsNegativeVersions()
    {
        AssertThrows<ArgumentOutOfRangeException>(() => DocumentVersion.Create(-1));
        Assert.IsFalse(DocumentVersion.TryCreate(-1, out _));
    }

    [TestMethod]
    public void DocumentVersion_TryCreate_ReturnsFalseForBlankValues()
    {
        Assert.IsFalse(DocumentVersion.TryCreate(null, out _));
        Assert.IsFalse(DocumentVersion.TryCreate("", out _));
        Assert.IsFalse(DocumentVersion.TryCreate("   ", out _));
    }

    [TestMethod]
    public void DocumentVersion_TryCreate_PreservesValidValues()
    {
        Assert.IsTrue(DocumentVersion.TryCreate("v1", out var stringVersion));
        Assert.AreEqual("v1", stringVersion.Value);

        Assert.IsTrue(DocumentVersion.TryCreate(42, out var numericVersion));
        Assert.AreEqual("42", numericVersion.Value);
        Assert.AreEqual("42", DocumentVersion.Create(42).ToString());
    }

    private static TException AssertThrows<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new AssertFailedException($"Expected exception of type {typeof(TException).Name}.");
    }
}
