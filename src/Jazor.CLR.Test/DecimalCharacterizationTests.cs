using System.Globalization;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class DecimalCharacterizationTests
{
    [TestMethod]
    public void Decimal_Parse_DefaultStyle_RejectsExponent_But_AllowsThousandsAndWhitespace()
    {
        Assert.ThrowsExactly<FormatException>(
            () => decimal.Parse("1e2", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            1234m,
            decimal.Parse("1,234", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            123.45m,
            decimal.Parse("  123.45  ", CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void Decimal_Parse_And_TryParse_With_NumberStyles_Follow_RuntimeRules()
    {
        Assert.ThrowsExactly<FormatException>(
            () => decimal.Parse(
                "1e2",
                NumberStyles.Number,
                CultureInfo.InvariantCulture));
        Assert.AreEqual(
            100m,
            decimal.Parse(
                "1e2",
                NumberStyles.Float,
                CultureInfo.InvariantCulture));

        Assert.IsFalse(
            decimal.TryParse(
                "1e2",
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out _));
        Assert.IsTrue(
            decimal.TryParse(
                "1e2",
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var exponentValue));
        Assert.AreEqual(100m, exponentValue);
    }

    [TestMethod]
    public void Decimal_ToString_Standard_And_Custom_Formats_Match_Runtime()
    {
        // decimal 的 "G" 格式会保留内部 scale，不会像 double 那样自动裁掉尾随零。
        Assert.AreEqual(
            "123.4500",
            123.4500m.ToString("G", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "123.46",
            123.456m.ToString("F2", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "1,234.50",
            1234.5m.ToString("N2", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "123.46",
            123.456m.ToString("0.00", CultureInfo.InvariantCulture));
    }
}
