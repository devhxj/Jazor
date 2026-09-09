using Microsoft.AspNetCore.Components;

namespace Jazor.Admin;

/// <summary>Shared styling and attribute surface for Jazor.Admin components.</summary>
public abstract class JComponentBase : ComponentBase, IVueComponent
{
    [Parameter]
    /// <summary>Additional CSS classes appended after the component's framework classes.</summary>
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    /// <summary>Inline style value applied to the component root element.</summary>
    public VueStyleValue? CssStyle { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    /// <summary>Unmatched HTML attributes forwarded to the component root element.</summary>
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Combines stable framework classes with caller classes while preserving Vue class-value shapes.
    /// Framework classes are emitted first so consumer overrides remain deterministic.
    /// </summary>
    protected VueClassValue BuildCssClass(params string[] frameworkClasses)
    {
        ArgumentNullException.ThrowIfNull(frameworkClasses);

        var cssClass = CssClass;
        if (cssClass is null)
        {
            return frameworkClasses.Length switch
            {
                0 => string.Empty,
                1 => frameworkClasses[0],
                _ => frameworkClasses
            };
        }

        var values = new List<VueValue>(frameworkClasses.Length + 4);
        foreach (var frameworkClass in frameworkClasses)
        {
            values.Add(frameworkClass);
        }

        AppendCssClass(values, cssClass.Value);
        return values.ToArray();
    }

    private static void AppendCssClass(List<VueValue> values, VueClassValue cssClass)
    {
        if (cssClass.AsString is { } cssClassString)
        {
            values.Add(cssClassString);
            return;
        }

        if (cssClass.AsStrings is { } cssClassStrings)
        {
            foreach (var cssClassValue in cssClassStrings)
            {
                values.Add(cssClassValue);
            }

            return;
        }

        if (cssClass.AsProps is { } cssClassProps)
        {
            values.Add(cssClassProps);
            return;
        }

        if (cssClass.AsValues is { } cssClassValues)
        {
            foreach (var cssClassValue in cssClassValues)
            {
                values.Add(cssClassValue);
            }
        }
    }
}

/// <summary>Base class for components exposing the standard child-content slot.</summary>
public abstract class JContentComponentBase : JComponentBase
{
    [Parameter]
    /// <summary>Content rendered inside the component's body slot.</summary>
    public RenderFragment? ChildContent { get; set; }
}
