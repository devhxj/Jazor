namespace Jazor.Compiler;

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
