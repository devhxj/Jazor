using System.Globalization;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class GregorianCalendarCharacterizationTests
{
    [TestMethod]
    public void GregorianCalendar_Constructor_PreservesCalendarType()
    {
        var calendar = new GregorianCalendar(GregorianCalendarTypes.USEnglish);

        Assert.AreEqual(GregorianCalendarTypes.USEnglish, calendar.CalendarType);
        Assert.AreEqual(2049, calendar.TwoDigitYearMax);
        Assert.AreEqual("System.Globalization.GregorianCalendar", calendar.ToString());
    }

    [TestMethod]
    public void GregorianCalendar_TwoDigitYearMax_AffectsToFourDigitYear()
    {
        var calendar = new GregorianCalendar();
        calendar.TwoDigitYearMax = 2099;

        Assert.AreEqual(2099, calendar.TwoDigitYearMax);
        Assert.AreEqual(2030, calendar.ToFourDigitYear(30));
    }
}
