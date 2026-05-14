using System.Reflection;
using ECMAScript;
using ECMAScript.Vben;
using Jazor.RazorVue.Descriptor;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenAuthoringSurfaceTests
{
    private static readonly Type AdditionalAttributesType = typeof(IReadOnlyDictionary<string, object?>);
    private static readonly string[] ExpectedComponentNames =
    [
        nameof(VbenAdminLayout),
        nameof(VbenHeaderBar),
        nameof(VbenPageContainer),
        nameof(VbenSidebarMenu)
    ];
    private static readonly string[] DisallowedUiNamespaces =
    [
        "ECMAScript.TDesign",
        "ECMAScript.ElementPlus",
        "ECMAScript.Vuetify"
    ];

    [TestMethod]
    public void Vben_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink()
    {
        var components = GetComponents();

        CollectionAssert.AreEquivalent(
            ExpectedComponentNames,
            components.Select(static type => type.Name).ToArray());

        foreach (var component in components)
        {
            var additionalAttributes = component.GetProperty(
                nameof(VbenComponentBase.AdditionalAttributes),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            Assert.IsNotNull(additionalAttributes, component.FullName);
            Assert.AreEqual(AdditionalAttributesType, additionalAttributes!.PropertyType, component.FullName);

            var parameter = additionalAttributes.GetCustomAttribute<ParameterAttribute>(inherit: true);
            Assert.IsNotNull(parameter, component.FullName);
            Assert.IsTrue(parameter!.CaptureUnmatchedValues, component.FullName);

            foreach (var property in component.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (property.Name == additionalAttributes.Name)
                    continue;

                AssertNoWeakObjectContract(property.PropertyType, $"{component.FullName}.{property.Name}");
            }
        }
    }

    [TestMethod]
    public void Vben_AuthoringComponents_UseVueStyleAndVueClassContracts()
    {
        AssertCssSurface(typeof(VbenAdminLayout));
        AssertCssSurface(typeof(VbenSidebarMenu));
        AssertCssSurface(typeof(VbenHeaderBar));
        AssertCssSurface(typeof(VbenPageContainer));
    }

    [TestMethod]
    public void Vben_ComponentRegistry_SeesNativeShellComponentsAsUserComponents()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/host-card")]
                public class HostCard : ComponentBase, IVueComponent
                {
                }
            }
            """);

        var registry = context.CreateComponentRegistry();
        foreach (var componentName in GetComponents().Select(static type => type.Name))
        {
            var result = registry.Resolve(
                componentName,
                VueComponentResolutionContext.Create("Demo.Pages", "ECMAScript.Vben"));

            Assert.AreEqual(VueComponentResolutionStatus.Resolved, result.Status, componentName);
            Assert.IsNotNull(result.Descriptor, componentName);
            Assert.AreEqual(VueComponentSourceKind.UserComponent, result.Descriptor.SourceKind, componentName);
            StringAssert.StartsWith(result.Descriptor.ImportSpecifier, "./components/", componentName);
        }
    }

    [TestMethod]
    public void Vben_PublicContracts_DoNotLeakThirdPartyUiLibraryTypes()
    {
        var publicTypes = typeof(VbenAdminLayout).Assembly
            .GetExportedTypes()
            .Where(static type => type.Namespace == "ECMAScript.Vben")
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        foreach (var type in publicTypes)
        {
            AssertNoThirdPartyUiType(type, type.FullName ?? type.Name);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                AssertNoThirdPartyUiType(property.PropertyType, $"{type.FullName}.{property.Name}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (method.IsSpecialName)
                    continue;

                AssertNoThirdPartyUiType(method.ReturnType, $"{type.FullName}.{method.Name} return");

                foreach (var parameter in method.GetParameters())
                {
                    AssertNoThirdPartyUiType(parameter.ParameterType, $"{type.FullName}.{method.Name} parameter {parameter.Name}");
                }
            }
        }
    }

    [TestMethod]
    public void Vue3_StyleContract_UsesDedicatedVueStyleValue()
    {
        Assert.AreEqual(typeof(VueStyleValue?), typeof(VueObject).GetProperty(nameof(VueObject.Style))?.PropertyType);
        Assert.AreEqual(typeof(VueStyleValue?), typeof(VueAttributeBag).GetProperty(nameof(VueAttributeBag.Style))?.PropertyType);
    }

    private static Type[] GetComponents()
        => typeof(VbenAdminLayout).Assembly
            .GetExportedTypes()
            .Where(static type => !type.IsAbstract && typeof(ECMAScript.Vue3.IVueComponent).IsAssignableFrom(type))
            .OrderBy(static type => type.Name, StringComparer.Ordinal)
            .ToArray();

    private static void AssertCssSurface(Type component)
    {
        Assert.AreEqual(typeof(VueClassValue?), component.GetProperty(nameof(VbenComponentBase.CssClass))?.PropertyType, component.FullName);
        Assert.AreEqual(typeof(VueStyleValue?), component.GetProperty(nameof(VbenComponentBase.CssStyle))?.PropertyType, component.FullName);
        Assert.IsNotNull(component.GetProperty(nameof(VbenComponentBase.AdditionalAttributes)), component.FullName);
    }

    private static void AssertNoWeakObjectContract(Type type, string memberName)
    {
        if (type == typeof(object))
            Assert.Fail($"{memberName} exposes '{type}' outside AdditionalAttributes.");

        if (type.IsArray && type.GetElementType() is { } elementType)
        {
            AssertNoWeakObjectContract(elementType, memberName);
            return;
        }

        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
                AssertNoWeakObjectContract(argument, memberName);
        }
    }

    private static void AssertNoThirdPartyUiType(Type type, string memberName)
    {
        if (type.IsArray && type.GetElementType() is { } elementType)
        {
            AssertNoThirdPartyUiType(elementType, memberName);
            return;
        }

        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
                AssertNoThirdPartyUiType(argument, memberName);
        }

        if (type.HasElementType && type.GetElementType() is { } element)
        {
            AssertNoThirdPartyUiType(element, memberName);
        }

        var typeNamespace = type.Namespace;
        if (typeNamespace is null)
            return;

        foreach (var disallowedNamespace in DisallowedUiNamespaces)
        {
            if (typeNamespace == disallowedNamespace
                || typeNamespace.StartsWith(disallowedNamespace + ".", StringComparison.Ordinal))
            {
                Assert.Fail($"{memberName} leaks third-party UI type '{type.FullName}'.");
            }
        }
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            assemblyName: "RazorVue.Vben.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
            references: RazorVueMetadataReferences.Create(),
            options: new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context!;
    }
}
