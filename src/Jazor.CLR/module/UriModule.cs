namespace Jazor.CLR;

/// <summary>
/// System.Uri 映射到浏览器的 URL 构造器。
/// URL 只表示绝对 URI，因此这里只提供 Blazor 导航路径上真正会用到的绝对 URI 切片；
/// 相对 URI、UNC/文件路径、Segments 与等值语义继续保持 unsupported，由使用点显式失败。
/// </summary>
[ECMAScriptModule("System/UriModule.js")]
[Jazor(Op.Alias, "System.Uri", "URL")]
public static class UriModule
{
	[Jazor(Op.Inline, "System.Uri.Uri(string)", "new URL(__arg1)")]
	public extern static URL _c69acf122e3679e8(string uriString);

	[Jazor(Op.Inline, "System.Uri.Uri(System.Uri, string)", "new URL(__arg2, __arg1.href)")]
	public extern static URL _1018fc46d28f8d3a(URL baseUri, string relativeUri);

	[Jazor(Op.Alias, "System.Uri.AbsoluteUri.get", "href")]
	public extern static string _8dd50aa0b90b1213(URL instance);

	[Jazor(Op.Inline, "override System.Uri.ToString()", "__arg1.href")]
	public extern static string _833680ed5ab9dcdd(URL instance);

	[Jazor(Op.Alias, "System.Uri.AbsolutePath.get", "pathname")]
	public extern static string _3f2baa0c5c2071a3(URL instance);

	[Jazor(Op.Alias, "System.Uri.Query.get", "search")]
	public extern static string _2f4402ceaba26b6a(URL instance);

	[Jazor(Op.Alias, "System.Uri.Fragment.get", "hash")]
	public extern static string _1f4ba98ea2f15f79(URL instance);

	// Uri.Host 与 Uri.Authority 的差别就是端口：URL.hostname 不带端口，URL.host 带非默认端口。
	[Jazor(Op.Alias, "System.Uri.Host.get", "hostname")]
	public extern static string _0bf33ac48e4b1418(URL instance);

	[Jazor(Op.Alias, "System.Uri.Authority.get", "host")]
	public extern static string _593ae4a9895276c1(URL instance);

	// URL.protocol 保留结尾冒号，Uri.Scheme 不保留。
	[Jazor(Op.Inline, "System.Uri.Scheme.get", "__arg1.protocol.slice(0, -1)")]
	public extern static string _3e0f742131d7d14c(URL instance);

	// 拼接需要读取 instance 两次，因此走 Import 而不是 Inline 模板，避免重复求值。
	[Jazor(Op.Import, "System.Uri.PathAndQuery.get", "getPathAndQuery")]
	public static string _a239992828cb65fe(URL instance)
		=> instance.Pathname + instance.Search;

	// URL.port 在使用协议默认端口时是空串，Uri.Port 则返回该默认端口，未知协议返回 -1。
	[Jazor(Op.Import, "System.Uri.Port.get", "getPort")]
	public static Number _6c33876665ad9277(URL instance)
	{
		var port = instance.Port;
		if (port.Length != 0)
			return ECMAScript.Global.ParseInt(port, 10);

		var protocol = instance.Protocol;
		if (protocol == "https:" || protocol == "wss:")
			return 443;
		if (protocol == "http:" || protocol == "ws:")
			return 80;
		if (protocol == "ftp:")
			return 21;

		return -1;
	}
}
