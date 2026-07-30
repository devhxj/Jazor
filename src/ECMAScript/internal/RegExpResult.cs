using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
/// <summary>
/// 表示 JavaScript RegExp.exec/match 返回的数组型结果及命名属性。
/// </summary>
/// <remarks>
/// 该类型同时具有数组索引和 input/index/groups 等附加属性，因此不能只用普通 string[] 表达；
/// 它是 host binding，不是 C# 正则引擎实现。
/// </remarks>
public sealed class RegExpResult : IArray<string?>
{
    ///<summary>
    ///Returns the Strings against which a regular expression search was performed. Read-only.
    ///</summary>
    [Description("@#input")]
    public extern string Input { get; }

    ///<summary>
    ///Returns the character position where the first successful match begins in a searched Strings. Read-only.
    ///</summary>
    [Description("@#index")]
    public extern Number Index { get; }

    /// <summary>
    /// Named capture groups returned by <c>RegExp.prototype.exec</c>.
    /// This is exposed as <see cref="IObject"/> because the value is consumed through
    /// JavaScript-style key access rather than a strongly typed CLR dictionary contract.
    /// </summary>
    [Description("@#groups")]
    public extern IObject? Groups { get; }

    /// <summary>
    /// Match index pairs returned when the regular expression uses the <c>d</c> flag.
    /// This is modeled explicitly because JavaScript exposes an array-like object with optional named-group metadata.
    /// </summary>
    [Description("@#indices")]
    public extern RegExpIndices? Indices { get; }

    /// <summary>
    /// Direct access to the match result elements.
    /// Unmatched capture groups are <c>undefined</c> in JavaScript, so this projection exposes nullable elements and maps that absence to <see langword="null" />.
    /// </summary>
    public extern string? this[Number index] { get; }

    [Description("@#length")]
    public extern Number Length { get; }

    public extern IEnumerator GetEnumerator();
}

/// <summary>
/// JavaScript object shape used by <c>RegExpResult.indices</c>.
/// Each element is a two-item array <c>[start, end]</c>, or <see langword="null"/> for an unmatched capture group.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class RegExpIndices : IArray<Array<Number>?>
{
    /// <summary>
    /// Named capture groups returned by <c>RegExpResult.indices.groups</c>.
    /// This stays object-shaped because JavaScript exposes dynamic group names.
    /// </summary>
    [Description("@#groups")]
    public extern IObject? Groups { get; }

    public extern Array<Number>? this[Number index] { get; }

    [Description("@#length")]
    public extern Number Length { get; }

    public extern IEnumerator GetEnumerator();
}
