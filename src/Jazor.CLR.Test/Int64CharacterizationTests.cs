namespace Jazor.CLR.Test;

[TestClass]
public sealed class Int64CharacterizationTests
{
    [TestMethod]
    public void Int64_MaxMinMagnitude_TieBehavior_MatchesNetRuntime()
    {
        var maxNegPos = long.MaxMagnitude(-3, 3);
        var maxPosNeg = long.MaxMagnitude(3, -3);
        var minNegPos = long.MinMagnitude(-3, 3);
        var minPosNeg = long.MinMagnitude(3, -3);

        Assert.AreEqual(3L, maxNegPos);
        Assert.AreEqual(3L, maxPosNeg);
        Assert.AreEqual(-3L, minNegPos);
        Assert.AreEqual(-3L, minPosNeg);
    }
}
