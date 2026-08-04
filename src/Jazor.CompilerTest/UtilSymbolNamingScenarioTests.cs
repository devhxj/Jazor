using ECMAScript;
using Jazor.Common;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class UtilSymbolNamingScenarioTests
{
    [TestMethod]
    public void SymbolNaming_UsesExplicitDescriptionBoundaryAndOverloadContracts()
    {
        var fixture = Compile(
            """
            using System.ComponentModel;
            using ECMAScript;

            [ECMAScript]
            [Description("@#")]
            public sealed class BoundaryHost
            {
            }

            [Description("@#description-name")]
            public sealed class DescriptionNamedHost
            {
            }

            [ECMAScriptName("explicit-name")]
            public sealed class ExplicitlyNamedHost
            {
            }

            [ECMAScriptName("   ")]
            public sealed class BlankNamedHost
            {
            }

            [Description("@#description-name")]
            [ECMAScriptName(null)]
            public sealed class NullExplicitNameHost
            {
            }

            [Description(null)]
            public sealed class NullDescriptionHost
            {
            }

            [ECMAScript]
            public record Release([property: ECMAScriptName("release-id")] string Id);

            public sealed class NamingSample
            {
                public int this[int index]
                {
                    get => index;
                    set { }
                }

                public void HTTPServer() { }

                public void Build() { }

                public void Build(int value) { }
            }
            """);
        var boundaryHost = fixture.GetType("BoundaryHost");
        var descriptionNamedHost = fixture.GetType("DescriptionNamedHost");
        var explicitlyNamedHost = fixture.GetType("ExplicitlyNamedHost");
        var blankNamedHost = fixture.GetType("BlankNamedHost");
        var nullExplicitNameHost = fixture.GetType("NullExplicitNameHost");
        var nullDescriptionHost = fixture.GetType("NullDescriptionHost");
        var release = fixture.GetType("Release");
        var sample = fixture.GetType("NamingSample");
        var indexer = sample.GetMembers().OfType<IPropertySymbol>()
            .Single(static property => property.IsIndexer);
        var overloads = sample.GetMembers("Build").OfType<IMethodSymbol>()
            .OrderBy(static method => method.Parameters.Length)
            .ToArray();

        Assert.IsTrue(Util.IsECMAScriptRuntimeType(boundaryHost));
        Assert.IsTrue(Util.HasNameResolutionBoundary(boundaryHost));
        Assert.AreEqual("description-name", Util.GetSymbolConfigName(descriptionNamedHost));
        Assert.AreEqual("explicit-name", Util.GetSymbolConfigName(explicitlyNamedHost));
        Assert.IsNull(Util.GetSymbolConfigName(blankNamedHost));
        Assert.IsNull(Util.GetSymbolConfigName(nullExplicitNameHost));
        Assert.IsNull(Util.GetSymbolConfigName(nullDescriptionHost));
        Assert.AreEqual("httpServer", Util.GetConfigOrSymbolName(sample.GetMembers("HTTPServer").Single()));

        var overloadNames = overloads.Select(Util.GetConfigOrSymbolName).ToArray();
        Assert.HasCount(2, overloadNames.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(overloadNames.All(static name => name.StartsWith("build_", StringComparison.Ordinal)));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(release.GetMembers("Id").OfType<IPropertySymbol>().Single()));
        StringAssert.StartsWith(Util.GetMemberIndexerAccessorHelperName(indexer.GetMethod!), "$get_", StringComparison.Ordinal);
        StringAssert.StartsWith(Util.GetMemberIndexerAccessorHelperName(indexer.SetMethod!), "$set_", StringComparison.Ordinal);
    }

    private static SymbolFixture Compile(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "UtilSymbolNamingScenario.cs");
        var compilation = CSharpCompilation.Create(
            "UtilSymbolNamingScenario_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScriptAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var types = syntaxTree.GetRoot().DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .ToDictionary(static symbol => symbol.Name, StringComparer.Ordinal);
        return new SymbolFixture(types);
    }

    private sealed record SymbolFixture(IReadOnlyDictionary<string, INamedTypeSymbol> Types)
    {
        public INamedTypeSymbol GetType(string name) => Types[name];
    }
}
