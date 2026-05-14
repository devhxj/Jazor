using System.Reflection;
using ECMAScript;
using ECMAScript.ElementPlus;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class ElementPlusSharedContractTests
{
    [TestMethod]
    public void ElementPlus_BaseAuthoringSurface_UsesSharedVueCssContracts()
    {
        Assert.AreEqual(typeof(VueClassValue?), typeof(ElementPlusComponentBase).GetProperty(nameof(ElementPlusComponentBase.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VueStyleValue?), typeof(ElementPlusComponentBase).GetProperty(nameof(ElementPlusComponentBase.CssStyle))?.PropertyType);

        var additionalAttributes = typeof(ElementPlusComponentBase).GetProperty(nameof(ElementPlusComponentBase.AdditionalAttributes));
        Assert.IsNotNull(additionalAttributes);
        Assert.AreEqual(typeof(IReadOnlyDictionary<string, object?>), additionalAttributes!.PropertyType);

        var parameter = additionalAttributes.GetCustomAttribute<ParameterAttribute>(inherit: true);
        Assert.IsNotNull(parameter);
        Assert.IsTrue(parameter!.CaptureUnmatchedValues);
    }

    [TestMethod]
    public void ElementPlus_CommonOptionShapes_ReuseSharedVueUnions()
    {
        Assert.AreEqual(typeof(VueTeleportTarget?), typeof(ElementPlusLoadingOptions).GetProperty(nameof(ElementPlusLoadingOptions.Target))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanStringValue?), typeof(ElementPlusLinkConfig).GetProperty(nameof(ElementPlusLinkConfig.Underline))?.PropertyType);
    }

    [TestMethod]
    public void ElementPlus_ComponentContracts_ReuseSharedVueUnions()
    {
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElDialog).GetProperty(nameof(ElDialog.Width))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanStringNumberValue?), typeof(ElSwitch).GetProperty(nameof(ElSwitch.ModelValue))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanStringNumberValue?), typeof(ElSwitch).GetProperty(nameof(ElSwitch.ActiveValue))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanStringNumberValue?), typeof(ElSwitch).GetProperty(nameof(ElSwitch.InactiveValue))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElInput).GetProperty(nameof(ElInput.ModelValue))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElInput).GetProperty(nameof(ElInput.Minlength))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElInputOtp).GetProperty(nameof(ElInputOtp.ModelValue))?.PropertyType);
        Assert.AreEqual(typeof(string[]), typeof(ElInputTag).GetProperty(nameof(ElInputTag.ModelValue))?.PropertyType);
    }

    [TestMethod]
    public void ElementPlus_PublicNamespace_DoesNotReintroduceDuplicatedCommonVueUnions()
    {
        var publicTypeNames = typeof(ElementPlus).Assembly
            .GetExportedTypes()
            .Where(static type => type.Namespace == "ECMAScript.ElementPlus")
            .Select(static type => type.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusBooleanStringValue");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusStringOrComponent");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusSelectorOrElement");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusCssStyleValue");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusCssStyleValues");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusStringNumberValue");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusStringArray");
    }

    [TestMethod]
    public void ElementPlus_ImportHosts_UseExpectedEntrypoints()
    {
        Assert.AreEqual("npm:element-plus", GetEcmaScriptImport(typeof(ElementPlus)));
        Assert.AreEqual("element-plus", GetEcmaScriptImport(typeof(ElementPlusComponents)));
        Assert.AreEqual("element-plus", GetEcmaScriptImport(typeof(ElementPlusDirectives)));
    }

    private static string? GetEcmaScriptImport(Type type)
        => type.GetCustomAttributesData()
            .SingleOrDefault(static attribute => attribute.AttributeType == typeof(ECMAScriptAttribute))
            ?.ConstructorArguments
            .FirstOrDefault()
            .Value as string;
}
