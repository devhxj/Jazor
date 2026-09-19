using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECMAScriptVueQueryTest;

/// <summary>
/// 验证 Vue Query 绑定交付的是标准 npm package graph，而不是绑定库自带的运行时代码。
/// </summary>
[TestClass]
public sealed class VueQueryManifestTests
{
    [TestMethod]
    public void VueQuery_Manifest_DeclaresPackageGraphAndLogicalEntries()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vue-query", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.AreEqual("^3.3.0", root.GetProperty("requires").GetProperty("vue3").GetString());

        var packages = root.GetProperty("packages");
        foreach (var packageName in new[]
                 { "@tanstack/match-sorter-utils", "@tanstack/query-core", "@tanstack/vue-query", "vue", "vue-demi" })
        {
            Assert.AreEqual("npm", packages.GetProperty(packageName).GetProperty("source").GetString());
            Assert.IsFalse(string.IsNullOrWhiteSpace(packages.GetProperty(packageName).GetProperty("version").GetString()));
        }

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(
            new[] { "@tanstack/match-sorter-utils", "@tanstack/query-core", "@tanstack/vue-query", "vue-demi" },
            imports.EnumerateObject().Select(static entry => entry.Name).ToArray());

        CollectionAssert.AreEquivalent(
            new[] { "@tanstack/match-sorter-utils", "@tanstack/query-core", "@vue/devtools-api", "vue-demi" },
            imports.GetProperty("@tanstack/vue-query").GetProperty("productionDependencies")
                .EnumerateArray().Select(static value => value.GetString()!).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "vue" },
            imports.GetProperty("vue-demi").GetProperty("productionDependencies")
                .EnumerateArray().Select(static value => value.GetString()!).ToArray());

        foreach (var entry in imports.EnumerateObject())
        {
            Assert.AreEqual("module", entry.Value.GetProperty("type").GetString());
            Assert.IsTrue(IsBareSpecifier(entry.Value.GetProperty("development").GetString()!), entry.Name);
            Assert.AreEqual(entry.Value.GetProperty("development").GetString(), entry.Value.GetProperty("production").GetString());
            Assert.IsFalse(entry.Value.TryGetProperty("developmentHash", out _), entry.Name);
            Assert.IsFalse(entry.Value.TryGetProperty("productionHash", out _), entry.Name);
            Assert.IsFalse(entry.Value.TryGetProperty("files", out _), entry.Name);
        }

        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());
        Assert.IsFalse(root.TryGetProperty("dist", out _));
        Assert.IsFalse(root.TryGetProperty("vendor", out _));
    }

    [TestMethod]
    public void VueQuery_RuntimeCarrierIsResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));

        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var packages = manifest.RootElement.GetProperty("packages");
        Assert.AreEqual("5.103.1", packages.GetProperty("@tanstack/vue-query").GetProperty("version").GetString());
        Assert.AreEqual("5.103.1", packages.GetProperty("@tanstack/query-core").GetProperty("version").GetString());
    }

    [TestMethod]
    public void VueQuery_BoundExportsUseTheAuthorPackageEntry()
    {
        var bound = ReadBoundExportNames(typeof(ECMAScript.VueQuery));
        Assert.IsTrue(bound.Count >= 6, "The first slice should expose the plugin and query composables.");
        CollectionAssert.Contains(bound.ToArray(), "VueQueryPlugin");
        CollectionAssert.Contains(bound.ToArray(), "useQuery");
        CollectionAssert.Contains(bound.ToArray(), "useMutation");

        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.IsTrue(manifest.RootElement.GetProperty("imports").TryGetProperty("@tanstack/vue-query", out _));
    }

    [TestMethod]
    public void VueQuery_ReturnShapesCoverLoadingErrorAndWriteEntries()
    {
        var query = typeof(ECMAScript.VueQueryQueryReturn<string>);
        Assert.AreEqual(typeof(ECMAScript.Vue.VueReadonlyRef<string?>), query.GetProperty("Data")!.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue.VueReadonlyRef<ECMAScript.Error?>), query.GetProperty("Error")!.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue.VueReadonlyRef<ECMAScript.VueQueryStatus>), query.GetProperty("Status")!.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue.VueReadonlyRef<bool>), query.GetProperty("IsLoading")!.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.PromiseResult), query.GetMethod("Refetch")!.ReturnType);

        var mutation = typeof(ECMAScript.VueQueryMutationReturn<string, string>);
        Assert.AreEqual(typeof(ECMAScript.Vue.VueReadonlyRef<string?>), mutation.GetProperty("Data")!.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue.VueReadonlyRef<string?>), mutation.GetProperty("Variables")!.PropertyType);
        Assert.AreEqual(typeof(void), mutation.GetMethod("Mutate")!.ReturnType);
        Assert.AreEqual(typeof(ECMAScript.PromiseResult<string>), mutation.GetMethod("MutateAsync")!.ReturnType);
    }

    [TestMethod]
    public void VueQuery_StringEnumsMirrorUpstreamValues()
    {
        AssertStringEnumMember<ECMAScript.VueQueryStatus>("pending", nameof(ECMAScript.VueQueryStatus.Pending));
        AssertStringEnumMember<ECMAScript.VueQueryStatus>("success", nameof(ECMAScript.VueQueryStatus.Success));
        AssertStringEnumMember<ECMAScript.VueQueryStatus>("error", nameof(ECMAScript.VueQueryStatus.Error));
    }

    [TestMethod]
    public void VueQuery_InventoryRecordsPackageVersionsAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual(inventory.GetProperty("version").GetString(), inventory.GetProperty("packages").GetProperty("@tanstack/vue-query").GetString());
        Assert.AreEqual(inventory.GetProperty("version").GetString(), inventory.GetProperty("packages").GetProperty("@tanstack/query-core").GetString());

        var fingerprint = inventory.GetProperty("fingerprint").GetString()!;
        var payload = new Dictionary<string, object?>();
        foreach (var property in inventory.EnumerateObject())
        {
            if (property.Name == "fingerprint")
                continue;
            payload.Add(property.Name, property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetInt32(),
                JsonValueKind.Object => property.Value.EnumerateObject().ToDictionary(static item => item.Name, static item => (object?)item.Value.GetString()),
                JsonValueKind.Array => property.Value.EnumerateArray().Select(static item => item.GetString()).ToArray(),
                _ => throw new InvalidOperationException("Unexpected inventory payload shape."),
            });
        }

        var json = JsonSerializer.Serialize(payload, InventoryPayloadOptions);
        var recomputed = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        Assert.AreEqual(fingerprint, recomputed, "inventory fingerprint must cover its upstream payload.");
    }

    private static readonly JsonSerializerOptions InventoryPayloadOptions = new() { WriteIndented = true };

    private static HashSet<string> ReadBoundExportNames(Type type)
        => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(static method => method.GetCustomAttributes<ComponentDescriptionAttribute>(inherit: false))
            .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(static property => property.GetCustomAttributes<ComponentDescriptionAttribute>(inherit: false)))
            .Select(static attribute => attribute.Description)
            .Where(static description => description.StartsWith("@#", StringComparison.Ordinal))
            .Select(static description => description[2..])
            .ToHashSet(StringComparer.Ordinal);

    private static void AssertStringEnumMember<TEnum>(string javascriptValue, string memberName)
        where TEnum : struct, Enum
    {
        var description = typeof(TEnum).GetField(memberName)!.GetCustomAttribute<ComponentDescriptionAttribute>()!.Description;
        Assert.AreEqual("@#" + javascriptValue, description);
    }

    private static bool IsBareSpecifier(string value)
        => !string.IsNullOrWhiteSpace(value) &&
           !value.StartsWith('.', StringComparison.Ordinal) &&
           !value.StartsWith('/', StringComparison.Ordinal) &&
           !value.Contains('\\', StringComparison.Ordinal);

    private static JsonElement GetInventory()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetProjectPath("inventory.json")));
        return document.RootElement.Clone();
    }

    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VueQuery"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
