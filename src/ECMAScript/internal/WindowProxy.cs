namespace ECMAScript;

/// <summary>
/// CLR view of a window-like object.
/// This interface is only a typing aid and does not correspond to a standalone JavaScript host object.
/// window 类对象的 CLR 视图；此接口仅用于类型辅助，不对应独立的 JavaScript 宿主对象。
/// </summary>
[ECMAScript]
[Description("@#")]
public interface IWindow
{
	/// <summary>Gets the window location value. 获取 window 的 location 值。</summary>
	[Description("@#location")]
	string Location { get; }
}

/// <summary>
/// CLR type representing JavaScript's window proxy object.
/// It is typically obtained from runtime APIs such as <c>globalThis.window</c> rather than constructed directly.
/// 表示 JavaScript window 代理对象的 CLR 类型；通常从 <c>globalThis.window</c> 等运行时 API 获取，而非直接构造。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class WindowProxy : Window, IWindow
{
	/// <summary>Gets the proxied window location value. 获取代理 window 的 location 值。</summary>
	[Description("@#location")]
	public extern new string Location { get; }
}
