#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Xml.Linq;

#pragma warning disable IL2026, IL2075 // The local gate intentionally reflects untrimmed test outputs.

const double minimumBindingSurfaceCoverage = 0.90;

var testLanes = new[]
{
    new VueBindingTestLane(
        "compiler-vue-bindings",
        "src/Jazor.CompilerTest/Jazor.CompilerTest.csproj",
        "vue-bindings-compiler.trx",
        MinimumPassedTests: 10_000,
        [
            new VueBindingTarget("vue3", "ECMAScript.Vue", "net11.0"),
            new VueBindingTarget("vuetify", "ECMAScript.Vuetify", "net11.0"),
            new VueBindingTarget("element-plus", "ECMAScript.ElementPlus", "net11.0"),
            new VueBindingTarget("tdesign", "ECMAScript.TDesign", "net11.0")
        ]),
    new VueBindingTestLane(
        "devtools",
        "src/ECMAScript.Vue.Devtools.Test/ECMAScript.Vue.Devtools.Test.csproj",
        "vue-bindings-devtools.trx",
        MinimumPassedTests: 10,
        [new VueBindingTarget("devtools", "ECMAScript.Vue.Devtools", "net11.0")]),
    new VueBindingTestLane(
        "vue-data-ui",
        "src/ECMAScript.VueDataUi.Test/ECMAScript.VueDataUi.Test.csproj",
        "vue-bindings-dataui.trx",
        MinimumPassedTests: 6,
        [new VueBindingTarget("vue-data-ui", "ECMAScript.VueDataUi", "net11.0")]),
    new VueBindingTestLane(
        "vu-icons",
        "src/ECMAScript.VuIcons.Test/ECMAScript.VuIcons.Test.csproj",
        "vue-bindings-vu-icons.trx",
        MinimumPassedTests: 4,
        [new VueBindingTarget("vu-icons", "ECMAScript.VuIcons", "net11.0")]),
    new VueBindingTestLane(
        "pinia",
        "src/ECMAScript.Pinia.Test/ECMAScript.Pinia.Test.csproj",
        "vue-bindings-pinia.trx",
        MinimumPassedTests: 68,
        [new VueBindingTarget("pinia", "ECMAScript.Pinia", "net11.0")]),
    new VueBindingTestLane(
        "pinia-testing",
        "src/ECMAScript.Pinia.Testing.Test/ECMAScript.Pinia.Testing.Test.csproj",
        "vue-bindings-pinia-testing.trx",
        MinimumPassedTests: 39,
        [new VueBindingTarget("pinia-testing", "ECMAScript.Pinia.Testing", "net11.0")]),
    new VueBindingTestLane(
        "vue-route",
        "src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj",
        "vue-bindings-vue-route.trx",
        MinimumPassedTests: 102,
        [new VueBindingTarget("vue-route", "ECMAScript.VueRoute", "net11.0")])
};

try
{
    var options = CoverageGateOptions.Parse(args);
    var repoRoot = RequireRepoRoot();
    var resultBase = Path.GetFullPath(
        options.ResultsDirectory ?? Path.Combine(repoRoot, ".tmp", "vue-binding-coverage-gate"));
    var resultRoot = Path.Combine(resultBase, Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(resultRoot);

    var failures = new List<string>();
    foreach (var lane in testLanes)
    {
        var laneResultDirectory = Path.Combine(resultRoot, lane.Id);
        Directory.CreateDirectory(laneResultDirectory);
        await RunTestLaneAsync(lane, repoRoot, laneResultDirectory, options);

        var trxPath = RequireSingleFile(laneResultDirectory, lane.TrxFileName);
        var tests = ReadTestCounters(trxPath);
        if (tests.Passed < lane.MinimumPassedTests)
        {
            failures.Add(
                $"{lane.Id}: passed tests {tests.Passed} are below {lane.MinimumPassedTests}");
        }
        if (tests.Total != tests.Passed || tests.Failed != 0)
        {
            failures.Add(
                $"{lane.Id}: test counters are not clean: total={tests.Total}, passed={tests.Passed}, failed={tests.Failed}");
        }

        Console.WriteLine($"{lane.Id} tests: {tests.Passed}/{tests.Total} passed (minimum {lane.MinimumPassedTests})");
        foreach (var target in lane.Targets)
        {
            var assemblyPath = Path.Combine(
                repoRoot,
                "src",
                target.AssemblyName,
                "bin",
                options.Configuration,
                target.TargetFramework,
                target.AssemblyName + ".dll");
            var coverage = AuditBindingSurface(target, assemblyPath);
            var requiredUnits = RequiredHits(coverage.TotalUnits, minimumBindingSurfaceCoverage);
            Console.WriteLine(
                $"{target.Id}: {coverage.AuditedUnits}/{coverage.TotalUnits} binding contract units = " +
                $"{FormatRate(coverage.AuditedUnits, coverage.TotalUnits)} (minimum {minimumBindingSurfaceCoverage:P0})");

            if (coverage.TotalUnits == 0)
            {
                failures.Add($"{target.Id}: no public binding contract units were discovered");
            }
            else if (coverage.AuditedUnits < requiredUnits)
            {
                failures.Add(
                    $"{target.Id}: binding contract coverage {FormatRate(coverage.AuditedUnits, coverage.TotalUnits)} " +
                    $"is below {minimumBindingSurfaceCoverage:P0}");
            }
        }
    }

    Console.WriteLine("Reports: " + resultRoot);
    if (failures.Count > 0)
    {
        throw new InvalidOperationException(
            "Vue binding coverage gate failed: " + string.Join("; ", failures));
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}

static async Task RunTestLaneAsync(
    VueBindingTestLane lane,
    string repoRoot,
    string resultsDirectory,
    CoverageGateOptions options)
{
    var arguments = new List<string>
    {
        "test",
        Path.Combine(repoRoot, lane.TestProjectPath.Replace('/', Path.DirectorySeparatorChar)),
        "--configuration",
        options.Configuration,
        "--logger",
        "trx;LogFileName=" + lane.TrxFileName,
        "--results-directory",
        resultsDirectory,
        "--verbosity",
        "minimal"
    };
    if (options.NoBuild)
        arguments.Add("--no-build");
    if (options.NoRestore)
        arguments.Add("--no-restore");

    await RunDotNetAsync(arguments, repoRoot);
}

static BindingSurfaceCoverage AuditBindingSurface(VueBindingTarget target, string assemblyPath)
{
    if (!File.Exists(assemblyPath))
    {
        throw new FileNotFoundException(
            $"The '{target.Id}' target assembly was not produced by its test lane.",
            assemblyPath);
    }

    var loadContext = new BindingAssemblyLoadContext(assemblyPath);
    try
    {
        var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
        var totalUnits = 0;
        var auditedUnits = 0;
        foreach (var unit in EnumeratePublicContractUnits(assembly))
        {
            totalUnits++;
            ValidateContractUnit(target, unit);
            auditedUnits++;
        }

        return new BindingSurfaceCoverage(auditedUnits, totalUnits);
    }
    finally
    {
        loadContext.Unload();
    }
}

static IEnumerable<BindingContractUnit> EnumeratePublicContractUnits(Assembly assembly)
{
    var types = assembly
        .GetExportedTypes()
        .Where(static type => !IsCompilerGenerated(type))
        .OrderBy(static type => type.FullName, StringComparer.Ordinal);

    foreach (var type in types)
    {
        yield return new BindingContractUnit(type.FullName ?? type.Name, type);

        const BindingFlags declaredPublic = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .Where(static constructor => !IsCompilerGenerated(constructor))
                     .OrderBy(static constructor => constructor.MetadataToken))
        {
            yield return new BindingContractUnit(BuildMemberDisplayName(type, constructor), constructor);
        }

        foreach (var method in type.GetMethods(declaredPublic)
                     .Where(static method => !method.IsSpecialName && !IsCompilerGenerated(method))
                     .OrderBy(static method => method.MetadataToken))
        {
            yield return new BindingContractUnit(BuildMemberDisplayName(type, method), method);
        }

        foreach (var property in type.GetProperties(declaredPublic)
                     .Where(static property => !IsCompilerGenerated(property))
                     .OrderBy(static property => property.MetadataToken))
        {
            yield return new BindingContractUnit(BuildMemberDisplayName(type, property), property);
        }

        foreach (var field in type.GetFields(declaredPublic)
                     .Where(static field => !IsCompilerGenerated(field))
                     .OrderBy(static field => field.MetadataToken))
        {
            yield return new BindingContractUnit(BuildMemberDisplayName(type, field), field);
        }

        foreach (var @event in type.GetEvents(declaredPublic)
                     .Where(static @event => !IsCompilerGenerated(@event))
                     .OrderBy(static @event => @event.MetadataToken))
        {
            yield return new BindingContractUnit(BuildMemberDisplayName(type, @event), @event);
        }
    }
}

static bool IsCompilerGenerated(MemberInfo member)
    => member.Name.StartsWith('<')
        || member.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false);

static string BuildMemberDisplayName(Type declaringType, MemberInfo member)
    => (declaringType.FullName ?? declaringType.Name) + "." + member.Name;

static void ValidateContractUnit(VueBindingTarget target, BindingContractUnit unit)
{
    if (string.IsNullOrWhiteSpace(unit.DisplayName))
        throw new InvalidOperationException($"{target.Id}: a public contract unit has no display name.");

    ValidateCustomAttributes(target, unit.DisplayName, unit.Member.GetCustomAttributesData());
    switch (unit.Member)
    {
        case Type type:
            if (!type.IsVisible || string.IsNullOrWhiteSpace(type.Namespace))
            {
                throw new InvalidOperationException(
                    $"{target.Id}: public contract type '{unit.DisplayName}' is not externally addressable.");
            }

            foreach (var genericParameter in type.GetGenericArguments().Where(static parameter => parameter.IsGenericParameter))
            {
                if (string.IsNullOrWhiteSpace(genericParameter.Name))
                {
                    throw new InvalidOperationException(
                        $"{target.Id}: type '{unit.DisplayName}' exposes an unnamed generic parameter.");
                }
            }
            break;
        case MethodBase method:
            ValidateMethodSignature(target, unit.DisplayName, method);
            break;
        case PropertyInfo property:
            ValidateTypeSignature(target, unit.DisplayName, property.PropertyType);
            foreach (var parameter in property.GetIndexParameters())
                ValidateParameter(target, unit.DisplayName, parameter);
            break;
        case FieldInfo field:
            ValidateTypeSignature(target, unit.DisplayName, field.FieldType);
            break;
        case EventInfo @event:
            ValidateTypeSignature(target, unit.DisplayName, @event.EventHandlerType);
            break;
        default:
            throw new InvalidOperationException(
                $"{target.Id}: unsupported public contract member kind '{unit.Member.MemberType}' for '{unit.DisplayName}'.");
    }
}

static void ValidateMethodSignature(VueBindingTarget target, string displayName, MethodBase method)
{
    if (method is MethodInfo methodInfo)
        ValidateTypeSignature(target, displayName, methodInfo.ReturnType);

    foreach (var genericParameter in method.GetGenericArguments().Where(static parameter => parameter.IsGenericParameter))
    {
        if (string.IsNullOrWhiteSpace(genericParameter.Name))
        {
            throw new InvalidOperationException(
                $"{target.Id}: method '{displayName}' exposes an unnamed generic parameter.");
        }
    }

    foreach (var parameter in method.GetParameters())
        ValidateParameter(target, displayName, parameter);
}

static void ValidateParameter(VueBindingTarget target, string displayName, ParameterInfo parameter)
{
    if (string.IsNullOrWhiteSpace(parameter.Name))
    {
        throw new InvalidOperationException(
            $"{target.Id}: '{displayName}' exposes a parameter without a stable metadata name.");
    }

    ValidateTypeSignature(target, displayName + "(" + parameter.Name + ")", parameter.ParameterType);
    ValidateCustomAttributes(target, displayName + "(" + parameter.Name + ")", parameter.GetCustomAttributesData());
}

static void ValidateTypeSignature(VueBindingTarget target, string displayName, Type? type)
{
    if (type is null)
    {
        throw new InvalidOperationException(
            $"{target.Id}: '{displayName}' exposes an unresolved runtime type.");
    }

    if (type.IsGenericParameter)
    {
        if (string.IsNullOrWhiteSpace(type.Name))
        {
            throw new InvalidOperationException(
                $"{target.Id}: '{displayName}' exposes an unnamed generic parameter.");
        }
        return;
    }

    if (type.HasElementType)
    {
        ValidateTypeSignature(target, displayName, type.GetElementType());
        return;
    }

    if (type.IsGenericTypeDefinition)
    {
        if (string.IsNullOrWhiteSpace(type.FullName))
        {
            throw new InvalidOperationException(
                $"{target.Id}: '{displayName}' exposes an unaddressable generic type definition '{type}'.");
        }
        return;
    }

    if (type.IsGenericType)
    {
        foreach (var genericArgument in type.GetGenericArguments())
            ValidateTypeSignature(target, displayName, genericArgument);
        return;
    }

    if (string.IsNullOrWhiteSpace(type.FullName))
    {
        throw new InvalidOperationException(
            $"{target.Id}: '{displayName}' exposes an unaddressable runtime type '{type}'.");
    }
}

static void ValidateCustomAttributes(
    VueBindingTarget target,
    string displayName,
    IList<CustomAttributeData> attributes)
{
    foreach (var attribute in attributes)
    {
        var attributeName = attribute.AttributeType.FullName;
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new InvalidOperationException(
                $"{target.Id}: '{displayName}' has an unaddressable custom attribute.");
        }

        if (attributeName.EndsWith("ECMAScriptInlineAttribute", StringComparison.Ordinal)
            && !HasNonEmptyStringArgument(attribute))
        {
            throw new InvalidOperationException(
                $"{target.Id}: '{displayName}' has an empty ECMAScript inline template.");
        }

        if (attributeName.EndsWith("ECMAScriptImportAttribute", StringComparison.Ordinal)
            && !HasNonEmptyStringArgument(attribute))
        {
            throw new InvalidOperationException(
                $"{target.Id}: '{displayName}' has an empty ECMAScript import path.");
        }
    }
}

static bool HasNonEmptyStringArgument(CustomAttributeData attribute)
    => attribute.ConstructorArguments.Any(static argument =>
        argument.ArgumentType == typeof(string)
        && argument.Value is string value
        && !string.IsNullOrWhiteSpace(value))
        || attribute.NamedArguments.Any(static argument =>
            argument.TypedValue.ArgumentType == typeof(string)
            && argument.TypedValue.Value is string value
            && !string.IsNullOrWhiteSpace(value));

static TestCounters ReadTestCounters(string path)
{
    var counters = XDocument.Load(path)
        .Descendants()
        .SingleOrDefault(static element => element.Name.LocalName == "Counters")
        ?? throw new InvalidOperationException($"TRX report has no Counters element: {path}");
    return new TestCounters(
        ReadIntAttribute(counters, "total"),
        ReadIntAttribute(counters, "passed"),
        ReadIntAttribute(counters, "failed"));
}

static int ReadIntAttribute(XElement element, string name)
{
    var value = element.Attribute(name)?.Value
        ?? throw new InvalidOperationException($"Element '{element.Name.LocalName}' is missing attribute '{name}'.");
    return int.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
}

static int RequiredHits(int total, double minimumRate)
    => (int)Math.Ceiling(total * minimumRate);

static string FormatRate(int covered, int total)
    => total == 0
        ? "0.00%"
        : ((double)covered / total).ToString("P2", CultureInfo.InvariantCulture);

static string RequireSingleFile(string root, string fileName)
{
    var matches = Directory.GetFiles(root, fileName, SearchOption.AllDirectories);
    return matches.Length switch
    {
        1 => matches[0],
        0 => throw new InvalidOperationException($"'{fileName}' was not produced under '{root}'."),
        _ => throw new InvalidOperationException($"Expected one '{fileName}' under '{root}', found {matches.Length}.")
    };
}

static async Task RunDotNetAsync(IReadOnlyList<string> arguments, string workingDirectory)
{
    var startInfo = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = workingDirectory,
        UseShellExecute = false
    };
    startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(workingDirectory, ".dotnet");
    startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
    startInfo.Environment["UseSharedCompilation"] = "false";
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start dotnet.");
    await process.WaitForExitAsync();
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"dotnet {string.Join(' ', arguments)} failed with exit code {process.ExitCode}.");
}

static string RequireRepoRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    }

    throw new InvalidOperationException("Repository root containing Jazor.slnx was not found.");
}

internal sealed class BindingAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly IReadOnlyList<string> _runtimeAssemblyDirectories;

    public BindingAssemblyLoadContext(string assemblyPath)
        : base($"jazor-binding-audit-{Path.GetFileNameWithoutExtension(assemblyPath)}-{Guid.NewGuid():N}", isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(assemblyPath);
        _runtimeAssemblyDirectories = GetRuntimeAssemblyDirectories();
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is not null)
            return LoadFromAssemblyPath(assemblyPath);

        try
        {
            return Default.LoadFromAssemblyName(assemblyName);
        }
        catch (FileNotFoundException)
        {
        }

        foreach (var runtimeAssemblyDirectory in _runtimeAssemblyDirectories
                     .OrderBy(directory => IsRequestedRuntimeDirectory(assemblyName, directory) ? 0 : 1))
        {
            var runtimeAssemblyPath = Path.Combine(runtimeAssemblyDirectory, assemblyName.Name + ".dll");
            if (File.Exists(runtimeAssemblyPath))
                return LoadFromAssemblyPath(runtimeAssemblyPath);
        }

        return null;
    }

    private static IReadOnlyList<string> GetRuntimeAssemblyDirectories()
    {
        var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
        var dotnetRoot = Directory.GetParent(runtimeDirectory)?.Parent?.Parent?.FullName;
        if (string.IsNullOrWhiteSpace(dotnetRoot))
            return [];

        return new[] { "Microsoft.NETCore.App", "Microsoft.AspNetCore.App" }
            .SelectMany(framework =>
            {
                var frameworkRoot = Path.Combine(dotnetRoot, "shared", framework);
                return Directory.Exists(frameworkRoot)
                    ? Directory.GetDirectories(frameworkRoot)
                    : [];
            })
            .OrderByDescending(static directory => directory, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsRequestedRuntimeDirectory(AssemblyName assemblyName, string directory)
    {
        var version = assemblyName.Version;
        return version is not null
            && Path.GetFileName(directory).StartsWith(
                version.Major.ToString(CultureInfo.InvariantCulture) + "." +
                version.Minor.ToString(CultureInfo.InvariantCulture) + ".",
                StringComparison.Ordinal);
    }
}

internal sealed record VueBindingTestLane(
    string Id,
    string TestProjectPath,
    string TrxFileName,
    int MinimumPassedTests,
    IReadOnlyList<VueBindingTarget> Targets);

internal sealed record VueBindingTarget(string Id, string AssemblyName, string TargetFramework);

internal sealed record BindingContractUnit(string DisplayName, MemberInfo Member);

internal sealed record BindingSurfaceCoverage(int AuditedUnits, int TotalUnits);

internal sealed record TestCounters(int Total, int Passed, int Failed);

internal sealed record CoverageGateOptions
{
    public string Configuration { get; init; } = "Debug";

    public bool NoBuild { get; init; }

    public bool NoRestore { get; init; }

    public string? ResultsDirectory { get; init; }

    public static CoverageGateOptions Parse(string[] args)
    {
        var result = new CoverageGateOptions();
        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--configuration":
                case "-c":
                    result = result with { Configuration = ReadValue(args, ref index) };
                    break;
                case "--no-build":
                    result = result with { NoBuild = true };
                    break;
                case "--no-restore":
                    result = result with { NoRestore = true };
                    break;
                case "--results-directory":
                    result = result with { ResultsDirectory = ReadValue(args, ref index) };
                    break;
                default:
                    throw new InvalidOperationException("Unknown argument: " + args[index]);
            }
        }

        if (result.Configuration is not "Debug" and not "Release")
            throw new InvalidOperationException("--configuration must be Debug or Release.");
        if (result.NoBuild && !result.NoRestore)
            result = result with { NoRestore = true };
        return result;
    }

    private static string ReadValue(string[] args, ref int index)
    {
        if (++index >= args.Length)
            throw new InvalidOperationException("Missing value for " + args[index - 1]);
        return args[index];
    }
}
