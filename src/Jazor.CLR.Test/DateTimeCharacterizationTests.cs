using System.Globalization;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class DateTimeCharacterizationTests
{
    [TestMethod]
    public void DateTime_Parse_ZSuffix_DefaultAndRoundtripKind_MatchNetRuntime()
    {
        var parsedDefault = DateTime.Parse(
            "2024-01-02T03:04:05Z",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);
        var parsedRoundtrip = DateTime.Parse(
            "2024-01-02T03:04:05Z",
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);

        Assert.AreEqual(DateTimeKind.Local, parsedDefault.Kind);
        Assert.AreEqual(DateTimeKind.Utc, parsedRoundtrip.Kind);
    }

    [TestMethod]
    public void DateTime_Parse_TimeOnlyWithZulu_DefaultAndRoundtripKind_MatchNetRuntime()
    {
        var parsedDefault = DateTime.Parse(
            "12:34:56Z",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);
        var parsedRoundtrip = DateTime.Parse(
            "12:34:56Z",
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);

        Assert.AreEqual(DateTimeKind.Local, parsedDefault.Kind);
        Assert.AreEqual(DateTimeKind.Utc, parsedRoundtrip.Kind);
    }

    [TestMethod]
    public void DateTimeOffset_ProjectionMembers_ExposeExpectedDateTimeKinds()
    {
        var value = DateTimeOffset.Parse(
            "2024-01-02T03:04:05+08:00",
            CultureInfo.InvariantCulture);

        Assert.AreEqual(DateTimeKind.Unspecified, value.Date.Kind);
        Assert.AreEqual(DateTimeKind.Unspecified, value.DateTime.Kind);
        Assert.AreEqual(DateTimeKind.Utc, value.UtcDateTime.Kind);
        Assert.AreEqual(DateTimeKind.Local, value.LocalDateTime.Kind);
    }

    [TestMethod]
    public void DateTimeOffset_ToString_StandardFormats_Match_Runtime()
    {
        var value = DateTimeOffset.Parse(
            "2024-01-02T03:04:05+08:00",
            CultureInfo.InvariantCulture);

        Assert.AreEqual(
            "2024-01-02T03:04:05.0000000+08:00",
            value.ToString("O", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024-01-02T03:04:05.0000000+08:00",
            value.ToString("o", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "01/02/2024 03:04:05",
            value.ToString("G", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "01/02/2024 03:04",
            value.ToString("g", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "01/02/2024",
            value.ToString("d", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "Tuesday, 02 January 2024",
            value.ToString("D", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "03:04",
            value.ToString("t", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "03:04:05",
            value.ToString("T", CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void DateTime_ToString_InvariantCulture_CommonFormats_Match_Runtime()
    {
        var value = DateTime.SpecifyKind(
            DateTime.Parse(
                "2024-01-02T03:04:05.1234567",
                CultureInfo.InvariantCulture),
            DateTimeKind.Unspecified);

        Assert.AreEqual(
            "01/02/2024 03:04:05",
            value.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "01/02/2024 03:04:05",
            value.ToString("G", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "01/02/2024 03:04",
            value.ToString("g", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024-01-02T03:04:05",
            value.ToString("s", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024-01-02 03:04:05Z",
            value.ToString("u", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024-01-02T03:04:05.1234567",
            value.ToString("O", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024-01-02",
            value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "03:04:05",
            value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "01/02/2024",
            value.ToString("d", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "Tuesday, 02 January 2024",
            value.ToString("D", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "03:04",
            value.ToString("t", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "03:04:05",
            value.ToString("T", CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void DateTime_ToString_InvariantCulture_AdditionalStandardAndPercentFormats_Match_Runtime()
    {
        var value = DateTime.SpecifyKind(
            DateTime.Parse(
                "2024-01-02T03:04:05.1234000",
                CultureInfo.InvariantCulture),
            DateTimeKind.Utc);

        Assert.AreEqual(
            "Tuesday, 02 January 2024 03:04",
            value.ToString("f", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "Tuesday, 02 January 2024 03:04:05",
            value.ToString("F", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "January 02",
            value.ToString("m", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024 January",
            value.ToString("y", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "Tue, 02 Jan 2024 03:04:05 GMT",
            value.ToString("r", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "Tuesday, 02 January 2024 03:04:05",
            value.ToString("U", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "Z",
            value.ToString("%K", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "+0",
            value.ToString("%z", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "1",
            value.ToString("%F", CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void DateTime_ToString_SingleCustomLetters_RequirePercentEscape()
    {
        var value = DateTime.SpecifyKind(
            DateTime.Parse(
                "2024-01-02T03:04:05.1234000",
                CultureInfo.InvariantCulture),
            DateTimeKind.Utc);

        Assert.ThrowsExactly<FormatException>(() => value.ToString("K", CultureInfo.InvariantCulture));
        Assert.ThrowsExactly<FormatException>(() => value.ToString("z", CultureInfo.InvariantCulture));
        Assert.ThrowsExactly<FormatException>(() => value.ToString("%", CultureInfo.InvariantCulture));
        Assert.ThrowsExactly<FormatException>(() => value.ToString("%%", CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void DateTimeOffset_ToString_InvariantCulture_DefaultAndCustomFormats_Match_Runtime()
    {
        var value = DateTimeOffset.Parse(
            "2024-01-02T03:04:05.1234567+08:00",
            CultureInfo.InvariantCulture);

        Assert.AreEqual(
            "01/02/2024 03:04:05 +08:00",
            value.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024-01-01 19:04:05Z",
            value.ToString("u", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024-01-02T03:04:05",
            value.ToString("s", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024-01-02",
            value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "03:04:05",
            value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "01/02/2024",
            value.ToString("d", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "Tuesday, 02 January 2024",
            value.ToString("D", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "03:04",
            value.ToString("t", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "03:04:05",
            value.ToString("T", CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void DateTimeOffset_ToString_InvariantCulture_AdditionalStandardAndPercentFormats_Match_Runtime()
    {
        var value = DateTimeOffset.Parse(
            "2024-01-02T03:04:05.1234567+08:00",
            CultureInfo.InvariantCulture);

        Assert.AreEqual(
            "Tuesday, 02 January 2024 03:04",
            value.ToString("f", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "Tuesday, 02 January 2024 03:04:05",
            value.ToString("F", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "January 02",
            value.ToString("m", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "2024 January",
            value.ToString("y", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "Mon, 01 Jan 2024 19:04:05 GMT",
            value.ToString("r", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "+08:00",
            value.ToString("%K", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "+8",
            value.ToString("%z", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "1",
            value.ToString("%F", CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void DateTimeOffset_ToString_SingleCustomLetters_RequirePercentEscape()
    {
        var value = DateTimeOffset.Parse(
            "2024-01-02T03:04:05.1234567+08:00",
            CultureInfo.InvariantCulture);

        Assert.ThrowsExactly<FormatException>(() => value.ToString("K", CultureInfo.InvariantCulture));
        Assert.ThrowsExactly<FormatException>(() => value.ToString("z", CultureInfo.InvariantCulture));
        Assert.ThrowsExactly<FormatException>(() => value.ToString("%", CultureInfo.InvariantCulture));
        Assert.ThrowsExactly<FormatException>(() => value.ToString("%%", CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void DateTime_ToString_CustomFormat_UsesProviderMonthDayNamesAndSeparators()
    {
        var culture = CultureInfo.GetCultureInfo("de-DE");
        var value = DateTime.SpecifyKind(
            DateTime.Parse(
                "2024-01-02T03:04:05.1234000",
                CultureInfo.InvariantCulture),
            DateTimeKind.Utc);

        Assert.AreEqual(
            "Dienstag, Januar 02 . 03:04",
            value.ToString("dddd, MMMM dd / HH:mm", culture));
        Assert.AreEqual(
            "Di, Jan. 02 . 03:04",
            value.ToString("ddd, MMM dd / HH:mm", culture));
        Assert.AreEqual(
            "02.01.2024",
            value.ToString("dd/MM/yyyy", culture));
        Assert.AreEqual(
            "03:04 AM",
            value.ToString("hh:mm tt", culture));
    }

    [TestMethod]
    public void DateTimeOffset_ToString_CustomFormat_UsesProviderMonthDayNamesAndSeparators()
    {
        var culture = CultureInfo.GetCultureInfo("de-DE");
        var value = DateTimeOffset.Parse(
            "2024-01-02T03:04:05.1234567+08:00",
            CultureInfo.InvariantCulture);

        Assert.AreEqual(
            "Dienstag, Januar 02 . 03:04",
            value.ToString("dddd, MMMM dd / HH:mm", culture));
        Assert.AreEqual(
            "Di, Jan. 02 . 03:04",
            value.ToString("ddd, MMM dd / HH:mm", culture));
        Assert.AreEqual(
            "02.01.2024",
            value.ToString("dd/MM/yyyy", culture));
        Assert.AreEqual(
            "03:04 AM",
            value.ToString("hh:mm tt", culture));
    }

    [TestMethod]
    public void DateTime_ToString_Custom12HourTokens_Match_Runtime()
    {
        var value = DateTime.SpecifyKind(
            DateTime.Parse(
                "2024-01-02T15:04:05.1234000",
                CultureInfo.InvariantCulture),
            DateTimeKind.Utc);

        Assert.AreEqual(
            "03:04 PM",
            value.ToString("hh:mm tt", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "3:4 P",
            value.ToString("h:m t", CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void DateTimeOffset_ToString_Custom12HourTokens_Match_Runtime()
    {
        var value = DateTimeOffset.Parse(
            "2024-01-02T15:04:05.1234567+08:00",
            CultureInfo.InvariantCulture);

        Assert.AreEqual(
            "03:04 PM",
            value.ToString("hh:mm tt", CultureInfo.InvariantCulture));
        Assert.AreEqual(
            "3:4 P",
            value.ToString("h:m t", CultureInfo.InvariantCulture));
    }
}
