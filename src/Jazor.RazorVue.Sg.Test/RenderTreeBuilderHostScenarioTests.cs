using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RenderTreeBuilderHostScenarioTests
{
    public static IEnumerable<TestDataRow<RenderTreeBuilderHostSuccessScenario>> SuccessCases
        => RenderTreeBuilderHostScenarioCatalog.Successes.Select(static scenario =>
            new TestDataRow<RenderTreeBuilderHostSuccessScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    public static IEnumerable<TestDataRow<RenderTreeBuilderHostFailureScenario>> FailureCases
        => RenderTreeBuilderHostScenarioCatalog.Failures.Select(static scenario =>
            new TestDataRow<RenderTreeBuilderHostFailureScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsKindsAndInputs()
    {
        var allIds = RenderTreeBuilderHostScenarioCatalog.Successes
            .Select(static scenario => scenario.Id)
            .Concat(RenderTreeBuilderHostScenarioCatalog.Failures.Select(static scenario => scenario.Id))
            .ToArray();
        var allInputs = RenderTreeBuilderHostScenarioCatalog.Successes
            .Select(static scenario => scenario.InputIdentity)
            .Concat(RenderTreeBuilderHostScenarioCatalog.Failures.Select(static scenario => scenario.InputIdentity))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.HasCount(allInputs.Length, allInputs.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("render-tree-host.", StringComparison.Ordinal)));
        Assert.IsTrue(RenderTreeBuilderHostScenarioCatalog.Successes.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension) &&
            (scenario.ExpectedBodyFragments.Count > 0 || scenario.ExpectedImports.Count > 0)));
        Assert.IsTrue(RenderTreeBuilderHostScenarioCatalog.Failures.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension) &&
            scenario.ExpectedMessageFragments.Count > 0));
        Assert.HasCount(
            Enum.GetValues<RenderTreeBuilderHostSuccessKind>().Length,
            RenderTreeBuilderHostScenarioCatalog.Successes.Select(static scenario => scenario.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<RenderTreeBuilderHostFailureKind>().Length,
            RenderTreeBuilderHostScenarioCatalog.Failures.Select(static scenario => scenario.Kind).Distinct());
    }

    [TestMethod]
    [DynamicData(nameof(SuccessCases))]
    public void Rewrite_MatchesRenderContextProtocol(RenderTreeBuilderHostSuccessScenario scenario)
    {
        var fixture = Compile(scenario.Source, scenario.Id);
        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };
        var argument = new SenseArgument(UseImportAliases: true);

        var body = walker.Visit(fixture.Body, argument)
            ?.ToKnRECMAScript()
            ?.ReplaceLineEndings("\n");
        var imports = argument.FlushImportSpecifiers();

        Assert.IsNotNull(body, scenario.Id);
        foreach (var expected in scenario.ExpectedBodyFragments)
            StringAssert.Contains(body, expected, StringComparison.Ordinal, scenario.Id);
        foreach (var forbidden in scenario.ForbiddenBodyFragments)
            Assert.IsFalse(body.Contains(forbidden, StringComparison.Ordinal), $"{scenario.Id}: {body}");
        AssertOrderedFragments(body, scenario.OrderedBodyFragments, scenario.Id);
        AssertImports(imports, scenario.ExpectedImports, scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(FailureCases))]
    public void Rewrite_RejectsUnresolvableComponentProtocol(RenderTreeBuilderHostFailureScenario scenario)
    {
        var fixture = Compile(scenario.Source, scenario.Id);
        var walker = new SemanticWalker(true)
        {
            Host = new RenderTreeBuilderSemanticWalkerHost()
        };

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            walker.Visit(fixture.Body, new SenseArgument(UseImportAliases: true)));

        foreach (var expected in scenario.ExpectedMessageFragments)
            StringAssert.Contains(exception.Message, expected, StringComparison.Ordinal, scenario.Id);
        Assert.AreEqual(
            "RenderTreeBuilderHostScenario.g.cs",
            Path.GetFileName(exception.Data["location.path"] as string),
            scenario.Id);
        Assert.IsGreaterThan(0, ReadLocationInt(exception, "location.startLine", scenario.Id), scenario.Id);
        Assert.IsGreaterThan(0, ReadLocationInt(exception, "location.startColumn", scenario.Id), scenario.Id);
    }

    private static void AssertImports(
        IReadOnlyList<KeyValuePair<string, NodeList<ImportDeclarationSpecifier>>> actual,
        IReadOnlyList<RenderTreeBuilderHostImportExpectation> expected,
        string scenarioId)
    {
        Assert.HasCount(expected.Count, actual, scenarioId);
        for (var groupIndex = 0; groupIndex < expected.Count; groupIndex++)
        {
            var expectedGroup = expected[groupIndex];
            var actualGroup = actual[groupIndex];
            Assert.AreEqual(expectedGroup.ModulePath, actualGroup.Key, scenarioId);
            Assert.HasCount(1, actualGroup.Value, scenarioId);

            var specifier = actualGroup.Value[0];
            switch (expectedGroup.Kind)
            {
                case RenderTreeBuilderHostImportKind.Default:
                    Assert.IsInstanceOfType<ImportDefaultSpecifier>(specifier, scenarioId);
                    break;
                case RenderTreeBuilderHostImportKind.Named:
                    Assert.IsInstanceOfType<ImportSpecifier>(specifier, scenarioId);
                    var named = (ImportSpecifier)specifier;
                    Assert.IsInstanceOfType<Identifier>(named.Imported, scenarioId);
                    Assert.AreEqual(expectedGroup.ImportedName, ((Identifier)named.Imported).Name, scenarioId);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"{scenarioId}: unsupported import kind '{expectedGroup.Kind}'.");
            }
        }
    }

    private static void AssertOrderedFragments(
        string body,
        IReadOnlyList<string> orderedFragments,
        string scenarioId)
    {
        var previousIndex = -1;
        foreach (var fragment in orderedFragments)
        {
            var index = body.IndexOf(fragment, previousIndex + 1, StringComparison.Ordinal);
            Assert.IsGreaterThan(previousIndex, index, $"{scenarioId}: expected '{fragment}' in order.\n{body}");
            previousIndex = index;
        }
    }

    private static int ReadLocationInt(Exception exception, string key, string scenarioId)
    {
        var value = exception.Data[key];
        Assert.IsInstanceOfType<int>(value, $"{scenarioId}: metadata '{key}'.");
        return (int)value;
    }

    private static RenderTreeBuilderHostFixture Compile(string source, string scenarioId)
    {
        const string usings = """
            global using System;
            global using ECMAScript;
            global using static ECMAScript.Global;
            global using Microsoft.AspNetCore.Components;
            global using Microsoft.AspNetCore.Components.Rendering;
            global using Microsoft.AspNetCore.Components.Web;
            global using ECMAScript.VueContract;
            """;
        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.VueContract.VueLibraryEmitAttribute).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(ComponentBase).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(MouseEventArgs).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(RenderTreeBuilder).Assembly.Location));
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "RenderTreeBuilderHostScenario.g.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "RenderTreeBuilderHostScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    usings,
                    TestMetadataReferences.PreviewParseOptions,
                    path: "GlobalUsings.g.cs"),
                sourceTree
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{scenarioId}:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "Render");
        var body = compilation.GetSemanticModel(sourceTree).GetOperation(method.Body!) as IBlockOperation
            ?? throw new InvalidOperationException($"{scenarioId}: Render body operation was not available.");
        return new RenderTreeBuilderHostFixture(body);
    }

    private sealed record RenderTreeBuilderHostFixture(IBlockOperation Body);
}

public enum RenderTreeBuilderHostSuccessKind
{
    CombinedDescriptorMap,
    ExistingListenerDescriptor,
    VueLibraryImportTrimming,
    ModulePathNormalization,
    ConvertedLocalComponentType,
    ReusedLocalComponentType,
    NestedLocalComponentType,
    RenderFragmentSequenceEvaluation,
    RenderFragmentReceiverEvaluation,
    InheritedDescriptorMap
}

public enum RenderTreeBuilderHostFailureKind
{
    BlankModulePath,
    BlankLibraryImportSpecifier,
    BlankLibraryExportName,
    ConditionalTypeExpression
}

public enum RenderTreeBuilderHostImportKind
{
    Default,
    Named
}

public sealed record RenderTreeBuilderHostImportExpectation(
    string ModulePath,
    string ImportedName,
    RenderTreeBuilderHostImportKind Kind);

public sealed record RenderTreeBuilderHostSuccessScenario(
    string Id,
    string Dimension,
    RenderTreeBuilderHostSuccessKind Kind,
    string Source,
    IReadOnlyList<string> ExpectedBodyFragments,
    IReadOnlyList<string> ForbiddenBodyFragments,
    IReadOnlyList<string> OrderedBodyFragments,
    IReadOnlyList<RenderTreeBuilderHostImportExpectation> ExpectedImports)
{
    public string InputIdentity => $"{Kind}|{Source}";
}

public sealed record RenderTreeBuilderHostFailureScenario(
    string Id,
    string Dimension,
    RenderTreeBuilderHostFailureKind Kind,
    string Source,
    IReadOnlyList<string> ExpectedMessageFragments)
{
    public string InputIdentity => $"{Kind}|{Source}";
}

internal static class RenderTreeBuilderHostScenarioCatalog
{
    public static IReadOnlyList<RenderTreeBuilderHostSuccessScenario> Successes { get; } =
    [
        Success(
            "inherited-descriptor-map",
            "derived-component-inherits-prop-emit-and-slot-runtime-name-map",
            RenderTreeBuilderHostSuccessKind.InheritedDescriptorMap,
            """
            [VueLibraryEmit(nameof(ValueChanged), Name = "value-change")]
            abstract class EditorBase : ComponentBase
            {
                [ECMAScriptName("model-value")]
                [Parameter] public string Value { get; set; } = "";
                [Parameter] public EventCallback<string> ValueChanged { get; set; }
                [Parameter] public RenderFragment? ChildContent { get; set; }
            }

            [ECMAScriptModule("./components/inherited-editor")]
            sealed class Editor : EditorBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<Editor>(0);
                    builder.CloseComponent();
                }
            }
            """,
            [
                "\"ChildContent\": \"default\"",
                "\"Value\": \"model-value\"",
                "\"ValueChanged\": \"onValueChange\""
            ],
            ["onValue-change", "onOnValueChange"],
            ["\"ChildContent\"", "\"Value\"", "\"ValueChanged\""],
            [DefaultImport("./components/inherited-editor.mjs")]),
        Success(
            "combined-descriptor-map",
            "prop-emit-default-slot-runtime-name-map",
            RenderTreeBuilderHostSuccessKind.CombinedDescriptorMap,
            """
            [ECMAScriptModule("./components/editor")]
            [VueLibraryEmit(nameof(ValueChanged), Name = "update:modelValue")]
            sealed class Editor : ComponentBase
            {
                [ECMAScriptName("model-value")]
                [Parameter] public string Value { get; set; } = "";
                [Parameter] public EventCallback<string> ValueChanged { get; set; }
                [Parameter] public RenderFragment? ChildContent { get; set; }
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<Editor>(0);
                    builder.CloseComponent();
                }
            }
            """,
            [
                "\"ChildContent\": \"default\"",
                "\"Value\": \"model-value\"",
                "\"ValueChanged\": \"onUpdate:modelValue\""
            ],
            ["onOnUpdate:modelValue"],
            ["\"ChildContent\"", "\"Value\"", "\"ValueChanged\""],
            [DefaultImport("./components/editor.mjs")]),
        Success(
            "existing-listener-descriptor",
            "emit-runtime-name-preserves-vue-listener-form",
            RenderTreeBuilderHostSuccessKind.ExistingListenerDescriptor,
            """
            [ECMAScriptModule("./components/button")]
            [VueLibraryEmit(nameof(Clicked), Name = "onClick")]
            sealed class Button : ComponentBase
            {
                [Parameter] public EventCallback Clicked { get; set; }
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<Button>(0);
                    builder.CloseComponent();
                }
            }
            """,
            ["\"Clicked\": \"onClick\""],
            ["onOnClick"],
            [],
            [DefaultImport("./components/button.mjs")]),
        Success(
            "vue-library-import-trimming",
            "library-component-import-and-export-normalization",
            RenderTreeBuilderHostSuccessKind.VueLibraryImportTrimming,
            """
            [VueLibraryComponent(" tdesign-vue-next ", " Button ")]
            sealed class TButton : ComponentBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<TButton>(0);
                    builder.CloseComponent();
                }
            }
            """,
            ["builder.openComponent(Button);", "builder.closeComponent();"],
            [" Button ", "tdesign-vue-next "],
            [],
            [NamedImport("tdesign-vue-next", "Button")]),
        Success(
            "module-path-normalization",
            "windows-separator-local-component-path-normalization",
            RenderTreeBuilderHostSuccessKind.ModulePathNormalization,
            """
            [ECMAScriptModule(@"components\child")]
            sealed class Child : ComponentBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<Child>(0);
                    builder.CloseComponent();
                }
            }
            """,
            ["builder.openComponent("],
            ["components\\child"],
            [],
            [DefaultImport("./components/child.mjs")]),
        Success(
            "converted-local-component-type",
            "explicit-type-conversion-preserves-static-component-resolution",
            RenderTreeBuilderHostSuccessKind.ConvertedLocalComponentType,
            """
            [ECMAScriptModule("./components/child")]
            sealed class Child : ComponentBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    Type childType = (Type)typeof(Child);
                    builder.OpenComponent(0, (Type)childType);
                    builder.CloseComponent();
                }
            }
            """,
            ["builder.openComponent("],
            ["childType", "typeof"],
            [],
            [DefaultImport("./components/child.mjs")]),
        Success(
            "reused-local-component-type",
            "one-static-type-local-shared-by-multiple-component-opens",
            RenderTreeBuilderHostSuccessKind.ReusedLocalComponentType,
            """
            [ECMAScriptModule("./components/child")]
            sealed class Child : ComponentBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    Type childType = typeof(Child);
                    builder.OpenComponent(0, childType);
                    builder.CloseComponent();
                    builder.OpenComponent(1, childType);
                    builder.CloseComponent();
                }
            }
            """,
            ["builder.openComponent("],
            ["childType", "typeof"],
            ["builder.openComponent(", "builder.closeComponent();"],
            [DefaultImport("./components/child.mjs")]),
        Success(
            "nested-local-component-type",
            "nested-scope-static-type-local-resolution",
            RenderTreeBuilderHostSuccessKind.NestedLocalComponentType,
            """
            [ECMAScriptModule("./components/child")]
            sealed class Child : ComponentBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder, bool visible)
                {
                    if (visible)
                    {
                        Type childType = typeof(Child);
                        builder.OpenComponent(0, childType);
                        builder.CloseComponent();
                    }
                }
            }
            """,
            ["if (visible)", "builder.openComponent("],
            ["childType", "typeof"],
            [],
            [DefaultImport("./components/child.mjs")]),
        Success(
            "render-fragment-sequence-evaluation",
            "non-generic-fragment-preserves-erased-sequence-side-effect",
            RenderTreeBuilderHostSuccessKind.RenderFragmentSequenceEvaluation,
            """
            sealed class TestClass
            {
                int NextSequence() => 0;

                void Render(RenderTreeBuilder builder)
                {
                    RenderFragment content = child => child.AddContent(0, "body");
                    builder.AddContent(NextSequence(), content);
                }
            }
            """,
            [
                "((__rtb, __arg0, __arg1) => __arg1?.(__rtb))(builder, this.nextSequence(), content);"
            ],
            ["builder.addContent(content)"],
            ["this.nextSequence()", "content"],
            []),
        Success(
            "render-fragment-receiver-evaluation",
            "complex-builder-and-fragment-evaluated-once-in-source-order",
            RenderTreeBuilderHostSuccessKind.RenderFragmentReceiverEvaluation,
            """
            sealed class TestClass
            {
                RenderTreeBuilder NextBuilder() => new();
                int NextSequence() => 0;
                RenderFragment NextContent() => child => child.AddContent(0, "body");

                void Render()
                {
                    NextBuilder().AddContent(NextSequence(), NextContent());
                }
            }
            """,
            [
                "((__rtb, __arg0, __arg1) => __arg1?.(__rtb))(this.nextBuilder(), this.nextSequence(), this.nextContent());"
            ],
            ["this.nextBuilder().addContent"],
            ["this.nextBuilder()", "this.nextSequence()", "this.nextContent()"],
            [])
    ];

    public static IReadOnlyList<RenderTreeBuilderHostFailureScenario> Failures { get; } =
    [
        Failure(
            "blank-module-path",
            "local-component-module-path-must-resolve",
            RenderTreeBuilderHostFailureKind.BlankModulePath,
            """
            [ECMAScriptModule(" ")]
            sealed class Child : ComponentBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<Child>(0);
                }
            }
            """,
            ["OpenComponent", "Child", "ECMAScriptModule"]),
        Failure(
            "blank-library-import-specifier",
            "vue-library-component-requires-import-specifier",
            RenderTreeBuilderHostFailureKind.BlankLibraryImportSpecifier,
            """
            [VueLibraryComponent(" ", "Button")]
            sealed class TButton : ComponentBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<TButton>(0);
                }
            }
            """,
            ["OpenComponent", "TButton", "VueLibraryComponent"]),
        Failure(
            "blank-library-export-name",
            "vue-library-component-requires-export-name",
            RenderTreeBuilderHostFailureKind.BlankLibraryExportName,
            """
            [VueLibraryComponent("tdesign-vue-next", " ")]
            sealed class TButton : ComponentBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<TButton>(0);
                }
            }
            """,
            ["OpenComponent", "TButton", "VueLibraryComponent"]),
        Failure(
            "conditional-type-expression",
            "runtime-component-type-selection-is-not-statically-resolvable",
            RenderTreeBuilderHostFailureKind.ConditionalTypeExpression,
            """
            [ECMAScriptModule("./components/child")]
            sealed class Child : ComponentBase
            {
            }

            [ECMAScriptModule("./components/alternate")]
            sealed class Alternate : ComponentBase
            {
            }

            sealed class TestClass
            {
                void Render(RenderTreeBuilder builder, bool alternate)
                {
                    builder.OpenComponent(0, alternate ? typeof(Alternate) : typeof(Child));
                }
            }
            """,
            ["Dynamic Type OpenComponent", "not supported"])
    ];

    private static RenderTreeBuilderHostSuccessScenario Success(
        string id,
        string dimension,
        RenderTreeBuilderHostSuccessKind kind,
        string source,
        IReadOnlyList<string> expectedBodyFragments,
        IReadOnlyList<string> forbiddenBodyFragments,
        IReadOnlyList<string> orderedBodyFragments,
        IReadOnlyList<RenderTreeBuilderHostImportExpectation> expectedImports)
        => new(
            $"render-tree-host.success.{id}",
            dimension,
            kind,
            source,
            expectedBodyFragments,
            forbiddenBodyFragments,
            orderedBodyFragments,
            expectedImports);

    private static RenderTreeBuilderHostFailureScenario Failure(
        string id,
        string dimension,
        RenderTreeBuilderHostFailureKind kind,
        string source,
        IReadOnlyList<string> expectedMessageFragments)
        => new(
            $"render-tree-host.failure.{id}",
            dimension,
            kind,
            source,
            expectedMessageFragments);

    private static RenderTreeBuilderHostImportExpectation DefaultImport(string modulePath)
        => new(modulePath, "default", RenderTreeBuilderHostImportKind.Default);

    private static RenderTreeBuilderHostImportExpectation NamedImport(string modulePath, string importedName)
        => new(modulePath, importedName, RenderTreeBuilderHostImportKind.Named);
}
