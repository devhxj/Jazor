using System.Reflection;
using ECMAScript;
using ECMAScript.Vue;
using ECMAScript.Vue.Vuetify;

namespace Jazor.ComplierTest;

#pragma warning disable CA1416

[TestClass]
public sealed class EcmaScriptVueProxyTests
{
    [TestMethod]
    public void Vue_CoreProxyMethods_DoNotExposeObject()
    {
        var proxyTypes = new[] { typeof(Vue), typeof(VueApp) };

        foreach (var method in proxyTypes.SelectMany(static type =>
            type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(static method => !method.IsSpecialName)))
        {
            AssertNotObject(method.ReturnType, $"{method.DeclaringType?.Name}.{method.Name} return");
            foreach (var parameter in method.GetParameters())
                AssertNotObject(parameter.ParameterType, $"{method.DeclaringType?.Name}.{method.Name}({parameter.Name})");
        }
    }

    [TestMethod]
    public void Vuetify_CreateVuetify_UsesStronglyTypedOptionsAndPlugin()
    {
        var overloads = typeof(Vuetify)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => method.Name == nameof(Vuetify.CreateVuetify))
            .OrderBy(static method => method.GetParameters().Length)
            .ToArray();

        Assert.AreEqual(2, overloads.Length);
        Assert.IsTrue(overloads.All(static method => method.ReturnType == typeof(VuetifyPlugin)));

        CollectionAssert.AreEqual(
            Array.Empty<Type>(),
            overloads[0].GetParameters().Select(static parameter => parameter.ParameterType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(VuetifyOptions) },
            overloads[1].GetParameters().Select(static parameter => parameter.ParameterType).ToArray());

        Assert.IsTrue(typeof(VuePlugin).IsAssignableFrom(typeof(VuetifyPlugin)));
        Assert.IsTrue(typeof(VuePluginOptions).IsAssignableFrom(typeof(VuetifyOptions)));
    }

    [TestMethod]
    public void Vuetify_ComponentExports_AreConcreteComponentTypes()
    {
        var exportedComponents = typeof(VuetifyComponents)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(static property => property.Name, StringComparer.Ordinal)
            .ToArray();
        var registryProperties = typeof(VuetifyComponentRegistry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(static property => property.Name, StringComparer.Ordinal);

        Assert.IsTrue(exportedComponents.Length > 0);
        foreach (var property in exportedComponents)
        {
            Assert.AreEqual(property.Name, property.PropertyType.Name, property.Name);
            Assert.IsTrue(typeof(VuetifyComponent).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.IsTrue(typeof(VueComponent).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.AreEqual(property.PropertyType, registryProperties[property.Name].PropertyType.UnwrapNullable(), property.Name);
            AssertModule(property.PropertyType, "vuetify/components", property.Name);
        }
    }

    [TestMethod]
    public void Vuetify_DirectiveExports_AreConcreteDirectiveTypes()
    {
        var exportedDirectives = typeof(VuetifyDirectives)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(static property => property.Name, StringComparer.Ordinal)
            .ToArray();
        var registryProperties = typeof(VuetifyDirectiveRegistry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(static property => property.Name, StringComparer.Ordinal);

        Assert.IsTrue(exportedDirectives.Length > 0);
        foreach (var property in exportedDirectives)
        {
            Assert.AreEqual($"{property.Name}Directive", property.PropertyType.Name, property.Name);
            Assert.IsTrue(typeof(VuetifyDirective).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.IsTrue(typeof(VueDirective).IsAssignableFrom(property.PropertyType), property.Name);
            Assert.AreEqual(property.PropertyType, registryProperties[property.Name].PropertyType.UnwrapNullable(), property.Name);
            AssertModule(property.PropertyType, "vuetify/directives", property.Name);
        }
    }

    [TestMethod]
    public void Vuetify_OptionsAndRegistries_DoNotExposeObjectProperties()
    {
        var optionTypes = typeof(VuetifyOptions).Assembly
            .GetTypes()
            .Where(static type =>
                type.Namespace == "ECMAScript.Vue.Vuetify" &&
                (type.Name.EndsWith("Options", StringComparison.Ordinal) ||
                 type.Name.EndsWith("Registry", StringComparison.Ordinal)))
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.IsTrue(optionTypes.Length > 0);
        foreach (var property in optionTypes.SelectMany(static type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)))
        {
            Assert.AreNotEqual(typeof(object), property.PropertyType.UnwrapNullable(), $"{property.DeclaringType?.Name}.{property.Name}");
        }
    }

    private static void AssertNotObject(Type type, string message)
    {
        Assert.AreNotEqual(typeof(object), type.UnwrapNullable(), message);

        if (!type.IsGenericType)
            return;

        foreach (var argument in type.GetGenericArguments())
            AssertNotObject(argument, message);
    }

    private static void AssertModule(Type type, string expectedModule, string expectedName)
    {
        var module = type.GetCustomAttribute<ECMAScriptModuleAttribute>();
        var name = type.GetCustomAttribute<ECMAScriptNameAttribute>();

        Assert.IsNotNull(module, type.FullName);
        Assert.IsNotNull(name, type.FullName);
        Assert.AreEqual(expectedModule, module.Import, type.FullName);
        Assert.AreEqual(expectedName, name.Name, type.FullName);
    }
}

internal static class TypeTestExtensions
{
    public static Type UnwrapNullable(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;
}

#pragma warning restore CA1416
