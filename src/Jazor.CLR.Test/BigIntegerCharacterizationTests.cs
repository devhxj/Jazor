using System.Numerics;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class BigIntegerCharacterizationTests
{
    [TestMethod]
    public void BigInteger_MaxMinMagnitude_TieBehavior_MatchesNetRuntime()
    {
        var maxNegPos = BigInteger.MaxMagnitude(new BigInteger(-3), new BigInteger(3));
        var maxPosNeg = BigInteger.MaxMagnitude(new BigInteger(3), new BigInteger(-3));
        var minNegPos = BigInteger.MinMagnitude(new BigInteger(-3), new BigInteger(3));
        var minPosNeg = BigInteger.MinMagnitude(new BigInteger(3), new BigInteger(-3));

        Assert.AreEqual(new BigInteger(3), maxNegPos);
        Assert.AreEqual(new BigInteger(3), maxPosNeg);
        Assert.AreEqual(new BigInteger(-3), minNegPos);
        Assert.AreEqual(new BigInteger(-3), minPosNeg);
    }

    [TestMethod]
    public void BigInteger_LeadingZeroCount_MatchesNetRuntime()
    {
        Assert.AreEqual(new BigInteger(64), BigInteger.LeadingZeroCount(BigInteger.Zero));
        Assert.AreEqual(new BigInteger(63), BigInteger.LeadingZeroCount(BigInteger.One));
        Assert.AreEqual(new BigInteger(62), BigInteger.LeadingZeroCount(new BigInteger(2)));
        Assert.AreEqual(new BigInteger(56), BigInteger.LeadingZeroCount(new BigInteger(255)));
        Assert.AreEqual(new BigInteger(55), BigInteger.LeadingZeroCount(new BigInteger(256)));
        Assert.AreEqual(BigInteger.Zero, BigInteger.LeadingZeroCount(BigInteger.MinusOne));
        Assert.AreEqual(new BigInteger(31), BigInteger.LeadingZeroCount(BigInteger.One << 32));
    }

    [TestMethod]
    public void BigInteger_LogAndLog10_DocumentedSpecialCases_MatchNetRuntime()
    {
        Assert.AreEqual(double.NegativeInfinity, BigInteger.Log(BigInteger.Zero));
        Assert.IsTrue(double.IsNaN(BigInteger.Log(BigInteger.MinusOne)));
        Assert.AreEqual(0d, BigInteger.Log(BigInteger.One, double.PositiveInfinity));
        Assert.AreEqual(0d, BigInteger.Log(BigInteger.One, 0d));
        Assert.IsTrue(double.IsNaN(BigInteger.Log(new BigInteger(10), 0d)));
        Assert.AreEqual(20d, BigInteger.Log10(BigInteger.Parse("100000000000000000000")), 1e-12);
        Assert.AreEqual(20d, BigInteger.Log(BigInteger.Parse("100000000000000000000"), 10d), 1e-12);
    }

    [TestMethod]
    public void BigInteger_ModPow_NegativeBaseSignBehavior_MatchesNetRuntime()
    {
        Assert.AreEqual(new BigInteger(-2), BigInteger.ModPow(new BigInteger(-3), new BigInteger(3), new BigInteger(5)));
        Assert.AreEqual(new BigInteger(-3), BigInteger.ModPow(new BigInteger(-2), new BigInteger(3), new BigInteger(5)));
        Assert.AreEqual(BigInteger.One, BigInteger.ModPow(new BigInteger(-2), new BigInteger(4), new BigInteger(5)));
    }
}
