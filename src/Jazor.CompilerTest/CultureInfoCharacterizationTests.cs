using System.Globalization;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class CultureInfoCharacterizationTests
{
    [TestMethod]
    public void CultureInfo_EnglishName_And_NativeName_Match_Runtime_ForRepresentativeCultures()
    {
        var english = new CultureInfo("en-US");
        var chinese = new CultureInfo("zh-CN");
        var serbian = new CultureInfo("sr-Cyrl-RS");

        Assert.AreEqual("English (United States)", english.EnglishName);
        Assert.AreEqual("English (United States)", english.NativeName);
        Assert.AreEqual("Chinese (China)", chinese.EnglishName);
        Assert.AreEqual("中文（中国）", chinese.NativeName);
        Assert.AreEqual("Serbian (Cyrillic, Serbia)", serbian.EnglishName);
        Assert.AreEqual("српски (ћирилица, Србија)", serbian.NativeName);
    }

    [TestMethod]
    public void CultureInfo_Parent_Matches_Runtime_ForScriptAndRegionCultures()
    {
        Assert.AreEqual("en", new CultureInfo("en-US").Parent.Name);
        Assert.AreEqual("zh-Hans", new CultureInfo("zh-CN").Parent.Name);
        Assert.AreEqual("zh-Hant", new CultureInfo("zh-TW").Parent.Name);
        Assert.AreEqual("sr-Cyrl", new CultureInfo("sr-Cyrl-RS").Parent.Name);
        Assert.AreEqual("sr", new CultureInfo("sr-Cyrl").Parent.Name);
        Assert.AreEqual(string.Empty, new CultureInfo("fr").Parent.Name);
    }

    [TestMethod]
    public void CultureInfo_InvariantCulture_CoreMetadata_Matches_Runtime()
    {
        var invariant = CultureInfo.InvariantCulture;

        Assert.AreEqual(string.Empty, invariant.Name);
        Assert.AreEqual("Invariant Language (Invariant Country)", invariant.DisplayName);
        Assert.AreEqual("Invariant Language (Invariant Country)", invariant.NativeName);
        Assert.AreEqual("Invariant Language (Invariant Country)", invariant.EnglishName);
        Assert.AreEqual("iv", invariant.TwoLetterISOLanguageName);
        Assert.AreEqual("ivl", invariant.ThreeLetterISOLanguageName);
        Assert.AreEqual("IVL", invariant.ThreeLetterWindowsLanguageName);
        Assert.AreEqual(string.Empty, invariant.Parent.Name);
        Assert.IsFalse(invariant.IsNeutralCulture);
    }

    [TestMethod]
    public void CultureInfo_ThreeLetterLanguageCodes_Match_Runtime_ForRepresentativeCultures()
    {
        Assert.AreEqual("eng", new CultureInfo("en-US").ThreeLetterISOLanguageName);
        Assert.AreEqual("ENU", new CultureInfo("en-US").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("ENG", new CultureInfo("en-GB").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("zho", new CultureInfo("zh-CN").ThreeLetterISOLanguageName);
        Assert.AreEqual("CHS", new CultureInfo("zh-CN").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("CHT", new CultureInfo("zh-TW").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("ZHH", new CultureInfo("zh-HK").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("fra", new CultureInfo("fr-FR").ThreeLetterISOLanguageName);
        Assert.AreEqual("FRA", new CultureInfo("fr-FR").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("deu", new CultureInfo("de-DE").ThreeLetterISOLanguageName);
        Assert.AreEqual("DEU", new CultureInfo("de-DE").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("ara", new CultureInfo("ar-EG").ThreeLetterISOLanguageName);
        Assert.AreEqual("ARE", new CultureInfo("ar-EG").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("ara", new CultureInfo("ar-SA").ThreeLetterISOLanguageName);
        Assert.AreEqual("ARA", new CultureInfo("ar-SA").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("spa", new CultureInfo("es-ES").ThreeLetterISOLanguageName);
        Assert.AreEqual("ESN", new CultureInfo("es-ES").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("por", new CultureInfo("pt-BR").ThreeLetterISOLanguageName);
        Assert.AreEqual("PTB", new CultureInfo("pt-BR").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("nob", new CultureInfo("nb-NO").ThreeLetterISOLanguageName);
        Assert.AreEqual("NOR", new CultureInfo("nb-NO").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("nno", new CultureInfo("nn-NO").ThreeLetterISOLanguageName);
        Assert.AreEqual("NON", new CultureInfo("nn-NO").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("swe", new CultureInfo("sv-SE").ThreeLetterISOLanguageName);
        Assert.AreEqual("SVE", new CultureInfo("sv-SE").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("tur", new CultureInfo("tr-TR").ThreeLetterISOLanguageName);
        Assert.AreEqual("TRK", new CultureInfo("tr-TR").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("pol", new CultureInfo("pl-PL").ThreeLetterISOLanguageName);
        Assert.AreEqual("PLK", new CultureInfo("pl-PL").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("ces", new CultureInfo("cs-CZ").ThreeLetterISOLanguageName);
        Assert.AreEqual("CSY", new CultureInfo("cs-CZ").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("srp", new CultureInfo("sr-Cyrl-RS").ThreeLetterISOLanguageName);
        Assert.AreEqual("SRO", new CultureInfo("sr-Cyrl-RS").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("SRM", new CultureInfo("sr-Latn-RS").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("aze", new CultureInfo("az-Cyrl-AZ").ThreeLetterISOLanguageName);
        Assert.AreEqual("AZC", new CultureInfo("az-Cyrl-AZ").ThreeLetterWindowsLanguageName);
        Assert.AreEqual("bos", new CultureInfo("bs-Cyrl-BA").ThreeLetterISOLanguageName);
        Assert.AreEqual("BSC", new CultureInfo("bs-Cyrl-BA").ThreeLetterWindowsLanguageName);
    }

    [TestMethod]
    public void CultureInfo_Iv_SpecialCase_Matches_Runtime()
    {
        var iv = new CultureInfo("iv");

        Assert.AreEqual("iv", iv.Name);
        Assert.AreEqual("iv", iv.DisplayName);
        Assert.AreEqual("iv", iv.NativeName);
        Assert.AreEqual("iv", iv.EnglishName);
        Assert.AreEqual("iv", iv.IetfLanguageTag);
        Assert.AreEqual("iv", iv.TwoLetterISOLanguageName);
        Assert.AreEqual(string.Empty, iv.ThreeLetterISOLanguageName);
        Assert.AreEqual("ZZZ", iv.ThreeLetterWindowsLanguageName);
        Assert.AreEqual(string.Empty, iv.Parent.Name);
        Assert.IsTrue(iv.IsNeutralCulture);
    }
}
