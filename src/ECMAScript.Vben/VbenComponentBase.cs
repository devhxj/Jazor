using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben;

public abstract class VbenComponentBase : ComponentBase, IVueComponent
{
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    public VueStyleValue? CssStyle { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

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

public abstract class VbenContentComponentBase : VbenComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
