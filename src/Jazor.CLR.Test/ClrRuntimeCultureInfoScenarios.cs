namespace Jazor.CLR.Test;

internal static class ClrRuntimeCultureInfoScenarios
{
    private const string ModulePath = "System/Globalization/CultureInfoModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("culture-info.ctor.name-canonical", "System.Globalization.CultureInfo.CultureInfo(string)", [Text("en-us")], Text("en-US")),
        Success("culture-info.ctor.name-user-override", "System.Globalization.CultureInfo.CultureInfo(string, bool)", [Text("de-DE"), Bool(false)], Text("de-DE")),
        Failure("culture-info.ctor.lcid", "System.Globalization.CultureInfo.CultureInfo(int)", [Number(1033)], "NotSupportedException"),
        Failure("culture-info.ctor.lcid-user-override", "System.Globalization.CultureInfo.CultureInfo(int, bool)", [Number(1033), Bool(true)], "NotSupportedException"),
        Success("culture-info.create-specific-neutral", "static System.Globalization.CultureInfo.CreateSpecificCulture(string)", [Text("en")], Text("en-US")),
        Success("culture-info.current-culture-self-equals", "override System.Globalization.CultureInfo.Equals(object)", [CurrentCulture(), CurrentCulture()], Bool(true)),
        Success("culture-info.current-ui-culture-self-equals", "override System.Globalization.CultureInfo.Equals(object)", [CurrentUICulture(), CurrentUICulture()], Bool(true)),
        Success("culture-info.installed-ui-culture-self-equals", "override System.Globalization.CultureInfo.Equals(object)", [InstalledUICulture(), InstalledUICulture()], Bool(true)),
        Success("culture-info.invariant", "static System.Globalization.CultureInfo.InvariantCulture.get", [], Text("")),
        Success("culture-info.parent-region", "virtual System.Globalization.CultureInfo.Parent.get", [Text("en-US")], Text("en")),
        Success("culture-info.parent-script-region", "virtual System.Globalization.CultureInfo.Parent.get", [Text("zh-CN")], Text("zh-Hans")),
        Success("culture-info.ietf-invariant", "System.Globalization.CultureInfo.IetfLanguageTag.get", [Text("")], Text("")),
        Success("culture-info.display-invariant", "virtual System.Globalization.CultureInfo.DisplayName.get", [Text("")], Text("Invariant Language (Invariant Country)")),
        Success("culture-info.native-invariant", "virtual System.Globalization.CultureInfo.NativeName.get", [Text("")], Text("Invariant Language (Invariant Country)")),
        Success("culture-info.english-invariant", "virtual System.Globalization.CultureInfo.EnglishName.get", [Text("")], Text("Invariant Language (Invariant Country)")),
        Success("culture-info.two-letter-language", "virtual System.Globalization.CultureInfo.TwoLetterISOLanguageName.get", [Text("en-US")], Text("en")),
        Success("culture-info.three-letter-iso", "virtual System.Globalization.CultureInfo.ThreeLetterISOLanguageName.get", [Text("zh-CN")], Text("zho")),
        Success("culture-info.three-letter-windows", "virtual System.Globalization.CultureInfo.ThreeLetterWindowsLanguageName.get", [Text("zh-CN")], Text("CHS")),
        Success("culture-info.equals-canonical", "override System.Globalization.CultureInfo.Equals(object)", [Text("en-us"), Text("en-US")], Bool(true)),
        Success("culture-info.equals-wrong-type", "override System.Globalization.CultureInfo.Equals(object)", [Text("en-US"), Number(1)], Bool(false)),
        Success("culture-info.hash-code", "override System.Globalization.CultureInfo.GetHashCode()", [Text("en-US")], Number(96598594)),
        Success("culture-info.to-string", "override System.Globalization.CultureInfo.ToString()", [Text("en-us")], Text("en-US")),
        Success("culture-info.neutral-language", "virtual System.Globalization.CultureInfo.IsNeutralCulture.get", [Text("en")], Bool(true)),
        Success("culture-info.neutral-region", "virtual System.Globalization.CultureInfo.IsNeutralCulture.get", [Text("en-US")], Bool(false)),
        Success("culture-info.clear-cached-data", "static System.Globalization.CultureInfo.ClearCachedData()", [], Undefined()),
        Success("culture-info.calendar", "virtual System.Globalization.CultureInfo.Calendar.get", [Text("en-US")], Text("System.Globalization.GregorianCalendar")),
        Success("culture-info.optional-calendars", "virtual System.Globalization.CultureInfo.OptionalCalendars.get", [Text("en-US")], Array(Text("System.Globalization.GregorianCalendar"))),
        Success("culture-info.use-user-override", "System.Globalization.CultureInfo.UseUserOverride.get", [Text("en-US")], Bool(false)),
        Success("culture-info.console-fallback-iv", "System.Globalization.CultureInfo.GetConsoleFallbackUICulture()", [Text("iv")], Text("")),
        Success("culture-info.clone", "virtual System.Globalization.CultureInfo.Clone()", [Text("en-US")], Text("en-US")),
        Success("culture-info.read-only", "static System.Globalization.CultureInfo.ReadOnly(System.Globalization.CultureInfo)", [Text("en-US")], Text("en-US")),
        Success("culture-info.is-read-only", "System.Globalization.CultureInfo.IsReadOnly.get", [Text("en-US")], Bool(true)),
        Failure("culture-info.get-culture-info.lcid", "static System.Globalization.CultureInfo.GetCultureInfo(int)", [Number(1033)], "NotSupportedException"),
        Success("culture-info.get-culture-info.name", "static System.Globalization.CultureInfo.GetCultureInfo(string)", [Text("fr-fr")], Text("fr-FR")),
        Success("culture-info.get-culture-info.alternate-name", "static System.Globalization.CultureInfo.GetCultureInfo(string, string)", [Text("fr-fr"), Text("en-US")], Text("fr-FR")),
        Success("culture-info.get-culture-info.predefined-only", "static System.Globalization.CultureInfo.GetCultureInfo(string, bool)", [Text("fr-fr"), Bool(true)], Text("fr-FR")),
        Success("culture-info.get-culture-info-ietf", "static System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(string)", [Text("iv")], Text("iv"))
    ];

    private static ClrRuntimeValue CurrentCulture()
        => Invoke("static System.Globalization.CultureInfo.CurrentCulture.get");

    private static ClrRuntimeValue CurrentUICulture()
        => Invoke("static System.Globalization.CultureInfo.CurrentUICulture.get");

    private static ClrRuntimeValue InstalledUICulture()
        => Invoke("static System.Globalization.CultureInfo.InstalledUICulture.get");

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Undefined() => ClrRuntimeValue.Undefined();
}
