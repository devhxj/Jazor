using System.ComponentModel;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Plugin;

[ECMAScriptModule("components/plugin-monaco-loader.js")]
public static class PluginMonacoLoader
{
    public static IPromise<PluginMonacoExports> Load()
    {
        // 首次打开编辑器时并行加载模块，等 C# 语言注册结束后再创建模型。
        var editor = Import<PluginMonacoExports>("/components/plugin-monaco.js");
        var language = Import<CSharpLanguageRegistration>("monaco-editor/languages/definitions/csharp/register.js");
        return Promise.All(editor, language).Then<PluginMonacoExports>(() => editor);
    }
}

[ECMAScript]
public sealed class PluginMonacoExports
{
    [Description("@#Mount")]
    public extern EditorSession Mount(HTMLElement container, string code, Action<string> changed);
}

[ECMAScript]
internal sealed class CSharpLanguageRegistration;
