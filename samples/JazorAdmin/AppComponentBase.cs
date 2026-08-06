namespace JazorAdmin;

[ECMAScript]
public abstract class AppComponentBase : ComponentBase, IVueComponent
{
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    public VueStyleValue? CssStyle { get; set; }

    protected VueClassValue BuildCssClass(params string[] componentClasses)
    {
        ArgumentNullException.ThrowIfNull(componentClasses);

        var cssClass = CssClass;
        if (cssClass is null)
        {
            return componentClasses.Length switch
            {
                0 => string.Empty,
                1 => componentClasses[0],
                _ => componentClasses
            };
        }

        var values = new List<VueValue>(componentClasses.Length + 4);
        foreach (var componentClass in componentClasses)
        {
            values.Add(componentClass);
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
