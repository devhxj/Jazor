using System.Text;
using System.Text.Json;
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
    private const string Version = "4.2.1";
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
        var descriptions = ReadWebTypeDescriptions(Path.Combine(upstreamRoot, "web-types.json"));
        var components = ReadComponents(repositoryRoot, projectRoot);
        var contractsByKey = ValidateInputs(repositoryRoot, projectRoot, upstreamRoot, schema, components);
        var outputs = new List<GeneratedFile>();

        foreach (var component in components)
        {
            var contract = contractsByKey[GetContractKey(component.SourceFile, component.TypeName)];
            outputs.Add(new GeneratedFile(
                component.SourcePath,
                RenderComponentSource(component, contract, descriptions)));
        }

        outputs.Add(new GeneratedFile(
            Path.Combine(projectRoot, "VuetifyCatalog.g.cs"),
            RenderCatalog(components)));

        if (check)
        {
            var stale = outputs
                .Where(static output =>
                    !SystemFile.Exists(output.Path) ||
                    !string.Equals(NormalizeGeneratedText(SystemFile.ReadAllText(output.Path)), NormalizeGeneratedText(output.Content), StringComparison.Ordinal))
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
                    .SingleOrDefault(IsComponentBinding);
                if (attribute is null)
                    continue;

                // 组件声明面：[ECMAScript("specifier")] 提供模块，导出名由 [ECMAScriptName] 给出。
                var arguments = attribute.ArgumentList?.Arguments;
                if (arguments is not { Count: 1 } ||
                    !TryReadString(arguments.Value[0], out var module) ||
                    !TryReadComponentExport(declaration, out var export))
                {
                    throw new InvalidOperationException(
                        $"ECMAScript Component binding on {Path.GetFileName(path)} must declare one module string literal and an [ECMAScriptName] export.");
                }

                var family = NormalizeComponentFamily(module, export);
                if (!seenExports.Add((family, export)))
                    throw new InvalidOperationException($"Duplicate Vuetify component export '{family}:{export}'.");

                components.Add(new Component(
                    path,
                    NormalizeRelativePath(repositoryRoot, path),
                    family,
                    export,
                    declaration.Identifier.ValueText));
            }
        }

        if (components.Count == 0)
            throw new InvalidOperationException("No [ECMAScript(\"module\")] component declarations were found in ECMAScript.Vuetify.");

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
        foreach (var component in components)
        {
            var key = GetContractKey(component.SourceFile, component.TypeName);
            if (!contractsByKey.TryGetValue(key, out var contract))
            {
                throw new InvalidOperationException(
                    $"Vuetify contract schema does not describe '{component.SourceFile}:{component.TypeName}'.");
            }

            if (!string.Equals(NormalizeComponentFamily(contract.Module, contract.Export), component.Module, StringComparison.Ordinal) ||
                !string.Equals(contract.Export, component.Export, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Vuetify contract '{component.TypeName}' no longer matches its C# module/export declaration.");
            }
            if (!webTypeTags.Contains(component.Export))
                throw new InvalidOperationException($"Vuetify web-types {Version} does not contain tag '{component.Export}'.");

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

    private static string NormalizeComponentFamily(string module, string export)
    {
        if (string.Equals(module, StableModule, StringComparison.Ordinal) ||
            string.Equals(module, StableModule + "/" + export, StringComparison.Ordinal))
        {
            return StableModule;
        }

        if (string.Equals(module, LabsModule, StringComparison.Ordinal) ||
            string.Equals(module, LabsModule + "/" + export, StringComparison.Ordinal))
        {
            return LabsModule;
        }

        throw new InvalidOperationException(
            $"Unsupported Vuetify component module '{module}' for export '{export}'. " +
            "A component module must be its aggregate contract family or its exact export subpath.");
    }

    private static string GetComponentImportPath(Component component)
        => component.Module + "/" + component.Export;

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

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ReadWebTypeDescriptions(string path)
    {
        using var document = JsonDocument.Parse(SystemFile.ReadAllText(path));
        var result = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
        foreach (var tag in document.RootElement.GetProperty("contributions").GetProperty("html").GetProperty("tags").EnumerateArray())
        {
            var name = tag.GetProperty("name").GetString();
            if (string.IsNullOrWhiteSpace(name)) continue;
            var members = new Dictionary<string, string>(StringComparer.Ordinal);
            if (tag.TryGetProperty("attributes", out var attributes))
                foreach (var attribute in attributes.EnumerateArray())
                    if (attribute.TryGetProperty("name", out var member) && attribute.TryGetProperty("description", out var description) && description.ValueKind == JsonValueKind.String)
                        members[member.GetString()!] = description.GetString()!;
            result[name!] = members;
        }
        return result;
    }

    private static string RenderComponentSource(Component component, VuetifyContract contract, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> descriptions)
    {
        var source = BindingDocumentationGenerator.NormalizeDocumentationPlacement(SystemFile.ReadAllText(component.SourcePath));
        var root = CSharpSyntaxTree.ParseText(source, path: component.SourcePath).GetRoot();
        var declaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .SingleOrDefault(candidate =>
                string.Equals(candidate.Identifier.ValueText, component.TypeName, StringComparison.Ordinal) &&
                candidate.AttributeLists.SelectMany(static list => list.Attributes).Any(IsComponentBinding));
        if (declaration is null)
            throw new InvalidOperationException($"Cannot locate Vuetify component declaration '{component.TypeName}'.");

        var expectedMembers = contract.Members.ToDictionary(static member => member.Name, StringComparer.Ordinal);
        var parameterProperties = declaration.Members.OfType<PropertyDeclarationSyntax>()
            .Where(IsParameterProperty)
            .ToDictionary(static property => property.Identifier.ValueText, StringComparer.Ordinal);
        ValidateParameterProjection(component, expectedMembers, parameterProperties);

        var edits = new List<TextEdit>();
        var lineEnding = source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var componentAttribute = declaration.AttributeLists
            .SelectMany(static list => list.Attributes)
            .SingleOrDefault(IsComponentBinding);
        if (componentAttribute is not null &&
            componentAttribute.ArgumentList?.Arguments is { Count: 1 } componentArguments &&
            TryReadString(componentArguments[0], out _))
        {
            // 只重写 specifier；导出名保留在 [ECMAScriptName] 上，由名字机制统一解析。
            var componentModule = GetComponentImportPath(component);
            var replacement = $"ECMAScript(\"{EscapeCSharpString(componentModule)}\")";
            edits.Add(new TextEdit(
                componentAttribute.Span.Start,
                componentAttribute.Span.Length,
                replacement));
        }

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
            if (descriptions.TryGetValue(component.Export, out var memberDescriptions) && memberDescriptions.TryGetValue(expectedName, out var description) &&
                !property.GetLeadingTrivia().ToFullString().Contains("<summary>", StringComparison.Ordinal))
            {
                var docIndentation = GetLineIndentation(source, property.SpanStart);
                edits.Add(new TextEdit(property.SpanStart, 0, $"{docIndentation}/// <summary>\n{docIndentation}/// {EscapeXml(description)}\n{docIndentation}/// </summary>\n"));
            }
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
        builder.AppendLine("/// <summary>供 Vue render/h 调用的组件导出；将所需导出传给渲染函数或应用组件注册表。</summary>");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine($"public static class {catalogName}");
        builder.AppendLine("{");
        foreach (var component in materialized)
        {
            builder.AppendLine($"    /// <summary>用于 render/h 调用的组件导出；组件用法与参数见 <see cref=\"{component.Export}\"/>。</summary>");
            builder.AppendLine($"    [ECMAScript(\"{GetComponentImportPath(component)}\")]");
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
            builder.AppendLine($"    /// <summary>按名称注册 <see cref=\"{component.Export}\"/> 组件，供 Vue 应用解析。</summary>");
            builder.AppendLine($"    [Description(\"@#{component.Export}\")]");
            builder.AppendLine($"    public IVuetifyComponent? {component.Export} {{ get; init; }}");
            builder.AppendLine();
        }

        builder.Length -= Environment.NewLine.Length;
        builder.AppendLine("}");
    }

    private static string NormalizeGeneratedText(string content)
        => content.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static bool IsParameterProperty(PropertyDeclarationSyntax property)
        => property.AttributeLists.SelectMany(static list => list.Attributes).Any(IsParameter);

    private static bool IsComponentBinding(AttributeSyntax attribute)
        => (IsAttribute(attribute, "ECMAScript") &&
            attribute.ArgumentList?.Arguments is { Count: 1 } arguments &&
            TryReadString(arguments[0], out _));

    /// <summary>
    /// 从组件声明读取导出名：由 [ECMAScriptName] 给出，缺省回退类型名（名字机制的三态规则）。
    /// </summary>
    private static bool TryReadComponentExport(ClassDeclarationSyntax declaration, out string export)
    {
        export = string.Empty;
        var nameAttribute = declaration.AttributeLists
            .SelectMany(static list => list.Attributes)
            .SingleOrDefault(IsECMAScriptName);

        if (nameAttribute is null)
        {
            export = declaration.Identifier.ValueText;
            return true;
        }

        var arguments = nameAttribute.ArgumentList?.Arguments;
        if (arguments is not { Count: 1 } || !TryReadString(arguments.Value[0], out export))
            return false;

        return !string.IsNullOrEmpty(export);
    }

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
        // Generated resources must be byte-stable across Windows/Linux generation hosts.
        var normalized = output.Content.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (SystemFile.Exists(output.Path) && string.Equals(SystemFile.ReadAllText(output.Path).Replace("\r\n", "\n", StringComparison.Ordinal), normalized, StringComparison.Ordinal))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(output.Path)!);
        SystemFile.WriteAllText(output.Path, normalized, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string NormalizeRelativePath(string repositoryRoot, string path)
        => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');

    private static string GetContractKey(string sourceFile, string typeName)
        => sourceFile + "\u001F" + typeName;

    private static string EscapeCSharpString(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string EscapeXml(string value)
        => value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

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
