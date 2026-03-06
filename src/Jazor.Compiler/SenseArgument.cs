using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// 语义上下文参数，传递给 Visit 方法。
/// 作为值类型，通过 with 语法创建新实例传递不同的语义上下文。
/// </summary>
/// <param name="Sense">语义场景标识，决定 Visit 方法的处理方式</param>
/// <param name="Depend">依赖项（引用类型），用于变量声明和导入收集，可为 null</param>
/// <param name="PatternInput">模式匹配输入表达式，用于 is pattern / switch pattern 等场景</param>
/// <param name="CatchExceptionVar">Catch 子句异常参数名，用于 re-throw 场景</param>
/// <param name="SwitchExpressionVar">Switch 表达式输入变量名，用于 switch expression 编译为 IIFE</param>
public readonly record struct SenseArgument(
    Sense Sense = Sense.Any,
    WalkerArgument? Depend = null,
    Expression? PatternInput = null,
    string? CatchExceptionVar = null,
    string? SwitchExpressionVar = null)
{
    /// <summary>默认参数（Depend 为 null，首次使用时创建）</summary>
    public static readonly SenseArgument Default = new();

    // ===== 核心：获取 Depend（确保非 null）=====
    /// <summary>
    /// 获取 Depend，如果为 null 则创建新实例。
    /// 这是访问 Depend 的推荐方式。
    /// </summary>
    public WalkerArgument DependOrNew => Depend ?? new WalkerArgument();

    // ===== Sense 变更 =====
    /// <summary>创建新实例，设置 Sense</summary>
    public SenseArgument With(Sense sense) => this with { Sense = sense };

    // ===== Depend 变更（作用域隔离）=====
    /// <summary>
    /// 创建新实例，使用新的 Depend（用于块级作用域隔离）。
    /// 新的 WalkerArgument 会共享导入字典（如果需要）。
    /// </summary>
    public SenseArgument WithNewScope()
    {
        // 共享导入，新的变量声明字典
        var newDepend = Depend is not null
            ? Depend.WithNewDeclarators()
            : new WalkerArgument();
        return this with { Depend = newDepend };
    }

    // ===== 模式匹配上下文 =====
    /// <summary>设置模式匹配输入表达式</summary>
    public SenseArgument WithPatternInput(Expression? input) => this with { PatternInput = input };

    // ===== 异常处理上下文 =====
    /// <summary>设置 Catch 异常参数名</summary>
    public SenseArgument WithCatchVar(string? varName) => this with { CatchExceptionVar = varName };

    // ===== Switch 表达式上下文 =====
    /// <summary>设置 Switch 表达式变量名</summary>
    public SenseArgument WithSwitchVar(string? varName) => this with { SwitchExpressionVar = varName };

    // ===== 组合设置 =====
    /// <summary>设置 Sense 和 PatternInput</summary>
    public SenseArgument With(Sense sense, Expression patternInput)
        => this with { Sense = sense, PatternInput = patternInput };

    // ===== WalkerArgument 便捷方法 =====
    /// <summary>
    /// 添加变量声明到 Depend。
    /// 注意：调用此方法前应确保 Depend 已初始化。
    /// </summary>
    /// <param name="declarator">变量声明</param>
    /// <param name="depth">作用域深度</param>
    public void AddVarDeclarator(VariableDeclarator declarator, int depth)
    {
        // 直接使用 DependOrNew，但这可能会创建新实例
        // 由于 SenseArgument 是 struct，我们需要确保调用方在使用时 Depend 已初始化
        (Depend ?? throw new InvalidOperationException("Depend must be initialized before adding variable declarators"))
            .AddVarDeclarator(declarator, depth);
    }

    /// <summary>
    /// 是否包含变量声明
    /// </summary>
    public bool HasVarDeclarator => Depend?.HasVarDeclarator ?? false;

    /// <summary>
    /// 添加导入声明规范
    /// </summary>
    public void MergeImportSpecifier(string modulePath, ImportDeclarationSpecifier specifier)
    {
        (Depend ?? throw new InvalidOperationException("Depend must be initialized before merging import specifiers"))
            .MergeImportSpecifier(modulePath, specifier);
    }

    /// <summary>
    /// 是否包含导入声明规范
    /// </summary>
    public bool HasVarImportDeclarationSpecifier => Depend?.HasVarImportDeclarationSpecifier ?? false;

    /// <summary>
    /// 刷新并获取当前累积的变量声明列表
    /// </summary>
    public NodeList<VariableDeclarator> FlushVarDeclarator()
        => Depend?.FlushVarDeclarator() ?? NodeList.From<VariableDeclarator>();
}
