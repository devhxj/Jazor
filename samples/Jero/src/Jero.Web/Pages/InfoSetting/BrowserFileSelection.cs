using static ECMAScript.Global;

namespace Jero.Web.Pages.InfoSetting;

[ECMAScriptModule("components/browser-file-selection.js")]
public static class BrowserFileSelection
{
    public static FileRef? Snapshot(string elementId)
    {
        var input = Document.GetElementById(elementId) as HTMLInputElement;
        return input?.Files?.GetItem(0);
    }
}
