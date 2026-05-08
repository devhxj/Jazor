using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Pinia
{
	/// <summary>
	/// 对象形式的 <c>mapActions()</c> 和 <c>mapWritableState()</c> 所使用的字符串键辅助映射器。
	/// String-keyed helper mapper used by object-form <c>mapActions()</c> and
	/// <c>mapWritableState()</c>.
	/// </summary>
	public record PiniaKeyMapper : Vue3.VueDictionary<string>;

	/// <summary>
	/// 对象形式的 <c>mapState()</c> / <c>mapGetters()</c> 所使用的字符串键辅助映射器。
	/// String-keyed helper mapper used by object-form <c>mapState()</c> /
	/// <c>mapGetters()</c>.
	/// </summary>
	/// <typeparam name="TStore">由 store 定义提供的类型化 store 投影。The typed store projection supplied by the store definition.</typeparam>
	public record PiniaStateMapper<TStore> : Vue3.VueDictionary<PiniaStateMapValue<TStore>>
		where TStore : class;

	/// <summary>
	/// 对象形式的 <c>mapState()</c> / <c>mapGetters()</c> 映射器值。
	/// 值可以是 store 成员名称或自定义选择器回调。
	/// Object-form <c>mapState()</c> / <c>mapGetters()</c> mapper value.
	/// Values can be either a store member name or a custom selector callback.
	/// </summary>
	/// <typeparam name="TStore">由 store 定义提供的类型化 store 投影。The typed store projection supplied by the store definition.</typeparam>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct PiniaStateMapValue<TStore>
		where TStore : class
	{
		private readonly byte _kind;
		private readonly string? _key;
		private readonly PiniaMapStateSelector<TStore>? _selector;

		private PiniaStateMapValue(string value)
		{
			_kind = 1;
			_key = value;
			_selector = default;
		}

		private PiniaStateMapValue(PiniaMapStateSelector<TStore> value)
		{
			_kind = 2;
			_key = default;
			_selector = value;
		}

		public string? AsKey => _kind == 1 ? _key : default;

		public PiniaMapStateSelector<TStore>? AsSelector => _kind == 2 ? _selector : default;

		[ECMAScriptInline("__arg1")]
		public extern static PiniaStateMapValue<TStore> From(string value);

		[ECMAScriptInline("__arg1")]
		public extern static PiniaStateMapValue<TStore> From(PiniaMapStateSelector<TStore> value);

		public static implicit operator PiniaStateMapValue<TStore>(string value)
			=> new(value);

		public static implicit operator PiniaStateMapValue<TStore>(PiniaMapStateSelector<TStore> value)
			=> new(value);
	}
}
