using System.Reflection;
using System.Runtime.CompilerServices;

namespace ECMAScript.Style.Tests;

[TestClass]
public sealed class EcmaScriptStyleValueDomainTests
{
    [TestMethod]
    public void GeneratedProperties_UseGrammarSpecificNativeUnions()
    {
        AssertPropertyType(nameof(CssDeclarations.Width), typeof(CssSizingValue));
        AssertPropertyType(nameof(CssDeclarations.Height), typeof(CssSizingValue));
        AssertPropertyType(nameof(CssDeclarations.FlexBasis), typeof(CssFlexBasisValue));
        AssertPropertyType(nameof(CssDeclarations.ColumnWidth), typeof(CssColumnWidthValue));
        AssertPropertyType(nameof(CssDeclarations.Top), typeof(CssAnchorPositionValue));
        AssertPropertyType(nameof(CssDeclarations.Inset), typeof(CssInsetValue));
        AssertPropertyType(nameof(CssDeclarations.MarginTop), typeof(CssAnchorMarginValue));
        AssertPropertyType(nameof(CssDeclarations.AnchorName), typeof(CssAnchorNameValue));
        AssertPropertyType(nameof(CssDeclarations.AnchorScope), typeof(CssAnchorScopeValue));
        AssertPropertyType(nameof(CssDeclarations.PositionAnchor), typeof(CssPositionAnchorValue));
        AssertPropertyType(nameof(CssDeclarations.Color), typeof(CssColorValue));
        AssertPropertyType(nameof(CssDeclarations.TransitionDuration), typeof(CssTimeValue));
        AssertPropertyType(nameof(CssDeclarations.Opacity), typeof(CssNumberPercentageValue));
        AssertPropertyType(nameof(CssDeclarations.Display), typeof(CssDisplayValue));
        AssertPropertyType(nameof(CssDeclarations.GridTemplateColumns), typeof(CssTrackValue));
        AssertPropertyType(nameof(CssDeclarations.BoxShadow), typeof(CssBoxShadowValue));
        AssertPropertyType(nameof(CssDeclarations.WebkitBoxShadow), typeof(CssBoxShadowValue));
        AssertPropertyType(nameof(CssDeclarations.Border), typeof(CssBorderValue));
        AssertPropertyType(nameof(CssDeclarations.BackdropFilter), typeof(CssFilterValue));
        AssertPropertyType(nameof(CssDeclarations.AlignItems), typeof(CssAlignmentValue));
        AssertPropertyType(nameof(CssDeclarations.Padding), typeof(CssPaddingValue));
        AssertPropertyType(nameof(CssDeclarations.Margin), typeof(CssMarginValue));
        AssertPropertyType(nameof(CssDeclarations.Gap), typeof(CssGapValue));
        AssertPropertyType(nameof(CssDeclarations.BorderRadius), typeof(CssRadiusValue));
        AssertPropertyType(nameof(CssDeclarations.Flex), typeof(CssFlexValue));
        AssertPropertyType(nameof(CssDeclarations.TextAlign), typeof(CssTextAlignValue));

        Assert.AreNotEqual(typeof(string), typeof(CssDeclarations).GetProperty(nameof(CssDeclarations.Width))!.PropertyType);
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
                    Padding = padding(px(8), px(12)),
                    Margin = important(margin(px(0), auto)),
                    Gap = gap(px(8), px(12)),
                    BorderRadius = radius(px(4), px(4), px(0), px(0)),
                    Flex = flexBox(0, 0, px(32)),
                    GridColumn = gridLine(1, -1)
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

            public static class InvalidStyles
            {
                public static readonly CssRule Rule = new() { Width = padding(px(8), px(12)) };
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
                private static readonly CssAnchorName Card = anchorName("--card");

                public static readonly CssRule Rule = new()
                {
                    AnchorName = anchorNames(Card, anchorName("--trigger")),
                    AnchorScope = anchorScopeAll,
                    PositionAnchor = Card,
                    Width = calcSize(minContent, size + px(2)),
                    Height = anchorSize(Card, anchorInline, px(20)),
                    FlexBasis = flexContent,
                    ColumnWidth = fitContent(percent(50)),
                    Top = anchor(Card, anchorBottom, px(8)),
                    Inset = insetSides(anchor(anchorTop), anchorSize(Card, anchorBlock)),
                    MarginTop = anchorSize(Card, anchorInline),
                    Margin = margin(anchorSize(Card, anchorInline), auto)
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
                    Width = anchor(anchorTop),
                    Top = calcSize(minContent, size + px(2)),
                    MarginTop = margin(px(8), px(12)),
                    AnchorName = anchorSize(anchorInline),
                    ColumnWidth = percent(100) - rem(2)
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
    public void CssFacade_PublicSurfaceUsesLowerCamelCase()
    {
        var members = typeof(css).GetMembers(BindingFlags.Public | BindingFlags.Static)
            .Where(static member => member.DeclaringType == typeof(css) && member.Name is not "Equals" and not "ReferenceEquals")
            .ToArray();
        Assert.IsNotEmpty(members);
        foreach (var member in members)
            Assert.IsTrue(char.IsLower(member.Name[0]), member.Name);
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
