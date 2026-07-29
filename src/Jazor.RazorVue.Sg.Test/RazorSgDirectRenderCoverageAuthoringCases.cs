using System.Globalization;

namespace Jazor.RazorVue.Sg.Test;

internal static partial class DirectRenderCaseCatalog
{
    private static readonly string[] CoverageHostTags =
    [
        "article", "aside", "button", "div", "form", "main", "section", "span"
    ];

    private static void AddCoverageCases(List<DirectRenderCase> cases)
    {
        AddCoverageAuthoringCases(cases);
        AddCoverageCallbackTemplateCases(cases);
        AddCoverageCompositionCompilerCases(cases);
    }

    private static void AddCoverageAuthoringCases(List<DirectRenderCase> cases)
    {
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ContentValue, "content_value", CreateContentValueCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ElementAttributeValue, "element_attribute_value", CreateElementAttributeValueCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.AttributeOverwrite, "attribute_overwrite", CreateAttributeOverwriteCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ComponentParameterValue, "component_parameter_value", CreateComponentParameterValueCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.KeyExpression, "key_expression", CreateKeyExpressionCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ComponentOpenForm, "component_open_form", CreateComponentOpenFormCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.RegionBoundary, "region_boundary", CreateRegionBoundaryCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.EmptyAndNullOutput, "empty_null_output", CreateEmptyAndNullOutputCase);
    }

    private static void AddCoverageFamily(
        List<DirectRenderCase> cases,
        RazorVueUsageScenarioFamily family,
        string familyId,
        CoverageCaseFactory createCase)
    {
        for (var shape = 0; shape < 8; shape++)
        {
            var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
            for (var hostIndex = 0; hostIndex < CoverageHostTags.Length; hostIndex++)
            {
                var hostId = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "coverage-" + familyId + "-" + shapeId + "-" + hostId;
                var spec = createCase(shape, hostIndex, marker, CoverageHostTags[hostIndex]);
                Add(
                    cases,
                    "coverage_authoring_" + familyId + "_" + shapeId + "_" + hostId,
                    spec.Body,
                    spec.ExpectedFragment,
                    spec.AdditionalExpectedFragment,
                    spec.UsesFragment,
                    spec.UsesStaticVNode,
                    group: DirectRenderCaseGroup.Coverage,
                    members: spec.Members,
                    usesProps: spec.UsesProps,
                    usesSlots: spec.UsesSlots,
                    importCount: spec.ImportCount,
                    tertiaryExpectedFragment: spec.TertiaryExpectedFragment,
                    unexpectedFragment: spec.UnexpectedFragment,
                    scenario: new RazorVueUsageScenarioId(family, shape));
            }
        }
    }

    private static CoverageCaseSpec CreateContentValueCase(int shape, int hostIndex, string marker, string tag)
        => shape switch
        {
            0 => new(
                "builder.AddContent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", " + CSharpStringLiteral(marker) + ");",
                marker),
            1 => new(
                "builder.AddContent(" + (hostIndex + 100).ToString(CultureInfo.InvariantCulture) + ", Text);",
                "props.text")
            {
                Members = "[Parameter] public string Text { get; set; } = " + CSharpStringLiteral(marker) + ";",
                UsesProps = true
            },
            2 => new(
                "builder.AddContent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", Count + " + (hostIndex + 1).ToString(CultureInfo.InvariantCulture) + ");",
                "props.count")
            {
                AdditionalExpectedFragment = (hostIndex + 1).ToString(CultureInfo.InvariantCulture),
                Members = "[Parameter] public int Count { get; set; }",
                UsesProps = true
            },
            3 => new(
                "builder.AddContent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", Visible ? " + CSharpStringLiteral(marker + "-visible") + " : " + CSharpStringLiteral(marker + "-hidden") + ");",
                "props.visible")
            {
                AdditionalExpectedFragment = marker + "-visible",
                TertiaryExpectedFragment = marker + "-hidden",
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            },
            4 => new(
                "builder.AddContent(0, " + CSharpStringLiteral(marker + "-before") + "); builder.AddContent(" + (hostIndex + 1).ToString(CultureInfo.InvariantCulture) + ", Text); builder.AddContent(" + (hostIndex + 10).ToString(CultureInfo.InvariantCulture) + ", " + CSharpStringLiteral(marker + "-after") + ");",
                "props.text")
            {
                AdditionalExpectedFragment = marker + "-before",
                TertiaryExpectedFragment = marker + "-after",
                Members = "[Parameter] public string? Text { get; set; }",
                UsesFragment = true,
                UsesProps = true
            },
            5 => new(
                "builder.AddContent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", (MarkupString)" + CSharpStringLiteral("<" + tag + ">" + marker + "</" + tag + ">") + ");",
                marker),
            6 => new(
                "builder.AddContent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", Items.Length + " + (hostIndex + 2).ToString(CultureInfo.InvariantCulture) + ");",
                "props.items.length")
            {
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            },
            _ => new(
                "RenderFragment fragment" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + " = child => { child.AddContent(0, " + CSharpStringLiteral(marker) + "); child.AddContent(1, Text); }; builder.AddContent(2, fragment" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + ");",
                marker)
            {
                AdditionalExpectedFragment = "props.text",
                Members = "[Parameter] public string Text { get; set; } = \"\";",
                UsesFragment = true,
                UsesProps = true
            }
        };

    private static CoverageCaseSpec CreateElementAttributeValueCase(int shape, int hostIndex, string marker, string tag)
    {
        string[] names = ["aria-label", "class", "data-state", "dir", "lang", "role", "tabindex", "title"];
        var name = names[hostIndex];
        var property = FormatObjectPropertyKey(name);
        var open = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); ";
        const string close = " builder.CloseElement();";

        return shape switch
        {
            0 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(marker) + ");" + close,
                property)
            {
                AdditionalExpectedFragment = marker
            },
            1 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", Count + " + (hostIndex + 1).ToString(CultureInfo.InvariantCulture) + ");" + close,
                property)
            {
                AdditionalExpectedFragment = "props.count",
                Members = "[Parameter] public int Count { get; set; }",
                UsesProps = true
            },
            2 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", Visible && Count >= " + hostIndex.ToString(CultureInfo.InvariantCulture) + ");" + close,
                property)
            {
                AdditionalExpectedFragment = "props.visible",
                TertiaryExpectedFragment = "props.count",
                Members = "[Parameter] public bool Visible { get; set; } [Parameter] public int Count { get; set; }",
                UsesProps = true
            },
            3 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", (string?)null); builder.AddAttribute(2, \"data-case\", " + CSharpStringLiteral(marker) + ");" + close,
                property)
            {
                AdditionalExpectedFragment = "null",
                TertiaryExpectedFragment = marker
            },
            4 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", Visible ? " + CSharpStringLiteral(marker) + " : Text);" + close,
                property)
            {
                AdditionalExpectedFragment = "props.visible",
                TertiaryExpectedFragment = "props.text",
                Members = "[Parameter] public bool Visible { get; set; } [Parameter] public string Text { get; set; } = \"\";",
                UsesProps = true
            },
            5 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(hostIndex % 2 == 0 ? "disabled" : "hidden") + "); builder.AddAttribute(2, \"data-case\", " + CSharpStringLiteral(marker) + ");" + close,
                hostIndex % 2 == 0 ? "disabled" : "hidden")
            {
                AdditionalExpectedFragment = "true",
                TertiaryExpectedFragment = marker
            },
            6 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", Items[" + hostIndex.ToString(CultureInfo.InvariantCulture) + "]);" + close,
                property)
            {
                AdditionalExpectedFragment = "props.items[" + hostIndex.ToString(CultureInfo.InvariantCulture) + "]",
                Members = "[Parameter] public string?[] Items { get; set; } = [];",
                UsesProps = true
            },
            _ => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", $\"" + marker + ":{Count}\");" + close,
                property)
            {
                AdditionalExpectedFragment = marker,
                TertiaryExpectedFragment = "props.count",
                Members = "[Parameter] public int Count { get; set; }",
                UsesProps = true
            }
        };
    }

    private static CoverageCaseSpec CreateAttributeOverwriteCase(int shape, int hostIndex, string marker, string tag)
    {
        string[] names = ["aria-label", "class", "data-state", "dir", "lang", "role", "tabindex", "title"];
        var name = names[hostIndex];
        var property = FormatObjectPropertyKey(name);
        var before = marker + "-before";
        var after = marker + "-after";
        var open = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); ";
        const string close = " builder.CloseElement();";

        return shape switch
        {
            0 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(before) + "); builder.AddAttribute(2, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(after) + ");" + close,
                property)
            {
                AdditionalExpectedFragment = before,
                TertiaryExpectedFragment = after
            },
            1 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(before) + "); builder.SetAttributeValue(2, " + CSharpStringLiteral(after) + ");" + close,
                property)
            {
                AdditionalExpectedFragment = after,
                UnexpectedFragment = CSharpStringLiteral(before)
            },
            2 => new(
                open + "builder.AddAttribute(1, \"data-first\", " + CSharpStringLiteral(marker + "-first") + "); builder.AddAttribute(2, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(marker + "-middle") + "); builder.AddAttribute(3, \"aria-label\", " + CSharpStringLiteral(marker + "-last") + ");" + close,
                "data-first")
            {
                AdditionalExpectedFragment = property,
                TertiaryExpectedFragment = marker + "-last"
            },
            3 => new(
                open + "if (Visible) { builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(marker + "-conditional") + "); } builder.AddAttribute(2, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(after) + ");" + close,
                "props.visible")
            {
                AdditionalExpectedFragment = property,
                TertiaryExpectedFragment = after,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            4 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(before) + "); if (Visible) { builder.AddAttribute(2, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(after) + "); }" + close,
                "props.visible")
            {
                AdditionalExpectedFragment = property,
                TertiaryExpectedFragment = before,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            5 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(name) + ", " + CSharpStringLiteral(before) + "); builder.SetAttributeValue(2, Text + " + CSharpStringLiteral(after) + ");" + close,
                property)
            {
                AdditionalExpectedFragment = "props.text",
                TertiaryExpectedFragment = after,
                UnexpectedFragment = CSharpStringLiteral(before),
                Members = "[Parameter] public string Text { get; set; } = \"\";",
                UsesProps = true
            },
            6 => new(
                "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(before) + "); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(after) + "); builder.AddComponentParameter(3, \"Count\", " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.CloseComponent();",
                "heading")
            {
                AdditionalExpectedFragment = before,
                TertiaryExpectedFragment = after,
                ImportCount = 1
            },
            _ => new(
                "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(before) + "); builder.SetAttributeValue(2, " + CSharpStringLiteral(after) + "); builder.AddComponentParameter(3, \"Count\", " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.CloseComponent();",
                "heading")
            {
                AdditionalExpectedFragment = after,
                UnexpectedFragment = CSharpStringLiteral(before),
                ImportCount = 1
            }
        };
    }

    private static CoverageCaseSpec CreateComponentParameterValueCase(int shape, int hostIndex, string marker, string tag)
    {
        var open = hostIndex % 2 == 0
            ? "builder.OpenComponent<MatrixChild>(0); "
            : "builder.OpenComponent(0, typeof(MatrixChild)); ";
        const string close = " builder.CloseComponent();";

        return shape switch
        {
            0 => new(
                open + "builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + ");" + close,
                "heading")
            {
                AdditionalExpectedFragment = marker,
                ImportCount = 1
            },
            1 => new(
                open + "builder.AddComponentParameter(1, \"Count\", Count + " + (hostIndex + 1).ToString(CultureInfo.InvariantCulture) + ");" + close,
                "count")
            {
                AdditionalExpectedFragment = "props.count",
                Members = "[Parameter] public int Count { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            2 => new(
                open + "builder.AddComponentParameter(1, \"Enabled\", Visible && Count > " + hostIndex.ToString(CultureInfo.InvariantCulture) + ");" + close,
                "enabled")
            {
                AdditionalExpectedFragment = "props.visible",
                TertiaryExpectedFragment = "props.count",
                Members = "[Parameter] public bool Visible { get; set; } [Parameter] public int Count { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            3 => new(
                open + "builder.AddComponentParameter(1, \"Value\", Text ?? " + CSharpStringLiteral(marker) + ");" + close,
                "modelValue")
            {
                AdditionalExpectedFragment = "props.text",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string? Text { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            4 => new(
                open + "builder.AddComponentParameter(1, \"Title\", $\"" + marker + ":{Count}\");" + close,
                "heading")
            {
                AdditionalExpectedFragment = marker,
                TertiaryExpectedFragment = "props.count",
                Members = "[Parameter] public int Count { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            5 => new(
                open + "builder.AddComponentParameter(1, \"Count\", Items.Length + " + hostIndex.ToString(CultureInfo.InvariantCulture) + ");" + close,
                "count")
            {
                AdditionalExpectedFragment = "props.items.length",
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true,
                ImportCount = 1
            },
            6 => new(
                open + "builder.AddComponentParameter(1, \"Enabled\", Count >= " + (hostIndex + 1).ToString(CultureInfo.InvariantCulture) + ");" + close,
                "enabled")
            {
                AdditionalExpectedFragment = "props.count",
                Members = "[Parameter] public int Count { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            _ => new(
                open + "builder.AddComponentParameter(1, \"Value\", Text); builder.AddComponentParameter(2, \"Title\", " + CSharpStringLiteral(marker) + ");" + close,
                "modelValue")
            {
                AdditionalExpectedFragment = "props.text",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string? Text { get; set; }",
                UsesProps = true,
                ImportCount = 1
            }
        };
    }

    private static CoverageCaseSpec CreateKeyExpressionCase(int shape, int hostIndex, string marker, string tag)
        => shape switch
        {
            0 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(" + CSharpStringLiteral(marker) + "); builder.CloseElement();",
                "key")
            {
                AdditionalExpectedFragment = marker
            },
            1 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(Count + " + (hostIndex + 1).ToString(CultureInfo.InvariantCulture) + "); builder.CloseElement();",
                "key")
            {
                AdditionalExpectedFragment = "props.count",
                Members = "[Parameter] public int Count { get; set; }",
                UsesProps = true
            },
            2 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(Visible ? " + CSharpStringLiteral(marker + "-on") + " : " + CSharpStringLiteral(marker + "-off") + "); builder.CloseElement();",
                "key")
            {
                AdditionalExpectedFragment = "props.visible",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            },
            3 => new(
                "builder.OpenComponent<MatrixChild>(0); builder.SetKey(" + CSharpStringLiteral(marker) + "); builder.CloseComponent();",
                "key")
            {
                AdditionalExpectedFragment = marker,
                ImportCount = 1
            },
            4 => new(
                "builder.OpenComponent(0, typeof(MatrixChild)); builder.SetKey(Text ?? " + CSharpStringLiteral(marker) + "); builder.CloseComponent();",
                "key")
            {
                AdditionalExpectedFragment = "props.text",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string? Text { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            5 => new(
                "foreach (var item in Items) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(" + CSharpStringLiteral(marker + ":") + " + item); builder.AddContent(1, item); builder.CloseElement(); }",
                "key")
            {
                AdditionalExpectedFragment = "Array.from(props.items ?? []",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            },
            6 => new(
                "foreach (var item in Items) { builder.OpenComponent<MatrixChild>(0); builder.SetKey(" + CSharpStringLiteral(marker + ":") + " + item); builder.AddComponentParameter(1, \"Title\", item); builder.CloseComponent(); }",
                "key")
            {
                AdditionalExpectedFragment = "Array.from(props.items ?? []",
                TertiaryExpectedFragment = "heading",
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true,
                ImportCount = 1
            },
            _ => new(
                "var key" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + " = Text ?? " + CSharpStringLiteral(marker) + "; builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(key" + hostIndex.ToString("D2", CultureInfo.InvariantCulture) + "); builder.CloseElement();",
                "const key" + hostIndex.ToString("D2", CultureInfo.InvariantCulture))
            {
                AdditionalExpectedFragment = "props.text",
                TertiaryExpectedFragment = "key: key" + hostIndex.ToString("D2", CultureInfo.InvariantCulture),
                Members = "[Parameter] public string? Text { get; set; }",
                UsesProps = true
            }
        };

    private static CoverageCaseSpec CreateComponentOpenFormCase(int shape, int hostIndex, string marker, string tag)
    {
        var alias = "componentType" + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var helper = "RenderChild" + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        return shape switch
        {
            0 => new(
                "builder.OpenComponent<MatrixChild>(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.CloseComponent();",
                "h(i$")
            {
                AdditionalExpectedFragment = "null",
                ImportCount = 1
            },
            1 => new(
                "builder.OpenComponent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", typeof(MatrixChild)); builder.CloseComponent();",
                "h(i$")
            {
                AdditionalExpectedFragment = "null",
                ImportCount = 1
            },
            2 => new(
                "var " + alias + " = typeof(MatrixChild); builder.OpenComponent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", " + alias + "); builder.CloseComponent();",
                "h(i$")
            {
                AdditionalExpectedFragment = "null",
                ImportCount = 1
            },
            3 => new(
                "var " + alias + " = typeof(MatrixChild); if (Visible) { builder.OpenComponent(0, " + alias + "); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + "-visible") + "); builder.CloseComponent(); } else { builder.OpenComponent(2, " + alias + "); builder.AddComponentParameter(3, \"Title\", " + CSharpStringLiteral(marker + "-hidden") + "); builder.CloseComponent(); }",
                "props.visible")
            {
                AdditionalExpectedFragment = "heading",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true,
                ImportCount = 1
            },
            4 => new(
                "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + "); builder.AddComponentParameter(2, \"Count\", " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.CloseComponent();",
                "heading")
            {
                AdditionalExpectedFragment = "count",
                TertiaryExpectedFragment = marker,
                ImportCount = 1
            },
            5 => new(
                "builder.OpenComponent(0, typeof(MatrixChild)); builder.AddComponentParameter(1, \"Value\", " + CSharpStringLiteral(marker) + "); builder.CloseComponent();",
                "modelValue")
            {
                AdditionalExpectedFragment = marker,
                ImportCount = 1
            },
            6 => new(
                "var " + alias + " = typeof(MatrixChild); foreach (var item in Items) { builder.OpenComponent(0, " + alias + "); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseComponent(); }",
                "Array.from(props.items ?? []")
            {
                AdditionalExpectedFragment = "heading",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true,
                ImportCount = 1
            },
            _ => new(
                helper + "(builder, " + CSharpStringLiteral(marker) + ");",
                "heading")
            {
                AdditionalExpectedFragment = marker,
                Members = "private static void " + helper + "(RenderTreeBuilder target, string title) { target.OpenComponent<MatrixChild>(0); target.AddComponentParameter(1, \"Title\", title); target.CloseComponent(); }",
                ImportCount = 1
            }
        };
    }

    private static CoverageCaseSpec CreateRegionBoundaryCase(int shape, int hostIndex, string marker, string tag)
        => shape switch
        {
            0 => new(
                "builder.OpenRegion(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.CloseRegion();",
                "null"),
            1 => new(
                "builder.OpenRegion(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.AddContent(1, " + CSharpStringLiteral(marker) + "); builder.CloseRegion();",
                marker),
            2 => new(
                "builder.OpenRegion(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.AddContent(1, " + CSharpStringLiteral(marker + "-first") + "); builder.AddContent(2, " + CSharpStringLiteral(marker + "-second") + "); builder.CloseRegion();",
                marker + "-first")
            {
                AdditionalExpectedFragment = marker + "-second",
                UsesFragment = true
            },
            3 => new(
                "builder.OpenRegion(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.OpenRegion(1); builder.CloseRegion(); builder.AddContent(2, " + CSharpStringLiteral(marker) + "); builder.CloseRegion();",
                marker)
            {
                AdditionalExpectedFragment = "null",
                UsesFragment = true
            },
            4 => new(
                "builder.OpenRegion(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.OpenElement(1, " + CSharpStringLiteral(tag) + "); builder.AddContent(2, " + CSharpStringLiteral(marker) + "); builder.CloseElement(); builder.CloseRegion();",
                "h(" + CSharpStringLiteral(tag))
            {
                AdditionalExpectedFragment = marker
            },
            5 => new(
                "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddAttribute(1, \"data-case\", " + CSharpStringLiteral(marker) + "); builder.OpenRegion(" + (hostIndex + 2).ToString(CultureInfo.InvariantCulture) + "); builder.CloseRegion(); builder.CloseElement();",
                "h(" + CSharpStringLiteral(tag))
            {
                AdditionalExpectedFragment = marker
            },
            6 => new(
                "builder.OpenRegion(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); if (Visible) { builder.AddContent(1, " + CSharpStringLiteral(marker + "-visible") + "); } else { builder.AddContent(2, " + CSharpStringLiteral(marker + "-hidden") + "); } builder.CloseRegion();",
                "props.visible")
            {
                AdditionalExpectedFragment = marker,
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            },
            _ => new(
                "builder.OpenRegion(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); foreach (var item in Items) { builder.OpenElement(1, " + CSharpStringLiteral(tag) + "); builder.AddContent(2, " + CSharpStringLiteral(marker + ":") + " + item); builder.CloseElement(); } builder.CloseRegion();",
                "Array.from(props.items ?? []")
            {
                AdditionalExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = [];",
                UsesProps = true
            }
        };

    private static CoverageCaseSpec CreateEmptyAndNullOutputCase(int shape, int hostIndex, string marker, string tag)
    {
        var repetitions = string.Concat(Enumerable.Repeat("builder.Dispose(); ", hostIndex + 1));
        var fragment = "emptyFragment" + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        return shape switch
        {
            0 => new(repetitions, "null"),
            1 => new(
                "builder.AddContent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", (string?)null);",
                "null"),
            2 => new(
                "builder.OpenRegion(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.CloseRegion(); builder.Dispose();",
                "null"),
            3 => new(
                "RenderFragment " + fragment + " = child => { child.Dispose(); }; builder.AddContent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", " + fragment + ");",
                "null"),
            4 => new(
                "if (!Visible) { return; } builder.AddContent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", " + CSharpStringLiteral(marker) + ");",
                marker)
            {
                AdditionalExpectedFragment = "props.visible",
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            },
            5 => new(
                "builder.AddContent(0, " + CSharpStringLiteral(marker + "-before") + "); builder.Clear(); builder.AddContent(" + (hostIndex + 1).ToString(CultureInfo.InvariantCulture) + ", " + CSharpStringLiteral(marker + "-after") + ");",
                marker + "-after")
            {
                UnexpectedFragment = CSharpStringLiteral(marker + "-before")
            },
            6 => new(
                "builder.AddMarkupContent(0, " + CSharpStringLiteral("<b>" + marker + "-before</b>") + "); builder.Clear(); builder.OpenElement(" + (hostIndex + 1).ToString(CultureInfo.InvariantCulture) + ", " + CSharpStringLiteral(tag) + "); builder.AddContent(2, " + CSharpStringLiteral(marker + "-after") + "); builder.CloseElement();",
                "h(" + CSharpStringLiteral(tag))
            {
                AdditionalExpectedFragment = marker + "-after",
                UnexpectedFragment = marker + "-before"
            },
            _ => new(
                "if (Visible) { builder.AddContent(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", (string?)null); } else { builder.AddContent(" + (hostIndex + 8).ToString(CultureInfo.InvariantCulture) + ", (string?)null); }",
                "props.visible")
            {
                AdditionalExpectedFragment = "null",
                Members = "[Parameter] public bool Visible { get; set; }",
                UsesProps = true
            }
        };
    }

    private delegate CoverageCaseSpec CoverageCaseFactory(int shape, int hostIndex, string marker, string tag);

    private sealed record CoverageCaseSpec(string Body, string ExpectedFragment)
    {
        public string? AdditionalExpectedFragment { get; init; }
        public string? TertiaryExpectedFragment { get; init; }
        public string? UnexpectedFragment { get; init; }
        public string Members { get; init; } = "";
        public bool UsesFragment { get; init; }
        public bool UsesStaticVNode { get; init; }
        public bool UsesProps { get; init; }
        public bool UsesSlots { get; init; }
        public int ImportCount { get; init; }
    }
}
