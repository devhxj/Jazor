using ECMAScript;
using static ECMAScript.Global;

namespace Jazor.CLR;

/// <summary>
/// Blazor LocationChangingContext 在浏览器里没有运行时身份，按普通对象映射。
/// </summary>
/// <remarks>
/// 上下文由 NavigationManagerModule 在提交内部导航前构造，PreventNavigation() 只在该对象上写一个
/// 私有标记，由导航侧读取后决定是否放弃本次导航；CancellationToken 擦除为 AbortSignal，其生命周期
/// （被后续导航取代时取消）由 NavigationManagerModule 拥有。
/// </remarks>
[ECMAScriptModule("Microsoft/AspNetCore/Components/Routing/LocationChangingContextModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext", "Object")]
public static class LocationChangingContextModule
{
	private const string PreventedKey = "__jazorNavigationPrevented";

	/// <summary>
	/// 构造一次内部导航的 location-changing 上下文。
	/// </summary>
	internal static object CreateLocationChangingContext(
		string targetLocation,
		string? historyEntryState,
		bool isNavigationIntercepted,
		AbortSignal cancellationToken)
		=> new
		{
			targetLocation,
			historyEntryState,
			isNavigationIntercepted,
			cancellationToken,
			__jazorNavigationPrevented = false
		};

	/// <summary>
	/// 读取 handler 是否已经取消本次导航。
	/// </summary>
	internal static bool IsNavigationPrevented(object context)
	{
		var prevented = context.Get(PreventedKey);
		return TypeOf(prevented) == "boolean" && (bool)prevented!;
	}

	// CLR 的 init-only 属性默认值是 null/false，字面量把每个字段都写出来，避免 JavaScript undefined；
	// CancellationToken 的默认值是 CancellationToken.None，必须落在那个共享的 never-abort 单例上。
	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.LocationChangingContext()", "createDefault")]
	public static object _cb05ff6d85c9ed62()
		=> new
		{
			targetLocation = (string?)null,
			historyEntryState = (string?)null,
			isNavigationIntercepted = false,
			cancellationToken = CancellationTokenModule.GetNoneSignal(),
			__jazorNavigationPrevented = false
		};

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.TargetLocation.get", "targetLocation")]
	public extern static string _e9697f6bb5348e79(object instance);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.TargetLocation.init", "targetLocation")]
	public extern static void _47222d9b8d1efd3c(object instance, string value);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.HistoryEntryState.get", "historyEntryState")]
	public extern static string? _6ae2081ed5f08f27(object instance);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.HistoryEntryState.init", "historyEntryState")]
	public extern static void _b92a7f49916332ed(object instance, string? value);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.IsNavigationIntercepted.get", "isNavigationIntercepted")]
	public extern static bool _6aba1ee2e8cdac36(object instance);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.IsNavigationIntercepted.init", "isNavigationIntercepted")]
	public extern static void _41750486d43c1fcf(object instance, bool value);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.CancellationToken.get", "cancellationToken")]
	public extern static AbortSignal _f525c28714bb7530(object instance);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.CancellationToken.init", "cancellationToken")]
	public extern static void _9dbc61a901ee1943(object instance, AbortSignal value);

	/// <summary>
	/// C#: context.PreventNavigation()
	/// JS: 在上下文对象上写入私有标记，导航侧在全部 handler 完成后读取该标记。
	/// </summary>
	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.Routing.LocationChangingContext.PreventNavigation()", "preventNavigation")]
	public static void _8e3814bc45081e90(object instance)
		=> Reflect.Set(instance, PreventedKey, true);
}
