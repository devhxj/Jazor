using System.Reflection;
using ECMAScript;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class DataTransferItemMappingTests
{
    [TestMethod]
    public void DataTransferItemModule_ExposesNativeReadSurfaceAndRejectsDtoMutation()
    {
        var module = typeof(DataTransferItemModule);
        var typeMapping = module.GetCustomAttribute<JazorAttribute>();
        Assert.IsNotNull(typeMapping);
        Assert.AreEqual(Op.Alias, typeMapping.Op);
        Assert.AreEqual("Microsoft.AspNetCore.Components.Web.DataTransferItem", typeMapping.Member);
        Assert.AreEqual("DataTransferItem", typeMapping.Value);

        var mappings = module.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => method.GetCustomAttribute<JazorAttribute>())
            .Where(static mapping => mapping is not null)
            .ToDictionary(static mapping => mapping!.Member, static mapping => mapping!, StringComparer.Ordinal);
        Assert.HasCount(5, mappings);
        Assert.AreEqual(Op.Inline, mappings["Microsoft.AspNetCore.Components.Web.DataTransferItem.Kind.get"].Op);
        Assert.AreEqual("__arg1.kind", mappings["Microsoft.AspNetCore.Components.Web.DataTransferItem.Kind.get"].Value);
        Assert.AreEqual(Op.Inline, mappings["Microsoft.AspNetCore.Components.Web.DataTransferItem.Type.get"].Op);
        Assert.AreEqual("__arg1.type", mappings["Microsoft.AspNetCore.Components.Web.DataTransferItem.Type.get"].Value);
        Assert.AreEqual(Op.Discard, mappings["Microsoft.AspNetCore.Components.Web.DataTransferItem.Kind.set"].Op);
        Assert.AreEqual(Op.Discard, mappings["Microsoft.AspNetCore.Components.Web.DataTransferItem.Type.set"].Op);
        Assert.AreEqual(Op.Discard, mappings["Microsoft.AspNetCore.Components.Web.DataTransferItem.DataTransferItem()"].Op);
    }

    [TestMethod]
    public void DataTransferItemModule_GettersUseWebIdlItemAndStringCarriers()
    {
        foreach (var name in new[] { nameof(DataTransferItemModule.GetKind), nameof(DataTransferItemModule.GetType) })
        {
            var method = typeof(DataTransferItemModule).GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(string), method.ReturnType, name);
            Assert.HasCount(1, method.GetParameters());
            Assert.AreEqual(typeof(DataTransferItem), method.GetParameters()[0].ParameterType, name);
        }
    }
}
