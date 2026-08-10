using System.Collections.Immutable;
using System.Text.Json;
using Acornima;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

internal static class RazorSgDirectRenderMatrixTestHost
{
    private static readonly Lazy<Fixture> SharedFixture = new(CreateFixture);

    public static DirectRenderObservation Emit(DirectRenderCase testCase)
    {
        var fixture = SharedFixture.Value;
        var component = fixture.Components[testCase.TypeName];
        Assert.IsTrue(
            MemberClosureBuilder.TryBuild(fixture.Binding, component, out var closure, out var closureFailure),
            closureFailure);
        Assert.IsNotNull(closure);

        var registry = VueInjectRegistry.ForCompilation(fixture.Binding.Compilation);
        var emitted = RenderEmitter.TryEmit(
            fixture.Binding.Compilation,
            component.ComponentSymbol,
            component.BuildRenderTreeMethod,
            component.BuildRenderTreeBody,
            declaredNames: null,
            registry,
            out var result,
            out var failure);

        Assert.IsTrue(emitted, failure);
        Assert.IsNotNull(result);
        return new DirectRenderObservation(
            result.RenderExpression.ToKnRECMAScript(),
            string.Join("\n", result.PreludeStatements.Select(static statement => statement.ToKnRECMAScript())),
            result.UsesFragment,
            result.UsesStaticVNode,
            result.UsesProps,
            result.UsesSlots,
            string.Join("\n", result.ImportDeclarations.Select(static declaration => declaration.ToKnRECMAScript())),
            result.ImportDeclarations.Length);
    }

    private static Fixture CreateFixture()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(
                SharedComponentSource,
                parseOptions,
                path: "Matrix/MatrixChild.cs")
        };
        syntaxTrees.AddRange(DirectRenderCaseCatalog.SuccessCases
            .Select(testCase => CSharpSyntaxTree.ParseText(
                BuildComponentSource(testCase),
                parseOptions,
                path: "Matrix/" + testCase.TypeName + ".cs")));
        var compilation = CSharpCompilation.Create(
            "RazorVue.DirectRender.Matrix",
            syntaxTrees,
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbols = DirectRenderCaseCatalog.SuccessCases
            .Select(testCase => compilation.GetTypeByMetadataName("RazorVue.Matrix." + testCase.TypeName))
            .ToArray();
        Assert.IsFalse(componentSymbols.Any(static symbol => symbol is null));
        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            componentSymbols.Cast<INamedTypeSymbol>().ToImmutableArray(),
            out var binding,
            out var failure), failure);
        Assert.IsNotNull(binding);

        return new Fixture(
            binding,
            binding!.Components.ToImmutableDictionary(
                static component => component.ComponentSymbol.Name,
                StringComparer.Ordinal));
    }

    private static string BuildComponentSource(DirectRenderCase testCase)
        => $$"""
            #nullable enable
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Web;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue3;

            namespace RazorVue.Matrix;

            [ECMAScriptModule("./matrix/{{testCase.Id}}")]
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
        using ECMAScript;
        using ECMAScript.VueContract;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;
        using static ECMAScript.Vue3;

        namespace RazorVue.Matrix;

        [ECMAScriptModule("./matrix/child.mjs")]
        public sealed class MatrixChild : ComponentBase, IVueComponent
        {
            [ECMAScriptName("heading")]
            [Parameter] public string? Title { get; set; }
            [ECMAScriptName("count")]
            [Parameter] public int Count { get; set; }
            [ECMAScriptName("enabled")]
            [Parameter] public bool Enabled { get; set; }
            [ECMAScriptName("modelValue")]
            [Parameter] public string? Value { get; set; }
            [ECMAScriptName("onClick")]
            [Parameter] public EventCallback OnClick { get; set; }
            [ECMAScriptName("onUpdate:modelValue")]
            [Parameter] public EventCallback<string> ValueChanged { get; set; }
            [ECMAScriptName("default")]
            [Parameter] public RenderFragment? ChildContent { get; set; }
            [ECMAScriptName("header")]
            [Parameter] public RenderFragment? Header { get; set; }
            [ECMAScriptName("item")]
            [Parameter] public RenderFragment<string>? ItemTemplate { get; set; }
        }

        [VueLibraryComponent(" matrix-library ", " MatrixLibraryChild ")]
        public sealed class MatrixLibraryChild : ComponentBase, IVueComponent;

        [ECMAScriptModule(" ./matrix/module-preferred ")]
        [VueLibraryComponent("discarded-library", "DiscardedLibraryChild")]
        public sealed class MatrixModulePreferredChild : ComponentBase, IVueComponent;

        public static class MatrixRenderTreeBuilderHelpers
        {
            public static void Render(RenderTreeBuilder target, string value)
            {
                target.OpenElement(0, "span");
                target.AddContent(1, value);
                target.CloseElement();
            }
        }
        """;

    private sealed record Fixture(
        GeneratedCSharpBinding Binding,
        ImmutableDictionary<string, BoundComponent> Components);
}

internal sealed record DirectRenderObservation(
    string RenderExpression,
    string Prelude,
    bool UsesFragment,
    bool UsesStaticVNode,
    bool UsesProps,
    bool UsesSlots,
    string Imports,
    int ImportCount);

public sealed record DirectRenderCase(
    string Id,
    string TypeName,
    string Body,
    string ExpectedFragment,
    string? AdditionalExpectedFragment,
    bool UsesFragment,
    bool UsesStaticVNode,
    DirectRenderCaseGroup Group,
    string Members,
    bool UsesProps,
    bool UsesSlots,
    int ImportCount,
    string? TertiaryExpectedFragment,
    string? UnexpectedFragment,
    RazorVueUsageScenarioId? Scenario,
    string? ExpectedImportFragment = null,
    string? UnexpectedImportFragment = null);

public enum DirectRenderCaseGroup
{
    Surface,
    Component,
    ControlFlow,
    Extended,
    Advanced,
    Coverage
}

internal static partial class DirectRenderCaseCatalog
{
    public static IReadOnlyList<DirectRenderCase> SuccessCases { get; } = CreateSuccessCases();

    private static IReadOnlyList<DirectRenderCase> CreateSuccessCases()
    {
        var cases = new List<DirectRenderCase>();
        AddTextCases(cases);
        AddMarkupCases(cases);
        AddElementCases(cases);
        AddAttributeCases(cases);
        AddComponentCases(cases);
        AddComponentImportCases(cases);
        AddHelperInvocationCases(cases);
        AddControlFlowCases(cases);
        AddExtendedCases(cases);
        AddAdvancedCases(cases);
        AddCoverageCases(cases);
        return cases;
    }

    private static void AddTextCases(List<DirectRenderCase> cases)
    {
        string[] values =
        [
            "",
            " ",
            "plain",
            "two words",
            " leading",
            "trailing ",
            "  both  ",
            "\t",
            "\n",
            "\r\n",
            "line1\nline2",
            "quote \" value",
            "single ' quote",
            "backslash \\",
            "slash / value",
            "${template}",
            "`backtick`",
            "<tag>",
            "</script>",
            "&amp;",
            "a\0b",
            "\u0001control",
            "\u007fdelete",
            "\u0085next-line",
            "汉字",
            "かな",
            "한글",
            "العربية",
            "עברית",
            "हिन्दी",
            "🙂",
            "🙂 text",
            "e\u0301",
            "\u00e9",
            "\u2028",
            "\u2029",
            "x\u2028y",
            "x\u2029y",
            "non-breaking\u00a0space",
            "\uffff",
            "a-b_c.d",
            "0",
            "false",
            "null",
            "undefined",
            "class",
            "onclick",
            new('x', 128)
        ];

        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];
            Add(
                cases,
                "content_" + index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture),
                "builder.AddContent(0, " + CSharpStringLiteral(value) + ");",
                JavaScriptAstFactory.CreateStringLiteral(value).ToKnRECMAScript(),
                additionalExpectedFragment: null,
                usesFragment: false,
                usesStaticVNode: false);
        }
    }

    private static void AddMarkupCases(List<DirectRenderCase> cases)
    {
        string[] values =
        [
            "<b>bold</b>",
            "<i>italic</i>",
            "<span></span>",
            "<span>text</span>",
            "<div class=\"box\"></div>",
            "<input disabled>",
            "<br>",
            "<hr>",
            "<!--comment-->",
            "&amp;",
            "&#169;",
            "<p>line1\nline2</p>",
            "<svg><path d=\"M0 0\"></path></svg>",
            "<template>content</template>",
            "<section data-id=\"1\">one</section>",
            "<section data-id='2'>two</section>",
            "<table><tbody><tr><td>x</td></tr></tbody></table>",
            "<ul><li>one</li><li>two</li></ul>",
            "<pre>  fixed  </pre>",
            "<code>${value}</code>",
            "<span>汉字</span>",
            "<span>🙂</span>",
            "<math><mi>x</mi></math>",
            "<details open><summary>x</summary></details>",
            "<picture><source srcset=\"a.webp\"><img src=\"a.png\"></picture>",
            "<video controls></video>",
            "<audio controls></audio>",
            "<canvas width=\"10\" height=\"10\"></canvas>",
            "<dialog open>hello</dialog>",
            "<slot></slot>",
            "<custom-element></custom-element>",
            "<div><span>nested</span></div>"
        ];

        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];
            Add(
                cases,
                "markup_" + index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture),
                "builder.AddMarkupContent(0, " + CSharpStringLiteral(value) + ");",
                JavaScriptAstFactory.CreateStringLiteral(value).ToKnRECMAScript(),
                additionalExpectedFragment: null,
                usesFragment: false,
                usesStaticVNode: true);
        }
    }

    private static void AddElementCases(List<DirectRenderCase> cases)
    {
        string[] tags =
        [
            "a", "article", "aside", "button", "canvas", "code", "details", "dialog",
            "div", "em", "fieldset", "figure", "footer", "form", "h1", "h2",
            "header", "img", "input", "label", "li", "main", "nav", "ol",
            "option", "p", "picture", "pre", "section", "select", "small", "span",
            "strong", "summary", "table", "tbody", "td", "textarea", "tfoot", "th",
            "thead", "tr", "ul", "video", "svg", "path", "circle", "custom-element"
        ];

        for (var index = 0; index < tags.Length; index++)
        {
            var tag = tags[index];
            Add(
                cases,
                "element_" + tag.Replace('-', '_'),
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.CloseElement();",
                "h(" + JavaScriptAstFactory.CreateStringLiteral(tag).ToKnRECMAScript(),
                additionalExpectedFragment: null,
                usesFragment: false,
                usesStaticVNode: false);
        }
    }

    private static void AddAttributeCases(List<DirectRenderCase> cases)
    {
        string[] names =
        [
            "id", "class", "style", "title", "role", "tabindex", "aria-label", "data-id",
            "href", "target", "rel", "src", "alt", "width", "height", "type",
            "name", "value", "placeholder", "disabled", "checked", "selected", "multiple", "required",
            "readonly", "autocomplete", "autofocus", "min", "max", "step", "pattern", "accept",
            "action", "method", "enctype", "for", "form", "rows", "cols", "rowspan",
            "colspan", "scope", "datetime", "cite", "download", "draggable", "hidden", "lang",
            "dir", "slot", "part", "exportparts", "contenteditable", "spellcheck", "translate", "loading",
            "decoding", "referrerpolicy", "srcset", "sizes", "viewBox", "fill", "stroke", "stroke-width"
        ];

        for (var index = 0; index < names.Length; index++)
        {
            var name = names[index];
            var value = "attribute-value-" + index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            Add(
                cases,
                "attribute_" + name.Replace('-', '_'),
                "builder.OpenElement(0, \"div\"); " +
                "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(value) + "); " +
                "builder.CloseElement();",
                FormatObjectPropertyKey(name),
                JavaScriptAstFactory.CreateStringLiteral(value).ToKnRECMAScript(),
                usesFragment: false,
                usesStaticVNode: false);
        }
    }

    private static void AddComponentCases(List<DirectRenderCase> cases)
    {
        for (var index = 0; index < 8; index++)
        {
            var value = "heading-" + index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            AddComponentParameterCase(cases, "title_" + index.ToString("D2"), "Title", CSharpStringLiteral(value), "heading", JavaScriptAstFactory.CreateStringLiteral(value).ToKnRECMAScript());
        }

        for (var index = 0; index < 8; index++)
        {
            var value = index * 17 - 34;
            AddComponentParameterCase(cases, "count_" + index.ToString("D2"), "Count", value.ToString(System.Globalization.CultureInfo.InvariantCulture), "count", value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        (string Id, string Expression, string ExpectedFragment)[] booleanExpressions =
        [
            ("literal_true", "true", "true"),
            ("literal_false", "false", "false"),
            ("negated_false", "!false", "false"),
            ("negated_true", "!true", "true"),
            ("equality", "true == true", "true"),
            ("inequality", "false != true", "false"),
            ("logical_and", "true && false", "false"),
            ("logical_or", "false || true", "true")
        ];
        foreach (var expression in booleanExpressions)
        {
            AddComponentParameterCase(
                cases,
                "enabled_" + expression.Id,
                "Enabled",
                expression.Expression,
                "enabled",
                expression.ExpectedFragment);
        }

        for (var index = 0; index < 8; index++)
        {
            var value = "model-" + index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            AddComponentParameterCase(cases, "value_" + index.ToString("D2"), "Value", CSharpStringLiteral(value), "modelValue", JavaScriptAstFactory.CreateStringLiteral(value).ToKnRECMAScript());
        }

        for (var index = 0; index < 8; index++)
        {
            var suffix = index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            Add(
                cases,
                "component_event_" + suffix,
                "builder.OpenComponent<MatrixChild>(0); " +
                "builder.AddComponentParameter(1, \"OnClick\", EventCallback.Factory.Create(this, HandleClick" + suffix + ")); " +
                "builder.CloseComponent();",
                "onClick",
                "HandleClick" + suffix,
                usesFragment: false,
                usesStaticVNode: false,
                group: DirectRenderCaseGroup.Component,
                members: "private void HandleClick" + suffix + "() { }",
                importCount: 1);
        }

        for (var index = 0; index < 8; index++)
        {
            var suffix = index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            var initial = "bind-" + suffix;
            var fieldName = "boundText" + suffix;
            Add(
                cases,
                "component_bind_" + suffix,
                "builder.OpenComponent<MatrixChild>(0); " +
                "builder.AddComponentParameter(1, \"Value\", " + fieldName + "); " +
                "builder.AddComponentParameter(2, \"ValueChanged\", EventCallback.Factory.CreateBinder(this, value => " + fieldName + " = value, " + fieldName + ")); " +
                "builder.CloseComponent();",
                "modelValue",
                JavaScriptAstFactory.CreateStringLiteral("onUpdate:modelValue").ToKnRECMAScript(),
                usesFragment: false,
                usesStaticVNode: false,
                group: DirectRenderCaseGroup.Component,
                members: "private string " + fieldName + " = " + CSharpStringLiteral(initial) + ";",
                importCount: 1);
        }

        for (var index = 0; index < 4; index++)
        {
            var suffix = index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            var content = "default-slot-" + suffix;
            AddRenderFragmentCase(cases, "default_slot_" + suffix, "ChildContent", "default", content, typed: false);
        }

        for (var index = 0; index < 4; index++)
        {
            var suffix = index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            var content = "header-slot-" + suffix;
            AddRenderFragmentCase(cases, "header_slot_" + suffix, "Header", "header", content, typed: false);
        }

        for (var index = 0; index < 8; index++)
        {
            var suffix = index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            var prefix = "item-slot-" + suffix + ":";
            AddRenderFragmentCase(cases, "item_slot_" + suffix, "ItemTemplate", "item", prefix, typed: true);
        }

    }

    private static void AddControlFlowCases(List<DirectRenderCase> cases)
    {
        for (var index = 0; index < 16; index++)
        {
            var suffix = index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            var visible = "visible-" + suffix;
            var hidden = "hidden-" + suffix;
            Add(
                cases,
                "control_conditional_content_" + suffix,
                "if (Visible) { builder.AddContent(0, " + CSharpStringLiteral(visible) + "); } " +
                "else { builder.AddContent(1, " + CSharpStringLiteral(hidden) + "); }",
                "props.Visible",
                JavaScriptAstFactory.CreateStringLiteral(visible).ToKnRECMAScript(),
                usesFragment: false,
                usesStaticVNode: false,
                group: DirectRenderCaseGroup.ControlFlow,
                members: "[Parameter] public bool Visible { get; set; }",
                usesProps: true);
        }

        string[] attributeNames =
        [
            "class", "title", "aria-label", "data-state", "role", "hidden", "disabled", "checked",
            "selected", "readonly", "required", "tabindex", "lang", "dir", "draggable", "contenteditable"
        ];
        for (var index = 0; index < attributeNames.Length; index++)
        {
            var suffix = index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            var attributeName = attributeNames[index];
            var whenTrue = "true-" + suffix;
            var whenFalse = "false-" + suffix;
            Add(
                cases,
                "control_conditional_attribute_" + suffix,
                "builder.OpenElement(0, \"div\"); " +
                "if (Visible) { builder.AddAttribute(1, " + CSharpStringLiteral(attributeName) + ", " + CSharpStringLiteral(whenTrue) + "); } " +
                "else { builder.AddAttribute(2, " + CSharpStringLiteral(attributeName) + ", " + CSharpStringLiteral(whenFalse) + "); } " +
                "builder.CloseElement();",
                "props.Visible",
                FormatObjectPropertyKey(attributeName),
                usesFragment: false,
                usesStaticVNode: false,
                group: DirectRenderCaseGroup.ControlFlow,
                members: "[Parameter] public bool Visible { get; set; }",
                usesProps: true,
                importCount: 1);
        }

        // Handwritten BuildRenderTree implementations commonly omit braces for a
        // single attribute in each branch. Roslyn exposes that form as expression
        // statements instead of blocks, while Razor SG normally emits blocks.
        Add(
            cases,
            "control_single_statement_conditional_attribute",
            "builder.OpenElement(0, \"button\"); " +
            "if (Visible) builder.AddAttribute(1, \"data-state\", \"ready\"); " +
            "else builder.AddAttribute(2, \"data-state\", \"blocked\"); " +
            "builder.CloseElement();",
            "props.Visible",
            "data-state",
            usesFragment: false,
            usesStaticVNode: false,
            group: DirectRenderCaseGroup.ControlFlow,
            members: "[Parameter] public bool Visible { get; set; }",
            usesProps: true,
            importCount: 1);

        for (var index = 0; index < 16; index++)
        {
            var suffix = index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
            var prefix = "item-" + suffix + ":";
            var body = index < 8
                ? "foreach (var item in Items) { builder.AddContent(0, " + CSharpStringLiteral(prefix) + " + item); }"
                : "foreach (var item in Items) { builder.OpenElement(0, \"span\"); builder.AddContent(1, " + CSharpStringLiteral(prefix) + " + item); builder.CloseElement(); }";
            Add(
                cases,
                "control_foreach_" + suffix,
                body,
                "Array.from(props.Items ?? []",
                JavaScriptAstFactory.CreateStringLiteral(prefix).ToKnRECMAScript(),
                usesFragment: false,
                usesStaticVNode: false,
                group: DirectRenderCaseGroup.ControlFlow,
                members: "[Parameter] public string[] Items { get; set; } = [];",
                usesProps: true);
        }

    }

    private static void AddComponentParameterCase(
        List<DirectRenderCase> cases,
        string id,
        string parameterName,
        string csharpValue,
        string runtimeName,
        string expectedValue)
        => Add(
            cases,
            "component_prop_" + id,
            "builder.OpenComponent<MatrixChild>(0); " +
            "builder.AddComponentParameter(1, " + CSharpStringLiteral(parameterName) + ", " + csharpValue + "); " +
            "builder.CloseComponent();",
            runtimeName,
            expectedValue,
            usesFragment: false,
            usesStaticVNode: false,
            group: DirectRenderCaseGroup.Component,
            importCount: 1);

    private static void AddRenderFragmentCase(
        List<DirectRenderCase> cases,
        string id,
        string parameterName,
        string runtimeName,
        string content,
        bool typed)
    {
        var declaration = typed
            ? "RenderFragment<string> fragment = context => child => { child.AddContent(0, " + CSharpStringLiteral(content) + "); child.AddContent(1, context); }; "
            : "RenderFragment fragment = child => child.AddContent(0, " + CSharpStringLiteral(content) + "); ";
        Add(
            cases,
            "component_" + id,
            declaration +
            "builder.OpenComponent<MatrixChild>(2); " +
            "builder.AddComponentParameter(3, " + CSharpStringLiteral(parameterName) + ", fragment); " +
            "builder.CloseComponent();",
            runtimeName,
            JavaScriptAstFactory.CreateStringLiteral(content).ToKnRECMAScript(),
            usesFragment: typed,
            usesStaticVNode: false,
            group: DirectRenderCaseGroup.Component,
            importCount: 1);
    }

    private static void AddComponentImportCases(List<DirectRenderCase> cases)
    {
        Add(
            cases,
            "component_library_import_trimmed_metadata",
            "builder.OpenComponent<MatrixLibraryChild>(0); builder.CloseComponent();",
            "MatrixLibraryChild",
            additionalExpectedFragment: null,
            usesFragment: false,
            usesStaticVNode: false,
            group: DirectRenderCaseGroup.Component,
            importCount: 1,
            expectedImportFragment: "matrix-library");
        Add(
            cases,
            "component_module_import_precedes_library_metadata",
            "builder.OpenComponent<MatrixModulePreferredChild>(0); builder.CloseComponent();",
            "h(",
            additionalExpectedFragment: null,
            usesFragment: false,
            usesStaticVNode: false,
            group: DirectRenderCaseGroup.Component,
            importCount: 1,
            expectedImportFragment: "./matrix/module-preferred.mjs",
            unexpectedImportFragment: "discarded-library");
    }

    private static void AddHelperInvocationCases(List<DirectRenderCase> cases)
        => Add(
            cases,
            "extended_external_static_builder_helper",
            "MatrixRenderTreeBuilderHelpers.Render(builder, \"external-static-builder-helper\");",
            "h(\"span\"",
            "external-static-builder-helper",
            usesFragment: false,
            usesStaticVNode: false,
            group: DirectRenderCaseGroup.Extended);

    private static void Add(
        List<DirectRenderCase> cases,
        string id,
        string body,
        string expectedFragment,
        string? additionalExpectedFragment,
        bool usesFragment,
        bool usesStaticVNode,
        DirectRenderCaseGroup group = DirectRenderCaseGroup.Surface,
        string members = "",
        bool usesProps = false,
        bool usesSlots = false,
        int importCount = 0,
        string? tertiaryExpectedFragment = null,
        string? unexpectedFragment = null,
        RazorVueUsageScenarioId? scenario = null,
        string? expectedImportFragment = null,
        string? unexpectedImportFragment = null)
    {
        var typeName = "DirectRender" + cases.Count.ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
        cases.Add(new DirectRenderCase(
            id,
            typeName,
            body,
            expectedFragment,
            additionalExpectedFragment,
            usesFragment,
            usesStaticVNode,
            group,
            members,
            usesProps,
            usesSlots,
            importCount,
            tertiaryExpectedFragment,
            unexpectedFragment,
            scenario,
            expectedImportFragment,
            unexpectedImportFragment));
    }

    private static string CSharpStringLiteral(string value)
        => JsonSerializer.Serialize(value);

    private static string FormatObjectPropertyKey(string name)
    {
        if (name.Length > 0 && (char.IsLetter(name[0]) || name[0] is '_' or '$') &&
            name.Skip(1).All(static character => char.IsLetterOrDigit(character) || character is '_' or '$'))
        {
            return name;
        }

        return JavaScriptAstFactory.CreateStringLiteral(name).ToKnRECMAScript();
    }
}
