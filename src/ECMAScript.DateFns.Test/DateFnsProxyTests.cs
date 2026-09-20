using System.Reflection;

using ECMAScript;

#pragma warning disable CA1416

namespace ECMAScriptDateFnsTest;

/// <summary>
/// Reflection checks for the date-fns host surface: import hosts, strongly typed shapes,
/// and the repository rule that public parameters and returns never degrade to object.
/// 宿主表面反射检查：import 宿主、强类型形状与禁用 object 万能参数的仓库规则。
/// </summary>
[TestClass]
public sealed class DateFnsProxyTests
{
    [TestMethod]
    public void DateFns_ImportHosts_UseDateFnsModuleSpecifiers()
    {
        AssertEcmaScriptImport(typeof(DateFns), "date-fns");
        AssertEcmaScriptImport(typeof(DateFnsLocale), "date-fns/locale");
    }

    [TestMethod]
    public void DateFns_CoreRuntimeShapes_DoNotExposeObject()
    {
        var runtimeTypes = new[]
        {
            typeof(DateFns),
            typeof(DateFnsLocale),
            typeof(DateFnsLocaleObject),
            typeof(DateFnsDuration),
            typeof(DateFnsInterval),
            typeof(DateFnsFormatOptions),
            typeof(DateFnsParseOptions),
            typeof(DateFnsParseISOOptions),
            typeof(DateFnsFormatISOOptions),
            typeof(DateFnsFormatDistanceOptions),
            typeof(DateFnsFormatDistanceStrictOptions),
            typeof(DateFnsFormatDurationOptions),
            typeof(DateFnsFormatRelativeOptions),
            typeof(DateFnsRoundingOptions),
            typeof(DateFnsEachDayOfIntervalOptions),
            typeof(DateFnsAreIntervalsOverlappingOptions),
            typeof(DateFnsDefaultOptions)
        };

        foreach (var type in runtimeTypes)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                AssertNotObject(property.PropertyType, $"{type.Name}.{property.Name}");

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(static method => !method.IsSpecialName)
                         .Where(static method => method.Name is not ("Equals" or "GetHashCode" or "ToString" or "PrintMembers" or "<Clone>$")))
            {
                AssertNotObject(method.ReturnType, $"{type.Name}.{method.Name} return");
                foreach (var parameter in method.GetParameters())
                    AssertNotObject(parameter.ParameterType, $"{type.Name}.{method.Name}({parameter.Name})");
            }
        }
    }

    [TestMethod]
    public void DateFns_StaticApi_ExposesTheFirstSliceSurface()
    {
        var methods = typeof(DateFns)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        // Parsing / formatting / arithmetic / comparison / interval / defaults must stay bound with
        // the exact strongly typed shapes locked by this slice.
        // 首期切片的解析/格式化/加减/比较/区间/默认选项表面必须保持锁定的强类型形状。
        RequiredStatic(methods, nameof(DateFns.ToDate), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date) }));
        RequiredStatic(methods, nameof(DateFns.Parse), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(string), typeof(string), typeof(Date), typeof(DateFnsParseOptions) }));
        RequiredStatic(methods, nameof(DateFns.ParseISO), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(string), typeof(DateFnsParseISOOptions) }));
        RequiredStatic(methods, nameof(DateFns.ParseJSON), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string) }));
        RequiredStatic(methods, nameof(DateFns.FromUnixTime), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Number) }));

        RequiredStatic(methods, nameof(DateFns.Format), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(string), typeof(DateFnsFormatOptions) }));
        RequiredStatic(methods, nameof(DateFns.FormatISO), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(DateFnsFormatISOOptions) }));
        RequiredStatic(methods, nameof(DateFns.FormatDistance), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(Date), typeof(DateFnsFormatDistanceOptions) }));
        RequiredStatic(methods, nameof(DateFns.FormatDistanceStrict), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(Date), typeof(DateFnsFormatDistanceStrictOptions) }));
        RequiredStatic(methods, nameof(DateFns.FormatDistanceToNow), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(DateFnsFormatDistanceOptions) }));
        RequiredStatic(methods, nameof(DateFns.FormatDistanceToNowStrict), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(DateFnsFormatDistanceStrictOptions) }));
        RequiredStatic(methods, nameof(DateFns.FormatDuration), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(DateFnsDuration), typeof(DateFnsFormatDurationOptions) }));
        RequiredStatic(methods, nameof(DateFns.FormatRelative), static method =>
            method.ReturnType == typeof(string) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(Date), typeof(DateFnsFormatRelativeOptions) }));

        RequiredStatic(methods, nameof(DateFns.Add), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(DateFnsDuration) }));
        RequiredStatic(methods, nameof(DateFns.Sub), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(DateFnsDuration) }));

        RequiredStatic(methods, nameof(DateFns.IsBefore), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date), typeof(Date) }));
        RequiredStatic(methods, nameof(DateFns.IsAfter), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date), typeof(Date) }));
        RequiredStatic(methods, nameof(DateFns.IsEqual), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date), typeof(Date) }));
        RequiredStatic(methods, nameof(DateFns.IsValid), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date) }));
        RequiredStatic(methods, nameof(DateFns.CompareAsc), static method =>
            method.ReturnType == typeof(Number) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date), typeof(Date) }));

        RequiredStatic(methods, nameof(DateFns.DifferenceInMilliseconds), static method =>
            method.ReturnType == typeof(Number) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date), typeof(Date) }));
        RequiredStatic(methods, nameof(DateFns.DifferenceInSeconds), static method =>
            method.ReturnType == typeof(Number) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Date), typeof(Date), typeof(DateFnsRoundingOptions) }));
        RequiredStatic(methods, nameof(DateFns.IntervalToDuration), static method =>
            method.ReturnType == typeof(DateFnsDuration) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(DateFnsInterval) }));
        RequiredStatic(methods, nameof(DateFns.IsWithinInterval), static method =>
            method.ReturnType == typeof(bool) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date), typeof(DateFnsInterval) }));
        RequiredStatic(methods, nameof(DateFns.EachDayOfInterval), static method =>
            method.ReturnType == typeof(Date[]) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(DateFnsInterval), typeof(DateFnsEachDayOfIntervalOptions) }));
        RequiredStatic(methods, nameof(DateFns.Min), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date[]) }));
        RequiredStatic(methods, nameof(DateFns.Max), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date[]) }));
        RequiredStatic(methods, nameof(DateFns.Clamp), static method =>
            method.ReturnType == typeof(Date) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Date), typeof(DateFnsInterval) }));

        RequiredStatic(methods, nameof(DateFns.GetDefaultOptions), static method =>
            method.ReturnType == typeof(DateFnsDefaultOptions) &&
            method.GetParameters().Length == 0);
        RequiredStatic(methods, nameof(DateFns.SetDefaultOptions), static method =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(DateFnsDefaultOptions) }));
    }

    [TestMethod]
    public void DateFns_StringEnums_AndNumericDomains_MirrorUpstreamValues()
    {
        // Numeric domains erase to the upstream scalar constants at usage sites.
        CollectionAssert.AreEquivalent(
            new[] { 0, 1, 2, 3, 4, 5, 6 },
            Enum.GetValues<DateFnsWeekStartsOn>().Select(static value => (int)value).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { 1, 4 },
            Enum.GetValues<DateFnsFirstWeekContainsDate>().Select(static value => (int)value).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { 0, 1, 2 },
            Enum.GetValues<DateFnsAdditionalDigits>().Select(static value => (int)value).ToArray());

        AssertStringEnumMember<DateFnsRoundingMethod>("ceil", nameof(DateFnsRoundingMethod.Ceil));
        AssertStringEnumMember<DateFnsRoundingMethod>("trunc", nameof(DateFnsRoundingMethod.Trunc));
        AssertStringEnumMember<DateFnsDistanceUnit>("second", nameof(DateFnsDistanceUnit.Second));
        AssertStringEnumMember<DateFnsDistanceUnit>("quarter", nameof(DateFnsDistanceUnit.Quarter));
        AssertStringEnumMember<DateFnsISOFormat>("basic", nameof(DateFnsISOFormat.Basic));
        AssertStringEnumMember<DateFnsISORepresentation>("complete", nameof(DateFnsISORepresentation.Complete));
    }

    [TestMethod]
    public void DateFns_LocaleContract_ExposesCuratedLocalesOnly()
    {
        var properties = typeof(DateFnsLocale)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        Assert.AreEqual(12, properties.Length);
        foreach (var property in properties)
            Assert.AreEqual(typeof(DateFnsLocaleObject), property.PropertyType, property.Name);

        Assert.IsNotNull(typeof(DateFnsLocale).GetProperty(nameof(DateFnsLocale.EnUS)));
        Assert.IsNotNull(typeof(DateFnsLocale).GetProperty(nameof(DateFnsLocale.ZhCN)));
        Assert.IsNotNull(typeof(DateFnsLocale).GetProperty(nameof(DateFnsLocale.Ja)));
        Assert.IsNotNull(typeof(DateFnsLocale).GetProperty(nameof(DateFnsLocale.PtBR)));
    }

    private static void AssertStringEnumMember<TEnum>(string javaScriptValue, string memberName)
        where TEnum : struct, Enum
    {
        Assert.IsNotNull(typeof(TEnum).GetCustomAttribute<ECMAScript.StringAttribute>(), typeof(TEnum).FullName);
        var description = typeof(TEnum).GetField(memberName)!
            .GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()!.Description;
        Assert.AreEqual("@#" + javaScriptValue, description, $"{typeof(TEnum).Name}.{memberName}");
    }

    private static void RequiredStatic(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
    {
        var matches = methods.Where(method => method.Name == name && predicate(method)).ToArray();
        Assert.IsTrue(matches.Length == 1, $"Expected exactly one strongly typed '{name}' overload, found {matches.Length}.");
    }

    private static void AssertEcmaScriptImport(Type type, string expectedImport)
    {
        var runtime = type.GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(runtime, type.FullName);
        Assert.AreEqual(expectedImport, runtime!.Import, type.FullName);
    }

    private static void AssertNotObject(Type type, string location)
    {
        // isDate(value: unknown) is the one intentional unknown-domain parameter; it follows the
        // Global.TypeOf host convention and stays excluded from this sweep.
        // isDate 的 unknown 值域参数是唯一例外，沿用 Global.TypeOf 的宿主约定。
        if (location.Contains(nameof(DateFns.IsDate), StringComparison.Ordinal))
            return;

        Assert.IsFalse(
            type == typeof(object),
            $"{location} must stay strongly typed; object is reserved for the isDate unknown-domain parameter.");
    }
}
