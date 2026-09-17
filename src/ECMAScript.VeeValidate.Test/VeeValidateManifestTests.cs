using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using ECMAScript;
using ECMAScript.Contract;

namespace ECMAScriptVeeValidateTest;

/// <summary>
/// File-backed manifest, inventory, and upstream drift checks for the vendored vee-validate runtime.
/// 资源闭包元数据与上游 drift 检查；manifest 哈希与 vendored 文件必须一一对应。
/// </summary>
[TestClass]
public sealed class VeeValidateManifestTests
{
    [TestMethod]
    public void VeeValidate_Manifest_DeclaresLockedRuntimeWithVerifiableHashes()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vee-validate", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        // The author entry peer-depends on Vue, supplied by the ECMAScript.Vue resource library.
        Assert.IsTrue(root.GetProperty("requires").TryGetProperty("vue3", out var vue));
        Assert.IsTrue(!string.IsNullOrWhiteSpace(vue.GetString()));

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(new[] { "vee-validate" }, imports.EnumerateObject().Select(static entry => entry.Name).ToArray());

        // vee-validate ships a self-contained bundle; its only bare import is the Vue peer.
        var entry = imports.GetProperty("vee-validate");
        CollectionAssert.AreEquivalent(
            new[] { "vue" },
            entry.GetProperty("productionDependencies").EnumerateArray().Select(static value => value.GetString()!).ToArray());
        Assert.AreEqual(0, entry.GetProperty("developmentModuleDependencies").EnumerateArray().Count());

        foreach (var import in imports.EnumerateObject())
        {
            var desired = import.Value.GetProperty("development").GetString()!;
            Assert.AreEqual(desired, import.Value.GetProperty("production").GetString());
            Assert.AreEqual(import.Value.GetProperty("developmentHash").GetString(), import.Value.GetProperty("productionHash").GetString());
        }

        AssertAllManifestFilesHashCorrect(root);
    }

    [TestMethod]
    public void VeeValidate_VendoredDist_ExactlyMatchesTheManifestClosure()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var declared = new HashSet<string>(StringComparer.Ordinal);
        foreach (var import in manifest.RootElement.GetProperty("imports").EnumerateObject())
        {
            declared.Add(import.Value.GetProperty("development").GetString()!);
            foreach (var file in import.Value.GetProperty("files").EnumerateArray())
            {
                var path = file.GetProperty("path").GetString()!;
                declared.Add(path);
                // 每条 module 声明都必须带同值的 moduleId，供 LibraryManifest.FindModule 解析。
                Assert.AreEqual(path, file.GetProperty("moduleId").GetString(), $"moduleId mismatch for '{path}'.");
                Assert.AreEqual("module", file.GetProperty("type").GetString());
            }
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
    public void VeeValidate_BoundExports_ExistInTheUpstreamEntry()
    {
        var bound = typeof(VeeValidate)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Concat(typeof(VeeValidate).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(static property => (MemberInfo)property))
            .Select(member => member.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .SingleOrDefault()?.Description)
            .Where(description => description is not null && description.StartsWith("@#", StringComparison.Ordinal))
            .Select(description => description![2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 20, $"First slice expects at least 20 bound exports, found {bound.Count}.");

        var upstream = ReadUpstreamEntryExports();
        var missing = bound.Except(upstream).Order().ToArray();
        Assert.IsFalse(missing.Length > 0, $"Bound exports missing from the vendored upstream entry: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void VeeValidate_Inventory_RecordsClosureVersionAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual("vee-validate", inventory.GetProperty("upstream").GetString());
        Assert.AreEqual("MIT", inventory.GetProperty("license").GetString());
        Assert.AreEqual("4", inventory.GetProperty("packages").GetProperty("vee-validate").GetString()!.Split('.')[0]);
        Assert.AreEqual(1, inventory.GetProperty("vendoredModuleCount").GetInt32());
        Assert.IsTrue(inventory.GetProperty("documentation").GetString()!.StartsWith("https://", StringComparison.Ordinal));

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

    private static HashSet<string> ReadUpstreamEntryExports()
    {
        var source = File.ReadAllText(GetProjectPath("dist", "vee-validate", "dist", "vee-validate.mjs"));
        var exports = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(source, @"export \{ ([^}]+) \}(?: from [""'][^""']+[""'])?;"))
        {
            foreach (var specifier in match.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var name = specifier.Split(" as ")[0].Trim();
                if (name != "default")
                    exports.Add(name);
            }
        }

        return exports;
    }

    private static void AssertAllManifestFilesHashCorrect(JsonElement root)
    {
        foreach (var import in root.GetProperty("imports").EnumerateObject())
        {
            AssertFileHash(import.Value.GetProperty("development").GetString()!, import.Value.GetProperty("developmentHash").GetString()!);
            AssertFileHash(import.Value.GetProperty("production").GetString()!, import.Value.GetProperty("productionHash").GetString()!);
            foreach (var file in import.Value.GetProperty("files").EnumerateArray())
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

    // CallerFilePath resolves the package tree from source, independent of the test output directory.
    // 用源码路径定位包目录，避免并行测试 lane 的 BaseOutputPath 差异影响资源解析。
    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VeeValidate"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
