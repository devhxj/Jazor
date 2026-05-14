using System.Reflection;
using System.Runtime.CompilerServices;
using ECMAScript;
using ECMAScript.ElementPlus;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class ElementPlusAuthoringSurfaceTests
{
    private static readonly Type AdditionalAttributesType = typeof(IReadOnlyDictionary<string, object?>);
    private static readonly string BannedChoiceName = "E" + "ither";
    private static readonly string BannedInterfaceChoiceName = "I" + BannedChoiceName;

    [TestMethod]
    public void ElementPlus_AuthoringComponents_MatchOfficialComponentExports()
    {
        var components = GetElementPlusAuthoringComponents();

        CollectionAssert.AreEquivalent(
            ElementPlusTestMetadata.OfficialComponentExportNames,
            components.Select(static type => type.Name).ToArray());

        CollectionAssert.AreEquivalent(
            ElementPlusTestMetadata.OfficialComponentExportNames,
            ElementPlusTestMetadata.RuntimeComponentExportNames);
    }

    [TestMethod]
    public void ElementPlus_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink()
    {
        foreach (var component in GetElementPlusAuthoringComponents())
        {
            var additionalAttributes = component.GetProperty(
                nameof(ElementPlusComponentBase.AdditionalAttributes),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            Assert.IsNotNull(additionalAttributes, component.FullName);
            Assert.AreEqual(AdditionalAttributesType, additionalAttributes!.PropertyType, component.FullName);

            var parameter = additionalAttributes.GetCustomAttribute<ParameterAttribute>(inherit: true);
            Assert.IsNotNull(parameter, component.FullName);
            Assert.IsTrue(parameter!.CaptureUnmatchedValues, component.FullName);

            foreach (var property in component.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (property.Name == additionalAttributes.Name && property.PropertyType == additionalAttributes.PropertyType)
                    continue;

                AssertNoWeakObjectContract(property.PropertyType, $"{component.FullName}.{property.Name}");
                AssertNoRuntimeDispatchContract(property, $"{component.FullName}.{property.Name}");
            }
        }
    }

    [TestMethod]
    public void ElementPlus_AuthoringComponents_ExposeExplicitCssPropsAsCssClassAndCssStyle()
    {
        foreach (var component in GetElementPlusAuthoringComponents())
        {
            var parameterNames = component
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(static property => property.GetCustomAttribute<ParameterAttribute>(inherit: true) is not null)
                .Select(static property => property.Name)
                .ToArray();

            CollectionAssert.DoesNotContain(parameterNames, "Class", component.FullName);
            CollectionAssert.DoesNotContain(parameterNames, "Style", component.FullName);

            AssertCssParameterMapping(
                component,
                nameof(ElementPlusComponentBase.CssClass),
                "class",
                typeof(VueClassValue?));
            AssertCssParameterMapping(
                component,
                nameof(ElementPlusComponentBase.CssStyle),
                "style",
                typeof(VueStyleValue?));
        }
    }

    [TestMethod]
    public void ElementPlus_PublicContracts_DoNotUseBannedChoiceWrappersOrRuntimeDispatch()
    {
        var publicTypes = typeof(ElementPlus).Assembly
            .GetExportedTypes()
            .Where(static type => type.Namespace == "ECMAScript.ElementPlus")
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        foreach (var type in publicTypes)
        {
            AssertNoBannedChoiceContract(type, type.FullName ?? type.Name);
            AssertNoRuntimeDispatchContract(type, type.FullName ?? type.Name);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                AssertNoBannedChoiceContract(property.PropertyType, $"{type.FullName}.{property.Name}");
                AssertNoRuntimeDispatchContract(property, $"{type.FullName}.{property.Name}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (method.IsSpecialName)
                    continue;

                AssertNoBannedChoiceContract(method.ReturnType, $"{type.FullName}.{method.Name} return");
                AssertNoRuntimeDispatchContract(method.ReturnParameter, $"{type.FullName}.{method.Name} return");

                foreach (var parameter in method.GetParameters())
                {
                    AssertNoBannedChoiceContract(parameter.ParameterType, $"{type.FullName}.{method.Name} parameter {parameter.Name}");
                    AssertNoRuntimeDispatchContract(parameter, $"{type.FullName}.{method.Name} parameter {parameter.Name}");
                }
            }
        }
    }

    [TestMethod]
    public void ElementPlus_AuthoringComponents_UseExpectedPackageEntrypointsAndPluginContracts()
    {
        foreach (var component in GetElementPlusAuthoringComponents())
        {
            var libraryComponent = component.GetCustomAttribute<VueLibraryComponentAttribute>(inherit: false);
            Assert.IsNotNull(libraryComponent, component.FullName);
            Assert.AreEqual("element-plus", libraryComponent!.ImportSpecifier, component.FullName);

            var styleAttribute = component.GetCustomAttribute<VueLibraryStyleAttribute>(inherit: false);
            Assert.IsNotNull(styleAttribute, component.FullName);
            Assert.AreEqual("element-plus/dist/index.css", styleAttribute!.StyleSpecifier, component.FullName);

            var pluginRequirement = component.GetCustomAttribute<VueLibraryPluginRequirementAttribute>(inherit: false);
            Assert.IsNotNull(pluginRequirement, component.FullName);
            Assert.AreEqual("element-plus", pluginRequirement!.RequirementId, component.FullName);
        }
    }

    [TestMethod]
    public void ElementPlus_DirectiveExports_MatchOfficialDirectiveSymbols()
    {
        var exportNames = typeof(ElementPlusDirectives)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(GetDirectiveExportName)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEquivalent(
            ElementPlusTestMetadata.OfficialDirectiveExportNames,
            exportNames);
    }

    private static Type[] GetElementPlusAuthoringComponents()
        => typeof(ElementPlusComponents).Assembly
            .GetExportedTypes()
            .Where(static type => !type.IsAbstract && typeof(IVueLibraryComponent).IsAssignableFrom(type))
            .OrderBy(static type => type.Name, StringComparer.Ordinal)
            .ToArray();

    private static string GetDirectiveExportName(PropertyInfo property)
    {
        foreach (var attribute in property.CustomAttributes)
        {
            if (attribute.AttributeType.FullName != "ECMAScript.ECMAScriptNameAttribute")
                continue;

            if (attribute.ConstructorArguments.Count == 1 &&
                attribute.ConstructorArguments[0].ArgumentType == typeof(string) &&
                attribute.ConstructorArguments[0].Value is string explicitName &&
                !string.IsNullOrWhiteSpace(explicitName))
            {
                return explicitName;
            }
        }

        return property.Name;
    }

    private static void AssertCssParameterMapping(
        Type component,
        string propertyName,
        string runtimeName,
        Type expectedPropertyType)
    {
        var property = component.GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        Assert.IsNotNull(property, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(expectedPropertyType, property!.PropertyType, $"{component.FullName}.{propertyName}");
        Assert.IsNotNull(property.GetCustomAttribute<ParameterAttribute>(inherit: true), $"{component.FullName}.{propertyName}");

        var mapping = component
            .GetCustomAttributes<VuePropAttribute>(inherit: false)
            .SingleOrDefault(attribute => attribute.PublicName == propertyName);
        Assert.IsNotNull(mapping, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(runtimeName, mapping!.Name, $"{component.FullName}.{propertyName}");
    }

    private static void AssertNoWeakObjectContract(Type type, string memberName)
    {
        if (UsesPlainObject(type))
            Assert.Fail($"{memberName} exposes '{type}' outside AdditionalAttributes.");
    }

    private static void AssertNoBannedChoiceContract(Type type, string memberName)
    {
        if (UsesBannedChoiceWrapper(type))
            Assert.Fail($"{memberName} exposes '{type}' with banned choice-wrapper naming.");
    }

    private static void AssertNoRuntimeDispatchContract(ICustomAttributeProvider provider, string memberName)
    {
        if (provider.IsDefined(typeof(DynamicAttribute), inherit: false))
            Assert.Fail($"{memberName} exposes a runtime-dispatch marker.");
    }

    private static bool UsesPlainObject(Type type)
    {
        if (type == typeof(object))
            return true;

        if (type.IsArray)
            return UsesPlainObject(type.GetElementType()!);

        if (type.IsGenericType)
            return type.GetGenericArguments().Any(UsesPlainObject);

        return type.HasElementType && UsesPlainObject(type.GetElementType()!);
    }

    private static bool UsesBannedChoiceWrapper(Type type)
    {
        if (type.Name.Contains(BannedChoiceName, StringComparison.Ordinal) ||
            type.Name.Contains(BannedInterfaceChoiceName, StringComparison.Ordinal) ||
            (type.FullName?.Contains(BannedChoiceName, StringComparison.Ordinal) ?? false) ||
            (type.FullName?.Contains(BannedInterfaceChoiceName, StringComparison.Ordinal) ?? false))
        {
            return true;
        }

        if (type.IsArray)
            return UsesBannedChoiceWrapper(type.GetElementType()!);

        if (type.IsGenericType)
            return type.GetGenericArguments().Any(UsesBannedChoiceWrapper);

        return type.HasElementType && UsesBannedChoiceWrapper(type.GetElementType()!);
    }
}
