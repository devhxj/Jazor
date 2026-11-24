using Jazor.Npm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmSourceRequestsTests
{
    [TestMethod]
    public void Discover_BindsGlobalFileAndNamespaceImports()
    {
        var requests = Discover(
            """
            global using Tool = global::Npm.EsToolkit.String.Exports;
            global using static Npm.EsToolkit.Exports;
            """,
            """
            using Npm.EsToolkit;
            namespace Local
            {
                using Fs = global::Npm.Scope.Widget.Fs;
                class Screen
                {
                    void Render()
                    {
                        Tool.CamelCase("text");
                        Exports.Chunk("text");
                        Fs.Exports.Open();
                        GlobalExport();
                    }
                }
            }
            """);

        Has(requests, "Npm.EsToolkit.String", "CamelCase");
        Has(requests, "Npm.EsToolkit", "Chunk");
        Has(requests, "Npm.Scope.Widget.Fs", "Open");
        Has(requests, "Npm.EsToolkit", "GlobalExport", unqualified: true);
        Has(requests, "Npm.EsToolkit.String", null);
        Has(requests, "Npm.Scope.Widget.Fs", null);
    }

    [TestMethod]
    public void Discover_UsesRelativePackageQualifiedNamesAndCompilerSymbols()
    {
        var requests = Discover("""
            namespace Npm;
            class Screen
            {
            #if ENABLE_NPM
                void Render() => EsToolkit.String.Exports.KebabCase("text");
            #else
                void Render() => global::Npm.EsToolkit.Exports.Disabled();
            #endif
            }
            """);
        Has(requests, "Npm.EsToolkit.String", "KebabCase");
        Assert.IsFalse(requests.Any(request => request.MemberName == "Disabled"));
    }

    [TestMethod]
    public void Discover_IgnoresResolvedReceiversAndExistingFacades()
    {
        var requests = Discover(
            """
            using static Npm.EsToolkit.Exports;
            using Existing = Npm.Scope.Widget.Exports;
            class Screen
            {
                void Render()
                {
                    var Exports = new Screen();
                    Exports.Local();
                    Local();
                    Existing.Open();
                    global::Npm.Scope.Widget.Exports.Open();
                }
                void Local() { }
            }
            """,
            """
            namespace Npm.Scope.Widget;
            public static class Exports { public static void Open() { } }
            """);

        Has(requests, "Npm.EsToolkit", null);
        Assert.IsFalse(requests.Any(request => request.MemberName is not null));
        Assert.IsFalse(requests.Any(request => request.NpmNamespace.StartsWith("Npm.Scope.Widget", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void Discover_FindsNamespaceOnlyAndIgnoresUndeclaredPackages()
    {
        var requests = Discover("""
            using Npm.EsToolkit;
            using Npm.NotDeclared;
            class Screen { }
            """);
        Assert.HasCount(1, requests);
        Has(requests, "Npm.EsToolkit", null);
    }

    [TestMethod]
    public void Discover_UsesIdentifierValueTextForEscapedAndUnicodeSpelling()
    {
        var requests = Discover("""
            using @ref = global::@Npm.\u0045sToolkit.@Exports;
            class Screen { void Render() => @ref.@Chunk(); }
            """);
        Has(requests, "Npm.EsToolkit", "Chunk");
    }

    [TestMethod]
    public void Discover_DoesNotReplaceAmbiguousFacadesFromTwoAssemblies()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var framework = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var facade = CSharpSyntaxTree.ParseText("namespace Npm.EsToolkit; public static class Exports { }", parseOptions);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        MetadataReference CreateReference(string name)
        {
            using var stream = new MemoryStream();
            var result = CSharpCompilation.Create(name, [facade], [framework], options).Emit(stream);
            Assert.IsTrue(result.Success);
            return MetadataReference.CreateFromImage(stream.ToArray());
        }
        var consumer = CSharpSyntaxTree.ParseText("""
            using Toolkit = Npm.EsToolkit.Exports;
            class Screen { void Render() => Toolkit.Chunk(); }
            """, parseOptions);
        var compilation = CSharpCompilation.Create("Consumer", [consumer], [framework, CreateReference("A"), CreateReference("B")], options);
        Assert.HasCount(0, NpmSourceRequests.Discover(compilation, ["es-toolkit"]));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Discover_RespectsExternAliasFacadeVisibility(bool globallyVisible)
    {
        var framework = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var facade = CSharpSyntaxTree.ParseText("namespace Npm.EsToolkit; public static class Exports { }");
        using var stream = new MemoryStream();
        Assert.IsTrue(CSharpCompilation.Create("Facade", [facade], [framework], options).Emit(stream).Success);
        var reference = MetadataReference.CreateFromImage(stream.ToArray(),
            new MetadataReferenceProperties(aliases: globallyVisible ? ["global", "Facade"] : ["Facade"]));
        var consumer = CSharpSyntaxTree.ParseText("""
            extern alias Facade;
            class Screen
            {
                void Render()
                {
                    global::Npm.EsToolkit.Exports.Chunk();
                    Facade::Npm.EsToolkit.Exports.Other();
                }
            }
            """);
        var compilation = CSharpCompilation.Create("Consumer", [consumer], [framework, reference], options);

        var requests = NpmSourceRequests.Discover(compilation, ["es-toolkit"]);
        if (globallyVisible)
            Assert.HasCount(0, requests);
        else
        {
            Assert.HasCount(1, requests);
            Has(requests, "Npm.EsToolkit", "Chunk");
        }
    }

    [TestMethod]
    public void Completion_BindsUnsavedGlobalAliasButRespectsLocalShadowingAndAuthoredTypes()
    {
        var definitions = CSharpSyntaxTree.ParseText("""
            global using Dates = global::Npm.DateFns.Exports;
            namespace Npm.Local { public static class Exports { } }
            """);
        var author = CSharpSyntaxTree.ParseText("""
            class Screen
            {
                void First() { Dates.; }
                void Shadow(string Dates) { Dates.; }
                void Existing() { Npm.Local.Exports.; }
            }
            """);
        var compilation = CSharpCompilation.Create("Editor", [definitions, author],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var receivers = author.GetRoot().DescendantNodes().OfType<MemberAccessExpressionSyntax>()
            .Where(node => node.Name.IsMissing).Select(node => node.Expression).ToArray();
        Assert.HasCount(3, receivers);
        Assert.AreEqual("Npm.DateFns", NpmSourceRequests.FindCompletionNamespace(compilation, receivers[0], default));
        Assert.IsNull(NpmSourceRequests.FindCompletionNamespace(compilation, receivers[1], default));
        Assert.IsNull(NpmSourceRequests.FindCompletionNamespace(compilation, receivers[2], default));
    }

    private static IReadOnlyList<NpmSourceRequest> Discover(params string[] sources)
    {
        var parseOptions = new CSharpParseOptions(preprocessorSymbols: ["ENABLE_NPM"]);
        var trees = sources.Select((source, index) => CSharpSyntaxTree.ParseText(source, parseOptions, $"file{index}.cs"));
        var compilation = CSharpCompilation.Create("Consumer", trees,
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        return NpmSourceRequests.Discover(compilation, ["es-toolkit", "@scope/widget"]);
    }

    private static void Has(IReadOnlyList<NpmSourceRequest> requests, string ns, string? member, bool unqualified = false) =>
        Assert.IsTrue(requests.Any(request => request.NpmNamespace == ns && request.MemberName == member &&
            request.IsUnqualified == unqualified), $"Missing request for {ns} / {member}.");
}
