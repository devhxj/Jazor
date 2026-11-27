using static ECMAScript.Global;

namespace Jero.Web.Pages.StressTest;

/// <summary>Typed DOM boundary for the dynamic stress-test parameter inputs.</summary>
[ECMAScriptModule("components/stress-test-browser-actions.js")]
public static class StressTestBrowserActions
{
    public static string ReadInputValue(string elementId)
    {
        var input = Document.GetElementById(elementId) as HTMLInputElement;
        return input?.Value ?? string.Empty;
    }
}
