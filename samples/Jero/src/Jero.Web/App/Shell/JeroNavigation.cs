global using JeroNavigation = Jero.Web.App.Shell.JeroNavigationModule.JeroNavigation;

namespace Jero.Web.App.Shell;

[ECMAScriptModule("components/jero-navigation.js")]
public static class JeroNavigationModule
{
    private static Action<string, bool> navigate = default!;
    private static Func<string> currentUri = default!;
    private static Func<string> baseUri = default!;

    public static void Bind(Action<string, bool> navigateTo, Func<string> getCurrentUri, Func<string> getBaseUri)
    {
        navigate = navigateTo;
        currentUri = getCurrentUri;
        baseUri = getBaseUri;
    }

    public sealed class JeroNavigation
    {
        public string Uri => currentUri();
        public string BaseUri => baseUri();
        public void NavigateTo(string uri, bool forceLoad = false) => navigate(uri, forceLoad);

        public string ToBaseRelativePath(string uri)
        {
            var appBase = BaseUri;
            return uri.StartsWith(appBase) ? uri[appBase.Length..] : uri;
        }
    }
}

