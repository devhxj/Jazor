using System.Collections.Immutable;
using System.Text;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Compiler;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal sealed class RazorVueCompilerModuleContext
{
    private readonly RazorVueSemanticSnapshot _snapshot;
    private readonly Dictionary<ISymbol, string> _declaredNames;
    private readonly Dictionary<INamedTypeSymbol, string> _helperTypeNames;
    private readonly HashSet<INamedTypeSymbol> _requiredHelperTypes;
    private readonly List<RazorVueCompilerImportBinding> _compilerImports;

    private RazorVueCompilerModuleContext(
        RazorVueSemanticSnapshot snapshot,
        Dictionary<ISymbol, string> declaredNames,
        Dictionary<INamedTypeSymbol, string> helperTypeNames)
    {
        _snapshot = snapshot;
        _declaredNames = declaredNames;
        _helperTypeNames = helperTypeNames;
        _requiredHelperTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        _compilerImports = new List<RazorVueCompilerImportBinding>();
    }

    public IReadOnlyDictionary<ISymbol, string> DeclaredNames => _declaredNames;

    public ImmutableArray<RazorVueCompilerImportBinding> CompilerImports => _compilerImports.Distinct().ToImmutableArray();

    public static RazorVueCompilerModuleContext Create(RazorVueSemanticSnapshot snapshot)
    {
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var localNames = BuildLocalNames(snapshot.ComponentSymbol);
        var usedNames = new HashSet<string>(StringComparer.Ordinal);
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
        var helperTypeNames = new Dictionary<INamedTypeSymbol, string>(SymbolEqualityComparer.Default);

        foreach (var method in snapshot.Logic.Methods
                     .Select(static method => method.MethodSymbol)
                     .Where(static method => ShouldDeclareMethod(method))
                     .OrderBy(static method => GetSourceOrder(method))
                     .ThenBy(static method => method.ToDisplayString(), StringComparer.Ordinal))
        {
            declaredNames[method.OriginalDefinition] = ChooseDeclaredName(
                method,
                usedNames,
                localNames,
                ToLowerCamelCase(method.Name));
        }

        foreach (var type in EnumerateSameSourceRuntimeHelperTypes(snapshot)
                     .OrderBy(static type => GetSourceOrder(type))
                     .ThenBy(static type => type.ToDisplayString(), StringComparer.Ordinal))
        {
            var name = ChooseDeclaredName(
                type,
                usedNames,
                localNames,
                Util.GetConfigOrSymbolName(type));
            declaredNames[type.OriginalDefinition] = name;
            helperTypeNames[type.OriginalDefinition] = name;
        }

        return new RazorVueCompilerModuleContext(snapshot, declaredNames, helperTypeNames);
    }

    public void RecordObjectCreation(IObjectCreationOperation operation)
    {
        if (operation.Type is INamedTypeSymbol namedType &&
            _helperTypeNames.ContainsKey(namedType.OriginalDefinition))
        {
            _requiredHelperTypes.Add(namedType.OriginalDefinition);
            return;
        }

        ThrowIfUnsupportedSameArtifactRuntimeHelperType(operation, operation.Type, "object creation");
    }

    public void RecordTypeReference(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType &&
            _helperTypeNames.ContainsKey(namedType.OriginalDefinition))
        {
            _requiredHelperTypes.Add(namedType.OriginalDefinition);
            return;
        }

        ThrowIfUnsupportedSameArtifactRuntimeHelperType(null, type, "runtime type reference");
    }

    public void AppendRequiredHelperTypeDeclarations(StringBuilder builder, string indent)
    {
        if (_requiredHelperTypes.Count == 0)
            return;

        var emitted = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        while (true)
        {
            var next = _requiredHelperTypes
                .Where(type => !emitted.Contains(type.OriginalDefinition))
                .OrderBy(static type => GetSourceOrder(type))
                .ThenBy(static type => type.ToDisplayString(), StringComparer.Ordinal)
                .FirstOrDefault();
            if (next is null)
                return;

            AppendHelperTypeDeclaration(builder, next, indent, emitted);
        }
    }

    private void AppendHelperTypeDeclaration(
        StringBuilder builder,
        INamedTypeSymbol type,
        string indent,
        HashSet<INamedTypeSymbol> emitted)
    {
        if (!emitted.Add(type.OriginalDefinition))
            return;

        if (type.BaseType is INamedTypeSymbol baseType &&
            _helperTypeNames.ContainsKey(baseType.OriginalDefinition))
        {
            AppendHelperTypeDeclaration(builder, baseType, indent, emitted);
        }

        if (TryConvertHelperType(type, out var declarationText))
        {
            AppendIndentedBlock(builder, declarationText, indent);
            return;
        }

        throw CreateUnsupportedSetupHelperTypeException(type);
    }

    private void MergeCompilerImports(ImmutableArray<ImportDeclaration> imports)
    {
        foreach (var import in imports)
        {
            var modulePath = TryGetImportModulePath(import);
            if (string.IsNullOrWhiteSpace(modulePath))
                continue;

            var source = modulePath!;
            foreach (var specifier in import.Specifiers)
            {
                switch (specifier)
                {
                    case ImportDefaultSpecifier defaultSpecifier:
                        _compilerImports.Add(new RazorVueCompilerImportBinding(
                            source,
                            RazorVueCompilerImportKind.Default,
                            defaultSpecifier.Local.Name,
                            ImportedName: null));
                        break;
                    case ImportNamespaceSpecifier namespaceSpecifier:
                        _compilerImports.Add(new RazorVueCompilerImportBinding(
                            source,
                            RazorVueCompilerImportKind.Namespace,
                            namespaceSpecifier.Local.Name,
                            ImportedName: null));
                        break;
                    case ImportSpecifier namedSpecifier:
                        _compilerImports.Add(new RazorVueCompilerImportBinding(
                            source,
                            RazorVueCompilerImportKind.Named,
                            namedSpecifier.Local.Name,
                            namedSpecifier.Imported.ToECMAScript()));
                        break;
                }
            }
        }
    }

    private static string? TryGetImportModulePath(ImportDeclaration import)
    {
        if (import.Source is not StringLiteral literal)
            return null;

        return literal.Value?.ToString();
    }

    private bool TryConvertHelperType(INamedTypeSymbol type, out string declarationText)
    {
        declarationText = string.Empty;
        try
        {
            var syntaxReference = type.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference is null)
                return false;

            var semanticModel = _snapshot.Compilation.GetSemanticModel(syntaxReference.SyntaxTree);
            var converter = new AstConverter(
                _snapshot.ComponentSymbol,
                semanticModel,
                new AstConverterOptions(
                    AstConverterProfile.RazorVueRuntime,
                    MemberFilter: null,
                    DeclaredNames: _declaredNames,
                    Host: new HelperDiscoveryCompilerHost(this)));
            var declaration = converter.ConvertRuntimeClass(type);
            if (declaration is null)
                return false;

            declarationText = declaration.ToKnRECMAScript();
            MergeCompilerImports(converter.FlushImportDeclarations([declaration]));
            return !string.IsNullOrWhiteSpace(declarationText);
        }
        catch (Exception ex) when (ex is NotSupportedException or OperationTransformationException)
        {
            return false;
        }
    }

    private static void AppendIndentedBlock(StringBuilder builder, string text, string indent)
    {
        var normalized = Util.NormalizeLineEndingsToLf(text).Trim();
        if (normalized.Length == 0)
            return;

        foreach (var line in normalized.Split('\n'))
            builder.Append(indent).AppendLine(line);
    }


    private static ImmutableArray<INamedTypeSymbol> EnumerateSameSourceRuntimeHelperTypes(RazorVueSemanticSnapshot snapshot)
    {
        var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var componentTrees = snapshot.ComponentSymbol.DeclaringSyntaxReferences
            .Select(static reference => reference.SyntaxTree)
            .Distinct()
            .ToArray();

        foreach (var tree in componentTrees)
        {
            var semanticModel = snapshot.Compilation.GetSemanticModel(tree);
            foreach (var declaration in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol type)
                    continue;

                if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, snapshot.ComponentSymbol.OriginalDefinition))
                    continue;

                if (!IsSameArtifactModuleType(snapshot.ComponentSymbol, type))
                    continue;

                if (!IsRuntimeHelperTypeCandidate(type))
                    continue;

                if (seen.Add(type.OriginalDefinition))
                    builder.Add(type);
            }
        }

        return builder.ToImmutable();
    }

    internal static bool IsSameArtifactModuleType(INamedTypeSymbol componentType, INamedTypeSymbol type)
    {
        for (var containingType = type.ContainingType; containingType is not null; containingType = containingType.ContainingType)
        {
            if (SymbolEqualityComparer.Default.Equals(containingType.OriginalDefinition, componentType.OriginalDefinition))
                return true;
        }

        var topLevelType = type;
        while (topLevelType.ContainingType is not null)
            topLevelType = topLevelType.ContainingType;

        return SymbolEqualityComparer.Default.Equals(topLevelType.ContainingNamespace, componentType.ContainingNamespace);
    }

    internal static bool IsRuntimeHelperTypeCandidate(INamedTypeSymbol type)
        => type.TypeKind == TypeKind.Class &&
           !type.IsRecord &&
           (type.TypeParameters.Length == 0 || IsErasedGenericRuntimeHelperTypeCandidate(type)) &&
           !HasDirectECMAScriptSupportMarker(type);

    private static bool IsErasedGenericRuntimeHelperTypeCandidate(INamedTypeSymbol type)
    {
        if (type.TypeParameters.Length == 0)
            return true;

        if (type.BaseType is { SpecialType: not SpecialType.System_Object } ||
            type.AllInterfaces.Length > 0)
        {
            return false;
        }

        if (type.TypeParameters.Any(static parameter =>
                parameter.HasConstructorConstraint ||
                parameter.HasReferenceTypeConstraint ||
                parameter.HasUnmanagedTypeConstraint ||
                parameter.HasValueTypeConstraint ||
                parameter.ConstraintTypes.Length > 0))
        {
            return false;
        }

        foreach (var member in type.GetMembers())
        {
            if (member.IsStatic && !member.IsImplicitlyDeclared)
                return false;

            switch (member)
            {
                case IFieldSymbol field when !field.IsImplicitlyDeclared &&
                    IsRuntimeSensitiveGenericHelperTypeUsage(type, field.Type):
                case IPropertySymbol property when
                    (IsRuntimeSensitiveGenericHelperTypeUsage(type, property.Type) ||
                     property.Parameters.Any(parameter => IsRuntimeSensitiveGenericHelperTypeUsage(type, parameter.Type))):
                case IMethodSymbol method when method.MethodKind is MethodKind.Ordinary or MethodKind.Constructor &&
                    (method.TypeParameters.Length > 0 ||
                     IsRuntimeSensitiveGenericHelperTypeUsage(type, method.ReturnType) ||
                     method.Parameters.Any(parameter => IsRuntimeSensitiveGenericHelperTypeUsage(type, parameter.Type))):
                    return false;
            }
        }

        foreach (var reference in type.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is TypeDeclarationSyntax declaration &&
                ContainsRuntimeSensitiveGenericHelperTypeSyntax(type, declaration))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsRuntimeSensitiveGenericHelperTypeUsage(
        INamedTypeSymbol genericHelperType,
        ITypeSymbol? type)
    {
        if (type is null)
            return false;

        if (type is ITypeParameterSymbol typeParameter)
            return !typeParameter.ContainingSymbol.Equals(genericHelperType, SymbolEqualityComparer.Default);

        if (type is IArrayTypeSymbol arrayType)
            return IsRuntimeSensitiveGenericHelperTypeUsage(genericHelperType, arrayType.ElementType);

        if (type is INamedTypeSymbol namedType)
            return namedType.TypeArguments.Any(argument => IsRuntimeSensitiveGenericHelperTypeUsage(genericHelperType, argument));

        return false;
    }

    private static bool ContainsRuntimeSensitiveGenericHelperTypeSyntax(
        INamedTypeSymbol genericHelperType,
        TypeDeclarationSyntax declaration)
    {
        var typeParameterNames = new HashSet<string>(
            genericHelperType.TypeParameters.Select(static parameter => parameter.Name),
            StringComparer.Ordinal);
        if (typeParameterNames.Count == 0)
            return false;

        foreach (var node in declaration.DescendantNodes())
        {
            switch (node)
            {
                case TypeOfExpressionSyntax typeOf when ContainsGenericTypeParameterName(typeOf.Type, typeParameterNames):
                case SizeOfExpressionSyntax sizeOf when ContainsGenericTypeParameterName(sizeOf.Type, typeParameterNames):
                case DefaultExpressionSyntax defaultExpression when ContainsGenericTypeParameterName(defaultExpression.Type, typeParameterNames):
                case ObjectCreationExpressionSyntax objectCreation when ContainsGenericTypeParameterName(objectCreation.Type, typeParameterNames):
                case IsPatternExpressionSyntax isPattern when ContainsGenericTypeParameterPattern(isPattern.Pattern, typeParameterNames):
                    return true;
            }
        }

        return false;
    }

    private static bool ContainsGenericTypeParameterName(TypeSyntax type, HashSet<string> typeParameterNames)
        => type.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
            .Any(identifier => typeParameterNames.Contains(identifier.Identifier.ValueText));

    private static bool ContainsGenericTypeParameterPattern(PatternSyntax pattern, HashSet<string> typeParameterNames)
        => pattern.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
            .Any(identifier => typeParameterNames.Contains(identifier.Identifier.ValueText));

    private void ThrowIfUnsupportedSameArtifactRuntimeHelperType(IOperation? operation, ITypeSymbol? typeSymbol, string usage)
    {
        if (typeSymbol is not INamedTypeSymbol namedType ||
            namedType.TypeKind != TypeKind.Class ||
            namedType.IsRecord ||
            !IsSameArtifactModuleType(_snapshot.ComponentSymbol, namedType) ||
            IsRuntimeHelperTypeCandidate(namedType))
        {
            return;
        }

        if (HasDirectECMAScriptSupportMarker(namedType) &&
            !RazorVueComponentTypeCarrierHelper.IsVueComponentType(_snapshot.Compilation, namedType))
        {
            return;
        }

        var reason = namedType.TypeParameters.Length > 0
            ? "generic helper classes require erased value-only usage with no static generic state or runtime type-parameter semantics"
            : namedType.IsRecord
                ? "record helper types lower structurally and do not produce runtime class declarations"
                : HasDirectECMAScriptSupportMarker(namedType)
                    ? "ECMAScript/RazorVue component types are not same-artifact runtime helper classes"
                    : "the type is not eligible for same-artifact runtime helper class lowering";

        throw CreateUnsupportedSetupHelperTypeException(namedType, operation, usage, reason);
    }

    private static bool HasDirectECMAScriptSupportMarker(INamedTypeSymbol type)
        => type.OriginalDefinition.GetAttributes().Any(static attribute =>
            Util.IsECMAScriptSupportMarkerAttribute(attribute.AttributeClass));

    private static bool ShouldDeclareMethod(IMethodSymbol method)
        => method.MethodKind == MethodKind.Ordinary &&
           !method.IsAsync &&
           method.DeclaringSyntaxReferences.Length > 0;

    private static HashSet<string> BuildLocalNames(INamedTypeSymbol componentSymbol)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var syntaxReference in componentSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not TypeDeclarationSyntax declaration)
                continue;

            var collector = new DeclaredNameCollector();
            collector.Visit(declaration);
            names.UnionWith(collector.Names);
        }

        return names;
    }

    private static string ChooseDeclaredName(
        ISymbol symbol,
        HashSet<string> usedNames,
        HashSet<string> localNames,
        string preferredName)
    {
        if (!string.IsNullOrWhiteSpace(preferredName) &&
            !localNames.Contains(preferredName) &&
            usedNames.Add(preferredName))
        {
            return preferredName;
        }

        var sourceName = symbol.Name;
        if (!string.IsNullOrWhiteSpace(sourceName) &&
            !localNames.Contains(sourceName) &&
            usedNames.Add(sourceName))
        {
            return sourceName;
        }

        var display = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        var alias = "rv$" + Format.HashName(display).TrimStart('_');
        var suffix = 0;
        while (localNames.Contains(alias) || !usedNames.Add(alias))
        {
            suffix++;
            alias = "rv$" + Format.HashName(display).TrimStart('_') + "$" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return alias;
    }

    private static int GetSourceOrder(ISymbol symbol)
        => symbol.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceSpan.Start ?? int.MaxValue;

    private static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return value.ToLowerInvariant();

        if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
            return value;

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    private static RazorVueCompilationIssueException CreateUnsupportedSetupHelperTypeException(INamedTypeSymbol type)
        => CreateUnsupportedSetupHelperTypeException(
            type,
            operation: null,
            usage: "helper type lowering",
            reason: "the type could not be converted to a runtime helper class");

    private static RazorVueCompilationIssueException CreateUnsupportedSetupHelperTypeException(
        INamedTypeSymbol type,
        IOperation? operation,
        string usage,
        string reason)
    {
        var originLocation = operation?.Syntax.GetLocation() ?? type.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedSetupLogicLowering,
            RazorVueIssueSeverity.Error,
            $"RazorVue setup lowering does not support helper type '{type.ToDisplayString()}' for {usage} in the same artifact module: {reason}.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, type.ToDisplayString(), origin);
    }

    private sealed class HelperDiscoveryCompilerHost(RazorVueCompilerModuleContext context) : SemanticWalkerHost
    {
        public override void ObserveTypeReference(ITypeSymbol type, SenseArgument argument)
        {
            _ = argument;
            context.RecordTypeReference(type);
        }

        public override Expression? RewriteObjectCreationPreorder(IObjectCreationOperation operation, SenseArgument argument)
        {
            _ = argument;
            context.RecordObjectCreation(operation);
            return null;
        }
    }

    private sealed class DeclaredNameCollector : Microsoft.CodeAnalysis.CSharp.CSharpSyntaxWalker
    {
        public HashSet<string> Names { get; } = new(StringComparer.Ordinal);

        public override void VisitParameter(ParameterSyntax node)
        {
            Add(node.Identifier);
            base.VisitParameter(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            Add(node.Identifier);
            base.VisitVariableDeclarator(node);
        }

        public override void VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
        {
            Add(node.Identifier);
            base.VisitSingleVariableDesignation(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Add(node.Identifier);
            base.VisitForEachStatement(node);
        }

        public override void VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            Add(node.Identifier);
            base.VisitCatchDeclaration(node);
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            Add(node.Identifier);
            base.VisitLocalFunctionStatement(node);
        }

        private void Add(SyntaxToken identifier)
        {
            if (!identifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.None) &&
                !string.IsNullOrWhiteSpace(identifier.ValueText))
            {
                Names.Add(identifier.ValueText);
            }
        }
    }
}
