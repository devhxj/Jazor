using System.Text.Json;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class ProjectEntryWriterTests
{
    [TestMethod]
    public void Write_UsesCompleteProjectPathsAndStableOrdering()
    {
        using var workspace = new EntryWorkspace();

        var paths = ProjectEntryWriter.Write(
            workspace.Root,
            ["host/app.js", "clr/System/BooleanModule.js", "host/app.js"]);

        Assert.HasCount(1, paths);
        Assert.AreEqual(
            "export * from \"./clr/System/BooleanModule.js\";\n" +
            "export * from \"./host/app.js\";\n",
            File.ReadAllText(Path.Combine(workspace.Root, ProjectEntryWriter.BrowserEntryFileName)));
    }

    [TestMethod]
    public void Write_AddsAndRemovesSsrEntryWithoutExecutingBrowserRoots()
    {
        using var workspace = new EntryWorkspace();

        ProjectEntryWriter.Write(workspace.Root, ["app/root.js"], enableSsr: true);
        var ssrEntry = File.ReadAllText(Path.Combine(workspace.Root, ProjectEntryWriter.SsrEntryFileName));
        Assert.IsFalse(ssrEntry.Contains("app/root.js", StringComparison.Ordinal));
        StringAssert.Contains(ssrEntry, "import { createSSRApp } from \"vue\";");
        StringAssert.Contains(ssrEntry, "Deno.serve({");
        StringAssert.Contains(ssrEntry, "new URL(\"./\", import.meta.url)");

        ProjectEntryWriter.Write(workspace.Root, ["app/root.js"]);
        Assert.IsFalse(File.Exists(Path.Combine(workspace.Root, ProjectEntryWriter.SsrEntryFileName)));
    }

    [TestMethod]
    public void PackageWriter_DeclaresVisibleEntryExports()
    {
        using var workspace = new EntryWorkspace();
        var libraries = CreateEmptyLibraries();

        LibraryPackageWriter.WritePackageProject(workspace.Root, libraries, hasSsrEntry: true);

        using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.Root, "package.json")));
        Assert.AreEqual("./entry.js", package.RootElement.GetProperty("main").GetString());
        var exports = package.RootElement.GetProperty("exports");
        Assert.AreEqual("./entry.js", exports.GetProperty(".").GetString());
        Assert.AreEqual("./ssr-entry.js", exports.GetProperty("./ssr").GetString());
    }

    [TestMethod]
    public void Write_UnchangedProjectPreservesFileTimestampsAndContent()
    {
        using var workspace = new EntryWorkspace();
        var libraries = CreateEmptyLibraries();
        var manifest = new ManifestModel("host.dll", []);
        var manifestPath = Path.Combine(workspace.Root, "jazor-manifest.json");

        void WriteProject()
        {
            ProjectEntryWriter.Write(workspace.Root, ["app/root.js"], enableSsr: true);
            LibraryPackageWriter.WritePackageProject(workspace.Root, libraries, hasSsrEntry: true);
            manifest.Save(manifestPath);
        }

        WriteProject();
        var originalTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var files = Directory.GetFiles(workspace.Root);
        var originalContents = files.ToDictionary(path => path, File.ReadAllText);
        foreach (var file in files)
            File.SetLastWriteTimeUtc(file, originalTime);

        WriteProject();

        foreach (var file in files)
        {
            Assert.AreEqual(originalTime, File.GetLastWriteTimeUtc(file), file);
            Assert.AreEqual(originalContents[file], File.ReadAllText(file), file);
        }
        CollectionAssert.AreEquivalent(files, Directory.GetFiles(workspace.Root));
    }

    [TestMethod]
    public void PackageWriter_PreservesAuthoredViteConfiguration()
    {
        using var workspace = new EntryWorkspace();
        var authored = "export default { build: { outDir: 'custom-dist' } };\n";
        File.WriteAllText(Path.Combine(workspace.Root, ViteProjectWriter.ConfigFileName), authored);

        LibraryPackageWriter.WritePackageProject(workspace.Root, CreateEmptyLibraries());

        Assert.AreEqual(authored, File.ReadAllText(Path.Combine(workspace.Root, ViteProjectWriter.ConfigFileName)));
    }

    private static LibraryAssets CreateEmptyLibraries()
        => new(
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            [], [], [], [], [], [],
            new Dictionary<string, LibraryPackageReference>());

    private sealed class EntryWorkspace : IDisposable
    {
        public EntryWorkspace()
        {
            Root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", "entries", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
