using System.Collections.Immutable;
using ECMAScript;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgDirectRenderFailureMatrixTests
{
    public static IEnumerable<TestDataRow<DirectRenderFailureCase>> Cases
        => DirectRenderFailureCaseCatalog.All.Select(static testCase =>
            new TestDataRow<DirectRenderFailureCase>(testCase)
            {
                DisplayName = "DirectRenderFailure_" + testCase.Id
            });

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void TryEmit_RejectsUnsupportedShapeWithActionableDiagnostic(DirectRenderFailureCase testCase)
    {
        var failure = RazorSgDirectRenderFailureMatrixTestHost.EmitFailure(testCase);

        StringAssert.Contains(failure, testCase.ExpectedFailureFragment, StringComparison.Ordinal);
        Assert.IsFalse(failure.Contains(".vue", StringComparison.Ordinal));
    }
}

public sealed record DirectRenderFailureCase(
    string Id,
    string TypeName,
    string Body,
    string Members,
    string ExpectedFailureFragment,
    RazorVueUsageScenarioId? Scenario);

internal static partial class DirectRenderFailureCaseCatalog
{
    public static IReadOnlyList<DirectRenderFailureCase> All { get; } = CreateCases();

    private static IReadOnlyList<DirectRenderFailureCase> CreateCases()
    {
        var cases = new List<DirectRenderFailureCase>(576);
        for (var shape = 0; shape < 16; shape++)
        {
            for (var variant = 0; variant < 4; variant++)
            {
                var suffix = shape.ToString("D2", System.Globalization.CultureInfo.InvariantCulture) + "_" +
                             variant.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
                var marker = "failure-" + suffix;
                var (id, body, members, expectedFailure) = CreateCase(shape, variant, marker);
                cases.Add(new DirectRenderFailureCase(
                    id + "_" + variant.ToString("D2", System.Globalization.CultureInfo.InvariantCulture),
                    "DirectRenderFailure" + cases.Count.ToString("D3", System.Globalization.CultureInfo.InvariantCulture),
                    body,
                    members,
                    expectedFailure,
                    Scenario: null));
            }
        }

        AddCoverageDiagnosticCases(cases);
        AddBoundaryDiagnosticCases(cases);

        return cases;
    }

    private static (string Id, string Body, string Members, string ExpectedFailure) CreateCase(
        int shape,
        int variant,
        string marker)
        => shape switch
        {
            0 => (
                "dynamic_element_tag",
                variant switch
                {
                    0 => "builder.OpenElement(0, TagName); builder.CloseElement();",
                    1 => "builder.OpenElement(0, ComputedTag); builder.CloseElement();",
                    2 => "builder.OpenElement(0, GetTag()); builder.CloseElement();",
                    _ => "builder.OpenElement(0, UseSection ? \"section\" : \"div\"); builder.CloseElement();"
                },
                variant switch
                {
                    0 => "[Parameter] public string TagName { get; set; } = " + Literal(marker) + ";",
                    1 => "private string ComputedTag => " + Literal(marker) + ";",
                    2 => "private string GetTag() => " + Literal(marker) + ";",
                    _ => "[Parameter] public bool UseSection { get; set; }"
                },
                "OpenElement tag names must be compile-time strings"),
            1 => (
                "dynamic_attribute_name",
                variant switch
                {
                    0 => "builder.OpenElement(0, \"div\"); builder.AddAttribute(1, AttributeName, " + Literal(marker) + "); builder.CloseElement();",
                    1 => "builder.OpenElement(0, \"div\"); builder.AddAttribute(1, ComputedName, " + Literal(marker) + "); builder.CloseElement();",
                    2 => "builder.OpenElement(0, \"div\"); builder.AddAttribute(1, GetName(), " + Literal(marker) + "); builder.CloseElement();",
                    _ => "builder.OpenElement(0, \"div\"); builder.AddAttribute(1, UseTitle ? \"title\" : \"class\", " + Literal(marker) + "); builder.CloseElement();"
                },
                variant switch
                {
                    0 => "[Parameter] public string AttributeName { get; set; } = \"title\";",
                    1 => "private string ComputedName => \"data-computed\";",
                    2 => "private static string GetName() => \"data-method\";",
                    _ => "[Parameter] public bool UseTitle { get; set; }"
                },
                "Attribute names must be compile-time strings"),
            2 => (
                "attribute_after_child",
                variant switch
                {
                    0 => "builder.OpenElement(0, \"div\"); builder.AddContent(1, " + Literal(marker) + "); builder.AddAttribute(2, \"title\", " + Literal(marker) + "); builder.CloseElement();",
                    1 => "builder.OpenElement(0, \"div\"); builder.AddMarkupContent(1, " + Literal("<b>" + marker + "</b>") + "); builder.AddAttribute(2, \"title\", " + Literal(marker) + "); builder.CloseElement();",
                    2 => "builder.OpenElement(0, \"div\"); builder.OpenElement(1, \"span\"); builder.CloseElement(); builder.AddAttribute(2, \"title\", " + Literal(marker) + "); builder.CloseElement();",
                    _ => "builder.OpenElement(0, \"div\"); builder.AddContent(1, ChildContent); builder.AddAttribute(2, \"title\", " + Literal(marker) + "); builder.CloseElement();"
                },
                variant == 3 ? "[Parameter] public RenderFragment? ChildContent { get; set; }" : "",
                "Attributes must be added before children"),
            3 => (
                "unclosed_element",
                variant switch
                {
                    0 => "builder.OpenElement(0, \"section\"); builder.AddContent(1, " + Literal(marker) + ");",
                    1 => "builder.OpenComponent<FailureMatrixChild>(0); builder.AddComponentParameter(1, \"ChildContent\", (RenderFragment)(child => child.AddContent(0, " + Literal(marker) + ")));",
                    2 => "builder.OpenRegion(0); builder.AddContent(1, " + Literal(marker) + ");",
                    _ => "builder.OpenElement(0, \"main\"); builder.OpenElement(1, \"span\"); builder.AddContent(2, " + Literal(marker) + "); builder.CloseElement();"
                },
                "",
                "unclosed RenderTreeBuilder frames"),
            4 => (
                "close_without_open",
                variant switch
                {
                    0 => "builder.CloseElement();",
                    1 => "builder.CloseComponent();",
                    2 => "builder.CloseRegion();",
                    _ => "builder.AddContent(0, " + Literal(marker) + "); builder.CloseElement();"
                },
                "",
                "frame close order is invalid"),
            5 => (
                "mismatched_close",
                variant switch
                {
                    0 => "builder.OpenElement(0, \"article\"); builder.CloseComponent();",
                    1 => "builder.OpenComponent<FailureMatrixChild>(0); builder.CloseElement();",
                    2 => "builder.OpenRegion(0); builder.CloseElement();",
                    _ => "builder.OpenElement(0, \"article\"); builder.AddContent(1, " + Literal(marker) + "); builder.CloseRegion();"
                },
                "",
                "frame close order is invalid"),
            6 => (
                "key_without_frame",
                variant switch
                {
                    0 => "builder.SetKey(" + Literal(marker) + ");",
                    1 => "builder.AddContent(0, " + Literal(marker) + "); builder.SetKey(" + Literal(marker) + ");",
                    2 => "builder.OpenElement(0, \"div\"); builder.CloseElement(); builder.SetKey(" + Literal(marker) + ");",
                    _ => "builder.OpenComponent<FailureMatrixChild>(0); builder.CloseComponent(); builder.SetKey(" + Literal(marker) + ");"
                },
                "",
                "SetKey must target an open element or component before children"),
            7 => (
                "key_after_child",
                variant switch
                {
                    0 => "builder.OpenElement(0, \"main\"); builder.AddContent(1, " + Literal(marker) + "); builder.SetKey(" + Literal(marker) + "); builder.CloseElement();",
                    1 => "builder.OpenElement(0, \"main\"); builder.AddMarkupContent(1, " + Literal("<i>" + marker + "</i>") + "); builder.SetKey(" + Literal(marker) + "); builder.CloseElement();",
                    2 => "builder.OpenElement(0, \"main\"); builder.OpenElement(1, \"span\"); builder.CloseElement(); builder.SetKey(" + Literal(marker) + "); builder.CloseElement();",
                    _ => "builder.OpenElement(0, \"main\"); builder.AddContent(1, ChildContent); builder.SetKey(" + Literal(marker) + "); builder.CloseElement();"
                },
                variant == 3 ? "[Parameter] public RenderFragment? ChildContent { get; set; }" : "",
                "SetKey must target an open element or component before children"),
            8 => (
                "local_inside_frame",
                variant switch
                {
                    0 => "builder.OpenElement(0, \"aside\"); var local0 = ReadValue(); builder.AddContent(1, local0); builder.CloseElement();",
                    1 => "builder.OpenComponent<FailureMatrixChild>(0); var local1 = ReadValue(); builder.AddComponentParameter(1, \"ChildContent\", local1); builder.CloseComponent();",
                    2 => "builder.OpenRegion(0); var local2 = ReadValue(); builder.AddContent(1, local2); builder.CloseRegion();",
                    _ => "builder.OpenElement(0, \"aside\"); builder.OpenElement(1, \"span\"); var local3 = ReadValue(); builder.AddContent(2, local3); builder.CloseElement(); builder.CloseElement();"
                },
                "private string ReadValue() => " + Literal(marker) + ";",
                "Runtime local declarations in direct render lowering are only supported outside open RenderTreeBuilder frames"),
            9 => (
                "for_loop",
                variant switch
                {
                    0 => "for (var index = 0; index < 1; index++) { builder.AddContent(index, " + Literal(marker) + "); }",
                    1 => "builder.OpenElement(0, \"div\"); for (var index = 0; index < 2; index++) { builder.AddContent(index + 1, " + Literal(marker) + "); } builder.CloseElement();",
                    2 => "for (var index = 3; index > 0; index--) { builder.OpenElement(index, \"span\"); builder.CloseElement(); }",
                    _ => "for (var index = 0; index < Items.Length; index++) { builder.AddContent(index, Items[index]); }"
                },
                variant == 3 ? "[Parameter] public string[] Items { get; set; } = [];" : "",
                "only supports straight-line RenderTreeBuilder statements"),
            10 => (
                "while_loop",
                variant switch
                {
                    0 => "var index = 0; while (index < 1) { builder.AddContent(index, " + Literal(marker) + "); index++; }",
                    1 => "var index = 0; do { builder.AddContent(index, " + Literal(marker) + "); index++; } while (index < 1);",
                    2 => "builder.OpenElement(0, \"div\"); while (Count < 1) { builder.AddContent(1, " + Literal(marker) + "); } builder.CloseElement();",
                    _ => "var index = Items.Length; while (index > 0) { builder.AddContent(index, Items[--index]); }"
                },
                variant switch
                {
                    2 => "[Parameter] public int Count { get; set; }",
                    3 => "[Parameter] public string[] Items { get; set; } = [];",
                    _ => ""
                },
                "only supports straight-line RenderTreeBuilder statements"),
            11 => (
                "dynamic_component_type",
                variant switch
                {
                    0 => "builder.OpenComponent(0, ComponentType); builder.CloseComponent();",
                    1 => "var componentType = ComponentType; builder.OpenComponent(0, componentType); builder.CloseComponent();",
                    2 => "builder.OpenComponent(0, GetComponentType()); builder.CloseComponent();",
                    _ => "builder.OpenComponent(0, UseChild ? typeof(FailureMatrixChild) : ComponentType); builder.CloseComponent();"
                },
                "private System.Type ComponentType => typeof(FailureMatrixChild); private System.Type GetComponentType() => ComponentType; [Parameter] public bool UseChild { get; set; } private string Marker => " + Literal(marker) + ";",
                "OpenComponent must use a generic component type or typeof(T)"),
            12 => (
                "element_capture_on_component",
                "builder.OpenComponent<FailureMatrixChild>(0); " +
                (variant switch
                {
                    0 => "",
                    1 => "builder.AddComponentParameter(1, \"ChildContent\", (RenderFragment)(child => child.AddContent(0, " + Literal(marker) + "))); ",
                    2 => "builder.SetKey(" + Literal(marker) + "); ",
                    _ => "builder.AddComponentParameter(1, \"data-case\", " + Literal(marker) + "); "
                }) +
                "builder.AddElementReferenceCapture(2, value => { _ = value; }); builder.CloseComponent();",
                "private string Marker => " + Literal(marker) + ";",
                "Element reference captures require the current open element before children"),
            13 => (
                "component_capture_on_element",
                "builder.OpenElement(0, " + Literal(variant switch { 0 => "div", 1 => "section", 2 => "form", _ => "button" }) + "); " +
                (variant == 2 ? "builder.SetKey(" + Literal(marker) + "); " : "") +
                "builder.AddComponentReferenceCapture(1, value => { _ = value; }); builder.CloseElement();",
                "private string Marker => " + Literal(marker) + ";",
                "Component reference captures require the current open component before children"),
            14 => (
                "splat_after_child",
                variant switch
                {
                    0 => "builder.OpenElement(0, \"form\"); builder.AddContent(1, " + Literal(marker) + "); builder.AddMultipleAttributes(2, new Dictionary<string, object> { [\"data-test\"] = " + Literal(marker) + " }); builder.CloseElement();",
                    1 => "builder.OpenElement(0, \"form\"); builder.AddMarkupContent(1, " + Literal("<b>" + marker + "</b>") + "); builder.AddMultipleAttributes(2, AdditionalAttributes); builder.CloseElement();",
                    2 => "builder.OpenElement(0, \"form\"); builder.OpenElement(1, \"span\"); builder.CloseElement(); builder.AddMultipleAttributes(2, AdditionalAttributes); builder.CloseElement();",
                    _ => "builder.OpenElement(0, \"form\"); builder.AddContent(1, ChildContent); builder.AddMultipleAttributes(2, AdditionalAttributes); builder.CloseElement();"
                },
                variant switch
                {
                    0 => "",
                    3 => "[Parameter] public RenderFragment? ChildContent { get; set; } [Parameter] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }",
                    _ => "[Parameter] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }"
                },
                "Multiple attributes must be added before children"),
            _ => (
                "invalid_child_content",
                "builder.OpenComponent<FailureMatrixChild>(0); builder.AddComponentParameter(1, \"ChildContent\", " +
                (variant switch
                {
                    0 => Literal(marker),
                    1 => (variant + 10).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    2 => "true",
                    _ => "Text"
                }) +
                "); builder.CloseComponent();",
                variant == 3 ? "[Parameter] public string Text { get; set; } = " + Literal(marker) + ";" : "",
                "ChildContent component parameter must be a RenderFragment")
        };

    private static string Literal(string value)
        => System.Text.Json.JsonSerializer.Serialize(value);
}

internal static class RazorSgDirectRenderFailureMatrixTestHost
{
    private static readonly Lazy<Fixture> SharedFixture = new(CreateFixture);

    public static string EmitFailure(DirectRenderFailureCase testCase)
    {
        var fixture = SharedFixture.Value;
        var component = fixture.Components[testCase.TypeName];
        Assert.IsTrue(
            MemberClosureBuilder.TryBuild(fixture.Binding, component, out var closure, out var closureFailure),
            closureFailure);
        Assert.IsNotNull(closure);

        var emitted = RenderEmitter.TryEmit(
            fixture.Binding.Compilation,
            component.ComponentSymbol,
            component.BuildRenderTreeMethod,
            component.BuildRenderTreeBody,
            declaredNames: null,
            VueInjectRegistry.ForCompilation(fixture.Binding.Compilation),
            out var result,
            out var failure);

        Assert.IsFalse(emitted, "Failure case unexpectedly emitted: " + testCase.Id);
        Assert.IsNull(result);
        Assert.IsNotNull(failure);
        return failure;
    }

    private static Fixture CreateFixture()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(
                SharedComponentSource,
                parseOptions,
                path: "FailureMatrix/FailureMatrixChild.cs")
        };
        syntaxTrees.AddRange(DirectRenderFailureCaseCatalog.All.Select(testCase =>
            CSharpSyntaxTree.ParseText(
                BuildComponentSource(testCase),
                parseOptions,
                path: "FailureMatrix/" + testCase.TypeName + ".cs")));

        var compilation = CSharpCompilation.Create(
            "RazorVue.DirectRender.FailureMatrix",
            syntaxTrees,
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbols = DirectRenderFailureCaseCatalog.All
            .Select(testCase => compilation.GetTypeByMetadataName("RazorVue.FailureMatrix." + testCase.TypeName))
            .ToArray();
        Assert.IsFalse(componentSymbols.Any(static symbol => symbol is null));
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                componentSymbols.Cast<INamedTypeSymbol>().ToImmutableArray(),
                out var binding,
                out var failure),
            failure);
        Assert.IsNotNull(binding);

        return new Fixture(
            binding,
            binding!.Components.ToImmutableDictionary(
                static component => component.ComponentSymbol.Name,
                StringComparer.Ordinal));
    }

    private static string BuildComponentSource(DirectRenderFailureCase testCase)
        => $$"""
            #nullable enable
            using System.Collections.Generic;
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Web;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace RazorVue.FailureMatrix;

            [ECMAScriptModule("./failure-matrix/{{testCase.Id}}")]
            public sealed class {{testCase.TypeName}} : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    {{testCase.Body}}
                }

                {{testCase.Members}}
            }
            """;

    private const string SharedComponentSource = """
        #nullable enable
        using System.ComponentModel;
        using ECMAScript;
        using ECMAScript.VueContract;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;
        using static ECMAScript.Vue;

        namespace RazorVue.FailureMatrix;

        [ECMAScriptModule("./failure-matrix/child.mjs")]
        public sealed class FailureMatrixChild : ComponentBase, IVueComponent
        {
            [Parameter, Description("@#default")] public RenderFragment? ChildContent { get; set; }
        }

        public sealed class FailureNoImportChild : ComponentBase, IVueComponent;

        [VueLibraryComponent(" ", "FailureWhitespaceLibrarySpecifierChild")]
        public sealed class FailureWhitespaceLibrarySpecifierChild : ComponentBase, IVueComponent;

        [VueLibraryComponent("failure-library", " ")]
        public sealed class FailureWhitespaceLibraryExportChild : ComponentBase, IVueComponent;

        [ECMAScriptModule(" ")]
        public sealed class FailureWhitespaceModuleChild : ComponentBase, IVueComponent;

        public sealed class ExternalRenderTreeBuilderHelper
        {
            public void Render(RenderTreeBuilder builder, string value)
            {
                builder.AddContent(0, value);
            }
        }

        [ECMAScriptModule("./failure-matrix/article-child.mjs")]
        public sealed class FailureMatrixArticleChild : ComponentBase, IVueComponent;

        [ECMAScriptModule("./failure-matrix/aside-child.mjs")]
        public sealed class FailureMatrixAsideChild : ComponentBase, IVueComponent;

        [ECMAScriptModule("./failure-matrix/button-child.mjs")]
        public sealed class FailureMatrixButtonChild : ComponentBase, IVueComponent;

        [ECMAScriptModule("./failure-matrix/div-child.mjs")]
        public sealed class FailureMatrixDivChild : ComponentBase, IVueComponent;

        [ECMAScriptModule("./failure-matrix/form-child.mjs")]
        public sealed class FailureMatrixFormChild : ComponentBase, IVueComponent;

        [ECMAScriptModule("./failure-matrix/main-child.mjs")]
        public sealed class FailureMatrixMainChild : ComponentBase, IVueComponent;

        [ECMAScriptModule("./failure-matrix/section-child.mjs")]
        public sealed class FailureMatrixSectionChild : ComponentBase, IVueComponent;

        [ECMAScriptModule("./failure-matrix/span-child.mjs")]
        public sealed class FailureMatrixSpanChild : ComponentBase, IVueComponent;

        public static class ExternalRenderFragments
        {
            public static RenderFragment Fragment => null!;

            public static RenderFragment<string> GenericFragment => null!;

            public static void Render(RenderTreeBuilder builder)
            {
            }
        }

        public static class FailureRenderTreeBuilderExtensions
        {
            public static int UnsupportedBuilderExtension(this RenderTreeBuilder builder) => 0;

            public static int UnsupportedBuilderExtensionWithValue(this RenderTreeBuilder builder, string value) => value.Length;

            public static T UnsupportedBuilderExtensionGeneric<T>(this RenderTreeBuilder builder, T value) => value;

            public static int UnsupportedBuilderExtensionOptional(this RenderTreeBuilder builder, int value = 0) => value;
        }
        """;

    private sealed record Fixture(
        GeneratedCSharpBinding Binding,
        ImmutableDictionary<string, BoundComponent> Components);
}
