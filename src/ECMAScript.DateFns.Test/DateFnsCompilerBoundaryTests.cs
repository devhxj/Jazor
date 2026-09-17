using System.Threading;
using Acornima.Ast;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptDateFnsTest;

/// <summary>
/// Compiler emission checks for date-fns authoring: module imports, option object literals,
/// enum-backed domains, locale imports, and duration/interval composition.
/// 编译器发射检查：模块导入、选项对象字面量、枚举值域、locale 导入与 Duration/Interval 组合。
/// </summary>
[TestClass]
public sealed class DateFnsCompilerBoundaryTests
{
    [TestMethod]
    public async Task DateFns_FormatWithOptions_EmitsNamedImportAndObjectLiteral()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.DateFns;
            using static ECMAScript.DateFnsLocale;

            public static class TestClass
            {
                public static string Build(Date date)
                {
                    return Format(date, "yyyy-MM-dd", new DateFnsFormatOptions
                    {
                        Locale = ZhCN,
                        WeekStartsOn = DateFnsWeekStartsOn.Monday
                    });
                }
            }
            """);

        StringAssert.Contains(script, "return format(date, \"yyyy-MM-dd\", { locale: zhCN, weekStartsOn: 1 });");
        AssertImport(script, "date-fns", "format");
        AssertImport(script, "date-fns/locale", "zhCN");
    }

    [TestMethod]
    public async Task DateFns_ParseAndArithmetic_EmitUpstreamCallShapes()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.DateFns;

            public static class TestClass
            {
                public static Date Build()
                {
                    var start = ParseISO("2026-09-17");
                    var shifted = AddDays(start, 3);
                    var moved = Add(start, new DateFnsDuration { Months = 1, Days = 2 });
                    var reduced = Sub(moved, new DateFnsDuration { Weeks = 1 });
                    return AddBusinessDays(reduced, 5);
                }
            }
            """);

        StringAssert.Contains(script, "let start = parseISO(\"2026-09-17\");");
        StringAssert.Contains(script, "let shifted = addDays(start, 3);");
        StringAssert.Contains(script, "let moved = add(start, { months: 1, days: 2 });");
        StringAssert.Contains(script, "let reduced = sub(moved, { weeks: 1 });");
        StringAssert.Contains(script, "return addBusinessDays(reduced, 5);");
        AssertImport(script, "date-fns", "parseISO", "addDays", "add", "sub", "addBusinessDays");
    }

    [TestMethod]
    public async Task DateFns_StringEnumOptions_EmitStringLiterals()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.DateFns;

            public static class TestClass
            {
                public static string Build(Date later, Date earlier)
                {
                    return FormatDistanceStrict(later, earlier, new DateFnsFormatDistanceStrictOptions
                    {
                        AddSuffix = true,
                        Unit = DateFnsDistanceUnit.Hour,
                        RoundingMethod = DateFnsRoundingMethod.Ceil
                    });
                }

                public static string Iso(Date date)
                {
                    return FormatISO(date, new DateFnsFormatISOOptions
                    {
                        Format = DateFnsISOFormat.Basic,
                        Representation = DateFnsISORepresentation.Date
                    });
                }
            }
            """);

        // 3+ property literals are emitted in the stable multi-line expanded form.
        StringAssert.Contains(script, """
            return formatDistanceStrict(later, earlier, {
                addSuffix: true,
                unit: "hour",
                roundingMethod: "ceil"
              });
            """);
        StringAssert.Contains(script, "return formatISO(date, { format: \"basic\", representation: \"date\" });");
        AssertImport(script, "date-fns", "formatDistanceStrict", "formatISO");
    }

    [TestMethod]
    public async Task DateFns_ComparisonAndInterval_EmitBooleanAndArrayResults()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.DateFns;

            public static class TestClass
            {
                public static bool Check(Date date, Date deadline)
                {
                    return IsBefore(date, deadline) && IsWithinInterval(date, new DateFnsInterval
                    {
                        Start = date,
                        End = deadline
                    });
                }

                public static Date[] ListDays()
                {
                    return EachDayOfInterval(
                        new DateFnsInterval { Start = ParseISO("2026-09-01"), End = ParseISO("2026-09-03") },
                        new DateFnsEachDayOfIntervalOptions { Step = 2 });
                }

                public static Date Latest(Date[] dates)
                {
                    return Max(dates);
                }
            }
            """);

        StringAssert.Contains(script, "return isBefore(date, deadline) && isWithinInterval(date, { start: date, end: deadline });");
        StringAssert.Contains(script, "return eachDayOfInterval({ start: parseISO(\"2026-09-01\"), end: parseISO(\"2026-09-03\") }, { step: 2 });");
        StringAssert.Contains(script, "return max(dates);");
        AssertImport(script, "date-fns", "isBefore", "isWithinInterval", "eachDayOfInterval", "parseISO", "max");
    }

    [TestMethod]
    public async Task DateFns_UnknownDomainProbe_EmitsHostValuePassThrough()
    {
        // isDate is the single unknown-domain parameter; it must lower as an opaque host value,
        // not require a CLR type projection. Follows the Global.TypeOf host convention.
        // isDate 是唯一的 unknown 值域参数，应按不透明宿主值直传，沿用 Global.TypeOf 约定。
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.DateFns;

            public static class TestClass
            {
                public static bool Probe(object? value)
                {
                    return IsDate(value) && IsValid(ToDate(ParseJSON("2026-09-17T00:00:00.000Z")));
                }
            }
            """);

        StringAssert.Contains(script, "return isDate(value) && isValid(toDate(parseJSON(\"2026-09-17T00:00:00.000Z\")));");
        AssertImport(script, "date-fns", "isDate", "isValid", "toDate", "parseJSON");
    }

    [TestMethod]
    public async Task DateFns_ModuleDefaults_AndDurationFormatting_EmitCalls()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.DateFns;
            using static ECMAScript.DateFnsLocale;

            public static class TestClass
            {
                public static string Build(Date start, Date end)
                {
                    SetDefaultOptions(new DateFnsDefaultOptions { Locale = EnGB, WeekStartsOn = DateFnsWeekStartsOn.Sunday });
                    var duration = IntervalToDuration(new DateFnsInterval { Start = start, End = end });
                    return FormatDuration(duration, new DateFnsFormatDurationOptions { Zero = true });
                }
            }
            """);

        StringAssert.Contains(script, "setDefaultOptions({ locale: enGB, weekStartsOn: 0 });");
        StringAssert.Contains(script, "let duration = intervalToDuration({ start: start, end: end });");
        StringAssert.Contains(script, "return formatDuration(duration, { zero: true });");
        AssertImport(script, "date-fns", "setDefaultOptions", "intervalToDuration", "formatDuration");
        AssertImport(script, "date-fns/locale", "enGB");
    }

    private static async Task<string> ConvertAsync(string code)
    {
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DateFns).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        Assert.IsNotNull(module);
        return module.ToKnRECMAScript();
    }

    private static void AssertImport(string script, string specifier, params string[] names)
    {
        // The compiler consolidates named imports of one module into a single alphabetically
        // ordered statement; assert that exact stable shape.
        // 编译器把同一模块的命名导入合并为单条按字母排序的语句；按该稳定形状断言。
        var ordered = names.Order(StringComparer.Ordinal).ToArray();
        StringAssert.Contains(
            script,
            $"import {{ {string.Join(", ", ordered)} }} from \"{specifier}\";",
            $"Missing consolidated import of {string.Join(", ", ordered)} from {specifier}.");
    }

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(
        string code,
        string className,
        params MetadataReference[] additionalReferences)
    {
        var compilation = CSharpCompilation.Create(
            "ECMAScript.DateFns.Test.Assembly",
            [CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview))],
            BuildCompilationReferences(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsFalse(diagnostics.Length > 0, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(node => node.Identifier.Text == className);

            if (classDeclaration is null)
                continue;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            Assert.IsNotNull(classSymbol);
            return (classSymbol, semanticModel);
        }

        throw new InvalidOperationException($"Cannot locate class '{className}'.");
    }

    private static MetadataReference[] BuildCompilationReferences(IEnumerable<MetadataReference>? additionalReferences = null)
    {
        var references = CurrentRuntimeReferences().ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(Number).Assembly.Location));
        if (additionalReferences is not null)
            references.AddRange(additionalReferences);

        return references.ToArray();
    }

    private static IEnumerable<MetadataReference> CurrentRuntimeReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
            return Net110.References.All.Cast<MetadataReference>();

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static path => MetadataReference.CreateFromFile(path));
    }
}
