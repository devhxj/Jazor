using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// The current Vue runtime version.
	/// </summary>
	[Description("@#version")]
	public extern static string Version { get; }

	/// <summary>
	/// Creates a Vue application instance from a root component. The returned
	/// <see cref="VueApp"/> can be configured with plugins, global components, and
	/// directives before mounting.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp(IVueComponent rootComponent);

	/// <summary>
	/// Creates a Vue application instance with root props passed to the root component
	/// during mounting.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <param name="rootProps">Props to pass to the root component when it mounts.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp(IVueComponent rootComponent, VueProps rootProps);

	/// <summary>
	/// Creates a Vue application instance with strongly typed root props.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <param name="rootComponent">The typed root component definition.</param>
	/// <param name="rootProps">The strongly typed root props object.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps>(IVueComponent<TProps> rootComponent, TProps rootProps)
		where TProps : VueProps;

	/// <summary>
	/// Creates a Vue application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/>.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <param name="rootComponent">The typed root component definition.</param>
	/// <param name="rootProps">A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps>(IVueComponent<TProps> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps;

	/// <summary>
	/// Creates a Vue application instance with strongly typed root props for a component
	/// that also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <typeparam name="TSlots">The root component slots contract.</typeparam>
	/// <param name="rootComponent">The fully typed root component definition.</param>
	/// <param name="rootProps">The strongly typed root props object.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, TProps rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a Vue application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/> for a component that
	/// also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <typeparam name="TSlots">The root component slots contract.</typeparam>
	/// <param name="rootComponent">The fully typed root component definition.</param>
	/// <param name="rootProps">A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>A new <see cref="VueApp"/> instance ready for configuration and mounting.</returns>
	[Description("@#createApp")]
	public extern static VueApp CreateApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a Vue application instance in SSR (server-side rendering) mode. In SSR mode,
	/// Vue renders the component tree to HTML strings instead of DOM nodes.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp(IVueComponent rootComponent);

	/// <summary>
	/// Creates a Vue application instance in SSR mode with root props.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <param name="rootProps">Props to pass to the root component during server-side rendering.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp(IVueComponent rootComponent, VueProps rootProps);

	/// <summary>
	/// Creates a Vue SSR application instance with strongly typed root props.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <param name="rootComponent">The typed root component definition.</param>
	/// <param name="rootProps">The strongly typed root props object.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp<TProps>(IVueComponent<TProps> rootComponent, TProps rootProps)
		where TProps : VueProps;

	/// <summary>
	/// Creates a Vue SSR application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/>.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <param name="rootComponent">The typed root component definition.</param>
	/// <param name="rootProps">A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp<TProps>(IVueComponent<TProps> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps;

	/// <summary>
	/// Creates a Vue SSR application instance with strongly typed root props for a component
	/// that also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <typeparam name="TSlots">The root component slots contract.</typeparam>
	/// <param name="rootComponent">The fully typed root component definition.</param>
	/// <param name="rootProps">The strongly typed root props object.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, TProps rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a Vue SSR application instance with strongly typed root props plus the common
	/// convenience members exposed by <see cref="VueObject{TProps}"/> for a component that
	/// also declares typed slots.
	/// </summary>
	/// <typeparam name="TProps">The root component props contract.</typeparam>
	/// <typeparam name="TSlots">The root component slots contract.</typeparam>
	/// <param name="rootComponent">The fully typed root component definition.</param>
	/// <param name="rootProps">A typed Vue object that flattens <typeparamref name="TProps"/> and
	/// also allows common authoring conveniences such as <c>class</c>, <c>style</c>, and spreads.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSSRApp<TProps, TSlots>(IVueComponent<TProps, TSlots> rootComponent, VueObject<TProps> rootProps)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Defines a Vue component from an options object with no typed props. Use this overload
	/// for simple components that do not declare typed props or slots.
	/// </summary>
	/// <param name="options">The component options including setup/render, name, and registrations.</param>
	/// <returns>An <see cref="IVueComponent"/> that can be passed to <c>h()</c> or registered globally.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent DefineComponent(VueComponentDefinition options);

	/// <summary>
	/// Defines a Vue component with typed props. The generic parameter enforces C# props
	/// authoring; runtime prop and emit declarations are emitted only when supplied on
	/// the options object.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	/// <param name="options">The typed component options including setup, name, and registrations.</param>
	/// <returns>An <see cref="IVueComponent{TProps}"/> that enforces typed props in <c>h()</c>.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent<TProps> DefineComponent<TProps>(VueComponentOptions<TProps> options)
		where TProps : VueProps;

	/// <summary>
	/// Defines a Vue component with typed slots but no typed props. Use this overload for
	/// components that accept named slots but do not declare typed props.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	/// <param name="options">The typed component options including setup and registrations.</param>
	/// <returns>An <see cref="IVueSlotComponent{TSlots}"/> that enforces typed slots in <c>h()</c>.</returns>
	[Description("@#defineComponent")]
	public extern static IVueSlotComponent<TSlots> DefineComponent<TSlots>(VueSlotComponentOptions<TSlots> options)
		where TSlots : VueSlots;

	/// <summary>
	/// Defines a Vue component with both typed props and typed slots. This is the most
	/// strongly-typed overload, enforcing both prop and slot types in <c>h()</c> calls.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	/// <param name="options">The fully typed component options including setup and registrations.</param>
	/// <returns>An <see cref="IVueComponent{TProps, TSlots}"/> that enforces both props and slots in <c>h()</c>.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent<TProps, TSlots> DefineComponent<TProps, TSlots>(VueComponentOptions<TProps, TSlots> options)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Defines a Vue-powered custom element from an existing component options object.
	/// The returned constructor can be registered with the browser's
	/// <see cref="CustomElementRegistry"/>.
	/// </summary>
	/// <param name="options">The component options used to render the custom element.</param>
	/// <returns>A custom element constructor compatible with <c>customElements.define()</c>.</returns>
	[Description("@#defineCustomElement")]
	public extern static CustomElementConstructor DefineCustomElement(VueComponentDefinition options);

	/// <summary>
	/// Defines a Vue-powered custom element from component options plus
	/// custom-element-specific runtime options.
	/// </summary>
	/// <param name="options">The component options used to render the custom element.</param>
	/// <param name="customElementOptions">Custom-element-specific styles, app configuration, shadow root, and CSP options.</param>
	/// <returns>A custom element constructor compatible with <c>customElements.define()</c>.</returns>
	[Description("@#defineCustomElement")]
	public extern static CustomElementConstructor DefineCustomElement(VueComponentDefinition options, VueCustomElementOptions customElementOptions);

	/// <summary>
	/// Defines an async component from a loader callback.
	/// </summary>
	/// <param name="loader">A callback returning a promise that resolves to the component definition.</param>
	/// <returns>An async component reference that can be rendered or registered like a normal component.</returns>
	[Description("@#defineAsyncComponent")]
	public extern static IVueComponent DefineAsyncComponent(VueAsyncComponentLoader loader);

	/// <summary>
	/// Defines an async component from object-form options.
	/// </summary>
	/// <param name="options">Async component loading, error, timing, and retry options.</param>
	/// <returns>An async component reference that can be rendered or registered like a normal component.</returns>
	[Description("@#defineAsyncComponent")]
	public extern static IVueComponent DefineAsyncComponent(VueAsyncComponentOptions options);

	/// <summary>
	/// Defines a strongly typed async component from object-form options.
	/// </summary>
	/// <typeparam name="TComponent">The component contract produced by the loader.</typeparam>
	/// <param name="options">Typed async component loading, error, timing, and retry options.</param>
	/// <returns>A typed async component reference that preserves prop/slot contracts.</returns>
	[Description("@#defineAsyncComponent")]
	public extern static TComponent DefineAsyncComponent<TComponent>(VueAsyncComponentOptions<TComponent> options)
		where TComponent : ECMAScript.VueContract.IVueComponent;

	/// <summary>
	/// Vue's built-in <c>Transition</c> component for animating a single element or
	/// component entering and leaving.
	/// </summary>
	[Description("@#Transition")]
	public extern static IVueComponent<VueTransitionProps> Transition { get; }

	/// <summary>
	/// Vue's built-in <c>TransitionGroup</c> component for animating list insertions,
	/// removals, and moves.
	/// </summary>
	[Description("@#TransitionGroup")]
	public extern static IVueComponent<VueTransitionGroupProps> TransitionGroup { get; }

	/// <summary>
	/// Vue's built-in <c>KeepAlive</c> component for caching inactive dynamic component
	/// instances.
	/// </summary>
	[Description("@#KeepAlive")]
	public extern static IVueComponent<VueKeepAliveProps> KeepAlive { get; }

	/// <summary>
	/// Vue's built-in <c>Teleport</c> component for rendering children into another DOM
	/// container.
	/// </summary>
	[Description("@#Teleport")]
	public extern static IVueComponent<VueTeleportProps> Teleport { get; }

	/// <summary>
	/// Vue's built-in <c>Suspense</c> component for coordinating async dependencies
	/// with default and fallback slots.
	/// </summary>
	[Description("@#Suspense")]
	public extern static IVueComponent<VueSuspenseProps, VueSuspenseSlots> Suspense { get; }

	/// <summary>
	/// Merges multiple props objects using Vue's VNode props merge semantics.
	/// </summary>
	/// <param name="props">The props objects to merge.</param>
	/// <returns>A merged props object suitable for <c>h(...)</c> and <c>cloneVNode(...)</c>.</returns>
	[Description("@#mergeProps")]
	public extern static VueProps MergeProps(params VueProps[] props);

	/// <summary>
	/// Clones an existing VNode.
	/// </summary>
	/// <param name="vnode">The VNode to clone.</param>
	/// <returns>A cloned VNode.</returns>
	[Description("@#cloneVNode")]
	public extern static IVNode CloneVNode(IVNode vnode);

	/// <summary>
	/// Clones an existing VNode and merges extra props into it.
	/// </summary>
	/// <param name="vnode">The VNode to clone.</param>
	/// <param name="extraProps">Additional props to merge into the clone.</param>
	/// <returns>A cloned VNode with merged props.</returns>
	[Description("@#cloneVNode")]
	public extern static IVNode CloneVNode(IVNode vnode, VueProps extraProps);

	/// <summary>
	/// Returns whether the supplied runtime value is a Vue VNode.
	/// </summary>
	/// <typeparam name="T">The static type of the runtime value being tested.</typeparam>
	/// <param name="value">The runtime value to test.</param>
	/// <returns><c>true</c> when the value is a VNode.</returns>
	[Description("@#isVNode")]
	public extern static bool IsVNode<T>(T value);

	/// <summary>
	/// Resolves a component by name from the current component/app context.
	/// </summary>
	/// <param name="name">The registered component name.</param>
	/// <returns>The resolved component.</returns>
	[Description("@#resolveComponent")]
	public extern static IVueComponent ResolveComponent(string name);

	/// <summary>
	/// Resolves a directive by name from the current component/app context.
	/// </summary>
	/// <param name="name">The registered directive name.</param>
	/// <returns>The resolved directive, or <c>null</c> when unavailable.</returns>
	[Description("@#resolveDirective")]
	public extern static VueDirectiveValue? ResolveDirective(string name);

	/// <summary>
	/// Applies runtime directives to a VNode created by <see cref="H(string)"/> or a
	/// component render call.
	/// </summary>
	/// <param name="vnode">The VNode to decorate.</param>
	/// <param name="directives">Directive argument tuples matching Vue's runtime contract.</param>
	/// <returns>The same VNode with directive metadata attached.</returns>
	[Description("@#withDirectives")]
	public extern static IVNode WithDirectives(IVNode vnode, [PreserveParamsArray] params VueDirectiveArguments[] directives);

	/// <summary>
	/// Wraps a parameterless event handler with Vue event modifiers such as
	/// <c>stop</c>, <c>prevent</c>, or <c>self</c>.
	/// </summary>
	/// <param name="handler">The original event handler.</param>
	/// <param name="modifiers">Modifier names in Vue runtime form.</param>
	/// <returns>A wrapped event handler.</returns>
	[Description("@#withModifiers")]
	public extern static Action WithModifiers(Action handler, [PreserveParamsArray] params string[] modifiers);

	/// <summary>
	/// Wraps a typed event handler with Vue event modifiers.
	/// </summary>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <param name="handler">The original typed event handler.</param>
	/// <param name="modifiers">Modifier names in Vue runtime form.</param>
	/// <returns>A wrapped typed event handler.</returns>
	[Description("@#withModifiers")]
	public extern static VueEventHandler<TEvent> WithModifiers<TEvent>(VueEventHandler<TEvent> handler, [PreserveParamsArray] params string[] modifiers);

}
