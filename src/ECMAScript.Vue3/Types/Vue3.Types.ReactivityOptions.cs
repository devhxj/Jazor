using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// Options shared by <c>watchEffect()</c>, <c>watchPostEffect()</c>, and
	/// <c>watchSyncEffect()</c>. Maps directly to Vue's plain options object.
	/// </summary>
	public record VueWatchEffectOptions : IVueOptionsBag
	{
		/// <summary>
		/// Controls when the watcher callback is flushed relative to component rendering.
		/// </summary>
		[Description("@#flush")]
		public VueWatchFlush? Flush { get; init; }

		/// <summary>
		/// Debug callback invoked when reactive dependencies are tracked.
		/// </summary>
		[Description("@#onTrack")]
		public VueDebuggerCallback? OnTrack { get; init; }

		/// <summary>
		/// Debug callback invoked when a tracked dependency triggers the watcher.
		/// </summary>
		[Description("@#onTrigger")]
		public VueDebuggerCallback? OnTrigger { get; init; }
	}

	/// <summary>
	/// Options for <c>watch()</c>. This extends effect options with source-specific
	/// behavior such as eager execution, deep traversal, and one-shot watches.
	/// </summary>
	public record VueWatchOptions : VueWatchEffectOptions
	{
		/// <summary>
		/// Run the callback immediately with the current value.
		/// </summary>
		[Description("@#immediate")]
		public bool? Immediate { get; init; }

		/// <summary>
		/// Traverse nested properties. Use <c>true</c> for full traversal or an integer
		/// depth limit when only a bounded traversal is needed.
		/// </summary>
		[Description("@#deep")]
		public Either<bool, int>? Deep { get; init; }

		/// <summary>
		/// Stop the watcher automatically after the first callback run.
		/// </summary>
		[Description("@#once")]
		public bool? Once { get; init; }
	}

	/// <summary>
	/// Options API watch declaration whose handler is a strongly typed callback.
	/// </summary>
	/// <typeparam name="T">The watched value type.</typeparam>
	public record VueWatchHandlerOptions<T> : VueWatchOptions
	{
		/// <summary>
		/// Callback invoked with the current and previous values.
		/// </summary>
		[Description("@#handler")]
		public Action<T, T> Handler { get; init; } = default!;
	}

	/// <summary>
	/// Options API watch declaration whose handler receives Vue's cleanup registration
	/// callback in addition to the current and previous values.
	/// </summary>
	/// <typeparam name="T">The watched value type.</typeparam>
	public record VueWatchCleanupHandlerOptions<T> : VueWatchOptions
	{
		/// <summary>
		/// Cleanup-aware callback invoked with the current value, previous value, and cleanup registration.
		/// </summary>
		[Description("@#handler")]
		public VueWatchCleanupCallback<T> Handler { get; init; } = default!;
	}

	/// <summary>
	/// Options API watch declaration whose handler is resolved by Vue from the component
	/// <c>methods</c> object.
	/// </summary>
	public record VueWatchNamedHandlerOptions : VueWatchOptions
	{
		/// <summary>
		/// Method name to resolve from the same component's <c>methods</c> option.
		/// </summary>
		[Description("@#handler")]
		public string Handler { get; init; } = default!;
	}

	/// <summary>
	/// Options for <c>useModel()</c>. Vue applies these transforms when reading from
	/// and writing to the model ref.
	/// </summary>
	/// <typeparam name="T">The model value type.</typeparam>
	public record VueModelOptions<T> : IVueOptionsBag
	{
		/// <summary>
		/// Transform the prop value when reading the model ref.
		/// </summary>
		[Description("@#get")]
		public Func<T, T>? Get { get; init; }

		/// <summary>
		/// Transform the assigned value before Vue emits the update event.
		/// </summary>
	[Description("@#set")]
	public Func<T, T>? Set { get; init; }
	}

	/// <summary>
	/// Strongly typed named-model contract used to keep <c>useModel()</c>, prop-name
	/// declarations, and <c>update:*</c> event names aligned without repeating raw
	/// string literals. At runtime this still erases to the final prop key string.
	/// </summary>
	/// <typeparam name="TProps">The typed props contract associated with this model.</typeparam>
	/// <typeparam name="TValue">The model value type.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueModelName<TProps, TValue>
		where TProps : VueProps
	{
		private VueModelName()
		{
		}

		/// <summary>
		/// Treat a final runtime prop key string as a typed model-name contract.
		/// </summary>
		/// <param name="key">The final runtime prop key, such as <c>"modelValue"</c> or <c>"count"</c>.</param>
		public extern static implicit operator VueModelName<TProps, TValue>(string key);

		/// <summary>
		/// Exposes the final runtime prop key string when an API expects a raw string.
		/// </summary>
		/// <param name="model">The typed model-name contract.</param>
		public extern static implicit operator string(VueModelName<TProps, TValue> model);
	}

	/// <summary>
	/// Read-side bag of model modifiers returned by Vue's <c>useModel()</c> tuple-like
	/// result. Modifier keys can be accessed through the string indexer or projected to a
	/// stronger typed subclass when a component defines custom modifiers.
	/// </summary>
	public abstract class VueModelModifiers
	{
		protected VueModelModifiers()
		{
		}

		/// <summary>
		/// Reads an arbitrary modifier flag by its final modifier key.
		/// </summary>
		/// <param name="key">The modifier key, for example <c>"trim"</c>.</param>
		/// <returns><c>true</c> when the modifier is present; otherwise <c>null</c> / <c>undefined</c>.</returns>
		public extern bool? this[string key] { get; }

		/// <summary>
		/// Reads Vue's built-in <c>.trim</c> model modifier.
		/// </summary>
		[Description("@#trim")]
		public extern bool? Trim { get; }

		/// <summary>
		/// Reads Vue's built-in <c>.number</c> model modifier.
		/// </summary>
		[Description("@#number")]
		public extern bool? Number { get; }

		/// <summary>
		/// Reads Vue's built-in <c>.lazy</c> model modifier.
		/// </summary>
		[Description("@#lazy")]
		public extern bool? Lazy { get; }
	}

	/// <summary>
	/// Result type returned by <c>useModel()</c>. Vue exposes a writable ref that also
	/// carries the tuple-like model-modifiers projection. This host surface keeps normal
	/// ref authoring on <see cref="IVueRef{T}.Value"/> while exposing modifiers through an
	/// inline helper instead of compiler special casing.
	/// </summary>
	/// <typeparam name="TValue">The model value type.</typeparam>
	public abstract class VueModelRef<TValue> : IVueRef<TValue>
	{
		protected VueModelRef()
		{
		}

		/// <summary>
		/// Gets or sets the current model value.
		/// </summary>
		[Description("@#value")]
		public extern TValue Value { get; set; }

		/// <summary>
		/// Reads the current model modifiers bag from Vue's tuple-like <c>useModel()</c>
		/// result.
		/// </summary>
		/// <returns>The raw model modifiers bag.</returns>
		[ECMAScriptInline("__arg1[1]")]
		public extern VueModelModifiers GetModifiers();

		/// <summary>
		/// Reads the current model modifiers bag projected to a stronger typed modifier
		/// subclass.
		/// </summary>
		/// <typeparam name="TModifiers">The typed modifier projection.</typeparam>
		/// <returns>The modifiers bag projected as <typeparamref name="TModifiers"/>.</returns>
		[ECMAScriptInline("__arg1[1]")]
		public extern TModifiers GetModifiers<TModifiers>()
			where TModifiers : VueModelModifiers;
	}

	/// <summary>
	/// Writable computed options. Vue expects a plain object with <c>get</c> and
	/// <c>set</c> members; C# exposes those as strongly typed delegates.
	/// </summary>
	/// <typeparam name="T">The computed value type.</typeparam>
	public record VueWritableComputedOptions<T> : IVueOptionsBag
	{
		/// <summary>
		/// Getter used by Vue to compute the current value.
		/// </summary>
		[Description("@#get")]
		public Func<T> Get { get; init; } = default!;

		/// <summary>
		/// Setter invoked when the computed ref is assigned.
		/// </summary>
		[Description("@#set")]
		public Action<T> Set { get; init; } = default!;
	}

	/// <summary>
	/// Get/set handlers returned by a <c>customRef()</c> factory.
	/// </summary>
	/// <typeparam name="T">The custom ref value type.</typeparam>
	public record VueCustomRefHandlers<T> : IVueOptionsBag
	{
		/// <summary>
		/// Getter used by Vue when the custom ref's <c>value</c> is read.
		/// </summary>
		[Description("@#get")]
		public Func<T> Get { get; init; } = default!;

		/// <summary>
		/// Setter used by Vue when the custom ref's <c>value</c> is assigned.
		/// </summary>
		[Description("@#set")]
		public Action<T> Set { get; init; } = default!;
	}

	/// <summary>
	/// Runtime effect scope returned by <c>effectScope()</c>. Effects created while a
	/// scope is active can be stopped together through the scope.
	/// </summary>
	public abstract class VueEffectScope
	{
		protected VueEffectScope()
		{
		}

		/// <summary>
		/// Run a callback inside this effect scope.
		/// </summary>
		/// <typeparam name="TResult">The callback return type.</typeparam>
		/// <param name="callback">The callback to execute while this scope is active.</param>
		/// <returns>The callback result.</returns>
		[Description("@#run")]
		public extern TResult Run<TResult>(Func<TResult> callback);

		/// <summary>
		/// Stop every effect captured by this scope.
		/// </summary>
		[Description("@#stop")]
		public extern void Stop();
	}

	/// <summary>
	/// Represents the public instance of a mounted Vue component. Obtained from
	/// <see cref="VueApp.Mount(string)"/> and used for testing or programmatic access
	/// to the component's public properties exposed via <c>expose()</c>.
	/// </summary>
	public sealed class VueComponentPublicInstance
	{
		private VueComponentPublicInstance()
		{
		}
	}

	/// <summary>
	/// Setup context available inside the <c>setup()</c> function. Provides access to
	/// fallthrough attributes, slots, event emission, and public instance exposure.
	/// </summary>
	public abstract class VueSetupContext
	{
		/// <summary>
		/// Fallthrough attributes passed to the component but not declared as props.
		/// Includes <c>class</c>, <c>style</c>, and event listeners when <c>inheritAttrs</c> is <c>true</c>.
		/// </summary>
		[Description("@#attrs")]
		public extern VueAttributeBag Attrs { get; }

		/// <summary>
		/// Slots available in the component. Use this to render default or named slot content
		/// via <c>context.slots.default?.()</c>.
		/// </summary>
		[Description("@#slots")]
		public extern VueSlotBag Slots { get; }

		/// <summary>
		/// Emit a custom event by name with no payload. The parent component can listen
		/// via <c>v-on:eventName</c> or <c>@eventName</c>.
		/// </summary>
		/// <param name="eventName">The name of the event to emit (e.g. <c>"close"</c>).</param>
		[Description("@#emit")]
		public extern void Emit(string eventName);

		/// <summary>
		/// Emit a custom event by name with a single typed payload value.
		/// </summary>
		/// <typeparam name="TValue">The type of the event payload.</typeparam>
		/// <param name="eventName">The name of the event to emit (e.g. <c>"update:modelValue"</c>).</param>
		/// <param name="value">The payload value sent with the event.</param>
		[Description("@#emit")]
		public extern void Emit<TValue>(string eventName, TValue value);

		/// <summary>
		/// Emit the <c>update:*</c> event corresponding to a typed model-name contract.
		/// This keeps named-model update emits aligned with the same contract used by
		/// <c>useModel()</c> and runtime prop declarations.
		/// </summary>
		/// <typeparam name="TProps">The typed props contract associated with this model.</typeparam>
		/// <typeparam name="TValue">The emitted model value type.</typeparam>
		/// <param name="model">The typed model-name contract.</param>
		/// <param name="value">The payload value sent with the corresponding <c>update:*</c> event.</param>
		[ECMAScriptInline("__arg1.emit(`update:${__arg2}`, __arg3)")]
		public extern void Emit<TProps, TValue>(VueModelName<TProps, TValue> model, TValue value)
			where TProps : VueProps;

		/// <summary>
		/// Emit a custom event by name with two typed payload values.
		/// </summary>
		/// <typeparam name="T0">The type of the first payload value.</typeparam>
		/// <typeparam name="T1">The type of the second payload value.</typeparam>
		/// <param name="eventName">The name of the event to emit (e.g. <c>"update"</c>).</param>
		/// <param name="value0">The first payload value sent with the event.</param>
		/// <param name="value1">The second payload value sent with the event.</param>
		[Description("@#emit")]
		public extern void Emit<T0, T1>(string eventName, T0 value0, T1 value1);

		/// <summary>
		/// Emit a custom event by name with three typed payload values.
		/// </summary>
		/// <typeparam name="T0">The type of the first payload value.</typeparam>
		/// <typeparam name="T1">The type of the second payload value.</typeparam>
		/// <typeparam name="T2">The type of the third payload value.</typeparam>
		/// <param name="eventName">The name of the event to emit.</param>
		/// <param name="value0">The first payload value sent with the event.</param>
		/// <param name="value1">The second payload value sent with the event.</param>
		/// <param name="value2">The third payload value sent with the event.</param>
		[Description("@#emit")]
		public extern void Emit<T0, T1, T2>(string eventName, T0 value0, T1 value1, T2 value2);

		/// <summary>
		/// Emit a custom event by name with four typed payload values.
		/// </summary>
		/// <typeparam name="T0">The type of the first payload value.</typeparam>
		/// <typeparam name="T1">The type of the second payload value.</typeparam>
		/// <typeparam name="T2">The type of the third payload value.</typeparam>
		/// <typeparam name="T3">The type of the fourth payload value.</typeparam>
		/// <param name="eventName">The name of the event to emit.</param>
		/// <param name="value0">The first payload value sent with the event.</param>
		/// <param name="value1">The second payload value sent with the event.</param>
		/// <param name="value2">The third payload value sent with the event.</param>
		/// <param name="value3">The fourth payload value sent with the event.</param>
		[Description("@#emit")]
		public extern void Emit<T0, T1, T2, T3>(string eventName, T0 value0, T1 value1, T2 value2, T3 value3);

		/// <summary>
		/// Expose a value on the component's public instance so parent components can
		/// access it via template refs (<c>ref="..."</c>). Only exposed values are
		/// accessible from the parent; all other internal state is hidden.
		/// </summary>
		/// <typeparam name="TValue">The type of the exposed value (must be a reference type).</typeparam>
		/// <param name="exposed">The object or value to expose on the public instance.</param>
		[Description("@#expose")]
		public extern void Expose<TValue>(TValue exposed) where TValue : class;
	}

	/// <summary>
	/// Typed setup context that provides typed slot access in addition to the standard
	/// <see cref="VueSetupContext"/> members. The <c>Slots</c> property returns the
	/// typed <typeparamref name="TSlots"/> record instead of the untyped <see cref="VueSlotBag"/>.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type declared by the component.</typeparam>
	public abstract class VueSetupContext<TSlots> : VueSetupContext
		where TSlots : VueSlots
	{
		/// <summary>
		/// Typed slots available in the component. Each property on <typeparamref name="TSlots"/>
		/// maps to a named slot that can be invoked to produce its VNode content.
		/// </summary>
		[Description("@#slots")]
		public new extern TSlots Slots { get; }
	}

	/// <summary>
	/// Bag of fallthrough attributes (<c>v-bind="$attrs"</c>). Contains attributes
	/// passed to the component that are not declared as props, including <c>class</c>,
	/// <c>style</c>, and event listeners.
	/// </summary>
	public abstract class VueAttributeBag
	{
		protected VueAttributeBag()
		{
		}

		/// <summary>
		/// Reads an arbitrary fallthrough attribute by its final emitted key.
		/// </summary>
		/// <param name="key">The final JavaScript attribute key.</param>
		/// <returns>The attribute value when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
		public extern VueValue? this[string key] { get; }

		/// <summary>
		/// Reads the fallthrough <c>class</c> binding.
		/// </summary>
		[Description("@#class")]
		public extern Either<string, string[], VueProps, VueValue[]>? Class { get; }

		/// <summary>
		/// Reads the fallthrough <c>style</c> binding.
		/// </summary>
		[Description("@#style")]
		public extern VueProps? Style { get; }

		/// <summary>
		/// Reads the fallthrough <c>id</c> attribute.
		/// </summary>
		[Description("@#id")]
		public extern string? Id { get; }

		/// <summary>
		/// Reads the fallthrough <c>title</c> attribute.
		/// </summary>
	[Description("@#title")]
	public extern string? Title { get; }

	/// <summary>
	/// Reads the fallthrough <c>for</c> attribute.
	/// </summary>
	[Description("@#for")]
	public extern string? For { get; }

	/// <summary>
	/// Reads the fallthrough <c>name</c> attribute.
	/// </summary>
	[Description("@#name")]
	public extern string? Name { get; }

	/// <summary>
	/// Reads the fallthrough <c>type</c> attribute.
	/// </summary>
	[Description("@#type")]
	public extern string? Type { get; }

	/// <summary>
	/// Reads the fallthrough <c>placeholder</c> attribute.
	/// </summary>
	[Description("@#placeholder")]
	public extern string? Placeholder { get; }

	/// <summary>
	/// Reads the fallthrough <c>disabled</c> attribute.
	/// </summary>
	[Description("@#disabled")]
	public extern bool? Disabled { get; }

	/// <summary>
	/// Reads the fallthrough <c>readonly</c> attribute.
	/// </summary>
	[Description("@#readonly")]
	public extern bool? Readonly { get; }

	/// <summary>
	/// Reads the fallthrough <c>required</c> attribute.
	/// </summary>
	[Description("@#required")]
	public extern bool? Required { get; }

	/// <summary>
	/// Reads the fallthrough <c>tabindex</c> attribute.
	/// </summary>
	[Description("@#tabindex")]
	public extern int? Tabindex { get; }

	/// <summary>
	/// Reads the fallthrough <c>role</c> attribute.
	/// </summary>
	[Description("@#role")]
	public extern string? Role { get; }
	}

	/// <summary>
	/// Bag of available slots (<c>$slots</c>). Each property is a callable slot
	/// function that returns VNode content.
	/// </summary>
	public abstract class VueSlotBag
	{
		protected VueSlotBag()
		{
		}

		/// <summary>
		/// Reads an arbitrary slot callback by its final slot name.
		/// </summary>
		/// <param name="key">The final Vue slot key.</param>
		/// <returns>The slot callback when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
		public extern VueSlotCallback? this[string key] { get; }

		/// <summary>
		/// Reads the default slot callback when present.
		/// </summary>
		[Description("@#default")]
		public extern VueSlotCallback? Default { get; }
	}

	/// <summary>
	/// Bag of directive modifiers. Each key corresponds to a modifier name used at the
	/// directive call site, for example <c>v-colorize.primary</c> exposing
	/// <c>binding.modifiers["primary"]</c>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveModifiers
	{
		protected VueDirectiveModifiers()
		{
		}

		/// <summary>
		/// Returns whether the given modifier flag is present on the current directive usage.
		/// </summary>
		/// <param name="key">The modifier name to check.</param>
		/// <returns><c>true</c> when the modifier is present; otherwise <c>false</c>.</returns>
		public extern bool this[string key] { get; }
	}

}
