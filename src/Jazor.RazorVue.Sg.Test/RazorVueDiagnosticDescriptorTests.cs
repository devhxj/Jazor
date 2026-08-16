using System.Collections.Immutable;
using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueDiagnosticDescriptorTests
{
    [TestMethod]
    public void DescriptorContract_MapsEveryCategoryToStableIdAndHelpLink()
    {
        var expected = new Dictionary<RazorVueDiagnosticCategory, (string Id, string Anchor)>
        {
            [RazorVueDiagnosticCategory.Internal] = ("JAZORVGA020", "#final-compilation"),
            [RazorVueDiagnosticCategory.DirectRender] = ("JAZORVGA021", "#direct-render"),
            [RazorVueDiagnosticCategory.CompilerBridge] = ("JAZORVGA022", "#compiler-boundary"),
            [RazorVueDiagnosticCategory.ComponentBinding] = ("JAZORVGA023", "#component-binding"),
            [RazorVueDiagnosticCategory.MemberClosure] = ("JAZORVGA024", "#member-closure"),
            [RazorVueDiagnosticCategory.VueInject] = ("JAZORVGA025", "#vue-inject"),
            [RazorVueDiagnosticCategory.VueModule] = ("JAZORVGA026", "#vue-module")
        };

        foreach (var (category, contract) in expected)
        {
            var info = RazorVueDiagnosticFactory.Create(category, "stable-detail");
            var descriptor = Diagnostics.GetDescriptor(category);
            var diagnostic = Diagnostics.Create(info);

            Assert.AreEqual(contract.Id, descriptor.Id, category.ToString());
            Assert.AreEqual(contract.Id, diagnostic.Id, category.ToString());
            Assert.AreEqual(DiagnosticSeverity.Error, descriptor.DefaultSeverity, category.ToString());
            Assert.IsTrue(
                descriptor.HelpLinkUri.EndsWith(contract.Anchor, StringComparison.Ordinal),
                descriptor.HelpLinkUri);
            StringAssert.Contains(diagnostic.GetMessage(), "stable-detail", StringComparison.Ordinal);
        }
    }

    [TestMethod]
    public void DiagnosticFactory_UsesGeneratedSourceKindOnlyForUnmappedGeneratedTrees()
    {
        var tree = CSharpSyntaxTree.ParseText(
            "class GeneratedComponent { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/GeneratedComponent.razor.g.cs");
        var location = tree.GetRoot().GetLocation();

        var info = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.ComponentBinding,
            "generated-detail",
            location);

        Assert.AreEqual(RazorVueDiagnosticSourceKind.GeneratedCSharp, info.SourceKind);
        Assert.AreEqual(location.GetLineSpan().Path, info.PrimaryLocation.GetLineSpan().Path);
        Assert.AreNotEqual(Location.None, info.PrimaryLocation);
    }

    [TestMethod]
    public void DiagnosticInfo_WithComponent_ReclassifiesResolvedComponentLocation()
    {
        var tree = CSharpSyntaxTree.ParseText(
            "namespace Demo; public sealed class GeneratedComponent { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/GeneratedComponent.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVueDiagnosticComponentLocation_" + Guid.NewGuid().ToString("N"),
            [tree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.GeneratedComponent");
        Assert.IsNotNull(component);

        var unresolved = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.ComponentBinding,
            "component-detail");
        var resolved = unresolved.WithComponent(component);

        Assert.AreNotEqual(Location.None, resolved.PrimaryLocation);
        Assert.AreEqual(
            RazorVueDiagnosticSourceKind.GeneratedCSharp,
            resolved.SourceKind);
        Assert.AreEqual(
            "Pages/GeneratedComponent.razor.g.cs",
            resolved.PrimaryLocation.GetLineSpan().Path);
    }

    [TestMethod]
    public void DiagnosticInfo_WithComponent_PreservesExistingMappedRazorLocation()
    {
        var tree = CSharpSyntaxTree.ParseText(
            "namespace Demo; public sealed class GeneratedComponent { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/GeneratedComponent.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVueDiagnosticMappedLocation_" + Guid.NewGuid().ToString("N"),
            [tree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.GeneratedComponent");
        Assert.IsNotNull(component);

        var mappedLocation = Location.Create(
            "Pages/GeneratedComponent.razor",
            new TextSpan(0, 1),
            new LinePositionSpan(new LinePosition(2, 3), new LinePosition(2, 4)));
        var diagnostic = new RazorVueDiagnosticInfo(
            RazorVueDiagnosticCategory.ComponentBinding,
            "RazorVue.ComponentBinding",
            ImmutableArray.Create("component-detail"),
            DiagnosticSeverity.Error,
            mappedLocation,
            ImmutableArray<Location>.Empty,
            RazorVueDiagnosticSourceKind.MappedRazor,
            ComponentId: null,
            Subject: null,
            HelpLinkKey: "component-binding",
            IsAuthorReachable: true);

        var resolved = diagnostic.WithComponent(component);

        Assert.AreEqual("Pages/GeneratedComponent.razor", resolved.PrimaryLocation.GetLineSpan().Path);
        Assert.AreEqual(RazorVueDiagnosticSourceKind.MappedRazor, resolved.SourceKind);
    }

    [TestMethod]
    public void DiagnosticFactory_ClassifiesAllTransformationExceptionsAsCompilerBridge()
    {
        var tree = CSharpSyntaxTree.ParseText(
            "class GeneratedComponent { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/GeneratedComponent.razor.g.cs");
        var location = tree.GetRoot().GetLocation();
        var exceptions = new Exception[]
        {
            new OperationTransformationException(default, "operation", location),
            new SyntaxNodeTransformationException(default, "syntax", location),
            new SymbolTransformationException(default, "symbol", location)
        };

        foreach (var exception in exceptions)
        {
            var info = RazorVueDiagnosticFactory.FromException(
                exception,
                RazorVueDiagnosticCategory.DirectRender);

            Assert.AreEqual(RazorVueDiagnosticCategory.CompilerBridge, info.Category);
            Assert.AreEqual(RazorVueDiagnosticSourceKind.GeneratedCSharp, info.SourceKind);
            Assert.AreEqual(location.GetLineSpan().Path, info.PrimaryLocation.GetLineSpan().Path);
        }
    }
}
