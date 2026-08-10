using System.Globalization;

namespace Jazor.RazorVue.Sg.Test;

internal static partial class DirectRenderCaseCatalog
{
    private static readonly string[] CoverageEventNames =
    [
        "onblur", "onchange", "onclick", "onfocus", "oninput", "onkeydown", "onsubmit", "onkeyup"
    ];

    private static void AddCoverageCallbackTemplateCases(List<DirectRenderCase> cases)
    {
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ElementEventCallback, "element_event_callback", CreateElementEventCallbackCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ElementBind, "element_bind", CreateElementBindCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.EventModifier, "event_modifier", CreateEventModifierCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ElementReference, "element_reference", CreateElementReferenceCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ComponentReference, "component_reference", CreateComponentReferenceCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.DefaultSlot, "default_slot", CreateDefaultSlotCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.NamedSlot, "named_slot", CreateNamedSlotCase);
        AddCoverageFamily(cases, RazorVueUsageScenarioFamily.ScopedSlot, "scoped_slot", CreateScopedSlotCase);
    }

    private static CoverageCaseSpec CreateElementEventCallbackCase(int shape, int hostIndex, string marker, string tag)
    {
        var eventName = CoverageEventNames[hostIndex];
        var runtimeName = NormalizeCoverageEventName(eventName);
        var suffix = shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var handler = "HandleCoverageEvent" + suffix;
        var runtimeHandler = handler;
        var field = "eventValue" + suffix;
        var openTag = shape is 3 or 4 or 5 ? "input" : tag;
        var open = "builder.OpenElement(0, " + CSharpStringLiteral(openTag) + "); ";
        const string close = " builder.CloseElement();";

        return shape switch
        {
            0 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handler + "));" + close,
                runtimeName)
            {
                AdditionalExpectedFragment = runtimeHandler,
                Members = "private void " + handler + "() { }"
            },
            1 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, () => " + handler + "()));" + close,
                runtimeName)
            {
                AdditionalExpectedFragment = runtimeHandler,
                Members = "private void " + handler + "() { }"
            },
            2 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", (System.Action)" + handler + ");" + close,
                runtimeName)
            {
                AdditionalExpectedFragment = runtimeHandler,
                Members = "private void " + handler + "() { }"
            },
            3 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create<string>(this, " + handler + "));" + close,
                runtimeName)
            {
                AdditionalExpectedFragment = runtimeHandler,
                Members = "private void " + handler + "(string value) { }"
            },
            4 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create<ChangeEventArgs>(this, " + handler + "));" + close,
                runtimeName)
            {
                AdditionalExpectedFragment = runtimeHandler,
                Members = "private void " + handler + "(ChangeEventArgs value) { }"
            },
            5 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create<string>(this, value => " + field + " = value)); builder.AddAttribute(2, \"data-case\", " + CSharpStringLiteral(marker) + ");" + close,
                runtimeName)
            {
                AdditionalExpectedFragment = field,
                TertiaryExpectedFragment = marker,
                Members = "private string " + field + " = \"\";"
            },
            6 => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create<MouseEventArgs>(this, " + handler + "));" + close,
                runtimeName)
            {
                AdditionalExpectedFragment = runtimeHandler,
                Members = "private void " + handler + "(MouseEventArgs value) { }"
            },
            _ => new(
                open + "builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, () => " + field + "++)); builder.AddAttribute(2, \"data-case\", " + CSharpStringLiteral(marker) + ");" + close,
                runtimeName)
            {
                AdditionalExpectedFragment = field,
                TertiaryExpectedFragment = marker,
                Members = "private int " + field + ";"
            }
        };
    }

    private static CoverageCaseSpec CreateElementBindCase(int shape, int hostIndex, string marker, string tag)
    {
        var suffix = shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var field = "boundCoverageValue" + suffix;
        var boolField = "boundCoverageFlag" + suffix;
        var intField = "boundCoverageNumber" + suffix;
        return shape switch
        {
            0 => CreateStringBindCase("input", "value", "oninput", field, marker),
            1 => CreateStringBindCase("input", "value", "onchange", field, marker),
            2 => CreateStringBindCase("textarea", "value", "oninput", field, marker),
            3 => CreateStringBindCase("select", "value", "onchange", field, marker),
            4 => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"type\", \"checkbox\"); builder.AddAttribute(2, \"checked\", " + boolField + "); builder.AddAttribute(3, \"onchange\", EventCallback.Factory.CreateBinder<bool>(this, value => " + boolField + " = value, " + boolField + ")); builder.SetUpdatesAttributeName(\"checked\"); builder.AddAttribute(4, \"data-case\", " + CSharpStringLiteral(marker) + "); builder.CloseElement();",
                "onChange")
            {
                AdditionalExpectedFragment = "eventOrValue",
                TertiaryExpectedFragment = "checked",
                Members = "private bool " + boolField + ";"
            },
            5 => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"value\", " + intField + "); builder.AddAttribute(2, \"onchange\", EventCallback.Factory.CreateBinder<int>(this, value => " + intField + " = value, " + intField + ")); builder.SetUpdatesAttributeName(\"value\"); builder.AddAttribute(3, \"data-case\", " + CSharpStringLiteral(marker) + "); builder.CloseElement();",
                "onChange")
            {
                AdditionalExpectedFragment = "eventOrValue",
                TertiaryExpectedFragment = intField,
                Members = "private int " + intField + " = " + hostIndex.ToString(CultureInfo.InvariantCulture) + ";"
            },
            6 => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"value\", " + field + "); builder.AddAttribute(2, \"oninput\", EventCallback.Factory.CreateBinder<string?>(this, value => " + field + " = value, " + field + ")); builder.SetUpdatesAttributeName(\"value\"); builder.AddAttribute(3, \"data-case\", " + CSharpStringLiteral(marker) + "); builder.CloseElement();",
                "onInput")
            {
                AdditionalExpectedFragment = "eventOrValue",
                TertiaryExpectedFragment = field,
                Members = "private string? " + field + " = " + CSharpStringLiteral(marker) + ";"
            },
            _ => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"value\", " + field + "); builder.AddAttribute(2, \"onchange\", EventCallback.Factory.Create<string>(this, value => " + field + " = value)); builder.SetUpdatesAttributeName(\"value\"); builder.AddAttribute(3, \"data-case\", " + CSharpStringLiteral(marker) + "); builder.CloseElement();",
                "onChange")
            {
                AdditionalExpectedFragment = "eventOrValue",
                TertiaryExpectedFragment = field,
                Members = "private string " + field + " = \"\";"
            }
        };
    }

    private static CoverageCaseSpec CreateStringBindCase(string tag, string attribute, string eventName, string field, string marker)
        => new(
            "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddAttribute(1, " + CSharpStringLiteral(attribute) + ", " + field + "); builder.AddAttribute(2, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.CreateBinder<string>(this, value => " + field + " = value, " + field + ")); builder.SetUpdatesAttributeName(" + CSharpStringLiteral(attribute) + "); builder.AddAttribute(3, \"data-case\", " + CSharpStringLiteral(marker) + "); builder.CloseElement();",
            NormalizeCoverageEventName(eventName))
        {
            AdditionalExpectedFragment = "eventOrValue",
            TertiaryExpectedFragment = marker,
            Members = "private string " + field + " = " + CSharpStringLiteral(marker) + ";"
        };

    private static CoverageCaseSpec CreateEventModifierCase(int shape, int hostIndex, string marker, string tag)
    {
        var eventName = CoverageEventNames[hostIndex];
        var runtimeName = NormalizeCoverageEventName(eventName);
        var suffix = shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var handler = "HandleCoverageModifier" + suffix;
        var runtimeHandler = handler;
        var open = "builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddAttribute(1, " + CSharpStringLiteral(eventName) + ", EventCallback.Factory.Create(this, " + handler + ")); ";
        const string close = " builder.CloseElement();";
        var members = "private void " + handler + "() { }";

        return shape switch
        {
            0 => new(open + "builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", true);" + close, runtimeName)
            {
                AdditionalExpectedFragment = "preventDefault",
                TertiaryExpectedFragment = runtimeHandler,
                Members = members
            },
            1 => new(open + "builder.AddEventStopPropagationAttribute(2, " + CSharpStringLiteral(eventName) + ", true);" + close, runtimeName)
            {
                AdditionalExpectedFragment = "stopPropagation",
                TertiaryExpectedFragment = runtimeHandler,
                Members = members
            },
            2 => new(open + "builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", true); builder.AddEventStopPropagationAttribute(3, " + CSharpStringLiteral(eventName) + ", true);" + close, runtimeName)
            {
                AdditionalExpectedFragment = "preventDefault",
                TertiaryExpectedFragment = "stopPropagation",
                Members = members
            },
            3 => new(open + "builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", false); builder.AddAttribute(3, \"data-case\", " + CSharpStringLiteral(marker) + ");" + close, runtimeName)
            {
                AdditionalExpectedFragment = runtimeHandler,
                TertiaryExpectedFragment = marker,
                UnexpectedFragment = "preventDefault",
                Members = members
            },
            4 => new(open + "builder.AddEventStopPropagationAttribute(2, " + CSharpStringLiteral(eventName) + ", false); builder.AddAttribute(3, \"data-case\", " + CSharpStringLiteral(marker) + ");" + close, runtimeName)
            {
                AdditionalExpectedFragment = runtimeHandler,
                TertiaryExpectedFragment = marker,
                UnexpectedFragment = "stopPropagation",
                Members = members
            },
            5 => new(open + "builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", PreventDefault);" + close, runtimeName)
            {
                AdditionalExpectedFragment = "props.PreventDefault",
                TertiaryExpectedFragment = "preventDefault",
                Members = members + " [Parameter] public bool PreventDefault { get; set; }",
                UsesProps = true
            },
            6 => new(open + "builder.AddEventStopPropagationAttribute(2, " + CSharpStringLiteral(eventName) + ", StopPropagation);" + close, runtimeName)
            {
                AdditionalExpectedFragment = "props.StopPropagation",
                TertiaryExpectedFragment = "stopPropagation",
                Members = members + " [Parameter] public bool StopPropagation { get; set; }",
                UsesProps = true
            },
            _ => new(open + "builder.AddEventPreventDefaultAttribute(2, " + CSharpStringLiteral(eventName) + ", PreventDefault); builder.AddEventPreventDefaultAttribute(3, " + CSharpStringLiteral(eventName) + ", AlternatePreventDefault); builder.AddEventStopPropagationAttribute(4, " + CSharpStringLiteral(eventName) + ", StopPropagation);" + close, runtimeName)
            {
                AdditionalExpectedFragment = "props.PreventDefault || props.AlternatePreventDefault",
                TertiaryExpectedFragment = "props.StopPropagation",
                Members = members + " [Parameter] public bool PreventDefault { get; set; } [Parameter] public bool AlternatePreventDefault { get; set; } [Parameter] public bool StopPropagation { get; set; }",
                UsesProps = true
            }
        };
    }

    private static CoverageCaseSpec CreateElementReferenceCase(int shape, int hostIndex, string marker, string tag)
    {
        var suffix = shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var field = "coverageElement" + suffix;
        var second = "secondaryElement" + suffix;
        var method = "CaptureCoverageElement" + suffix;
        var runtimeMethod = method;
        var capture = "builder.AddElementReferenceCapture(2, value => " + field + " = value); ";
        return shape switch
        {
            0 => new("builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddElementReferenceCapture(1, value => " + field + " = value); builder.CloseElement();", "ref")
            {
                AdditionalExpectedFragment = field,
                Members = "private ElementReference " + field + ";"
            },
            1 => new("builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddElementReferenceCapture(1, " + method + "); builder.CloseElement();", "ref")
            {
                AdditionalExpectedFragment = runtimeMethod,
                Members = "private void " + method + "(ElementReference value) { }"
            },
            2 => new("builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddElementReferenceCapture(1, value => " + field + " = value); builder.AddElementReferenceCapture(2, value => " + second + " = value); builder.CloseElement();", "ref")
            {
                AdditionalExpectedFragment = field,
                TertiaryExpectedFragment = second,
                Members = "private ElementReference " + field + "; private ElementReference " + second + ";"
            },
            3 => new("builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(" + CSharpStringLiteral(marker) + "); builder.AddElementReferenceCapture(1, value => " + field + " = value); builder.CloseElement();", "ref")
            {
                AdditionalExpectedFragment = "key",
                TertiaryExpectedFragment = marker,
                Members = "private ElementReference " + field + ";"
            },
            4 => new("builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddAttribute(1, \"data-case\", " + CSharpStringLiteral(marker) + "); " + capture + "builder.CloseElement();", "ref")
            {
                AdditionalExpectedFragment = marker,
                TertiaryExpectedFragment = field,
                Members = "private ElementReference " + field + ";"
            },
            5 => new("builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"value\", " + CSharpStringLiteral(marker) + "); builder.AddElementReferenceCapture(2, value => " + field + " = value); builder.CloseElement();", "ref")
            {
                AdditionalExpectedFragment = "value",
                TertiaryExpectedFragment = marker,
                Members = "private ElementReference " + field + ";"
            },
            6 => new("if (Visible) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.AddElementReferenceCapture(1, value => " + field + " = value); builder.CloseElement(); } else { builder.OpenElement(2, \"div\"); builder.AddElementReferenceCapture(3, value => " + second + " = value); builder.CloseElement(); }", "props.Visible")
            {
                AdditionalExpectedFragment = "ref",
                TertiaryExpectedFragment = field,
                Members = "[Parameter] public bool Visible { get; set; } private ElementReference " + field + "; private ElementReference " + second + ";",
                UsesProps = true
            },
            _ => new("foreach (var item in Items) { builder.OpenElement(0, " + CSharpStringLiteral(tag) + "); builder.SetKey(" + CSharpStringLiteral(marker + ":") + " + item); builder.AddElementReferenceCapture(1, value => " + field + " = value); builder.CloseElement(); }", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = "ref",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = []; private ElementReference " + field + ";",
                UsesProps = true
            }
        };
    }

    private static CoverageCaseSpec CreateComponentReferenceCase(int shape, int hostIndex, string marker, string tag)
    {
        var suffix = shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var field = "coverageChild" + suffix;
        var second = "secondaryChild" + suffix;
        var method = "CaptureCoverageChild" + suffix;
        var runtimeMethod = method;
        return shape switch
        {
            0 => new("builder.OpenComponent<MatrixChild>(0); builder.AddComponentReferenceCapture(1, value => " + field + " = (MatrixChild)value); builder.CloseComponent();", "ref")
            {
                AdditionalExpectedFragment = field,
                Members = "private MatrixChild? " + field + ";",
                ImportCount = 1
            },
            1 => new("builder.OpenComponent<MatrixChild>(0); builder.AddComponentReferenceCapture(1, " + method + "); builder.CloseComponent();", "ref")
            {
                AdditionalExpectedFragment = runtimeMethod,
                Members = "private void " + method + "(object value) { }",
                ImportCount = 1
            },
            2 => new("builder.OpenComponent<MatrixChild>(0); builder.AddComponentReferenceCapture(1, value => " + field + " = (MatrixChild)value); builder.AddComponentReferenceCapture(2, value => " + second + " = (MatrixChild)value); builder.CloseComponent();", "ref")
            {
                AdditionalExpectedFragment = field,
                TertiaryExpectedFragment = second,
                Members = "private MatrixChild? " + field + "; private MatrixChild? " + second + ";",
                ImportCount = 1
            },
            3 => new("builder.OpenComponent<MatrixChild>(0); builder.SetKey(" + CSharpStringLiteral(marker) + "); builder.AddComponentReferenceCapture(1, value => " + field + " = (MatrixChild)value); builder.CloseComponent();", "ref")
            {
                AdditionalExpectedFragment = "key",
                TertiaryExpectedFragment = marker,
                Members = "private MatrixChild? " + field + ";",
                ImportCount = 1
            },
            4 => new("builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Title\", " + CSharpStringLiteral(marker) + "); builder.AddComponentReferenceCapture(2, value => " + field + " = (MatrixChild)value); builder.CloseComponent();", "ref")
            {
                AdditionalExpectedFragment = "heading",
                TertiaryExpectedFragment = marker,
                Members = "private MatrixChild? " + field + ";",
                ImportCount = 1
            },
            5 => new("builder.OpenComponent(0, typeof(MatrixChild)); builder.AddComponentParameter(1, \"Count\", " + hostIndex.ToString(CultureInfo.InvariantCulture) + "); builder.AddComponentReferenceCapture(2, value => " + field + " = (MatrixChild)value); builder.CloseComponent();", "ref")
            {
                AdditionalExpectedFragment = "count",
                TertiaryExpectedFragment = field,
                Members = "private MatrixChild? " + field + ";",
                ImportCount = 1
            },
            6 => new("if (Visible) { builder.OpenComponent<MatrixChild>(0); builder.AddComponentReferenceCapture(1, value => " + field + " = (MatrixChild)value); builder.CloseComponent(); } else { builder.OpenComponent<MatrixChild>(2); builder.AddComponentReferenceCapture(3, value => " + second + " = (MatrixChild)value); builder.CloseComponent(); }", "props.Visible")
            {
                AdditionalExpectedFragment = "ref",
                TertiaryExpectedFragment = field,
                Members = "[Parameter] public bool Visible { get; set; } private MatrixChild? " + field + "; private MatrixChild? " + second + ";",
                UsesProps = true,
                ImportCount = 1
            },
            _ => new("foreach (var item in Items) { builder.OpenComponent<MatrixChild>(0); builder.SetKey(" + CSharpStringLiteral(marker + ":") + " + item); builder.AddComponentReferenceCapture(1, value => " + field + " = (MatrixChild)value); builder.CloseComponent(); }", "Array.from(props.Items ?? []")
            {
                AdditionalExpectedFragment = "ref",
                TertiaryExpectedFragment = marker,
                Members = "[Parameter] public string[] Items { get; set; } = []; private MatrixChild? " + field + ";",
                UsesProps = true,
                ImportCount = 1
            }
        };
    }

    private static CoverageCaseSpec CreateDefaultSlotCase(int shape, int hostIndex, string marker, string tag)
        => CreateSlotCase(shape, hostIndex, marker, tag, "ChildContent", "default", scoped: false);

    private static CoverageCaseSpec CreateNamedSlotCase(int shape, int hostIndex, string marker, string tag)
        => CreateSlotCase(shape, hostIndex, marker, tag, "Header", "header", scoped: false);

    private static CoverageCaseSpec CreateScopedSlotCase(int shape, int hostIndex, string marker, string tag)
        => CreateSlotCase(shape, hostIndex, marker, tag, "ItemTemplate", "item", scoped: true);

    private static CoverageCaseSpec CreateSlotCase(
        int shape,
        int hostIndex,
        string marker,
        string tag,
        string parameterName,
        string runtimeName,
        bool scoped)
    {
        var suffix = runtimeName + shape.ToString("D2", CultureInfo.InvariantCulture) + hostIndex.ToString("D2", CultureInfo.InvariantCulture);
        var fragment = "coverage" + char.ToUpperInvariant(suffix[0]) + suffix[1..];
        var helper = "Build" + char.ToUpperInvariant(fragment[0]) + fragment[1..];
        var property = char.ToUpperInvariant(fragment[0]) + fragment[1..] + "Content";
        var fragmentType = scoped ? "RenderFragment<string>" : "RenderFragment";
        var lambdaStart = scoped ? "value => child => " : "child => ";
        var valueExpression = scoped ? " + value" : "";
        var outerParameterMembers = scoped ? "[Parameter] public string Text { get; set; } = \"\";" : "[Parameter] public string Text { get; set; } = \"\";";
        string body;
        string? additional = runtimeName;
        string? tertiary = null;
        string members = "";
        var usesFragment = false;
        var usesStaticVNode = false;
        var usesProps = false;

        switch (shape)
        {
            case 0:
                body = fragmentType + " " + fragment + " = " + lambdaStart + "child.AddContent(0, " + CSharpStringLiteral(marker + ":") + valueExpression + "); " + OpenSlotComponent(parameterName, fragment);
                tertiary = scoped ? "value" : marker;
                break;
            case 1:
                body = fragmentType + " " + fragment + " = " + lambdaStart + "{ child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker + ":") + valueExpression + "); child.CloseElement(); }; " + OpenSlotComponent(parameterName, fragment);
                tertiary = "h(" + CSharpStringLiteral(tag);
                break;
            case 2:
                body = fragmentType + " " + fragment + " = " + lambdaStart + "{ child.AddContent(0, " + CSharpStringLiteral(marker + "-first:") + valueExpression + "); child.AddContent(1, " + CSharpStringLiteral(marker + "-second:") + valueExpression + "); }; " + OpenSlotComponent(parameterName, fragment);
                tertiary = marker + "-second";
                usesFragment = true;
                break;
            case 3:
                if (scoped)
                {
                    body = fragmentType + " " + fragment + " = value => child => { if (value.Length > " + hostIndex.ToString(CultureInfo.InvariantCulture) + ") { child.AddContent(0, " + CSharpStringLiteral(marker + "-long:") + " + value); } else { child.AddContent(1, " + CSharpStringLiteral(marker + "-short:") + " + value); } }; " + OpenSlotComponent(parameterName, fragment);
                    tertiary = "value.length";
                }
                else
                {
                    body = fragmentType + " " + fragment + " = child => child.AddMarkupContent(0, " + CSharpStringLiteral("<" + tag + ">" + marker + "</" + tag + ">") + "); " + OpenSlotComponent(parameterName, fragment);
                    tertiary = "createStaticVNode";
                    usesStaticVNode = true;
                }
                break;
            case 4:
                if (scoped)
                {
                    body = fragmentType + " " + fragment + " = value => child => child.AddContent(0, Text + " + CSharpStringLiteral(marker + ":") + " + value); " + OpenSlotComponent(parameterName, fragment);
                    tertiary = "props.Text";
                }
                else
                {
                    body = fragmentType + " " + fragment + " = child => { if (Visible) { child.AddContent(0, " + CSharpStringLiteral(marker + "-visible") + "); } else { child.AddContent(1, " + CSharpStringLiteral(marker + "-hidden") + "); } }; " + OpenSlotComponent(parameterName, fragment);
                    tertiary = "props.Visible";
                }
                members = scoped ? outerParameterMembers : "[Parameter] public bool Visible { get; set; }";
                usesProps = true;
                break;
            case 5:
                if (scoped)
                {
                    body = fragmentType + " " + fragment + " = value => child => { foreach (var character in value) { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + character); child.CloseElement(); } }; " + OpenSlotComponent(parameterName, fragment);
                    tertiary = "Array.from(value ?? []";
                }
                else
                {
                    body = fragmentType + " " + fragment + " = child => { foreach (var item in Items) { child.OpenElement(0, " + CSharpStringLiteral(tag) + "); child.AddContent(1, " + CSharpStringLiteral(marker + ":") + " + item); child.CloseElement(); } }; " + OpenSlotComponent(parameterName, fragment);
                    tertiary = "Array.from(props.Items ?? []";
                    members = "[Parameter] public string[] Items { get; set; } = [];";
                    usesProps = true;
                }
                break;
            case 6:
                body = fragmentType + " " + fragment + " = " + helper + "(" + CSharpStringLiteral(marker) + "); " + OpenSlotComponent(parameterName, fragment);
                members = scoped
                    ? "private static RenderFragment<string> " + helper + "(string prefix) => value => child => child.AddContent(0, prefix + value);"
                    : "private static RenderFragment " + helper + "(string value) => child => child.AddContent(0, value);";
                tertiary = marker;
                break;
            default:
                body = OpenSlotComponent(parameterName, property);
                members = scoped
                    ? "private RenderFragment<string> " + property + " => value => child => child.AddContent(0, " + CSharpStringLiteral(marker + ":") + " + value);"
                    : "private RenderFragment " + property + " => child => child.AddContent(0, " + CSharpStringLiteral(marker) + ");";
                tertiary = marker;
                break;
        }

        return new CoverageCaseSpec(body, marker)
        {
            AdditionalExpectedFragment = additional,
            TertiaryExpectedFragment = tertiary,
            Members = members,
            UsesFragment = usesFragment,
            UsesStaticVNode = usesStaticVNode,
            UsesProps = usesProps,
            ImportCount = 1
        };
    }

    private static string OpenSlotComponent(string parameterName, string expression)
        => "builder.OpenComponent<MatrixChild>(2); builder.AddComponentParameter(3, " + CSharpStringLiteral(parameterName) + ", " + expression + "); builder.CloseComponent();";

    private static string NormalizeCoverageEventName(string eventName)
        => "on" + char.ToUpperInvariant(eventName[2]) + eventName[3..];
}
