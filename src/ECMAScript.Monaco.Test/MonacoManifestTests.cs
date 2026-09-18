using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using ECMAScript;

#pragma warning disable CA1416

namespace ECMAScriptMonacoTest;

/// <summary>
/// File-backed manifest, inventory, and bundled-closure checks for the vendored Monaco runtime.
/// Monaco 的资源闭包由 esbuild 打包，因此本测试同时校验打包产物、worker 声明与浏览器安全。
/// </summary>
[TestClass]
public sealed class MonacoManifestTests
{
    [TestMethod]
    public void Monaco_Manifest_DeclaresBundledClosureWithWorkersAndStyles()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("monaco", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(new[] { "monaco-editor" }, imports.EnumerateObject().Select(static entry => entry.Name).ToArray());

        var entry = imports.GetProperty("monaco-editor");
        Assert.AreEqual("dist/monaco-editor/editor.api.mjs", entry.GetProperty("development").GetString());
        Assert.AreEqual(entry.GetProperty("developmentHash").GetString(), entry.GetProperty("productionHash").GetString());

        // 五个 worker 以模块依赖声明，Emit 随之物化；样式单独声明。
        var moduleDependencies = entry.GetProperty("productionModuleDependencies").EnumerateArray()
            .Select(static value => value.GetString()!)
            .ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "dist/monaco-editor/editor.worker.start.mjs",
                "dist/monaco-editor/json.worker.mjs",
                "dist/monaco-editor/css.worker.mjs",
                "dist/monaco-editor/html.worker.mjs",
                "dist/monaco-editor/ts.worker.mjs"
            },
            moduleDependencies);

        // 每个 worker 都必须有可解析的 moduleId 文件条目。
        var fileIds = entry.GetProperty("files").EnumerateArray()
            .Select(static value => value.GetProperty("moduleId").GetString()!)
            .ToArray();
        CollectionAssert.AreEquivalent(moduleDependencies, fileIds);

        CollectionAssert.AreEquivalent(
            new[] { "dist/monaco-editor/editor.api.css" },
            root.GetProperty("styles").EnumerateArray().Select(static value => value.GetProperty("path").GetString()!).ToArray());

        // 许可证与第三方声明随包交付。
        CollectionAssert.AreEquivalent(
            new[] { "licenses/MONACO-LICENSE", "licenses/THIRD-PARTY-NOTICES.txt" },
            root.GetProperty("files").EnumerateArray().Select(static value => value.GetProperty("path").GetString()!).ToArray());

        AssertAllManifestFilesHashCorrect(root);
    }

    [TestMethod]
    public void Monaco_VendoredDist_ExactlyMatchesTheManifestClosure()
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
    public void Monaco_BundledArtifacts_AreSelfContainedEsmAndBrowserSafe()
    {
        // 打包产物必须是自包含 ESM（无裸 import），且 process.env 引用必须带环境守卫。
        // monaco 的 vs/base/common/process.js 使用 typeof process 守卫，属正当用法。
        foreach (var artifact in Directory.EnumerateFiles(GetProjectPath("dist"), "*.mjs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(artifact);
            var name = Path.GetFileName(artifact);

            // 顶层 import 说明符必须都是相对路径（打包后不应残留裸依赖）。
            var bare = System.Text.RegularExpressions.Regex.Matches(
                    source,
                    @"(?m)^\s*import\s+(?:[^'""]*from\s*)?['""]([^'""]+)['""]\s*;?\s*$")
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(static match => match.Groups[1].Value)
                .Where(static specifier => !specifier.StartsWith(".", StringComparison.Ordinal))
                .ToArray();
            Assert.IsFalse(bare.Length > 0, $"{name} still contains bare imports: {string.Join(", ", bare)}");

            var index = source.IndexOf("process.env", StringComparison.Ordinal);
            while (index >= 0)
            {
                var window = source[Math.Max(0, index - 400)..index];
                Assert.IsTrue(
                    window.Contains("typeof process", StringComparison.Ordinal) ||
                    window.Contains("typeof globalThis.process", StringComparison.Ordinal) ||
                    window.Contains("typeof window", StringComparison.Ordinal),
                    $"{name} references process.env without an environment guard.");
                index = source.IndexOf("process.env", index + 1, StringComparison.Ordinal);
            }
        }
    }

    [TestMethod]
    public void Monaco_BoundExports_ExistInTheBundledEntry()
    {
        var bound = typeof(Monaco)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(method => method.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .SingleOrDefault()?.Description)
            .Where(description => description is not null && description.StartsWith("@#", StringComparison.Ordinal))
            .Select(description => description![2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 11, $"Expected at least 11 bound editor APIs, found {bound.Count}.");

        // Monaco 的顶层导出是命名空间对象：编辑器函数（create/createModel/...）与 languages 都挂在
        // 顶层导出的 `editor` / `languages` 之下，而不是顶层具名导出。因此按命名空间成员断言。
        // Monaco's top-level exports are namespace objects: editor functions live under the exported
        // `editor` namespace rather than being top-level named exports.
        var entry = File.ReadAllText(GetProjectPath("dist", "monaco-editor", "editor.api.mjs"));
        Assert.IsTrue(
            System.Text.RegularExpressions.Regex.IsMatch(entry, @"export\s*\{[^}]*\beditor\b[^}]*\}", System.Text.RegularExpressions.RegexOptions.Singleline),
            "The bundled entry must export the `editor` namespace.");

        var missing = bound.Except(MonacoEditorNamespaceMembers).Order().ToArray();
        Assert.IsFalse(missing.Length > 0, "Bound exports missing from the `editor` namespace: " + string.Join(", ", missing));
    }

    [TestMethod]
    public void Monaco_Inventory_RecordsVersionFingerprintAndBuildNote()
    {
        var inventory = GetInventory();
        Assert.AreEqual(inventory.GetProperty("version").GetString(), inventory.GetProperty("packages").GetProperty("monaco-editor").GetString());
        Assert.IsTrue(inventory.GetProperty("buildNote").GetString()!.Contains("esbuild", StringComparison.Ordinal));

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
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.Monaco"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());

    // 上游 editor.api.d.ts 的 `editor` 命名空间成员中，本绑定使用的那部分；用于校验打包产物仍导出该命名空间。
    private static readonly HashSet<string> MonacoEditorNamespaceMembers = new(StringComparer.Ordinal)
    {
        "create",
        "createModel",
        "createDiffEditor",
        "getModels",
        "getEditors",
        "getDiffEditors",
        "setModelLanguage",
        "defineTheme",
        "setTheme",
        "setModelMarkers",
        "removeAllMarkers",
        "onDidChangeMarkers",
        "createWebWorker",
        "getModel",
        "onDidCreateModel",
        "onWillDisposeModel",
        "onDidChangeModelLanguage"
    };
}