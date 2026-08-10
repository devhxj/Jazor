using System.Globalization;

namespace Jazor.RazorVue.Sg.Test;

internal static partial class DirectRenderCaseCatalog
{
    private static void AddCoverageCompositionCompilerCases(List<DirectRenderCase> cases)
    {
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.NestedElement, "nested_element", CreateNestedElementCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.RepeatedComponentImport, "repeated_component_import", CreateRepeatedComponentImportCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.HelperComposition, "helper_composition", CreateHelperCompositionCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.FragmentComposition, "fragment_composition", CreateFragmentCompositionCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.LocalPrelude, "local_prelude", CreateLocalPreludeCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ConditionalComposition, "conditional_composition", CreateConditionalCompositionCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ForeachCollection, "foreach_collection", CreateForeachCollectionCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.DescriptorMapping, "descriptor_mapping", CreateDescriptorMappingCase);
    }

    private static CoverageCaseSpec CreateNestedElementCase(int shape, int hostIndex, string marker, string tag)
        => shape switch
        {
            0 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.OpenElement(1, \"span\"); builder.AddContent(2, " + CSharpStringLiteral(marker) + "); builder.CloseElement(); builder.CloseElement();",
                "h(" + CSharpStringLiteral(tag))
            {
                AdditionalExpectedFragment = "h(\"span\"",
                TertiaryExpectedFragment = marker
            },
            1 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.OpenElement(1, \"div\"); builder.OpenElement(2, \"strong\"); builder.AddContent(3, " + CSharpStringLiteral(marker) + "); builder.CloseElement(); builder.CloseElement(); builder.CloseElement();",
                "h(" + CSharpStringLiteral(tag))
            {
                AdditionalExpectedFragment = "h(\"div\"",
                TertiaryExpectedFragment = "h(\"strong\""
            },
            2 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.OpenElement(1, \"span\"); builder.AddContent(2, " + CSharpStringLiteral(marker + "-first") + "); builder.CloseElement(); builder.OpenElement(3, \"strong\"); builder.AddContent(4, " + CSharpStringLiteral(marker + "-second") + "); builder.CloseElement(); builder.CloseElement();",
                marker + "-first")
            {
                AdditionalExpectedFragment = marker + "-second",
                TertiaryExpectedFragment = "h(" + CSharpStringLiteral(tag)
            },
            3 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker) + "); builder.CloseComponent(); builder.CloseElement();",
                "h(" + CSharpStringLiteral(tag))
            {
                AdditionalExpectedFragment = "heading",
                TertiaryExpectedFragment = marker,
                ImportCount = 1
            },
            4 => new(
                "RenderFragment fragment" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + " = child => { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker) + "); child.CloseElement(); }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"ChildContent\", fragment" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + "); builder.CloseComponent();",
                "default")
            {
                AdditionalExpectedFragment = "h(" + CSharpStringLiteral(tag),
                TertiaryExpectedFragment = marker,
                ImportCount = 1
            },
            5 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddMarkupContent(1, " + CSharpStringLiteral("<strong>" + marker + "</strong>") + "); builder.CloseElement();",
                "h(" + CSharpStringLiteral(tag))
            {
                AdditionalExpectedFragment = "createStaticVNode",
                TertiaryExpectedFragment = marker,
                UsesStaticVNode = true
            },
            6 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.OpenRegion(1); builder.AddContent(2, " + CSharpStringLiteral(marker + "-first") + "); builder.AddContent(3, " + CSharpStringLiteral(marker + "-second") + "); builder.CloseRegion(); builder.CloseElement();",
                "Fragment")
            {
                AdditionalExpectedFragment = marker + "-first",
                TertiaryExpectedFragment = marker + "-second",
                UsesFragment = true
            },
            _ => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); foreach (var item in Items) { builder.OpenElement(1, \"span\"); builder.AddContent(2, " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseElement(); } builder.CloseElement();",
                "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = "h(" + CSharpStringLiteral(tag),
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            }
        };

    private static CoverageCaseSpec CreateRepeatedComponentImportCase(int shape, int hostIndex, string marker, string tag)
    {
        var helper = "RenderRepeatedChild" + shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var fragment = "repeatedFragment" + shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        return shape switch
        {
            0 => new(RepeatedChild(marker + "-first", 0) + RepeatedChild(marker + "-second", 3), "heading")
            {
                AdditionalExpectedFragment = marker + "-first",
                TertiaryExpectedFragment = marker + "-second",
                UsesFragment = true,
                ImportCount = 1
            },
            1 => new(
                "if (Visible) { " + RepeatedChild(marker + "-visible", 0) + " } else { " + RepeatedChild(marker + "-hidden", 3) + " }",
                "props.Visible")
            {
                AdditionalExpectedFragment = "heading",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            2 => new(
                "foreach (var item in Items) { builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseComponent(); }",
                "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = "heading",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true,
                ImportCount = 1
            },
            3 => new(
                "RenderFragment " + fragment + " = child => { child.OpenComponent<MatrixChild>(0); child.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + "-fragment") + "); child.CloseComponent(); }; builder.AddContent(2, " + fragment + "); " + RepeatedChild(marker + "-root", 3),
                marker + "-fragment")
            {
                AdditionalExpectedFragment = marker + "-root",
                TertiaryExpectedFragment = "heading",
                UsesFragment = true,
                ImportCount = 1
            },
            4 => new(
                helper + "(builder, " + CSharpStringLiteral(marker + "-first") + "); " + helper + "(builder, " + CSharpStringLiteral(marker + "-second") + ");",
                marker + "-first")
            {
                AdditionalExpectedFragment = marker + "-second",
                TertiaryExpectedFragment = "heading",
                Members = "private static void " + helper + "(RenderTreeBuilder target, string value) { target.OpenComponent<MatrixChild>(0); target.AddComponentParameter(1, \"Title\", value); target.CloseComponent(); }",
                UsesFragment = true,
                ImportCount = 1
            },
            5 => new(
                "RenderFragment " + fragment + " = child => { child.OpenComponent<MatrixChild>(0); child.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + "-nested") + "); child.CloseComponent(); }; builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, \"Title\", " + CSharpStringLiteral(marker + "-parent") + "); builder.AddComponentParameter(4, \"ChildContent\", " + fragment + "); builder.CloseComponent();",
                "default")
            {
                AdditionalExpectedFragment = marker + "-nested",
                TertiaryExpectedFragment = marker + "-parent",
                ImportCount = 1
            },
            6 => new(
                RepeatedChild(marker + "-generic", 0) + "builder.OpenComponent(3, typeof(MatrixChild)); builder.AddComponentParameter(4, \"Title\", " + CSharpStringLiteral(marker + "-typeof") + "); builder.CloseComponent();",
                marker + "-generic")
            {
                AdditionalExpectedFragment = marker + "-typeof",
                TertiaryExpectedFragment = "heading",
                UsesFragment = true,
                ImportCount = 1
            },
            _ => new(
                "builder.OpenComponent<MatrixChild>(0); builder.SetKey(" + CSharpStringLiteral(marker + "-first") + "); builder.CloseComponent(); builder.OpenComponent<MatrixChild>(2); builder.SetKey(" + CSharpStringLiteral(marker + "-second") + "); builder.CloseComponent();",
                "key")
            {
                AdditionalExpectedFragment = marker + "-first",
                TertiaryExpectedFragment = marker + "-second",
                UsesFragment = true,
                ImportCount = 1
            }
        };
    }

    private static string RepeatedChild(string title, int sequence)
        => "builder.OpenComponent<MatrixChild>(" + sequence.ToString(CultureInfo.InvariantCulture) + "); builder.AddComponentParameter(" + (sequence + 1).ToString(CultureInfo.InvariantCulture) + ", \"Title\", " + CSharpStringLiteral(title) + "); builder.CloseComponent(); ";

    private static CoverageCaseSpec CreateHelperCompositionCase(int shape, int hostIndex, string marker, string tag)
    {
        var suffix = shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var helper = "ComposeCoverage" + suffix;
        var nested = "ComposeCoverageNested" + suffix;
        return shape switch
        {
            0 => new(helper + "(builder, Text + " + CSharpStringLiteral(marker) + ");", marker)
            {
                AdditionalExpectedFragment = "props.Text",
                Members = "[Parameter] public string Text { get; set; } = \"\"; private static void " + helper + "(RenderTreeBuilder target, string value) { target.AddContent(0, value); }",
                UsesProps = true
            },
            1 => new(helper + "(builder, " + CSharpStringLiteral(marker) + ");", marker)
            {
                AdditionalExpectedFragment = "h(" + CSharpStringLiteral(tag),
                Members = "private static void " + helper + "(RenderTreeBuilder target, string value) { target.OpenElement(0, " + CSharpStringLiteral(tag) + "); target.AddContent(1, value); target.CloseElement(); }"
            },
            2 => new(helper + "(builder, " + CSharpStringLiteral(marker) + ");", marker)
            {
                AdditionalExpectedFragment = "heading",
                Members = "private static void " + helper + "(RenderTreeBuilder target, string value) { target.OpenComponent<MatrixChild>(0); target.AddComponentParameter(1, \"Title\", value); target.CloseComponent(); }",
                ImportCount = 1
            },
            3 => new(helper + "(builder, " + CSharpStringLiteral(marker + "-first") + ", " + CSharpStringLiteral(marker + "-second") + ");", marker + "-first")
            {
                AdditionalExpectedFragment = marker + "-second",
                Members = "private static void " + helper + "(RenderTreeBuilder target, string first, string second) { target.OpenRegion(0); target.AddContent(1, first); target.AddContent(2, second); target.CloseRegion(); }",
                UsesFragment = true
            },
            4 => new(helper + "(builder, Visible, " + CSharpStringLiteral(marker + "-yes") + ", " + CSharpStringLiteral(marker + "-no") + ");", "props.Visible")
            {
                AdditionalExpectedFragment = marker + "-yes",
                TertiaryExpectedFragment = marker + "-no",
                Members = "[Parameter] public bool Visible { get; set; } private static void " + helper + "(RenderTreeBuilder target, bool visible, string yes, string no) { if (visible) { target.AddContent(0, yes); } else { target.AddContent(1, no); } }",
                UsesProps = true
            },
            5 => new(helper + "(builder, Items, " + CSharpStringLiteral(marker + ":") + ");", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = []; private static void " + helper + "(RenderTreeBuilder target, string[] items, string prefix) { foreach (var item in items) { target.AddContent(0, prefix + item); } }",
                UsesProps = true
            },
            6 => new(helper + "(builder, " + CSharpStringLiteral(marker) + ");", marker)
            {
                AdditionalExpectedFragment = "h(" + CSharpStringLiteral(tag),
                Members = "private static void " + helper + "(RenderTreeBuilder target, string value) { " + nested + "(target, value); } private static void " + nested + "(RenderTreeBuilder target, string value) { target.OpenElement(0, " + CSharpStringLiteral(tag) + "); target.AddContent(1, value); target.CloseElement(); }"
            },
            _ => new(helper + "(builder, " + CSharpStringLiteral(marker) + ");", "const normalized" + suffix)
            {
                AdditionalExpectedFragment = marker,
                Members = "private static void " + helper + "(RenderTreeBuilder target, string value) { var normalized" + suffix + " = value + \"!\"; target.AddContent(0, normalized" + suffix + "); }"
            }
        };
    }

    private static CoverageCaseSpec CreateFragmentCompositionCase(int shape, int hostIndex, string marker, string tag)
    {
        var suffix = shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var fragment = "coverageFragment" + suffix;
        var nested = "nestedFragment" + suffix;
        return shape switch
        {
            0 => new("RenderFragment " + fragment + " = child => child.AddContent(0, " + CSharpStringLiteral(marker) + "); builder.AddContent(1, " + fragment + ");", marker),
            1 => new("RenderFragment " + fragment + " = child => { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker) + "); child.CloseElement(); }; builder.AddContent(2, " + fragment + ");", marker)
            {
                AdditionalExpectedFragment = "h(" + CSharpStringLiteral(tag)
            },
            2 => new("RenderFragment " + fragment + " = child => { child.AddContent(0, " + CSharpStringLiteral(marker + "-first") + "); child.AddContent(1, " + CSharpStringLiteral(marker + "-second") + "); }; builder.AddContent(2, " + fragment + ");", marker + "-first")
            {
                AdditionalExpectedFragment = marker + "-second",
                UsesFragment = true
            },
            3 => new("RenderFragment " + fragment + " = child => child.AddMarkupContent(0, " + CSharpStringLiteral("<strong>" + marker + "</strong>") + "); builder.AddContent(1, " + fragment + ");", marker)
            {
                AdditionalExpectedFragment = "createStaticVNode",
                UsesStaticVNode = true
            },
            4 => new("RenderFragment " + fragment + " = child => { if (Visible) { child.AddContent(0, " + CSharpStringLiteral(marker + "-yes") + "); } else { child.AddContent(1, " + CSharpStringLiteral(marker + "-no") + "); } }; builder.AddContent(2, " + fragment + ");", "props.Visible")
            {
                AdditionalExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            },
            5 => new("RenderFragment " + fragment + " = child => { foreach (var item in Items) { child.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item); } }; builder.AddContent(1, " + fragment + ");", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            },
            6 => new("RenderFragment " + nested + " = child => child.AddContent(0, " + CSharpStringLiteral(marker) + "); RenderFragment " + fragment + " = child => " + nested + "(child); builder.AddContent(1, " + fragment + ");", marker),
            _ => new("RenderFragment first" + suffix + " = child => child.AddContent(0, " + CSharpStringLiteral(marker + "-first") + "); RenderFragment second" + suffix + " = child => child.AddContent(0, " + CSharpStringLiteral(marker + "-second") + "); RenderFragment " + fragment + " = Visible ? first" + suffix + " : second" + suffix + "; builder.AddContent(1, " + fragment + ");", "props.Visible")
            {
                AdditionalExpectedFragment = marker + "-first",
                TertiaryExpectedFragment = marker + "-second",
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            }
        };
    }

    private static CoverageCaseSpec CreateLocalPreludeCase(int shape, int hostIndex, string marker, string tag)
    {
        var local = "coverageLocal" + shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        return shape switch
        {
            0 => new("var " + local + " = Text + " + CSharpStringLiteral(marker) + "; builder.AddContent(0, " + local + ");", "const " + local)
            {
                AdditionalExpectedFragment = "props.Text",
                Members = "[Parameter] public string Text { get; set; } = \"\";",
                UsesProps = true
            },
            1 => new("var " + local + " = Count * 2 + " + hostIndex.ToString(CultureInfo.InvariantCulture) + "; builder.AddContent(0, " + local + ");", "const " + local)
            {
                AdditionalExpectedFragment = "props.Count * 2",
                Members = "[Parameter] public int Count { get; set; }",
                UsesProps = true
            },
            2 => new("var " + local + " = Visible && Count > " + hostIndex.ToString(CultureInfo.InvariantCulture) + "; builder.AddContent(0, " + local + " ? " + CSharpStringLiteral(marker + "-yes") + " : " + CSharpStringLiteral(marker + "-no") + ");", "const " + local)
            {
                AdditionalExpectedFragment = "props.Visible",
                TertiaryExpectedFragment = "props.Count",
                Members = "[Parameter] public bool Visible { get; set; } [Parameter] public int Count { get; set; }",
                UsesProps = true
            },
            3 => new("var " + local + " = Items; foreach (var item in " + local + ") { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item); }", "const " + local)
            {
                AdditionalExpectedFragment = "Array.from(" + local + " ?? []",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            },
            4 => new("var " + local + " = Text ?? " + CSharpStringLiteral(marker) + "; builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddContent(1, " + local + "); builder.CloseElement();", "const " + local)
            {
                AdditionalExpectedFragment = "props.Text",
                TertiaryExpectedFragment = "h(" + CSharpStringLiteral(tag),
                Members = "[Parameter] public string? Text { get; set; }",
                UsesProps = true
            },
            5 => new("var " + local + " = (MarkupString)" + CSharpStringLiteral("<strong>" + marker + "</strong>") + "; builder.AddContent(0, " + local + ");", "const " + local)
            {
                AdditionalExpectedFragment = marker,
                UsesStaticVNode = true
            },
            6 => new("var " + local + " = Count + " + hostIndex.ToString(CultureInfo.InvariantCulture) + "; builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Count\", " + local + "); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker) + "); builder.CloseComponent();", "const " + local)
            {
                AdditionalExpectedFragment = "count",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public int Count { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            _ => new("builder.AddContent(0, " + CSharpStringLiteral(marker + "-before") + "); var " + local + " = Text + " + CSharpStringLiteral(marker + "-after") + "; builder.AddContent(1, " + local + ");", "const " + local)
            {
                AdditionalExpectedFragment = marker + "-before",
                TertiaryExpectedFragment = "props.Text",
                Members = "[Parameter] public string Text { get; set; } = \"\";",
                UsesFragment = true,
                UsesProps = true
            }
        };
    }

    private static CoverageCaseSpec CreateConditionalCompositionCase(int shape, int hostIndex, string marker, string tag)
    {
        var fragment = "conditionalFragment" + shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        return shape switch
        {
            0 => new("if (Visible) { builder.AddContent(0, " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddContent(1, " + CSharpStringLiteral(marker + "-no") + "); }", "props.Visible")
            {
                AdditionalExpectedFragment = marker + "-yes",
                TertiaryExpectedFragment = marker + "-no",
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            },
            1 => new("if (Visible) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddContent(1, " + CSharpStringLiteral(marker + "-yes") + "); builder.CloseElement(); } else { builder.OpenElement(2, \"span\"); builder.AddContent(3, " + CSharpStringLiteral(marker + "-no") + "); builder.CloseElement(); }", "props.Visible")
            {
                AdditionalExpectedFragment = "h(" + CSharpStringLiteral(tag),
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            },
            2 => new("if (Visible) { " + RepeatedChild(marker + "-yes", 0) + " } else { " + RepeatedChild(marker + "-no", 3) + " }", "props.Visible")
            {
                AdditionalExpectedFragment = "heading",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            3 => new("if (Visible) { builder.AddContent(0, " + CSharpStringLiteral(marker) + "); } else { builder.Dispose(); }", "props.Visible")
            {
                AdditionalExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            },
            4 => new("builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); if (Visible) { builder.AddAttribute(1, \"class\", " + CSharpStringLiteral(marker + "-yes") + "); } else { builder.AddAttribute(2, \"class\", " + CSharpStringLiteral(marker + "-no") + "); } builder.CloseElement();", "props.Visible")
            {
                AdditionalExpectedFragment = "class",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            5 => new("RenderFragment first" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + " = child => child.AddContent(0, " + CSharpStringLiteral(marker + "-yes") + "); RenderFragment second" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + " = child => child.AddContent(0, " + CSharpStringLiteral(marker + "-no") + "); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"ChildContent\", Visible ? first" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + " : second" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + "); builder.CloseComponent();", "props.Visible")
            {
                AdditionalExpectedFragment = "default",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            6 => new("if (Visible) { if (Enabled) { builder.AddContent(0, " + CSharpStringLiteral(marker + "-enabled") + "); } else { builder.AddContent(1, " + CSharpStringLiteral(marker + "-disabled") + "); } } else { builder.AddContent(2, " + CSharpStringLiteral(marker + "-hidden") + "); }", "props.Visible")
            {
                AdditionalExpectedFragment = "props.Enabled",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; } [Parameter] public bool Enabled { get; set; }",
                UsesProps = true
            },
            _ => new("RenderFragment first" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + " = child => child.AddContent(0, " + CSharpStringLiteral(marker + "-first") + "); RenderFragment second" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + " = child => child.AddContent(0, " + CSharpStringLiteral(marker + "-second") + "); RenderFragment " + fragment + " = Visible ? first" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + " : second" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + "; builder.AddContent(1, " + fragment + ");", "props.Visible")
            {
                AdditionalExpectedFragment = marker + "-first",
                TertiaryExpectedFragment = marker + "-second",
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            }
        };
    }

    private static CoverageCaseSpec CreateForeachCollectionCase(int shape, int hostIndex, string marker, string tag)
    {
        var local = "collectionAlias" + shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var fragment = "collectionFragment" + shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        return shape switch
        {
            0 => new("foreach (var item in Items) { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item); }", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            },
            1 => new("foreach (var item in Numbers) { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item); }", "Array.from(props.Numbers ?? []")
            {
                AdditionalExpectedFragment = marker,
                Members = "[Parameter] public int[] Numbers { get; set; } = [];",
                UsesProps = true
            },
            2 => new("foreach (var item in Items) { builder.AddContent(0, item ?? " + CSharpStringLiteral(marker) + "); }", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = marker,
                Members = "[Parameter] public string?[] Items { get; set; } = [];",
                UsesProps = true
            },
            3 => new("var " + local + " = Items; foreach (var item in " + local + ") { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item); }", "const " + local)
            {
                AdditionalExpectedFragment = "Array.from(" + local + " ?? []",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            },
            4 => new("foreach (var item in Items) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(item); builder.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseElement(); }", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = "key",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            },
            5 => new("foreach (var item in Items) { foreach (var number in Numbers) { builder.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + item + number); } }", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = "Array.from(props.Numbers ?? []",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = []; [Parameter] public int[] Numbers { get; set; } = [];",
                UsesProps = true
            },
            6 => new("foreach (var item in Items) { builder.OpenComponent<MatrixChild>(0); builder.SetKey(item); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseComponent(); }", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = "heading",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true,
                ImportCount = 1
            },
            _ => new("RenderFragment " + fragment + " = child => { foreach (var item in Items) { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + item); child.CloseElement(); } }; builder.AddContent(2, " + fragment + ");", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = "h(" + CSharpStringLiteral(tag),
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            }
        };
    }

    private static CoverageCaseSpec CreateDescriptorMappingCase(int shape, int hostIndex, string marker, string tag)
    {
        var suffix = shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var handler = "HandleDescriptor" + suffix;
        var runtimeHandler = handler;
        var fragment = "descriptorFragment" + suffix;
        return shape switch
        {
            0 => new("builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + "); builder.CloseComponent();", "heading")
            {
                AdditionalExpectedFragment = marker,
                ImportCount = 1
            },
            1 => new("builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Value\", " + CSharpStringLiteral(marker) + "); builder.CloseComponent();", "modelValue")
            {
                AdditionalExpectedFragment = marker,
                ImportCount = 1
            },
            2 => new("builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"OnClick\", EventCallback.Factory.Create(this, " + handler + ")); builder.CloseComponent();", "onClick")
            {
                AdditionalExpectedFragment = runtimeHandler,
                Members = "private void " + handler + "() { }",
                ImportCount = 1
            },
            3 => new("builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"ValueChanged\", EventCallback.Factory.Create<string>(this, " + handler + ")); builder.CloseComponent();", CSharpStringLiteral("onUpdate:modelValue"))
            {
                AdditionalExpectedFragment = runtimeHandler,
                Members = "private void " + handler + "(string value) { }",
                ImportCount = 1
            },
            4 => new("RenderFragment " + fragment + " = child => child.AddContent(0, " + CSharpStringLiteral(marker) + "); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"ChildContent\", " + fragment + "); builder.CloseComponent();", "default")
            {
                AdditionalExpectedFragment = marker,
                ImportCount = 1
            },
            5 => new("RenderFragment " + fragment + " = child => child.AddContent(0, " + CSharpStringLiteral(marker) + "); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"Header\", " + fragment + "); builder.CloseComponent();", "header")
            {
                AdditionalExpectedFragment = marker,
                ImportCount = 1
            },
            6 => new("RenderFragment<string> " + fragment + " = value => child => child.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + value); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"ItemTemplate\", " + fragment + "); builder.CloseComponent();", "item")
            {
                AdditionalExpectedFragment = marker,
                TertiaryExpectedFragment = "value",
                ImportCount = 1
            },
            _ => new("RenderFragment " + fragment + " = child => child.AddContent(0, " + CSharpStringLiteral(marker + "-slot") + "); builder.OpenComponent<MatrixChild>(1); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker + "-title") + "); builder.AddComponentParameter(3, \"Value\", " + CSharpStringLiteral(marker + "-value") + "); builder.AddComponentParameter(4, \"OnClick\", EventCallback.Factory.Create(this, " + handler + ")); builder.AddComponentParameter(5, \"Header\", " + fragment + "); builder.CloseComponent();", "heading")
            {
                AdditionalExpectedFragment = "modelValue",
                TertiaryExpectedFragment = "header",
                Members = "private void " + handler + "() { }",
                ImportCount = 1
            }
        };
    }
}
