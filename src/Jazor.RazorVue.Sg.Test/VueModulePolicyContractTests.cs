using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueModulePolicyContractTests
{
    [TestMethod]
    public void ComponentHierarchyProjection_UsesBaseFirstEnumerationAndSingleDerivedExportOwner()
    {
        var compilation = CreateCompilation();
        var baseType = GetNamedType(compilation, "ModulePolicyContracts.BaseComponent");
        var componentType = GetNamedType(compilation, "ModulePolicyContracts.DerivedComponent");
        var foreignType = GetNamedType(compilation, "ModulePolicyContracts.ForeignComponent");
        var privateShadowType = GetNamedType(compilation, "ModulePolicyContracts.PrivateShadowComponent");
        var interfaceType = GetNamedType(compilation, "ModulePolicyContracts.IStandaloneContract");
        var baseShared = GetMethod(baseType, "Shared");
        var baseInheritedOnly = GetMethod(baseType, "InheritedOnly");
        var derivedShared = GetMethod(componentType, "Shared");
        var foreignShared = GetMethod(foreignType, "Shared");
        var privateShadow = GetMethod(privateShadowType, "InheritedOnly");
        var basePropertyGetter = GetProperty(baseType, "SharedProperty").GetMethod;
        var derivedPropertyGetter = GetProperty(componentType, "SharedProperty").GetMethod;
        Assert.IsNotNull(basePropertyGetter);
        Assert.IsNotNull(derivedPropertyGetter);

        CollectionAssert.AreEqual(
            new[] { "BaseComponent", "DerivedComponent" },
            VueModulePolicy.Instance
                .EnumerateModuleTypes(componentType)
                .Select(static type => type.Name)
                .ToArray());
        Assert.IsTrue(VueModulePolicy.Instance.ShouldFlattenNestedRuntimeClass(
            componentType,
            baseType,
            foreignType));

        Assert.IsTrue(VueModulePolicy.Instance.ShouldExportModuleMember(componentType, compilation.Assembly));
        Assert.IsTrue(VueModulePolicy.Instance.ShouldExportModuleMember(componentType, derivedShared));
        Assert.IsTrue(VueModulePolicy.Instance.ShouldExportModuleMember(componentType, foreignShared));
        Assert.IsTrue(VueModulePolicy.Instance.ShouldExportModuleMember(componentType, baseInheritedOnly));
        Assert.IsFalse(VueModulePolicy.Instance.ShouldExportModuleMember(componentType, baseShared));
        Assert.IsFalse(VueModulePolicy.Instance.ShouldExportModuleMember(componentType, basePropertyGetter!));
        Assert.IsTrue(VueModulePolicy.Instance.ShouldExportModuleMember(privateShadowType, baseInheritedOnly));

        Assert.IsNull(VueModulePolicy.Instance.GetPreferredModuleDeclaredName(derivedShared));
        Assert.IsNull(VueModulePolicy.Instance.GetPreferredModuleDeclaredName(GetProperty(componentType, "SharedProperty").SetMethod!));
        Assert.AreEqual(
            "SharedProperty",
            VueModulePolicy.Instance.GetPreferredModuleDeclaredName(derivedPropertyGetter!));
        Assert.IsEmpty(VueModulePolicy.Instance.EnumerateModuleTypes(compilation.GetSpecialType(SpecialType.System_Object)));
        CollectionAssert.AreEqual(
            new[] { "IStandaloneContract" },
            VueModulePolicy.Instance
                .EnumerateModuleTypes(interfaceType)
                .Select(static type => type.Name)
                .ToArray());
        Assert.IsTrue(VueModulePolicy.Instance.IsAdditionalTopLevelAccessibilityAllowed(Accessibility.Internal));
        Assert.IsFalse(VueModulePolicy.Instance.IsAdditionalTopLevelAccessibilityAllowed(Accessibility.Public));
    }

    private static CSharpCompilation CreateCompilation()
    {
        var source = CSharpSyntaxTree.ParseText(
            """
            namespace ModulePolicyContracts;

            public interface IStandaloneContract
            {
            }

            public class BaseComponent
            {
                public void Shared() { }
                public void InheritedOnly() { }
                public int SharedProperty { get; } = 1;
            }

            public sealed class DerivedComponent : BaseComponent
            {
                public new void Shared() { }
                public new int SharedProperty { get; } = 2;
            }

            public sealed class ForeignComponent
            {
                public void Shared() { }
            }

            public sealed class PrivateShadowComponent : BaseComponent
            {
                private new void InheritedOnly() { }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "VueModulePolicyContracts.cs");
        var compilation = CSharpCompilation.Create(
            "Jazor.RazorVue.VueModulePolicy.Contracts",
            [source],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return compilation;
    }

    private static INamedTypeSymbol GetNamedType(Compilation compilation, string metadataName)
    {
        var type = compilation.GetTypeByMetadataName(metadataName);
        Assert.IsNotNull(type, metadataName);
        return type!;
    }

    private static IMethodSymbol GetMethod(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IMethodSymbol>().Single();

    private static IPropertySymbol GetProperty(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IPropertySymbol>().Single();
}
