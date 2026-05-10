using System.Collections.Concurrent;
using Jolt.Workspace;

namespace Jolt.Test;

internal sealed class JoltIntegrationTestTopology : IDisposable
{
    private readonly List<string> _solutionPaths = [];
    private bool _disposed;

    private JoltIntegrationTestTopology(string rootPath)
    {
        RootPath = rootPath;
    }

    public string RootPath { get; }

    public static JoltIntegrationTestTopology Create(string scenarioName)
    {
        var safeScenarioName = string.Join(
            "-",
            scenarioName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var rootPath = Path.Combine(
            Path.GetTempPath(),
            "jolt-integration-topology",
            safeScenarioName,
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);
        return new JoltIntegrationTestTopology(rootPath);
    }

    public JoltIntegrationSolution CreateSolution(string solutionName)
    {
        ThrowIfDisposed();

        var solutionRoot = Path.Combine(RootPath, solutionName);
        Directory.CreateDirectory(solutionRoot);
        var solutionPath = Path.Combine(solutionRoot, solutionName + ".slnx");
        var solution = new JoltIntegrationSolution(solutionName, solutionRoot, solutionPath);
        _solutionPaths.Add(solutionPath);
        solution.WriteSolutionFile();
        return solution;
    }

    public JoltIntegrationProject CreateSingleProjectSolution(
        string solutionName,
        string projectName,
        string? projectDirectoryName = null)
    {
        // 单项目场景仍然走 `.slnx -> project entry` 的真实拓扑，
        // 避免测试再次退回到手写零散 fixture。
        var solution = CreateSolution(solutionName);
        return solution.AddProject(projectName, projectDirectoryName);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        // 裸 topology 使用者也可能触发 `.slnx` resolver 缓存，统一在根生命周期结束时失效。
        foreach (var solutionPath in _solutionPaths)
        {
            JoltWorkspaceResolver.InvalidatePath(solutionPath);
        }

        DeleteDirectoryWithRetry(RootPath);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static void DeleteDirectoryWithRetry(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}

internal sealed class JoltIntegrationSolution
{
    private readonly List<JoltIntegrationProject> _projects = [];

    internal JoltIntegrationSolution(string name, string rootPath, string solutionPath)
    {
        Name = name;
        RootPath = rootPath;
        SolutionPath = solutionPath;
    }

    public string Name { get; }

    public string RootPath { get; }

    public string SolutionPath { get; }

    public IReadOnlyList<JoltIntegrationProject> Projects => _projects;

    public JoltIntegrationProject AddProject(string projectName, string? projectDirectoryName = null)
    {
        var projectRoot = string.IsNullOrWhiteSpace(projectDirectoryName)
            ? Path.Combine(RootPath, projectName)
            : string.Equals(projectDirectoryName, ".", StringComparison.Ordinal)
                ? RootPath
                : Path.Combine(RootPath, projectDirectoryName);
        Directory.CreateDirectory(projectRoot);

        var projectPath = Path.Combine(projectRoot, projectName + ".csproj");
        File.WriteAllText(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var project = new JoltIntegrationProject(projectName, projectRoot, projectPath, this);
        _projects.Add(project);
        WriteSolutionFile();
        return project;
    }

    internal void WriteSolutionFile()
    {
        // 集成测试拓扑只认 `.slnx`，并在每次增删项目后重写 project entries。
        // 这样一个测试场景可以用单个 Jolt 实例同时覆盖多 solution / 多 project。
        var projectLines = string.Join(
            Environment.NewLine,
            _projects.Select(project =>
            {
                var relativePath = Path.GetRelativePath(RootPath, project.ProjectPath).Replace('\\', '/');
                return $"  <Project Path=\"{relativePath}\" />";
            }));
        File.WriteAllText(
            SolutionPath,
            $$"""
            <Solution>
            {{projectLines}}
            </Solution>
            """);
    }
}

internal sealed record JoltIntegrationProject(
    string Name,
    string RootPath,
    string ProjectPath,
    JoltIntegrationSolution Solution)
{
    public string GetPath(params string[] relativeSegments)
        => Path.Combine(new[] { RootPath }.Concat(relativeSegments).ToArray());

    public string WriteFile(string relativePath, string content)
    {
        // 统一由 project root 负责落盘，保证测试文件天然处于 owning project 边界内。
        var path = GetPath(relativePath);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content);
        return path;
    }
}

internal sealed class JoltIntegrationProjectScope : IDisposable
{
    private readonly JoltIntegrationTestTopology _topology;
    private int _disposed;

    private JoltIntegrationProjectScope(JoltIntegrationTestTopology topology, JoltIntegrationProject project)
    {
        _topology = topology;
        Project = project;
    }

    public JoltIntegrationProject Project { get; }

    public string ProjectRoot => Project.RootPath;

    public string SolutionPath => Project.Solution.SolutionPath;

    public static JoltIntegrationProjectScope CreateSingleProject(
        string scenarioName,
        string solutionName,
        string projectName,
        string? projectDirectoryName = null)
    {
        var topology = JoltIntegrationTestTopology.Create(scenarioName);
        try
        {
            var project = topology.CreateSingleProjectSolution(solutionName, projectName, projectDirectoryName);
            return new JoltIntegrationProjectScope(topology, project);
        }
        catch
        {
            topology.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        // 缓存失效由 topology 统一处理，避免每种 fixture 各自实现一份回收逻辑。
        _topology.Dispose();
    }
}

internal static class JoltIntegrationRootedProjectDirectory
{
    private static readonly ConcurrentDictionary<string, JoltIntegrationProjectScope> Scopes = new(StringComparer.OrdinalIgnoreCase);

    public static string Create(
        string scenarioName,
        string solutionName,
        string projectName,
        string? projectDirectoryName = ".")
    {
        var scope = JoltIntegrationProjectScope.CreateSingleProject(
            scenarioName,
            solutionName,
            projectName,
            projectDirectoryName);
        var projectRoot = NormalizeDirectoryPath(scope.ProjectRoot);
        if (!Scopes.TryAdd(projectRoot, scope))
        {
            scope.Dispose();
            throw new IOException($"Temporary rooted project directory '{projectRoot}' is already tracked.");
        }

        return projectRoot;
    }

    public static bool TryDispose(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var projectRoot = NormalizeDirectoryPath(path);
        if (!Scopes.TryRemove(projectRoot, out var scope))
        {
            return false;
        }

        // 所有以 string 形式暴露的 rooted project 目录都从这里释放，保证 topology 根目录不残留。
        scope.Dispose();
        return true;
    }

    private static string NormalizeDirectoryPath(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
}
