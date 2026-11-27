using System.Diagnostics;
using System.Text.Json;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SdkFeedbackPublishTests
{
    [TestMethod]
    public async Task Build_SourceSdkHandoffMaterializesOnlyChangedModule()
    {
        using var workspace = new Workspace();
        var emitHost = Path.Combine(AppContext.BaseDirectory, OperatingSystem.IsWindows() ? "Jazor.Emit.exe" : "Jazor.Emit");
        var project = workspace.CreateProject("<JazorDir>frontend</JazorDir>", $$"""
            <Target Name="_ResolveJazorEmitTool">
              <PropertyGroup><_JazorToolCommand>&quot;{{emitHost}}&quot;</_JazorToolCommand></PropertyGroup>
            </Target>
            <Target Name="HandoffAfterBuild" AfterTargets="Build" DependsOnTargets="JazorModules" />
            """);
        var build = new[] { "build", project, "-nologo", "/m:1", "/nr:false" };
        var changed = Path.Combine(workspace.Root, "frontend", "changed.mjs");
        var stable = Path.Combine(workspace.Root, "frontend", "stable.mjs");
        var before = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        WriteCatalog(1);
        var initial = await RunAsync(workspace.Root, build);
        Assert.AreEqual(0, initial.ExitCode, initial.Output);
        File.SetLastWriteTimeUtc(stable, before);
        File.SetLastWriteTimeUtc(changed, before);
        var repeated = await RunAsync(workspace.Root, build);
        Assert.AreEqual(0, repeated.ExitCode, repeated.Output);
        Assert.AreEqual(before, File.GetLastWriteTimeUtc(changed));

        WriteCatalog(2);
        var updated = await RunAsync(workspace.Root, build);
        Assert.AreEqual(0, updated.ExitCode, updated.Output);
        StringAssert.Contains(await File.ReadAllTextAsync(changed), "export const version = 2;");
        Assert.AreEqual(before, File.GetLastWriteTimeUtc(stable));
        Assert.IsFalse(File.Exists(Path.Combine(workspace.Root, "frontend", "entry.js")));

        void WriteCatalog(int version)
        {
            var body = $"export const version = {version};";
            const string unchanged = """export const load = () => import("/Pages/View.jazor");""";
            workspace.Write("ModuleCatalog.cs", $$"""
                namespace Jazor.Generated;

                internal static class ModuleCatalog
                {
                    internal const int SchemaVersion = 2;
                    internal const string AssemblyName = "Fixture";
                    internal static System.Collections.IEnumerable GetModules() => new Module[]
                    {
                        new("Changed", "Changed", "changed.mjs", "{{body}}", "{{Hash(body)}}", []),
                        new("Stable", "Stable", "stable.mjs", {{JsonSerializer.Serialize(unchanged)}}, "{{Hash(unchanged)}}", []),
                    };
                    private sealed record Module(string Id, string DeclarationName, string RelativePath,
                        string Content, string Hash, string[] PackageImports);
                }
                """);
        }

        static string Hash(string text) => Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    [TestMethod]
    public async Task SdkDefaults_ProjectConditionalModeRunsBeforeDefaultAndAuthoredJazorSourcesRemainInputs()
    {
        using var workspace = new Workspace();
        var project = workspace.CreateProject("""
            <JazorMode Condition="'$(JazorMode)' == ''">debug</JazorMode>
            """);
        workspace.Write("Jazor/Authored.cs", "public static class Authored { public const int Value = 8; }");
        workspace.Write("Jazor/Authored.razor", "<div>authored source</div>");
        workspace.Write(".jazor/runtime/generated.js", "export const value = 8;");
        var result = await RunAsync(workspace.Root,
            ["msbuild", project, "-nologo", "-getProperty:JazorMode", "-getItem:Compile,Content,None"]);
        Assert.AreEqual(0, result.ExitCode, result.Output);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.AreEqual("debug", json.RootElement.GetProperty("Properties").GetProperty("JazorMode").GetString());
        var items = json.RootElement.GetProperty("Items");
        Assert.IsTrue(ItemsContain(items, "Compile", "Jazor/Authored.cs"), result.Output);
        Assert.IsTrue(ItemsContain(items, "Content", "Jazor/Authored.razor") ||
            ItemsContain(items, "None", "Jazor/Authored.razor"), result.Output);
        Assert.IsFalse(ItemsContain(items, "Content", ".jazor/runtime/generated.js"), result.Output);
        Assert.IsFalse(ItemsContain(items, "None", ".jazor/runtime/generated.js"), result.Output);
    }

    [TestMethod]
    public async Task SdkDefaults_CustomRelativeGeneratedRootExcludesOnlyThatRoot()
    {
        using var workspace = new Workspace();
        var project = workspace.CreateProject("<JazorDir>generated/frontend</JazorDir>");
        workspace.Write("generated/frontend/runtime/generated.js", "export const value = 8;");
        workspace.Write("jazor/authored.js", "export const authored = true;");
        var result = await RunAsync(workspace.Root, ["msbuild", project, "-nologo", "-getProperty:JazorMode", "-getItem:Content,None"]);
        Assert.AreEqual(0, result.ExitCode, result.Output);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.AreEqual("none", json.RootElement.GetProperty("Properties").GetProperty("JazorMode").GetString());
        var items = json.RootElement.GetProperty("Items");
        Assert.IsFalse(ItemsContain(items, "Content", "generated/frontend/runtime/generated.js"), result.Output);
        Assert.IsFalse(ItemsContain(items, "None", "generated/frontend/runtime/generated.js"), result.Output);
        Assert.IsTrue(ItemsContain(items, "Content", "jazor/authored.js") || ItemsContain(items, "None", "jazor/authored.js"), result.Output);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("<JazorMode>none</JazorMode>")]
    public async Task Publish_CustomDirectoryWithDefaultOrDisabledModeSkipsEmit(string mode)
    {
        using var workspace = new Workspace();
        var project = workspace.CreateProject("<JazorDir>frontend</JazorDir>" + mode);
        var result = await RunAsync(workspace.Root, ["publish", project, "-nologo", "-o", "published"]);
        Assert.AreEqual(0, result.ExitCode, result.Output);
        Assert.IsTrue(File.Exists(Path.Combine(workspace.Root, "published", "Fixture.dll")));
        Assert.IsFalse(Directory.Exists(Path.Combine(workspace.Root, "frontend")));
        Assert.IsFalse(Directory.Exists(Path.Combine(workspace.Root, "published", "jazor")));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Publish_RuntimeClosureContainsChunksAndSsrExportsAndSupportsSourceMapOptIn(bool includeSourceMaps)
    {
        using var workspace = new Workspace();
        var project = workspace.CreateProject($"""
            <JazorMode>debug</JazorMode>
            <JazorSSR>true</JazorSSR>
            <JazorDir>generated/frontend</JazorDir>
            <JazorPublishSourceMaps>{includeSourceMaps.ToString().ToLowerInvariant()}</JazorPublishSourceMaps>
            """, """
            <!-- This fixture isolates SDK selection from the separately tested Emit bundler.
                 Publishing must invoke the release materialization target after Debug build. -->
            <Target Name="JazorDebug" />
            <Target Name="_JazorEmitRelease">
              <WriteLinesToFile File="$(MSBuildProjectDirectory)/release-emitted.txt" Lines="release" Overwrite="true" />
            </Target>
            """);
        workspace.Write("generated/frontend/dist/bundle.js", "export const ready = true;");
        workspace.Write("generated/frontend/dist/chunks/view.js", "export const value = 8;");
        workspace.Write("generated/frontend/dist/bundle.js.map", "{}");
        workspace.Write("generated/frontend/ssr/ssr-entry.js", "export const server = true;");
        workspace.Write("generated/frontend/ssr/chunks/view.js", "export default {};");
        workspace.Write("generated/frontend/ssr/package.json", "{\"type\":\"module\"}");
        workspace.Write("generated/frontend/ssr/ssr-entry.js.map", "{}");
        workspace.Write("generated/frontend/host/source.mjs", "export const authored = 8;");
        workspace.Write("generated/frontend/host/source.mjs.map", "{}");
        workspace.Write("generated/frontend/node_modules/vue/package.json", "{}");
        workspace.Write("generated/frontend/package.json", "{}");
        workspace.Write("generated/frontend/vite.config.js", "export default {};");
        workspace.Write("wwwroot/logo.txt", "authored public file");
        var result = await RunAsync(workspace.Root,
            ["publish", project, "-nologo", "-o", Path.Combine(workspace.Root, "deployment"), "/m:1", "/nr:false"]);
        Assert.AreEqual(0, result.ExitCode, result.Output);
        Assert.IsTrue(File.Exists(Path.Combine(workspace.Root, "release-emitted.txt")), result.Output);
        var closureRoot = Path.Combine(workspace.Root, "deployment", "jazor");
        var actual = Directory.EnumerateFiles(closureRoot, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(closureRoot, path).Replace('\\', '/')).OrderBy(path => path).ToArray();
        var expected = new List<string>
        {
            "ssr/chunks/view.js", "ssr/package.json", "ssr/ssr-entry.js"
        };
        if (includeSourceMaps)
            expected.Add("ssr/ssr-entry.js.map");
        CollectionAssert.AreEqual(expected.OrderBy(path => path).ToArray(), actual, result.Output);
        var webRoot = Path.Combine(workspace.Root, "deployment", "wwwroot");
        var browserFiles = Directory.EnumerateFiles(webRoot, "*", SearchOption.AllDirectories)
            .Where(path => Path.GetExtension(path) is not (".gz" or ".br"))
            .Select(path => Path.GetRelativePath(webRoot, path).Replace('\\', '/')).OrderBy(path => path).ToArray();
        var expectedBrowserFiles = new List<string> { "bundle.js", "chunks/view.js", "logo.txt" };
        if (includeSourceMaps)
            expectedBrowserFiles.Add("bundle.js.map");
        CollectionAssert.AreEqual(expectedBrowserFiles.OrderBy(path => path).ToArray(), browserFiles, result.Output);
    }

    [TestMethod]
    public async Task SdkRuntimeSelection_UsesRequestedRidAndSdkGraphThenHostRidForPortableBuilds()
    {
        using var workspace = new Workspace();
        var project = workspace.CreateProject("", """
            <Target Name="FixtureRuntimeSelection" DependsOnTargets="_ResolveJazorRuntimeIdentifier" />
            """);
        foreach (var requestedRid in new[] { "linux-x64", "" })
        {
            var arguments = new List<string>
            {
                "msbuild", project, "-nologo", "-t:FixtureRuntimeSelection",
                "-getProperty:_JazorTargetRuntimeIdentifier,NETCoreSdkRuntimeIdentifier,_JazorRuntimeIdentifierGraphArgument,RuntimeIdentifierGraphPath"
            };
            if (requestedRid.Length > 0)
                arguments.Add("-p:RuntimeIdentifier=" + requestedRid);
            var result = await RunAsync(workspace.Root, [.. arguments]);
            Assert.AreEqual(0, result.ExitCode, result.Output);
            using var json = JsonDocument.Parse(result.StandardOutput);
            var properties = json.RootElement.GetProperty("Properties");
            var expectedRid = requestedRid.Length > 0 ? requestedRid : properties.GetProperty("NETCoreSdkRuntimeIdentifier").GetString();
            Assert.AreEqual(expectedRid, properties.GetProperty("_JazorTargetRuntimeIdentifier").GetString());
            StringAssert.Contains(properties.GetProperty("_JazorRuntimeIdentifierGraphArgument").GetString()!,
                properties.GetProperty("RuntimeIdentifierGraphPath").GetString()!);
        }
    }

    private static bool ItemsContain(JsonElement items, string itemType, string relativePath)
        => items.GetProperty(itemType).EnumerateArray().Any(item =>
            string.Equals(item.GetProperty("Identity").GetString()!.Replace('\\', '/'), relativePath, StringComparison.OrdinalIgnoreCase));

    private static async Task<ProcessResult> RunAsync(string root, string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        RepositoryTemp.ApplyProcessEnvironment(start);
        start.Environment["DOTNET_NOLOGO"] = "1";
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            throw;
        }
        return new ProcessResult(process.ExitCode, (await output).Trim(), (await error).Trim());
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public string Output => StandardOutput + Environment.NewLine + StandardError;
    }

    private sealed class Workspace : IDisposable
    {
        public string Root { get; } = RepositoryTemp.CreateDirectory("sdk-feedback-");

        public string CreateProject(string properties, string targets = "")
        {
            var repositoryRoot = FindRepositoryRoot();
            var sourceBuild = Path.Combine(repositoryRoot, "src", "Jazor", "build").Replace('\\', '/');
            // Explicit SDK imports preserve the package props -> project body -> package targets
            // order without packing the full compiler for an MSBuild-only contract regression.
            Write("Fixture.csproj", $$"""
                <Project>
                  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk.Web" />
                  <Import Project="{{sourceBuild}}/Jazor.props" />
                  <PropertyGroup>
                    <TargetFramework>net11.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                    {{properties}}
                  </PropertyGroup>
                  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk.Web" />
                  <Import Project="{{sourceBuild}}/Jazor.targets" />
                  {{targets}}
                </Project>
                """);
            Write("Program.cs", "var builder = WebApplication.CreateBuilder(args); var app = builder.Build(); app.MapGet(\"/\", () => \"ready\"); app.Run();");
            return Path.Combine(Root, "Fixture.csproj");
        }

        public void Write(string relativePath, string text)
        {
            var path = Path.Combine(Root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);

        private static string FindRepositoryRoot()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                    return directory.FullName;
            throw new InvalidOperationException("Could not locate repository root.");
        }
    }
}
