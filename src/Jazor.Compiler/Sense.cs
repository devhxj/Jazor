// File: Sense.cs
// Purpose: Enumerates the immediate semantic context in which an operation is lowered.
// Sense 通过显式参数传递上下文，避免靠 Roslyn Parent 链猜测当前表达式/语句用途。
namespace Jazor.Compiler;

/// <summary>
/// 表示编译器内部使用的语法场景
///
/// <para><b>设计原则</b></para>
/// <list type="number">
/// <item>Sense 值应该描述"语义上下文"，而非"语法结构"</item>
/// <item>优先使用 Sense 判断，避免 operation.Parent 检查</item>
/// <item>通过 SenseArgument.With() 传递上下文，保持不可变性</item>
/// </list>
///
/// <para><b>使用指南</b></para>
/// <list type="bullet">
/// <item><see cref="Any"/>: 默认值，不限制上下文</item>
/// <item><see cref="LeftValue"/>: 赋值表达式左侧，如 x = 5 中的 x</item>
/// <item><see cref="RightValue"/>: 赋值表达式右侧，如 x = 5 中的 5</item>
/// <item><see cref="PatternInput"/>: 模式匹配输入，如 obj is int x 中的 obj</item>
/// <item><see cref="FunctionBody"/>: 函数体上下文，Block 应返回 FunctionBody</item>
/// </list>
/// </summary>
public enum Sense
{
    // ===== 通用 =====
    /// <summary>不限制，默认值</summary>
    Any,

    // ===== 赋值上下文 =====
    /// <summary>左值上下文（赋值目标）</summary>
    LeftValue,
    /// <summary>右值上下文（赋值源）</summary>
    RightValue,
    /// <summary>属性赋值上下文（对象初始化器中）</summary>
    PropertyAssignment,
    /// <summary>解构赋值上下文</summary>
    Deconstruction,

    // ===== Block 上下文 =====
    /// <summary>函数体上下文（方法、Lambda、局部函数、构造函数）</summary>
    FunctionBody,
    /// <summary>表达式体成员根；保留表达式 lowering 的根作用域，同时标记可直接物化的根语句。</summary>
    ExpressionBody,
    /// <summary>静态初始化块上下文</summary>
    StaticBlock,
    /// <summary>嵌套块上下文</summary>
    NestedBlock,
    /// <summary>Catch 处理器上下文</summary>
    CatchHandler,

    // ===== 模式匹配上下文 =====
    /// <summary>模式匹配输入表达式（传递给子模式）</summary>
    PatternInput,
    /// <summary>Switch case 模式上下文</summary>
    PatternCase,
    /// <summary>Switch expression arm 上下文</summary>
    SwitchExpressionArm,
    /// <summary>属性子模式上下文</summary>
    PropertySubpattern,
    /// <summary>模式表达式上下文（需要作为独立表达式返回，不需要 SequenceExpression 包装）</summary>
    PatternExpression,

    // ===== 引用上下文 =====
    /// <summary>属性读取</summary>
    PropertyRead,
    /// <summary>属性写入</summary>
    PropertyWrite,
    /// <summary>包含类型实例（this）</summary>
    ContainingTypeInstance,
    /// <summary>隐式接收者</summary>
    ImplicitReceiver,

    // ===== 创建上下文 =====
    /// <summary>对象初始化器上下文</summary>
    ObjectInitializer,
    /// <summary>集合初始化器上下文</summary>
    CollectionInitializer,

    // ===== 异常上下文 =====
    /// <summary>抛出新异常</summary>
    ThrowNew,
    /// <summary>重新抛出异常</summary>
    Rethrow,

    // ===== 声明上下文 =====
    /// <summary>Out 参数声明</summary>
    OutParameter,
    /// <summary>变量声明</summary>
    VariableDeclaration,
    /// <summary>方法参数上下文（用于判断是否需要添加变量声明）</summary>
    Argument,

    // ===== 丢弃上下文 =====
    /// <summary>丢弃赋值</summary>
    DiscardAssignment,
    /// <summary>默认值</summary>
    DefaultValue,
}
