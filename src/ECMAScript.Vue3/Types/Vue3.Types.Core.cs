using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>Vue 组件、VNode、props/slots 等核心类型 contract。</summary>
/// <remarks>本分片描述运行时形状，组件实际发射由 RazorVue/compiler framing 负责。</remarks>
public static partial class Vue3
{
	/// <summary>
	/// RazorVue/Vue3 组件创作契约。
	/// Components are expected to inherit <c>ComponentBase</c> and implement this marker.
	/// </summary>
	public interface IVueComponent : IUIComponent
	{
	}

	/// <summary>
	/// 外部 Vue 库组件存根的标记接口，参与描述符/注册流程但不作为普通用户组件处理。
	/// Marker for external Vue library component stubs that participate in
	/// descriptor/registry flows without being treated as ordinary user components.
	/// </summary>
	public interface IVueLibraryComponent : IVueComponent
	{
	}

	/// <summary>
	/// 声明类型化 props 的 Vue 组件。编译器使用此接口为仅有 props 的组件选择正确的 <c>h()</c> 重载。
	/// A Vue component that declares typed props. The compiler uses this interface
	/// to select the correct <c>h()</c> overload for props-only components.
	/// </summary>
	/// <typeparam name="TProps">描述组件接受的 props 的 props 记录类型。The props record type describing the component's accepted props.</typeparam>
	public interface IVueComponent<TProps> : ECMAScript.Vue3.IVueComponent
		where TProps : VueProps
	{
	}

	/// <summary>
	/// 声明类型化插槽但没有类型化 props 的 Vue 组件。编译器使用此接口为仅有插槽的组件选择正确的 <c>h()</c> 重载。
	/// A Vue component that declares typed slots but no typed props. The compiler uses
	/// this interface to select the correct <c>h()</c> overload for slots-only components.
	/// </summary>
	/// <typeparam name="TSlots">描述组件接受的插槽的插槽记录类型。The slots record type describing the component's accepted slots.</typeparam>
	public interface IVueSlotComponent<TSlots> : ECMAScript.Vue3.IVueComponent
		where TSlots : VueSlots
	{
	}

	/// <summary>
	/// 同时声明类型化 props 和类型化插槽的 Vue 组件。编译器使用此接口为两者皆有的组件选择正确的 <c>h()</c> 重载。
	/// A Vue component that declares both typed props and typed slots. The compiler uses
	/// this interface to select the correct <c>h()</c> overload for components with both.
	/// </summary>
	/// <typeparam name="TProps">描述组件接受的 props 的 props 记录类型。The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">描述组件接受的插槽的插槽记录类型。The slots record type describing the component's accepted slots.</typeparam>
	public interface IVueComponent<TProps, TSlots> : IVueComponent<TProps>, IVueSlotComponent<TSlots>
		where TProps : VueProps
		where TSlots : VueSlots
	{
	}

	/// <summary>
	/// 表示由 <c>h()</c> 返回的 Vue 虚拟 DOM 节点（VNode）。VNode 是 Vue 渲染树的构建块，由运行时进行 diff/patch。
	/// Represents a Vue virtual DOM node (VNode) returned by <c>h()</c>. VNodes are the
	/// building blocks of Vue's render tree and are diffed/patched by the runtime.
	/// </summary>
	public interface IVNode { }

	/// <summary>
	/// 响应式引用包装器。读取 <c>Value</c> 会将 ref 追踪为响应式依赖；写入 <c>Value</c> 会触发依赖此 ref 的所有侦听器。
	/// A reactive reference wrapper. Reading <c>Value</c> tracks the ref as a reactive
	/// dependency; writing <c>Value</c> triggers any watchers depending on this ref.
	/// </summary>
	/// <typeparam name="T">被包装值的类型。The type of the wrapped value.</typeparam>
	public interface IVueRef<T>
	{
		/// <summary>
		/// 获取或设置底层的响应式值。读取被追踪；写入通知侦听器。
		/// Gets or sets the underlying reactive value. Reads are tracked; writes notify watchers.
		/// </summary>
		[Description("@#value")]
		public T Value { get; set; }
	}

	/// <summary>
	/// 映射到纯 JavaScript 对象的选项包标记接口，用于 Vue 组件选项、插件配置和注册表。
	/// Marker interface for option bags that map to plain JavaScript objects in Vue component
	/// options, plugin configuration, and registries.
	/// </summary>
	public interface IVueOptionsBag { }

	/// <summary>
	/// 强类型的 Vue 依赖注入键。运行时仍然是用户提供的 JavaScript <see cref="Symbol"/> 值；
	/// 泛型参数仅用于约束 C# 中匹配的 <c>Provide</c> / <c>Inject</c> 调用。
	/// Strongly typed Vue dependency-injection key. At runtime this is still the
	/// JavaScript <see cref="Symbol"/> value supplied by the user; the generic argument
	/// only constrains matching <c>Provide</c> / <c>Inject</c> calls in C#.
	/// </summary>
	/// <typeparam name="TValue">与此注入键关联的值契约。The value contract associated with this injection key.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public sealed class VueInjectionKey<TValue>
	{
		private VueInjectionKey()
		{
		}

		/// <summary>
		/// 将 JavaScript symbol 视为类型化的 Vue 注入键。在发射时擦除为原始 symbol 值。
		/// Treat a JavaScript symbol as a typed Vue injection key. This erases to the
		/// original symbol value at emission time.
		/// </summary>
		/// <param name="key">用作注入键的 JavaScript symbol。The JavaScript symbol used as the injection key.</param>
		public extern static implicit operator VueInjectionKey<TValue>(Symbol key);

		/// <summary>
		/// 当 API 需要原始 symbol 键时暴露底层的 JavaScript symbol。
		/// Exposes the underlying JavaScript symbol when an API needs a raw symbol key.
		/// </summary>
		/// <param name="key">类型化的 Vue 注入键。The typed Vue injection key.</param>
		public extern static implicit operator Symbol(VueInjectionKey<TValue> key);
	}

	/// <summary>
	/// 组件 props 声明的基记录。继承此记录并声明属性以定义组件接受的 props。
	/// 映射为 Vue <c>props</c> 选项中的纯 JS 对象。
	/// Base record for component prop declarations. Inherit from this record and declare
	/// properties to define the props a component accepts. Maps to a plain JS object in
	/// Vue's <c>props</c> option.
	/// </summary>
	public abstract record VueProps : IVueOptionsBag;

	/// <summary>
	/// 用于任意字符串键的泛型字典式 Vue 对象创作表面。
	/// 保持为 record 以参与结构化对象 lowering，并发射纯 JavaScript 对象而非运行时 <c>Map</c>。
	/// 字符串键发射为普通对象成员；<see cref="Symbol"/> 键发射为计算属性。
	/// Generic dictionary-style Vue object authoring surface for arbitrary string keys.
	/// This remains a record so it participates in structural object lowering and emits
	/// a plain JavaScript object rather than a runtime <c>Map</c>. String keys emit
	/// normal object members; <see cref="Symbol"/> keys emit computed properties.
	/// </summary>
	/// <typeparam name="TValue">每个任意键的值契约。The value contract for each arbitrary key.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VueDictionary<TValue> : VueProps, System.Collections.IEnumerable
	{
		/// <summary>
		/// 通过最终发射的键获取或设置任意 Vue/对象属性。
		/// Gets or sets an arbitrary Vue/object property by its final emitted key.
		/// </summary>
		/// <param name="key">要发射的最终 JavaScript 对象键。The final JavaScript object key to emit.</param>
		/// <returns>映射到给定键的值。The value mapped to the given key.</returns>
		public extern TValue? this[string key] { get; set; }

		/// <summary>
		/// 通过 JavaScript symbol 键获取或设置任意 Vue/对象属性。编译器将其 lowering 为计算对象属性。
		/// Gets or sets an arbitrary Vue/object property by a JavaScript symbol key.
		/// The compiler lowers this to a computed object property.
		/// </summary>
		/// <param name="key">用作属性键的 JavaScript symbol。The JavaScript symbol used as the property key.</param>
		/// <returns>映射到给定 symbol 键的值。The value mapped to the given symbol key.</returns>
		public extern TValue? this[Symbol key] { get; set; }

		/// <summary>
		/// CLR 桥接，用于字符串键条目的集合初始化器创作。
		/// 编译器将其 lowering 为普通对象字面量属性，而非运行时 <c>Add(...)</c> 调用。
		/// CLR bridge kept for collection-initializer authoring of string-keyed entries.
		/// The compiler lowers this into a plain object literal property instead of a
		/// runtime <c>Add(...)</c> call.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(string key, TValue value);

		/// <summary>
		/// CLR 桥接，用于 symbol 键条目的集合初始化器创作。
		/// 编译器将其 lowering 为计算对象字面量属性，而非运行时 <c>Add(...)</c> 调用。
		/// CLR bridge kept for collection-initializer authoring of symbol-keyed entries.
		/// The compiler lowers this into a computed object literal property instead of a
		/// runtime <c>Add(...)</c> call.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public extern void Add(Symbol key, TValue value);

		extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// 用于字典/索引器创作表面的泛型 Vue 值契约。
	/// 仅在编译时使用；隐式转换在发射时擦除为底层 JavaScript 值。
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
	/// <c>h(...)</c> 重载的规范子值契约。
	/// 保留了 JS 端的灵活性（VNode / 文本 / 数字 / 布尔 / VNode 数组），
	/// 同时保持 C# 公共表面紧凑且稳定。
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
	/// Vue VNode 键契约。Vue 接受字符串、数字和 symbol 键；
	/// 此包装器在保持该联合强类型的同时，允许自然的 C# 赋值，
	/// 无需依赖通过 <see cref="Number"/> 的链式隐式转换。
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
	/// Vue props 声明中接受的 JavaScript 构造函数值。
	/// 这些属性发射原始构造函数标识符，如 <c>String</c>、<c>Number</c> 和 <c>Boolean</c>。
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
	/// 用于常见 Vue 对象创作的便捷非泛型字典表面。
	/// 当值契约为通用 <see cref="VueValue"/> 时的直接默认选择。
	/// Convenience non-generic dictionary surface for common Vue object authoring.
	/// This is the direct default when the value contract is the general <see cref="VueValue"/>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VueDictionary : VueDictionary<VueValue>
	{
	}

}
