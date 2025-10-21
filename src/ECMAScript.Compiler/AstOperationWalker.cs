using Acornima.Ast;
using Acornima;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

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
public sealed class AstOperationWalker : OperationVisitor<Queue<VariableDeclaration>, Node?>
{
    private int _recursionDepth;

    private readonly Action<Location, string>? _report;

    public AstOperationWalker()
    {

    }

    public AstOperationWalker(Action<Location, string> report) => _report = report;

    [DebuggerStepThrough]
    public static void EnsureSufficientExecutionStack(int recursionDepth)
    {
        if (recursionDepth > 20)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
        }
    }

    /// <summary>
    /// 操作无法转换时的兜底方法
    /// </summary>
    /// <param name="operation">无法转换的Operation</param>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="OperationTransformationException"></exception>
    public Node HandleTransformationFailure(IOperation operation, string? message = null)
    {
        var error = message ?? $"Unsupported operation: {operation.GetType().Name}";
        var location = operation.Syntax.GetLocation();

        //内部可能有其他处理改进，如推给analysis
        _report?.Invoke(location, error);

        throw new OperationTransformationException(operation, error);
    }

    /// <summary>
    /// 语法无法转换时的兜底方法
    /// </summary>
    /// <param name="node">无法转换的语法节点</param>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxNodeTransformationException"></exception>
    public Node HandleTransformationFailure(SyntaxNode node, string? message = null)
    {
        var error = message ?? $"Unsupported operation: {node.GetType().Name}";
        var location = node.GetLocation();

        //内部可能有其他处理改进，如推给analysis
        _report?.Invoke(location, error);

        throw new SyntaxNodeTransformationException(node, error);
    }

    /// <summary>
    /// 根据操作生成以v开头的稳定的唯一名称
    /// </summary>
    /// <param name="operation">操作</param>
    /// <returns></returns>
    private static string GetUniqueName(IOperation operation)
    {
        var syntaxTree = operation.Syntax.SyntaxTree;
        var sourceSpan = operation.Syntax.GetLocation().SourceSpan;
        using var sha256 = SHA256.Create();
        var key = $"{syntaxTree.FilePath}${operation.Kind}${sourceSpan.Start}${sourceSpan.End}";
        var bytes = Encoding.UTF8.GetBytes(key);
        var hashBytes = sha256.ComputeHash(bytes);
        var sb = new StringBuilder("v");
        for (int i = 0; i < 8; i++)
            sb.Append(hashBytes[i].ToString("x2"));
        return sb.ToString();
    }

    /// <summary>
    /// 提取模式操作中引用对象名称
    /// </summary>
    /// <param name="operation">模式相关操作</param>
    /// <returns>引用对象名称</returns>
    private string? ExtractPatternValName(IOperation? operation)
    {
        var refOperation = operation switch
        {
            // is 模式匹配：从 obj is Pattern 中获取 obj 的名称
            IIsPatternOperation isPattern => isPattern.Value,

            // switch 表达式分支：直接从父节点获取switch输入名称
            ISwitchExpressionArmOperation armOp when armOp.Parent is ISwitchExpressionOperation switchExpr
                => switchExpr.Value,

            // 关系模式匹配：从 value is > 5 中获取 value 的名称
            IRelationalPatternOperation relPattern when relPattern.Parent is not null
                => relPattern.Parent,

            // 处理嵌套在递归模式中的关系模式
            IPropertySubpatternOperation propSubPattern when propSubPattern.Parent is IRecursivePatternOperation recPattern
                => recPattern.Parent switch
                {
                    IIsPatternOperation isPattern => isPattern.Value,
                    ISwitchExpressionOperation switchExpr => switchExpr.Value,
                    _ => null
                },//递归模式的父节点可能是 is 模式或 switch 表达式

            // 其他情况返回null
            _ => null
        };

        // 特殊处理：如果传入的operation本身就是RelationalPatternOperation，需要从其父节点获取参考名称
        if (operation is IRelationalPatternOperation relationalPattern && relationalPattern.Parent is not null)
        {
            refOperation = relationalPattern.Parent;
        }

        var refName = refOperation switch
        {
            ILocalReferenceOperation localRef => localRef.Local.Name,
            IParameterReferenceOperation paramRef => paramRef.Parameter.Name,
            IFieldReferenceOperation fieldRef when fieldRef.Instance is null => fieldRef.Field.Name,
            IPropertyReferenceOperation propRef when propRef.Instance is null => propRef.Property.Name,
            IInstanceReferenceOperation => "this",
            _ => null
        };

        return refName;
    }

    /// <summary>
    /// 方法体、静态字段初始值、属性 getter/setter、构造函数初始值设定项、局部函数、匿名函数/Lambda 转换为 Acornima AST。
    /// </summary>
    /// <param name="operation">BlockSyntax对应的IOperation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? Visit(IOperation? operation, Queue<VariableDeclaration> argument)
    {
        if (operation is not null)
        {
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

        return default;
    }

    /// <summary>
    /// 处理 using 语句操作
    /// C# 示例：
    /// using (var resource = new DisposableResource()) {
    ///     resource.DoWork();
    /// }
    /// 转换结果：不支持，JavaScript 没有内置的资源管理机制
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitUsing(IUsingOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Using statements are not supported in JavaScript conversion");

    /// <summary>
    /// 处理 Stop 操作（编译器内部）
    /// 这是编译器内部使用的操作，不对应具体的 C# 语法
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitStop(IStopOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Stop operations are compiler-internal and not supported in JavaScript conversion.");

    /// <summary>
    /// 处理 End 操作（编译器内部）
    /// 这是编译器内部使用的操作，不对应具体的 C# 语法
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitEnd(IEndOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "End operations are compiler-internal and not supported in JavaScript conversion.");

    /// <summary>
    /// 处理事件触发操作
    /// C# 示例：
    /// MyEvent?.Invoke(args);  // 触发事件
    /// 转换结果：不支持，JavaScript 事件模型不同
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitRaiseEvent(IRaiseEventOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Event raising operations are not supported in JavaScript conversion");

    /// <summary>
    /// 处理 For-To 循环操作（VB.NET 特有）
    /// VB.NET 示例：
    /// For i = 1 To 10
    ///     Console.WriteLine(i)
    /// Next
    /// 转换结果：不支持，请使用标准 for 循环
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitForToLoop(IForToLoopOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "For-to loops are not supported in JavaScript conversion");

    /// <summary>
    /// 处理 lock 语句操作
    /// C# 示例：
    /// lock (lockObject) {
    ///     // 线程安全代码
    /// }
    /// 转换结果：不支持，JavaScript 没有内置的锁机制
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitLock(ILockOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Lock statements are not supported in JavaScript conversion");

    /// <summary>
    /// 处理事件引用操作
    /// C# 示例：
    /// obj.MyEvent += Handler;      // 事件订阅
    /// obj.MyEvent -= Handler;      // 事件取消订阅
    /// 转换结果：不支持，JavaScript 事件模型不同
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitEventReference(IEventReferenceOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Event references are not supported in JavaScript conversion");

    /// <summary>
    /// 处理事件赋值操作
    /// C# 示例：
    /// event += handler;           // 事件订阅
    /// event -= handler;           // 事件取消订阅
    /// 转换结果：不支持，JavaScript 事件模型不同
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitEventAssignment(IEventAssignmentOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "C# 事件模型（多播、弱引用、线程安全）与 JavaScript 事件模型根本不同，无论订阅、取消订阅还是直接赋值都无法保证语义等价");

    /// <summary>
    /// 处理动态对象创建操作
    /// C# 示例：
    /// dynamic obj = new ExpandoObject();  // 动态对象创建
    /// dynamic result = Activator.CreateInstance(type);  // 动态类型实例化
    /// 转换结果：不支持，JavaScript 的动态性与 C# dynamic 语义不同
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, Queue<VariableDeclaration> argument)
         => HandleTransformationFailure(operation, "C# 动态绑定语义（运行时解析、重载决策、动态分派）与 JavaScript 静态分派模型根本不可通约，无法保证语义等价");

    /// <summary>
    /// 处理动态成员引用操作
    /// C# 示例：
    /// dynamic obj = GetDynamicObject();
    /// var value = obj.SomeMember;         // 动态成员访问
    /// 转换结果：不支持，需要编译时确定成员信息
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "C# 动态绑定语义（运行时解析、重载决策、动态分派）与 JavaScript 静态分派模型根本不可通约，无法保证语义等价");

    /// <summary>
    /// 处理动态方法调用操作
    /// C# 示例：
    /// dynamic obj = GetDynamicObject();
    /// obj.SomeMethod(arg1, arg2);         // 动态方法调用
    /// 转换结果：不支持，需要编译时确定方法签名
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDynamicInvocation(IDynamicInvocationOperation operation, Queue<VariableDeclaration> argument)
     => HandleTransformationFailure(operation, "C# 动态绑定语义（运行时解析、重载决策、动态分派）与 JavaScript 静态分派模型根本不可通约，无法保证语义等价");

    /// <summary>
    /// 处理动态索引器访问操作
    /// C# 示例：
    /// dynamic obj = GetDynamicObject();
    /// var value = obj["key"];             // 动态索引器访问
    /// obj[0] = newValue;                  // 动态索引器赋值
    /// 转换结果：不支持，需要编译时确定索引器类型
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDynamicIndexerAccess(IDynamicIndexerAccessOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "C# 动态绑定语义（运行时解析、重载决策、动态分派）与 JavaScript 静态分派模型根本不可通约，无法保证语义等价");

    /// <summary>
    /// 处理已翻译的 LINQ 查询操作
    /// C# 示例：
    /// var result = from x in collection
    ///              where x > 0
    ///              select x * 2;              // LINQ 查询表达式
    /// var filtered = list.Where(x => x > 0); // LINQ 方法链
    /// 转换结果：不支持，LINQ 语义复杂且 JavaScript 没有对应构造
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitTranslatedQuery(ITranslatedQueryOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "LINQ queries are not supported in JavaScript conversion");

    /// <summary>
    /// 处理 typeof 运算符操作
    /// C# 示例：
    /// typeof(int)                         // 获取类型信息
    /// typeof(MyClass)                     // 获取自定义类型信息
    /// 转换结果：不支持，JavaScript typeof 语义与 C# 不同
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitTypeOf(ITypeOfOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "typeof operator is not supported in JavaScript conversion");

    /// <summary>
    /// 处理 sizeof 运算符操作
    /// C# 示例：
    /// sizeof(int)                         // 4 字节
    /// sizeof(double)                      // 8 字节
    /// 转换结果：不支持，JavaScript 没有直接的内存大小概念
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitSizeOf(ISizeOfOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "sizeof operator is not supported in JavaScript conversion");

    /// <summary>
    /// 处理取地址运算符操作
    /// C# 示例：
    /// unsafe {
    ///     int x = 42;
    ///     int* ptr = &x;                   // 获取变量地址
    /// }
    /// 转换结果：不支持，JavaScript 是安全语言，不支持指针操作
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitAddressOf(IAddressOfOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "JavaScript 为安全语言，不支持指针、函数指针或 unsafe 语义，取地址操作符（&）无法转换");

    /// <summary>
    /// 处理方法体操作（编译器内部）
    /// 这是编译器内部使用的操作，不对应具体的 C# 语法
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitMethodBodyOperation(IMethodBodyOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, $"方法体操作（{operation.Kind}）是编译器内部使用的操作，不对应具体的 C# 语法，无法转换为 JavaScript");

    /// <summary>
    /// 处理构造函数体操作（编译器内部）
    /// 这是编译器内部使用的操作，不对应具体的 C# 语法
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitConstructorBodyOperation(IConstructorBodyOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, $"构造函数体操作（{operation.Kind}）是编译器内部使用的操作，不对应具体的 C# 语法，无法转换为 JavaScript");

    /// <summary>
    /// 处理捕获异常操作（编译器内部）
    /// 这是编译器内部使用的操作，表示在 catch 块中捕获的异常
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitCaughtException(ICaughtExceptionOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Caught exception operations are compiler-internal and not supported in JavaScript conversion.");

    /// <summary>
    /// 处理静态本地初始化信号量操作（编译器内部）
    /// 这是编译器内部使用的操作，用于线程安全的静态变量初始化
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitStaticLocalInitializationSemaphore(IStaticLocalInitializationSemaphoreOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Static local initialization semaphore operations are compiler-internal and not supported in JavaScript conversion.");

    /// <summary>
    /// 处理流匿名函数操作（编译器内部）
    /// 这是编译器内部使用的操作，用于数据流分析中的匿名函数
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitFlowAnonymousFunction(IFlowAnonymousFunctionOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Flow anonymous function operations are compiler-internal and not supported in JavaScript conversion.");

    /// <summary>
    /// 处理范围 case 子句操作（VB.NET 特有）
    /// VB.NET 示例：
    /// Select Case value
    ///     Case 1 To 10
    ///         DoSomething()
    /// End Select
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitRangeCaseClause(IRangeCaseClauseOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Range case clauses require complex range checking logic and are not supported in JavaScript conversion.");

    /// <summary>
    /// 处理流捕获操作（编译器内部）
    /// 这是编译器内部使用的操作，用于数据流分析
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitFlowCapture(IFlowCaptureOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, $"流捕获操作（{operation.Kind}）是编译器内部使用的操作，用于数据流分析，无法转换为 JavaScript");

    /// <summary>
    /// 处理流捕获引用操作（编译器内部）
    /// 这是编译器内部使用的操作，用于数据流分析
    /// 转换结果：不支持
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitFlowCaptureReference(IFlowCaptureReferenceOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, $"流捕获引用操作（{operation.Kind}）是编译器内部使用的操作，用于数据流分析，无法转换为 JavaScript");

    /// <summary>
    /// 处理关系 case 子句操作（VB.NET 特有）
    /// VB.NET 示例：
    /// Select Case value
    ///     Case Is > 10
    ///         DoSomething()
    /// End Select
    /// 转换结果：返回比较值，关系操作符在上级处理
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitRelationalCaseClause(IRelationalCaseClauseOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation);

    /// <summary>
    /// 处理范围操作（Range 操作符）
    /// C# 示例：
    /// Range range = 1..5;                // 创建范围 1 到 5
    /// var slice = array[1..^1];          // 使用范围进行切片
    /// array[start..end]                  // 数组切片操作
    /// list[2..^2]                        // 列表切片，从索引 2 到倒数第 2 个
    /// 转换结果：C# Range 操作在没有下游消费逗辑（如索引器转换为 slice 调用）时属于“悬空 AST”
    /// 为保证语义等价，必须由上层语法节点实现 Range 消费逗辑
    /// </summary>
    /// <param name="operation">范围操作</param>
    /// <param name="argument">当前operation所属的父operation</param>
    /// <returns>JavaScript范围对象字面量</returns>
    public override Acornima.Ast.Node? VisitRangeOperation(IRangeOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "C# Range 操作必须在索引器操作中被消费转换为 slice/splice 调用，单独的 Range 对象在 JavaScript 中无意义且可能导致运行时错误");

    /// <summary>
    /// 处理 ReDim 操作（VB.NET 特有）
    /// VB.NET 示例：
    /// ReDim array(10)         // 重新设置数组大小
    /// ReDim Preserve array(20) // 保留数据同时重新设置大小
    /// 转换结果：不支持，这是 VB.NET 特有功能
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitReDim(IReDimOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "ReDim operations are VB.NET-specific and not supported in JavaScript conversion.");

    /// <summary>
    /// 处理 ReDim 子句操作（VB.NET 特有）
    /// VB.NET 示例：
    /// ReDim array1(10), array2(20) 中的每个数组重新定义
    /// 转换结果：不支持，这是 VB.NET 特有功能
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitReDimClause(IReDimClauseOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "ReDim clause operations are VB.NET-specific and not supported in JavaScript conversion.");

    /// <summary>
    /// 处理 using 声明操作
    /// C# 示例：
    /// using var file = File.OpenRead("data.txt"); // using 声明
    /// using FileStream fs = new FileStream(...);   // 传统 using 声明
    /// 转换结果：不支持，JavaScript 没有内置的资源管理机制
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitUsingDeclaration(IUsingDeclarationOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "Using declarations are not supported in JavaScript conversion.");

    /// <summary>
    /// 处理插值字符串处理器创建操作
    /// C# 示例：
    /// public void WriteInterpolated([InterpolatedStringHandler] ref CustomHandler handler) { ... }
    /// WriteInterpolated($"Hello {name}!"); // 触发处理器创建
    /// 转换结果：不支持，插值字符串处理器的自定义 AppendLiteral/AppendFormatted 调用链无法在 JS 端重现
    /// 处理器的副作用、格式化逗辑、状态管理均无法保证语义等价
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInterpolatedStringHandlerCreation(IInterpolatedStringHandlerCreationOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "插值字符串处理器的自定义 AppendLiteral/AppendFormatted 调用链无法在 JavaScript 端重现，处理器的副作用、格式化逗辑、状态管理均无法保证语义等价");

    /// <summary>
    /// 处理插值字符串追加操作
    /// C# 示例：
    /// 在插值字符串处理器中追加内容的操作
    /// 转换结果：插值字符串追加操作属于处理器框架的一部分，无法单独转换
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInterpolatedStringAppend(IInterpolatedStringAppendOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "插值字符串追加操作属于处理器框架的一部分，其语义依赖于处理器上下文，无法在 JavaScript 中单独转换而保证语义等价");

    /// <summary>
    /// 处理插值字符串处理器参数占位符操作
    /// C# 示例：
    /// 插值字符串处理器中的参数占位符（编译器内部）
    /// 转换结果：插值字符串处理器参数占位符是编译器内部操作，无法转换
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInterpolatedStringHandlerArgumentPlaceholder(IInterpolatedStringHandlerArgumentPlaceholderOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, $"插值字符串处理器参数占位符操作（{operation.Kind}）是编译器内部实现细节，无法转换为 JavaScript 并保证语义等价");

    /// <summary>
    /// 处理函数指针调用操作
    /// C# 示例：
    /// unsafe {
    ///     delegate*<int, int> ptr = &MyMethod;
    ///     int result = ptr(42);    // 函数指针调用
    /// }
    /// 转换结果：不支持，JavaScript 是安全语言，不支持函数指针
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitFunctionPointerInvocation(IFunctionPointerInvocationOperation operation, Queue<VariableDeclaration> argument)
        => HandleTransformationFailure(operation, "JavaScript 为安全语言，不支持指针、函数指针或 unsafe 语义，函数指针调用无法转换");

    /// <summary>
    /// 处理 IInvalidOperation，它包装了在当前上下文中无法用单一类型表示其结果的操作。
    /// 此方法会尝试解包结合语法节点和子操作实现语义的精准匹配（不支持dynamic，不用考虑这个）。
    /// why do:如“var x = Math.Abs(-5);”，IInvalidOperation的子操作会把Math.Abs(-5)计算成5，而我们需要完整的方法调用转换而不是一个字面量。
    /// - 如果只有一个子表达式，则直接返回该表达式的转换结果。
    /// - 如果有多个子表达式，则将它们组合成一个 Acornima 的 SequenceExpression (逗号表达式)。
    /// - 如果没有可转换的子节点，则抛出异常。
    /// C# 示例 1: var x = Math.Abs(-5); -> let x = Math.Abs(-5); (注意: 这通常不是IInvalidOperation)
    /// C# 示例 2: var y = (a = 1, b = 2, a + b); -> let y = (a = 1, b = 2, a + b);
    /// C# 示例 3: MyMethod(); -> MyMethod();
    /// C# 示例 4: condition ? whenTrue : whenFalse; -> condition ? whenTrue : whenFalse;
    /// </summary>
    /// <param name="operation">当前访问的 IInvalidOperation。</param>
    /// <param name="argument">当前访问的 operation 的根 operation。</param>
    /// <returns>转换后的 Acornima AST Node。</returns>
    public override Acornima.Ast.Node? VisitInvalid(IInvalidOperation operation, Queue<VariableDeclaration> argument)
        => ConvertFromSyntaxNode(operation.Syntax);

    /// <summary>
    /// 核心转换器，基于 C# 语法节点类型进行模式匹配，转换为 Acornima AST 节点。
    /// </summary>
    /// <param name="node">要转换的 C# 语法节点。</param>
    /// <returns>转换后的 Acornima AST Node。</returns>
    /// <exception cref="ArgumentNullException">当 syn 参数为 null 时抛出。</exception>
    /// <exception cref="NotSupportedException">当遇到不支持的语法节点类型时抛出。</exception>
    private Node ConvertFromSyntaxNode(SyntaxNode node)
    {
        var result = node switch
        {
            // 基础表达式和字面量
            LiteralExpressionSyntax lit => (lit.Token.Value switch
            {
                null => new NullLiteral("null"),
                bool b => new BooleanLiteral(b, b.ToString().ToLower()),
                string s => new StringLiteral(s, $"\"{s}\""),
                int i => new NumericLiteral(i, i.ToString()),
                long l => new NumericLiteral(l, l.ToString()),
                double d => new NumericLiteral(d, d.ToString()),
                float f => new NumericLiteral(f, f.ToString()),
                decimal dec => new NumericLiteral(System.Convert.ToDouble(dec), dec.ToString()),
                _ => null
            }),
            IdentifierNameSyntax id => new Identifier(id.Identifier.Text),
            DefaultExpressionSyntax _ => new Identifier("null"),

            ParenthesizedExpressionSyntax pe => ConvertFromSyntaxNode(pe.Expression),

            // 调用、创建与访问
            InvocationExpressionSyntax ie => new CallExpression(
                (Expression)ConvertFromSyntaxNode(ie.Expression),
                NodeList.From(ie.ArgumentList.Arguments.Select(a => (Expression)ConvertFromSyntaxNode(a.Expression))),
                optional: false),

            ObjectCreationExpressionSyntax oc => new NewExpression(
                (Expression)ConvertFromSyntaxNode(oc.Type),
                NodeList.From(oc.ArgumentList?.Arguments.Select(a => (Expression)ConvertFromSyntaxNode(a.Expression)) ?? [])),

            MemberAccessExpressionSyntax ma => new MemberExpression(
                (Expression)ConvertFromSyntaxNode(ma.Expression),
                new Identifier(ma.Name.Identifier.Text),
                computed: false,
                optional: false),

            ConditionalAccessExpressionSyntax ca => new ConditionalExpression(
                new LogicalExpression(
                    Operator.StrictEquality,
                    (Expression)ConvertFromSyntaxNode(ca.Expression),
                    new NullLiteral("null")
                ),
                new Identifier("undefined"),
                (Expression)ConvertFromSyntaxNode(ca.WhenNotNull)),

            ElementAccessExpressionSyntax ea => new MemberExpression(
                (Expression)ConvertFromSyntaxNode(ea.Expression),
                ea.ArgumentList.Arguments.Count > 0
                ? (Expression)ConvertFromSyntaxNode(ea.ArgumentList.Arguments[0].Expression)
                : new Identifier("undefined"),
                computed: true,
                optional: false),

            // 赋值与运算符
            AssignmentExpressionSyntax ae => new AssignmentExpression(
                Operator.Assignment,
                (Expression)ConvertFromSyntaxNode(ae.Left),
                (Expression)ConvertFromSyntaxNode(ae.Right)),

            ConditionalExpressionSyntax ce => new ConditionalExpression(
                (Expression)ConvertFromSyntaxNode(ce.Condition),
                (Expression)ConvertFromSyntaxNode(ce.WhenTrue),
                (Expression)ConvertFromSyntaxNode(ce.WhenFalse)),

            BinaryExpressionSyntax be => be.OperatorToken.Kind() switch
            {
                SyntaxKind.PlusToken => new NonLogicalBinaryExpression(Operator.Addition, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                SyntaxKind.MinusToken => new NonLogicalBinaryExpression(Operator.Subtraction, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                SyntaxKind.AsteriskToken => new NonLogicalBinaryExpression(Operator.Multiplication, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                SyntaxKind.SlashToken => new NonLogicalBinaryExpression(Operator.Division, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                SyntaxKind.EqualsEqualsToken => new NonLogicalBinaryExpression(Operator.Equality, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                SyntaxKind.ExclamationEqualsToken => new NonLogicalBinaryExpression(Operator.Inequality, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                SyntaxKind.GreaterThanToken => new NonLogicalBinaryExpression(Operator.GreaterThan, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                SyntaxKind.LessThanToken => new NonLogicalBinaryExpression(Operator.LessThan, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                SyntaxKind.AmpersandAmpersandToken => new LogicalExpression(Operator.LogicalAnd, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                SyntaxKind.BarBarToken => new LogicalExpression(Operator.LogicalOr, (Expression)ConvertFromSyntaxNode(be.Left), (Expression)ConvertFromSyntaxNode(be.Right)),
                _ => null
            },

            PrefixUnaryExpressionSyntax pu => pu.OperatorToken.Kind() switch
            {
                SyntaxKind.MinusToken => new NonUpdateUnaryExpression(Operator.UnaryNegation, (Expression)ConvertFromSyntaxNode(pu.Operand)),
                SyntaxKind.PlusPlusToken => new UpdateExpression(Operator.Increment, (Expression)ConvertFromSyntaxNode(pu.Operand), prefix: true),
                SyntaxKind.MinusMinusToken => new UpdateExpression(Operator.Decrement, (Expression)ConvertFromSyntaxNode(pu.Operand), prefix: true),
                SyntaxKind.ExclamationToken => new UpdateExpression(Operator.LogicalNot, (Expression)ConvertFromSyntaxNode(pu.Operand), prefix: true),
                SyntaxKind.PlusToken => new UpdateExpression(Operator.Addition, (Expression)ConvertFromSyntaxNode(pu.Operand), prefix: true),
                _ => null
            },
            PostfixUnaryExpressionSyntax po when po.OperatorToken.IsKind(SyntaxKind.PlusPlusToken) || po.OperatorToken.IsKind(SyntaxKind.MinusMinusToken) =>
                new UpdateExpression(
                    po.OperatorToken.IsKind(SyntaxKind.PlusPlusToken) ? Operator.Increment : Operator.Decrement,
                    (Expression)ConvertFromSyntaxNode(po.Operand),
                    prefix: false),

            CastExpressionSyntax cs => (Expression)ConvertFromSyntaxNode(cs.Expression),

            AwaitExpressionSyntax aw => new AwaitExpression((Expression)ConvertFromSyntaxNode(aw.Expression)),

            TupleExpressionSyntax te => new SequenceExpression(NodeList.From(te.Arguments.Select(a => (Expression)ConvertFromSyntaxNode(a.Expression)))),

            ExpressionStatementSyntax es => new NonSpecialExpressionStatement((Expression)ConvertFromSyntaxNode(es.Expression)),

            _ => null
        };

        return result ?? HandleTransformationFailure(node);
    }

    /// <summary>
    /// 处理代码块操作
    /// C# 示例：
    /// {
    ///     int x = 5;
    ///     Console.WriteLine(x);
    /// }
    /// 转换结果：根据上下文返回 NestedBlockStatement、FunctionBody 或 StaticBlock
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitBlock(IBlockOperation operation, Queue<VariableDeclaration> argument)
    {
        var statements = new List<Statement>();
        foreach (var stmt in operation.Operations)
        {
            var node = Visit(stmt, argument);
            if (node is Statement statement)
                statements.Add(statement);
            else if (node is Expression expr)
                statements.Add(new NonSpecialExpressionStatement(expr));
        }

        // 根据上下文判断返回不同类型的语句块
        // 如果父节点是方法或函数，返回 FunctionBody
        if (operation.Parent is IMethodBodyOperation ||
            operation.Parent is ILocalFunctionOperation ||
            operation.Parent is IAnonymousFunctionOperation ||
            operation.Parent is IConstructorBodyOperation)
        {
            return new FunctionBody(NodeList.From(statements), strict: true);
        }

        // 如果父节点是类型或类定义的静态初始化块，返回 StaticBlock
        if (operation.Parent is IFieldInitializerOperation &&
            operation.Parent is IFieldReferenceOperation fieldRef &&
            fieldRef.Field?.IsStatic == true)
        {
            return new StaticBlock(NodeList.From(statements));
        }

        // 默认情况返回 NestedBlockStatement
        return new NestedBlockStatement(NodeList.From(statements));
    }

    /// <summary>
    /// 处理变量声明组操作
    /// C# 示例：int a = 1, b = 2, c;
    /// 转换结果：let a = 1, b = 2, c;
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation operation, Queue<VariableDeclaration> argument)
    {
        // 可以假设 IVariableDeclarationGroupOperation.Declarations 只包含一个元素
        // 除非你正在处理一些非常特殊的、涉及类型推断的复合声明（如 using 语句）。
        // 这里有一个关键点：IVariableDeclarationGroupOperation 主要用于表示局部变量。
        // 对于 using 语句，Roslyn 更倾向于使用 IUsingDeclarationOperation 或 IUsingOperation 来封装其语义。
        // 在这些操作内部，你可能会找到多个 IVariableDeclarationOperation，但它们不一定被包装在一个公开的 IVariableDeclarationGroupOperation 中。
        var declarators = new List<VariableDeclarator>();
        foreach (var declaration in operation.Declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                var node = Visit(declarator, argument);
                if (node is VariableDeclarator variableDeclarator)
                    declarators.Add(variableDeclarator);
                else
                    return HandleTransformationFailure(declarator);
            }
        }
        return new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarators));
    }

    /// <summary>
    /// 处理 switch 语句操作
    /// C# 示例：
    /// switch (value) {
    ///     case 1: 
    ///         DoOne(); 
    ///         break;
    ///     case 2:
    ///         DoTwo();
    ///         break;
    ///     default: 
    ///         DoDefault(); 
    ///         break;
    /// }
    /// 转换结果：直接转换为 JavaScript 的 switch 语句
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitSwitch(ISwitchOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Value, argument) is not Expression discriminant)
            return HandleTransformationFailure(operation);

        var cases = new List<SwitchCase>();
        foreach (var switchCase in operation.Cases)
        {
            var tests = new List<Expression?>();
            var consequent = new List<Statement>();

            // 处理case条件
            foreach (var clause in switchCase.Clauses)
            {
                if (clause.CaseKind == CaseKind.Default)
                {
                    tests.Add(null); // null表示default case
                }
                else if (clause is ISingleValueCaseClauseOperation singleValue)
                {
                    if (Visit(singleValue.Value, argument) is Expression caseValue)
                        tests.Add(caseValue);
                    else
                        return HandleTransformationFailure(singleValue.Value);
                }
                else return HandleTransformationFailure(clause);
            }

            // 处理case体
            foreach (var bodyOp in switchCase.Body)
            {
                var bodyNode = Visit(bodyOp, argument);
                if (bodyNode is Statement stmt)
                    consequent.Add(stmt);
                else if (bodyNode is Expression expr)
                    consequent.Add(new NonSpecialExpressionStatement(expr));
                else
                    HandleTransformationFailure(bodyOp);
            }

            // 为每个test值创建一个SwitchCase
            // 避免重复添加相同的consequent，只为第一个case添加语句
            for (int i = 0; i < tests.Count; i++)
            {
                var testExpr = tests[i];
                // 只有第一个case包含语句，其余case为fallthrough
                var statements = i == 0 ? consequent : [];
                cases.Add(new SwitchCase(testExpr, NodeList.From<Statement>(statements)));
            }
        }

        return new SwitchStatement(discriminant, NodeList.From<SwitchCase>(cases));
    }

    /// <summary>
    /// 处理 foreach 循环操作
    /// C# 示例：
    /// foreach (var item in collection) {
    ///     Console.WriteLine(item);
    /// }
    /// 转换结果：for (let item of collection) { console.log(item); }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitForEachLoop(IForEachLoopOperation operation, Queue<VariableDeclaration> argument)
    {
        // 获取循环变量 - 使用 LoopControlVariable 直接访问
        var left = Visit(operation.LoopControlVariable, argument);
        if (left is null)
            return HandleTransformationFailure(operation.LoopControlVariable);

        if (Visit(operation.Collection, argument) is not Expression right || Visit(operation.Body, argument) is not Statement body)
            return HandleTransformationFailure(operation);

        return new ForOfStatement(left, right, body, @await: false);
    }

    /// <summary>
    /// 处理 for 循环操作
    /// C# 示例：
    /// for (int i = 0; i < 10; i++) {
    ///     Console.WriteLine(i);
    /// }
    /// 转换结果：for (let i = 0; i < 10; i++) { console.log(i); }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitForLoop(IForLoopOperation operation, Queue<VariableDeclaration> argument)
    {
        StatementOrExpression? init = null;
        Expression? test = null;
        if (operation.Before.Length > 0)
        {
            var variableDecls = new List<VariableDeclaration>();
            foreach (var before in operation.Before)
            {
                if (Visit(before, argument) is not VariableDeclaration variableDecl)
                    return HandleTransformationFailure(before);

                variableDecls.Add(variableDecl);
            }
            if (variableDecls.Count == 1)
                init = variableDecls[0];
            else
            {
                var declarations = new List<VariableDeclarator>();
                foreach (var decl in variableDecls)
                    declarations.AddRange(decl.Declarations);

                variableDecls.Select(x => x.Declarations);
                init = new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarations));
            }
        }

        if (operation.Condition is not null)
        {
            test = Visit(operation.Condition, argument) as Expression;
            if (test is null)
                return HandleTransformationFailure(operation.Condition);
        }

        // 处理多个 AtLoopBottom 操作的情况
        // IForLoopOperation.AtLoopBottom 出现“多个”运算并不代表 C# 支持写多个迭代表达式，
        // 而是 Roslyn 在 lowering 阶段把源码里的一个迭代表达式拆成多条中间指令。
        // 常见场景：
        // 1. 复合赋值  i += x + y  →  先算临时变量，再执行加法赋值
        // 2. 方法调用  M(out var tmp)  →  调用 + 丢弃返回值
        // 3. 异步/迭代器状态机生成
        // 遍历列表时按顺序依次输出即可，源代码层面仍只有一段“迭代表达式”。        
        Expression? updateExpression = null;
        if (operation.AtLoopBottom.Length > 0)
        {
            // 如果只有一个操作，直接使用
            if (operation.AtLoopBottom.Length == 1)
            {
                var updateStatement = Visit(operation.AtLoopBottom[0], argument) as NonSpecialExpressionStatement;
                updateExpression = updateStatement?.Expression;
            }
            else
            {
                // 如果有多个操作，将它们组合成一个逗号表达式
                var expressions = new List<Expression>();
                foreach (var atLoopBottomOp in operation.AtLoopBottom)
                {
                    var stmt = Visit(atLoopBottomOp, argument) as NonSpecialExpressionStatement;
                    if (stmt?.Expression is null)
                        return HandleTransformationFailure(atLoopBottomOp);

                    expressions.Add(stmt.Expression);
                }

                // 如果只有一个有效表达式，直接使用
                if (expressions.Count == 1)
                    updateExpression = expressions[0];

                // 如果有多个有效表达式，使用逗号表达式组合
                else if (expressions.Count > 1)
                {
                    updateExpression = expressions[0];
                    for (int i = 1; i < expressions.Count; i++)
                    {
                        updateExpression = new SequenceExpression(
                            NodeList.From([updateExpression, expressions[i]])
                        );
                    }
                }
            }
        }


        if (Visit(operation.Body, argument) is not Statement body)
            return HandleTransformationFailure(operation.Body);

        return new ForStatement(init, test, updateExpression, body);
    }

    /// <summary>
    /// 处理 while 循环操作
    /// C# 示例：
    /// while (condition) {
    ///     DoSomething();
    /// }
    /// 转换结果：while (condition) { doSomething(); }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitWhileLoop(IWhileLoopOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Condition!, argument) is not Expression test)
            return HandleTransformationFailure(operation.Condition!);//not null

        if (Visit(operation.Body, argument) is not Statement body)
            return HandleTransformationFailure(operation.Body);

        return new WhileStatement(test, body);
    }

    /// <summary>
    /// 处理标签语句操作
    /// C# 示例：
    /// label1:
    ///     Console.WriteLine("Labeled statement");
    /// goto label1;
    /// 转换结果：label1: console.log("Labeled statement");
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitLabeled(ILabeledOperation operation, Queue<VariableDeclaration> argument)
    {
        var label = new Identifier(operation.Label.Name);

        Statement statement;
        if (operation.Operation is null)
            statement = new EmptyStatement();
        else
        {
            if (Visit(operation.Operation, argument) is not Statement st)
                return HandleTransformationFailure(operation.Operation);
            statement = st;
        }

        return new LabeledStatement(label, statement);
    }

    /// <summary>
    /// 处理分支操作（break/continue）
    /// C# 示例：
    /// break;           // 跳出循环
    /// continue;        // 继续下一次循环
    /// break label1;    // 跳出到指定标签
    /// 转换结果：break; / continue; / break label1;
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitBranch(IBranchOperation operation, Queue<VariableDeclaration> argument)
    {
        var label = new Identifier(operation.Target.Name);

        return operation.BranchKind switch
        {
            BranchKind.Break => new BreakStatement(label),
            BranchKind.Continue => new ContinueStatement(label),
            _ => HandleTransformationFailure(operation, $"Branch kind {operation.BranchKind} is not supported in JavaScript conversion")
        };
    }

    /// <summary>
    /// 处理空语句操作
    /// C# 示例：; // 空语句
    /// 转换结果：; // JavaScript 空语句
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitEmpty(IEmptyOperation operation, Queue<VariableDeclaration> argument)
        => new EmptyStatement();

    /// <summary>
    /// 处理 return 语句操作
    /// C# 示例：
    /// return;          // 无返回值
    /// return value;    // 返回值
    /// 转换结果：return; / return value;
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitReturn(IReturnOperation operation, Queue<VariableDeclaration> argument)
    {
        if (operation.ReturnedValue is null)
            return new ReturnStatement(null);

        if (Visit(operation.ReturnedValue, argument) is not Expression exp)
            return HandleTransformationFailure(operation.ReturnedValue);

        return new ReturnStatement(exp);
    }

    /// <summary>
    /// 处理 try-catch-finally 语句操作
    /// C# 示例：
    /// try {
    ///     RiskyOperation();
    /// } catch (Exception ex) {
    ///     HandleError(ex);
    /// } finally {
    ///     Cleanup();
    /// }
    /// 转换结果：try { riskyOperation(); } catch (ex) { handleError(ex); } finally { cleanup(); }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitTry(ITryOperation operation, Queue<VariableDeclaration> argument)
    {
        var bodyStatements = new List<Statement>();
        foreach (var stmt in operation.Body.Operations)
        {
            var node = Visit(stmt, argument);
            if (node is Statement statement)
                bodyStatements.Add(statement);
            else
                HandleTransformationFailure(stmt);
        }
        var block = new NestedBlockStatement(NodeList.From(bodyStatements));

        // js只有单catch，多个catch需要合并成一个catch，在内部使用if分支
        CatchClause? handler = null;
        if (operation.Catches.Length == 1)
        {
            var @catch = operation.Catches[0];
            if (Visit(@catch, argument) is not CatchClause node)
                return HandleTransformationFailure(@catch);
        }
        else if (operation.Catches.Length > 1)
        {
            // 定义catch使用的param
            var tryParam = new Identifier(GetUniqueName(operation));
            var queue = new Stack<ICatchClauseOperation>();
            foreach (var @catch in operation.Catches)
                queue.Push(@catch);

            Statement? alternate = null;
            while (queue.TryPop(out var @catch))
            {
                var right = new Identifier(@catch.ExceptionType.Name);
                var statements = ExtractCatchClauseBody(@catch, argument);
                var param = ExtractCatchClauseParam(@catch);
                if (param is not null)
                {
                    // 需要定义一个变量，将tryParam转为当前catch的param
                    var catchParamDeclarator = new VariableDeclarator(param, tryParam);
                    var catchParamDeclaration = new VariableDeclaration(
                        VariableDeclarationKind.Const,
                        NodeList.From(catchParamDeclarator));
                    statements.Insert(0, catchParamDeclaration);
                }
                var body = new NestedBlockStatement(NodeList.From(statements));
                var test = new NonLogicalBinaryExpression(Operator.InstanceOf, tryParam, right);
                alternate = new IfStatement(test, body, alternate: alternate);
            }

            if (alternate is null)
                return HandleTransformationFailure(operation);

            var catchBody = new NestedBlockStatement(NodeList.From(alternate));
            handler = new CatchClause(tryParam, catchBody);
        }

        NestedBlockStatement? finalizer = null;
        if (operation.Finally is not null)
        {
            var finallyStatements = new List<Statement>();
            foreach (var stmt in operation.Finally.Operations)
            {
                var node = Visit(stmt, argument);
                if (node is Statement statement)
                    finallyStatements.Add(statement);
                else
                    return HandleTransformationFailure(operation);
            }
            finalizer = new NestedBlockStatement(NodeList.From(finallyStatements));
        }

        return new TryStatement(block, handler, finalizer);
    }

    /// <summary>
    /// 从ExceptionDeclarationOrExpression中提取异常变量名
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="argument"></param>
    /// <returns></returns>
    private Acornima.Ast.Identifier? ExtractCatchClauseParam(ICatchClauseOperation operation)
    {
        // 从ExceptionDeclarationOrExpression中提取异常变量名
        Identifier? param = null;

        if (operation.ExceptionDeclarationOrExpression is not null)
        {
            // 尝试从异常声明中提取变量名
            switch (operation.ExceptionDeclarationOrExpression)
            {
                case ILocalReferenceOperation localRef when localRef.Local is not null:
                    param = new Identifier(localRef.Local.Name);
                    break;
                case IParameterReferenceOperation paramRef when paramRef.Parameter is not null:
                    param = new Identifier(paramRef.Parameter.Name);
                    break;
                case IVariableDeclaratorOperation varDeclarator when varDeclarator.Symbol is not null:
                    param = new Identifier(varDeclarator.Symbol.Name);
                    break;
                default:
                    HandleTransformationFailure(operation.ExceptionDeclarationOrExpression);
                    break;
            }
        }

        return param;
    }

    /// <summary>
    /// 从ExceptionDeclarationOrExpression中提取异常变量名
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="argument"></param>
    /// <returns></returns>
    private List<Acornima.Ast.Statement> ExtractCatchClauseBody(ICatchClauseOperation operation, Queue<VariableDeclaration> argument)
    {
        var bodyStatements = new List<Statement>();
        foreach (var stmt in operation.Handler.Operations)
        {
            var node = Visit(stmt, argument);
            if (node is Statement statement)
                bodyStatements.Add(statement);
            else if (node is Expression expr)
                bodyStatements.Add(new NonSpecialExpressionStatement(expr));
            else
                HandleTransformationFailure(stmt);
        }
        return bodyStatements;
    }

    /// <summary>
    /// 处理 catch 子句操作
    /// C# 示例：
    /// try { ... }
    /// catch (Exception ex) { ... }     // catch 子句
    /// catch (InvalidOperationException) { ... } // 不带变量的 catch
    /// 转换结果：catch (ex) { ... } / catch (error) { ... }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitCatchClause(ICatchClauseOperation operation, Queue<VariableDeclaration> argument)
    {
        // 此处不用担心多个catch，多catch会在 VisitTry中处理
        var param = ExtractCatchClauseParam(operation);
        var statements = ExtractCatchClauseBody(operation, argument);
        var body = new NestedBlockStatement(NodeList.From(statements));

        return new CatchClause(param, body);
    }

    /// <summary>
    /// 处理表达式语句操作
    /// C# 示例：
    /// Method();        // 方法调用表达式语句
    /// x++;            // 自增表达式语句
    /// 转换结果：method(); / x++;
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitExpressionStatement(IExpressionStatementOperation operation, Queue<VariableDeclaration> argument)
    {
        return Visit(operation.Operation, argument) is Expression expr
            ? new NonSpecialExpressionStatement(expr)
            : HandleTransformationFailure(operation.Operation);
    }

    /// <summary>
    /// 处理局部函数操作
    /// C# 示例：
    /// void LocalFunction(int param) {
    ///     Console.WriteLine(param);
    /// }
    /// LocalFunction(42);
    /// 转换结果：function localFunction(param) { console.log(param); }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitLocalFunction(ILocalFunctionOperation operation, Queue<VariableDeclaration> argument)
    {
        var id = new Identifier(operation.Symbol.Name);
        var parameters = new List<Node>();
        foreach (var param in operation.Symbol.Parameters)
            parameters.Add(new Identifier(param.Name));

        var bodyStatements = new List<Statement>();
        if (operation.Body is not null)
        {
            foreach (var stmt in operation.Body.Operations)
            {
                var node = Visit(stmt, argument);
                if (node is Statement statement)
                    bodyStatements.Add(statement);
                else if (node is Expression expr)
                    bodyStatements.Add(new NonSpecialExpressionStatement(expr));
                else
                    HandleTransformationFailure(stmt);
            }
        }

        var body = new FunctionBody(NodeList.From(bodyStatements), strict: true);

        // 检查函数是否为async或generator
        var isAsync = operation.Symbol.IsAsync;

        // 检查是否返回IEnumerable类型（可能是迭代器）
        var returnTypeName = operation.Symbol.ReturnType.Name;
        var isGenerator = returnTypeName.Contains("IEnumerable") || returnTypeName.Contains("IEnumerator");

        return new FunctionDeclaration(id,
            NodeList.From(parameters),
            body,
            generator: isGenerator,
            @async: isAsync);
    }

    /// <summary>
    /// 处理字面量操作
    /// C# 示例：
    /// 42              // 整数字面量
    /// "Hello"         // 字符串字面量
    /// true            // 布尔字面量
    /// 'A'             // 字符字面量
    /// null            // 空字面量
    /// 转换结果：42 / "Hello" / true / "A" / null
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitLiteral(ILiteralOperation operation, Queue<VariableDeclaration> argument)
    {
        if (operation.ConstantValue.Value is null)
            return new NullLiteral("null");

        var value = operation.ConstantValue.Value;
        var raw = value.ToString() ?? "null";

        return operation.Type?.SpecialType switch
        {
            SpecialType.System_Boolean => new BooleanLiteral((bool)value, raw.ToLower()),
            SpecialType.System_String => new StringLiteral((string)value, $"\"{raw}\""),
            SpecialType.System_Char => new StringLiteral(value.ToString() ?? "", $"'{raw}'"),
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
            SpecialType.System_Decimal => new NumericLiteral(System.Convert.ToDouble(value), raw),
            _ => new NullLiteral("null")
        };
    }

    /// <summary>
    /// 处理类型转换操作
    /// C# 示例：
    /// (int)3.14       // 显式类型转换
    /// obj as Type     // 安全类型转换
    /// 转换结果：直接返回操作数（JavaScript 是动态类型）
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitConversion(IConversionOperation operation, Queue<VariableDeclaration> argument)
    {
        return Visit(operation.Operand, argument);
    }

    /// <summary>
    /// 处理方法调用操作
    /// C# 示例：
    /// obj.Method(arg1, arg2)      // 实例方法调用
    /// StaticClass.Method(arg)     // 静态方法调用
    /// obj.ExtensionMethod(arg)     // 扩展方法调用
    /// 转换结果：obj.method(arg1, arg2) / staticClass.method(arg) / obj.extensionMethod(arg)
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInvocation(IInvocationOperation operation, Queue<VariableDeclaration> argument)
    {
        var arguments = new List<Expression>();

        foreach (var arg in operation.Arguments)
        {
            var argNode = Visit(arg.Value, argument) as Expression;
            if (argNode is not null)
                arguments.Add(argNode);
            else
                return HandleTransformationFailure(arg.Value);
        }

        // 判断方法调用的类型
        Expression callee;

        if (operation.Instance is null)
        {
            // 静态方法调用或扩展方法调用
            if (operation.TargetMethod.IsStatic == true)
            {
                // 静态方法调用：StaticClass.Method()
                var className = operation.TargetMethod.ContainingType.Name;
                var methodName = operation.TargetMethod.Name;
                callee = new MemberExpression(
                    new Identifier(className),
                    new Identifier(methodName),
                    computed: false,
                    optional: false
                );
            }
            else
            {
                // 扩展方法调用：ExtensionMethod(arg)
                var methodName = operation.TargetMethod.Name;
                callee = new Identifier(methodName);
            }
        }
        else
        {
            // 实例方法调用：obj.Method()
            var instanceNode = Visit(operation.Instance, argument) as Expression;
            if (instanceNode is null)
                return HandleTransformationFailure(operation.Instance);

            var methodName = operation.TargetMethod.Name;

            callee = new MemberExpression(
                instanceNode,
                new Identifier(methodName),
                computed: false,
                optional: false
            );
        }

        return new CallExpression(callee, NodeList.From(arguments), optional: false);
    }

    /// <summary>
    /// 处理数组元素访问操作
    /// C# 示例：
    /// array[0]        // 一维数组访问
    /// array[i, j]     // 多维数组访问（不支持）
    /// 转换结果：array[0] / 不支持多维数组
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitArrayElementReference(IArrayElementReferenceOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.ArrayReference, argument) is not Expression exp)
            return HandleTransformationFailure(operation.ArrayReference);

        var indices = new List<Expression>();
        foreach (var item in operation.Indices)
        {
            if (Visit(item, argument) is not Expression indexExpr)
                return HandleTransformationFailure(item);
            else
                indices.Add(indexExpr);
        }

        if (indices.Count == 1)
            return new MemberExpression(exp, indices[0], computed: true, optional: false);

        return HandleTransformationFailure(operation, "Multi-dimensional array access is not supported in JavaScript conversion");
    }

    /// <summary>
    /// 处理局部变量引用操作
    /// C# 示例：
    /// int localVar = 5;
    /// Console.WriteLine(localVar);  // localVar 引用
    /// 转换结果：localVar
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitLocalReference(ILocalReferenceOperation operation, Queue<VariableDeclaration> argument)
    {
        return new Identifier(operation.Local.Name);
    }

    /// <summary>
    /// 处理参数引用操作
    /// C# 示例：
    /// void Method(int param) {
    ///     Console.WriteLine(param);  // param 引用
    /// }
    /// 转换结果：param
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitParameterReference(IParameterReferenceOperation operation, Queue<VariableDeclaration> argument)
    {
        return new Identifier(operation.Parameter.Name);
    }

    /// <summary>
    /// 处理字段引用操作
    /// C# 示例：
    /// obj.field        // 实例字段访问
    /// MyClass.field    // 静态字段访问
    /// 转换结果：obj.field / MyClass.field
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitFieldReference(IFieldReferenceOperation operation, Queue<VariableDeclaration> argument)
    {
        if (operation.Instance is not null)
        {
            if (Visit(operation.Instance, argument) is not Expression exp)
                return HandleTransformationFailure(operation.Instance);

            var property = new Identifier(operation.Field.Name);
            return new MemberExpression(exp, property, computed: false, optional: false);

        }
        return new Identifier(operation.Field.Name);
    }

    /// <summary>
    /// 处理方法引用操作（不调用）
    /// C# 示例：
    /// Action action = obj.Method;  // 方法引用（委托）
    /// 转换结果：obj.method
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitMethodReference(IMethodReferenceOperation operation, Queue<VariableDeclaration> argument)
    {
        if (operation.Instance is not null)
        {
            if (Visit(operation.Instance, argument) is not Expression exp)
                return HandleTransformationFailure(operation.Instance);

            var methodName = operation.Method.Name;
            var property = new Identifier(methodName);
            return new MemberExpression(exp, property, computed: false, optional: false);
        }
        return new Identifier(operation.Method.Name);
    }

    /// <summary>
    /// 处理属性引用操作
    /// C# 示例：
    /// obj.Property     // 实例属性访问
    /// MyClass.Property // 静态属性访问
    /// 转换结果：obj.property / MyClass.property
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitPropertyReference(IPropertyReferenceOperation operation, Queue<VariableDeclaration> argument)
    {
        if (operation.Instance is not null)
        {
            if (Visit(operation.Instance, argument) is not Expression exp)
                return HandleTransformationFailure(operation.Instance);

            var propName = operation.Property.Name;
            var property = new Identifier(propName);
            return new MemberExpression(exp, property, computed: false, optional: false);
        }
        return new Identifier(operation.Property.Name);
    }

    /// <summary>
    /// 处理一元运算符操作
    /// C# 示例：
    /// +x              // 正号运算符
    /// -x              // 负号运算符
    /// !condition      // 逻辑非运算符
    /// ~value          // 按位取反运算符
    /// 转换结果：+x / -x / !condition / ~value
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitUnaryOperator(IUnaryOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Operand, argument) is not Expression operand)
            return HandleTransformationFailure(operation.Operand);

        if (operation.OperatorKind == UnaryOperatorKind.BitwiseNegation ||
            operation.OperatorKind == UnaryOperatorKind.Not ||
            operation.OperatorKind == UnaryOperatorKind.Plus ||
            operation.OperatorKind == UnaryOperatorKind.Minus)
        {
            // 一元运算
            var @operator = operation.OperatorKind switch
            {
                UnaryOperatorKind.BitwiseNegation => Operator.BitwiseNot,
                UnaryOperatorKind.Not => Operator.LogicalNot,
                UnaryOperatorKind.Plus => Operator.UnaryPlus,
                UnaryOperatorKind.Minus => Operator.UnaryNegation,
                _ => Operator.Unknown
            };
            return new NonUpdateUnaryExpression(@operator, operand);
        }
        else if (operation.OperatorKind == UnaryOperatorKind.True ||
                 operation.OperatorKind == UnaryOperatorKind.False)
        {
            // 将操作数强制转换为布尔值，应该转换为!!(operand) 或 Boolean(operand)
            var innerOperand = new NonUpdateUnaryExpression(Operator.LogicalNot, operand);
            return new NonUpdateUnaryExpression(Operator.LogicalNot, innerOperand);
        }
        else if (operation.OperatorKind == UnaryOperatorKind.None)
        {
            // 对应语义()
            return new ParenthesizedExpression(operand);
        }
        else
            return HandleTransformationFailure(operation.Operand, "");
    }

    /// <summary>
    /// 处理二元运算符操作
    /// C# 示例：
    /// a + b           // 加法运算
    /// a - b           // 减法运算
    /// a * b           // 乘法运算
    /// a / b           // 除法运算
    /// a % b           // 取模运算
    /// a == b          // 相等比较
    /// a != b          // 不等比较
    /// a < b           // 小于比较
    /// a > b           // 大于比较
    /// a <= b          // 小于等于比较
    /// a >= b          // 大于等于比较
    /// a &amp;&amp; b          // 逻辑与运算
    /// a || b          // 逻辑或运算
    /// 转换结果：相同的 JavaScript 运算符
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitBinaryOperator(IBinaryOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.LeftOperand, argument) is not Expression left)
            return HandleTransformationFailure(operation.LeftOperand);

        if (Visit(operation.RightOperand, argument) is not Expression right)
            return HandleTransformationFailure(operation.RightOperand);

        var @operator = operation.OperatorKind switch
        {
            BinaryOperatorKind.Add => Operator.Addition,
            BinaryOperatorKind.Subtract => Operator.Subtraction,
            BinaryOperatorKind.Multiply => Operator.Multiplication,
            BinaryOperatorKind.Divide => Operator.Division,
            BinaryOperatorKind.Remainder => Operator.Remainder,
            BinaryOperatorKind.Equals => Operator.Equality,
            BinaryOperatorKind.NotEquals => Operator.Inequality,
            BinaryOperatorKind.LessThan => Operator.LessThan,
            BinaryOperatorKind.GreaterThan => Operator.GreaterThan,
            BinaryOperatorKind.LessThanOrEqual => Operator.LessThanOrEqual,
            BinaryOperatorKind.GreaterThanOrEqual => Operator.GreaterThanOrEqual,
            BinaryOperatorKind.ConditionalAnd => Operator.LogicalAnd,
            BinaryOperatorKind.ConditionalOr => Operator.LogicalOr,
            _ => Operator.Unknown
        };

        // 逻辑运算符 → LogicalExpression
        if (@operator is Operator.LogicalAnd or Operator.LogicalOr)
            return new LogicalExpression(@operator, left, right);

        else if (@operator == Operator.Unknown)
            return HandleTransformationFailure(operation);

        // 其余 → BinaryExpression
        return new NonLogicalBinaryExpression(@operator, left, right);
    }

    /// <summary>
    /// 处理条件运算符操作（三元运算符）
    /// C# 示例：
    /// condition ? trueValue : falseValue
    /// 转换结果：condition ? trueValue : falseValue
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitConditional(IConditionalOperation operation, Queue<VariableDeclaration> argument)
    {
        var alternate = Visit(operation.WhenFalse, argument);
        var consequent = Visit(operation.WhenTrue, argument);
        if (consequent is null)
            return HandleTransformationFailure(operation.WhenTrue);

        if (Visit(operation.Condition, argument) is not Expression test)
            return HandleTransformationFailure(operation.Condition);

        if (operation.Syntax is ConditionalExpressionSyntax &&
            consequent is Expression expConsequent &&
            alternate is Expression expAlternate)
        {
            // 这是三元表达式 a ? b : c
            // 生成 JavaScript 的三元表达式
            return new ConditionalExpression(test, expConsequent, expAlternate);
        }
        else if (operation.Syntax is IfStatementSyntax &&
            consequent is Statement ifConsequent)
        {
            // 这是 if 语句
            // 生成 JavaScript 的 if...else 语句
            return new IfStatement(test, ifConsequent, alternate as Statement);
        }

        return HandleTransformationFailure(operation);
    }

    /// <summary>
    /// 处理空合并运算符操作
    /// C# 示例：
    /// value ?? defaultValue
    /// 转换结果：value ?? defaultValue
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitCoalesce(ICoalesceOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.WhenNull, argument) is not Expression left)
            return HandleTransformationFailure(operation.WhenNull);

        if (Visit(operation.Value, argument) is not Expression right)
            return HandleTransformationFailure(operation.Value);

        return new LogicalExpression(Operator.NullishCoalescing, left, right);
    }

    /// <summary>
    /// 处理匿名函数操作（Lambda 表达式）
    /// C# 示例：
    /// (x, y) => x + y               // 多参数 Lambda
    /// x => x * 2                   // 单参数 Lambda
    /// () => Console.WriteLine("Hi") // 无参数 Lambda
    /// 转换结果：(x, y) => { return x + y; } / x => { return x * 2; }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitAnonymousFunction(IAnonymousFunctionOperation operation, Queue<VariableDeclaration> argument)
    {
        var parameters = new List<Node>();
        if (operation.Symbol?.Parameters is not null)
        {
            foreach (var param in operation.Symbol.Parameters)
            {
                var paramName = param.Name;
                parameters.Add(new Identifier(paramName));
            }
        }

        var statements = new List<Statement>();
        if (operation.Body is not null)
        {
            foreach (var stmt in operation.Body.Operations)
            {
                var node = Visit(stmt, argument);
                if (node is Statement statement)
                    statements.Add(statement);
                else if (node is Expression expr)
                    statements.Add(new NonSpecialExpressionStatement(expr));
                else
                    return HandleTransformationFailure(stmt);
            }
        }

        var body = new FunctionBody(NodeList.From(statements), strict: true);

        // 创建箭头函数
        return new ArrowFunctionExpression(
            NodeList.From(parameters), body,
            @async: false,
            expression: false);
    }

    /// <summary>
    /// 处理对象创建操作
    /// C# 示例：
    /// new MyClass()               // 无参数构造
    /// new MyClass(arg1, arg2)     // 有参数构造
    /// new { Name = "John", Age = 30 }  // 匿名类型
    /// 转换结果：new MyClass() / new MyClass(arg1, arg2) / { Name: "John", Age: 30 }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitObjectCreation(IObjectCreationOperation operation, Queue<VariableDeclaration> argument)
    {
        if (operation.Type is null)
            return HandleTransformationFailure(operation);

        // 处理匿名类型创建
        if (operation.Type.IsAnonymousType)
        {
            var properties = new List<Node>();
            if (operation.Initializer is not null)
            {
                foreach (var initializer in operation.Initializer.Initializers)
                {
                    var node = Visit(initializer, argument);
                    if (node is null)
                        return HandleTransformationFailure(initializer);
                    else
                        properties.Add(node);
                }
            }

            return new ObjectExpression(NodeList.From(properties));
        }

        // 普通对象创建
        var callee = new Identifier(operation.Type.Name);
        var arguments = new List<Expression>();

        foreach (var arg in operation.Arguments)
        {
            if (Visit(arg.Value, argument) is not Expression argNode)
                return HandleTransformationFailure(arg.Value);
            else
                arguments.Add(argNode);
        }

        return new NewExpression(callee, NodeList.From(arguments));
    }

    /// <summary>
    /// 处理泛型对象创建操作
    /// C# 示例：
    /// new T()                            // 泛型类型参数构造
    /// new List<T>()                      // 泛型集合构造
    /// 转换结果：忽略泛型参数，转换为普通对象创建 new T() / new List()
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, Queue<VariableDeclaration> argument)
    {
        if (operation.Type is null)
            return HandleTransformationFailure(operation);

        // 泛型类型参数对象创建，忽略泛型参数，当作普通对象创建
        var typeName = operation.Type.Name;
        var callee = new Identifier(typeName);

        // 泛型类型参数对象通常使用无参数构造函数
        return new NewExpression(callee, NodeList.From<Expression>());
    }

    /// <summary>
    /// 处理数组创建操作
    /// C# 示例：
    /// new int[] {1, 2, 3}         // 带初始化器的数组
    /// new int[5]                  // 指定大小的数组
    /// new int[,] {{1,2}, {3,4}}   // 多维数组（转为 new Array(size)）
    /// 转换结果：[1, 2, 3] / new Array(5)
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitArrayCreation(IArrayCreationOperation operation, Queue<VariableDeclaration> argument)
    {
        // 检查是否为多维数组
        if (operation.Type is IArrayTypeSymbol arrayType)
        {
            if (arrayType.Rank > 1)
            {
                // 多维数组在 C# 中可以创建但无法访问，导致“能创建却无法访问”的悄论
                // 为保证语义一致性，禁止创建多维数组
                return HandleTransformationFailure(operation,
                    $"多维数组（Rank={arrayType.Rank}）在 JavaScript 中不存在，无法保证语义等价，同时数组元素访问也不支持多维索引");
            }
        }

        var elements = new List<Expression?>();
        if (operation.Initializer is not null)
        {
            foreach (var element in operation.Initializer.ElementValues)
            {
                if (Visit(element, argument) is not Expression elementNode)
                    return HandleTransformationFailure(element);

                elements.Add(elementNode);
            }
        }
        else
        {
            // 处理空数组或基于大小的数组
            foreach (var dimension in operation.DimensionSizes)
            {
                if (Visit(dimension, argument) is not Expression sizeNode)
                    return HandleTransformationFailure(dimension);

                // 为简化，创建一个空数组，实际可能需要 new Array(size)
                return new NewExpression(new Identifier("Array"), NodeList.From(sizeNode));
            }
        }

        return new ArrayExpression(NodeList.From(elements));
    }

    /// <summary>
    /// 处理匿名对象创建操作
    /// C# 示例：
    /// new { Name = "John", Age = 25 }  // 匿名对象
    /// 转换结果：{ name: "John", age: 25 }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, Queue<VariableDeclaration> argument)
    {
        var properties = new List<Node>();

        foreach (var initializer in operation.Initializers)
        {
            var property = Visit(initializer, argument);
            if (property is null)
                return HandleTransformationFailure(initializer);
            else
                properties.Add(property);
        }

        return new ObjectExpression(NodeList.From(properties));
    }

    /// <summary>
    /// 处理对象或集合初始化器操作
    /// C# 示例：
    /// new List<int> { 1, 2, 3 }      // 集合初始化器
    /// new MyClass { Prop1 = val1 }   // 对象初始化器
    /// 转换结果：{ prop1: val1 } / [1, 2, 3]
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, Queue<VariableDeclaration> argument)
    {
        var initializers = new List<Node>();

        foreach (var initializer in operation.Initializers)
        {
            var node = Visit(initializer, argument);
            if (node is null)
                return HandleTransformationFailure(initializer);
            else
                initializers.Add(node);
        }

        // 默认返回对象表达式
        return new ObjectExpression(NodeList.From(initializers));
    }

    /// <summary>
    /// 处理成员初始化器操作
    /// C# 示例：
    /// new MyClass { Property = value } 中的 Property = value 部分
    /// 转换结果：property = value
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitMemberInitializer(IMemberInitializerOperation operation, Queue<VariableDeclaration> argument)
    {
        string memberName;
        if (operation.InitializedMember is IFieldSymbol field)
        {
            memberName = field.Name;
        }
        else if (operation.InitializedMember is IPropertySymbol property)
        {
            memberName = property.Name;
        }
        else
            return HandleTransformationFailure(operation.InitializedMember);

        var key = new Identifier(memberName);

        if (Visit(operation.Initializer, argument) is not Expression value)
            return HandleTransformationFailure(operation.Initializer);

        return new AssignmentExpression(Operator.Assignment, key, value);
    }

    /// <summary>
    /// 处理实例引用操作（this 关键字）
    /// C# 示例：
    /// this.Property   // 引用当前实例
    /// this            // 直接使用 this
    /// 转换结果：this
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInstanceReference(IInstanceReferenceOperation operation, Queue<VariableDeclaration> argument)
    {
        return new ThisExpression();
    }

    /// <summary>
    /// 处理类型检查操作（is 运算符）
    /// C# 示例：
    /// obj is string   // 检查对象是否为特定类型
    /// typeof obj === 'string'
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitIsType(IIsTypeOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.ValueOperand, argument) is not Expression valueOperand)
            return HandleTransformationFailure(operation.ValueOperand);

        var targetType = operation.TypeOperand;
        var typeName = targetType.Name;
        var fullTypeName = targetType.ToDisplayString();

        // 类型映射
        // object -> js object
        // string -> js string
        // byte、sbyte、short、ushort、int、uint、decimal、double、float -> js Number
        // long、timestamp -> BigInt
        // DateOnly、TimeOnly、DateTime、DateTimeOffset -> js Date
        // Array -> js array
        // IDictionary -> js Map
        // IEnumerable(非IDictionary) -> js Set
        // 其他 class -> js class

        // 使用 SpecialType 进行基础类型检查，更加类型安全和高效
        switch (targetType.SpecialType)
        {
            case SpecialType.System_String:
                return new NonLogicalBinaryExpression(
                    Operator.StrictEquality,
                    new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
                    new StringLiteral("string", "\"string\"")
                );
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                return new NonLogicalBinaryExpression(
                    Operator.StrictEquality,
                    new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
                    new StringLiteral("number", "\"number\"")
                );
            case SpecialType.System_Boolean:
                return new NonLogicalBinaryExpression(
                    Operator.StrictEquality,
                    new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
                    new StringLiteral("boolean", "\"boolean\"")
                );
            case SpecialType.System_Object:
                return new LogicalExpression(
                    Operator.LogicalAnd,
                    new NonLogicalBinaryExpression(
                        Operator.StrictEquality,
                        new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
                        new StringLiteral("object", "\"object\"")
                    ),
                    new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
                );
        }

        // 对于非基础类型，使用字符串名称进行判断
        // 大整数类型检查（long、timestamp等）
        if (targetType.SpecialType == SpecialType.System_Int64 ||
            targetType.SpecialType == SpecialType.System_UInt64 ||
            typeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
        {
            return new LogicalExpression(
                Operator.LogicalAnd,
                new NonLogicalBinaryExpression(
                    Operator.StrictEquality,
                    new NonUpdateUnaryExpression(Operator.TypeOf, valueOperand),
                    new StringLiteral("bigint", "\"bigint\"")
                ),
                new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
            );
        }

        // 日期类型检查
        if (typeName.Equals("DateOnly", StringComparison.OrdinalIgnoreCase) ||
            typeName.Equals("TimeOnly", StringComparison.OrdinalIgnoreCase) ||
            typeName.Equals("DateTime", StringComparison.OrdinalIgnoreCase) ||
            typeName.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
        {
            return new LogicalExpression(
                Operator.LogicalAnd,
                new NonLogicalBinaryExpression(Operator.InstanceOf, valueOperand, new Identifier("Date")),
                new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
            );
        }

        // 数组类型检查
        if (typeName.Equals("Array", StringComparison.OrdinalIgnoreCase) ||
            fullTypeName.Contains("[]"))
        {
            return new LogicalExpression(
                Operator.LogicalAnd,
                new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null")),
                new CallExpression(
                    new MemberExpression(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false),
                    NodeList.From(valueOperand),
                    optional: false
                )
            );
        }

        // 字典类型检查
        if (typeName.Equals("IDictionary", StringComparison.OrdinalIgnoreCase) ||
            (targetType is INamedTypeSymbol namedType &&
             namedType.AllInterfaces.Any(i => i.Name.Equals("IDictionary", StringComparison.OrdinalIgnoreCase))))
        {
            return new LogicalExpression(
                Operator.LogicalAnd,
                new NonLogicalBinaryExpression(Operator.InstanceOf, valueOperand, new Identifier("Map")),
                new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
            );
        }

        // 集合类型检查（非字典）
        if (typeName.Equals("IEnumerable", StringComparison.OrdinalIgnoreCase) ||
            (targetType is INamedTypeSymbol enumType &&
             enumType.AllInterfaces.Any(i => i.Name.Equals("IEnumerable", StringComparison.OrdinalIgnoreCase)) &&
             !enumType.AllInterfaces.Any(i => i.Name.Equals("IDictionary", StringComparison.OrdinalIgnoreCase))))
        {
            return new LogicalExpression(
                Operator.LogicalAnd,
                new NonLogicalBinaryExpression(Operator.InstanceOf, valueOperand, new Identifier("Set")),
                new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
            );
        }

        // 其他自定义类类型检查
        return new LogicalExpression(
            Operator.LogicalAnd,
            new NonLogicalBinaryExpression(Operator.InstanceOf, valueOperand, new Identifier(typeName)),
            new NonLogicalBinaryExpression(Operator.Inequality, valueOperand, new NullLiteral("null"))
        );
    }

    /// <summary>
    /// 处理 await 表达式操作
    /// C# 示例：
    /// await SomeAsyncMethod()     // 等待异步操作完成
    /// 转换结果：await someAsyncMethod()
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitAwait(IAwaitOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Operation, argument) is not Expression awaitedExpression)
            return HandleTransformationFailure(operation.Operation);

        return new AwaitExpression(awaitedExpression);
    }

    /// <summary>
    /// 处理简单赋值操作
    /// C# 示例：
    /// x = 5           // 基本赋值
    /// obj.prop = val  // 属性赋值
    /// arr[0] = item   // 数组元素赋值
    /// 转换结果：x = 5 / obj.prop = val / arr[0] = item
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitSimpleAssignment(ISimpleAssignmentOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Target, argument) is not Expression left)
            return HandleTransformationFailure(operation.Target);

        if (Visit(operation.Value, argument) is not Expression right)
            return HandleTransformationFailure(operation.Value);

        return new AssignmentExpression(Operator.Assignment, left, right);
    }

    /// <summary>
    /// 处理复合赋值操作
    /// C# 示例：
    /// x += 5          // 加法赋值
    /// x -= 3          // 减法赋值
    /// x *= 2          // 乘法赋值
    /// x /= 4          // 除法赋值
    /// x %= 7          // 取模赋值
    /// 转换结果：x += 5 / x -= 3 / x *= 2 / x /= 4 / x %= 7
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitCompoundAssignment(ICompoundAssignmentOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Target, argument) is not Expression left)
            return HandleTransformationFailure(operation.Target);

        if (Visit(operation.Value, argument) is not Expression right)
            return HandleTransformationFailure(operation.Target);

        var @operator = operation.OperatorKind switch
        {
            BinaryOperatorKind.Add => Operator.AdditionAssignment,
            BinaryOperatorKind.Subtract => Operator.SubtractionAssignment,
            BinaryOperatorKind.Multiply => Operator.MultiplicationAssignment,
            BinaryOperatorKind.Divide => Operator.DivisionAssignment,
            BinaryOperatorKind.Remainder => Operator.RemainderAssignment,
            _ => Operator.Unknown
        };

        if (@operator == Operator.Unknown)
            return HandleTransformationFailure(operation, $"Compound assignment operator {operation.OperatorKind} is not supported");

        return new AssignmentExpression(@operator, left, right);
    }

    /// <summary>
    /// 处理括号表达式操作
    /// C# 示例：
    /// (x + y)         // 括号表达式
    /// 转换结果：直接返回内部表达式（JavaScript 中括号由解析器处理）
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitParenthesized(IParenthesizedOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Operand, argument) is not Expression exp)
            return HandleTransformationFailure(operation.Operand);

        return new SequenceExpression(NodeList.From(exp));
    }

    /// <summary>
    /// 处理条件访问操作（可选链操作符）
    /// C# 示例：
    /// obj?.Property               // 属性可选访问
    /// obj?.Method()               // 方法可选调用
    /// 转换结果：obj?.property
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitConditionalAccess(IConditionalAccessOperation operation, Queue<VariableDeclaration> argument)
    {
        // 不需要处理 operation.WhenNotNull
        if (Visit(operation.Operation, argument) is not Expression operand)
            return HandleTransformationFailure(operation.Operation);

        // 转换为可选链
        return new ChainExpression(operand);
    }

    /// <summary>
    /// 处理条件访问实例操作。
    /// C# 示例：
    /// obj?.member 中的 obj 部分
    /// 转换结果：递归访问并返回 obj 部分对应的节点。
    /// </summary>
    public override Acornima.Ast.Node? VisitConditionalAccessInstance(IConditionalAccessInstanceOperation operation, Queue<VariableDeclaration> argument)
    {
        // IConditionalAccessInstanceOperation 是一个包装器。
        // 它唯一的子操作就是 ?. 左侧的表达式 (someExpr)。
        // 我们需要访问并返回这个子操作的结果。
        // 递归访问这个子操作，将其进行转换。
        // 现有语法下，理论上 ChildOperations只会有一个元素
        return Visit(operation.ChildOperations.First(), argument);
    }

    /// <summary>
    /// 处理插值字符串操作
    /// C# 示例：
    /// $"Hello, {name}!"           // 插值字符串
    /// $"Value: {x + y}"           // 包含表达式的插值字符串
    /// 转换结果：`Hello${name}!` / `Value: ${(x + y)}`
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInterpolatedString(IInterpolatedStringOperation operation, Queue<VariableDeclaration> argument)
    {
        var quasis = new List<TemplateElement>();
        var expressions = new List<Expression>();

        foreach (var part in operation.Parts)
        {
            switch (part)
            {
                case IInterpolatedStringTextOperation textOp:
                    // 遇到文本，直接添加为 quasi
                    var literal = textOp.Text as ILiteralOperation;
                    var cooked = literal?.ConstantValue.Value as string ?? "";
                    quasis.Add(new TemplateElement(
                        TemplateValue.From(cooked, cooked),
                        tail: false // tail 将在最后统一设置
                    ));
                    break;

                case IInterpolationOperation interpOp:
                    // 核心逻辑：在处理表达式前，确保它前面有一个 quasi。
                    // 如果当前 quasi 数量不比 expression 多一个，说明前面是表达式或这是开头，需要补一个空的 quasi。
                    if (quasis.Count == expressions.Count)
                    {
                        quasis.Add(new TemplateElement(
                            TemplateValue.From("", ""),
                            tail: false
                        ));
                    }

                    // 转换并添加表达式
                    var expr = Visit(interpOp.Expression, argument) as Expression;
                    if (expr is not null)
                    {
                        expressions.Add(expr);
                    }
                    break;
            }
        }

        // 循环结束后，处理尾部 quasi
        if (quasis.Count == expressions.Count)
        {
            // 如果数量相等，说明字符串以表达式结尾，需要补一个空的尾部 quasi。
            quasis.Add(new TemplateElement(TemplateValue.From("", ""), tail: true));
        }
        else if (quasis.Count > 0)
        {
            // 否则，字符串以文本结尾，将最后一个 quasi 标记为 tail。
            var lastQuasi = quasis[^1];
            quasis[^1] = new TemplateElement(lastQuasi.Value, tail: true);
        }

        // 优化：如果没有任何表达式，只有一个文本部分，返回更简单的 StringLiteral。
        if (expressions.Count == 0 && quasis.Count == 1)
        {
            return new StringLiteral(quasis[0].Value.Cooked ?? "", quasis[0].Value.Raw);
        }

        // 返回结构完整的 TemplateLiteral
        return new TemplateLiteral(NodeList.From(quasis), NodeList.From(expressions));
    }

    /// <summary>
    /// 处理 nameof 表达式操作
    /// C# 示例：
    /// nameof(variable)            // 获取变量名称
    /// nameof(MyClass.Property)    // 获取属性名称
    /// 转换结果："variable" / "Property"
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitNameOf(INameOfOperation operation, Queue<VariableDeclaration> argument)
    {
        string? name = null;
        if (operation.Argument.ConstantValue.HasValue)
            name = operation.Argument.ConstantValue.Value?.ToString();

        if (string.IsNullOrEmpty(name))
            return HandleTransformationFailure(operation.Argument);

        return new StringLiteral(name, $"\"{name}\"");
    }

    /// <summary>
    /// 处理元组操作
    /// C# 示例：
    /// (1, "hello", true)          // 元组字面量
    /// var tuple = (x, y);         // 元组创建
    /// (double Sum, int Count) t2 = (4.5, 3);// 命名元组创建
    /// 转换结果：{ Item1: 1, Item2: "hello", Item3: true } 或 { Sum: 4.5, Count: 3 } （使用对象模拟）
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitTuple(ITupleOperation operation, Queue<VariableDeclaration> argument)
    {
        var properties = new List<Node>();

        // 尝试从元组类型中获取元素名称
        var tupleType = operation.Type as INamedTypeSymbol;
        var elementNames = new List<string>();

        // 检查是否为命名元组
        if (tupleType?.TupleElements is not null && tupleType.TupleElements.Length > 0)
        {
            // 对于命名元组，使用指定的元素名称
            foreach (var element in tupleType.TupleElements)
            {
                elementNames.Add(element.Name);
            }
        }
        else
        {
            // 对于未命名元组，使用默认的 Item1, Item2, Item3...
            for (int i = 0; i < operation.Elements.Length; i++)
            {
                elementNames.Add($"Item{i + 1}");
            }
        }

        for (int i = 0; i < operation.Elements.Length; i++)
        {
            var element = operation.Elements[i];
            if (Visit(element, argument) is not Expression exp)
                return HandleTransformationFailure(element);

            // 使用获取的元素名称
            var propertyName = i < elementNames.Count ? elementNames[i] : $"Item{i + 1}";
            var key = new Identifier(propertyName);

            // 创建属性定义
            var property = new PropertyDefinition(
                key: key,
                value: exp,
                computed: false,
                isStatic: false,
                decorators: NodeList.Empty<Decorator>()
            );

            properties.Add(property);
        }

        return new ObjectExpression(NodeList.From(properties));
    }

    /// <summary>
    /// 处理委托创建操作
    /// C# 示例：
    /// Action action = Method;              // 方法组转委托
    /// Func<int, string> func = x => x.ToString(); // Lambda 转委托
    /// EventHandler handler = new EventHandler(OnEvent); // 显式委托创建
    /// 转换结果：转换为函数引用或箭头函数
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDelegateCreation(IDelegateCreationOperation operation, Queue<VariableDeclaration> argument)
    {
        // 委托创建转换为函数引用或箭头函数
        return Visit(operation.Target, argument);
    }

    /// <summary>
    /// 处理默认值操作
    /// C# 示例：
    /// default(int)                        // 0
    /// default(string)                     // null
    /// default(bool)                       // false
    /// default(T)                          // 泛型类型的默认值
    /// 转换结果：0 / "" / false / null（根据类型）
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDefaultValue(IDefaultValueOperation operation, Queue<VariableDeclaration> argument)
    {
        // default(T) 转换为适当的默认值
        return operation.Type?.SpecialType switch
        {
            SpecialType.System_Boolean => new BooleanLiteral(false, "false"),
            SpecialType.System_String => new StringLiteral("", "\"\""),
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
            SpecialType.System_Decimal => new NumericLiteral(0, "0"),
            _ => new NullLiteral("null")
        };
    }

    /// <summary>
    /// 处理 is 模式匹配操作
    /// C# 示例：
    /// obj is int value                    // 模式匹配并声明变量
    /// x is > 0 and < 10                   // 关系模式匹配
    /// input is "hello"                    // 常量模式匹配
    /// 转换结果：对于复杂模式，替换占位符并返回条件表达式
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitIsPattern(IIsPatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // is 模式转换，支持复杂模式匹配
        if (Visit(operation.Value, argument) is not Expression value)
            return HandleTransformationFailure(operation.Value);

        if (Visit(operation.Pattern, argument) is not Expression pattern)
            return HandleTransformationFailure(operation.Pattern);

        // 对于常量模式，直接比较
        if (operation.Pattern.Kind == OperationKind.ConstantPattern)
        {
            return new LogicalExpression(Operator.StrictEquality, value, pattern);
        }

        // 对于复杂模式，直接使用模式表达式（已经包含实际目标）
        return pattern;
    }

    /// <summary>
    /// 处理递增递减操作
    /// C# 示例：
    /// x++                                 // 后缀递增
    /// ++x                                 // 前缀递增
    /// x--                                 // 后缀递减
    /// --x                                 // 前缀递减
    /// 转换结果：x++ / ++x / x-- / --x
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Target, argument) is not Expression operand)
            return HandleTransformationFailure(operation);

        var @operator = operation.Kind == OperationKind.Increment
            ? Operator.Increment
            : Operator.Decrement;
        var prefix = !operation.IsPostfix; // 前缀当IsPostfix为false时

        return new UpdateExpression(@operator, operand, prefix: prefix);
    }

    /// <summary>
    /// 处理 throw 语句操作
    /// C# 示例：
    /// throw new Exception("Error message"); // 抛出异常
    /// throw;                              // 重新抛出当前异常
    /// 转换结果：throw new Error("Error message") / throw Error
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitThrow(IThrowOperation operation, Queue<VariableDeclaration> argument)
    {
        Expression expr;
        if (operation.Exception is not null)
        {
            if (Visit(operation.Exception, argument) is not Expression ex)
                return HandleTransformationFailure(operation.Exception);

            expr = ex;
        }
        else
            expr = new Identifier(GetUniqueName(operation));

        return new ThrowStatement(expr);
    }

    /// <summary>
    /// 处理解构赋值操作
    /// C# 示例：
    /// (var x, var y) = GetTuple();        // 元组解构
    /// (int a, string b) = (1, "hello");   // 元组解构赋值
    /// 转换结果：转换为普通赋值表达式（简化处理）
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Target, argument) is not Expression target)
            return HandleTransformationFailure(operation.Target);

        if (Visit(operation.Value, argument) is not Expression value)
            return HandleTransformationFailure(operation.Value);

        // 将解构赋值转换为普通赋值表达式
        return new AssignmentExpression(Operator.Assignment, target, value);
    }

    /// <summary>
    /// 处理省略参数操作
    /// C# 示例：
    /// SomeMethod(arg1, , arg3);           // 省略中间参数
    /// Optional(a: 1, c: 3);               // 命名参数中省略了 b
    /// 转换结果：undefined
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitOmittedArgument(IOmittedArgumentOperation operation, Queue<VariableDeclaration> argument)
    {
        // 省略的参数返回 undefined
        return new Identifier("undefined");
    }

    /// <summary>
    /// 处理数组初始化器操作
    /// C# 示例：
    /// new int[] {1, 2, 3, 4, 5}           // 数组初始化器
    /// {"apple", "banana", "cherry"}      // 集合初始化器
    /// 转换结果：[1, 2, 3, 4, 5] / ["apple", "banana", "cherry"]
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitArrayInitializer(IArrayInitializerOperation operation, Queue<VariableDeclaration> argument)
    {
        var elements = new List<Expression?>();

        foreach (var element in operation.ElementValues)
        {
            Expression? expr = null;
            if (element is not null)
            {
                if (Visit(element, argument) is not Expression exp)
                    return HandleTransformationFailure(element);

                expr = exp;
            }
            elements.Add(expr);
        }

        return new ArrayExpression(NodeList.From(elements));
    }

    /// <summary>
    /// 处理字段初始化器操作
    /// C# 示例：
    /// public int Field = 42;              // 字段初始化器
    /// private string _name = "default";   // 私有字段初始化
    /// 转换结果：直接返回初始化值表达式
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitFieldInitializer(IFieldInitializerOperation operation, Queue<VariableDeclaration> argument)
    {
        return Visit(operation.Value, argument);
    }

    /// <summary>
    /// 处理变量初始化器操作
    /// C# 示例：
    /// int x = 10;                         // 变量初始化器
    /// string name = "Hello";              // 字符串初始化
    /// 转换结果：直接返回初始化值表达式
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitVariableInitializer(IVariableInitializerOperation operation, Queue<VariableDeclaration> argument)
    {
        return Visit(operation.Value, argument);
    }

    /// <summary>
    /// 处理变量声明符操作
    /// C# 示例：
    /// int x = 5 中的 "x = 5" 部分      // 变量声明符
    /// string name 中的 "name" 部分     // 无初始化的声明符
    /// 转换结果：x = 5 / name
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitVariableDeclarator(IVariableDeclaratorOperation operation, Queue<VariableDeclaration> argument)
    {
        var identifier = new Identifier(operation.Symbol.Name);
        Expression? init = null;
        if (operation.Initializer is not null)
        {
            if (Visit(operation.Initializer, argument) is not Expression expr)
                return HandleTransformationFailure(operation.Initializer);

            init = expr;
        }
        return new VariableDeclarator(identifier, init);
    }

    /// <summary>
    /// 处理变量声明操作
    /// C# 示例：
    /// int x = 5, y = 10;                  // 多变量声明
    /// string name = "test", message;      // 混合声明
    /// 转换结果：let x = 5, y = 10; / let name = "test", message;
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitVariableDeclaration(IVariableDeclarationOperation operation, Queue<VariableDeclaration> argument)
    {
        var declarators = new List<VariableDeclarator>();
        foreach (var declarator in operation.Declarators)
        {
            if (Visit(declarator, argument) is VariableDeclarator variableDeclarator)
                declarators.Add(variableDeclarator);
            else
                return HandleTransformationFailure(declarator);
        }
        return new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarators));
    }

    /// <summary>
    /// 处理参数操作
    /// C# 示例：
    /// Method(arg1, ref arg2, out arg3)    // 方法参数
    /// Constructor(param1, param2)         // 构造函数参数
    /// 转换结果：直接返回参数值表达式
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitArgument(IArgumentOperation operation, Queue<VariableDeclaration> argument)
    {
        return Visit(operation.Value, argument);
    }

    /// <summary>
    /// 处理 switch case 操作
    /// C# 示例：
    /// switch (value) {
    ///     case 1:
    ///         DoSomething();
    ///         break;
    /// }
    /// 转换结果：转换为 if-else 链的一部分
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitSwitchCase(ISwitchCaseOperation operation, Queue<VariableDeclaration> argument)
    {
        // 将switch case转换为if-else链
        var clauses = operation.Clauses;
        if (clauses.Length == 0)
            return null;

        var statements = new List<Statement>();
        foreach (var clause in clauses)
        {
            var clauseNode = Visit(clause, argument);
            if (clauseNode is Statement stmt)
                statements.Add(stmt);
            else
                return HandleTransformationFailure(clause);
        }

        // 如果有body操作，添加到语句中
        foreach (var bodyOp in operation.Body)
        {
            var bodyNode = Visit(bodyOp, argument);
            if (bodyNode is Statement bodyStmt)
                statements.Add(bodyStmt);
            else if (bodyNode is Expression bodyExpr)
                statements.Add(new NonSpecialExpressionStatement(bodyExpr));
            else
                return HandleTransformationFailure(bodyOp);
        }

        return statements.Count > 0 ? new NestedBlockStatement(NodeList.From(statements)) : null;
    }

    /// <summary>
    /// 处理模式 case 子句操作
    /// C# 示例：
    /// switch (obj) {
    ///     case string s when s.Length > 0:
    ///         Console.WriteLine(s);
    ///         break;
    /// }
    /// 转换结果：转换为条件表达式
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitPatternCaseClause(IPatternCaseClauseOperation operation, Queue<VariableDeclaration> argument)
    {
        // 模式 case 子句转换为条件表达式
        return Visit(operation.Pattern, argument);
    }

    /// <summary>
    /// 处理单值 case 子句操作
    /// C# 示例：
    /// switch (value) {
    ///     case 42:
    ///         DoSomething();
    ///         break;
    /// }
    /// 转换结果：返回比较值，在上级组合成 if-else 链
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitSingleValueCaseClause(ISingleValueCaseClauseOperation operation, Queue<VariableDeclaration> argument)
    {
        // 将单值case转换为条件比较
        if (Visit(operation.Value, argument) is not Expression expr)
            return HandleTransformationFailure(operation.Value);

        // 返回比较表达式，需要在上级switch中组合成if-else
        return expr;
    }

    /// <summary>
    /// 处理插值字符串文本操作
    /// C# 示例：
    /// $"Hello {name}, welcome!" 中的 "Hello " 和 ", welcome!" 部分
    /// 转换结果：字符串字面量 "Hello " / ", welcome!"
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInterpolatedStringText(IInterpolatedStringTextOperation operation, Queue<VariableDeclaration> argument)
    {
        // 插值字符串中的文本部分转换为字符串字面量
        var text = operation.Text.ConstantValue.Value?.ToString() ?? "";
        return new StringLiteral(text, $"\"{text}\"");
    }

    /// <summary>
    /// 处理插值表达式操作
    /// C# 示例：
    /// $"Hello {name}!" 中的 {name} 部分
    /// $"Value: {x + y:F2}" 中的 {x + y:F2} 部分
    /// 转换结果：返回插值表达式 name / (x + y)，并处理格式化
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInterpolation(IInterpolationOperation operation, Queue<VariableDeclaration> argument)
    {
        // 插值表达式转换为表达式
        if (Visit(operation.Expression, argument) is not Expression expr)
            return HandleTransformationFailure(operation);

        // 修复：处理格式化说明符（如 :F2）
        // 注意：由于API限制，格式化信息可能不在IInterpolationOperation中
        // 这里保留原始行为，只返回表达式
        return expr;
    }

    /// <summary>
    /// 处理常量模式操作
    /// C# 示例：
    /// obj is 42              // 常量模式匹配
    /// obj is "hello"         // 字符串常量模式
    /// obj is null            // null 常量模式
    /// 转换结果：返回常量字面量进行比较
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitConstantPattern(IConstantPatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 常量模式转换为字面量比较
        return Visit(operation.Value, argument);
    }

    /// <summary>
    /// 处理声明模式操作
    /// C# 示例：
    /// obj is int value        // 类型模式声明
    /// obj is string { Length: > 0 } str // 属性模式声明
    /// 转换结果：转换为变量声明 let value / let str
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDeclarationPattern(IDeclarationPatternOperation operation, Queue<VariableDeclaration> argument)
    {
        if (operation.DeclaredSymbol is null)
            return null;

        // 声明模式转换为变量声明
        var identifier = new Identifier(operation.DeclaredSymbol.Name);
        return new VariableDeclaration(VariableDeclarationKind.Let,
            NodeList.From(new VariableDeclarator(identifier, null)));
    }

    /// <summary>
    /// 处理元组二元操作符操作
    /// C# 示例：
    /// (a, b) == (c, d)                    // 元组相等比较
    /// (x, y) != (1, 2)                    // 元组不等比较
    /// (name, age) == ("John", 25)         // 元组与常量比较
    /// tuple1 == tuple2                    // 元组变量比较
    /// 转换结果：利用编译时信息生成最简洁的比较代码
    /// </summary>
    /// <param name="operation">元组二元操作</param>
    /// <param name="argument">当前operation所属的父operation</param>
    /// <returns>JavaScript逻辑表达式</returns>
    public override Acornima.Ast.Node? VisitTupleBinaryOperator(ITupleBinaryOperation operation, Queue<VariableDeclaration> argument)
    {
        var isEq = operation.OperatorKind == BinaryOperatorKind.Equals;

        // 处理空元组比较：() == () 为 true, () == (1,) 为 false。
        // 空元组的类型没有 TupleElements。
        if (operation.Type is not INamedTypeSymbol { TupleElements.Length: > 0 } tupleType)
        {
            // 对于空元组，相等性仅取决于操作符本身。
            return new BooleanLiteral(isEq, isEq ? "true" : "false");
        }

        // 递归访问左右操作元，获取它们的表达式。
        if (Visit(operation.LeftOperand, argument) is not Expression left)
            return HandleTransformationFailure(operation.LeftOperand);

        if (Visit(operation.RightOperand, argument) is not Expression right)
            return HandleTransformationFailure(operation.RightOperand);

        Acornima.Ast.Expression? result = null;

        // 遍历元组的每个元素，为每个元素生成比较表达式。
        foreach (var field in tupleType.TupleElements)
        {
            // 创建访问元组元素的表达式，如 left.Item1 或 left.Name。
            // field.Name 会正确解析为 "Item1" 或自定义名称如 "Name"。
            var leftMember = new MemberExpression(left, new Identifier(field.Name), false, false);
            var rightMember = new MemberExpression(right, new Identifier(field.Name), false, false);

            // 为当前元素创建严格相等/不相等比较。
            // 使用严格相等 (===) 更贴近C#的强类型比较语义。
            var currentComparison = new NonLogicalBinaryExpression(
                isEq ? Operator.StrictEquality : Operator.StrictInequality,
                leftMember,
                rightMember);

            // 将当前比较与之前的结果用逻辑运算符组合。
            // 相等要求所有元素都相等 (&&)，不等要求至少一个元素不等 (||)。
            if (result is null)
            {
                result = currentComparison;
            }
            else
            {
                result = new LogicalExpression(
                    isEq ? Operator.LogicalAnd : Operator.LogicalOr,
                    result,
                    currentComparison);
            }
        }

        if (result is null)
            return HandleTransformationFailure(operation);

        return new ParenthesizedExpression(result);
    }

    /// <summary>
    /// 处理丢弃操作（下划线变量）
    /// C# 示例：
    /// _ = SomeMethod();        // 丢弃返回值
    /// (_, var y) = GetTuple(); // 丢弃元组的第一个元素
    /// 转换结果：undefined
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDiscardOperation(IDiscardOperation operation, Queue<VariableDeclaration> argument)
    {
        // 在解构赋值模式中 (例如: (_, y) = tuple)
        if (operation.Parent is IDeconstructionAssignmentOperation)
        {
            // 在JavaScript的解构模式中，丢弃元素用空槽位表示。
            // 例如: const [, y] = tuple;
            // 但是，AST节点本身不能是“空的”。这里需要特殊处理。
            // 一个常见的策略是返回一个特殊的标记节点，然后在 VisitDeconstructionAssignment 中处理它。
            // 或者，更简单的方法是，让 VisitDeconstructionAssignment 自己处理丢弃操作，
            // 而不是让 VisitDiscardOperation 返回一个有意义的节点。
            // 如果必须返回一个节点，可以返回 null，并在父节点中处理。
            // 最简单直接的方式是返回一个代表“无”的节点，让父节点忽略它。
            // 这里我们返回一个特殊的Identifier，父节点需要识别它。
            // 但更好的做法是让父节点直接检查子操作是否为 IDiscardOperation。
            return null; // 让父节点处理
        }

        // 在简单赋值操作的左侧 (例如: _ = value)
        if (operation.Parent is ISimpleAssignmentOperation assignment && assignment.Target == operation)
        {
            // C# 的 _ = value 意味着“执行 value 但不关心结果”。
            // 这等价于直接执行 value 这个表达式语句。
            // 所以，我们应该返回 value 本身，而不是 undefined。
            // 父节点 VisitSimpleAssignmentOperation 应该检测到目标是丢弃操作，
            // 然后只返回对 operation.Value 的访问结果。
            return null; // 让父节点处理
        }

        // 在其他表达式中作为值使用 (非常罕见，但可能)
        // 例如: var isNull = _ is null; // 这在C#中是编译时错误
        // 或者作为方法参数: SomeMethod(_); // 这也是编译时错误
        // 如果真的遇到了，可以返回 undefined。
        return new Identifier("undefined");
    }

    /// <summary>
    /// 处理丢弃模式操作
    /// C# 示例：
    /// value switch {
    ///     _ => "Default",              // 丢弃模式，总是匹配
    ///     var _ => "Any value",        // 丢弃模式变量声明
    /// }
    /// obj is _                         // 丢弃模式类型检查
    /// 转换结果：转换为JavaScript的true条件表达式
    /// 丢弃模式表示"总是匹配"，在条件判断中等价于true
    /// </summary>
    /// <param name="operation">丢弃模式操作</param>
    /// <param name="argument">当前operation所属的父operation</param>
    /// <returns>JavaScript布尔字面量true</returns>
    public override Acornima.Ast.Node? VisitDiscardPattern(IDiscardPatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 丢弃模式的条件判断转换
        // C# 示例：_ 表示"总是匹配"的模式
        // 转换结果：生成JavaScript的true字面量
        // 因为丢弃模式在任何情况下都应该匹配成功

        // 丢弃模式总是匹配，返回true字面量
        return new BooleanLiteral(true, "true");
    }

    /// <summary>
    /// 处理 null 检查操作
    /// C# 示例：
    /// obj is null             // 检查是否为 null
    /// value == null           // 直接 null 比较
    /// 转换结果：obj === null
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitIsNull(IIsNullOperation operation, Queue<VariableDeclaration> argument)
    {
        // null检查转换为 === null 比较
        if (Visit(operation.Operand, argument) is not Expression operand)
            return HandleTransformationFailure(operation.Operand);

        return new LogicalExpression(Operator.StrictEquality, operand, new NullLiteral("null"));
    }

    /// <summary>
    /// 处理空合并赋值操作
    /// C# 示例：
    /// name ??= "Default";     // 如果 name 为 null，则赋值 "Default"
    /// value ??= 0;            // 如果 value 为 null，则赋值 0
    /// 转换结果：name ??= "Default" / value ??= 0
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitCoalesceAssignment(ICoalesceAssignmentOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Target, argument) is not Expression left)
            return HandleTransformationFailure(operation.Target);

        if (Visit(operation.Value, argument) is not Expression right)
            return HandleTransformationFailure(operation.Value);

        return new AssignmentExpression(Operator.NullishCoalescing, left, right);
    }

    /// <summary>
    /// 处理递归模式操作
    /// C# 示例：
    /// obj is Person { Name: "John", Age: > 18 }        // 类型模式 + 属性模式组合
    /// value is { Length: > 0, Count: var c }          // 属性模式组合
    /// data is MyClass { Prop1: 1, Prop2: { X: 2 } }   // 嵌套递归模式
    /// item is (int x, string s) when x > 0            // 元组模式 + when子句
    /// 转换结果：转换为JavaScript的组合条件表达式
    /// 递归模式是多个模式的组合，生成 &amp;&amp;连接的条件判断
    /// </summary>
    /// <param name="operation">递归模式操作</param>
    /// <param name="argument">当前operation所属的父operation</param>
    /// <returns>JavaScript组合条件表达式</returns>
    public override Acornima.Ast.Node? VisitRecursivePattern(IRecursivePatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 递归模式的条件判断转换
        // C# 示例：obj is Person { Name: "John", Age: > 18 }
        // 转换结果：生成 (obj instanceof Person) && (obj.Name === "John") && (obj.Age > 18)
        // 递归模式由多个子模式组成，需要用 && 连接所有条件

        var conditions = new List<Expression>();

        // 1. 处理类型模式（如果存在）
        if (operation.MatchedType is not null)
        {
            // 从父operation获取目标名称，在节点内构建表达式
            var targetName = ExtractPatternValName(operation.Parent);
            if (string.IsNullOrEmpty(targetName))
                return HandleTransformationFailure(operation, "cannot extract reference name.");

            // 根据获取的名称构建目标表达式
            var target = new Identifier(targetName);
            var typeName = operation.MatchedType.Name;
            var condition = typeName.ToLowerInvariant() switch
            {
                "string" => new LogicalExpression(
                        Operator.StrictEquality,
                        new UpdateExpression(Operator.TypeOf, target, prefix: true),
                        new StringLiteral("string", "\"string\"")
                    ),
                "number" or "int32" or "int64" or "double" or "float" or "decimal" =>
                             new LogicalExpression(
                        Operator.StrictEquality,
                        new UpdateExpression(Operator.TypeOf, target, prefix: true),
                        new StringLiteral("number", "\"number\"")
                    ),
                "boolean" => new LogicalExpression(
                        Operator.StrictEquality,
                        new UpdateExpression(Operator.TypeOf, target, prefix: true),
                        new StringLiteral("boolean", "\"boolean\"")
                    ),
                "object" => new LogicalExpression(
                            Operator.LogicalAnd,
                            new LogicalExpression(Operator.StrictInequality, target, new NullLiteral("null")),
                            new LogicalExpression(
                                Operator.StrictEquality,
                                new UpdateExpression(Operator.TypeOf, target, prefix: true),
                                new StringLiteral("object", "\"object\"")
                            )
                        ),// 对于对象类型，检查是否不为null且为object
                _ => new LogicalExpression(Operator.InstanceOf, target, new Identifier(typeName)),// 对于自定义类型，使用instanceof检查
            };
            conditions.Add(condition);
        }

        // 2. 处理属性子模式（如果存在）
        if (operation.PropertySubpatterns.Length > 0)
        {
            foreach (var propertySubpattern in operation.PropertySubpatterns)
            {
                // 根据AST转换器方法复用原则，argument是父节点，这里传递当前的递归模式操作
                if (Visit(propertySubpattern, argument) is Expression propertyCondition)
                    conditions.Add(propertyCondition);
                else
                    return HandleTransformationFailure(propertySubpattern);
            }
        }

        // 3. 处理声明模式（变量声明）
        if (operation.DeclaredSymbol is not null)
        {
            // 声明模式不影响条件判断，只是绑定变量
            // 在模式匹配中，我们只关心条件判断部分
        }

        // 4. 组合所有条件
        if (conditions.Count == 0)
        {
            // 如果没有条件，返回true（空模式总是匹配）
            return new BooleanLiteral(true, "true");
        }
        else if (conditions.Count == 1)
        {
            // 只有一个条件，直接返回
            return conditions[0];
        }
        else
        {
            // 多个条件，用 && 连接
            Expression result = conditions[0];
            for (int i = 1; i < conditions.Count; i++)
            {
                result = new LogicalExpression(Operator.LogicalAnd, result, conditions[i]);
            }
            return result;
        }
    }

    /// <summary>
    /// 处理 switch 表达式操作
    /// C# 示例：
    /// var result = value switch {
    ///     1 => "One",              // 常量模式
    ///     string s => $"String: {s}", // 类型模式
    ///     { Length: > 5 } => "Long",   // 属性模式
    ///     var x when x > 0 => "Positive", // when 子句
    ///     _ => "Other"             // 默认模式
    /// };
    /// 转换结果：根据模式复杂度转换为嵌套条件表达式或函数调用
    /// <summary>
    /// 将C# switch表达式转换为JavaScript switch语句或IIFE
    /// 非模式匹配switch转换为switch语句，模式匹配switch转换为IIFE
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitSwitchExpression(ISwitchExpressionOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Value, argument) is not Expression input || operation.Arms.Length == 0)
            return HandleTransformationFailure(operation.Value);

        // 检查是否为非模式匹配switch（传统switch）
        bool isTraditionalSwitch = true;
        foreach (var arm in operation.Arms)
        {
            // 检查是否为常量模式或丢弃模式
            if (arm.Pattern.Kind != OperationKind.ConstantPattern &&
                arm.Pattern.Kind != OperationKind.DiscardPattern)
            {
                isTraditionalSwitch = false;
                break;
            }

            // 检查是否有when子句
            if (arm.Guard is not null)
            {
                isTraditionalSwitch = false;
                break;
            }
        }

        if (isTraditionalSwitch)
        {
            // 非模式匹配switch，转换为JavaScript switch语句
            var cases = new List<SwitchCase>();

            foreach (var arm in operation.Arms)
            {
                if (VisitSwitchExpressionArm(arm, argument) is SwitchCase switchCase)
                {
                    cases.Add(switchCase);
                }
            }

            // 确保有默认情况
            bool hasDefault = false;
            foreach (var c in cases)
            {
                if (c.Test == null)
                {
                    hasDefault = true;
                    break;
                }
            }

            if (!hasDefault)
            {
                cases.Add(new SwitchCase(
                    null,
                    NodeList.From<Statement>(new ReturnStatement(new Identifier("undefined")))
                ));
            }

            var switchStatement = new SwitchStatement(input, NodeList.From<SwitchCase>(cases));
            var functionBody = new FunctionBody(NodeList.From<Statement>(switchStatement), strict: true);
            var arrowFunction = new ArrowFunctionExpression(
                NodeList.From<Node>(),
                functionBody,
                expression: false,
                async: false
            );

            // 立即调用箭头函数 (() => { ... })()
            return new CallExpression(arrowFunction, NodeList.From<Expression>(), optional: false);
        }
        else
        {
            // 复杂模式匹配switch，生成健全的IIFE保证副作用顺序
            // 采用分层判断：先模式匹配，后when条件，确保求值节拍与C#一致
            var statements = new List<Statement>();
            var discardArm = (ISwitchExpressionArmOperation?)null;

            // 创建临时变量存储输入值，确保仅求值一次
            var inputVar = new Identifier(GetUniqueName(operation));
            statements.Add(new VariableDeclaration(
                VariableDeclarationKind.Const,
                NodeList.From(
                    new VariableDeclarator(inputVar, input)
                )
            ));

            // 处理所有非丢弃模式，采用嵌套if确保副作用顺序
            foreach (var arm in operation.Arms)
            {
                if (arm.Pattern.Kind == OperationKind.DiscardPattern)
                {
                    discardArm = arm; // 保存丢弃模式，最后处理
                    continue;
                }

                if (Visit(arm.Value, argument) is not Expression value)
                    return HandleTransformationFailure(arm.Value);

                // 生成模式匹配条件
                Expression? patternCondition = null;

                if (arm.Pattern.Kind == OperationKind.ConstantPattern)
                {
                    if (Visit(arm.Pattern, argument) is not Expression constantValue)
                        return HandleTransformationFailure(arm.Pattern);

                    patternCondition = new LogicalExpression(Operator.StrictEquality, inputVar, constantValue);
                }
                else if (arm.Pattern.Kind == OperationKind.TypePattern)
                {
                    // 对于类型模式，使用typeof或instanceof检查
                    if (arm.Pattern is ITypePatternOperation typeOp && typeOp.InputType is not null)
                    {
                        var typeName = typeOp.InputType.Name.ToLowerInvariant();
                        patternCondition = typeName switch
                        {
                            "string" => new LogicalExpression(Operator.StrictEquality,
                                new UpdateExpression(Operator.TypeOf, inputVar, prefix: true),
                                new StringLiteral("string", "\"string\"")),
                            "number" or "int32" or "int64" or "double" or "float" or "decimal" =>
                                new LogicalExpression(Operator.StrictEquality,
                                    new UpdateExpression(Operator.TypeOf, inputVar, prefix: true),
                                    new StringLiteral("number", "\"number\"")),
                            "boolean" => new LogicalExpression(Operator.StrictEquality,
                                new UpdateExpression(Operator.TypeOf, inputVar, prefix: true),
                                new StringLiteral("boolean", "\"boolean\"")),
                            _ => new LogicalExpression(Operator.InstanceOf, inputVar, new Identifier(typeName))
                        };
                    }
                }
                else
                {
                    // 其他模式类型，尝试访问并处理占位符替换
                    if (Visit(arm.Pattern, argument) is not Expression complexPattern)
                        return HandleTransformationFailure(arm.Pattern);

                    // 这里需要实现占位符替换逻辑
                    // 暂时使用原始表达式，但需要注意占位符问题
                    patternCondition = complexPattern;

                }

                if (patternCondition is not null)
                {
                    Statement branchStatement;

                    if (arm.Guard is not null)
                    {
                        // 有when子句：关键的分层判断保证副作用顺序
                        // 先模式匹配，成功后才执行when条件
                        if (Visit(arm.Guard, argument) is not Expression guardCondition)
                            return HandleTransformationFailure(arm.Guard);


                        // 嵌套if结构：
                        // if (pattern) {
                        //   if (when) return value;
                        // }
                        var innerIf = new IfStatement(guardCondition, new ReturnStatement(value), null);
                        branchStatement = new IfStatement(patternCondition, innerIf, null);
                    }
                    else
                    {
                        // 无when子句：直接判断模式
                        branchStatement = new IfStatement(patternCondition, new ReturnStatement(value), null);
                    }

                    statements.Add(branchStatement);
                }
            }

            // 最后处理丢弃模式（默认情况）
            if (discardArm is not null)
            {
                if (Visit(discardArm.Value, argument) is not Expression discardValue)
                    return HandleTransformationFailure(discardArm.Value);

                statements.Add(new ReturnStatement(discardValue));
            }

            // 确保有返回值
            if (statements.Count == 0 || statements[^1] is not ReturnStatement)
            {
                statements.Add(new ReturnStatement(new Identifier("undefined")));
            }

            var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
            var arrowFunction = new ArrowFunctionExpression(
                NodeList.From<Node>(),
                functionBody,
                expression: false,
                async: false
            );

            // 立即调用箭头函数
            return new CallExpression(arrowFunction, NodeList.From<Expression>(), optional: false);
        }
    }

    /// <summary>
    /// 处理 switch 表达式分支操作
    /// 根据上下文返回SwitchCase（传统switch）或Statement（模式匹配）
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitSwitchExpressionArm(ISwitchExpressionArmOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Pattern, argument) is not Expression pattern)
            return HandleTransformationFailure(operation.Pattern);

        Expression? guard = null;
        if (operation.Guard is not null)
        {
            if (Visit(operation.Guard, argument) is not Expression expr)
                return HandleTransformationFailure(operation.Guard);
        }

        if (Visit(operation.Value, argument) is not Expression value)
            return HandleTransformationFailure(operation.Value); ;

        // 检查是否为传统的常量模式（无when子句）
        bool isTraditionalPattern = (operation.Pattern.Kind == OperationKind.ConstantPattern ||
                                   operation.Pattern.Kind == OperationKind.DiscardPattern) &&
                                   operation.Guard == null;

        if (isTraditionalPattern)
        {
            // 生成SwitchCase用于传统switch语句
            Expression? test = null;

            if (operation.Pattern.Kind == OperationKind.ConstantPattern)
                test = pattern;

            // DiscardPattern的test为null（默认情况）
            // 修复P0：SwitchCase中不能直接使用ReturnStatement，应该使用break
            // 对于switch表达式转换为switch语句的场景，需要在外层包装函数来处理返回值
            var breakStatement = new BreakStatement(null);

            return new SwitchCase(
                test,
                NodeList.From<Statement>(new NonSpecialExpressionStatement(value), breakStatement)
            );
        }
        else
        {
            // 生成Statement用于模式匹配IIFE
            if (operation.Pattern.Kind == OperationKind.DiscardPattern)
            {
                // 默认情况，直接返回
                return new ReturnStatement(value);
            }
            else if (pattern is not null)
            {
                Expression condition;

                if (operation.Pattern.Kind == OperationKind.ConstantPattern)
                {
                    // 从父operation获取switch目标名称并构建表达式
                    var targetName = ExtractPatternValName(operation.Parent);
                    if (string.IsNullOrEmpty(targetName))
                        return HandleTransformationFailure(operation, "cannot extract reference name.");

                    var target = new Identifier(targetName);
                    condition = new LogicalExpression(Operator.StrictEquality, target, pattern);
                }
                else
                {
                    // 复杂模式，直接使用模式表达式（已经包含实际目标）
                    condition = pattern;
                }

                // 处理when子句
                if (guard is not null)
                {
                    condition = new LogicalExpression(Operator.LogicalAnd, condition, guard);
                }

                return new IfStatement(condition, new ReturnStatement(value), null);
            }
        }

        return HandleTransformationFailure(operation);
    }

    /// <summary>
    /// 处理属性子模式操作
    /// C# 示例：
    /// obj is { Name: "John" }             // 属性模式中的 Name: "John" 部分
    /// person is { Age: > 18 }             // 属性条件模式
    /// item is { Length: var len }         // 属性声明模式
    /// data is { Count: not 0 }            // 属性取反模式
    /// 转换结果：转换为JavaScript的属性访问和比较表达式
    /// Name: "John" 转换为 obj.Name === "John"
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitPropertySubpattern(IPropertySubpatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 属性子模式的条件判断转换
        // C# 示例：obj is { Name: "John" } 中的 Name: "John" 部分
        //         转换为 obj.Name === "John" 的JavaScript表达式
        // 转换结果：生成属性访问和比较的组合表达式

        // 从父operation获取目标名称，在节点内构建表达式
        var targetName = ExtractPatternValName(operation.Parent);
        if (string.IsNullOrEmpty(targetName))
            return HandleTransformationFailure(operation, "cannot extract reference name.");

        var targetExpression = new Identifier(targetName);

        // 从Member中获取名称
        var propertyName = operation.Member switch
        {
            IFieldReferenceOperation fieldRef => fieldRef.Field.Name,
            IPropertyReferenceOperation propRef => propRef.Property.Name,
            _ => null
        };

        // 访问属性模式并转换为表达式
        if (string.IsNullOrEmpty(propertyName) || Visit(operation.Pattern, argument) is not Expression patternExpression)
            return HandleTransformationFailure(operation, "Unsupported member type in property subpattern.");

        // 根据AST节点构造规范，生成属性访问表达式
        // 使用实际的目标表达式而不是占位符
        var propertyAccess = new MemberExpression(
            targetExpression,
            new Identifier(propertyName),
            computed: false,
            optional: false
        );

        // 根据模式类型生成不同的比较表达式
        // 对于常量模式，生成 === 比较
        // 对于其他模式，直接返回模式表达式（如关系比较等）
        return new LogicalExpression(Operator.StrictEquality, propertyAccess, patternExpression);
    }

    /// <summary>
    /// 处理取反模式操作
    /// C# 示例：
    /// obj is not null         // not 模式
    /// value is not 0          // 取反常量模式
    /// item is not { IsValid: false } // 取反属性模式
    /// 转换结果：转换为JavaScript的逻辑非操作符（!）
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitNegatedPattern(INegatedPatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 取反模式的条件判断转换
        // C# 示例：obj is not null 是一个布尔条件表达式
        //         value is not 0 检查值是否不等于0
        // 转换结果：生成JavaScript的逻辑非表达式

        // 访问内部模式并转换为表达式
        if (Visit(operation.Pattern, argument) is not Expression innerExpression)
            return HandleTransformationFailure(operation.Pattern);

        // 使用UpdateExpression处理逻辑非操作
        // 生成 !(innerExpression) 的JavaScript表达式
        return new UpdateExpression(Operator.LogicalNot, innerExpression, prefix: true);
    }

    /// <summary>
    /// 处理二元模式操作
    /// C# 示例：
    /// value is > 0 and < 100              // and 模式（组合条件）
    /// obj is string or int                // or 模式（类型选择）
    /// item is not null and [1, 2, 3]     // 复杂and模式
    /// data is { Length: > 5 } or null    // 属性模式与or组合
    /// 转换结果：转换为JavaScript的逻辑表达式（&amp;&amp; 或 ||）
    /// and 模式转换为 (left) &amp;&amp; (right)，or 模式转换为 (left) || (right)
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitBinaryPattern(IBinaryPatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 二元模式的条件判断转换
        // C# 示例：value is > 0 and < 100 是一个布尔条件表达式
        //         obj is string or int 检查对象是否为指定类型中的任意一种
        // 转换结果：生成相应的JavaScript逻辑表达式

        // 访问左右两个子模式
        if (Visit(operation.LeftPattern, argument) is not Expression left)
            return HandleTransformationFailure(operation.LeftPattern);

        if (Visit(operation.RightPattern, argument) is not Expression right)
            return HandleTransformationFailure(operation.RightPattern);

        // 检查模式的类型来确定操作符
        var @operator = operation.OperatorKind switch
        {
            BinaryOperatorKind.And => Operator.LogicalAnd,
            BinaryOperatorKind.Or => Operator.LogicalOr,
            _ => Operator.Unknown
        };

        if (@operator == Operator.Unknown)
            return HandleTransformationFailure(operation);

        return new LogicalExpression(@operator, left, right);
    }

    /// <summary>
    /// 处理类型模式操作
    /// C# 示例：
    /// obj is string           // 类型模式
    /// value is int            // 值类型模式
    /// item is MyClass         // 自定义类型模式
    /// 转换结果：根据类型生成相应的JavaScript类型检查条件
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitTypePattern(ITypePatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 类型模式的条件判断转换
        // C# 示例：obj is string 是一个布尔条件表达式
        //         value is MyClass 检查对象是否为指定类型
        // 转换结果：根据类型生成对应的JavaScript检查条件

        var inputType = operation.InputType;
        // 从父operation获取目标名称，在节点内构建表达式
        var targetName = ExtractPatternValName(operation.Parent);
        if (string.IsNullOrEmpty(targetName))
            return HandleTransformationFailure(operation, "cannot extract reference name.");

        // 根据获取的名称构建目标表达式
        var targetExpression = new Identifier(targetName);

        // 根据编译时优化原则和强弱类型转换优化原则
        // 利用C#的编译时类型信息，生成最简洁的JavaScript类型检查

        var typeName = inputType.Name;

        // 对于基本类型，使用typeof检查
        return typeName.ToLowerInvariant() switch
        {
            "string" => new LogicalExpression(
                                Operator.StrictEquality,
                                new UpdateExpression(Operator.TypeOf, targetExpression, prefix: true),
                                new StringLiteral("string", "\"string\"")
                            ),
            "number" or "int32" or "int64" or "double" or "float" or "decimal" => new LogicalExpression(
                                Operator.StrictEquality,
                                new UpdateExpression(Operator.TypeOf, targetExpression, prefix: true),
                                new StringLiteral("number", "\"number\"")
                            ),
            "boolean" => new LogicalExpression(
                                Operator.StrictEquality,
                                new UpdateExpression(Operator.TypeOf, targetExpression, prefix: true),
                                new StringLiteral("boolean", "\"boolean\"")
                            ),
            "object" => new LogicalExpression(
                                Operator.LogicalAnd,
                                new LogicalExpression(Operator.StrictInequality, targetExpression, new NullLiteral("null")),
                                new LogicalExpression(
                                    Operator.StrictEquality,
                                    new UpdateExpression(Operator.TypeOf, targetExpression, prefix: true),
                                    new StringLiteral("object", "\"object\"")
                                )
                            ),// 对于对象类型，检查是否不为null且为object
            _ => new LogicalExpression(
                                Operator.InstanceOf,
                                targetExpression,
                                new Identifier(typeName)
                            ),// 对于自定义类型，使用instanceof检查
        };
    }

    /// <summary>
    /// 处理关系模式操作
    /// C# 示例：
    /// value is > 0            // 大于模式
    /// age is >= 18 and <= 65  // 组合关系模式
    /// score is < 60           // 小于模式
    /// 转换结果：转换为JavaScript的关系比较表达式
    /// value > 0, age >= 18, score < 60
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitRelationalPattern(IRelationalPatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 关系模式的条件判断转换
        // C# 示例：value is > 0 是一个布尔条件表达式
        //         age is >= 18 检查年龄是否满足条件
        // 转换结果：生成相应的JavaScript关系比较表达式

        // 获取参考操作
        IOperation? refOperation = operation.Parent switch
        {
            IIsPatternOperation isPattern => isPattern.Value,
            ISwitchExpressionOperation switchExpr => switchExpr.Value,
            INegatedPatternOperation negatedPattern => negatedPattern.Parent switch
            {
                IIsPatternOperation nestedIsPattern => nestedIsPattern.Value,
                ISwitchExpressionOperation nestedSwitchExpr => nestedSwitchExpr.Value,
                _ => null
            },
            _ => null
        };

        // 从参考操作中提取名称
        var targetName = refOperation switch
        {
            ILocalReferenceOperation localRef => localRef.Local.Name,
            IParameterReferenceOperation paramRef => paramRef.Parameter.Name,
            IFieldReferenceOperation fieldRef when fieldRef.Instance is null => fieldRef.Field.Name,
            IPropertyReferenceOperation propRef when propRef.Instance is null => propRef.Property.Name,
            IInstanceReferenceOperation => "this",
            _ => "value"
        };

        // 根据获取的名称构建目标表达式
        var targetExpression = new Identifier(targetName);

        // 获取右操作数（比较值）
        if (Visit(operation.Value, argument) is not Expression value)
            return HandleTransformationFailure(operation.Value);

        // 检查是否在取反模式中（使用简化的检查逻辑）
        bool isInNegatedPattern = operation.OperatorKind == BinaryOperatorKind.NotEquals ||
                                  operation.Parent is INegatedPatternOperation ||
                                  (operation.Parent is IIsPatternOperation isPatternOp &&
                                   isPatternOp.Pattern is INegatedPatternOperation) ||
                                  (operation.Parent is IIsPatternOperation parentPattern &&
                                   parentPattern.Parent is INegatedPatternOperation) ||
                                  (operation.OperatorKind == BinaryOperatorKind.Equals &&
                                   operation.Parent is IIsPatternOperation patternOp &&
                                   patternOp.Parent is INegatedPatternOperation);

        // 根据编译时优化原则，直接生成最简洁的JavaScript关系比较表达式
        // 将C#的关系操作符映射到JavaScript的操作符
        // 如果在取反模式中，需要反转操作符（如 Equals 变为 StrictInequality）
        var @operator = (operation.OperatorKind, isInNegatedPattern) switch
        {
            (BinaryOperatorKind.GreaterThan, false) => Operator.GreaterThan,
            (BinaryOperatorKind.GreaterThan, true) => Operator.LessThanOrEqual,
            (BinaryOperatorKind.GreaterThanOrEqual, false) => Operator.GreaterThanOrEqual,
            (BinaryOperatorKind.GreaterThanOrEqual, true) => Operator.LessThan,
            (BinaryOperatorKind.LessThan, false) => Operator.LessThan,
            (BinaryOperatorKind.LessThan, true) => Operator.GreaterThanOrEqual,
            (BinaryOperatorKind.LessThanOrEqual, false) => Operator.LessThanOrEqual,
            (BinaryOperatorKind.LessThanOrEqual, true) => Operator.GreaterThan,
            (BinaryOperatorKind.Equals, false) => Operator.StrictEquality,
            (BinaryOperatorKind.Equals, true) => Operator.StrictInequality,
            (BinaryOperatorKind.NotEquals, false) => Operator.StrictInequality,
            (BinaryOperatorKind.NotEquals, true) => Operator.StrictEquality,
            _ => Operator.Unknown
        };

        if (@operator == Operator.Unknown)
            return HandleTransformationFailure(operation);

        // 根据AST节点构造规范，使用LogicalExpression表示比较操作
        // 使用实际的目标表达式而不是占位符
        return new LogicalExpression(@operator, targetExpression, value);
    }

    /// <summary>
    /// 处理 with 表达式操作（记录类型的复制修改）
    /// C# 示例：
    /// var newPerson = person with { Name = "John" }; // 记录类型复制修改
    /// var updated = point with { X = 10 };          // 部分属性更新
    /// 转换结果：转换为JavaScript的对象展开语法
    /// { ...person, Name: "John" } / { ...point, X: 10 }
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitWith(IWithOperation operation, Queue<VariableDeclaration> argument)
    {
        // with表达式的直接AST转换
        // C# 示例：person with { Name = "John" } 表示创建一个新对象，复制原对象并修改指定属性
        //         point with { X = 10 } 表示复制point对象并更新X属性
        // 转换结果：生成JavaScript的对象展开语法 { ...original, property: newValue }
        // 语义等价：C# record的with表达式在运行时语义上等同于JavaScript的对象展开

        // 获取原始对象
        if (Visit(operation.Operand, argument) is not Expression operand)
            return HandleTransformationFailure(operation.Operand);

        // 处理初始化器（要修改的属性）
        var properties = new List<Node>
        {
            // 添加展开元素（复制原始对象的所有属性）
            new SpreadElement(operand)
        };

        // 处理初始化器中的新属性
        // 获取初始化器中的所有初始化操作
        foreach (var initializer in operation.Initializer.Initializers)
        {
            if (initializer is IMemberInitializerOperation memberInit)
            {
                // 获取成员名称
                string memberName;
                if (memberInit.InitializedMember is IFieldSymbol f)
                    memberName = f.Name;
                else if (memberInit.InitializedMember is IPropertySymbol p)
                    memberName = p.Name;
                else
                    return HandleTransformationFailure(operation.Initializer);

                // 获取初始化值
                if (Visit(memberInit.Initializer, argument) is Expression initValue)
                {
                    // 根据AST节点构造规范，使用PropertyDefinition创建对象属性
                    // 确保生成正确的属性语法：{ ...original, propertyName: value }
                    properties.Add(new ObjectProperty(
                        kind: PropertyKind.Init,
                        key: new Identifier(memberName),
                        value: initValue,
                        computed: false,
                        shorthand: false,
                        method: false
                    ));
                }
                else
                    return HandleTransformationFailure(operation.Initializer);
            }
            else
            {
                // 对于其他类型的初始化器，需要确保生成正确的属性语法
                var initNode = Visit(initializer, argument);

                // 如果初始化器不是PropertyDefinition，需要包装成PropertyDefinition
                // 这样可以确保生成正确的JavaScript对象属性语法
                if (initNode is PropertyDefinition)
                    properties.Add(initNode);
                else if (initNode is AssignmentExpression assignment)
                {
                    // 如果是赋值表达式，提取左侧作为属性名，右侧作为值
                    var key = assignment.Left switch
                    {
                        Identifier i => i,
                        MemberExpression m => m.Property as Identifier,
                        _ => null
                    };

                    if (key is not null)
                    {
                        properties.Add(new ObjectProperty(
                            kind: PropertyKind.Init,
                            key: key,
                            value: assignment.Right,
                            computed: false,
                            shorthand: false,
                            method: false
                        ));
                    }
                }
                else
                    return HandleTransformationFailure(operation);

            }
        }


        // 根据编译时优化原则，直接生成最简洁的JavaScript对象字面量
        // 返回对象表达式：{ ...original, property: value }
        return new ObjectExpression(NodeList.From<Node>(properties));
    }

    /// <summary>
    /// 处理插值字符串添加操作
    /// C# 示例：
    /// $"Hello {name}!" 中的字符串连接操作
    /// 转换结果： `Hello ${name}!`
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInterpolatedStringAddition(IInterpolatedStringAdditionOperation operation, Queue<VariableDeclaration> argument)
    {
        // 递归收集所有静态字符串和动态表达式
        var quasis = new List<TemplateElement>();
        var exprs = new List<Expression>();

        void Collect(IOperation? node)
        {
            switch (node)
            {
                case IInterpolatedStringAdditionOperation add:
                    // 递归处理左子树和右子树，以压平编译器生成的二叉树结构
                    Collect(add.Left);
                    Collect(add.Right);
                    break;

                case ILiteralOperation { ConstantValue: { HasValue: true, Value: string cookedValue } }:
                    // s 是 C# 解释后的 "cooked" 值
                    var rawValue = CookedToRaw(cookedValue);
                    var templateValue = TemplateValue.From(cookedValue, rawValue);
                    quasis.Add(new TemplateElement(templateValue, tail: false));
                    break;

                default:
                    if (node is not null)
                    {
                        // 任何非字面量操作（如变量、方法调用等）都被视为动态表达式
                        if (Visit(node, argument) is Expression e)
                            exprs.Add(e);
                        else
                            HandleTransformationFailure(node);
                    }
                    break;
            }
        }

        Collect(operation);

        // 最后一个 quasi 的 tail 标志必须是 true
        if (quasis.Count == exprs.Count)
            quasis.Add(new TemplateElement(TemplateValue.From("", ""), tail: true));
        else
            quasis[^1] = new TemplateElement(quasis[^1].Value, tail: true);

        // 生成最终的 TemplateLiteral AST 节点
        return new TemplateLiteral(NodeList.From(quasis), NodeList.From(exprs));

        string CookedToRaw(string cooked)
        {
            var sb = new StringBuilder(cooked.Length);
            foreach (var c in cooked)
            {
                switch (c)
                {
                    case '`': sb.Append("\\`"); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '$': sb.Append("\\$"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// 处理列表模式操作
    /// C# 示例：
    ///   list is [1, 2, ..]            // 长度 ≥2 即可
    ///   list is [var a, .. var rest]  // 长度 ≥1，rest 运行时就是 slice(1)
    /// 转换结果：
    ///   Array.isArray(list) &&
    ///   list.length >= 2 &&
    ///   list[0] === 1
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitListPattern(IListPatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 从父operation获取目标名称，在节点内构建表达式
        var targetName = ExtractPatternValName(operation.Parent);
        if (string.IsNullOrEmpty(targetName))
            return HandleTransformationFailure(operation, "cannot extract reference name.");

        var patterns = operation.Patterns;
        if (patterns == null || patterns.IsEmpty) return null;

        Expression targetExpr = new Identifier(targetName);

        /* 1. Array.isArray(target) */
        // 修正 CallExpression 的构造：callee 和 arguments 分开
        var arrayCheck = new CallExpression(
            callee: new MemberExpression(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false),
            args: NodeList.From(targetExpr),
            optional: false
        );

        /* 2. 统计“固定头”长度，并检测有没有 .. */
        int fixedLen = 0;
        bool hasSlice = false;
        foreach (var p in patterns)
        {
            if (p is ISlicePatternOperation)
            {
                hasSlice = true;
                break;
            }
            fixedLen++;
        }

        /* 3. 长度检查：有切片就用 >=，没有就用 === */
        var lengthCheck = new NonLogicalBinaryExpression(
            hasSlice ? Operator.GreaterThanOrEqual : Operator.StrictEquality,
            new MemberExpression(targetExpr, new Identifier("length"), computed: false, optional: false),
            new NumericLiteral(fixedLen, fixedLen.ToString())
        );

        /* 4. 固定头元素检查 */
        Expression? elemChecks = null;
        for (int i = 0; i < fixedLen; i++)
        {
            var pattern = patterns[i];
            // 创建对 target[i] 的访问表达式
            var indexAccess = new MemberExpression(targetExpr,
                new NumericLiteral(i, i.ToString()), computed: true, optional: false);

            Expression? subCondition = null;

            // 【核心修正】直接在此处处理子模式，而不是递归调用 Visit
            // 因为我们不能改变 argument，所以子模式必须由父模式“解释”和“生成”
            switch (pattern)
            {
                case IConstantPatternOperation cpo:
                    // 递归访问常量表达式，这是安全的，因为它不依赖于 argument
                    var valueExpr = (Expression)Visit(cpo.Value, argument)!;
                    subCondition = new NonLogicalBinaryExpression(Operator.StrictEquality, indexAccess, valueExpr);
                    break;

                case IDeclarationPatternOperation:
                    // 声明模式总是匹配，不增加布尔条件
                    subCondition = null;
                    break;

                case IDiscardPatternOperation:
                    // 弃元模式忽略，不增加条件
                    subCondition = null;
                    break;

                    // 可以根据需要添加其他模式类型的处理
                    // default:
                    //     return HandleTransformationFailure(pattern, "Unsupported pattern type in list.");
            }

            if (subCondition is not null)
            {
                elemChecks = elemChecks is null
                    ? subCondition
                    : new LogicalExpression(Operator.LogicalAnd, elemChecks, subCondition);
            }
        }

        /* 5. 拼总表达式 */
        Expression result = new LogicalExpression(Operator.LogicalAnd, arrayCheck, lengthCheck);
        if (elemChecks != null)
            result = new LogicalExpression(Operator.LogicalAnd, result, elemChecks);

        return result;
    }

    /// <summary>
    /// 处理切片模式操作
    /// C# 示例：
    /// array is [1, .., 5]                 // 切片模式匹配（条件表达式）
    /// list is [var first, .. var middle, var last] // 切片解构条件
    /// 转换结果：生成 Array.isArray 条件检查，返回布尔值
    /// 符合模式匹配语义：判断是否匹配，而不是提取数据
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitSlicePattern(ISlicePatternOperation operation, Queue<VariableDeclaration> argument)
    {
        // 切片模式的条件判断转换
        // C# 示例：array is [.., var lastPart] 是一个布尔条件表达式
        // 转换结果：Array.isArray(array) && array.length >= minLength

        var pattern = operation.Pattern;
        if (pattern == null) return null;

        // 根据编译时优化原则，直接生成最简单的数组类型检查条件
        // 在模式匹配上下文中，这会被应用到具体的目标上
        // 返回Array.isArray的模板调用，实际参数在上下文中提供

        return new CallExpression(
            new MemberExpression(new Identifier("Array"), new Identifier("isArray"), computed: false, optional: false),
            NodeList.From<Expression>(), // 参数由上下文提供
            optional: false
        );
    }

    /// <summary>
    /// 处理隐式索引器引用操作
    /// C# 示例：
    /// array[^1]                           // 从末尾开始的索引
    /// array[^n]                           // 从末尾开始的第n个位置
    /// 转换结果：直接生成最简单的 array[array.length - n] 表达式
    /// 利用C#强类型系统，避免不必要的运行时检测，生成高效简洁的代码
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitImplicitIndexerReference(IImplicitIndexerReferenceOperation operation, Queue<VariableDeclaration> argument)
    {
        // 隐式索引器引用的直接AST转换，生成最简洁的代码
        var instance = Visit(operation.Instance, argument) as Expression;
        if (instance == null) return null;

        var indexArgument = Visit(operation.Argument, argument) as Expression;
        if (indexArgument == null) return null;

        // 生成 array.length 访问
        var lengthAccess = new MemberExpression(instance, new Identifier("length"), computed: false, optional: false);

        // 处理从末尾开始的索引（^n），转换为 length - n
        Expression indexCalculation;

        // 检查是否为 System.Index 类型或包含 Hat 操作的一元运算  
        if (operation.Argument?.Type?.Name == "Index" ||
            (operation.Argument is IUnaryOperation unaryOp &&
             (unaryOp.OperatorKind == UnaryOperatorKind.Hat || unaryOp.OperatorKind.ToString().Contains("Hat"))))
        {
            // 对于 ^n 模式，转换为 length - n
            Expression innerIndex;
            if (operation.Argument is IUnaryOperation unary)
            {
                innerIndex = Visit(unary.Operand, argument) as Expression ?? new NumericLiteral(1, "1");
            }
            else
            {
                // 如果是 Index类型，尝试获取其值
                innerIndex = indexArgument;
            }
            indexCalculation = new LogicalExpression(Operator.Subtraction, lengthAccess, innerIndex);
        }
        else
        {
            // 普通索引计算
            indexCalculation = new LogicalExpression(Operator.Subtraction, lengthAccess, indexArgument);
        }

        // 直接返回数组访问表达式：array[array.length - n]
        return new MemberExpression(instance, indexCalculation, computed: true, optional: false);
    }

    /// <summary>
    /// 处理 UTF-8 字符串操作
    /// C# 示例：
    /// ReadOnlySpan<byte> utf8 = "Hello"u8; // UTF-8 字符串字面量
    /// 转换结果：不支持，JavaScript 字符串是 UTF-16 编码
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitUtf8String(IUtf8StringOperation operation, Queue<VariableDeclaration> argument)
    {
        throw new OperationTransformationException(operation,
            "UTF-8 字节序列与 JavaScript UTF-16 字符串模型语义不兼容，无法保持字节级等价，编码语义不可映射");
    }

    /// <summary>
    /// 处理特性操作
    /// C# 示例：
    /// [Obsolete("Use NewMethod instead")]  // 特性应用
    /// [JsonPropertyName("custom_name")]    // 序列化特性
    /// 转换结果：不支持，JavaScript 没有特性系统
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitAttribute(IAttributeOperation operation, Queue<VariableDeclaration> argument)
    {
        throw new OperationTransformationException(operation, "Attributes are not supported in JavaScript conversion");
    }

    /// <summary>
    /// 处理内联数组访问操作
    /// C# 示例：
    /// Span<int> span = stackalloc int[10]; // 内联数组创建
    /// span[0] = 42;                        // 内联数组访问
    /// 转换结果：不支持，JavaScript 没有堆栈分配数组概念
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitInlineArrayAccess(IInlineArrayAccessOperation operation, Queue<VariableDeclaration> argument)
    {
        throw new OperationTransformationException(operation, "Inline array access is not supported in JavaScript conversion");
    }

    /// <summary>
    /// 处理集合表达式操作
    /// C# 示例：
    /// int[] array = [1, 2, 3, 4, 5];      // 集合表达式语法
    /// List<string> list = ["a", "b", "c"]; // 集合表达式初始化
    /// 转换结果：[1, 2, 3, 4, 5] / ["a", "b", "c"]
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitCollectionExpression(ICollectionExpressionOperation operation, Queue<VariableDeclaration> argument)
    {
        var elements = new List<Expression?>();

        foreach (var element in operation.Elements)
        {
            var elementNode = Visit(element, argument) as Expression;
            elements.Add(elementNode);
        }

        return new ArrayExpression(NodeList.From<Expression?>(elements));
    }

    /// <summary>
    /// 处理展开操作（扩展运算符）
    /// C# 示例：
    /// int[] combined = [..array1, ..array2]; // 数组展开
    /// Method(..args);                      // 参数展开
    /// 转换结果：...array1 / ...args（JavaScript 扩展运算符）
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitSpread(ISpreadOperation operation, Queue<VariableDeclaration> argument)
    {
        if (Visit(operation.Operand, argument) is not Expression operand)
            return null;

        return new SpreadElement(operand);
    }

    /// <summary>
    /// 处理声明表达式操作
    /// C# 示例：
    /// if (int.TryParse(input, out var result)) // out var 声明
    /// if (dict.TryGetValue(key, out string value)) // out 声明
    /// 转换结果：转换为 let 变量声明
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDeclarationExpression(IDeclarationExpressionOperation operation, Queue<VariableDeclaration> argument)
    {
        // 声明表达式（如 out var x）转换为变量声明
        var declarator = Visit(operation.Expression, argument);
        if (declarator is VariableDeclarator variableDeclarator)
        {
            return new VariableDeclaration(VariableDeclarationKind.Let,
                NodeList.From(new[] { variableDeclarator }));
        }

        return declarator;
    }

    /// <summary>
    /// 处理 switch 默认 case 子句操作
    /// C# 示例：
    /// switch (value) {
    ///     default:
    ///         DefaultAction();
    ///         break;
    /// }
    /// 转换结果：true（作为最终的 else 条件）
    /// </summary>
    /// <param name="operation">当前访问的operation</param>
    /// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
    /// <returns>Acornima的ESTree的Node</returns>
    public override Acornima.Ast.Node? VisitDefaultCaseClause(IDefaultCaseClauseOperation operation, Queue<VariableDeclaration> argument)
    {
        // 默认case子句转换为else分支，返回true作为最终条件
        return new BooleanLiteral(true, "true");
    }
}