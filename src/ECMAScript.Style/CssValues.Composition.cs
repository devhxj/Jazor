namespace ECMAScript.Style;

// Distinct intermediate types retain arity at compile time without allocating JS wrappers.
// A finished CssPadding has no pipe operator: neither a fifth side nor appending to a
// padding(...) value of unknown arity can silently produce invalid CSS.
// 不同的中间类型在编译期保留数量，JS 中只拼接字符串。最终 CssPadding 不提供继续追加入口，
// 因此第五值及对未知数量的 padding(...) 继续追加都会在 C# 编译时被拒绝。

/// <summary>Two padding sides; append one side to obtain a triple。双值 padding，可继续追加第三值。</summary>
[ECMAScript]
[Description("@#")]
public sealed class CssPaddingPair
{
    internal CssPaddingPair() { }

    /// <summary>Appends the bottom side。追加 bottom 值。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingTriple operator |(CssPaddingPair left, CssPaddingPart right);
}

/// <summary>Three padding sides; append the left side to finish。三值 padding，可追加 left 值完成四边简写。</summary>
[ECMAScript]
[Description("@#")]
public sealed class CssPaddingTriple
{
    internal CssPaddingTriple() { }

    /// <summary>Appends the fourth and final side。追加第四个、也是最后一个边值。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPadding operator |(CssPaddingTriple left, CssPaddingPart right);
}

public sealed partial class CssLength
{
    /// <summary>Starts a padding pair with lengths or percentages。使用长度或百分比开始双值 padding。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingPair operator |(CssLength left, CssPercentage right);

    /// <summary>Starts a padding pair with lengths or percentages。使用长度或百分比开始双值 padding。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingPair operator |(CssLength left, CssLengthPercentage right);

}

public sealed partial class CssPercentage
{
    /// <summary>Starts a padding pair with lengths or percentages。使用长度或百分比开始双值 padding。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingPair operator |(CssPercentage left, CssLength right);

    /// <summary>Starts a padding pair with lengths or percentages。使用长度或百分比开始双值 padding。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingPair operator |(CssPercentage left, CssPercentage right);

    /// <summary>Starts a padding pair with lengths or percentages。使用长度或百分比开始双值 padding。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingPair operator |(CssPercentage left, CssLengthPercentage right);

}

public sealed partial class CssLengthPercentage
{
    /// <summary>Starts a padding pair with lengths or percentages。使用长度或百分比开始双值 padding。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingPair operator |(CssLengthPercentage left, CssLength right);

    /// <summary>Starts a padding pair with lengths or percentages。使用长度或百分比开始双值 padding。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingPair operator |(CssLengthPercentage left, CssPercentage right);

    /// <summary>Starts a padding pair with lengths or percentages。使用长度或百分比开始双值 padding。</summary>
    [ECMAScriptInline("__arg1 + \" \" + __arg2")]
    public static extern CssPaddingPair operator |(CssLengthPercentage left, CssLengthPercentage right);

}
