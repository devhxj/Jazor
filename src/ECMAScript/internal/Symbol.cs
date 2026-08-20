namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>Symbol</c> constructor host and its well-known symbols.
/// Members stay on this runtime host instead of being redistributed into CLR helper types.
/// JavaScript <c>Symbol</c> 构造器宿主及其众所周知 Symbol 的投影；成员保留在此运行时宿主上，不拆分到 CLR 辅助类型。
/// </summary>
[ECMAScript]
[Description("@#Symbol")]
public sealed class Symbol
{
	/// <summary>
	/// Gets JavaScript <c>Symbol.prototype</c> object.
	/// Keeping this on the constructor host avoids inventing a separate CLR helper surface.
	/// 获取 JavaScript <c>Symbol.prototype</c> 对象；保留在构造器宿主上可避免虚构独立的 CLR 辅助表面。
	/// </summary>
	[Description("@#prototype")]
	public extern static Symbol Prototype { get; }

	/// <summary>Gets <c>Symbol.hasInstance</c>, used by JavaScript <c>instanceof</c> customization. 获取用于自定义 JavaScript <c>instanceof</c> 的 <c>Symbol.hasInstance</c>。</summary>
	[Description("@#hasInstance")]
	public extern static Symbol HasInstance { get; }
	/// <summary>Gets <c>Symbol.isConcatSpreadable</c>, controlling array-like spreading in <c>concat</c>. 获取控制 <c>concat</c> 中数组类值展开的 <c>Symbol.isConcatSpreadable</c>。</summary>
	[Description("@#isConcatSpreadable")]
	public extern static Symbol IsConcatSpreadable { get; }
	/// <summary>Gets <c>Symbol.asyncIterator</c>, the async-iteration protocol key. 获取异步迭代协议键 <c>Symbol.asyncIterator</c>。</summary>
	[Description("@#asyncIterator")]
	public extern static Symbol AsyncIterator { get; }
	/// <summary>Gets <c>Symbol.asyncDispose</c>, the asynchronous explicit-disposal protocol key. 获取异步显式释放协议键 <c>Symbol.asyncDispose</c>。</summary>
	[Description("@#asyncDispose")]
	public extern static Symbol AsyncDispose { get; }
	/// <summary>Gets <c>Symbol.dispose</c>, the synchronous explicit-disposal protocol key. 获取同步显式释放协议键 <c>Symbol.dispose</c>。</summary>
	[Description("@#dispose")]
	public extern static Symbol Dispose { get; }
	/// <summary>Gets <c>Symbol.iterator</c>, the synchronous iteration protocol key. 获取同步迭代协议键 <c>Symbol.iterator</c>。</summary>
	[Description("@#iterator")]
	public extern static Symbol Iterator { get; }
	/// <summary>Gets <c>Symbol.match</c>, the regular-expression matching protocol key. 获取正则匹配协议键 <c>Symbol.match</c>。</summary>
	[Description("@#match")]
	public extern static Symbol Match { get; }
	/// <summary>Gets <c>Symbol.matchAll</c>, the all-matches iteration protocol key. 获取全部匹配迭代协议键 <c>Symbol.matchAll</c>。</summary>
	[Description("@#matchAll")]
	public extern static Symbol MatchAll { get; }
	/// <summary>Gets <c>Symbol.replace</c>, the string replacement protocol key. 获取字符串替换协议键 <c>Symbol.replace</c>。</summary>
	[Description("@#replace")]
	public extern static Symbol Replace { get; }
	/// <summary>Gets <c>Symbol.search</c>, the string search protocol key. 获取字符串搜索协议键 <c>Symbol.search</c>。</summary>
	[Description("@#search")]
	public extern static Symbol Search { get; }
	/// <summary>Gets <c>Symbol.species</c>, used by derived constructors to select produced collection types. 获取由派生构造器选择结果集合类型的 <c>Symbol.species</c>。</summary>
	[Description("@#species")]
	public extern static Symbol Species { get; }
	/// <summary>Gets <c>Symbol.split</c>, the string splitting protocol key. 获取字符串拆分协议键 <c>Symbol.split</c>。</summary>
	[Description("@#split")]
	public extern static Symbol Split { get; }
	/// <summary>Gets <c>Symbol.toPrimitive</c>, the object-to-primitive conversion protocol key. 获取对象到原始值转换协议键 <c>Symbol.toPrimitive</c>。</summary>
	[Description("@#toPrimitive")]
	public extern static Symbol ToPrimitive { get; }
	/// <summary>Gets <c>Symbol.toStringTag</c>, used to customize object type tags. 获取用于自定义对象类型标签的 <c>Symbol.toStringTag</c>。</summary>
	[Description("@#toStringTag")]
	public extern static Symbol ToStringTag { get; }
	/// <summary>Gets <c>Symbol.unscopables</c>, which excludes names from legacy <c>with</c> scope lookup. 获取从遗留 <c>with</c> 作用域查找中排除名称的 <c>Symbol.unscopables</c>。</summary>
	[Description("@#unscopables")]
	public extern static Symbol Unscopables { get; }

	/// <summary>
	/// Gets the optional description carried by the JavaScript symbol.
	/// Nullable is used because symbols may be created without a description; it is not a globally unique identifier.
	/// 获取 JavaScript Symbol 携带的可选说明；可空是因为 Symbol 可在无说明时创建，且说明不是全局唯一标识。
	/// </summary>
	[Description("@#description")]
	public extern string? Description { get; }

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>Symbol.prototype[@@toPrimitive]</c>.
	/// JavaScript ignores the hint and returns the wrapped symbol value directly.
	/// JavaScript <c>Symbol.prototype[@@toPrimitive]</c> 的隐藏协议桥接；JavaScript 忽略 hint 并直接返回包装的 Symbol 值。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@toPrimitive")]
	public extern Symbol ToPrimitive_();

	/// <summary>
	/// Hidden projection of JavaScript <c>Symbol.prototype[@@toStringTag]</c>.
	/// This stays hidden because it is primarily used by host protocol machinery such as <c>Object.prototype.toString</c>.
	/// JavaScript <c>Symbol.prototype[@@toStringTag]</c> 的隐藏投影；主要供 <c>Object.prototype.toString</c> 等宿主协议机制使用。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@toStringTag")]
	public extern string ToStringTag_ { get; }

	/// <summary>Returns JavaScript's descriptive Symbol text, such as <c>Symbol(key)</c>. 返回 JavaScript 的描述性 Symbol 文本，例如 <c>Symbol(key)</c>。</summary>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>
	/// Returns the primitive symbol value carried by this host projection.
	/// Returns the same symbol identity rather than creating a new Symbol.
	/// 返回此宿主投影携带的原始 Symbol 值；返回相同 Symbol 标识，不会创建新 Symbol。
	/// </summary>
	[Description("@#valueOf")]
	public extern Symbol ValueOf();

	/// <summary>
	/// Retrieves or creates a symbol from JavaScript's realm-wide global registry.
	/// Repeated calls with the same key return the same Symbol identity, unlike separately created symbols with the same description.
	/// 从 JavaScript realm 范围的全局注册表获取或创建 Symbol；相同键的重复调用返回相同 Symbol 标识，不同于说明相同但独立创建的 Symbol。
	/// </summary>
	[Description("@#for")]
	public extern static Symbol For(string key);

	/// <summary>
	/// Returns the key associated with the given symbol in the global registry, or <see langword="null"/>.
	/// Symbols not created through <see cref="For"/> have no registry key.
	/// 返回给定 Symbol 在全局注册表中的键；不在注册表中时为 <see langword="null"/>。未通过 <see cref="For"/> 创建的 Symbol 没有注册表键。
	/// </summary>
	[Description("@#keyFor")]
	public extern static string? KeyFor(Symbol sym);
}
