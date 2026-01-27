using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using ECMAScript.Common;
using System.Dynamic;

namespace ECMAScript.Compiler;

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
public sealed partial class SemanticWalker : OperationVisitor<WalkerArgument, Node?>
{
    private static readonly NullLiteral Null = new("null");

    private static readonly Identifier Undefined = new("undefined");

	private static readonly MemberExpression IsArrayExpr = new(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false);

    private static TypeMapper GetMapperType(ITypeSymbol typeSymbol, out string typeName)
    {
        // 类型映射
        // object、匿名类型、元组类型 -> js object
        // string -> js string
        // byte、sbyte、short、ushort、int、uint、decimal、double、float -> js Number
        // long、ulong、Int128、UInt128、timestamp、BigInteger ->js BigInt
        // DateOnly、TimeOnly、DateTime、DateTimeOffset -> js Date
        // Array -> js array
        // IDictionary -> js Map
        // IEnumerable(非IDictionary) -> js Set
        // 其他 class -> js class
        // 其他类型不支持 -> Unknown

        typeName = typeSymbol.Name;
        var displayName = typeSymbol.OriginalDefinition.ToDisplayString(Util.NameFormat);

        if (typeSymbol.IsTupleType || typeSymbol.IsAnonymousType)
            return TypeMapper.Object;

        // 使用 SpecialType 进行基础类型检查，更加类型安全和高效
        switch (typeSymbol.OriginalDefinition.SpecialType)
        {
            case SpecialType.System_Char:
            case SpecialType.System_String:
                return TypeMapper.String;
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                return TypeMapper.Number;
            case SpecialType.System_Boolean:
                return TypeMapper.Boolean;
            case SpecialType.System_Object:
                return TypeMapper.Object;
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                return TypeMapper.BigInt;
            case SpecialType.System_DateTime:
                typeName = "Date"; 
                return TypeMapper.Date;
            default:
                {
                    // Array 类型检查
                    if (typeSymbol.TypeKind == TypeKind.Array)
                        return TypeMapper.Array;

                    // Enum 类型映射到 Number
                    else if (typeSymbol.TypeKind == TypeKind.Enum)
                        return TypeMapper.Number;

                    // Date 相关类型（SpecialType 只包含 DateTime）
                    else if (displayName == "System.DateTimeOffset" ||
                        displayName == "System.DateOnly" ||
                        displayName == "System.TimeOnly")
                    {
                        typeName = "Date";
                        return TypeMapper.Date;
                    }

                    // BigInt 相关类型（SpecialType 只包含 Int64/UInt64）
                    else if (displayName == "System.Int128" ||
                        displayName == "System.UInt128" ||
                        displayName == "System.TimeSpan" ||
                        displayName == "System.Numerics.BigInteger")
                    {
                        typeName = "BigInt";
                        return TypeMapper.BigInt;
                    }

                    else if (
                        displayName == "System.Collections.Generic.Dictionary<TKey, TValue>" ||
                        displayName == "System.Collections.Generic.IDictionary<TKey, TValue>")
                    {
                        typeName = "Map";
                        return TypeMapper.Map;
                    }

                    else if (displayName == "System.Collections.Generic.HashSet<T>")
                    {
                        typeName = "Set";
                        return TypeMapper.Set;
                    }

                    // 集合类型检查
                    else if (displayName == "System.Collections.Generic.List<T>" ||
                        displayName == "System.Collections.Generic.IList" ||
                        displayName == "System.Collections.Generic.IList<T>" ||
                        displayName == "System.Collections.Generic.IEnumerable" ||
                        displayName == "System.Collections.Generic.IEnumerable<T>" ||
                        displayName == "System.Collections.Generic.ICollection<T>")
                    {
                        typeName = "Array";
                        return TypeMapper.Array;
                    }

                    // Struct 类型映射到 Object
                    else if (typeSymbol.TypeKind == TypeKind.Struct)
                        return TypeMapper.Class;

                    // 对于自定义类型，使用instanceof检查（优先于接口检查）
                    else if (typeSymbol.TypeKind == TypeKind.Class)
                        return TypeMapper.Class;
                }
                break;
        }

        return TypeMapper.Unknown;
    }

    private int _recursionDepth;

    /// <summary>
    /// 调试标识
    /// </summary>
    private readonly bool _test;

    private readonly List<string> _testCache = [];

    private readonly Action<Location, string?>? _report;

    public SemanticWalker() { }

    public SemanticWalker(bool test) => _test = test;

    public SemanticWalker(Action<Location, string?> report) => _report = report;

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
    public override Node? Visit(IOperation? operation, WalkerArgument argument)
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
    /// 根据操作生成以v开头的稳定的唯一名称
    /// </summary>
    /// <param name="operation">操作</param>
    /// <returns></returns>
    private string GetUniqueName(SyntaxNode node)
    {
        //var hash = operation.Syntax.GetHashCode();
        //if(_cache.TryGetValue(hash,out var cache) && cache.Kind == operation.Kind)
        //    return cache.Name;

        var syntaxTree = node.SyntaxTree;
        var sourceSpan = node.GetLocation().SourceSpan;

        using var sha256 = SHA256.Create();
        var key = $"{syntaxTree.FilePath}${node.Kind()}${sourceSpan.Start}${sourceSpan.End}";
        var bytes = Encoding.UTF8.GetBytes(key);
        var hashBytes = sha256.ComputeHash(bytes);
        var sb = new StringBuilder("_");
        for (int i = 0; i < 8; i++)
            sb.Append(hashBytes[i].ToString("x2"));
        var name = sb.ToString();

        //方便单元测试，生成固定名称
        if (_test)
        {
            var index = _testCache.IndexOf(name);
            if(index <0)
            {
                _testCache.Add(name);
                return $"v$0";
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
    private T HandleTransformationFailure<T>(IOperation operation, string? message) where T : INode
    {
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);
        throw new OperationTransformationException(operation, message);
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
        throw new SyntaxNodeTransformationException(node, message);
    }

    /// <summary>
    /// 安全访问可能为null的操作并转换为Expression
    /// <para>
    /// </summary>
    /// <param name="operation">要访问和转换的操作，可能为null</param>
    /// <param name="argument">用于存放变量声明的队列</param>
    /// <returns>转换后的Expression节点，如果操作为null或转换结果为null则抛出异常</returns>
    /// <exception cref="OperationTransformationException"></exception>
    private Expression TranslateExpression(IOperation operation, WalkerArgument argument)
    {
        var node = Visit(operation, argument);
        if (node is Expression result)
            return result;

        var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(Expression).Name}'. This indicates missing support for this operation type or a type mismatch. ";
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);

        throw new OperationTransformationException(operation, message);
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
    private T Translate<T>(IOperation operation, WalkerArgument argument) where T : INode
    {
        var node = Visit(operation, argument);
        if (node is T result)
            return result;

        var message = $"Cannot convert operation '{operation.Kind}' to AST node type '{typeof(T).Name}'. This indicates missing support for this operation type or a type mismatch. ";
        var location = operation.Syntax.GetLocation();
        _report?.Invoke(location, message);

        throw new OperationTransformationException(operation, message);
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
    private T? Translate<T>(IOperation? operation, WalkerArgument argument, T? defaultValue) where T : INode
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
    private void Translate<T>(ICollection<T> target, IOperation? operation, WalkerArgument argument) where T : INode
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
    private void Translate<T>(ICollection<T?> target, IOperation? operation, WalkerArgument argument, T? defaultValue) where T : INode
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
