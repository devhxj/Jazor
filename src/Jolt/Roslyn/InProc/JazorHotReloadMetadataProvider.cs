using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jolt.VirtualDocuments.Mapping;
using Jolt.Workspace;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Jolt.Roslyn.InProc;

internal sealed class JazorHotReloadMetadataProvider
{
    private static readonly CSharpParseOptions ParseOptions = new(languageVersion: LanguageVersion.Preview);
    private static readonly ImmutableArray<MetadataReference> MetadataReferences = CreateMetadataReferences();
    private static readonly SymbolDisplayFormat TypeDisplayFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private readonly InProcRoslynCodeService _projectionService;

    public JazorHotReloadMetadataProvider(InProcRoslynCodeService? projectionService = null)
    {
        _projectionService = projectionService ?? new InProcRoslynCodeService();
    }

    public JazorVueHotReloadMetadata CreateMetadata(
        JazorVueDocument document,
        IReadOnlyList<string> loweringDiagnostics,
        IReadOnlyList<DocumentSnapshot>? companionDocuments = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var snapshot = new DocumentSnapshot(
            document.FilePath,
            DocumentKind.Jazor,
            document.SourceText,
            version: null);
        var resolvedCompanionDocuments = LoadCodeBehindDocuments(document.FilePath, companionDocuments);
        var projection = _projectionService.CreateProjection(snapshot, document);
        var parts = AnalyzeProjection(document, projection, resolvedCompanionDocuments);

        if (ShouldRetryWithFallbackProjection(document, parts))
        {
            var fallbackProjection = InProcRoslynCodeService.CreateFallbackProjection(snapshot, document);
            var fallbackParts = AnalyzeProjection(document, fallbackProjection, resolvedCompanionDocuments);
            if (fallbackParts.MappedUserDeclarationCount > parts.MappedUserDeclarationCount)
                parts = fallbackParts;
        }

        return new JazorVueHotReloadMetadata(
            ComputeSignature(BuildDescriptorSignature(parts.Props, parts.Methods)),
            ComputeSignature(document.Template),
            ComputeSignature(BuildLogicSignature(parts.States, parts.Computeds, parts.Methods)),
            ClassifyBoundary(document, parts, loweringDiagnostics));
    }

    private static SemanticHotReloadParts AnalyzeProjection(
        JazorVueDocument document,
        (string ProjectedDocumentPath, string SourceText, ProjectionMap ProjectionMap) projection,
        IReadOnlyList<DocumentSnapshot> companionDocuments)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            projection.SourceText,
            ParseOptions,
            path: projection.ProjectedDocumentPath,
            encoding: Encoding.UTF8);
        var companionTrees = companionDocuments
            .Select(document => CSharpSyntaxTree.ParseText(
                document.Text,
                ParseOptions,
                path: document.DocumentPath,
                encoding: Encoding.UTF8))
            .ToArray();
        var supportTree = CSharpSyntaxTree.ParseText(
            CreateSupportSource(),
            ParseOptions,
            path: "virtual:Jolt.HotReloadSupport.g.cs",
            encoding: Encoding.UTF8);
        var compilation = CSharpCompilation.Create(
            assemblyName: "__JoltHotReloadMetadata",
            syntaxTrees: [syntaxTree, .. companionTrees, supportTree],
            references: MetadataReferences,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
        var root = syntaxTree.GetRoot();

        var props = new List<SemanticPropDescriptor>();
        var states = new List<SemanticLogicDescriptor>();
        var computeds = new List<SemanticLogicDescriptor>();
        var methods = new List<SemanticLogicDescriptor>();
        var mappedUserDeclarationCount = 0;

        foreach (var member in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
        {
            if (!IsUserCodeDeclaration(document, projection.ProjectionMap, member))
                continue;

            mappedUserDeclarationCount++;
            switch (member)
            {
                case PropertyDeclarationSyntax propertyDeclaration:
                    AddPropertyDescriptor(semanticModel, propertyDeclaration, props, states, computeds);
                    break;
                case FieldDeclarationSyntax fieldDeclaration:
                    AddFieldDescriptors(semanticModel, fieldDeclaration, states);
                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    AddMethodDescriptor(semanticModel, methodDeclaration, computeds, methods);
                    break;
            }
        }

        var componentName = Path.GetFileNameWithoutExtension(document.FilePath);
        foreach (var companionTree in companionTrees)
        {
            var companionModel = compilation.GetSemanticModel(companionTree, ignoreAccessibility: true);
            AddCompanionCodeBehindDescriptors(componentName, companionModel, states, computeds, methods);
        }

        return new SemanticHotReloadParts(
            props
                .OrderBy(static item => item.SourceName, StringComparer.Ordinal)
                .ThenBy(static item => item.VueTypeExpression, StringComparer.Ordinal)
                .ToArray(),
            states
                .OrderBy(static item => item.Name, StringComparer.Ordinal)
                .ThenBy(static item => item.Signature, StringComparer.Ordinal)
                .ToArray(),
            computeds
                .OrderBy(static item => item.Name, StringComparer.Ordinal)
                .ThenBy(static item => item.Signature, StringComparer.Ordinal)
                .ToArray(),
            methods
                .OrderBy(static item => item.Name, StringComparer.Ordinal)
                .ThenBy(static item => item.Signature, StringComparer.Ordinal)
                .ToArray(),
            mappedUserDeclarationCount);
    }

    private static IReadOnlyList<DocumentSnapshot> LoadCodeBehindDocuments(
        string jazorDocumentPath,
        IReadOnlyList<DocumentSnapshot>? companionDocuments)
    {
        var pathComparer = GetPathComparer();
        var documents = new List<DocumentSnapshot>();
        var seenPaths = new HashSet<string>(pathComparer);
        var candidatePaths = JoltWorkspaceResolver.GetCoLocatedCodeBehindPaths(jazorDocumentPath)
            .Select(JoltWorkspaceResolver.NormalizePath)
            .ToHashSet(pathComparer);

        if (companionDocuments is not null)
        {
            foreach (var document in companionDocuments)
            {
                if (document.DocumentKind != DocumentKind.CSharp)
                {
                    continue;
                }

                var normalizedPath = JoltWorkspaceResolver.NormalizePath(document.DocumentPath);
                if (!candidatePaths.Contains(normalizedPath) || !seenPaths.Add(normalizedPath))
                {
                    continue;
                }

                documents.Add(new DocumentSnapshot(
                    normalizedPath,
                    DocumentKind.CSharp,
                    document.Text,
                    document.Version));
            }
        }

        foreach (var candidatePath in JoltWorkspaceResolver.GetCoLocatedCodeBehindPaths(jazorDocumentPath))
        {
            var normalizedPath = JoltWorkspaceResolver.NormalizePath(candidatePath);
            if (!seenPaths.Add(normalizedPath))
            {
                continue;
            }

            if (!SafeFileExists(candidatePath))
            {
                continue;
            }

            try
            {
                documents.Add(new DocumentSnapshot(
                    normalizedPath,
                    DocumentKind.CSharp,
                    File.ReadAllText(candidatePath),
                    version: null));
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return documents;
    }

    private static void AddCompanionCodeBehindDescriptors(
        string componentName,
        SemanticModel semanticModel,
        ICollection<SemanticLogicDescriptor> states,
        ICollection<SemanticLogicDescriptor> computeds,
        ICollection<SemanticLogicDescriptor> methods)
    {
        if (string.IsNullOrWhiteSpace(componentName))
            return;

        var root = semanticModel.SyntaxTree.GetRoot();
        foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            if (!typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                || !string.Equals(typeDeclaration.Identifier.ValueText, componentName, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var member in typeDeclaration.Members)
            {
                switch (member)
                {
                    case PropertyDeclarationSyntax propertyDeclaration:
                        AddCompanionPropertyDescriptor(semanticModel, propertyDeclaration, states, computeds);
                        break;
                    case FieldDeclarationSyntax fieldDeclaration:
                        AddFieldDescriptors(semanticModel, fieldDeclaration, states);
                        break;
                    case MethodDeclarationSyntax methodDeclaration:
                        AddMethodDescriptor(semanticModel, methodDeclaration, computeds, methods);
                        break;
                }
            }
        }
    }

    private static void AddPropertyDescriptor(
        SemanticModel semanticModel,
        PropertyDeclarationSyntax propertyDeclaration,
        ICollection<SemanticPropDescriptor> props,
        ICollection<SemanticLogicDescriptor> states,
        ICollection<SemanticLogicDescriptor> computeds)
    {
        var symbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
        if (HasAttribute(symbol, propertyDeclaration.AttributeLists, "Prop"))
        {
            var sourceName = symbol?.Name ?? propertyDeclaration.Identifier.ValueText;
            props.Add(new SemanticPropDescriptor(
                sourceName,
                JazorVueNaming.ToCamelCase(sourceName),
                MapVueType(symbol?.Type, propertyDeclaration.Type)));
        }

        if (HasAttribute(symbol, propertyDeclaration.AttributeLists, "State"))
        {
            var sourceName = symbol?.Name ?? propertyDeclaration.Identifier.ValueText;
            states.Add(new SemanticLogicDescriptor(
                sourceName,
                CreatePropertySignature(symbol, propertyDeclaration),
                NormalizeSyntax(propertyDeclaration)));
        }

        if (HasAttribute(symbol, propertyDeclaration.AttributeLists, "Computed"))
        {
            var sourceName = symbol?.Name ?? propertyDeclaration.Identifier.ValueText;
            computeds.Add(new SemanticLogicDescriptor(
                sourceName,
                CreatePropertySignature(symbol, propertyDeclaration),
                NormalizeSyntax(propertyDeclaration)));
        }
    }

    private static void AddCompanionPropertyDescriptor(
        SemanticModel semanticModel,
        PropertyDeclarationSyntax propertyDeclaration,
        ICollection<SemanticLogicDescriptor> states,
        ICollection<SemanticLogicDescriptor> computeds)
    {
        var symbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
        if (HasAttribute(symbol, propertyDeclaration.AttributeLists, "State"))
        {
            var sourceName = symbol?.Name ?? propertyDeclaration.Identifier.ValueText;
            states.Add(new SemanticLogicDescriptor(
                sourceName,
                CreatePropertySignature(symbol, propertyDeclaration),
                NormalizeSyntax(propertyDeclaration)));
        }

        if (HasAttribute(symbol, propertyDeclaration.AttributeLists, "Computed"))
        {
            var sourceName = symbol?.Name ?? propertyDeclaration.Identifier.ValueText;
            computeds.Add(new SemanticLogicDescriptor(
                sourceName,
                CreatePropertySignature(symbol, propertyDeclaration),
                NormalizeSyntax(propertyDeclaration)));
        }
    }

    private static void AddFieldDescriptors(
        SemanticModel semanticModel,
        FieldDeclarationSyntax fieldDeclaration,
        ICollection<SemanticLogicDescriptor> states)
    {
        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            var symbol = semanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (!HasAttribute(symbol, fieldDeclaration.AttributeLists, "State"))
                continue;

            var sourceName = symbol?.Name ?? variable.Identifier.ValueText;
            states.Add(new SemanticLogicDescriptor(
                sourceName,
                CreateFieldSignature(symbol, fieldDeclaration, variable),
                NormalizeStateInitializer(variable)));
        }
    }

    private static void AddMethodDescriptor(
        SemanticModel semanticModel,
        MethodDeclarationSyntax methodDeclaration,
        ICollection<SemanticLogicDescriptor> computeds,
        ICollection<SemanticLogicDescriptor> methods)
    {
        var symbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
        if (HasAttribute(symbol, methodDeclaration.AttributeLists, "Computed"))
        {
            var sourceName = symbol?.Name ?? methodDeclaration.Identifier.ValueText;
            computeds.Add(new SemanticLogicDescriptor(
                sourceName,
                CreateMethodSignature(symbol, methodDeclaration),
                NormalizeSyntax(methodDeclaration)));
            return;
        }

        if (!IsPublicInstanceMethod(symbol, methodDeclaration))
            return;

        var methodName = symbol?.Name ?? methodDeclaration.Identifier.ValueText;
        methods.Add(new SemanticLogicDescriptor(
            methodName,
            CreateMethodSignature(symbol, methodDeclaration),
            NormalizeSyntax(methodDeclaration)));
    }

    private static bool IsUserCodeDeclaration(
        JazorVueDocument document,
        ProjectionMap projectionMap,
        SyntaxNode node)
    {
        if (document.CodeStartIndex < 0 || document.CodeLength <= 0)
            return false;

        var codeStart = document.CodeStartIndex;
        var codeEnd = document.CodeStartIndex + document.CodeLength;
        foreach (var segment in projectionMap.Segments)
        {
            if (!Intersects(node.SpanStart, node.Span.End, segment.ProjectedStart, segment.ProjectedEnd))
                continue;

            if (Intersects(codeStart, codeEnd, segment.OriginalStart, segment.OriginalEnd))
                return true;
        }

        return false;
    }

    private static bool Intersects(int leftStart, int leftEnd, int rightStart, int rightEnd)
        => leftStart < rightEnd && rightStart < leftEnd;

    private static bool HasAttribute(
        ISymbol? symbol,
        SyntaxList<AttributeListSyntax> attributeLists,
        string attributeName)
    {
        if (symbol is not null)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                var name = attribute.AttributeClass?.Name;
                if (IsAttributeNameMatch(name, attributeName))
                    return true;
            }
        }

        return attributeLists
            .SelectMany(static list => list.Attributes)
            .Any(attribute => IsAttributeNameMatch(GetSimpleAttributeName(attribute.Name), attributeName));
    }

    private static bool IsAttributeNameMatch(string? actualName, string expectedName)
        => string.Equals(actualName, expectedName, StringComparison.Ordinal) ||
           string.Equals(actualName, expectedName + "Attribute", StringComparison.Ordinal);

    private static string GetSimpleAttributeName(NameSyntax name)
    {
        return name switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            QualifiedNameSyntax qualified => GetSimpleAttributeName(qualified.Right),
            AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name.Identifier.ValueText,
            _ => name.ToString().Split('.').Last()
        };
    }

    private static bool IsPublicInstanceMethod(
        IMethodSymbol? symbol,
        MethodDeclarationSyntax methodDeclaration)
    {
        if (symbol is not null)
        {
            return symbol.MethodKind == MethodKind.Ordinary &&
                   !symbol.IsStatic &&
                   symbol.DeclaredAccessibility == Accessibility.Public;
        }

        return methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword) &&
               !methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
    }

    private static string BuildDescriptorSignature(IReadOnlyList<SemanticPropDescriptor> props)
        => BuildDescriptorSignature(props, Array.Empty<SemanticLogicDescriptor>());

    private static string BuildDescriptorSignature(
        IReadOnlyList<SemanticPropDescriptor> props,
        IReadOnlyList<SemanticLogicDescriptor> methods)
    {
        var descriptor = new StringBuilder();
        descriptor.AppendLine("props:");
        foreach (var prop in props)
            descriptor.AppendLine(prop.SourceName + "|" + prop.RuntimeName + "|" + prop.VueTypeExpression);

        descriptor.AppendLine("methods:");
        foreach (var method in methods)
            descriptor.AppendLine(method.Name + "|" + method.Signature);

        return descriptor.ToString();
    }

    private static string BuildLogicSignature(
        IReadOnlyList<SemanticLogicDescriptor> states,
        IReadOnlyList<SemanticLogicDescriptor> computeds,
        IReadOnlyList<SemanticLogicDescriptor> methods)
    {
        var builder = new StringBuilder();
        AppendLogicSection(builder, "states:", states);
        AppendLogicSection(builder, "computeds:", computeds);
        AppendLogicSection(builder, "methods:", methods);
        return builder.ToString();
    }

    private static void AppendLogicSection(
        StringBuilder builder,
        string heading,
        IReadOnlyList<SemanticLogicDescriptor> descriptors)
    {
        builder.AppendLine(heading);
        foreach (var descriptor in descriptors)
            builder.AppendLine(descriptor.Name + "|" + descriptor.Signature + "|" + descriptor.Body);
    }

    private static RazorVueHmrBoundaryKind ClassifyBoundary(
        JazorVueDocument document,
        SemanticHotReloadParts parts,
        IReadOnlyList<string>? loweringDiagnostics)
    {
        if (loweringDiagnostics?.Any(static diagnostic =>
                diagnostic.Contains("could not be lowered", StringComparison.Ordinal)) == true)
        {
            return RazorVueHmrBoundaryKind.FullReloadRequired;
        }

        if (parts.States.Count > 0 || parts.Computeds.Count > 0 || parts.Methods.Count > 0)
            return RazorVueHmrBoundaryKind.LogicSafe;

        return string.IsNullOrWhiteSpace(document.Template)
            ? RazorVueHmrBoundaryKind.Unknown
            : RazorVueHmrBoundaryKind.TemplateOnly;
    }

    private static bool ShouldRetryWithFallbackProjection(
        JazorVueDocument document,
        SemanticHotReloadParts parts)
        => !string.IsNullOrWhiteSpace(document.Code) &&
           parts.MappedUserDeclarationCount == 0;

    private static string CreatePropertySignature(
        IPropertySymbol? symbol,
        PropertyDeclarationSyntax declaration)
    {
        var typeName = symbol?.Type.ToDisplayString(TypeDisplayFormat) ?? declaration.Type.ToString();
        var sourceName = symbol?.Name ?? declaration.Identifier.ValueText;
        return typeName + " " + sourceName;
    }

    private static string CreateFieldSignature(
        IFieldSymbol? symbol,
        FieldDeclarationSyntax declaration,
        VariableDeclaratorSyntax variable)
    {
        var typeName = symbol?.Type.ToDisplayString(TypeDisplayFormat) ?? declaration.Declaration.Type.ToString();
        var sourceName = symbol?.Name ?? variable.Identifier.ValueText;
        return typeName + " " + sourceName;
    }

    private static string CreateMethodSignature(
        IMethodSymbol? symbol,
        MethodDeclarationSyntax declaration)
    {
        if (symbol is null)
            return NormalizeMethodSyntaxSignature(declaration);

        var builder = new StringBuilder();
        if (declaration.Modifiers.Any(SyntaxKind.AsyncKeyword))
            builder.Append("async ");

        builder.Append(symbol.ReturnType.ToDisplayString(TypeDisplayFormat))
            .Append(' ')
            .Append(symbol.Name);

        if (symbol.TypeParameters.Length > 0)
        {
            builder.Append('<')
                .Append(string.Join(", ", symbol.TypeParameters.Select(static parameter => parameter.Name)))
                .Append('>');
        }

        builder.Append('(')
            .Append(string.Join(", ", symbol.Parameters.Select(FormatParameter)))
            .Append(')');
        return builder.ToString();
    }

    private static string FormatParameter(IParameterSymbol parameter)
    {
        var builder = new StringBuilder();
        if (parameter.RefKind != RefKind.None)
            builder.Append(parameter.RefKind.ToString().ToLowerInvariant()).Append(' ');

        builder.Append(parameter.Type.ToDisplayString(TypeDisplayFormat))
            .Append(' ')
            .Append(parameter.Name);

        if (parameter.HasExplicitDefaultValue)
        {
            builder.Append(" = ")
                .Append(parameter.ExplicitDefaultValue ?? "null");
        }

        return builder.ToString();
    }

    private static string NormalizeMethodSyntaxSignature(MethodDeclarationSyntax declaration)
    {
        var modifiers = declaration.Modifiers.ToString();
        return string.Join(
            " ",
            new[]
            {
                modifiers,
                declaration.ReturnType.NormalizeWhitespace().ToFullString(),
                declaration.Identifier.ValueText
            }.Where(static part => !string.IsNullOrWhiteSpace(part))) +
            "(" +
            string.Join(", ", declaration.ParameterList.Parameters.Select(static parameter =>
                parameter.NormalizeWhitespace().ToFullString())) +
            ")";
    }

    private static string NormalizeStateInitializer(VariableDeclaratorSyntax variable)
        => variable.Initializer is null
            ? string.Empty
            : variable.Initializer.Value.NormalizeWhitespace().ToFullString();

    private static string NormalizeSyntax(SyntaxNode node)
        => node.NormalizeWhitespace(elasticTrivia: false).ToFullString();

    private static string MapVueType(ITypeSymbol? typeSymbol, TypeSyntax fallbackType)
    {
        if (typeSymbol is not null)
        {
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_String => "String",
                SpecialType.System_Boolean => "Boolean",
                SpecialType.System_Byte or
                SpecialType.System_SByte or
                SpecialType.System_Int16 or
                SpecialType.System_UInt16 or
                SpecialType.System_Int32 or
                SpecialType.System_UInt32 or
                SpecialType.System_Int64 or
                SpecialType.System_UInt64 or
                SpecialType.System_Single or
                SpecialType.System_Double or
                SpecialType.System_Decimal => "Number",
                _ => "null"
            };
        }

        return fallbackType.ToString() switch
        {
            "string" or "String" or "string?" or "String?" => "String",
            "bool" or "Boolean" or "bool?" or "Boolean?" => "Boolean",
            "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or
                "float" or "double" or "decimal" => "Number",
            _ => "null"
        };
    }

    private static string ComputeSignature(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content ?? string.Empty));
        return Convert.ToHexString(bytes);
    }

    private static string CreateSupportSource()
        => """
            [global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]
            internal sealed class PropAttribute : global::System.Attribute { }

            [global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]
            internal sealed class StateAttribute : global::System.Attribute { }

            [global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]
            internal sealed class ComputedAttribute : global::System.Attribute { }
            """;

    private static ImmutableArray<MetadataReference> CreateMetadataReferences()
    {
        var references = ImmutableArray.CreateBuilder<MetadataReference>();
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (!string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            foreach (var path in trustedPlatformAssemblies
                         .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (TryCreateMetadataReference(path, out var reference))
                {
                    references.Add(reference);
                }
            }
        }

        AddReference(references, typeof(ComponentBase).Assembly.Location);
        return references.ToImmutable();
    }

    private static void AddReference(
        ImmutableArray<MetadataReference>.Builder references,
        string? assemblyPath)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath) || !File.Exists(assemblyPath))
            return;

        if (references.Any(reference =>
                reference is PortableExecutableReference portableReference &&
                string.Equals(portableReference.FilePath, assemblyPath, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        if (TryCreateMetadataReference(assemblyPath, out var reference))
        {
            references.Add(reference);
        }
    }

    private static bool SafeFileExists(string filePath)
    {
        try
        {
            return File.Exists(filePath);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static StringComparer GetPathComparer()
        => OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private static bool TryCreateMetadataReference(
        string? assemblyPath,
        [NotNullWhen(true)] out MetadataReference? reference)
    {
        reference = null;
        if (string.IsNullOrWhiteSpace(assemblyPath) || !SafeFileExists(assemblyPath))
        {
            return false;
        }

        try
        {
            reference = MetadataReference.CreateFromFile(assemblyPath);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private sealed record SemanticPropDescriptor(
        string SourceName,
        string RuntimeName,
        string VueTypeExpression);

    private sealed record SemanticLogicDescriptor(
        string Name,
        string Signature,
        string Body);

    private sealed record SemanticHotReloadParts(
        IReadOnlyList<SemanticPropDescriptor> Props,
        IReadOnlyList<SemanticLogicDescriptor> States,
        IReadOnlyList<SemanticLogicDescriptor> Computeds,
        IReadOnlyList<SemanticLogicDescriptor> Methods,
        int MappedUserDeclarationCount);
}
