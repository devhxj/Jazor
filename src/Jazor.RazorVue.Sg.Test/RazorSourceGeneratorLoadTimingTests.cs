using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSourceGeneratorLoadTimingTests
{
    [TestMethod]
    public void ExternalBuild_AnalyzerModuleInitializer_RunsBeforeRazorCompilerAssemblyLoad()
    {
        var rootDirectory = CreateTemporaryDirectory();

        try
        {
            var toolset = RazorSdkToolsetProbeResolver.Resolve();
            if (toolset is null)
            {
                Assert.Inconclusive("A Razor SDK toolset could not be resolved for the load-timing probe.");
            }

            var analyzerDirectory = Path.Combine(rootDirectory, "analyzer");
            var projectDirectory = Path.Combine(rootDirectory, "project");
            Directory.CreateDirectory(analyzerDirectory);
            Directory.CreateDirectory(projectDirectory);

            var analyzerAssemblyPath = Path.Combine(analyzerDirectory, "TimingProbe.Analyzer.dll");
            var logPath = Path.Combine(rootDirectory, "timing.log");
            var outputRoot = Path.Combine(rootDirectory, "out");
            var intermediateRoot = Path.Combine(rootDirectory, "obj");

            CompileTimingProbeAnalyzer(analyzerAssemblyPath);
            WriteProjectFiles(
                projectDirectory,
                analyzerAssemblyPath,
                toolset.SdkVersion,
                toolset.TargetFramework,
                toolset.RazorLangVersion);

            var buildResult = RunDotNetBuild(
                projectDirectory,
                logPath,
                outputRoot,
                intermediateRoot);

            Assert.IsTrue(buildResult.ExitCode == 0, buildResult.DescribeFailure());
            Assert.IsTrue(File.Exists(logPath), "The analyzer timing log file was not produced.");

            var logLines = File.ReadAllLines(logPath)
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            TestContext.WriteLine("Timing log:");
            foreach (var line in logLines)
            {
                TestContext.WriteLine("  " + line);
            }

            Assert.IsTrue(
                logLines.Any(static line => string.Equals(line, "ModuleInitializer|RazorLoaded=False", StringComparison.Ordinal)),
                "The analyzer module initializer did not run before the Razor compiler assembly became observable.");

            var razorAssemblyLoadIndex = Array.FindIndex(
                logLines,
                static line => string.Equals(line, "AssemblyLoad|Microsoft.CodeAnalysis.Razor.Compiler", StringComparison.Ordinal));
            Assert.IsTrue(razorAssemblyLoadIndex >= 0, "The timing probe did not observe Microsoft.CodeAnalysis.Razor.Compiler loading.");

            var moduleInitializerIndex = Array.FindIndex(
                logLines,
                static line => string.Equals(line, "ModuleInitializer|RazorLoaded=False", StringComparison.Ordinal));
            Assert.IsTrue(moduleInitializerIndex >= 0, "The timing probe did not record the module initializer event.");
            Assert.IsTrue(
                moduleInitializerIndex < razorAssemblyLoadIndex,
                "The analyzer module initializer did not execute before the Razor compiler assembly load event.");
        }
        finally
        {
            TryDeleteDirectory(rootDirectory);
        }
    }

    public TestContext TestContext { get; set; } = default!;

    private static void CompileTimingProbeAnalyzer(string outputAssemblyPath)
    {
        const string source =
            """
            using System;
            using System.IO;
            using System.Linq;
            using System.Reflection;
            using System.Runtime.CompilerServices;
            using Microsoft.CodeAnalysis;

            namespace TimingProbe;

            internal static class ProbeRuntime
            {
                private static string? _logPath;

                [ModuleInitializer]
                internal static void InitializeModule()
                {
                    _logPath = Environment.GetEnvironmentVariable("JAZOR_RAZOR_SG_TIMING_LOG");
                    Log("ModuleInitializer|RazorLoaded=" + IsRazorCompilerLoaded().ToString());
                    AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
                }

                private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
                {
                    var assemblyName = args.LoadedAssembly.GetName().Name;
                    if (string.Equals(assemblyName, "Microsoft.CodeAnalysis.Razor.Compiler", StringComparison.Ordinal))
                    {
                        Log("AssemblyLoad|" + assemblyName);
                    }
                }

                internal static void LogGeneratorInitialize()
                    => Log("GeneratorInitialize|RazorLoaded=" + IsRazorCompilerLoaded().ToString());

                private static bool IsRazorCompilerLoaded()
                    => AppDomain.CurrentDomain.GetAssemblies()
                        .Any(static assembly => string.Equals(assembly.GetName().Name, "Microsoft.CodeAnalysis.Razor.Compiler", StringComparison.Ordinal));

                private static void Log(string message)
                {
                    if (string.IsNullOrWhiteSpace(_logPath))
                    {
                        return;
                    }

                    var directory = Path.GetDirectoryName(_logPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(_logPath, message + Environment.NewLine);
                }
            }

            [Generator]
            public sealed class TimingProbeGenerator : IIncrementalGenerator
            {
                public void Initialize(IncrementalGeneratorInitializationContext context)
                {
                    ProbeRuntime.LogGeneratorInitialize();
                    context.RegisterPostInitializationOutput(static output =>
                    {
                        output.AddSource(
                            "TimingProbe.Generated.g.cs",
                            "internal static class TimingProbeGenerated { internal const int Value = 1; }");
                    });
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "TimingProbe.Analyzer",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(source, path: "TimingProbe.Analyzer.cs")
            ],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Directory.CreateDirectory(Path.GetDirectoryName(outputAssemblyPath)!);
        using var stream = File.Create(outputAssemblyPath);
        var emitResult = compilation.Emit(stream);
        Assert.IsTrue(
            emitResult.Success,
            string.Join(
                Environment.NewLine,
                emitResult.Diagnostics
                    .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(static diagnostic => diagnostic.ToString())));
    }

    private static void WriteProjectFiles(
        string projectDirectory,
        string analyzerAssemblyPath,
        string sdkVersion,
        string targetFramework,
        string razorLangVersion)
    {
        File.WriteAllText(
            Path.Combine(projectDirectory, "global.json"),
            """
            {
              "sdk": {
                "version": "SDK_VERSION"
              }
            }
            """.Replace("SDK_VERSION", sdkVersion, StringComparison.Ordinal));

        File.WriteAllText(
            Path.Combine(projectDirectory, "TimingProbe.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk.Razor">
              <PropertyGroup>
                <TargetFramework>TARGET_FRAMEWORK</TargetFramework>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
                <RazorLangVersion>RAZOR_LANG_VERSION</RazorLangVersion>
                <UseRazorSourceGenerator>true</UseRazorSourceGenerator>
              </PropertyGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
                <Analyzer Include="ANALYZER_PATH" />
              </ItemGroup>
            </Project>
            """.Replace("ANALYZER_PATH", analyzerAssemblyPath, StringComparison.Ordinal)
            .Replace("TARGET_FRAMEWORK", targetFramework, StringComparison.Ordinal)
            .Replace("RAZOR_LANG_VERSION", razorLangVersion, StringComparison.Ordinal));

        File.WriteAllText(
            Path.Combine(projectDirectory, "Counter.razor"),
            """
            <h1>Hello</h1>
            """);
    }

    private static BuildProcessResult RunDotNetBuild(
        string projectDirectory,
        string logPath,
        string outputRoot,
        string intermediateRoot)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments =
                    "build \"TimingProbe.csproj\" /nodeReuse:false -p:UseSharedCompilation=false " +
                    "-p:BaseOutputPath=\"" + EnsureTrailingDirectorySeparator(outputRoot) + "\" " +
                    "-p:BaseIntermediateOutputPath=\"" + EnsureTrailingDirectorySeparator(intermediateRoot) + "\" " +
                    "-v minimal",
                WorkingDirectory = projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.StartInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        process.StartInfo.Environment["JAZOR_RAZOR_SG_TIMING_LOG"] = logPath;

        Assert.IsTrue(process.Start(), "Failed to start dotnet build for timing probe.");
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new BuildProcessResult(process.ExitCode, standardOutput, standardError);
    }

    private static string CreateTemporaryDirectory()
    {
        return RazorSgTestHost.CreateTestArtifactDirectory("source-generator-timing");
    }

    private static string EnsureTrailingDirectorySeparator(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty.", nameof(path));

        var normalized = Path.GetFullPath(path).Replace('\\', '/');
        return normalized.EndsWith("/", StringComparison.Ordinal)
            ? normalized
            : normalized + "/";
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed record BuildProcessResult(
        int ExitCode,
        string StandardOutput,
        string StandardError)
    {
        public string DescribeFailure()
        {
            var builder = new StringBuilder();
            builder.AppendLine("dotnet build failed.");
            builder.AppendLine("ExitCode: " + ExitCode);
            if (!string.IsNullOrWhiteSpace(StandardOutput))
            {
                builder.AppendLine("stdout:");
                builder.AppendLine(StandardOutput);
            }

            if (!string.IsNullOrWhiteSpace(StandardError))
            {
                builder.AppendLine("stderr:");
                builder.AppendLine(StandardError);
            }

            return builder.ToString();
        }
    }
}
