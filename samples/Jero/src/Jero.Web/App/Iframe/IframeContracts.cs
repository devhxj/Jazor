using ECMAScript;
using Jero.Web.App.Shell;

namespace Jero.Web.App.Iframe;

[ECMAScript]
public static class IframeShell
{
    [ECMAScript("@jero/runtime/components/shell-state.js")]
    public static extern AppIframeDestination? FindIframe(string menuId);
}
