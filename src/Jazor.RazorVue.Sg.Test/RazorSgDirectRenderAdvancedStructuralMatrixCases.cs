using System.Globalization;

namespace Jazor.RazorVue.Sg.Test;

internal static partial class DirectRenderCaseCatalog
{
    private static void AddAdvancedCases(List<DirectRenderCase> cases)
    {
        AddRegionCompositionCases(cases);
        AddRootSequenceCases(cases);
        AddLocalValueCases(cases);
        AddHelperMethodCases(cases);
        AddInlineRenderFragmentCases(cases);
        AddGenericRenderFragmentCases(cases);
        AddRenderFragmentMethodGroupCase(cases);
        AddConditionalElementAttributeCases(cases);
        AddConditionalComponentParameterCases(cases);
        AddNestedControlFlowCases(cases);
        AddAttributeCollectionCases(cases);
        AddEventMetadataCases(cases);
        AddComponentSlotCompositionCases(cases);
        AddComponentExpressionCases(cases);
        AddMixedStaticDynamicCases(cases);
        AddReturnGuardCases(cases);
        AddBuilderMutationCases(cases);
    }

    private static void AddRegionCompositionCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["article", "aside", "div", "main", "nav", "section", "span", "strong"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var marker = "region-" + host + "-" + shape.ToString("D2", CultureInfo.InvariantCulture);
                string body;
                string? additional;
                string members = "";
                var usesFragment = false;
                var usesStaticVNode = false;
                var usesProps = false;

                switch (shape)
                {
                    case 0:
                        body = "builder.OpenRegion(0); builder.AddContent(1, " + CSharpStringLiteral(marker) + "); builder.CloseRegion();";
                        additional = null;
                        break;
                    case 1:
                        body = "builder.OpenRegion(0); builder.AddContent(1, " + CSharpStringLiteral(marker + "-a") + "); builder.AddContent(2, " + CSharpStringLiteral(marker + "-b") + "); builder.CloseRegion();";
                        additional = CSharpStringLiteral(marker + "-b");
                        usesFragment = true;
                        break;
                    case 2:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.OpenRegion(1); builder.AddContent(2, " + CSharpStringLiteral(marker + "-a") + "); builder.AddContent(3, " + CSharpStringLiteral(marker + "-b") + "); builder.CloseRegion(); builder.CloseElement();";
                        additional = "h(" + CSharpStringLiteral(tag);
                        usesFragment = true;
                        break;
                    case 3:
                        body = "builder.OpenRegion(0); builder.OpenElement(1, " + CSharpStringLiteral(tag) + "); builder.AddContent(2, " + CSharpStringLiteral(marker) + "); builder.CloseElement(); builder.CloseRegion();";
                        additional = "h(" + CSharpStringLiteral(tag);
                        break;
                    case 4:
                        body = "builder.OpenRegion(0); builder.OpenRegion(1); builder.AddContent(2, " + CSharpStringLiteral(marker + "-inner-a") + "); builder.AddContent(3, " + CSharpStringLiteral(marker + "-inner-b") + "); builder.CloseRegion(); builder.CloseRegion();";
                        additional = CSharpStringLiteral(marker + "-inner-b");
                        usesFragment = true;
                        break;
                    case 5:
                        body = "builder.OpenRegion(0); if (Visible) { builder.AddContent(1, " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddContent(2, " + CSharpStringLiteral(marker + "-no") + "); } builder.CloseRegion();";
                        additional = "props.visible";
                        members = "[Parameter] public bool Visible { get; set; }";
                        usesProps = true;
                        break;
                    case 6:
                        body = "builder.OpenRegion(0); foreach (var item in Items) { builder.OpenElement(1, " + CSharpStringLiteral(tag) + "); builder.AddContent(2, " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseElement(); } builder.CloseRegion();";
                        additional = "Array.from(props.items ?? []";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        usesProps = true;
                        break;
                    default:
                        body = "builder.OpenRegion(0); builder.AddMarkupContent(1, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); builder.AddContent(2, Text); builder.CloseRegion();";
                        additional = "props.text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        usesFragment = true;
                        usesStaticVNode = true;
                        usesProps = true;
                        break;
                }

                Add(
                    cases,
                    "advanced_region_" + host + "_" + shape.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    marker,
                    additional,
                    usesFragment,
                    usesStaticVNode,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: usesProps);
            }
        }
    }

    private static void AddRootSequenceCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["article", "code", "div", "em", "section", "small", "span", "strong"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var marker = "roots-" + host + "-" + shape.ToString("D2", CultureInfo.InvariantCulture);
                string body;
                string? additional;
                string members = "";
                var usesStaticVNode = false;
                var usesProps = false;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "builder.AddContent(0, " + CSharpStringLiteral(marker + "-a") + "); builder.AddContent(1, " + CSharpStringLiteral(marker + "-b") + ");";
                        additional = CSharpStringLiteral(marker + "-b");
                        break;
                    case 1:
                        body = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddContent(1, " + CSharpStringLiteral(marker) + "); builder.CloseElement(); builder.AddContent(2, " + CSharpStringLiteral(marker + "-tail") + ");";
                        additional = CSharpStringLiteral(marker + "-tail");
                        break;
                    case 2:
                        body = "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + "); builder.CloseComponent(); builder.OpenElement(2, " + CSharpStringLiteral(tag) + "); builder.CloseElement();";
                        additional = "heading";
                        importCount = 1;
                        break;
                    case 3:
                        body = "builder.AddMarkupContent(0, " + CSharpStringLiteral("<i>" + marker + "</i>") + "); builder.AddContent(1, " + CSharpStringLiteral(marker + "-text") + ");";
                        additional = "createStaticVNode";
                        usesStaticVNode = true;
                        break;
                    case 4:
                        body = "builder.OpenRegion(0); builder.AddContent(1, " + CSharpStringLiteral(marker) + "); builder.CloseRegion(); builder.OpenElement(2, " + CSharpStringLiteral(tag) + "); builder.CloseElement();";
                        additional = "h(" + CSharpStringLiteral(tag);
                        break;
                    case 5:
                        body = "if (Visible) { builder.AddContent(0, " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddContent(1, " + CSharpStringLiteral(marker + "-no") + "); } builder.OpenElement(2, " + CSharpStringLiteral(tag) + "); builder.CloseElement();";
                        additional = "props.visible";
                        members = "[Parameter] public bool Visible { get; set; }";
                        usesProps = true;
                        break;
                    case 6:
                        body = "foreach (var item in Items) { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item); } builder.AddContent(1, " + CSharpStringLiteral(marker + "-tail") + ");";
                        additional = "Array.from(props.items ?? []";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        usesProps = true;
                        break;
                    default:
                        body = "builder.AddMarkupContent(0, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker + "-child") + "); builder.CloseComponent(); builder.OpenElement(3, " + CSharpStringLiteral(tag) + "); builder.CloseElement();";
                        additional = "heading";
                        usesStaticVNode = true;
                        importCount = 1;
                        break;
                }

                Add(
                    cases,
                    "advanced_roots_" + host + "_" + shape.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    marker,
                    additional,
                    usesFragment: true,
                    usesStaticVNode: usesStaticVNode,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: usesProps,
                    importCount: importCount);
            }
        }
    }

    private static void AddLocalValueCases(List<DirectRenderCase> cases)
    {
        for (var hostIndex = 0; hostIndex < 8; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            for (var shape = 0; shape < 8; shape++)
            {
                var marker = "local-" + host + "-" + shape.ToString("D2", CultureInfo.InvariantCulture);
                var localName = "localValue" + host + shape.ToString("D2", CultureInfo.InvariantCulture);
                string body;
                string additional;
                string members = "";
                var usesStaticVNode = false;
                var usesProps = false;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "var " + localName + " = " + CSharpStringLiteral(marker) + "; builder.AddContent(0, " + localName + ");";
                        additional = localName;
                        break;
                    case 1:
                        body = "var " + localName + " = Count + " + hostIndex.ToString(CultureInfo.InvariantCulture) + "; builder.AddContent(0, " + localName + ");";
                        additional = "props.count";
                        members = "[Parameter] public int Count { get; set; }";
                        usesProps = true;
                        break;
                    case 2:
                        body = "var " + localName + " = !Visible; if (" + localName + ") { builder.AddContent(0, " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddContent(1, " + CSharpStringLiteral(marker + "-no") + "); }";
                        additional = "props.visible";
                        members = "[Parameter] public bool Visible { get; set; }";
                        usesProps = true;
                        break;
                    case 3:
                        body = "var " + localName + " = Items; foreach (var item in " + localName + ") { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item); }";
                        additional = "Array.from(" + localName + " ?? []";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        usesProps = true;
                        break;
                    case 4:
                        body = "var " + localName + " = Text ?? " + CSharpStringLiteral(marker) + "; builder.OpenElement(0, \"span\"); builder.AddContent(1, " + localName + "); builder.CloseElement();";
                        additional = "props.text";
                        members = "[Parameter] public string? Text { get; set; }";
                        usesProps = true;
                        break;
                    case 5:
                        body = "var " + localName + " = Items.Length + " + hostIndex.ToString(CultureInfo.InvariantCulture) + "; builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Count\", " + localName + "); builder.CloseComponent();";
                        additional = "props.items.length";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        usesProps = true;
                        importCount = 1;
                        break;
                    case 6:
                        body = "var " + localName + " = (MarkupString)" + CSharpStringLiteral("<b>" + marker + "</b>") + "; builder.AddContent(0, " + localName + ");";
                        additional = "createStaticVNode";
                        usesStaticVNode = true;
                        break;
                    default:
                        body = "var " + localName + " = Visible ? " + CSharpStringLiteral("on-" + marker) + " : " + CSharpStringLiteral("off-" + marker) + "; builder.OpenElement(0, \"div\"); builder.AddAttribute(1, \"class\", " + localName + "); builder.CloseElement();";
                        additional = "props.visible";
                        members = "[Parameter] public bool Visible { get; set; }";
                        usesProps = true;
                        break;
                }

                Add(
                    cases,
                    "advanced_local_" + host + "_" + shape.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    "const " + localName,
                    additional,
                    usesFragment: false,
                    usesStaticVNode: usesStaticVNode,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: usesProps,
                    importCount: importCount);
            }
        }
    }

    private static void AddHelperMethodCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["article", "code", "div", "em", "mark", "section", "span", "strong"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var suffix = host + shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "helper-" + host + "-" + shape.ToString("D2", CultureInfo.InvariantCulture);
                var method = "RenderHelper" + suffix;
                string body;
                string additional;
                string members;
                var usesFragment = false;
                var usesStaticVNode = false;
                var usesProps = false;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = method + "(builder, Text + " + CSharpStringLiteral("-" + marker) + ");";
                        members = "[Parameter] public string Text { get; set; } = \"\"; private static void " + method + "(RenderTreeBuilder builder, string value) { builder.AddContent(0, value); }";
                        additional = "props.text";
                        usesProps = true;
                        break;
                    case 1:
                        body = method + "(builder, " + CSharpStringLiteral(marker) + ");";
                        members = "private static void " + method + "(RenderTreeBuilder builder, string value) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddContent(1, value); builder.CloseElement(); }";
                        additional = "h(" + CSharpStringLiteral(tag);
                        break;
                    case 2:
                        body = method + "(builder, " + CSharpStringLiteral("<i>" + marker + "</i>") + ");";
                        members = "private static void " + method + "(RenderTreeBuilder builder, string value) { builder.AddMarkupContent(0, value); }";
                        additional = "createStaticVNode";
                        usesStaticVNode = true;
                        break;
                    case 3:
                        body = method + "(builder, Visible, " + CSharpStringLiteral(marker + "-yes") + ", " + CSharpStringLiteral(marker + "-no") + ");";
                        members = "[Parameter] public bool Visible { get; set; } private static void " + method + "(RenderTreeBuilder builder, bool visible, string yes, string no) { if (visible) { builder.AddContent(0, yes); } else { builder.AddContent(1, no); } }";
                        additional = "props.visible";
                        usesProps = true;
                        break;
                    case 4:
                        body = method + "(builder, Items, " + CSharpStringLiteral(marker + ":") + ");";
                        members = "[Parameter] public string[] Items { get; set; } = []; private static void " + method + "(RenderTreeBuilder builder, string[] items, string prefix) { foreach (var item in items) { builder.AddContent(0, prefix + item); } }";
                        additional = "Array.from(props.items ?? []";
                        usesProps = true;
                        break;
                    case 5:
                        body = method + "(builder, " + CSharpStringLiteral(marker) + ");";
                        members = "private static void " + method + "(RenderTreeBuilder builder, string value) { builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", value); builder.CloseComponent(); }";
                        additional = "heading";
                        importCount = 1;
                        break;
                    case 6:
                        body = method + "(builder, " + CSharpStringLiteral(marker + "-a") + ", " + CSharpStringLiteral(marker + "-b") + ");";
                        members = "private static void " + method + "(RenderTreeBuilder builder, string first, string second) { builder.OpenRegion(0); builder.AddContent(1, first); builder.AddContent(2, second); builder.CloseRegion(); }";
                        additional = CSharpStringLiteral(marker + "-b");
                        usesFragment = true;
                        break;
                    default:
                        body = method + "(builder, Visible, " + CSharpStringLiteral(marker) + ");";
                        members = "[Parameter] public bool Visible { get; set; } private void " + method + "(RenderTreeBuilder builder, bool visible, string value) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); if (visible) { builder.AddContent(1, value); } else { builder.AddContent(2, value + \"-hidden\"); } builder.CloseElement(); }";
                        additional = "props.visible";
                        usesProps = true;
                        break;
                }

                Add(
                    cases,
                    "advanced_helper_" + host + "_" + shape.ToString("D2", CultureInfo.InvariantCulture),
                    body,
                    marker,
                    additional,
                    usesFragment,
                    usesStaticVNode,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: usesProps,
                    importCount: importCount);
            }
        }
    }

    private static void AddInlineRenderFragmentCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["article", "code", "div", "em", "mark", "section", "span", "strong"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "fragment-" + host + "-" + shapeId;
                var fragment = "fragment" + host + shapeId;
                string body;
                string? additional;
                string members = "";
                var usesFragment = false;
                var usesStaticVNode = false;
                var usesProps = false;

                switch (shape)
                {
                    case 0:
                        body = "RenderFragment " + fragment + " = child => child.AddContent(0, " + CSharpStringLiteral(marker) + "); builder.AddContent(1, " + fragment + ");";
                        additional = null;
                        break;
                    case 1:
                        body = "RenderFragment " + fragment + " = child => { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker) + "); child.CloseElement(); }; builder.AddContent(2, " + fragment + ");";
                        additional = "h(" + CSharpStringLiteral(tag);
                        break;
                    case 2:
                        body = "RenderFragment " + fragment + " = child => { child.AddContent(0, " + CSharpStringLiteral(marker + "-a") + "); child.AddContent(1, " + CSharpStringLiteral(marker + "-b") + "); }; builder.AddContent(2, " + fragment + ");";
                        additional = CSharpStringLiteral(marker + "-b");
                        usesFragment = true;
                        break;
                    case 3:
                        body = "RenderFragment " + fragment + " = child => { child.OpenRegion(0); child.AddContent(1, " + CSharpStringLiteral(marker + "-a") + "); child.AddContent(2, " + CSharpStringLiteral(marker + "-b") + "); child.CloseRegion(); }; builder.AddContent(3, " + fragment + ");";
                        additional = CSharpStringLiteral(marker + "-b");
                        usesFragment = true;
                        break;
                    case 4:
                        body = "RenderFragment " + fragment + " = child => { if (Visible) { child.AddContent(0, " + CSharpStringLiteral(marker + "-yes") + "); } else { child.AddContent(1, " + CSharpStringLiteral(marker + "-no") + "); } }; builder.AddContent(2, " + fragment + ");";
                        additional = "props.visible";
                        members = "[Parameter] public bool Visible { get; set; }";
                        usesProps = true;
                        break;
                    case 5:
                        body = "RenderFragment " + fragment + " = child => { foreach (var item in Items) { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + item); child.CloseElement(); } }; builder.AddContent(2, " + fragment + ");";
                        additional = "Array.from(props.items ?? []";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        usesProps = true;
                        break;
                    case 6:
                        body = "RenderFragment " + fragment + " = child => child.AddMarkupContent(0, " + CSharpStringLiteral("<b>" + marker + "</b>") + "); builder.AddContent(1, " + fragment + ");";
                        additional = "createStaticVNode";
                        usesStaticVNode = true;
                        break;
                    default:
                        body = "RenderFragment " + fragment + " = child => { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker) + "); child.CloseElement(); }; " + fragment + "(builder);";
                        additional = "h(" + CSharpStringLiteral(tag);
                        break;
                }

                Add(
                    cases,
                    "advanced_fragment_" + host + "_" + shapeId,
                    body,
                    marker,
                    additional,
                    usesFragment,
                    usesStaticVNode,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: usesProps);
            }
        }
    }

    private static void AddGenericRenderFragmentCases(List<DirectRenderCase> cases)
    {
        string[] tags = ["article", "code", "div", "em", "mark", "section", "span", "strong"];
        for (var hostIndex = 0; hostIndex < tags.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var tag = tags[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "generic-fragment-" + host + "-" + shapeId;
                var fragment = "template" + host + shapeId;
                string body;
                string additional;
                string members;
                var usesFragment = false;
                var importCount = 0;

                switch (shape)
                {
                    case 0:
                        body = "RenderFragment<string> " + fragment + " = value => child => child.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + value); builder.AddContent(1, " + fragment + ", Text);";
                        additional = "props.text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        break;
                    case 1:
                        body = "RenderFragment<string> " + fragment + " = value => child => { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, value); child.CloseElement(); }; builder.AddContent(2, " + fragment + ", Text + " + CSharpStringLiteral("-" + marker) + ");";
                        additional = "props.text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        break;
                    case 2:
                        body = "RenderFragment<string> " + fragment + " = value => child => { child.AddContent(0, " + CSharpStringLiteral(marker) + "); child.AddContent(1, value); }; builder.AddContent(2, " + fragment + ", Text);";
                        additional = "props.text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        usesFragment = true;
                        break;
                    case 3:
                        body = "RenderFragment<int> " + fragment + " = value => child => child.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + (value * 2)); builder.AddContent(1, " + fragment + ", Count + " + hostIndex.ToString(CultureInfo.InvariantCulture) + ");";
                        additional = "props.count";
                        members = "[Parameter] public int Count { get; set; }";
                        break;
                    case 4:
                        body = "RenderFragment<bool> " + fragment + " = value => child => { if (value) { child.AddContent(0, " + CSharpStringLiteral(marker + "-yes") + "); } else { child.AddContent(1, " + CSharpStringLiteral(marker + "-no") + "); } }; builder.AddContent(2, " + fragment + ", Visible);";
                        additional = "props.visible";
                        members = "[Parameter] public bool Visible { get; set; }";
                        break;
                    case 5:
                        body = "RenderFragment<string> " + fragment + " = value => child => { child.OpenRegion(0); child.AddContent(1, " + CSharpStringLiteral(marker) + "); child.AddContent(2, value); child.CloseRegion(); }; builder.AddContent(3, " + fragment + ", Text);";
                        additional = "props.text";
                        members = "[Parameter] public string Text { get; set; } = \"\";";
                        usesFragment = true;
                        break;
                    case 6:
                        body = "RenderFragment<string[]> " + fragment + " = values => child => { foreach (var value in values) { child.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + value); } }; builder.AddContent(1, " + fragment + ", Items);";
                        additional = "Array.from(values ?? []";
                        members = "[Parameter] public string[] Items { get; set; } = [];";
                        break;
                    default:
                        body = "RenderFragment<string> " + fragment + " = value => child => child.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + value); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"ItemTemplate\", " + fragment + "); builder.CloseComponent();";
                        additional = "item";
                        members = "";
                        importCount = 1;
                        break;
                }

                Add(
                    cases,
                    "advanced_generic_fragment_" + host + "_" + shapeId,
                    body,
                    marker,
                    additional,
                    usesFragment,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Advanced,
                    members: members,
                    usesProps: shape != 7,
                    importCount: importCount);
            }
        }
    }

    private static void AddConditionalElementAttributeCases(List<DirectRenderCase> cases)
    {
        string[] attributes = ["aria-label", "class", "data-state", "dir", "lang", "role", "tabindex", "title"];
        for (var hostIndex = 0; hostIndex < attributes.Length; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            var attribute = attributes[hostIndex];
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "conditional-attribute-" + host + "-" + shapeId;
                string conditional;
                string additional = FormatObjectPropertyKey(attribute);
                var members = "[Parameter] public bool Visible { get; set; }";

                switch (shape)
                {
                    case 0:
                        conditional = "if (Visible) { builder.AddAttribute(1, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddAttribute(2, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker + "-no") + "); }";
                        break;
                    case 1:
                        conditional = "if (Visible) { builder.AddAttribute(1, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker) + "); } else { builder.AddAttribute(2, \"data-fallback\", " + CSharpStringLiteral(marker + "-fallback") + "); }";
                        break;
                    case 2:
                        conditional = "if (Visible) { builder.AddAttribute(1, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker) + "); }";
                        break;
                    case 3:
                        conditional = "if (Visible) { } else { builder.AddAttribute(1, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker) + "); }";
                        break;
                    case 4:
                        conditional = "if (Visible) { builder.AddAttribute(1, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker + "-yes") + "); builder.AddAttribute(2, \"data-branch\", \"yes\"); } else { builder.AddAttribute(3, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker + "-no") + "); builder.AddAttribute(4, \"data-branch\", \"no\"); }";
                        break;
                    case 5:
                        conditional = "if (Visible) { builder.AddAttribute(1, \"hidden\"); } else { builder.AddAttribute(2, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker) + "); }";
                        additional = "hidden";
                        break;
                    case 6:
                        conditional = "if (Visible) { builder.AddAttribute(1, " + CSharpStringLiteral(attribute) + ", Text); } else { builder.AddAttribute(2, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker) + "); }";
                        members += " [Parameter] public string Text { get; set; } = \"\";";
                        break;
                    default:
                        conditional = "if (Visible) { builder.AddAttribute(1, " + CSharpStringLiteral(attribute) + ", Count + " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); } else { builder.AddAttribute(2, " + CSharpStringLiteral(attribute) + ", " + CSharpStringLiteral(marker) + "); }";
                        members += " [Parameter] public int Count { get; set; }";
                        break;
                }

                Add(
                    cases,
                    "advanced_conditional_element_" + host + "_" + shapeId,
                    "builder.OpenElement(0, \"div\"); " + conditional + " builder.CloseElement();",
                    "props.visible",
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

    private static void AddConditionalComponentParameterCases(List<DirectRenderCase> cases)
    {
        for (var hostIndex = 0; hostIndex < 8; hostIndex++)
        {
            var host = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
            for (var shape = 0; shape < 8; shape++)
            {
                var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "conditional-component-" + host + "-" + shapeId;
                string conditional;
                string additional;

                switch (shape)
                {
                    case 0:
                        conditional = "if (Visible) { builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker + "-no") + "); }";
                        additional = "heading";
                        break;
                    case 1:
                        conditional = "if (Visible) { builder.AddComponentParameter(1, \"Count\", " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker) + "); }";
                        additional = "count";
                        break;
                    case 2:
                        conditional = "if (Visible) { } else { builder.AddComponentParameter(1, \"Enabled\", true); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker) + "); }";
                        additional = "enabled";
                        break;
                    case 3:
                        conditional = "if (Visible) { builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + "); builder.AddComponentParameter(2, \"Count\", " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); } else { builder.AddComponentParameter(3, \"Title\", " + CSharpStringLiteral(marker + "-fallback") + "); builder.AddComponentParameter(4, \"Enabled\", false); }";
                        additional = "count";
                        break;
                    case 4:
                        conditional = "if (Visible) { builder.AddComponentParameter(1, \"Value\", " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddComponentParameter(2, \"Value\", " + CSharpStringLiteral(marker + "-no") + "); }";
                        additional = "modelValue";
                        break;
                    case 5:
                        conditional = "if (Visible) { builder.AddAttribute(1, \"Title\", " + CSharpStringLiteral(marker) + "); } else { builder.AddAttribute(2, \"Title\", " + CSharpStringLiteral(marker + "-fallback") + "); }";
                        additional = "heading";
                        break;
                    case 6:
                        conditional = "if (Visible) { builder.AddAttribute(1, \"Title\", " + CSharpStringLiteral(marker) + "); } else { builder.AddComponentParameter(2, \"Count\", " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); }";
                        additional = "heading";
                        break;
                    default:
                        conditional = "if (Visible) { builder.AddAttribute(1, \"Enabled\"); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddComponentParameter(3, \"Enabled\", false); builder.AddComponentParameter(4, \"Title\", " + CSharpStringLiteral(marker + "-no") + "); }";
                        additional = "enabled";
                        break;
                }

                Add(
                    cases,
                    "advanced_conditional_component_" + host + "_" + shapeId,
                    "builder.OpenComponent<MatrixChild>(0); " + conditional + " builder.CloseComponent();",
                    "props.visible",
                    additional,
                    usesFragment: false,
                    usesStaticVNode: false,
                    group: DirectRenderCaseGroup.Advanced,
                    members: "[Parameter] public bool Visible { get; set; }",
                    usesProps: true,
                    importCount: 2,
                    tertiaryExpectedFragment: marker);
            }
        }
    }
}
