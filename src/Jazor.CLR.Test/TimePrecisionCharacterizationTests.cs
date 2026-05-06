namespace Jazor.CLR.Test;

[TestClass]
public sealed class TimePrecisionCharacterizationTests
{
    [TestMethod]
    public void TimeSpan_FromDoubleFactoryMethods_UseTickPrecisionAndTruncation()
    {
        Assert.AreEqual(864L, TimeSpan.FromDays(0.000000001d).Ticks);
        Assert.AreEqual(36L, TimeSpan.FromHours(0.000000001d).Ticks);
        Assert.AreEqual(5L, TimeSpan.FromMilliseconds(0.0006d).Ticks);
        Assert.AreEqual(0L, TimeSpan.FromMilliseconds(0.00005d).Ticks);
        Assert.AreEqual(0L, TimeSpan.FromMilliseconds(-0.00005d).Ticks);
        Assert.AreEqual(5L, TimeSpan.FromMicroseconds(0.55d).Ticks);
        Assert.AreEqual(-5L, TimeSpan.FromMicroseconds(-0.55d).Ticks);
    }

    [TestMethod]
    public void TimeSpan_DoubleOperators_RoundToNearestTickWithMidpointToEven()
    {
        var large = TimeSpan.FromTicks(9_007_199_254_740_993L);

        Assert.AreEqual(13_510_798_882_111_488L, (large * 1.5d).Ticks);
        Assert.AreEqual(3_002_399_751_580_330L, (large / 3d).Ticks);

        Assert.AreEqual(0L, (TimeSpan.FromTicks(1) * 0.5d).Ticks);
        Assert.AreEqual(2L, (TimeSpan.FromTicks(1) * 1.5d).Ticks);
        Assert.AreEqual(0L, (TimeSpan.FromTicks(-1) * 0.5d).Ticks);
        Assert.AreEqual(-2L, (TimeSpan.FromTicks(-1) * 1.5d).Ticks);
    }

    [TestMethod]
    public void DateTime_AddDoubleUnits_UseTickPrecision()
    {
        var value = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        Assert.AreEqual(864L, value.AddDays(0.000000001d).Ticks - value.Ticks);
        Assert.AreEqual(36L, value.AddHours(0.000000001d).Ticks - value.Ticks);
        Assert.AreEqual(5L, value.AddMicroseconds(0.55d).Ticks - value.Ticks);
        Assert.AreEqual(0L, value.AddMilliseconds(0.00005d).Ticks - value.Ticks);
    }

    [TestMethod]
    public void DateTimeOffset_AddDoubleUnits_UseTickPrecision()
    {
        var value = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

        Assert.AreEqual(864L, value.AddDays(0.000000001d).Ticks - value.Ticks);
        Assert.AreEqual(36L, value.AddHours(0.000000001d).Ticks - value.Ticks);
        Assert.AreEqual(5L, value.AddMicroseconds(0.55d).Ticks - value.Ticks);
        Assert.AreEqual(0L, value.AddMilliseconds(0.00005d).Ticks - value.Ticks);
    }

    [TestMethod]
    public void TimeOnly_AddDoubleUnits_UseTickPrecisionAndWrapByDay()
    {
        var value = new TimeOnly(0, 0);

        Assert.AreEqual(36L, value.AddHours(0.000000001d).Ticks);
        Assert.AreEqual(TimeSpan.TicksPerDay - 36L, value.AddHours(-0.000000001d).Ticks);
        var wrapped = value.AddHours(-0.000000001d, out var wrappedDays);
        Assert.AreEqual(TimeSpan.TicksPerDay - 36L, wrapped.Ticks);
        Assert.AreEqual(-1, wrappedDays);
    }
}
