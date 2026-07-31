using Acornima;
using Acornima.Ast;
using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        ConstructorInitializerSyntax? InitializerSyntax,
        IMethodSymbol? BaseConstructorSymbol,
        FunctionBody Body,
        string HelperName);

    private readonly INamedTypeSymbol _classSymbol = classSymbol;
    private readonly SemanticModel _classModel = classModel;
    private readonly AstConverterOptions _options = options ?? AstConverterOptions.Default;
    private readonly Dictionary<string, List<ImportDeclarationSpecifier>> _imports = [];
    private readonly Dictionary<string, string> _importBindings = [];
    private readonly Dictionary<string, string> _importLocalBindings = [];
    private readonly ModuleNamePlan _moduleNamePlan = BuildModuleNamePlan(
        classSymbol,
        options?.MemberFilter,
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
            throw new NotSupportedException($"类 {_classSymbol.Name} 不是 public，无法转换");

        if (_classSymbol.ContainingType != null)
            throw new NotSupportedException($"嵌套类 {_classSymbol.Name} 需要扁平化处理");

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
                    throw new NotSupportedException($"Jazor 模块类不支持{member.Kind}:{member.Name}。");
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
        var referencedIdentifiers = CollectReferencedIdentifiers(members);
        foreach (var pair in _imports.OrderBy(static pair => pair.Key, System.StringComparer.Ordinal))
        {
            var uniqueSpecifiers = pair.Value
                .Where(specifier => ShouldRetainImportSpecifier(specifier, referencedIdentifiers))
                .ToArray();

            if (uniqueSpecifiers.Length > 0 &&
                string.Equals(
                    ECMAScriptModulePath.NormalizeImportSpecifier(pair.Key),
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
        => specifier switch
        {
            ImportSpecifier named => referencedIdentifiers.Contains(named.Local.Name),
            ImportDefaultSpecifier @default => referencedIdentifiers.Contains(@default.Local.Name),
            ImportNamespaceSpecifier @namespace => referencedIdentifiers.Contains(@namespace.Local.Name),
            _ => true
        };

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

    private SemanticWalker CreateSemanticWalker(CancellationToken cancellationToken)
        => new(_classSymbol, ModuleDeclaredNames, cancellationToken)
        {
            Host = _semanticWalkerHost
        };

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
        if (ShouldBePrivate(symbol.DeclaredAccessibility))
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
            throw new NotSupportedException($"Jazor 模块类不支持Event:{eventSymbol.Name}。");

        if (symbol.MethodKind == MethodKind.SharedConstructor)
        {
            if (symbol.IsImplicitlyDeclared)
                return;

            throw new NotSupportedException($"Jazor 模块类{symbol.Name}不支持静态构造函数。");
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
            var operation = GetSemanticModel(blockSyntax).GetOperation(blockSyntax);
            if (operation is not null)
            {
                var walker = CreateSemanticWalker(cancellationToken);
                var argument = CreateImportAwareArgument(Sense.FunctionBody);
                body = MaterializeFunctionBody(walker.Visit(operation, argument)!, argument, symbol.ReturnsVoid);
                MergeImports(argument);
            }
            else
            {
                throw CreateMissingOperationException(symbol, blockSyntax);
            }
        }
        else if (expressionSyntax is not null)
        {
            var operation = GetSemanticModel(expressionSyntax).GetOperation(expressionSyntax);
            if (operation is not null)
            {
                var walker = CreateSemanticWalker(cancellationToken);
                var argument = CreateImportAwareArgument(Sense.Any);
                var visited = walker.Visit(operation, argument)!;
                MergeImports(argument);
                body = MaterializeFunctionBody(visited, argument, symbol.ReturnsVoid);
            }
            else
            {
                throw CreateMissingOperationException(symbol, expressionSyntax);
            }
        }
        if (body is null)
            throw new NotSupportedException($"Jazor 不支持转换方法 {symbol.Name}，无法从操作生成函数体。");

        if (refParas.Count > 0)
            body = RefOutReturnProtocol.Apply(body, refParas, hasReturn);

        var localName = GetModuleDeclaredName(symbol);
        var identifier = new Identifier(localName);
        // todo:分析使用ArrowFunctionExpression的可能性
        var declaration = new FunctionDeclaration(
            id: identifier,
            parameters: NodeList.From(parameters),
            body: body,
            generator: false,
            async: symbol.IsAsync);

        if (ShouldBePrivate(symbol.DeclaredAccessibility))
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
        memberName = string.Empty;
        if (_options.Profile != AstConverterProfile.ClrRuntime)
            return false;

        var annotatedSymbol = symbol.AssociatedSymbol ?? (ISymbol)symbol;
        foreach (var attribute in annotatedSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is not ("JazorAttribute" or "Jazor") ||
                attribute.ConstructorArguments.Length < 2 ||
                !IsImportOperation(attribute.ConstructorArguments[0]) ||
                attribute.ConstructorArguments[1].Value is not string { Length: > 0 } authoredMemberName)
            {
                continue;
            }

            memberName = authoredMemberName;
            return true;
        }

        return false;
    }

    private static bool IsImportOperation(TypedConstant argument)
        => argument.Value is not null &&
           System.Convert.ToInt32(argument.Value, CultureInfo.InvariantCulture) == (int)Op.Import;

    private static NotSupportedException CreateMissingOperationException(ISymbol symbol, SyntaxNode syntax)
    {
        var lineSpan = syntax.GetLocation().GetLineSpan();
        var path = string.IsNullOrWhiteSpace(lineSpan.Path) ? "<unknown>" : lineSpan.Path;
        var start = lineSpan.StartLinePosition;
        var kind = syntax.Kind().ToString();
        var snippet = syntax.ToString().Replace("\r", string.Empty).Replace("\n", "\\n");
        return new NotSupportedException(
            $"Jazor 不支持转换方法 {symbol.Name}，Roslyn 未返回操作树。Kind={kind} Location={path}:{start.Line + 1}:{start.Character + 1} Syntax={snippet}");
    }
    private async Task<(VariableDeclaration Declaration, string LocalName)> ConvertVariableField(IFieldSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Expression? init = null;
        if (symbol.HasConstantValue)
            init = CreateEqualsValueClauseSyntaxLiteral(symbol.Type.SpecialType, symbol.ConstantValue);
        else
            foreach (var item in symbol.DeclaringSyntaxReferences)
            {
                var syntax = await item.GetSyntaxAsync(cancellationToken) as VariableDeclaratorSyntax;
                if (syntax is not null && syntax.Initializer is not null)
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
                    var syntax = await item.GetSyntaxAsync(cancellationToken) as PropertyDeclarationSyntax;
                    if (syntax is not null && syntax.Initializer is not null)
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

    private PropertyDefinition ConvertMemberField(IFieldSymbol symbol)
    {
        var name = GetMemberFieldDeclaredName(symbol);
        var init = GetMemberFieldInitializer(symbol);

        Expression identifier = ShouldBePrivate(symbol.DeclaredAccessibility)
            ? new PrivateIdentifier(name)
            : new Identifier(name);
        return new PropertyDefinition(
            key: identifier,
            value: init,
            computed: false,
            isStatic: symbol.IsStatic,
            decorators: NodeList.Empty<Decorator>()
        );
    }

    private Expression? GetMemberFieldInitializer(IFieldSymbol symbol)
    {
        if (symbol.HasConstantValue)
            return CreateEqualsValueClauseSyntaxLiteral(symbol.Type.SpecialType, symbol.ConstantValue);

        foreach (var item in symbol.DeclaringSyntaxReferences)
        {
            if (item.GetSyntax() is VariableDeclaratorSyntax syntax && syntax.Initializer is not null)
                return CreateEqualsValueClauseSyntaxLiteral(syntax.Initializer);
        }

        if (symbol.AssociatedSymbol is IPropertySymbol property)
        {
            foreach (var item in property.DeclaringSyntaxReferences)
            {
                if (item.GetSyntax() is PropertyDeclarationSyntax syntax && syntax.Initializer is not null)
                    return CreateEqualsValueClauseSyntaxLiteral(syntax.Initializer);
            }
        }

        return null;
    }

    private FunctionBody ConvertMemberOperationToFunctionBody(
        IOperation operation,
        bool returnsVoid,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var walker = CreateSemanticWalker(cancellationToken);
        var argument = CreateImportAwareArgument(Sense.Any);
        var visited = walker.Visit(operation, argument)!;
        MergeImports(argument);

        return MaterializeFunctionBody(visited, argument, returnsVoid);
    }

    private MethodDefinition ConvertMemberMethod(IMethodSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (symbol.AssociatedSymbol is IEventSymbol eventSymbol)
            throw new NotSupportedException($"Jazor member class does not support Event:{eventSymbol.Name}.");

        if (symbol.IsAbstract)
        {
            if (symbol.AssociatedSymbol is IPropertySymbol propertySymbol)
                throw new NotSupportedException($"Jazor member class does not support abstract property {propertySymbol.Name}.");

            throw new NotSupportedException($"Jazor member class does not support abstract method {symbol.Name}.");
        }

        IOperation? operation = null;
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var syntax = reference.GetSyntax();
            if (syntax is MethodDeclarationSyntax methodDecl)
            {
                if (methodDecl.Body is not null)
                {
                    operation = GetSemanticModel(methodDecl.Body).GetOperation(methodDecl.Body);
                    break;
                }
                else if (methodDecl.ExpressionBody is not null)
                {
                    operation = GetSemanticModel(methodDecl.ExpressionBody).GetOperation(methodDecl.ExpressionBody);
                    break;
                }
            }
            else if (syntax is AccessorDeclarationSyntax accessorDecl)
            {
                if (accessorDecl.Body is not null)
                {
                    operation = GetSemanticModel(accessorDecl.Body).GetOperation(accessorDecl.Body);
                    break;
                }
                else if (accessorDecl.ExpressionBody is not null)
                {
                    operation = GetSemanticModel(accessorDecl.ExpressionBody).GetOperation(accessorDecl.ExpressionBody);
                    break;
                }
            }
            else if (syntax is ArrowExpressionClauseSyntax arrowExpr)
            {
                operation = GetSemanticModel(arrowExpr.Expression).GetOperation(arrowExpr.Expression);
                break;
            }
        }

        var isProperty = symbol.AssociatedSymbol?.Kind == SymbolKind.Property;
        FunctionBody body;
        if (operation is not null)
        {
            body = ConvertMemberOperationToFunctionBody(operation, symbol.ReturnsVoid, cancellationToken);
        }
        // Body-less property accessors only map to auto-properties.
        else if (isProperty)
        {
            var backName = GetMemberBackingFieldName((IPropertySymbol)symbol.AssociatedSymbol!);
            var backField = new PrivateIdentifier(backName);

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

        var name = GetSymbolName(symbol);
        var key = new Identifier(name);

        var propertyKind = isProperty
            ? (symbol.MethodKind == MethodKind.PropertyGet ? PropertyKind.Get : PropertyKind.Set)
            : PropertyKind.Method;

        return new MethodDefinition(
            propertyKind,
            key: key,
            value: new FunctionExpression(
                id: null,
                parameters: NodeList.From(parameters),
                body: body,
                generator: false,
                async: symbol.IsAsync),
            computed: false,
            isStatic: symbol.IsStatic,
            decorators: NodeList.Empty<Decorator>()
        );
    }


    private List<ClassProperty> ConvertMemberProperty(IPropertySymbol symbol, CancellationToken cancellationToken)
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
            var backingFieldDecl = ConvertMemberField(backingFieldSymbol);
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
            : PrependSuperConstructorCall(lowering.Body, lowering.InitializerSyntax, lowering.BaseConstructorSymbol, cancellationToken);

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
                branchStatements.Add(CreateSuperConstructorCallStatement(lowering.InitializerSyntax, lowering.BaseConstructorSymbol, cancellationToken));

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

        if (baseType is null &&
            symbol.BaseType is INamedTypeSymbol unresolvedBaseType &&
            unresolvedBaseType.SpecialType != SpecialType.System_Object)
            throw new NotSupportedException($"Jazor member class does not support inheritance {symbol.Name} : {unresolvedBaseType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}.");

        var nodes = new List<Node>();
        var constructorLowerings = GetMemberConstructorLowerings(symbol, baseType, cancellationToken);
        var hasExplicitConstructor = constructorLowerings.Count > 0;
        var constructorsEmitted = false;

        foreach (var member in symbol.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!ShouldIncludeMemberClassMember(member))
                continue;

            switch (member)
            {
                case IFieldSymbol field when field.AssociatedSymbol is IPropertySymbol && field.IsImplicitlyDeclared:
                    break;
                case IFieldSymbol field:
                    nodes.Add(ConvertMemberField(field));
                    break;
                case IPropertySymbol prop:
                    nodes.AddRange(ConvertMemberProperty(prop, cancellationToken));
                    break;
                case IMethodSymbol accessor when accessor.AssociatedSymbol is IPropertySymbol:
                    break;
                case IMethodSymbol ctor when ctor.MethodKind == MethodKind.Constructor:
                    if (!ctor.IsImplicitlyDeclared && !constructorsEmitted)
                    {
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
                case IMethodSymbol accessor when accessor.AssociatedSymbol is IEventSymbol eventSymbol:
                    throw new NotSupportedException($"Jazor member class does not support Event:{eventSymbol.Name}.");
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
                    _options.Profile == AstConverterProfile.RazorVueRuntime &&
                    ModuleDeclaredNames.ContainsKey(nestedClass.OriginalDefinition):
                    // RazorVue runtime helper classes are flattened to artifact-module scope
                    // by RazorVueCompilerModuleContext so type references keep one stable
                    // compiler-owned declared-name context.
                    break;
                case IEventSymbol eventSymbol:
                    throw new NotSupportedException($"Jazor member class does not support Event:{eventSymbol.Name}.");
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

        IOperation? operation = null;
        ConstructorInitializerSyntax? initializerSyntax = null;
        IMethodSymbol? baseConstructorSymbol = null;
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not ConstructorDeclarationSyntax ctorDecl)
                continue;

            initializerSyntax = ctorDecl.Initializer;
            if (ctorDecl.Initializer is not null &&
                (baseType is null || !ctorDecl.Initializer.ThisOrBaseKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.BaseKeyword)))
                throw new NotSupportedException($"Jazor member class does not support constructor initializer on {symbol.Name}.");

            if (ctorDecl.Initializer is not null)
                baseConstructorSymbol = GetSemanticModel(ctorDecl.Initializer).GetSymbolInfo(ctorDecl.Initializer).Symbol as IMethodSymbol;

            if (ctorDecl.Body is not null)
            {
                operation = GetSemanticModel(ctorDecl.Body).GetOperation(ctorDecl.Body);
                break;
            }

            if (ctorDecl.ExpressionBody is not null)
            {
                operation = GetSemanticModel(ctorDecl.ExpressionBody).GetOperation(ctorDecl.ExpressionBody);
                break;
            }
        }

        if (operation is null)
            throw new NotSupportedException($"Jazor member class constructor {symbol.Name} requires a body.");

        var body = ConvertMemberOperationToFunctionBody(operation, returnsVoid: true, cancellationToken);

        return new MemberConstructorLowering(
            Symbol: symbol,
            InitializerSyntax: initializerSyntax,
            BaseConstructorSymbol: baseConstructorSymbol,
            Body: body,
            HelperName: GetMemberConstructorHelperName(symbol));
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
        ConstructorInitializerSyntax? initializerSyntax,
        IMethodSymbol? baseConstructorSymbol,
        CancellationToken cancellationToken)
        => new NonSpecialExpressionStatement(
            new CallExpression(
                new Super(),
                CreateConstructorInitializerArguments(initializerSyntax, baseConstructorSymbol, cancellationToken),
                optional: false));

    private static string GetMemberConstructorHelperName(IMethodSymbol symbol)
        => Util.GetMemberConstructorHelperName(symbol);

    private string GetMemberBackingFieldName(IPropertySymbol property)
    {
        var backingField = property.ContainingType
            .GetMembers($"<{property.Name}>k__BackingField")
            .OfType<IFieldSymbol>()
            .FirstOrDefault();

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

    private IEnumerable<Statement> ConvertModuleClass(
        INamedTypeSymbol symbol,
        INamedTypeSymbol? baseType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (symbol.IsStatic)
            throw new NotSupportedException($"Jazor 模块类中不支持静态成员类{symbol.Name}。");
            
        var declaration = ConvertMemberClass(symbol, baseType, cancellationToken);

        if (ShouldBePrivate(symbol.DeclaredAccessibility))
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
        ConstructorInitializerSyntax? initializerSyntax,
        IMethodSymbol? baseConstructorSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var statements = new List<Statement>
        {
            new NonSpecialExpressionStatement(
                new CallExpression(
                    new Super(),
                    CreateConstructorInitializerArguments(initializerSyntax, baseConstructorSymbol, cancellationToken),
                    optional: false))
        };

        statements.AddRange(body.Body);
        return new FunctionBody(NodeList.From(statements), body.Strict);
    }

    private NodeList<Expression> CreateConstructorInitializerArguments(
        ConstructorInitializerSyntax? initializerSyntax,
        IMethodSymbol? baseConstructorSymbol,
        CancellationToken cancellationToken)
    {
        var arguments = initializerSyntax?.ArgumentList is null
            ? []
            : initializerSyntax.ArgumentList.Arguments
                .Select(arg => ConvertConstructorInitializerArgument(arg, cancellationToken))
                .ToList();

        if (baseConstructorSymbol is not null && HasMultipleExplicitConstructors(baseConstructorSymbol.ContainingType))
            arguments.Insert(0, JavaScriptAstFactory.CreateStringLiteral(GetMemberConstructorHelperName(baseConstructorSymbol)));

        return NodeList.From(arguments);
    }

    private static bool HasMultipleExplicitConstructors(INamedTypeSymbol? typeSymbol)
        => typeSymbol?.InstanceConstructors.Count(static ctor => !ctor.IsImplicitlyDeclared) > 1;

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
        foreach (var reference in parameter.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is ParameterSyntax syntax && syntax.Default is not null)
                return CreateEqualsValueClauseSyntaxLiteral(syntax.Default);
        }

        return CreateEqualsValueClauseSyntaxLiteral(parameter.Type.SpecialType, parameter.ExplicitDefaultValue);
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

        if (value is LiteralExpressionSyntax lit)
            return CreateLiteralExpression(lit.Token.Value);

        var operation = GetSemanticModel(value).GetOperation(value);
        if (operation is not null)
        {
            var walker = CreateSemanticWalker(cancellationToken);
            var argument = CreateImportAwareArgument(Sense.Any);
            var expr = walker.Visit(operation, argument) as Expression;
            MergeImports(argument);
            if (expr is not null)
                return MaterializeExpression(expr, argument);
        }

        throw new NotSupportedException($"Only literal expressions are supported, got: {value.Kind()}");
    }

    private static FunctionBody MaterializeFunctionBody(Node visited, SenseArgument argument, bool returnsVoid)
    {
        if (visited is FunctionBody body)
        {
            var statements = MaterializeTemporaryDeclarationPrefix(argument);
            if (statements.Count == 0)
                return body;

            statements.AddRange(body.Body);
            return new FunctionBody(NodeList.From(statements), body.Strict);
        }

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
        => EnumerateModuleMembers(_classSymbol, _options.Profile);

    private static IEnumerable<ISymbol> EnumerateModuleMembers(
        INamedTypeSymbol classSymbol,
        AstConverterProfile profile)
    {
        if (profile != AstConverterProfile.RazorVueRuntime)
            return classSymbol.GetMembers();

        var types = new Stack<INamedTypeSymbol>();
        for (var current = classSymbol; current is { SpecialType: not SpecialType.System_Object }; current = current.BaseType)
            types.Push(current);

        var members = new List<ISymbol>();
        while (types.Count > 0)
            members.AddRange(types.Pop().GetMembers());

        return members;
    }

    private static ModuleNamePlan BuildModuleNamePlan(
        INamedTypeSymbol classSymbol,
        Func<ISymbol, bool>? includeMember,
        AstConverterProfile profile)
    {
        var localNames = BuildModuleLocalNames(classSymbol, profile);
        var declaredNames = BuildModuleDeclaredNames(classSymbol, localNames, includeMember, profile);
        var reservedImportNames = BuildReservedImportNames(classSymbol, declaredNames, localNames, includeMember, profile);
        return new ModuleNamePlan(localNames, declaredNames, reservedImportNames);
    }

    private string GetModuleDeclaredName(ISymbol symbol)
        => ModuleDeclaredNames.TryGetValue(symbol.OriginalDefinition, out var name)
            ? name
            : GetPreferredModuleDeclaredName(symbol);

    private static string GetModuleNamedExportName(ISymbol symbol)
        => symbol switch
        {
            INamedTypeSymbol type => Util.GetConfigOrSymbolName(type),
            _ => Util.GetConfigOrSymbolName(symbol)
        };

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
        if (ShouldBePrivate(symbol.DeclaredAccessibility))
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

    private static HashSet<string> BuildModuleLocalNames(INamedTypeSymbol classSymbol, AstConverterProfile profile)
    {
        var names = new HashSet<string>(System.StringComparer.Ordinal);
        foreach (var type in EnumerateModuleTypes(classSymbol, profile))
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

    private static IEnumerable<INamedTypeSymbol> EnumerateModuleTypes(
        INamedTypeSymbol classSymbol,
        AstConverterProfile profile)
    {
        if (profile != AstConverterProfile.RazorVueRuntime)
        {
            yield return classSymbol;
            yield break;
        }

        var types = new Stack<INamedTypeSymbol>();
        for (var current = classSymbol; current is { SpecialType: not SpecialType.System_Object }; current = current.BaseType)
            types.Push(current);

        while (types.Count > 0)
            yield return types.Pop();
    }

    private static Dictionary<ISymbol, string> BuildModuleDeclaredNames(
        INamedTypeSymbol classSymbol,
        HashSet<string> localNames,
        Func<ISymbol, bool>? includeMember,
        AstConverterProfile profile)
    {
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
        var usedDeclaredNames = new HashSet<string>(System.StringComparer.Ordinal);

        foreach (var member in EnumerateModuleMembers(classSymbol, profile))
        {
            if (includeMember is not null && !includeMember(member))
                continue;

            switch (member)
            {
                case IFieldSymbol field:
                    declaredNames[field.OriginalDefinition] = ChooseModuleDeclaredName(field, usedDeclaredNames, localNames, profile);
                    break;
                case IMethodSymbol method when ShouldReserveModuleMethodName(method):
                    declaredNames[method.OriginalDefinition] = ChooseModuleDeclaredName(method, usedDeclaredNames, localNames, profile);
                    break;
                case INamedTypeSymbol type when IsRuntimeMemberClass(type):
                    declaredNames[type.OriginalDefinition] = ChooseModuleDeclaredName(type, usedDeclaredNames, localNames, profile);
                    break;
            }
        }

        return declaredNames;
    }

    private static string ChooseModuleDeclaredName(
        ISymbol symbol,
        HashSet<string> usedDeclaredNames,
        HashSet<string> localNames,
        AstConverterProfile profile)
    {
        var preferredName = GetPreferredModuleDeclaredName(symbol, profile);
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

    private static string GetPreferredModuleDeclaredName(ISymbol symbol)
        => GetPreferredModuleDeclaredName(symbol, AstConverterProfile.Standard);

    private static string GetPreferredModuleDeclaredName(ISymbol symbol, AstConverterProfile profile)
        => symbol switch
        {
            IFieldSymbol field => GetPreferredModuleFieldDeclaredName(field),
            IMethodSymbol
            {
                MethodKind: MethodKind.PropertyGet,
                AssociatedSymbol: IPropertySymbol property
            } when profile == AstConverterProfile.RazorVueRuntime => Util.GetConfigOrSymbolName(property),
            IMethodSymbol method => Util.GetConfigOrSymbolName(method),
            INamedTypeSymbol type => Util.GetConfigOrSymbolName(type),
            _ => Util.GetConfigOrSymbolName(symbol)
        };

    private static string? GetSourceDeclaredNameCandidate(ISymbol symbol)
        => symbol switch
        {
            IFieldSymbol field when field.AssociatedSymbol is IPropertySymbol property && !field.IsImplicitlyDeclared => property.Name,
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
        AstConverterProfile profile)
    {
        var names = new HashSet<string>(System.StringComparer.Ordinal);

        foreach (var member in EnumerateModuleMembers(classSymbol, profile))
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
        {
            names.Add(localName);
            names.Add(Util.ConvertPascalCaseIdentifierToJsNaming(localName));
        }

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
        => _options.Profile switch
        {
            AstConverterProfile.ClrRuntime => accessibility is Accessibility.Public or Accessibility.Internal,
            AstConverterProfile.RazorVueRuntime => accessibility is Accessibility.Public or Accessibility.Internal,
            _ => accessibility == Accessibility.Public
        };

    private bool ShouldIncludeModuleMember(ISymbol member)
        => _options.MemberFilter?.Invoke(member) ?? true;

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
}
