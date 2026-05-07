using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// Transition implementation type used by Vue's <c>Transition</c> and
	/// <c>TransitionGroup</c> built-in components.
	/// </summary>
	[String]
	public enum VueTransitionType
	{
		[Description("@#transition")]
		Transition,

		[Description("@#animation")]
		Animation
	}

	/// <summary>
	/// Transition sequencing mode for the <c>Transition</c> built-in component.
	/// </summary>
	[String]
	public enum VueTransitionMode
	{
		[Description("@#in-out")]
		InOut,

		[Description("@#out-in")]
		OutIn
	}

	/// <summary>
	/// Object-form transition duration for entering and leaving phases.
	/// </summary>
	public record VueTransitionDuration : VueProps
	{
		[Description("@#enter")]
		public Number? Enter { get; init; }

		[Description("@#leave")]
		public Number? Leave { get; init; }
	}

	/// <summary>
	/// Transition lifecycle hook receiving the transitioning element.
	/// </summary>
	/// <param name="element">The element currently entering or leaving.</param>
	public delegate void VueTransitionHook(Element element);

	/// <summary>
	/// Transition lifecycle hook that can explicitly complete async transitions.
	/// </summary>
	/// <param name="element">The element currently entering or leaving.</param>
	/// <param name="done">Callback to invoke when the transition phase has completed.</param>
	public delegate void VueTransitionDoneHook(Element element, Action done);

	/// <summary>
	/// Props for Vue's built-in <c>Transition</c> component.
	/// </summary>
	public record VueTransitionProps : VueProps
	{
		[Description("@#name")]
		public string? Name { get; init; }

		[Description("@#css")]
		public bool? Css { get; init; }

		[Description("@#type")]
		public VueTransitionType? Type { get; init; }

		[Description("@#duration")]
		public VueTransitionDurationValue? Duration { get; init; }

		[Description("@#mode")]
		public VueTransitionMode? Mode { get; init; }

		[Description("@#appear")]
		public bool? Appear { get; init; }

		[Description("@#enterFromClass")]
		public string? EnterFromClass { get; init; }

		[Description("@#enterActiveClass")]
		public string? EnterActiveClass { get; init; }

		[Description("@#enterToClass")]
		public string? EnterToClass { get; init; }

		[Description("@#appearFromClass")]
		public string? AppearFromClass { get; init; }

		[Description("@#appearActiveClass")]
		public string? AppearActiveClass { get; init; }

		[Description("@#appearToClass")]
		public string? AppearToClass { get; init; }

		[Description("@#leaveFromClass")]
		public string? LeaveFromClass { get; init; }

		[Description("@#leaveActiveClass")]
		public string? LeaveActiveClass { get; init; }

		[Description("@#leaveToClass")]
		public string? LeaveToClass { get; init; }

		[Description("@#onBeforeEnter")]
		public VueTransitionHook? OnBeforeEnter { get; init; }

		[Description("@#onEnter")]
		public VueTransitionDoneHook? OnEnter { get; init; }

		[Description("@#onAfterEnter")]
		public VueTransitionHook? OnAfterEnter { get; init; }

		[Description("@#onEnterCancelled")]
		public VueTransitionHook? OnEnterCancelled { get; init; }

		[Description("@#onBeforeLeave")]
		public VueTransitionHook? OnBeforeLeave { get; init; }

		[Description("@#onLeave")]
		public VueTransitionDoneHook? OnLeave { get; init; }

		[Description("@#onAfterLeave")]
		public VueTransitionHook? OnAfterLeave { get; init; }

		[Description("@#onLeaveCancelled")]
		public VueTransitionHook? OnLeaveCancelled { get; init; }

		[Description("@#onBeforeAppear")]
		public VueTransitionHook? OnBeforeAppear { get; init; }

		[Description("@#onAppear")]
		public VueTransitionDoneHook? OnAppear { get; init; }

		[Description("@#onAfterAppear")]
		public VueTransitionHook? OnAfterAppear { get; init; }

		[Description("@#onAppearCancelled")]
		public VueTransitionHook? OnAppearCancelled { get; init; }
	}

	/// <summary>
	/// Props for Vue's built-in <c>TransitionGroup</c> component.
	/// </summary>
	public record VueTransitionGroupProps : VueProps
	{
		[Description("@#name")]
		public string? Name { get; init; }

		[Description("@#tag")]
		public string? Tag { get; init; }

		[Description("@#moveClass")]
		public string? MoveClass { get; init; }

		[Description("@#css")]
		public bool? Css { get; init; }

		[Description("@#type")]
		public VueTransitionType? Type { get; init; }

		[Description("@#duration")]
		public VueTransitionDurationValue? Duration { get; init; }

		[Description("@#appear")]
		public bool? Appear { get; init; }
	}

	/// <summary>
	/// Props for Vue's built-in <c>KeepAlive</c> component.
	/// </summary>
	public record VueKeepAliveProps : VueProps
	{
		[Description("@#include")]
		public VueKeepAliveMatch? Include { get; init; }

		[Description("@#exclude")]
		public VueKeepAliveMatch? Exclude { get; init; }

		[Description("@#max")]
		public VueIntStringValue? Max { get; init; }
	}

	/// <summary>
	/// Props for Vue's built-in <c>Teleport</c> component.
	/// </summary>
	public record VueTeleportProps : VueProps
	{
		[Description("@#to")]
		public VueTeleportTarget? To { get; init; }

		[Description("@#disabled")]
		public bool? Disabled { get; init; }

		[Description("@#defer")]
		public bool? Defer { get; init; }
	}

	/// <summary>
	/// Props for Vue's built-in <c>Suspense</c> component.
	/// </summary>
	public record VueSuspenseProps : VueProps
	{
		[Description("@#timeout")]
		public Number? Timeout { get; init; }

		[Description("@#onPending")]
		public Action? OnPending { get; init; }

		[Description("@#onResolve")]
		public Action? OnResolve { get; init; }

		[Description("@#onFallback")]
		public Action? OnFallback { get; init; }
	}

	/// <summary>
	/// Slots accepted by Vue's built-in <c>Suspense</c> component.
	/// </summary>
	public record VueSuspenseSlots : VueSlots
	{
		[Description("@#default")]
		public VueSlotCallback? Default { get; init; }

		[Description("@#fallback")]
		public VueSlotCallback? Fallback { get; init; }
	}

	/// <summary>
	/// Application-level uncaught error handler configured through <c>app.config</c>.
	/// Vue's error value is unknown-like, so this uses <see cref="VueValue"/> instead
	/// of exposing <c>object</c> on the public Vue surface.
	/// </summary>
	public delegate void VueAppErrorHandler(VueValue? error, VueComponentPublicInstance? instance, string info);

	/// <summary>
	/// Application-level runtime warning handler configured through <c>app.config</c>.
	/// </summary>
	public delegate void VueAppWarnHandler(string message, VueComponentPublicInstance? instance, string trace);

	/// <summary>
	/// Runtime compiler predicate that marks tags as native custom elements.
	/// </summary>
	public delegate bool VueIsCustomElementCallback(string tag);

	/// <summary>
	/// Merge function for custom Options API option keys.
	/// </summary>
	public delegate VueValue? VueOptionMergeFunction(VueValue? parent, VueValue? child);

	/// <summary>
	/// Runtime compiler whitespace handling mode.
	/// </summary>
	[String]
	public enum VueCompilerWhitespace
	{
		[Description("@#condense")]
		Condense,

		[Description("@#preserve")]
		Preserve
	}

	/// <summary>
	/// Bag of app-level global properties available on every component instance.
	/// </summary>
	public abstract class VueGlobalProperties
	{
		protected VueGlobalProperties()
		{
		}

		/// <summary>
		/// Gets or sets a global property by its final runtime key.
		/// </summary>
		public extern VueValue? this[string key] { get; set; }
	}

	/// <summary>
	/// Bag of app-level custom option merge strategies.
	/// </summary>
	public abstract class VueOptionMergeStrategies
	{
		protected VueOptionMergeStrategies()
		{
		}

		/// <summary>
		/// Gets or sets a merge strategy by custom option name.
		/// </summary>
		public extern VueOptionMergeFunction? this[string key] { get; set; }
	}

	/// <summary>
	/// Runtime compiler options exposed through <c>app.config.compilerOptions</c>.
	/// These only affect apps using Vue's in-browser template compiler.
	/// </summary>
	public abstract class VueAppCompilerOptions
	{
		protected VueAppCompilerOptions()
		{
		}

		[Description("@#isCustomElement")]
		public extern VueIsCustomElementCallback? IsCustomElement { get; set; }

		[Description("@#whitespace")]
		public extern VueCompilerWhitespace? Whitespace { get; set; }

		[Description("@#delimiters")]
		public extern string[]? Delimiters { get; set; }

		[Description("@#comments")]
		public extern bool Comments { get; set; }
	}

	/// <summary>
	/// Vue application configuration exposed by <c>app.config</c>.
	/// </summary>
	public abstract class VueAppConfig
	{
		protected VueAppConfig()
		{
		}

		[Description("@#errorHandler")]
		public extern VueAppErrorHandler? ErrorHandler { get; set; }

		[Description("@#warnHandler")]
		public extern VueAppWarnHandler? WarnHandler { get; set; }

		[Description("@#performance")]
		public extern bool Performance { get; set; }

		[Description("@#compilerOptions")]
		public extern VueAppCompilerOptions CompilerOptions { get; }

		[Description("@#globalProperties")]
		public extern VueGlobalProperties GlobalProperties { get; }

		[Description("@#optionMergeStrategies")]
		public extern VueOptionMergeStrategies OptionMergeStrategies { get; }

		[Description("@#idPrefix")]
		public extern string? IdPrefix { get; set; }

		[Description("@#throwUnhandledErrorInProduction")]
		public extern bool ThrowUnhandledErrorInProduction { get; set; }
	}

	/// <summary>
	/// A readonly reactive reference. Only the <c>value</c> getter is available; writes
	/// are not permitted. Typically created by <see cref="Computed{T}"/> or <c>readonly()</c>.
	/// </summary>
	/// <typeparam name="T">The type of the wrapped value.</typeparam>
	public class VueReadonlyRef<T>
	{
		/// <summary>
		/// Gets the current value. Reads are tracked as reactive dependencies.
		/// </summary>
		[Description("@#value")]
		public extern T Value { get; }
	}

	/// <summary>
	/// A readonly computed ref produced by Vue's <c>computed(getter)</c>. This remains
	/// distinct from the broader <see cref="VueReadonlyRef{T}"/> contract so library
	/// surfaces can preserve official APIs that specifically guarantee computed semantics.
	/// </summary>
	/// <typeparam name="T">The computed value type.</typeparam>
	public abstract class VueComputedRef<T> : VueReadonlyRef<T>
	{
		protected VueComputedRef()
		{
		}
	}

	/// <summary>
	/// A writable computed ref produced by Vue's <c>computed({ get, set })</c> overload.
	/// This remains distinct from generic writable refs so higher-level libraries can
	/// encode official writable-computed contracts without collapsing them into
	/// <see cref="IVueRef{T}"/>.
	/// </summary>
	/// <typeparam name="T">The computed value type.</typeparam>
	public abstract class VueWritableComputedRef<T> : IVueRef<T>
	{
		protected VueWritableComputedRef()
		{
		}

		/// <summary>
		/// Gets or sets the current computed value.
		/// </summary>
		[Description("@#value")]
		public extern T Value { get; set; }
	}

	/// <summary>
	/// A shallow ref produced by Vue's <c>shallowRef()</c>. The outer <c>value</c> slot
	/// is reactive while nested object members are not recursively converted to deep
	/// reactive proxies by Vue.
	/// </summary>
	/// <typeparam name="T">The wrapped value type.</typeparam>
	public abstract class VueShallowRef<T> : IVueRef<T>
	{
		protected VueShallowRef()
		{
		}

		/// <summary>
		/// Gets or sets the wrapped shallow reactive value.
		/// </summary>
		[Description("@#value")]
		public extern T Value { get; set; }
	}

	/// <summary>
	/// Untyped refs object returned by <c>toRefs()</c>. Keys are final runtime property
	/// names and values are linked refs for those properties.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueRefs
	{
		protected VueRefs()
		{
		}

		/// <summary>
		/// Reads a linked ref by runtime property name.
		/// </summary>
		/// <param name="key">The source object's final runtime property name.</param>
		/// <returns>The linked ref when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
		public extern IVueRef<VueValue>? this[string key] { get; }
	}

	/// <summary>
	/// Typed base for user-defined <c>toRefs()</c> projections. Inherit from this type
	/// and declare <c>IVueRef&lt;T&gt;</c> properties to get C# IntelliSense over the
	/// refs object returned by Vue.
	/// </summary>
	/// <typeparam name="TSource">The source reactive object contract.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueRefs<TSource> : VueRefs
		where TSource : class
	{
		protected VueRefs()
		{
		}
	}

	/// <summary>
	/// Flush timing for Vue watcher callbacks.
	/// </summary>
	[String]
	public enum VueWatchFlush
	{
		/// <summary>
		/// Run before component rendering. This is Vue's default watcher flush timing.
		/// </summary>
		[Description("@#pre")]
		Pre,

		/// <summary>
		/// Run after component rendering has flushed.
		/// </summary>
		[Description("@#post")]
		Post,

		/// <summary>
		/// Run synchronously when a dependency changes.
		/// </summary>
		[Description("@#sync")]
		Sync
	}

	/// <summary>
	/// Reactivity debugger event operation kind supplied to watcher debug hooks.
	/// </summary>
	[String]
	public enum VueDebuggerEventType
	{
		[Description("@#get")]
		Get,

		[Description("@#has")]
		Has,

		[Description("@#iterate")]
		Iterate,

		[Description("@#set")]
		Set,

		[Description("@#add")]
		Add,

		[Description("@#delete")]
		Delete,

		[Description("@#clear")]
		Clear
	}

	/// <summary>
	/// Debug information supplied to <c>onTrack</c> and <c>onTrigger</c> watcher
	/// options. Runtime values are unknown-like Vue internals, so the value-bearing
	/// members use <see cref="VueValue"/> instead of <c>object</c>.
	/// </summary>
	public abstract class VueDebuggerEvent
	{
		protected VueDebuggerEvent()
		{
		}

		[Description("@#effect")]
		public extern VueValue? Effect { get; }

		[Description("@#target")]
		public extern VueValue? Target { get; }

		[Description("@#type")]
		public extern VueDebuggerEventType Type { get; }

		[Description("@#key")]
		public extern VueValue? Key { get; }

		[Description("@#newValue")]
		public extern VueValue? NewValue { get; }

		[Description("@#oldValue")]
		public extern VueValue? OldValue { get; }

		[Description("@#oldTarget")]
		public extern VueValue? OldTarget { get; }
	}

}
