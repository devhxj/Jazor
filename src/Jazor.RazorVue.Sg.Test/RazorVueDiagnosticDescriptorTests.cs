using System.Collections.Immutable;
using System.Reflection;
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
    public void DiagnosticFactory_PreservesUnmappedGeneratedTreeLocation()
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

        Assert.AreEqual(location.GetLineSpan().Path, info.PrimaryLocation.GetLineSpan().Path);
        Assert.AreNotEqual(Location.None, info.PrimaryLocation);
    }

    [TestMethod]
    public void DiagnosticInfo_WithComponent_UsesComponentFallbackLocation()
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
            "component-detail",
            mappedLocation,
            ImmutableArray<Location>.Empty,
            ComponentId: null);

        var resolved = diagnostic.WithComponent(component);

        Assert.AreEqual("Pages/GeneratedComponent.razor", resolved.PrimaryLocation.GetLineSpan().Path);
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
            Assert.AreEqual(location.GetLineSpan().Path, info.PrimaryLocation.GetLineSpan().Path);
        }
    }

    [TestMethod]
    public void DiagnosticFactory_NormalizesMappedAndDuplicateAdditionalLocations()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var primaryTree = CSharpSyntaxTree.ParseText(
            "class First { }",
            parseOptions,
            path: "Pages/First.cs");
        var duplicateTree = CSharpSyntaxTree.ParseText(
            "class First { }",
            parseOptions,
            path: "pages/first.cs");
        var mappedTree = CSharpSyntaxTree.ParseText(
            "#line 7 \"Pages/Mapped.razor\"\nclass Mapped { }",
            parseOptions,
            path: "Pages/Mapped.razor.g.cs");
        var primary = primaryTree.GetRoot().GetLocation();
        var duplicate = duplicateTree.GetRoot().GetLocation();
        var mapped = mappedTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .Single()
            .Identifier
            .GetLocation();

        var info = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.DirectRender,
            "normalized locations",
            primaryLocation: mapped,
            additionalLocations: ImmutableArray.Create(primary, duplicate, mapped, Location.None));

        Assert.AreEqual("Pages/Mapped.razor", info.PrimaryLocation.GetLineSpan().Path);
        Assert.HasCount(2, info.AdditionalLocations);
        Assert.AreEqual("Pages/First.cs", info.AdditionalLocations[0].GetLineSpan().Path);
        Assert.AreEqual("Pages/Mapped.razor", info.AdditionalLocations[1].GetLineSpan().Path);
    }

    [TestMethod]
    public void DiagnosticFactory_HandlesEmptyFallbackAndTypedDiagnosticCarrier()
    {
        var fallback = RazorVueDiagnosticFactory.FromException(
            new InvalidOperationException(string.Empty),
            RazorVueDiagnosticCategory.Internal);

        Assert.AreEqual(RazorVueDiagnosticCategory.Internal, fallback.Category);
        Assert.AreEqual("No diagnostic detail was provided.", fallback.Message);
        Assert.AreEqual(Location.None, fallback.PrimaryLocation);

        var typed = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.VueModule,
            "typed diagnostic");
        var preserved = RazorVueDiagnosticFactory.FromException(
            new RazorVueDiagnosticException(typed),
            RazorVueDiagnosticCategory.Internal);

        Assert.AreEqual(typed, preserved);
    }

    [TestMethod]
    public void DiagnosticFactory_UsesDeterministicSymbolFallbackLocation()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var first = CSharpSyntaxTree.ParseText(
            "namespace Demo; public partial class Component { }",
            parseOptions,
            path: "Z/Component.cs");
        var second = CSharpSyntaxTree.ParseText(
            "namespace Demo; public partial class Component { }",
            parseOptions,
            path: "A/Component.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVueDiagnosticLocation_" + Guid.NewGuid().ToString("N"),
            [first, second],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.Component");
        Assert.IsNotNull(component);

        var info = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.ComponentBinding,
            "component fallback",
            component: component);

        Assert.AreEqual("A/Component.cs", info.PrimaryLocation.GetLineSpan().Path);
        Assert.AreEqual(Location.None, RazorVueDiagnosticFactory.GetSymbolLocation(null));
    }

    [TestMethod]
    public void DiagnosticFactory_SymbolFallbackUsesOrdinalTieBreakerForCaseInsensitivePaths()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var lower = CSharpSyntaxTree.ParseText(
            "namespace Demo; public partial class Component { }",
            parseOptions,
            path: "pages/component.cs");
        var upper = CSharpSyntaxTree.ParseText(
            "namespace Demo; public partial class Component { }",
            parseOptions,
            path: "Pages/Component.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVueDiagnosticCaseTie_" + Guid.NewGuid().ToString("N"),
            [lower, upper],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.Component");
        Assert.IsNotNull(component);

        var location = RazorVueDiagnosticFactory.GetSymbolLocation(component);

        Assert.AreEqual("Pages/Component.cs", location.GetLineSpan().Path);
    }

    [TestMethod]
    public void DiagnosticFactory_PreservesAuthorLocationPriorityAcrossFallbacks()
    {
        var tree = CSharpSyntaxTree.ParseText(
            "namespace Demo; public sealed class Component { public void Render() { } }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Component.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVueDiagnosticPriority_" + Guid.NewGuid().ToString("N"),
            [tree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.Component");
        Assert.IsNotNull(component);
        var authored = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .Single()
            .Identifier
            .GetLocation();
        var external = Location.Create(
            "external-location",
            new TextSpan(0, 1),
            new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 1)));

        Assert.AreEqual(Location.None, RazorVueDiagnosticFactory.ToAuthorLocation(null));
        Assert.AreEqual(Location.None, RazorVueDiagnosticFactory.ToAuthorLocation(Location.None));
        Assert.AreEqual(Location.None, RazorVueDiagnosticFactory.ToAuthorLocation(external));
        Assert.AreEqual(authored, RazorVueDiagnosticFactory.PreferLocation(authored, external));
        Assert.AreEqual(authored, RazorVueDiagnosticFactory.PreferLocation(external, authored));

        var subjectFallback = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.DirectRender,
            "subject fallback",
            primaryLocation: external,
            subject: component);
        Assert.AreEqual("Pages/Component.cs", subjectFallback.PrimaryLocation.GetLineSpan().Path);

        var primaryWins = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.DirectRender,
            "primary wins",
            primaryLocation: authored,
            component: component);
        Assert.AreEqual(authored.SourceSpan, primaryWins.PrimaryLocation.SourceSpan);

        var unresolved = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.DirectRender,
            "unresolved");
        Assert.AreSame(unresolved, unresolved.WithComponent(null));
        var resolvedTypedException = RazorVueDiagnosticFactory.FromException(
            new RazorVueDiagnosticException(unresolved),
            RazorVueDiagnosticCategory.Internal,
            component);
        Assert.AreEqual("Pages/Component.cs", resolvedTypedException.PrimaryLocation.GetLineSpan().Path);

        var whitespaceMessage = RazorVueDiagnosticFactory.FromException(
            new InvalidOperationException("   "),
            RazorVueDiagnosticCategory.DirectRender);
        Assert.AreEqual("No diagnostic detail was provided.", whitespaceMessage.Message);
    }

    [TestMethod]
    public void DiagnosticLocationComparer_UsesSourceSpanPathAndMappedLineSpan()
    {
        var comparerType = typeof(RazorVueDiagnosticFactory).GetNestedType(
            "LocationComparer",
            BindingFlags.NonPublic);
        Assert.IsNotNull(comparerType);
        var comparer = comparerType.GetProperty(
                "Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(null);
        Assert.IsNotNull(comparer);
        var equals = comparerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.DeclaringType == comparerType &&
                              method.Name == "Equals" &&
                              method.GetParameters().Length == 2);
        var getHashCode = comparerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == "GetHashCode" && method.GetParameters().Length == 1);

        var same = Location.Create(
            "Pages/Counter.razor",
            new TextSpan(4, 2),
            new LinePositionSpan(new LinePosition(2, 3), new LinePosition(2, 5)));
        var sameDifferentCase = Location.Create(
            "pages/counter.razor",
            new TextSpan(4, 2),
            new LinePositionSpan(new LinePosition(2, 3), new LinePosition(2, 5)));
        var differentPath = Location.Create(
            "Pages/Other.razor",
            new TextSpan(4, 2),
            new LinePositionSpan(new LinePosition(2, 3), new LinePosition(2, 5)));
        var differentSourceSpan = Location.Create(
            "Pages/Counter.razor",
            new TextSpan(5, 2),
            new LinePositionSpan(new LinePosition(2, 3), new LinePosition(2, 5)));
        var differentMappedSpan = Location.Create(
            "Pages/Counter.razor",
            new TextSpan(4, 2),
            new LinePositionSpan(new LinePosition(3, 0), new LinePosition(3, 2)));

        Assert.IsTrue((bool)equals.Invoke(comparer, [null, null])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [null, same])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [same, null])!);
        Assert.IsTrue((bool)equals.Invoke(comparer, [same, same])!);
        Assert.IsTrue((bool)equals.Invoke(comparer, [same, sameDifferentCase])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [same, differentPath])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [same, differentSourceSpan])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [same, differentMappedSpan])!);

        // Locations without source trees must not compare equal merely because their spans are
        // both default. This protects de-duplication when Roslyn reports Location.None.
        var emptyPath = Location.Create(
            string.Empty,
            new TextSpan(0, 0),
            new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0)));
        Assert.IsFalse((bool)equals.Invoke(comparer, [Location.None, emptyPath])!);
        Assert.IsFalse((bool)equals.Invoke(comparer, [emptyPath, Location.None])!);
        Assert.AreEqual(
            (int)getHashCode.Invoke(comparer, [same])!,
            (int)getHashCode.Invoke(comparer, [sameDifferentCase])!);
        Assert.IsNotNull(getHashCode.Invoke(comparer, [Location.None]));
    }

    [TestMethod]
    public void DiagnosticFactory_IgnoresMetadataOnlySymbolsForLocationFallback()
    {
        var compilation = CSharpCompilation.Create(
            "RazorVueDiagnosticMetadata_" + Guid.NewGuid().ToString("N"),
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var metadataString = compilation.GetSpecialType(SpecialType.System_String);

        Assert.AreEqual(Location.None, RazorVueDiagnosticFactory.GetSymbolLocation(metadataString));

        var diagnostic = RazorVueDiagnosticFactory.Create(
            RazorVueDiagnosticCategory.VueModule,
            "metadata-only symbol",
            subject: metadataString);

        Assert.AreEqual(Location.None, diagnostic.PrimaryLocation);
        Assert.AreEqual(RazorVueDiagnosticCategory.VueModule, diagnostic.Category);
        Assert.AreEqual("metadata-only symbol", diagnostic.Message);
    }

}
