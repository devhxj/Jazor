using Monaco = Npm.MonacoEditor.Editor.EditorApiJs.Exports;

namespace Jero.Web.Pages.Plugin;

[ECMAScriptModule("components/plugin-monaco.js")]
public static class PluginMonaco
{
    public static EditorSession Mount(HTMLElement container, string code, Action<string> changed)
    {
        var model = Monaco.Editor.CreateModel(code, "csharp");
        var editor = Monaco.Editor.Create(container, new()
        {
            Theme = "vs-dark",
            FontSize = 14,
            Minimap = new() { Enabled = true },
            WordWrap = Monaco.WordWrap.Off,
            ScrollBeyondLastLine = false,
            AutomaticLayout = true,
            TabSize = 4,
            InsertSpaces = true,
        });
        editor.SetModel(model);
        var listener = model.OnDidChangeContent(_ => changed(model.GetValue()));
        return new(
            () => model.GetValue(),
            text =>
            {
                if (model.GetValue() != text)
                    model.SetValue(text);
            },
            () => { editor.Layout(); editor.Focus(); },
            () =>
            {
                listener.Dispose();
                editor.Dispose();
                model.Dispose();
            });
    }
}

/// <summary>页面通过会话操作当前编辑器，关闭会话时一起释放监听器、编辑器和文本模型。</summary>
public sealed record EditorSession(Func<string> Read, Action<string> Write, Action Focus, Action Close);
