using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// Write-side modifier object used in <c>withDirectives()</c> directive argument
	/// tuples. Keys are final modifier names and values indicate whether the modifier is
	/// present.
	/// </summary>
	public record VueDirectiveModifierBag : VueDictionary<bool>;

	/// <summary>
	/// One directive argument tuple accepted by Vue's <c>withDirectives()</c> helper.
	/// This maps to JavaScript <c>Array</c> so the emitted runtime shape is compatible
	/// with Vue's <c>[directive, value, argument, modifiers]</c> tuple contract.
	/// </summary>
	[ECMAScript]
	[Description("@#Array")]
	public class VueDirectiveArguments
	{
		protected VueDirectiveArguments()
		{
		}

		/// <summary>
		/// Applies a directive with no explicit value, argument, or modifiers.
		/// </summary>
		/// <param name="directive">The directive definition or function shorthand.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive);

		/// <summary>
		/// Applies a directive with a value.
		/// </summary>
		/// <param name="directive">The directive definition or function shorthand.</param>
		/// <param name="value">The directive value.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive, VueValue? value);

		/// <summary>
		/// Applies a directive with a value and argument.
		/// </summary>
		/// <param name="directive">The directive definition or function shorthand.</param>
		/// <param name="value">The directive value.</param>
		/// <param name="arg">The directive argument.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive, VueValue? value, string arg);

		/// <summary>
		/// Applies a directive with the full Vue tuple shape.
		/// </summary>
		/// <param name="directive">The directive definition or function shorthand.</param>
		/// <param name="value">The directive value.</param>
		/// <param name="arg">The directive argument. Use <c>null</c> when only modifiers are needed.</param>
		/// <param name="modifiers">The directive modifier flags.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive, VueValue? value, string? arg, VueDirectiveModifierBag modifiers);
	}

	/// <summary>
	/// Strongly typed directive argument tuple. The generic argument keeps the supplied
	/// value aligned with the typed directive definition while preserving Vue's runtime
	/// array tuple shape.
	/// </summary>
	/// <typeparam name="TValue">The directive value contract.</typeparam>
	[ECMAScript]
	[Description("@#Array")]
	public sealed class VueDirectiveArguments<TValue> : VueDirectiveArguments
	{
		/// <summary>
		/// Applies a typed directive with a typed value.
		/// </summary>
		/// <param name="directive">The typed directive definition.</param>
		/// <param name="value">The directive value.</param>
		public extern VueDirectiveArguments(VueDirective<TValue> directive, TValue value);

		/// <summary>
		/// Applies a typed directive with a typed value and argument.
		/// </summary>
		/// <param name="directive">The typed directive definition.</param>
		/// <param name="value">The directive value.</param>
		/// <param name="arg">The directive argument.</param>
		public extern VueDirectiveArguments(VueDirective<TValue> directive, TValue value, string arg);

		/// <summary>
		/// Applies a typed directive with the full Vue tuple shape.
		/// </summary>
		/// <param name="directive">The typed directive definition.</param>
		/// <param name="value">The directive value.</param>
		/// <param name="arg">The directive argument. Use <c>null</c> when only modifiers are needed.</param>
		/// <param name="modifiers">The directive modifier flags.</param>
		public extern VueDirectiveArguments(VueDirective<TValue> directive, TValue value, string? arg, VueDirectiveModifierBag modifiers);
	}

	/// <summary>
	/// Current runtime binding payload for a directive lifecycle hook.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveBinding
	{
		protected VueDirectiveBinding()
		{
		}

		/// <summary>
		/// Current directive value passed by the user. For richer non-primitive contracts,
		/// prefer the generic <see cref="VueDirectiveBinding{TValue}"/>.
		/// </summary>
		[Description("@#value")]
		public extern VueValue Value { get; }

		/// <summary>
		/// Dynamic argument segment provided to the directive, such as <c>focus</c> in
		/// <c>v-demo:focus</c>.
		/// </summary>
		[Description("@#arg")]
		public extern string? Arg { get; }

		/// <summary>
		/// Modifier flags provided to the directive call site.
		/// </summary>
		[Description("@#modifiers")]
		public extern VueDirectiveModifiers Modifiers { get; }

		/// <summary>
		/// Component public instance that owns the directive usage, when available.
		/// </summary>
		[Description("@#instance")]
		public extern VueComponentPublicInstance? Instance { get; }

		/// <summary>
		/// The directive definition currently being invoked.
		/// </summary>
		[Description("@#dir")]
		public extern VueDirective Dir { get; }
	}

	/// <summary>
	/// Current runtime binding payload for a directive lifecycle hook with a strongly typed value contract.
	/// </summary>
	/// <typeparam name="TValue">The typed contract of the directive's current binding value.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveBinding<TValue> : VueDirectiveBinding
	{
		protected VueDirectiveBinding()
		{
		}

		/// <summary>
		/// Current directive value passed by the user.
		/// </summary>
		[Description("@#value")]
		public new extern TValue Value { get; }

		/// <summary>
		/// The typed directive definition currently being invoked.
		/// </summary>
		[Description("@#dir")]
		public new extern VueDirective<TValue> Dir { get; }
	}

	/// <summary>
	/// Runtime binding payload for a directive update hook, including the previous value.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveUpdateBinding : VueDirectiveBinding
	{
		protected VueDirectiveUpdateBinding()
		{
		}

		/// <summary>
		/// Previous directive value observed on the same element during the preceding update cycle.
		/// </summary>
		[Description("@#oldValue")]
		public extern VueValue OldValue { get; }
	}

	/// <summary>
	/// Typed runtime binding payload for a directive update hook, including the previous value.
	/// </summary>
	/// <typeparam name="TValue">The typed contract of the directive's current and previous binding values.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveUpdateBinding<TValue> : VueDirectiveBinding<TValue>
	{
		protected VueDirectiveUpdateBinding()
		{
		}

		/// <summary>
		/// Previous directive value observed on the same element during the preceding update cycle.
		/// </summary>
		[Description("@#oldValue")]
		public extern TValue OldValue { get; }
	}

	/// <summary>
	/// Union-like directive value contract used at registration and retrieval boundaries
	/// where Vue accepts either an object-form directive or a function shorthand.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public class VueDirectiveValue
	{
		protected VueDirectiveValue()
		{
		}

		public extern static implicit operator VueDirectiveValue(VueDirective value);

		public extern static implicit operator VueDirectiveValue(VueDirectiveFunction value);
	}

	/// <summary>
	/// Direct object-form Vue directive authoring surface. This maps to a plain JavaScript
	/// directive object whose lifecycle hooks are invoked by Vue.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueDirective : IVueOptionsBag
	{
		/// <summary>
		/// Marks the directive as deep, so Vue traverses nested values for change detection.
		/// </summary>
		[Description("@#deep")]
		public bool? Deep { get; init; }

		/// <summary>
		/// Called before any attributes or listeners are applied to the element.
		/// </summary>
		[Description("@#created")]
		public VueDirectiveHook? Created { get; init; }

		/// <summary>
		/// Called right before the element is inserted into the DOM.
		/// </summary>
		[Description("@#beforeMount")]
		public VueDirectiveHook? BeforeMount { get; init; }

		/// <summary>
		/// Called after the element is inserted into the DOM.
		/// </summary>
		[Description("@#mounted")]
		public VueDirectiveHook? Mounted { get; init; }

		/// <summary>
		/// Called right before the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#beforeUpdate")]
		public VueDirectiveUpdateHook? BeforeUpdate { get; init; }

		/// <summary>
		/// Called after the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#updated")]
		public VueDirectiveUpdateHook? Updated { get; init; }

		/// <summary>
		/// Called right before the containing component unmounts the element.
		/// </summary>
		[Description("@#beforeUnmount")]
		public VueDirectiveHook? BeforeUnmount { get; init; }

		/// <summary>
		/// Called after the containing component unmounts the element.
		/// </summary>
		[Description("@#unmounted")]
		public VueDirectiveHook? Unmounted { get; init; }

		/// <summary>
		/// Called during SSR to contribute additional props to the rendered element.
		/// </summary>
		[Description("@#getSSRProps")]
		public VueDirectiveSsrPropsCallback? GetSsrProps { get; init; }
	}

	/// <summary>
	/// Typed object-form Vue directive authoring surface. This keeps the directive's binding
	/// value strongly typed while still lowering to the same plain JavaScript directive object.
	/// </summary>
	/// <typeparam name="TValue">The typed contract of the directive's binding value.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueDirective<TValue> : VueDirective
	{
		/// <summary>
		/// Called before any attributes or listeners are applied to the element.
		/// </summary>
		[Description("@#created")]
		public new VueDirectiveHook<TValue>? Created { get; init; }

		/// <summary>
		/// Called right before the element is inserted into the DOM.
		/// </summary>
		[Description("@#beforeMount")]
		public new VueDirectiveHook<TValue>? BeforeMount { get; init; }

		/// <summary>
		/// Called after the element is inserted into the DOM.
		/// </summary>
		[Description("@#mounted")]
		public new VueDirectiveHook<TValue>? Mounted { get; init; }

		/// <summary>
		/// Called right before the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#beforeUpdate")]
		public new VueDirectiveUpdateHook<TValue>? BeforeUpdate { get; init; }

		/// <summary>
		/// Called after the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#updated")]
		public new VueDirectiveUpdateHook<TValue>? Updated { get; init; }

		/// <summary>
		/// Called right before the containing component unmounts the element.
		/// </summary>
		[Description("@#beforeUnmount")]
		public new VueDirectiveHook<TValue>? BeforeUnmount { get; init; }

		/// <summary>
		/// Called after the containing component unmounts the element.
		/// </summary>
		[Description("@#unmounted")]
		public new VueDirectiveHook<TValue>? Unmounted { get; init; }

		/// <summary>
		/// Called during SSR to contribute additional props to the rendered element.
		/// </summary>
		[Description("@#getSSRProps")]
		public new VueDirectiveSsrPropsCallback<TValue>? GetSsrProps { get; init; }
	}

}
