using Acornima;
using Acornima.Ast;
using Jazor.Name;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Jazor.Compiler;

/// <summary>
/// C# 到 JavaScript AST 转换器
/// 基于 Roslyn 将语义兼容的C#代码 AST 转换为 Acornima 的 AST (ESTree)
/// INamedTypeSymbol 的 TypeKind 是 TypeKind.Class
/// 当前classSymbol 对应的代码是一个public static 类，最终对应一个 Acornima es6 module
/// 内部发成员有公开的、私有的，有静态字段、静态属性、静态方法、类（非静态）、枚举（非静态）、接口、没有构造函数
/// 内部发成员若是非private的，均需要具名导出，禁止使用 export default
/// 静态字段 转换为 Acornima变量
/// 静态属性 转换为 Acornima方法（考虑get、set）
/// 静态方法 转换为 Acornima方法
/// 成员类 转换为 Acornima类
/// 成员枚举 转换为 Acornima静态对象
/// 其他的如接口、委托、事件等，都忽略
/// 对于代码段，基于operationwalker 根据 IOperation 生成 Acornima AST
/// </summary>
public class AstConverter(INamedTypeSymbol classSymbol, SemanticModel classModel)
{
    private readonly INamedTypeSymbol _classSymbol = classSymbol;
    private readonly SemanticModel _classModel = classModel;
    private readonly Dictionary<string, List<ImportDeclarationSpecifier>> _imports = [];
    private readonly Dictionary<string, string> _importBindings = [];
    private readonly Dictionary<string, string> _importLocalBindings = [];
    private readonly HashSet<string> _reservedImportNames = BuildReservedImportNames(classSymbol);

    /// <summary>
    /// 将C# 14 ClassDeclarationSyntax 转换为Acornima.Ast.Module(es6+ module)
    /// </summary>
    /// <returns></returns>
    public async Task<Module?> Convert()
    {
        // 检查是否为 public 顶层类型
        if (_classSymbol.DeclaredAccessibility != Accessibility.Public)
            throw new NotSupportedException($"类 {_classSymbol.Name} 不是 public，无法转换");

        if (_classSymbol.ContainingType != null)
            throw new NotSupportedException($"嵌套类 {_classSymbol.Name} 需要扁平化处理");

        var members = new List<Statement>();
        var a = _classSymbol.GetMembers();
        foreach (var member in _classSymbol.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol field:
                    await ConvertModuleField(members, field);
                    break;
                case IPropertySymbol:
                    break;                    
                // Property被Field和Method代替了
                case IMethodSymbol func:
                    await ConvertModuleMethod(members, func);
                    break;
                case INamedTypeSymbol @class when @class.TypeKind == TypeKind.Class:
                    members.Add(ConvertModuleClass(@class));
                    break;
                case INamedTypeSymbol @enum when @enum.TypeKind == TypeKind.Enum:
                    members.Add(ConvertModuleEnum(@enum));
                    break;
                default:
                    throw new NotSupportedException($"Jazor 模块类不支持{member.Kind}:{member.Name}。");
            }
        }

        var statements = NodeList.From(BuildImportDeclarations().Concat(members));
        return statements.Count > 0
            ? new Module(statements)
            : null;
    }

    private IEnumerable<ImportDeclaration> BuildImportDeclarations()
    {
        foreach (var pair in _imports.OrderBy(static pair => pair.Key, System.StringComparer.Ordinal))
        {
            var specifierList = string.Join(", ", pair.Value
                .OrderBy(static specifier => specifier.ToECMAScript(), System.StringComparer.Ordinal)
                .Select(static specifier => specifier.ToECMAScript()));
            var modulePath = EscapeJavaScriptString(pair.Key);
            var importScript = $"import {{ {specifierList} }} from \"{modulePath}\";";
            var importStatement = new Parser().ParseModule(importScript).Body.Single() as ImportDeclaration;
            if (importStatement is null)
                throw new NotSupportedException($"Jazor 无法生成模块导入：{pair.Key}");

            yield return importStatement;
        }
    }

    private void MergeImports(in SenseArgument argument)
    {
        foreach (var pair in argument.FlushImportSpecifiers())
        {
            if (_imports.TryGetValue(pair.Key, out var list))
            {
                foreach (var specifier in pair.Value)
                {
                    if (!list.Any(existing => existing.ToECMAScript() == specifier.ToECMAScript()))
                        list.Add(specifier);
                }
            }
            else
                _imports.Add(pair.Key, [.. pair.Value]);
        }
    }

    /// <summary>
    /// 为模块输出阶段创建共享导入上下文。
    /// 这里刻意让整个模块共用同一份导入绑定状态，
    /// 这样同一个外部符号在不同方法、字段初始化器或成员转换过程中
    /// 都会得到同一个本地名字，不会因为访问顺序不同而抖动。
    /// </summary>
    private SenseArgument CreateImportAwareArgument(Sense sense)
        => new SenseArgument(sense, UseImportAliases: true)
            .WithImportContext(_importBindings, _importLocalBindings, _reservedImportNames);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statements"></param>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private async Task ConvertModuleField(List<Statement> statements, IFieldSymbol symbol)
    {
        var declaration = await ConvertVariableField(symbol);
        if (ShouldBePrivate(symbol.DeclaredAccessibility))
            statements.Add(declaration);
        else
            statements.Add(new ExportNamedDeclaration(
                declaration,
                NodeList.From<ExportSpecifier>([]),
                null,
                NodeList.From<ImportAttribute>([])));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statements"></param>
    /// <param name="symbol"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private async Task ConvertModuleMethod(List<Statement> statements, IMethodSymbol symbol)
    {
        if (symbol.AssociatedSymbol is IEventSymbol eventSymbol)
            throw new NotSupportedException($"Jazor 模块类不支持Event:{eventSymbol.Name}。");

        if (symbol.MethodKind == MethodKind.SharedConstructor)
        {
            if (symbol.IsImplicitlyDeclared)
                return;

            throw new NotSupportedException($"Jazor 模块类{symbol.Name}不支持静态构造函数。");
        }

        if (symbol.MethodKind == MethodKind.PropertySet)
        {
            foreach (var reference in symbol.DeclaringSyntaxReferences)
            {
                if (await reference.GetSyntaxAsync() is AccessorDeclarationSyntax accessor &&
                    accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InitAccessorDeclaration))
                    return;
            }
        }

        if (symbol.IsInitOnly)
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
            var syntax = await reference.GetSyntaxAsync();
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
            var operation = _classModel.GetOperation(blockSyntax);
            if (operation is not null)
            {
                var walker = new SemanticWalker(_classSymbol);
                var argument = CreateImportAwareArgument(Sense.FunctionBody);
                body = MaterializeFunctionBody(walker.Visit(operation, argument), argument, symbol.ReturnsVoid);
                MergeImports(argument);
            }
        }
        else if (expressionSyntax is not null)
        {
            var operation = _classModel.GetOperation(expressionSyntax);
            if (operation is not null)
            {
                var walker = new SemanticWalker(_classSymbol);
                var argument = CreateImportAwareArgument(Sense.Any);
                var visited = walker.Visit(operation, argument);
                MergeImports(argument);
                body = MaterializeFunctionBody(visited, argument, symbol.ReturnsVoid);
            }
        }
        if (body is null)
            throw new NotSupportedException($"Jazor 不支持转换方法 {symbol.Name}，无法从操作生成函数体。");

        if (refParas.Count > 0)
            body = ApplyRefOutReturnProtocol(body, refParas, hasReturn);

        var name = Util.GetConfigOrSymbolName(symbol);
        var identifier = new Identifier(name);
        // todo:分析使用ArrowFunctionExpression的可能性
        var declaration = new FunctionDeclaration(
            id: identifier,
            parameters: NodeList.From(parameters),
            body: body,
            generator: false,
            async: symbol.IsAsync);

        if (ShouldBePrivate(symbol.DeclaredAccessibility))
            statements.Add(declaration);
        else
            statements.Add(new ExportNamedDeclaration(
                declaration,
                NodeList.From<ExportSpecifier>([]),
                null,
                NodeList.From<ImportAttribute>([])));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private Declaration ConvertModuleEnum(INamedTypeSymbol symbol)
    {
        var declaration = ConvertMemberEnum(symbol);
        if (ShouldBePrivate(symbol.DeclaredAccessibility))
            return declaration;
        else
            return new ExportNamedDeclaration(
                    declaration,
                    NodeList.From<ExportSpecifier>([]),
                    null,
                    NodeList.From<ImportAttribute>([]));
    }


    private async Task<VariableDeclaration> ConvertVariableField(IFieldSymbol symbol)
    {
        Expression? init = null;
        if (symbol.HasConstantValue)
            init = CreateEqualsValueClauseSyntaxLiteral(symbol.Type.SpecialType, symbol.ConstantValue);
        else
            foreach (var item in symbol.DeclaringSyntaxReferences)
            {
                var syntax = await item.GetSyntaxAsync() as VariableDeclaratorSyntax;
                if (syntax is not null && syntax.Initializer is not null)
                {
                    init = CreateEqualsValueClauseSyntaxLiteral(syntax.Initializer);
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
                    var syntax = await item.GetSyntaxAsync() as PropertyDeclarationSyntax;
                    if (syntax is not null && syntax.Initializer is not null)
                    {
                        init = CreateEqualsValueClauseSyntaxLiteral(syntax.Initializer);
                        break;
                    }
                }
            }
        }
        else
            name = Util.GetConfigOrSymbolName(symbol);

        var identifier = new Identifier(name);
        var kind = symbol.IsConst || isPropertyInitOnly
            ? VariableDeclarationKind.Const
            : VariableDeclarationKind.Let;
        var declarator = new VariableDeclarator(identifier, init);
        var declaration = new VariableDeclaration(kind, NodeList.From([declarator]));

        return declaration;
    }

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

    private FunctionBody? ConvertMemberOperationToFunctionBody(IOperation operation, bool returnsVoid)
    {
        var walker = new SemanticWalker();
        var argument = CreateImportAwareArgument(Sense.Any);
        var visited = walker.Visit(operation, argument);
        MergeImports(argument);

        return MaterializeFunctionBody(visited, argument, returnsVoid);
    }

    private MethodDefinition ConvertMemberMethod(IMethodSymbol symbol)
    {
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
                    operation = _classModel.GetOperation(methodDecl.Body);
                    break;
                }
                else if (methodDecl.ExpressionBody is not null)
                {
                    operation = _classModel.GetOperation(methodDecl.ExpressionBody);
                    break;
                }
            }
            else if (syntax is AccessorDeclarationSyntax accessorDecl)
            {
                if (accessorDecl.Body is not null)
                {
                    operation = _classModel.GetOperation(accessorDecl.Body);
                    break;
                }
                else if (accessorDecl.ExpressionBody is not null)
                {
                    operation = _classModel.GetOperation(accessorDecl.ExpressionBody);
                    break;
                }
            }
            else if (syntax is ArrowExpressionClauseSyntax arrowExpr)
            {
                operation = _classModel.GetOperation(arrowExpr.Expression);
                break;
            }
        }

        var isProperty = symbol.AssociatedSymbol?.Kind == SymbolKind.Property;
        FunctionBody body;
        if (operation is not null)
        {
            body = ConvertMemberOperationToFunctionBody(operation, symbol.ReturnsVoid)
                ?? throw new NotSupportedException($"Jazor member class failed to convert body for {symbol.Name}.");
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
            {
                var parameter = ConvertParameter(p)
                    ?? throw new NotSupportedException($"Jazor member class does not support parameter {p.Name} on {symbol.Name}.");
                parameters.Add(parameter);
            }
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


    private List<ClassProperty> ConvertMemberProperty(IPropertySymbol symbol)
    {
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
            var getFuncDecl = ConvertMemberMethod(symbol.GetMethod);
            properties.Add(getFuncDecl);
        }

        // 处理 setter
        if (symbol.SetMethod is not null)
        {
            if (!IsInitOnlyAccessor(symbol.SetMethod))
            {
                var setFuncDecl = ConvertMemberMethod(symbol.SetMethod);
                properties.Add(setFuncDecl);
            }
        }

        return properties;
    }

    private MethodDefinition ConvertMemberConstructor(IMethodSymbol symbol)
    {
        if (symbol.MethodKind == MethodKind.SharedConstructor)
            throw new NotSupportedException($"Jazor member class does not support static constructor {symbol.Name}.");

        if (symbol.MethodKind != MethodKind.Constructor)
            throw new NotSupportedException($"Jazor member class does not support constructor kind {symbol.MethodKind}:{symbol.Name}.");

        IOperation? operation = null;
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not ConstructorDeclarationSyntax ctorDecl)
                continue;

            if (ctorDecl.Initializer is not null)
                throw new NotSupportedException($"Jazor member class does not support constructor initializer on {symbol.Name}.");

            if (ctorDecl.Body is not null)
            {
                operation = _classModel.GetOperation(ctorDecl.Body);
                break;
            }

            if (ctorDecl.ExpressionBody is not null)
            {
                operation = _classModel.GetOperation(ctorDecl.ExpressionBody);
                break;
            }
        }

        if (operation is null)
            throw new NotSupportedException($"Jazor member class constructor {symbol.Name} requires a body.");

        var body = ConvertMemberOperationToFunctionBody(operation, returnsVoid: true)
            ?? throw new NotSupportedException($"Jazor member class failed to convert constructor body for {symbol.Name}.");

        var parameters = new List<Node>();
        if (symbol.Parameters.Length > 0)
        {
            foreach (var p in symbol.Parameters)
            {
                var parameter = ConvertParameter(p)
                    ?? throw new NotSupportedException($"Jazor member class does not support parameter {p.Name} on {symbol.Name}.");
                parameters.Add(parameter);
            }
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
            decorators: NodeList.Empty<Decorator>()
        );
    }

    private ClassDeclaration ConvertMemberClass(INamedTypeSymbol symbol)
    {
        var nodes = new List<Node>();
        foreach (var member in symbol.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol field when field.AssociatedSymbol is IPropertySymbol && field.IsImplicitlyDeclared:
                    break;
                case IFieldSymbol field:
                    nodes.Add(ConvertMemberField(field));
                    break;
                case IPropertySymbol prop:
                    nodes.AddRange(ConvertMemberProperty(prop));
                    break;
                case IMethodSymbol accessor when accessor.AssociatedSymbol is IPropertySymbol:
                    break;
                case IMethodSymbol ctor when ctor.MethodKind is MethodKind.Constructor or MethodKind.SharedConstructor:
                    if (!ctor.IsImplicitlyDeclared)
                        nodes.Add(ConvertMemberConstructor(ctor));
                    break;
                case IMethodSymbol accessor when accessor.AssociatedSymbol is IEventSymbol eventSymbol:
                    throw new NotSupportedException($"Jazor member class does not support Event:{eventSymbol.Name}.");
                case IMethodSymbol func when func.MethodKind == MethodKind.Ordinary:
                    nodes.Add(ConvertMemberMethod(func));
                    break;
                case IEventSymbol eventSymbol:
                    throw new NotSupportedException($"Jazor member class does not support Event:{eventSymbol.Name}.");
                default:
                    throw new NotSupportedException($"Jazor member class does not support {member.Kind}:{member.Name}.");
            }
        }

        var className = symbol.Name;
        var declaration = new ClassDeclaration(
            id: new Identifier(className),
            superClass: null,
            body: new ClassBody(NodeList.From(nodes)),
            decorators: NodeList.Empty<Decorator>()
        );

        return declaration;
    }

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

        return symbol.Name;
    }

    private static bool IsInitOnlyAccessor(IMethodSymbol method)
    {
        if (!method.IsInitOnly && method.MethodKind != MethodKind.PropertySet)
            return false;

        foreach (var reference in method.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is AccessorDeclarationSyntax accessor &&
                accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InitAccessorDeclaration))
                return true;
        }

        return method.IsInitOnly;
    }

    private Declaration ConvertModuleClass(INamedTypeSymbol symbol)
    {
        if (symbol.IsStatic)
            throw new NotSupportedException($"Jazor 模块类中不支持静态成员类{symbol.Name}。");
            
        var declaration = ConvertMemberClass(symbol);

        if (ShouldBePrivate(symbol.DeclaredAccessibility))
            return declaration;
        else
            return new ExportNamedDeclaration(
                declaration,
                NodeList.From<ExportSpecifier>([]),
                null,
                NodeList.From<ImportAttribute>([]));
    }

    private VariableDeclaration ConvertMemberEnum(INamedTypeSymbol symbol)
    {
        var fields = symbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.HasConstantValue)
            .ToDictionary(f => f.Name, f => f.ConstantValue);

        var props = NodeList.From(fields.Select(static kv =>
        {
            if (kv.Value is null)
                throw new NotSupportedException($"Cannot convert null to literal.");

            //枚举一般不会使用long，所以double足够
            var value = System.Convert.ToDouble(kv.Value);
            var raw = kv.Value.ToString();
            var definition = new ObjectProperty(
                    kind: PropertyKind.Init,
                    key: new Identifier(kv.Key),
                    value: new NumericLiteral(value: value, raw: raw),
                    computed: false,
                    shorthand: false,
                    method: false
                ) as Node;

            return definition;
        }));

        // 生成冻结的值面量对象
        var arg = new ObjectExpression(props);
        var init = new CallExpression(
            callee: new MemberExpression(
                obj: new Identifier("Object"),
                property: new Identifier("freeze"),
                computed: false,
                optional: false),
            args: NodeList.From<Expression>(arg),
            optional: false);
        var name = Util.GetConfigOrSymbolName(symbol);
        var declarator = new VariableDeclarator(new Identifier(name), init);
        var declaration = new VariableDeclaration(VariableDeclarationKind.Const, NodeList.From([declarator]));

        return declaration;
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

    private Expression CreateEqualsValueClauseSyntaxLiteral(EqualsValueClauseSyntax syntax)
    {
        var value = syntax.Value;

        // 仅处理字面量表达式
        if (value is LiteralExpressionSyntax lit)
            return CreateLiteralExpression(lit.Token.Value);

        var operation = _classModel.GetOperation(value) ?? _classModel.GetOperation(syntax);
        if (operation is not null)
        {
            var walker = new SemanticWalker(_classSymbol);
            var argument = CreateImportAwareArgument(Sense.Any);
            var expr = walker.Visit(operation, argument) as Expression;
            MergeImports(argument);
            if (expr is not null)
                return MaterializeExpression(expr, argument);
        }

        throw new NotSupportedException($"Only literal expressions are supported, got: {value.Kind()}");
    }

    private static FunctionBody? MaterializeFunctionBody(Node? visited, SenseArgument argument, bool returnsVoid)
    {
        if (visited is null)
            return null;

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
                return null;
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

    private static FunctionBody ApplyRefOutReturnProtocol(FunctionBody body, IReadOnlyList<Expression> refParas, bool hasReturn)
    {
        var returnExpr = new ArrayExpression(NodeList.From<Expression?>(BuildReturnElements(null, refParas, hasReturn)));
        var rewriter = new RefOutReturnRewriter(refParas, hasReturn);
        var rewritten = (FunctionBody)(rewriter.Visit(body) ?? body);
        if (!hasReturn)
        {
            var statements = rewritten.Body.ToList();
            statements.Add(new ReturnStatement(returnExpr));
            rewritten = new FunctionBody(NodeList.From(statements), rewritten.Strict);
        }

        return rewritten;

        static List<Expression> BuildReturnElements(Expression? returnValue, IReadOnlyList<Expression> refs, bool hasReturnValue)
        {
            var items = new List<Expression>();
            if (hasReturnValue)
                items.Add(returnValue ?? new Identifier("undefined"));
            items.AddRange(refs);
            return items;
        }
    }

    private sealed class RefOutReturnRewriter(IReadOnlyList<Expression> refParas, bool hasReturn) : AstRewriter
    {
        public bool HasReturnStatement { get; private set; }

        protected override object? VisitReturnStatement(ReturnStatement node)
        {
            HasReturnStatement = true;
            var elements = new List<Expression>();
            if (hasReturn)
                elements.Add(node.Argument ?? new Identifier("undefined"));
            elements.AddRange(refParas);
            return new ReturnStatement(new ArrayExpression(NodeList.From<Expression?>(elements)));
        }

        protected override object VisitFunctionExpression(FunctionExpression node) => node;
        protected override object VisitArrowFunctionExpression(ArrowFunctionExpression node) => node;
    }

    private static Expression CreateLiteralExpression(object? value)
    {
        if (value is not null && TryCreateSpecialLiteralExpression(value, out var special))
            return special;

        return value switch
        {
            null => new NullLiteral("null"),
            bool b => new BooleanLiteral(b, b.ToString().ToLowerInvariant()),
            char c => new StringLiteral(c.ToString(), $"\"{EscapeJavaScriptString(c.ToString())}\""),
            string s => new StringLiteral(s, $"\"{EscapeJavaScriptString(s)}\""),
            sbyte sb => new NumericLiteral(sb, sb.ToString(CultureInfo.InvariantCulture)),
            byte b => new NumericLiteral(b, b.ToString(CultureInfo.InvariantCulture)),
            short s => new NumericLiteral(s, s.ToString(CultureInfo.InvariantCulture)),
            ushort us => new NumericLiteral(us, us.ToString(CultureInfo.InvariantCulture)),
            int i => new NumericLiteral(i, i.ToString(CultureInfo.InvariantCulture)),
            uint ui => new NumericLiteral(ui, ui.ToString(CultureInfo.InvariantCulture)),
            long l => new BigIntLiteral(new BigInteger(l), $"{l.ToString(CultureInfo.InvariantCulture)}n"),
            ulong ul => new BigIntLiteral(new BigInteger(ul), $"{ul.ToString(CultureInfo.InvariantCulture)}n"),
            double d => new NumericLiteral(d, d.ToString("R", CultureInfo.InvariantCulture)),
            float f => new NumericLiteral(f, f.ToString("R", CultureInfo.InvariantCulture)),
            decimal dec => new NumericLiteral(System.Convert.ToDouble(dec), dec.ToString(CultureInfo.InvariantCulture)),
            _ => throw new NotSupportedException($"Unsupported literal type: {value.GetType()}")
        };
    }

    private static bool TryCreateSpecialLiteralExpression(object value, out Expression expression)
    {
        if (value.GetType().FullName == "System.Half")
        {
            var number = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            var raw = value is IFormattable formattable
                ? formattable.ToString("R", CultureInfo.InvariantCulture)
                : number.ToString("R", CultureInfo.InvariantCulture);
            expression = new NumericLiteral(number, raw);
            return true;
        }

        expression = null!;
        return false;
    }

    private static string EscapeJavaScriptString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\0", "\\0")
            .Replace("\a", "\\a")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\v", "\\v");
    }

    /// <summary>
    /// 收集模块级保留名。
    /// 这不是逐词法作用域的精确遮蔽分析，而是为导入绑定提供一个稳定的保守上界：
    /// 只要名字在模块成员或任意局部声明里出现过，就视为该名字可能与导入冲突。
    /// 这样会放大一部分本可直接使用原名的场景，但能避免漏判导致的错误绑定。
    /// </summary>
    private static HashSet<string> BuildReservedImportNames(INamedTypeSymbol classSymbol)
    {
        var names = new HashSet<string>(System.StringComparer.Ordinal);

        foreach (var member in classSymbol.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol field:
                    names.Add(GetModuleFieldDeclaredName(field));
                    break;
                case IMethodSymbol method when ShouldReserveModuleMethodName(method):
                    names.Add(Util.GetConfigOrSymbolName(method));
                    break;
                case INamedTypeSymbol type when type.TypeKind is TypeKind.Class or TypeKind.Enum:
                    names.Add(Util.GetConfigOrSymbolName(type));
                    break;
            }
        }

        foreach (var syntaxRef in classSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not ClassDeclarationSyntax classSyntax)
                continue;

            var collector = new DeclaredNameCollector();
            collector.Visit(classSyntax);
            names.UnionWith(collector.Names);
        }

        return names;
    }

    private static bool ShouldReserveModuleMethodName(IMethodSymbol method)
    {
        if (method.MethodKind == MethodKind.SharedConstructor && method.IsImplicitlyDeclared)
            return false;

        if (method.IsInitOnly)
            return false;

        return method.MethodKind is MethodKind.Ordinary or MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.SharedConstructor;
    }

    private static string GetModuleFieldDeclaredName(IFieldSymbol symbol)
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

	private static bool IsNumeric(SpecialType type)
    {
        return type switch
        {
            SpecialType.System_SByte or
            SpecialType.System_Byte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_Decimal => true,
            _ => false,
        };
    }

    private string GetSymbolName(ISymbol symbol)
    {
        if (symbol is IMethodSymbol method &&
            (method.MethodKind == MethodKind.PropertyGet || method.MethodKind == MethodKind.PropertySet))
        {
            if (method.AssociatedSymbol is IPropertySymbol property)
                return property.Name;

            if (method.Name.StartsWith("get_", StringComparison.Ordinal) || method.Name.StartsWith("set_", StringComparison.Ordinal))
                return method.Name.Substring(4);
        }

        if (symbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol propertySymbol })
            return propertySymbol.Name;

        return symbol.Name;
    }

    /// <summary>
    /// 约定，C# 的Public 和 Internal 都是Public，其余都是private
    /// </summary>
    /// <param name="accessibility"></param>
    /// <returns></returns>
    private bool ShouldBePrivate(Accessibility accessibility)
        => accessibility != Accessibility.Public && accessibility != Accessibility.Internal;
}
