#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.IO.Enumeration;

if (args.Any(argument => argument != "--apply"))
    throw new ArgumentException("Usage: dotnet run --file scripts/csharp/clean-workspace.cs -- [--apply]");

var apply = args.Contains("--apply", StringComparer.Ordinal);
var root = new DirectoryInfo(Directory.GetCurrentDirectory());
while (!File.Exists(Path.Combine(root.FullName, "Jazor.slnx")))
    root = root.Parent ?? throw new DirectoryNotFoundException("Run this script inside the Jazor repository.");

var protectedPaths = (await Git(["ls-files", "--cached", "--others", "--exclude-standard", "-z"]))
    .Split('\0', StringSplitOptions.RemoveEmptyEntries);
var candidates = new List<string>();
foreach (var name in new[] { ".tmp", ".artifacts", "artifacts", "output", "test", "TestResults", ".playwright-cli", "jazor" })
    AddCandidate(Path.Combine(root.FullName, name));

foreach (var name in new[] { "src", "samples" })
    Discover(new DirectoryInfo(Path.Combine(root.FullName, name)));

if (candidates.Count == 0)
{
    Console.WriteLine("No generated directories to clean.");
    return;
}

var ignoredPaths = (await Git(["check-ignore", "--stdin", "-z"],
        string.Join('\0', candidates.Select(Relative)) + "\0"))
    .Split('\0', StringSplitOptions.RemoveEmptyEntries)
    .ToHashSet(StringComparer.OrdinalIgnoreCase);
var plan = new List<(string Path, long Bytes, long Files)>();
foreach (var path in candidates.Where(path => ignoredPaths.Contains(Relative(path))))
{
    var files = new FileSystemEnumerable<long>(path, (ref FileSystemEntry entry) => entry.Length,
        new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.ReparsePoint })
    {
        ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory
    };
    long bytes = 0, count = 0;
    foreach (var size in files)
    {
        bytes += size;
        count++;
    }
    plan.Add((path, bytes, count));
    if (bytes >= 1024L * 1024 * 1024)
        Console.WriteLine($"{Relative(path),-65} {GiB(bytes),9:F2} GiB  {count,10:N0} files");
}

Console.WriteLine($"Total: {plan.Count} generated directories, {GiB(plan.Sum(item => item.Bytes)):F2} GiB, {plan.Sum(item => item.Files):N0} files (logical sizes; hard links may share disk space).");
if (!apply)
{
    Console.WriteLine("Preview only. Add --apply to delete these generated files. NuGet caches, .git, worktrees and IDE settings are retained.");
    return;
}

var errors = new List<string>();
long deletedFiles = 0;
foreach (var item in plan.OrderByDescending(item => item.Bytes))
{
    Console.WriteLine($"Deleting {Relative(item.Path)} ({GiB(item.Bytes):F2} GiB)...");
    DeleteDirectory(new DirectoryInfo(item.Path));
}
Console.WriteLine($"Deleted {deletedFiles:N0} files. Errors: {errors.Count}.");
foreach (var error in errors.Take(20))
    Console.Error.WriteLine(error);
Environment.ExitCode = errors.Count == 0 ? 0 : 1;

string Relative(string path) => Path.GetRelativePath(root.FullName, path).Replace('\\', '/');
static double GiB(long bytes) => bytes / (1024.0 * 1024 * 1024);

bool AddCandidate(string path)
{
    if (!Directory.Exists(path) || (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        return false;
    var fullPath = Path.GetFullPath(path);
    if (!fullPath.StartsWith(root.FullName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"Cleanup target is outside the repository: {fullPath}");
    var relative = Relative(fullPath);
    // Git tracks source and baselines even when an ancestor is ignored. Never delete them.
    if (protectedPaths.Any(path => path.Equals(relative, StringComparison.OrdinalIgnoreCase)
        || path.StartsWith(relative + "/", StringComparison.OrdinalIgnoreCase)))
        return false;
    candidates.Add(fullPath);
    return true;
}

void Discover(DirectoryInfo directory)
{
    if (!directory.Exists || directory.Name == ".git" || (directory.Attributes & FileAttributes.ReparsePoint) != 0)
        return;
    // Nested repositories own their source and outputs; this cleaner only owns this checkout.
    if (File.Exists(Path.Combine(directory.FullName, ".git")) || Directory.Exists(Path.Combine(directory.FullName, ".git")))
        return;
    var generated = directory.Name.ToLowerInvariant() is "bin" or "obj" or "node_modules" or "dist"
        or "dist-prod-optimized" or "jazor" or ".deno-build" or "testresults" or ".tmp" or ".artifacts"
        || directory.Name.StartsWith(".todolist-hmr-", StringComparison.OrdinalIgnoreCase)
        || directory.Name.StartsWith(".wiki-browser-chrome-profile-", StringComparison.OrdinalIgnoreCase);
    if (generated && AddCandidate(directory.FullName))
        return;
    foreach (var child in directory.EnumerateDirectories())
        Discover(child);
}

void DeleteDirectory(DirectoryInfo directory)
{
    try
    {
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            if (entry is DirectoryInfo child && (entry.Attributes & FileAttributes.ReparsePoint) == 0)
            {
                DeleteDirectory(child);
                continue;
            }
            try
            {
                // Delete links themselves; never traverse their targets. NuGet copies can be read-only.
                if ((entry.Attributes & (FileAttributes.ReadOnly | FileAttributes.ReparsePoint)) == FileAttributes.ReadOnly)
                    entry.Attributes &= ~FileAttributes.ReadOnly;
                entry.Delete();
                if (entry is FileInfo && ++deletedFiles % 50000 == 0)
                    Console.WriteLine($"  Deleted {deletedFiles:N0} files...");
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                errors.Add($"{Relative(entry.FullName)}: {exception.Message}");
            }
        }
        directory.Delete();
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
        errors.Add($"{Relative(directory.FullName)}: {exception.Message}");
    }
}

async Task<string> Git(string[] arguments, string? input = null)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = root.FullName,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = input is not null
        }
    };
    foreach (var argument in arguments)
        process.StartInfo.ArgumentList.Add(argument);
    process.Start();
    var output = process.StandardOutput.ReadToEndAsync();
    var error = process.StandardError.ReadToEndAsync();
    if (input is not null)
    {
        await process.StandardInput.WriteAsync(input);
        process.StandardInput.Close();
    }
    await process.WaitForExitAsync();
    if (process.ExitCode != 0 && !(arguments[0] == "check-ignore" && process.ExitCode == 1))
        throw new InvalidOperationException(await error);
    return await output;
}
