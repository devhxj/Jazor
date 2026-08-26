// File: AstConverter.cs
// Purpose: Builds the module-level ESTree artifact from a Roslyn source type.
// 负责成员组织、导入提升和模块声明；方法体语义必须委托给 SemanticWalker，不能在此绕过 lowering。
using Acornima;
using Acornima.Ast;
using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jazor.Compiler;

/// <summary>
/// C# 到 JavaScript AST 转换器
/// 基于 Roslyn 将语义兼容的C#代码 AST 转换为 Acornima 的 AST (ESTree)
/// INamedTypeSymbol 的 TypeKind 是 TypeKind.Class
/// 当前classSymbol 对应的代码是一个public static 类，最终对应一个 Acornima es6 module
/// 内部发成员有公开的、私有的，有静态字段、静态属性、静态方法、类（非静态）、枚举（非静态）、接口、没有构造函数
/// 内部成员若是非private的，统一走具名导出；模块层不支持 default export
/// 静态字段 转换为 Acornima变量
/// 静态属性 转换为 Acornima方法（考虑get、set）
/// 静态方法 转换为 Acornima方法
/// 成员类 转换为 Acornima类
/// 成员枚举 仅作为编译期值域类型参与，不发射模块级 runtime 声明
/// 其他的如接口、委托、事件等，都忽略
/// 对于代码段，基于operationwalker 根据 IOperation 生成 Acornima AST
/// <para><b>职责边界</b></para>
/// AstConverter 负责模块级声明、成员组织和导入提升；表达式/语句语义统一交给 SemanticWalker。
/// 不要在这里通过字符串拼接绕过 walker，否则会丢失白名单裁决、稳定命名和 source origin。
/// </summary>
public class AstConverter(INamedTypeSymbol classSymbol, SemanticModel classModel, AstConverterOptions? options = null)
{
    private sealed record MemberConstructorLowering(
        IMethodSymbol Symbol,
        ImmutableArray<ArgumentSyntax> BaseArguments,
        IMethodSymbol? BaseConstructorSymbol,
        FunctionBody Body,
        string HelperName,
        ImmutableArray<PrimaryConstructorParameterStorage> PrimaryParameterStorage);

    private sealed record PrimaryConstructorParameterStorage(
        IParameterSymbol Parameter,
        string FieldName);

    private sealed record PrimaryConstructorInitializer(
        IFieldSymbol Field,
        EqualsValueClauseSyntax Syntax);

    /// <summary>
    /// Projects captured primary-constructor parameters onto compiler-owned private storage.
    /// Primary constructor syntax has no Roslyn constructor-body operation, so later instance
    /// methods need an explicit carrier after the generated JavaScript constructor returns.
    /// </summary>
    private sealed class PrimaryConstructorParameterSemanticWalkerHost(
        IReadOnlyDictionary<string, string> storage,
        RuntimeClassPrivateStorage privateStorage) : SemanticWalkerHost
    {
        public override Expression? RewriteParameterReference(
            IParameterReferenceOperation operation,
            SenseArgument argument)
            => storage.TryGetValue(
                    operation.Parameter.OriginalDefinition.ToDisplayString(Format.NameFormat),
                    out var fieldName)
                ? new MemberExpression(
                    new ThisExpression(),
                    CreatePrivateStorageKey(fieldName),
                    computed: false,
                    optional: false)
                : null;

        private Expression CreatePrivateStorageKey(string fieldName)
            => privateStorage == RuntimeClassPrivateStorage.ProxySafeMangledProperties
                ? new Identifier(RuntimeClassPrivateStorageNames.GetSyntheticStorageName(privateStorage, fieldName))
                : new PrivateIdentifier(fieldName);
    }

    private readonly INamedTypeSymbol _classSymbol = classSymbol;
    private readonly SemanticModel _classModel = classModel;
    private readonly AstConverterOptions _options = options ?? AstConverterOptions.Default;
    private readonly AstConverterModulePolicy _modulePolicy = options?.ModulePolicy ?? AstConverterModulePolicy.Default;
    private readonly Dictionary<string, List<ImportDeclarationSpecifier>> _imports = [];
    private readonly Dictionary<string, string> _importBindings = [];
    private readonly Dictionary<string, string> _importLocalBindings = [];
    private readonly ModuleNamePlan _moduleNamePlan = BuildModuleNamePlan(
        classSymbol,
        options?.MemberFilter,
        options?.ModulePolicy ?? AstConverterModulePolicy.Default,
        options?.Profile ?? AstConverterProfile.Standard);
    private readonly IReadOnlyDictionary<ISymbol, string>? _declaredNameOverrides = options?.DeclaredNames;
    private readonly SemanticWalkerHost? _semanticWalkerHost = options?.Host;
    private readonly string? _currentModuleImportPath = Util.GetECMAScriptModuleImportPath(classSymbol);
    private HashSet<string>? _moduleDeclaredBindings;

    private HashSet<string> ModuleLocalNames => _moduleNamePlan.LocalNames;

    private IReadOnlyDictionary<ISymbol, string> ModuleDeclaredNames => _declaredNameOverrides ?? _moduleNamePlan.DeclaredNames;

    private HashSet<string> ReservedImportNames => _moduleNamePlan.ReservedImportNames;

    private HashSet<string> ModuleDeclaredBindings
        => _moduleDeclaredBindings ??= new HashSet<string>(ModuleDeclaredNames.Values, System.StringComparer.Ordinal);

    /// <summary>
    /// 将C# 14 ClassDeclarationSyntax 转换为Acornima.Ast.Module(es6+ module)
    /// </summary>
    /// <returns></returns>
    public Task<Module?> Convert() => Convert(CancellationToken.None);

    public ClassDeclaration ConvertRuntimeClass(INamedTypeSymbol symbol)
        => ConvertRuntimeClass(symbol, CancellationToken.None);

    public ClassDeclaration ConvertRuntimeClass(INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        if (symbol is null)
            throw new ArgumentNullException(nameof(symbol));

        if (symbol.TypeKind != TypeKind.Class || symbol.IsRecord)
            throw new NotSupportedException($"Jazor runtime class conversion does not support {symbol.Kind}:{symbol.Name}.");

        return ConvertMemberClass(symbol, GetSupportedRuntimeClassBaseType(symbol), cancellationToken);
    }

    public ImmutableArray<ImportDeclaration> FlushImportDeclarations(IReadOnlyList<Statement> members)
        => BuildImportDeclarations(members).ToImmutableArray();

    public async Task<Module?> Convert(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 检查是否为 public 顶层类型
        if (!IsAllowedTopLevelAccessibility(_classSymbol.DeclaredAccessibility))
            throw new NotSupportedException($"Jazor module class '{_classSymbol.Name}' must be public to be converted.");

        if (_classSymbol.ContainingType != null)
            throw new NotSupportedException($"Nested class '{_classSymbol.Name}' must be flattened before conversion.");

        ValidateModuleExportPolicy();

        var members = new List<Statement>();
        var emittedMemberClasses = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var member in EnumerateModuleMembersForConversion())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!ShouldIncludeModuleMember(member))
                continue;

            switch (member)
            {
                case IFieldSymbol field:
                    await ConvertModuleField(members, field, cancellationToken);
                    break;
                case IPropertySymbol:
                    break;                    
                // Property被Field和Method代替了
                case IMethodSymbol func:
                    await ConvertModuleMethod(members, func, cancellationToken);
                    break;
                case INamedTypeSymbol @class when IsRuntimeMemberClass(@class):
                    AppendModuleClass(members, @class, emittedMemberClasses, cancellationToken);
                    break;
                case INamedTypeSymbol recordType when recordType.IsRecord:
                    // record 统一走 structural lowering，不发射模块级 runtime 声明。
                    break;
                case INamedTypeSymbol @enum when @enum.TypeKind == TypeKind.Enum:
                    // enum 在模块层走“声明擦除 + 使用点常量化”路线：
                    // 定义只存在于编译期，运行时不生成独立声明对象。
                    break;
                case INamedTypeSymbol @interface when @interface.TypeKind == TypeKind.Interface:
                    // interface 是契约，不是运行时对象。
                    // 模块层仅保留其编译期约束，不发射 JS 声明。
                    break;
                default:
                    throw new NotSupportedException($"Jazor module class does not support {member.Kind}:{member.Name}.");
            }
        }

        var statements = NodeList.From(BuildImportDeclarations(members).Concat(members));
        return statements.Count > 0
            ? new Module(statements)
            : null;
    }

    private void AppendModuleClass(
        List<Statement> members,
        INamedTypeSymbol symbol,
        HashSet<INamedTypeSymbol> emittedMemberClasses,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!emittedMemberClasses.Add(symbol))
            return;

        var baseType = GetSupportedMemberBaseType(symbol);
        if (baseType is not null)
            AppendModuleClass(members, baseType, emittedMemberClasses, cancellationToken);

        members.AddRange(ConvertModuleClass(symbol, baseType, cancellationToken));
    }

    private IEnumerable<ImportDeclaration> BuildImportDeclarations(IReadOnlyList<Statement> members)
    {
        // Collection happens while each member is lowered, but import declarations belong at the
        // module header. Retain only bindings actually referenced by the final AST: some host
        // rewrites claim a mapping and later replace its original expression entirely.
        // 先收集、后提升能让 walker 保持表达式语义；这里依据最终 AST 再过滤死导入。
        var referencedIdentifiers = CollectReferencedIdentifiers(members);
        foreach (var pair in _imports.OrderBy(static pair => pair.Key, System.StringComparer.Ordinal))
        {
            var uniqueSpecifiers = pair.Value
                .Where(specifier => ShouldRetainImportSpecifier(specifier, referencedIdentifiers))
                .ToArray();

            if (uniqueSpecifiers.Length > 0 &&
                string.Equals(
                    pair.Key,
                    _currentModuleImportPath,
                    System.StringComparison.Ordinal))
            {
                var importedNames = string.Join(
                    ", ",
                    uniqueSpecifiers.Select(GetImportedSpecifierName));
                throw new NotSupportedException(
                    $"Import '{importedNames}' resolves to the current module '{_currentModuleImportPath}', " +
                    "but that module does not declare a matching local binding.");
            }

            foreach (var declaration in ImportDeclarationFactory.Create(
                         pair.Key,
                         uniqueSpecifiers))
            {
                yield return declaration;
            }
        }
    }

    private static HashSet<string> CollectReferencedIdentifiers(IReadOnlyList<Statement> members)
        => AstReferenceAnalysis.CollectIdentifiers(members);

    private static bool ShouldRetainImportSpecifier(
        ImportDeclarationSpecifier specifier,
        HashSet<string> referencedIdentifiers)
        => referencedIdentifiers.Contains(specifier.Local.Name);

    private static string GetImportedSpecifierName(ImportDeclarationSpecifier specifier)
        => specifier switch
        {
            ImportSpecifier named when named.Imported is Identifier identifier => identifier.Name,
            ImportSpecifier named when named.Imported is StringLiteral literal => literal.Value,
            ImportDefaultSpecifier => "default",
            ImportNamespaceSpecifier => "*",
            _ => specifier.Local.Name
        };

    private void MergeImports(in SenseArgument argument)
    {
        // Do not normalize per member. The module owns dedupe and ordering after every member has
        // contributed, which keeps one import alias stable across fields, methods, and classes.
        // 不在此处排序/去重，避免局部转换顺序影响模块级 import 的最终形状。
        foreach (var pair in argument.FlushImportSpecifiers())
        {
            if (_imports.TryGetValue(pair.Key, out var list))
            {
                foreach (var specifier in pair.Value)
                    list.Add(specifier);
            }
            else
                _imports.Add(pair.Key, [.. pair.Value]);
        }
    }

    private SemanticWalker CreateSemanticWalker(
        CancellationToken cancellationToken,
        SemanticWalkerHost? host = null,
        INamedTypeSymbol? runtimeType = null,
        bool rewritePrimaryConstructorParameters = true)
    {
        SemanticWalkerHost? primaryConstructorHost = null;
        if (rewritePrimaryConstructorParameters && runtimeType is not null)
        {
            var parameterStorage = GetPrimaryConstructorParameterStorage(runtimeType);
            if (parameterStorage.Length > 0)
            {
                primaryConstructorHost = new PrimaryConstructorParameterSemanticWalkerHost(
                    parameterStorage.ToDictionary(
                        static storage => storage.Parameter.OriginalDefinition.ToDisplayString(Format.NameFormat),
                        static storage => storage.FieldName,
                        StringComparer.Ordinal),
                    _options.RuntimeClassPrivateStorage);
            }
        }

        var effectiveHost = CombineSemanticWalkerHosts(primaryConstructorHost, host, _semanticWalkerHost);
        return new SemanticWalker(
            _classSymbol,
            ModuleDeclaredNames,
            cancellationToken,
            _options.RuntimeClassPrivateStorage)
        {
            Host = effectiveHost
        };
    }

    private static SemanticWalkerHost? CombineSemanticWalkerHosts(params SemanticWalkerHost?[] hosts)
    {
        var effectiveHosts = hosts
            .Where(static host => host is not null)
            .Cast<SemanticWalkerHost>()
            .ToArray();
        return effectiveHosts.Length switch
        {
            0 => null,
            1 => effectiveHosts[0],
            _ => new CompositeSemanticWalkerHost(effectiveHosts)
        };
    }

    private ImmutableArray<PrimaryConstructorParameterStorage> GetPrimaryConstructorParameterStorage(INamedTypeSymbol runtimeType)
    {
        foreach (var reference in runtimeType.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not ClassDeclarationSyntax { ParameterList: { } parameterList } declaration)
                continue;

            var semanticModel = GetSemanticModel(declaration);
            var storage = ImmutableArray.CreateBuilder<PrimaryConstructorParameterStorage>(parameterList.Parameters.Count);
            foreach (var parameter in parameterList.Parameters)
            {
                // A valid primary-constructor parameter syntax always binds to its parameter
                // symbol in the owning compilation; keep this path aligned with Roslyn's
                // declaration contract instead of silently dropping a captured parameter.
                var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter)!;

                storage.Add(new PrimaryConstructorParameterStorage(
                    parameterSymbol.OriginalDefinition,
                    "$jazorPrimary_" + Format.HashName(
                        parameterSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_')));
            }

            return storage.MoveToImmutable();
        }

        return ImmutableArray<PrimaryConstructorParameterStorage>.Empty;
    }

    /// <summary>
    /// 为模块输出阶段创建共享导入上下文。
    /// 这里刻意让整个模块共用同一份导入绑定状态，
    /// 这样同一个外部符号在不同方法、字段初始化器或成员转换过程中
    /// 都会得到同一个本地名字，不会因为访问顺序不同而抖动。
    /// </summary>
    private SenseArgument CreateImportAwareArgument(Sense sense)
        => new SenseArgument(sense, UseImportAliases: true)
            .WithImportContext(
                _importBindings,
                _importLocalBindings,
                ReservedImportNames,
                _currentModuleImportPath,
                ModuleDeclaredBindings);

    private SemanticModel GetSemanticModel(SyntaxNode syntax)
        => syntax.SyntaxTree == _classModel.SyntaxTree
            ? _classModel
            : _classModel.Compilation.GetSemanticModel(syntax.SyntaxTree);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statements"></param>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private async Task ConvertModuleField(List<Statement> statements, IFieldSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (declaration, localName) = await ConvertVariableField(symbol, cancellationToken);
        if (ShouldBePrivate(symbol.DeclaredAccessibility) ||
            !ShouldExportModuleMember(symbol))
            statements.Add(declaration);
        else if (string.Equals(localName, GetModuleNamedExportName(symbol), System.StringComparison.Ordinal))
        {
            statements.Add(new ExportNamedDeclaration(
                declaration,
                NodeList.From<ExportSpecifier>([]),
                null,
                NodeList.From<ImportAttribute>([])));
        }
        else
        {
            statements.Add(declaration);
            statements.Add(CreateNamedExport(localName, GetModuleNamedExportName(symbol)));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statements"></param>
    /// <param name="symbol"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private async Task ConvertModuleMethod(List<Statement> statements, IMethodSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (symbol.AssociatedSymbol is IEventSymbol eventSymbol)
            throw new NotSupportedException($"Jazor module class does not support Event:{eventSymbol.Name}.");

        if (symbol.MethodKind == MethodKind.SharedConstructor)
        {
            if (symbol.IsImplicitlyDeclared)
                return;

            throw new NotSupportedException($"Jazor module class does not support static constructor {symbol.Name}.");
        }

        if (Util.IsBodylessInitAccessor(symbol))
            return;

        var parameters = new List<Node>();
		var refParas = new List<Expression>();
		var hasReturn = !symbol.ReturnsVoid;
        foreach (var item in symbol.Parameters)
        {
            var expr = ConvertParameter(item);
            parameters.Add(expr);
            if (item.RefKind is RefKind.Out or RefKind.Ref)
                refParas.Add(expr);
        }

        // 获取方法体
        BlockSyntax? blockSyntax = null;
        ExpressionSyntax? expressionSyntax = null;
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var syntax = await reference.GetSyntaxAsync(cancellationToken);
            if (syntax is MethodDeclarationSyntax methodDecl)
            {
                if (methodDecl.Body is not null)
                {
                    blockSyntax = methodDecl.Body;
                    break;
                }
                else if (methodDecl.ExpressionBody is not null)
                {
                    expressionSyntax = methodDecl.ExpressionBody.Expression;
                    break;
                }
            }
            else if (syntax is AccessorDeclarationSyntax accessorDecl)
            {
                if (accessorDecl.Body is not null)
                {
                    blockSyntax = accessorDecl.Body;
                    break;
                }
                else if (accessorDecl.ExpressionBody is not null)
                {
                    expressionSyntax = accessorDecl.ExpressionBody.Expression;
                    break;
                }
            }
            else if (syntax is ArrowExpressionClauseSyntax arrowExpr)
            {
                expressionSyntax = arrowExpr.Expression;
                break;
            }
        }

        FunctionBody? body = null;
        IOperation? functionOperation = null;
        if (blockSyntax is null && expressionSyntax is null &&
            (symbol.MethodKind == MethodKind.PropertyGet || symbol.MethodKind == MethodKind.PropertySet))
        {
            // 自动属性
            var displayString = symbol.AssociatedSymbol!.OriginalDefinition.ToDisplayString(Format.NameFormat);
            var fieldId = new Identifier(Format.HashName(displayString));
            if (symbol.MethodKind == MethodKind.PropertyGet)
            {
                var returnStmt = new ReturnStatement(fieldId);
                body = new FunctionBody(strict: true, body: NodeList.From<Statement>(returnStmt));
            }
            else
            {
                var value = new Identifier("value");
                var assignExpr = new AssignmentExpression(Operator.Assignment, fieldId, value);
                var assignStmt = new NonSpecialExpressionStatement(assignExpr);
                body = new FunctionBody(strict: true, body: NodeList.From<Statement>(assignStmt));
            }
        }
        else if (blockSyntax is not null)
        {
            // The source compilation has succeeded, and Roslyn always binds a method/accessor
            // block to IBlockOperation. This is an SDK semantic contract, not a best-effort probe.
            var operation = GetSemanticModel(blockSyntax).GetOperation(blockSyntax)!;
            functionOperation = operation;
            var walker = CreateSemanticWalker(cancellationToken);
            var argument = CreateImportAwareArgument(Sense.FunctionBody);
            body = MaterializeFunctionBody(walker.Visit(operation, argument)!, argument, symbol.ReturnsVoid);
            MergeImports(argument);
        }
        else if (expressionSyntax is not null)
        {
            // Expression-bodied members are equally guaranteed to have a semantic operation once
            // the owning source tree passed C# compilation.
            var operation = GetSemanticModel(expressionSyntax).GetOperation(expressionSyntax)!;
            functionOperation = operation;
            var walker = CreateSemanticWalker(cancellationToken);
            // Keep the historic expression-root scope key stable. The marker only distinguishes
            // a root `=> throw ...` from a nested throw expression during materialization.
            var argument = CreateImportAwareArgument(Sense.ExpressionBody);
            var visited = walker.Visit(operation, argument)!;
            MergeImports(argument);
            body = MaterializeFunctionBody(visited, argument, symbol.ReturnsVoid);
        }
        if (body is null)
            throw new NotSupportedException($"Jazor cannot convert method {symbol.Name}: no function body could be generated from its operation.");

        if (refParas.Count > 0)
            body = RefOutReturnProtocol.Apply(body, refParas, hasReturn);

        var localName = GetModuleDeclaredName(symbol);
        var identifier = new Identifier(localName);
        // todo:分析使用ArrowFunctionExpression的可能性
        var declaration = new FunctionDeclaration(
            id: identifier,
            parameters: NodeList.From(parameters),
            body: body,
            generator: functionOperation is not null && OperationTree.ContainsYieldOperation(functionOperation),
            async: symbol.IsAsync);

        if (ShouldBePrivate(symbol.DeclaredAccessibility) ||
            !ShouldExportModuleMember(symbol))
            AddModuleMethodDeclaration(statements, declaration, symbol);
        else if (string.Equals(localName, GetModuleNamedExportName(symbol), System.StringComparison.Ordinal))
        {
            AddModuleMethodDeclaration(statements, new ExportNamedDeclaration(
                declaration,
                NodeList.From<ExportSpecifier>([]),
                null,
                NodeList.From<ImportAttribute>([])), symbol);
        }
        else
        {
            AddModuleMethodDeclaration(statements, declaration, symbol);
            statements.Add(CreateNamedExport(localName, GetModuleNamedExportName(symbol)));
        }
    }

    private void AddModuleMethodDeclaration(
        List<Statement> statements,
        Statement declaration,
        IMethodSymbol symbol)
    {
        if (TryGetClrImportMemberName(symbol, out var memberName))
            statements.Add(new BlockComment($"jazor:clr-member {memberName}"));

        statements.Add(declaration);
    }

    private bool TryGetClrImportMemberName(IMethodSymbol symbol, out string memberName)
    {
        if (_options.Profile != AstConverterProfile.ClrRuntime)
        {
            memberName = string.Empty;
            return false;
        }

        return Util.TryGetJazorImportMapping(symbol, out memberName, out _);
    }

    private static bool TryGetClrImportRuntimeName(ISymbol symbol, out string runtimeName)
        => Util.TryGetJazorImportRuntimeName(symbol, out runtimeName);

    private async Task<(VariableDeclaration Declaration, string LocalName)> ConvertVariableField(IFieldSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Expression? init = null;
        if (symbol.HasConstantValue)
            init = CreateEqualsValueClauseSyntaxLiteral(symbol.Type.SpecialType, symbol.ConstantValue);
        else
            foreach (var item in symbol.DeclaringSyntaxReferences)
            {
                // Source fields bind directly to VariableDeclaratorSyntax. Implicit property
                // backing fields have no declaration here and continue to the property route.
                var syntax = (VariableDeclaratorSyntax)await item.GetSyntaxAsync(cancellationToken);
                if (syntax.Initializer is not null)
                {
                    init = CreateEqualsValueClauseSyntaxLiteral(syntax.Initializer, cancellationToken);
                    break;
                }
            }

        string name;
        bool isPropertyInitOnly = false;
        if (symbol.AssociatedSymbol is IPropertySymbol property)
        {
            isPropertyInitOnly = property.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<PropertyDeclarationSyntax>()
                .Any(static p => p.AccessorList?.Accessors.Any(a => a.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InitAccessorDeclaration)) == true);
            if (symbol.IsImplicitlyDeclared)
                name = Format.HashName(symbol.AssociatedSymbol!.OriginalDefinition.ToDisplayString(Format.NameFormat));
            else
                name = Util.GetConfigOrSymbolName(symbol);

            // C#只有自动实现的属性或使用 ‘field’ 关键字的属性才能具有初始值设定项。
            // 要查找对应的属性，是否有初始化赋值
            if (init is null)
            {
                foreach (var item in property.DeclaringSyntaxReferences)
                {
                    var syntax = (PropertyDeclarationSyntax)await item.GetSyntaxAsync(cancellationToken);
                    if (syntax.Initializer is not null)
                    {
                        init = CreateEqualsValueClauseSyntaxLiteral(syntax.Initializer, cancellationToken);
                        break;
                    }
                }
            }
        }
        else
            name = Util.GetConfigOrSymbolName(symbol);

        name = GetModuleDeclaredName(symbol);

        var identifier = new Identifier(name);
        var kind = symbol.IsConst || isPropertyInitOnly
            ? VariableDeclarationKind.Const
            : VariableDeclarationKind.Let;
        var declarator = new VariableDeclarator(identifier, init);
        var declaration = new VariableDeclaration(kind, NodeList.From([declarator]));

        return (declaration, name);
    }

    private static ExportNamedDeclaration CreateNamedExport(string localName, string exportName)
        => new ExportNamedDeclaration(
            null!,
            NodeList.From([
                new ExportSpecifier(
                    new Identifier(localName),
                    JavaScriptAstFactory.CreateModuleExportName(exportName))
            ]),
            null,
            NodeList.From<ImportAttribute>([]));

    private PropertyDefinition ConvertMemberField(
        IFieldSymbol symbol,
        bool suppressInitializer = false)
    {
        var name = GetMemberFieldDeclaredName(symbol);
        var init = suppressInitializer
            ? GetMemberFieldDefaultValue(symbol)
            : GetMemberFieldInitializer(symbol);

        Expression identifier = ShouldBePrivate(symbol.DeclaredAccessibility)
            ? CreatePrivateStorageKey(symbol, name)
            : new Identifier(name);
        return new PropertyDefinition(
            key: identifier,
            value: init,
            computed: false,
            isStatic: symbol.IsStatic,
            decorators: NodeList.Empty<Decorator>()
        );
    }

    private Expression? GetMemberFieldDefaultValue(IFieldSymbol symbol)
    {
        if (symbol.HasConstantValue)
            return CreateEqualsValueClauseSyntaxLiteral(symbol.Type.SpecialType, symbol.ConstantValue);

        // Field declarations and auto-property backing fields share the compiler-owned default
        // lowering. Do not synthesize a scalar fallback here: CLR carriers such as DateTime,
        // long, and tuple values need SemanticWalker to preserve imports and representation.
        var walker = CreateSemanticWalker(CancellationToken.None);
        var argument = CreateImportAwareArgument(Sense.Any);
        var defaultValue = walker.BuildImplicitMemberFieldDefaultValue(symbol, argument);
        MergeImports(argument);
        return defaultValue;
    }

    private Expression? GetMemberFieldInitializer(IFieldSymbol symbol)
    {
        if (symbol.HasConstantValue)
            return CreateEqualsValueClauseSyntaxLiteral(symbol.Type.SpecialType, symbol.ConstantValue);

        foreach (var item in symbol.DeclaringSyntaxReferences)
        {
            // Explicit source fields bind directly to VariableDeclaratorSyntax. Implicit backing
            // fields have no declaration here and are handled through their associated property.
            var syntax = (VariableDeclaratorSyntax)item.GetSyntax();
            if (syntax.Initializer is not null)
                return CreateEqualsValueClauseSyntaxLiteral(syntax.Initializer);
        }

        // Auto-properties materialize their private storage through the property conversion path.
        // Their implicit backing field has no declaration syntax of its own, so its initializer
        // remains owned by the associated source property.
        if (symbol.AssociatedSymbol is IPropertySymbol property)
        {
            foreach (var item in property.DeclaringSyntaxReferences)
            {
                var syntax = (PropertyDeclarationSyntax)item.GetSyntax();
                if (syntax.Initializer is not null)
                    return CreateEqualsValueClauseSyntaxLiteral(syntax.Initializer);
            }
        }

        return GetMemberFieldDefaultValue(symbol);
    }

    private Expression GetImplicitMemberFieldDefaultValue(IParameterSymbol parameter)
    {
        // Captured primary-constructor parameters become private JS class fields. Their initial
        // value is usually overwritten immediately, but it must still use CLR default semantics
        // because class-field initialization is observable in the generated runtime shape.
        var walker = CreateSemanticWalker(CancellationToken.None);
        var argument = CreateImportAwareArgument(Sense.Any);
        var defaultValue = walker.BuildImplicitPrimaryConstructorParameterDefaultValue(parameter, argument);
        MergeImports(argument);
        return defaultValue;
    }

    private FunctionBody ConvertMemberOperationToFunctionBody(
        IOperation operation,
        bool returnsVoid,
        CancellationToken cancellationToken,
        SemanticWalkerHost? host = null,
        INamedTypeSymbol? runtimeType = null,
        bool rewritePrimaryConstructorParameters = true,
        bool isExpressionBody = false)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var walker = CreateSemanticWalker(
            cancellationToken,
            host,
            runtimeType,
            rewritePrimaryConstructorParameters);
        var argument = CreateImportAwareArgument(isExpressionBody ? Sense.ExpressionBody : Sense.Any);
        var visited = walker.Visit(operation, argument)!;
        MergeImports(argument);

        return MaterializeFunctionBody(visited, argument, returnsVoid);
    }

    private MethodDefinition ConvertMemberMethod(IMethodSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (symbol.IsAbstract)
            throw new NotSupportedException($"Jazor member class does not support abstract method {symbol.Name}.");

        IOperation operation = null!;
        var isExpressionBody = false;
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var syntax = reference.GetSyntax();
            if (syntax is MethodDeclarationSyntax methodDecl)
            {
                if (methodDecl.Body is not null)
                {
                    operation = GetSemanticModel(methodDecl.Body).GetOperation(methodDecl.Body)!;
                    break;
                }
                else if (methodDecl.ExpressionBody is not null)
                {
                    operation = GetSemanticModel(methodDecl.ExpressionBody).GetOperation(methodDecl.ExpressionBody)!;
                    isExpressionBody = true;
                    break;
                }
            }
            else if (syntax is AccessorDeclarationSyntax accessorDecl)
            {
                if (accessorDecl.Body is not null)
                {
                    operation = GetSemanticModel(accessorDecl.Body).GetOperation(accessorDecl.Body)!;
                    break;
                }
                else if (accessorDecl.ExpressionBody is not null)
                {
                    operation = GetSemanticModel(accessorDecl.ExpressionBody).GetOperation(accessorDecl.ExpressionBody)!;
                    isExpressionBody = true;
                    break;
                }
            }
            else if (syntax is ArrowExpressionClauseSyntax arrowExpr)
            {
                operation = GetSemanticModel(arrowExpr.Expression).GetOperation(arrowExpr.Expression)!;
                isExpressionBody = true;
                break;
            }
        }

        var isProperty = symbol.AssociatedSymbol?.Kind == SymbolKind.Property;
        var isGenerator = operation is not null && OperationTree.ContainsYieldOperation(operation);
        FunctionBody body;
        if (operation is not null)
        {
            body = ConvertMemberOperationToFunctionBody(
                operation,
                symbol.ReturnsVoid,
                cancellationToken,
                runtimeType: symbol.ContainingType,
                isExpressionBody: isExpressionBody);
        }
        // Body-less property accessors only map to auto-properties.
        else if (isProperty)
        {
            var backingField = GetMemberBackingFieldSymbol((IPropertySymbol)symbol.AssociatedSymbol!);
            var backName = backingField is null
                ? GetMemberBackingFieldName((IPropertySymbol)symbol.AssociatedSymbol!)
                : GetMemberFieldDeclaredName(backingField);
            var backField = CreatePrivateStorageKey(backingField, backName);

            if (symbol.MethodKind == MethodKind.PropertyGet)
                body = new FunctionBody(
                    strict: true,
                    body: NodeList.From<Statement>(
                        new ReturnStatement(
                            new MemberExpression(
                                obj: new ThisExpression(),
                                property: backField,
                                computed: false,
                                optional: false))));
            else
            {
                var value = new Identifier("value");
                body = new FunctionBody(
                        strict: true,
                        body: NodeList.From<Statement>(
                            new NonSpecialExpressionStatement(
                                new AssignmentExpression("=",
                                    new MemberExpression(
                                        obj: new ThisExpression(),
                                        property: backField,
                                        computed: false,
                                        optional: false),
                                    value))));
            }
        }
        else
            throw new NotSupportedException($"Jazor member class method {symbol.Name} requires a body.");

        var parameters = new List<Node>();
        if (symbol.Parameters.Length > 0)
        {
            foreach (var p in symbol.Parameters)
                parameters.Add(ConvertParameter(p));
        }

        var isIndexerAccessor = symbol.AssociatedSymbol is IPropertySymbol { IsIndexer: true };
        var name = isIndexerAccessor
            ? Util.GetMemberIndexerAccessorHelperName(symbol)
            : GetSymbolName(symbol);
        var key = new Identifier(name);

        var propertyKind = isProperty && !isIndexerAccessor
            ? (symbol.MethodKind == MethodKind.PropertyGet ? PropertyKind.Get : PropertyKind.Set)
            : PropertyKind.Method;

        return new MethodDefinition(
            propertyKind,
            key: key,
            value: new FunctionExpression(
                id: null,
                parameters: NodeList.From(parameters),
                body: body,
                generator: isGenerator,
                async: symbol.IsAsync),
            computed: false,
            isStatic: symbol.IsStatic,
            decorators: NodeList.Empty<Decorator>()
        );
    }


    private List<ClassProperty> ConvertMemberProperty(
        IPropertySymbol symbol,
        CancellationToken cancellationToken,
        bool suppressInitializer = false)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if ((symbol.GetMethod?.IsAbstract ?? false) || (symbol.SetMethod?.IsAbstract ?? false))
            throw new NotSupportedException($"Jazor member class does not support abstract property {symbol.Name}.");

        var properties = new List<ClassProperty>();
        // 找出BackingField
        var backName = $"<{symbol.Name}>k__BackingField";
        var backingFieldSymbol = symbol.ContainingType
                .GetMembers(backName)
                .OfType<IFieldSymbol>()
                .FirstOrDefault();
        if (backingFieldSymbol is not null)
        {
            var backingFieldDecl = ConvertMemberField(backingFieldSymbol, suppressInitializer);
            properties.Add(backingFieldDecl);
        }

        // 处理 getter
        if (symbol.GetMethod is not null)
        {
            var getFuncDecl = ConvertMemberMethod(symbol.GetMethod, cancellationToken);
            properties.Add(getFuncDecl);
        }

        // 处理 setter
        if (symbol.SetMethod is not null)
        {
            if (!Util.IsBodylessInitAccessor(symbol.SetMethod))
            {
                var setFuncDecl = ConvertMemberMethod(symbol.SetMethod, cancellationToken);
                properties.Add(setFuncDecl);
            }
        }

        return properties;
    }

    private IEnumerable<Node> ConvertMemberEvent(IEventSymbol symbol)
    {
        if (!EventLowering.IsSupportedFieldLikeInstanceEvent(symbol, out var reason))
        {
            throw new NotSupportedException(
                $"Jazor member class event '{symbol.Name}' cannot lower: {reason}");
        }

        var invokeMethod = EventLowering.GetInvokeMethod(symbol);
        yield return new PropertyDefinition(
            key: CreateSyntheticPrivateStorageKey(EventLowering.GetStorageName(symbol)),
            value: new ArrayExpression(NodeList.Empty<Expression?>()),
            computed: false,
            isStatic: false,
            decorators: NodeList.Empty<Decorator>());
        yield return CreateEventAddMethod(symbol);
        yield return CreateEventRemoveMethod(symbol);
        yield return CreateEventSnapshotMethod(symbol, invokeMethod);
    }

    private MethodDefinition CreateEventAddMethod(IEventSymbol symbol)
    {
        var handler = new Identifier("$eventHandler");
        var receiver = new Identifier("$eventReceiver");
        var append = new CallExpression(
            new MemberExpression(
                CreateEventStorageAccess(symbol),
                new Identifier("push"),
                computed: false,
                optional: false),
            NodeList.From<Expression>(
                new ArrayExpression(NodeList.From<Expression?>(handler, receiver))),
            optional: false);
        var body = new FunctionBody(
            NodeList.From<Statement>(
                new IfStatement(
                    new NonLogicalBinaryExpression(Operator.Inequality, handler, new NullLiteral("null")),
                    new NestedBlockStatement(NodeList.From<Statement>(
                        new NonSpecialExpressionStatement(append))),
                    null)),
            strict: true);

        return CreateEventProtocolMethod(
            EventLowering.GetAddMethodName(symbol),
            [handler, receiver],
            body);
    }

    private MethodDefinition CreateEventRemoveMethod(IEventSymbol symbol)
    {
        var handler = new Identifier("$eventHandler");
        var receiver = new Identifier("$eventReceiver");
        var index = new Identifier("$eventIndex");
        var entry = new Identifier("$eventEntry");
        var entryCallback = CreateEventEntryAccess(entry, 0);
        var entryReceiver = CreateEventEntryAccess(entry, 1);
        var matches = new LogicalExpression(
            Operator.LogicalAnd,
            new NonLogicalBinaryExpression(Operator.StrictEquality, entryCallback, handler),
            new NonLogicalBinaryExpression(Operator.StrictEquality, entryReceiver, receiver));
        var remove = new CallExpression(
            new MemberExpression(
                CreateEventStorageAccess(symbol),
                new Identifier("splice"),
                computed: false,
                optional: false),
            NodeList.From<Expression>(index, new NumericLiteral(1, "1")),
            optional: false);
        var loopBody = new NestedBlockStatement(NodeList.From<Statement>(
            new VariableDeclaration(
                VariableDeclarationKind.Const,
                NodeList.From(new VariableDeclarator(entry, CreateEventStorageIndexAccess(symbol, index)))),
            new IfStatement(
                matches,
                new NestedBlockStatement(NodeList.From<Statement>(
                    new NonSpecialExpressionStatement(remove),
                    new ReturnStatement(null))),
                null),
            new NonSpecialExpressionStatement(
                new UpdateExpression(Operator.Decrement, index, prefix: false))));
        var storageLength = new MemberExpression(
            CreateEventStorageAccess(symbol),
            new Identifier("length"),
            computed: false,
            optional: false);
        var body = new FunctionBody(
            NodeList.From<Statement>(
                new IfStatement(
                    new NonLogicalBinaryExpression(Operator.Equality, handler, new NullLiteral("null")),
                    new ReturnStatement(null),
                    null),
                new VariableDeclaration(
                    VariableDeclarationKind.Let,
                    NodeList.From(new VariableDeclarator(
                        index,
                        new NonLogicalBinaryExpression(
                            Operator.Subtraction,
                            storageLength,
                            new NumericLiteral(1, "1"))))),
                new WhileStatement(
                    new NonLogicalBinaryExpression(
                        Operator.GreaterThanOrEqual,
                        index,
                        new NumericLiteral(0, "0")),
                    loopBody)),
            strict: true);

        return CreateEventProtocolMethod(
            EventLowering.GetRemoveMethodName(symbol),
            [handler, receiver],
            body);
    }

    private MethodDefinition CreateEventSnapshotMethod(IEventSymbol symbol, IMethodSymbol invokeMethod)
    {
        var snapshot = new Identifier("$eventSnapshot");
        var entry = new Identifier("$eventEntry");
        var result = new Identifier("$eventResult");
        var arguments = invokeMethod.Parameters
            .Select((_, index) => new Identifier("$eventArg" + index))
            .ToArray();
        var callback = CreateEventEntryAccess(entry, 0);
        var receiver = CreateEventEntryAccess(entry, 1);
        var call = new CallExpression(
            new MemberExpression(callback, new Identifier("apply"), computed: false, optional: false),
            NodeList.From<Expression>(
                receiver,
                new ArrayExpression(NodeList.From<Expression?>(arguments.Cast<Expression?>()))),
            optional: false);
        var delegateBody = new FunctionBody(
            NodeList.From<Statement>(
                new VariableDeclaration(
                    VariableDeclarationKind.Let,
                    NodeList.From(new VariableDeclarator(result, null))),
                new ForOfStatement(
                    new VariableDeclaration(
                        VariableDeclarationKind.Const,
                        NodeList.From(new VariableDeclarator(entry, null))),
                    snapshot,
                    new NestedBlockStatement(NodeList.From<Statement>(
                        new NonSpecialExpressionStatement(
                            new AssignmentExpression(Operator.Assignment, result, call)))),
                    @await: false),
                new ReturnStatement(result)),
            strict: true);
        var snapshotDelegate = new ArrowFunctionExpression(
            NodeList.From<Node>(arguments),
            delegateBody,
            expression: false,
            async: false);
        var storageLength = new MemberExpression(
            CreateEventStorageAccess(symbol),
            new Identifier("length"),
            computed: false,
            optional: false);
        var copy = new CallExpression(
            new MemberExpression(
                CreateEventStorageAccess(symbol),
                new Identifier("slice"),
                computed: false,
                optional: false),
            NodeList.Empty<Expression>(),
            optional: false);
        var body = new FunctionBody(
            NodeList.From<Statement>(
                new IfStatement(
                    new NonLogicalBinaryExpression(
                        Operator.StrictEquality,
                        storageLength,
                        new NumericLiteral(0, "0")),
                    new ReturnStatement(new NullLiteral("null")),
                    null),
                new VariableDeclaration(
                    VariableDeclarationKind.Const,
                    NodeList.From(new VariableDeclarator(snapshot, copy))),
                new ReturnStatement(snapshotDelegate)),
            strict: true);

        return CreateEventProtocolMethod(
            EventLowering.GetSnapshotMethodName(symbol),
            [],
            body);
    }

    private static MethodDefinition CreateEventProtocolMethod(
        string name,
        IReadOnlyList<Identifier> parameters,
        FunctionBody body)
        => new(
            PropertyKind.Method,
            key: new Identifier(name),
            value: new FunctionExpression(
                id: null,
                parameters: NodeList.From<Node>(parameters),
                body: body,
                generator: false,
                async: false),
            computed: false,
            isStatic: false,
            decorators: NodeList.Empty<Decorator>());

    private MemberExpression CreateEventStorageAccess(IEventSymbol symbol)
        => new(
            new ThisExpression(),
            CreateSyntheticPrivateStorageKey(EventLowering.GetStorageName(symbol)),
            computed: false,
            optional: false);

    private MemberExpression CreateEventStorageIndexAccess(IEventSymbol symbol, Expression index)
        => new(
            CreateEventStorageAccess(symbol),
            index,
            computed: true,
            optional: false);

    private static MemberExpression CreateEventEntryAccess(Identifier entry, int index)
        => new(
            entry,
            new NumericLiteral(index, index.ToString(CultureInfo.InvariantCulture)),
            computed: true,
            optional: false);

    private MethodDefinition ConvertMemberConstructor(
        MemberConstructorLowering lowering,
        INamedTypeSymbol? baseType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var parameters = new List<Node>();
        if (lowering.Symbol.Parameters.Length > 0)
        {
            foreach (var parameterSymbol in lowering.Symbol.Parameters)
                parameters.Add(ConvertParameter(parameterSymbol));
        }

        var body = baseType is null
            ? lowering.Body
            : PrependSuperConstructorCall(lowering.Body, lowering.BaseArguments, lowering.BaseConstructorSymbol, cancellationToken);

        var writableParameters = lowering.Symbol.Parameters
            .Where(static parameter => parameter.RefKind is RefKind.Ref or RefKind.Out)
            .Select(static parameter => (Expression)new Identifier(parameter.Name))
            .ToArray();
        if (writableParameters.Length > 0)
        {
            // A JS constructor must retain its instance result. Do not reuse RefOutReturnProtocol
            // here: returning its array would replace `this` when the array is an object.
            var sink = CreateConstructorRefOutSinkIdentifier(lowering.Symbol);
            parameters.Add(sink);
            body = ConstructorRefOutSinkProtocol.Apply(body, writableParameters, sink);
        }

        return new MethodDefinition(
            PropertyKind.Method,
            key: new Identifier("constructor"),
            value: new FunctionExpression(
                id: null,
                parameters: NodeList.From(parameters),
                body: body,
                generator: false,
                async: false),
            computed: false,
            isStatic: false,
            decorators: NodeList.Empty<Decorator>());
    }

    private static Identifier CreateConstructorRefOutSinkIdentifier(IMethodSymbol constructor)
    {
        // `$jazorRefOut` is outside the C# identifier grammar, so a source parameter cannot
        // collide with this compiler-owned constructor protocol slot.
        return new Identifier("$jazorRefOut");
    }

    private MethodDefinition ConvertMemberConstructorDispatcher(
        INamedTypeSymbol containingType,
        IReadOnlyList<MemberConstructorLowering> lowerings,
        INamedTypeSymbol? baseType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var argsIdentifier = new Identifier("$args");
        var ctorIdentifier = new Identifier("$ctor");
        var statements = new List<Statement>
        {
            new VariableDeclaration(
                VariableDeclarationKind.Let,
                NodeList.From(new VariableDeclarator(argsIdentifier, new Identifier("arguments")))),
            new VariableDeclaration(
                VariableDeclarationKind.Let,
                NodeList.From(new VariableDeclarator(
                    ctorIdentifier,
                    new MemberExpression(
                        argsIdentifier,
                        new NumericLiteral(0, "0"),
                        computed: true,
                        optional: false))))
        };

        foreach (var lowering in lowerings)
        {
            var branchStatements = new List<Statement>();

            foreach (var binding in BuildConstructorDispatcherParameterBindings(lowering, argsIdentifier))
                branchStatements.Add(binding);

            if (baseType is not null)
                branchStatements.Add(CreateSuperConstructorCallStatement(lowering.BaseArguments, lowering.BaseConstructorSymbol, cancellationToken));

            branchStatements.Add(new NonSpecialExpressionStatement(
                new CallExpression(
                    new MemberExpression(
                        new ThisExpression(),
                        new Identifier(lowering.HelperName),
                        computed: false,
                        optional: false),
                    NodeList.From<Expression>(lowering.Symbol.Parameters.Select(static parameter => new Identifier(parameter.Name))),
                    optional: false)));
            branchStatements.Add(new ReturnStatement(null));

            statements.Add(new IfStatement(
                new NonLogicalBinaryExpression(
                    Operator.StrictEquality,
                    ctorIdentifier,
                    JavaScriptAstFactory.CreateStringLiteral(lowering.HelperName)),
                new NestedBlockStatement(NodeList.From(branchStatements)),
                null));
        }

        statements.Add(new ThrowStatement(
            new NewExpression(
                new Identifier("Error"),
                NodeList.From<Expression>(
                    JavaScriptAstFactory.CreateStringLiteral(
                        $"No matching constructor overload for {containingType.Name}.")))));

        return new MethodDefinition(
            PropertyKind.Method,
            key: new Identifier("constructor"),
            value: new FunctionExpression(
                id: null,
                parameters: NodeList.Empty<Node>(),
                body: new FunctionBody(NodeList.From(statements), strict: true),
                generator: false,
                async: false),
            computed: false,
            isStatic: false,
            decorators: NodeList.Empty<Decorator>());
    }

    private MethodDefinition ConvertMemberConstructorHelper(MemberConstructorLowering lowering)
    {
        var parameters = new List<Node>();
        if (lowering.Symbol.Parameters.Length > 0)
        {
            foreach (var parameterSymbol in lowering.Symbol.Parameters)
                parameters.Add(new Identifier(parameterSymbol.Name));
        }

        return new MethodDefinition(
            PropertyKind.Method,
            key: new Identifier(lowering.HelperName),
            value: new FunctionExpression(
                id: null,
                parameters: NodeList.From(parameters),
                body: lowering.Body,
                generator: false,
                async: false),
            computed: false,
            isStatic: false,
            decorators: NodeList.Empty<Decorator>());
    }

    private ClassDeclaration ConvertMemberClass(
        INamedTypeSymbol symbol,
        INamedTypeSymbol? baseType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ValidateRuntimeClassJavaScriptNameScope(symbol);

        var nodes = new List<Node>();
        var constructorLowerings = GetMemberConstructorLowerings(symbol, baseType, cancellationToken);
        var hasExplicitConstructor = constructorLowerings.Count > 0;
        var hasPrimaryConstructor = constructorLowerings
            .SelectMany(static lowering => lowering.PrimaryParameterStorage)
            .Any();
        var primaryConstructorInitializerFields = hasPrimaryConstructor
            ? new HashSet<IFieldSymbol>(
                GetPrimaryConstructorInitializers(symbol)
                    .Select(static initializer => initializer.Field.OriginalDefinition),
                SymbolEqualityComparer.Default)
            : new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var constructorsEmitted = false;

        foreach (var member in symbol.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!ShouldIncludeMemberClassMember(member))
                continue;

            switch (member)
            {
                case IFieldSymbol field when field.IsImplicitlyDeclared:
                    break;
                case IFieldSymbol field:
                    nodes.Add(ConvertMemberField(
                        field,
                        suppressInitializer: primaryConstructorInitializerFields.Contains(field.OriginalDefinition)));
                    break;
                case IPropertySymbol prop:
                    nodes.AddRange(ConvertMemberProperty(
                        prop,
                        cancellationToken,
                        suppressInitializer: HasPrimaryConstructorInitializer(
                            prop,
                            primaryConstructorInitializerFields)));
                    break;
                case IMethodSymbol accessor when accessor.AssociatedSymbol is IPropertySymbol:
                    break;
                case IMethodSymbol ctor when ctor.MethodKind == MethodKind.Constructor:
                    if (!ctor.IsImplicitlyDeclared && !constructorsEmitted)
                    {
                        foreach (var parameterStorage in constructorLowerings
                                     .SelectMany(static lowering => lowering.PrimaryParameterStorage)
                                     .GroupBy(static storage => storage.FieldName, StringComparer.Ordinal)
                                     .Select(static group => group.First()))
                        {
                            nodes.Add(new PropertyDefinition(
                                key: CreateSyntheticPrivateStorageKey(parameterStorage.FieldName),
                                value: GetImplicitMemberFieldDefaultValue(parameterStorage.Parameter),
                                computed: false,
                                isStatic: false,
                                decorators: NodeList.Empty<Decorator>()));
                        }

                        if (constructorLowerings.Count == 1)
                        {
                            nodes.Add(ConvertMemberConstructor(constructorLowerings[0], baseType, cancellationToken));
                        }
                        else if (constructorLowerings.Count > 1)
                        {
                            nodes.Add(ConvertMemberConstructorDispatcher(symbol, constructorLowerings, baseType, cancellationToken));
                            foreach (var lowering in constructorLowerings)
                                nodes.Add(ConvertMemberConstructorHelper(lowering));
                        }

                        constructorsEmitted = true;
                    }
                    break;
                case IMethodSymbol ctor when ctor.MethodKind == MethodKind.SharedConstructor:
                    if (!ctor.IsImplicitlyDeclared)
                        throw new NotSupportedException($"Jazor member class does not support static constructor {ctor.Name}.");
                    break;
                case IMethodSymbol accessor when accessor.AssociatedSymbol is IEventSymbol:
                    break;
                case IMethodSymbol func when func.MethodKind == MethodKind.Ordinary:
                    nodes.Add(ConvertMemberMethod(func, cancellationToken));
                    break;
                case INamedTypeSymbol nestedEnum when nestedEnum.TypeKind == TypeKind.Enum:
                    // enum 在成员类内同样仅保留编译期值域角色，运行时声明擦除。
                    break;
                case INamedTypeSymbol nestedRecord when nestedRecord.IsRecord:
                    // record 在成员类内同样只保留编译期/结构化值语义，不发射 runtime class。
                    break;
                case INamedTypeSymbol nestedInterface when nestedInterface.TypeKind == TypeKind.Interface:
                    // interface 在成员类内同样只作为契约参与分析，不发射运行时对象。
                    break;
                case INamedTypeSymbol nestedClass when
                    ModuleDeclaredNames.ContainsKey(nestedClass.OriginalDefinition) &&
                    _modulePolicy.ShouldFlattenNestedRuntimeClass(_classSymbol, symbol, nestedClass):
                    // A host may emit a nested runtime class separately at module scope. The
                    // shared declared-name plan keeps references stable across both declarations.
                    break;
                case IEventSymbol eventSymbol:
                    nodes.AddRange(ConvertMemberEvent(eventSymbol));
                    break;
                default:
                    throw new NotSupportedException($"Jazor member class does not support {member.Kind}:{member.Name}.");
            }
        }

        if (baseType is not null && !hasExplicitConstructor)
            nodes.Insert(0, CreateImplicitBaseConstructor(baseType));

        var className = ModuleDeclaredNames.ContainsKey(symbol.OriginalDefinition)
            ? GetModuleDeclaredName(symbol)
            : Util.GetConfigOrSymbolName(symbol);
        var superClassName =
            baseType is not null &&
            ModuleDeclaredNames.ContainsKey(baseType.OriginalDefinition)
                ? GetModuleDeclaredName(baseType)
                : baseType is null
                    ? null
                    : Util.GetConfigOrSymbolName(baseType);
        var declaration = new ClassDeclaration(
            id: new Identifier(className),
            superClass: superClassName is null ? null : new Identifier(superClassName),
            body: new ClassBody(NodeList.From(nodes)),
            decorators: NodeList.Empty<Decorator>()
        );

        return declaration;
    }

    private static bool HasPrimaryConstructorInitializer(
        IPropertySymbol property,
        ISet<IFieldSymbol> primaryConstructorInitializerFields)
    {
        if (property.IsStatic || primaryConstructorInitializerFields.Count == 0)
            return false;

        var backingField = property.ContainingType
            .GetMembers($"<{property.Name}>k__BackingField")
            .OfType<IFieldSymbol>()
            .FirstOrDefault();
        return backingField is not null &&
               primaryConstructorInitializerFields.Contains(backingField.OriginalDefinition);
    }

    private void ValidateRuntimeClassJavaScriptNameScope(INamedTypeSymbol runtimeType)
    {
        var instanceNames = new Dictionary<string, ISymbol>(System.StringComparer.Ordinal);
        var staticNames = new Dictionary<string, ISymbol>(System.StringComparer.Ordinal);

        // `constructor` remains an ES class structural slot even when C# has no explicit
        // constructor. An ECMAScriptName alias must never redefine instance construction.
        instanceNames.Add("constructor", runtimeType);

        foreach (var member in runtimeType.GetMembers())
        {
            // Auto-property backing fields are emitted from the property branch instead of the
            // normal field branch. They still become ordinary `$jazor$private$...` properties
            // in the Proxy-safe profile and therefore participate in the class name scope.
            // auto-property backing field 虽是隐式 symbol，但实际会发射，不能跳过冲突检查。
            if (member is IFieldSymbol
                {
                    IsImplicitlyDeclared: true,
                    AssociatedSymbol: IPropertySymbol associatedProperty
                } implicitBackingField)
            {
                if (ShouldIncludeMemberClassMember(associatedProperty))
                {
                    AddRuntimeClassJavaScriptName(
                        implicitBackingField.IsStatic ? staticNames : instanceNames,
                        implicitBackingField,
                        GetRuntimeClassPrivateStorageName(
                            implicitBackingField,
                            GetMemberFieldDeclaredName(implicitBackingField)),
                        runtimeType);
                }

                continue;
            }

            if (!ShouldIncludeMemberClassMember(member))
                continue;

            if (member is IFieldSymbol field)
            {
                if (field.IsImplicitlyDeclared || field.AssociatedSymbol is IEventSymbol)
                    continue;

                var name = GetMemberFieldDeclaredName(field);
                if (ShouldBePrivate(field.DeclaredAccessibility))
                    name = GetRuntimeClassPrivateStorageName(field, name);

                AddRuntimeClassJavaScriptName(
                    field.IsStatic ? staticNames : instanceNames,
                    field,
                    name,
                    runtimeType);
                continue;
            }

            if (member is IPropertySymbol property)
            {
                if (!property.IsIndexer)
                {
                    AddRuntimeClassJavaScriptName(
                        property.IsStatic ? staticNames : instanceNames,
                        property,
                        Util.GetConfigOrSymbolName(property),
                        runtimeType);
                }

                continue;
            }

            if (member is IEventSymbol eventSymbol)
            {
                if (EventLowering.IsSupportedFieldLikeInstanceEvent(eventSymbol, out _))
                {
                    AddRuntimeClassJavaScriptName(
                        instanceNames,
                        eventSymbol,
                        GetRuntimeClassPrivateStorageName(
                            field: null,
                            EventLowering.GetStorageName(eventSymbol)),
                        runtimeType);
                    AddRuntimeClassJavaScriptName(
                        instanceNames,
                        eventSymbol,
                        EventLowering.GetAddMethodName(eventSymbol),
                        runtimeType);
                    AddRuntimeClassJavaScriptName(
                        instanceNames,
                        eventSymbol,
                        EventLowering.GetRemoveMethodName(eventSymbol),
                        runtimeType);
                    AddRuntimeClassJavaScriptName(
                        instanceNames,
                        eventSymbol,
                        EventLowering.GetSnapshotMethodName(eventSymbol),
                        runtimeType);
                }

                continue;
            }

            if (member is IMethodSymbol method &&
                method.MethodKind == MethodKind.Ordinary &&
                method.AssociatedSymbol is not IEventSymbol)
            {
                AddRuntimeClassJavaScriptName(
                    method.IsStatic ? staticNames : instanceNames,
                    method,
                    Util.GetConfigOrSymbolName(method),
                    runtimeType);
            }
        }

        var explicitConstructors = runtimeType.InstanceConstructors
            .Where(static constructor => !constructor.IsImplicitlyDeclared)
            .ToImmutableArray();
        if (explicitConstructors.Any(ShouldIncludeMemberClassMember))
        {
            foreach (var storage in GetPrimaryConstructorParameterStorage(runtimeType))
            {
                AddRuntimeClassJavaScriptName(
                    instanceNames,
                    storage.Parameter,
                    GetRuntimeClassPrivateStorageName(field: null, storage.FieldName),
                    runtimeType);
            }

            if (explicitConstructors.Length > 1)
            {
                foreach (var constructor in explicitConstructors)
                {
                    AddRuntimeClassJavaScriptName(
                        instanceNames,
                        constructor,
                        GetMemberConstructorHelperName(constructor),
                        runtimeType);
                }
            }
        }
    }

    private string GetRuntimeClassPrivateStorageName(IFieldSymbol? field, string fallbackName)
    {
        if (_options.RuntimeClassPrivateStorage != RuntimeClassPrivateStorage.ProxySafeMangledProperties)
            return "#" + fallbackName;

        return field is null
            ? RuntimeClassPrivateStorageNames.GetSyntheticStorageName(
                _options.RuntimeClassPrivateStorage,
                fallbackName)
            : RuntimeClassPrivateStorageNames.GetFieldStorageName(
                _options.RuntimeClassPrivateStorage,
                field,
                fallbackName);
    }

    private static void AddRuntimeClassJavaScriptName(
        Dictionary<string, ISymbol> names,
        ISymbol symbol,
        string name,
        INamedTypeSymbol runtimeType)
    {
        if (!names.TryGetValue(name, out var existingSymbol) ||
            SymbolEqualityComparer.Default.Equals(existingSymbol, symbol))
        {
            names[name] = symbol;
            return;
        }

        throw new NotSupportedException(
            $"Jazor runtime class '{runtimeType.ToDisplayString(Format.NameFormat)}' has duplicate JavaScript member name '{name}' for '" +
            $"{existingSymbol.ToDisplayString(Format.NameFormat)}' and '{symbol.ToDisplayString(Format.NameFormat)}'. " +
            "Use unique member names or explicit ECMAScriptName mappings.");
    }

    private List<MemberConstructorLowering> GetMemberConstructorLowerings(
        INamedTypeSymbol symbol,
        INamedTypeSymbol? baseType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var lowerings = symbol.InstanceConstructors
            .Where(static ctor => !ctor.IsImplicitlyDeclared)
            .OrderBy(static ctor => ctor.DeclaringSyntaxReferences[0].Span.Start)
            .Select(constructor => PrepareMemberConstructorLowering(constructor, baseType, cancellationToken))
            .ToList();

        if (lowerings.Count <= 1)
            return lowerings;

        if (lowerings.Any(static lowering =>
                lowering.Symbol.Parameters.Any(parameter =>
                    parameter.RefKind is RefKind.Ref or RefKind.Out or RefKind.In || parameter.IsParams)))
            throw new NotSupportedException($"Jazor member class does not support constructor overload dispatch with ref/out/in/params parameters {symbol.Name}.");

        return lowerings;
    }

    private MemberConstructorLowering PrepareMemberConstructorLowering(
        IMethodSymbol symbol,
        INamedTypeSymbol? baseType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ConstructorDeclarationSyntax? constructorSyntax = null;
        ImmutableArray<ArgumentSyntax> baseArguments = ImmutableArray<ArgumentSyntax>.Empty;
        IMethodSymbol? baseConstructorSymbol = null;
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            switch (reference.GetSyntax())
            {
                case ConstructorDeclarationSyntax ctorDecl:
                    constructorSyntax = ctorDecl;
                    if (ctorDecl.Initializer is not null &&
                        (baseType is null || !ctorDecl.Initializer.ThisOrBaseKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.BaseKeyword)))
                    {
                        throw new NotSupportedException($"Jazor member class does not support constructor initializer on {symbol.Name}.");
                    }

                    if (ctorDecl.Initializer is not null)
                    {
                        baseArguments = ctorDecl.Initializer.ArgumentList.Arguments.ToImmutableArray();
                        baseConstructorSymbol = GetSemanticModel(ctorDecl.Initializer).GetSymbolInfo(ctorDecl.Initializer).Symbol as IMethodSymbol;
                    }

                    break;
                case ClassDeclarationSyntax { ParameterList: { } } primaryConstructorDeclaration:
                    return PreparePrimaryConstructorLowering(
                        symbol,
                        primaryConstructorDeclaration,
                        baseType,
                        cancellationToken);
            }

            if (constructorSyntax is not null)
                break;
        }

        if (constructorSyntax is null)
            throw new NotSupportedException($"Jazor member class constructor {symbol.Name} has no supported source declaration.");

        var declaredConstructor = constructorSyntax;
        var isExpressionBody = declaredConstructor.Body is null;
        var operation = declaredConstructor.Body is { } bodySyntax
            ? GetSemanticModel(bodySyntax).GetOperation(bodySyntax)!
            : GetSemanticModel(declaredConstructor.ExpressionBody!).GetOperation(declaredConstructor.ExpressionBody!)!;
        var body = ConvertMemberOperationToFunctionBody(
            operation,
            returnsVoid: true,
            cancellationToken,
            runtimeType: symbol.ContainingType,
            isExpressionBody: isExpressionBody);

        return new MemberConstructorLowering(
            Symbol: symbol,
            BaseArguments: baseArguments,
            BaseConstructorSymbol: baseConstructorSymbol,
            Body: body,
            HelperName: GetMemberConstructorHelperName(symbol),
            PrimaryParameterStorage: ImmutableArray<PrimaryConstructorParameterStorage>.Empty);
    }

    private MemberConstructorLowering PreparePrimaryConstructorLowering(
        IMethodSymbol symbol,
        ClassDeclarationSyntax declaration,
        INamedTypeSymbol? baseType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var parameterStorage = GetPrimaryConstructorParameterStorage(symbol.ContainingType);
        var storageByParameter = parameterStorage.ToDictionary(
            static storage => storage.Parameter.OriginalDefinition.ToDisplayString(Format.NameFormat),
            static storage => storage.FieldName,
            StringComparer.Ordinal);
        var parameterHost = new PrimaryConstructorParameterSemanticWalkerHost(
            storageByParameter,
            _options.RuntimeClassPrivateStorage);
        var statements = new List<Statement>();

        // Primary constructor syntax has no IConstructorBodyOperation. Rebuild only the source
        // initialization phase here, in declaration order, and let SemanticWalker own every RHS.
        // The private slots are assigned first so later initializer expressions observe the same
        // captured parameter values as C# instance members.
        foreach (var storage in parameterStorage)
        {
            statements.Add(new NonSpecialExpressionStatement(
                new AssignmentExpression(
                    Operator.Assignment,
                    new MemberExpression(
                        new ThisExpression(),
                        CreateSyntheticPrivateStorageKey(storage.FieldName),
                        computed: false,
                        optional: false),
                    new Identifier(storage.Parameter.Name))));
        }

        foreach (var initializer in GetPrimaryConstructorInitializers(symbol.ContainingType))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var expression = ConvertPrimaryConstructorInitializerExpression(
                initializer.Syntax,
                parameterHost,
                symbol.ContainingType,
                cancellationToken);
            statements.Add(new NonSpecialExpressionStatement(
                new AssignmentExpression(
                    Operator.Assignment,
                    BuildFieldAccess(initializer.Field),
                    expression)));
        }

        var (baseArguments, baseConstructorSymbol) = GetPrimaryConstructorBaseInvocation(
            declaration,
            baseType);
        return new MemberConstructorLowering(
            Symbol: symbol,
            BaseArguments: baseArguments,
            BaseConstructorSymbol: baseConstructorSymbol,
            Body: new FunctionBody(NodeList.From(statements), strict: true),
            HelperName: GetMemberConstructorHelperName(symbol),
            PrimaryParameterStorage: parameterStorage);
    }

    private ImmutableArray<PrimaryConstructorInitializer> GetPrimaryConstructorInitializers(INamedTypeSymbol runtimeType)
    {
        var initializers = new List<PrimaryConstructorInitializer>();
        foreach (var member in runtimeType.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false } field:
                    foreach (var reference in field.DeclaringSyntaxReferences)
                    {
                        if (reference.GetSyntax() is VariableDeclaratorSyntax { Initializer: { } initializer })
                            initializers.Add(new PrimaryConstructorInitializer(field, initializer));
                    }

                    break;
                case IPropertySymbol property when !property.IsStatic:
                    foreach (var reference in property.DeclaringSyntaxReferences)
                    {
                        if (reference.GetSyntax() is PropertyDeclarationSyntax { Initializer: { } initializer })
                        {
                            var backingField = property.ContainingType
                                .GetMembers($"<{property.Name}>k__BackingField")
                                .OfType<IFieldSymbol>()
                                .FirstOrDefault();
                            if (backingField is not null)
                                initializers.Add(new PrimaryConstructorInitializer(backingField, initializer));
                        }
                    }

                    break;
            }
        }

        return initializers
            .OrderBy(static initializer => initializer.Syntax.SpanStart)
            .ToImmutableArray();
    }

    private Expression ConvertPrimaryConstructorInitializerExpression(
        EqualsValueClauseSyntax syntax,
        SemanticWalkerHost parameterHost,
        INamedTypeSymbol runtimeType,
        CancellationToken cancellationToken)
    {
        var operation = GetSemanticModel(syntax.Value).GetOperation(syntax.Value)!;
        var walker = CreateSemanticWalker(
            cancellationToken,
            parameterHost,
            runtimeType,
            rewritePrimaryConstructorParameters: false);
        var argument = CreateImportAwareArgument(Sense.Any);
        var expression = (Expression)walker.Visit(operation, argument)!;
        MergeImports(argument);
        return MaterializeExpression(expression, argument);
    }

    private Expression BuildFieldAccess(IFieldSymbol field)
    {
        var fieldName = GetMemberFieldDeclaredName(field);
        return new MemberExpression(
            new ThisExpression(),
            ShouldBePrivate(field.DeclaredAccessibility)
                ? CreatePrivateStorageKey(field, fieldName)
                : new Identifier(fieldName),
            computed: false,
            optional: false);
    }

    private (ImmutableArray<ArgumentSyntax> Arguments, IMethodSymbol? Constructor) GetPrimaryConstructorBaseInvocation(
        ClassDeclarationSyntax declaration,
        INamedTypeSymbol? baseType)
    {
        var baseTypeSyntax = declaration.BaseList?.Types
            .OfType<PrimaryConstructorBaseTypeSyntax>()
            .SingleOrDefault();
        if (baseTypeSyntax is null)
            return (ImmutableArray<ArgumentSyntax>.Empty, null);

        if (baseType is null)
            throw new NotSupportedException($"Jazor member class does not support primary constructor base initializer on {declaration.Identifier.ValueText}.");

        return (
            baseTypeSyntax.ArgumentList?.Arguments.ToImmutableArray() ?? ImmutableArray<ArgumentSyntax>.Empty,
            GetSemanticModel(baseTypeSyntax).GetSymbolInfo(baseTypeSyntax).Symbol as IMethodSymbol);
    }

    private IEnumerable<Statement> BuildConstructorDispatcherParameterBindings(
        MemberConstructorLowering lowering,
        Identifier argsIdentifier)
    {
        if (lowering.Symbol.Parameters.Length == 0)
            yield break;

        var argsLength = new MemberExpression(argsIdentifier, new Identifier("length"), computed: false, optional: false);
        var declarators = new List<VariableDeclarator>(lowering.Symbol.Parameters.Length);
        for (var index = 0; index < lowering.Symbol.Parameters.Length; index++)
        {
            var parameter = lowering.Symbol.Parameters[index];
            var argumentIndex = index + 1;
            Expression suppliedValue = new MemberExpression(
                argsIdentifier,
                new NumericLiteral(argumentIndex, argumentIndex.ToString(CultureInfo.InvariantCulture)),
                computed: true,
                optional: false);
            Expression value = parameter.HasExplicitDefaultValue
                ? new ConditionalExpression(
                    new NonLogicalBinaryExpression(
                        Operator.GreaterThan,
                        argsLength,
                        new NumericLiteral(argumentIndex, argumentIndex.ToString(CultureInfo.InvariantCulture))),
                    suppliedValue,
                    CreateParameterDefaultValue(parameter))
                : suppliedValue;
            declarators.Add(new VariableDeclarator(new Identifier(parameter.Name), value));
        }

        yield return new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarators));
    }

    private Statement CreateSuperConstructorCallStatement(
        ImmutableArray<ArgumentSyntax> baseArguments,
        IMethodSymbol? baseConstructorSymbol,
        CancellationToken cancellationToken)
        => new NonSpecialExpressionStatement(
            new CallExpression(
                new Super(),
                CreateConstructorInitializerArguments(baseArguments, baseConstructorSymbol, cancellationToken),
                optional: false));

    private static string GetMemberConstructorHelperName(IMethodSymbol symbol)
        => Util.GetMemberConstructorHelperName(symbol);

    private string GetMemberBackingFieldName(IPropertySymbol property)
    {
        var backingField = GetMemberBackingFieldSymbol(property);

        if (backingField is not null)
            return GetMemberFieldDeclaredName(backingField);

        return Format.HashName(property.OriginalDefinition.ToDisplayString(Format.NameFormat));
    }

    private static string GetMemberFieldDeclaredName(IFieldSymbol symbol)
    {
        if (symbol.AssociatedSymbol is IPropertySymbol && symbol.IsImplicitlyDeclared)
            return Format.HashName(symbol.AssociatedSymbol!.OriginalDefinition.ToDisplayString(Format.NameFormat));

        return Util.GetConfigOrSymbolName(symbol);
    }

    private IFieldSymbol? GetMemberBackingFieldSymbol(IPropertySymbol property)
        => property.ContainingType
            .GetMembers($"<{property.Name}>k__BackingField")
            .OfType<IFieldSymbol>()
            .FirstOrDefault();

    private Expression CreatePrivateStorageKey(IFieldSymbol? field, string fieldName)
    {
        if (_options.RuntimeClassPrivateStorage != RuntimeClassPrivateStorage.ProxySafeMangledProperties)
            return new PrivateIdentifier(fieldName);

        var storageName = field is null
            ? RuntimeClassPrivateStorageNames.GetSyntheticStorageName(
                _options.RuntimeClassPrivateStorage,
                fieldName)
            : RuntimeClassPrivateStorageNames.GetFieldStorageName(
                _options.RuntimeClassPrivateStorage,
                field,
                fieldName);
        return new Identifier(storageName);
    }

    private Expression CreateSyntheticPrivateStorageKey(string fieldName)
        => _options.RuntimeClassPrivateStorage == RuntimeClassPrivateStorage.ProxySafeMangledProperties
            ? new Identifier(RuntimeClassPrivateStorageNames.GetSyntheticStorageName(
                _options.RuntimeClassPrivateStorage,
                fieldName))
            : new PrivateIdentifier(fieldName);

    private IEnumerable<Statement> ConvertModuleClass(
        INamedTypeSymbol symbol,
        INamedTypeSymbol? baseType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (symbol.IsStatic)
            throw new NotSupportedException($"Jazor module class does not support static member class {symbol.Name}.");
            
        var declaration = ConvertMemberClass(symbol, baseType, cancellationToken);

        if (ShouldBePrivate(symbol.DeclaredAccessibility) ||
            !ShouldExportModuleMember(symbol))
        {
            yield return declaration;
            yield break;
        }

        var localName = GetModuleDeclaredName(symbol);
        var exportName = Util.GetConfigOrSymbolName(symbol);
        if (string.Equals(localName, exportName, System.StringComparison.Ordinal))
        {
            yield return new ExportNamedDeclaration(
                declaration,
                NodeList.From<ExportSpecifier>([]),
                null,
                NodeList.From<ImportAttribute>([]));
            yield break;
        }

        yield return declaration;
        yield return CreateNamedExport(localName, exportName);
    }

    private INamedTypeSymbol? GetSupportedMemberBaseType(INamedTypeSymbol symbol)
    {
        if (symbol.BaseType is not INamedTypeSymbol baseType ||
            baseType.SpecialType == SpecialType.System_Object)
            return null;

        if (!SymbolEqualityComparer.Default.Equals(baseType.ContainingType?.OriginalDefinition, _classSymbol.OriginalDefinition))
            throw new NotSupportedException($"Jazor member class does not support inheritance {symbol.Name} : {baseType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}.");

        return baseType;
    }

    private INamedTypeSymbol? GetSupportedRuntimeClassBaseType(INamedTypeSymbol symbol)
    {
        if (symbol.BaseType is not INamedTypeSymbol baseType ||
            baseType.SpecialType == SpecialType.System_Object)
        {
            return null;
        }

        if (ModuleDeclaredNames.ContainsKey(baseType.OriginalDefinition))
            return baseType;

        throw new NotSupportedException($"Jazor runtime class does not support inheritance {symbol.Name} : {baseType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}.");
    }

    private static MethodDefinition CreateImplicitBaseConstructor(INamedTypeSymbol baseType)
    {
        var arguments = new List<Expression>();
        var baseConstructor = ResolveImplicitBaseConstructor(baseType);
        if (baseConstructor is not null &&
            HasMultipleExplicitConstructors(baseType))
        {
            var helperName = GetMemberConstructorHelperName(baseConstructor);
            arguments.Add(JavaScriptAstFactory.CreateStringLiteral(helperName));
        }

        var body = new FunctionBody(
            strict: true,
            body: NodeList.From<Statement>(
                new NonSpecialExpressionStatement(
                    new CallExpression(
                        new Super(),
                        NodeList.From(arguments),
                        optional: false))));

        return new MethodDefinition(
            PropertyKind.Method,
            key: new Identifier("constructor"),
            value: new FunctionExpression(
                id: null,
                parameters: NodeList.Empty<Node>(),
                body: body,
                generator: false,
                async: false),
            computed: false,
            isStatic: false,
            decorators: NodeList.Empty<Decorator>());
    }

    private FunctionBody PrependSuperConstructorCall(
        FunctionBody body,
        ImmutableArray<ArgumentSyntax> baseArguments,
        IMethodSymbol? baseConstructorSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var statements = new List<Statement>
        {
            new NonSpecialExpressionStatement(
                new CallExpression(
                    new Super(),
                    CreateConstructorInitializerArguments(baseArguments, baseConstructorSymbol, cancellationToken),
                    optional: false))
        };

        statements.AddRange(body.Body);
        return new FunctionBody(NodeList.From(statements), body.Strict);
    }

    private NodeList<Expression> CreateConstructorInitializerArguments(
        ImmutableArray<ArgumentSyntax> baseArguments,
        IMethodSymbol? baseConstructorSymbol,
        CancellationToken cancellationToken)
    {
        var arguments = baseArguments
            .Select(argument => ConvertConstructorInitializerArgument(argument, cancellationToken))
            .ToList();

        if (baseConstructorSymbol is not null && HasMultipleExplicitConstructors(baseConstructorSymbol.ContainingType))
            arguments.Insert(0, JavaScriptAstFactory.CreateStringLiteral(GetMemberConstructorHelperName(baseConstructorSymbol)));

        return NodeList.From(arguments);
    }

    private static bool HasMultipleExplicitConstructors(INamedTypeSymbol typeSymbol)
        => typeSymbol.InstanceConstructors.Count(static ctor => !ctor.IsImplicitlyDeclared) > 1;

    private static IMethodSymbol? ResolveImplicitBaseConstructor(INamedTypeSymbol baseType)
    {
        var explicitConstructors = baseType.InstanceConstructors
            .Where(static ctor => !ctor.IsImplicitlyDeclared)
            .ToList();
        return explicitConstructors.FirstOrDefault(static ctor => ctor.Parameters.Length == 0) ??
            explicitConstructors.SingleOrDefault(static ctor => ctor.Parameters.All(static parameter => parameter.HasExplicitDefaultValue));
    }

    private Expression ConvertConstructorInitializerArgument(ArgumentSyntax argumentSyntax, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (argumentSyntax.NameColon is not null)
            throw new NotSupportedException("Jazor member class does not support named constructor initializer arguments.");

        if (argumentSyntax.RefKindKeyword.Kind() is Microsoft.CodeAnalysis.CSharp.SyntaxKind.RefKeyword or Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword)
            throw new NotSupportedException("Jazor member class does not support ref/out constructor initializer arguments.");

        return ConvertExpressionSyntax(argumentSyntax.Expression, cancellationToken);
    }

    private Expression ConvertParameter(IParameterSymbol parameter)
    {
        var identifier = new Identifier(parameter.Name);
        if (parameter.HasExplicitDefaultValue)
        {
            var right = CreateParameterDefaultValue(parameter);
            return new AssignmentExpression("=", identifier, right);
        }

        return identifier;
    }

    private Expression CreateParameterDefaultValue(IParameterSymbol parameter)
    {
        // AstConverter only emits source-declared member methods. A parameter with an explicit
        // default therefore owns exactly one ParameterSyntax carrying the authored expression.
        var syntax = (ParameterSyntax)parameter.DeclaringSyntaxReferences.Single().GetSyntax();
        return CreateEqualsValueClauseSyntaxLiteral(syntax.Default!);
    }

    private static Expression CreateEqualsValueClauseSyntaxLiteral(SpecialType type, object? value)
    {
        _ = type;
        if (value is null)
        {
            // Optional parameters and const reference fields can carry a null constant
            // without a useful SpecialType discriminator, so null must lower directly.
            return new NullLiteral("null");
        }

        return CreateLiteralExpression(value);
    }

    private Expression CreateEqualsValueClauseSyntaxLiteral(EqualsValueClauseSyntax syntax, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value = syntax.Value;

        return ConvertExpressionSyntax(value, cancellationToken);
    }

    private Expression ConvertExpressionSyntax(ExpressionSyntax value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (value is LiteralExpressionSyntax lit &&
            !lit.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DefaultLiteralExpression))
            return CreateLiteralExpression(lit.Token.Value);

        // The null-forgiving postfix is an authoring-time nullable annotation. Roslyn may not
        // expose an IOperation for the wrapper syntax, so lower its operand directly instead.
        if (value is PostfixUnaryExpressionSyntax suppressNullableWarning &&
            suppressNullableWarning.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SuppressNullableWarningExpression))
            return ConvertExpressionSyntax(suppressNullableWarning.Operand, cancellationToken);

        // This helper receives successful C# expression syntax only. Apart from the two syntax
        // forms handled above, Roslyn always supplies an expression operation for that input.
        var operation = GetSemanticModel(value).GetOperation(value)!;
        var walker = CreateSemanticWalker(cancellationToken);
        var argument = CreateImportAwareArgument(Sense.Any);
        var expr = (Expression)walker.Visit(operation, argument)!;
        MergeImports(argument);
        return MaterializeExpression(expr, argument);
    }

    private static FunctionBody MaterializeFunctionBody(Node visited, SenseArgument argument, bool returnsVoid)
    {
        if (visited is FunctionBody body)
            // VisitBlock(Sense.FunctionBody) materializes the scope-owned temp prefix before
            // constructing FunctionBody. Expression-bodied members take the branch below.
            return body;

        var bodyStatements = MaterializeTemporaryDeclarationPrefix(argument);
        switch (visited)
        {
            case NestedBlockStatement block:
                bodyStatements.AddRange(block.Body);
                break;

            case Statement statement:
                bodyStatements.Add(statement);
                break;

            case Expression expression:
                bodyStatements.Add(returnsVoid
                    ? new NonSpecialExpressionStatement(expression)
                    : new ReturnStatement(expression));
                break;

            default:
                throw new InvalidOperationException($"Unsupported member body node: {visited.Type}.");
        }

        return new FunctionBody(NodeList.From(bodyStatements), true);
    }

    private static Expression MaterializeExpression(Expression expression, SenseArgument argument)
    {
        var statements = MaterializeTemporaryDeclarationPrefix(argument);
        if (statements.Count == 0)
            return expression;

        statements.Add(new ReturnStatement(expression));
        var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
        var arrowFunction = new ArrowFunctionExpression(
            NodeList.From<Node>(),
            functionBody,
            expression: false,
            async: false);
        return new CallExpression(arrowFunction, NodeList.From<Expression>(), optional: false);
    }

    private static List<Statement> MaterializeTemporaryDeclarationPrefix(SenseArgument argument)
    {
        var statements = new List<Statement>();
        if (!argument.HasVarDeclarator)
            return statements;

        var declarators = argument.FlushVarDeclarator();
        if (declarators.Count > 0)
            statements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));

        return statements;
    }

    private static Expression CreateLiteralExpression(object? value)
    {
        return value switch
        {
            null => new NullLiteral("null"),
            bool b => new BooleanLiteral(b, b.ToString().ToLowerInvariant()),
            char c => JavaScriptAstFactory.CreateStringLiteral(c.ToString()),
            string s => JavaScriptAstFactory.CreateStringLiteral(s),
            sbyte sb => JavaScriptAstFactory.CreateNumericExpression(sb, sb.ToString(CultureInfo.InvariantCulture)),
            byte b => JavaScriptAstFactory.CreateNumericExpression(b, b.ToString(CultureInfo.InvariantCulture)),
            short s => JavaScriptAstFactory.CreateNumericExpression(s, s.ToString(CultureInfo.InvariantCulture)),
            ushort us => JavaScriptAstFactory.CreateNumericExpression(us, us.ToString(CultureInfo.InvariantCulture)),
            int i => JavaScriptAstFactory.CreateNumericExpression(i, i.ToString(CultureInfo.InvariantCulture)),
            uint ui => JavaScriptAstFactory.CreateNumericExpression(ui, ui.ToString(CultureInfo.InvariantCulture)),
            long l => JavaScriptAstFactory.CreateBigIntExpression(new BigInteger(l), $"{l.ToString(CultureInfo.InvariantCulture)}n"),
            ulong ul => JavaScriptAstFactory.CreateBigIntExpression(new BigInteger(ul), $"{ul.ToString(CultureInfo.InvariantCulture)}n"),
            double d => JavaScriptAstFactory.CreateNumericExpression(d, d.ToString("R", CultureInfo.InvariantCulture)),
            float f => JavaScriptAstFactory.CreateNumericExpression(f, f.ToString("R", CultureInfo.InvariantCulture)),
            decimal dec => JavaScriptAstFactory.CreateNumericExpression(System.Convert.ToDouble(dec), dec.ToString(CultureInfo.InvariantCulture)),
            _ => throw new NotSupportedException($"Unsupported literal type: {value.GetType()}")
        };
    }

    private sealed record ModuleNamePlan(
        HashSet<string> LocalNames,
        Dictionary<ISymbol, string> DeclaredNames,
        HashSet<string> ReservedImportNames);

    private IEnumerable<ISymbol> EnumerateModuleMembersForConversion()
        => EnumerateModuleMembers(_classSymbol, _modulePolicy);

    private static IEnumerable<ISymbol> EnumerateModuleMembers(
        INamedTypeSymbol classSymbol,
        AstConverterModulePolicy modulePolicy)
        => modulePolicy
            .EnumerateModuleTypes(classSymbol)
            .SelectMany(static type => type.GetMembers());

    private static ModuleNamePlan BuildModuleNamePlan(
        INamedTypeSymbol classSymbol,
        Func<ISymbol, bool>? includeMember,
        AstConverterModulePolicy modulePolicy,
        AstConverterProfile profile)
    {
        var localNames = BuildModuleLocalNames(classSymbol, modulePolicy);
        var declaredNames = BuildModuleDeclaredNames(classSymbol, localNames, includeMember, modulePolicy, profile);
        var reservedImportNames = BuildReservedImportNames(classSymbol, declaredNames, localNames, includeMember, modulePolicy);
        return new ModuleNamePlan(localNames, declaredNames, reservedImportNames);
    }

    private string GetModuleDeclaredName(ISymbol symbol)
        => ModuleDeclaredNames.TryGetValue(symbol.OriginalDefinition, out var name)
            ? name
            : GetPreferredModuleDeclaredName(symbol, _modulePolicy, _options.Profile);

    private string GetModuleNamedExportName(ISymbol symbol)
    {
        if (_options.Profile == AstConverterProfile.ClrRuntime &&
            TryGetClrImportRuntimeName(symbol, out var runtimeName))
        {
            return runtimeName;
        }

        return Util.GetConfigOrSymbolName(symbol);
    }

    private void ValidateModuleExportPolicy()
    {
        var exportedNames = new Dictionary<string, ISymbol>(System.StringComparer.Ordinal);
        foreach (var member in EnumerateModuleMembersForConversion())
        {
            if (!ShouldIncludeModuleMember(member))
                continue;

            switch (member)
            {
                case IFieldSymbol field:
                    ValidateModuleExportPolicy(field, exportedNames);
                    break;
                case IMethodSymbol method when ShouldReserveModuleMethodName(method):
                    ValidateModuleExportPolicy(method, exportedNames);
                    break;
                case INamedTypeSymbol type when IsRuntimeMemberClass(type):
                    ValidateModuleExportPolicy(type, exportedNames);
                    break;
            }
        }
    }

    private void ValidateModuleExportPolicy(
        ISymbol symbol,
        Dictionary<string, ISymbol> exportedNames)
    {
        if (ShouldBePrivate(symbol.DeclaredAccessibility) ||
            !ShouldExportModuleMember(symbol))
            return;

        var exportName = GetModuleNamedExportName(symbol);
        if (string.Equals(exportName, "default", System.StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                $"Jazor module export does not support default export. Member '{symbol.ToDisplayString(Format.NameFormat)}' resolves to export name 'default'. Use a named export instead.");
        }

        if (exportedNames.TryGetValue(exportName, out var existingSymbol))
        {
            throw new NotSupportedException(
                $"Jazor module export does not support duplicate named export '{exportName}'. " +
                $"Members '{existingSymbol.ToDisplayString(Format.NameFormat)}' and " +
                $"'{symbol.ToDisplayString(Format.NameFormat)}' resolve to the same export name. " +
                "Use unique named exports instead.");
        }

        exportedNames.Add(exportName, symbol);
    }

    private static HashSet<string> BuildModuleLocalNames(
        INamedTypeSymbol classSymbol,
        AstConverterModulePolicy modulePolicy)
    {
        var names = new HashSet<string>(System.StringComparer.Ordinal);
        foreach (var type in modulePolicy.EnumerateModuleTypes(classSymbol))
        foreach (var syntaxRef in type.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not ClassDeclarationSyntax classSyntax)
                continue;

            var collector = new DeclaredNameCollector();
            collector.Visit(classSyntax);
            names.UnionWith(collector.Names);
        }

        return names;
    }

    private static Dictionary<ISymbol, string> BuildModuleDeclaredNames(
        INamedTypeSymbol classSymbol,
        HashSet<string> localNames,
        Func<ISymbol, bool>? includeMember,
        AstConverterModulePolicy modulePolicy,
        AstConverterProfile profile)
    {
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
        var usedDeclaredNames = new HashSet<string>(System.StringComparer.Ordinal);

        foreach (var member in EnumerateModuleMembers(classSymbol, modulePolicy))
        {
            if (includeMember is not null && !includeMember(member))
                continue;

            switch (member)
            {
                case IFieldSymbol field:
                    declaredNames[field.OriginalDefinition] = ChooseModuleDeclaredName(field, usedDeclaredNames, localNames, modulePolicy, profile);
                    break;
                case IMethodSymbol method when ShouldReserveModuleMethodName(method):
                    declaredNames[method.OriginalDefinition] = ChooseModuleDeclaredName(method, usedDeclaredNames, localNames, modulePolicy, profile);
                    break;
                case INamedTypeSymbol type when IsRuntimeMemberClass(type):
                    declaredNames[type.OriginalDefinition] = ChooseModuleDeclaredName(type, usedDeclaredNames, localNames, modulePolicy, profile);
                    break;
            }
        }

        return declaredNames;
    }

    private static string ChooseModuleDeclaredName(
        ISymbol symbol,
        HashSet<string> usedDeclaredNames,
        HashSet<string> localNames,
        AstConverterModulePolicy modulePolicy,
        AstConverterProfile profile)
    {
        var preferredName = GetPreferredModuleDeclaredName(symbol, modulePolicy, profile);
        if (JavaScriptAstFactory.IsJavaScriptBindingIdentifier(preferredName) &&
            !localNames.Contains(preferredName) &&
            usedDeclaredNames.Add(preferredName))
        {
            return preferredName;
        }

        var sourceName = GetSourceDeclaredNameCandidate(symbol);
        if (JavaScriptAstFactory.IsJavaScriptBindingIdentifier(sourceName) &&
            !localNames.Contains(sourceName!) &&
            usedDeclaredNames.Add(sourceName!))
        {
            return sourceName!;
        }

        var displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        var alias = $"m${Format.HashName(displayString).TrimStart('_')}";
        var suffix = 0;
        while (localNames.Contains(alias) || !usedDeclaredNames.Add(alias))
        {
            suffix++;
            alias = $"m${Format.HashName(displayString).TrimStart('_')}${suffix}";
        }

        return alias;
    }

    private static string GetPreferredModuleDeclaredName(
        ISymbol symbol,
        AstConverterModulePolicy modulePolicy,
        AstConverterProfile profile)
    {
        if (profile == AstConverterProfile.ClrRuntime &&
            TryGetClrImportRuntimeName(symbol, out var runtimeName))
        {
            return runtimeName;
        }

        return modulePolicy.GetPreferredModuleDeclaredName(symbol) ?? symbol switch
        {
            IFieldSymbol field => GetPreferredModuleFieldDeclaredName(field),
            IMethodSymbol method => Util.GetConfigOrSymbolName(method),
            INamedTypeSymbol type => Util.GetConfigOrSymbolName(type),
            _ => Util.GetConfigOrSymbolName(symbol)
        };
    }

    private static string? GetSourceDeclaredNameCandidate(ISymbol symbol)
        => symbol switch
        {
            IFieldSymbol field when field.IsImplicitlyDeclared => null,
            IFieldSymbol field => field.Name,
            IMethodSymbol method when method.AssociatedSymbol is IPropertySymbol property => property.Name,
            IMethodSymbol method => method.Name,
            INamedTypeSymbol type => type.Name,
            _ => symbol.Name
        };

    /// <summary>
    /// 收集模块级保留名。
    /// 这不是逐词法作用域的精确遮蔽分析，而是为导入绑定提供一个稳定的保守上界：
    /// 只要名字在模块成员或任意局部声明里出现过，就视为该名字可能与导入冲突。
    /// 这样会放大一部分本可直接使用原名的场景，但能避免漏判导致的错误绑定。
    /// </summary>
    private static HashSet<string> BuildReservedImportNames(
        INamedTypeSymbol classSymbol,
        IReadOnlyDictionary<ISymbol, string> declaredNames,
        HashSet<string> localNames,
        Func<ISymbol, bool>? includeMember,
        AstConverterModulePolicy modulePolicy)
    {
        var names = new HashSet<string>(System.StringComparer.Ordinal);

        foreach (var member in EnumerateModuleMembers(classSymbol, modulePolicy))
        {
            if (includeMember is not null && !includeMember(member))
                continue;

            switch (member)
            {
                case IFieldSymbol field when declaredNames.TryGetValue(field.OriginalDefinition, out var fieldName):
                    names.Add(fieldName);
                    break;
                case IMethodSymbol method when ShouldReserveModuleMethodName(method) &&
                                               declaredNames.TryGetValue(method.OriginalDefinition, out var methodName):
                    names.Add(methodName);
                    break;
                case INamedTypeSymbol type when IsRuntimeMemberClass(type) &&
                                               declaredNames.TryGetValue(type.OriginalDefinition, out var typeName):
                    names.Add(typeName);
                    break;
            }
        }

        foreach (var localName in localNames)
            names.Add(localName);

        return names;
    }

    private static bool ShouldReserveModuleMethodName(IMethodSymbol method)
    {
        if (method.MethodKind == MethodKind.SharedConstructor && method.IsImplicitlyDeclared)
            return false;

        if (Util.IsBodylessInitAccessor(method))
            return false;

        return method.MethodKind is MethodKind.Ordinary or MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.SharedConstructor;
    }

    private static bool IsRuntimeMemberClass(INamedTypeSymbol type)
        => type.TypeKind == TypeKind.Class && !type.IsRecord;

    private bool IsAllowedTopLevelAccessibility(Accessibility accessibility)
        => accessibility == Accessibility.Public ||
           _options.Profile == AstConverterProfile.ClrRuntime && accessibility == Accessibility.Internal ||
           _modulePolicy.IsAdditionalTopLevelAccessibilityAllowed(accessibility);

    private bool ShouldIncludeModuleMember(ISymbol member)
    {
        // A policy can project ordinary source classes to collect their static API. Their
        // instance constructors belong to runtime-class lowering, never to a module artifact.
        if (member is IMethodSymbol { MethodKind: MethodKind.Constructor })
            return false;

        return _options.MemberFilter?.Invoke(member) ?? true;
    }

    private bool ShouldIncludeMemberClassMember(ISymbol member)
        => _options.MemberFilter?.Invoke(member) ?? true;

    private static string GetPreferredModuleFieldDeclaredName(IFieldSymbol symbol)
    {
        if (symbol.AssociatedSymbol is IPropertySymbol && symbol.IsImplicitlyDeclared)
            return Format.HashName(symbol.AssociatedSymbol!.OriginalDefinition.ToDisplayString(Format.NameFormat));

        return Util.GetConfigOrSymbolName(symbol);
    }

    /// <summary>
    /// 仅用于构建模块级保留名集合。
    /// 这里收集的是“声明过哪些名字”，而不是“这些名字在何处可见”，
    /// 因此服务的是保守冲突判定，不承担精确词法作用域解析职责。
    /// </summary>
    private sealed class DeclaredNameCollector : CSharpSyntaxWalker
    {
        public HashSet<string> Names { get; } = new(System.StringComparer.Ordinal);

        public override void VisitParameter(ParameterSyntax node)
        {
            Add(node.Identifier);
            base.VisitParameter(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            if (node.Parent?.Parent is FieldDeclarationSyntax &&
                node.Parent.Parent.Parent is ClassDeclarationSyntax)
            {
                return;
            }

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

        public override void VisitFromClause(FromClauseSyntax node)
        {
            Add(node.Identifier);
            base.VisitFromClause(node);
        }

        public override void VisitJoinClause(JoinClauseSyntax node)
        {
            Add(node.Identifier);
            base.VisitJoinClause(node);
        }

        public override void VisitJoinIntoClause(JoinIntoClauseSyntax node)
        {
            Add(node.Identifier);
            base.VisitJoinIntoClause(node);
        }

        public override void VisitLetClause(LetClauseSyntax node)
        {
            Add(node.Identifier);
            base.VisitLetClause(node);
        }

        public override void VisitQueryContinuation(QueryContinuationSyntax node)
        {
            Add(node.Identifier);
            base.VisitQueryContinuation(node);
        }

        private void Add(SyntaxToken identifier)
        {
            if (!identifier.IsKind(SyntaxKind.None) && !string.IsNullOrWhiteSpace(identifier.ValueText))
                Names.Add(identifier.ValueText);
        }
    }

    private string GetSymbolName(ISymbol symbol)
    {
        if (symbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol propertySymbol })
            return Util.GetConfigOrSymbolName(propertySymbol);

        return Util.GetConfigOrSymbolName(symbol);
    }

    /// <summary>
    /// 约定，C# 的Public 和 Internal 都是Public，其余都是private
    /// </summary>
    /// <param name="accessibility"></param>
    /// <returns></returns>
    private bool ShouldBePrivate(Accessibility accessibility)
        => accessibility != Accessibility.Public && accessibility != Accessibility.Internal;

    private bool ShouldExportModuleMember(ISymbol symbol)
        => _modulePolicy.ShouldExportModuleMember(_classSymbol, symbol);
}
