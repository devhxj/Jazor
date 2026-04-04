namespace Jazor.ComplierTest;

[TestClass]
public sealed class Int16CharacterizationTests
{
    [TestMethod]
    public void Int16_MaxMinMagnitude_TieBehavior_MatchesNetRuntime()
    {
        var maxNegPos = short.MaxMagnitude(-3, 3);
        var maxPosNeg = short.MaxMagnitude(3, -3);
        var minNegPos = short.MinMagnitude(-3, 3);
        var minPosNeg = short.MinMagnitude(3, -3);

        Assert.AreEqual((short)3, maxNegPos);
        Assert.AreEqual((short)3, maxPosNeg);
        Assert.AreEqual((short)-3, minNegPos);
        Assert.AreEqual((short)-3, minPosNeg);
    }
}
