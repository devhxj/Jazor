using System.Diagnostics;

/// <summary>
/// Provides a repository-owned workspace for tests that need to materialize files.
/// Keeping this root under <c>.tmp</c> makes test execution independent of the host's
/// system temporary directory and keeps all generated files inside the checkout.
/// </summary>
internal static class RepositoryTemp
{
    private static readonly Lazy<string> RootValue = new(ResolveRoot, LazyThreadSafetyMode.ExecutionAndPublication);

    public static string Root
    {
        get
        {
            Directory.CreateDirectory(RootValue.Value);
            return RootValue.Value;
        }
    }

    public static string CreateDirectory(string prefix)
    {
        var path = Path.Combine(Root, prefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    public static DirectoryInfo CreateSubdirectory(string prefix)
        => new(CreateDirectory(prefix));

    public static void ApplyProcessEnvironment(ProcessStartInfo startInfo)
    {
        ArgumentNullException.ThrowIfNull(startInfo);

        var root = Root;
        var temp = Path.Combine(root, "process-temp");
        var dotnet = Path.Combine(root, "dotnet-home");
        var nuget = Path.Combine(dotnet, ".nuget", "packages") + Path.DirectorySeparatorChar;
        var nugetHttp = Path.Combine(root, "nuget-http-cache");
        var deno = Path.Combine(root, "deno-cache");
        var npm = Path.Combine(root, "npm-cache");
        foreach (var directory in new[] { temp, dotnet, nuget, nugetHttp, deno, npm })
            Directory.CreateDirectory(directory);

        startInfo.Environment["TEMP"] = temp;
        startInfo.Environment["TMP"] = temp;
        startInfo.Environment["DOTNET_CLI_HOME"] = dotnet;
        startInfo.Environment["NUGET_PACKAGES"] = nuget;
        startInfo.Environment["NUGET_HTTP_CACHE_PATH"] = nugetHttp;
        startInfo.Environment["DENO_DIR"] = deno;
        startInfo.Environment["NPM_CONFIG_CACHE"] = npm;
        startInfo.Environment["npm_config_cache"] = npm;
    }

    public static bool IsOwned(string path)
    {
        var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(Root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            directory = directory.Parent;

        var repositoryRoot = directory?.FullName
            ?? throw new InvalidOperationException("Unable to locate Jazor.slnx for the repository-owned test workspace.");
        var root = Path.Combine(repositoryRoot, ".tmp", "test-workspaces");
        var temp = Path.Combine(root, "process-temp");
        var dotnet = Path.Combine(root, "dotnet-home");
        var nuget = Path.Combine(dotnet, ".nuget", "packages") + Path.DirectorySeparatorChar;
        var nugetHttp = Path.Combine(root, "nuget-http-cache");
        var deno = Path.Combine(root, "deno-cache");
        var npm = Path.Combine(root, "npm-cache");
        foreach (var path in new[] { temp, dotnet, nuget, nugetHttp, deno, npm })
            Directory.CreateDirectory(path);

        Environment.SetEnvironmentVariable("TEMP", temp);
        Environment.SetEnvironmentVariable("TMP", temp);
        Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", dotnet);
        Environment.SetEnvironmentVariable("NUGET_PACKAGES", nuget);
        Environment.SetEnvironmentVariable("NUGET_HTTP_CACHE_PATH", nugetHttp);
        Environment.SetEnvironmentVariable("DENO_DIR", deno);
        Environment.SetEnvironmentVariable("NPM_CONFIG_CACHE", npm);
        Environment.SetEnvironmentVariable("npm_config_cache", npm);
        return root;
    }
}
