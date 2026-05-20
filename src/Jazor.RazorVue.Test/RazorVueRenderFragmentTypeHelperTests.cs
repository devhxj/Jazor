using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueRenderFragmentTypeHelperTests
{
    [TestMethod]
    public void RenderFragmentTypeHelper_UsesExactNameFormatDelegateSignatures()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RenderFragmentTypeHelper.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(
                """
                namespace Demo.Components
                {
                    public class Host : ComponentBase, IVueComponent
                    {
                        [Parameter]
                        public RenderFragment? Header { get; set; }

                        [Parameter]
                        public RenderFragment<int>? ItemTemplate { get; set; }
                    }
                }
                """),
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var host = compilation.GetTypeByMetadataName("Demo.Components.Host");
        Assert.IsNotNull(host);

        var header = (IPropertySymbol)host.GetMembers("Header").Single();
        var itemTemplate = (IPropertySymbol)host.GetMembers("ItemTemplate").Single();

        var headerDisplayName = ((INamedTypeSymbol)header.Type).OriginalDefinition.ToDisplayString(Format.NameFormat);
        var itemTemplateDisplayName = ((INamedTypeSymbol)itemTemplate.Type).OriginalDefinition.ToDisplayString(Format.NameFormat);

        Assert.AreEqual(RazorVueRenderFragmentTypeHelper.RenderFragmentMetadataName, headerDisplayName);
        Assert.AreEqual(RazorVueRenderFragmentTypeHelper.ParameterizedRenderFragmentMetadataName, itemTemplateDisplayName);
        Assert.AreNotEqual("Microsoft.AspNetCore.Components.RenderFragment", headerDisplayName);
        Assert.AreNotEqual("Microsoft.AspNetCore.Components.RenderFragment<T>", itemTemplateDisplayName);

        Assert.IsTrue(RazorVueRenderFragmentTypeHelper.IsUntypedRenderFragmentType(header.Type));
        Assert.IsTrue(RazorVueRenderFragmentTypeHelper.IsParameterizedRenderFragmentType(itemTemplate.Type));
        Assert.IsTrue(RazorVueRenderFragmentTypeHelper.IsRenderFragmentType(header.Type));
        Assert.IsTrue(RazorVueRenderFragmentTypeHelper.IsRenderFragmentType(itemTemplate.Type));
    }
}
