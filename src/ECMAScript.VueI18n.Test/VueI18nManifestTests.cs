using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using ECMAScript;

namespace ECMAScriptVueI18nTest;

/// <summary>
/// File-backed manifest, inventory, and upstream drift checks for the vendored vue-i18n runtime.
/// 资源闭包元数据与上游 drift 检查；manifest 哈希与 vendored 文件必须一一对应。
/// </summary>
[TestClass]
public sealed class VueI18nManifestTests
{
    [TestMethod]
    public void VueI18n_Manifest_DeclaresLockedBrowserBuildWithVerifiableHashes()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vue-i18n", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());
        Assert.IsTrue(root.GetProperty("requires").TryGetProperty("vue3", out var vue));
        Assert.IsTrue(!string.IsNullOrWhiteSpace(vue.GetString()));

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(
            new[] { "vue-i18n" },
            imports.EnumerateObject().Select(static entry => entry.Name).ToArray());

        // The vendored artifact is the upstream `browser`-condition build
        // (vue-i18n.esm-browser.js), which inlines @intlify/* and only needs the vue peer.
        // The bundler variant (vue-i18n.mjs) references process.env at module top level and
        // cannot load in a browser, so it must not be vendored.
        var entry = imports.GetProperty("vue-i18n");
        Assert.AreEqual("dist/vue-i18n/dist/vue-i18n.esm-browser.js", entry.GetProperty("development").GetString());
        CollectionAssert.AreEquivalent(
            new[] { "vue" },
            entry.GetProperty("productionDependencies").EnumerateArray().Select(static value => value.GetString()!).ToArray());

        foreach (var declared in imports.EnumerateObject())
        {
            Assert.AreEqual("module", declared.Value.GetProperty("type").GetString());
            Assert.AreEqual(declared.Value.GetProperty("development").GetString(), declared.Value.GetProperty("production").GetString());
            Assert.AreEqual(declared.Value.GetProperty("developmentHash").GetString(), declared.Value.GetProperty("productionHash").GetString());
        }

        AssertAllManifestFilesHashCorrect(root);
    }

    [TestMethod]
    public void VueI18n_VendoredDist_ExactlyMatchesTheManifestClosure()
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
    public void VueI18n_BoundExports_ExistInTheUpstreamEntry()
    {
        var bound = typeof(VueI18n)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(method => method.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .SingleOrDefault()?.Description)
            .Where(description => description is not null && description.StartsWith("@#", StringComparison.Ordinal))
            .Select(description => description![2..])
            .ToHashSet(StringComparer.Ordinal);
        Assert.IsTrue(bound.Count >= 2, $"Expected the createI18n/useI18n entry pair, found {bound.Count}.");

        var upstream = ReadNamedExports(GetProjectPath("dist", "vue-i18n", "dist", "vue-i18n.esm-browser.js"));
        var missing = bound.Except(upstream).Order().ToArray();
        Assert.IsFalse(missing.Length > 0, $"Bound exports missing from the vendored upstream entry: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void VueI18n_ComposerSurface_CoversLocaleMessagesAndFormatting()
    {
        var composer = typeof(VueI18nComposer);
        Assert.AreEqual(typeof(Vue.IVueRef<string>), composer.GetProperty("Locale")!.PropertyType);
        Assert.AreEqual(typeof(Vue.IVueRef<string>), composer.GetProperty("FallbackLocale")!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<Vue.VueDictionary>), composer.GetProperty("Messages")!.PropertyType);
        Assert.AreEqual(typeof(string[]), composer.GetProperty("AvailableLocales")!.PropertyType);

        var tOverloads = composer.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(static method => method.Name == "T" && method.ReturnType == typeof(string))
            .ToArray();
        Assert.IsTrue(tOverloads.Length >= 3, $"The composer must expose the t() overload family, found {tOverloads.Length}.");
        Assert.AreEqual(typeof(bool), composer.GetMethod("Te", [typeof(string)])!.ReturnType);
        Assert.AreEqual(typeof(Vue.VueDictionary), composer.GetMethod("Tm", [typeof(string)])!.ReturnType);
        Assert.IsTrue(composer.GetMethods().Any(static method => method.Name == "D"), "The composer must expose datetime formatting.");
    }

    [TestMethod]
    public void VueI18n_Inventory_RecordsPackageVersionsAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual(inventory.GetProperty("version").GetString(), inventory.GetProperty("packages").GetProperty("vue-i18n").GetString());

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

    private static HashSet<string> ReadNamedExports(string path)
    {
        var source = File.ReadAllText(path);
        var exports = new HashSet<string>(StringComparer.Ordinal);
        foreach (var match in System.Text.RegularExpressions.Regex.Matches(source, @"(?m)^export (?:declare )?(?:function|const|class|let|var) (\w+)").Cast<System.Text.RegularExpressions.Match>())
            exports.Add(match.Groups[1].Value);
        foreach (var match in System.Text.RegularExpressions.Regex.Matches(source, @"export \{ ([^}]+) \}(?: from [""'][^""']+[""'])?;").Cast<System.Text.RegularExpressions.Match>())
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
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VueI18n"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
