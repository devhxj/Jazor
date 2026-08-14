using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.VueDataUi.Test;

[TestClass]
public sealed class VueDataUiProxyTests
{
    [TestMethod]
    public void VueDataUi_ComponentDescriptorsUsePerChartEntriesPresentInManifest()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetManifestPath()));
        var imports = manifest.RootElement.GetProperty("imports");
        var componentTypes = typeof(VueUiXy).Assembly
            .GetExportedTypes()
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<VueLibraryComponentAttribute>()))
            .Where(static item => item.Attribute is not null)
            .OrderBy(static item => item.Type.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.IsTrue(componentTypes.Length >= 20, "The initial catalog must cover the major chart families.");
        Assert.IsFalse(imports.TryGetProperty("vue-data-ui", out _), "The root entry eagerly aggregates the whole library.");

        foreach (var (type, attribute) in componentTypes)
        {
            Assert.IsNotNull(attribute);
            Assert.IsTrue(
                attribute!.ImportSpecifier.StartsWith("vue-data-ui/vue-ui-", StringComparison.Ordinal),
                $"{type.Name} must use a per-component vue-data-ui entry.");
            Assert.IsTrue(
                imports.TryGetProperty(attribute.ImportSpecifier, out _),
                $"manifest.json is missing {attribute.ImportSpecifier} for {type.Name}.");
            Assert.AreEqual(type.Name, attribute.ExportName, type.Name);
        }

        Assert.AreEqual("3.23.4", manifest.RootElement.GetProperty("version").GetString());
        Assert.AreEqual("dist/jspdf.es.min.js", imports.GetProperty("jspdf").GetProperty("production").GetString());
        CollectionAssert.Contains(
            manifest.RootElement.GetProperty("styles").EnumerateArray().Select(static value => value.GetString()).ToArray(),
            "dist/style.css");
    }

    [TestMethod]
    public void VueDataUi_PublicAuthoringSurfaceHasNoObjectCatchAlls()
    {
        var assembly = typeof(VueDataUiConfig).Assembly;
        foreach (var type in assembly.GetExportedTypes().Where(static type => !type.Name.StartsWith('<')))
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var property in type.GetProperties(flags))
            {
                if (IsNativeUnionValue(property))
                    continue;

                AssertNotObject(property.PropertyType, type.FullName + "." + property.Name);
            }

            foreach (var method in type.GetMethods(flags)
                         .Where(static method => !method.IsSpecialName)
                         .Where(static method => method.Name is not ("Equals" or "GetHashCode" or "ToString" or "PrintMembers" or "<Clone>$")))
            {
                AssertNotObject(method.ReturnType, type.FullName + "." + method.Name + " return");
                foreach (var parameter in method.GetParameters())
                    AssertNotObject(parameter.ParameterType, type.FullName + "." + method.Name + "(" + parameter.Name + ")");
            }
        }
    }

    [TestMethod]
    public void VueDataUi_NativeUnionsKeepErasedValueBranchesPrecise()
    {
        AssertNativeUnion(typeof(VueDataUiCellValue), typeof(string), typeof(double));
        AssertNativeUnion(typeof(VueUiXySeriesValues), typeof(double?[]), typeof(VueUiXyCoordinate[]));
        AssertNativeUnion(typeof(VueUiWordCloudDataset), typeof(VueUiWordCloudDatasetItem[]), typeof(string));
        AssertNativeUnion(typeof(VueUiQuickChartDataset), typeof(double?[]), typeof(VueUiQuickChartDatasetItem), typeof(VueUiQuickChartDatasetItem[]));
    }

    [TestMethod]
    public void VueDataUi_ConfigExtensibilityRemainsStructured()
    {
        Assert.IsTrue(typeof(Vue.VueDictionary<Vue.VueValue>).IsAssignableFrom(typeof(VueDataUiConfig)));
        Assert.IsTrue(typeof(Vue.VueDictionary<Vue.VueValue>).IsAssignableFrom(typeof(VueDataUiDatasetItem)));
        Assert.AreEqual(typeof(VueDataUiConfig), typeof(VueUiDonutConfig).BaseType);
        Assert.AreEqual(typeof(VueDataUiConfig), typeof(VueUiXyConfig).BaseType);
    }

    [TestMethod]
    public void VueDataUi_DonutLegendCallbackUsesTheUpstreamSummaryPayload()
    {
        var callback = typeof(VueUiDonut).GetProperty(nameof(VueUiDonut.OnSelectLegend));
        Assert.IsNotNull(callback);
        Assert.AreEqual(typeof(EventCallback<VueUiDonutLegendItem[]>), callback!.PropertyType);

        var name = callback.GetCustomAttribute<ECMAScriptNameAttribute>();
        Assert.IsNotNull(name);
        Assert.AreEqual("onSelectLegend", name!.Name);

        Assert.AreEqual(typeof(string), typeof(VueUiDonutLegendItem).GetProperty(nameof(VueUiDonutLegendItem.Color))!.PropertyType);
        Assert.AreEqual(typeof(string), typeof(VueUiDonutLegendItem).GetProperty(nameof(VueUiDonutLegendItem.Name))!.PropertyType);
        Assert.AreEqual(typeof(double), typeof(VueUiDonutLegendItem).GetProperty(nameof(VueUiDonutLegendItem.Value))!.PropertyType);
    }

    [TestMethod]
    public void VueDataUi_ChartDescriptorsKeepTheirSpecializedDatasetTypes()
    {
        AssertDatasetType(typeof(VueUiHorizontalBar), typeof(VueUiHorizontalBarDatasetItem[]));
        AssertDatasetType(typeof(VueUiTableHeatmap), typeof(VueUiTableHeatmapDatasetItem[]));
        AssertDatasetType(
            typeof(VueUiTableSparkline),
            typeof(VueUiTableSparklineDatasetItem[]),
            typeof(VueDataUiRequiredConfigChartComponent<,>));
        AssertDatasetType(typeof(VueUiCandlestick), typeof(VueDataUiCellValue[][]));

        Assert.AreEqual(typeof(VueDataUiCellValue?[]), typeof(VueUiTableHeatmapDatasetItem)
            .GetProperty(nameof(VueUiTableHeatmapDatasetItem.Values))!.PropertyType);
        Assert.AreEqual(typeof(double?[]), typeof(VueUiTableSparklineDatasetItem)
            .GetProperty(nameof(VueUiTableSparklineDatasetItem.Values))!.PropertyType);

        var ohlc = typeof(VueUiCandlestickData).GetMethod(nameof(VueUiCandlestickData.Ohlc));
        Assert.IsNotNull(ohlc);
        Assert.AreEqual(typeof(VueDataUiCellValue[]), ohlc!.ReturnType);
        Assert.AreEqual(
            "[__arg1, __arg2, __arg3, __arg4, __arg5, __arg6]",
            ohlc.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

        Assert.IsNotNull(typeof(VueDataUiChartComponent<,>).GetProperty(nameof(VueDataUiChartComponent<int, VueUiDonutConfig>.Dataset))
            ?.GetCustomAttribute<EditorRequiredAttribute>());
        Assert.IsNotNull(typeof(VueDataUiRequiredConfigChartComponent<,>)
            .GetProperty(nameof(VueDataUiRequiredConfigChartComponent<int, VueUiTableSparklineConfig>.Config))
            ?.GetCustomAttribute<EditorRequiredAttribute>());
    }

    private static string GetManifestPath([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(sourceFilePath)!,
            "..",
            "ECMAScript.VueDataUi",
            "manifest.json"));

    private static bool IsNativeUnionValue(PropertyInfo property)
        => property.Name == nameof(IUnion.Value) &&
           property.DeclaringType is not null &&
           typeof(IUnion).IsAssignableFrom(property.DeclaringType);

    private static void AssertNotObject(Type type, string displayName)
    {
        Assert.AreNotEqual(typeof(object), Nullable.GetUnderlyingType(type) ?? type, displayName);

        if (type.IsArray)
        {
            AssertNotObject(type.GetElementType()!, displayName);
            return;
        }

        if (!type.IsGenericType)
            return;

        foreach (var argument in type.GetGenericArguments().Where(static argument => !argument.IsGenericParameter))
            AssertNotObject(argument, displayName);
    }

    private static void AssertNativeUnion(Type type, params Type[] branches)
    {
        Assert.IsNotNull(type.GetCustomAttribute<UnionAttribute>(), type.FullName);
        Assert.IsTrue(typeof(IUnion).IsAssignableFrom(type), type.FullName);
        CollectionAssert.AreEquivalent(
            branches,
            type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(static constructor => constructor.GetParameters().SingleOrDefault()?.ParameterType)
                .Where(static parameter => parameter is not null)
                .ToArray(),
            type.FullName);
    }

    private static void AssertDatasetType(
        Type componentType,
        Type expectedDatasetType,
        Type? expectedBaseDefinition = null)
    {
        var chartBase = componentType.BaseType;
        Assert.IsNotNull(chartBase, componentType.FullName);
        Assert.AreEqual(expectedBaseDefinition ?? typeof(VueDataUiChartComponent<,>), chartBase!.GetGenericTypeDefinition());
        Assert.AreEqual(expectedDatasetType, chartBase.GetGenericArguments()[0]);
    }
}
