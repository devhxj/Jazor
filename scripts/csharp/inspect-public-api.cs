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
var assemblyPaths = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["Jazor.AspNetCore"] = "net11.0", ["Jazor.AspNetCore.Dev"] = "net11.0", ["Jazor.Admin"] = "net11.0",
    ["ECMAScript"] = "net11.0", ["ECMAScript.ElementPlus"] = "net11.0", ["ECMAScript.Pinia"] = "net11.0",
    ["ECMAScript.Pinia.Testing"] = "net11.0", ["ECMAScript.Style"] = "net11.0", ["ECMAScript.TDesign"] = "net11.0",
    ["ECMAScript.Vue"] = "net11.0", ["ECMAScript.Vue.Devtools"] = "net11.0", ["ECMAScript.VueDataUi"] = "net11.0",
    ["ECMAScript.VueRoute"] = "net11.0", ["ECMAScript.Vuetify"] = "net11.0", ["ECMAScript.VuIcons"] = "net11.0",
    ["Jazor"] = "net11.0", ["Jazor.Vue"] = "net11.0"
};
var assemblies = assemblyPaths.Select(pair => Path.Combine(repoRoot, "src", pair.Key, "bin", configuration, pair.Value, pair.Key + ".dll"));
var preferredPaths = assemblyPaths.ToDictionary(pair => pair.Key, pair => Path.Combine(repoRoot, "src", pair.Key, "bin", configuration, pair.Value, pair.Key + ".dll"), StringComparer.Ordinal);
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    if (name.Name is not null && preferredPaths.TryGetValue(name.Name, out var preferred) && File.Exists(preferred))
        return AssemblyLoadContext.Default.LoadFromAssemblyPath(preferred);
    var local = Directory.GetFiles(Path.Combine(repoRoot, "src"), name.Name + ".dll", SearchOption.AllDirectories)
        .Where(static path => path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        .OrderBy(static path => path.Contains(Path.DirectorySeparatorChar + ".tmp" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        .FirstOrDefault();
    if (local is not null)
        return AssemblyLoadContext.Default.LoadFromAssemblyPath(local);
    var sharedRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared", "Microsoft.AspNetCore.App");
    var candidate = Directory.Exists(sharedRoot)
        ? Directory.GetDirectories(sharedRoot).OrderByDescending(static path => path, StringComparer.Ordinal).Select(path => Path.Combine(path, name.Name + ".dll")).FirstOrDefault(File.Exists)
        : null;
    return candidate is null ? null : AssemblyLoadContext.Default.LoadFromAssemblyPath(candidate);
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
    catch (TypeLoadException exception)
    {
        builder.AppendLine($"> Skipped: exported type could not be resolved ({exception.TypeName ?? exception.Message}).");
        builder.AppendLine();
        continue;
    }
    catch (FileNotFoundException exception)
    {
        builder.AppendLine($"> Skipped: dependency could not be resolved ({exception.FileName}).");
        builder.AppendLine();
        continue;
    }

    foreach (var type in exportedTypes
                 .Where(static type => !type.Name.StartsWith("_", StringComparison.Ordinal))
                 .OrderBy(static type => type.FullName, StringComparer.Ordinal))
    {
        builder.AppendLine($"- {FormatType(type)}");
        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                     .Where(static member => member.MemberType is MemberTypes.Constructor or MemberTypes.Method or MemberTypes.Property or MemberTypes.Field or MemberTypes.Event)
                     .OrderBy(static member => member.MemberType).ThenBy(FormatMember, StringComparer.Ordinal))
        {
            builder.AppendLine($"  - {FormatMember(member)}");
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

static string FormatMember(MemberInfo member)
{
    try { return member.ToString() ?? member.Name; }
    catch (FileNotFoundException) { return member.Name; }
    catch (TypeLoadException) { return member.Name; }
}
