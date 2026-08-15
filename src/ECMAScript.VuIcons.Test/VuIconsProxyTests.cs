using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.VuIcons.Test;

[TestClass]
public sealed class VuIconsProxyTests
{
    [TestMethod]
    public void VuIcons_StaticCatalogAndManifestMatchTheUpstreamIconSet()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(GetManifestPath()));
        var imports = manifest.RootElement.GetProperty("imports");
        var componentTypes = typeof(VuIcon).Assembly
            .GetExportedTypes()
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<VueLibraryComponentAttribute>()))
            .Where(static item => item.Attribute is not null)
            .OrderBy(static item => item.Type.Name, StringComparer.Ordinal)
            .ToArray();
        var staticTypes = componentTypes
            .Where(static item => item.Attribute!.ImportSpecifier.StartsWith("vu-icons/", StringComparison.Ordinal))
            .ToArray();
        var descriptorEntries = staticTypes
            .Select(static item => item.Attribute!.ImportSpecifier)
            .OrderBy(static entry => entry, StringComparer.Ordinal)
            .ToArray();
        var shippedEntries = Directory
            .EnumerateFiles(GetComponentsPath(), "*.mjs")
            .Select(static path => "vu-icons/" + Path.GetFileNameWithoutExtension(path))
            .OrderBy(static entry => entry, StringComparer.Ordinal)
            .ToArray();

        Assert.AreEqual(1821, shippedEntries.Length, "vu-icons 1.5.4 exposes 1821 static Vue3 icon wrappers.");
        Assert.AreEqual(shippedEntries.Length + 1, componentTypes.Length, "The dynamic VuIcon bridge is the only non-static descriptor.");
        Assert.AreEqual(shippedEntries.Length, staticTypes.Length);
        CollectionAssert.AreEquivalent(shippedEntries, descriptorEntries);
        Assert.AreEqual("1.5.4", manifest.RootElement.GetProperty("version").GetString());
        Assert.AreEqual(componentTypes.Length, imports.EnumerateObject().Count());
        Assert.AreEqual("dist/jazor-vu-icon.mjs", imports.GetProperty("vu-icons").GetProperty("production").GetString());
        Assert.AreEqual("^3.2.0", manifest.RootElement.GetProperty("requires").GetProperty("vue3").GetString());
        CollectionAssert.Contains(
            manifest.RootElement.GetProperty("styles").EnumerateArray().Select(static value => value.GetString()).ToArray(),
            "dist/jazor-vu-icon.css");

        foreach (var (type, attribute) in componentTypes)
        {
            Assert.IsNotNull(attribute);
            Assert.IsTrue(imports.TryGetProperty(attribute!.ImportSpecifier, out _), type.Name);
            Assert.AreEqual(type.Name, attribute.ExportName, type.Name);
        }
    }

    [TestMethod]
    public void VuIcons_EnumLiteralsExactlyMatchIconData()
    {
        var expected = Regex.Matches(
                File.ReadAllText(GetIconDataPath()),
                "^\\s*\"([^\"]+)\": \\{ viewBox:",
                RegexOptions.Multiline | RegexOptions.CultureInvariant)
            .Select(static match => match.Groups[1].Value)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        var actual = typeof(VuIconName)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(static field => field.GetCustomAttribute<ComponentDescriptionAttribute>()?.Description)
            .Where(static value => value is not null)
            .Select(static value => value![2..])
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        var wrapperNames = typeof(VuIcon).Assembly
            .GetExportedTypes()
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<VueLibraryComponentAttribute>()))
            .Where(static item => item.Attribute?.ImportSpecifier.StartsWith("vu-icons/", StringComparison.Ordinal) == true)
            .Select(static item => item.Type.Name[2..])
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

        Assert.AreEqual(1821, expected.Length);
        CollectionAssert.AreEqual(expected, actual);
        CollectionAssert.AreEqual(Enum.GetNames<VuIconName>().OrderBy(static value => value, StringComparer.Ordinal).ToArray(), wrapperNames);
        Assert.AreEqual("@#user", GetDescription(VuIconName.User));
        Assert.AreEqual("@#arrow-down-0-1", GetDescription(VuIconName.ArrowDown01));
        Assert.AreEqual("@#axis-3d", GetDescription(VuIconName.Axis3d));
    }

    [TestMethod]
    public void VuIcons_DynamicAndStaticAuthoringSurfacesKeepUpstreamProps()
    {
        Assert.AreEqual(typeof(VuIconComponentBase), typeof(VuUser).BaseType);
        Assert.AreEqual(typeof(VuIconName), typeof(VuIcon).GetProperty(nameof(VuIcon.Name))!.PropertyType);
        Assert.IsNotNull(typeof(VuIcon).GetProperty(nameof(VuIcon.Name))!.GetCustomAttribute<EditorRequiredAttribute>());
        Assert.IsNull(typeof(VuIcon).GetProperty("Icon"));
        Assert.AreEqual(typeof(Vue.VueStringNumberValue?), typeof(VuIcon).GetProperty(nameof(VuIcon.Size))!.PropertyType);
        Assert.AreEqual(typeof(bool?), typeof(VuIconComponentBase).GetProperty(nameof(VuIconComponentBase.Spin))!.PropertyType);
        Assert.AreEqual(
            "className",
            typeof(VuIconComponentBase).GetProperty(nameof(VuIconComponentBase.ClassName))!
                .GetCustomAttribute<ECMAScriptNameAttribute>()!.Name);
        Assert.AreEqual(
            "class",
            typeof(VuIcon).GetProperty(nameof(VuIcon.Class))!
                .GetCustomAttribute<ECMAScriptNameAttribute>()!.Name);
    }

    private static string GetManifestPath([CallerFilePath] string sourceFilePath = "")
        => GetProjectPath(sourceFilePath, "manifest.json");

    private static string GetComponentsPath([CallerFilePath] string sourceFilePath = "")
        => GetProjectPath(sourceFilePath, "dist", "components");

    private static string GetIconDataPath([CallerFilePath] string sourceFilePath = "")
        => GetProjectPath(sourceFilePath, "dist", "icons-data.js");

    private static string GetProjectPath(string sourceFilePath, params string[] paths)
        => Path.GetFullPath(Path.Combine(
            new[] { Path.GetDirectoryName(sourceFilePath)!, "..", "ECMAScript.VuIcons" }
                .Concat(paths)
                .ToArray()));

    private static string GetDescription(VuIconName icon)
        => typeof(VuIconName)
            .GetField(icon.ToString())!
            .GetCustomAttribute<ComponentDescriptionAttribute>()!
            .Description;
}
