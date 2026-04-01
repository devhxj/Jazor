namespace ECMAScript;

/// <summary>
/// CLR view of a window-like object.
/// This interface is only a typing aid and does not correspond to a standalone JavaScript host object.
/// </summary>
[ECMAScript]
[Description("@#")]
public interface IWindow
{
	[Description("@#location")]
	string Location { get; }
}

/// <summary>
/// CLR type representing JavaScript's window proxy object.
/// It is typically obtained from runtime APIs such as <c>globalThis.window</c> rather than constructed directly.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class WindowProxy : Window, IWindow
{
	[Description("@#location")]
	public extern new string Location { get; }
}
