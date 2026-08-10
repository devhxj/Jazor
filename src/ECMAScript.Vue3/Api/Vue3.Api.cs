using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>Vue 应用创建、组件注册和基础 runtime API。</summary>
/// <remarks>调用直接映射 Vue 3 host module；compiler 不在此分片实现应用生命周期。</remarks>
public static partial class Vue3
{
	/// <summary>
	/// 当前 Vue 运行时版本。
	/// The current Vue runtime version.
	/// </summary>
	[Description("@#version")]
	public extern static string Version { get; }

	/// <summary>
	/// 从根组件创建 Vue 应用实例。返回的
	/// <see cref="VueApp"/> 可在挂载前配置插件、全局组件和指令。
	/// Creates a Vue application instance from a root component. The returned
	/// <see cref="VueApp"/> can be configured with plugins, global components, and
	/// directives before mounting.
	/// </summary>
	/// <param name="rootComponent">由 <c>defineComponent()</c> 产生的根组件定义。The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <returns>一个准备好配置和挂载的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp(IVueComponent rootComponent);

	/// <summary>
	/// 创建带有根 props 的 Vue 应用实例，根 props 在挂载时传递给根组件。
	/// Creates a Vue application instance with root props passed to the root component
	/// during mounting.
	/// </summary>
	/// <param name="rootComponent">由 <c>defineComponent()</c> 产生的根组件定义。The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <param name="rootProps">挂载时传递给根组件的 props。Props to pass to the root component when it mounts.</param>
	/// <returns>一个准备好配置和挂载的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp(IVueComponent rootComponent, VueProps rootProps);

	/// <summary>
	/// 创建带有强类型根 props 的 Vue 应用实例。
	/// Creates a Vue application instance with strongly typed root props.
	/// </summary>
	/// <typeparam name="TProps">根组件 props 契约。The root component props contract.</typeparam>
	/// <param name="rootComponent">类型化的根组件定义。The typed root component definition.</param>
	/// <param name="rootProps">强类型的根 props 对象。The strongly typed root props object.</param>
	/// <returns>一个准备好配置和挂载的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps>(IVueComponent<TProps> rootComponent, TProps rootProps)
		where TProps : VueProps;

	/// <summary>
	/// 创建带有强类型根 props 和 <see cref="VueObject{TProps}"/> 公开的常用便捷成员的 Vue 应用实例。
	/// Creates a Vue application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/>.
	/// </summary>
	/// <typeparam name="TProps">根组件 props 契约。The root component props contract.</typeparam>
	/// <param name="rootComponent">类型化的根组件定义。The typed root component definition.</param>
	/// <param name="rootProps">一个类型化的 Vue 对象，展开 <typeparamref name="TProps"/> 并
	/// 同时允许常用创作便捷方式如 <c>class</c>、<c>style</c> 和展开运算符。A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>一个准备好配置和挂载的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps>(IVueComponent<TProps> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps;

	/// <summary>
	/// 为同时声明了类型化插槽的组件创建带有强类型根 props 的 Vue 应用实例。
	/// Creates a Vue application instance with strongly typed root props for a component
	/// that also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">根组件 props 契约。The root component props contract.</typeparam>
	/// <typeparam name="TSlots">根组件插槽契约。The root component slots contract.</typeparam>
	/// <param name="rootComponent">完全类型化的根组件定义。The fully typed root component definition.</param>
	/// <param name="rootProps">强类型的根 props 对象。The strongly typed root props object.</param>
	/// <returns>一个准备好配置和挂载的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, TProps rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// 为同时声明了类型化插槽的组件创建带有强类型根 props 和
	/// <see cref="VueObject{TProps}"/> 公开的常用便捷成员的 Vue 应用实例。
	/// Creates a Vue application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/> for a component that
	/// also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">根组件 props 契约。The root component props contract.</typeparam>
	/// <typeparam name="TSlots">根组件插槽契约。The root component slots contract.</typeparam>
	/// <param name="rootComponent">完全类型化的根组件定义。The fully typed root component definition.</param>
	/// <param name="rootProps">一个类型化的 Vue 对象，展开 <typeparamref name="TProps"/> 并
	/// 同时允许常用创作便捷方式如 <c>class</c>、<c>style</c> 和展开运算符。A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>一个准备好配置和挂载的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// 以 SSR（服务端渲染）模式创建 Vue 应用实例。在 SSR 模式下，
	/// Vue 将组件树渲染为 HTML 字符串而非 DOM 节点。
	/// Creates a Vue application instance in SSR (server-side rendering) mode. In SSR mode,
	/// Vue renders the component tree to HTML strings instead of DOM nodes.
	/// </summary>
	/// <param name="rootComponent">由 <c>defineComponent()</c> 产生的根组件定义。The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <returns>一个配置为服务端渲染的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp(IVueComponent rootComponent);

	/// <summary>
	/// 创建带有根 props 的 SSR 模式 Vue 应用实例。
	/// Creates a Vue application instance in SSR mode with root props.
	/// </summary>
	/// <param name="rootComponent">由 <c>defineComponent()</c> 产生的根组件定义。The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <param name="rootProps">服务端渲染期间传递给根组件的 props。Props to pass to the root component during server-side rendering.</param>
	/// <returns>一个配置为服务端渲染的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp(IVueComponent rootComponent, VueProps rootProps);

	/// <summary>
	/// 创建带有强类型根 props 的 Vue SSR 应用实例。
	/// Creates a Vue SSR application instance with strongly typed root props.
	/// </summary>
	/// <typeparam name="TProps">根组件 props 契约。The root component props contract.</typeparam>
	/// <param name="rootComponent">类型化的根组件定义。The typed root component definition.</param>
	/// <param name="rootProps">强类型的根 props 对象。The strongly typed root props object.</param>
	/// <returns>一个配置为服务端渲染的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp<TProps>(IVueComponent<TProps> rootComponent, TProps rootProps)
		where TProps : VueProps;

	/// <summary>
	/// 创建带有强类型根 props 和 <see cref="VueObject{TProps}"/> 公开的常用便捷成员的 Vue SSR 应用实例。
	/// Creates a Vue SSR application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/>.
	/// </summary>
	/// <typeparam name="TProps">根组件 props 契约。The root component props contract.</typeparam>
	/// <param name="rootComponent">类型化的根组件定义。The typed root component definition.</param>
	/// <param name="rootProps">一个类型化的 Vue 对象，展开 <typeparamref name="TProps"/> 并
	/// 同时允许常用创作便捷方式如 <c>class</c>、<c>style</c> 和展开运算符。A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>一个配置为服务端渲染的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp<TProps>(IVueComponent<TProps> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps;

	/// <summary>
	/// 为同时声明了类型化插槽的组件创建带有强类型根 props 的 Vue SSR 应用实例。
	/// Creates a Vue SSR application instance with strongly typed root props for a component
	/// that also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">根组件 props 契约。The root component props contract.</typeparam>
	/// <typeparam name="TSlots">根组件插槽契约。The root component slots contract.</typeparam>
	/// <param name="rootComponent">完全类型化的根组件定义。The fully typed root component definition.</param>
	/// <param name="rootProps">强类型的根 props 对象。The strongly typed root props object.</param>
	/// <returns>一个配置为服务端渲染的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, TProps rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// 为同时声明了类型化插槽的组件创建带有强类型根 props 和
	/// <see cref="VueObject{TProps}"/> 公开的常用便捷成员的 Vue SSR 应用实例。
	/// Creates a Vue SSR application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/> for a component that
	/// also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">根组件 props 契约。The root component props contract.</typeparam>
	/// <typeparam name="TSlots">根组件插槽契约。The root component slots contract.</typeparam>
	/// <param name="rootComponent">完全类型化的根组件定义。The fully typed root component definition.</param>
	/// <param name="rootProps">一个类型化的 Vue 对象，展开 <typeparamref name="TProps"/> 并
	/// 同时允许常用创作便捷方式如 <c>class</c>、<c>style</c> 和展开运算符。A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>一个配置为服务端渲染的新 <see cref="VueApp"/> 实例。A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// 从无类型 props 的选项对象定义 Vue 组件。用于不声明类型 props 或插槽的简单组件。
	/// Defines a Vue component from an options object with no typed props. Use this overload
	/// for simple components that do not declare typed props or slots.
	/// </summary>
	/// <param name="options">包含 setup/render、name 和注册的组件选项。The component options including setup/render, name, and registrations.</param>
	/// <returns>可传递给 <c>h()</c> 或全局注册的 <see cref="IVueComponent"/>。An <see cref="IVueComponent"/> that can be passed to <c>h()</c> or registered globally.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent DefineComponent(VueComponentDefinition options);

	/// <summary>
	/// 定义带有类型化 props 的 Vue 组件。泛型参数强制 C# props
	/// 创作方式；运行时 prop 和 emit 声明仅在选项对象上提供时才会发射。
	/// Defines a Vue component with typed props. The generic parameter enforces C# props
	/// authoring; runtime prop and emit declarations are emitted only when supplied on
	/// the options object.
	/// </summary>
	/// <typeparam name="TProps">描述组件接受的 props 的 props 记录类型。The props record type describing the component's accepted props.</typeparam>
	/// <param name="options">包含 setup、name 和注册的类型化组件选项。The typed component options including setup, name, and registrations.</param>
	/// <returns>在 <c>h()</c> 中强制类型化 props 的 <see cref="IVueComponent{TProps}"/>。An <see cref="IVueComponent{TProps}"/> that enforces typed props in <c>h()</c>.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent<TProps> DefineComponent<TProps>(VueComponentOptions<TProps> options)
		where TProps : VueProps;

	/// <summary>
	/// 定义带有类型化插槽但无类型 props 的 Vue 组件。用于接受命名插槽但不声明类型 props 的组件。
	/// Defines a Vue component with typed slots but no typed props. Use this overload for
	/// components that accept named slots but do not declare typed props.
	/// </summary>
	/// <typeparam name="TSlots">描述组件接受的插槽的插槽记录类型。The slots record type describing the component's accepted slots.</typeparam>
	/// <param name="options">包含 setup 和注册的类型化组件选项。The typed component options including setup and registrations.</param>
	/// <returns>在 <c>h()</c> 中强制类型化插槽的 <see cref="IVueSlotComponent{TSlots}"/>。An <see cref="IVueSlotComponent{TSlots}"/> that enforces typed slots in <c>h()</c>.</returns>
	[Description("@#defineComponent")]
	public extern static IVueSlotComponent<TSlots> DefineComponent<TSlots>(VueSlotComponentOptions<TSlots> options)
		where TSlots : VueSlots;

	/// <summary>
	/// 同时带有类型化 props 和类型化插槽的 Vue 组件定义。这是最强的
	/// 类型化重载，在 <c>h()</c> 调用中同时强制 prop 和 slot 类型。
	/// Defines a Vue component with both typed props and typed slots. This is the most
	/// strongly-typed overload, enforcing both prop and slot types in <c>h()</c> calls.
	/// </summary>
	/// <typeparam name="TProps">描述组件接受的 props 的 props 记录类型。The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">描述组件接受的插槽的插槽记录类型。The slots record type describing the component's accepted slots.</typeparam>
	/// <param name="options">包含 setup 和注册的完全类型化组件选项。The fully typed component options including setup and registrations.</param>
	/// <returns>在 <c>h()</c> 中同时强制 props 和 slots 的 <see cref="IVueComponent{TProps, TSlots}"/>。An <see cref="IVueComponent{TProps, TSlots}"/> that enforces both props and slots in <c>h()</c>.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent<TProps, TSlots> DefineComponent<TProps, TSlots>(VueComponentOptions<TProps, TSlots> options)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// 从现有组件选项对象定义由 Vue 驱动的自定义元素。
	/// 返回的构造函数可注册到浏览器的 <see cref="CustomElementRegistry"/>。
	/// Defines a Vue-powered custom element from an existing component options object.
	/// The returned constructor can be registered with the browser's
	/// <see cref="CustomElementRegistry"/>.
	/// </summary>
	/// <param name="options">用于渲染自定义元素的组件选项。The component options used to render the custom element.</param>
	/// <returns>兼容 <c>customElements.define()</c> 的自定义元素构造函数。A custom element constructor compatible with <c>customElements.define()</c>.</returns>
	[Description("@#defineCustomElement")]
	public extern static CustomElementConstructor DefineCustomElement(VueComponentDefinition options);

	/// <summary>
	/// 从组件选项和自定义元素特有的运行时选项定义由 Vue 驱动的自定义元素。
	/// Defines a Vue-powered custom element from component options plus
	/// custom-element-specific runtime options.
	/// </summary>
	/// <param name="options">用于渲染自定义元素的组件选项。The component options used to render the custom element.</param>
	/// <param name="customElementOptions">自定义元素特有的样式、应用配置、shadow root 和 CSP 选项。Custom-element-specific styles, app configuration, shadow root, and CSP options.</param>
	/// <returns>兼容 <c>customElements.define()</c> 的自定义元素构造函数。A custom element constructor compatible with <c>customElements.define()</c>.</returns>
	[Description("@#defineCustomElement")]
	public extern static CustomElementConstructor DefineCustomElement(VueComponentDefinition options, VueCustomElementOptions customElementOptions);

	/// <summary>
	/// 从加载回调定义异步组件。
	/// Defines an async component from a loader callback.
	/// </summary>
	/// <param name="loader">返回解析为组件定义的 promise 的回调。A callback returning a promise that resolves to the component definition.</param>
	/// <returns>可以像普通组件一样渲染或注册的异步组件引用。An async component reference that can be rendered or registered like a normal component.</returns>
	[Description("@#defineAsyncComponent")]
	public extern static IVueComponent DefineAsyncComponent(VueAsyncComponentLoader loader);

	/// <summary>
	/// 从对象形式的选项定义异步组件。
	/// Defines an async component from object-form options.
	/// </summary>
	/// <param name="options">异步组件加载、错误、计时和重试选项。Async component loading, error, timing, and retry options.</param>
	/// <returns>可以像普通组件一样渲染或注册的异步组件引用。An async component reference that can be rendered or registered like a normal component.</returns>
	[Description("@#defineAsyncComponent")]
	public extern static IVueComponent DefineAsyncComponent(VueAsyncComponentOptions options);

	/// <summary>
	/// 从对象形式的选项定义强类型异步组件。
	/// Defines a strongly typed async component from object-form options.
	/// </summary>
	/// <typeparam name="TComponent">加载器产生的组件契约。The component contract produced by the loader.</typeparam>
	/// <param name="options">类型化异步组件加载、错误、计时和重试选项。Typed async component loading, error, timing, and retry options.</param>
	/// <returns>保留 prop/slot 契约的类型化异步组件引用。A typed async component reference that preserves prop/slot contracts.</returns>
	[Description("@#defineAsyncComponent")]
	public extern static TComponent DefineAsyncComponent<TComponent>(VueAsyncComponentOptions<TComponent> options)
		where TComponent : ECMAScript.Vue3.IVueComponent;

	/// <summary>
	/// Vue 内置的 <c>Transition</c> 组件，用于动画化单个元素或组件的进入和离开。
	/// Vue's built-in <c>Transition</c> component for animating a single element or
	/// component entering and leaving.
	/// </summary>
	[Description("@#Transition")]
	public extern static IVueComponent<VueTransitionProps, VueDefaultSlots> Transition { get; }

	/// <summary>
	/// Vue 内置的 <c>TransitionGroup</c> 组件，用于动画化列表的插入、删除和移动。
	/// Vue's built-in <c>TransitionGroup</c> component for animating list insertions,
	/// removals, and moves.
	/// </summary>
	[Description("@#TransitionGroup")]
	public extern static IVueComponent<VueTransitionGroupProps, VueDefaultSlots> TransitionGroup { get; }

	/// <summary>
	/// Vue 内置的 <c>KeepAlive</c> 组件，用于缓存非活动的动态组件实例。
	/// Vue's built-in <c>KeepAlive</c> component for caching inactive dynamic component
	/// instances.
	/// </summary>
	[Description("@#KeepAlive")]
	public extern static IVueComponent<VueKeepAliveProps, VueDefaultSlots> KeepAlive { get; }

	/// <summary>
	/// Vue 内置的 <c>Teleport</c> 组件，用于将子内容渲染到另一个 DOM 容器中。
	/// Vue's built-in <c>Teleport</c> component for rendering children into another DOM
	/// container.
	/// </summary>
	[Description("@#Teleport")]
	public extern static IVueComponent<VueTeleportProps, VueDefaultSlots> Teleport { get; }

	/// <summary>
	/// Vue 内置的 <c>Suspense</c> 组件，用于协调异步依赖，配合默认和后备插槽使用。
	/// Vue's built-in <c>Suspense</c> component for coordinating async dependencies
	/// with default and fallback slots.
	/// </summary>
	[Description("@#Suspense")]
	public extern static IVueComponent<VueSuspenseProps, VueSuspenseSlots> Suspense { get; }

	/// <summary>
	/// 使用 Vue 的 VNode props 合并语义合并多个 props 对象。
	/// Merges multiple props objects using Vue's VNode props merge semantics.
	/// </summary>
	/// <param name="props">要合并的 props 对象。The props objects to merge.</param>
	/// <returns>适用于 <c>h(...)</c> 和 <c>cloneVNode(...)</c> 的合并后 props 对象。A merged props object suitable for <c>h(...)</c> and <c>cloneVNode(...)</c>.</returns>
	[Description("@#mergeProps")]
	public extern static VueProps MergeProps(params VueProps[] props);

	/// <summary>
	/// 克隆现有 VNode。
	/// Clones an existing VNode.
	/// </summary>
	/// <param name="vnode">要克隆的 VNode。The VNode to clone.</param>
	/// <returns>克隆后的 VNode。A cloned VNode.</returns>
	[Description("@#cloneVNode")]
	public extern static IVNode CloneVNode(IVNode vnode);

	/// <summary>
	/// 克隆现有 VNode 并合并额外的 props。
	/// Clones an existing VNode and merges extra props into it.
	/// </summary>
	/// <param name="vnode">要克隆的 VNode。The VNode to clone.</param>
	/// <param name="extraProps">要合并到克隆中的额外 props。Additional props to merge into the clone.</param>
	/// <returns>合并了 props 的克隆 VNode。A cloned VNode with merged props.</returns>
	[Description("@#cloneVNode")]
	public extern static IVNode CloneVNode(IVNode vnode, VueProps extraProps);

	/// <summary>
	/// 返回提供的运行时值是否为 Vue VNode。
	/// Returns whether the supplied runtime value is a Vue VNode.
	/// </summary>
	/// <typeparam name="T">正在测试的运行时值的静态类型。The static type of the runtime value being tested.</typeparam>
	/// <param name="value">要测试的运行时值。The runtime value to test.</param>
	/// <returns>值为 VNode 时返回 <c>true</c>。<c>true</c> when the value is a VNode.</returns>
	[Description("@#isVNode")]
	public extern static bool IsVNode<T>(T value);

	/// <summary>
	/// 通过名称从当前组件/应用上下文解析组件。
	/// Resolves a component by name from the current component/app context.
	/// </summary>
	/// <param name="name">已注册的组件名称。The registered component name.</param>
	/// <returns>解析到的组件。The resolved component.</returns>
	[Description("@#resolveComponent")]
	public extern static IVueComponent ResolveComponent(string name);

	/// <summary>
	/// 通过名称从当前组件/应用上下文解析指令。
	/// Resolves a directive by name from the current component/app context.
	/// </summary>
	/// <param name="name">已注册的指令名称。The registered directive name.</param>
	/// <returns>解析到的指令，不可用时返回 <c>null</c>。The resolved directive, or <c>null</c> when unavailable.</returns>
	[Description("@#resolveDirective")]
	public extern static VueDirectiveValue? ResolveDirective(string name);

	/// <summary>
	/// 将运行时指令应用到由 <see cref="H(string)"/> 或组件渲染调用创建的 VNode 上。
	/// Applies runtime directives to a VNode created by <see cref="H(string)"/> or a
	/// component render call.
	/// </summary>
	/// <param name="vnode">要装饰的 VNode。The VNode to decorate.</param>
	/// <param name="directives">匹配 Vue 运行时契约的指令参数元组。Directive argument tuples matching Vue's runtime contract.</param>
	/// <returns>附加了指令元数据的同一 VNode。The same VNode with directive metadata attached.</returns>
	[Description("@#withDirectives")]
	public extern static IVNode WithDirectives(IVNode vnode, [PreserveParamsArray] params VueDirectiveArguments[] directives);

	/// <summary>
	/// 用 Vue 事件修饰符（如 <c>stop</c>、<c>prevent</c> 或 <c>self</c>）包装无参数事件处理器。
	/// Wraps a parameterless event handler with Vue event modifiers such as
	/// <c>stop</c>, <c>prevent</c>, or <c>self</c>.
	/// </summary>
	/// <param name="handler">原始事件处理器。The original event handler.</param>
	/// <param name="modifiers">Vue 运行时形式的修饰符名称。Modifier names in Vue runtime form.</param>
	/// <returns>包装后的事件处理器。A wrapped event handler.</returns>
	[Description("@#withModifiers")]
	public extern static Action WithModifiers(Action handler, [PreserveParamsArray] params string[] modifiers);

	/// <summary>
	/// 用 Vue 事件修饰符包装类型化事件处理器。
	/// Wraps a typed event handler with Vue event modifiers.
	/// </summary>
	/// <typeparam name="TEvent">事件负载类型。The event payload type.</typeparam>
	/// <param name="handler">原始类型化事件处理器。The original typed event handler.</param>
	/// <param name="modifiers">Vue 运行时形式的修饰符名称。Modifier names in Vue runtime form.</param>
	/// <returns>包装后的类型化事件处理器。A wrapped typed event handler.</returns>
	[Description("@#withModifiers")]
	public extern static VueEventHandler<TEvent> WithModifiers<TEvent>(VueEventHandler<TEvent> handler, [PreserveParamsArray] params string[] modifiers);

}
