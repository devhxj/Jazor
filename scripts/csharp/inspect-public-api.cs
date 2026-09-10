#!/usr/bin/env dotnet run
#:property NoWarn=IL2026;IL2075

using System.Reflection;
using System.Runtime.Loader;
using System.Text;

var repoRoot = Directory.GetCurrentDirectory();
while (!File.Exists(Path.Combine(repoRoot, "Jazor.slnx")))
{
    repoRoot = Directory.GetParent(repoRoot)?.FullName
        ?? throw new InvalidOperationException("Repository root was not found.");
}

var configuration = GetOption("--configuration") ?? "Debug";
var outputPath = GetOption("--output");
AssemblyLoadContext.Default.Resolving += static (_, name) =>
{
    var sharedRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared", "Microsoft.AspNetCore.App");
    var candidate = Directory.Exists(sharedRoot)
        ? Directory.GetDirectories(sharedRoot).OrderByDescending(static path => path, StringComparer.Ordinal).Select(path => Path.Combine(path, name.Name + ".dll")).FirstOrDefault(File.Exists)
        : null;
    return candidate is null ? null : AssemblyLoadContext.Default.LoadFromAssemblyPath(candidate);
};
var assemblies = new[]
{
    Path.Combine(repoRoot, "src", "Jazor.AspNetCore", "bin", configuration, "net11.0", "Jazor.AspNetCore.dll"),
    Path.Combine(repoRoot, "src", "Jazor.AspNetCore.Dev", "bin", configuration, "net11.0", "Jazor.AspNetCore.Dev.dll"),
    Path.Combine(repoRoot, "src", "Jazor.Admin", "bin", configuration, "net11.0", "Jazor.Admin.dll")
};

var builder = new StringBuilder();
builder.AppendLine($"# Public API snapshot ({DateTimeOffset.UtcNow:yyyy-MM-dd})");
builder.AppendLine();
foreach (var assemblyPath in assemblies.OrderBy(static path => path, StringComparer.Ordinal))
{
    if (!File.Exists(assemblyPath))
        throw new FileNotFoundException($"Build output was not found. Build the solution first: {assemblyPath}");

    var assembly = Assembly.LoadFrom(assemblyPath);
    builder.AppendLine($"## {assembly.GetName().Name}");
    Type[] exportedTypes;
    try
    {
        exportedTypes = assembly.GetExportedTypes();
    }
    catch (FileNotFoundException exception)
    {
        builder.AppendLine($"> Skipped: dependency could not be resolved ({exception.FileName}).");
        builder.AppendLine();
        continue;
    }

    foreach (var type in exportedTypes.OrderBy(static type => type.FullName, StringComparer.Ordinal))
    {
        builder.AppendLine($"- {FormatType(type)}");
        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                     .Where(static member => member.MemberType is MemberTypes.Constructor or MemberTypes.Method or MemberTypes.Property or MemberTypes.Field or MemberTypes.Event)
                     .OrderBy(static member => member.MemberType).ThenBy(static member => member.ToString(), StringComparer.Ordinal))
        {
            builder.AppendLine($"  - {member}");
        }
    }

    builder.AppendLine();
}

if (outputPath is null)
{
    Console.Write(builder.ToString());
}
else
{
    var resolved = Path.IsPathRooted(outputPath) ? outputPath : Path.Combine(repoRoot, outputPath);
    Directory.CreateDirectory(Path.GetDirectoryName(resolved) ?? repoRoot);
    File.WriteAllText(resolved, builder.ToString(), Encoding.UTF8);
    Console.WriteLine($"Wrote public API snapshot: {resolved}");
}

string? GetOption(string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

static string FormatType(Type type)
    => type.IsEnum ? $"enum {type.FullName}" : $"type {type.FullName}";
