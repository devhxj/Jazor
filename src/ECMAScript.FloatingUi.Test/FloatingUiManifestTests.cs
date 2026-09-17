using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using ECMAScript;
using ECMAScript.Contract;

namespace ECMAScriptFloatingUiTest;

/// <summary>
/// File-backed manifest, inventory, and upstream drift checks for the vendored Floating UI runtime.
/// 资源闭包元数据与上游 drift 检查；manifest 哈希与 vendored 文件必须一一对应。
/// </summary>
[TestClass]
public sealed class FloatingUiManifestTests
{
    [TestMethod]
    public void FloatingUi_Manifest_DeclaresLockedMultiPackageRuntimeWithVerifiableHashes()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("floating-ui", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        // The author entry peer-depends on Vue, supplied by the ECMAScript.Vue resource library.
        Assert.AreEqual("^3.5.0", root.GetProperty("requires").GetProperty("vue3").GetString());

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(
            new[] { "@floating-ui/vue", "@floating-ui/dom", "@floating-ui/core", "@floating-ui/utils", "@floating-ui/utils/dom" },
            imports.EnumerateObject().Select(static entry => entry.Name).ToArray());

        // Sibling entries are wired through the package-dependency channel so the materializer
        // recursively selects the full closure.
        var vueEntry = imports.GetProperty("@floating-ui/vue");
        CollectionAssert.AreEquivalent(
            new[] { "@floating-ui/dom", "@floating-ui/utils/dom" },
            vueEntry.GetProperty("productionDependencies").EnumerateArray().Select(static value => value.GetString()!).ToArray());
        Assert.AreEqual(0, imports.GetProperty("@floating-ui/utils").GetProperty("developmentDependencies").EnumerateArray().Count());

        foreach (var entry in imports.EnumerateObject())
        {
            var desired = entry.Value.GetProperty("development").GetString()!;
            Assert.AreEqual(desired, entry.Value.GetProperty("production").GetString());
            Assert.AreEqual(entry.Value.GetProperty("developmentHash").GetString(), entry.Value.GetProperty("productionHash").GetString());
        }

        AssertAllManifestFilesHashCorrect(root);
    }

    [TestMethod]
    public void FloatingUi_VendoredDist_ExactlyMatchesTheManifestClosure()
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
    public void FloatingUi_BoundExports_ExistInTheUpstreamEntry()
    {
        var bound = typeof(FloatingUi)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(method => method.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .SingleOrDefault()?.Description)
            .Where(description => description is not null && description.StartsWith("@#", StringComparison.Ordinal))
            .Select(description => description![2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 12, $"First slice expects at least 12 bound functions, found {bound.Count}.");

        var upstream = ReadUpstreamEntryExports("vue", "floating-ui.vue.mjs");
        var missing = bound.Except(upstream).Order().ToArray();
        Assert.IsFalse(missing.Length > 0, $"Bound exports missing from the vendored upstream entry: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void FloatingUi_Inventory_RecordsEveryClosurePackageVersionAndFingerprint()
    {
        var inventory = GetInventory();
        var packages = inventory.GetProperty("packages");
        Assert.AreEqual(4, packages.EnumerateObject().Count());
        Assert.AreEqual(inventory.GetProperty("version").GetString(), packages.GetProperty("@floating-ui/vue").GetString());
        Assert.IsTrue(Regex.IsMatch(packages.GetProperty("@floating-ui/dom").GetString()!, @"^\d+\.\d+\.\d+"));
        Assert.IsTrue(Regex.IsMatch(packages.GetProperty("@floating-ui/core").GetString()!, @"^\d+\.\d+\.\d+"));
        Assert.IsTrue(Regex.IsMatch(packages.GetProperty("@floating-ui/utils").GetString()!, @"^\d+\.\d+\.\d+"));

        // Recompute the payload the generator hashed: every property except the fingerprint itself.
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

    private static HashSet<string> ReadUpstreamEntryExports(string name, string file)
    {
        // The entry re-exports from siblings with `export { ... } from "..."` and declares its own
        // named exports; both contribute to the public surface.
        var source = File.ReadAllText(GetProjectPath("dist", "@floating-ui", name, file));
        var exports = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(source, @"export \{ ([^}]+) \}(?: from [""'][^""']+[""'])?;"))
        {
            foreach (var specifier in match.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var name_ = specifier.Split(" as ")[0].Trim();
                if (name_ != "default")
                    exports.Add(name_);
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

    // CallerFilePath resolves the package tree from source, independent of the test output directory.
    // 用源码路径定位包目录，避免并行测试 lane 的 BaseOutputPath 差异影响资源解析。
    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.FloatingUi"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
