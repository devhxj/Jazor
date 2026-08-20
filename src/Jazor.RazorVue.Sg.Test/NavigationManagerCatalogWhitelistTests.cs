using System.Reflection;
using ECMAScript.Contract;
using Jazor.CLR;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class NavigationManagerCatalogWhitelistTests
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
    public void LocationChangedPayload_UsesPlainBrowserObjectAliases()
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

        Assert.AreEqual("location", members["Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.Location.get"].Value);
        Assert.AreEqual("isNavigationIntercepted", members["Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.IsNavigationIntercepted.get"].Value);
    }
}
