using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using ECMAScript;

#pragma warning disable CA1416

namespace ECMAScriptVueQueryTest;

/// <summary>
/// File-backed manifest, inventory, and upstream drift checks for the vendored vue-query runtime.
/// 资源闭包元数据与上游 drift 检查；manifest 哈希与 vendored 文件必须一一对应。
/// </summary>
[TestClass]
public sealed class VueQueryManifestTests
{
    [TestMethod]
    public void VueQuery_Manifest_DeclaresLockedTanStackClosureWithVerifiableHashes()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vue-query", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.IsTrue(root.GetProperty("requires").TryGetProperty("vue3", out var vue));
        Assert.IsTrue(!string.IsNullOrWhiteSpace(vue.GetString()));

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(
            new[] { "@tanstack/vue-query", "@tanstack/query-core", "@tanstack/match-sorter-utils", "vue-demi" },
            imports.EnumerateObject().Select(static entry => entry.Name).ToArray());

        // The vue-query entry re-exports query-core and match-sorter-utils through the package
        // channel; @vue/devtools-api and vue are peers provided by the ECMAScript.Vue library.
        // vue-query 入口通过 package 通道引用 query-core 与 match-sorter-utils；
        // devtools-api 与 vue 是由 ECMAScript.Vue 提供的 peer。
        var entry = imports.GetProperty("@tanstack/vue-query");
        CollectionAssert.AreEquivalent(
            new[] { "@tanstack/match-sorter-utils", "@tanstack/query-core", "@vue/devtools-api", "vue-demi" },
            entry.GetProperty("productionDependencies").EnumerateArray().Select(static value => value.GetString()!).ToArray());
        Assert.AreEqual(0, imports.GetProperty("@tanstack/query-core").GetProperty("productionDependencies").EnumerateArray().Count());

        foreach (var declared in imports.EnumerateObject())
        {
            Assert.AreEqual("module", declared.Value.GetProperty("type").GetString());
            Assert.AreEqual(declared.Value.GetProperty("development").GetString(), declared.Value.GetProperty("production").GetString());
            Assert.AreEqual(declared.Value.GetProperty("developmentHash").GetString(), declared.Value.GetProperty("productionHash").GetString());
        }

        AssertAllManifestFilesHashCorrect(root);
    }

    [TestMethod]
    public void VueQuery_VendoredDist_ExactlyMatchesTheManifestClosure()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var declared = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in manifest.RootElement.GetProperty("imports").EnumerateObject())
        {
            declared.Add(entry.Value.GetProperty("development").GetString()!);
            foreach (var file in entry.Value.GetProperty("files").EnumerateArray())
                declared.Add(file.GetProperty("path").GetString()!);
        }

        var vendored = Directory.EnumerateFiles(GetProjectPath("dist"), "*", SearchOption.AllDirectories)
            .Select(path => "dist/" + Path.GetRelativePath(GetProjectPath("dist"), path).Replace('\\', '/'))
            .ToHashSet(StringComparer.Ordinal);

        Assert.IsTrue(vendored.SetEquals(declared), $"""
            Vendored tree and manifest closure disagree.
            Only on disk: {string.Join(", ", vendored.Except(declared).Order())}
            Only in manifest: {string.Join(", ", declared.Except(vendored).Order())}
            """);
    }

    [TestMethod]
    public void VueQuery_BoundExports_ExistInTheUpstreamEntry()
    {
        var bound = typeof(VueQuery)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(method => method.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .SingleOrDefault()?.Description)
            .Where(description => description is not null && description.StartsWith("@#", StringComparison.Ordinal))
            .Select(description => description![2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 5, $"Expected the plugin plus four composables, found {bound.Count}.");

        var upstream = ReadExportedNames(GetProjectPath("dist", "@tanstack", "vue-query", "build", "modern", "index.js"));
        var missing = bound.Except(upstream).Order().ToArray();
        Assert.IsFalse(missing.Length > 0, $"Bound exports missing from the vendored upstream entry: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void VueQuery_ReturnShapes_CoverLoadingErrorAndWriteEntries()
    {
        var query = typeof(VueQueryQueryReturn<string>);
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<string?>), query.GetProperty("Data")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<Error?>), query.GetProperty("Error")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<VueQueryStatus>), query.GetProperty("Status")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<bool>), query.GetProperty("IsLoading")!.PropertyType);
        Assert.AreEqual(typeof(PromiseResult), query.GetMethod("Refetch")!.ReturnType);

        var mutation = typeof(VueQueryMutationReturn<string, string>);
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<string?>), mutation.GetProperty("Data")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<string?>), mutation.GetProperty("Variables")!.PropertyType);
        Assert.AreEqual(typeof(void), mutation.GetMethod("Mutate")!.ReturnType);
        Assert.AreEqual(typeof(PromiseResult<string>), mutation.GetMethod("MutateAsync")!.ReturnType);
    }

    [TestMethod]
    public void VueQuery_StringEnums_MirrorUpstreamValues()
    {
        AssertStringEnumMember<VueQueryStatus>("pending", nameof(VueQueryStatus.Pending));
        AssertStringEnumMember<VueQueryStatus>("success", nameof(VueQueryStatus.Success));
        AssertStringEnumMember<VueQueryStatus>("error", nameof(VueQueryStatus.Error));
    }

    [TestMethod]
    public void VueQuery_Inventory_RecordsPackageVersionsAndFingerprint()
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

    private static void AssertStringEnumMember<TEnum>(string javaScriptValue, string memberName)
        where TEnum : struct, Enum
    {
        Assert.IsNotNull(typeof(TEnum).GetCustomAttribute<ECMAScript.StringAttribute>(), typeof(TEnum).FullName);
        var description = typeof(TEnum).GetField(memberName)!
            .GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()!.Description;
        Assert.AreEqual("@#" + javaScriptValue, description, $"{typeof(TEnum).Name}.{memberName}");
    }

    private static HashSet<string> ReadExportedNames(string path)
    {
        var source = File.ReadAllText(path);
        var exports = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(source, @"(?m)^export (?:declare )?(?:function|const|class|let|var) (\w+)"))
            exports.Add(match.Groups[1].Value);
        foreach (Match match in Regex.Matches(source, @"export \{ ([^}]+) \}(?: from [""'][^""']+[""'])?;"))
        {
            foreach (var specifier in match.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = specifier.Split(" as ", StringSplitOptions.TrimEntries);
                var exported = parts.Length > 1 ? parts[1] : parts[0];
                if (exported != "default")
                    exports.Add(exported);
            }
        }

        return exports;
    }

    private static void AssertAllManifestFilesHashCorrect(JsonElement root)
    {
        foreach (var entry in root.GetProperty("imports").EnumerateObject())
        {
            AssertFileHash(entry.Value.GetProperty("development").GetString()!, entry.Value.GetProperty("developmentHash").GetString()!);
            AssertFileHash(entry.Value.GetProperty("production").GetString()!, entry.Value.GetProperty("productionHash").GetString()!);
            foreach (var file in entry.Value.GetProperty("files").EnumerateArray())
                AssertFileHash(file.GetProperty("path").GetString()!, file.GetProperty("hash").GetString()!);
        }

        foreach (var file in root.GetProperty("files").EnumerateArray())
            AssertFileHash(file.GetProperty("path").GetString()!, file.GetProperty("hash").GetString()!);
    }

    private static void AssertFileHash(string relativePath, string expectedHash)
    {
        var fullPath = GetProjectPath(relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.IsTrue(File.Exists(fullPath), $"Manifest file '{relativePath}' is missing from the package tree.");
        var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fullPath))).ToLowerInvariant();
        Assert.AreEqual(expectedHash.ToLowerInvariant(), actual, $"Hash mismatch for '{relativePath}'.");
    }

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
