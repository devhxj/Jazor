using System.Reflection;
using System.Runtime.Loader;

#pragma warning disable IL2026
#pragma warning disable IL2075

var repoRoot = Directory.GetCurrentDirectory();
var packageOutRoot = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "package-out");
var vueRouteAssemblyPath = Directory.EnumerateFiles(packageOutRoot, "ECMAScript.VueRoute.dll", SearchOption.AllDirectories)
    .OrderByDescending(File.GetLastWriteTimeUtc)
    .First();
var buildRoot = vueRouteAssemblyPath.Split(
    [$"{Path.DirectorySeparatorChar}ECMAScript.VueRoute{Path.DirectorySeparatorChar}"],
    StringSplitOptions.None)[0];
var nupkgPath = Directory.EnumerateFiles(Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "nupkg"), "Jazor.*.nupkg", SearchOption.AllDirectories)
    .OrderByDescending(File.GetLastWriteTimeUtc)
    .First();
var nupkgExtractRoot = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "nupkg-inspect-csharp");
if (Directory.Exists(nupkgExtractRoot))
{
    Directory.Delete(nupkgExtractRoot, recursive: true);
}

System.IO.Compression.ZipFile.ExtractToDirectory(nupkgPath, nupkgExtractRoot);
var packagedVueRouteAssemblyPath = Path.Combine(nupkgExtractRoot, "lib", "net11.0", "ECMAScript.VueRoute.dll");

AssemblyLoadContext.Default.Resolving += (_, assemblyName) =>
{
    foreach (var path in Directory.EnumerateFiles(buildRoot, $"{assemblyName.Name}.dll", SearchOption.AllDirectories)
        .OrderByDescending(File.GetLastWriteTimeUtc))
    {
        return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    }

    return null;
};

var ecmaPath = Path.Combine(buildRoot, "ECMAScript", "bin", "Debug", "net11.0", "ECMAScript.dll");
var vueRouteAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(vueRouteAssemblyPath);
AssemblyLoadContext.Default.LoadFromAssemblyPath(ecmaPath);

InspectAssembly("Isolated build output", vueRouteAssemblyPath);

var packagedLoadContext = new AssemblyLoadContext("packaged", isCollectible: true);
packagedLoadContext.Resolving += (_, assemblyName) =>
{
    var candidate = Path.Combine(nupkgExtractRoot, "lib", "net11.0", $"{assemblyName.Name}.dll");
    return File.Exists(candidate) ? packagedLoadContext.LoadFromAssemblyPath(candidate) : null;
};
InspectAssembly("Packaged nupkg lib/net11.0", packagedVueRouteAssemblyPath, packagedLoadContext);

static void InspectAssembly(string title, string assemblyPath, AssemblyLoadContext? loadContext = null)
{
    var context = loadContext ?? AssemblyLoadContext.Default;
    var vueRouteAssembly = context.LoadFromAssemblyPath(assemblyPath);
    var routerType = vueRouteAssembly.GetType("ECMAScript.Router", throwOnError: true)!;
var currentRoute = routerType.GetProperty("CurrentRoute", BindingFlags.Public | BindingFlags.Instance)!;
var getter = currentRoute.GetMethod!;

    Console.WriteLine($"[{title}]");
    Console.WriteLine($"VueRouteAssembly={assemblyPath}");
    Console.WriteLine($"Router={routerType.FullName}");
    PrintAttributes("Router attributes", routerType.GetCustomAttributesData());
    PrintAttributes("CurrentRoute attributes", currentRoute.GetCustomAttributesData());
    PrintAttributes("CurrentRoute.get attributes", getter.GetCustomAttributesData());
}

static void PrintAttributes(string title, IEnumerable<CustomAttributeData> attributes)
{
    Console.WriteLine(title);
    foreach (var attribute in attributes)
    {
        var args = string.Join(", ", attribute.ConstructorArguments.Select(static arg => arg.Value?.ToString() ?? "<null>"));
        Console.WriteLine($"  {attribute.AttributeType.FullName}({args})");
    }
}
