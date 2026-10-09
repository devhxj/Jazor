using System.Reflection;
using ECMAScript;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class InputFileChangeEventArgsMappingTests
{
    [TestMethod]
    public void NativeFileEventMapping_UsesEventCarrierAndOnlyFileCountFromClrSurface()
    {
        var module = typeof(InputFileChangeEventArgsModule);
        var mapping = module.GetCustomAttribute<JazorAttribute>()!;
        Assert.AreEqual(Op.Alias, mapping.Op);
        Assert.AreEqual("Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs", mapping.Member);
        Assert.AreEqual("EventRef", mapping.Value);
        Assert.IsFalse(module.IsDefined(typeof(ECMAScriptModuleAttribute), inherit: false));

        var members = module.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.IsDefined(typeof(JazorAttribute), inherit: false))
            .ToDictionary(method => method.GetCustomAttribute<JazorAttribute>()!.Member);
        var count = members["Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs.FileCount.get"];
        var countMapping = count.GetCustomAttribute<JazorAttribute>()!;
        Assert.AreEqual(Op.Inline, countMapping.Op);
        Assert.AreEqual("__arg1.target.files.length", countMapping.Value);
        Assert.AreEqual(typeof(Number), count.ReturnType);
        Assert.AreEqual(typeof(EventRef), count.GetParameters().Single().ParameterType);

        Assert.IsTrue(members.Where(pair => pair.Value != count)
            .All(pair => pair.Value.GetCustomAttribute<JazorAttribute>()!.Op == Op.Discard));
        CollectionAssert.AreEquivalent(
            new[]
            {
                "Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs.FileCount.get",
                "Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs.File.get",
                "Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs.GetMultipleFiles(int)",
                "Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs.InputFileChangeEventArgs(System.Collections.Generic.IReadOnlyList<Microsoft.AspNetCore.Components.Forms.IBrowserFile>)"
            },
            members.Keys.ToArray());
    }
}
