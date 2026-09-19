using System.Reflection;
using System.Text.Json;
using System.Runtime.CompilerServices;
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
        var componentTypes = typeof(VdXy).Assembly
            .GetExportedTypes()
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<ECMAScriptAttribute>()))
            .Where(static item => item.Attribute?.Transform == Transform.Component)
            .OrderBy(static item => item.Type.Name, StringComparer.Ordinal)
            .ToArray();
        var shippedEntries = imports.EnumerateObject()
            .Select(static entry => entry.Name)
            .Where(static entry => entry.StartsWith("vue-data-ui/vue-ui-", StringComparison.Ordinal))
            .OrderBy(static entry => entry, StringComparer.Ordinal)
            .ToArray();
        var descriptorEntries = componentTypes
            .Select(static item => item.Attribute!.Import!)
            .OrderBy(static entry => entry, StringComparer.Ordinal)
            .ToArray();

        Assert.AreEqual(71, shippedEntries.Length, "vue-data-ui 3.23.4 exposes 71 public vue-ui-* entries.");
        Assert.AreEqual(shippedEntries.Length, componentTypes.Length, "Every shipped visual entry needs one Razor descriptor.");
        CollectionAssert.AreEquivalent(shippedEntries, descriptorEntries, "Descriptor catalog must exactly match manifest package entries.");
        Assert.IsFalse(imports.TryGetProperty("vue-data-ui", out _), "The root entry eagerly aggregates the whole library.");

        foreach (var (type, attribute) in componentTypes)
        {
            Assert.IsNotNull(attribute);
            Assert.IsTrue(
                attribute!.Import!.StartsWith("vue-data-ui/vue-ui-", StringComparison.Ordinal),
                $"{type.Name} must use a per-component vue-data-ui entry.");
            Assert.IsTrue(
                imports.TryGetProperty(attribute.Import, out _),
                $"manifest.json is missing {attribute.Import} for {type.Name}.");
            // C# uses the short Vd prefix; the npm entry still exports its original VueUi name.
            Assert.IsTrue(type.Name.StartsWith("Vd", StringComparison.Ordinal), type.Name);
            Assert.AreEqual("VueUi" + type.Name[2..], attribute.ExportName, type.Name);
        }

        Assert.AreEqual("3.23.4", manifest.RootElement.GetProperty("version").GetString());
        Assert.AreEqual("jspdf", imports.GetProperty("jspdf").GetProperty("production").GetString());
        CollectionAssert.Contains(
            imports.GetProperty("vue-data-ui/vue-ui-table")
                .GetProperty("productionDependencies")
                .EnumerateArray()
                .Select(static value => value.GetString())
                .ToArray(),
            "jspdf");
        Assert.AreEqual(0, manifest.RootElement.GetProperty("styles").GetArrayLength());
        CollectionAssert.Contains(
            imports.GetProperty("vue-data-ui/vue-ui-table")
                .GetProperty("productionStylesheetImports")
                .EnumerateArray()
                .Select(static value => value.GetString())
                .ToArray(),
            "vue-data-ui/style.css");
    }

    [TestMethod]
    public void VueDataUi_IconAndPatternLiteralsMatchUpstreamDeclarations()
    {
        Assert.IsTrue(typeof(VdPatternName).GetFields(BindingFlags.Public | BindingFlags.Static).Length > 0);
        Assert.IsTrue(typeof(VdIconName).GetFields(BindingFlags.Public | BindingFlags.Static).Length > 0);

        Assert.AreEqual(
            typeof(VdPatternName),
            typeof(VdPattern).GetProperty(nameof(VdPattern.Name))!.PropertyType);
        Assert.AreEqual(
            typeof(VdIconName),
            typeof(VdIcon).GetProperty(nameof(VdIcon.Name))!.PropertyType);
    }

    [TestMethod]
    public void VueDataUi_PositionalDatasetFactoriesKeepArrayRuntimeShapes()
    {
        var agePyramidRow = typeof(VdAgePyramidData).GetMethod(nameof(VdAgePyramidData.Row));
        Assert.IsNotNull(agePyramidRow);
        Assert.AreEqual(
            "[__arg1, __arg2, __arg3, __arg4]",
            agePyramidRow!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

        var flowLink = typeof(VdFlowData).GetMethod(nameof(VdFlowData.Link));
        Assert.IsNotNull(flowLink);
        Assert.AreEqual(
            "[__arg1, __arg2, __arg3]",
            flowLink!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
    }

    [TestMethod]
    public void VueDataUi_PublicAuthoringSurfaceHasNoObjectCatchAlls()
    {
        var assembly = typeof(VdConfig).Assembly;
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
        AssertNativeUnion(typeof(VdCellValue), typeof(string), typeof(double));
        AssertNativeUnion(typeof(VdXySeriesValues), typeof(double?[]), typeof(VdXyCoordinate[]));
        AssertNativeUnion(typeof(VdWordCloudDataset), typeof(VdWordCloudDatasetItem[]), typeof(string));
        AssertNativeUnion(typeof(VdQuickChartDataset), typeof(double?[]), typeof(VdQuickChartDatasetItem), typeof(VdQuickChartDatasetItem[]));
    }

    [TestMethod]
    public void VueDataUi_ConfigExtensibilityRemainsStructured()
    {
        Assert.IsTrue(typeof(Vue.VueDictionary<Vue.VueValue>).IsAssignableFrom(typeof(VdConfig)));
        Assert.IsTrue(typeof(Vue.VueDictionary<Vue.VueValue>).IsAssignableFrom(typeof(VdDatasetItem)));
        Assert.AreEqual(typeof(VdConfig), typeof(VdDonutConfig).BaseType);
        Assert.AreEqual(typeof(VdConfig), typeof(VdXyConfig).BaseType);
    }

    [TestMethod]
    public void VueDataUi_DonutLegendCallbackUsesTheUpstreamSummaryPayload()
    {
        var callback = typeof(VdDonut).GetProperty(nameof(VdDonut.OnSelectLegend));
        Assert.IsNotNull(callback);
        Assert.AreEqual(typeof(EventCallback<VdDonutLegendItem[]>), callback!.PropertyType);

        var name = callback.GetCustomAttribute<ECMAScriptNameAttribute>();
        Assert.IsNotNull(name);
        Assert.AreEqual("onSelectLegend", name!.Name);

        Assert.AreEqual(typeof(string), typeof(VdDonutLegendItem).GetProperty(nameof(VdDonutLegendItem.Color))!.PropertyType);
        Assert.AreEqual(typeof(string), typeof(VdDonutLegendItem).GetProperty(nameof(VdDonutLegendItem.Name))!.PropertyType);
        Assert.AreEqual(typeof(double), typeof(VdDonutLegendItem).GetProperty(nameof(VdDonutLegendItem.Value))!.PropertyType);
    }

    [TestMethod]
    public void VueDataUi_ChartDescriptorsKeepTheirSpecializedDatasetTypes()
    {
        AssertDatasetType(typeof(VdHorizontalBar), typeof(VdHorizontalBarDatasetItem[]));
        AssertDatasetType(typeof(VdTableHeatmap), typeof(VdTableHeatmapDatasetItem[]));
        AssertDatasetType(
            typeof(VdTableSparkline),
            typeof(VdTableSparklineDatasetItem[]),
            typeof(VdRequiredConfigChartComponent<,>));
        AssertDatasetType(typeof(VdCandlestick), typeof(VdCellValue[][]));

        Assert.AreEqual(typeof(VdCellValue?[]), typeof(VdTableHeatmapDatasetItem)
            .GetProperty(nameof(VdTableHeatmapDatasetItem.Values))!.PropertyType);
        Assert.AreEqual(typeof(double?[]), typeof(VdTableSparklineDatasetItem)
            .GetProperty(nameof(VdTableSparklineDatasetItem.Values))!.PropertyType);

        var ohlc = typeof(VdCandlestickData).GetMethod(nameof(VdCandlestickData.Ohlc));
        Assert.IsNotNull(ohlc);
        Assert.AreEqual(typeof(VdCellValue[]), ohlc!.ReturnType);
        Assert.AreEqual(
            "[__arg1, __arg2, __arg3, __arg4, __arg5, __arg6]",
            ohlc.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

        Assert.IsNotNull(typeof(VdChartComponent<,>).GetProperty(nameof(VdChartComponent<int, VdDonutConfig>.Dataset))
            ?.GetCustomAttribute<EditorRequiredAttribute>());
        Assert.IsNotNull(typeof(VdRequiredConfigChartComponent<,>)
            .GetProperty(nameof(VdRequiredConfigChartComponent<int, VdTableSparklineConfig>.Config))
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
        Assert.AreEqual(expectedBaseDefinition ?? typeof(VdChartComponent<,>), chartBase!.GetGenericTypeDefinition());
        Assert.AreEqual(expectedDatasetType, chartBase.GetGenericArguments()[0]);
    }
}
