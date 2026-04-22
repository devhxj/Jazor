namespace Jazor.CLR.Test;

[TestClass]
public sealed class DoubleCharacterizationTests
{
    [TestMethod]
    public void Double_MaxMinMagnitude_Behavior_MatchesNetRuntime()
    {
        var maxTie = double.MaxMagnitude(-3d, 3d);
        var minTie = double.MinMagnitude(-3d, 3d);
        var maxNaNLeft = double.MaxMagnitude(double.NaN, 1d);
        var maxNaNRight = double.MaxMagnitude(1d, double.NaN);
        var minNaNLeft = double.MinMagnitude(double.NaN, 1d);
        var minNaNRight = double.MinMagnitude(1d, double.NaN);
        var maxNumberNaNLeft = double.MaxMagnitudeNumber(double.NaN, 1d);
        var maxNumberNaNRight = double.MaxMagnitudeNumber(1d, double.NaN);
        var minNumberNaNLeft = double.MinMagnitudeNumber(double.NaN, 1d);
        var minNumberNaNRight = double.MinMagnitudeNumber(1d, double.NaN);
        var maxZero = double.MaxMagnitude(-0d, 0d);
        var minZero = double.MinMagnitude(-0d, 0d);
        var maxNumberZero = double.MaxMagnitudeNumber(-0d, 0d);
        var minNumberZero = double.MinMagnitudeNumber(-0d, 0d);

        Assert.AreEqual(3d, maxTie);
        Assert.AreEqual(-3d, minTie);
        Assert.IsTrue(double.IsNaN(maxNaNLeft));
        Assert.IsTrue(double.IsNaN(maxNaNRight));
        Assert.IsTrue(double.IsNaN(minNaNLeft));
        Assert.IsTrue(double.IsNaN(minNaNRight));
        Assert.AreEqual(1d, maxNumberNaNLeft);
        Assert.AreEqual(1d, maxNumberNaNRight);
        Assert.AreEqual(1d, minNumberNaNLeft);
        Assert.AreEqual(1d, minNumberNaNRight);
        Assert.AreEqual(0L, BitConverter.DoubleToInt64Bits(maxZero));
        Assert.AreEqual(unchecked((long)0x8000000000000000), BitConverter.DoubleToInt64Bits(minZero));
        Assert.AreEqual(0L, BitConverter.DoubleToInt64Bits(maxNumberZero));
        Assert.AreEqual(unchecked((long)0x8000000000000000), BitConverter.DoubleToInt64Bits(minNumberZero));
    }

    [TestMethod]
    public void Double_MaxMinNumber_Behavior_MatchesNetRuntime()
    {
        var maxNaNLeft = double.MaxNumber(double.NaN, 1d);
        var maxNaNRight = double.MaxNumber(1d, double.NaN);
        var minNaNLeft = double.MinNumber(double.NaN, 1d);
        var minNaNRight = double.MinNumber(1d, double.NaN);
        var maxZero = double.MaxNumber(-0d, 0d);
        var minZero = double.MinNumber(-0d, 0d);

        Assert.AreEqual(1d, maxNaNLeft);
        Assert.AreEqual(1d, maxNaNRight);
        Assert.AreEqual(1d, minNaNLeft);
        Assert.AreEqual(1d, minNaNRight);
        Assert.AreEqual(0L, BitConverter.DoubleToInt64Bits(maxZero));
        Assert.AreEqual(unchecked((long)0x8000000000000000), BitConverter.DoubleToInt64Bits(minZero));
    }

    [TestMethod]
    public void Double_SignAndIsPow2_Behavior_MatchesNetRuntime()
    {
        Assert.ThrowsExactly<ArithmeticException>(() => double.Sign(double.NaN));
        Assert.AreEqual(0, double.Sign(-0d));
        Assert.AreEqual(0, double.Sign(0d));
        Assert.AreEqual(-1, double.Sign(-2.5d));
        Assert.AreEqual(1, double.Sign(2.5d));

        Assert.IsTrue(double.IsPow2(8d));
        Assert.IsTrue(double.IsPow2(0.5d));
        Assert.IsFalse(double.IsPow2(3d));
        Assert.IsFalse(double.IsPow2(0d));
        Assert.IsFalse(double.IsPow2(-8d));
        Assert.IsFalse(double.IsPow2(double.NaN));
        Assert.IsFalse(double.IsPow2(double.PositiveInfinity));
    }

    [TestMethod]
    public void Double_IsNormalAndIsSubnormal_Behavior_MatchesNetRuntime()
    {
        var minNormal = BitConverter.Int64BitsToDouble(unchecked((long)0x0010000000000000));
        var minSubnormal = BitConverter.Int64BitsToDouble(0x0000000000000001);

        Assert.IsTrue(double.IsNormal(minNormal));
        Assert.IsFalse(double.IsSubnormal(minNormal));
        Assert.IsFalse(double.IsNormal(minSubnormal));
        Assert.IsTrue(double.IsSubnormal(minSubnormal));
        Assert.IsFalse(double.IsNormal(-minSubnormal));
        Assert.IsTrue(double.IsSubnormal(-minSubnormal));
        Assert.IsFalse(double.IsNormal(0d));
        Assert.IsFalse(double.IsSubnormal(0d));
        Assert.IsFalse(double.IsNormal(double.NaN));
        Assert.IsFalse(double.IsSubnormal(double.NaN));
        Assert.IsFalse(double.IsNormal(double.PositiveInfinity));
        Assert.IsFalse(double.IsSubnormal(double.PositiveInfinity));
    }
}
