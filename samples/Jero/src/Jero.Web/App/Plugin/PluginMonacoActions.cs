using System.ComponentModel;

namespace Jero.Web.App.Plugin;

/// <summary>Controls the lazy Monaco session owned by the plugin dialog.</summary>
[ECMAScript]
public sealed class PluginMonacoActions
{
    [Description("@#loadEditor")]
    public extern IPromise<PluginMonacoEditorApi> Load();

    [Description("@#create")]
    public extern bool Create(PluginMonacoEditorApi api, string containerId, string value, Action<string> changed);

    [Description("@#getValue")]
    public extern string GetValue(string fallback);

    [Description("@#setValue")]
    public extern void SetValue(string value);

    [Description("@#layoutAndFocus")]
    public extern void LayoutAndFocus();

    [Description("@#dispose")]
    public extern void Dispose();
}

[ECMAScript]
public sealed class PluginMonacoEditorApi;
