using System.Reflection;
using ECMAScript.Contract;
using Jazor.RazorVue.RazorSdk.Catalog;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class ComponentBaseCatalogWhitelistTests
{
    [TestMethod]
    public void ComponentBaseType_IsAllowed()
    {
        var attribute = typeof(ComponentBaseCatalog).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Allowed, attribute.Op);
        Assert.AreEqual("Microsoft.AspNetCore.Components.ComponentBase", attribute.Member);
    }

    [TestMethod]
    public void ComponentBaseDispatchMembers_MapTheCurrentComponentHostContract()
    {
        var members = typeof(ComponentBaseCatalog)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(static attribute => attribute.Member!, static attribute => attribute.Op);

        Assert.AreEqual(Op.Allowed, members["Microsoft.AspNetCore.Components.ComponentBase.StateHasChanged()"]);
        Assert.AreEqual(Op.Allowed, members["Microsoft.AspNetCore.Components.ComponentBase.InvokeAsync(System.Action)"]);
        Assert.AreEqual(Op.Allowed, members["Microsoft.AspNetCore.Components.ComponentBase.InvokeAsync(System.Func<System.Threading.Tasks.Task>)"]);
    }
}
