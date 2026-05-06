using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// A Vue component that declares typed props. The compiler uses this interface
	/// to select the correct <c>h()</c> overload for props-only components.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	public interface IVueComponent<TProps> : ECMAScript.VueContract.IVueComponent
		where TProps : VueProps
	{
	}

	/// <summary>
	/// A Vue component that declares typed slots but no typed props. The compiler uses
	/// this interface to select the correct <c>h()</c> overload for slots-only components.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public interface IVueSlotComponent<TSlots> : ECMAScript.VueContract.IVueComponent
		where TSlots : VueSlots
	{
	}

	/// <summary>
	/// A Vue component that declares both typed props and typed slots. The compiler uses
	/// this interface to select the correct <c>h()</c> overload for components with both.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public interface IVueComponent<TProps, TSlots> : IVueComponent<TProps>, IVueSlotComponent<TSlots>
		where TProps : VueProps
		where TSlots : VueSlots
	{
	}

	/// <summary>
	/// Represents a Vue virtual DOM node (VNode) returned by <c>h()</c>. VNodes are the
	/// building blocks of Vue's render tree and are diffed/patched by the runtime.
	/// </summary>
	public interface IVNode { }

	/// <summary>
	/// A reactive reference wrapper. Reading <c>Value</c> tracks the ref as a reactive
	/// dependency; writing <c>Value</c> triggers any watchers depending on this ref.
	/// </summary>
	/// <typeparam name="T">The type of the wrapped value.</typeparam>
	public interface IVueRef<T>
	{
		/// <summary>
		/// Gets or sets the underlying reactive value. Reads are tracked; writes notify watchers.
		/// </summary>
		[Description("@#value")]
		public T Value { get; set; }
	}

	/// <summary>
	/// Marker interface for option bags that map to plain JavaScript objects in Vue component
	/// options, plugin configuration, and registries.
	/// </summary>
	public interface IVueOptionsBag { }

	/// <summary>
	/// Strongly typed Vue dependency-injection key. At runtime this is still the
	/// JavaScript <see cref="Symbol"/> value supplied by the user; the generic argument
	/// only constrains matching <c>Provide</c> / <c>Inject</c> calls in C#.
	/// </summary>
	/// <typeparam name="TValue">The value contract associated with this injection key.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueInjectionKey<TValue>
	{
		private VueInjectionKey()
		{
		}

		/// <summary>
		/// Treat a JavaScript symbol as a typed Vue injection key. This erases to the
		/// original symbol value at emission time.
		/// </summary>
		/// <param name="key">The JavaScript symbol used as the injection key.</param>
		public extern static implicit operator VueInjectionKey<TValue>(Symbol key);

		/// <summary>
		/// Exposes the underlying JavaScript symbol when an API needs a raw symbol key.
		/// </summary>
		/// <param name="key">The typed Vue injection key.</param>
		public extern static implicit operator Symbol(VueInjectionKey<TValue> key);
	}

	/// <summary>
	/// Base record for component prop declarations. Inherit from this record and declare
	/// properties to define the props a component accepts. Maps to a plain JS object in
	/// Vue's <c>props</c> option.
	/// </summary>
	public abstract record VueProps : IVueOptionsBag;

	/// <summary>
	/// Generic dictionary-style Vue object authoring surface for arbitrary string keys.
	/// This remains a record so it participates in structural object lowering and emits
	/// a plain JavaScript object rather than a runtime <c>Map</c>. String keys emit
	/// normal object members; <see cref="Symbol"/> keys emit computed properties.
	/// </summary>
	/// <typeparam name="TValue">The value contract for each arbitrary key.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueDictionary<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// Gets or sets an arbitrary Vue/object property by its final emitted key.
		/// </summary>
		/// <param name="key">The final JavaScript object key to emit.</param>
		/// <returns>The value mapped to the given key.</returns>
		public extern TValue? this[string key] { get; set; }

		/// <summary>
		/// Gets or sets an arbitrary Vue/object property by a JavaScript symbol key.
		/// The compiler lowers this to a computed object property.
		/// </summary>
		/// <param name="key">The JavaScript symbol used as the property key.</param>
		/// <returns>The value mapped to the given symbol key.</returns>
		public extern TValue? this[Symbol key] { get; set; }

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of string-keyed entries.
		/// The compiler lowers this into a plain object literal property instead of a
		/// runtime <c>Add(...)</c> call.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, TValue value);

		/// <summary>
		/// CLR bridge kept for collection-initializer authoring of symbol-keyed entries.
		/// The compiler lowers this into a computed object literal property instead of a
		/// runtime <c>Add(...)</c> call.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(Symbol key, TValue value);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Generic Vue value contract for dictionary/indexer authoring surfaces.
	/// This is a compile-time wrapper only; implicit conversions erase to the
	/// underlying JavaScript value at emission time.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueValue
	{
		private VueValue()
		{
		}

		public extern static implicit operator VueValue(string value);

		public extern static implicit operator VueValue(bool value);

		public extern static implicit operator VueValue(Number value);

		public extern static implicit operator VueValue(BigInt value);

		public extern static implicit operator VueValue(char value);

		public extern static implicit operator VueValue(double value);

		public extern static implicit operator VueValue(float value);

		public extern static implicit operator VueValue(int value);

		public extern static implicit operator VueValue(long value);

		public extern static implicit operator VueValue(short value);

		public extern static implicit operator VueValue(ushort value);

		public extern static implicit operator VueValue(byte value);

		public extern static implicit operator VueValue(sbyte value);

		public extern static implicit operator VueValue(uint value);

		public extern static implicit operator VueValue(ulong value);

		public extern static implicit operator VueValue(decimal value);

		public extern static implicit operator VueValue(Action value);

		public extern static implicit operator VueValue(VueProps value);

		public extern static implicit operator VueValue(VueValue[] value);
	}

	/// <summary>
	/// Canonical child value contract for <c>h(...)</c> overloads.
	/// This preserves JS-facing flexibility (VNode / text / number / boolean / VNode array)
	/// while keeping the C# public surface compact and stable.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueChild
	{
		private VueChild()
		{
		}

		public extern static implicit operator VueChild(string value);

		public extern static implicit operator VueChild(Number value);

		public extern static implicit operator VueChild(byte value);

		public extern static implicit operator VueChild(sbyte value);

		public extern static implicit operator VueChild(short value);

		public extern static implicit operator VueChild(ushort value);

		public extern static implicit operator VueChild(int value);

		public extern static implicit operator VueChild(uint value);

		public extern static implicit operator VueChild(long value);

		public extern static implicit operator VueChild(ulong value);

		public extern static implicit operator VueChild(float value);

		public extern static implicit operator VueChild(double value);

		public extern static implicit operator VueChild(decimal value);

		public extern static implicit operator VueChild(bool value);

		public extern static implicit operator VueChild(IVNode[] value);
	}

	/// <summary>
	/// Vue VNode key contract. Vue accepts string, number, and symbol keys; this wrapper
	/// keeps that union strongly typed while allowing natural C# assignments without
	/// relying on chained implicit conversions through <see cref="Number"/>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueKey
	{
		private VueKey()
		{
		}

		public extern static implicit operator VueKey(string value);

		public extern static implicit operator VueKey(Symbol value);

		public extern static implicit operator VueKey(Number value);

		public extern static implicit operator VueKey(byte value);

		public extern static implicit operator VueKey(sbyte value);

		public extern static implicit operator VueKey(short value);

		public extern static implicit operator VueKey(ushort value);

		public extern static implicit operator VueKey(int value);

		public extern static implicit operator VueKey(uint value);

		public extern static implicit operator VueKey(long value);

		public extern static implicit operator VueKey(ulong value);

		public extern static implicit operator VueKey(float value);

		public extern static implicit operator VueKey(double value);

		public extern static implicit operator VueKey(decimal value);
	}

	/// <summary>
	/// JavaScript constructor values accepted by Vue prop declarations.
	/// These properties emit the raw constructor identifiers such as <c>String</c>,
	/// <c>Number</c>, and <c>Boolean</c>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class VuePropType
	{
		private VuePropType()
		{
		}

		[Description("@#String")]
		public extern static VuePropType String { get; }

		[Description("@#Number")]
		public extern static VuePropType Number { get; }

		[Description("@#Boolean")]
		public extern static VuePropType Boolean { get; }

		[Description("@#Array")]
		public extern static VuePropType Array { get; }

		[Description("@#Object")]
		public extern static VuePropType Object { get; }

		[Description("@#Date")]
		public extern static VuePropType Date { get; }

		[Description("@#Function")]
		public extern static VuePropType Function { get; }

		[Description("@#Symbol")]
		public extern static VuePropType Symbol { get; }

		[Description("@#Error")]
		public extern static VuePropType Error { get; }
	}

	/// <summary>
	/// Convenience non-generic dictionary surface for common Vue object authoring.
	/// This is the direct default when the value contract is the general <see cref="VueValue"/>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueDictionary : VueDictionary<VueValue>
	{
	}

}
