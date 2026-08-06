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
}
