namespace ECMAScript;

public delegate void VueWatchHandle();

public delegate void VueEventHandler<T>(T value);

public delegate Vue.IVNode VueRenderCallback();

public delegate Vue.IVNode VueSlotCallback();

public delegate Vue.IVNode VueSlotCallback<TScope>(TScope scope);

public delegate VueRenderCallback VueSetupCallback();

public delegate VueRenderCallback VueTypedSetupCallback<TProps>(TProps props, Vue.VueSetupContext context)
	where TProps : Vue.VueProps;

public delegate VueRenderCallback VueTypedSlotSetupCallback<TSlots>(Vue.VueSetupContext<TSlots> context)
	where TSlots : Vue.VueSlots;

public delegate VueRenderCallback VueTypedSetupCallback<TProps, TSlots>(TProps props, Vue.VueSetupContext<TSlots> context)
	where TProps : Vue.VueProps
	where TSlots : Vue.VueSlots;

[ECMAScript("npm:vue@3")]
[Description("@#")]
[Jazor]
public static class Vue
{
    public interface IVueComponent : Contract.IUIComponent { }

    public interface IVueComponent<TProps> : IVueComponent
		where TProps : VueProps
	{
	}

	public interface IVueSlotComponent<TSlots> : IVueComponent
		where TSlots : VueSlots
	{
	}

	public interface IVueComponent<TProps, TSlots> : IVueComponent<TProps>, IVueSlotComponent<TSlots>
		where TProps : VueProps
		where TSlots : VueSlots
	{
	}

    public interface IVNode { }

    public interface IVueRef<T>
    {
        [Description("@#value")]
        public T Value { get; set; }
    }

	public interface IVueOptionsBag { }

	[Description("@#")]
	public abstract record VueProps : IVueOptionsBag;

	[Description("@#")]
	public abstract record VueSlots : IVueOptionsBag;

	[Description("@#")]
	public abstract record VueComponentDefinition : IVueOptionsBag;

	[Description("@#")]
	public abstract record VueComponentRegistry : IVueOptionsBag;

	[Description("@#")]
	public abstract record VueDirectiveRegistry : IVueOptionsBag;

	[Description("@#")]
	public abstract record VuePluginOptions : IVueOptionsBag;

	[Description("@#VueComponentOptions")]
	public sealed record VueComponentOptions : VueComponentDefinition
	{
		[Description("@#name")]
		public string? Name { get; init; }

		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		[Description("@#emits")]
		public string[]? EmitNames { get; init; }

		[Description("@#setup")]
		public VueSetupCallback? Setup { get; init; }

		[Description("@#render")]
		public VueRenderCallback? Render { get; init; }
	}

	[Description("@#VueComponentOptions")]
	public sealed record VueComponentOptions<TProps> : VueComponentDefinition
		where TProps : VueProps
	{
		[Description("@#name")]
		public string? Name { get; init; }

		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		[Description("@#props")]
		[Props]
		public string[]? PropNames { get; init; }

		[Description("@#emits")]
		[Emits]
		public string[]? EmitNames { get; init; }

		[Description("@#setup")]
		public VueTypedSetupCallback<TProps>? Setup { get; init; }
	}

	[Description("@#VueComponentOptions")]
	public sealed record VueComponentOptions<TProps, TSlots> : VueComponentDefinition
		where TProps : VueProps
		where TSlots : VueSlots
	{
		[Description("@#name")]
		public string? Name { get; init; }

		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		[Description("@#props")]
		[Props]
		public string[]? PropNames { get; init; }

		[Description("@#emits")]
		[Emits]
		public string[]? EmitNames { get; init; }

		[Description("@#setup")]
		public VueTypedSetupCallback<TProps, TSlots>? Setup { get; init; }
	}

	[Description("@#VueComponentOptions")]
	public sealed record VueSlotComponentOptions<TSlots> : VueComponentDefinition
		where TSlots : VueSlots
	{
		[Description("@#name")]
		public string? Name { get; init; }

		[Description("@#components")]
		public VueComponentRegistry? Components { get; init; }

		[Description("@#directives")]
		public VueDirectiveRegistry? Directives { get; init; }

		[Description("@#emits")]
		[Emits]
		public string[]? EmitNames { get; init; }

		[Description("@#setup")]
		public VueTypedSlotSetupCallback<TSlots>? Setup { get; init; }
	}

	public class VueReadonlyRef<T>
	{
		[Description("@#value")]
		public extern T Value { get; }
	}

	public sealed class VueComponentPublicInstance
	{
		private VueComponentPublicInstance()
		{
		}
	}

	public abstract class VueSetupContext
	{
		[Description("@#attrs")]
		public extern VueAttributeBag Attrs { get; }

		[Description("@#slots")]
		public extern VueSlotBag Slots { get; }

		[Description("@#emit")]
		public extern void Emit(string eventName);

		[Description("@#emit")]
		public extern void Emit<TValue>(string eventName, TValue value);

		[Description("@#emit")]
		public extern void Emit<T0, T1>(string eventName, T0 value0, T1 value1);

		[Description("@#expose")]
		public extern void Expose<TValue>(TValue exposed) where TValue : class;
	}

	public abstract class VueSetupContext<TSlots> : VueSetupContext
		where TSlots : VueSlots
	{
		[Description("@#slots")]
		public new extern TSlots Slots { get; }
	}

	public abstract class VueAttributeBag
	{
		protected VueAttributeBag()
		{
		}
	}

	public abstract class VueSlotBag
	{
		protected VueSlotBag()
		{
		}
	}

	public abstract class VueDirective
	{
		protected VueDirective()
		{
		}
	}

	public abstract class VuePlugin
	{
		protected VuePlugin()
		{
		}
	}

	public abstract class VueApp
	{
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(string selector);

		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(Element container);

		[Description("@#unmount")]
		public extern void Unmount();

		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin);

		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin, VuePluginOptions options);

		[Description("@#component")]
		public extern VueApp Component(string name, IVueComponent component);

		[Description("@#component")]
		public extern IVueComponent Component(string name);

		[Description("@#directive")]
		public extern VueApp Directive(string name, VueDirective directive);

		[Description("@#directive")]
		public extern VueDirective Directive(string name);

		[Description("@#provide")]
		public extern VueApp Provide<TValue>(string key, TValue value);
	}

	[Description("@#createApp")]
    public extern static VueApp CreateApp(IVueComponent rootComponent);

    [Description("@#createApp")]
    public extern static VueApp CreateApp(IVueComponent rootComponent, VueProps rootProps);

    [Description("@#createSSRApp")]
    public extern static VueApp CreateSsrApp(IVueComponent rootComponent);

	[Description("@#createSSRApp")]
	public extern static VueApp CreateSsrApp(IVueComponent rootComponent, [Anonymous] VueProps rootProps);

    [Description("@#defineComponent")]
    public extern static IVueComponent DefineComponent(VueComponentDefinition options);

    [Description("@#defineComponent")]
    public extern static IVueComponent<TProps> DefineComponent<TProps>(VueComponentOptions<TProps> options)
		where TProps : VueProps;

	[Description("@#defineComponent")]
	public extern static IVueSlotComponent<TSlots> DefineComponent<TSlots>(VueSlotComponentOptions<TSlots> options)
		where TSlots : VueSlots;

	[Description("@#defineComponent")]
	public extern static IVueComponent<TProps, TSlots> DefineComponent<TProps, TSlots>(VueComponentOptions<TProps, TSlots> options)
		where TProps : VueProps
		where TSlots : VueSlots;

    [Description("@#h")]
    public extern static IVNode H(string type);

    [Description("@#h")]
    public extern static IVNode H(string type, IVNode child);

    [Description("@#h")]
    public extern static IVNode H(string type, Either<string, Number, bool, IVNode, IVNode[]> children);

    [Description("@#h")]
    public extern static IVNode H(string type, VueProps props);

    [Description("@#h")]
    public extern static IVNode H(string type, VueProps props, IVNode child);

    [Description("@#h")]
    public extern static IVNode H(string type, VueProps props, Either<string, Number, bool, IVNode, IVNode[]> children);

    [Description("@#h")]
    public extern static IVNode H(IVueComponent component);

    [Description("@#h")]
    [Jazor]
    public extern static IVNode H(IVueComponent component, IVNode child);

    [Description("@#h")]
    [Jazor]
    public extern static IVNode H(IVueComponent component, Either<string, Number, bool, IVNode, IVNode[]> children);

    [Description("@#h")]
    public extern static IVNode H(IVueComponent component, VueSlots slots);

    [Description("@#h")]
    public extern static IVNode H(IVueComponent component, VueProps props);

    [Description("@#h")]
    [Jazor]
    public extern static IVNode H(IVueComponent component, VueProps props, IVNode child);

    [Description("@#h")]
    public extern static IVNode H(IVueComponent component, VueProps props, VueSlots slots);

    [Description("@#h")]
    public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props)
		where TProps : VueProps;

	[Description("@#h")]
	[Jazor]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode child)
		where TSlots : VueSlots;

	[Description("@#h")]
	[Jazor]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, Either<string, Number, bool, IVNode, IVNode[]> children)
		where TSlots : VueSlots;

	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, TSlots slots)
		where TSlots : VueSlots;

	[Description("@#h")]
	[Jazor]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	[Description("@#h")]
	[Jazor]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, Either<string, Number, bool, IVNode, IVNode[]> children)
		where TProps : VueProps
		where TSlots : VueSlots;

	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	[Description("@#h")]
	[Jazor]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	[Description("@#h")]
	[Jazor]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, Either<string, Number, bool, IVNode, IVNode[]> children)
		where TProps : VueProps
		where TSlots : VueSlots;

	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

    [Description("@#h")]
    [Jazor]
    public extern static IVNode H(IVueComponent component, VueProps props, Either<string, Number, bool, IVNode, IVNode[]> children);

    [Description("@#reactive")]
    public extern static T Reactive<T>(T value) where T : class;

    [Description("@#readonly")]
    public extern static T Readonly<T>(T value) where T : class;

    [Description("@#ref")]
    public extern static IVueRef<T> Ref<T>(T value);

    [Description("@#shallowRef")]
    public extern static IVueRef<T> ShallowRef<T>(T value);

    [Description("@#computed")]
    public extern static VueReadonlyRef<T> Computed<T>(Func<T> getter);

    [Description("@#watch")]
    public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback);

    [Description("@#watchEffect")]
    public extern static VueWatchHandle WatchEffect(Action effect);

    [Description("@#nextTick")]
    public extern static PromiseResult NextTick();

    [Description("@#onMounted")]
    public extern static void OnMounted(Action callback);

    [Description("@#onUnmounted")]
    public extern static void OnUnmounted(Action callback);

    [Description("@#onUpdated")]
    public extern static void OnUpdated(Action callback);
}
