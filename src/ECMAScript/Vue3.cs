namespace ECMAScript;

/// <summary>
/// Handle returned by <c>watch()</c> and <c>watchEffect()</c>. Invoking this delegate stops the watcher
/// and cleans up its reactive dependency tracking.
/// </summary>
public delegate void VueWatchHandle();

/// <summary>
/// Callback signature for Vue event handlers that receive a typed event payload.
/// </summary>
/// <typeparam name="T">The type of the event payload value.</typeparam>
/// <param name="value">The event payload emitted by the source component.</param>
public delegate void VueEventHandler<T>(T value);

/// <summary>
/// Callback that returns a render tree (VNode). Used as the return type of <c>setup()</c>
/// to provide the component's render function.
/// </summary>
/// <returns>A root VNode representing the rendered component output.</returns>
public delegate Vue3.IVNode VueRenderCallback();

/// <summary>
/// Callback that returns a VNode from a slot with no scoped data.
/// </summary>
/// <returns>A VNode produced by the slot, or <c>null</c> if the slot is empty.</returns>
public delegate Vue3.IVNode VueSlotCallback();

/// <summary>
/// Callback that returns a VNode from a scoped slot that receives slot props.
/// </summary>
/// <typeparam name="TScope">The type of the scoped data passed into the slot.</typeparam>
/// <param name="scope">The scoped data object provided by the parent component to the slot.</param>
/// <returns>A VNode produced by the slot, or <c>null</c> if the slot is empty.</returns>
public delegate Vue3.IVNode VueSlotCallback<TScope>(TScope scope);

/// <summary>
/// Callback signature for a component <c>setup()</c> function with no typed props.
/// The setup function runs before the component is mounted and returns a render callback.
/// </summary>
/// <returns>A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueSetupCallback();

/// <summary>
/// Callback signature for a component <c>setup()</c> function that receives typed props.
/// </summary>
/// <typeparam name="TProps">The props record type, inheriting from <see cref="Vue3.VueProps"/>.</typeparam>
/// <param name="props">The reactive props object passed by the parent component.</param>
/// <param name="context">The setup context providing <c>attrs</c>, <c>slots</c>, <c>emit</c>, and <c>expose</c>.</param>
/// <returns>A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueTypedSetupCallback<TProps>(TProps props, Vue3.VueSetupContext context)
	where TProps : Vue3.VueProps;

/// <summary>
/// Callback signature for a component <c>setup()</c> function that receives typed slots but no typed props.
/// </summary>
/// <typeparam name="TSlots">The slots record type, inheriting from <see cref="Vue3.VueSlots"/>.</typeparam>
/// <param name="context">The typed setup context providing typed <c>slots</c> in addition to the standard context members.</param>
/// <returns>A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueTypedSlotSetupCallback<TSlots>(Vue3.VueSetupContext<TSlots> context)
	where TSlots : Vue3.VueSlots;

/// <summary>
/// Callback signature for a component <c>setup()</c> function that receives both typed props and typed slots.
/// </summary>
/// <typeparam name="TProps">The props record type, inheriting from <see cref="Vue3.VueProps"/>.</typeparam>
/// <typeparam name="TSlots">The slots record type, inheriting from <see cref="Vue3.VueSlots"/>.</typeparam>
/// <param name="props">The reactive props object passed by the parent component.</param>
/// <param name="context">The typed setup context providing typed <c>slots</c> in addition to the standard context members.</param>
/// <returns>A <see cref="VueRenderCallback"/> that the framework calls to produce the component's VNode tree.</returns>
public delegate VueRenderCallback VueTypedSetupCallback<TProps, TSlots>(TProps props, Vue3.VueSetupContext<TSlots> context)
	where TProps : Vue3.VueProps
	where TSlots : Vue3.VueSlots;

[ECMAScript("npm:vue@3")]
[Description("@#")]
[Jazor]
public static class Vue3
{
	private const string HDefaultSlotNoPropsCompileMember = "VueHDefaultSlotNoProps";

	private const string HDefaultSlotWithPropsCompileMember = "VueHDefaultSlotWithProps";

	/// <summary>
	/// Marker interface for a Vue component reference. Implemented by all component types
	/// produced by <c>defineComponent()</c> and consumed by <c>h()</c>.
	/// </summary>
	public interface IVueComponent : IUIComponent { }

	/// <summary>
	/// A Vue component that declares typed props. The compiler uses this interface
	/// to select the correct <c>h()</c> overload for props-only components.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	public interface IVueComponent<TProps> : IVueComponent
		where TProps : VueProps
	{
	}

	/// <summary>
	/// A Vue component that declares typed slots but no typed props. The compiler uses
	/// this interface to select the correct <c>h()</c> overload for slots-only components.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public interface IVueSlotComponent<TSlots> : IVueComponent
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
	/// Base record for component prop declarations. Inherit from this record and declare
	/// properties to define the props a component accepts. Maps to a plain JS object in
	/// Vue's <c>props</c> option.
	/// </summary>
	public abstract record VueProps : IVueOptionsBag;

	/// <summary>
	/// Base record for component slot declarations. Inherit from this record and declare
	/// callback properties to define the named slots a component accepts. Maps to a plain
	/// JS object in Vue's <c>slots</c> option.
	/// </summary>
	public abstract record VueSlots : IVueOptionsBag;

	/// <summary>
	/// Base record for component definition objects passed to <c>defineComponent()</c>.
	/// Holds shared options like <c>name</c>, <c>components</c>, <c>directives</c>, etc.
	/// </summary>
	public abstract record VueComponentDefinition : IVueOptionsBag;

	/// <summary>
	/// Registry of child components that the current component can use in its template.
	/// Inherit from this record and declare <c>IVueComponent</c> properties to register
	/// child components. Maps to Vue's <c>components</c> option.
	/// </summary>
	public abstract record VueComponentRegistry : IVueOptionsBag;

	/// <summary>
	/// Registry of custom directives that the current component can use in its template.
	/// Inherit from this record and declare <c>VueDirective</c> properties to register
	/// custom directives. Maps to Vue's <c>directives</c> option.
	/// </summary>
	public abstract record VueDirectiveRegistry : IVueOptionsBag;

	/// <summary>
	/// Options bag for plugin configuration passed as the second argument to
	/// <c>app.use(plugin, options)</c>. Inherit from this record to define typed
	/// plugin options.
	/// </summary>
	public abstract record VuePluginOptions : IVueOptionsBag;

	/// <summary>
	/// Options for <c>defineComponent()</c> with no typed props or slots. Use this variant
	/// for simple components that rely on untyped props or have no props at all.
	/// </summary>
	public sealed record VueComponentOptions : VueComponentDefinition
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Declared emit event names for this component. Only events listed here will
		/// be emitted to the parent. If omitted, all event listeners passed by the
		/// parent are treated as fallthrough attributes.
		/// </summary>
		[Description("@#emits")]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives no props and
		/// must return a <see cref="VueRenderCallback"/> that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueSetupCallback? Setup { get; init; }

		/// <summary>
		/// Render function called directly to produce the component's VNode tree. This is
		/// an alternative to <see cref="Setup"/>; if both are provided, <c>render</c> takes
		/// precedence over the setup return value.
		/// </summary>
		[Description("@#render")]
		public VueRenderCallback? Render { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with typed props. The <c>[Props]</c> generator
	/// auto-infers prop names from the <typeparamref name="TProps"/> record properties.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	public sealed record VueComponentOptions<TProps> : VueComponentDefinition
		where TProps : VueProps
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Prop names auto-inferred from <typeparamref name="TProps"/> by the <c>[Props]</c>
		/// generator. Do not set manually; the source generator populates this at compile time.
		/// </summary>
		[Description("@#props")]
		[Props]
		public string[]? PropNames { get; init; }

		/// <summary>
		/// Emit event names auto-inferred from <typeparamref name="TProps"/> by the <c>[Emits]</c>
		/// generator. Emit properties in the props record (those matching the <c>On*</c> pattern)
		/// are extracted as emit declarations.
		/// </summary>
		[Description("@#emits")]
		[Emits]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives the typed props
		/// and a setup context, and must return a <see cref="VueRenderCallback"/> that produces
		/// the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps>? Setup { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with both typed props and typed slots. The
	/// <c>[Props]</c> generator auto-infers prop names from <typeparamref name="TProps"/>,
	/// and the setup context provides typed access to the declared slots.
	/// </summary>
	/// <typeparam name="TProps">The props record type describing the component's accepted props.</typeparam>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public sealed record VueComponentOptions<TProps, TSlots> : VueComponentDefinition
		where TProps : VueProps
		where TSlots : VueSlots
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Prop names auto-inferred from <typeparamref name="TProps"/> by the <c>[Props]</c>
		/// generator. Do not set manually; the source generator populates this at compile time.
		/// </summary>
		[Description("@#props")]
		[Props]
		public string[]? PropNames { get; init; }

		/// <summary>
		/// Emit event names auto-inferred from <typeparamref name="TProps"/> by the <c>[Emits]</c>
		/// generator. Emit properties in the props record (those matching the <c>On*</c> pattern)
		/// are extracted as emit declarations.
		/// </summary>
		[Description("@#emits")]
		[Emits]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives the typed props
		/// and a typed setup context (with typed slot access), and must return a
		/// <see cref="VueRenderCallback"/> that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps, TSlots>? Setup { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with typed slots but no typed props. Use this
	/// variant for components that accept named slots but do not declare typed props.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type describing the component's accepted slots.</typeparam>
	public sealed record VueSlotComponentOptions<TSlots> : VueComponentDefinition
		where TSlots : VueSlots
	{
		/// <summary>
		/// Component name used for devtools display, recursive self-reference, and
		/// warning messages. If omitted, Vue infers the name from the file or variable.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component, making them available in
		/// the render function by name.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Emit event names declared by this component. If omitted, all event listeners
		/// passed by the parent are treated as fallthrough attributes.
		/// </summary>
		[Description("@#emits")]
		[Emits]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function called before the component is mounted. Receives a typed setup
		/// context with typed slot access, and must return a <see cref="VueRenderCallback"/>
		/// that produces the component's VNode tree.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSlotSetupCallback<TSlots>? Setup { get; init; }
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
	}

	/// <summary>
	/// Base class for custom Vue directive definitions. Inherit from this class to
	/// define a directive with lifecycle hooks (<c>created</c>, <c>mounted</c>,
	/// <c>updated</c>, <c>unmounted</c>).
	/// </summary>
	public abstract class VueDirective
	{
		protected VueDirective()
		{
		}
	}

	/// <summary>
	/// Base class for Vue plugin definitions. Inherit from this class to create a
	/// plugin that can be installed via <see cref="VueApp.Use(VuePlugin)"/>.
	/// The plugin's <c>install()</c> method is called when <c>app.use()</c> is invoked.
	/// </summary>
	public abstract class VuePlugin
	{
		protected VuePlugin()
		{
		}
	}

	/// <summary>
	/// A Vue application instance created by <c>createApp()</c>. Provides methods for
	/// mounting, configuration, and global registration of components, directives,
	/// and plugins.
	/// </summary>
	public abstract class VueApp
	{
		/// <summary>
		/// Mount the application to the first DOM element matching the given CSS selector.
		/// The mounted component becomes the root of the application's component tree.
		/// </summary>
		/// <param name="selector">A CSS selector string (e.g. <c>"#app"</c>) identifying the mount point.</param>
		/// <returns>The public instance of the mounted root component.</returns>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(string selector);

		/// <summary>
		/// Mount the application directly to a specific DOM element.
		/// </summary>
		/// <param name="container">The DOM element to mount into. The element's existing content is replaced.</param>
		/// <returns>The public instance of the mounted root component.</returns>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(Element container);

		/// <summary>
		/// Unmount the application, destroying the component tree and cleaning up all
		/// reactive effects, watchers, and event listeners.
		/// </summary>
		[Description("@#unmount")]
		public extern void Unmount();

		/// <summary>
		/// Install a Vue plugin with no configuration options. The plugin's <c>install()</c>
		/// method receives the app instance.
		/// </summary>
		/// <param name="plugin">The plugin to install. Must inherit from <see cref="VuePlugin"/>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin);

		/// <summary>
		/// Install a Vue plugin with configuration options. The plugin's <c>install()</c>
		/// method receives the app instance and the options object.
		/// </summary>
		/// <param name="plugin">The plugin to install. Must inherit from <see cref="VuePlugin"/>.</param>
		/// <param name="options">Plugin-specific configuration. Must inherit from <see cref="VuePluginOptions"/>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin, VuePluginOptions options);

		/// <summary>
		/// Register a global component by name, making it available in all component
		/// templates without explicit import.
		/// </summary>
		/// <param name="name">The component name to register (e.g. <c>"MyButton"</c>).</param>
		/// <param name="component">The component definition, produced by <c>defineComponent()</c>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#component")]
		public extern VueApp Component(string name, IVueComponent component);

		/// <summary>
		/// Retrieve a previously registered global component by name.
		/// </summary>
		/// <param name="name">The registered component name to look up.</param>
		/// <returns>The component definition registered under the given name.</returns>
		[Description("@#component")]
		public extern IVueComponent Component(string name);

		/// <summary>
		/// Register a global custom directive by name, making it available in all component
		/// templates as <c>v-name</c>.
		/// </summary>
		/// <param name="name">The directive name to register (without the <c>v-</c> prefix, e.g. <c>"focus"</c>).</param>
		/// <param name="directive">The directive definition. Must inherit from <see cref="VueDirective"/>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive(string name, VueDirective directive);

		/// <summary>
		/// Retrieve a previously registered global directive by name.
		/// </summary>
		/// <param name="name">The registered directive name to look up (without the <c>v-</c> prefix).</param>
		/// <returns>The directive definition registered under the given name.</returns>
		[Description("@#directive")]
		public extern VueDirective Directive(string name);

		/// <summary>
		/// Provide a value at the application level, injectable by any descendant component
		/// via <c>inject()</c>. Application-level provides are available to all components
		/// in the tree, regardless of nesting depth.
		/// </summary>
		/// <typeparam name="TValue">The type of the provided value.</typeparam>
		/// <param name="key">The injection key string used by <c>inject()</c> to retrieve the value.</param>
		/// <param name="value">The value to provide. Can be any type: primitives, objects, functions, etc.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#provide")]
		public extern VueApp Provide<TValue>(string key, TValue value);
	}

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
	/// Creates a Vue application instance in SSR (server-side rendering) mode. In SSR mode,
	/// Vue renders the component tree to HTML strings instead of DOM nodes.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp(IVueComponent rootComponent);

	/// <summary>
	/// Creates a Vue application instance in SSR mode with root props.
	/// </summary>
	/// <param name="rootComponent">The root component definition, produced by <c>defineComponent()</c>.</param>
	/// <param name="rootProps">Props to pass to the root component during server-side rendering.</param>
	/// <returns>A new <see cref="VueApp"/> instance configured for server-side rendering.</returns>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp(IVueComponent rootComponent, VueProps rootProps);

	/// <summary>
	/// Defines a Vue component from an options object with no typed props. Use this overload
	/// for simple components that do not declare typed props or slots.
	/// </summary>
	/// <param name="options">The component options including setup/render, name, and registrations.</param>
	/// <returns>An <see cref="IVueComponent"/> that can be passed to <c>h()</c> or registered globally.</returns>
	[Description("@#defineComponent")]
	public extern static IVueComponent DefineComponent(VueComponentDefinition options);

	/// <summary>
	/// Defines a Vue component with typed props. The <c>[Props]</c> generator auto-infers
	/// prop names and emit names from the <typeparamref name="TProps"/> record.
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
	/// Creates a VNode for an HTML element with no children or props.
	/// </summary>
	/// <param name="type">The HTML tag name (e.g. <c>"div"</c>, <c>"span"</c>, <c>"input"</c>).</param>
	/// <returns>A VNode representing the empty HTML element.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type);

	/// <summary>
	/// Creates a VNode for an HTML element with a single VNode child.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="child">A single child VNode rendered inside this element.</param>
	/// <returns>A VNode representing the HTML element wrapping the child.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, IVNode child);

	/// <summary>
	/// Creates a VNode for an HTML element with a text child.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="child">A text string rendered as a text node inside this element.</param>
	/// <returns>A VNode representing the HTML element wrapping the text content.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, string child);

	/// <summary>
	/// Creates a VNode for an HTML element with a numeric child.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="child">A numeric value rendered as a text node inside this element.</param>
	/// <returns>A VNode representing the HTML element wrapping the numeric content.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, Number child);

	/// <summary>
	/// Creates a VNode for an HTML element with a boolean child. Renders as an empty
	/// text node when <c>false</c>, or the string <c>"true"</c> when <c>true</c>.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="child">A boolean value rendered inside this element.</param>
	/// <returns>A VNode representing the HTML element wrapping the boolean content.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, bool child);

	/// <summary>
	/// Creates a VNode for an HTML element with an array of children.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="children">An array of child VNodes rendered inside this element in order.</param>
	/// <returns>A VNode representing the HTML element wrapping all children.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, IVNode[] children);

	/// <summary>
	/// Creates a VNode for an HTML element with props (attributes) but no children.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="props">The element attributes (class, style, event handlers, etc.).</param>
	/// <returns>A VNode representing the HTML element with the specified attributes.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props);

	/// <summary>
	/// Creates a VNode for an HTML element with props and a single VNode child.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="props">The element attributes (class, style, event handlers, etc.).</param>
	/// <param name="child">A single child VNode rendered inside this element.</param>
	/// <returns>A VNode representing the HTML element with attributes wrapping the child.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, IVNode child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and a text child.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="props">The element attributes (class, style, event handlers, etc.).</param>
	/// <param name="child">A text string rendered as a text node inside this element.</param>
	/// <returns>A VNode representing the HTML element with attributes wrapping the text content.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, string child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and a numeric child.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="props">The element attributes (class, style, event handlers, etc.).</param>
	/// <param name="child">A numeric value rendered as a text node inside this element.</param>
	/// <returns>A VNode representing the HTML element with attributes wrapping the numeric content.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, Number child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and a boolean child.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="props">The element attributes (class, style, event handlers, etc.).</param>
	/// <param name="child">A boolean value rendered inside this element.</param>
	/// <returns>A VNode representing the HTML element with attributes wrapping the boolean content.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, bool child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and an array of children.
	/// </summary>
	/// <param name="type">The HTML tag name.</param>
	/// <param name="props">The element attributes (class, style, event handlers, etc.).</param>
	/// <param name="children">An array of child VNodes rendered inside this element in order.</param>
	/// <returns>A VNode representing the HTML element with attributes wrapping all children.</returns>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, IVNode[] children);

	/// <summary>
	/// Creates a VNode for a component with no props or children.
	/// </summary>
	/// <param name="component">The component definition to render, produced by <c>defineComponent()</c>.</param>
	/// <returns>A VNode representing the component instance.</returns>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component);

	/// <summary>
	/// Creates a VNode for a component with a single VNode child, automatically wrapped
	/// as the default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="child">A single child VNode passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with default slot content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, IVNode child);

	/// <summary>
	/// Creates a VNode for a component with a text child, automatically wrapped as the
	/// default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="child">A text string passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with default slot text content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, string child);

	/// <summary>
	/// Creates a VNode for a component with a numeric child, automatically wrapped as
	/// the default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="child">A numeric value passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with default slot numeric content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, Number child);

	/// <summary>
	/// Creates a VNode for a component with a boolean child, automatically wrapped as
	/// the default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="child">A boolean value passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with default slot boolean content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, bool child);

	/// <summary>
	/// Creates a VNode for a component with an array of children, automatically wrapped
	/// as the default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="children">An array of child VNodes passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with multiple default slot children.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, IVNode[] children);

	/// <summary>
	/// Creates a VNode for a component with named slots. Each property on the slots record
	/// maps to a named slot callback.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="slots">A slots record whose properties are named slot callbacks.</param>
	/// <returns>A VNode representing the component with the provided named slots.</returns>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueSlots slots);

	/// <summary>
	/// Creates a VNode for a component with typed props but no slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="props">The props to pass to the component.</param>
	/// <returns>A VNode representing the component with the specified props.</returns>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props);

	/// <summary>
	/// Creates a VNode for a component with props and a single VNode child as default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="props">The props to pass to the component.</param>
	/// <param name="child">A single child VNode passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with props and default slot content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, IVNode child);

	/// <summary>
	/// Creates a VNode for a component with props and a text child as default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="props">The props to pass to the component.</param>
	/// <param name="child">A text string passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with props and default slot text content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, string child);

	/// <summary>
	/// Creates a VNode for a component with props and a numeric child as default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="props">The props to pass to the component.</param>
	/// <param name="child">A numeric value passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with props and default slot numeric content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, Number child);

	/// <summary>
	/// Creates a VNode for a component with props and a boolean child as default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="props">The props to pass to the component.</param>
	/// <param name="child">A boolean value passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with props and default slot boolean content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, bool child);

	/// <summary>
	/// Creates a VNode for a component with props and an array of children as default slot content.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="props">The props to pass to the component.</param>
	/// <param name="children">An array of child VNodes passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with props and multiple default slot children.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, IVNode[] children);

	/// <summary>
	/// Creates a VNode for a component with props and named slots.
	/// </summary>
	/// <param name="component">The component definition to render.</param>
	/// <param name="props">The props to pass to the component.</param>
	/// <param name="slots">A slots record whose properties are named slot callbacks.</param>
	/// <returns>A VNode representing the component with props and named slots.</returns>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props, VueSlots slots);

	/// <summary>
	/// Creates a VNode for a typed-props component with its typed props object.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <param name="component">The typed-props component to render.</param>
	/// <param name="props">The typed props object whose properties are passed as individual props.</param>
	/// <returns>A VNode representing the component with typed props.</returns>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-slots component with a single VNode child as default slot content.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The typed-slots component to render.</param>
	/// <param name="child">A single child VNode passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with default slot content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with a text child as default slot content.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The typed-slots component to render.</param>
	/// <param name="child">A text string passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with default slot text content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, string child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with a numeric child as default slot content.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The typed-slots component to render.</param>
	/// <param name="child">A numeric value passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with default slot numeric content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, Number child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with a boolean child as default slot content.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The typed-slots component to render.</param>
	/// <param name="child">A boolean value passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with default slot boolean content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, bool child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with an array of children as default slot content.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The typed-slots component to render.</param>
	/// <param name="children">An array of child VNodes passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with multiple default slot children.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode[] children)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with named slots.
	/// </summary>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The typed-slots component to render.</param>
	/// <param name="slots">A typed slots record whose properties are named slot callbacks.</param>
	/// <returns>A VNode representing the component with the provided named slots.</returns>
	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, TSlots slots)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with a single VNode child
	/// as default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="child">A single child VNode passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with default slot content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with a text child as
	/// default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="child">A text string passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with default slot text content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, string child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with a numeric child as
	/// default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="child">A numeric value passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with default slot numeric content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, Number child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with a boolean child as
	/// default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="child">A boolean value passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with default slot boolean content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, bool child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with an array of children
	/// as default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="children">An array of child VNodes passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with multiple default slot children.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, IVNode[] children)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with named slots.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="slots">A typed slots record whose properties are named slot callbacks.</param>
	/// <returns>A VNode representing the component with the provided named slots.</returns>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and a single VNode
	/// child as default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="props">The typed props object whose properties are passed as individual props.</param>
	/// <param name="child">A single child VNode passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with props and default slot content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and a text child
	/// as default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="props">The typed props object whose properties are passed as individual props.</param>
	/// <param name="child">A text string passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with props and default slot text content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, string child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and a numeric child
	/// as default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="props">The typed props object whose properties are passed as individual props.</param>
	/// <param name="child">A numeric value passed to the component's default slot as a text node.</param>
	/// <returns>A VNode representing the component with props and default slot numeric content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, Number child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and a boolean child
	/// as default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="props">The typed props object whose properties are passed as individual props.</param>
	/// <param name="child">A boolean value passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with props and default slot boolean content.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, bool child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and an array of
	/// children as default slot content.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="props">The typed props object whose properties are passed as individual props.</param>
	/// <param name="children">An array of child VNodes passed to the component's default slot.</param>
	/// <returns>A VNode representing the component with props and multiple default slot children.</returns>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode[] children)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and named slots.
	/// </summary>
	/// <typeparam name="TProps">The props record type matching the component's declared props.</typeparam>
	/// <typeparam name="TSlots">The slots record type matching the component's declared slots.</typeparam>
	/// <param name="component">The fully typed component to render.</param>
	/// <param name="props">The typed props object whose properties are passed as individual props.</param>
	/// <param name="slots">A typed slots record whose properties are named slot callbacks.</param>
	/// <returns>A VNode representing the component with props and named slots.</returns>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a deep reactive proxy of an object. Vue recursively converts all nested
	/// properties into reactive getters/setters, so reads and writes at any depth are tracked.
	/// </summary>
	/// <typeparam name="T">The type of the object to make reactive (must be a reference type).</typeparam>
	/// <param name="value">The plain object to wrap in a reactive proxy.</param>
	/// <returns>A reactive proxy of the same type. All property accesses are tracked.</returns>
	[Description("@#reactive")]
	public extern static T Reactive<T>(T value) where T : class;

	/// <summary>
	/// Creates a readonly proxy of a reactive (or plain) object. Attempts to write to
	/// properties on the returned object will trigger a runtime warning and be ignored.
	/// </summary>
	/// <typeparam name="T">The type of the object to make readonly (must be a reference type).</typeparam>
	/// <param name="value">The object to wrap in a readonly proxy. Can be a reactive proxy or a plain object.</param>
	/// <returns>A readonly proxy of the same type. Reads are tracked; writes are blocked.</returns>
	[Description("@#readonly")]
	public extern static T Readonly<T>(T value) where T : class;

	/// <summary>
	/// Creates a reactive ref wrapping a single value. Unlike <see cref="Reactive{T}"/>,
	/// <c>ref()</c> wraps the entire value, not its properties. Access the value via
	/// <see cref="IVueRef{T}.Value"/>.
	/// </summary>
	/// <typeparam name="T">The type of the value to wrap.</typeparam>
	/// <param name="value">The initial value of the ref.</param>
	/// <returns>A reactive ref whose <c>Value</c> property reads and writes the wrapped value.</returns>
	[Description("@#ref")]
	public extern static IVueRef<T> Ref<T>(T value);

	/// <summary>
	/// Creates a shallow reactive ref that only tracks replacements of <c>Value</c>, not
	/// mutations of the value itself. Use this for large objects where deep tracking is
	/// unnecessary or when the value is replaced wholesale.
	/// </summary>
	/// <typeparam name="T">The type of the value to wrap.</typeparam>
	/// <param name="value">The initial value of the shallow ref.</param>
	/// <returns>A shallow ref whose <c>Value</c> property only triggers on replacement, not on deep mutation.</returns>
	[Description("@#shallowRef")]
	public extern static IVueRef<T> ShallowRef<T>(T value);

	/// <summary>
	/// Creates a computed reactive value derived from a getter function. The getter is
	/// evaluated lazily and cached; it is re-evaluated only when its reactive dependencies
	/// change. The returned ref is readonly.
	/// </summary>
	/// <typeparam name="T">The type of the computed value.</typeparam>
	/// <param name="getter">A function that computes the derived value. Reactive values accessed inside are tracked as dependencies.</param>
	/// <returns>A readonly ref whose <c>Value</c> is the latest computed result.</returns>
	[Description("@#computed")]
	public extern static VueReadonlyRef<T> Computed<T>(Func<T> getter);

	/// <summary>
	/// Watches a reactive source and calls the callback when it changes. The callback
	/// receives both the new value and the previous value. Returns a handle that can be
	/// called to stop the watcher.
	/// </summary>
	/// <typeparam name="T">The type of the watched value.</typeparam>
	/// <param name="source">A getter function that returns the reactive value to watch. Called on each evaluation cycle.</param>
	/// <param name="callback">A callback invoked with <c>(newValue, oldValue)</c> whenever the source's return value changes.</param>
	/// <returns>A <see cref="VueWatchHandle"/> that stops the watcher when invoked.</returns>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback);

	/// <summary>
	/// Runs a side-effect function immediately and re-runs it whenever its reactive
	/// dependencies change. Unlike <see cref="Watch{T}"/>, this does not receive old/new
	/// values — it simply re-executes the entire effect.
	/// </summary>
	/// <param name="effect">The side-effect function to run. Reactive values accessed inside are tracked as dependencies.</param>
	/// <returns>A <see cref="VueWatchHandle"/> that stops the effect when invoked.</returns>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(Action effect);

	/// <summary>
	/// Waits for the next DOM update cycle to complete. Use this after modifying reactive
	/// state to ensure the DOM has been updated before asserting on the rendered output.
	/// </summary>
	/// <returns>A <see cref="PromiseResult"/> that resolves after the DOM update flush.</returns>
	[Description("@#nextTick")]
	public extern static PromiseResult NextTick();

	/// <summary>
	/// Registers a callback to run after the component's initial mount into the DOM.
	/// The callback runs once; use <see cref="OnUpdated"/> for subsequent re-renders.
	/// </summary>
	/// <param name="callback">The function to execute after the component is mounted. Has access to the live DOM.</param>
	[Description("@#onMounted")]
	public extern static void OnMounted(Action callback);

	/// <summary>
	/// Registers a callback to run after the component is unmounted (removed from the DOM).
	/// Use this for cleanup: stopping timers, removing event listeners, disconnecting observables, etc.
	/// </summary>
	/// <param name="callback">The cleanup function to execute after the component is unmounted.</param>
	[Description("@#onUnmounted")]
	public extern static void OnUnmounted(Action callback);

	/// <summary>
	/// Registers a callback to run after a reactive state change causes the component's
	/// DOM to be updated. Fires after every re-render, not just the first.
	/// </summary>
	/// <param name="callback">The function to execute after each DOM update caused by a reactive state change.</param>
	[Description("@#onUpdated")]
	public extern static void OnUpdated(Action callback);
}
