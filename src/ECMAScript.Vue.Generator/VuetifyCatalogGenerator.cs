using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SystemFile = global::System.IO.File;

namespace ECMAScript.VueGenerator;

/// <summary>
/// Maintains the Vuetify projection from the pinned upstream metadata.
/// 以固定的上游 metadata 维护 Vuetify projection，不让 RazorVue 推断 Vue ABI。
/// </summary>
internal static class VuetifyCatalogGenerator
{
    private const string Version = "4.1.8";
    private const int ContractSchemaVersion = 1;
    private const string StableModule = "vuetify/components";
    private const string LabsModule = "vuetify/labs/components";

    public static void Run(string[] args)
    {
        var check = args is ["--check"];
        if (!check && args.Length != 0)
            throw new ArgumentException("Supported arguments: --check.");

        var repositoryRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
        var projectRoot = Path.Combine(repositoryRoot, "src", "ECMAScript.Vuetify");
        var upstreamRoot = Path.Combine(
            repositoryRoot,
            "src",
            "ECMAScript.Vue.Generator",
            "upstream",
            "vuetify",
            Version);
        var schema = ReadContractSchema(Path.Combine(upstreamRoot, "contracts.json"));
        var components = ReadComponents(repositoryRoot, projectRoot);
        var contractsByKey = ValidateInputs(repositoryRoot, projectRoot, upstreamRoot, schema, components);
        var outputs = new List<GeneratedFile>();

        foreach (var component in components)
        {
            var contract = contractsByKey[GetContractKey(component.SourceFile, component.TypeName)];
            outputs.Add(new GeneratedFile(
                component.SourcePath,
                RenderComponentSource(component, contract)));
        }

        outputs.Add(new GeneratedFile(
            Path.Combine(projectRoot, "VuetifyCatalog.g.cs"),
            RenderCatalog(components)));
        outputs.Add(new GeneratedFile(
            Path.Combine(projectRoot, "dist", "components.mjs"),
            RenderComponentShim(components, StableModule, "./vuetify.esm.js")));
        outputs.Add(new GeneratedFile(
            Path.Combine(projectRoot, "dist", "labs.mjs"),
            RenderComponentShim(components, LabsModule, "./vuetify-labs.esm.js")));
        outputs.Add(new GeneratedFile(
            Path.Combine(projectRoot, "manifest.json"),
            RenderManifest()));

        if (check)
        {
            var stale = outputs
                .Where(static output =>
                    !SystemFile.Exists(output.Path) ||
                    !string.Equals(SystemFile.ReadAllText(output.Path), output.Content, StringComparison.Ordinal))
                .Select(output => Path.GetRelativePath(repositoryRoot, output.Path).Replace('\\', '/'))
                .OrderBy(static path => path, StringComparer.Ordinal)
                .ToArray();
            if (stale.Length > 0)
            {
                throw new InvalidOperationException(
                    "Vuetify projection is stale. Run `dotnet run --project src/ECMAScript.Vue.Generator -- vuetify`.\n" +
                    string.Join("\n", stale));
            }

            Console.WriteLine($"Vuetify projection is current: {components.Count} components, version {Version}.");
            return;
        }

        foreach (var output in outputs)
            WriteIfChanged(output);

        Console.WriteLine($"Generated Vuetify projection: {components.Count} components, version {Version}.");
    }

    private static VuetifyContractSchema ReadContractSchema(string path)
    {
        if (!SystemFile.Exists(path))
            throw new InvalidOperationException($"Missing Vuetify contract schema: {path}");

        var schema = JsonSerializer.Deserialize<VuetifyContractSchema>(
            SystemFile.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (schema is null || schema.Components is null)
            throw new InvalidOperationException($"Vuetify contract schema is invalid: {path}");
        if (schema.Version != ContractSchemaVersion ||
            !string.Equals(schema.UpstreamVersion, Version, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Vuetify contract schema must be v{ContractSchemaVersion} for upstream {Version}.");
        }

        return schema;
    }

    private static IReadOnlyList<Component> ReadComponents(string repositoryRoot, string projectRoot)
    {
        var components = new List<Component>();
        var seenExports = new HashSet<(string Module, string Export)>();

        foreach (var path in Directory.EnumerateFiles(projectRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(static path => !path.EndsWith(".g.cs", StringComparison.Ordinal))
                     .OrderBy(static path => path, StringComparer.Ordinal))
        {
            var root = CSharpSyntaxTree.ParseText(SystemFile.ReadAllText(path), path: path).GetRoot();
            foreach (var declaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var attribute = declaration.AttributeLists
                    .SelectMany(static list => list.Attributes)
                    .SingleOrDefault(IsVueLibraryComponent);
                if (attribute is null)
                    continue;

                var arguments = attribute.ArgumentList?.Arguments;
                if (arguments is not { Count: 2 } ||
                    !TryReadString(arguments.Value[0], out var module) ||
                    !TryReadString(arguments.Value[1], out var export))
                {
                    throw new InvalidOperationException(
                        $"VueLibraryComponent on {Path.GetFileName(path)} must declare module and export string literals.");
                }

                if (module is not StableModule and not LabsModule)
                    throw new InvalidOperationException($"Unsupported Vuetify component module '{module}' on {declaration.Identifier.ValueText}.");
                if (!seenExports.Add((module, export)))
                    throw new InvalidOperationException($"Duplicate Vuetify component export '{module}:{export}'.");

                components.Add(new Component(
                    path,
                    NormalizeRelativePath(repositoryRoot, path),
                    module,
                    export,
                    declaration.Identifier.ValueText));
            }
        }

        if (components.Count == 0)
            throw new InvalidOperationException("No [VueLibraryComponent] declarations were found in ECMAScript.Vuetify.");

        return components
            .OrderBy(static component => component.Module, StringComparer.Ordinal)
            .ThenBy(static component => component.Export, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, VuetifyContract> ValidateInputs(
        string repositoryRoot,
        string projectRoot,
        string upstreamRoot,
        VuetifyContractSchema schema,
        IReadOnlyList<Component> components)
    {
        ValidatePackageVersion(Path.Combine(upstreamRoot, "package.json"));
        var contractsByKey = schema.Components.ToDictionary(
            static contract => GetContractKey(contract.SourceFile, contract.AuthoringType),
            StringComparer.Ordinal);
        if (contractsByKey.Count != schema.Components.Count)
            throw new InvalidOperationException("Vuetify contract schema contains duplicate component entries.");
        if (contractsByKey.Count != components.Count)
        {
            throw new InvalidOperationException(
                $"Vuetify contract schema has {contractsByKey.Count} components, but the C# projection has {components.Count}.");
        }

        var webTypeTags = ReadWebTypeTags(Path.Combine(upstreamRoot, "web-types.json"));
        var stableBundleExports = ReadBundleComponentExports(Path.Combine(projectRoot, "dist", "vuetify.esm.js"));
        var labsBundleExports = ReadBundleComponentExports(Path.Combine(projectRoot, "dist", "vuetify-labs.esm.js"));
        foreach (var component in components)
        {
            var key = GetContractKey(component.SourceFile, component.TypeName);
            if (!contractsByKey.TryGetValue(key, out var contract))
            {
                throw new InvalidOperationException(
                    $"Vuetify contract schema does not describe '{component.SourceFile}:{component.TypeName}'.");
            }

            if (!string.Equals(contract.Module, component.Module, StringComparison.Ordinal) ||
                !string.Equals(contract.Export, component.Export, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Vuetify contract '{component.TypeName}' no longer matches its C# module/export declaration.");
            }
            if (!webTypeTags.Contains(component.Export))
                throw new InvalidOperationException($"Vuetify web-types {Version} does not contain tag '{component.Export}'.");

            var bundleExports = component.Module == StableModule ? stableBundleExports : labsBundleExports;
            if (!bundleExports.Contains(component.Export))
            {
                throw new InvalidOperationException(
                    $"Vuetify {Version} bundle '{component.Module}' does not export component '{component.Export}'.");
            }

            if (contract.Members is null || contract.Members.Count == 0)
                throw new InvalidOperationException($"Vuetify contract '{component.TypeName}' has no parameter metadata.");
            if (contract.Members.Select(static member => member.Name).Distinct(StringComparer.Ordinal).Count() != contract.Members.Count)
                throw new InvalidOperationException($"Vuetify contract '{component.TypeName}' contains duplicate member names.");
        }

        return contractsByKey;
    }

    private static void ValidatePackageVersion(string path)
    {
        if (!SystemFile.Exists(path))
            throw new InvalidOperationException($"Missing Vuetify package metadata: {path}");

        using var document = JsonDocument.Parse(SystemFile.ReadAllText(path));
        var version = document.RootElement.GetProperty("version").GetString();
        if (!string.Equals(version, Version, StringComparison.Ordinal))
            throw new InvalidOperationException($"Vuetify package metadata must declare version {Version}, found '{version}'.");
    }

    private static HashSet<string> ReadWebTypeTags(string path)
    {
        if (!SystemFile.Exists(path))
            throw new InvalidOperationException($"Missing Vuetify web-types metadata: {path}");

        using var document = JsonDocument.Parse(SystemFile.ReadAllText(path));
        var tags = document.RootElement
            .GetProperty("contributions")
            .GetProperty("html")
            .GetProperty("tags");
        return tags.EnumerateArray()
            .Select(static tag => tag.GetProperty("name").GetString())
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Select(static name => name!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static HashSet<string> ReadBundleComponentExports(string path)
    {
        if (!SystemFile.Exists(path))
            throw new InvalidOperationException($"Missing Vuetify component bundle: {path}");

        var source = SystemFile.ReadAllText(path);
        const string marker = "var components = /*#__PURE__*/Object.freeze({";
        var start = source.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
            throw new InvalidOperationException($"Cannot locate the components export object in {path}.");

        start += marker.Length;
        var end = source.IndexOf("\n});", start, StringComparison.Ordinal);
        if (end < 0)
            throw new InvalidOperationException($"Cannot locate the end of the components export object in {path}.");

        return Regex.Matches(source[start..end], @"(?m)^\s*(?<name>[$A-Za-z_][$\w]*)\s*:")
            .Select(static match => match.Groups["name"].Value)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string RenderComponentSource(Component component, VuetifyContract contract)
    {
        var source = SystemFile.ReadAllText(component.SourcePath);
        var root = CSharpSyntaxTree.ParseText(source, path: component.SourcePath).GetRoot();
        var declaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .SingleOrDefault(candidate =>
                string.Equals(candidate.Identifier.ValueText, component.TypeName, StringComparison.Ordinal) &&
                candidate.AttributeLists.SelectMany(static list => list.Attributes).Any(IsVueLibraryComponent));
        if (declaration is null)
            throw new InvalidOperationException($"Cannot locate Vuetify component declaration '{component.TypeName}'.");

        var expectedMembers = contract.Members.ToDictionary(static member => member.Name, StringComparer.Ordinal);
        var parameterProperties = declaration.Members.OfType<PropertyDeclarationSyntax>()
            .Where(IsParameterProperty)
            .ToDictionary(static property => property.Identifier.ValueText, StringComparer.Ordinal);
        ValidateParameterProjection(component, expectedMembers, parameterProperties);

        var edits = new List<TextEdit>();
        var lineEnding = source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";

        // The component marker is part of the generated declaration contract. Keep the
        // existing ComponentBase/custom authoring base intact so generated proxies retain
        // their parameter inheritance while satisfying RazorVue's ComponentBase + IVueComponent policy.
        var baseList = declaration.BaseList ?? throw new InvalidOperationException(
            $"Vuetify component '{component.TypeName}' must declare a ComponentBase-derived base type.");
        if (!baseList.Types.Any(static type =>
                type.Type.DescendantNodesAndSelf().OfType<SimpleNameSyntax>().LastOrDefault()?.Identifier.ValueText is "IVuetifyComponent" or "IVueComponent"))
        {
            edits.Add(new TextEdit(baseList.Span.End, 0, ", IVuetifyComponent"));
        }

        foreach (var (memberName, property) in parameterProperties)
        {
            var expectedName = expectedMembers[memberName].RuntimeName;
            if (string.IsNullOrWhiteSpace(expectedName))
                throw new InvalidOperationException($"Vuetify contract '{component.TypeName}.{memberName}' has an empty runtime name.");

            var configuredName = GetConfiguredName(property);
            if (string.Equals(expectedName, property.Identifier.ValueText, StringComparison.Ordinal))
            {
                if (configuredName is not null && !string.Equals(configuredName, expectedName, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Vuetify component '{component.TypeName}.{memberName}' conflicts with contract runtime name '{expectedName}'.");
                }

                continue;
            }

            if (string.Equals(configuredName, expectedName, StringComparison.Ordinal))
                continue;

            var ecmaNameAttribute = property.AttributeLists
                .SelectMany(static list => list.Attributes)
                .SingleOrDefault(IsECMAScriptName);
            if (ecmaNameAttribute is not null)
            {
                edits.Add(new TextEdit(
                    ecmaNameAttribute.Span.Start,
                    ecmaNameAttribute.Span.Length,
                    $"ECMAScriptName(\"{EscapeCSharpString(expectedName)}\")"));
                continue;
            }

            if (configuredName is not null)
            {
                throw new InvalidOperationException(
                    $"Vuetify component '{component.TypeName}.{memberName}' has a Description name '{configuredName}' that conflicts with contract runtime name '{expectedName}'.");
            }

            var lastAttributeList = property.AttributeLists.LastOrDefault();
            if (lastAttributeList is null)
                throw new InvalidOperationException($"Vuetify parameter '{component.TypeName}.{memberName}' has no attribute list.");

            var indentation = GetLineIndentation(source, lastAttributeList.SpanStart);
            edits.Add(new TextEdit(
                lastAttributeList.Span.End,
                0,
                lineEnding + indentation + $"[ECMAScriptName(\"{EscapeCSharpString(expectedName)}\")]"));
        }

        return ApplyEdits(source, edits);
    }

    private static void ValidateParameterProjection(
        Component component,
        IReadOnlyDictionary<string, VuetifyContractMember> expectedMembers,
        IReadOnlyDictionary<string, PropertyDeclarationSyntax> parameterProperties)
    {
        var missing = expectedMembers.Keys.Except(parameterProperties.Keys, StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal).ToArray();
        var unexpected = parameterProperties.Keys.Except(expectedMembers.Keys, StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal).ToArray();
        if (missing.Length == 0 && unexpected.Length == 0)
            return;

        throw new InvalidOperationException(
            $"Vuetify parameter contract mismatch for '{component.TypeName}'. " +
            $"Missing: {string.Join(", ", missing)}. Unexpected: {string.Join(", ", unexpected)}.");
    }

    private static string RenderCatalog(IReadOnlyList<Component> components)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine();
        builder.AppendLine("namespace ECMAScript.Vuetify;");
        builder.AppendLine();

        RenderExports(builder, components.Where(static component => component.Module == StableModule), "VuetifyComponents");
        builder.AppendLine();
        RenderExports(builder, components.Where(static component => component.Module == LabsModule), "VuetifyLabsComponents");
        builder.AppendLine();
        RenderRegistry(builder, components.Where(static component => component.Module == StableModule), "VuetifyComponentRegistry", "stable Vuetify components");
        builder.AppendLine();
        RenderRegistry(builder, components.Where(static component => component.Module == LabsModule), "VuetifyLabsComponentRegistry", "Vuetify labs components");

        return builder.ToString();
    }

    private static void RenderExports(StringBuilder builder, IEnumerable<Component> components, string catalogName)
    {
        var materialized = components.OrderBy(static component => component.Export, StringComparer.Ordinal).ToArray();
        var module = materialized[0].Module;
        builder.AppendLine($"[ECMAScript(\"{module}\")]");
        builder.AppendLine($"public static class {catalogName}");
        builder.AppendLine("{");
        foreach (var component in materialized)
        {
            builder.AppendLine($"    [ECMAScriptName(\"{component.Export}\")]");
            builder.AppendLine($"    public extern static IVuetifyComponent {component.Export} {{ get; }}");
            builder.AppendLine();
        }

        builder.Length -= Environment.NewLine.Length;
        builder.AppendLine("}");
    }

    private static void RenderRegistry(
        StringBuilder builder,
        IEnumerable<Component> components,
        string catalogName,
        string description)
    {
        builder.AppendLine("/// <summary>");
        builder.AppendLine($"/// Registry of {description}.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine($"[Description(\"@#{catalogName}\")]");
        builder.AppendLine($"public sealed record {catalogName} : VueComponentRegistry");
        builder.AppendLine("{");
        foreach (var component in components.OrderBy(static component => component.Export, StringComparer.Ordinal))
        {
            builder.AppendLine($"    [Description(\"@#{component.Export}\")]");
            builder.AppendLine($"    public IVuetifyComponent? {component.Export} {{ get; init; }}");
            builder.AppendLine();
        }

        builder.Length -= Environment.NewLine.Length;
        builder.AppendLine("}");
    }

    private static string RenderComponentShim(
        IEnumerable<Component> components,
        string module,
        string bundlePath)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"import {{ components }} from \"{bundlePath}\";");
        builder.AppendLine();
        foreach (var component in components
                     .Where(component => component.Module == module)
                     .OrderBy(static component => component.Export, StringComparer.Ordinal))
        {
            builder.AppendLine($"export const {component.Export} = components.{component.Export};");
        }

        return builder.ToString();
    }

    private static string RenderManifest()
        => $$"""
        {
          "schemaVersion": 1,
          "libraryId": "vuetify",
          "version": "{{Version}}",
          "imports": {
            "vuetify": {
              "development": "dist/vuetify.esm.js",
              "production": "dist/vuetify.esm.js",
              "developmentDependencies": [
                "vue"
              ],
              "productionDependencies": [
                "vue"
              ]
            },
            "vuetify/components": {
              "development": "dist/components.mjs",
              "production": "dist/components.mjs",
              "developmentDependencies": [
                "vuetify"
              ],
              "productionDependencies": [
                "vuetify"
              ]
            },
            "vuetify/labs/components": {
              "development": "dist/labs.mjs",
              "production": "dist/labs.mjs",
              "developmentDependencies": [
                "vuetify"
              ],
              "productionDependencies": [
                "vuetify"
              ],
              "files": [
                "dist/vuetify-labs.esm.js"
              ]
            },
            "vuetify/directives": {
              "development": "dist/directives.mjs",
              "production": "dist/directives.mjs",
              "developmentDependencies": [
                "vuetify"
              ],
              "productionDependencies": [
                "vuetify"
              ],
              "files": []
            }
          },
          "requires": {
            "vue3": "^3.5.0"
          },
          "styles": [
            "dist/vuetify.min.css"
          ],
          "files": [
            "licenses/LICENSE.md"
          ]
        }
        """ + Environment.NewLine;

    private static bool IsParameterProperty(PropertyDeclarationSyntax property)
        => property.AttributeLists.SelectMany(static list => list.Attributes).Any(IsParameter);

    private static bool IsVueLibraryComponent(AttributeSyntax attribute)
        => IsAttribute(attribute, "VueLibraryComponent");

    private static bool IsECMAScriptName(AttributeSyntax attribute)
        => IsAttribute(attribute, "ECMAScriptName");

    private static bool IsParameter(AttributeSyntax attribute)
        => IsAttribute(attribute, "Parameter");

    private static bool IsAttribute(AttributeSyntax attribute, string expectedName)
    {
        var simpleName = attribute.Name.DescendantNodesAndSelf().OfType<SimpleNameSyntax>().LastOrDefault()?.Identifier.ValueText;
        return string.Equals(simpleName, expectedName, StringComparison.Ordinal) ||
               string.Equals(simpleName, expectedName + "Attribute", StringComparison.Ordinal);
    }

    private static string? GetConfiguredName(PropertyDeclarationSyntax property)
    {
        var ecmaName = property.AttributeLists
            .SelectMany(static list => list.Attributes)
            .SingleOrDefault(IsECMAScriptName);
        var explicitName = TryReadString(ecmaName);
        if (!string.IsNullOrWhiteSpace(explicitName))
            return explicitName;

        var description = property.AttributeLists
            .SelectMany(static list => list.Attributes)
            .SingleOrDefault(attribute => IsAttribute(attribute, "Description"));
        var descriptionValue = TryReadString(description);
        return descriptionValue is { Length: > 2 } && descriptionValue.StartsWith("@#", StringComparison.Ordinal)
            ? descriptionValue[2..]
            : null;
    }

    private static bool TryReadString(AttributeArgumentSyntax argument, out string value)
        => TryReadString(argument.Expression, out value);

    private static bool TryReadString(AttributeSyntax? attribute, out string value)
    {
        if (attribute?.ArgumentList?.Arguments.FirstOrDefault() is { } argument)
            return TryReadString(argument, out value);

        value = string.Empty;
        return false;
    }

    private static string? TryReadString(AttributeSyntax? attribute)
        => TryReadString(attribute, out var value) ? value : null;

    private static bool TryReadString(ExpressionSyntax expression, out string value)
    {
        if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            value = literal.Token.ValueText;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static string ApplyEdits(string source, IReadOnlyList<TextEdit> edits)
    {
        var result = source;
        var nextStart = int.MaxValue;
        foreach (var edit in edits.OrderByDescending(static edit => edit.Start))
        {
            if (edit.Start < 0 || edit.Length < 0 || edit.Start + edit.Length > result.Length || edit.Start + edit.Length > nextStart)
                throw new InvalidOperationException("Vuetify source rewrite contains overlapping edits.");

            result = result.Remove(edit.Start, edit.Length).Insert(edit.Start, edit.Replacement);
            nextStart = edit.Start;
        }

        return result;
    }

    private static string GetLineIndentation(string source, int position)
    {
        var lineStart = source.LastIndexOf('\n', Math.Max(0, position - 1)) + 1;
        var indentationEnd = lineStart;
        while (indentationEnd < source.Length &&
               source[indentationEnd] is ' ' or '\t')
        {
            indentationEnd++;
        }

        return source[lineStart..indentationEnd];
    }

    private static void WriteIfChanged(GeneratedFile output)
    {
        if (SystemFile.Exists(output.Path) && string.Equals(SystemFile.ReadAllText(output.Path), output.Content, StringComparison.Ordinal))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(output.Path)!);
        SystemFile.WriteAllText(output.Path, output.Content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string NormalizeRelativePath(string repositoryRoot, string path)
        => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');

    private static string GetContractKey(string sourceFile, string typeName)
        => sourceFile + "\u001F" + typeName;

    private static string EscapeCSharpString(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string FindRepositoryRoot(string startDirectory)
    {
        for (var directory = new DirectoryInfo(Path.GetFullPath(startDirectory)); directory is not null; directory = directory.Parent)
        {
            if (SystemFile.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }

        throw new InvalidOperationException("Unable to locate Jazor.slnx.");
    }

    private sealed record Component(
        string SourcePath,
        string SourceFile,
        string Module,
        string Export,
        string TypeName);

    private sealed record GeneratedFile(string Path, string Content);

    private sealed record TextEdit(int Start, int Length, string Replacement);

    private sealed record VuetifyContractSchema(
        int Version,
        string UpstreamVersion,
        IReadOnlyList<VuetifyContract> Components);

    private sealed record VuetifyContract(
        string SourceFile,
        string AuthoringType,
        string Module,
        string Export,
        IReadOnlyList<VuetifyContractMember> Members);

    // RawEmitName is retained only in the upstream audit schema. The C# projection
    // consumes RuntimeName and never emits a class-level event descriptor.
    private sealed record VuetifyContractMember(
        string Name,
        string Kind,
        string RuntimeName,
        string? RawEmitName);
}
