global using JeroNavigation = Jero.Web.Pages.Shell.JeroNavigationModule.JeroNavigation;

namespace Jero.Web.Pages.Shell;

[ECMAScriptModule("components/jero-navigation.js")]
public static class JeroNavigationModule
{
    private static Action<string, bool> _navigate = default!;
    private static Func<string> _currentUri = default!;
    private static Func<string> _baseUri = default!;

    public static void Bind(Action<string, bool> navigateTo, Func<string> getCurrentUri, Func<string> getBaseUri)
    {
        _navigate = navigateTo;
        _currentUri = getCurrentUri;
        _baseUri = getBaseUri;
    }

    public sealed class JeroNavigation
    {
        public string Uri => _currentUri();
        public string BaseUri => _baseUri();
        public void NavigateTo(string uri, bool forceLoad = false) => _navigate(uri, forceLoad);

        public string ToBaseRelativePath(string uri)
        {
            var appBase = BaseUri;
            return uri.StartsWith(appBase) ? uri[appBase.Length..] : uri;
        }
    }
}

