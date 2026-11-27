using Jolt.Compiler;
using Jolt.Project;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class SharedCSharpSourcesTests
{
    [TestMethod]
    public async Task GetGeneratedRoot_ReadsImportedJazorDirBeforeFirstBuild()
    {
        var fixture = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-direct-output-" + Guid.NewGuid()));
        try
        {
            var project = fixture.CreateSubdirectory("Web");
            Assert.AreEqual(Path.Combine(project.FullName, ".jazor"),
                await JoltProjectLoader.GetGeneratedRootAsync(project.FullName, CancellationToken.None));
            await File.WriteAllTextAsync(Path.Combine(project.FullName, "Web.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup><TargetFramework>net11.0</TargetFramework></PropertyGroup>
                  <Import Project="../Generated.props" />
                </Project>
                """);
            await File.WriteAllTextAsync(Path.Combine(fixture.FullName, "Generated.props"), """
                <Project><PropertyGroup>
                  <JazorDir Condition="'$(TargetFramework)' == 'net11.0'">../generated</JazorDir>
                </PropertyGroup></Project>
                """);

            Assert.AreEqual(Path.Combine(fixture.FullName, "generated"),
                await JoltProjectLoader.GetGeneratedRootAsync(project.FullName, CancellationToken.None));
        }
        finally
        {
            fixture.Delete(recursive: true);
        }
    }

    [TestMethod]
    public async Task LoadSources_SkipsEvaluatedOutputDirectories()
    {
        var project = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-custom-output-" + Guid.NewGuid()));
        try
        {
            var projectPath = Path.Combine(project.FullName, "App.csproj");
            await File.WriteAllTextAsync(projectPath, """<Project Sdk="Microsoft.NET.Sdk" />""");
            await File.WriteAllTextAsync(Path.Combine(project.FullName, "Main.jazor"), "<div/>");
            var generated = project.CreateSubdirectory("custom-output");
            var intermediate = project.CreateSubdirectory("custom-intermediate");
            var context = project.CreateSubdirectory(".jazor");
            await File.WriteAllLinesAsync(Path.Combine(context.FullName, "project.inputs"),
                [$"project|{projectPath}", "namespace|Custom.App", $"output|{generated.FullName}", "output|custom-intermediate"]);
            await File.WriteAllTextAsync(Path.Combine(generated.FullName, "Generated.jazor"), "<div/>");
            await File.WriteAllTextAsync(Path.Combine(intermediate.FullName, "Intermediate.vue"), "<template/>");

            var workspace = await JoltProjectLoader.LoadSourcesAsync(project.FullName, new Dictionary<string, string>
            {
                [Path.Combine(intermediate.FullName, "Unsaved.jazor")] = "<div/>",
            }, CancellationToken.None);

            Assert.AreEqual("Custom.App", workspace.Options.RootNamespace);
            CollectionAssert.AreEqual(new[] { "Main.jazor" }, workspace.Sources.Select(source => source.Path).ToArray());
            Assert.IsEmpty(workspace.VuePaths);
        }
        finally
        {
            project.Delete(recursive: true);
        }
    }

    [TestMethod]
    public async Task LoadSources_ExpandsSharedGlobsAndOverlaysInStableOrder()
    {
        var project = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-shared-glob-" + Guid.NewGuid()));
        try
        {
            await Write("Z/RowContracts.cs", "disk");
            await Write("RootContracts.cs", "root");
            await Write("A/RowContracts.cs", "first");
            await Write("A/Extras.cs", "unmatched");
            await Write("node_modules/PackageContracts.cs", "dependency");
            await Write("bin/Debug/BuildContracts.cs", "build");
            await Write("obj/GeneratedContracts.cs", "generated");
            await Write("dist/BundleContracts.cs", "bundle");
            await Write(".hidden/CacheContracts.cs", "cache");
            await Write("A/.PrivateContracts.cs", "hidden");
            await Write("package.json", """
                {"jolt":{"csharpSources":["Z/RowContracts.cs","A/*.cs","**/*Contracts.cs","NoMatches/*.cs"]}}
                """);

            var workspace = await JoltProjectLoader.LoadSourcesAsync(project.FullName, new Dictionary<string, string>
            {
                ["Z/RowContracts.cs"] = "overlay",
                [Path.Combine(project.FullName, "New", "LiveContracts.cs")] = "unsaved",
                ["node_modules/TempContracts.cs"] = "dependency overlay",
                ["obj/TempContracts.cs"] = "build overlay",
                [".hidden/TempContracts.cs"] = "hidden overlay",
                ["../OutsideContracts.cs"] = "outside",
            }, CancellationToken.None);

            CollectionAssert.AreEqual(new[] { "A/Extras.cs", "A/RowContracts.cs", "New/LiveContracts.cs", "RootContracts.cs", "Z/RowContracts.cs" },
                workspace.Options.CSharpSources.Select(source => Path.GetRelativePath(project.FullName, source.Path).Replace('\\', '/')).ToArray());
            CollectionAssert.AreEqual(new[] { "unmatched", "first", "unsaved", "root", "overlay" },
                workspace.Options.CSharpSources.Select(source => source.Content).ToArray());
        }
        finally
        {
            project.Delete(recursive: true);
        }

        async Task Write(string relative, string contents)
        {
            var file = Path.Combine(project.FullName, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            await File.WriteAllTextAsync(file, contents);
        }
    }

    [TestMethod]
    public async Task LoadSources_RejectsGlobOutsideProject()
    {
        var project = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-shared-glob-" + Guid.NewGuid()));
        try
        {
            await File.WriteAllTextAsync(Path.Combine(project.FullName, "package.json"),
                """{"jolt":{"csharpSources":["../**/*Contracts.cs"]}}""");
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
                JoltProjectLoader.LoadSourcesAsync(project.FullName, null, CancellationToken.None));
        }
        finally
        {
            project.Delete(recursive: true);
        }
    }

    [TestMethod]
    public async Task Compile_UsesManifestSourcesAndOverlays_AndIgnoresToolScripts()
    {
        var project = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-shared-csharp-" + Guid.NewGuid()));
        try
        {
            var contract = Path.Combine(project.FullName, "Contract.cs");
            await File.WriteAllTextAsync(contract, "namespace Shared; public sealed record Row(string Label);");
            await File.WriteAllTextAsync(Path.Combine(project.FullName, "Tool.cs"), "this is not C#");
            await File.WriteAllTextAsync(Path.Combine(project.FullName, "Page.jazor"), """
                @using Shared
                <p>@Entry.Label</p>
                @code { private Row Entry = new("Shared source"); }
                """);
            await File.WriteAllTextAsync(Path.Combine(project.FullName, "package.json"),
                """{"jolt":{"csharpSources":["Contract.cs"]}}""");

            var workspace = await JoltProjectLoader.LoadSourcesAsync(project.FullName, null, CancellationToken.None);
            var output = await JoltProjectCompiler.CompileAsync(workspace.Sources, workspace.Options);
            Assert.HasCount(1, workspace.Options.CSharpSources);
            Assert.IsEmpty(output.Diagnostics, string.Join(Environment.NewLine, output.Diagnostics));
            Assert.HasCount(1, output.Modules);

            workspace = await JoltProjectLoader.LoadSourcesAsync(project.FullName,
                new Dictionary<string, string> { [contract] = "namespace Shared; public sealed record Row(string Other);" }, CancellationToken.None);
            output = await JoltProjectCompiler.CompileAsync(workspace.Sources, workspace.Options);
            Assert.IsTrue(output.Diagnostics.Any(diagnostic => diagnostic.Id == "CS1061"));

            workspace = await JoltProjectLoader.LoadSourcesAsync(project.FullName,
                new Dictionary<string, string> { [contract] = "namespace Shared; public sealed record Row(MissingType Label);" }, CancellationToken.None);
            output = await JoltProjectCompiler.CompileAsync(workspace.Sources, workspace.Options);
            Assert.IsTrue(output.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0246" &&
                diagnostic.Location.GetLineSpan().Path == contract));

            File.Delete(contract);
            try
            {
                await JoltProjectLoader.LoadSourcesAsync(project.FullName, null, CancellationToken.None);
                Assert.Fail("The configured source is missing.");
            }
            catch (FileNotFoundException error)
            {
                Assert.AreEqual(contract, error.FileName);
            }
        }
        finally
        {
            project.Delete(recursive: true);
        }
    }
}
