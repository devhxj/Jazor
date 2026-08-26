using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
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
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<ECMAScriptAttribute>()))
            .Where(static item => item.Attribute?.Transform == Transform.Component)
            .OrderBy(static item => item.Type.Name, StringComparer.Ordinal)
            .ToArray();
        var shippedEntries = Directory
            .EnumerateFiles(GetComponentsPath(), "vue-ui-*.js")
            .Select(static path => "vue-data-ui/" + Path.GetFileNameWithoutExtension(path))
            .OrderBy(static entry => entry, StringComparer.Ordinal)
            .ToArray();
        var descriptorEntries = componentTypes
            .Select(static item => item.Attribute!.Import!)
            .OrderBy(static entry => entry, StringComparer.Ordinal)
            .ToArray();

        Assert.AreEqual(71, shippedEntries.Length, "vue-data-ui 3.23.4 exposes 71 public vue-ui-* entries.");
        Assert.AreEqual(shippedEntries.Length, componentTypes.Length, "Every shipped visual entry needs one Razor descriptor.");
        CollectionAssert.AreEquivalent(shippedEntries, descriptorEntries, "Descriptor catalog must exactly match dist/components.");
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
            Assert.AreEqual(type.Name, attribute.ExportName, type.Name);
        }

        Assert.AreEqual("3.23.4", manifest.RootElement.GetProperty("version").GetString());
        Assert.AreEqual("dist/jspdf.browser.mjs", imports.GetProperty("jspdf").GetProperty("production").GetString());
        CollectionAssert.Contains(
            imports.GetProperty("vue-data-ui/vue-ui-table")
                .GetProperty("productionDependencies")
                .EnumerateArray()
                .Select(static value => value.GetString())
                .ToArray(),
            "jspdf");
        CollectionAssert.Contains(
            manifest.RootElement.GetProperty("styles").EnumerateArray().Select(static value => value.GetString()).ToArray(),
            "dist/style.css");
    }

    [TestMethod]
    public void VueDataUi_IconAndPatternLiteralsMatchUpstreamDeclarations()
    {
        AssertStringEnumMatchesDeclaration(typeof(VueUiPatternName), nameof(VueUiPatternName));
        AssertStringEnumMatchesDeclaration(typeof(VueUiIconName), nameof(VueUiIconName));

        Assert.AreEqual(
            typeof(VueUiPatternName),
            typeof(VueUiPattern).GetProperty(nameof(VueUiPattern.Name))!.PropertyType);
        Assert.AreEqual(
            typeof(VueUiIconName),
            typeof(VueUiIcon).GetProperty(nameof(VueUiIcon.Name))!.PropertyType);
    }

    [TestMethod]
    public void VueDataUi_PositionalDatasetFactoriesKeepArrayRuntimeShapes()
    {
        var agePyramidRow = typeof(VueUiAgePyramidData).GetMethod(nameof(VueUiAgePyramidData.Row));
        Assert.IsNotNull(agePyramidRow);
        Assert.AreEqual(
            "[__arg1, __arg2, __arg3, __arg4]",
            agePyramidRow!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);

        var flowLink = typeof(VueUiFlowData).GetMethod(nameof(VueUiFlowData.Link));
        Assert.IsNotNull(flowLink);
        Assert.AreEqual(
            "[__arg1, __arg2, __arg3]",
            flowLink!.GetCustomAttribute<ECMAScriptInlineAttribute>()?.RawFuncCode);
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

    private static string GetComponentsPath([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(sourceFilePath)!,
            "..",
            "ECMAScript.VueDataUi",
            "dist",
            "components"));

    private static string GetTypeDeclarationsPath([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(sourceFilePath)!,
            "..",
            "ECMAScript.VueDataUi",
            "dist",
            "types",
            "vue-data-ui.d.ts"));

    private static void AssertStringEnumMatchesDeclaration(Type enumType, string declarationName)
    {
        var expected = ReadDeclarationLiterals(declarationName);
        var actual = enumType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(static field => field.GetCustomAttribute<ComponentDescriptionAttribute>()?.Description)
            .Where(static value => value is not null)
            .Select(static value => value![2..])
            .ToArray();

        CollectionAssert.AreEquivalent(expected, actual, enumType.Name);
    }

    private static string[] ReadDeclarationLiterals(string declarationName)
    {
        var declarationLines = File.ReadAllLines(GetTypeDeclarationsPath());
        var start = Array.FindIndex(
            declarationLines,
            line => line.Trim() == $"export type {declarationName} =");

        Assert.IsTrue(start >= 0, $"Could not find upstream {declarationName} declaration.");

        var values = new List<string>();
        for (var index = start + 1; index < declarationLines.Length; index++)
        {
            var line = declarationLines[index].Trim();
            var match = Regex.Match(line, "^\\| '([^']+)';?$");
            if (match.Success)
                values.Add(match.Groups[1].Value);

            if (line.EndsWith(';'))
                break;
        }

        return values.Distinct(StringComparer.Ordinal).ToArray();
    }

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
