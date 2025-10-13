using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECMAScript.Compiler;

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
    private readonly List<ImportDeclaration> _imports = [];

    /// <summary>
    /// 将C# 14 ClassDeclarationSyntax 转换为Acornima.Ast.Module(es6+ module)
    /// </summary>
    /// <returns></returns>
    public Module? Convert()
    {
        // 检查是否为 public 顶层类型
        if (_classSymbol.DeclaredAccessibility != Accessibility.Public)
            throw new NotSupportedException($"类 {_classSymbol.Name} 不是 public，无法转换");

        if (_classSymbol.ContainingType != null)
            throw new NotSupportedException($"嵌套类 {_classSymbol.Name} 需要扁平化处理");

        var members = new List<Statement>();
        
        foreach (var member in _classSymbol.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol field:
                    members.Add(ConvertModuleField(field));
                    break;
                case IPropertySymbol prop:
                    members.AddRange(ConvertModuleProperty(prop));
                    break;
                case IMethodSymbol func when func.MethodKind == MethodKind.Ordinary:
                    members.Add(ConvertModuleMethod(func));
                    break;
                case INamedTypeSymbol @class when @class.TypeKind == TypeKind.Class:
                    members.Add(ConvertModuleClass(@class));
                    break;
                case INamedTypeSymbol @enum when @enum.TypeKind == TypeKind.Enum:
                    members.Add(ConvertModuleEnum(@enum));
                    break;
            }
        }

        var statements = NodeList.From(_imports.Concat(members));
        return statements.Count > 0
            ? new Module(statements)
            : null;
    }

    private VariableDeclaration ConvertVariableField(IFieldSymbol symbol)
    {
        var name = GetSymbolName(symbol);
        var init = symbol.HasConstantValue
            ? CreateEqualsValueClauseSyntaxLiteral(symbol.Type.SpecialType, symbol.ConstantValue)
            : null;
        var identifier = new Identifier(name);
        var kind = symbol.IsConst
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

    private Declaration ConvertModuleField(IFieldSymbol symbol)
    {
        var declaration = ConvertVariableField(symbol);

        if(ShouldBePrivate(symbol.DeclaredAccessibility))
            return declaration;
        else
            return new ExportNamedDeclaration(
                declaration,
                NodeList.From<ExportSpecifier>([]),
                null,
                NodeList.From<ImportAttribute>([]));
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
            var walker = new AstOperationWalker();
            body = walker.Visit(operation, operation) as FunctionBody
                ?? throw new NotSupportedException($"Jazor cannot suport {symbol.Name}.");
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

    private Declaration ConvertModuleMethod(IMethodSymbol symbol)
    {
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
        }

        FunctionBody body;
        if (operation is not null)
        {
            var walker = new AstOperationWalker();
            body = walker.Visit(operation, operation) as FunctionBody
                ?? throw new NotSupportedException($"Jazor 不支持转换方法 {symbol.Name}，无法从操作生成函数体。");
        }
        //如果没有方法体，并且是属性的get、set方法，则是自动属性
        else if (symbol.AssociatedSymbol?.Kind == SymbolKind.Property)
        {
            var backName = $"<{symbol.AssociatedSymbol.Name}>k__BackingField";
            var backField = new Identifier(backName);

            // 注意，顶层类需要扁平化，没有类结构，在扁平化Property时
            // 会自动检测backField生成一个VariableDeclaration，此处直接返回Identifier
            if (symbol.MethodKind == MethodKind.PropertyGet)
                body = new FunctionBody(
                    strict: true,
                    body: NodeList.From<Statement>(
                    new ReturnStatement(backField)));
            else
            {
                var value = new Identifier("value");
                body = new FunctionBody(
                    strict: true,
                    body: NodeList.From<Statement>(
                        new NonSpecialExpressionStatement(
                            new AssignmentExpression("=", backField, value))));
            }
        }
        else
            throw new NotSupportedException($"Jazor 不支持转换方法 {symbol.Name}，无法从操作生成函数体。");

        var parameters = new List<Node>();
        if (symbol.Parameters.Length > 0)
        {
            foreach (var p in symbol.Parameters)
            {
                var parameter = ConvertParameter(p)
                    ?? throw new NotSupportedException($"Jazor 不支持转换参数 {p.Name}，无法从符号生成参数节点。");
                parameters.Add(parameter);
            }
        }

        var name = GetSymbolName(symbol);
        var identifier = new Identifier(name);
        var declaration = new FunctionDeclaration(
            id: identifier,
            parameters: NodeList.From(parameters),
            body: body,
            generator: false,
            async: false);

        if (ShouldBePrivate(symbol.DeclaredAccessibility))
            return declaration;
        else
            return new ExportNamedDeclaration(
                declaration,
                NodeList.From<ExportSpecifier>([]),
                null,
                NodeList.From<ImportAttribute>([]));
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

    private List<Declaration> ConvertModuleProperty(IPropertySymbol symbol)
    {
        var declarations = new List<Declaration>();
        // 找出BackingField
        var backName = $"<{symbol.Name}>k__BackingField";
        var backingFieldSymbol = symbol.ContainingType
                .GetMembers(backName)
                .OfType<IFieldSymbol>()
                .FirstOrDefault();
        if (backingFieldSymbol is not null)
        {
            // BackingField 是私有的
            var backingFieldDecl = ConvertVariableField(backingFieldSymbol);
            declarations.Add(backingFieldDecl);
        }

        // 处理 getter
        if (symbol.GetMethod is not null)
        {
            var getFuncDecl = ConvertModuleMethod(symbol.GetMethod);
            declarations.Add(getFuncDecl);
        }

        // 处理 setter
        if (symbol.SetMethod is not null)
        {
            var setFuncDecl = ConvertModuleMethod(symbol.SetMethod);
            declarations.Add(setFuncDecl);
        }

        // 处理属性初始化器，如 int P { get; set; } = 42;
        // 属性初始化器 是只有自动属性或field关键字实现的属性才有
        // 会在BackingField的默认值上处理
        
        return declarations;
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
            var definition = new PropertyDefinition(
                    key: new Identifier(kv.Key),
                    value: new NumericLiteral(value: value, raw: raw),
                    computed: false,
                    isStatic: false,
                    decorators: NodeList.Empty<Decorator>()
                ) as Node;

            return definition;
        }));

        var init = new ObjectExpression(props);
        var name = GetSymbolName(symbol);
        var declarator = new VariableDeclarator(new Identifier(name), init);
        var declaration = new VariableDeclaration(VariableDeclarationKind.Const, NodeList.From([declarator]));

        return declaration;
    }

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
        if(value is null)
            throw new NotSupportedException($"Cannot convert null to literal.");

        var val = value.ToString();

        Expression right;
        if (type == SpecialType.None)
            return new NullLiteral("null");

        else if (IsNumeric(type))
            right = new NumericLiteral(value: System.Convert.ToDouble(value), raw: val);

        else if (type == SpecialType.System_String || type == SpecialType.System_Char)
        {
            var raw = val
                .Replace("\\", "\\\\")  // 必须先转义反斜杠
                .Replace("\"", "\\\"")
                .Replace("\0", "\\0")
                .Replace("\a", "\\a")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\v", "\\v");
                
            if (type == SpecialType.System_String)
                right = new StringLiteral(value: val, raw: $"\"{raw}\"");
            else
                right = new StringLiteral(value: val, raw: $"'{raw}'");
        }
        else if (type == SpecialType.System_Boolean)
            right = new BooleanLiteral(value: val.ToLower() == "true", raw: val.ToLower());

        else
            throw new NotSupportedException($"Cannot convert {value} value to literal.");

        return right;
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
