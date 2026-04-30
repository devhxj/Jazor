namespace ECMAScript;

/// <summary>
/// Handle returned by <c>watch()</c> and <c>watchEffect()</c>; calling it stops the watcher.
/// </summary>
public delegate void VueWatchHandle();

/// <summary>
/// Callback signature for Vue event handlers that receive a typed event value.
/// </summary>
public delegate void VueEventHandler<T>(T value);

/// <summary>
/// Callback that returns a render tree (VNode). Used as the return type of <c>setup()</c>.
/// </summary>
public delegate Vue3.IVNode VueRenderCallback();

/// <summary>
/// Callback that returns a VNode from a slot with no scoped data.
/// </summary>
public delegate Vue3.IVNode VueSlotCallback();

/// <summary>
/// Callback that returns a VNode from a scoped slot that receives slot props.
/// </summary>
public delegate Vue3.IVNode VueSlotCallback<TScope>(TScope scope);

/// <summary>
/// Callback signature for a component <c>setup()</c> function with no typed props.
/// </summary>
public delegate VueRenderCallback VueSetupCallback();

/// <summary>
/// Callback signature for a component <c>setup()</c> function that receives typed props.
/// </summary>
public delegate VueRenderCallback VueTypedSetupCallback<TProps>(TProps props, Vue3.VueSetupContext context)
	where TProps : Vue3.VueProps;

/// <summary>
/// Callback signature for a component <c>setup()</c> function that receives typed slots only.
/// </summary>
public delegate VueRenderCallback VueTypedSlotSetupCallback<TSlots>(Vue3.VueSetupContext<TSlots> context)
	where TSlots : Vue3.VueSlots;

/// <summary>
/// Callback signature for a component <c>setup()</c> function that receives both typed props and typed slots.
/// </summary>
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
	/// Marker interface for a Vue component reference (used by <c>defineComponent()</c> and <c>h()</c>).
	/// </summary>
	public interface IVueComponent : IUIComponent { }

	/// <summary>
	/// A Vue component that declares typed props.
	/// </summary>
	public interface IVueComponent<TProps> : IVueComponent
		where TProps : VueProps
	{
	}

	/// <summary>
	/// A Vue component that declares typed slots (but no typed props).
	/// </summary>
	public interface IVueSlotComponent<TSlots> : IVueComponent
		where TSlots : VueSlots
	{
	}

	/// <summary>
	/// A Vue component that declares both typed props and typed slots.
	/// </summary>
	public interface IVueComponent<TProps, TSlots> : IVueComponent<TProps>, IVueSlotComponent<TSlots>
		where TProps : VueProps
		where TSlots : VueSlots
	{
	}

	/// <summary>
	/// Represents a Vue virtual DOM node returned by <c>h()</c>.
	/// </summary>
	public interface IVNode { }

	/// <summary>
	/// A reactive reference wrapper. The <c>value</c> property reads/writes the underlying ref.
	/// </summary>
	public interface IVueRef<T>
	{
		[Description("@#value")]
		public T Value { get; set; }
	}

	/// <summary>
	/// Marker interface for option bags that map to plain JavaScript objects in Vue component options.
	/// </summary>
	public interface IVueOptionsBag { }

	/// <summary>
	/// Base record for component prop declarations. Maps to a plain JS object in Vue <c>props</c>.
	/// </summary>
	[Description("@#")]
	public abstract record VueProps : IVueOptionsBag;

	/// <summary>
	/// Base record for component slot declarations. Maps to a plain JS object in Vue <c>slots</c>.
	/// </summary>
	[Description("@#")]
	public abstract record VueSlots : IVueOptionsBag;

	/// <summary>
	/// Base record for component definition objects passed to <c>defineComponent()</c>.
	/// </summary>
	[Description("@#")]
	public abstract record VueComponentDefinition : IVueOptionsBag;

	/// <summary>
	/// Registry of child components, mapping to Vue <c>components</c> option.
	/// </summary>
	[Description("@#")]
	public abstract record VueComponentRegistry : IVueOptionsBag;

	/// <summary>
	/// Registry of custom directives, mapping to Vue <c>directives</c> option.
	/// </summary>
	[Description("@#")]
	public abstract record VueDirectiveRegistry : IVueOptionsBag;

	/// <summary>
	/// Options bag for plugin configuration passed to <c>app.use(plugin, options)</c>.
	/// </summary>
	[Description("@#")]
	public abstract record VuePluginOptions : IVueOptionsBag;

	/// <summary>
	/// Options for <c>defineComponent()</c> with no typed props or slots.
	/// </summary>
	[Description("@#VueComponentOptions")]
	public sealed record VueComponentOptions : VueComponentDefinition
	{
		/// <summary>
		/// Component name for devtools and recursive self-reference.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Declared emit event names for this component.
		/// </summary>
		[Description("@#emits")]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function returning a render callback.
		/// </summary>
		[Description("@#setup")]
		public VueSetupCallback? Setup { get; init; }

		/// <summary>
		/// Render function returning a VNode tree directly (alternative to template/setup).
		/// </summary>
		[Description("@#render")]
		public VueRenderCallback? Render { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with typed props.
	/// </summary>
	[Description("@#VueComponentOptions")]
	public sealed record VueComponentOptions<TProps> : VueComponentDefinition
		where TProps : VueProps
	{
		/// <summary>
		/// Component name for devtools and recursive self-reference.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Prop names auto-inferred from <typeparamref name="TProps"/> by the <c>[Props]</c> generator.
		/// </summary>
		[Description("@#props")]
		[Props]
		public string[]? PropNames { get; init; }

		/// <summary>
		/// Emit event names auto-inferred from <typeparamref name="TProps"/> by the <c>[Emits]</c> generator.
		/// </summary>
		[Description("@#emits")]
		[Emits]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function receiving typed props, returning a render callback.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps>? Setup { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with both typed props and typed slots.
	/// </summary>
	[Description("@#VueComponentOptions")]
	public sealed record VueComponentOptions<TProps, TSlots> : VueComponentDefinition
		where TProps : VueProps
		where TSlots : VueSlots
	{
		/// <summary>
		/// Component name for devtools and recursive self-reference.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Prop names auto-inferred from <typeparamref name="TProps"/> by the <c>[Props]</c> generator.
		/// </summary>
		[Description("@#props")]
		[Props]
		public string[]? PropNames { get; init; }

		/// <summary>
		/// Emit event names auto-inferred from <typeparamref name="TProps"/> by the <c>[Emits]</c> generator.
		/// </summary>
		[Description("@#emits")]
		[Emits]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function receiving typed props and typed slots context, returning a render callback.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSetupCallback<TProps, TSlots>? Setup { get; init; }
	}

	/// <summary>
	/// Options for <c>defineComponent()</c> with typed slots but no typed props.
	/// </summary>
	[Description("@#VueComponentOptions")]
	public sealed record VueSlotComponentOptions<TSlots> : VueComponentDefinition
		where TSlots : VueSlots
	{
		/// <summary>
		/// Component name for devtools and recursive self-reference.
		/// </summary>
		[Description("@#name")]
		public string? Name { get; init; }

		/// <summary>
		/// Child components registered on this component.
		/// </summary>
		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		/// <summary>
		/// Custom directives registered on this component.
		/// </summary>
		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		/// <summary>
		/// Emit event names auto-inferred from <typeparamref name="TSlots"/> by the <c>[Emits]</c> generator.
		/// </summary>
		[Description("@#emits")]
		[Emits]
		public string[]? EmitNames { get; init; }

		/// <summary>
		/// Setup function receiving typed slots context, returning a render callback.
		/// </summary>
		[Description("@#setup")]
		public VueTypedSlotSetupCallback<TSlots>? Setup { get; init; }
	}

	/// <summary>
	/// A readonly reactive reference. Only the <c>value</c> getter is available.
	/// </summary>
	public class VueReadonlyRef<T>
	{
		[Description("@#value")]
		public extern T Value { get; }
	}

	/// <summary>
	/// Represents the public instance of a mounted Vue component.
	/// </summary>
	public sealed class VueComponentPublicInstance
	{
		private VueComponentPublicInstance()
		{
		}
	}

	/// <summary>
	/// Setup context available inside the <c>setup()</c> function, providing <c>attrs</c>, <c>slots</c>, <c>emit</c>, and <c>expose</c>.
	/// </summary>
	public abstract class VueSetupContext
	{
		/// <summary>
		/// Fallthrough attributes passed to the component but not declared as props.
		/// </summary>
		[Description("@#attrs")]
		public extern VueAttributeBag Attrs { get; }

		/// <summary>
		/// Slots available in the component.
		/// </summary>
		[Description("@#slots")]
		public extern VueSlotBag Slots { get; }

		/// <summary>
		/// Emit an event by name with no payload.
		/// </summary>
		[Description("@#emit")]
		public extern void Emit(string eventName);

		/// <summary>
		/// Emit an event by name with a single payload value.
		/// </summary>
		[Description("@#emit")]
		public extern void Emit<TValue>(string eventName, TValue value);

		/// <summary>
		/// Emit an event by name with two payload values.
		/// </summary>
		[Description("@#emit")]
		public extern void Emit<T0, T1>(string eventName, T0 value0, T1 value1);

		/// <summary>
		/// Expose a value on the component public instance for parent access via template refs.
		/// </summary>
		[Description("@#expose")]
		public extern void Expose<TValue>(TValue exposed) where TValue : class;
	}

	/// <summary>
	/// Typed setup context that provides typed slots in addition to the standard context.
	/// </summary>
	public abstract class VueSetupContext<TSlots> : VueSetupContext
		where TSlots : VueSlots
	{
		/// <summary>
		/// Typed slots available in the component.
		/// </summary>
		[Description("@#slots")]
		public new extern TSlots Slots { get; }
	}

	/// <summary>
	/// Bag of fallthrough attributes (<c>v-bind="$attrs"</c>).
	/// </summary>
	public abstract class VueAttributeBag
	{
		protected VueAttributeBag()
		{
		}
	}

	/// <summary>
	/// Bag of available slots (<c>$slots</c>).
	/// </summary>
	public abstract class VueSlotBag
	{
		protected VueSlotBag()
		{
		}
	}

	/// <summary>
	/// Base class for custom Vue directive definitions.
	/// </summary>
	public abstract class VueDirective
	{
		protected VueDirective()
		{
		}
	}

	/// <summary>
	/// Base class for Vue plugin definitions (passed to <c>app.use()</c>).
	/// </summary>
	public abstract class VuePlugin
	{
		protected VuePlugin()
		{
		}
	}

	/// <summary>
	/// A Vue application instance created by <c>createApp()</c>.
	/// </summary>
	public abstract class VueApp
	{
		/// <summary>
		/// Mount the application to a DOM element matching the given CSS selector.
		/// </summary>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(string selector);

		/// <summary>
		/// Mount the application to a specific DOM element.
		/// </summary>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(Element container);

		/// <summary>
		/// Unmount the application and clean up reactivity effects.
		/// </summary>
		[Description("@#unmount")]
		public extern void Unmount();

		/// <summary>
		/// Install a plugin with no options.
		/// </summary>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin);

		/// <summary>
		/// Install a plugin with configuration options.
		/// </summary>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin, VuePluginOptions options);

		/// <summary>
		/// Register a global component by name.
		/// </summary>
		[Description("@#component")]
		public extern VueApp Component(string name, IVueComponent component);

		/// <summary>
		/// Retrieve a previously registered global component by name.
		/// </summary>
		[Description("@#component")]
		public extern IVueComponent Component(string name);

		/// <summary>
		/// Register a global directive by name.
		/// </summary>
		[Description("@#directive")]
		public extern VueApp Directive(string name, VueDirective directive);

		/// <summary>
		/// Retrieve a previously registered global directive by name.
		/// </summary>
		[Description("@#directive")]
		public extern VueDirective Directive(string name);

		/// <summary>
		/// Provide a value at the app level, injectable by any descendant component.
		/// </summary>
		[Description("@#provide")]
		public extern VueApp Provide<TValue>(string key, TValue value);
	}

	/// <summary>
	/// Creates a Vue application instance from a root component.
	/// </summary>
	[Description("@#createApp")]
	public extern static VueApp CreateApp(IVueComponent rootComponent);

	/// <summary>
	/// Creates a Vue application instance with root props.
	/// </summary>
	[Description("@#createApp")]
	public extern static VueApp CreateApp(IVueComponent rootComponent, VueProps rootProps);

	/// <summary>
	/// Creates a Vue application instance in SSR (server-side rendering) mode.
	/// </summary>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp(IVueComponent rootComponent);

	/// <summary>
	/// Creates a Vue application instance in SSR mode with root props.
	/// </summary>
	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp(IVueComponent rootComponent, VueProps rootProps);

	/// <summary>
	/// Defines a Vue component from an options object with no typed props.
	/// </summary>
	[Description("@#defineComponent")]
	public extern static IVueComponent DefineComponent(VueComponentDefinition options);

	/// <summary>
	/// Defines a Vue component with typed props.
	/// </summary>
	[Description("@#defineComponent")]
	public extern static IVueComponent<TProps> DefineComponent<TProps>(VueComponentOptions<TProps> options)
		where TProps : VueProps;

	/// <summary>
	/// Defines a Vue component with typed slots but no typed props.
	/// </summary>
	[Description("@#defineComponent")]
	public extern static IVueSlotComponent<TSlots> DefineComponent<TSlots>(VueSlotComponentOptions<TSlots> options)
		where TSlots : VueSlots;

	/// <summary>
	/// Defines a Vue component with both typed props and typed slots.
	/// </summary>
	[Description("@#defineComponent")]
	public extern static IVueComponent<TProps, TSlots> DefineComponent<TProps, TSlots>(VueComponentOptions<TProps, TSlots> options)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for an HTML element with no children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type);

	/// <summary>
	/// Creates a VNode for an HTML element with a single VNode child.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, IVNode child);

	/// <summary>
	/// Creates a VNode for an HTML element with a text child.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, string child);

	/// <summary>
	/// Creates a VNode for an HTML element with a numeric child.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, Number child);

	/// <summary>
	/// Creates a VNode for an HTML element with a boolean child.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, bool child);

	/// <summary>
	/// Creates a VNode for an HTML element with an array of children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, IVNode[] children);

	/// <summary>
	/// Creates a VNode for an HTML element with props.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props);

	/// <summary>
	/// Creates a VNode for an HTML element with props and a single VNode child.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, IVNode child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and a text child.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, string child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and a numeric child.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, Number child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and a boolean child.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, bool child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and an array of children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, IVNode[] children);

	/// <summary>
	/// Creates a VNode for a component with no props or children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component);

	/// <summary>
	/// Creates a VNode for a component with a single VNode child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, IVNode child);

	/// <summary>
	/// Creates a VNode for a component with a text child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, string child);

	/// <summary>
	/// Creates a VNode for a component with a numeric child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, Number child);

	/// <summary>
	/// Creates a VNode for a component with a boolean child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, bool child);

	/// <summary>
	/// Creates a VNode for a component with an array of children (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, IVNode[] children);

	/// <summary>
	/// Creates a VNode for a component with named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueSlots slots);

	/// <summary>
	/// Creates a VNode for a component with props.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props);

	/// <summary>
	/// Creates a VNode for a component with props and a single VNode child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, IVNode child);

	/// <summary>
	/// Creates a VNode for a component with props and a text child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, string child);

	/// <summary>
	/// Creates a VNode for a component with props and a numeric child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, Number child);

	/// <summary>
	/// Creates a VNode for a component with props and a boolean child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, bool child);

	/// <summary>
	/// Creates a VNode for a component with props and an array of children (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H(IVueComponent component, VueProps props, IVNode[] children);

	/// <summary>
	/// Creates a VNode for a component with props and named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props, VueSlots slots);

	/// <summary>
	/// Creates a VNode for a typed-props component with its props.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-slots component with a single VNode child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with a text child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, string child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with a numeric child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, Number child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with a boolean child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, bool child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with an array of children (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode[] children)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, TSlots slots)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with a single VNode child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with a text child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, string child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with a numeric child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, Number child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with a boolean child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, bool child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with an array of children (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotNoPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, IVNode[] children)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and a single VNode child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and a text child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, string child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and a numeric child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, Number child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and a boolean child (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, bool child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and an array of children (default slot).
	/// </summary>
	[Description("@#h")]
	[Jazor(Contract.Op.Compile, "", HDefaultSlotWithPropsCompileMember)]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode[] children)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-props, typed-slots component with props and named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a deep reactive proxy of an object.
	/// </summary>
	[Description("@#reactive")]
	public extern static T Reactive<T>(T value) where T : class;

	/// <summary>
	/// Creates a readonly proxy of a reactive object.
	/// </summary>
	[Description("@#readonly")]
	public extern static T Readonly<T>(T value) where T : class;

	/// <summary>
	/// Creates a reactive ref wrapping a value.
	/// </summary>
	[Description("@#ref")]
	public extern static IVueRef<T> Ref<T>(T value);

	/// <summary>
	/// Creates a reactive ref that only tracks top-level value changes (shallow).
	/// </summary>
	[Description("@#shallowRef")]
	public extern static IVueRef<T> ShallowRef<T>(T value);

	/// <summary>
	/// Creates a computed reactive value derived from a getter function.
	/// </summary>
	[Description("@#computed")]
	public extern static VueReadonlyRef<T> Computed<T>(Func<T> getter);

	/// <summary>
	/// Watches a reactive source and calls the callback when it changes.
	/// Returns a handle that can be called to stop watching.
	/// </summary>
	[Description("@#watch")]
	public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback);

	/// <summary>
	/// Runs a side-effect function immediately and re-runs it when its reactive dependencies change.
	/// Returns a handle that can be called to stop the effect.
	/// </summary>
	[Description("@#watchEffect")]
	public extern static VueWatchHandle WatchEffect(Action effect);

	/// <summary>
	/// Waits for the next DOM update cycle to complete.
	/// </summary>
	[Description("@#nextTick")]
	public extern static PromiseResult NextTick();

	/// <summary>
	/// Registers a callback to run after the component is mounted.
	/// </summary>
	[Description("@#onMounted")]
	public extern static void OnMounted(Action callback);

	/// <summary>
	/// Registers a callback to run after the component is unmounted.
	/// </summary>
	[Description("@#onUnmounted")]
	public extern static void OnUnmounted(Action callback);

	/// <summary>
	/// Registers a callback to run after a reactive state change causes a DOM update.
	/// </summary>
	[Description("@#onUpdated")]
	public extern static void OnUpdated(Action callback);
}
