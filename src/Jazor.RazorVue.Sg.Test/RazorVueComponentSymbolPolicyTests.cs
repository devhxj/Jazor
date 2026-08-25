using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class ComponentSymbolPolicyTests
{
    [TestMethod]
    public void IsDeclaredOnComponentHierarchy_DistinguishesComponentBaseChainFromExternalSymbols()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            namespace Demo;

            public abstract class ReleaseTemplateBase
            {
            }

            public sealed class ReleasePage : ReleaseTemplateBase
            {
            }

            public sealed class ExternalWidget
            {
            }
            """);
        var compilation = CSharpCompilation.Create(
            "ComponentSymbolPolicyTests",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.ReleasePage");
        var baseType = compilation.GetTypeByMetadataName("Demo.ReleaseTemplateBase");
        var externalType = compilation.GetTypeByMetadataName("Demo.ExternalWidget");

        Assert.IsNotNull(component);
        Assert.IsNotNull(baseType);
        Assert.IsNotNull(externalType);
        Assert.IsTrue(ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(component, component));
        Assert.IsTrue(ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(component, baseType));
        Assert.IsFalse(ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(component, externalType));
        Assert.IsFalse(ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(component, null));
    }

    [TestMethod]
    public void IsRazorVueComponent_RequiresComponentBaseAndSupportsDerivedVueMarkers()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue;

            namespace Demo;

            public interface IDerivedVueComponent : IVueComponent
            {
            }

            public abstract class ComponentBaseAlias : ComponentBase
            {
            }

            public sealed class IndirectComponent : ComponentBaseAlias, IDerivedVueComponent
            {
            }

            public sealed class DirectComponent : ComponentBase, IDerivedVueComponent
            {
            }

            public sealed class MissingComponentBase : IDerivedVueComponent
            {
            }

            public sealed class MissingVueMarker : ComponentBase
            {
            }
            """);
        var compilation = CSharpCompilation.Create(
            "ComponentSymbolPolicyContractTests",
            [syntaxTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentBase = compilation.GetTypeByMetadataName(ComponentSymbolPolicy.ComponentBaseMetadataName);
        var vueComponentMarker = compilation.GetTypeByMetadataName(ComponentSymbolPolicy.VueComponentMarkerMetadataName);
        Assert.IsNotNull(componentBase);
        Assert.IsNotNull(vueComponentMarker);

        Assert.IsTrue(ComponentSymbolPolicy.IsRazorVueComponent(
            compilation.GetTypeByMetadataName("Demo.IndirectComponent")!,
            componentBase,
            vueComponentMarker));
        Assert.IsTrue(ComponentSymbolPolicy.IsRazorVueComponent(
            compilation.GetTypeByMetadataName("Demo.DirectComponent")!,
            componentBase,
            vueComponentMarker));
        Assert.IsFalse(ComponentSymbolPolicy.IsRazorVueComponent(
            compilation.GetTypeByMetadataName("Demo.MissingComponentBase")!,
            componentBase,
            vueComponentMarker));
        Assert.IsFalse(ComponentSymbolPolicy.IsRazorVueComponent(
            compilation.GetTypeByMetadataName("Demo.MissingVueMarker")!,
            componentBase,
            vueComponentMarker));
    }
}
