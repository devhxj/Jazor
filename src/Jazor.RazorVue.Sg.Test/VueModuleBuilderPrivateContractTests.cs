using System.Reflection;
using Acornima;
using Acornima.Ast;
using Jazor.Common.SourceMaps;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueModuleBuilderPrivateContractTests
{
    [TestMethod]
    public void NamingHelpers_KeepSourceCandidatesAndReservedMethodRulesStable()
    {
        var compilation = CreateCompilation();
        var shapes = GetNamedType(compilation, "PrivateContracts.Shapes");
        var nested = GetNamedType(compilation, "PrivateContracts.Shapes+Nested");
        var autoProperty = GetProperty(shapes, "Auto");
        var ordinaryMethod = GetMethod(shapes, "Ordinary");
        var staticConstructor = shapes.StaticConstructors.Single();
        var finalizer = shapes.GetMembers().OfType<IMethodSymbol>().Single(method => method.MethodKind == MethodKind.Destructor);
        var backingField = shapes.GetMembers().OfType<IFieldSymbol>()
            .Single(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, autoProperty));
        var normalField = shapes.GetMembers("Field").OfType<IFieldSymbol>().Single();

        Assert.AreEqual("Field", Invoke<string>("GetSourceDeclaredNameCandidate", normalField));
        Assert.IsNull(Invoke<string?>("GetSourceDeclaredNameCandidate", backingField));
        Assert.AreEqual("Auto", Invoke<string>("GetSourceDeclaredNameCandidate", autoProperty.GetMethod!));
        Assert.AreEqual("Ordinary", Invoke<string>("GetSourceDeclaredNameCandidate", ordinaryMethod));
        Assert.AreEqual("Nested", Invoke<string>("GetSourceDeclaredNameCandidate", nested));
        Assert.AreEqual(
            "PrivateContracts",
            Invoke<string>(
                "GetSourceDeclaredNameCandidate",
                compilation.GlobalNamespace.GetNamespaceMembers().Single(@namespace => @namespace.Name == "PrivateContracts")));

        Assert.IsFalse(Invoke<bool>("ShouldReserveModuleMethodName", staticConstructor));
        Assert.IsFalse(Invoke<bool>("ShouldReserveModuleMethodName", autoProperty.SetMethod!));
        Assert.IsTrue(Invoke<bool>("ShouldReserveModuleMethodName", ordinaryMethod));
        Assert.IsFalse(Invoke<bool>("ShouldReserveModuleMethodName", finalizer));
    }

    [TestMethod]
    public void NamingAndPathHelpers_NormalizeIdentifierAndModuleShapesDeterministically()
    {
        var compilation = CreateCompilation();
        var marked = GetNamedType(compilation, "PrivateContracts.Marked");
        var blank = GetNamedType(compilation, "PrivateContracts.Blank");
        var plain = GetNamedType(compilation, "PrivateContracts.Plain");
        var record = GetNamedType(compilation, "PrivateContracts.RecordShape");
        var value = GetNamedType(compilation, "PrivateContracts.ValueShape");

        Assert.AreEqual("fallback", Invoke<string>("SanitizeJavaScriptIdentifierPart", string.Empty, "fallback"));
        Assert.AreEqual("fallback9_value", Invoke<string>("SanitizeJavaScriptIdentifierPart", "9-value", "fallback"));
        Assert.AreEqual("ready$value", Invoke<string>("SanitizeJavaScriptIdentifierPart", "ready$value", "fallback"));

        Assert.AreEqual("components/marked.mjs", Invoke<string>("GetRelativePath", marked));
        Assert.AreEqual("RazorVue.PrivateContracts/PrivateContracts/Blank.mjs", Invoke<string>("GetRelativePath", blank));
        Assert.AreEqual("RazorVue.PrivateContracts/PrivateContracts/Plain.mjs", Invoke<string>("GetRelativePath", plain));

        Assert.IsTrue(Invoke<bool>("IsRuntimeMemberClass", plain));
        Assert.IsFalse(Invoke<bool>("IsRuntimeMemberClass", record));
        Assert.IsFalse(Invoke<bool>("IsRuntimeMemberClass", value));
    }

    [TestMethod]
    public void ImportHelpers_TrackSpecifierNamesAndResolveVueSfcAssets()
    {
        var module = new Parser().ParseModule(
            """
            import defaultLocal, * as namespaceLocal from "./names.mjs";
            import { sourceName as namedLocal } from "./named.mjs";
            import Child from "./components/child.vue.mjs";
            import "vue";
            """);
        var imports = module.Body.OfType<ImportDeclaration>().ToArray();
        var localNames = new HashSet<string>(StringComparer.Ordinal);

        InvokeVoid("AddImportLocalNames", imports[0], localNames);
        InvokeVoid("AddImportLocalNames", imports[1], localNames);
        Assert.IsTrue(localNames.SetEquals(new[] { "defaultLocal", "namespaceLocal", "namedLocal" }));
        Assert.IsTrue(Invoke<bool>("HasAnyImportLocalName", imports[0], localNames));
        Assert.IsFalse(Invoke<bool>("HasAnyImportLocalName", imports[2], localNames));
        CollectionAssert.AreEquivalent(
            new[] { "defaultLocal", "namespaceLocal", "namedLocal" },
            imports.Take(2)
                .SelectMany(static declaration => declaration.Specifiers)
                .Select(specifier => Invoke<string>("GetImportLocalName", specifier))
                .ToArray());

        var assetArguments = new object?[] { imports[2], "pages/host.mjs", null };
        Assert.IsTrue(Invoke<bool>("TryCreateVueSfcAsset", assetArguments));
        var asset = assetArguments[2] as VueAsset;
        Assert.IsNotNull(asset);
        Assert.AreEqual("pages/components/child.vue", asset.ArtifactPath);
        Assert.AreEqual("vue-sfc", asset.Kind);
        Assert.IsFalse(Invoke<bool>("TryCreateVueSfcAsset", new object?[] { imports[3], "pages/host.mjs", null }));

        Assert.AreEqual(
            "pages/components/child.vue",
            Invoke<string>("ResolveImportArtifactPath", "./components/child.vue", "pages/host.mjs"));
        Assert.AreEqual(
            "shared/child.vue",
            Invoke<string>("ResolveImportArtifactPath", "../shared/child.vue", "pages/host.mjs"));
        var escape = Assert.Throws<TargetInvocationException>(() =>
            Invoke<string>("ResolveImportArtifactPath", "../escape.vue", "host.mjs"));
        StringAssert.Contains(escape.InnerException!.Message, "cannot escape", StringComparison.Ordinal);

        Assert.AreEqual(
            "./entry.mjs",
            Invoke<string>("RebaseRootRelativeModuleSpecifier", "pages/entry.mjs", "pages/host.mjs"));
        Assert.AreEqual(
            "../../components/card.mjs",
            Invoke<string>("RebaseRootRelativeModuleSpecifier", "components/card.mjs", "pages/nested/host.mjs"));
    }

    [TestMethod]
    public void SourceMapPathHelpers_KeepNormalizationAndIndexingDeterministic()
    {
        var separator = Path.DirectorySeparatorChar.ToString();
        Assert.AreEqual(string.Empty, Invoke<string>("EnsureDirectorySeparator", string.Empty));
        Assert.AreEqual("root" + separator, Invoke<string>("EnsureDirectorySeparator", "root"));
        Assert.AreEqual("root" + separator, Invoke<string>("EnsureDirectorySeparator", "root" + separator));

        Assert.AreEqual("component.razor", Invoke<string>("NormalizeSourcePath", string.Empty));
        Assert.AreEqual("Pages/Counter.razor", Invoke<string>("NormalizeSourcePath", "/repo/Pages/Counter.razor"));
        Assert.AreEqual("relative/source.razor", Invoke<string>("NormalizeSourcePath", "relative/source.razor"));
        Assert.AreEqual("Rooted.razor", Invoke<string>("NormalizeSourcePath", Path.Combine(Path.GetTempPath(), "Rooted.razor")));
        Assert.AreEqual("pages/source.mjs", Invoke<string>("NormalizeGeneratedSourcePath", "././pages\\source.mjs"));

        Assert.IsTrue(Invoke<bool>("IsIntermediateSource", "pages/component.mjs", "pages/component.mjs"));
        Assert.IsTrue(Invoke<bool>("IsIntermediateSource", "generated/component.g.cs", "pages/component.mjs"));
        Assert.IsFalse(Invoke<bool>("IsIntermediateSource", "pages/component.cs", "pages/component.mjs"));
        Assert.IsTrue(Invoke<bool>("IsGeneratedCSharpSourcePath", "generated/component.g.cs", "component.g.cs"));
        Assert.IsTrue(Invoke<bool>("IsGeneratedCSharpSourcePath", "generated/component.g.cs", "other.g.cs"));
        Assert.IsFalse(Invoke<bool>("IsGeneratedCSharpSourcePath", "pages/component.razor", "component.g.cs"));
        Assert.AreEqual(1, Invoke<int>("FindGeneratedLine", "before\nscope.$renderDirect()\nafter", "scope.$renderDirect()"));
        Assert.AreEqual(0, Invoke<int>("FindGeneratedLine", "before\nafter", "scope.$renderDirect()"));

        var text = SourceText.From("zero\none");
        var invalidPosition = new object?[] { text, -1, 0, null };
        Assert.IsFalse(Invoke<bool>("TryGetAbsoluteIndex", invalidPosition));
        var clampedPosition = new object?[] { text, 1, 99, null };
        Assert.IsTrue(Invoke<bool>("TryGetAbsoluteIndex", clampedPosition));
        Assert.AreEqual(text.Lines[1].End, clampedPosition[3]);

        var sources = new List<SourceMapSource>();
        var indexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        Assert.AreEqual(0, Invoke<int>("GetOrAddSourceIndex", sources, indexes, "Pages\\Counter.razor", null));
        Assert.AreEqual(0, Invoke<int>("GetOrAddSourceIndex", sources, indexes, "pages/counter.razor", "source"));
        Assert.AreEqual("source", sources[0].Content);
        Assert.AreEqual(1, Invoke<int>("GetOrAddSourceIndex", sources, indexes, "pages/Other.razor", "other"));
    }

    [TestMethod]
    public void ComponentTypeHelpers_ClassifySlotsCallbacksAndMemberNames()
    {
        var compilation = CreateCompilation();
        var shapes = GetNamedType(compilation, "PrivateContracts.ComponentShapes");
        var childContent = GetProperty(shapes, "ChildContent");
        var title = GetProperty(shapes, "Title");
        var callback = GetProperty(shapes, "Changed");
        var genericCallback = GetProperty(shapes, "GenericChanged");
        var auto = GetProperty(shapes, "Auto");
        var ordinary = GetMethod(shapes, "Ordinary");

        Assert.IsTrue(Invoke<bool>("IsChildContentParameter", childContent));
        Assert.IsFalse(Invoke<bool>("IsChildContentParameter", title));
        Assert.IsTrue(Invoke<bool>("IsEventCallbackType", callback.Type));
        Assert.IsTrue(Invoke<bool>("IsEventCallbackType", genericCallback.Type));
        Assert.IsFalse(Invoke<bool>("IsEventCallbackType", title.Type));
        Assert.IsFalse(Invoke<bool>("IsEventCallbackType", new object?[] { null }));

        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [ordinary.OriginalDefinition] = "mappedOrdinary"
        };
        Assert.AreEqual("mappedOrdinary", Invoke<string>("GetRuntimeMemberName", ordinary, declaredNames));
        declaredNames[ordinary.OriginalDefinition] = string.Empty;
        Assert.AreEqual("ordinary", Invoke<string>("GetRuntimeMemberName", ordinary, declaredNames));
        Assert.IsNotNull(Invoke<string?>("GetPropertyBackingFieldName", shapes, auto));
        StringAssert.Contains(Invoke<string>("GetStableSymbolSortKey", ordinary), "PrivateContracts.cs", StringComparison.Ordinal);
        Assert.IsFalse(string.IsNullOrWhiteSpace(Invoke<string>("GetStableSymbolSortKey", compilation.GetSpecialType(SpecialType.System_String))));
    }

    private static T Invoke<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(VueModuleBuilder)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }

    private static void InvokeVoid(string methodName, params object?[] arguments)
        => _ = Invoke<object?>(methodName, arguments);

    private static CSharpCompilation CreateCompilation()
    {
        var source = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;

            namespace PrivateContracts;

            public sealed class Shapes
            {
                private static readonly int Seed = 1;
                public int Field;
                public int Auto { get; init; }
                public static void Ordinary() { }
                ~Shapes() { }
                public sealed class Nested { }
            }

            [ECMAScriptModule("./components/marked")]
            public sealed class Marked { }

            [ECMAScriptModule("")]
            public sealed class Blank { }

            public sealed class Plain { }
            public sealed record RecordShape;
            public readonly struct ValueShape;

            public sealed class ComponentShapes
            {
                [Parameter] public RenderFragment? ChildContent { get; set; }
                [Parameter] public string? Title { get; set; }
                [Parameter] public EventCallback Changed { get; set; }
                [Parameter] public EventCallback<string> GenericChanged { get; set; }
                public int Auto { get; set; }
                public void Ordinary() { }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "PrivateContracts.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.PrivateContracts",
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

    private static IPropertySymbol GetProperty(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IPropertySymbol>().Single();

    private static IMethodSymbol GetMethod(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IMethodSymbol>().Single();
}
