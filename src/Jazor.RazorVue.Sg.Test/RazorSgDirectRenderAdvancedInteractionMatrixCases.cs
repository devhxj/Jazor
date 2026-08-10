using System.Globalization;

namespace Jazor.RazorVue.Sg.Test;

internal static partial class DirectRenderCaseCatalog
{
    private static void AddNestedControlFlowCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["article", "code", "div", "em", "mark", "section", "span", "strong"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "nested-control-" + host + "-" + shapeId;
                string body;
                string additional;
                string tertiary;
                string members;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "foreach (var item in Items) { if (item.Length > " + hostIndex.ToString(CultureInfo.InvariantCulture) + ") { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item); } else { builder.AddContent(1, " + CSharpStringLiteral(marker + "-short:") + " + item); } }";
                        additional = "Array.from(props.Items ?? []";
                        tertiary = "item.length";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        break;
                    case 1:
                        body = "if (Visible) { foreach (var item in Items) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseElement(); } } else { builder.AddContent(2, " + CSharpStringLiteral(marker + "-empty") + "); }";
                        additional = "props.Visible";
                        tertiary = "Array.from(props.Items ?? []";
                        members = "[Parameter] public bool Visible { get; set; } [Parameter] public string[] Items { get; set; } = [];";
                        break;
                    case 2:
                        body = "foreach (var item in Items) { foreach (var other in OtherItems) { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item + other); } }";
                        additional = "Array.from(props.Items ?? []";
                        tertiary = "Array.from(props.OtherItems ?? []";
                        members = "[Parameter] public string[] Items { get; set; } = []; [Parameter] public string[] OtherItems { get; set; } = [];";
                        break;
                    case 3:
                        body = "if (Visible) { if (Enabled) { builder.AddContent(0, " + CSharpStringLiteral(marker + "-enabled") + "); } else { builder.AddContent(1, " + CSharpStringLiteral(marker + "-disabled") + "); } } else { builder.AddContent(2, " + CSharpStringLiteral(marker + "-hidden") + "); }";
                        additional = "props.Visible";
                        tertiary = "props.Enabled";
                        members = "[Parameter] public bool Visible { get; set; } [Parameter] public bool Enabled { get; set; }";
                        break;
                    case 4:
                        body = "foreach (var item in Items) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); if (Visible) { builder.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + item); } else { builder.AddContent(2, " + CSharpStringLiteral(marker + "-hidden:") + " + item); } builder.CloseElement(); }";
                        additional = "Array.from(props.Items ?? []";
                        tertiary = "props.Visible";
                        members = "[Parameter] public string[] Items { get; set; } = []; [Parameter] public bool Visible { get; set; }";
                        break;
                    case 5:
                        body = "if (Visible) { foreach (var item in Items) { builder.AddContent(0, " + CSharpStringLiteral(marker + "-primary:") + " + item); } } else { foreach (var item in OtherItems) { builder.AddContent(1, " + CSharpStringLiteral(marker + "-secondary:") + " + item); } }";
                        additional = "props.Visible";
                        tertiary = "Array.from(props.Items ?? []";
                        members = "[Parameter] public bool Visible { get; set; } [Parameter] public string[] Items { get; set; } = []; [Parameter] public string[] OtherItems { get; set; } = [];";
                        break;
                    case 6:
                        body = "foreach (var item in Items) { builder.OpenComponent<MatrixChild>(0); if (Visible) { builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + ":") + " + item); } else { builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker + "-hidden:") + " + item); } builder.CloseComponent(); }";
                        additional = "Array.from(props.Items ?? []";
                        tertiary = "heading";
                        members = "[Parameter] public string[] Items { get; set; } = []; [Parameter] public bool Visible { get; set; }";
                        importCount = 2;
                        break;
                    default:
                        body = "builder.OpenRegion(0); if (Visible) { foreach (var item in Items) { builder.OpenElement(1, " + CSharpStringLiteral(tag) + "); builder.AddContent(2, " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseElement(); } } else { builder.AddContent(3, " + CSharpStringLiteral(marker + "-empty") + "); } builder.CloseRegion();";
                        additional = "props.Visible";
                        tertiary = "Array.from(props.Items ?? []";
                        members = "[Parameter] public bool Visible { get; set; } [Parameter] public string[] Items { get; set; } = [];";
                        break;
                }

                Add(
                    cases,
                    "advanced_nested_control_" + host + "_" + shapeId,
                    body,
                    marker,
                    additional,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: true,
                    importCount: importCount,
                    tertiaryExpectedFragment: tertiary);
            }
        }
    }

    private static void AddAttributeCollectionCases(List<DirectRenderCase> cases)
    {
        string[] attributes = ["aria-label", "class", "data-state", "dir", "lang", "role", "tabindex", "title"];
        for (var hostIndex = 0; hostIndex < attributes.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var attribute = attributes[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "attribute-collection-" + host + "-" + shapeId;
                string body;
                string additional;
                string tertiary;
                string members = "";
                var usesProps = false;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "builder.OpenElement(0, \"div\"); builder.AddMultipleAttributes(1, new System.Collections.Generic.Dictionary<string, object> { [" + CSharpStringLiteral(attribute) + "] = " + CSharpStringLiteral(marker) + " }); builder.CloseElement();";
                        additional = FormatObjectPropertyKey(attribute);
                        tertiary = "h(\"div\"";
                        break;
                    case 1:
                        body = "builder.OpenElement(0, \"section\"); builder.AddMultipleAttributes(1, new System.Collections.Generic.Dictionary<string, object> { [" + CSharpStringLiteral(attribute) + "] = " + CSharpStringLiteral(marker) + ", [\"data-index\"] = " + hostIndex.ToString(CultureInfo.InvariantCulture) + " }); builder.CloseElement();";
                        additional = FormatObjectPropertyKey(attribute);
                        tertiary = "data-index";
                        break;
                    case 2:
                        body = "builder.OpenElement(0, \"article\"); builder.AddMultipleAttributes(1, new System.Collections.Generic.Dictionary<string, object> { { " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker) + " }, { \"data-shape\", " + CSharpStringLiteral(shapeId) + " } }); builder.CloseElement();";
                        additional = FormatObjectPropertyKey(attribute);
                        tertiary = "data-shape";
                        break;
                    case 3:
                        body = "builder.OpenElement(0, \"div\"); builder.AddAttribute(1, \"data-marker\", " + CSharpStringLiteral(marker) + "); builder.AddMultipleAttributes(2, AdditionalAttributes); builder.CloseElement();";
                        additional = "mergeProps";
                        tertiary = "props.AdditionalAttributes";
                        members = "[Parameter] public System.Collections.Generic.IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }";
                        usesProps = true;
                        importCount = 1;
                        break;
                    case 4:
                        body = "builder.OpenElement(0, \"section\"); builder.AddMultipleAttributes(1, AdditionalAttributes); builder.AddAttribute(2, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker) + "); builder.CloseElement();";
                        additional = "mergeProps";
                        tertiary = "props.AdditionalAttributes";
                        members = "[Parameter] public System.Collections.Generic.IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }";
                        usesProps = true;
                        importCount = 1;
                        break;
                    case 5:
                        body = "builder.OpenElement(0, \"article\"); builder.AddAttribute(1, \"data-before\", " + CSharpStringLiteral(marker + "-before") + "); builder.AddMultipleAttributes(2, AdditionalAttributes); builder.AddAttribute(3, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker + "-after") + "); builder.CloseElement();";
                        additional = "mergeProps";
                        tertiary = "props.AdditionalAttributes";
                        members = "[Parameter] public System.Collections.Generic.IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }";
                        usesProps = true;
                        importCount = 1;
                        break;
                    case 6:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddMultipleAttributes(1, new System.Collections.Generic.Dictionary<string, object> { [\"Title\"] = " + CSharpStringLiteral(marker) + ", [\"Count\"] = " + hostIndex.ToString(CultureInfo.InvariantCulture) + " }); builder.CloseComponent();";
                        additional = "heading";
                        tertiary = "count";
                        importCount = 1;
                        break;
                    default:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + "); builder.AddMultipleAttributes(2, AdditionalAttributes); builder.CloseComponent();";
                        additional = "mergeProps";
                        tertiary = "props.AdditionalAttributes";
                        members = "[Parameter] public System.Collections.Generic.IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }";
                        usesProps = true;
                        importCount = 2;
                        break;
                }

                Add(
                    cases,
                    "advanced_attribute_collection_" + host + "_" + shapeId,
                    body,
                    marker,
                    additional,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: usesProps,
                    importCount: importCount,
                    tertiaryExpectedFragment: tertiary);
            }
        }
    }

    private static void AddEventMetadataCases(List<DirectRenderCase> cases)
    {
        string[] eventNames = ["onblur", "onchange", "onclick", "onfocus", "oninput", "onkeydown", "onsubmit", "onkeyup"];
        for (var hostIndex = 0; hostIndex < eventNames.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var eventName = eventNames[hostIndex];
            var runtimeName = "on" + char.ToUpperInvariant(eventName[2]) + eventName[3..];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "event-metadata-" + host + "-" + shapeId;
                var handler = "HandleMetadata" + host + shapeId;
                var runtimeHandler = handler;
                string body;
                string additional;
                string? tertiary = null;
                string? unexpected = null;
                string members;

                switch (shape)
                {
                    case 0:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handler + ")); builder.AddNamedEvent(" + CSharpStringLiteral(eventName) + ", " + CSharpStringLiteral(marker) + "); builder.CloseElement();";
                        additional = runtimeHandler;
                        members = "private void " + handler + "() { }";
                        break;
                    case 1:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handler + ")); builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", false); builder.CloseElement();";
                        additional = runtimeHandler;
                        unexpected = "preventDefault";
                        members = "private void " + handler + "() { }";
                        break;
                    case 2:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handler + ")); builder.AddEventStopPropagationAttribute(2, " + CSharpStringLiteral(eventName) + ", false); builder.CloseElement();";
                        additional = runtimeHandler;
                        unexpected = "stopPropagation";
                        members = "private void " + handler + "() { }";
                        break;
                    case 3:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handler + ")); builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", true); builder.CloseElement();";
                        additional = runtimeHandler;
                        tertiary = "preventDefault";
                        members = "private void " + handler + "() { }";
                        break;
                    case 4:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handler + ")); builder.AddEventStopPropagationAttribute(2, " + CSharpStringLiteral(eventName) + ", true); builder.CloseElement();";
                        additional = runtimeHandler;
                        tertiary = "stopPropagation";
                        members = "private void " + handler + "() { }";
                        break;
                    case 5:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handler + ")); builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", true); builder.AddEventStopPropagationAttribute(3, " + CSharpStringLiteral(eventName) + ", true); builder.CloseElement();";
                        additional = "preventDefault";
                        tertiary = "stopPropagation";
                        members = "private void " + handler + "() { }";
                        break;
                    case 6:
                        var field = "boundMetadata" + host + shapeId;
                        body = "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"value\", " + CSharpStringLiteral(marker) + "); builder.AddAttribute(2, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.CreateBinder<string>(this, value => " + field + " = value, " + field + ")); builder.SetUpdatesAttributeName(\"value\"); builder.CloseElement();";
                        additional = "eventOrValue";
                        tertiary = marker;
                        members = "private string " + field + " = \"\";";
                        break;
                    default:
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", (System.Action)" + handler + "); builder.AddNamedEvent(" + CSharpStringLiteral(eventName) + ", " + CSharpStringLiteral(marker) + "); builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", true); builder.AddEventStopPropagationAttribute(3, " + CSharpStringLiteral(eventName) + ", true); builder.CloseElement();";
                        additional = runtimeHandler;
                        tertiary = "preventDefault";
                        members = "private void " + handler + "() { }";
                        break;
                }

                Add(
                    cases,
                    "advanced_event_metadata_" + host + "_" + shapeId,
                    body,
                    runtimeName,
                    additional,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    tertiaryExpectedFragment: tertiary,
                    unexpectedFragment: unexpected);
            }
        }
    }

    private static void AddComponentSlotCompositionCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["article", "code", "div", "em", "mark", "section", "span", "strong"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "slot-composition-" + host + "-" + shapeId;
                var first = "slotFirst" + host + shapeId;
                var second = "slotSecond" + host + shapeId;
                string body;
                string additional;
                string tertiary;
                string members = "";
                var usesFragment = false;
                var usesStaticVNode = false;
                var usesProps = false;

                switch (shape)
                {
                    case 0:
                        body = "RenderFragment " + first + " = child => { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker) + "); child.CloseElement(); }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"ChildContent\", " + first + "); builder.CloseComponent();";
                        additional = "default";
                        tertiary = "h(" + CSharpStringLiteral(tag);
                        break;
                    case 1:
                        body = "RenderFragment " + first + " = child => child.AddMarkupContent(0, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"Header\", " + first + "); builder.CloseComponent();";
                        additional = "header";
                        tertiary = "createStaticVNode";
                        usesStaticVNode = true;
                        break;
                    case 2:
                        body = "RenderFragment<string> " + first + " = value => child => child.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + value); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"ItemTemplate\", " + first + "); builder.CloseComponent();";
                        additional = "item";
                        tertiary = "value";
                        break;
                    case 3:
                        body = "RenderFragment " + first + " = child => child.AddContent(0, " + CSharpStringLiteral(marker + "-default") + "); RenderFragment " + second + " = child => child.AddContent(0, " + CSharpStringLiteral(marker + "-header") + "); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"ChildContent\", " + first + "); builder.AddComponentParameter(3, \"Header\", " + second + "); builder.CloseComponent();";
                        additional = "default";
                        tertiary = "header";
                        break;
                    case 4:
                        body = "RenderFragment " + first + " = child => { child.AddContent(0, " + CSharpStringLiteral(marker + "-a") + "); child.AddContent(1, " + CSharpStringLiteral(marker + "-b") + "); }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"ChildContent\", " + first + "); builder.CloseComponent();";
                        additional = "default";
                        tertiary = "Fragment";
                        usesFragment = true;
                        break;
                    case 5:
                        body = "RenderFragment<string> " + first + " = value => child => { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + value); child.CloseElement(); }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"ItemTemplate\", " + first + "); builder.CloseComponent();";
                        additional = "item";
                        tertiary = "h(" + CSharpStringLiteral(tag);
                        break;
                    case 6:
                        body = "RenderFragment " + first + " = child => { if (Visible) { child.AddContent(0, " + CSharpStringLiteral(marker + "-yes") + "); } else { child.AddContent(1, " + CSharpStringLiteral(marker + "-no") + "); } }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"ChildContent\", " + first + "); builder.CloseComponent();";
                        additional = "default";
                        tertiary = "props.Visible";
                        members = "[Parameter] public bool Visible { get; set; }";
                        usesProps = true;
                        break;
                    default:
                        body = "RenderFragment " + first + " = child => { foreach (var item in Items) { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + item); child.CloseElement(); } }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"Header\", " + first + "); builder.CloseComponent();";
                        additional = "header";
                        tertiary = "Array.from(props.Items ?? []";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        usesProps = true;
                        break;
                }

                Add(
                    cases,
                    "advanced_slot_composition_" + host + "_" + shapeId,
                    body,
                    marker,
                    additional,
                    usesFragment,
                    usesStaticVNode,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: usesProps,
                    importCount: 1,
                    tertiaryExpectedFragment: tertiary);
            }
        }
    }

    private static void AddComponentExpressionCases(List<DirectRenderCase> cases)
    {
        for (var hostIndex = 0; hostIndex < 8; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "component-expression-" + host + "-" + shapeId;
                string parameters;
                string expected;
                string additional;
                string members;

                switch (shape)
                {
                    case 0:
                        parameters = "builder.AddComponentParameter(1, \"Title\", Text + " + CSharpStringLiteral("-" + marker) + ");";
                        expected = "heading";
                        additional = "props.Text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        break;
                    case 1:
                        parameters = "builder.AddComponentParameter(1, \"Count\", Count * 2 + " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker) + ");";
                        expected = "count";
                        additional = "props.Count * 2";
                        members = "[Parameter] public int Count { get; set; }";
                        break;
                    case 2:
                        parameters = "builder.AddComponentParameter(1, \"Enabled\", Visible && Count > " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker) + ");";
                        expected = "enabled";
                        additional = "props.Visible";
                        members = "[Parameter] public bool Visible { get; set; } [Parameter] public int Count { get; set; }";
                        break;
                    case 3:
                        parameters = "builder.AddComponentParameter(1, \"Value\", Text ?? " + CSharpStringLiteral(marker) + ");";
                        expected = "modelValue";
                        additional = "props.Text";
                        members = "[Parameter] public string? Text { get; set; }";
                        break;
                    case 4:
                        parameters = "builder.AddComponentParameter(1, \"Title\", Visible ? " + CSharpStringLiteral(marker + "-yes") + " : " + CSharpStringLiteral(marker + "-no") + ");";
                        expected = "heading";
                        additional = "props.Visible";
                        members = "[Parameter] public bool Visible { get; set; }";
                        break;
                    case 5:
                        parameters = "builder.AddComponentParameter(1, \"Count\", Items.Length + " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker) + ");";
                        expected = "count";
                        additional = "props.Items.length";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        break;
                    case 6:
                        parameters = "builder.AddComponentParameter(1, \"Title\", Items[" + hostIndex.ToString(CultureInfo.InvariantCulture) + "] ?? " + CSharpStringLiteral(marker) + ");";
                        expected = "heading";
                        additional = "props.Items[" + hostIndex.ToString(CultureInfo.InvariantCulture) + "]";
                        members = "[Parameter] public string?[] Items { get; set; } = new string?[8];";
                        break;
                    default:
                        parameters = "builder.AddComponentParameter(1, \"Title\", $\"" + marker + ":{Count}\");";
                        expected = "heading";
                        additional = "props.Count";
                        members = "[Parameter] public int Count { get; set; }";
                        break;
                }

                Add(
                    cases,
                    "advanced_component_expression_" + host + "_" + shapeId,
                    "builder.OpenComponent<MatrixChild>(0); " + parameters + " builder.CloseComponent();",
                    expected,
                    additional,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: true,
                    importCount: 1,
                    tertiaryExpectedFragment: marker);
            }
        }
    }

    private static void AddMixedStaticDynamicCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["article", "code", "div", "em", "mark", "section", "span", "strong"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "mixed-static-" + host + "-" + shapeId;
                string body;
                string additional;
                string tertiary;
                string members = "";
                var usesFragment = false;
                var usesProps = false;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddMarkupContent(1, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); builder.AddContent(2, Text); builder.CloseElement();";
                        additional = "createStaticVNode";
                        tertiary = "props.Text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        usesProps = true;
                        break;
                    case 1:
                        body = "builder.AddMarkupContent(0, " + CSharpStringLiteral("<i>" + marker + "</i>") + "); builder.OpenElement(1, " + CSharpStringLiteral(tag) + "); builder.AddContent(2, Text); builder.CloseElement();";
                        additional = "createStaticVNode";
                        tertiary = "props.Text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        usesFragment = true;
                        usesProps = true;
                        break;
                    case 2:
                        var fragment = "mixedSlot" + host + shapeId;
                        body = "RenderFragment " + fragment + " = child => { child.AddMarkupContent(0, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); child.AddContent(1, Text); }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"ChildContent\", " + fragment + "); builder.CloseComponent();";
                        additional = "createStaticVNode";
                        tertiary = "default";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        usesFragment = true;
                        usesProps = true;
                        importCount = 1;
                        break;
                    case 3:
                        body = "if (Visible) { builder.AddMarkupContent(0, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); } else { builder.AddContent(1, " + CSharpStringLiteral(marker + "-text") + "); }";
                        additional = "createStaticVNode";
                        tertiary = "props.Visible";
                        members = "[Parameter] public bool Visible { get; set; }";
                        usesProps = true;
                        break;
                    case 4:
                        body = "foreach (var item in Items) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddMarkupContent(1, " + CSharpStringLiteral("<i>" + marker + "</i>") + "); builder.AddContent(2, item); builder.CloseElement(); }";
                        additional = "createStaticVNode";
                        tertiary = "Array.from(props.Items ?? []";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        usesProps = true;
                        break;
                    case 5:
                        body = "builder.OpenRegion(0); builder.AddMarkupContent(1, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); builder.OpenElement(2, " + CSharpStringLiteral(tag) + "); builder.AddContent(3, Text); builder.CloseElement(); builder.CloseRegion();";
                        additional = "createStaticVNode";
                        tertiary = "props.Text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        usesFragment = true;
                        usesProps = true;
                        break;
                    case 6:
                        body = "builder.AddMarkupContent(0, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"Title\", Text); builder.CloseComponent();";
                        additional = "createStaticVNode";
                        tertiary = "heading";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        usesFragment = true;
                        usesProps = true;
                        importCount = 1;
                        break;
                    default:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddAttribute(1, \"class\", " + CSharpStringLiteral(marker) + "); builder.AddMarkupContent(2, " + CSharpStringLiteral("<i>static</i>") + "); builder.AddContent(3, Text); builder.CloseElement();";
                        additional = "createStaticVNode";
                        tertiary = "props.Text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        usesProps = true;
                        break;
                }

                Add(
                    cases,
                    "advanced_mixed_static_" + host + "_" + shapeId,
                    body,
                    marker,
                    additional,
                    usesFragment,
                    usesStaticVNode: true,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: usesProps,
                    importCount: importCount,
                    tertiaryExpectedFragment: tertiary);
            }
        }
    }

    private static void AddReturnGuardCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["article", "code", "div", "em", "mark", "section", "span", "strong"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "return-guard-" + host + "-" + shapeId;
                var ignored = marker + "-ignored";
                string body;
                string? tertiary = null;
                string members;
                var usesProps = true;
                var importCount = 0;
                string? unexpected = null;

                switch (shape)
                {
                    case 0:
                        body = "if (!Visible) { return; } builder.AddContent(0, " + CSharpStringLiteral(marker) + ");";
                        members = "[Parameter] public bool Visible { get; set; }";
                        break;
                    case 1:
                        body = "if (Visible) { } else { return; } builder.AddContent(0, " + CSharpStringLiteral(marker) + ");";
                        members = "[Parameter] public bool Visible { get; set; }";
                        break;
                    case 2:
                        body = "if (!Visible) { return; } if (!Enabled) { return; } builder.AddContent(0, " + CSharpStringLiteral(marker) + ");";
                        tertiary = "props.Enabled";
                        members = "[Parameter] public bool Visible { get; set; } [Parameter] public bool Enabled { get; set; }";
                        break;
                    case 3:
                        body = "if (!Visible) { return; } builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddContent(1, " + CSharpStringLiteral(marker) + "); builder.CloseElement();";
                        tertiary = "h(" + CSharpStringLiteral(tag);
                        members = "[Parameter] public bool Visible { get; set; }";
                        break;
                    case 4:
                        body = "if (!Visible) { return; } builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + "); builder.CloseComponent();";
                        tertiary = "heading";
                        members = "[Parameter] public bool Visible { get; set; }";
                        importCount = 1;
                        break;
                    case 5:
                        body = "if (!Visible) { return; } foreach (var item in Items) { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item); }";
                        tertiary = "Array.from(props.Items ?? []";
                        members = "[Parameter] public bool Visible { get; set; } [Parameter] public string[] Items { get; set; } = [];";
                        break;
                    case 6:
                        body = "if (Visible) { builder.AddContent(0, " + CSharpStringLiteral(marker) + "); } else { return; }";
                        members = "[Parameter] public bool Visible { get; set; }";
                        break;
                    default:
                        body = "builder.AddContent(0, " + CSharpStringLiteral(marker) + "); return; builder.AddContent(1, " + CSharpStringLiteral(ignored) + ");";
                        members = "";
                        usesProps = false;
                        unexpected = CSharpStringLiteral(ignored);
                        break;
                }

                Add(
                    cases,
                    "advanced_return_guard_" + host + "_" + shapeId,
                    body,
                    marker,
                    usesProps ? "props.Visible" : CSharpStringLiteral(marker),
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: usesProps,
                    importCount: importCount,
                    tertiaryExpectedFragment: tertiary,
                    unexpectedFragment: unexpected);
            }
        }
    }

    private static void AddBuilderMutationCases(List<DirectRenderCase> cases)
    {
        string[] events = ["onblur", "onchange", "onclick", "onfocus", "oninput", "onkeydown", "onsubmit", "onkeyup"];
        for (var hostIndex = 0; hostIndex < events.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var eventName = events[hostIndex];
            var runtimeName = "on" + char.ToUpperInvariant(eventName[2]) + eventName[3..];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "builder-mutation-" + host + "-" + shapeId;
                var before = "before-" + marker;
                var after = "after-" + marker;
                string body;
                string expected;
                string additional;
                string? tertiary = null;
                string? unexpected = null;
                string members = "";
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "builder.OpenElement(0, \"div\"); builder.AddAttribute(1, \"class\", " + CSharpStringLiteral(before) + "); builder.SetAttributeValue(2, " + CSharpStringLiteral(after) + "); builder.CloseElement();";
                        expected = after;
                        additional = "class";
                        unexpected = CSharpStringLiteral(before);
                        break;
                    case 1:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(before) + "); builder.SetAttributeValue(2, " + CSharpStringLiteral(after) + "); builder.CloseComponent();";
                        expected = after;
                        additional = "heading";
                        unexpected = CSharpStringLiteral(before);
                        importCount = 1;
                        break;
                    case 2:
                        body = "builder.AddContent(0, " + CSharpStringLiteral(before) + "); builder.Clear(); builder.AddContent(1, " + CSharpStringLiteral(after) + ");";
                        expected = after;
                        additional = CSharpStringLiteral(after);
                        unexpected = CSharpStringLiteral(before);
                        break;
                    case 3:
                        body = "builder.AddMarkupContent(0, " + CSharpStringLiteral("<b>" + before + "</b>") + "); builder.Clear(); builder.OpenElement(1, \"span\"); builder.AddContent(2, " + CSharpStringLiteral(after) + "); builder.CloseElement();";
                        expected = after;
                        additional = "h(\"span\"";
                        unexpected = before;
                        break;
                    case 4:
                        body = "builder.Dispose(); builder.AddContent(0, " + CSharpStringLiteral(marker) + "); builder.Dispose();";
                        expected = marker;
                        additional = CSharpStringLiteral(marker);
                        break;
                    case 5:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + "); builder.AddComponentRenderMode(RenderMode(null!)); builder.CloseComponent();";
                        expected = marker;
                        additional = "heading";
                        members = "private static IComponentRenderMode RenderMode(IComponentRenderMode mode) => mode;";
                        importCount = 1;
                        break;
                    case 6:
                        var handler = "HandleMutation" + host + shapeId;
                        var runtimeHandler = handler;
                        body = "builder.OpenElement(0, \"button\"); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handler + ")); builder.AddNamedEvent(" + CSharpStringLiteral(eventName) + ", " + CSharpStringLiteral(marker) + "); builder.CloseElement();";
                        expected = runtimeName;
                        additional = runtimeHandler;
                        members = "private void " + handler + "() { }";
                        break;
                    default:
                        body = "builder.OpenElement(0, \"form\"); builder.AddAttribute(1, \"class\", " + CSharpStringLiteral(before) + "); builder.SetAttributeValue(2, " + CSharpStringLiteral(after) + "); builder.SetKey(" + CSharpStringLiteral(marker + "-key") + "); builder.AddNamedEvent(" + CSharpStringLiteral(eventName) + ", " + CSharpStringLiteral(marker) + "); builder.CloseElement();";
                        expected = after;
                        additional = "key";
                        tertiary = marker + "-key";
                        unexpected = CSharpStringLiteral(before);
                        break;
                }

                Add(
                    cases,
                    "advanced_builder_mutation_" + host + "_" + shapeId,
                    body,
                    expected,
                    additional,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    importCount: importCount,
                    tertiaryExpectedFragment: tertiary,
                    unexpectedFragment: unexpected);
            }
        }
    }
}
