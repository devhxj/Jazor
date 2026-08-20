namespace Jazor.CLR;

/// <summary>
/// Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs 映射为普通 JavaScript 对象。
/// 该类型在浏览器端没有运行时身份，构造后由 <c>path</c> 属性承载全部状态，
/// 因此构造器显式写出该字段，避免未赋值时读到 undefined。
/// </summary>
[ECMAScriptModule("Microsoft/AspNetCore/Components/Routing/NotFoundEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs", "Object")]
public static class NotFoundEventArgsModule
{
	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs.NotFoundEventArgs()", "createNotFoundEventArgs")]
	public static object _8ed2c94001d3c848()
		=> new { path = (string?)null };

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs.Path.get", "path")]
	public extern static string? _5dc44417c1aea460(Object instance);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs.Path.set", "path")]
	public extern static void _66b297a27c19ef7c(Object instance, string? value);
}
