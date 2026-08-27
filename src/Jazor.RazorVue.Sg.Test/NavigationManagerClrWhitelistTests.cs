using System.Reflection;
using ECMAScript.Contract;
using Jazor.CLR;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class NavigationManagerClrWhitelistTests
{
    [TestMethod]
    public void NavigationManagerMembers_AreDeclaredAsBrowserAliases()
    {
        var attributes = typeof(NavigationManagerModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(static attribute => attribute.Member!, static attribute => attribute);

        Assert.AreEqual(Op.Import, attributes["Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, bool)"].Op);
        Assert.AreEqual("navigateToForceLoad", attributes["Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, bool)"].Value);
        Assert.AreEqual("toAbsoluteUri", attributes["Microsoft.AspNetCore.Components.NavigationManager.ToAbsoluteUri(string)"].Value);
        Assert.AreEqual("getHistoryEntryState", attributes["Microsoft.AspNetCore.Components.NavigationManager.HistoryEntryState.get"].Value);

        // OnNotFound and NotFound() own an invocation list plus event-args construction, so they
        // stay module imports instead of collapsing into inline templates.
        var notFoundAdd = attributes["Microsoft.AspNetCore.Components.NavigationManager.OnNotFound.add"];
        Assert.AreEqual(Op.Import, notFoundAdd.Op);
        Assert.AreEqual("addOnNotFound", notFoundAdd.Value);

        var notFoundRemove = attributes["Microsoft.AspNetCore.Components.NavigationManager.OnNotFound.remove"];
        Assert.AreEqual(Op.Import, notFoundRemove.Op);
        Assert.AreEqual("removeOnNotFound", notFoundRemove.Value);

        var notFound = attributes["Microsoft.AspNetCore.Components.NavigationManager.NotFound()"];
        Assert.AreEqual(Op.Import, notFound.Op);
        Assert.AreEqual("notFound", notFound.Value);

        // Registration owns an invocation list and hands back an IDisposable, so it stays an import.
        var registerLocationChanging = attributes[
            "Microsoft.AspNetCore.Components.NavigationManager.RegisterLocationChangingHandler(System.Func<Microsoft.AspNetCore.Components.Routing.LocationChangingContext, System.Threading.Tasks.ValueTask>)"];
        Assert.AreEqual(Op.Import, registerLocationChanging.Op);
        Assert.AreEqual("registerLocationChangingHandler", registerLocationChanging.Value);

        var extensionAttributes = typeof(NavigationManagerExtensionsModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(static attribute => attribute.Member!, static attribute => attribute);

        var queryKey = "static Microsoft.AspNetCore.Components.NavigationManagerExtensions.GetUriWithQueryParameter(Microsoft.AspNetCore.Components.NavigationManager, string, string)";
        Assert.AreEqual(Op.Import, extensionAttributes[queryKey].Op);
        Assert.AreEqual("getUriWithQueryParameterString", extensionAttributes[queryKey].Value);
        Assert.IsNull(extensionAttributes[queryKey].ModulePath);
    }

    [TestMethod]
    public void LocationChangedPayload_UsesPlainBrowserObjectInlineAccessors()
    {
        var typeAttribute = typeof(LocationChangedEventArgsModule)
            .GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(typeAttribute);
        Assert.AreEqual(Op.Alias, typeAttribute.Op);
        Assert.AreEqual("Object", typeAttribute.Value);

        var members = typeof(LocationChangedEventArgsModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(static attribute => attribute.Member!, static attribute => attribute);

        var location = members["Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.Location.get"];
        Assert.AreEqual(Op.Inline, location.Op);
        Assert.AreEqual("__arg1.location", location.Value);

        var intercepted = members["Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.IsNavigationIntercepted.get"];
        Assert.AreEqual(Op.Inline, intercepted.Op);
        Assert.AreEqual("__arg1.isNavigationIntercepted", intercepted.Value);

        var historyEntryState = members["Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.HistoryEntryState.get"];
        Assert.AreEqual(Op.Inline, historyEntryState.Op);
        Assert.AreEqual("__arg1.historyEntryState", historyEntryState.Value);
    }

    [TestMethod]
    public void NotFoundPayload_UsesPlainBrowserObjectWithExplicitPathField()
    {
        var typeAttribute = typeof(NotFoundEventArgsModule).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(typeAttribute);
        Assert.AreEqual(Op.Alias, typeAttribute.Op);
        Assert.AreEqual("Object", typeAttribute.Value);

        var members = Members(typeof(NotFoundEventArgsModule));

        // The constructor writes the field so a freshly dispatched payload reads as null instead
        // of JavaScript undefined; the property itself is a plain name remap.
        var constructor = members["Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs.NotFoundEventArgs()"];
        Assert.AreEqual(Op.Import, constructor.Op);
        Assert.AreEqual("createNotFoundEventArgs", constructor.Value);

        Assert.AreEqual(Op.Alias, members["Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs.Path.get"].Op);
        Assert.AreEqual("path", members["Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs.Path.get"].Value);
        Assert.AreEqual(Op.Alias, members["Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs.Path.set"].Op);
        Assert.AreEqual("path", members["Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs.Path.set"].Value);
    }

    [TestMethod]
    public void NavigationOptions_ConstructorSpellsOutEveryClrDefault()
    {
        var typeAttribute = typeof(NavigationOptionsModule).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(typeAttribute);
        Assert.AreEqual(Op.Alias, typeAttribute.Op);
        Assert.AreEqual("Object", typeAttribute.Value);

        var constructor = Members(typeof(NavigationOptionsModule))[
            "Microsoft.AspNetCore.Components.NavigationOptions.NavigationOptions()"];
        Assert.AreEqual(Op.Inline, constructor.Op);
        Assert.AreEqual(
            "({ forceLoad: false, replaceHistoryEntry: false, relativeToCurrentUri: false, historyEntryState: null })",
            constructor.Value);
    }

    [TestMethod]
    public void LocationChangingContext_MapsInitOnlyFieldsOverAPlainBrowserObject()
    {
        var typeAttribute = typeof(LocationChangingContextModule).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(typeAttribute);
        Assert.AreEqual(Op.Alias, typeAttribute.Op);
        Assert.AreEqual("Object", typeAttribute.Value);

        var members = Members(typeof(LocationChangingContextModule));

        var constructor = members["Microsoft.AspNetCore.Components.Routing.LocationChangingContext.LocationChangingContext()"];
        // CancellationToken 的 CLR 默认值是 CancellationToken.None，它必须落在取消链共享的
        // never-abort 单例上，因此默认上下文无法压成一个自足的对象字面量。
        Assert.AreEqual(Op.Import, constructor.Op);
        Assert.AreEqual("createDefault", constructor.Value);

        foreach (var (member, field) in new[]
        {
            ("TargetLocation", "targetLocation"),
            ("HistoryEntryState", "historyEntryState"),
            ("IsNavigationIntercepted", "isNavigationIntercepted"),
        })
        {
            foreach (var accessor in new[] { "get", "init" })
            {
                var key = $"Microsoft.AspNetCore.Components.Routing.LocationChangingContext.{member}.{accessor}";
                Assert.AreEqual(Op.Alias, members[key].Op, key);
                Assert.AreEqual(field, members[key].Value, key);
            }
        }

        // PreventNavigation() writes a private marker the navigation side reads after every handler
        // settles, so it owns runtime behavior instead of a name remap.
        var prevent = members["Microsoft.AspNetCore.Components.Routing.LocationChangingContext.PreventNavigation()"];
        Assert.AreEqual(Op.Import, prevent.Op);
        Assert.AreEqual("preventNavigation", prevent.Value);

        // dispatch 的取消 token 就是宿主 AbortSignal，因此这里只是字段名改写。
        var cancellationToken =
            members["Microsoft.AspNetCore.Components.Routing.LocationChangingContext.CancellationToken.get"];
        Assert.AreEqual(Op.Alias, cancellationToken.Op);
        Assert.AreEqual("cancellationToken", cancellationToken.Value);
    }

    [TestMethod]
    public void UriMembers_LowerToTheBrowserUrlCarrier()
    {
        var typeAttribute = typeof(UriModule).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(typeAttribute);
        Assert.AreEqual(Op.Alias, typeAttribute.Op);
        Assert.AreEqual("URL", typeAttribute.Value);

        var members = Members(typeof(UriModule));

        Assert.AreEqual("new URL(__arg1)", members["System.Uri.Uri(string)"].Value);
        Assert.AreEqual("new URL(__arg2, __arg1.href)", members["System.Uri.Uri(System.Uri, string)"].Value);
        Assert.AreEqual("href", members["System.Uri.AbsoluteUri.get"].Value);
        Assert.AreEqual("pathname", members["System.Uri.AbsolutePath.get"].Value);
        Assert.AreEqual("search", members["System.Uri.Query.get"].Value);
        Assert.AreEqual("hash", members["System.Uri.Fragment.get"].Value);
        // Uri.Host drops the port while Uri.Authority keeps it, matching hostname vs host.
        Assert.AreEqual("hostname", members["System.Uri.Host.get"].Value);
        Assert.AreEqual("host", members["System.Uri.Authority.get"].Value);
        Assert.AreEqual("__arg1.protocol.slice(0, -1)", members["System.Uri.Scheme.get"].Value);
        Assert.AreEqual("__arg1.href", members["override System.Uri.ToString()"].Value);

        // Both of these read the receiver more than once or branch on the protocol, so they stay
        // module imports rather than inline templates.
        Assert.AreEqual(Op.Import, members["System.Uri.PathAndQuery.get"].Op);
        Assert.AreEqual("getPathAndQuery", members["System.Uri.PathAndQuery.get"].Value);
        Assert.AreEqual(Op.Import, members["System.Uri.Port.get"].Op);
        Assert.AreEqual("getPort", members["System.Uri.Port.get"].Value);
    }

    private static Dictionary<string, JazorAttribute> Members(Type module)
        => module
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(static attribute => attribute.Member!, static attribute => attribute);
}
