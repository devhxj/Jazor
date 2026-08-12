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
            .Where(static property => property.Name != "additional" && property.GetIndexParameters().Length == 0)
            .Select(static property => property.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description)
            .ToArray();

        Assert.HasCount(817, cssNames);
        Assert.AreEqual(1, cssNames.Count(static name => name == "@#background-color"));
        Assert.AreEqual(1, cssNames.Count(static name => name == "@#float"));
        Assert.AreEqual(1, cssNames.Count(static name => name == "@#-webkit-text-fill-color"));
        Assert.DoesNotContain("@#css-text", cssNames);
        Assert.AreEqual(cssNames.Length, cssNames.Distinct(StringComparer.Ordinal).Count());
        foreach (var property in properties.Where(static property => property.Name != "additional" && property.GetIndexParameters().Length == 0))
        {
            Assert.IsTrue(IsLowerSnakeCase(property.Name), property.Name);
        }

        var additional = typeof(CssDeclarations).GetProperty(nameof(CssDeclarations.additional));
        Assert.IsNotNull(additional);
        Assert.AreEqual("@#$additional", additional.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description);
        Assert.HasCount(1, typeof(CssDeclarations).GetDefaultMembers().OfType<PropertyInfo>());

        Assert.IsNotNull(typeof(CssDeclarations).GetProperty(nameof(CssDeclarations.grid_template_columns)));
        Assert.IsNotNull(typeof(CssDeclarations).GetProperty(nameof(CssDeclarations.webkit_box_shadow)));
        Assert.IsNotNull(typeof(CssDeclarations).GetProperty(nameof(CssDeclarations.float_)));
        Assert.IsNull(typeof(CssDeclarations).GetProperty("GridTemplateColumns"));
        Assert.IsNull(typeof(CssDeclarations).GetProperty("WebkitBoxShadow"));
        Assert.IsNull(typeof(CssDeclarations).GetProperty("Float"));
    }

    [TestMethod]
    public void RuleStructuralMembers_UseDslSnakeCaseWhileAtRuleModelRemainsPascalCase()
    {
        Assert.IsNotNull(typeof(CssDeclarations).GetProperty(nameof(CssDeclarations.additional)));
        Assert.IsNull(typeof(CssDeclarations).GetProperty("Additional"));
        Assert.IsNotNull(typeof(CssRule).GetProperty(nameof(CssRule.children)));
        Assert.IsNull(typeof(CssRule).GetProperty("Children"));

        // CssAtRule is a CLR record model rather than a CSS declaration DSL block.
        Assert.IsNotNull(typeof(CssAtRule).GetProperty(nameof(CssAtRule.Children)));
    }

    private static bool IsLowerSnakeCase(string value)
    {
        if (value.Length == 0 || !char.IsLower(value[0]))
            return false;

        for (var index = 1; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsLower(character) || char.IsDigit(character))
                continue;

            if (character == '_' && index == value.Length - 1 ||
                character == '_' && index + 1 < value.Length && char.IsLower(value[index + 1]))
            {
                continue;
            }

            return false;
        }

        return true;
    }
}
