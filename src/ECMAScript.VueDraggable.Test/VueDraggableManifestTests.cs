using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using ECMAScript;

#pragma warning disable CA1416

namespace ECMAScriptVueDraggableTest;

/// <summary>
/// File-backed manifest, inventory, and upstream drift checks for the vendored VueDraggable runtime.
/// 资源闭包元数据与上游 drift 检查；manifest 哈希与 vendored 文件必须一一对应。
/// </summary>
[TestClass]
public sealed class VueDraggableManifestTests
{
    [TestMethod]
    public void VueDraggable_Manifest_DeclaresLockedClosureWithVerifiableHashes()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vue-draggable", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.AreEqual("^3.0.0", root.GetProperty("requires").GetProperty("vue3").GetString());

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(
            new[] { "vue-draggable-plus" },
            imports.EnumerateObject().Select(static entry => entry.Name).ToArray());

        Assert.AreEqual(0, imports.GetProperty("vue-draggable-plus").GetProperty("productionDependencies").EnumerateArray().Count());
        foreach (var entry in imports.EnumerateObject())
        {
            Assert.AreEqual("module", entry.Value.GetProperty("type").GetString());
            Assert.AreEqual(entry.Value.GetProperty("development").GetString(), entry.Value.GetProperty("production").GetString());
            Assert.AreEqual(entry.Value.GetProperty("developmentHash").GetString(), entry.Value.GetProperty("productionHash").GetString());
        }

        Assert.AreEqual(0, root.GetProperty("styles").EnumerateArray().Count());

        AssertAllManifestFilesHashCorrect(root);
    }

    [TestMethod]
    public void VueDraggable_VendoredDist_ExactlyMatchesTheManifestClosure()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var declared = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in manifest.RootElement.GetProperty("imports").EnumerateObject())
        {
            declared.Add(entry.Value.GetProperty("development").GetString()!);
            foreach (var file in entry.Value.GetProperty("files").EnumerateArray())
                declared.Add(file.GetProperty("path").GetString()!);
        }

        foreach (var style in manifest.RootElement.GetProperty("styles").EnumerateArray())
            declared.Add(style.GetProperty("path").GetString()!);

        var vendored = Directory.EnumerateFiles(GetProjectPath("dist"), "*", SearchOption.AllDirectories)
            .Select(path => "dist/" + Path.GetRelativePath(GetProjectPath("dist"), path).Replace('\\', '/'))
            .ToHashSet(StringComparer.Ordinal);

        Assert.IsTrue(vendored.SetEquals(declared),
            "Vendored tree and manifest closure disagree. Only on disk: " +
            string.Join(", ", vendored.Except(declared).Order()) +
            " | Only in manifest: " + string.Join(", ", declared.Except(vendored).Order()));
    }

    [TestMethod]
    public void VueDraggable_VendoredModules_AreBrowserSafe()
    {
        // 浏览器安全：vendor 的模块不得在裸浏览器环境引用 process.env（bundler 变体会在导入期崩溃）。
        foreach (var module in Directory.EnumerateFiles(GetProjectPath("dist"), "*", SearchOption.AllDirectories)
                     .Where(static path => path.EndsWith(".js", StringComparison.Ordinal) || path.EndsWith(".mjs", StringComparison.Ordinal)))
        {
            var source = File.ReadAllText(module);
            Assert.IsFalse(
                source.Contains("process.env", StringComparison.Ordinal),
                "Vendored module '" + Path.GetFileName(module) + "' references process.env and cannot load in a browser.");
        }
    }

    [TestMethod]
    public void VueDraggable_BoundExports_ExistInTheUpstreamEntry()
    {
        // 宿主函数条目（@# 描述）与组件代理（Transform.Component 的 ExportName）都必须在上游入口存在；
        // 默认导出的组件代理改用 default 导出断言。
        var upstream = ReadUpstreamEntryExports();
        var hostNames = ReadBoundExportNames(typeof(VueDraggable));
        var componentExports = ReadComponentExports(typeof(VueDraggable).Assembly);
        Assert.IsTrue(hostNames.Count + componentExports.Count >= 2,
            "Expected at least 2 bound entries, found " + (hostNames.Count + componentExports.Count) + ".");

        var named = componentExports.Where(static name => name != "default").Concat(hostNames).ToHashSet(StringComparer.Ordinal);
        var missing = named.Except(upstream).Order().ToArray();
        Assert.IsFalse(missing.Length > 0, "Bound exports missing from the vendored upstream entry: " + string.Join(", ", missing));

        if (componentExports.Contains("default"))
            Assert.IsTrue(Entries.Any(static path => File.ReadAllText(path).Contains("export default", StringComparison.Ordinal)),
                "A component proxy binds the default export, but no vendored entry declares one.");
    }

    private static HashSet<string> ReadComponentExports(System.Reflection.Assembly assembly)
        => assembly.GetExportedTypes()
            .Select(static type => type.GetCustomAttribute<ECMAScriptAttribute>())
            .Where(static attribute => attribute?.Transform == Transform.Component)
            .Select(static attribute => attribute!.ExportName ?? "default")
            .ToHashSet(StringComparer.Ordinal);

    [TestMethod]
    public void VueDraggable_Inventory_RecordsPackageVersionsAndFingerprint()
    {
        var inventory = GetInventory();
        var packages = inventory.GetProperty("packages");
        Assert.AreEqual(inventory.GetProperty("version").GetString(), packages.GetProperty("vue-draggable-plus").GetString());

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

    private static HashSet<string> ReadBoundExportNames(Type hostType)
        => hostType
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(method => method.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .SingleOrDefault()?.Description)
            .Where(description => description is not null && description.StartsWith("@#", StringComparison.Ordinal))
            .Select(description => description![2..])
            .ToHashSet(StringComparer.Ordinal);

    private static HashSet<string> ReadUpstreamEntryExports()
    {
        var exports = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in Entries)
        {
            var source = File.ReadAllText(entry);
            foreach (var match in System.Text.RegularExpressions.Regex.Matches(source, "(?m)^export (?:declare )?(?:function|const|class|let|var) (\\w+)").Cast<System.Text.RegularExpressions.Match>())
                exports.Add(match.Groups[1].Value);
            // 导出块常跨多行（打包器输出），必须用 Singleline 才能匹配整块；空白需容忍换行。
            var exportBlocks = System.Text.RegularExpressions.Regex.Matches(
                source,
                "export\\s*\\{([^}]+)\\}(?:\\s*from\\s*[\"'][^\"']+[\"'])?\\s*;",
                System.Text.RegularExpressions.RegexOptions.Singleline);
            foreach (System.Text.RegularExpressions.Match match in exportBlocks)
            {
                foreach (var specifier in match.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = System.Text.RegularExpressions.Regex.Split(specifier.Trim(), "\\s+as\\s+");
                    var exported = parts.Length > 1 ? parts[1].Trim() : parts[0].Trim();
                    if (exported != "default" && exported.Length > 0)
                        exports.Add(exported);
                }
            }
        }

        return exports;
    }

    private static IEnumerable<string> Entries => new[] { GetProjectPath("dist", "vue-draggable-plus", "vue-draggable-plus.js") };

    private static void AssertAllManifestFilesHashCorrect(JsonElement root)
    {
        foreach (var entry in root.GetProperty("imports").EnumerateObject())
        {
            AssertFileHash(entry.Value.GetProperty("development").GetString()!, entry.Value.GetProperty("developmentHash").GetString()!);
            AssertFileHash(entry.Value.GetProperty("production").GetString()!, entry.Value.GetProperty("productionHash").GetString()!);
            foreach (var file in entry.Value.GetProperty("files").EnumerateArray())
                AssertFileHash(file.GetProperty("path").GetString()!, file.GetProperty("hash").GetString()!);
        }

        foreach (var style in root.GetProperty("styles").EnumerateArray())
            AssertFileHash(style.GetProperty("path").GetString()!, style.GetProperty("hash").GetString()!);

        foreach (var file in root.GetProperty("files").EnumerateArray())
            AssertFileHash(file.GetProperty("path").GetString()!, file.GetProperty("hash").GetString()!);
    }

    private static void AssertFileHash(string relativePath, string expectedHash)
    {
        var fullPath = GetProjectPath(relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.IsTrue(File.Exists(fullPath), "Manifest file '" + relativePath + "' is missing from the package tree.");
        var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fullPath))).ToLowerInvariant();
        Assert.AreEqual(expectedHash.ToLowerInvariant(), actual, "Hash mismatch for '" + relativePath + "'.");
    }

    private static JsonElement GetInventory()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetProjectPath("inventory.json")));
        return document.RootElement.Clone();
    }

    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VueDraggable"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
