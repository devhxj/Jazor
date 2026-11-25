using static ECMAScript.Global;

namespace Jero.Web.App.Template;

/// <summary>Typed DOM boundary for the application-owned rich text editor.</summary>
[ECMAScriptModule("components/template-browser-actions.js")]
public static class TemplateBrowserActions
{
    public static string ReadHtml(string elementId)
    {
        var element = Document.GetElementById(elementId);
        return element is null ? string.Empty : element.InnerHTML.AsString ?? string.Empty;
    }

    public static void ExecuteEditorCommand(string elementId, string command, string value = "")
    {
        var element = Document.GetElementById(elementId) as HTMLElement;
        if (element is null)
            return;

        element.Focus(new FocusOptions(false, false));
        Document.ExecCommand(command, false, value);
    }

    public static void InsertTextAtEnd(string elementId, string text)
    {
        var element = Document.GetElementById(elementId) as HTMLElement;
        if (element is null)
            return;

        element.Focus(new FocusOptions(false, false));
        var range = Document.CreateRange();
        range.SelectNodeContents(element);
        range.Collapse(false);
        var selection = Document.GetSelection();
        if (selection is null)
            return;
        selection.RemoveAllRanges();
        selection.AddRange(range);
        Document.ExecCommand("insertText", false, text);
    }
}
