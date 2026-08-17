using System.Collections.Immutable;
using System.Reflection;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.Common.SourceMaps;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
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
        var explicitMethod = shapes.GetMembers()
            .OfType<IMethodSymbol>()
            .Single(static method => method.MethodKind == MethodKind.ExplicitInterfaceImplementation);
        var staticConstructor = shapes.StaticConstructors.Single();
        var finalizer = shapes.GetMembers().OfType<IMethodSymbol>().Single(method => method.MethodKind == MethodKind.Destructor);
        var backingField = shapes.GetMembers().OfType<IFieldSymbol>()
            .Single(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, autoProperty));
        var normalField = shapes.GetMembers("Field").OfType<IFieldSymbol>().Single();

        Assert.AreEqual("Field", Invoke<string>("GetSourceDeclaredNameCandidate", normalField));
        Assert.IsNull(Invoke<string?>("GetSourceDeclaredNameCandidate", backingField));
        Assert.AreEqual("Auto", Invoke<string>("GetSourceDeclaredNameCandidate", autoProperty.GetMethod!));
        Assert.AreEqual("Auto", Invoke<string>("GetPreferredModuleDeclaredName", autoProperty.GetMethod!));
        Assert.AreEqual("Auto", Invoke<string>("GetPreferredModuleDeclaredName", autoProperty));
        Assert.AreEqual("Ordinary", Invoke<string>("GetSourceDeclaredNameCandidate", ordinaryMethod));
        Assert.AreEqual("Nested", Invoke<string>("GetSourceDeclaredNameCandidate", nested));
        var backingPreferredName = Invoke<string>("GetPreferredModuleDeclaredName", backingField);
        var backingAlias = Invoke<string>(
            "ChooseModuleDeclaredName",
            backingField,
            new HashSet<string>(StringComparer.Ordinal),
            new HashSet<string>(StringComparer.Ordinal) { backingPreferredName });
        Assert.IsTrue(backingAlias.StartsWith("m$", StringComparison.Ordinal));
        Assert.AreNotEqual(backingPreferredName, backingAlias);
        Assert.IsTrue(Invoke<string>("GetPreferredModuleDeclaredName", explicitMethod).StartsWith("m$", StringComparison.Ordinal));
        Assert.AreEqual(
            "PrivateContracts",
            Invoke<string>(
                "GetSourceDeclaredNameCandidate",
                compilation.GlobalNamespace.GetNamespaceMembers().Single(@namespace => @namespace.Name == "PrivateContracts")));

        Assert.IsFalse(Invoke<bool>("ShouldReserveModuleMethodName", staticConstructor));
        Assert.IsFalse(Invoke<bool>("ShouldReserveModuleMethodName", autoProperty.SetMethod!));
        Assert.IsTrue(Invoke<bool>("ShouldReserveModuleMethodName", ordinaryMethod));
        Assert.IsTrue(Invoke<bool>("ShouldReserveModuleMethodName", explicitMethod));
        Assert.IsFalse(Invoke<bool>("ShouldReserveModuleMethodName", finalizer));

        StringAssert.Contains(
            Invoke<string>("GetStableSymbolSortKey", ordinaryMethod),
            "PrivateContracts.cs",
            StringComparison.Ordinal);
        Assert.AreEqual(
            "string",
            Invoke<string>("GetStableSymbolSortKey", compilation.GetSpecialType(SpecialType.System_String)));

        var lookalikeCompilation = CreateStandaloneCompilation(
            "namespace Lookalike; public readonly struct EventCallback { }",
            "Lookalike.cs");
        var lookalikeEventCallback = lookalikeCompilation.GetTypeByMetadataName("Lookalike.EventCallback");
        Assert.IsNotNull(lookalikeEventCallback);
        Assert.IsFalse(Invoke<bool>("IsEventCallbackType", lookalikeEventCallback!));

        var propertyVariants = CreateStandaloneCompilation(
            """
            public sealed class PropertyVariants
            {
                public int ExpressionOnly => 1;

                public int BodyOnly
                {
                    get
                    {
                        return 1;
                    }
                }
            }
            """,
            "PropertyVariants.cs");
        var propertyVariantType = GetNamedType(propertyVariants, "PropertyVariants");
        Assert.IsFalse(Invoke<bool>("IsAutoProperty", GetProperty(propertyVariantType, "ExpressionOnly")));
        Assert.IsFalse(Invoke<bool>("IsAutoProperty", GetProperty(propertyVariantType, "BodyOnly")));
    }

    [TestMethod]
    public void NamingAndPathHelpers_NormalizeIdentifierAndModuleShapesDeterministically()
    {
        var compilation = CreateCompilation();
        var marked = GetNamedType(compilation, "PrivateContracts.Marked");
        var blank = GetNamedType(compilation, "PrivateContracts.Blank");
        var nullMarked = GetNamedType(compilation, "PrivateContracts.NullMarked");
        var plain = GetNamedType(compilation, "PrivateContracts.Plain");
        var record = GetNamedType(compilation, "PrivateContracts.RecordShape");
        var value = GetNamedType(compilation, "PrivateContracts.ValueShape");

        Assert.AreEqual("fallback", Invoke<string>("SanitizeJavaScriptIdentifierPart", string.Empty, "fallback"));
        Assert.AreEqual("fallback9_value", Invoke<string>("SanitizeJavaScriptIdentifierPart", "9-value", "fallback"));
        Assert.AreEqual("ready$value", Invoke<string>("SanitizeJavaScriptIdentifierPart", "ready$value", "fallback"));

        Assert.AreEqual("components/marked.mjs", Invoke<string>("GetRelativePath", marked));
        Assert.AreEqual("RazorVue.PrivateContracts/PrivateContracts/Blank.mjs", Invoke<string>("GetRelativePath", blank));
        Assert.AreEqual("RazorVue.PrivateContracts/PrivateContracts/NullMarked.mjs", Invoke<string>("GetRelativePath", nullMarked));
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
        Assert.AreEqual("module-source", asset.Kind);
        Assert.AreEqual("pages/components/child.vue.mjs", asset.ImportPath);
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
    public void ImportHelpers_IncludeConstructorInitializationImportsInCollisionReservations()
    {
        var imports = new Parser().ParseModule(
            """
            import compilerLocal from "./compiler.mjs";
            import renderLocal from "./render.mjs";
            import initializerLocal from "./initializer.mjs";
            """)
            .Body
            .OfType<ImportDeclaration>()
            .ToArray();
        var directRender = CreatePrivateRecord(
            "DirectRenderBuildResult",
            new Identifier("render"),
            "$renderDirect",
            CreateImmutableArray(typeof(Statement)),
            CreateImmutableArray(typeof(RenderModuleHoist)),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            CreateImmutableArray(typeof(ImportDeclaration), imports[1]),
            CreateImmutableArray(typeof(ISymbol)));

        var names = Invoke<HashSet<string>>(
            "CollectImportLocalNames",
            null,
            directRender,
            new[] { imports[2] });
        Assert.IsTrue(names.SetEquals(new[] { "renderLocal", "initializerLocal" }));

        var compilerNames = Invoke<HashSet<string>>(
            "CollectCompilerImportLocalNames",
            null,
            new[] { imports[2] });
        Assert.IsTrue(compilerNames.SetEquals(new[] { "initializerLocal" }));

        var compilerModule = new Parser().ParseModule(
            "import compilerLocal from \"./compiler.mjs\";");
        var combinedNames = Invoke<HashSet<string>>(
            "CollectImportLocalNames",
            compilerModule,
            directRender,
            new[] { imports[2] });
        Assert.IsTrue(combinedNames.SetEquals(new[] { "compilerLocal", "renderLocal", "initializerLocal" }));

        var directOnlyNames = Invoke<HashSet<string>>(
            "CollectImportLocalNames",
            null,
            directRender,
            null);
        Assert.IsTrue(directOnlyNames.SetEquals(new[] { "renderLocal" }));
        Assert.IsEmpty(Invoke<HashSet<string>>(
            "CollectCompilerImportLocalNames",
            null,
            null));
    }

    [TestMethod]
    public void RenderFunctionPruningHelper_RemovesOnlyTheBoundBuildRenderTreeDeclaration()
    {
        var module = new Parser().ParseModule(
            """
            function BuildRenderTree() { }
            function retainedHelper() { }
            const retainedValue = 1;
            """);
        var compilerStatements = module.Body
            .OfType<Statement>()
            .Select(statement => CreatePrivateRecord("CompilerStatement", statement, 0, 0))
            .ToArray();

        var retained = Invoke<object>(
            "RemoveBuildRenderTreeFunction",
            CreateImmutableArray(compilerStatements[0].GetType(), compilerStatements),
            "BuildRenderTree");
        var statements = ((System.Collections.IEnumerable)retained)
            .Cast<object>()
            .Select(item => (Statement)item.GetType().GetProperty("Statement")!.GetValue(item)!)
            .Select(static statement => statement.ToKnRECMAScript())
            .ToArray();

        Assert.IsFalse(statements.Any(static statement => statement.Contains("function BuildRenderTree", StringComparison.Ordinal)));
        Assert.IsTrue(statements.Any(static statement => statement.Contains("function retainedHelper", StringComparison.Ordinal)));
        Assert.IsTrue(statements.Any(static statement => statement.Contains("const retainedValue", StringComparison.Ordinal)));
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
    public void ComponentTypeHelpers_ClassifyRenderFragmentsCallbacksAndMemberNames()
    {
        var compilation = CreateCompilation();
        var shapes = GetNamedType(compilation, "PrivateContracts.ComponentShapes");
        var childContent = GetProperty(shapes, "ChildContent");
        var title = GetProperty(shapes, "Title");
        var callback = GetProperty(shapes, "Changed");
        var genericCallback = GetProperty(shapes, "GenericChanged");
        var auto = GetProperty(shapes, "Auto");
        var ordinary = GetMethod(shapes, "Ordinary");

        Assert.IsTrue(Invoke<bool>("IsAnyRenderFragmentType", childContent.Type));
        Assert.IsFalse(Invoke<bool>("IsAnyRenderFragmentType", title.Type));
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
        Assert.AreEqual("Ordinary", Invoke<string>("GetRuntimeMemberName", ordinary, declaredNames));
        Assert.AreEqual(
            "Ordinary",
            Invoke<string>(
                "GetRuntimeMemberName",
                ordinary,
                new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)));
        Assert.IsNotNull(Invoke<string?>("GetPropertyBackingFieldName", shapes, auto));
        StringAssert.Contains(Invoke<string>("GetStableSymbolSortKey", ordinary), "PrivateContracts.cs", StringComparison.Ordinal);
        Assert.IsFalse(string.IsNullOrWhiteSpace(Invoke<string>("GetStableSymbolSortKey", compilation.GetSpecialType(SpecialType.System_String))));
    }

    [TestMethod]
    public void AstLocalAndModuleNamingHelpers_PreserveFramingContracts()
    {
        var compilation = CreateCompilation();
        var shapes = GetNamedType(compilation, "PrivateContracts.Shapes");
        var ordinary = GetMethod(shapes, "Ordinary");
        var explicitMethod = shapes.GetMembers().OfType<IMethodSymbol>()
            .Single(method => method.MethodKind == MethodKind.ExplicitInterfaceImplementation);
        var localShapes = GetMethodBody(compilation, "LocalShapes");
        var loop = localShapes.Descendants().OfType<IForEachLoopOperation>().Single();
        var localNames = Invoke<HashSet<string>>("CollectDirectRenderLocalNames", localShapes);

        foreach (var name in new[] { "ordinary", "text", "field", "first", "rest", "item" })
            Assert.IsTrue(localNames.Contains(name), name);
        var loopArguments = new object?[] { loop.LoopControlVariable, null };
        Assert.IsTrue(Invoke<bool>("TryGetLoopControlVariable", loopArguments));
        Assert.AreEqual("item", ((ILocalSymbol)loopArguments[1]!).Name);
        Assert.IsFalse(Invoke<bool>("TryGetLoopControlVariable", new object?[] { localShapes.Operations[0], null }));

        var preferredName = Invoke<string>("GetPreferredModuleDeclaredName", ordinary);
        Assert.AreEqual(preferredName, Invoke<string>(
            "ChooseModuleDeclaredName",
            ordinary,
            new HashSet<string>(StringComparer.Ordinal),
            new HashSet<string>(StringComparer.Ordinal)));
        var sourceName = Invoke<string?>("GetSourceDeclaredNameCandidate", ordinary);
        Assert.IsNotNull(sourceName);
        var alias = Invoke<string>(
            "ChooseModuleDeclaredName",
            ordinary,
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName! },
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName! });
        Assert.IsTrue(alias.StartsWith("m$", StringComparison.Ordinal));
        Assert.IsTrue(Invoke<string>("GetPreferredModuleDeclaredName", explicitMethod).StartsWith("m$", StringComparison.Ordinal));

        var identifierAccess = Invoke<MemberExpression>("CreateMemberAccess", new Identifier("scope"), "ready");
        var stringAccess = Invoke<MemberExpression>("CreateMemberAccess", new Identifier("scope"), "data-title");
        Assert.IsFalse(identifierAccess.Computed);
        Assert.IsTrue(stringAccess.Computed);
        Assert.IsInstanceOfType<Identifier>(Invoke<ObjectProperty>("CreateObjectProperty", "ready", new Identifier("value")).Key);
        Assert.IsInstanceOfType<StringLiteral>(Invoke<ObjectProperty>("CreateObjectProperty", "data-title", new Identifier("value")).Key);

        var minimalVueImport = Invoke<ImportDeclaration>("BuildVueImportDeclaration", false, false, false, false, false, false, false, false, false, false, false, false, false);
        CollectionAssert.AreEquivalent(
            new[] { "defineComponent", "h" },
            GetImportedNames(minimalVueImport));
        var fullVueImport = Invoke<ImportDeclaration>("BuildVueImportDeclaration", true, true, true, true, true, true, true, true, true, true, true, true, true);
        CollectionAssert.AreEquivalent(
            new[] { "defineComponent", "h", "Fragment", "createStaticVNode", "openBlock", "createElementBlock", "createBlock", "createTextVNode", "renderList", "withCtx", "createSlots", "onMounted", "onUnmounted", "onUpdated", "reactive", "watch" },
            GetImportedNames(fullVueImport));
        var framingReservedNames = (ImmutableHashSet<string>)typeof(VueModuleBuilder)
            .GetField("FramingReservedNames", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        Assert.IsTrue(framingReservedNames.Contains("createTextVNode"));
        Assert.IsTrue(framingReservedNames.Contains("renderList"));
        Assert.IsTrue(framingReservedNames.Contains("withCtx"));
        Assert.IsTrue(framingReservedNames.Contains("createSlots"));

        var module = new Parser().ParseModule(
            """
            import { h } from "vue";
            import local from "./local.mjs";
            """);
        var imports = module.Body.OfType<ImportDeclaration>().ToArray();
        Assert.IsTrue(Invoke<bool>("IsVueFramingImport", imports[0]));
        Assert.IsFalse(Invoke<bool>("IsVueFramingImport", imports[1]));
    }

    [TestMethod]
    public void BranchHelpers_ResolveNestedRuntimeClassesAndImportCollisions()
    {
        var compilation = CreateCompilation();
        var component = GetNamedType(compilation, "PrivateContracts.ComponentContainer");
        var directNested = GetNamedType(compilation, "PrivateContracts.ComponentContainer+RuntimeOuter");
        var nestedRuntime = GetNamedType(compilation, "PrivateContracts.ComponentContainer+RuntimeOuter+RuntimeInner");
        var recordNested = GetNamedType(compilation, "PrivateContracts.ComponentContainer+RecordOuter+RecordInner");
        var foreignNested = GetNamedType(compilation, "PrivateContracts.ForeignContainer+RuntimeOuter+RuntimeInner");
        var value = GetNamedType(compilation, "PrivateContracts.ValueShape");

        Assert.IsFalse(Invoke<bool>("IsFlattenedRuntimeClass", component, directNested));
        Assert.IsTrue(Invoke<bool>("IsFlattenedRuntimeClass", component, nestedRuntime));
        Assert.IsFalse(Invoke<bool>("IsFlattenedRuntimeClass", component, recordNested));
        Assert.IsFalse(Invoke<bool>("IsFlattenedRuntimeClass", component, foreignNested));
        Assert.IsFalse(Invoke<bool>("IsFlattenedRuntimeClass", component, value));

        var localShapes = GetMethodBody(compilation, "LocalShapes");
        var ordinaryDeclarator = localShapes.Descendants()
            .OfType<IVariableDeclaratorOperation>()
            .Single(declarator => declarator.Symbol.Name == "ordinary");
        var convertedInitializer = localShapes.Descendants()
            .OfType<IVariableDeclaratorOperation>()
            .Single(declarator => declarator.Symbol.Name == "converted")
            .Initializer!
            .Value;
        var declaredLocal = new object?[] { ordinaryDeclarator, null };
        var convertedLocal = new object?[] { convertedInitializer, null };
        Assert.IsTrue(Invoke<bool>("TryGetLoopControlVariable", declaredLocal));
        Assert.AreEqual("ordinary", ((ILocalSymbol)declaredLocal[1]!).Name);
        Assert.IsTrue(Invoke<bool>("TryGetLoopControlVariable", convertedLocal));
        Assert.AreEqual("ordinary", ((ILocalSymbol)convertedLocal[1]!).Name);

        var ordinary = GetMethod(GetNamedType(compilation, "PrivateContracts.Shapes"), "Ordinary");
        var preferredName = Invoke<string>("GetPreferredModuleDeclaredName", ordinary);
        var sourceName = Invoke<string?>("GetSourceDeclaredNameCandidate", ordinary);
        Assert.IsNotNull(sourceName);
        var sourceFallback = Invoke<string>(
            "ChooseModuleDeclaredName",
            ordinary,
            new HashSet<string>(StringComparer.Ordinal) { preferredName },
            new HashSet<string>(StringComparer.Ordinal));
        Assert.IsTrue(sourceFallback.StartsWith("m$", StringComparison.Ordinal));
        var firstAlias = Invoke<string>(
            "ChooseModuleDeclaredName",
            ordinary,
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName!, "m$placeholder" },
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName! });
        var secondAlias = Invoke<string>(
            "ChooseModuleDeclaredName",
            ordinary,
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName!, firstAlias },
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName! });
        Assert.IsTrue(secondAlias.EndsWith("$1", StringComparison.Ordinal));

        var imports = new Parser().ParseModule(
            """
            import defaultLocal from "./default.mjs";
            import * as namespaceLocal from "./namespace.mjs";
            import { source as namedLocal } from "./named.mjs";
            import "./side-effect.mjs";
            """)
            .Body
            .OfType<ImportDeclaration>()
            .ToArray();
        Assert.AreEqual("defaultLocal", Invoke<string>("GetImportLocalName", imports[0].Specifiers[0]));
        Assert.AreEqual("namespaceLocal", Invoke<string>("GetImportLocalName", imports[1].Specifiers[0]));
        Assert.AreEqual("namedLocal", Invoke<string>("GetImportLocalName", imports[2].Specifiers[0]));

        Assert.AreEqual("pages/host.mjs", Invoke<string>("ResolveImportArtifactPath", "host.mjs", "pages/host.mjs"));
        Assert.AreEqual("./host.mjs", Invoke<string>("RebaseRootRelativeModuleSpecifier", "pages/host.mjs", "pages/host.mjs"));
    }

    [TestMethod]
    public void ImportBindingFilter_DeduplicatesEquivalentBindingsAndRejectsCollisions()
    {
        var imports = new Parser().ParseModule(
            """
            import { source as namedLocal } from "./named.mjs";
            import defaultLocal from "./default.mjs";
            import * as namespaceLocal from "./namespace.mjs";
            import { "wire-name" as stringLocal } from "./wire.mjs";
            import "./side-effect.mjs";
            """)
            .Body
            .OfType<ImportDeclaration>()
            .ToArray();
        var duplicate = new Parser().ParseModule(
            "import { source as namedLocal } from \"./named.mjs\";")
            .Body
            .OfType<ImportDeclaration>()
            .Single();
        var collision = new Parser().ParseModule(
            "import { other as namedLocal } from \"./other.mjs\";")
            .Body
            .OfType<ImportDeclaration>()
            .Single();
        var bindingType = typeof(VueModuleBuilder).GetNestedType("ImportBinding", BindingFlags.NonPublic);
        Assert.IsNotNull(bindingType);
        var bindings = Activator.CreateInstance(
            typeof(Dictionary<,>).MakeGenericType(typeof(string), bindingType!));
        Assert.IsNotNull(bindings);

        foreach (var index in new[] { 0, 1, 2, 3 })
        {
            var retained = Invoke<ImportDeclaration?>(
                "FilterEmittedImportSpecifiers",
                imports[index],
                bindings);
            Assert.IsNotNull(retained);
            Assert.HasCount(1, retained.Specifiers);
        }

        Assert.IsNull(Invoke<ImportDeclaration?>(
            "FilterEmittedImportSpecifiers",
            duplicate,
            bindings));
        Assert.AreSame(
            imports[4],
            Invoke<ImportDeclaration?>("FilterEmittedImportSpecifiers", imports[4], bindings));

        var collisionException = Assert.Throws<TargetInvocationException>(() =>
            Invoke<ImportDeclaration?>("FilterEmittedImportSpecifiers", collision, bindings));
        StringAssert.Contains(collisionException.InnerException!.Message, "namedLocal", StringComparison.Ordinal);
        StringAssert.Contains(collisionException.InnerException!.Message, "Import aliases must be unique", StringComparison.Ordinal);
    }

    [TestMethod]
    public void DirectRenderNamingAndSlotHelpers_RespectImportsReservedNamesAndBracketAccess()
    {
        var fixture = CreateRuntimeComponentFixture();
        var names = Invoke<IReadOnlyDictionary<ISymbol, string>>(
            "BuildDirectRenderDeclaredNames",
            fixture.Component,
            fixture.Closure,
            new[] { "counter", "RuntimeOuter", "RuntimeInner", "RuntimeLeaf" });

        Assert.IsNotEmpty(names);
        Assert.IsFalse(names.Values.Contains("counter", StringComparer.Ordinal));
        Assert.IsFalse(names.Values.Contains("RuntimeOuter", StringComparer.Ordinal));
        Assert.AreEqual(names.Count, names.Values.Distinct(StringComparer.Ordinal).Count());

        var identifierSlot = Invoke<ConditionalExpression>("BuildDirectRenderSlotValueExpression", "header");
        var identifierAccess = (MemberExpression)identifierSlot.Consequent;
        Assert.IsFalse(identifierAccess.Computed);
        Assert.AreEqual("header", ((Identifier)identifierAccess.Property).Name);

        var bracketSlot = Invoke<ConditionalExpression>("BuildDirectRenderSlotValueExpression", "item-value");
        var bracketAccess = (MemberExpression)bracketSlot.Consequent;
        Assert.IsTrue(bracketAccess.Computed);
        Assert.AreEqual("item-value", ((StringLiteral)bracketAccess.Property).Value);

        var imports = new Parser().ParseModule(
            """
            import local from "./local.mjs";
            import packageLocal from "package";
            """)
            .Body
            .OfType<ImportDeclaration>()
            .ToArray();
        Assert.AreEqual(
            "../local.mjs",
            Invoke<ImportDeclaration>("RebaseImportDeclaration", imports[0], "pages/host.mjs").Source.Value);
        Assert.AreSame(
            imports[1],
            Invoke<ImportDeclaration>("RebaseImportDeclaration", imports[1], "pages/host.mjs"));
    }

    [TestMethod]
    public void PathAndIdentifierEdgeCases_PreserveArtifactAndSourceMapContracts()
    {
        var compilation = CreateCompilation();
        var noArgumentMarked = GetNamedType(compilation, "PrivateContracts.NoArgumentMarked");
        var noisyMarked = GetNamedType(compilation, "PrivateContracts.NoisyMarked");

        Assert.AreEqual(
            "RazorVue.PrivateContracts/PrivateContracts/NoArgumentMarked.mjs",
            Invoke<string>("GetRelativePath", noArgumentMarked));
        Assert.AreEqual("components/noisy.mjs", Invoke<string>("GetRelativePath", noisyMarked));
        var globalCompilation = CreateStandaloneCompilation("public sealed class GlobalComponent { }", "GlobalComponent.cs");
        var globalComponent = globalCompilation.GetTypeByMetadataName("GlobalComponent");
        Assert.IsNotNull(globalComponent);
        Assert.AreEqual("Standalone/GlobalComponent.mjs", Invoke<string>("GetRelativePath", globalComponent!));

        var emptyImport = Assert.Throws<TargetInvocationException>(() =>
            Invoke<string>("ResolveImportArtifactPath", ".", "host.mjs"));
        StringAssert.Contains(emptyImport.InnerException!.Message, "cannot be empty", StringComparison.Ordinal);
        Assert.AreEqual(
            "../../shared/card.mjs",
            Invoke<string>("RebaseRootRelativeModuleSpecifier", "../shared/card.mjs", "pages/host.mjs"));
        Assert.AreEqual("./", Invoke<string>("RebaseRootRelativeModuleSpecifier", string.Empty, string.Empty));

        Assert.IsTrue(Invoke<bool>("IsJavaScriptIdentifierStart", '$'));
        Assert.IsTrue(Invoke<bool>("IsJavaScriptIdentifierStart", '_'));
        Assert.IsTrue(Invoke<bool>("IsJavaScriptIdentifierStart", 'a'));
        Assert.IsFalse(Invoke<bool>("IsJavaScriptIdentifierStart", '1'));
        Assert.IsTrue(Invoke<bool>("IsJavaScriptIdentifierPart", '$'));
        Assert.IsTrue(Invoke<bool>("IsJavaScriptIdentifierPart", '_'));
        Assert.IsTrue(Invoke<bool>("IsJavaScriptIdentifierPart", '1'));
        Assert.IsFalse(Invoke<bool>("IsJavaScriptIdentifierPart", '-'));
        Assert.AreEqual(
            "sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
            Invoke<string>("ComputeContentHash", string.Empty));
        Assert.AreNotEqual(Invoke<string>("ComputeContentHash", string.Empty), Invoke<string>("ComputeContentHash", "content"));

        var text = SourceText.From("zero\none");
        var negativeColumn = new object?[] { text, 0, -1, null };
        Assert.IsTrue(Invoke<bool>("TryGetAbsoluteIndex", negativeColumn));
        Assert.AreEqual(0, negativeColumn[3]);
    }

    [TestMethod]
    public void GeneratedCSharpSourceMapHelpers_ProjectOnlyMappedRazorLocations()
    {
        var generatedCSharp = SourceText.From("alpha\nbeta\ngamma");
        var mappings = ImmutableArray.Create(
            new RazorSourceMap(
                new RazorSourceSpan("Pages/Counter.razor", 0, 3, 4, 2),
                new RazorSourceSpan("Generated/Counter.razor.g.cs", 0, 5, 0, 0)),
            new RazorSourceMap(
                new RazorSourceSpan("Pages/Counter.razor", 4, 3, 5, 1),
                new RazorSourceSpan("Generated/Counter.razor.g.cs", 6, 4, 1, 0)));
        var document = new GeneratedDocument(
            "Generated/Counter.razor.g.cs",
            "Pages/Counter.razor",
            generatedCSharp,
            mappings);
        var compilerMap = new SourceMapDocument(
            "component.mjs",
            [
                new SourceMapSource("Generated/Counter.razor.g.cs", null),
                new SourceMapSource("External/Helper.cs", null)
            ],
            [
                new SourceMapSegment(0, 2, 0, 0, 2),
                new SourceMapSegment(1, 1, 1, 0, 0),
                new SourceMapSegment(2, 0, 3, 0, 0),
                new SourceMapSegment(99, 0, 0, 0, 0)
            ]);

        var generatedMap = Invoke<SourceMapDocument>("BuildGeneratedCSharpSourceMap", document, compilerMap);
        Assert.IsTrue(generatedMap.Segments.Count >= 3);
        Assert.IsTrue(generatedMap.Sources.All(static source => source.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)));

        var mappedPosition = new object?[] { document, mappings, 1, 2, null };
        Assert.IsTrue(Invoke<bool>("TryResolveOriginalSourcePosition", mappedPosition));
        Assert.IsNotNull(mappedPosition[4]);
        Assert.IsFalse(Invoke<bool>(
            "TryResolveOriginalSourcePosition",
            new object?[] { document, ImmutableArray<RazorSourceMap>.Empty, 0, 0, null }));
        Assert.IsFalse(Invoke<bool>(
            "TryResolveOriginalSourcePosition",
            new object?[] { document, mappings, 99, 0, null }));

        var beforeFirstMapping = ImmutableArray.Create(new RazorSourceMap(
            new RazorSourceSpan("Pages/Before.razor", 0, 1, 0, 0),
            new RazorSourceSpan("Generated/Before.razor.g.cs", 6, 1, 1, 0)));
        var beforeDocument = new GeneratedDocument(
            "Generated/Before.razor.g.cs",
            "Pages/Before.razor",
            SourceText.From("alpha\nbeta"),
            beforeFirstMapping);
        Assert.IsFalse(Invoke<bool>(
            "TryResolveOriginalSourcePosition",
            new object?[] { beforeDocument, beforeFirstMapping, 0, 0, null }));

        var zeroLengthOriginal = ImmutableArray.Create(new RazorSourceMap(
            new RazorSourceSpan("Pages/Zero.razor", 0, 0, 4, 2),
            new RazorSourceSpan("Generated/Zero.razor.g.cs", 0, 1, 0, 0)));
        var zeroLengthDocument = new GeneratedDocument(
            "Generated/Zero.razor.g.cs",
            "Pages/Zero.razor",
            SourceText.From("x"),
            zeroLengthOriginal);
        var zeroLengthArguments = new object?[] { zeroLengthDocument, zeroLengthOriginal, 0, 0, null };
        Assert.IsTrue(Invoke<bool>("TryResolveOriginalSourcePosition", zeroLengthArguments));
        Assert.IsNotNull(zeroLengthArguments[4]);

        var pruned = Invoke<SourceMapDocument>(
            "PruneIntermediateSources",
            new SourceMapDocument(
                "component.mjs",
                [
                    new SourceMapSource("component.mjs", null),
                    new SourceMapSource("Generated/Counter.razor.g.cs", null),
                    new SourceMapSource("Pages/Counter.razor", "@page \"/\"")
                ],
                [
                    new SourceMapSegment(0, 0, 0, 0, 0),
                    new SourceMapSegment(1, 0, 1, 0, 0),
                    new SourceMapSegment(2, 0, 2, 0, 0),
                    new SourceMapSegment(3, 0, 3, 0, 0)
                ]),
            "component.mjs");
        Assert.HasCount(1, pruned.Sources);
        Assert.HasCount(1, pruned.Segments);

        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        InvokeVoid("AddModuleMapAlias", aliases, string.Empty, "map");
        InvokeVoid("AddModuleMapAlias", aliases, "./Generated/Counter.razor.g.cs", "map");
        Assert.AreEqual("map", aliases["Generated/Counter.razor.g.cs"]);
    }

    [TestMethod]
    public void SourceMapAndSymbolFallbackEdges_KeepGeneratedArtifactsDeterministic()
    {
        var globalCompilation = CreateStandaloneCompilation(
            "public sealed class GlobalComponent { }",
            "GlobalComponent.cs");
        var globalComponent = globalCompilation.GetTypeByMetadataName("GlobalComponent");
        Assert.IsNotNull(globalComponent);
        Assert.AreEqual("Standalone/GlobalComponent.mjs", Invoke<string>("GetRelativePath", globalComponent!));

        var callbackCompilation = CreateStandaloneCompilation(
            "namespace Microsoft.AspNetCore.Components; public sealed class EventCallback<TLeft, TRight> { }",
            "EventCallbackFallback.cs");
        var callbackFallback = callbackCompilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallback`2");
        Assert.IsNotNull(callbackFallback);
        Assert.IsTrue(Invoke<bool>("IsEventCallbackType", callbackFallback!));

        var emptyDocument = new GeneratedDocument(
            "Generated/Empty.razor.g.cs",
            "Pages/Empty.razor",
            SourceText.From(string.Empty),
            []);
        var emptyMap = Invoke<SourceMapDocument>(
            "BuildGeneratedCSharpSourceMap",
            emptyDocument,
            new SourceMapDocument("empty.mjs", [], []));
        Assert.IsEmpty(emptyMap.Sources);
        Assert.IsEmpty(emptyMap.Segments);

        var fallbackMappings = ImmutableArray.Create(new RazorSourceMap(
            new RazorSourceSpan(null, 0, 1, 0, 0),
            new RazorSourceSpan(null, 0, 1, 0, 0)));
        var fallbackDocument = new GeneratedDocument(
            "Generated/Fallback.razor.g.cs",
            "Pages/Fallback.razor",
            SourceText.From("x"),
            fallbackMappings);
        var fallbackMap = Invoke<SourceMapDocument>(
            "BuildGeneratedCSharpSourceMap",
            fallbackDocument,
            new SourceMapDocument(
                "fallback.mjs",
                [new SourceMapSource("Generated/Fallback.razor.g.cs", null)],
                [
                    new SourceMapSegment(0, 0, -1, 0, 0),
                    new SourceMapSegment(0, 0, 0, 0, 0)
                ]));
        Assert.IsTrue(fallbackMap.Sources.Any(static source => source.Path == "Pages/Fallback.razor"));

        Assert.AreEqual("component.razor", Invoke<string>("NormalizeSourcePath", new object?[] { null }));
        Assert.AreEqual(
            "component.razor",
            Invoke<string>("NormalizeSourcePath", Path.GetPathRoot(Path.GetTempPath())!));
        Assert.AreEqual(string.Empty, Invoke<string>("NormalizeGeneratedSourcePath", new object?[] { null }));
    }

    [TestMethod]
    public void RuntimeClassAndCompilerModuleParts_KeepClosureDrivenOutputDeterministic()
    {
        var fixture = CreateRuntimeComponentFixture();

        var flattened = Invoke<ImmutableArray<INamedTypeSymbol>>(
            "GetFlattenedRuntimeClasses",
            fixture.Component.ComponentSymbol,
            fixture.Closure);
        var flattenedNames = flattened.Select(static type => type.Name).ToArray();
        var closureNames = fixture.Closure.OrderedMembers.Select(static member => member.Name).ToArray();
        Assert.IsTrue(
            flattenedNames.Contains("RuntimeInner", StringComparer.Ordinal),
            "Flattened: " + string.Join(", ", flattenedNames) + "; Closure: " + string.Join(", ", closureNames));
        Assert.IsTrue(
            flattenedNames.Contains("RuntimeLeaf", StringComparer.Ordinal),
            "Flattened: " + string.Join(", ", flattenedNames) + "; Closure: " + string.Join(", ", closureNames));
        Assert.IsTrue(
            Array.IndexOf(flattenedNames, "RuntimeLeaf") < Array.IndexOf(flattenedNames, "RuntimeInner"),
            string.Join(", ", flattenedNames));

        var emptyParts = Invoke<object>("BuildCompilerModuleParts", null, null, fixture.Closure);
        Assert.IsNotNull(emptyParts);

        var module = new Parser().ParseModule(
            """
            import { source } from "./source.mjs";
            export { source };
            let counter = 1;
            let secondary = 2, kept = 3;
            let [destructured] = source;
            render();
            """);
        var positions = new Dictionary<Node, GeneratedNodePosition>();
        var nextLine = 0;
        foreach (var statement in module.Body)
        {
            AddNodePosition(statement);
            if (statement is VariableDeclaration declaration)
                AddVariableInitializerPositions(declaration);
        }

        var parts = Invoke<object>("BuildCompilerModuleParts", module, positions, fixture.Closure);
        var importDeclarations = GetRecordItems(parts, "ImportDeclarations");
        var moduleStatements = GetRecordItems(parts, "ModuleStatements");
        var setupStatements = GetRecordItems(parts, "SetupStatements");
        var stateSlots = GetRecordItems(parts, "StateSlots");
        Assert.HasCount(1, importDeclarations);
        Assert.IsEmpty(moduleStatements);
        Assert.HasCount(3, setupStatements);
        Assert.AreEqual(
            2,
            stateSlots.Count(slot => slot.GetType().GetProperty("Initializer")!.GetValue(slot) is not null));

        var missingPositions = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("BuildCompilerModuleParts", module, null, fixture.Closure));
        StringAssert.Contains(
            missingPositions.InnerException!.Message,
            "Compiler AST node positions are required",
            StringComparison.Ordinal);

        void AddNodePosition(Node node)
            => positions[node] = new GeneratedNodePosition(nextLine++, 0);

        void AddVariableInitializerPositions(VariableDeclaration declaration)
        {
            foreach (var declarator in declaration.Declarations)
            {
                if (declarator.Init is not null)
                    AddNodePosition(declarator.Init);
            }
        }
    }

    [TestMethod]
    public void StaticComponentProjection_UsesModuleLifetimeNamesAndClrDefaultSlots()
    {
        var fixture = CreateStaticRuntimeComponentFixture();
        var componentType = fixture.Component.ComponentSymbol;
        var staticField = componentType.GetMembers("staticField").OfType<IFieldSymbol>().Single();
        var staticAuto = GetProperty(componentType, "StaticAuto");
        var staticWriteOnly = GetProperty(componentType, "StaticWriteOnly");
        var staticComputed = GetProperty(componentType, "StaticComputed");
        var staticMethod = GetMethod(componentType, "Touch");
        var autoBackingField = componentType
            .GetMembers("<StaticAuto>k__BackingField")
            .OfType<IFieldSymbol>()
            .Single();
        Assert.IsNotNull(staticAuto.GetMethod);
        Assert.IsNotNull(staticAuto.SetMethod);
        Assert.IsNull(staticWriteOnly.GetMethod);
        Assert.IsNotNull(staticWriteOnly.SetMethod);
        Assert.IsNotNull(staticComputed.GetMethod);
        Assert.IsNotNull(staticComputed.SetMethod);

        var declaredNames = Invoke<IReadOnlyDictionary<ISymbol, string>>(
            "BuildDirectRenderDeclaredNames",
            fixture.Component,
            fixture.Closure,
            new[] { "StaticAuto" });
        Assert.IsTrue(declaredNames.ContainsKey(staticField.OriginalDefinition));
        Assert.IsTrue(declaredNames.ContainsKey(autoBackingField.OriginalDefinition));
        Assert.IsTrue(declaredNames.ContainsKey(staticAuto.GetMethod!.OriginalDefinition));
        Assert.IsTrue(declaredNames.ContainsKey(staticAuto.SetMethod!.OriginalDefinition));
        Assert.IsTrue(declaredNames.ContainsKey(staticWriteOnly.SetMethod!.OriginalDefinition));
        Assert.IsTrue(declaredNames.ContainsKey(staticComputed.GetMethod!.OriginalDefinition));
        Assert.IsTrue(declaredNames.ContainsKey(staticComputed.SetMethod!.OriginalDefinition));
        Assert.IsTrue(declaredNames.ContainsKey(staticMethod.OriginalDefinition));
        Assert.AreNotEqual("StaticAuto", declaredNames[staticAuto.GetMethod.OriginalDefinition]);

        var staticTypes = Invoke<Dictionary<string, ITypeSymbol>>(
            "BuildStaticFieldTypeMap",
            fixture.Closure,
            declaredNames);
        Assert.AreEqual(staticField.Type, staticTypes[declaredNames[staticField.OriginalDefinition]]);
        Assert.AreEqual(staticAuto.Type, staticTypes[declaredNames[autoBackingField.OriginalDefinition]]);

        var moduleLifetimeFunctions = Invoke<HashSet<string>>(
            "BuildModuleLifetimeFunctionNames",
            fixture.Closure,
            declaredNames);
        Assert.Contains(declaredNames[staticMethod.OriginalDefinition], moduleLifetimeFunctions);
        Assert.Contains(declaredNames[staticComputed.GetMethod.OriginalDefinition], moduleLifetimeFunctions);
        Assert.Contains(declaredNames[staticComputed.SetMethod.OriginalDefinition], moduleLifetimeFunctions);

        var initialized = new VariableDeclarator(
            new Identifier("initialized"),
            new NumericLiteral(1, "1"));
        Assert.AreSame(
            initialized,
            Invoke<VariableDeclarator>(
                "ProjectStaticFieldDeclarator",
                initialized,
                staticField.Type));

        var uninitialized = new VariableDeclarator(new Identifier("uninitialized"), null);
        var projected = Invoke<VariableDeclarator>(
            "ProjectStaticFieldDeclarator",
            uninitialized,
            staticField.Type);
        Assert.AreNotSame(uninitialized, projected);
        Assert.IsNotNull(projected.Init);
    }

    [TestMethod]
    public void InitializationAndReferenceCaptureHelpers_PreserveSetupOrderingAndDefaultStateSlots()
    {
        var compilation = CreateStandaloneCompilation(
            """
            namespace PrivateContracts;

            public class InitializationBase
            {
                public int baseField = 4;
            }

            public sealed class InitializationComponent : InitializationBase
            {
                public int counter = 1;
                public int secondary;
            }
            """,
            "InitializationComponent.razor.g.cs");
        var componentBase = GetNamedType(compilation, "PrivateContracts.InitializationBase");
        var component = GetNamedType(compilation, "PrivateContracts.InitializationComponent");
        var baseField = componentBase.GetMembers("baseField").OfType<IFieldSymbol>().Single();
        var counter = component.GetMembers("counter").OfType<IFieldSymbol>().Single();
        var secondary = component.GetMembers("secondary").OfType<IFieldSymbol>().Single();

        var baseSlot = CreatePrivateRecord(
            "StateSlot",
            baseField,
            "baseField",
            "baseField",
            baseField.Type,
            new NumericLiteral(4, "4"),
            true,
            null,
            null);

        var initializedSlot = CreatePrivateRecord(
            "StateSlot",
            counter,
            "counter",
            "counter",
            counter.Type,
            new NumericLiteral(5, "5"),
            true,
            null,
            null);
        var defaultedSlot = CreatePrivateRecord(
            "StateSlot",
            secondary,
            "secondary",
            "secondary",
            secondary.Type,
            null,
            true,
            null,
            null);
        var skippedSlot = CreatePrivateRecord(
            "StateSlot",
            counter,
            "skipped",
            "skipped",
            counter.Type,
            null,
            false,
            null,
            null);
        var stateSlots = CreateImmutableArray(
            initializedSlot.GetType(),
            baseSlot,
            initializedSlot,
            defaultedSlot,
            skippedSlot);
        var constructorStatement = new Parser().ParseModule("runConstructor();").Body.Single();
        var phases = ImmutableArray.Create(
            new ComponentInitializationPhaseBuild(componentBase!, null),
            new ComponentInitializationPhaseBuild(component, constructorStatement));

        var statements = Invoke<IEnumerable<Statement>>(
                "BuildComponentInitializationStatements",
                stateSlots,
                phases)
            .Select(static statement => statement.ToKnRECMAScript())
            .ToArray();
        CollectionAssert.AreEqual(
            new[]
            {
                "state.baseField = 4",
                "state.counter = 5",
                "state.secondary = 0",
                "runConstructor()"
            },
            statements,
            string.Join(Environment.NewLine, statements));
        Assert.IsTrue(
            Invoke<string>("GetStableSourceOrder", counter)
                .Contains("InitializationComponent.razor.g.cs", StringComparison.Ordinal));
        var stringType = compilation.GetSpecialType(SpecialType.System_String);
        var metadataEmpty = stringType.GetMembers("Empty")
            .OfType<IFieldSymbol>()
            .Single();
        Assert.IsTrue(
            Invoke<string>("GetStableSourceOrder", metadataEmpty)
                .StartsWith("~|", StringComparison.Ordinal));
        var metadataLength = stringType
            .GetMembers("Length")
            .OfType<IPropertySymbol>()
            .Single();
        Assert.IsFalse(Invoke<bool>("IsAutoProperty", metadataLength));

        var capturedSlots = Invoke<object>(
            "ApplyReferenceCaptureStateInitializers",
            stateSlots,
            ImmutableArray.Create<ISymbol>(secondary));
        var captured = ((System.Collections.IEnumerable)capturedSlots).Cast<object>().ToArray();
        Assert.IsInstanceOfType<NumericLiteral>(
            captured[0].GetType().GetProperty("Initializer")!.GetValue(captured[0]));
        Assert.IsInstanceOfType<NumericLiteral>(
            captured[1].GetType().GetProperty("Initializer")!.GetValue(captured[1]));
        Assert.IsInstanceOfType<NullLiteral>(
            captured[2].GetType().GetProperty("Initializer")!.GetValue(captured[2]));
        Assert.IsNull(captured[3].GetType().GetProperty("Initializer")!.GetValue(captured[3]));

        var unchangedSlots = Invoke<object>(
            "ApplyReferenceCaptureStateInitializers",
            stateSlots,
            ImmutableArray<ISymbol>.Empty);
        var unchanged = ((System.Collections.IEnumerable)unchangedSlots).Cast<object>().ToArray();
        Assert.IsNull(unchanged[2].GetType().GetProperty("Initializer")!.GetValue(unchanged[2]));
    }

    [TestMethod]
    public void DirectRenderFunctionPruning_KeepsConstructorAndStateReachableHelpers()
    {
        var fixture = CreateRuntimeComponentFixture();
        var component = fixture.Component.ComponentSymbol;
        var counter = component.GetMembers("counter").OfType<IFieldSymbol>().Single();
        var module = new Parser().ParseModule(
            """
            function dropped() { }
            function retainedByRender() { renderDependency(); }
            function renderDependency() { }
            function retainedByState() { }
            function retainedByConstructor() { }
            function returnedMember() { }
            const sideEffect = sideEffectDependency;
            function sideEffectDependency() { }
            """);
        var statements = module.Body.OfType<Statement>().ToArray();
        var compilerStatements = statements
            .Select(statement => CreatePrivateRecord("CompilerStatement", statement, 0, 0))
            .ToArray();
        var stateSlot = CreatePrivateRecord(
            "StateSlot",
            counter,
            "counter",
            "counter",
            counter.Type,
            new Identifier("retainedByState"),
            true,
            null,
            null);
        var directRender = CreatePrivateRecord(
            "DirectRenderBuildResult",
            new CallExpression(
                new Identifier("retainedByRender"),
                NodeList.Empty<Expression>(),
                optional: false),
            "$renderDirect",
            CreateImmutableArray(typeof(Statement)),
            CreateImmutableArray(typeof(RenderModuleHoist)),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            CreateImmutableArray(typeof(ImportDeclaration)),
            CreateImmutableArray(typeof(ISymbol)));
        var initializationPhases = ImmutableArray.Create(
            new ComponentInitializationPhaseBuild(component.BaseType!, null),
            new ComponentInitializationPhaseBuild(
                component,
                new Parser().ParseModule("retainedByConstructor();").Body.Single()));

        var retained = Invoke<object>(
            "RemoveDirectRenderOnlyFunctions",
            CreateImmutableArray(compilerStatements[0].GetType(), compilerStatements),
            directRender,
            CreateImmutableArray(stateSlot.GetType(), stateSlot),
            initializationPhases,
            ImmutableArray.Create("returnedMember"));
        var output = ((System.Collections.IEnumerable)retained)
            .Cast<object>()
            .Select(item => (Statement)item.GetType().GetProperty("Statement")!.GetValue(item)!)
            .Select(static statement => statement.ToKnRECMAScript())
            .ToArray();

        Assert.IsFalse(
            output.Any(static statement => statement.Contains("function dropped", StringComparison.Ordinal)),
            string.Join(Environment.NewLine, output));
        foreach (var functionName in new[]
                 {
                     "retainedByRender",
                     "renderDependency",
                     "retainedByState",
                     "retainedByConstructor",
                     "returnedMember",
                     "sideEffectDependency"
                 })
        {
            Assert.IsTrue(
                output.Any(statement => statement.Contains("function " + functionName, StringComparison.Ordinal)),
                string.Join(Environment.NewLine, output));
        }
    }

    [TestMethod]
    public void SourceMapProjectionAndCompilationRootEdges_PreserveStableArtifacts()
    {
        var compilerMap = new SourceMapDocument(
            "compiler.mjs",
            [
                new SourceMapSource("Generated/Component.razor.g.cs", null),
                new SourceMapSource("External/Helper.cs", null)
            ],
            [
                new SourceMapSegment(4, 0, -1, 0, 0),
                new SourceMapSegment(4, 0, 0, 0, 0),
                new SourceMapSegment(5, 2, 0, 0, 0),
                new SourceMapSegment(5, 10, 1, 1, 2),
                new SourceMapSegment(5, 10, 1, 1, 2)
            ]);
        var projected = Invoke<SourceMapDocument>(
            "ProjectCompilerSourceMap",
            "components/output.mjs",
            compilerMap,
            CreateCompiledLineMappings(
                (GeneratedLine: 20, GeneratedColumn: 3, CompiledLine: 5, CompiledColumn: 0),
                (GeneratedLine: 20, GeneratedColumn: 9, CompiledLine: 5, CompiledColumn: 8)),
            "Generated/Component.razor.g.cs");
        Assert.HasCount(2, projected.Segments);
        Assert.AreEqual(20, projected.Segments[0].GeneratedLine);
        Assert.AreEqual(5, projected.Segments[0].GeneratedColumn);
        Assert.AreEqual(11, projected.Segments[1].GeneratedColumn);

        var sources = new List<SourceMapSource>();
        var indexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        Assert.AreEqual(0, Invoke<int>("GetOrAddSourceIndex", sources, indexes, "Pages/Empty.razor", null));
        Assert.AreEqual(0, Invoke<int>("GetOrAddSourceIndex", sources, indexes, "pages/empty.razor", null));

        var root = Path.Combine(Path.GetTempPath(), "JazorVue", "PrivateContractSourceRoot");
        var rootedTree = CSharpSyntaxTree.ParseText(
            "public sealed class RootedComponent { }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: Path.Combine(root, "Components", "RootedComponent.cs"));
        var rootedCompilation = CSharpCompilation.Create(
            "Rooted",
            [rootedTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var rootedDocument = new GeneratedDocument(
            "Generated/Rooted.razor.g.cs",
            Path.Combine(root, "Pages", "Rooted.razor"),
            SourceText.From("class Generated { }"),
            []);
        Assert.AreEqual(
            Path.GetFullPath(root),
            Invoke<string?>("TryGetCompilationSourceRoot", rootedCompilation, rootedDocument));

        var relativeDocument = new GeneratedDocument(
            "Generated/Relative.razor.g.cs",
            "Pages/Relative.razor",
            SourceText.From("class Generated { }"),
            []);
        Assert.IsNull(Invoke<string?>("TryGetCompilationSourceRoot", CreateCompilation(), relativeDocument));
    }

    [TestMethod]
    public void ImportRetentionAndModuleNamingEdges_PreserveCompilerArtifactBoundaries()
    {
        var compilation = CreateCompilation();
        var shapes = GetNamedType(compilation, "PrivateContracts.Shapes");
        var ordinary = GetMethod(shapes, "Ordinary");
        var explicitMethod = shapes.GetMembers().OfType<IMethodSymbol>()
            .Single(method => method.MethodKind == MethodKind.ExplicitInterfaceImplementation);
        var preferredExplicitName = Invoke<string>("GetPreferredModuleDeclaredName", explicitMethod);
        var explicitAlias = Invoke<string>(
            "ChooseModuleDeclaredName",
            explicitMethod,
            new HashSet<string>(StringComparer.Ordinal) { preferredExplicitName },
            new HashSet<string>(StringComparer.Ordinal));
        Assert.IsTrue(explicitAlias.StartsWith("m$", StringComparison.Ordinal));
        Assert.IsTrue(explicitAlias.EndsWith("$1", StringComparison.Ordinal));

        Assert.AreEqual("child.vue", Invoke<string>("ResolveImportArtifactPath", "child.vue", string.Empty));
        Assert.AreEqual(
            "pages/child.vue",
            Invoke<string>("ResolveImportArtifactPath", "./nested/../child.vue", "pages/host.mjs"));

        var imports = new Parser().ParseModule(
            """
            import renderReference from "./render.mjs";
            import preludeReference from "./prelude.mjs";
            import setupReference from "./setup.mjs";
            import stateReference from "./state.mjs";
            import unusedReference from "./unused.mjs";
            import "./side-effect.mjs";
            """)
            .Body
            .OfType<ImportDeclaration>()
            .ToArray();
        var preludeStatements = CreateImmutableArray(
            typeof(Statement),
            new Parser().ParseModule("preludeReference;").Body.Single());
        var directRender = CreatePrivateRecord(
            "DirectRenderBuildResult",
            new Identifier("renderReference"),
            "$renderDirect",
            preludeStatements,
            CreateImmutableArray(typeof(RenderModuleHoist)),
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            CreateImmutableArray(typeof(ImportDeclaration)),
            CreateImmutableArray(typeof(ISymbol)));
        var setupStatement = new Parser().ParseModule("setupReference;").Body.Single();
        var compilerStatement = CreatePrivateRecord("CompilerStatement", setupStatement, 0, 0);
        var stateSlot = CreatePrivateRecord(
            "StateSlot",
            ordinary,
            "stateReference",
            "stateReference",
            compilation.GetSpecialType(SpecialType.System_String),
            new Identifier("stateReference"),
            true,
            null,
            null);
        var compilerPartsConstructor = GetPrivateRecordConstructor("CompilerModuleParts", 6);
        var compilerParts = CreatePrivateRecord(
            "CompilerModuleParts",
            CreateImmutableArray(typeof(ImportDeclaration)),
            CreateImmutableArray(compilerStatement.GetType(), compilerStatement),
            CreateImmutableArray(compilerStatement.GetType(), compilerStatement),
            CreateImmutableArray(stateSlot.GetType(), stateSlot),
            CreateImmutableArray(typeof(ComponentInitializationPhaseBuild)),
            CreateEmptyReadOnlyDictionary(compilerPartsConstructor.GetParameters()[5].ParameterType));

        Assert.IsTrue(Invoke<bool>("IsCompilerImportReferenced", imports[0], directRender, compilerParts));
        Assert.IsTrue(Invoke<bool>("IsCompilerImportReferenced", imports[1], directRender, compilerParts));
        Assert.IsTrue(Invoke<bool>("IsCompilerImportReferenced", imports[2], directRender, compilerParts));
        Assert.IsTrue(Invoke<bool>("IsCompilerImportReferenced", imports[3], directRender, compilerParts));
        Assert.IsFalse(Invoke<bool>("IsCompilerImportReferenced", imports[4], directRender, compilerParts));
        Assert.IsTrue(Invoke<bool>("IsCompilerImportReferenced", imports[5], directRender, compilerParts));

        var uninitializedStateSlot = CreatePrivateRecord(
            "StateSlot",
            ordinary,
            "uninitializedStateReference",
            "uninitializedStateReference",
            compilation.GetSpecialType(SpecialType.System_String),
            null,
            false,
            null,
            null);
        var uninitializedStateParts = CreatePrivateRecord(
            "CompilerModuleParts",
            CreateImmutableArray(typeof(ImportDeclaration)),
            CreateImmutableArray(compilerStatement.GetType(), compilerStatement),
            CreateImmutableArray(compilerStatement.GetType(), compilerStatement),
            CreateImmutableArray(uninitializedStateSlot.GetType(), uninitializedStateSlot),
            CreateImmutableArray(typeof(ComponentInitializationPhaseBuild)),
            CreateEmptyReadOnlyDictionary(compilerPartsConstructor.GetParameters()[5].ParameterType));
        Assert.IsFalse(Invoke<bool>("IsCompilerImportReferenced", imports[3], directRender, uninitializedStateParts));

        var fixture = CreateRuntimeComponentFixture();
        var stateModule = new Parser().ParseModule("let counter = 1;");
        var declaration = (VariableDeclaration)stateModule.Body.Single();
        var incompletePositions = new Dictionary<Node, GeneratedNodePosition>
        {
            [declaration] = new GeneratedNodePosition(0, 0)
        };
        var missingInitializerPosition = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("BuildCompilerModuleParts", stateModule, incompletePositions, fixture.Closure));
        StringAssert.Contains(
            missingInitializerPosition.InnerException!.Message,
            "generated position",
            StringComparison.Ordinal);

    }

    [TestMethod]
    public void ReturnedMemberAndDisposeHelpers_PreserveNameFallbackAndExplicitInterfaceContracts()
    {
        var fixture = CreateRuntimeComponentFixture();
        var renderMethod = fixture.Component.BuildRenderTreeMethod;
        var returnedClosure = fixture.Closure with
        {
            LifecycleRoots = ImmutableArray.Create(renderMethod)
        };
        var returned = Invoke<ImmutableArray<string>>(
            "GetReturnedMembers",
            returnedClosure,
            new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
            {
                [renderMethod.OriginalDefinition] = "renderFromMap"
            });
        CollectionAssert.AreEqual(new[] { "renderFromMap" }, returned.ToArray());
        Assert.AreEqual("BuildRenderTree", Invoke<string>("GetRuntimeMemberName", renderMethod, null));

        var compilation = CreateStandaloneCompilation(
            """
            using System;
            using System.Threading.Tasks;

            public sealed class DisposalShape : IDisposable, IAsyncDisposable
            {
                public void Dispose() { }
                public ValueTask DisposeAsync() => ValueTask.CompletedTask;
                public void Cleanup() { }
            }

            public sealed class ExplicitDisposalShape : IDisposable, IAsyncDisposable
            {
                void IDisposable.Dispose() { }
                ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;
            }
            """,
            "DisposalShape.cs");
        var disposalShape = GetNamedType(compilation, "DisposalShape");
        var explicitShape = GetNamedType(compilation, "ExplicitDisposalShape");
        var dispose = GetMethod(disposalShape, "Dispose");
        var disposeAsync = GetMethod(disposalShape, "DisposeAsync");
        var cleanup = GetMethod(disposalShape, "Cleanup");
        var explicitDispose = explicitShape.GetMembers().OfType<IMethodSymbol>()
            .Single(static method => method.MethodKind == MethodKind.ExplicitInterfaceImplementation && method.Name == "System.IDisposable.Dispose");
        var explicitDisposeAsync = explicitShape.GetMembers().OfType<IMethodSymbol>()
            .Single(static method => method.MethodKind == MethodKind.ExplicitInterfaceImplementation && method.Name == "System.IAsyncDisposable.DisposeAsync");

        Assert.IsTrue(Invoke<bool>("IsSynchronousDisposeMethod", dispose));
        Assert.IsTrue(Invoke<bool>("IsSynchronousDisposeMethod", explicitDispose));
        Assert.IsFalse(Invoke<bool>("IsSynchronousDisposeMethod", cleanup));
        Assert.IsTrue(Invoke<bool>("IsAsynchronousDisposeMethod", disposeAsync));
        Assert.IsTrue(Invoke<bool>("IsAsynchronousDisposeMethod", explicitDisposeAsync));
        Assert.IsFalse(Invoke<bool>("IsAsynchronousDisposeMethod", cleanup));

        var lifecycleMembers = typeof(VueModuleBuilder).GetNestedType(
            "ComponentLifecycleRuntimeMembers",
            BindingFlags.NonPublic);
        Assert.IsNotNull(lifecycleMembers);
        var addDistinct = lifecycleMembers!
            .GetMethod("AddDistinct", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(addDistinct);
        var names = ImmutableArray.CreateBuilder<string>();
        addDistinct!.Invoke(null, [names, "dispose"]);
        addDistinct.Invoke(null, [names, "dispose"]);
        addDistinct.Invoke(null, [names, "disposeAsync"]);
        CollectionAssert.AreEqual(new[] { "dispose", "disposeAsync" }, names.ToArray());

        var runtimeDispose = fixture.Component.ComponentSymbol.GetMembers("Dispose")
            .OfType<IMethodSymbol>()
            .Single();
        var runtimeDisposeAsync = fixture.Component.ComponentSymbol.GetMembers("DisposeAsync")
            .OfType<IMethodSymbol>()
            .Single();
        var create = lifecycleMembers.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(create);
        var runtimeLifecycle = create!.Invoke(null, [fixture.Closure, null]);
        Assert.IsNotNull(runtimeLifecycle);
        CollectionAssert.AreEqual(
            new[] { "Dispose" },
            GetRecordItems(runtimeLifecycle!, "DisposeMemberNames").Cast<string>().ToArray());
        CollectionAssert.AreEqual(
            new[] { "DisposeAsync" },
            GetRecordItems(runtimeLifecycle!, "DisposeAsyncMemberNames").Cast<string>().ToArray());
        Assert.AreEqual("Dispose", Invoke<string>("GetRuntimeMemberName", runtimeDispose, null));
        Assert.AreEqual("DisposeAsync", Invoke<string>("GetRuntimeMemberName", runtimeDisposeAsync, null));

        var ignoredRootClosure = fixture.Closure with
        {
            LifecycleRoots = ImmutableArray.Create(fixture.Component.BuildRenderTreeMethod)
        };
        var ignoredRootLifecycle = create.Invoke(null, [ignoredRootClosure, null]);
        Assert.IsNotNull(ignoredRootLifecycle);
        Assert.IsEmpty(GetRecordItems(ignoredRootLifecycle!, "DisposeMemberNames"));
        Assert.IsEmpty(GetRecordItems(ignoredRootLifecycle!, "DisposeAsyncMemberNames"));
    }

    [TestMethod]
    public void ResidualBranchEdges_PreserveNameAndSourceMapFallbackContracts()
    {
        var compilation = CreateCompilation();
        var ordinary = GetMethod(GetNamedType(compilation, "PrivateContracts.Shapes"), "Ordinary");
        var preferredName = Invoke<string>("GetPreferredModuleDeclaredName", ordinary);
        var sourceName = Invoke<string?>("GetSourceDeclaredNameCandidate", ordinary);
        Assert.IsNotNull(sourceName);

        var alias = Invoke<string>(
            "ChooseModuleDeclaredName",
            ordinary,
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName! },
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName! });
        var localCollisionAlias = Invoke<string>(
            "ChooseModuleDeclaredName",
            ordinary,
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName! },
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName!, alias });
        var usedCollisionAlias = Invoke<string>(
            "ChooseModuleDeclaredName",
            ordinary,
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName!, alias },
            new HashSet<string>(StringComparer.Ordinal) { preferredName, sourceName! });
        Assert.IsTrue(localCollisionAlias.EndsWith("$1", StringComparison.Ordinal));
        Assert.IsTrue(usedCollisionAlias.EndsWith("$1", StringComparison.Ordinal));

        var pruned = Invoke<SourceMapDocument>(
            "PruneIntermediateSources",
            new SourceMapDocument(
                "component.mjs",
                [new SourceMapSource("Pages/Retained.razor", "<div />")],
                [
                    new SourceMapSegment(0, 0, -1, 0, 0),
                    new SourceMapSegment(1, 0, 0, 0, 0)
                ]),
            "component.mjs");
        Assert.HasCount(1, pruned.Sources);
        Assert.HasCount(1, pruned.Segments);

        var sources = new List<SourceMapSource>
        {
            new("Pages/Retained.razor", "first")
        };
        var sourceIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["pages/retained.razor"] = 0
        };
        Assert.AreEqual(
            0,
            Invoke<int>("GetOrAddSourceIndex", sources, sourceIndexes, "Pages/Retained.razor", "second"));
        Assert.AreEqual("first", sources[0].Content);

        var divergentCompilation = CreateStandaloneCompilation(
            "public sealed class DivergentComponent { }",
            @"C:\\RazorVue\\Divergent\\Component.cs");
        var divergentDocument = new GeneratedDocument(
            "Generated/Divergent.razor.g.cs",
            @"D:\\RazorVue\\Divergent\\Page.razor",
            SourceText.From("class Generated { }"),
            []);
        Assert.IsNull(Invoke<string?>("TryGetCompilationSourceRoot", divergentCompilation, divergentDocument));
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

    private static ConstructorInfo GetPrivateRecordConstructor(string typeName, int parameterCount)
    {
        var type = typeof(VueModuleBuilder).GetNestedType(typeName, BindingFlags.NonPublic);
        Assert.IsNotNull(type, typeName);
        return type!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(constructor => constructor.GetParameters().Length == parameterCount);
    }

    private static object CreatePrivateRecord(string typeName, params object?[] arguments)
        => GetPrivateRecordConstructor(typeName, arguments.Length).Invoke(arguments)!;

    private static object CreateImmutableArray(Type elementType, params object[] values)
    {
        var array = Array.CreateInstance(elementType, values.Length);
        for (var index = 0; index < values.Length; index++)
            array.SetValue(values[index], index);

        var createRange = typeof(ImmutableArray)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method =>
                method.Name == nameof(ImmutableArray.CreateRange) &&
                method.IsGenericMethodDefinition &&
                method.GetParameters() is [var parameter] &&
                parameter.ParameterType.IsGenericType &&
                parameter.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return createRange.MakeGenericMethod(elementType).Invoke(null, [array])!;
    }

    private static object CreateEmptyReadOnlyDictionary(Type dictionaryType)
    {
        Assert.IsTrue(dictionaryType.IsGenericType);
        var arguments = dictionaryType.GetGenericArguments();
        return Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(arguments))!;
    }

    private static object[] GetRecordItems(object value, string propertyName)
        => ((System.Collections.IEnumerable)value.GetType().GetProperty(propertyName)!.GetValue(value)!)
            .Cast<object>()
            .ToArray();

    private static object CreateCompiledLineMappings(
        params (int GeneratedLine, int GeneratedColumn, int CompiledLine, int CompiledColumn)[] mappings)
    {
        var mappingType = typeof(VueModuleBuilder).GetNestedType("CompiledLineMapping", BindingFlags.NonPublic);
        Assert.IsNotNull(mappingType);
        var constructor = mappingType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 5);
        var values = Array.CreateInstance(mappingType!, mappings.Length);
        for (var index = 0; index < mappings.Length; index++)
        {
            var mapping = mappings[index];
            values.SetValue(
                constructor.Invoke([mapping.GeneratedLine, mapping.GeneratedColumn, mapping.CompiledLine, mapping.CompiledColumn, false]),
                index);
        }

        var createRange = typeof(ImmutableArray)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method =>
                method.Name == nameof(ImmutableArray.CreateRange) &&
                method.IsGenericMethodDefinition &&
                method.GetParameters() is [var parameter] &&
                parameter.ParameterType.IsGenericType &&
                parameter.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return createRange.MakeGenericMethod(mappingType!).Invoke(null, [values])!;
    }

    private static string[] GetImportedNames(ImportDeclaration declaration)
        => declaration.Specifiers
            .OfType<ImportSpecifier>()
            .Select(static specifier => ((Identifier)specifier.Imported).Name)
            .ToArray();

    private static CSharpCompilation CreateCompilation()
    {
        var source = CSharpSyntaxTree.ParseText(
            """
            using System;
            using ECMAScript;
            using Microsoft.AspNetCore.Components;

            namespace PrivateContracts;

            public interface IExplicit
            {
                void Execute();
            }

            public sealed class Shapes : IExplicit
            {
                private static readonly int Seed = 1;
                public int Field;
                public int Auto { get; init; }
                public static void Ordinary() { }
                void IExplicit.Execute() { }
                ~Shapes() { }
                public sealed class Nested { }
            }

            public static class LocalShapeHost
            {
                public static void LocalShapes(object? value, int[] values)
                {
                    var ordinary = 0;
                    object converted = (object)ordinary;
                    if (value is string text) { }
                    if (value is Shapes { Field: var field }) { }
                    if (values is [var first, .. var rest]) { }
                    foreach (var item in values) { }
                }
            }

            [ECMAScriptModule("./components/marked")]
            public sealed class Marked { }

            [ECMAScriptModule("")]
            public sealed class Blank { }

            [ECMAScriptModule(null)]
            public sealed class NullMarked { }

            [ECMAScriptModule]
            public sealed class NoArgumentMarked { }

            [Obsolete, ECMAScriptModule("./components/noisy")]
            public sealed class NoisyMarked { }

            public sealed class Plain { }
            public sealed record RecordShape;
            public readonly struct ValueShape;

            public sealed class ComponentContainer
            {
                public sealed class RuntimeOuter
                {
                    public sealed class RuntimeInner { }
                }

                public sealed record RecordOuter
                {
                    public sealed class RecordInner { }
                }
            }

            public sealed class ForeignContainer
            {
                public sealed class RuntimeOuter
                {
                    public sealed class RuntimeInner { }
                }
            }

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

    private static CSharpCompilation CreateStandaloneCompilation(string source, string path)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: path);
        var compilation = CSharpCompilation.Create(
            "Standalone",
            [syntaxTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return compilation;
    }

    private static RuntimeComponentFixture CreateStaticRuntimeComponentFixture()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using static ECMAScript.Vue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace PrivateContracts;

            [ECMAScriptModule("./components/static-runtime")]
            public sealed class StaticRuntimeComponent : ComponentBase, IVueComponent
            {
                private static int staticField;

                public static int StaticAuto { get; set; }

                public static int StaticWriteOnly
                {
                    set => staticField = value;
                }

                public static int StaticComputed
                {
                    get => staticField;
                    set => staticField = value;
                }

                public static void Touch()
                {
                    staticField++;
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    Touch();
                    StaticAuto = StaticComputed;
                    StaticWriteOnly = StaticAuto;
                    builder.AddContent(0, StaticAuto + StaticComputed + staticField);
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "StaticRuntimeComponent.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.StaticRuntimeComponent.PrivateContracts",
            [syntaxTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbol = compilation.GetTypeByMetadataName("PrivateContracts.StaticRuntimeComponent");
        Assert.IsNotNull(componentSymbol);
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                ImmutableArray.Create(componentSymbol!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var component = binding!.Components.Single();
        Assert.IsTrue(
            MemberClosureBuilder.TryBuild(binding, component, out var closure, out var closureFailure),
            closureFailure);
        Assert.IsNotNull(closure);
        return new RuntimeComponentFixture(component, closure!);
    }

    private static RuntimeComponentFixture CreateRuntimeComponentFixture()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace PrivateContracts;

            public sealed class RuntimeComponent : ComponentBase, IDisposable, IAsyncDisposable
            {
                private int counter = 1;
                private int secondary = 2;

                public sealed class RuntimeOuter
                {
                    public sealed class RuntimeInner
                    {
                        public sealed class RuntimeLeaf
                        {
                        }
                    }
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    var outer = new RuntimeOuter();
                    var instance = new RuntimeOuter.RuntimeInner();
                    var leaf = new RuntimeOuter.RuntimeInner.RuntimeLeaf();
                    builder.AddContent(0, counter + secondary + outer.GetHashCode() + instance.GetHashCode() + leaf.GetHashCode());
                }

                public void Dispose()
                {
                }

                public ValueTask DisposeAsync() => ValueTask.CompletedTask;
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RuntimeComponent.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.RuntimeComponent.PrivateContracts",
            [syntaxTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbol = compilation.GetTypeByMetadataName("PrivateContracts.RuntimeComponent");
        Assert.IsNotNull(componentSymbol);
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                ImmutableArray.Create(componentSymbol!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var component = binding!.Components.Single();
        Assert.IsTrue(
            MemberClosureBuilder.TryBuild(binding, component, out var closure, out var closureFailure),
            closureFailure);
        Assert.IsNotNull(closure);
        return new RuntimeComponentFixture(component, closure!);
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

    private static IBlockOperation GetMethodBody(Compilation compilation, string methodName)
    {
        var syntaxTree = compilation.SyntaxTrees.Single();
        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(candidate => candidate.Identifier.ValueText == methodName);
        var operation = compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!) as IBlockOperation;
        Assert.IsNotNull(operation, methodName);
        return operation!;
    }

    private sealed record RuntimeComponentFixture(
        BoundComponent Component,
        MemberClosure Closure);
}
