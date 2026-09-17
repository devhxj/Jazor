using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using ECMAScript;

namespace ECMAScriptVueUseTest;

/// <summary>
/// File-backed manifest, inventory, and upstream drift checks for the vendored VueUse runtime.
/// 资源闭包元数据与上游 drift 检查；manifest 哈希与 vendored 文件必须一一对应。
/// </summary>
[TestClass]
public sealed class VueUseManifestTests
{
    [TestMethod]
    public void VueUse_Manifest_DeclaresLockedCoreAndSharedClosureWithVerifiableHashes()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vueuse", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.AreEqual("^3.5.0", root.GetProperty("requires").GetProperty("vue3").GetString());

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(
            new[] { "@vueuse/core", "@vueuse/shared" },
            imports.EnumerateObject().Select(static entry => entry.Name).ToArray());

        // @vueuse/core re-exports @vueuse/shared, so core depends on it through the package channel.
        var core = imports.GetProperty("@vueuse/core");
        CollectionAssert.AreEquivalent(
            new[] { "@vueuse/shared" },
            core.GetProperty("productionDependencies").EnumerateArray().Select(static value => value.GetString()!).ToArray());
        Assert.AreEqual(0, imports.GetProperty("@vueuse/shared").GetProperty("productionDependencies").EnumerateArray().Count());

        foreach (var entry in imports.EnumerateObject())
        {
            Assert.AreEqual("module", entry.Value.GetProperty("type").GetString());
            Assert.AreEqual(entry.Value.GetProperty("development").GetString(), entry.Value.GetProperty("production").GetString());
            Assert.AreEqual(entry.Value.GetProperty("developmentHash").GetString(), entry.Value.GetProperty("productionHash").GetString());
        }

        AssertAllManifestFilesHashCorrect(root);
    }

    [TestMethod]
    public void VueUse_VendoredDist_ExactlyMatchesTheManifestClosure()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var declared = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in manifest.RootElement.GetProperty("imports").EnumerateObject())
            declared.Add(entry.Value.GetProperty("development").GetString()!);

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
    public void VueUse_BoundExports_ExistInTheUpstreamRuntime()
    {
        var bound = typeof(VueUse)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(method => method.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .SingleOrDefault()?.Description)
            .Where(description => description is not null && description.StartsWith("@#", StringComparison.Ordinal))
            .Select(description => description![2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 69, $"First slice expects at least 69 bound composables, found {bound.Count}.");

        // The authoritative export set is core's own named exports plus the shared barrel that
        // core re-exports via `export * from "@vueuse/shared"`.
        var upstream = ReadExportedNames(GetProjectPath("dist", "@vueuse", "core", "index.js"))
            .Concat(ReadExportedNames(GetProjectPath("dist", "@vueuse", "shared", "index.js")))
            .ToHashSet(StringComparer.Ordinal);
        var missing = bound.Except(upstream).Order().ToArray();
        Assert.IsFalse(missing.Length > 0, $"Bound exports missing from the vendored upstream runtime: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void VueUse_Inventory_RecordsPackageVersionsAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual(inventory.GetProperty("version").GetString(), inventory.GetProperty("packages").GetProperty("@vueuse/core").GetString());
        Assert.AreEqual(inventory.GetProperty("version").GetString(), inventory.GetProperty("packages").GetProperty("@vueuse/shared").GetString());

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

    private static HashSet<string> ReadExportedNames(string path)
    {
        // The bundled module declares named exports and re-exports sibling members with
        // `export { a, b as c } from "..."`. Default exports are excluded.
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
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VueUse"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
