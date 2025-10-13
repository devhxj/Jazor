namespace ECMAScript;

[ECMAScript]
public interface IWindow
{
	void Navigate(string url);
	string Location { get; }
}

[ECMAScript]
public sealed class WindowProxy : Window, IWindow
{
	public extern WindowProxy(string allowedOrigin);

	public extern void Navigate(string url);

	public extern new string Location { get; }
}