using System.Reflection;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorCompatibilityProbe
{
    private const string RazorSourceGeneratorTypeName = "Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator";
    private const string IncrementalGeneratorInterfaceName = "Microsoft.CodeAnalysis.IIncrementalGenerator";

    public static RazorSourceGeneratorCompatibilityProbeResult CollectCurrent()
    {
        var assembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(static item => string.Equals(
                item.GetName().Name,
                "Microsoft.CodeAnalysis.Razor.Compiler",
                StringComparison.Ordinal));
        if (assembly is null)
        {
            return RazorSourceGeneratorCompatibilityProbeResult.Fail(
                "Microsoft.CodeAnalysis.Razor.Compiler is not loaded in the current analyzer process.");
        }

        return Collect(assembly);
    }

    public static RazorSourceGeneratorCompatibilityProbeResult Collect(Assembly assembly)
    {
        try
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            var generatorType = assembly.GetType(RazorSourceGeneratorTypeName, throwOnError: false);
            if (generatorType is null)
            {
                return RazorSourceGeneratorCompatibilityProbeResult.Fail(
                    RazorSourceGeneratorTypeName + " was not found in " + assembly.FullName + ".");
            }

            var assemblyPath = assembly.Location ?? string.Empty;

            var initializeMethod = generatorType.GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: [typeof(IncrementalGeneratorInitializationContext)],
                modifiers: null);
            if (initializeMethod is null)
            {
                return RazorSourceGeneratorCompatibilityProbeResult.Fail("RazorSourceGenerator.Initialize(IncrementalGeneratorInitializationContext) was not found.");
            }

            var methodBody = initializeMethod.GetMethodBody();
            if (methodBody is null)
            {
                return RazorSourceGeneratorCompatibilityProbeResult.Fail("RazorSourceGenerator.Initialize(...) did not expose a method body.");
            }

            var ilBytes = methodBody.GetILAsByteArray();
            if (ilBytes is null || ilBytes.Length == 0)
            {
                return RazorSourceGeneratorCompatibilityProbeResult.Fail("RazorSourceGenerator.Initialize(...) IL bytes were unavailable.");
            }

            var initializeParameterType = initializeMethod.GetParameters().SingleOrDefault()?.ParameterType;
            var declaredMethodNames = generatorType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Select(static method => method.Name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray();

            var shape = new RazorSourceGeneratorCompatibilityShape(
                AssemblyPath: assemblyPath,
                AssemblyVersion: assembly.GetName().Version?.ToString() ?? string.Empty,
                ModuleVersionId: generatorType.Module.ModuleVersionId.ToString(),
                TypeFullName: generatorType.FullName ?? RazorSourceGeneratorTypeName,
                ImplementsIncrementalGenerator: generatorType.GetInterfaces().Any(static item => string.Equals(item.FullName, IncrementalGeneratorInterfaceName, StringComparison.Ordinal)),
                InitializeMethodName: initializeMethod.Name,
                InitializeContextParameterType: initializeParameterType?.FullName ?? string.Empty,
                InitializeMethodIlLength: ilBytes.Length,
                InitializeMethodIlSha256: ComputeSha256Hex(ilBytes),
                DeclaredMethodNames: declaredMethodNames);

            return RazorSourceGeneratorCompatibilityProbeResult.Succeed(shape);
        }
        catch (Exception ex)
        {
            return RazorSourceGeneratorCompatibilityProbeResult.Fail(ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static string ComputeSha256Hex(byte[] bytes)
    {
        using var sha = SHA256.Create();
        return ConvertHashToHex(sha.ComputeHash(bytes));
    }

    private static string ConvertHashToHex(byte[] hashBytes)
    {
        var builder = new System.Text.StringBuilder(hashBytes.Length * 2);
        foreach (var item in hashBytes)
        {
            builder.Append(item.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}

internal sealed record RazorSourceGeneratorCompatibilityProbeResult(
    bool Success,
    RazorSourceGeneratorCompatibilityShape? Shape,
    string? Failure)
{
    public static RazorSourceGeneratorCompatibilityProbeResult Succeed(RazorSourceGeneratorCompatibilityShape shape)
        => new(true, shape, null);

    public static RazorSourceGeneratorCompatibilityProbeResult Fail(string failure)
        => new(false, null, failure);
}

internal sealed record RazorSourceGeneratorCompatibilityShape(
    string AssemblyPath,
    string AssemblyVersion,
    string ModuleVersionId,
    string TypeFullName,
    bool ImplementsIncrementalGenerator,
    string InitializeMethodName,
    string InitializeContextParameterType,
    int InitializeMethodIlLength,
    string InitializeMethodIlSha256,
    IReadOnlyList<string> DeclaredMethodNames);
