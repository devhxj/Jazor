using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECMAScriptVeeValidateTest;

/// <summary>验证 vee-validate 绑定交付标准 npm package graph 和稳定的类型入口。</summary>
[TestClass]
public sealed class VeeValidateManifestTests
{
    [TestMethod]
    public void VeeValidate_Manifest_DeclaresPackageIdentityAndLogicalEntry()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        var root = manifest.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("vee-validate", root.GetProperty("libraryId").GetString());
        Assert.AreEqual(GetInventory().GetProperty("version").GetString(), root.GetProperty("version").GetString());

        var packages = root.GetProperty("packages");
        Assert.AreEqual("npm", packages.GetProperty("vee-validate").GetProperty("source").GetString());
        Assert.AreEqual("4.15.1", packages.GetProperty("vee-validate").GetProperty("version").GetString());
        Assert.AreEqual("npm", packages.GetProperty("vue").GetProperty("source").GetString());

        var imports = root.GetProperty("imports");
        CollectionAssert.AreEquivalent(new[] { "vee-validate" }, imports.EnumerateObject().Select(static entry => entry.Name).ToArray());
        var entry = imports.GetProperty("vee-validate");
        CollectionAssert.AreEquivalent(
            new[] { "@vue/devtools-api", "vue" },
            entry.GetProperty("productionDependencies").EnumerateArray().Select(static value => value.GetString()!).ToArray());
        Assert.AreEqual("vee-validate", entry.GetProperty("development").GetString());
        Assert.AreEqual("vee-validate", entry.GetProperty("production").GetString());
        Assert.IsFalse(entry.TryGetProperty("developmentHash", out _));
        Assert.IsFalse(entry.TryGetProperty("productionHash", out _));
        Assert.IsFalse(entry.TryGetProperty("files", out _));
        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());
        Assert.IsFalse(root.TryGetProperty("dist", out _));
        Assert.IsFalse(root.TryGetProperty("vendor", out _));
    }

    [TestMethod]
    public void VeeValidate_RuntimeCarrierIsResolvedFromTheRestoredPackageGraph()
    {
        Assert.IsFalse(Directory.Exists(GetProjectPath("dist")));
        Assert.IsFalse(Directory.Exists(GetProjectPath("vendor")));
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.AreEqual("npm", manifest.RootElement.GetProperty("packages").GetProperty("vee-validate").GetProperty("source").GetString());
    }

    [TestMethod]
    public void VeeValidate_BoundExportsUseTheAuthorPackageEntry()
    {
        var bound = ReadBoundExportNames(typeof(ECMAScript.VeeValidate));
        Assert.IsTrue(bound.Count >= 20, "The first slice should expose the form and field composables.");
        CollectionAssert.Contains(bound.ToArray(), "useForm");
        CollectionAssert.Contains(bound.ToArray(), "useField");
        CollectionAssert.Contains(bound.ToArray(), "configure");

        using var manifest = JsonDocument.Parse(File.ReadAllText(GetProjectPath("manifest.json")));
        Assert.IsTrue(manifest.RootElement.GetProperty("imports").TryGetProperty("vee-validate", out _));
    }

    [TestMethod]
    public void VeeValidate_PublicReturnShapesRemainTyped()
    {
        var form = typeof(ECMAScript.VeeValidateFormReturn<string>);
        Assert.IsNotNull(form.GetProperty("Values"));
        Assert.IsNotNull(form.GetProperty("Errors"));
        Assert.AreEqual(typeof(ECMAScript.PromiseResult<ECMAScript.VeeValidateValidationResult>), form.GetMethod("Validate")!.ReturnType);

        var field = typeof(ECMAScript.VeeValidateFieldReturn<string>);
        Assert.IsNotNull(field.GetProperty("Value"));
        Assert.IsNotNull(field.GetProperty("Meta"));
        Assert.AreEqual(typeof(ECMAScript.PromiseResult<ECMAScript.VeeValidateValidationResult>), field.GetMethod("Validate")!.ReturnType);
    }

    [TestMethod]
    public void VeeValidate_InventoryRecordsPackageVersionAndFingerprint()
    {
        var inventory = GetInventory();
        Assert.AreEqual("vee-validate", inventory.GetProperty("upstream").GetString());
        Assert.AreEqual("MIT", inventory.GetProperty("license").GetString());
        Assert.AreEqual("4.15.1", inventory.GetProperty("packages").GetProperty("vee-validate").GetString());
        Assert.AreEqual(1, inventory.GetProperty("vendoredModuleCount").GetInt32());

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
            .Select(static attribute => attribute.Description)
            .Where(static description => description.StartsWith("@#", StringComparison.Ordinal))
            .Select(static description => description[2..])
            .ToHashSet(StringComparer.Ordinal);

    private static JsonElement GetInventory()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetProjectPath("inventory.json")));
        return document.RootElement.Clone();
    }

    private static string GetPackageRoot([CallerFilePath] string sourceFilePath = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VeeValidate"));

    private static string GetProjectPath(params string[] paths)
        => Path.Combine(new[] { GetPackageRoot() }.Concat(paths).ToArray());
}
