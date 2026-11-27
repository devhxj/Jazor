using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.File;

[ECMAScriptModule("components/file-office-loaders.js")]
public static class FileOfficeLoaderModule
{
    public static IPromise<IVueComponent> LoadDocx() => WithStyle(
        Global.Import<OfficeRenderer>("@vue-office/docx/lib/v3/vue-office-docx.mjs"),
        Global.Import("@vue-office/docx/lib/v3/index.css"));

    public static IPromise<IVueComponent> LoadExcel() => WithStyle(
        Global.Import<OfficeRenderer>("@vue-office/excel/lib/v3/vue-office-excel.mjs"),
        Global.Import("@vue-office/excel/lib/v3/index.css"));

    private static IPromise<IVueComponent> WithStyle(IPromise<OfficeRenderer> renderer, IPromise style)
        => Promise.All(new IPromise[] { renderer, style })
            .Then<OfficeRenderer>(() => renderer)
            .Then<IVueComponent>(module => module.Default);
}

[ECMAScript]
internal sealed class OfficeRenderer
{
#pragma warning disable CS0626 // JavaScript owns the module namespace property.
    [Description("@#default")]
    public extern IVueComponent Default { get; }
#pragma warning restore CS0626
}
