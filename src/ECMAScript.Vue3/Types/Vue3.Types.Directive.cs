using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>Vue directive 及 model modifier 的类型化参数形状。</summary>
/// <remarks>modifier bag 是对象字面量 contract，不应生成额外的指令包装协议。</remarks>
public static partial class Vue3
{
	/// <summary>
	/// 在 <c>withDirectives()</c> 指令参数元组中使用的写入侧修饰符对象。键为最终修饰符名称，值指示该修饰符是否存在。
	/// Write-side modifier object used in <c>withDirectives()</c> directive argument
	/// tuples. Keys are final modifier names and values indicate whether the modifier is
	/// present.
	/// </summary>
	public record VueDirectiveModifierBag : VueDictionary<bool>;

	/// <summary>
	/// 组件 props authoring 中使用的写入侧模型修饰符对象。
	/// 与 <see cref="VueModelModifiers"/> 的读取侧抽象包不同，这个记录用于
	/// 声明诸如 <c>modelModifiers</c> 一类组件输入契约。
	/// Write-side model modifier object used on component prop authoring surfaces.
	/// Unlike the read-side <see cref="VueModelModifiers"/> abstraction, this record is
	/// meant for declaring component input contracts such as <c>modelModifiers</c>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueModelModifierBag : VueDictionary<bool>
	{
		[Description("@#trim")]
		public bool? Trim { get; init; }

		[Description("@#number")]
		public bool? Number { get; init; }

		[Description("@#lazy")]
		public bool? Lazy { get; init; }
	}

	/// <summary>
	/// Vue 的 <c>withDirectives()</c> 辅助函数接受的指令参数元组。映射到 JavaScript <c>Array</c>，使发射的运行时形状与 Vue 的 <c>[directive, value, argument, modifiers]</c> 元组契约兼容。
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
		/// 应用不带显式值、参数或修饰符的指令。
		/// Applies a directive with no explicit value, argument, or modifiers.
		/// </summary>
		/// <param name="directive">指令定义或函数简写形式。The directive definition or function shorthand.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive);

		/// <summary>
		/// 应用带值的指令。
		/// Applies a directive with a value.
		/// </summary>
		/// <param name="directive">指令定义或函数简写形式。The directive definition or function shorthand.</param>
		/// <param name="value">指令的值。The directive value.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive, VueValue? value);

		/// <summary>
		/// 应用带值和参数的指令。
		/// Applies a directive with a value and argument.
		/// </summary>
		/// <param name="directive">指令定义或函数简写形式。The directive definition or function shorthand.</param>
		/// <param name="value">指令的值。The directive value.</param>
		/// <param name="arg">指令参数。The directive argument.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive, VueValue? value, string arg);

		/// <summary>
		/// 应用具有完整 Vue 元组形状的指令。
		/// Applies a directive with the full Vue tuple shape.
		/// </summary>
		/// <param name="directive">指令定义或函数简写形式。The directive definition or function shorthand.</param>
		/// <param name="value">指令的值。The directive value.</param>
		/// <param name="arg">指令参数。仅需要修饰符时使用 <c>null</c>。The directive argument. Use <c>null</c> when only modifiers are needed.</param>
		/// <param name="modifiers">指令修饰符标志。The directive modifier flags.</param>
		public extern VueDirectiveArguments(VueDirectiveValue directive, VueValue? value, string? arg, VueDirectiveModifierBag modifiers);
	}

	/// <summary>
	/// 强类型指令参数元组。泛型参数使提供的值与类型化指令定义保持一致，同时保留 Vue 的运行时数组元组形状。
	/// Strongly typed directive argument tuple. The generic argument keeps the supplied
	/// value aligned with the typed directive definition while preserving Vue's runtime
	/// array tuple shape.
	/// </summary>
	/// <typeparam name="TValue">指令的值契约。The directive value contract.</typeparam>
	[ECMAScript]
	[Description("@#Array")]
	public sealed class VueDirectiveArguments<TValue> : VueDirectiveArguments
	{
		/// <summary>
		/// 应用带类型化值的类型化指令。
		/// Applies a typed directive with a typed value.
		/// </summary>
		/// <param name="directive">类型化指令定义。The typed directive definition.</param>
		/// <param name="value">指令的值。The directive value.</param>
		public extern VueDirectiveArguments(VueDirective<TValue> directive, TValue value);

		/// <summary>
		/// 应用带类型化值和参数的类型化指令。
		/// Applies a typed directive with a typed value and argument.
		/// </summary>
		/// <param name="directive">类型化指令定义。The typed directive definition.</param>
		/// <param name="value">指令的值。The directive value.</param>
		/// <param name="arg">指令参数。The directive argument.</param>
		public extern VueDirectiveArguments(VueDirective<TValue> directive, TValue value, string arg);

		/// <summary>
		/// 应用具有完整 Vue 元组形状的类型化指令。
		/// Applies a typed directive with the full Vue tuple shape.
		/// </summary>
		/// <param name="directive">类型化指令定义。The typed directive definition.</param>
		/// <param name="value">指令的值。The directive value.</param>
		/// <param name="arg">指令参数。仅需要修饰符时使用 <c>null</c>。The directive argument. Use <c>null</c> when only modifiers are needed.</param>
		/// <param name="modifiers">指令修饰符标志。The directive modifier flags.</param>
		public extern VueDirectiveArguments(VueDirective<TValue> directive, TValue value, string? arg, VueDirectiveModifierBag modifiers);
	}

	/// <summary>
	/// 指令生命周期钩子的当前运行时绑定有效载荷。
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
		/// 用户传递的当前指令值。对于更丰富的非原始类型契约，推荐使用泛型 <see cref="VueDirectiveBinding{TValue}"/>。
		/// Current directive value passed by the user. For richer non-primitive contracts,
		/// prefer the generic <see cref="VueDirectiveBinding{TValue}"/>.
		/// </summary>
		[Description("@#value")]
		public extern VueValue Value { get; }

		/// <summary>
		/// 提供给指令的动态参数片段，例如 <c>v-demo:focus</c> 中的 <c>focus</c>。
		/// Dynamic argument segment provided to the directive, such as <c>focus</c> in
		/// <c>v-demo:focus</c>.
		/// </summary>
		[Description("@#arg")]
		public extern string? Arg { get; }

		/// <summary>
		/// 提供给指令调用点的修饰符标志。
		/// Modifier flags provided to the directive call site.
		/// </summary>
		[Description("@#modifiers")]
		public extern VueDirectiveModifiers Modifiers { get; }

		/// <summary>
		/// 拥有该指令用法的组件公开实例（如果可用）。
		/// Component public instance that owns the directive usage, when available.
		/// </summary>
		[Description("@#instance")]
		public extern VueComponentPublicInstance? Instance { get; }

		/// <summary>
		/// 当前正在调用的指令定义。
		/// The directive definition currently being invoked.
		/// </summary>
		[Description("@#dir")]
		public extern VueDirective Dir { get; }
	}

	/// <summary>
	/// 带有强类型值契约的指令生命周期钩子的当前运行时绑定有效载荷。
	/// Current runtime binding payload for a directive lifecycle hook with a strongly typed value contract.
	/// </summary>
	/// <typeparam name="TValue">指令当前绑定值的类型化契约。The typed contract of the directive's current binding value.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveBinding<TValue> : VueDirectiveBinding
	{
		protected VueDirectiveBinding()
		{
		}

		/// <summary>
		/// 用户传递的当前指令值。
		/// Current directive value passed by the user.
		/// </summary>
		[Description("@#value")]
		public new extern TValue Value { get; }

		/// <summary>
		/// 当前正在调用的类型化指令定义。
		/// The typed directive definition currently being invoked.
		/// </summary>
		[Description("@#dir")]
		public new extern VueDirective<TValue> Dir { get; }
	}

	/// <summary>
	/// 指令更新钩子的运行时绑定有效载荷，包括之前的值。
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
		/// 在前一个更新周期中在同一元素上观察到的之前指令值。
		/// Previous directive value observed on the same element during the preceding update cycle.
		/// </summary>
		[Description("@#oldValue")]
		public extern VueValue OldValue { get; }
	}

	/// <summary>
	/// 指令更新钩子的类型化运行时绑定有效载荷，包括之前的值。
	/// Typed runtime binding payload for a directive update hook, including the previous value.
	/// </summary>
	/// <typeparam name="TValue">指令当前和之前绑定值的类型化契约。The typed contract of the directive's current and previous binding values.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class VueDirectiveUpdateBinding<TValue> : VueDirectiveBinding<TValue>
	{
		protected VueDirectiveUpdateBinding()
		{
		}

		/// <summary>
		/// 在前一个更新周期中在同一元素上观察到的之前指令值。
		/// Previous directive value observed on the same element during the preceding update cycle.
		/// </summary>
		[Description("@#oldValue")]
		public extern TValue OldValue { get; }
	}

	/// <summary>
	/// 在注册和检索边界使用的联合类型指令值契约，Vue 在此处接受对象形式指令或函数简写形式。
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
	/// 直接对象形式的 Vue 指令编写表面。映射到普通的 JavaScript 指令对象，其生命周期钩子由 Vue 调用。
	/// Direct object-form Vue directive authoring surface. This maps to a plain JavaScript
	/// directive object whose lifecycle hooks are invoked by Vue.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueDirective : IVueOptionsBag
	{
		/// <summary>
		/// 将指令标记为深度，使 Vue 遍历嵌套值以进行变更检测。
		/// Marks the directive as deep, so Vue traverses nested values for change detection.
		/// </summary>
		[Description("@#deep")]
		public bool? Deep { get; init; }

		/// <summary>
		/// 在任何属性或监听器应用到元素之前调用。
		/// Called before any attributes or listeners are applied to the element.
		/// </summary>
		[Description("@#created")]
		public VueDirectiveHook? Created { get; init; }

		/// <summary>
		/// 在元素插入 DOM 之前调用。
		/// Called right before the element is inserted into the DOM.
		/// </summary>
		[Description("@#beforeMount")]
		public VueDirectiveHook? BeforeMount { get; init; }

		/// <summary>
		/// 在元素插入 DOM 之后调用。
		/// Called after the element is inserted into the DOM.
		/// </summary>
		[Description("@#mounted")]
		public VueDirectiveHook? Mounted { get; init; }

		/// <summary>
		/// 在包含组件更新且指令重新运行之前调用。
		/// Called right before the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#beforeUpdate")]
		public VueDirectiveUpdateHook? BeforeUpdate { get; init; }

		/// <summary>
		/// 在包含组件更新且指令重新运行之后调用。
		/// Called after the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#updated")]
		public VueDirectiveUpdateHook? Updated { get; init; }

		/// <summary>
		/// 在包含组件卸载元素之前调用。
		/// Called right before the containing component unmounts the element.
		/// </summary>
		[Description("@#beforeUnmount")]
		public VueDirectiveHook? BeforeUnmount { get; init; }

		/// <summary>
		/// 在包含组件卸载元素之后调用。
		/// Called after the containing component unmounts the element.
		/// </summary>
		[Description("@#unmounted")]
		public VueDirectiveHook? Unmounted { get; init; }

		/// <summary>
		/// 在 SSR 期间调用，以向渲染的元素贡献额外的 props。
		/// Called during SSR to contribute additional props to the rendered element.
		/// </summary>
		[Description("@#getSSRProps")]
		public VueDirectiveSSRPropsCallback? GetSSRProps { get; init; }
	}

	/// <summary>
	/// 类型化对象形式的 Vue 指令编写表面。保持指令的绑定值强类型，同时仍降低为相同的普通 JavaScript 指令对象。
	/// Typed object-form Vue directive authoring surface. This keeps the directive's binding
	/// value strongly typed while still lowering to the same plain JavaScript directive object.
	/// </summary>
	/// <typeparam name="TValue">指令绑定值的类型化契约。The typed contract of the directive's binding value.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueDirective<TValue> : VueDirective
	{
		/// <summary>
		/// 在任何属性或监听器应用到元素之前调用。
		/// Called before any attributes or listeners are applied to the element.
		/// </summary>
		[Description("@#created")]
		public new VueDirectiveHook<TValue>? Created { get; init; }

		/// <summary>
		/// 在元素插入 DOM 之前调用。
		/// Called right before the element is inserted into the DOM.
		/// </summary>
		[Description("@#beforeMount")]
		public new VueDirectiveHook<TValue>? BeforeMount { get; init; }

		/// <summary>
		/// 在元素插入 DOM 之后调用。
		/// Called after the element is inserted into the DOM.
		/// </summary>
		[Description("@#mounted")]
		public new VueDirectiveHook<TValue>? Mounted { get; init; }

		/// <summary>
		/// 在包含组件更新且指令重新运行之前调用。
		/// Called right before the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#beforeUpdate")]
		public new VueDirectiveUpdateHook<TValue>? BeforeUpdate { get; init; }

		/// <summary>
		/// 在包含组件更新且指令重新运行之后调用。
		/// Called after the containing component updates and the directive re-runs.
		/// </summary>
		[Description("@#updated")]
		public new VueDirectiveUpdateHook<TValue>? Updated { get; init; }

		/// <summary>
		/// 在包含组件卸载元素之前调用。
		/// Called right before the containing component unmounts the element.
		/// </summary>
		[Description("@#beforeUnmount")]
		public new VueDirectiveHook<TValue>? BeforeUnmount { get; init; }

		/// <summary>
		/// 在包含组件卸载元素之后调用。
		/// Called after the containing component unmounts the element.
		/// </summary>
		[Description("@#unmounted")]
		public new VueDirectiveHook<TValue>? Unmounted { get; init; }

		/// <summary>
		/// 在 SSR 期间调用，以向渲染的元素贡献额外的 props。
		/// Called during SSR to contribute additional props to the rendered element.
		/// </summary>
		[Description("@#getSSRProps")]
		public new VueDirectiveSSRPropsCallback<TValue>? GetSSRProps { get; init; }
	}

}
