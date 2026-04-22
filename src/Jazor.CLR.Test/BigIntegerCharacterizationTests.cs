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
}
