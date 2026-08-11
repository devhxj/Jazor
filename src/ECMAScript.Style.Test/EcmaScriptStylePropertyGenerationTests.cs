using System.Reflection;

namespace ECMAScript.Style.Tests;

[TestClass]
public sealed class EcmaScriptStylePropertyGenerationTests
{
    [TestMethod]
    public void GeneratedProperties_ExposeStableCssNamesWithoutRuntimeInventory()
    {
        var properties = typeof(CssDeclarations)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        var cssNames = properties
            .Where(static property => property.Name != "Additional" && property.GetIndexParameters().Length == 0)
            .Select(static property => property.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description)
            .ToArray();

        Assert.HasCount(817, cssNames);
        Assert.AreEqual(1, cssNames.Count(static name => name == "@#background-color"));
        Assert.AreEqual(1, cssNames.Count(static name => name == "@#float"));
        Assert.AreEqual(1, cssNames.Count(static name => name == "@#-webkit-text-fill-color"));
        Assert.DoesNotContain("@#css-text", cssNames);
        Assert.AreEqual(cssNames.Length, cssNames.Distinct(StringComparer.Ordinal).Count());

        var additional = typeof(CssDeclarations).GetProperty(nameof(CssDeclarations.Additional));
        Assert.IsNotNull(additional);
        Assert.AreEqual("@#$additional", additional.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description);
        Assert.HasCount(1, typeof(CssDeclarations).GetDefaultMembers().OfType<PropertyInfo>());
    }
}
