namespace Jazor.VueHost.LanguageServers;

internal sealed class LanguageServerCatalog
{
    private const string RoslynServerPathEnvironmentVariable = "JAZOR_VUEHOST_ROSLYN_SERVER";
    private const string RoslynExtensionAssemblyEnvironmentVariable = "JAZOR_VUEHOST_ROSLYN_EXTENSION";
    private const string RazorSourceGeneratorEnvironmentVariable = "JAZOR_VUEHOST_RAZOR_SOURCE_GENERATOR";
    private const string RazorDesignTimePathEnvironmentVariable = "JAZOR_VUEHOST_RAZOR_DESIGN_TIME";
    private const string CSharpDesignTimePathEnvironmentVariable = "JAZOR_VUEHOST_CSHARP_DESIGN_TIME";
    private const string VolarServerPathEnvironmentVariable = "JAZOR_VUEHOST_VOLAR_SERVER";
    private const string VolarNodePathEnvironmentVariable = "JAZOR_VUEHOST_VOLAR_NODE";
    private const string TypeScriptServerPathEnvironmentVariable = "JAZOR_VUEHOST_TSSERVER";
    private const string TypeScriptSdkPathEnvironmentVariable = "JAZOR_VUEHOST_TSDK";
    private const string RazorServiceHubRootEnvironmentVariable = "JAZOR_VUEHOST_RAZOR_SERVICEHUB_ROOT";

    public ExternalProcessOptions? Roslyn { get; init; }

    public ExternalProcessOptions? Volar { get; init; }

    public ExternalProcessOptions? TypeScript { get; init; }

    public string? RoslynExtensionAssemblyPath { get; init; }

    public string? RazorSourceGeneratorPath { get; init; }

    public string? RazorDesignTimePath { get; init; }

    public string? CSharpDesignTimePath { get; init; }

    public string? RazorServiceHubRoot { get; init; }

    public static LanguageServerCatalog CreateDefault()
    {
        var csharpExtensionRoot = GetLatestDirectory(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vscode", "extensions"),
            "ms-dotnettools.csharp-*");
        var roslynExtensionAssemblyPath = ResolveExistingFile(
            Environment.GetEnvironmentVariable(RoslynExtensionAssemblyEnvironmentVariable),
            CombineFile(csharpExtensionRoot?.FullName, ".razorExtension", "Microsoft.VisualStudioCode.RazorExtension.dll"));
        var razorSourceGeneratorPath = ResolveExistingFile(
            Environment.GetEnvironmentVariable(RazorSourceGeneratorEnvironmentVariable),
            CombineFile(csharpExtensionRoot?.FullName, ".roslyn", "Microsoft.CodeAnalysis.ExternalAccess.RazorCompiler.dll"));
        var razorDesignTimePath = ResolveExistingFile(
            Environment.GetEnvironmentVariable(RazorDesignTimePathEnvironmentVariable),
            CombineFile(csharpExtensionRoot?.FullName, ".razorExtension", "Targets", "Microsoft.NET.Sdk.Razor.DesignTime.targets"));
        var csharpDesignTimePath = ResolveExistingFile(
            Environment.GetEnvironmentVariable(CSharpDesignTimePathEnvironmentVariable),
            CombineFile(csharpExtensionRoot?.FullName, ".razorExtension", "Targets", "Microsoft.CSharpExtension.DesignTime.targets"));
        var typeScriptServerPath = ResolveTypeScriptServerPath();
        var typeScriptSdkPath = ResolveTypeScriptSdkPath(typeScriptServerPath);
        var nodePath = ResolveNodePath();
        var volarServerPath = ResolveVolarServerPath();

        return new LanguageServerCatalog
        {
            Roslyn = ResolveRoslyn(
                roslynExtensionAssemblyPath,
                razorSourceGeneratorPath,
                razorDesignTimePath,
                csharpDesignTimePath),
            Volar = ResolveVolar(nodePath, volarServerPath, typeScriptSdkPath),
            TypeScript = ResolveTypeScript(nodePath, typeScriptServerPath),
            RoslynExtensionAssemblyPath = roslynExtensionAssemblyPath,
            RazorSourceGeneratorPath = razorSourceGeneratorPath,
            RazorDesignTimePath = razorDesignTimePath,
            CSharpDesignTimePath = csharpDesignTimePath,
            RazorServiceHubRoot = ResolveRazorServiceHubRoot()
        };
    }

    private static ExternalProcessOptions? ResolveRoslyn(
        string? roslynExtensionAssemblyPath,
        string? razorSourceGeneratorPath,
        string? razorDesignTimePath,
        string? csharpDesignTimePath)
    {
        var path = ResolveExistingFile(
            Environment.GetEnvironmentVariable(RoslynServerPathEnvironmentVariable),
            GetLatestDirectoryFile(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vscode", "extensions"),
                "ms-dotnettools.csharp-*",
                ".roslyn",
                "Microsoft.CodeAnalysis.LanguageServer.exe"));
        if (path is null)
        {
            return null;
        }

        var arguments = new List<string>
        {
            "--stdio",
            "--autoLoadProjects",
            "--clientProcessId",
            Environment.ProcessId.ToString()
        };
        AddArgumentPair(arguments, "--extension", roslynExtensionAssemblyPath);
        AddArgumentPair(arguments, "--razorSourceGenerator", razorSourceGeneratorPath);
        AddArgumentPair(arguments, "--razorDesignTimePath", razorDesignTimePath);
        AddArgumentPair(arguments, "--csharpDesignTimePath", csharpDesignTimePath);

        return new ExternalProcessOptions
        {
            Name = "Roslyn",
            FileName = path,
            Arguments = [.. arguments],
            WorkingDirectory = Path.GetDirectoryName(path)
        };
    }

    private static ExternalProcessOptions? ResolveVolar(
        string? nodePath,
        string? volarServerPath,
        string? typeScriptSdkPath)
    {
        if (nodePath is null || volarServerPath is null || typeScriptSdkPath is null)
        {
            return null;
        }

        return new ExternalProcessOptions
        {
            Name = "Volar",
            FileName = nodePath,
            Arguments =
            [
                volarServerPath,
                "--stdio",
                $"--tsdk={typeScriptSdkPath}"
            ],
            WorkingDirectory = Path.GetDirectoryName(volarServerPath)
        };
    }

    private static ExternalProcessOptions? ResolveTypeScript(
        string? nodePath,
        string? typeScriptServerPath)
    {
        if (nodePath is null || typeScriptServerPath is null)
        {
            return null;
        }

        return new ExternalProcessOptions
        {
            Name = "TypeScriptServer",
            FileName = nodePath,
            Arguments =
            [
                typeScriptServerPath,
                "--stdio"
            ],
            WorkingDirectory = Path.GetDirectoryName(typeScriptServerPath)
        };
    }

    private static string? ResolveRazorServiceHubRoot()
        => ResolveExistingDirectory(
            Environment.GetEnvironmentVariable(RazorServiceHubRootEnvironmentVariable),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Microsoft Visual Studio",
                "18",
                "Insiders",
                "Common7",
                "IDE",
                "CommonExtensions",
                "Microsoft",
                "RazorLanguageServices",
                "ServiceHubCore"));

    private static string? ResolveTypeScriptServerPath()
        => ResolveExistingFile(
            Environment.GetEnvironmentVariable(TypeScriptServerPathEnvironmentVariable),
            GetLatestDirectoryFile(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code"),
                "*",
                "resources",
                "app",
                "extensions",
                "node_modules",
                "typescript",
                "lib",
                "tsserver.js"),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Microsoft Visual Studio",
                "18",
                "Insiders",
                "Common7",
                "IDE",
                "CommonExtensions",
                "Microsoft",
                "TSServer",
                "tsserver.js"));

    private static string? ResolveTypeScriptSdkPath(string? typeScriptServerPath)
    {
        var configured = ResolveExistingDirectory(Environment.GetEnvironmentVariable(TypeScriptSdkPathEnvironmentVariable));
        if (configured is not null)
        {
            return configured;
        }

        return typeScriptServerPath is null
            ? null
            : Path.GetDirectoryName(typeScriptServerPath);
    }

    private static string? ResolveVolarServerPath()
        => ResolveExistingFile(
            Environment.GetEnvironmentVariable(VolarServerPathEnvironmentVariable),
            CombineFile(Environment.CurrentDirectory, "node_modules", "@vue", "language-server", "bin", "vue-language-server.js"),
            GetLatestDirectoryFile(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vscode", "extensions"),
                "vue.volar-*",
                "dist",
                "language-server.js"),
            GetLatestDirectoryFile(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".trae-cn", "extensions"),
                "vue.volar-*",
                "dist",
                "language-server.js"));

    private static string? ResolveNodePath()
    {
        var configured = Environment.GetEnvironmentVariable(VolarNodePathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        return "node";
    }

    private static string? ResolveExistingFile(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string? ResolveExistingDirectory(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string? GetLatestDirectoryFile(string rootDirectory, string pattern, params string[] relativePath)
    {
        var directory = GetLatestDirectory(rootDirectory, pattern);
        if (directory is null)
        {
            return null;
        }

        var path = Path.Combine([directory.FullName, .. relativePath]);
        return File.Exists(path) ? path : null;
    }

    private static DirectoryInfo? GetLatestDirectory(string rootDirectory, string pattern)
    {
        if (!Directory.Exists(rootDirectory))
        {
            return null;
        }

        return new DirectoryInfo(rootDirectory)
            .EnumerateDirectories(pattern, SearchOption.TopDirectoryOnly)
            .OrderByDescending(static candidate => candidate.LastWriteTimeUtc)
            .FirstOrDefault();
    }

    private static string? CombineFile(string? rootPath, params string[] pathSegments)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            return null;
        }

        var path = Path.Combine([rootPath, .. pathSegments]);
        return File.Exists(path) ? path : null;
    }

    private static void AddArgumentPair(ICollection<string> arguments, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        arguments.Add(name);
        arguments.Add(value);
    }
}
