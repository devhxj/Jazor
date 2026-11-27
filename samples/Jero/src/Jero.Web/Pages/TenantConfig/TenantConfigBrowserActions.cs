using static ECMAScript.Global;

namespace Jero.Web.Pages.TenantConfig;

/// <summary>Typed WebIDL boundary for CSV downloads and print-window rendering.</summary>
[ECMAScriptModule("components/tenant-config-browser-actions.js")]
public static class TenantConfigBrowserActions
{
    public static void DownloadCsv(string content, string fileName)
    {
        var blob = new Blob(["\uFEFF" + content], new BlobPropertyBag("text/csv;charset=utf-8"));
        var url = URL.CreateObjectURL(blob);
        var link = (HTMLAnchorElement)Document.CreateElement("a", (DocumentCreateElementOptions?)null);
        link.Href = url;
        link.Download = fileName;
        link.Click();
        URL.RevokeObjectURL(url);
    }

    public static bool PrintHtml(string html)
    {
        var printWindow = Window.Open(string.Empty, "_blank", string.Empty);
        if (printWindow is null)
            return false;

        printWindow.Document.Write(html);
        printWindow.Document.Close();
        printWindow.Focus();
        printWindow.Print();
        return true;
    }
}
