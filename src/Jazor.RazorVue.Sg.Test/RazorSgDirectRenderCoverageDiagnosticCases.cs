using System.Globalization;

namespace Jazor.RazorVue.Sg.Test;

internal static partial class DirectRenderFailureCaseCatalog
{
    private static readonly string[] DiagnosticTags =
    [
        "article", "aside", "button", "div", "form", "main", "section", "span"
    ];

    private static readonly string[] DiagnosticEvents =
    [
        "onblur", "onchange", "onclick", "onfocus", "oninput", "onkeydown", "onsubmit", "onkeyup"
    ];

    private static readonly string[] DiagnosticComponentTypes =
    [
        "FailureMatrixArticleChild",
        "FailureMatrixAsideChild",
        "FailureMatrixButtonChild",
        "FailureMatrixDivChild",
        "FailureMatrixFormChild",
        "FailureMatrixMainChild",
        "FailureMatrixSectionChild",
        "FailureMatrixSpanChild"
    ];

    private static void AddCoverageDiagnosticCases(List<DirectRenderFailureCase> cases)
    {
        AddDiagnosticFamily(cases, RazorVueUsageScenarioFamily.MissingAttributeTarget, "missing_attribute_target", CreateMissingAttributeTargetCase);
        AddDiagnosticFamily(cases, RazorVueUsageScenarioFamily.InvalidUpdatesTarget, "invalid_updates_target", CreateInvalidUpdatesTargetCase);
        AddDiagnosticFamily(cases, RazorVueUsageScenarioFamily.UpdatesAfterChild, "updates_after_child", CreateUpdatesAfterChildCase);
        AddDiagnosticFamily(cases, RazorVueUsageScenarioFamily.InvalidEventModifierTarget, "invalid_event_modifier_target", CreateInvalidEventModifierTargetCase);
        AddDiagnosticFamily(cases, RazorVueUsageScenarioFamily.InvalidNamedEvent, "invalid_named_event", CreateInvalidNamedEventCase);
        AddDiagnosticFamily(cases, RazorVueUsageScenarioFamily.InvalidRenderMode, "invalid_render_mode", CreateInvalidRenderModeCase);
        AddDiagnosticFamily(cases, RazorVueUsageScenarioFamily.ReferenceAfterChild, "reference_after_child", CreateReferenceAfterChildCase);
        AddDiagnosticFamily(cases, RazorVueUsageScenarioFamily.InvalidMetadataTiming, "invalid_metadata_timing", CreateInvalidMetadataTimingCase);
    }

    private static void AddDiagnosticFamily(
        List<DirectRenderFailureCase> cases,
        RazorVueUsageScenarioFamily family,
        string familyId,
        DiagnosticCaseFactory createCase)
    {
        for (var shape = 0; shape < 8; shape++)
        {
            var shapeId = shape.ToString("D2", CultureInfo.InvariantCulture);
            for (var hostIndex = 0; hostIndex < DiagnosticTags.Length; hostIndex++)
            {
                var hostId = hostIndex.ToString("D2", CultureInfo.InvariantCulture);
                var marker = "coverage-diagnostic-" + familyId + "-" + shapeId + "-" + hostId;
                var spec = createCase(shape, hostIndex, marker, DiagnosticTags[hostIndex], DiagnosticEvents[hostIndex]);
                cases.Add(new DirectRenderFailureCase(
                    "coverage_diagnostic_" + familyId + "_" + shapeId + "_" + hostId,
                    "DirectRenderFailure" + cases.Count.ToString("D3", CultureInfo.InvariantCulture),
                    spec.Body,
                    spec.Members,
                    spec.ExpectedFailureFragment,
                    new RazorVueUsageScenarioId(family, shape)));
            }
        }
    }

    private static DiagnosticCaseSpec CreateMissingAttributeTargetCase(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string eventName)
        => shape switch
        {
            0 => new(
                "builder.AddAttribute(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", \"data-case\", " + Literal(marker) + ");",
                "Attributes must be added before children on an open element or component"),
            1 => new(
                "builder.AddComponentParameter(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", \"Title\", " + Literal(marker) + ");",
                "Component parameters must be added before component children"),
            2 => new(
                "builder.AddMultipleAttributes(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", AdditionalAttributes);",
                "Multiple attributes must be added before children on an open element or component")
            {
                Members = "[Parameter] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }"
            },
            3 => new(
                "builder.SetAttributeValue(" + hostIndex.ToString(CultureInfo.InvariantCulture) + ", " + Literal(marker) + ");",
                "SetAttributeValue must target the most recent open element or component before children"),
            4 => new(
                "builder.OpenElement(0, " + Literal(tag) + "); builder.CloseElement(); builder.AddAttribute(1, \"data-case\", " + Literal(marker) + ");",
                "Attributes must be added before children on an open element or component"),
            5 => new(
                "builder.OpenComponent<FailureMatrixChild>(0); builder.CloseComponent(); builder.AddComponentParameter(1, \"Title\", " + Literal(marker) + ");",
                "Component parameters must be added before component children"),
            6 => new(
                "builder.OpenRegion(" + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.AddAttribute(1, \"data-case\", " + Literal(marker) + "); builder.CloseRegion();",
                "Attributes must be added before children on an open element or component"),
            _ => new(
                "builder.OpenElement(0, " + Literal(tag) + "); builder.AddComponentParameter(1, \"Title\", " + Literal(marker) + "); builder.CloseElement();",
                "Component parameters must be added before component children")
        };

    private static DiagnosticCaseSpec CreateInvalidUpdatesTargetCase(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string eventName)
    {
        const string expected = "SetUpdatesAttributeName must target an open element before children";
        return shape switch
        {
            0 => new("builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + ");", expected),
            1 => new("builder.OpenComponent<FailureMatrixChild>(0); builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + "); builder.CloseComponent();", expected),
            2 => new("builder.OpenRegion(0); builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + "); builder.CloseRegion();", expected),
            3 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.CloseElement(); builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + ");", expected),
            4 => new("builder.OpenComponent<FailureMatrixChild>(0); builder.CloseComponent(); builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + ");", expected),
            5 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.Clear(); builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + ");", expected),
            6 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.OpenRegion(1); builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + "); builder.CloseRegion(); builder.CloseElement();", expected),
            _ => new("builder.OpenElement(0, " + Literal(tag) + "); builder.OpenComponent<FailureMatrixChild>(1); builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + "); builder.CloseComponent(); builder.CloseElement();", expected)
        };
    }

    private static DiagnosticCaseSpec CreateUpdatesAfterChildCase(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string eventName)
    {
        const string expected = "SetUpdatesAttributeName must target an open element before children";
        var open = "builder.OpenElement(0, " + Literal(tag) + "); ";
        var close = " builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + "); builder.CloseElement();";
        return shape switch
        {
            0 => new(open + "builder.AddContent(1, " + Literal(marker) + ");" + close, expected),
            1 => new(open + "builder.AddMarkupContent(1, " + Literal("<b>" + marker + "</b>") + ");" + close, expected),
            2 => new(open + "builder.OpenElement(1, \"strong\"); builder.AddContent(2, " + Literal(marker) + "); builder.CloseElement();" + close, expected),
            3 => new(open + "builder.OpenRegion(1); builder.AddContent(2, " + Literal(marker) + "); builder.CloseRegion();" + close, expected),
            4 => new(open + "builder.OpenComponent<FailureMatrixChild>(1); builder.CloseComponent();" + close, expected),
            5 => new(open + "builder.AddContent(1, ChildContent);" + close, expected)
            {
                Members = "[Parameter] public RenderFragment? ChildContent { get; set; }"
            },
            6 => new(open + "if (Visible) { builder.AddContent(1, " + Literal(marker + "-yes") + "); } else { builder.AddContent(2, " + Literal(marker + "-no") + "); }" + close, expected)
            {
                Members = "[Parameter] public bool Visible { get; set; }"
            },
            _ => new(open + "builder.AddElementReferenceCapture(1, value => { _ = value; }); builder.AddContent(2, " + Literal(marker) + ");" + close, expected)
        };
    }

    private static DiagnosticCaseSpec CreateInvalidEventModifierTargetCase(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string eventName)
    {
        const string expected = "Event modifier attributes must target an open element before children";
        var modifier = hostIndex % 2 == 0
            ? "builder.AddEventPreventDefaultAttribute(2, " + Literal(eventName) + ", true);"
            : "builder.AddEventStopPropagationAttribute(2, " + Literal(eventName) + ", true);";
        return shape switch
        {
            0 => new(modifier, expected),
            1 => new("builder.OpenComponent<FailureMatrixChild>(0); " + modifier + " builder.CloseComponent();", expected),
            2 => new("builder.OpenRegion(0); " + modifier + " builder.CloseRegion();", expected),
            3 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.CloseElement(); " + modifier, expected),
            4 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddContent(1, " + Literal(marker) + "); " + modifier + " builder.CloseElement();", expected),
            5 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.OpenElement(1, \"strong\"); builder.CloseElement(); " + modifier + " builder.CloseElement();", expected),
            6 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.OpenComponent<FailureMatrixChild>(1); " + modifier + " builder.CloseComponent(); builder.CloseElement();", expected),
            _ => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddContent(1, ChildContent); " + modifier + " builder.CloseElement();", expected)
            {
                Members = "[Parameter] public RenderFragment? ChildContent { get; set; }"
            }
        };
    }

    private static DiagnosticCaseSpec CreateInvalidNamedEventCase(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string eventName)
        => shape switch
        {
            0 => new("builder.AddNamedEvent(" + Literal(eventName) + ", " + Literal(marker) + ");", "Named event metadata must target an open element before children"),
            1 => new("builder.OpenComponent<FailureMatrixChild>(0); builder.AddNamedEvent(" + Literal(eventName) + ", " + Literal(marker) + "); builder.CloseComponent();", "Named event metadata must target an open element before children"),
            2 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddContent(1, " + Literal(marker) + "); builder.AddNamedEvent(" + Literal(eventName) + ", " + Literal(marker) + "); builder.CloseElement();", "Named event metadata must target an open element before children"),
            3 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddNamedEvent(EventName, " + Literal(marker) + "); builder.CloseElement();", "Named event metadata requires compile-time event names")
            {
                Members = "[Parameter] public string EventName { get; set; } = " + Literal(eventName) + ";"
            },
            4 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddNamedEvent(" + Literal(eventName) + ", AssignedName); builder.CloseElement();", "Named event metadata requires compile-time event names")
            {
                Members = "[Parameter] public string AssignedName { get; set; } = " + Literal(marker) + ";"
            },
            5 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddNamedEvent(\"\", " + Literal(marker) + "); builder.CloseElement();", "Named event metadata requires compile-time event names"),
            6 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddNamedEvent(" + Literal(eventName) + ", \"   \" + " + Literal(marker) + ".Substring(0, 0)); builder.CloseElement();", "Named event metadata requires compile-time event names"),
            _ => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddNamedEvent(" + Literal(eventName) + ", Visible ? " + Literal(marker + "-yes") + " : " + Literal(marker + "-no") + "); builder.CloseElement();", "Named event metadata requires compile-time event names")
            {
                Members = "[Parameter] public bool Visible { get; set; }"
            }
        };

    private static DiagnosticCaseSpec CreateInvalidRenderModeCase(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string eventName)
    {
        const string expected = "Component render mode metadata requires the current open component before children";
        var (modeExpression, members) = hostIndex switch
        {
            0 => ("null!", ""),
            1 => ("(IComponentRenderMode)null!", ""),
            2 => ("default!", ""),
            3 => ("StaticRenderMode", "private static IComponentRenderMode StaticRenderMode => null!;"),
            4 => ("InstanceRenderMode", "private IComponentRenderMode InstanceRenderMode => null!;"),
            5 => ("_staticRenderMode", "private static readonly IComponentRenderMode _staticRenderMode = null!;"),
            6 => ("_instanceRenderMode", "private readonly IComponentRenderMode _instanceRenderMode = null!;"),
            _ => (
                "Visible ? StaticRenderMode : InstanceRenderMode",
                "[Parameter] public bool Visible { get; set; } private static IComponentRenderMode StaticRenderMode => null!; private IComponentRenderMode InstanceRenderMode => null!;")
        };
        var mode = "builder.AddComponentRenderMode(" + modeExpression + ");";
        var spec = shape switch
        {
            0 => new DiagnosticCaseSpec(mode, expected),
            1 => new DiagnosticCaseSpec("builder.OpenElement(0, " + Literal(tag) + "); " + mode + " builder.CloseElement();", expected),
            2 => new DiagnosticCaseSpec("builder.OpenRegion(0); " + mode + " builder.CloseRegion();", expected),
            3 => new DiagnosticCaseSpec("builder.OpenComponent<FailureMatrixChild>(0); builder.CloseComponent(); " + mode, expected),
            4 => new DiagnosticCaseSpec("builder.OpenComponent<FailureMatrixChild>(0); builder.AddContent(1, " + Literal(marker) + "); " + mode + " builder.CloseComponent();", expected),
            5 => new DiagnosticCaseSpec("builder.OpenComponent<FailureMatrixChild>(0); builder.OpenElement(1, " + Literal(tag) + "); builder.CloseElement(); " + mode + " builder.CloseComponent();", expected),
            6 => new DiagnosticCaseSpec("builder.OpenComponent<FailureMatrixChild>(0); builder.OpenElement(1, " + Literal(tag) + "); " + mode + " builder.CloseElement(); builder.CloseComponent();", expected),
            _ => new DiagnosticCaseSpec("builder.AddContent(0, " + Literal(marker) + "); " + mode, expected)
        };
        return spec with { Members = members };
    }

    private static DiagnosticCaseSpec CreateReferenceAfterChildCase(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string eventName)
    {
        const string elementExpected = "Element reference captures require the current open element before children";
        const string componentExpected = "Component reference captures require the current open component before children";
        return shape switch
        {
            0 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddContent(1, " + Literal(marker) + "); builder.AddElementReferenceCapture(2, value => { _ = value; }); builder.CloseElement();", elementExpected),
            1 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddMarkupContent(1, " + Literal("<b>" + marker + "</b>") + "); builder.AddElementReferenceCapture(2, value => { _ = value; }); builder.CloseElement();", elementExpected),
            2 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.OpenElement(1, \"strong\"); builder.CloseElement(); builder.AddElementReferenceCapture(2, value => { _ = value; }); builder.CloseElement();", elementExpected),
            3 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.OpenRegion(1); builder.AddContent(2, " + Literal(marker) + "); builder.CloseRegion(); builder.AddElementReferenceCapture(3, value => { _ = value; }); builder.CloseElement();", elementExpected),
            4 => new("builder.OpenComponent<FailureMatrixChild>(0); builder.AddContent(1, " + Literal(marker) + "); builder.AddComponentReferenceCapture(2, value => { _ = value; }); builder.CloseComponent();", componentExpected),
            5 => new("builder.OpenComponent<FailureMatrixChild>(0); builder.OpenElement(1, " + Literal(tag) + "); builder.CloseElement(); builder.AddComponentReferenceCapture(2, value => { _ = value; }); builder.CloseComponent();", componentExpected),
            6 => new("builder.OpenElement(0, " + Literal(tag) + "); builder.AddContent(1, ChildContent); builder.AddElementReferenceCapture(2, value => { _ = value; }); builder.CloseElement();", elementExpected)
            {
                Members = "[Parameter] public RenderFragment? ChildContent { get; set; }"
            },
            _ => new(
                "builder.OpenComponent<" + DiagnosticComponentTypes[hostIndex] + ">(0); builder.OpenComponent<FailureMatrixChild>(1); builder.CloseComponent(); builder.AddComponentReferenceCapture(2, value => { _ = value; }); builder.CloseComponent();",
                componentExpected)
        };
    }

    private static DiagnosticCaseSpec CreateInvalidMetadataTimingCase(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string eventName)
    {
        var elementOpen = "builder.OpenElement(0, " + Literal(tag) + "); builder.AddContent(1, " + Literal(marker) + "); ";
        return shape switch
        {
            0 => new(elementOpen + "builder.SetKey(" + Literal(marker + "-key") + "); builder.CloseElement();", "SetKey must target an open element or component before children"),
            1 => new(elementOpen + "builder.SetAttributeValue(2, " + Literal(marker + "-after") + "); builder.CloseElement();", "SetAttributeValue must target the most recent open element or component before children"),
            2 => new(elementOpen + "builder.AddNamedEvent(" + Literal(eventName) + ", " + Literal(marker) + "); builder.CloseElement();", "Named event metadata must target an open element before children"),
            3 => new("builder.OpenComponent<FailureMatrixChild>(0); builder.AddContent(1, " + Literal(marker) + "); builder.AddComponentRenderMode(RenderMode); builder.CloseComponent();", "Component render mode metadata requires the current open component before children")
            {
                Members = "private static IComponentRenderMode RenderMode => null!;"
            },
            4 => new(elementOpen + "builder.AddMultipleAttributes(2, AdditionalAttributes); builder.CloseElement();", "Multiple attributes must be added before children on an open element or component")
            {
                Members = "[Parameter] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }"
            },
            5 => new(elementOpen + "builder.AddAttribute(2, \"data-after\", " + Literal(marker) + "); builder.CloseElement();", "Attributes must be added before children on an open element or component"),
            6 => new(elementOpen + "builder.SetUpdatesAttributeName(" + Literal("value-" + marker) + "); builder.CloseElement();", "SetUpdatesAttributeName must target an open element before children"),
            _ => new(elementOpen + "builder.AddEventPreventDefaultAttribute(2, " + Literal(eventName) + ", true); builder.CloseElement();", "Event modifier attributes must target an open element before children")
        };
    }

    private delegate DiagnosticCaseSpec DiagnosticCaseFactory(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string eventName);

    private sealed record DiagnosticCaseSpec(string Body, string ExpectedFailureFragment)
    {
        public string Members { get; init; } = "";
    }
}
