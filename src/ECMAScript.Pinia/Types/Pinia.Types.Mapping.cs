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

		/// <summary>
		/// 从字符串键初始化。
		/// Initializes from a string key.
		/// </summary>
		/// <param name="value">store 成员名称字符串。The store member name string.</param>
		private PiniaStateMapValue(string value)
		{
			_kind = 1;
			_key = value;
			_selector = default;
		}

		/// <summary>
		/// 从选择器回调初始化。
		/// Initializes from a selector callback.
		/// </summary>
		/// <param name="value">状态选择器回调。The state selector callback.</param>
		private PiniaStateMapValue(PiniaMapStateSelector<TStore> value)
		{
			_kind = 2;
			_key = default;
			_selector = value;
		}

		/// <summary>
		/// 以 store 成员名称字符串形式返回值，如果不是字符串变体则返回 default。
		/// Returns the value as a store member name string, or default if not a string variant.
		/// </summary>
		public string? AsKey => _kind == 1 ? _key : default;

		/// <summary>
		/// 以选择器回调形式返回值，如果不是选择器变体则返回 default。
		/// Returns the value as a selector callback, or default if not a selector variant.
		/// </summary>
		public PiniaMapStateSelector<TStore>? AsSelector => _kind == 2 ? _selector : default;

		/// <summary>
		/// 从字符串键创建状态映射值。
		/// Creates a state map value from a string key.
		/// </summary>
		/// <param name="value">store 成员名称。The store member name.</param>
		/// <returns>包装字符串键的状态映射值。A state map value wrapping the string key.</returns>
		[ECMAScriptInline("__arg1")]
		public extern static PiniaStateMapValue<TStore> From(string value);

		/// <summary>
		/// 从选择器回调创建状态映射值。
		/// Creates a state map value from a selector callback.
		/// </summary>
		/// <param name="value">状态选择器回调。The state selector callback.</param>
		/// <returns>包装选择器回调的状态映射值。A state map value wrapping the selector callback.</returns>
		[ECMAScriptInline("__arg1")]
		public extern static PiniaStateMapValue<TStore> From(PiniaMapStateSelector<TStore> value);

		/// <summary>
		/// 从字符串隐式转换为状态映射值。
		/// Implicitly converts a string to a state map value.
		/// </summary>
		/// <param name="value">store 成员名称。The store member name.</param>
		public static implicit operator PiniaStateMapValue<TStore>(string value)
			=> new(value);

		/// <summary>
		/// 从选择器回调隐式转换为状态映射值。
		/// Implicitly converts a selector callback to a state map value.
		/// </summary>
		/// <param name="value">状态选择器回调。The state selector callback.</param>
		public static implicit operator PiniaStateMapValue<TStore>(PiniaMapStateSelector<TStore> value)
			=> new(value);
	}
}
