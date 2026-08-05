// File: ScopeSite.cs
// Purpose: Describes the kind of emitted JavaScript scope entered during lowering.
// scope site 参与稳定名称分配，确保同一 C# 语义在不同 lexical 边界内不会发生名称冲突。
namespace Jazor.Compiler;

/// <summary>
/// 标识一个合成发射作用域的来源位置。
/// </summary>
/// <remarks>
/// 作用域类别参与稳定名称哈希。新增类别或改变进入作用域的边界时，可能改变生成名称，
/// 因此应只在确实改变语义作用域时扩展此枚举。
/// </remarks>
internal enum ScopeSiteKind
{
    RootFragment,
    FunctionBody,
    StaticBlock,
    NestedBlock,
    LocalFunctionBody,
    LambdaBody,
    TryBody,
    CatchBody,
    FinallyBody,
    SwitchCaseBody,
    PatternIife,
    SwitchExpressionIife,
    ObjectInitializerIife
}

/// <summary>
/// 携带作用域类别的不可变作用域描述值。
/// </summary>
/// <remarks>
/// 工厂方法集中在这里，调用方不应直接复用不匹配的作用域类别；作用域类别会影响稳定临时
/// 名称，因此必须与实际 lexical/emission 边界一致。
/// </remarks>
internal readonly record struct ScopeSite(ScopeSiteKind Kind)
{
    public static ScopeSite RootFragment()
        => new(ScopeSiteKind.RootFragment);

    public static ScopeSite FunctionBody()
        => new(ScopeSiteKind.FunctionBody);

    public static ScopeSite StaticBlock()
        => new(ScopeSiteKind.StaticBlock);

    public static ScopeSite NestedBlock()
        => new(ScopeSiteKind.NestedBlock);

    public static ScopeSite LocalFunctionBody()
        => new(ScopeSiteKind.LocalFunctionBody);

    public static ScopeSite LambdaBody()
        => new(ScopeSiteKind.LambdaBody);

    public static ScopeSite TryBody()
        => new(ScopeSiteKind.TryBody);

    public static ScopeSite CatchBody()
        => new(ScopeSiteKind.CatchBody);

    public static ScopeSite FinallyBody()
        => new(ScopeSiteKind.FinallyBody);

    public static ScopeSite SwitchCaseBody()
        => new(ScopeSiteKind.SwitchCaseBody);

    public static ScopeSite PatternIife()
        => new(ScopeSiteKind.PatternIife);

    public static ScopeSite SwitchExpressionIife()
        => new(ScopeSiteKind.SwitchExpressionIife);

    public static ScopeSite ObjectInitializerIife()
        => new(ScopeSiteKind.ObjectInitializerIife);
}
