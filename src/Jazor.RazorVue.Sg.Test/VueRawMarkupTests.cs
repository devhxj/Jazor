using System.Collections;
using System.Reflection;
using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Verifies raw-markup HTML facts and the optional runtime-provider contract.
/// 覆盖 hydration cardinality 与 provider ABI，避免只断言最终 JavaScript 字符串。
/// </summary>
[TestClass]
public sealed class VueRawMarkupTests
{
    [TestMethod]
    [DataRow("", 0, false)]
    [DataRow("plain", 1, true)]
    [DataRow("<strong>one</strong>", 1, true)]
    [DataRow("<strong>one</strong><em>two</em>", 2, true)]
    [DataRow("text<span>two</span>", 2, true)]
    [DataRow("<!--lead--><span>two</span>", 2, false)]
    [DataRow("<template><span>nested</span></template><b>end</b>", 2, true)]
    [DataRow("<table><tr><td>cell</td></tr></table>", 1, true)]
    [DataRow("<svg><circle /></svg><math><mi>x</mi></math>", 2, true)]
    public void AnalyzeStatic_UsesHtmlFragmentCardinality(
        string markup,
        int expectedCount,
        bool expectedStaticHydration)
    {
        var result = VueRawMarkup.AnalyzeStatic(markup);

        Assert.AreEqual(expectedCount, result.NodeCount);
        Assert.AreEqual(expectedStaticHydration, result.CanHydrateAsStaticVNode);
    }

    [TestMethod]
    public void RuntimeProviderCatalog_ExposesOnlyRawMarkupRuntime()
    {
        var assembly = typeof(VueRawMarkup).Assembly;
        var catalog = assembly.GetType("Jazor.Artifacts.RuntimeProviderCatalog", throwOnError: true)!;
        Assert.AreEqual(1, ReadStatic<int>(catalog, "SchemaVersion"));
        Assert.AreEqual("jazor.vue", ReadStatic<string>(catalog, "ProviderId"));

        var modules = InvokeEnumerable(catalog, "GetModules").Cast<object>().ToArray();
        Assert.HasCount(1, modules);
        Assert.AreEqual("@jazor/vue-runtime/raw-markup.mjs", ReadProperty<string>(modules[0], "RelativePath"));
        Assert.AreEqual("Jazor.RazorVue.Runtime.raw-markup.mjs", ReadProperty<string>(modules[0], "ResourceName"));
        Assert.IsFalse(
            assembly.GetManifestResourceNames().Any(static name => name.Contains("render-context", StringComparison.Ordinal)),
            "The retired render-context runtime must not return as a provider dependency.");
    }

    private static IEnumerable InvokeEnumerable(Type type, string methodName)
        => type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!
            .Invoke(null, null) as IEnumerable
            ?? throw new InvalidOperationException($"{type.FullName}.{methodName} returned null.");

    private static T ReadStatic<T>(Type type, string fieldName)
        => (T)(type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null)
            ?? throw new InvalidOperationException($"Static field '{fieldName}' was not found."));

    private static T ReadProperty<T>(object value, string propertyName)
        => (T)(value.GetType().GetProperty(propertyName)?.GetValue(value)
            ?? throw new InvalidOperationException($"Property '{propertyName}' was not found."));
}
