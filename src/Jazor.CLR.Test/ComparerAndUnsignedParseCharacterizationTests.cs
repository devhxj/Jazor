namespace Jazor.CLR.Test;

[TestClass]
public sealed class ComparerAndUnsignedParseCharacterizationTests
{
    [TestMethod]
    public void ArrayBinarySearch_ObjectAndGenericNumericArrays_UseNumericComparerSemantics()
    {
        object[] objectArray = [2, 10, 100];
        var genericArray = new[] { 2, 10, 100 };

        Assert.AreEqual(~2, System.Array.BinarySearch(objectArray, 11));
        Assert.AreEqual(~2, System.Array.BinarySearch(genericArray, 11));
    }

    [TestMethod]
    public void Comparer_ObjectDefault_MixedPrimitiveTypes_ThrowArgumentException()
    {
        var comparer = Comparer<object>.Default;

        Assert.ThrowsExactly<ArgumentException>(() => comparer.Compare(1, "1"));
    }

    [TestMethod]
    public void Comparer_ObjectDefault_UnrelatedObjects_ThrowArgumentException()
    {
        var comparer = Comparer<object>.Default;

        Assert.ThrowsExactly<ArgumentException>(() => comparer.Compare(new object(), new object()));
    }

    [TestMethod]
    public void UnsignedParsers_NegativeAndTooLargeInputs_ThrowOverflowException()
    {
        Assert.ThrowsExactly<OverflowException>(() => byte.Parse("-1"));
        Assert.ThrowsExactly<OverflowException>(() => byte.Parse("256"));

        Assert.ThrowsExactly<OverflowException>(() => ushort.Parse("-1"));
        Assert.ThrowsExactly<OverflowException>(() => ushort.Parse("65536"));

        Assert.ThrowsExactly<OverflowException>(() => uint.Parse("-1"));
        Assert.ThrowsExactly<OverflowException>(() => uint.Parse("4294967296"));
    }

    [TestMethod]
    public void UnsignedTryParse_NegativeZero_IsAccepted()
    {
        Assert.IsTrue(byte.TryParse("-0", out var parsedByte));
        Assert.AreEqual((byte)0, parsedByte);

        Assert.IsTrue(ushort.TryParse("-0", out var parsedUInt16));
        Assert.AreEqual((ushort)0, parsedUInt16);

        Assert.IsTrue(uint.TryParse("-0", out var parsedUInt32));
        Assert.AreEqual((uint)0, parsedUInt32);
    }
}
