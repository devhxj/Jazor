#!/usr/bin/env dotnet run
#:package Microsoft.CodeAnalysis.CSharp@5.7.0-1.26207.106

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

var options = AuditOptions.Parse(args);
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
const string VuetifyVersion = "4.1.8";

var targetDirectory = Path.Combine(repoRoot, "src", "ECMAScript.Vuetify");

if (!Directory.Exists(targetDirectory))
    throw new DirectoryNotFoundException(targetDirectory);

var totalComponents = 0;
var totalEntries = 0;
var unresolvedEntries = 0;
var contracts = new List<VuetifyContract>();
foreach (var path in Directory.EnumerateFiles(targetDirectory, "*.cs", SearchOption.AllDirectories)
             .Where(static path => !path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase) &&
                                   !path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase))
             .OrderBy(static path => path, StringComparer.Ordinal))
{
    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), new CSharpParseOptions(LanguageVersion.Preview), path);
    var root = tree.GetCompilationUnitRoot();

    foreach (var component in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                 .Where(static declaration => FlattenAttributes(declaration.AttributeLists).Any(IsComponentBinding)))
    {
        totalComponents++;
        var result = AuditComponent(component, path, repoRoot, options.WriteSchema);
        totalEntries += result.TotalEntries;
        unresolvedEntries += result.UnresolvedEntries;
        contracts.Add(result.Contract);
    }
}

if (options.WriteSchema)
{
    var schemaPath = WriteSchema(repoRoot, contracts);
    Console.WriteLine($"Wrote {contracts.Count} Vuetify contracts to {Path.GetRelativePath(repoRoot, schemaPath).Replace('\\', '/')}." );
}

Console.WriteLine($"components={totalComponents}, contract-entries={totalEntries}, unresolved={unresolvedEntries}");
if (!options.WriteSchema && unresolvedEntries > 0)
    Environment.ExitCode = 1;

static AuditResult AuditComponent(
    ClassDeclarationSyntax component,
    string path,
    string repoRoot,
    bool suppressReport)
{
    var parameters = component.Members
        .OfType<PropertyDeclarationSyntax>()
        .Where(static property => HasAttribute(property.AttributeLists, "Parameter"))
        .Select(CreateParameter)
        .ToArray();
    var normalRuntimeNames = parameters
        .Where(static parameter => parameter.Kind is not ComponentParameterKind.Event)
        .ToDictionary(
            static parameter => parameter.SourceName,
            static parameter => parameter.ExplicitName ?? ToLowerCamelCase(parameter.SourceName),
            StringComparer.Ordinal);
    var componentName = component.Identifier.ValueText;
    var identity = GetComponentIdentity(component, path, repoRoot);
    var unresolved = 0;
    var members = new List<VuetifyContractMember>(parameters.Length);

    foreach (var parameter in parameters)
    {
        var mapping = ResolveLegacyMapping(parameter, normalRuntimeNames);
        var needsMemberMapping = !string.Equals(mapping.ListenerOrRuntimeName, parameter.SourceName, StringComparison.Ordinal) &&
                                 parameter.ExplicitName is null;
        if (needsMemberMapping)
        {
            unresolved++;
            if (!suppressReport)
            {
                Console.WriteLine(
                    string.Join(
                        "\t",
                        identity.SourceFile,
                        componentName,
                        parameter.SourceName,
                        parameter.Kind,
                        parameter.SourceName,
                        mapping.ListenerOrRuntimeName,
                        mapping.RawEmitName ?? string.Empty,
                        "member-metadata"));
            }
        }

        members.Add(new VuetifyContractMember(
            parameter.SourceName,
            parameter.Kind,
            mapping.ListenerOrRuntimeName,
            mapping.RawEmitName));
    }

    return new AuditResult(
        parameters.Length,
        unresolved,
        new VuetifyContract(identity.SourceFile, componentName, identity.Module, identity.Export, members));
}

static ComponentParameter CreateParameter(PropertyDeclarationSyntax declaration)
{
    var sourceName = declaration.Identifier.ValueText;
    return new ComponentParameter(
        declaration,
        sourceName,
        ClassifyParameterKind(declaration.Type),
        GetExplicitName(declaration.AttributeLists));
}

static ComponentNameMapping ResolveLegacyMapping(
    ComponentParameter parameter,
    IReadOnlyDictionary<string, string> normalRuntimeNames)
{
    if (parameter.Kind is ComponentParameterKind.Prop)
    {
        return new ComponentNameMapping(
            parameter.ExplicitName ?? ToLowerCamelCase(parameter.SourceName),
            RawEmitName: null);
    }

    if (parameter.Kind is ComponentParameterKind.Slot)
    {
        var runtimeName = parameter.ExplicitName ?? (parameter.SourceName is "ChildContent" or "DefaultContent"
            ? "default"
            : ToKebabCase(RemoveContentSuffix(parameter.SourceName)));
        return new ComponentNameMapping(runtimeName, RawEmitName: null);
    }

    if (parameter.ExplicitName is { } explicitName)
    {
        return new ComponentNameMapping(explicitName, ToEmitName(explicitName));
    }

    if (parameter.SourceName.EndsWith("Changed", StringComparison.Ordinal))
    {
        var modelName = parameter.SourceName[..^"Changed".Length];
        if (normalRuntimeNames.TryGetValue(modelName, out var modelRuntimeName))
        {
            var rawEmitName = "update:" + modelRuntimeName;
            return new ComponentNameMapping(ToListenerPropertyName(rawEmitName), rawEmitName);
        }
    }

    var listenerName = ToLowerCamelCase(parameter.SourceName);
    var rawName = parameter.SourceName.Length > 2 &&
                  parameter.SourceName.StartsWith("On", StringComparison.Ordinal) &&
                  char.IsUpper(parameter.SourceName[2])
        ? ToKebabCase(parameter.SourceName[2..])
        : ToEmitName(listenerName);
    return new ComponentNameMapping(listenerName, rawName);
}

static string WriteSchema(string repositoryRoot, IReadOnlyList<VuetifyContract> contracts)
{
    var schemaPath = Path.Combine(
        repositoryRoot,
        "src",
        "ECMAScript.Vue.Generator",
        "upstream",
        "vuetify",
        VuetifyVersion,
        "contracts.json");
    Directory.CreateDirectory(Path.GetDirectoryName(schemaPath)!);

    var schema = new VuetifyContractSchema(
        Version: 1,
        UpstreamVersion: VuetifyVersion,
        Components: contracts
            .OrderBy(static contract => contract.SourceFile, StringComparer.Ordinal)
            .ThenBy(static contract => contract.AuthoringType, StringComparer.Ordinal)
            .Select(static contract => contract with
            {
                Members = contract.Members
                    .OrderBy(static member => member.Name, StringComparer.Ordinal)
                    .ToArray()
            })
            .ToArray());
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    File.WriteAllText(
        schemaPath,
        JsonSerializer.Serialize(schema, options) + Environment.NewLine,
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    return schemaPath;
}

static ComponentIdentity GetComponentIdentity(
    ClassDeclarationSyntax component,
    string path,
    string repositoryRoot)
{
    var attribute = FlattenAttributes(component.AttributeLists)
        .SingleOrDefault(IsComponentBinding)
        ?? throw new InvalidOperationException($"{component.Identifier.ValueText} is missing ECMAScript Component metadata.");
    var arguments = attribute.ArgumentList?.Arguments;
    if (arguments is not { Count: 3 } ||
        GetStringLiteral(arguments.Value[0].Expression) is not { Length: > 0 } module ||
        !IsComponentTransform(arguments.Value[1].Expression) ||
        GetStringLiteral(arguments.Value[2].Expression) is not { Length: > 0 } export)
    {
        throw new InvalidOperationException(
            $"ECMAScript Component on {component.Identifier.ValueText} must declare module and export string literals.");
    }

    return new ComponentIdentity(
        Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/'),
        module,
        export);
}

static string? GetExplicitName(SyntaxList<AttributeListSyntax> attributes)
{
    foreach (var attribute in FlattenAttributes(attributes).Where(static attribute => IsAttribute(attribute, "ECMAScriptName")))
    {
        if (attribute.ArgumentList?.Arguments.FirstOrDefault() is { Expression: var expression } &&
            GetStringLiteral(expression) is { Length: > 0 } name &&
            !string.IsNullOrWhiteSpace(name))
        {
            return name.Trim();
        }
    }

    foreach (var attribute in FlattenAttributes(attributes).Where(static attribute => IsAttribute(attribute, "Description")))
    {
        if (attribute.ArgumentList?.Arguments.FirstOrDefault() is { Expression: var expression } &&
            GetStringLiteral(expression) is { } description &&
            description.StartsWith("@#", StringComparison.Ordinal) &&
            description.Length > 2 &&
            !string.IsNullOrWhiteSpace(description[2..]))
        {
            return description[2..].Trim();
        }
    }

    return null;
}

static ComponentParameterKind ClassifyParameterKind(TypeSyntax type)
{
    var simpleName = type.DescendantNodesAndSelf().OfType<SimpleNameSyntax>().FirstOrDefault()?.Identifier.ValueText;
    return simpleName switch
    {
        "EventCallback" => ComponentParameterKind.Event,
        "RenderFragment" => ComponentParameterKind.Slot,
        _ => ComponentParameterKind.Prop
    };
}

static bool HasAttribute(SyntaxList<AttributeListSyntax> attributes, string attributeName)
    => FlattenAttributes(attributes).Any(attribute => IsAttribute(attribute, attributeName));

static IEnumerable<AttributeSyntax> FlattenAttributes(SyntaxList<AttributeListSyntax> attributes)
    => attributes.SelectMany(static list => list.Attributes);

static bool IsAttribute(AttributeSyntax attribute, string expectedName)
{
    var simpleName = attribute.Name.DescendantNodesAndSelf().OfType<SimpleNameSyntax>().LastOrDefault()?.Identifier.ValueText;
    return string.Equals(simpleName, expectedName, StringComparison.Ordinal) ||
           string.Equals(simpleName, expectedName + "Attribute", StringComparison.Ordinal);
}

static bool IsComponentBinding(AttributeSyntax attribute)
    => IsAttribute(attribute, "ECMAScript") &&
       attribute.ArgumentList?.Arguments is { Count: 2 or 3 } arguments &&
       IsComponentTransform(arguments[1].Expression);

static bool IsComponentTransform(ExpressionSyntax expression)
    => string.Equals(expression.ToString(), "Transform.Component", StringComparison.Ordinal) ||
       string.Equals(expression.ToString(), "ECMAScript.Transform.Component", StringComparison.Ordinal) ||
       string.Equals(expression.ToString(), "global::ECMAScript.Transform.Component", StringComparison.Ordinal);

static string? GetStringLiteral(ExpressionSyntax expression)
    => expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression)
        ? literal.Token.ValueText
        : null;

static string RemoveContentSuffix(string value)
    => value.EndsWith("Content", StringComparison.Ordinal) && value.Length > "Content".Length
        ? value[..^"Content".Length]
        : value;

static string ToLowerCamelCase(string value)
    => value.Length == 0 ? value : char.ToLowerInvariant(value[0]) + value[1..];

static string ToKebabCase(string value)
{
    var result = new StringBuilder(value.Length + 4);
    for (var index = 0; index < value.Length; index++)
    {
        var character = value[index];
        if (char.IsUpper(character))
        {
            var separatesWord = index > 0 &&
                (char.IsLower(value[index - 1]) ||
                 char.IsDigit(value[index - 1]) ||
                 index + 1 < value.Length && char.IsLower(value[index + 1]));
            if (separatesWord)
                result.Append('-');

            result.Append(char.ToLowerInvariant(character));
            continue;
        }

        result.Append(character);
    }

    return result.ToString();
}

static string ToListenerPropertyName(string eventName)
{
    if (eventName.Length == 0 ||
        eventName.StartsWith("on", StringComparison.Ordinal) &&
        eventName.Length > 2 &&
        char.IsUpper(eventName[2]))
    {
        return eventName;
    }

    var result = new StringBuilder(eventName.Length + 2);
    result.Append("on");
    var capitalizeNext = true;
    foreach (var character in eventName)
    {
        if (character == '-')
        {
            capitalizeNext = true;
            continue;
        }

        result.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
        capitalizeNext = false;
    }

    return result.ToString();
}

static string ToEmitName(string listenerName)
    => listenerName.Length > 2 &&
       listenerName.StartsWith("on", StringComparison.Ordinal) &&
       char.IsUpper(listenerName[2])
        ? char.ToLowerInvariant(listenerName[2]) + listenerName[3..]
        : listenerName;

static string FindRepositoryRoot(string startDirectory)
{
    for (var directory = new DirectoryInfo(Path.GetFullPath(startDirectory)); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    }

    throw new InvalidOperationException("Unable to locate Jazor.slnx.");
}

enum ComponentParameterKind
{
    Prop,
    Event,
    Slot
}

sealed record ComponentParameter(
    PropertyDeclarationSyntax Declaration,
    string SourceName,
    ComponentParameterKind Kind,
    string? ExplicitName);

sealed record ComponentNameMapping(string ListenerOrRuntimeName, string? RawEmitName);

sealed record AuditResult(int TotalEntries, int UnresolvedEntries, VuetifyContract Contract);

sealed record ComponentIdentity(string SourceFile, string Module, string Export);

sealed record VuetifyContractSchema(int Version, string UpstreamVersion, IReadOnlyList<VuetifyContract> Components);

sealed record VuetifyContract(
    string SourceFile,
    string AuthoringType,
    string Module,
    string Export,
    IReadOnlyList<VuetifyContractMember> Members);

sealed record VuetifyContractMember(
    string Name,
    ComponentParameterKind Kind,
    string RuntimeName,
    string? RawEmitName);

sealed record AuditOptions(bool WriteSchema)
{
    public static AuditOptions Parse(string[] args)
    {
        if (args.Length == 0)
            return new AuditOptions(WriteSchema: false);
        if (args is ["--write-schema"])
            return new AuditOptions(WriteSchema: true);

        throw new ArgumentException("Usage: dotnet run --file scripts/csharp/audit-vue-component-names.cs [--write-schema]");
    }
}
