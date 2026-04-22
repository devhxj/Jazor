namespace Jazor.CLR.Test;

[TestClass]
public sealed class SingleCharacterizationTests
{
    [TestMethod]
    public void Single_MaxMinMagnitude_Behavior_MatchesNetRuntime()
    {
        var maxTie = float.MaxMagnitude(-3f, 3f);
        var minTie = float.MinMagnitude(-3f, 3f);
        var maxNaNLeft = float.MaxMagnitude(float.NaN, 1f);
        var maxNaNRight = float.MaxMagnitude(1f, float.NaN);
        var minNaNLeft = float.MinMagnitude(float.NaN, 1f);
        var minNaNRight = float.MinMagnitude(1f, float.NaN);
        var maxNumberNaNLeft = float.MaxMagnitudeNumber(float.NaN, 1f);
        var maxNumberNaNRight = float.MaxMagnitudeNumber(1f, float.NaN);
        var minNumberNaNLeft = float.MinMagnitudeNumber(float.NaN, 1f);
        var minNumberNaNRight = float.MinMagnitudeNumber(1f, float.NaN);
        var maxZero = float.MaxMagnitude(-0f, 0f);
        var minZero = float.MinMagnitude(-0f, 0f);
        var maxNumberZero = float.MaxMagnitudeNumber(-0f, 0f);
        var minNumberZero = float.MinMagnitudeNumber(-0f, 0f);

        Assert.AreEqual(3f, maxTie);
        Assert.AreEqual(-3f, minTie);
        Assert.IsTrue(float.IsNaN(maxNaNLeft));
        Assert.IsTrue(float.IsNaN(maxNaNRight));
        Assert.IsTrue(float.IsNaN(minNaNLeft));
        Assert.IsTrue(float.IsNaN(minNaNRight));
        Assert.AreEqual(1f, maxNumberNaNLeft);
        Assert.AreEqual(1f, maxNumberNaNRight);
        Assert.AreEqual(1f, minNumberNaNLeft);
        Assert.AreEqual(1f, minNumberNaNRight);
        Assert.AreEqual(0, BitConverter.SingleToInt32Bits(maxZero));
        Assert.AreEqual(unchecked((int)0x80000000), BitConverter.SingleToInt32Bits(minZero));
        Assert.AreEqual(0, BitConverter.SingleToInt32Bits(maxNumberZero));
        Assert.AreEqual(unchecked((int)0x80000000), BitConverter.SingleToInt32Bits(minNumberZero));
    }

    [TestMethod]
    public void Single_MaxMinNumber_Behavior_MatchesNetRuntime()
    {
        var maxNaNLeft = float.MaxNumber(float.NaN, 1f);
        var maxNaNRight = float.MaxNumber(1f, float.NaN);
        var minNaNLeft = float.MinNumber(float.NaN, 1f);
        var minNaNRight = float.MinNumber(1f, float.NaN);
        var maxZero = float.MaxNumber(-0f, 0f);
        var minZero = float.MinNumber(-0f, 0f);

        Assert.AreEqual(1f, maxNaNLeft);
        Assert.AreEqual(1f, maxNaNRight);
        Assert.AreEqual(1f, minNaNLeft);
        Assert.AreEqual(1f, minNaNRight);
        Assert.AreEqual(0, BitConverter.SingleToInt32Bits(maxZero));
        Assert.AreEqual(unchecked((int)0x80000000), BitConverter.SingleToInt32Bits(minZero));
    }

    [TestMethod]
    public void Single_SignAndIsPow2_Behavior_MatchesNetRuntime()
    {
        Assert.ThrowsExactly<ArithmeticException>(() => float.Sign(float.NaN));
        Assert.AreEqual(0, float.Sign(-0f));
        Assert.AreEqual(0, float.Sign(0f));
        Assert.AreEqual(-1, float.Sign(-2.5f));
        Assert.AreEqual(1, float.Sign(2.5f));

        Assert.IsTrue(float.IsPow2(8f));
        Assert.IsTrue(float.IsPow2(0.5f));
        Assert.IsFalse(float.IsPow2(3f));
        Assert.IsFalse(float.IsPow2(0f));
        Assert.IsFalse(float.IsPow2(-8f));
        Assert.IsFalse(float.IsPow2(float.NaN));
        Assert.IsFalse(float.IsPow2(float.PositiveInfinity));
    }

    [TestMethod]
    public void Single_IsNormalAndIsSubnormal_Behavior_MatchesNetRuntime()
    {
        var minNormal = BitConverter.Int32BitsToSingle(0x00800000);
        var minSubnormal = BitConverter.Int32BitsToSingle(0x00000001);

        Assert.IsTrue(float.IsNormal(minNormal));
        Assert.IsFalse(float.IsSubnormal(minNormal));
        Assert.IsFalse(float.IsNormal(minSubnormal));
        Assert.IsTrue(float.IsSubnormal(minSubnormal));
        Assert.IsFalse(float.IsNormal(-minSubnormal));
        Assert.IsTrue(float.IsSubnormal(-minSubnormal));
        Assert.IsFalse(float.IsNormal(0f));
        Assert.IsFalse(float.IsSubnormal(0f));
        Assert.IsFalse(float.IsNormal(float.NaN));
        Assert.IsFalse(float.IsSubnormal(float.NaN));
        Assert.IsFalse(float.IsNormal(float.PositiveInfinity));
        Assert.IsFalse(float.IsSubnormal(float.PositiveInfinity));
    }

    [TestMethod]
    public void Single_SinCosAndSinCosPi_Behavior_MatchesNetRuntime()
    {
        var (sin, cos) = float.SinCos(0.5f);
        var (sinPi, cosPi) = float.SinCosPi(0.5f);

        Assert.AreEqual(MathF.Sin(0.5f), sin, 1e-6f);
        Assert.AreEqual(MathF.Cos(0.5f), cos, 1e-6f);
        Assert.AreEqual(1f, sinPi, 1e-6f);
        Assert.AreEqual(0f, cosPi, 1e-6f);
    }
}
