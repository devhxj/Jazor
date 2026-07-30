namespace Jazor.CLR.Test;

[TestClass]
public sealed class NumericWidthCharacterizationTests
{
    [TestMethod]
    public void Half_ArithmeticAndRootN_Behavior_MatchesNetRuntime()
    {
        var rounded = (Half)1 + (Half)0.00048828125;
        var negativeZero = BitConverter.UInt16BitsToHalf(0x8000);
        var evenRoot = Half.RootN(negativeZero, 2);
        var oddRoot = Half.RootN(negativeZero, 3);

        Assert.AreEqual((ushort)0x3C00, BitConverter.HalfToUInt16Bits(rounded));
        Assert.AreEqual((ushort)0x0000, BitConverter.HalfToUInt16Bits(evenRoot));
        Assert.AreEqual((ushort)0x8000, BitConverter.HalfToUInt16Bits(oddRoot));
        Assert.IsTrue(Half.IsNaN(Half.RootN((Half)(-8), 2)));
        Assert.ThrowsExactly<ArgumentException>(() => Half.Clamp((Half)1, (Half)2, (Half)0));
    }

    [TestMethod]
    public void Int128_DivisionFamily_UsesSharedOverflowBoundary()
    {
        Assert.ThrowsExactly<OverflowException>(() =>
        {
            _ = Int128.MinValue / -1;
        });
        Assert.ThrowsExactly<OverflowException>(() =>
        {
            _ = Int128.MinValue % -1;
        });
        Assert.ThrowsExactly<OverflowException>(() => Int128.DivRem(Int128.MinValue, -1));
        Assert.AreEqual(Int128.MinValue, unchecked(Int128.MaxValue + 1));
    }
}
