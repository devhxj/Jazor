using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using ECMAScript.Contract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Lucide.Test;

[TestClass]
public sealed class LucideBindingTests
{
    [TestMethod]
    public void GeneratedCatalog_ContainsOfficialExportsAndDocumentation()
    {
        var assembly = typeof(ECMAScript.Lucide.User).Assembly;
        var icons = assembly.GetExportedTypes()
            .Where(type => type.BaseType == typeof(ECMAScript.Lucide.LucideIconBase))
            .ToArray();

        Assert.AreEqual(1704, icons.Length);
        foreach (var icon in icons)
        {
            var import = icon.GetCustomAttribute<ECMAScriptAttribute>();
            Assert.IsNotNull(import, icon.Name);
            Assert.AreEqual("lucide-vue-next", import!.Import, icon.Name);
            Assert.AreEqual(icon.Name, icon.GetCustomAttribute<ECMAScriptNameAttribute>()?.Name, icon.Name);
            var documentation = icon.GetCustomAttributes(inherit: false)
                .Select(attribute => attribute.ToString())
                .ToArray();
            Assert.IsNotNull(documentation);
        }

        var baseType = typeof(ECMAScript.Lucide.LucideIconBase);
        Assert.IsNotNull(baseType.GetProperty(nameof(ECMAScript.Lucide.LucideIconBase.Size))?.GetCustomAttribute<ParameterAttribute>());
        Assert.IsNotNull(baseType.GetProperty(nameof(ECMAScript.Lucide.LucideIconBase.AdditionalAttributes))?.GetCustomAttribute<ParameterAttribute>());
    }

    [TestMethod]
    public void Manifest_UsesStandardNpmSpecifier()
    {
        using var document = JsonDocument.Parse(System.IO.File.ReadAllText(FindManifestPath()));
        var root = document.RootElement;
        Assert.AreEqual("npm", root.GetProperty("source").GetString());
        Assert.AreEqual("lucide-vue-next", root.GetProperty("libraryId").GetString());
        Assert.AreEqual("lucide-vue-next", root.GetProperty("imports").EnumerateObject().Single().Name);
    }

    [TestMethod]
    public void Manifest_DeclaresLockstepPackageContract()
    {
        using var document = JsonDocument.Parse(System.IO.File.ReadAllText(FindManifestPath()));
        var root = document.RootElement;

        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        var package = root.GetProperty("packages").GetProperty("lucide-vue-next");
        Assert.AreEqual("npm", package.GetProperty("source").GetString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(package.GetProperty("version").GetString()));
        Assert.IsFalse(string.IsNullOrWhiteSpace(package.GetProperty("version").GetString()));
        Assert.AreEqual(0, root.GetProperty("styles").GetArrayLength());
    }

    private static string FindManifestPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "src", "ECMAScript.Lucide", "manifest.json");
            if (System.IO.File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException("Could not locate ECMAScript.Lucide manifest from the test output directory.");
    }

    [TestMethod]
    public void DocumentationFile_DescribesGeneratedComponentContract()
    {
        var assemblyPath = typeof(ECMAScript.Lucide.User).Assembly.Location;
        var path = Path.ChangeExtension(assemblyPath, ".xml");
        Assert.IsTrue(System.IO.File.Exists(path), $"Generated XML documentation was not copied to {path}.");

        var document = XDocument.Load(path);
        var members = document.Descendants("member")
            .Select(member => member.Attribute("name")?.Value)
            .Where(static name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        Assert.IsTrue(members.Contains("T:ECMAScript.Lucide.LucideIconBase"));
        Assert.IsTrue(members.Contains("T:ECMAScript.Lucide.User"));
        Assert.IsTrue(members.Contains("P:ECMAScript.Lucide.LucideIconBase.AdditionalAttributes"));
    }
}
