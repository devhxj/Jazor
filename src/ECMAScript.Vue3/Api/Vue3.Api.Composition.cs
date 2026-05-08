using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	private const string IdentityInlineTemplate = "__arg1";

	private const string DefaultModelNameInlineTemplate = "\"modelValue\"";

	private const string ModelUpdateEventNameInlineTemplate = "`update:${__arg1}`";

	/// <summary>
	/// 返回当前 setup 上下文的透传属性。
	/// Returns the fallthrough attributes from the current setup context.
	/// </summary>
	/// <returns>当前组件的透传属性包。The current component's fallthrough attribute bag.</returns>
	[Description("@#useAttrs")]
	public extern static VueAttributeBag UseAttrs();

	/// <summary>
	/// 以用户声明的类型投影返回透传属性。
	/// Returns the fallthrough attributes as a user-declared typed projection.
	/// This does not convert the runtime object; it only gives C# IntelliSense for
	/// known attribute keys.
	/// </summary>
	/// <typeparam name="TAttrs">类型化的属性投影记录。The typed attribute projection record.</typeparam>
	/// <returns>当前组件的透传属性，投影为 <typeparamref name="TAttrs"/>。The current component's fallthrough attributes projected as <typeparamref name="TAttrs"/>.</returns>
	[Description("@#useAttrs")]
	public extern static TAttrs UseAttrs<TAttrs>()
		where TAttrs : VueProps;

	/// <summary>
	/// 返回当前 setup 上下文的插槽对象。
	/// Returns the slots object from the current setup context.
	/// </summary>
	/// <returns>当前组件的插槽包。The current component's slot bag.</returns>
	[Description("@#useSlots")]
	public extern static VueSlotBag UseSlots();

	/// <summary>
	/// 以用户声明的类型投影返回插槽对象。
	/// Returns the slots object as a user-declared typed slot projection.
	/// </summary>
	/// <typeparam name="TSlots">类型化的插槽投影记录。The typed slots projection record.</typeparam>
	/// <returns>当前组件的插槽，投影为 <typeparamref name="TSlots"/>。The current component's slots projected as <typeparamref name="TSlots"/>.</returns>
	[Description("@#useSlots")]
	public extern static TSlots UseSlots<TSlots>()
		where TSlots : VueSlots;

	/// <summary>
	/// 创建一个与模板 <c>ref</c> 键关联的只读模板引用。
	/// Creates a readonly template ref linked to a template <c>ref</c> key.
	/// </summary>
	/// <typeparam name="TElement">引用所期望的元素或组件实例类型。The element or component instance type expected for the ref.</typeparam>
	/// <param name="key">模板 ref 键。The template ref key.</param>
	/// <returns>一个只读引用，其值在挂载后填充、卸载时重置。A readonly ref whose value is populated after mount and reset on unmount.</returns>
	[Description("@#useTemplateRef")]
	public extern static VueReadonlyRef<TElement?> UseTemplateRef<TElement>(string key)
		where TElement : class;

	/// <summary>
	/// 生成一个稳定的、应用内唯一的 id，适用于 SSR 水合。
	/// Generates a stable per-application unique id that is safe for SSR hydration.
	/// </summary>
	/// <returns>当前应用实例的唯一 id 字符串。A unique id string for the current app instance.</returns>
	[Description("@#useId")]
	public extern static string UseId();

	/// <summary>
	/// 创建一个由已声明 prop 及其对应 <c>update:*</c> 事件支撑的可写模型引用。
	/// Creates a writable model ref backed by a declared prop and its corresponding
	/// <c>update:*</c> event. The component must still declare the prop and emit entry.
	/// </summary>
	/// <typeparam name="TValue">模型值类型。The model value type.</typeparam>
	/// <param name="props">Vue 提供的 setup props 对象。The setup props object supplied by Vue.</param>
	/// <param name="key">最终运行时 prop 键，例如 <c>"modelValue"</c>。The final runtime prop key, such as <c>"modelValue"</c>.</param>
	/// <returns>与命名模型 prop 关联的可写引用。A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static VueModelRef<TValue> UseModel<TValue>(VueProps props, string key);

	/// <summary>
	/// 创建一个由强类型模型名契约支撑的可写模型引用。
	/// Creates a writable model ref backed by a strongly typed model-name contract.
	/// The model wrapper still erases to the final runtime prop key string.
	/// </summary>
	/// <typeparam name="TProps">当前组件使用的类型化 props 契约。The typed props contract used by the current component.</typeparam>
	/// <typeparam name="TValue">模型值类型。The model value type.</typeparam>
	/// <param name="props">Vue 提供的 setup props 对象。The setup props object supplied by Vue.</param>
	/// <param name="model">描述 prop 键的类型化模型名契约。The typed model-name contract describing the prop key.</param>
	/// <returns>与命名模型 prop 关联的可写引用。A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static VueModelRef<TValue> UseModel<TProps, TValue>(TProps props, VueModelName<TProps, TValue> model)
		where TProps : VueProps;

	/// <summary>
	/// 创建一个带读/写转换的可写模型引用。
	/// Creates a writable model ref with read/write transforms.
	/// </summary>
	/// <typeparam name="TValue">模型值类型。The model value type.</typeparam>
	/// <param name="props">Vue 提供的 setup props 对象。The setup props object supplied by Vue.</param>
	/// <param name="key">最终运行时 prop 键，例如 <c>"modelValue"</c>。The final runtime prop key, such as <c>"modelValue"</c>.</param>
	/// <param name="options">Vue 模型辅助工具应用的读/写转换。Read/write transforms applied by Vue's model helper.</param>
	/// <returns>与命名模型 prop 关联的可写引用。A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static VueModelRef<TValue> UseModel<TValue>(VueProps props, string key, VueModelOptions<TValue> options);

	/// <summary>
	/// 使用强类型模型名契约创建带读/写转换的可写模型引用。
	/// Creates a writable model ref with read/write transforms using a strongly typed
	/// model-name contract.
	/// </summary>
	/// <typeparam name="TProps">当前组件使用的类型化 props 契约。The typed props contract used by the current component.</typeparam>
	/// <typeparam name="TValue">模型值类型。The model value type.</typeparam>
	/// <param name="props">Vue 提供的 setup props 对象。The setup props object supplied by Vue.</param>
	/// <param name="model">描述 prop 键的类型化模型名契约。The typed model-name contract describing the prop key.</param>
	/// <param name="options">Vue 模型辅助工具应用的读/写转换。Read/write transforms applied by Vue's model helper.</param>
	/// <returns>与命名模型 prop 关联的可写引用。A writable ref linked to the named model prop.</returns>
	[Description("@#useModel")]
	public extern static VueModelRef<TValue> UseModel<TProps, TValue>(TProps props, VueModelName<TProps, TValue> model, VueModelOptions<TValue> options)
		where TProps : VueProps;

	/// <summary>
	/// 为 <c>modelValue</c> 创建默认的 Vue 模型名契约。此为编译期辅助工具，擦除为最终运行时 prop 键。
	/// Creates the default Vue model-name contract for <c>modelValue</c>.
	/// This is a compile-time helper only and erases to the final runtime prop key.
	/// </summary>
	/// <typeparam name="TProps">与此模型关联的类型化 props 契约。The typed props contract associated with this model.</typeparam>
	/// <typeparam name="TValue">模型值类型。The model value type.</typeparam>
	/// <returns><c>modelValue</c> 的类型化模型名契约。A typed model-name contract for <c>modelValue</c>.</returns>
	[ECMAScriptInline(DefaultModelNameInlineTemplate)]
	public extern static VueModelName<TProps, TValue> ModelName<TProps, TValue>()
		where TProps : VueProps;

	/// <summary>
	/// 为命名模型 prop 创建强类型模型名契约。
	/// Creates a strongly typed model-name contract for a named model prop.
	/// </summary>
	/// <typeparam name="TProps">与此模型关联的类型化 props 契约。The typed props contract associated with this model.</typeparam>
	/// <typeparam name="TValue">模型值类型。The model value type.</typeparam>
	/// <param name="key">最终运行时 prop 键，例如 <c>"count"</c>。The final runtime prop key, such as <c>"count"</c>.</param>
	/// <returns>所提供运行时 prop 键的类型化模型名契约。A typed model-name contract for the supplied runtime prop key.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static VueModelName<TProps, TValue> ModelName<TProps, TValue>(string key)
		where TProps : VueProps;

	/// <summary>
	/// 从类型化模型名契约中读取运行时 prop 键。适用于声明 <c>props</c> 数组时避免重复字符串字面量。
	/// Reads the runtime prop key from a typed model-name contract.
	/// This is useful when declaring <c>props</c> arrays without repeating string literals.
	/// </summary>
	/// <typeparam name="TProps">与此模型关联的类型化 props 契约。The typed props contract associated with this model.</typeparam>
	/// <typeparam name="TValue">模型值类型。The model value type.</typeparam>
	/// <param name="model">类型化模型名契约。The typed model-name contract.</param>
	/// <returns>最终运行时 prop 键。The final runtime prop key.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static string ModelPropName<TProps, TValue>(VueModelName<TProps, TValue> model)
		where TProps : VueProps;

	/// <summary>
	/// 为类型化模型名契约构建对应的 <c>update:*</c> 事件名，保持命名模型事件声明与同一源模型键对齐。
	/// Builds the corresponding <c>update:*</c> event name for a typed model-name contract.
	/// This keeps named-model event declarations aligned with the same source model key.
	/// </summary>
	/// <typeparam name="TProps">与此模型关联的类型化 props 契约。The typed props contract associated with this model.</typeparam>
	/// <typeparam name="TValue">模型值类型。The model value type.</typeparam>
	/// <param name="model">类型化模型名契约。The typed model-name contract.</param>
	/// <returns>最终运行时 update 事件名，例如 <c>"update:count"</c>。The final runtime update event name, such as <c>"update:count"</c>.</returns>
	[ECMAScriptInline(ModelUpdateEventNameInlineTemplate)]
	public extern static string ModelUpdateEventName<TProps, TValue>(VueModelName<TProps, TValue> model)
		where TProps : VueProps;

	/// <summary>
	/// 在自定义元素 setup 上下文中返回当前的 Vue 自定义元素宿主。
	/// Returns the current Vue custom element host while running inside a custom
	/// element setup context.
	/// </summary>
	/// <returns>当前自定义元素宿主，不可用时为 <c>null</c>。The current custom element host, or <c>null</c> when unavailable.</returns>
	[Description("@#useHost")]
	public extern static HTMLElement? UseHost();

	/// <summary>
	/// 将当前 Vue 自定义元素宿主投影为类型化宿主元素。此为类型投影，不创建新的运行时包装。
	/// Returns the current Vue custom element host projected to a typed host element.
	/// This is a typed projection only and does not create a new runtime wrapper.
	/// </summary>
	/// <typeparam name="THost">期望的自定义元素宿主类型。The expected custom element host type.</typeparam>
	/// <returns>当前自定义元素宿主，不可用时为 <c>null</c>。The current custom element host, or <c>null</c> when unavailable.</returns>
	[Description("@#useHost")]
	public extern static THost? UseHost<THost>()
		where THost : HTMLElement;

	/// <summary>
	/// 在自定义元素 setup 上下文中返回当前的 Vue 自定义元素 shadow root。
	/// Returns the current Vue custom element shadow root while running inside a
	/// custom element setup context.
	/// </summary>
	/// <returns>当前自定义元素 shadow root，不可用时为 <c>null</c>。The current custom element shadow root, or <c>null</c> when unavailable.</returns>
	[Description("@#useShadowRoot")]
	public extern static ShadowRoot? UseShadowRoot();

	/// <summary>
	/// 从当前组件 setup 上下文向后代组件提供值。
	/// Provides a value from the current component setup context to descendant components.
	/// </summary>
	/// <typeparam name="TValue">值的类型。The value type.</typeparam>
	/// <param name="key">注入键。The injection key.</param>
	/// <param name="value">要提供的值。The value to provide.</param>
	[Description("@#provide")]
	public extern static void Provide<TValue>(string key, TValue value);

	/// <summary>
	/// 使用强类型注入键从当前组件 setup 上下文提供值。
	/// Provides a value from the current component setup context using a strongly typed
	/// injection key.
	/// </summary>
	/// <typeparam name="TValue">与注入键关联的值类型。The value type associated with the injection key.</typeparam>
	/// <param name="key">类型化注入键符号。The typed injection key symbol.</param>
	/// <param name="value">要提供的值。The value to provide.</param>
	[Description("@#provide")]
	public extern static void Provide<TValue>(VueInjectionKey<TValue> key, TValue value);

	/// <summary>
	/// 使用字符串键从最近的祖先提供者注入值。
	/// Injects a value from the nearest ancestor provider using a string key.
	/// </summary>
	/// <typeparam name="TValue">期望的值类型。The expected value type.</typeparam>
	/// <param name="key">注入键。The injection key.</param>
	/// <returns>存在时返回注入的值；否则为 <c>null</c> / <c>undefined</c>。The injected value when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
	[Description("@#inject")]
	public extern static TValue? Inject<TValue>(string key);

	/// <summary>
	/// 使用强类型注入键从最近的祖先提供者注入值。
	/// Injects a value from the nearest ancestor provider using a strongly typed
	/// injection key.
	/// </summary>
	/// <typeparam name="TValue">与注入键关联的值类型。The value type associated with the injection key.</typeparam>
	/// <param name="key">类型化注入键符号。The typed injection key symbol.</param>
	/// <returns>存在时返回注入的值；否则为 <c>null</c> / <c>undefined</c>。The injected value when present; otherwise <c>null</c> / <c>undefined</c>.</returns>
	[Description("@#inject")]
	public extern static TValue? Inject<TValue>(VueInjectionKey<TValue> key);

	/// <summary>
	/// 使用字符串键注入值，无提供者时返回默认值。
	/// Injects a value using a string key, returning a default value when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">期望的值类型。The expected value type.</typeparam>
	/// <param name="key">注入键。The injection key.</param>
	/// <param name="defaultValue">无提供者时使用的默认值。The default value used when no provider exists.</param>
	/// <returns>注入的值或所提供的默认值。The injected value or the supplied default value.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(string key, TValue defaultValue);

	/// <summary>
	/// 使用强类型注入键注入值，无提供者时返回默认值。
	/// Injects a value using a strongly typed injection key, returning a default value
	/// when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">与注入键关联的值类型。The value type associated with the injection key.</typeparam>
	/// <param name="key">类型化注入键符号。The typed injection key symbol.</param>
	/// <param name="defaultValue">无提供者时使用的默认值。The default value used when no provider exists.</param>
	/// <returns>注入的值或所提供的默认值。The injected value or the supplied default value.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(VueInjectionKey<TValue> key, TValue defaultValue);

	/// <summary>
	/// 使用字符串键注入值，无提供者时执行默认工厂函数。
	/// Injects a value using a string key, evaluating a default factory when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">期望的值类型。The expected value type.</typeparam>
	/// <param name="key">注入键。The injection key.</param>
	/// <param name="defaultFactory">无提供者时使用的工厂函数。Factory used when no provider exists.</param>
	/// <param name="treatDefaultAsFactory">传入 <c>true</c> 以使 Vue 将第二个参数视为工厂函数。Pass <c>true</c> so Vue treats the second argument as a factory.</param>
	/// <returns>注入的值或工厂函数结果。The injected value or the factory result.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(string key, Func<TValue> defaultFactory, bool treatDefaultAsFactory = true);

	/// <summary>
	/// 使用强类型注入键注入值，无提供者时执行默认工厂函数。
	/// Injects a value using a strongly typed injection key, evaluating a default factory
	/// when no provider exists.
	/// </summary>
	/// <typeparam name="TValue">与注入键关联的值类型。The value type associated with the injection key.</typeparam>
	/// <param name="key">类型化注入键符号。The typed injection key symbol.</param>
	/// <param name="defaultFactory">无提供者时使用的工厂函数。Factory used when no provider exists.</param>
	/// <param name="treatDefaultAsFactory">传入 <c>true</c> 以使 Vue 将第二个参数视为工厂函数。Pass <c>true</c> so Vue treats the second argument as a factory.</param>
	/// <returns>注入的值或工厂函数结果。The injected value or the factory result.</returns>
	[Description("@#inject")]
	public extern static TValue Inject<TValue>(VueInjectionKey<TValue> key, Func<TValue> defaultFactory, bool treatDefaultAsFactory = true);

	/// <summary>
	/// 返回当前调用栈是否存在活动的注入上下文。
	/// Returns whether the current call stack has an active injection context.
	/// </summary>
	/// <returns>当 <c>inject()</c> 可安全使用（不产生警告）时为 <c>true</c>。<c>true</c> when <c>inject()</c> can be used without a warning.</returns>
	[Description("@#hasInjectionContext")]
	public extern static bool HasInjectionContext();

	/// <summary>
	/// 创建一个新的作用域。在该作用域内创建的副作用可以被统一停止。
	/// Creates a new effect scope. Effects created inside the scope can be stopped together.
	/// </summary>
	/// <param name="detached">为 <c>true</c> 时创建独立作用域，不链接到当前活动作用域。When <c>true</c>, create a detached scope not linked to the current active scope.</param>
	/// <returns>新的副作用作用域。A new effect scope.</returns>
	[Description("@#effectScope")]
	public extern static VueEffectScope EffectScope(bool detached = false);

	/// <summary>
	/// 返回当前活动的副作用作用域（如果存在）。
	/// Returns the currently active effect scope, if one exists.
	/// </summary>
	/// <returns>可用的当前副作用作用域；否则为 <c>null</c> / <c>undefined</c>。The current effect scope when available; otherwise <c>null</c> / <c>undefined</c>.</returns>
	[Description("@#getCurrentScope")]
	public extern static VueEffectScope? GetCurrentScope();

	/// <summary>
	/// 在当前活动的副作用作用域上注册清理回调。
	/// Registers a cleanup callback on the current active effect scope.
	/// </summary>
	/// <param name="callback">当前作用域停止时运行的清理回调。The cleanup callback to run when the current scope is stopped.</param>
	[Description("@#onScopeDispose")]
	public extern static void OnScopeDispose(Action callback);

	/// <summary>
	/// 在当前活动的副作用作用域上注册清理回调。
	/// Registers a cleanup callback on the current active effect scope.
	/// </summary>
	/// <param name="callback">当前作用域停止时运行的清理回调。The cleanup callback to run when the current scope is stopped.</param>
	/// <param name="failSilently">为 <c>true</c> 时，Vue 抑制缺少作用域的警告。When <c>true</c>, Vue suppresses the missing-scope warning.</param>
	[Description("@#onScopeDispose")]
	public extern static void OnScopeDispose(Action callback, bool failSilently);

}
