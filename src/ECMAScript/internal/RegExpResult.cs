using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
/// <summary>
/// Represents the array-like result and named properties returned by JavaScript <c>RegExp.exec</c> or matching APIs.
/// 表示 JavaScript <c>RegExp.exec</c> 或匹配 API 返回的数组型结果及命名属性。
/// </summary>
/// <remarks>
/// The type has both array indices and extra properties such as <c>input</c>, <c>index</c>, and <c>groups</c>, so a plain <c>string[]</c> is insufficient.
/// It is a host binding, not a C# regular-expression engine implementation.
/// 此类型同时具有数组索引和 <c>input</c>、<c>index</c>、<c>groups</c> 等附加属性，不能只用普通 <c>string[]</c> 表达；
/// 它是宿主绑定，不是 C# 正则引擎实现。
/// </remarks>
public sealed class RegExpResult : IArray<string?>
{
    ///<summary>
    /// <summary>Gets the input string searched by the regular expression. 获取正则表达式搜索的输入字符串。</summary>
    ///</summary>
    [Description("@#input")]
    public extern string Input { get; }

    ///<summary>
    /// <summary>Gets the zero-based UTF-16 code-unit position where the full match begins. 获取完整匹配开始处的零基 UTF-16 代码单元位置。</summary>
    ///</summary>
    [Description("@#index")]
    public extern Number Index { get; }

    /// <summary>
    /// Gets named capture groups returned by <c>RegExp.prototype.exec</c>.
    /// This is exposed as <see cref="IObject"/> because the value is consumed through JavaScript-style key access rather than a strongly typed CLR dictionary contract.
    /// 获取 <c>RegExp.prototype.exec</c> 返回的命名捕获组；以 <see cref="IObject"/> 公开，因为其通过 JavaScript 风格键访问而非强类型 CLR 字典契约使用。
    /// </summary>
    [Description("@#groups")]
    public extern IObject? Groups { get; }

    /// <summary>
    /// Gets match index pairs returned when the regular expression uses the <c>d</c> flag.
    /// This is modeled explicitly because JavaScript exposes an array-like object with optional named-group metadata.
    /// 获取表达式使用 <c>d</c> 标志时返回的匹配索引对；明确建模是因为 JavaScript 公开带可选命名组元数据的数组类对象。
    /// </summary>
    [Description("@#indices")]
    public extern RegExpIndices? Indices { get; }

    /// <summary>
    /// Gets direct access to match result elements.
    /// Unmatched capture groups are <c>undefined</c> in JavaScript, so this projection exposes nullable elements and maps that absence to <see langword="null"/>.
    /// 直接访问匹配结果元素；未匹配的捕获组在 JavaScript 中为 <c>undefined</c>，此投影公开可空元素并将缺失映射为 <see langword="null"/>。
    /// </summary>
    public extern string? this[Number index] { get; }

    /// <summary>Gets the full match plus capture count. 获取完整匹配及捕获组数量。</summary>
    [Description("@#length")]
    public extern Number Length { get; }

    /// <summary>Returns an enumerator over the match and capture values. 返回枚举匹配及捕获值的枚举器。</summary>
    public extern IEnumerator GetEnumerator();
}

/// <summary>
/// JavaScript object shape used by <c>RegExpResult.indices</c>.
/// Each element is a two-item array <c>[start, end]</c>, or <see langword="null"/> for an unmatched capture group.
/// <c>RegExpResult.indices</c> 使用的 JavaScript 对象形状；每个元素是 <c>[start, end]</c> 双项数组，未匹配捕获组为 <see langword="null"/>。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class RegExpIndices : IArray<Array<Number>?>
{
    /// <summary>
    /// Gets named capture groups returned by <c>RegExpResult.indices.groups</c>.
    /// This stays object-shaped because JavaScript exposes dynamic group names.
    /// 获取 <c>RegExpResult.indices.groups</c> 返回的命名捕获组；保持对象形状是因为 JavaScript 公开动态组名称。
    /// </summary>
    [Description("@#groups")]
    public extern IObject? Groups { get; }

    /// <summary>Gets a capture index pair, or <see langword="null"/> for an unmatched capture. 获取捕获索引对；未匹配捕获时为 <see langword="null"/>。</summary>
    public extern Array<Number>? this[Number index] { get; }

    /// <summary>Gets the full match plus capture index count. 获取完整匹配及捕获索引数量。</summary>
    [Description("@#length")]
    public extern Number Length { get; }

    /// <summary>Returns an enumerator over capture index pairs. 返回枚举捕获索引对的枚举器。</summary>
    public extern IEnumerator GetEnumerator();
}
