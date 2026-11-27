using System;
using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.FormDesign;

[ECMAScript("element-plus")]
public static class FormDesignerElementPlus
{
#pragma warning disable CS0626 // JavaScript owns this accessor.
    [Description("@#default")]
    public static extern IVuePlugin Plugin { get; }
#pragma warning restore CS0626
}

[ECMAScript]
public sealed record FormDesignerPlugin : IVuePlugin
{
    [Description("@#VFormDesigner")]
    public required IVueComponent Component { get; init; }

    [Description("@#install")]
    public required Action<VueApp> Install { get; init; }
}

[ECMAScript("/components/form-designer.js")]
public static class FormDesignerRuntime
{
#pragma warning disable CS0626 // JavaScript owns this export.
    public static extern IPromise<FormDesignerPlugin> Load();
#pragma warning restore CS0626
}
