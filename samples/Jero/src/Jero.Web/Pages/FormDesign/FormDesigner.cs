using System;
using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Global;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.FormDesign;

[ECMAScriptModule("components/form-designer.js")]
[Style("/Pages/FormDesign/runtime/designer.style.css")]
public static class FormDesigner
{
    private static IPromise<FormDesignerPlugin>? _pending;

    public static IPromise<FormDesignerPlugin> Load() => _pending ??= Import<IVueNamespace>("vue").Then<FormDesignerPlugin>(LoadScript);

    private static IPromise<FormDesignerPlugin> LoadScript(IVueNamespace vue)
    {
        return new Promise<FormDesignerPlugin>((Action<FormDesignerPlugin> resolve, Action<Error> reject) =>
        {
            var script = (HTMLScriptElement)Document.CreateElement("script", (DocumentCreateElementOptions?)null);
            script.Src = FormDesignerAsset.Url;

            // 传统 UMD 脚本与宿主应用共用同一个 Vue 命名空间。
            var hadVue = Object.HasOwn(Window, "Vue");
            var previousVue = Reflect.Get(Window, "Vue");
            Reflect.Set(Window, "Vue", vue);

            void RestoreVue()
            {
                if (hadVue)
                    Reflect.Set(Window, "Vue", previousVue);
                else
                    Reflect.DeleteProperty(Window, "Vue");
                script.Remove();
            }

            script.Onload = _ =>
            {
                RestoreVue();
                var plugin = (FormDesignerPlugin?)Reflect.Get(Window, "VFormDesigner") ?? null;
                if (plugin is null || TypeOf(plugin.Install) != "function" || IsUndefined(plugin.Component) || plugin.Component is null)
                    reject(new Error("VForm 3 runtime did not expose a valid designer plugin."));
                else
                    resolve(plugin);
                return null!;
            };
            script.Onerror = (message, source, line, column, error) =>
            {
                RestoreVue();
                reject(new Error("VForm 3 runtime could not be loaded."));
                return null!;
            };
            Document.Head!.AppendChild(script);
        });
    }
}

[ECMAScript]
public interface IVueNamespace;

[ECMAScript("/Pages/FormDesign/runtime/designer.umd.js?url&no-inline")]
internal static class FormDesignerAsset
{
    [Description("@#default")]
#pragma warning disable CS0626 // JavaScript owns this accessor.
    public static extern string Url { get; }
#pragma warning restore CS0626
}
