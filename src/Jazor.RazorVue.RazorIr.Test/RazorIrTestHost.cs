using System.Collections;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;

namespace Jazor.RazorVue.RazorIr.Test;

internal static class RazorIrTestHost
{
    private static readonly ConcurrentDictionary<string, PortableExecutableReference> MetadataReferenceCache =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly string[] DefaultProducerFactoryTypeNames =
    [
        "Microsoft.AspNetCore.Razor.Language.TagHelpers.Producers.DefaultTagHelperProducer+Factory, Microsoft.CodeAnalysis.Razor.Compiler",
        "Microsoft.AspNetCore.Razor.Language.TagHelpers.Producers.BindTagHelperProducer+Factory, Microsoft.CodeAnalysis.Razor.Compiler",
        "Microsoft.AspNetCore.Razor.Language.TagHelpers.Producers.ComponentTagHelperProducer+Factory, Microsoft.CodeAnalysis.Razor.Compiler",
        "Microsoft.AspNetCore.Razor.Language.TagHelpers.Producers.EventHandlerTagHelperProducer+Factory, Microsoft.CodeAnalysis.Razor.Compiler",
        "Microsoft.AspNetCore.Razor.Language.TagHelpers.Producers.KeyTagHelperProducer+Factory, Microsoft.CodeAnalysis.Razor.Compiler",
        "Microsoft.AspNetCore.Razor.Language.TagHelpers.Producers.RefTagHelperProducer+Factory, Microsoft.CodeAnalysis.Razor.Compiler",
        "Microsoft.AspNetCore.Razor.Language.TagHelpers.Producers.SplatTagHelperProducer+Factory, Microsoft.CodeAnalysis.Razor.Compiler",
        "Microsoft.AspNetCore.Razor.Language.TagHelpers.Producers.FormNameTagHelperProducer+Factory, Microsoft.CodeAnalysis.Razor.Compiler",
        "Microsoft.AspNetCore.Razor.Language.TagHelpers.Producers.RenderModeTagHelperProducer+Factory, Microsoft.CodeAnalysis.Razor.Compiler"
    ];

    private static readonly string[] InterestingPropertyNames =
    [
        "TagName",
        "AttributeName",
        "AttributeStructure",
        "Content",
        "Prefix",
        "Suffix",
        "Name",
        "IsParameterized",
        "ParameterName",
        "TypeName"
    ];

    public static RazorProjectEngine CreateProjectEngine(string documentPath)
        => CreateProjectEngine(documentPath, static _ => { });

    public static RazorProjectEngine CreateProjectEngineWithExplicitProducerFactories(string documentPath)
        => CreateProjectEngine(documentPath, AddExplicitProducerFactories);

    public static RazorProjectEngine CreateProjectEngineWithOfficialCompilerFeatures(string documentPath)
        => CreateProjectEngine(documentPath, static builder =>
        {
            CompilerFeatures.Register(builder);
        });

    public static RazorProjectEngine CreateProjectEngineWithOfficialSourceGeneratorRegistration(string documentPath)
        => CreateProjectEngine(documentPath, static builder =>
        {
            CompilerFeatures.Register(builder);
            RazorExtensions.Register(builder);
            builder.SetCSharpLanguageVersion(CSharpParseOptions.Default.LanguageVersion);
        });

    private static RazorProjectEngine CreateProjectEngine(
        string documentPath,
        Action<RazorProjectEngineBuilder> configureBuilder)
    {
        var rootPath = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            rootPath = Directory.GetCurrentDirectory();
        }

        return RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(rootPath),
            builder =>
            {
                builder.SetRootNamespace("Jazor.RazorVue.RazorIr.TestHost");
                builder.SetSupportLocalizedComponentNames();
                ComponentCodeDirective.Register(builder);
                configureBuilder(builder);
            });
    }

    public static RazorCodeDocument CreateCodeDocument(string documentPath, string sourceText)
        => CreateCodeDocument(documentPath, sourceText, importSources: [], tagHelpers: null);

    public static RazorCodeDocument CreateCodeDocument(
        string documentPath,
        string sourceText,
        RazorSourceDocument[] importSources,
        IReadOnlyList<TagHelperDescriptor>? tagHelpers)
    {
        var sourceDocument = RazorSourceDocument.Create(sourceText, documentPath);
        var projectEngine = CreateProjectEngine(documentPath);

        return projectEngine.ProcessDesignTime(
            sourceDocument,
            RazorFileKind.Component,
            importSources.ToImmutableArray(),
            tagHelpers is null ? null : TagHelperCollection.Create(tagHelpers));
    }

    public static object GetDocumentNode(RazorCodeDocument codeDocument)
    {
        var method = typeof(RazorCodeDocument).GetMethod(
            "GetDocumentNode",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method is null)
        {
            var availableMethods = typeof(RazorCodeDocument)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(static candidate => candidate.Name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray();
            Assert.Fail(
                "RazorCodeDocument.GetDocumentNode() was not found via reflection. Available methods: "
                + string.Join(", ", availableMethods));
        }

        var documentNode = method.Invoke(codeDocument, null);
        Assert.IsNotNull(documentNode, "RazorCodeDocument.GetDocumentNode() returned null.");
        return documentNode;
    }

    public static IEnumerable<string> EnumerateIntermediateNodeTypeNames(object root)
    {
        yield return root.GetType().Name;

        foreach (var child in EnumerateChildren(root))
        {
            foreach (var typeName in EnumerateIntermediateNodeTypeNames(child))
            {
                yield return typeName;
            }
        }
    }

    public static string DumpIntermediateNodeTree(object root)
    {
        var builder = new StringBuilder();
        AppendNode(builder, root, depth: 0);
        return builder.ToString();
    }

    public static object[] GetEngineFeatures(RazorProjectEngine projectEngine)
    {
        var engineProperty = typeof(RazorProjectEngine).GetProperty(
            "Engine",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(engineProperty, "RazorProjectEngine.Engine was not found.");

        var engine = engineProperty.GetValue(projectEngine);
        Assert.IsNotNull(engine, "RazorProjectEngine.Engine returned null.");

        var featuresProperty = engine.GetType().GetProperty(
            "Features",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(featuresProperty, "Razor engine Features collection was not found.");

        if (featuresProperty.GetValue(engine) is not IEnumerable features)
        {
            return [];
        }

        return features.Cast<object>().Where(static feature => feature is not null).ToArray()!;
    }

    public static CSharpCompilation CreateCompilation(string assemblyName, params string[] sources)
    {
        var syntaxTrees = sources
            .Select(static source => CSharpSyntaxTree.ParseText(source))
            .ToArray();

        return CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    public static TagHelperDescriptor[] DiscoverTagHelpers(RazorProjectEngine projectEngine, Compilation compilation)
    {
        var discoveryService = GetTagHelperDiscoveryService(projectEngine);
        var method = discoveryService.GetType().GetMethod(
            "GetTagHelpers",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(Compilation), typeof(CancellationToken)],
            modifiers: null);
        Assert.IsNotNull(method, "TagHelperDiscoveryService.GetTagHelpers(Compilation, CancellationToken) was not found.");

        var result = method.Invoke(discoveryService, [compilation, CancellationToken.None]);
        return ConvertTagHelpers(result);
    }

    public static TagHelperDescriptor[] DiscoverTagHelpersForCompilation(RazorProjectEngine projectEngine, Compilation compilation)
    {
        var discoveryService = GetTagHelperDiscoveryService(projectEngine);
        var method = discoveryService.GetType().GetMethod(
            "GetTagHelpersForCompilation",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types:
            [
                typeof(Compilation),
                Type.GetType("Microsoft.AspNetCore.Razor.Language.TagHelperDiscoveryOptions, Microsoft.CodeAnalysis.Razor.Compiler", throwOnError: true)!,
                typeof(CancellationToken)
            ],
            modifiers: null);
        Assert.IsNotNull(method, "TagHelperDiscoveryService.GetTagHelpersForCompilation(...) was not found.");

        var optionsType = method.GetParameters()[1].ParameterType;
        var options = Activator.CreateInstance(optionsType);
        Assert.IsNotNull(options, "Could not construct TagHelperDiscoveryOptions.");

        var result = method.Invoke(discoveryService, [compilation, options, CancellationToken.None]);
        return ConvertTagHelpers(result);
    }

    public static object[] GetTagHelperProducers(RazorProjectEngine projectEngine, Compilation compilation)
    {
        var discoveryService = GetTagHelperDiscoveryService(projectEngine);
        var method = discoveryService.GetType().GetMethod(
            "GetProducers",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(Compilation), typeof(bool), typeof(bool)],
            modifiers: null);
        Assert.IsNotNull(method, "TagHelperDiscoveryService.GetProducers(...) was not found.");

        var result = method.Invoke(discoveryService, [compilation, false, false]);
        if (result is not IEnumerable producers)
        {
            return [];
        }

        return producers.Cast<object>().Where(static producer => producer is not null).ToArray()!;
    }

    public static object CreateDefaultTagHelperDiscoveryOptions(RazorProjectEngine projectEngine)
    {
        _ = projectEngine;

        var optionsType = Type.GetType(
            "Microsoft.AspNetCore.Razor.Language.TagHelperDiscoveryOptions, Microsoft.CodeAnalysis.Razor.Compiler",
            throwOnError: true)!;
        var options = Activator.CreateInstance(optionsType);
        Assert.IsNotNull(options, "Could not construct TagHelperDiscoveryOptions.");
        return options;
    }

    public static object? InvokeDiscoveryMethod(
        RazorProjectEngine projectEngine,
        string methodName,
        params object?[] arguments)
    {
        var discoveryService = GetTagHelperDiscoveryService(projectEngine);
        var methods = discoveryService.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => string.Equals(method.Name, methodName, StringComparison.Ordinal))
            .OrderBy(method => method.GetParameters().Length)
            .ToArray();
        Assert.IsTrue(methods.Length > 0, "Discovery method was not found: " + methodName);

        var candidate = methods.FirstOrDefault(method => method.GetParameters().Length == arguments.Length);
        Assert.IsNotNull(candidate, "Discovery method overload was not found for argument count: " + methodName);
        return candidate.Invoke(discoveryService, arguments);
    }

    public static string DumpObjectSurface(object instance)
    {
        var builder = new StringBuilder();
        var instanceType = instance.GetType();
        builder.AppendLine(instanceType.FullName ?? instanceType.Name);

        foreach (var property in instanceType
                     .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .OrderBy(static property => property.Name, StringComparer.Ordinal))
        {
            object? value;
            try
            {
                value = property.GetValue(instance);
            }
            catch (Exception ex)
            {
                value = "<" + ex.GetType().Name + ">";
            }

            builder.Append("  ");
            builder.Append(property.PropertyType.FullName ?? property.PropertyType.Name);
            builder.Append(' ');
            builder.Append(property.Name);
            builder.Append(" = ");
            builder.AppendLine(value?.ToString() ?? "<null>");
        }

        return builder.ToString();
    }

    public static string DumpObjectFieldSurface(object instance)
    {
        var builder = new StringBuilder();
        var instanceType = instance.GetType();
        builder.AppendLine(instanceType.FullName ?? instanceType.Name);

        foreach (var field in instanceType
                     .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .OrderBy(static field => field.Name, StringComparer.Ordinal))
        {
            object? value;
            try
            {
                value = field.GetValue(instance);
            }
            catch (Exception ex)
            {
                value = "<" + ex.GetType().Name + ">";
            }

            builder.Append("  ");
            builder.Append(field.FieldType.FullName ?? field.FieldType.Name);
            builder.Append(' ');
            builder.Append(field.Name);
            builder.Append(" = ");
            builder.AppendLine(value?.ToString() ?? "<null>");
        }

        return builder.ToString();
    }

    public static string[] GetTagHelperProducerFactoryTypeNames(RazorProjectEngine projectEngine)
    {
        var discoveryService = GetTagHelperDiscoveryService(projectEngine);
        var field = discoveryService.GetType().GetField(
            "_producerFactories",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, "TagHelperDiscoveryService._producerFactories was not found.");

        if (field.GetValue(discoveryService) is not IEnumerable factories)
        {
            return [];
        }

        return factories.Cast<object>()
            .Where(static factory => factory is not null)
            .Select(static factory => factory.GetType().FullName ?? factory.GetType().Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray()!;
    }

    public static bool CompilationContainsMetadataType(Compilation compilation, string metadataName)
        => compilation.GetTypeByMetadataName(metadataName) is not null;

    private static void AddExplicitProducerFactories(RazorProjectEngineBuilder builder)
    {
        foreach (var typeName in DefaultProducerFactoryTypeNames)
        {
            var type = Type.GetType(typeName, throwOnError: true)!;
            var instance = Activator.CreateInstance(type);
            Assert.IsNotNull(instance, "Could not instantiate producer factory: " + typeName);
            Assert.IsInstanceOfType<IRazorEngineFeature>(instance, "Producer factory did not implement IRazorEngineFeature: " + typeName);
            builder.Features.Add((IRazorEngineFeature)instance);
        }
    }

    public static object GetTagHelperDiscoveryService(RazorProjectEngine projectEngine)
    {
        var discoveryService = GetEngineFeatures(projectEngine)
            .FirstOrDefault(static feature => string.Equals(
                feature.GetType().FullName,
                "Microsoft.AspNetCore.Razor.Language.TagHelperDiscoveryService",
                StringComparison.Ordinal));
        Assert.IsNotNull(discoveryService, "TagHelperDiscoveryService was not exposed by the Razor project engine.");
        return discoveryService;
    }

    public static string[] GetCompilationErrors(Compilation compilation)
        => compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(static diagnostic => diagnostic.ToString())
            .ToArray();

    public static MetadataReference EmitToMetadataReference(Compilation compilation)
    {
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        if (!result.Success)
        {
            var errors = string.Join(
                Environment.NewLine,
                result.Diagnostics
                    .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(static diagnostic => diagnostic.ToString()));
            Assert.Fail("Compilation emit failed:" + Environment.NewLine + errors);
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    private static TagHelperDescriptor[] ConvertTagHelpers(object? result)
    {
        if (result is not IEnumerable discovered)
        {
            return [];
        }

        return discovered.Cast<object>()
            .OfType<TagHelperDescriptor>()
            .ToArray();
    }

    public static string DescribeTagHelper(TagHelperDescriptor descriptor)
        => descriptor.Name
            + " | "
            + descriptor.AssemblyName
            + " | "
            + descriptor.DisplayName;

    public static string GetLoadedRazorCompilerAssemblyPath()
    {
        var assemblyPath = typeof(RazorProjectEngine).Assembly.Location;
        Assert.IsFalse(string.IsNullOrWhiteSpace(assemblyPath), "RazorProjectEngine assembly location was empty.");
        return assemblyPath;
    }

    public static string ComputeFileSha256(string path)
    {
        Assert.IsTrue(File.Exists(path), "Expected file does not exist: " + path);

        using var stream = File.OpenRead(path);
        var hashBytes = SHA256.HashData(stream);
        return Convert.ToHexString(hashBytes);
    }

    private static void AppendNode(StringBuilder builder, object node, int depth)
    {
        builder.Append(' ', depth * 2);
        builder.Append(node.GetType().Name);
        AppendInterestingProperties(builder, node);
        builder.AppendLine();

        foreach (var child in EnumerateChildren(node))
        {
            AppendNode(builder, child, depth + 1);
        }
    }

    private static void AppendInterestingProperties(StringBuilder builder, object node)
    {
        foreach (var propertyName in InterestingPropertyNames)
        {
            if (!TryGetPropertyText(node, propertyName, out var propertyText))
            {
                continue;
            }

            builder.Append(' ');
            builder.Append(propertyName);
            builder.Append("=\"");
            builder.Append(Escape(propertyText));
            builder.Append('"');
        }
    }

    private static IEnumerable<object> EnumerateChildren(object node)
    {
        var childrenProperty = node.GetType().GetProperty(
            "Children",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (childrenProperty?.GetValue(node) is not IEnumerable children)
        {
            yield break;
        }

        foreach (var child in children)
        {
            if (child is not null)
            {
                yield return child;
            }
        }
    }

    private static bool TryGetPropertyText(object node, string propertyName, out string propertyText)
    {
        var property = node.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property is null)
        {
            propertyText = string.Empty;
            return false;
        }

        var value = property.GetValue(node);
        switch (value)
        {
            case null:
                propertyText = string.Empty;
                return false;
            case string text when !string.IsNullOrWhiteSpace(text):
                propertyText = text;
                return true;
            case Enum enumValue:
                propertyText = enumValue.ToString();
                return true;
            case bool boolValue:
                propertyText = boolValue ? "true" : "false";
                return true;
            case char charValue:
                propertyText = charValue.ToString();
                return true;
            case sbyte or byte or short or ushort or int or uint or long or ulong:
                propertyText = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
                return true;
            default:
                propertyText = string.Empty;
                return false;
        }
    }

    private static string Escape(string text)
        => text
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    public static MetadataReference[] CreateMetadataReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var referencePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                referencePaths.Add(path);
            }
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic || string.IsNullOrWhiteSpace(assembly.Location))
            {
                continue;
            }

            referencePaths.Add(assembly.Location);
        }

        AddAssemblyLocation(referencePaths, typeof(ECMAScript.Vue3));
        AddAssemblyLocation(referencePaths, typeof(ECMAScript.VueContract.VueLibraryComponentAttribute));
        AddAssemblyLocation(referencePaths, typeof(Microsoft.AspNetCore.Components.ComponentBase));
        AddAssemblyLocation(referencePaths, typeof(Microsoft.CodeAnalysis.Compilation));
        AddAssemblyLocation(referencePaths, typeof(Microsoft.CodeAnalysis.CSharp.CSharpCompilation));

        var references = new List<MetadataReference>(referencePaths.Count);
        foreach (var path in referencePaths)
        {
            if (TryCreateCachedPortableExecutableReference(path, out var reference))
            {
                references.Add(reference);
            }
        }

        return [.. references];
    }

    private static void AddAssemblyLocation(HashSet<string> referencePaths, Type markerType)
    {
        var location = markerType.Assembly.Location;
        if (!string.IsNullOrWhiteSpace(location))
        {
            referencePaths.Add(location);
        }
    }

    private static bool TryCreateCachedPortableExecutableReference(
        string path,
        out PortableExecutableReference reference)
    {
        reference = null!;
        if (!TryNormalizeMetadataReferencePath(path, out var normalizedPath))
        {
            return false;
        }

        reference = MetadataReferenceCache.GetOrAdd(
            normalizedPath,
            static candidatePath => MetadataReference.CreateFromFile(candidatePath));
        return true;
    }

    private static bool TryNormalizeMetadataReferencePath(string? path, out string normalizedPath)
    {
        normalizedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var candidatePath = Path.GetFullPath(path);
        if (!File.Exists(candidatePath))
        {
            return false;
        }

        normalizedPath = candidatePath;
        return true;
    }
}
