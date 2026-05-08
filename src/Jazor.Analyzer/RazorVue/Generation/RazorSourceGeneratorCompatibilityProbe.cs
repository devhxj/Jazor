using System.Reflection;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.NET.Sdk.Razor.SourceGenerators;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorCompatibilityProbe
{
    private const string RazorSourceGeneratorTypeName = "Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator";
    private const string IncrementalGeneratorInterfaceName = "Microsoft.CodeAnalysis.IIncrementalGenerator";

    public static RazorSourceGeneratorCompatibilityProbeResult CollectCurrent()
    {
        try
        {
            var generatorType = typeof(RazorSourceGenerator);
            var assembly = generatorType.Assembly;
            var assemblyPath = assembly.Location;
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                return RazorSourceGeneratorCompatibilityProbeResult.Fail("The loaded Razor source generator assembly path was unavailable.");
            }

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
                DeclaredMethodNames: declaredMethodNames,
                HasGetGenerationProjectEngineMethod: HasDeclaredMethod(generatorType, "GetGenerationProjectEngine"),
                HasComputeProjectItemsMethod: HasDeclaredMethod(generatorType, "ComputeProjectItems"),
                HasComputeRazorSourceGeneratorOptionsMethod: HasDeclaredMethod(generatorType, "ComputeRazorSourceGeneratorOptions"));

            return RazorSourceGeneratorCompatibilityProbeResult.Succeed(shape);
        }
        catch (Exception ex)
        {
            return RazorSourceGeneratorCompatibilityProbeResult.Fail(ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static bool HasDeclaredMethod(Type generatorType, string methodName)
        => generatorType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Any(method => string.Equals(method.Name, methodName, StringComparison.Ordinal));

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
    IReadOnlyList<string> DeclaredMethodNames,
    bool HasGetGenerationProjectEngineMethod,
    bool HasComputeProjectItemsMethod,
    bool HasComputeRazorSourceGeneratorOptionsMethod);
