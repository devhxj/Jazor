using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
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
            throw new InvalidOperationException("Jazor package restore requires package.json in the staging root.");

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
            // behind. The staging transaction makes removing it safe before resolution.
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
        var checkImportMapPath = WriteCheckImportMap(workspaceRoot, entryPaths, libraries);
        if (checkImportMapPath is not null)
        {
            arguments.Add("--import-map");
            arguments.Add(checkImportMapPath);
        }
        arguments.AddRange(relativeEntries);

        try
        {
            var result = await RunAsync(deno, workspaceRoot, arguments, cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Deno package graph validation failed.\n" + FormatFailure("deno check", result));
            }
        }
        finally
        {
            if (checkImportMapPath is not null && File.Exists(checkImportMapPath))
                File.Delete(checkImportMapPath);
        }
    }

    private static string? WriteCheckImportMap(
        string workspaceRoot,
        IReadOnlyList<string> entryPaths,
        LibraryAssets? libraries)
    {
        var imports = new JsonObject();
        if (libraries is not null)
        {
            foreach (var (specifier, target) in libraries.ImportPaths.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(specifier) || string.IsNullOrWhiteSpace(target))
                    continue;

                // Let Deno resolve an upstream bare package through its restored package.json
                // and conditional exports. Only logical binding aliases need an import-map
                // target; mapping an already-authored package specifier to a guessed file would
                // bypass the package's own resolution contract.
                if (IsExternalPackage(libraries, specifier) &&
                    string.Equals(specifier, target, StringComparison.Ordinal))
                    continue;

                AddImport(imports, specifier, ResolveCheckTarget(libraries, specifier, target));
            }
        }

        foreach (var entryPath in entryPaths)
        {
            var relative = Path.GetRelativePath(workspaceRoot, entryPath).Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(relative) && !relative.StartsWith("../", StringComparison.Ordinal))
                AddImport(imports, relative, "./" + relative);
        }

        if (imports.Count == 0)
            return null;

        var path = Path.Combine(workspaceRoot, ".jazor-deno-check-importmap.json");
        var payload = new JsonObject { ["imports"] = imports };
        File.WriteAllText(path, payload.ToJsonString() + "\n");
        return path;
    }

    private static void AddImport(JsonObject imports, string specifier, string target)
    {
        if (imports[specifier] is { } existing)
        {
            if (!string.Equals(existing.GetValue<string>(), target, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"Deno check import '{specifier}' resolves to conflicting targets.");
            return;
        }

        imports[specifier] = target;
    }

    private static string ResolveCheckTarget(
        LibraryAssets libraries,
        string specifier,
        string fallbackTarget)
    {
        // The materializer target for external packages is often an upstream file hint such as
        // `tdesign-vue-next/es/button/index.mjs` or `dist/index.js`. Package identity must come
        // from the authored specifier, otherwise `dist/index.js` is misread as package `dist`.
        var packageName = GetPackageName(specifier);
        if (libraries.PackageReferences.TryGetValue(specifier, out var exactReference) &&
            exactReference.Source is "npm" or "jsr")
        {
            // Import-map targets are URLs, never bare package specifiers. The authored logical
            // name is still used as the map key; point its fallback at the restored package
            // tree so Deno can validate it offline even when the package exports map is absent.
            // Conditional exports remain available for ordinary authored package imports.
            return "./node_modules/" + packageName + GetPackageTargetSuffix(packageName, fallbackTarget);
        }

        if (libraries.PackageReferences.TryGetValue(packageName, out var packageReference) &&
            packageReference.Source is "npm" or "jsr")
        {
            return "./node_modules/" + packageName + GetPackageTargetSuffix(packageName, fallbackTarget);
        }

        if (libraries.PackageProjections.TryGetValue(packageName, out var projection))
        {
            var exportName = string.Equals(specifier, packageName, StringComparison.Ordinal)
                ? "."
                : "./" + specifier[(packageName.Length + 1)..];
            if (projection.Exports.TryGetValue(exportName, out var exportTarget))
            {
                return "./node_modules/" + packageName + "/" + exportTarget.TrimStart('.', '/');
            }
        }

        return "./" + fallbackTarget.Replace('\\', '/').TrimStart('/');
    }

    private static string GetPackageTargetSuffix(string packageName, string target)
    {
        var normalized = target.Replace('\\', '/').TrimStart('.', '/');
        if (string.Equals(normalized, packageName, StringComparison.Ordinal))
            return string.Empty;
        if (normalized.StartsWith(packageName + "/", StringComparison.Ordinal))
            normalized = normalized[(packageName.Length + 1)..];
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : "/" + normalized;
    }

    private static string GetPackageName(string specifier)
    {
        var segments = specifier.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var count = specifier.StartsWith('@', StringComparison.Ordinal) ? 2 : 1;
        return segments.Length >= count ? string.Join('/', segments.Take(count)) : specifier;
    }

    private static bool IsExternalPackage(LibraryAssets libraries, string specifier)
    {
        if (libraries.PackageReferences.TryGetValue(specifier, out var exact))
            return exact.Source is "npm" or "jsr";

        var packageName = GetPackageName(specifier);
        return libraries.PackageReferences.TryGetValue(packageName, out var packageReference) &&
               packageReference.Source is "npm" or "jsr";
    }

    private static int ReadDependencyCount(string packagePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(packagePath));
        if (!document.RootElement.TryGetProperty("dependencies", out var dependencies) ||
            dependencies.ValueKind != JsonValueKind.Object)
        {
            return 0;
        }

        return dependencies.EnumerateObject().Count();
    }

    private static bool RequiresRootLock(string packagePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(packagePath));
        if (!document.RootElement.TryGetProperty("dependencies", out var dependencies) ||
            dependencies.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return dependencies.EnumerateObject().Any(static dependency =>
            dependency.Value.ValueKind != JsonValueKind.String ||
            !dependency.Value.GetString()!.StartsWith("file:", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LockMatchesPackageJson(string packagePath, string lockPath)
    {
        try
        {
            using var packageDocument = JsonDocument.Parse(File.ReadAllText(packagePath));
            using var lockDocument = JsonDocument.Parse(File.ReadAllText(lockPath));
            if (!packageDocument.RootElement.TryGetProperty("dependencies", out var dependencies) ||
                dependencies.ValueKind != JsonValueKind.Object ||
                !lockDocument.RootElement.TryGetProperty("workspace", out var workspace) ||
                !workspace.TryGetProperty("packageJson", out var packageJson) ||
                !packageJson.TryGetProperty("dependencies", out var lockDependencies) ||
                lockDependencies.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var expected = dependencies.EnumerateObject()
                .Where(static property => property.Value.ValueKind == JsonValueKind.String)
                .Select(static property => CreateDependencyIdentity(property.Name, property.Value.GetString()!))
                .Where(static value => value is not null)
                .Select(static value => value!)
                .ToArray();
            var actual = lockDependencies.EnumerateArray()
                .Where(static value => value.ValueKind == JsonValueKind.String)
                .Select(static value => value.GetString()!)
                .ToArray();

            // Deno records canonical package specifiers rather than package.json keys. A lock is
            // reusable only when the complete root identity set is represented; local file
            // packages intentionally have no root-lock entry. JSR dependencies are represented
            // by Deno's npm compatibility name (`@jsr/<scope>__<name>`), so their source range is
            // checked through the canonical package prefix and requested major version.
            return actual.Length == expected.Length &&
                   expected.All(identity => actual.Any(value => identity.Matches(value)));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            return false;
        }
    }

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

    private static string ResolveExecutable(string? explicitPath)
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

        // Let Process resolve the executable through PATH. This keeps source builds usable when
        // Deno is installed globally while packaged builds pass the NuGet-carried runtime path.
        return OperatingSystem.IsWindows() ? "deno.exe" : "deno";
    }

    private static async Task<ProcessResult> RunAsync(
        string executable,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        try
        {
            if (!process.Start())
                throw new InvalidOperationException($"Could not start Deno executable '{executable}'.");
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Could not start Deno executable '{executable}'. Install DenoHost 2.9.7 or set JAZOR_DENO_PATH.",
                exception);
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch
            {
            }

            throw;
        }

        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);
        return new ProcessResult(process.ExitCode == 0, process.ExitCode, stdout, stderr);
    }

    private static string FormatFailure(string command, ProcessResult result)
    {
        var output = (result.StandardError + Environment.NewLine + result.StandardOutput).Trim();
        return $"{command} exited with code {result.ExitCode}." +
            (output.Length == 0 ? string.Empty : Environment.NewLine + output);
    }

    private sealed record ProcessResult(bool Succeeded, int ExitCode, string StandardOutput, string StandardError);

    private sealed record DependencyIdentity(
        string? Exact,
        string? JsrPrefix,
        int? JsrMajor)
    {
        public static DependencyIdentity ForExact(string value) => new(value, null, null);

        public static DependencyIdentity ForJsr(string value)
        {
            var at = FindVersionSeparator(value);
            var packageName = at < 0 ? value : value[..at];
            var version = at < 0 ? string.Empty : value[(at + 1)..];
            var encoded = packageName.TrimStart('@').Replace("/", "__", StringComparison.Ordinal);
            var major = TryReadMajor(version);
            return new(null, "npm:@jsr/" + encoded + "@", major);
        }

        public bool Matches(string actual)
        {
            if (Exact is not null)
                return string.Equals(Exact, actual, StringComparison.Ordinal);
            if (JsrPrefix is null || !actual.StartsWith(JsrPrefix, StringComparison.Ordinal))
                return false;
            return JsrMajor is null || TryReadMajor(actual[JsrPrefix.Length..]) == JsrMajor;
        }

        private static int FindVersionSeparator(string value)
        {
            // JSR package names are scoped (`@scope/name`); the first @ after the slash
            // separates the package name from its version/range.
            var slash = value.IndexOf('/', StringComparison.Ordinal);
            return slash < 0 ? value.IndexOf('@', StringComparison.Ordinal) : value.IndexOf('@', slash);
        }

        private static int? TryReadMajor(string value)
        {
            var digits = value.TrimStart('^', '~', '>', '=', '<', ' ')
                .Split(new[] { '.', '-', '+', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
            return int.TryParse(digits, out var major) ? major : null;
        }
    }
}
