using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using ECMAScript;

namespace ECMAScriptDateFnsTest;

/// <summary>
/// File-backed manifest, inventory, and upstream drift checks for the vendored date-fns runtime.
/// 资源闭包元数据与上游 drift 检查；manifest 哈希与 vendored 文件必须一一对应。
/// </summary>
[TestClass]
public sealed class DateFnsManifestTests
{
    [TestMethod]
    public void DateFns_Manifest_DeclaresLockedUpstreamRuntimeWithVerifiableHashes()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("date-fns", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.AreEqual(JsonValueKind.Object, root.GetProperty("requires").ValueKind);
        Assert.AreEqual(0, root.GetProperty("requires").EnumerateObject().Count());

        var imports = root.GetProperty("imports");
        Assert.AreEqual(2, imports.EnumerateObject().Count());
        var functionEntry = imports.GetProperty("date-fns");
        var localeEntry = imports.GetProperty("date-fns/locale");

        // date-fns has no development/production split; both profiles point at the same vendored files.
        // 上游无 dev/prod 差异构建，两个 profile 指向同一 vendored 文件。
        Assert.AreEqual("module", functionEntry.GetProperty("type").GetString());
        Assert.AreEqual("dist/index.js", functionEntry.GetProperty("development").GetString());
        Assert.AreEqual("dist/index.js", functionEntry.GetProperty("production").GetString());
        Assert.AreEqual("module", localeEntry.GetProperty("type").GetString());
        Assert.AreEqual("dist/locale.mjs", localeEntry.GetProperty("development").GetString());
        Assert.AreEqual("dist/locale.mjs", localeEntry.GetProperty("production").GetString());
        Assert.AreEqual(0, functionEntry.GetProperty("developmentDependencies").EnumerateArray().Count());
        Assert.AreEqual(0, localeEntry.GetProperty("productionDependencies").EnumerateArray().Count());

        // Every declared module dependency must resolve to a module file entry with a matching hash.
        // 每个模块依赖都必须能通过 moduleId 解析到对应文件并匹配哈希。
        foreach (var entry in new[] { functionEntry, localeEntry })
        {
            var moduleIds = entry.GetProperty("developmentModuleDependencies").EnumerateArray()
                .Select(static value => value.GetString()!)
                .ToHashSet(StringComparer.Ordinal);
            var declaredFiles = entry.GetProperty("files").EnumerateArray()
                .Select(static value => value.GetProperty("moduleId").GetString()!)
                .ToHashSet(StringComparer.Ordinal);
            Assert.IsTrue(moduleIds.SetEquals(declaredFiles), "moduleDependencies and files moduleId sets must match.");
            Assert.AreEqual(
                entry.GetProperty("developmentModuleDependencies").EnumerateArray().Count(),
                entry.GetProperty("productionModuleDependencies").EnumerateArray().Count());
        }

        AssertAllManifestFilesHashCorrect(root);
    }

    [TestMethod]
    public void DateFns_VendoredDist_ExactlyMatchesTheManifestClosure()
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
    public void DateFns_BoundExports_ExistInTheUpstreamBarrel()
    {
        var bound = typeof(DateFns)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(method => method.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .SingleOrDefault()?.Description)
            .Where(description => description is not null && description.StartsWith("@#", StringComparison.Ordinal))
            .Select(description => description![2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 59, $"First slice expects at least 59 bound functions, found {bound.Count}.");

        var upstream = ReadUpstreamBarrelExports();
        var missing = bound.Except(upstream).Order().ToArray();
        Assert.IsFalse(missing.Length > 0, $"Bound exports missing from the vendored upstream barrel: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void DateFns_LocaleBridge_MatchesManifestClosureContractAndInventory()
    {
        var bridge = File.ReadAllLines(GetProjectPath("dist", "locale.mjs"));
        var bridgeEntries = bridge
            .Select(line => Regex.Match(line, "^export \\{ (\\w+) \\} from \"\\./locale/([^\"]+)\\.js\";$"))
            .Where(static match => match.Success)
            .ToDictionary(
                static match => match.Groups[1].Value,
                static match => match.Groups[2].Value,
                StringComparer.Ordinal);
        Assert.IsTrue(bridgeEntries.Count > 0, "The generated locale bridge must contain curated re-exports.");

        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var localeClosure = manifest.RootElement.GetProperty("imports").GetProperty("date-fns/locale")
            .GetProperty("developmentModuleDependencies").EnumerateArray()
            .Select(static value => value.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var code in bridgeEntries.Values)
            Assert.IsTrue(localeClosure.Contains("dist/locale/" + code + ".js"), $"Locale entry module '{code}' is missing from the manifest closure.");

        var inventory = GetInventory();
        var inventoryLocales = inventory.GetProperty("locales").EnumerateArray()
            .Select(static value => value.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(inventoryLocales.SetEquals(bridgeEntries.Values.ToHashSet(StringComparer.Ordinal)),
            "inventory.json locales and the generated locale bridge must list the same curated set.");

        var boundLocales = typeof(DateFnsLocale)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .Single().Description[2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(boundLocales.SetEquals(bridgeEntries.Keys.ToHashSet(StringComparer.Ordinal)),
            "DateFnsLocale properties and the locale bridge exports must be one-to-one.");
    }

    [TestMethod]
    public void DateFns_Inventory_FingerprintMatchesItsOwnPayload()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetProjectPath("inventory.json")));
        var root = document.RootElement;
        var fingerprint = root.GetProperty("fingerprint").GetString()!;

        // Recompute the payload the generator hashed: every property except the fingerprint itself,
        // serialized with the same indented settings.
        // 复算生成器哈希的 payload：除 fingerprint 外的全部属性，用相同缩进设置序列化。
        var payload = new Dictionary<string, object?>();
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name == "fingerprint")
                continue;
            payload.Add(property.Name, property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetInt32(),
                JsonValueKind.Array => property.Value.EnumerateArray().Select(static item => item.GetString()).ToArray(),
                _ => throw new InvalidOperationException("Unexpected inventory payload shape."),
            });
        }

        var json = JsonSerializer.Serialize(payload, InventoryPayloadOptions);
        var recomputed = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        Assert.AreEqual(fingerprint, recomputed, "inventory fingerprint must cover its upstream payload.");
    }

    private static readonly JsonSerializerOptions InventoryPayloadOptions = new() { WriteIndented = true };

    private static HashSet<string> ReadUpstreamBarrelExports()
    {
        // The barrel is `export * from "./<file>.js"`; each referenced module contributes its named
        // exports (`export function x` / `export { a, b as c }`). Default exports are excluded.
        var exports = new HashSet<string>(StringComparer.Ordinal);
        var barrel = File.ReadAllText(GetProjectPath("dist", "index.js"));
        foreach (Match reexport in Regex.Matches(barrel, "^export \\* from \"\\./([^\"]+)\\.js\";$", RegexOptions.Multiline))
        {
            var modulePath = GetProjectPath("dist", reexport.Groups[1].Value + ".js");
            if (!File.Exists(modulePath))
                continue;

            var source = Regex.Replace(File.ReadAllText(modulePath), @"/\*[\s\S]*?\*/", string.Empty);
            foreach (Match named in Regex.Matches(source, @"export function (\w+)"))
                exports.Add(named.Groups[1].Value);
            foreach (Match list in Regex.Matches(source, @"export \{ ([^}]+) \}"))
            {
                foreach (var specifier in list.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    var name = specifier.Split(" as ")[0];
                    if (name != "default")
                        exports.Add(name);
                }
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

    // CallerFilePath keeps the package tree resolution independent of the test output directory,
    // so parallel lanes with distinct BaseOutputPath keep locating the checked-in resources.
    // 用源码路径定位包目录，避免并行测试 lane 的 BaseOutputPath 差异影响资源解析。
    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.DateFns"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
