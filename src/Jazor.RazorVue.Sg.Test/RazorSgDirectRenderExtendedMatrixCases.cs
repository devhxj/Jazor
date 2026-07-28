using System.Globalization;
using Jazor.Compiler;

namespace Jazor.RazorVue.Sg.Test;

internal static partial class DirectRenderCaseCatalog
{
    private static void AddExtendedCases(List<DirectRenderCase> cases)
    {
        AddExpressionContentCases(cases);
        AddDynamicAttributeCases(cases);
        AddTreeCompositionCases(cases);
        AddSplatAndKeyCases(cases);
        AddElementEventCases(cases);
        AddReferenceCaptureCases(cases);
        AddStructuredComponentCases(cases);
    }

    private static void AddExpressionContentCases(List<DirectRenderCase> cases)
    {
        var variants = CreateExpressionVariants();
        for (var variantIndex = 0; variantIndex < variants.Length; variantIndex++)
        {
            var variant = variants[variantIndex];
            for (var placement = 0; placement < 8; placement++)
            {
                var suffix = variantIndex.ToString("D2", CultureInfo.InvariantCulture) + "_" +
                             placement.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "expression-" + suffix;
                var (body, additionalExpected, usesFragment) = placement switch
                {
                    0 => (
                        "builder.AddContent(0, " + variant.Expression + ");",
                        null,
                        false),
                    1 => (
                        "builder.OpenElement(0, \"div\"); builder.AddContent(1, " + variant.Expression + "); builder.CloseElement();",
                        "h(\"div\"",
                        false),
                    2 => (
                        "builder.AddContent(0, " + CSharpStringLiteral(marker) + "); builder.AddContent(1, " + variant.Expression + ");",
                        JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript(),
                        true),
                    3 => (
                        "builder.OpenRegion(0); builder.AddContent(1, " + variant.Expression + "); builder.CloseRegion();",
                        null,
                        false),
                    4 => (
                        "builder.OpenElement(0, \"section\"); builder.OpenElement(1, \"strong\"); builder.AddContent(2, " + variant.Expression + "); builder.CloseElement(); builder.CloseElement();",
                        "h(\"strong\"",
                        false),
                    5 => (
                        "builder.OpenElement(0, \"aside\"); builder.AddAttribute(1, \"data-case\", " + CSharpStringLiteral(marker) + "); builder.AddContent(2, " + variant.Expression + "); builder.CloseElement();",
                        JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript(),
                        false),
                    6 => (
                        "builder.AddContent(0, " + variant.Expression + "); builder.AddContent(1, " + CSharpStringLiteral(marker) + ");",
                        JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript(),
                        true),
                    _ => (
                        "builder.OpenElement(0, \"div\"); builder.OpenElement(1, \"span\"); builder.AddContent(2, " + variant.Expression + "); builder.CloseElement(); builder.CloseElement();",
                        "h(\"span\"",
                        false)
                };

                Add(
                    cases,
                    "expression_content_" + variant.Id + "_" + placement.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    variant.ExpectedFragment,
                    additionalExpected,
                    usesFragment,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Extended,
                    members: variant.Members,
                    usesProps: true);
            }
        }
    }

    private static void AddDynamicAttributeCases(List<DirectRenderCase> cases)
    {
        string[] names =
        [
            "title", "data-state", "aria-label", "tabindex",
            "disabled", "class", "value", "stroke-width"
        ];
        var variants = CreateExpressionVariants();
        for (var nameIndex = 0; nameIndex < names.Length; nameIndex++)
        {
            var name = names[nameIndex];
            for (var variantIndex = 0; variantIndex < variants.Length; variantIndex++)
            {
                var variant = variants[variantIndex];
                Add(
                    cases,
                    "dynamic_attribute_" + name.Replace('-', '_') + "_" + variant.Id,
                    "builder.OpenElement(0, \"div\"); " +
                    "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", " + variant.Expression + "); " +
                    "builder.CloseElement();",
                    FormatObjectPropertyKey(name),
                    variant.ExpectedFragment,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Extended,
                    members: variant.Members,
                    usesProps: true);
            }
        }
    }

    private static void AddTreeCompositionCases(List<DirectRenderCase> cases)
    {
        string[] outerTags = ["div", "article", "section", "main", "nav", "aside", "form", "ul"];
        for (var tagIndex = 0; tagIndex < outerTags.Length; tagIndex++)
        {
            var outerTag = outerTags[tagIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var suffix = tagIndex.ToString("D2", CultureInfo.InvariantCulture) + "_" +
                             shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "tree-" + suffix;
                var openOuter = "builder.OpenElement(0, " + CSharpStringLiteral(outerTag) + "); ";
                var (body, additionalExpected, usesFragment, usesStaticVNode, members, usesProps, importCount) = shape switch
                {
                    0 => (
                        openOuter + "builder.OpenElement(1, \"span\"); builder.AddContent(2, " + CSharpStringLiteral(marker) + "); builder.CloseElement(); builder.CloseElement();",
                        JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript(),
                        false, false, "", false, 0),
                    1 => (
                        openOuter + "builder.OpenElement(1, \"em\"); builder.AddContent(2, " + CSharpStringLiteral(marker + "-a") + "); builder.CloseElement(); builder.OpenElement(3, \"strong\"); builder.AddContent(4, " + CSharpStringLiteral(marker + "-b") + "); builder.CloseElement(); builder.CloseElement();",
                        JavaScriptAstFactory.CreateStringLiteral(marker + "-b").ToKnRECMAScript(),
                        false, false, "", false, 0),
                    2 => (
                        openOuter + "builder.OpenElement(1, \"section\"); builder.OpenElement(2, \"em\"); builder.AddContent(3, " + CSharpStringLiteral(marker) + "); builder.CloseElement(); builder.CloseElement(); builder.CloseElement();",
                        "h(\"em\"",
                        false, false, "", false, 0),
                    3 => (
                        openOuter + "builder.AddContent(1, " + CSharpStringLiteral(marker) + "); builder.AddMarkupContent(2, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); builder.CloseElement();",
                        "createStaticVNode",
                        false, true, "", false, 0),
                    4 => (
                        openOuter + "builder.OpenRegion(1); builder.AddContent(2, " + CSharpStringLiteral(marker + "-a") + "); builder.AddContent(3, " + CSharpStringLiteral(marker + "-b") + "); builder.CloseRegion(); builder.CloseElement();",
                        "Fragment",
                        true, false, "", false, 0),
                    5 => (
                        openOuter + "if (Visible) { builder.AddContent(1, " + CSharpStringLiteral(marker + "-visible") + "); } else { builder.AddContent(2, " + CSharpStringLiteral(marker + "-hidden") + "); } builder.CloseElement();",
                        "props.visible",
                        false, false, "[Parameter] public bool Visible { get; set; }", true, 0),
                    6 => (
                        openOuter + "foreach (var item in Items) { builder.OpenElement(1, \"li\"); builder.AddContent(2, " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseElement(); } builder.CloseElement();",
                        "Array.from(props.items ?? []",
                        false, false, "[Parameter] public string[] Items { get; set; } = [];", true, 0),
                    _ => (
                        openOuter + "builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker) + "); builder.CloseComponent(); builder.CloseElement();",
                        "heading",
                        false, false, "", false, 1)
                };

                Add(
                    cases,
                    "tree_composition_" + outerTag + "_" + shape.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    "h(" + JavaScriptAstFactory.CreateStringLiteral(outerTag).ToKnRECMAScript(),
                    additionalExpected,
                    usesFragment,
                    usesStaticVNode,
                    group: DirectRenderCaseGroup.Extended,
                    members: members,
                    usesProps: usesProps,
                    importCount: importCount);
            }
        }
    }

    private static void AddSplatAndKeyCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["div", "article", "section", "main", "nav", "aside", "form", "button"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var tag = tags[hostIndex];
            var suffix = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            for (var shape = 0; shape < 8; shape++)
            {
                var marker = "splat-key-" + suffix + "-" + shape.ToString("D2", CultureInfo.InvariantCulture);
                string body;
                string expected;
                string? additional;
                string members = "";
                var usesProps = false;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(" + CSharpStringLiteral(marker) + "); builder.CloseElement();";
                        expected = "key";
                        additional = JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript();
                        break;
                    case 1:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(KeyValue); builder.CloseElement();";
                        expected = "key";
                        additional = "props.keyValue";
                        members = "[Parameter] public string KeyValue { get; set; } = \"\";";
                        usesProps = true;
                        break;
                    case 2:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.SetKey(" + CSharpStringLiteral(marker) + "); builder.CloseComponent();";
                        expected = "key";
                        additional = JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript();
                        importCount = 1;
                        break;
                    case 3:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Count\", " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.SetKey(KeyValue); builder.CloseComponent();";
                        expected = "key";
                        additional = "props.keyValue";
                        members = "[Parameter] public string KeyValue { get; set; } = \"\";";
                        usesProps = true;
                        importCount = 1;
                        break;
                    case 4:
                        var attributeName = "data-key-" + suffix;
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); " +
                               "builder.AddMultipleAttributes(1, new System.Collections.Generic.Dictionary<string, object> { [" + CSharpStringLiteral(attributeName) + "] = " + CSharpStringLiteral(marker) + " }); " +
                               "builder.CloseElement();";
                        expected = FormatObjectPropertyKey(attributeName);
                        additional = JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript();
                        break;
                    case 5:
                        body = "builder.OpenComponent<MatrixChild>(0); " +
                               "builder.AddMultipleAttributes(1, new System.Collections.Generic.Dictionary<string, object> { [\"Title\"] = " + CSharpStringLiteral(marker) + " }); " +
                               "builder.CloseComponent();";
                        expected = "heading";
                        additional = JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript();
                        importCount = 1;
                        break;
                    case 6:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddAttribute(1, \"class\", " + CSharpStringLiteral(marker) + "); builder.AddMultipleAttributes(2, AdditionalAttributes); builder.CloseElement();";
                        expected = "mergeProps";
                        additional = "props.additionalAttributes";
                        members = "[Parameter] public System.Collections.Generic.IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }";
                        usesProps = true;
                        importCount = 1;
                        break;
                    default:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Count\", " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.AddMultipleAttributes(2, AdditionalAttributes); builder.CloseComponent();";
                        expected = "mergeProps";
                        additional = "props.additionalAttributes";
                        members = "[Parameter] public System.Collections.Generic.IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }";
                        usesProps = true;
                        importCount = 2;
                        break;
                }

                Add(
                    cases,
                    "splat_key_" + suffix + "_" + shape.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    expected,
                    additional,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Extended,
                    members: members,
                    usesProps: usesProps,
                    importCount: importCount);
            }
        }
    }

    private static void AddElementEventCases(List<DirectRenderCase> cases)
    {
        string[] eventNames = ["onclick", "onchange", "oninput", "onfocus", "onblur", "onkeydown", "onkeyup", "onsubmit"];
        for (var eventIndex = 0; eventIndex < eventNames.Length; eventIndex++)
        {
            var eventName = eventNames[eventIndex];
            var runtimeName = "on" + char.ToUpperInvariant(eventName[2]) + eventName[3..];
            for (var shape = 0; shape < 8; shape++)
            {
                var suffix = eventIndex.ToString("D2", CultureInfo.InvariantCulture) +
                             shape.ToString("D2", CultureInfo.InvariantCulture);
                var handlerName = "HandleEvent" + suffix;
                var valueHandlerName = "HandleValue" + suffix;
                var marker = "event-" + suffix;
                string body;
                string additional;
                string members;

                switch (shape)
                {
                    case 0:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handlerName + ")); builder.CloseElement();";
                        additional = char.ToLowerInvariant(handlerName[0]) + handlerName[1..];
                        members = "private void " + handlerName + "() { }";
                        break;
                    case 1:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, () => " + handlerName + "())); builder.CloseElement();";
                        additional = char.ToLowerInvariant(handlerName[0]) + handlerName[1..];
                        members = "private void " + handlerName + "() { }";
                        break;
                    case 2:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", (System.Action)" + handlerName + "); builder.CloseElement();";
                        additional = char.ToLowerInvariant(handlerName[0]) + handlerName[1..];
                        members = "private void " + handlerName + "() { }";
                        break;
                    case 3:
                        body = "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create<string>(this, " + valueHandlerName + ")); builder.CloseElement();";
                        additional = char.ToLowerInvariant(valueHandlerName[0]) + valueHandlerName[1..];
                        members = "private void " + valueHandlerName + "(string value) { }";
                        break;
                    case 4:
                        body = "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"value\", " + CSharpStringLiteral(marker) + "); builder.AddAttribute(2, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create<string>(this, " + valueHandlerName + ")); builder.SetUpdatesAttributeName(\"value\"); builder.CloseElement();";
                        additional = "eventOrValue";
                        members = "private void " + valueHandlerName + "(string value) { }";
                        break;
                    case 5:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handlerName + ")); builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", true); builder.CloseElement();";
                        additional = "preventDefault";
                        members = "private void " + handlerName + "() { }";
                        break;
                    case 6:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handlerName + ")); builder.AddEventStopPropagationAttribute(2, " + CSharpStringLiteral(eventName) + ", true); builder.CloseElement();";
                        additional = "stopPropagation";
                        members = "private void " + handlerName + "() { }";
                        break;
                    default:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handlerName + ")); builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", true); builder.AddEventStopPropagationAttribute(3, " + CSharpStringLiteral(eventName) + ", true); builder.CloseElement();";
                        additional = "preventDefault";
                        members = "private void " + handlerName + "() { }";
                        break;
                }

                Add(
                    cases,
                    "dom_event_" + eventName[2..] + "_" + shape.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    runtimeName,
                    additional,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Extended,
                    members: members);
            }
        }
    }

    private static void AddReferenceCaptureCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["div", "article", "section", "main", "nav", "aside", "form", "button"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var suffix = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var fieldName = shape < 4 ? "capturedElement" + suffix : "capturedChild" + suffix;
                var methodName = shape < 4 ? "CaptureElement" + suffix : "CaptureChild" + suffix;
                var marker = "reference-" + suffix + "-" + shape.ToString("D2", CultureInfo.InvariantCulture);
                string body;
                string additional;
                string members;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddElementReferenceCapture(1, value => " + fieldName + " = value); builder.CloseElement();";
                        additional = fieldName;
                        members = "private ElementReference " + fieldName + ";";
                        break;
                    case 1:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddElementReferenceCapture(1, " + methodName + "); builder.CloseElement();";
                        additional = char.ToLowerInvariant(methodName[0]) + methodName[1..];
                        members = "private void " + methodName + "(ElementReference value) { }";
                        break;
                    case 2:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddElementReferenceCapture(1, value => " + fieldName + " = value); builder.AddElementReferenceCapture(2, " + methodName + "); builder.CloseElement();";
                        additional = "value";
                        members = "private ElementReference " + fieldName + "; private void " + methodName + "(ElementReference value) { }";
                        break;
                    case 3:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(" + CSharpStringLiteral(marker) + "); builder.AddElementReferenceCapture(1, value => " + fieldName + " = value); builder.CloseElement();";
                        additional = JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript();
                        members = "private ElementReference " + fieldName + ";";
                        break;
                    case 4:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentReferenceCapture(1, value => " + fieldName + " = (MatrixChild)value); builder.CloseComponent();";
                        additional = fieldName;
                        members = "private MatrixChild? " + fieldName + ";";
                        importCount = 1;
                        break;
                    case 5:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentReferenceCapture(1, " + methodName + "); builder.CloseComponent();";
                        additional = char.ToLowerInvariant(methodName[0]) + methodName[1..];
                        members = "private void " + methodName + "(object value) { }";
                        importCount = 1;
                        break;
                    case 6:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentReferenceCapture(1, value => " + fieldName + " = (MatrixChild)value); builder.AddComponentReferenceCapture(2, " + methodName + "); builder.CloseComponent();";
                        additional = "value";
                        members = "private MatrixChild? " + fieldName + "; private void " + methodName + "(object value) { }";
                        importCount = 1;
                        break;
                    default:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.SetKey(" + CSharpStringLiteral(marker) + "); builder.AddComponentReferenceCapture(1, value => " + fieldName + " = (MatrixChild)value); builder.CloseComponent();";
                        additional = JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript();
                        members = "private MatrixChild? " + fieldName + ";";
                        importCount = 1;
                        break;
                }

                Add(
                    cases,
                    "reference_capture_" + suffix + "_" + shape.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    "ref",
                    additional,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Extended,
                    members: members,
                    importCount: importCount);
            }
        }
    }

    private static void AddStructuredComponentCases(List<DirectRenderCase> cases)
    {
        for (var variant = 0; variant < 8; variant++)
        {
            var suffix = variant.ToString("D2", CultureInfo.InvariantCulture);
            for (var shape = 0; shape < 8; shape++)
            {
                var marker = "structured-" + suffix + "-" + shape.ToString("D2", CultureInfo.InvariantCulture);
                string body;
                string expected;
                string? additional;
                string members;
                var usesFragment = false;
                var usesProps = false;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + "); builder.AddComponentParameter(2, \"Count\", " + variant.ToString(CultureInfo.InvariantCulture) + "); builder.AddComponentParameter(3, \"Enabled\", true); builder.CloseComponent();";
                        expected = "heading";
                        additional = "count";
                        members = "";
                        importCount = 1;
                        break;
                    case 1:
                        body = "builder.OpenComponent<MatrixChild>(0); if (Visible) { builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker + "-no") + "); } builder.CloseComponent();";
                        expected = "props.visible";
                        additional = "heading";
                        members = "[Parameter] public bool Visible { get; set; }";
                        usesProps = true;
                        importCount = 2;
                        break;
                    case 2:
                        body = "foreach (var item in Items) { builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseComponent(); }";
                        expected = "Array.from(props.items ?? []";
                        additional = "heading";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        usesProps = true;
                        importCount = 1;
                        break;
                    case 3:
                        body = "RenderFragment fragment = child => { child.OpenElement(0, \"span\"); child.AddContent(1, " + CSharpStringLiteral(marker) + "); child.CloseElement(); }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"ChildContent\", fragment); builder.CloseComponent();";
                        expected = "default";
                        additional = JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript();
                        members = "";
                        importCount = 1;
                        break;
                    case 4:
                        body = "RenderFragment<string> fragment = context => child => { child.OpenElement(0, \"strong\"); child.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + context); child.CloseElement(); }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"ItemTemplate\", fragment); builder.CloseComponent();";
                        expected = "item";
                        additional = JavaScriptAstFactory.CreateStringLiteral(marker + ":").ToKnRECMAScript();
                        members = "";
                        importCount = 1;
                        break;
                    case 5:
                        body = "RenderHelper(builder, Text + " + CSharpStringLiteral("-" + marker) + ");";
                        expected = "h(\"mark\"";
                        additional = "props.text";
                        members = "[Parameter] public string Text { get; set; } = \"\"; private static void RenderHelper(RenderTreeBuilder builder, string value) { builder.OpenElement(0, \"mark\"); builder.AddContent(1, value); builder.CloseElement(); }";
                        usesProps = true;
                        break;
                    case 6:
                        body = "RenderFragment fragment = child => { child.OpenElement(0, \"code\"); child.AddContent(1, " + CSharpStringLiteral(marker) + "); child.CloseElement(); }; builder.AddContent(2, fragment);";
                        expected = "h(\"code\"";
                        additional = JavaScriptAstFactory.CreateStringLiteral(marker).ToKnRECMAScript();
                        members = "";
                        break;
                    default:
                        body = "if (Visible) { foreach (var item in Items) { builder.OpenElement(0, \"span\"); builder.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseElement(); } } else { builder.AddContent(2, " + CSharpStringLiteral(marker + "-empty") + "); }";
                        expected = "props.visible";
                        additional = "Array.from(props.items ?? []";
                        members = "[Parameter] public bool Visible { get; set; } [Parameter] public string[] Items { get; set; } = [];";
                        usesProps = true;
                        break;
                }

                Add(
                    cases,
                    "structured_component_" + suffix + "_" + shape.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    expected,
                    additional,
                    usesFragment,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Extended,
                    members: members,
                    usesProps: usesProps,
                    importCount: importCount);
            }
        }
    }

    private static ExpressionVariant[] CreateExpressionVariants()
        =>
        [
            new("text", "Text", "props.text", "[Parameter] public string Text { get; set; } = \"\";"),
            new("concat", "\"prefix-\" + Text", "\"prefix-\" + props.text", "[Parameter] public string Text { get; set; } = \"\";"),
            new("count", "Count", "props.count", "[Parameter] public int Count { get; set; }"),
            new("arithmetic", "Count * 3 + 1", "props.count * 3 + 1", "[Parameter] public int Count { get; set; }"),
            new("conditional", "Visible ? \"shown\" : \"hidden\"", "props.visible ? \"shown\" : \"hidden\"", "[Parameter] public bool Visible { get; set; }"),
            new("negated", "!Visible", "!props.visible", "[Parameter] public bool Visible { get; set; }"),
            new("length", "Items.Length", "props.items.length", "[Parameter] public string[] Items { get; set; } = [];"),
            new("coalesce", "Text ?? \"fallback\"", "props.text ?? \"fallback\"", "[Parameter] public string? Text { get; set; }")
        ];

    private sealed record ExpressionVariant(
        string Id,
        string Expression,
        string ExpectedFragment,
        string Members);
}
