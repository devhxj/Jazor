using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class WebRenderTreeBuilderExtensionsModuleWhitelistTests
{
    [TestMethod]
    public void WebRenderTreeBuilderExtensionsPublicSurface_MatchesModuleSurface()
    {
        var actual = typeof(Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(static method => method.Name + "(" + string.Join(", ", method.GetParameters().Select(static parameter => GetTypeSurfaceName(parameter.ParameterType))) + ")")
            .Order(StringComparer.Ordinal)
            .ToArray();

        var expected = new[]
            {
                "AddEventPreventDefaultAttribute(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder, int, string, bool)",
                "AddEventStopPropagationAttribute(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder, int, string, bool)"
            }
            .Order(StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(expected, actual, string.Join(Environment.NewLine, actual));
    }

    [TestMethod]
    public void WebRenderTreeBuilderExtensionsType_IsAllowed()
    {
        var attribute = typeof(Jazor.CLR.WebRenderTreeBuilderExtensionsModule).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Allowed, attribute.Op);
        Assert.AreEqual("Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions", attribute.Member);
    }

    [TestMethod]
    public void EventModifierMethods_AreAllowed()
    {
        var mappings = typeof(Jazor.CLR.WebRenderTreeBuilderExtensionsModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(static attribute => attribute.Member, StringComparer.Ordinal);
        var actual = mappings.Keys
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expected = typeof(Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(static method => "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions." + method.Name + "(" + string.Join(", ", method.GetParameters().Select(static parameter => GetTypeSurfaceName(parameter.ParameterType))) + ")")
            .Order(StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(expected, actual, string.Join(Environment.NewLine, actual));
        foreach (var mapping in mappings.Values)
            Assert.AreEqual(Op.Allowed, mapping.Op, mapping.Member);
    }

    private static string GetTypeSurfaceName(Type type)
        => type == typeof(int)
            ? "int"
            : type == typeof(string)
                ? "string"
                : type == typeof(bool)
                    ? "bool"
                    : (type.FullName ?? type.Name).Replace('+', '.');
}
