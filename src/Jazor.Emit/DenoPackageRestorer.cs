using System.Text;
using System.Text.Json;
using DenoHost.Core;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Restores the package project emitted for one Jazor profile. The restore belongs to Emit so
/// browser bundling and optional SSR observe the same node_modules and deno.lock transaction.
/// </summary>
internal static class DenoPackageRestorer
{
    public static async Task RestoreAndCheckAsync(
        string workspaceRoot,
        string? executablePath,
        IReadOnlyList<string> entryPaths,
        LibraryAssets? libraries,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        ArgumentNullException.ThrowIfNull(entryPaths);

        var packagePath = Path.Combine(workspaceRoot, "package.json");
        if (!File.Exists(packagePath))
            throw new InvalidOperationException("Jazor package restore requires package.json in the project root.");

        var dependencyCount = ReadDependencyCount(packagePath);
        var lockPath = Path.Combine(workspaceRoot, "deno.lock");
        if (dependencyCount == 0)
        {
            // The package graph owns this directory. A clean graph must not retain a lock or
            // node_modules left by a previous profile, even when the caller requested an
            // incremental output transaction.
            DeleteFile(lockPath);
            DeleteDirectory(Path.Combine(workspaceRoot, "node_modules"));

            // Direct Emit callers may intentionally omit Deno for a graph with no dependencies.
            // MSBuild always supplies the packaged runtime, so production builds still run the
            // same generated-module check.
            if (!string.IsNullOrWhiteSpace(executablePath))
                await CheckAsync(workspaceRoot, ResolveExecutable(executablePath), entryPaths, libraries, cancellationToken).ConfigureAwait(false);
            return;
        }

        var deno = ResolveExecutable(executablePath);
        ProcessResult restoreResult;
        if (File.Exists(lockPath) && LockMatchesPackageJson(packagePath, lockPath))
        {
            restoreResult = await RunAsync(
                deno,
                workspaceRoot,
                [
                    "install",
                    "--package-json",
                    "--node-modules-dir=manual",
                    "--node-modules-linker=hoisted",
                    "--frozen=true"
                ],
                cancellationToken).ConfigureAwait(false);

            if (!restoreResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Deno frozen restore failed for an unchanged package identity.\n" +
                    FormatFailure("deno install --frozen=true", restoreResult));
            }
        }
        else
        {
            // Deno install updates a stale lock in most cases, but a graph that changes from an
            // external package to a local-only package can otherwise leave obsolete identities
            // behind. Remove the stale lock before resolving the new dependency graph.
            DeleteFile(lockPath);
            restoreResult = await RunAsync(
                deno,
                workspaceRoot,
                [
                    "install",
                    "--package-json",
                    "--node-modules-dir=manual",
                    "--node-modules-linker=hoisted",
                    "--frozen=false"
                ],
                cancellationToken).ConfigureAwait(false);
            if (!restoreResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Deno package restore failed.\n" + FormatFailure("deno install --node-modules-linker=hoisted", restoreResult));
            }
        }

        if (!File.Exists(lockPath) && RequiresRootLock(packagePath))
        {
            throw new InvalidOperationException(
                "Deno restored external packages but did not produce deno.lock. " +
                "The package graph cannot be committed without a frozen lock.");
        }

        if (!RequiresRootLock(packagePath))
            DeleteFile(lockPath);

        await CheckAsync(workspaceRoot, deno, entryPaths, libraries, cancellationToken).ConfigureAwait(false);
    }

    private static async Task CheckAsync(
        string workspaceRoot,
        string? executablePath,
        IReadOnlyList<string> entryPaths,
        LibraryAssets? libraries,
        CancellationToken cancellationToken)
    {
        var deno = ResolveExecutable(executablePath);
        var relativeEntries = entryPaths
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.GetRelativePath(workspaceRoot, path).Replace('\\', '/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();
        if (relativeEntries.Length == 0)
            return;

        if (relativeEntries.Any(static path => path.StartsWith("../", StringComparison.Ordinal) ||
                                               string.Equals(path, "..", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Deno check entry escaped the package workspace.");
        }

        var arguments = new List<string>
        {
            "check",
            "--node-modules-dir=manual",
            "--no-remote",
            "--no-config"
        };
        // A graph containing only local `file:` packages has no remote resolution state and
        // intentionally has no deno.lock. Freeze the lock whenever Deno produced one; requiring
        // a nonexistent lock would reject valid embedded-only package projects.
        if (File.Exists(Path.Combine(workspaceRoot, "deno.lock")))
            arguments.Add("--frozen-lockfile");
        arguments.AddRange(relativeEntries);
        var result = await RunAsync(deno, workspaceRoot, arguments, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Deno package graph validation failed.\n" + FormatFailure("deno check", result));
        }
    }

    private static int ReadDependencyCount(string packagePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(packagePath));
        return ReadDependencies(document.RootElement).Count();
    }

    private static bool RequiresRootLock(string packagePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(packagePath));
        return ReadDependencies(document.RootElement).Any(static dependency =>
            dependency.Value.ValueKind != JsonValueKind.String ||
            !dependency.Value.GetString()!.StartsWith("file:", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LockMatchesPackageJson(string packagePath, string lockPath)
    {
        try
        {
            using var packageDocument = JsonDocument.Parse(File.ReadAllText(packagePath));
            using var lockDocument = JsonDocument.Parse(File.ReadAllText(lockPath));
            if (!lockDocument.RootElement.TryGetProperty("workspace", out var workspace) ||
                !workspace.TryGetProperty("packageJson", out var packageJson) ||
                !packageJson.TryGetProperty("dependencies", out var lockDependencies) ||
                lockDependencies.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var expected = ReadDependencies(packageDocument.RootElement)
                .Where(static property => property.Value.ValueKind == JsonValueKind.String)
                .Select(static property => CreateDependencyIdentity(property.Name, property.Value.GetString()!))
                .Where(static value => value is not null)
                .Select(static value => value!)
                .Distinct()
                .ToArray();
            var actual = lockDependencies.EnumerateArray()
                .Where(static value => value.ValueKind == JsonValueKind.String)
                .Select(static value => value.GetString()!)
                .ToArray();

            // Deno records canonical package specifiers rather than package.json keys. A lock is
            // reusable only when the complete root identity set is represented; local file
            // packages intentionally have no root-lock entry. JSR dependencies are represented
            // by Deno's npm compatibility name (`@jsr/<scope>__<name>`). Compare the complete
            // requested version: a patch upgrade must update the lock rather than freeze the old graph.
            return actual.Length == expected.Length &&
                   expected.All(identity => actual.Any(value => identity.Matches(value)));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            return false;
        }
    }

    private static IEnumerable<JsonProperty> ReadDependencies(JsonElement package)
        => new[] { "dependencies", "devDependencies" }
            .Where(name => package.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object)
            .SelectMany(name => package.GetProperty(name).EnumerateObject());

    private static DependencyIdentity? CreateDependencyIdentity(string name, string value)
    {
        if (value.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            return null;

        if (value.StartsWith("jsr:", StringComparison.OrdinalIgnoreCase))
            return DependencyIdentity.ForJsr(value[4..]);

        var canonical = value.StartsWith("npm:", StringComparison.OrdinalIgnoreCase)
            ? value
            : "npm:" + name + "@" + value;
        return DependencyIdentity.ForExact(canonical);
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    internal static string ResolveExecutable(string? explicitPath)
    {
        var candidate = string.IsNullOrWhiteSpace(explicitPath)
            ? Environment.GetEnvironmentVariable("JAZOR_DENO_PATH")
            : explicitPath;
        if (!string.IsNullOrWhiteSpace(candidate))
        {
            var fullPath = Path.GetFullPath(candidate);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    $"Jazor Deno runtime was not found at '{fullPath}'.",
                    fullPath);
            }

            return fullPath;
        }

        var runtimeName = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
        var rid = OperatingSystem.IsWindows()
            ? "win-x64"
            : OperatingSystem.IsMacOS()
                ? (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64 ? "osx-arm64" : "osx-x64")
                : "linux-x64";
        var bundled = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", runtimeName);
        if (File.Exists(bundled))
            return bundled;

        // DenoHost 固定从 AppContext.BaseDirectory 解析 bundled runtime；这里的裸名称只作为
        // 兼容入口的诊断占位返回，不再交给 PATH 解析。
        return OperatingSystem.IsWindows() ? "deno.exe" : "deno";
    }

    internal static async Task<ProcessResult> RunAsync(
        string executable,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        // DenoHost 拥有进程生命周期：它只解析 AppContext.BaseDirectory 下带签名校验的 bundled
        // runtime。`executable` 保留为兼容入口——显式路径（--deno / JAZOR_DENO_PATH）仍由调用方的
        // ResolveExecutable 做存在性校验，并用于诊断消息，但实际启动固定走 DenoProcess。
        var standardOutput = new StringBuilder();
        var standardError = new StringBuilder();
        using var process = new DenoProcess([.. arguments], workingDirectory);
        // DataReceived 事件在读取线程上触发，追加必须串行化；null 表示流关闭哨兵。
        process.OutputDataReceived += (_, eventArgs) => AppendLine(standardOutput, eventArgs.Data);
        process.ErrorDataReceived += (_, eventArgs) => AppendLine(standardError, eventArgs.Data);

        try
        {
            await process.StartAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"Could not start Deno executable '{executable}'. Install DenoHost 2.9.7 or set JAZOR_DENO_PATH.",
                exception);
        }

        int exitCode;
        try
        {
            exitCode = await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try
            {
                // 取消必须立即终止整棵进程树；零宽超时即强制 kill。
                if (process.IsRunning)
                    await process.StopAsync(TimeSpan.Zero, CancellationToken.None).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                // The process may exit between the cancellation observation and the stop request.
            }

            throw;
        }

        return new ProcessResult(
            exitCode == 0,
            exitCode,
            standardOutput.ToString(),
            standardError.ToString());

        static void AppendLine(StringBuilder builder, string? line)
        {
            if (line is null)
                return;
            lock (builder)
                builder.AppendLine(line);
        }
    }

    private static string FormatFailure(string command, ProcessResult result)
    {
        var output = (result.StandardError + Environment.NewLine + result.StandardOutput).Trim();
        return $"{command} exited with code {result.ExitCode}." +
            (output.Length == 0 ? string.Empty : Environment.NewLine + output);
    }

    internal sealed record ProcessResult(bool Succeeded, int ExitCode, string StandardOutput, string StandardError);

    private sealed record DependencyIdentity(string Exact)
    {
        public static DependencyIdentity ForExact(string value) => new(value);

        public static DependencyIdentity ForJsr(string value)
        {
            var at = FindVersionSeparator(value);
            var packageName = at < 0 ? value : value[..at];
            var version = at < 0 ? string.Empty : value[(at + 1)..];
            var encoded = packageName.TrimStart('@').Replace("/", "__", StringComparison.Ordinal);
            return new("npm:@jsr/" + encoded + "@" + version);
        }

        public bool Matches(string actual)
            => string.Equals(Exact, actual, StringComparison.Ordinal);

        private static int FindVersionSeparator(string value)
        {
            // JSR package names are scoped (`@scope/name`); the first @ after the slash
            // separates the package name from its version/range.
            var slash = value.IndexOf('/', StringComparison.Ordinal);
            return slash < 0 ? value.IndexOf('@', StringComparison.Ordinal) : value.IndexOf('@', slash);
        }

    }
}
