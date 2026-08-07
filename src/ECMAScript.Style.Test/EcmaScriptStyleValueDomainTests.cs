using System.Reflection;
using System.Runtime.CompilerServices;

namespace ECMAScript.Style.Tests;

[TestClass]
public sealed class EcmaScriptStyleValueDomainTests
{
    [TestMethod]
    public void GeneratedProperties_UseGrammarSpecificNativeUnions()
    {
        AssertPropertyType(nameof(CssDeclarations.Width), typeof(CssLengthPercentageValue));
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

        Assert.AreNotEqual(typeof(string), typeof(CssDeclarations).GetProperty(nameof(CssDeclarations.Width))!.PropertyType);
    }

    [TestMethod]
    public void ValueDomains_AreNativeUnionsAndTokensCannotBeConstructedPublicly()
    {
        foreach (var type in new[]
        {
            typeof(CssValue),
            typeof(CssLengthPercentageValue),
            typeof(CssColorValue),
            typeof(CssTimeValue),
            typeof(CssBoxShadowValue)
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
}
