using static ECMAScript.Vue;

namespace Jero.Web.Pages.Template;

public sealed record TemplateRenderEntry : VueProps
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
