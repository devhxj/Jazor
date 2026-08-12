using System.Reflection;
using System.Runtime.CompilerServices;

namespace ECMAScript.Style.Tests;

[TestClass]
public sealed class EcmaScriptStyleValueDomainTests
{
    [TestMethod]
    public void GeneratedProperties_UseGrammarSpecificNativeUnions()
    {
        AssertPropertyType(nameof(CssDeclarations.width), typeof(CssSizingValue));
        AssertPropertyType(nameof(CssDeclarations.height), typeof(CssSizingValue));
        AssertPropertyType(nameof(CssDeclarations.flex_basis), typeof(CssFlexBasisValue));
        AssertPropertyType(nameof(CssDeclarations.column_width), typeof(CssColumnWidthValue));
        AssertPropertyType(nameof(CssDeclarations.top), typeof(CssAnchorPositionValue));
        AssertPropertyType(nameof(CssDeclarations.inset), typeof(CssInsetValue));
        AssertPropertyType(nameof(CssDeclarations.margin_top), typeof(CssAnchorMarginValue));
        AssertPropertyType(nameof(CssDeclarations.anchor_name), typeof(CssAnchorNameValue));
        AssertPropertyType(nameof(CssDeclarations.anchor_scope), typeof(CssAnchorScopeValue));
        AssertPropertyType(nameof(CssDeclarations.position_anchor), typeof(CssPositionAnchorValue));
        AssertPropertyType(nameof(CssDeclarations.color), typeof(CssColorValue));
        AssertPropertyType(nameof(CssDeclarations.transition_duration), typeof(CssTimeValue));
        AssertPropertyType(nameof(CssDeclarations.opacity), typeof(CssNumberPercentageValue));
        AssertPropertyType(nameof(CssDeclarations.display), typeof(CssDisplayValue));
        AssertPropertyType(nameof(CssDeclarations.grid_template_columns), typeof(CssTrackValue));
        AssertPropertyType(nameof(CssDeclarations.box_shadow), typeof(CssBoxShadowValue));
        AssertPropertyType(nameof(CssDeclarations.webkit_box_shadow), typeof(CssBoxShadowValue));
        AssertPropertyType(nameof(CssDeclarations.border), typeof(CssBorderValue));
        AssertPropertyType(nameof(CssDeclarations.backdrop_filter), typeof(CssFilterValue));
        AssertPropertyType(nameof(CssDeclarations.align_items), typeof(CssAlignmentValue));
        AssertPropertyType(nameof(CssDeclarations.padding), typeof(CssPaddingValue));
        AssertPropertyType(nameof(CssDeclarations.margin), typeof(CssMarginValue));
        AssertPropertyType(nameof(CssDeclarations.gap), typeof(CssGapValue));
        AssertPropertyType(nameof(CssDeclarations.border_radius), typeof(CssRadiusValue));
        AssertPropertyType(nameof(CssDeclarations.flex), typeof(CssFlexValue));
        AssertPropertyType(nameof(CssDeclarations.text_align), typeof(CssTextAlignValue));

        Assert.AreNotEqual(typeof(string), typeof(CssDeclarations).GetProperty(nameof(CssDeclarations.width))!.PropertyType);
    }

    [TestMethod]
    public void ValueDomains_AreNativeUnionsAndTokensCannotBeConstructedPublicly()
    {
        foreach (var type in new[]
        {
            typeof(CssValue),
            typeof(CssLengthPercentageValue),
            typeof(CssSizingValue),
            typeof(CssFlexBasisValue),
            typeof(CssColumnWidthValue),
            typeof(CssAnchorPositionValue),
            typeof(CssAnchorMarginValue),
            typeof(CssInsetValue),
            typeof(CssAnchorNameValue),
            typeof(CssAnchorScopeValue),
            typeof(CssPositionAnchorValue),
            typeof(CssColorValue),
            typeof(CssTimeValue),
            typeof(CssBoxShadowValue),
            typeof(CssPaddingValue),
            typeof(CssMarginValue),
            typeof(CssGapValue),
            typeof(CssRadiusValue),
            typeof(CssFlexValue)
        })
        {
            Assert.IsNotNull(type.GetCustomAttribute<UnionAttribute>(), type.FullName);
            Assert.IsTrue(typeof(IUnion).IsAssignableFrom(type), type.FullName);
        }

        foreach (var type in new[]
        {
            typeof(CssRaw),
            typeof(CssLength),
            typeof(CssLengthPercentage),
            typeof(CssFitContent),
            typeof(CssAnchorName),
            typeof(CssAnchor),
            typeof(CssAnchorSize),
            typeof(CssCalcSize),
            typeof(CssInset),
            typeof(CssColor),
            typeof(CssTime),
            typeof(CssBorderWidth),
            typeof(CssBorderStyle)
        })
            Assert.IsEmpty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance), type.FullName);
    }

    [TestMethod]
    public void CompositeDomains_ExposeTypedCompositionOperators()
    {
        Assert.IsTrue(typeof(ICssBorderPart).IsAssignableFrom(typeof(CssLength)));
        Assert.IsTrue(typeof(ICssBorderPart).IsAssignableFrom(typeof(CssBorderStyle)));
        Assert.IsTrue(typeof(ICssBorderPart).IsAssignableFrom(typeof(CssColor)));

        var pipe = typeof(CssLength).GetMethod(
            "op_BitwiseOr",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(CssLength), typeof(ICssBorderPart)]);
        Assert.IsNotNull(pipe);
        Assert.AreEqual(typeof(CssBorder), pipe.ReturnType);
        Assert.AreEqual(
            "@#important",
            typeof(CssDeclarationPriority)
                .GetField(nameof(CssDeclarationPriority.Important))!
                .GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()!
                .Description);
    }

    [TestMethod]
    public void CompositeDomains_KeepShorthandsOutOfSingleValueProperties()
    {
        var validSource = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            public static class ValidStyles
            {
                public static readonly CssRule Rule = new()
                {
                    padding = important(px(8) | px(12)),
                    margin = important(margin(px(0), auto)),
                    gap = gap(px(8), px(12)),
                    border_radius = radius(px(4), px(4), px(0), px(0)),
                    flex = flex_box(0, 0, px(32)),
                    grid_column = grid_line(1, -1)
                };
            }
            """;
        var validErrors = Compile(validSource).GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(validErrors, string.Join(Environment.NewLine, validErrors.Select(static error => error.ToString())));

        var paddingPipe = typeof(CssLength).GetMethod(
            "op_BitwiseOr",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(CssLength), typeof(CssLength)]);
        Assert.IsNotNull(paddingPipe);
        Assert.AreEqual(typeof(CssPadding), paddingPipe.ReturnType);

        var invalidSource = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            public static class InvalidStyles
            {
                public static readonly CssRule Rule = new() { width = padding(px(8), px(12)) };
            }
            """;
        var invalidErrors = Compile(invalidSource).GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(1, invalidErrors, string.Join(Environment.NewLine, invalidErrors.Select(static error => error.ToString())));
        StringAssert.Contains(invalidErrors[0].GetMessage(), nameof(CssPadding));
    }

    [TestMethod]
    public void AnchorAndSizingDomains_ExpressModernGrammarWithoutGenericFallbacks()
    {
        var validSource = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            public static class ValidAnchorStyles
            {
                private static readonly CssAnchorName Card = anchor_name("--card");

                public static readonly CssRule Rule = new()
                {
                    anchor_name = anchor_names(Card, anchor_name("--trigger")),
                    anchor_scope = anchor_scope_all,
                    position_anchor = Card,
                    width = calc_size(min_content, size + px(2)),
                    height = anchor_size(Card, anchor_inline, px(20)),
                    flex_basis = flex_content,
                    column_width = fit_content(percent(50)),
                    top = anchor(Card, anchor_bottom, px(8)),
                    inset = inset_sides(anchor(anchor_top), anchor_size(Card, anchor_block)),
                    margin_top = anchor_size(Card, anchor_inline),
                    margin = margin(anchor_size(Card, anchor_inline), auto)
                };
            }
            """;
        var validErrors = Compile(validSource).GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(validErrors, string.Join(Environment.NewLine, validErrors.Select(static error => error.ToString())));

        var invalidSource = """
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            public static class InvalidAnchorStyles
            {
                public static readonly CssRule Rule = new()
                {
                    width = anchor(anchor_top),
                    top = calc_size(min_content, size + px(2)),
                    margin_top = margin(px(8), px(12)),
                    anchor_name = anchor_size(anchor_inline),
                    column_width = percent(100) - rem(2)
                };
            }
            """;
        var invalidErrors = Compile(invalidSource).GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToArray();

        Assert.HasCount(5, invalidErrors, string.Join(Environment.NewLine, invalidErrors.Select(static error => error.ToString())));
        StringAssert.Contains(invalidErrors[0].GetMessage(), nameof(CssAnchor));
        StringAssert.Contains(invalidErrors[1].GetMessage(), nameof(CssCalcSize));
        StringAssert.Contains(invalidErrors[2].GetMessage(), nameof(CssMargin));
        StringAssert.Contains(invalidErrors[3].GetMessage(), nameof(CssAnchorSize));
        StringAssert.Contains(invalidErrors[4].GetMessage(), nameof(CssLengthPercentage));
    }

    [TestMethod]
    public void CssFacade_PublicSurfaceUsesLowerSnakeCase()
    {
        var members = typeof(css).GetMembers(BindingFlags.Public | BindingFlags.Static)
            .Where(static member => member.DeclaringType == typeof(css) && member.Name is not "Equals" and not "ReferenceEquals")
            .ToArray();
        Assert.IsNotEmpty(members);
        foreach (var member in members)
        {
            Assert.IsTrue(char.IsLower(member.Name[0]), member.Name);
            Assert.IsFalse(member.Name.Any(char.IsUpper), member.Name);
        }
    }

    [TestMethod]
    public void CssFacade_SnakeCaseMembersKeepExplicitJavaScriptAbiNames()
    {
        var snakeCaseMembers = typeof(css).GetMembers(BindingFlags.Public | BindingFlags.Static)
            .Where(static member => member.DeclaringType == typeof(css) && member.Name.Contains('_'))
            .ToArray();

        Assert.IsNotEmpty(snakeCaseMembers);
        foreach (var member in snakeCaseMembers)
        {
            var javaScriptName = member.GetCustomAttribute<global::ECMAScript.ECMAScriptNameAttribute>()?.Name;
            Assert.IsFalse(string.IsNullOrWhiteSpace(javaScriptName), member.Name);
            Assert.AreNotEqual(member.Name, javaScriptName, member.Name);
            Assert.IsFalse(javaScriptName!.Contains('_'), javaScriptName);
        }
    }

    private static void AssertPropertyType(string propertyName, Type expected)
    {
        var property = typeof(CssDeclarations).GetProperty(propertyName);
        Assert.IsNotNull(property);
        Assert.AreEqual(expected, Nullable.GetUnderlyingType(property.PropertyType), propertyName);
    }

    private static Microsoft.CodeAnalysis.CSharp.CSharpCompilation Compile(string source)
    {
        var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
            source,
            Microsoft.CodeAnalysis.CSharp.CSharpParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview));
        return Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            "EcmaScriptStyleCompositeDomainTests",
            [syntaxTree],
            Basic.Reference.Assemblies.Net110.References.All.Concat(
            [
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(css).Assembly.Location)
            ]),
            new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
    }
}
