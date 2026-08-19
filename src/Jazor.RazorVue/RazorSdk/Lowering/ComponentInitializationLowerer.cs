using System.Collections.Immutable;
using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Lowers the accepted component constructor chain into setup-instance statements.
/// C# bodies remain entirely compiler-owned; this type only supplies the Vue component
/// host projection and keeps its imports compatible with the ordinary module pass.
/// 构造函数不是普通 module member：它必须在 state 建立后按 base-to-derived 顺序执行。
/// </summary>
internal static class ComponentInitializationLowerer
{
    public static ComponentInitializationBuildResult Build(
        Compilation compilation,
        MemberClosure closure,
        IReadOnlyDictionary<ISymbol, string> declaredNames,
        IEnumerable<ImportDeclaration> existingImports,
        IEnumerable<string> reservedImportNames,
        CancellationToken cancellationToken)
    {
        if (!closure.InitializationPlan.HasExplicitConstructors)
            return ComponentInitializationBuildResult.Empty;

        var importBindings = new Dictionary<string, string>(StringComparer.Ordinal);
        var importLocalBindings = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var import in existingImports)
            AddExistingImportBindings(import, importBindings, importLocalBindings);

        var reservedNames = new HashSet<string>(reservedImportNames, StringComparer.Ordinal);
        reservedNames.UnionWith(declaredNames.Values);
        reservedNames.UnionWith(importLocalBindings.Keys);
        var importContext = new SenseArgument(Sense.FunctionBody, UseImportAliases: true)
            .WithImportContext(
                importBindings,
                importLocalBindings,
                reservedNames,
                currentModuleImportPath: null,
                currentModuleBindings: new HashSet<string>(StringComparer.Ordinal));
        var phases = ImmutableArray.CreateBuilder<ComponentInitializationPhaseBuild>(
            closure.InitializationPlan.Phases.Length);

        foreach (var phase in closure.InitializationPlan.Phases)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var statement = phase.Constructor is null
                ? null
                : LowerConstructorBody(
                    compilation,
                    closure,
                    phase.Constructor,
                    declaredNames,
                    importContext.WithNewScope(),
                    cancellationToken);
            phases.Add(new ComponentInitializationPhaseBuild(phase.ComponentType, statement));
        }

        var imports = ImmutableArray.CreateBuilder<ImportDeclaration>();
        foreach (var pair in importContext.FlushImportSpecifiers())
            imports.AddRange(ImportDeclarationFactory.Create(pair.Key, pair.Value));

        return new ComponentInitializationBuildResult(
            imports.ToImmutable(),
            phases.ToImmutable());
    }

    private static Statement LowerConstructorBody(
        Compilation compilation,
        MemberClosure closure,
        IMethodSymbol constructor,
        IReadOnlyDictionary<ISymbol, string> declaredNames,
        SenseArgument argument,
        CancellationToken cancellationToken)
    {
        var walker = new SemanticWalker(
            closure.ComponentSymbol,
            declaredNames,
            cancellationToken,
            RuntimeClassPrivateStorage.ProxySafeMangledProperties)
        {
            Host = new VueSemanticWalkerHost(
                closure.ComponentSymbol,
                parameterRuntimeNames: LibraryComponentConventions.BuildParameterRuntimeNameMap(closure.ComponentSymbol),
                memberRuntimeNames: declaredNames)
        };
        var body = GetConstructorFunctionBody(compilation, constructor, walker, argument, cancellationToken);

        // A constructor return exits only that constructor, not the surrounding setup factory.
        // Wrapping the lowered body in an IIFE keeps this C# boundary intact without inventing
        // a separate component object or a second runtime dispatch protocol.
        // constructor 内的 return 只能结束该阶段，不能提前 return setup factory。
        return new NonSpecialExpressionStatement(new CallExpression(
            new ArrowFunctionExpression(
                NodeList.Empty<Node>(),
                body,
                expression: false,
                async: false),
            NodeList.Empty<Expression>(),
            optional: false));
    }

    private static FunctionBody GetConstructorFunctionBody(
        Compilation compilation,
        IMethodSymbol constructor,
        SemanticWalker walker,
        SenseArgument argument,
        CancellationToken cancellationToken)
    {
        foreach (var reference in constructor.DeclaringSyntaxReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Constructor symbols expose only constructor declarations through Roslyn.
            var declaration = (ConstructorDeclarationSyntax)reference.GetSyntax(cancellationToken);

            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
            if (declaration.Body is not null)
            {
                var operation = semanticModel.GetOperation(declaration.Body, cancellationToken)
                    ?? throw new InvalidOperationException(
                        "RazorVue could not bind the accepted source component constructor body.");
                return MaterializeFunctionBody(walker.Visit(operation, argument)
                    ?? throw new InvalidOperationException(
                        "RazorVue could not lower the accepted source component constructor body."), argument);
            }

            if (declaration.ExpressionBody is not null)
            {
                var operation = semanticModel.GetOperation(declaration.ExpressionBody.Expression, cancellationToken)
                    ?? throw new InvalidOperationException(
                        "RazorVue could not bind the accepted expression-bodied component constructor.");
                return MaterializeFunctionBody(walker.Visit(operation, argument)
                    ?? throw new InvalidOperationException(
                        "RazorVue could not lower the accepted expression-bodied component constructor."), argument);
            }
        }

        throw new InvalidOperationException(
            "RazorVue accepted a source component constructor without a lowerable source body.");
    }

    private static FunctionBody MaterializeFunctionBody(Node visited, SenseArgument argument)
    {
        if (visited is FunctionBody functionBody)
            return functionBody;

        var statements = new List<Statement>();
        if (argument.HasVarDeclarator)
        {
            // HasVarDeclarator is the non-empty invariant for this scope, so flushing must
            // materialize one declaration. Avoid a second impossible count check on the path.
            var declarators = argument.FlushVarDeclarator();
            statements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
        }

        switch (visited)
        {
            case NestedBlockStatement block:
                statements.AddRange(block.Body);
                break;
            case Statement statement:
                statements.Add(statement);
                break;
            case Expression expression:
                statements.Add(new NonSpecialExpressionStatement(expression));
                break;
            default:
                throw new InvalidOperationException(
                    "RazorVue constructor lowering received unsupported compiler body node '" +
                    visited.Type + "'.");
        }

        return new FunctionBody(NodeList.From(statements), strict: true);
    }

    private static void AddExistingImportBindings(
        ImportDeclaration declaration,
        Dictionary<string, string> importBindings,
        Dictionary<string, string> importLocalBindings)
    {
        foreach (var specifier in declaration.Specifiers)
        {
            var importedName = specifier switch
            {
                ImportDefaultSpecifier => "default",
                ImportNamespaceSpecifier => "*",
                ImportSpecifier { Imported: Identifier identifier } => identifier.Name,
                ImportSpecifier { Imported: StringLiteral literal } => literal.Value,
                _ => throw new NotSupportedException(
                    "RazorVue component constructor lowering encountered an unsupported import specifier '" +
                    specifier.Type + "'.")
            };
            var key = declaration.Source.Value + "\0" + importedName;
            if (importBindings.ContainsKey(key))
                continue;

            importBindings.Add(key, specifier.Local.Name);
            if (!importLocalBindings.ContainsKey(specifier.Local.Name))
                importLocalBindings.Add(specifier.Local.Name, key);
        }
    }
}

/// <summary>Constructor imports and per-type setup statements awaiting Vue framing.</summary>
internal sealed record ComponentInitializationBuildResult(
    ImmutableArray<ImportDeclaration> ImportDeclarations,
    ImmutableArray<ComponentInitializationPhaseBuild> Phases)
{
    public static ComponentInitializationBuildResult Empty { get; } = new(
        ImmutableArray<ImportDeclaration>.Empty,
        ImmutableArray<ComponentInitializationPhaseBuild>.Empty);

    public bool HasExplicitConstructors
        => Phases.Any(static phase => phase.ConstructorStatement is not null);
}

/// <summary>One source type's lowered constructor statement; null means initializer-only phase.</summary>
internal sealed record ComponentInitializationPhaseBuild(
    INamedTypeSymbol ComponentType,
    Statement? ConstructorStatement);
