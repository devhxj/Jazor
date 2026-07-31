using System.Reflection;
using ECMAScript.Contract;
using Jazor.RazorVue.RazorSdk.Catalog;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RenderTreeBuilderCatalogWhitelistTests
{
    [TestMethod]
    public void RenderTreeBuilderType_IsAllowed()
    {
        var attribute = typeof(RenderTreeBuilderCatalog).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Allowed, attribute.Op);
        Assert.AreEqual("Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder", attribute.Member);
    }

    [TestMethod]
    public void PublicRenderTreeBuilderMethods_AreAllowed()
    {
        var mappings = typeof(RenderTreeBuilderCatalog)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(static attribute => attribute.Member, StringComparer.Ordinal);
        var actual = mappings.Keys
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expected = typeof(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(GetRenderTreeBuilderMemberKey)
            .Concat(
                typeof(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(static _ => "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.RenderTreeBuilder()"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(expected, actual, string.Join(Environment.NewLine, actual));
        foreach (var mapping in mappings.Values)
            Assert.AreEqual(Op.Allowed, mapping.Op, mapping.Member);
    }

    private static string GetRenderTreeBuilderMemberKey(MethodInfo method)
        => "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder." + GetMethodSurfaceSignature(method);

    private static string GetMethodSurfaceSignature(MethodInfo method)
    {
        var genericArguments = method.IsGenericMethodDefinition
            ? "<" + string.Join(", ", method.GetGenericArguments().Select(static argument => argument.Name)) + ">"
            : string.Empty;
        var parameters = string.Join(
            ", ",
            method.GetParameters().Select(static parameter => GetTypeSurfaceName(parameter.ParameterType)));
        return method.Name + genericArguments + "(" + parameters + ")";
    }

    private static string GetTypeSurfaceName(Type type)
    {
        if (type.IsGenericParameter)
            return type.Name;

        if (!type.IsGenericType)
            return GetNonGenericTypeSurfaceName(type);

        var definition = type.GetGenericTypeDefinition();
        var definitionName = definition.FullName ?? definition.Name;
        var tickIndex = definitionName.IndexOf('`');
        if (tickIndex >= 0)
            definitionName = definitionName.Substring(0, tickIndex);

        var arguments = string.Join(
            ", ",
            type.GetGenericArguments().Select(static argument => GetTypeSurfaceName(argument)));
        return definitionName.Replace('+', '.') + "<" + arguments + ">";
    }

    private static string GetNonGenericTypeSurfaceName(Type type)
        => type == typeof(int)
            ? "int"
            : type == typeof(string)
                ? "string"
                : type == typeof(bool)
                    ? "bool"
                    : type == typeof(object)
                        ? "object"
                        : (type.FullName ?? type.Name).Replace('+', '.');
}
