using ECMAScript;
using Jero.Web.Pages.Shell;

namespace Jero.Web.Pages.Iframe;

[ECMAScript]
public static class IframeShell
{
    [ECMAScript("/components/shell-state.js")]
    public static extern AppIframeDestination? FindIframe(string menuId);
}
