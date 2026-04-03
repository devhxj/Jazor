using Acornima;
using Acornima.Ast;
using Jazor.Name;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Jazor.Compiler;

/// <summary>
/// C# Roslyn 操作树到 JavaScript Acornima AST 的转换器
/// <para><b>转换器功能范围</b></para>
/// 支持将方法体、静态字段初始值、属性 getter/setter、构造函数初始值设定项、局部函数、匿名函数/Lambda 转换为 Acornima AST。
/// <para><b>核心转换原则</b></para>
/// 1. <b>语义等价性</b>：确保 C# 和 JavaScript 之间的语义完全等价，禁止任何形式的简化处理
/// 2. <b>直接AST构造</b>：必须直接构造目标AST节点，禁止使用Parser进行解析
/// 3. <b>空值安全处理</b>：构造AST节点时必须先检查输入值是否为null，避免NullReferenceException
/// 4. <b>编译时优化</b>：利用C#强类型系统的编译时信息直接生成最简AST，避免不必要的运行时检测
/// 5. <b>方法复用原则</b>：优先复用现有的Visit方法，避免为相似语义创建多个独立生成方法
/// <para><b>性能优化策略</b></para>
/// - 利用编译时类型信息避免运行时类型检测
/// - 对于强类型到弱类型转换，依赖编译时类型安全
/// - 生成最简洁的JavaScript代码，避免复杂的IIFE包装（除非必要）
/// - 递归深度控制，防止栈溢出
/// </summary>
public sealed partial class SemanticWalker : OperationVisitor<SenseArgument, Node?>, IWhiteList
{
    private static readonly NullLiteral Null = new("null");

    private static readonly Identifier Undefined = new("undefined");

    private static readonly MemberExpression IsArrayExpr = new(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    private static (TypeMapper Mapper, string TypeName) GetMapperType(ITypeSymbol typeSymbol)
    {
        // 类型映射
        // object、匿名类型、元组类型 -> js object
        // string -> js string
        // byte、sbyte、short、ushort、int、uint、decimal、double、float -> js Number
        // long、ulong、Int128、UInt128、timestamp、BigInteger ->js BigInt
        // DateOnly、DateTime -> js Date
        // DateTimeOffset -> js Object wrapper
        // TimeOnly -> js BigInt（ticks）
        // Array -> js array
        // IDictionary -> js Map
        // IEnumerable(非IDictionary) -> js Set
        // 其他 class -> js class
        // 其他类型不支持 -> Unknown

        var displayName = typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        if (typeSymbol.IsTupleType || typeSymbol.IsAnonymousType)
            return (TypeMapper.Object, "Object");

        // 使用 SpecialType 进行基础类型检查，更加类型安全和高效
        switch (typeSymbol.OriginalDefinition.SpecialType)
        {
            case SpecialType.System_Char:
            case SpecialType.System_String:
                return (TypeMapper.String, "String");
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                return (TypeMapper.Number, "Number");
            case SpecialType.System_Boolean:
                return (TypeMapper.Boolean, "Boolean");
            case SpecialType.System_Object:
                return (TypeMapper.Object, "Object");
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                return (TypeMapper.BigInt, "BigInt");
            case SpecialType.System_DateTime:
                return (TypeMapper.Date, "Date");
            default:
                {
                    // Array 类型检查
                    if (typeSymbol.TypeKind == TypeKind.Array)
                        return (TypeMapper.Array, "Array");

                    // Enum 类型映射到 Number
                    else if (typeSymbol.TypeKind == TypeKind.Enum)
                        return (TypeMapper.Number, "Number");

                    // Date 相关类型（SpecialType 只包含 DateTime）
                    else if (displayName == "System.DateOnly")
                        return (TypeMapper.Date, "Date");

                    else if (displayName == "System.DateTimeOffset")
                        return (TypeMapper.Object, "Object");

                    else if (displayName == "System.TimeOnly")
                        return (TypeMapper.BigInt, "BigInt");

                    // BigInt 相关类型（SpecialType 只包含 Int64/UInt64）
                    else if (displayName == "System.Int128" ||
                        displayName == "System.UInt128" ||
                        displayName == "System.TimeSpan" ||
                        displayName == "System.Numerics.BigInteger")
                        return (TypeMapper.BigInt, "BigInt");

                    else if (
                        displayName == "System.Collections.Generic.Dictionary<TKey, TValue>" ||
                        displayName == "System.Collections.Generic.IDictionary<TKey, TValue>")
                        return (TypeMapper.Map, "Map");


                    else if (displayName == "System.Collections.Generic.HashSet<T>")
                        return (TypeMapper.Set, "Set");

                    // 集合类型检查
                    else if (displayName == "System.Collections.Generic.List<T>" ||
                        displayName == "System.Collections.Generic.IList" ||
                        displayName == "System.Collections.Generic.IList<T>" ||
                        displayName == "System.Collections.Generic.IEnumerable" ||
                        displayName == "System.Collections.Generic.IEnumerable<T>" ||
                        displayName == "System.Collections.Generic.ICollection<T>")
                        return (TypeMapper.Array, "Array");

                    // 对于自定义类型，使用instanceof检查（优先于接口检查）
                    else if (typeSymbol.TypeKind == TypeKind.Struct || typeSymbol.TypeKind == TypeKind.Class)
                    {
                        if (WhiteList.Types.TryGetValue(displayName, out var entry))
                        {
                            // 白名单中的类型
                            if (entry.Op == Common.Op.Alias && !string.IsNullOrEmpty(entry.Value))
                            {
                                var mapper = entry.Value! switch
                                {
                                    "String" => TypeMapper.String,
                                    "Object" => TypeMapper.Object,
                                    "Array" => TypeMapper.Array,
                                    "Number" => TypeMapper.Number,
                                    "Date" => TypeMapper.Date,
                                    "BigInt" => TypeMapper.BigInt,
                                    "Map" => TypeMapper.Map,
                                    "Set" => TypeMapper.Set,
                                    _ => TypeMapper.Class
                                };
                                return (mapper, entry.Value!);
                            }
                            // Op.Allowed 等其他情况，使用原始名称
                            // 例如：System.Nullable<T>, void 等
                        }

                        // 不在白名单中或 Op != Alias 的自定义类型
                        // 这些类型应该已经被 Analyzer 验证过（被 [ECMAScript] 标记）
                        // 使用 GetTypeConfigOrWhiteListName 获取正确的名称（支持特性配置）
                        var configName = GetTypeConfigOrWhiteListName(typeSymbol);
                        return (TypeMapper.Class, configName ?? typeSymbol.Name);
                    }
                }
                break;
        }

        // 未知类型，使用白名单检查获取名称
        var unknownName = GetTypeConfigOrWhiteListName(typeSymbol);
        return (TypeMapper.Unknown, unknownName ?? typeSymbol.Name);
    }

    private static ISymbol GetWhiteListSymbol(IMemberReferenceOperation operation, bool isRead = true)
    {
        if (operation is IPropertyReferenceOperation propertyReferenceOp)
        {
            if (isRead && propertyReferenceOp.Property.GetMethod is not null)
                return propertyReferenceOp.Property.GetMethod;
            else if (!isRead && propertyReferenceOp.Property.SetMethod is not null)
                return propertyReferenceOp.Property.SetMethod;
        }

        return operation.Member;
    }

    private Expression? GetWhiteListExpression(ISymbol symbol, SenseArgument context, List<Expression> arguments, out string? alias)
        => GetWhiteListExpressionCore(symbol, context, arguments, instance: null, out alias);

    private Expression? GetWhiteListExpression(ISymbol symbol, SenseArgument context, List<Expression> arguments, Expression? instance, out string? alias)
        => GetWhiteListExpressionCore(symbol, context, arguments, instance, out alias);

    /// <summary>
    /// 统一消费白名单成员映射。
    ///
    /// 这里刻意把 `Compile` 和旧的 `Alias/Inline/Import` 分成两套参数语义：
    /// - `Compile`：`handler` 表示实例宿主，`args` 只保留显式参数
    /// - `Alias/Inline/Import`：继续沿用历史占位符布局，实例方法把宿主拼到参数前缀
    ///
    /// 这样既能把 `Compile` 接到主分发优先级前面，又不会一次性打坏既有模板和导入规则。
    /// </summary>
    private Expression? GetWhiteListExpressionCore(ISymbol symbol, SenseArgument context, List<Expression> arguments, Expression? instance, out string? alias)
    {
        alias = null;

        var compileExpr = TryGetCompileExpression(symbol, arguments, instance);
        if (compileExpr is not null)
            return compileExpr;

        var displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        var legacyArguments = CreateLegacyWhiteListArguments(symbol, arguments, instance);
        if (WhiteList.Members.TryGetValue(displayString, out var entry))
        {
            if (entry.Op == Op.Alias)
                alias = entry.Value!;

            else if (entry.Op == Op.Inline)
                return InstantiateInlineTemplate(displayString, entry.Value!, legacyArguments);

            else if (entry.Op == Op.Import)
            {
                // Import 仍沿用历史参数语义：实例宿主拼到实参数组前缀。
                var id = context.BindImportSpecifier(entry.Path!, entry.Value!);
                return new CallExpression(id, NodeList.From(legacyArguments), optional: false);
            }
        }

        return null;
    }

    private Expression? TryGetCompileExpression(ISymbol symbol, List<Expression> arguments, Expression? instance)
    {
        var displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        if (!_whiteListCompiles.TryGetValue(displayString, out var compile))
            return null;

        var (handler, explicitArgs) = CreateCompileArguments(symbol, arguments, instance);
        return compile(handler, explicitArgs);
    }

    private static List<Expression> CreateLegacyWhiteListArguments(ISymbol symbol, List<Expression> arguments, Expression? instance)
    {
        if (symbol.IsStatic || instance is null)
            return arguments;

        var legacyArguments = new List<Expression>(arguments.Count + 1) { instance };
        legacyArguments.AddRange(arguments);
        return legacyArguments;
    }

    private static (Expression? Handler, Expression?[] Args) CreateCompileArguments(ISymbol symbol, List<Expression> arguments, Expression? instance)
    {
        if (symbol.IsStatic)
            return (null, ToNullableExpressionArray(arguments));

        // 普通调用路径会显式传入 instance；
        // 方法组等路径可能还保留“宿主在参数前缀”的旧布局，这里顺手拆开。
        if (instance is not null)
            return (instance, ToNullableExpressionArray(arguments));

        if (arguments.Count == 0)
            throw new InvalidOperationException($"Jazor 无法为实例成员 {symbol.Name} 绑定 Compile handler。");

        var compileArgs = new Expression?[arguments.Count - 1];
        for (var i = 1; i < arguments.Count; i++)
            compileArgs[i - 1] = arguments[i];

        return (arguments[0], compileArgs);
    }

    private static Expression?[] ToNullableExpressionArray(List<Expression> arguments)
    {
        var result = new Expression?[arguments.Count];
        for (var i = 0; i < arguments.Count; i++)
            result[i] = arguments[i];

        return result;
    }

    private int _recursionDepth;

    /// <summary>
    /// 调试标识
    /// </summary>
    private readonly bool _test;

    private readonly List<string> _testCache = [];

    private readonly Action<Location, string?>? _report;

    private readonly ITypeSymbol? _moduleRootType;

    private readonly Dictionary<string, Func<Expression?, Expression?[], Expression?>> _whiteListCompiles;

    public SemanticWalker()
    {
        _whiteListCompiles = [];
        Generate(ref _whiteListCompiles);
	}

    public SemanticWalker(ITypeSymbol moduleRootType) : this() => _moduleRootType = moduleRootType;

    public SemanticWalker(bool test) : this() => _test = test;

    public SemanticWalker(Action<Location, string?> report):this() => _report = report;

	partial void Generate(ref Dictionary<string, Func<Expression?, Expression?[], Expression?>> funcs);

	[DebuggerStepThrough]
    public static void EnsureSufficientExecutionStack(int recursionDepth)
    {
        if (recursionDepth > 20)
            RuntimeHelpers.EnsureSufficientExecutionStack();
    }

    /// <summary>
    /// 方法体、静态字段初始值、属性 getter/setter、构造函数初始值设定项、局部函数、匿名函数/Lambda 转换为 Acornima AST。
    /// </summary>
    /// <param name="operation">BlockSyntax对应的IOperation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Node? Visit(IOperation? operation, SenseArgument argument)
    {
        if (operation is null)
            return null;

        _recursionDepth++;
        try
        {
            EnsureSufficientExecutionStack(_recursionDepth);
            return operation.Accept(this, argument);
        }
        finally
        {
            _recursionDepth--;
        }
    }

    /// <summary>
    /// 根据操作生成稳定的唯一名称
    /// </summary>
    /// <param name="operation">操作</param>
    /// <param name="prefix">后缀</param>
    /// <returns>此名称仅针对语法树唯一</returns>
    private string GetUniqueName(IOperation operation, string? prefix = null)
    {
        var syntaxTree = operation.Syntax.SyntaxTree;
        var sourceSpan = operation.Syntax.GetLocation().SourceSpan;
        var key = $"{syntaxTree.FilePath}${operation.Syntax.Kind()}${sourceSpan.Start}${sourceSpan.End}${operation.Kind}${prefix}";
        var name = Format.HashName(key);

        //方便单元测试，生成固定名称
        if (_test)
        {
            var index = _testCache.IndexOf(name);
            if (index < 0)
            {
                _testCache.Add(name);
                return $"v${_testCache.Count - 1}";
            }
            return $"v${index}";
        }

        return name;
    }

    /// <summary>
    /// 操作无法转换时的兜底方法，提供详细的错误信息，包括操作类型
    /// </summary>
    /// <param name="operation">无法转换的Operation</param>
    /// <param name="message">错误信息</param>
    /// <returns></returns>
    /// <exception cref="OperationTransformationException">当操作无法转换时抛出</exception>
    private T HandleTransformationFailure<T>(IOperation operation, string? message)
    {
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);
        throw new OperationTransformationException(operation.Kind, message);
    }

    /// <summary>
    /// 语法无法转换时的兜底方法
    /// </summary>
    /// <param name="operation">无法转换的Operation</param>
    /// <param name="message">错误信息</param>
    /// <returns></returns>
    /// <exception cref="SyntaxNodeTransformationException"></exception>
    private Node HandleTransformationFailure(SyntaxNode node, string? message)
    {
        var location = node.GetLocation();
        _report?.Invoke(location, message);
        throw new SyntaxNodeTransformationException(node.Kind(), message);
    }

    /// <summary>
    /// 安全访问可能为null的操作并转换为Expression
    /// <para>
    /// </summary>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    /// <returns>转换后的Expression节点，如果操作为null或转换结果为null则抛出异常</returns>
    /// <exception cref="OperationTransformationException"></exception>
    private Expression TranslateExpression(IOperation operation, SenseArgument argument)
    {
        var node = Visit(operation, argument);
        if (node is Expression result)
            return result;

        var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(Expression).Name}'. This indicates missing support for this operation type or a type mismatch. ";
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);

        throw new OperationTransformationException(operation.Kind, message);
    }

    /// <summary>
    /// 安全访问可能为null的操作并转换为指定类型的AST节点
    /// <para>
    /// 此方法是类型安全的访问器，用于处理可能为null的操作，确保转换结果符合预期的节点类型。
    /// 如果操作为null、转换结果为null或无法转换，抛出异常。
    /// </para>
    /// </summary>
    /// <typeparam name="T">期望返回的AST节点类型</typeparam>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    /// <returns>转换后的指定类型AST节点，如果操作为null或转换结果为null则抛出异常</returns>
    /// <exception cref="OperationTransformationException">当操作不为null但无法转换为目标类型时抛出</exception>
    private T Translate<T>(IOperation operation, SenseArgument argument) where T : INode
    {
        var node = Visit(operation, argument);
        if (node is T result)
            return result;

        var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(T).Name}'. This indicates missing support for this operation type or a type mismatch. ";
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);

        throw new OperationTransformationException(operation.Kind, message);
    }

    /// <summary>
    /// 安全访问可能为null的操作并转换为指定类型的AST节点
    /// <para>
    /// 此方法是类型安全的访问器，用于处理可能为null的操作，确保转换结果符合预期的节点类型。
    /// 如果操作为null、转换结果为null或无法转换，返回默认值而不是抛出异常。
    /// </para>
    /// </summary>
    /// <typeparam name="T">期望返回的AST节点类型</typeparam>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    /// <param name="defaultValue">为空时的默认值</param>
    /// <returns>转换后的指定类型AST节点，如果操作为null或转换结果为null则返回默认值</returns>
    private T? Translate<T>(IOperation? operation, SenseArgument argument, T? defaultValue) where T : INode
    {
        if (operation is null)
            return defaultValue;

        var node = Visit(operation, argument);
        if (node is null)
            return defaultValue;

        if (node is T result)
            return result;

        var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(T).Name}'. This indicates missing support for this operation type or a type mismatch. ";
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);

        return defaultValue;
    }

    /// <summary>
    /// 安全访问操作并将其添加到集合中
    /// <para>
    /// 此方法用于将操作转换为指定类型的AST节点，并将成功转换的节点添加到集合中。
    /// 如果操作为null或转换结果为null，则跳过处理，不抛出异常。
    /// 如果转换结果类型不匹配，会记录错误信息但不中断处理流程。
    /// </para>
    /// </summary>
    /// <typeparam name="T">期望的AST节点类型</typeparam>
    /// <param name="target">用于存放成功转换的AST节点的集合</param>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    private void Translate<T>(ICollection<T> target, IOperation? operation, SenseArgument argument) where T : INode
    {
        if (operation is null)
            return;

        var node = Visit(operation, argument);
        if (node is null)
            return;

        if (node is T item)
            target.Add(item);
        else
        {
            var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(T).Name}'. This indicates missing support for this operation type or a type mismatch. ";
            var location = operation.Syntax.GetLocation();
            _report?.Invoke(location, message);
        }
    }

    /// <summary>
    /// 安全访问操作并将其添加到集合中
    /// <para>
    /// 此方法用于将操作转换为指定类型的AST节点，并将成功转换的节点添加到集合中。
    /// 如果操作为null或转换结果为null，则跳过处理，不抛出异常。
    /// 如果转换结果类型不匹配，会记录错误信息但不中断处理流程。
    /// </para>
    /// </summary>
    /// <typeparam name="T">期望的AST节点类型</typeparam>
    /// <param name="target">用于存放成功转换的AST节点的集合</param>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    /// <param name="defaultValue">为空时的默认值</param>
    private void Translate<T>(ICollection<T?> target, IOperation? operation, SenseArgument argument, T? defaultValue) where T : INode
    {
        if (operation is null)
            return;

        var node = Visit(operation, argument);
        if (node is null)
            return;

        if (node is T item)
            target.Add(item);
        else
        {
            var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(T).Name}'. This indicates missing support for this operation type or a type mismatch. ";
            var location = operation.Syntax.GetLocation();
            _report?.Invoke(location, message);
        }
    }
}
