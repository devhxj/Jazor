using static ECMAScript.Global;

namespace Jero.Web.App.Notice;

/// <summary>Typed DOM boundary for notice rich text editing and summaries.</summary>
[ECMAScriptModule("components/notice-browser-actions.js")]
public static class NoticeBrowserActions
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

    public static string ToPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return "-";

        var element = (HTMLElement)Document.CreateElement("div", (DocumentCreateElementOptions?)null);
        element.InnerHTML = html;
        var text = element.InnerText.Trim();
        return text.Length == 0 ? "-" : text;
    }
}
