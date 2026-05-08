using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// <c>watchEffect()</c>、<c>watchPostEffect()</c> 和 <c>watchSyncEffect()</c> 共享的选项。直接映射到 Vue 的普通选项对象。
	/// Options shared by <c>watchEffect()</c>, <c>watchPostEffect()</c>, and
	/// <c>watchSyncEffect()</c>. Maps directly to Vue's plain options object.
	/// </summary>
	public record VueWatchEffectOptions : IVueOptionsBag
	{
		/// <summary>
		/// 控制侦听器回调相对于组件渲染的刷新时机。
		/// Controls when the watcher callback is flushed relative to component rendering.
		/// </summary>
		[Description("@#flush")]
		public VueWatchFlush? Flush { get; init; }

		/// <summary>
		/// 当响应式依赖被追踪时调用的调试回调。
		/// Debug callback invoked when reactive dependencies are tracked.
		/// </summary>
		[Description("@#onTrack")]
		public VueDebuggerCallback? OnTrack { get; init; }

		/// <summary>
		/// 当被追踪的依赖触发侦听器时调用的调试回调。
		/// Debug callback invoked when a tracked dependency triggers the watcher.
		/// </summary>
		[Description("@#onTrigger")]
		public VueDebuggerCallback? OnTrigger { get; init; }
	}

	/// <summary>
	/// <c>watch()</c> 的选项。在副作用选项基础上扩展了源特定的行为，如立即执行、深层遍历和单次侦听。
	/// Options for <c>watch()</c>. This extends effect options with source-specific
	/// behavior such as eager execution, deep traversal, and one-shot watches.
	/// </summary>
	public record VueWatchOptions : VueWatchEffectOptions
	{
		/// <summary>
		/// 立即使用当前值运行回调。
		/// Run the callback immediately with the current value.
		/// </summary>
		[Description("@#immediate")]
		public bool? Immediate { get; init; }

		/// <summary>
		/// 遍历嵌套属性。使用 <c>true</c> 进行完整遍历，或使用整数深度限制进行有界遍历。
		/// Traverse nested properties. Use <c>true</c> for full traversal or an integer
		/// depth limit when only a bounded traversal is needed.
		/// </summary>
		[Description("@#deep")]
		public VueWatchDeep? Deep { get; init; }

		/// <summary>
		/// 在首次回调运行后自动停止侦听器。
		/// Stop the watcher automatically after the first callback run.
		/// </summary>
		[Description("@#once")]
		public bool? Once { get; init; }
	}

	/// <summary>
	/// 处理程序为强类型回调的 Options API 侦听声明。
	/// Options API watch declaration whose handler is a strongly typed callback.
	/// </summary>
	/// <typeparam name="T">被侦听的值类型。</typeparam>
	public record VueWatchHandlerOptions<T> : VueWatchOptions
	{
		/// <summary>
		/// 以当前值和先前值调用的回调。
		/// Callback invoked with the current and previous values.
		/// </summary>
		[Description("@#handler")]
		public Action<T, T> Handler { get; init; } = default!;
	}

	/// <summary>
	/// 处理程序除当前值和先前值外还接收 Vue 清理注册回调的 Options API 侦听声明。
	/// Options API watch declaration whose handler receives Vue's cleanup registration
	/// callback in addition to the current and previous values.
	/// </summary>
	/// <typeparam name="T">被侦听的值类型。</typeparam>
	public record VueWatchCleanupHandlerOptions<T> : VueWatchOptions
	{
		/// <summary>
		/// 感知清理的回调，以当前值、先前值和清理注册函数作为参数。
		/// Cleanup-aware callback invoked with the current value, previous value, and cleanup registration.
		/// </summary>
		[Description("@#handler")]
		public VueWatchCleanupCallback<T> Handler { get; init; } = default!;
	}

	/// <summary>
	/// 处理程序由 Vue 从组件 <c>methods</c> 对象解析的 Options API 侦听声明。
	/// Options API watch declaration whose handler is resolved by Vue from the component
	/// <c>methods</c> object.
	/// </summary>
	public record VueWatchNamedHandlerOptions : VueWatchOptions
	{
		/// <summary>
		/// 从同一组件的 <c>methods</c> 选项中解析的方法名。
		/// Method name to resolve from the same component's <c>methods</c> option.
		/// </summary>
		[Description("@#handler")]
		public string Handler { get; init; } = default!;
	}

	/// <summary>
	/// <c>useModel()</c> 的选项。Vue 在读取和写入模型引用时应用这些转换。
	/// Options for <c>useModel()</c>. Vue applies these transforms when reading from
	/// and writing to the model ref.
	/// </summary>
	/// <typeparam name="T">模型值类型。</typeparam>
	public record VueModelOptions<T> : IVueOptionsBag
	{
		/// <summary>
		/// 读取模型引用时转换属性值。
		/// Transform the prop value when reading the model ref.
		/// </summary>
		[Description("@#get")]
		public Func<T, T>? Get { get; init; }

		/// <summary>
		/// 在 Vue 触发更新事件之前转换赋值。
		/// Transform the assigned value before Vue emits the update event.
		/// </summary>
	[Description("@#set")]
	public Func<T, T>? Set { get; init; }
	}

	/// <summary>
	/// 强类型命名模型契约，用于保持 <c>useModel()</c>、属性名声明和 <c>update:*</c> 事件名一致，而无需重复原始字符串字面量。运行时仍擦除为最终的属性键字符串。
	/// Strongly typed named-model contract used to keep <c>useModel()</c>, prop-name
	/// declarations, and <c>update:*</c> event names aligned without repeating raw
	/// string literals. At runtime this still erases to the final prop key string.
	/// </summary>
	/// <typeparam name="TProps">与此模型关联的类型化属性契约。</typeparam>
	/// <typeparam name="TValue">模型值类型。</typeparam>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueModelName<TProps, TValue>
		where TProps : VueProps
	{
		private VueModelName()
		{
		}

		/// <summary>
		/// 将最终运行时属性键字符串视为类型化模型名契约。
		/// Treat a final runtime prop key string as a typed model-name contract.
		/// </summary>
		/// <param name="key">最终运行时属性键，如 <c>"modelValue"</c> 或 <c>"count"</c>。</param>
		public extern static implicit operator VueModelName<TProps, TValue>(string key);

		/// <summary>
		/// 当 API 期望原始字符串时公开最终运行时属性键字符串。
		/// Exposes the final runtime prop key string when an API expects a raw string.
		/// </summary>
		/// <param name="model">类型化模型名契约。</param>
		public extern static implicit operator string(VueModelName<TProps, TValue> model);
	}

	/// <summary>
	/// Vue <c>useModel()</c> 类元组结果返回的模型修饰符只读包。修饰符键可通过字符串索引器访问，或在组件定义自定义修饰符时投影到更强的类型化子类。
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
		/// 通过最终修饰符键读取任意修饰符标志。
		/// Reads an arbitrary modifier flag by its final modifier key.
		/// </summary>
		/// <param name="key">修饰符键，例如 <c>"trim"</c>。</param>
		/// <returns>修饰符存在时为 <c>true</c>；否则为 <c>null</c> / <c>undefined</c>。</returns>
		public extern bool? this[string key] { get; }

		/// <summary>
		/// 读取 Vue 内置 <c>.trim</c> 模型修饰符。
		/// Reads Vue's built-in <c>.trim</c> model modifier.
		/// </summary>
		[Description("@#trim")]
		public extern bool? Trim { get; }

		/// <summary>
		/// 读取 Vue 内置 <c>.number</c> 模型修饰符。
		/// Reads Vue's built-in <c>.number</c> model modifier.
		/// </summary>
		[Description("@#number")]
		public extern bool? Number { get; }

		/// <summary>
		/// 读取 Vue 内置 <c>.lazy</c> 模型修饰符。
		/// Reads Vue's built-in <c>.lazy</c> model modifier.
		/// </summary>
		[Description("@#lazy")]
		public extern bool? Lazy { get; }
	}

	/// <summary>
	/// <c>useModel()</c> 返回的结果类型。Vue 公开一个可写引用，同时携带类元组的模型修饰符投影。此宿主界面在 <see cref="IVueRef{T}.Value"/> 上保持常规引用编写，同时通过内联辅助方法公开修饰符而非编译器特殊处理。
	/// Result type returned by <c>useModel()</c>. Vue exposes a writable ref that also
	/// carries the tuple-like model-modifiers projection. This host surface keeps normal
	/// ref authoring on <see cref="IVueRef{T}.Value"/> while exposing modifiers through an
	/// inline helper instead of compiler special casing.
	/// </summary>
	/// <typeparam name="TValue">模型值类型。</typeparam>
	public abstract class VueModelRef<TValue> : IVueRef<TValue>
	{
		protected VueModelRef()
		{
		}

		/// <summary>
		/// 获取或设置当前模型值。
		/// Gets or sets the current model value.
		/// </summary>
		[Description("@#value")]
		public extern TValue Value { get; set; }

		/// <summary>
		/// 从 Vue 类元组 <c>useModel()</c> 结果中读取当前模型修饰符包。
		/// Reads the current model modifiers bag from Vue's tuple-like <c>useModel()</c>
		/// result.
		/// </summary>
		/// <returns>原始模型修饰符包。</returns>
		[ECMAScriptInline("__arg1[1]")]
		public extern VueModelModifiers GetModifiers();

		/// <summary>
		/// 将当前模型修饰符包投影到更强的类型化修饰符子类后读取。
		/// Reads the current model modifiers bag projected to a stronger typed modifier
		/// subclass.
		/// </summary>
		/// <typeparam name="TModifiers">类型化修饰符投影。</typeparam>
		/// <returns>投影为 <typeparamref name="TModifiers"/> 的修饰符包。</returns>
		[ECMAScriptInline("__arg1[1]")]
		public extern TModifiers GetModifiers<TModifiers>()
			where TModifiers : VueModelModifiers;
	}

	/// <summary>
	/// 可写计算属性选项。Vue 期望一个包含 <c>get</c> 和 <c>set</c> 成员的普通对象；C# 将这些公开为强类型委托。
	/// Writable computed options. Vue expects a plain object with <c>get</c> and
	/// <c>set</c> members; C# exposes those as strongly typed delegates.
	/// </summary>
	/// <typeparam name="T">计算属性值类型。</typeparam>
	public record VueWritableComputedOptions<T> : IVueOptionsBag
	{
		/// <summary>
		/// Vue 用于计算当前值的 getter。
		/// Getter used by Vue to compute the current value.
		/// </summary>
		[Description("@#get")]
		public Func<T> Get { get; init; } = default!;

		/// <summary>
		/// 当计算引用被赋值时调用的 setter。
		/// Setter invoked when the computed ref is assigned.
		/// </summary>
		[Description("@#set")]
		public Action<T> Set { get; init; } = default!;
	}

	/// <summary>
	/// <c>customRef()</c> 工厂返回的 get/set 处理程序。
	/// Get/set handlers returned by a <c>customRef()</c> factory.
	/// </summary>
	/// <typeparam name="T">自定义引用值类型。</typeparam>
	public record VueCustomRefHandlers<T> : IVueOptionsBag
	{
		/// <summary>
		/// 当自定义引用的 <c>value</c> 被读取时 Vue 使用的 getter。
		/// Getter used by Vue when the custom ref's <c>value</c> is read.
		/// </summary>
		[Description("@#get")]
		public Func<T> Get { get; init; } = default!;

		/// <summary>
		/// 当自定义引用的 <c>value</c> 被赋值时 Vue 使用的 setter。
		/// Setter used by Vue when the custom ref's <c>value</c> is assigned.
		/// </summary>
		[Description("@#set")]
		public Action<T> Set { get; init; } = default!;
	}

	/// <summary>
	/// <c>effectScope()</c> 返回的运行时副作用作用域。在作用域激活期间创建的副作用可通过该作用域一起停止。
	/// Runtime effect scope returned by <c>effectScope()</c>. Effects created while a
	/// scope is active can be stopped together through the scope.
	/// </summary>
	public abstract class VueEffectScope
	{
		protected VueEffectScope()
		{
		}

		/// <summary>
		/// 在此副作用作用域内运行回调。
		/// Run a callback inside this effect scope.
		/// </summary>
		/// <typeparam name="TResult">回调返回类型。</typeparam>
		/// <param name="callback">在此作用域激活期间执行的回调。</param>
		/// <returns>回调结果。</returns>
		[Description("@#run")]
		public extern TResult Run<TResult>(Func<TResult> callback);

		/// <summary>
		/// 停止此作用域捕获的所有副作用。
		/// Stop every effect captured by this scope.
		/// </summary>
		[Description("@#stop")]
		public extern void Stop();
	}

	/// <summary>
	/// 表示已挂载 Vue 组件的公共实例。从 <see cref="VueApp.Mount(string)"/> 获取，用于测试或程序化访问通过 <c>expose()</c> 暴露的组件公共属性。
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
	/// <c>setup()</c> 函数内可用的 setup 上下文。提供对透传属性、插槽、事件触发和公共实例暴露的访问。
	/// Setup context available inside the <c>setup()</c> function. Provides access to
	/// fallthrough attributes, slots, event emission, and public instance exposure.
	/// </summary>
	public abstract class VueSetupContext
	{
		/// <summary>
		/// 传递给组件但未声明为属性的透传属性。当 <c>inheritAttrs</c> 为 <c>true</c> 时，包括 <c>class</c>、<c>style</c> 和事件侦听器。
		/// Fallthrough attributes passed to the component but not declared as props.
		/// Includes <c>class</c>, <c>style</c>, and event listeners when <c>inheritAttrs</c> is <c>true</c>.
		/// </summary>
		[Description("@#attrs")]
		public extern VueAttributeBag Attrs { get; }

		/// <summary>
		/// 组件中可用的插槽。用于通过 <c>context.slots.default?.()</c> 渲染默认或命名插槽内容。
		/// Slots available in the component. Use this to render default or named slot content
		/// via <c>context.slots.default?.()</c>.
		/// </summary>
		[Description("@#slots")]
		public extern VueSlotBag Slots { get; }

		/// <summary>
		/// 按名称触发无载荷的自定义事件。父组件可通过 <c>v-on:eventName</c> 或 <c>@eventName</c> 侦听。
		/// Emit a custom event by name with no payload. The parent component can listen
		/// via <c>v-on:eventName</c> or <c>@eventName</c>.
		/// </summary>
		/// <param name="eventName">要触发的事件名（如 <c>"close"</c>）。</param>
		[Description("@#emit")]
		public extern void Emit(string eventName);

		/// <summary>
		/// 按名称触发带单个类型化载荷值的自定义事件。
		/// Emit a custom event by name with a single typed payload value.
		/// </summary>
		/// <typeparam name="TValue">事件载荷的类型。</typeparam>
		/// <param name="eventName">要触发的事件名（如 <c>"update:modelValue"</c>）。</param>
		/// <param name="value">随事件发送的载荷值。</param>
		[Description("@#emit")]
		public extern void Emit<TValue>(string eventName, TValue value);

		/// <summary>
		/// 触发与类型化模型名契约对应的 <c>update:*</c> 事件。保持命名模型更新触发与 <c>useModel()</c> 和运行时属性声明所使用的同一契约一致。
		/// Emit the <c>update:*</c> event corresponding to a typed model-name contract.
		/// This keeps named-model update emits aligned with the same contract used by
		/// <c>useModel()</c> and runtime prop declarations.
		/// </summary>
		/// <typeparam name="TProps">与此模型关联的类型化属性契约。</typeparam>
		/// <typeparam name="TValue">触发的模型值类型。</typeparam>
		/// <param name="model">类型化模型名契约。</param>
		/// <param name="value">随对应的 <c>update:*</c> 事件发送的载荷值。</param>
		[ECMAScriptInline("__arg1.emit(`update:${__arg2}`, __arg3)")]
		public extern void Emit<TProps, TValue>(VueModelName<TProps, TValue> model, TValue value)
			where TProps : VueProps;

		/// <summary>
		/// 按名称触发带两个类型化载荷值的自定义事件。
		/// Emit a custom event by name with two typed payload values.
		/// </summary>
		/// <typeparam name="T0">第一个载荷值的类型。</typeparam>
		/// <typeparam name="T1">第二个载荷值的类型。</typeparam>
		/// <param name="eventName">要触发的事件名（如 <c>"update"</c>）。</param>
		/// <param name="value0">随事件发送的第一个载荷值。</param>
		/// <param name="value1">随事件发送的第二个载荷值。</param>
		[Description("@#emit")]
		public extern void Emit<T0, T1>(string eventName, T0 value0, T1 value1);

		/// <summary>
		/// 按名称触发带三个类型化载荷值的自定义事件。
		/// Emit a custom event by name with three typed payload values.
		/// </summary>
		/// <typeparam name="T0">第一个载荷值的类型。</typeparam>
		/// <typeparam name="T1">第二个载荷值的类型。</typeparam>
		/// <typeparam name="T2">第三个载荷值的类型。</typeparam>
		/// <param name="eventName">要触发的事件名。</param>
		/// <param name="value0">随事件发送的第一个载荷值。</param>
		/// <param name="value1">随事件发送的第二个载荷值。</param>
		/// <param name="value2">随事件发送的第三个载荷值。</param>
		[Description("@#emit")]
		public extern void Emit<T0, T1, T2>(string eventName, T0 value0, T1 value1, T2 value2);

		/// <summary>
		/// 按名称触发带四个类型化载荷值的自定义事件。
		/// Emit a custom event by name with four typed payload values.
		/// </summary>
		/// <typeparam name="T0">第一个载荷值的类型。</typeparam>
		/// <typeparam name="T1">第二个载荷值的类型。</typeparam>
		/// <typeparam name="T2">第三个载荷值的类型。</typeparam>
		/// <typeparam name="T3">第四个载荷值的类型。</typeparam>
		/// <param name="eventName">要触发的事件名。</param>
		/// <param name="value0">随事件发送的第一个载荷值。</param>
		/// <param name="value1">随事件发送的第二个载荷值。</param>
		/// <param name="value2">随事件发送的第三个载荷值。</param>
		/// <param name="value3">随事件发送的第四个载荷值。</param>
		[Description("@#emit")]
		public extern void Emit<T0, T1, T2, T3>(string eventName, T0 value0, T1 value1, T2 value2, T3 value3);

		/// <summary>
		/// 在组件的公共实例上暴露一个值，以便父组件可通过模板引用（<c>ref="..."</c>）访问。只有被暴露的值可从父组件访问；所有其他内部状态均被隐藏。
		/// Expose a value on the component's public instance so parent components can
		/// access it via template refs (<c>ref="..."</c>). Only exposed values are
		/// accessible from the parent; all other internal state is hidden.
		/// </summary>
		/// <typeparam name="TValue">被暴露值的类型（必须是引用类型）。</typeparam>
		/// <param name="exposed">要暴露在公共实例上的对象或值。</param>
		[Description("@#expose")]
		public extern void Expose<TValue>(TValue exposed) where TValue : class;
	}

	/// <summary>
	/// 除标准 <see cref="VueSetupContext"/> 成员外还提供类型化插槽访问的类型化 setup 上下文。<c>Slots</c> 属性返回类型化的 <typeparamref name="TSlots"/> 记录而非非类型化的 <see cref="VueSlotBag"/>。
	/// Typed setup context that provides typed slot access in addition to the standard
	/// <see cref="VueSetupContext"/> members. The <c>Slots</c> property returns the
	/// typed <typeparamref name="TSlots"/> record instead of the untyped <see cref="VueSlotBag"/>.
	/// </summary>
	/// <typeparam name="TSlots">组件声明的插槽记录类型。</typeparam>
	public abstract class VueSetupContext<TSlots> : VueSetupContext
		where TSlots : VueSlots
	{
		/// <summary>
		/// 组件中可用的类型化插槽。<typeparamref name="TSlots"/> 上的每个属性映射到一个可调用以产生其 VNode 内容的命名插槽。
		/// Typed slots available in the component. Each property on <typeparamref name="TSlots"/>
		/// maps to a named slot that can be invoked to produce its VNode content.
		/// </summary>
		[Description("@#slots")]
		public new extern TSlots Slots { get; }
	}

	/// <summary>
	/// 透传属性包（<c>v-bind="$attrs"</c>）。包含传递给组件但未声明为属性的属性，包括 <c>class</c>、<c>style</c> 和事件侦听器。
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
		/// 通过最终触发键读取任意透传属性。
		/// Reads an arbitrary fallthrough attribute by its final emitted key.
		/// </summary>
		/// <param name="key">最终的 JavaScript 属性键。</param>
		/// <returns>存在时返回属性值；否则为 <c>null</c> / <c>undefined</c>。</returns>
		public extern VueValue? this[string key] { get; }

		/// <summary>
		/// 读取透传 <c>class</c> 绑定。
		/// Reads the fallthrough <c>class</c> binding.
		/// </summary>
		[Description("@#class")]
		public extern VueClassValue? Class { get; }

		/// <summary>
		/// 读取透传 <c>style</c> 绑定。
		/// Reads the fallthrough <c>style</c> binding.
		/// </summary>
		[Description("@#style")]
		public extern VueProps? Style { get; }

		/// <summary>
		/// 读取透传 <c>id</c> 属性。
		/// Reads the fallthrough <c>id</c> attribute.
		/// </summary>
		[Description("@#id")]
		public extern string? Id { get; }

		/// <summary>
		/// 读取透传 <c>title</c> 属性。
		/// Reads the fallthrough <c>title</c> attribute.
		/// </summary>
	[Description("@#title")]
	public extern string? Title { get; }

	/// <summary>
	/// 读取透传 <c>for</c> 属性。
	/// Reads the fallthrough <c>for</c> attribute.
	/// </summary>
	[Description("@#for")]
	public extern string? For { get; }

	/// <summary>
	/// 读取透传 <c>name</c> 属性。
	/// Reads the fallthrough <c>name</c> attribute.
	/// </summary>
	[Description("@#name")]
	public extern string? Name { get; }

	/// <summary>
	/// 读取透传 <c>type</c> 属性。
	/// Reads the fallthrough <c>type</c> attribute.
	/// </summary>
	[Description("@#type")]
	public extern string? Type { get; }

	/// <summary>
	/// 读取透传 <c>placeholder</c> 属性。
	/// Reads the fallthrough <c>placeholder</c> attribute.
	/// </summary>
	[Description("@#placeholder")]
	public extern string? Placeholder { get; }

	/// <summary>
	/// 读取透传 <c>disabled</c> 属性。
	/// Reads the fallthrough <c>disabled</c> attribute.
	/// </summary>
	[Description("@#disabled")]
	public extern bool? Disabled { get; }

	/// <summary>
	/// 读取透传 <c>readonly</c> 属性。
	/// Reads the fallthrough <c>readonly</c> attribute.
	/// </summary>
	[Description("@#readonly")]
	public extern bool? Readonly { get; }

	/// <summary>
	/// 读取透传 <c>required</c> 属性。
	/// Reads the fallthrough <c>required</c> attribute.
	/// </summary>
	[Description("@#required")]
	public extern bool? Required { get; }

	/// <summary>
	/// 读取透传 <c>tabindex</c> 属性。
	/// Reads the fallthrough <c>tabindex</c> attribute.
	/// </summary>
	[Description("@#tabindex")]
	public extern int? Tabindex { get; }

	/// <summary>
	/// 读取透传 <c>role</c> 属性。
	/// Reads the fallthrough <c>role</c> attribute.
	/// </summary>
	[Description("@#role")]
	public extern string? Role { get; }
	}

	/// <summary>
	/// 可用插槽包（<c>$slots</c>）。每个属性是返回 VNode 内容的可调用插槽函数。
	/// Bag of available slots (<c>$slots</c>). Each property is a callable slot
	/// function that returns VNode content.
	/// </summary>
	public abstract class VueSlotBag
	{
		protected VueSlotBag()
		{
		}

		/// <summary>
		/// 通过最终插槽名读取任意插槽回调。
		/// Reads an arbitrary slot callback by its final slot name.
		/// </summary>
		/// <param name="key">最终的 Vue 插槽键。</param>
		/// <returns>存在时返回插槽回调；否则为 <c>null</c> / <c>undefined</c>。</returns>
		public extern VueSlotCallback? this[string key] { get; }

		/// <summary>
		/// 读取存在时的默认插槽回调。
		/// Reads the default slot callback when present.
		/// </summary>
		[Description("@#default")]
		public extern VueSlotCallback? Default { get; }
	}

	/// <summary>
	/// 指令修饰符包。每个键对应指令调用点使用的修饰符名称，例如 <c>v-colorize.primary</c> 暴露 <c>binding.modifiers["primary"]</c>。
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
		/// 返回给定的修饰符标志是否存在于当前指令用法上。
		/// Returns whether the given modifier flag is present on the current directive usage.
		/// </summary>
		/// <param name="key">要检查的修饰符名称。</param>
		/// <returns>修饰符存在时为 <c>true</c>；否则为 <c>false</c>。</returns>
		public extern bool this[string key] { get; }
	}

}
