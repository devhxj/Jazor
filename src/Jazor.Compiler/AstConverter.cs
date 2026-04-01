using Acornima;
using Acornima.Ast;
using Jazor.Name;
using Microsoft.CodeAnalysis;
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
        foreach (var pair in _imports)
        {
            var specifierList = string.Join(", ", pair.Value.Select(static specifier => specifier.ToECMAScript()));
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
                var argument = new SenseArgument(Sense.FunctionBody);
                body = walker.Visit(operation, argument) as FunctionBody;
                MergeImports(argument);
            }
        }
        else if (expressionSyntax is not null)
        {
            var operation = _classModel.GetOperation(expressionSyntax);
            if (operation is not null)
            {
                var walker = new SemanticWalker(_classSymbol);
                var argument = new SenseArgument(Sense.Any);
                var stmt = walker.Visit(operation, argument) switch
                {
                    Statement s => s,
                    Expression e => symbol.ReturnsVoid
                        ? new NonSpecialExpressionStatement(e)
                        : new ReturnStatement(e),
                    _ => null
                };
                MergeImports(argument);
                if (stmt is not null)
                    body = new FunctionBody(NodeList.From(stmt), true);
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
            async: false);

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
        var name = GetSymbolName(symbol);
        var init = symbol.HasConstantValue
            ? CreateEqualsValueClauseSyntaxLiteral(symbol.Type.SpecialType, symbol.ConstantValue)
            : null;

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

    private MethodDefinition ConvertMemberMethod(IMethodSymbol symbol)
    {
        IOperation? operation = null;
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var methodDecl = (MethodDeclarationSyntax)reference.GetSyntax();
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

        var isProperty = symbol.AssociatedSymbol?.Kind == SymbolKind.Property;
        FunctionBody body;
        if (operation is not null)
        {
            var walker = new SemanticWalker();
            var argument = new SenseArgument(Sense.FunctionBody);
            body = walker.Visit(operation, argument) as FunctionBody
                ?? throw new NotSupportedException($"Jazor cannot suport {symbol.Name}.");
            MergeImports(argument);
        }
        //如果没有方法体，并且是属性的get、set方法，则是自动属性
        else if (isProperty)
        {
            var backName = $"<{symbol.AssociatedSymbol!.Name}>k__BackingField";
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
            throw new NotSupportedException($"Jazor cannot suport {symbol.Name}.");

        var parameters = new List<Node>();
        if (symbol.Parameters.Length > 0)
        {
            foreach (var p in symbol.Parameters)
            {
                var parameter = ConvertParameter(p)
                    ?? throw new NotSupportedException($"Jazor cannot suport {p.Name}.");
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
                async: false),
            computed: false,
            isStatic: symbol.IsStatic,
            decorators: NodeList.Empty<Decorator>()
        );
    }


    private List<ClassProperty> ConvertMemberProperty(IPropertySymbol symbol)
    {
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
            var setFuncDecl = ConvertMemberMethod(symbol.SetMethod);
            properties.Add(setFuncDecl);
        }

        return properties;
    }

    private ClassDeclaration ConvertMemberClass(INamedTypeSymbol symbol)
    {
        var nodes = new List<Node>();
        foreach (var member in symbol.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol field:
                    nodes.Add(ConvertMemberField(field));
                    break;
                case IPropertySymbol prop:
                    nodes.AddRange(ConvertMemberProperty(prop));
                    break;
                case IMethodSymbol func when func.MethodKind == MethodKind.Ordinary:
                    nodes.Add(ConvertMemberMethod(func));
                    break;
                default:
                    throw new NotSupportedException();
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

    private static Expression ConvertParameter(IParameterSymbol parameter)
    {
        var identifier = new Identifier(parameter.Name);
        if (parameter.HasExplicitDefaultValue)
        {
            var val = parameter.ExplicitDefaultValue;
            var right = CreateEqualsValueClauseSyntaxLiteral(parameter.Type.SpecialType, val);
            return new AssignmentExpression("=", identifier, right);
        }

        return identifier;
    }

    private static Expression CreateEqualsValueClauseSyntaxLiteral(SpecialType type, object? value)
    {
        if (value is null)
            throw new NotSupportedException($"Cannot convert null to literal.");

        if (type == SpecialType.None)
            return new NullLiteral("null");

        return CreateLiteralExpression(value);
    }

    private Expression CreateEqualsValueClauseSyntaxLiteral(EqualsValueClauseSyntax syntax)
    {
        var value = syntax.Value;

        // 仅处理字面量表达式
        if (value is LiteralExpressionSyntax lit)
            return CreateLiteralExpression(lit.Token.Value);

        var operation = _classModel.GetOperation(syntax);
        if (operation is not null)
        {
            var walker = new SemanticWalker(_classSymbol);
            var argument = new SenseArgument(Sense.Any);
            var expr = walker.Visit(operation, argument) as Expression;
            MergeImports(argument);
            if (expr is not null)
                return expr;
        }

        throw new NotSupportedException($"Only literal expressions are supported, got: {value.Kind()}");
    }

    private static FunctionBody ApplyRefOutReturnProtocol(FunctionBody body, IReadOnlyList<Expression> refParas, bool hasReturn)
    {
        var returnExpr = new ArrayExpression(NodeList.From(BuildReturnElements(null, refParas, hasReturn)));
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
            return new ReturnStatement(new ArrayExpression(NodeList.From(elements)));
        }

        protected override object VisitFunctionExpression(FunctionExpression node) => node;
        protected override object VisitArrowFunctionExpression(ArrowFunctionExpression node) => node;
    }

    private static Expression CreateLiteralExpression(object? value)
    {
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

    private string GetSymbolName(ISymbol symbol) => symbol.Name;

    /// <summary>
    /// 约定，C# 的Public 和 Internal 都是Public，其余都是private
    /// </summary>
    /// <param name="accessibility"></param>
    /// <returns></returns>
    private bool ShouldBePrivate(Accessibility accessibility)
        => accessibility != Accessibility.Public && accessibility != Accessibility.Internal;
}
