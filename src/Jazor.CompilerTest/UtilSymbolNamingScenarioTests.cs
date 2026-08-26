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

            [ECMAScript]
            public sealed class RuntimeOverloadSample
            {
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

        var runtimeOverloadSample = fixture.GetType("RuntimeOverloadSample");
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
        Assert.AreEqual("HTTPServer", Util.GetConfigOrSymbolName(sample.GetMembers("HTTPServer").Single()));

        var overloadNames = overloads.Select(Util.GetConfigOrSymbolName).ToArray();
        Assert.HasCount(2, overloadNames.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(overloadNames.All(static name => name.StartsWith("Build_", StringComparison.Ordinal)));

        var runtimeOverloadNames = runtimeOverloadSample.GetMembers("Build")
            .OfType<IMethodSymbol>()
            .Select(Util.GetConfigOrSymbolName)
            .ToArray();
        CollectionAssert.AreEqual(new[] { "Build", "Build" }, runtimeOverloadNames);

        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(release.GetMembers("Id").OfType<IPropertySymbol>().Single()));
        StringAssert.StartsWith(Util.GetMemberIndexerAccessorHelperName(indexer.GetMethod!), "$get_", StringComparison.Ordinal);
        StringAssert.StartsWith(Util.GetMemberIndexerAccessorHelperName(indexer.SetMethod!), "$set_", StringComparison.Ordinal);
    }

    [TestMethod]
    public void SymbolNaming_IndexerAccessorHelperRejectsOrdinaryMethodsAndKeepsTupleRuntimeFields()
    {
        var fixture = Compile(
            """
            public sealed class NamingSample
            {
                public (int count, string label) Tuple;

                public void Build()
                {
                }
            }
            """);
        var sample = fixture.GetType("NamingSample");
        var method = sample.GetMembers("Build").OfType<IMethodSymbol>().Single();
        var tuple = sample.GetMembers("Tuple").OfType<IFieldSymbol>().Single().Type as INamedTypeSymbol;
        Assert.IsNotNull(tuple);

        var exception = Assert.Throws<ArgumentException>(
            () => Util.GetMemberIndexerAccessorHelperName(method));
        Assert.AreEqual("symbol", exception.ParamName);
        Assert.AreEqual("count", Util.GetConfigOrSymbolName(tuple.TupleElements[0]));
        Assert.AreEqual("label", Util.GetConfigOrSymbolName(tuple.TupleElements[1]));
    }

    [TestMethod]
    public void ClrImportNames_KeepMemberIdentitySeparateFromRuntimeExportName()
    {
        var fixture = Compile(
            """
            using System;

            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
            public sealed class JazorAttribute : Attribute
            {
                public JazorAttribute()
                {
                }

                public JazorAttribute(object? op, string? member, string? value = null)
                {
                }
            }

            [AttributeUsage(AttributeTargets.Method, Inherited = false)]
            public sealed class OtherAttribute : Attribute
            {
            }

            public sealed class ImportNameHost
            {
                [Other]
                public void Unrelated()
                {
                }

                [Jazor]
                public void MissingArguments()
                {
                }

                [Jazor(null, "Ignored")]
                public void NullOperation()
                {
                }

                [Jazor(2, "AliasOnly")]
                public void AliasOnly()
                {
                }

                [Jazor(3, "")]
                public void EmptyMember()
                {
                }

                [Jazor(3, null)]
                public void NullMember()
                {
                }

                [Jazor(3, "Demo.NoRuntimeValue")]
                public void NoRuntimeValue()
                {
                }

                [Jazor(3, "Demo.EmptyRuntimeValue", "")]
                public void EmptyRuntimeValue()
                {
                }

                [Jazor(3, "Demo.RuntimeNamed", "runtimeNamed")]
                public void RuntimeNamed()
                {
                }

                [Jazor(3, "Demo.MappedProperty", "mappedProperty")]
                public int MappedProperty { get; set; }
            }
            """);
        var host = fixture.GetType("ImportNameHost");

        AssertNoMapping(GetMethod("Unrelated"));
        AssertNoMapping(GetMethod("MissingArguments"));
        AssertNoMapping(GetMethod("NullOperation"));
        AssertNoMapping(GetMethod("AliasOnly"));
        AssertNoMapping(GetMethod("EmptyMember"));
        AssertNoMapping(GetMethod("NullMember"));
        AssertMapping(GetMethod("NoRuntimeValue"), "Demo.NoRuntimeValue", "", hasRuntimeName: false);
        AssertMapping(GetMethod("EmptyRuntimeValue"), "Demo.EmptyRuntimeValue", "", hasRuntimeName: false);
        AssertMapping(GetMethod("RuntimeNamed"), "Demo.RuntimeNamed", "runtimeNamed", hasRuntimeName: true);

        var getter = host.GetMembers("MappedProperty")
            .OfType<IPropertySymbol>()
            .Single()
            .GetMethod!;
        AssertMapping(getter, "Demo.MappedProperty", "mappedProperty", hasRuntimeName: true);

        IMethodSymbol GetMethod(string name)
            => host.GetMembers(name).OfType<IMethodSymbol>().Single();

        static void AssertNoMapping(ISymbol symbol)
        {
            Assert.IsFalse(Util.TryGetJazorImportMapping(symbol, out var memberName, out var runtimeName));
            Assert.AreEqual("", memberName);
            Assert.AreEqual("", runtimeName);
            Assert.IsFalse(Util.TryGetJazorImportRuntimeName(symbol, out runtimeName));
            Assert.AreEqual("", runtimeName);
        }

        static void AssertMapping(ISymbol symbol, string expectedMemberName, string expectedRuntimeName, bool hasRuntimeName)
        {
            Assert.IsTrue(Util.TryGetJazorImportMapping(symbol, out var memberName, out var runtimeName));
            Assert.AreEqual(expectedMemberName, memberName);
            Assert.AreEqual(expectedRuntimeName, runtimeName);
            Assert.AreEqual(hasRuntimeName, Util.TryGetJazorImportRuntimeName(symbol, out runtimeName));
            Assert.AreEqual(expectedRuntimeName, runtimeName);
        }
    }

    [TestMethod]
    public void RuntimeRecordProxy_UsesMappedMembersAndInheritsTheRuntimeMarkerBoundary()
    {
        var fixture = Compile(
            """
            using ECMAScript;

            [ECMAScript]
            public record DirectRuntimeProxy
            {
                [ECMAScriptName("title")]
                public string Title { get; set; } = "";

                public string Plain { get; set; } = "";
            }

            [ECMAScript]
            public record RuntimeProxyBase;

            public record InheritedRuntimeProxy : RuntimeProxyBase
            {
                [ECMAScriptName("code")]
                public int Code { get; set; }
            }
            """);
        var directRuntimeProxy = fixture.GetType("DirectRuntimeProxy");
        var inheritedRuntimeProxy = fixture.GetType("InheritedRuntimeProxy");
        var title = directRuntimeProxy.GetMembers("Title").OfType<IPropertySymbol>().Single();
        var plain = directRuntimeProxy.GetMembers("Plain").OfType<IPropertySymbol>().Single();
        var code = inheritedRuntimeProxy.GetMembers("Code").OfType<IPropertySymbol>().Single();

        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(title));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(title.GetMethod!));
        Assert.IsFalse(Util.IsECMAScriptRecordProxyMember(plain));
        Assert.IsTrue(Util.IsECMAScriptRecordProxyMember(code));
    }

    [TestMethod]
    public void SymbolNaming_ModuleOverloads_PreserveOneRawEntryAlongsideAnExplicitAlias()
    {
        var fixture = Compile(
            """
            using ECMAScript;

            [ECMAScriptModule("./runtime.mjs")]
            public static class ModuleOverloadHost
            {
                public static void Style() { }

                [ECMAScriptName("styleIn")]
                public static void Style(int value) { }

                public static void Configure() { }

                public static void Configure(int value) { }
            }
            """);
        var host = fixture.GetType("ModuleOverloadHost");
        var style = host.GetMembers("Style").OfType<IMethodSymbol>()
            .OrderBy(static method => method.Parameters.Length)
            .ToArray();
        var configure = host.GetMembers("Configure").OfType<IMethodSymbol>()
            .OrderBy(static method => method.Parameters.Length)
            .ToArray();

        CollectionAssert.AreEqual(
            new[] { "Style", "styleIn" },
            style.Select(Util.GetConfigOrSymbolName).ToArray());
        var configureNames = configure.Select(Util.GetConfigOrSymbolName).ToArray();
        Assert.HasCount(2, configureNames.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(configureNames.All(static name => name.StartsWith("Configure_", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void JavaScriptNameMetadata_RecoveredDuplicateExplicitNames_UsesTheFirstSourceValue()
    {
        const string source = """
            using ECMAScript;

            [ECMAScriptName("first")]
            [ECMAScriptName("second")]
            public sealed class RecoveredNameHost
            {
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "RecoveredNameMetadata",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScriptAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptNameAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var host = compilation.GetTypeByMetadataName("RecoveredNameHost")!;

        Assert.IsTrue(compilation.GetDiagnostics().Any(static diagnostic => diagnostic.Id == "CS0579"));
        var metadata = Util.GetJavaScriptNameMetadata(host);
        Assert.IsTrue(metadata.HasECMAScriptNameAttribute);
        Assert.AreEqual("first", metadata.ECMAScriptName);
        Assert.HasCount(2, host.GetAttributes().Where(static attribute =>
            attribute.AttributeClass?.Name == "ECMAScriptNameAttribute"));
        Assert.IsFalse(metadata.HasConflictingExplicitNames);
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
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScriptAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptNameAttribute).Assembly.Location)),
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
