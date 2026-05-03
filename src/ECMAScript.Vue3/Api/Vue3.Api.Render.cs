using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
	/// Binds a this-aware data callback to Vue's Options API <c>data()</c> runtime shape.
	/// </summary>
	/// <typeparam name="TThis">Typed view of the component public instance.</typeparam>
	/// <param name="callback">The callback that receives runtime <c>this</c> first.</param>
	/// <returns>A standard Vue data callback.</returns>
	private const string BindThisInlineTemplate = "((__cb) => function(){ return __cb(this, ...arguments); })(__arg1)";

	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static VueDataCallback BindThis<TThis>(VueThisDataCallback<TThis> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware action callback with no explicit runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Action BindThis<TThis>(VueThisAction<TThis> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware action callback with one runtime argument.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Action<T1> BindThis<TThis, T1>(VueThisAction<TThis, T1> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware action callback with two runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Action<T1, T2> BindThis<TThis, T1, T2>(VueThisAction<TThis, T1, T2> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware action callback with three runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Action<T1, T2, T3> BindThis<TThis, T1, T2, T3>(VueThisAction<TThis, T1, T2, T3> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware watch cleanup callback.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static VueWatchCleanupCallback<TValue> BindThis<TThis, TValue>(VueThisWatchCleanupCallback<TThis, TValue> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware function callback with no explicit runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Func<TResult> BindThis<TThis, TResult>(VueThisFunc<TThis, TResult> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware function callback with one runtime argument.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Func<T1, TResult> BindThis<TThis, T1, TResult>(VueThisFunc<TThis, T1, TResult> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware function callback with two runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Func<T1, T2, TResult> BindThis<TThis, T1, T2, TResult>(VueThisFunc<TThis, T1, T2, TResult> callback)
		where TThis : class;

	/// <summary>
	/// Binds a this-aware function callback with three runtime arguments.
	/// </summary>
	[ECMAScriptInline(BindThisInlineTemplate)]
	[Description("@#bindThis")]
	public extern static Func<T1, T2, T3, TResult> BindThis<TThis, T1, T2, T3, TResult>(VueThisFunc<TThis, T1, T2, T3, TResult> callback)
		where TThis : class;

	/// <summary>
	/// Creates a VNode for an HTML element with no props or children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type);

	/// <summary>
	/// Creates a VNode for an HTML element with direct child content.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, IVNode child);

	/// <summary>
	/// Creates a VNode for an HTML element with direct children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, IVNode[] children);

	/// <summary>
	/// Creates a VNode for an HTML element with direct child content.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueChild child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and no children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props);

	/// <summary>
	/// Creates a VNode for an HTML element with props and direct child content.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, IVNode child);

	/// <summary>
	/// Creates a VNode for an HTML element with props and direct children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, IVNode[] children);

	/// <summary>
	/// Creates a VNode for an HTML element with props and direct child content.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(string type, VueProps props, VueChild child);

	/// <summary>
	/// Creates a VNode for an untyped component with no props or children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component);

	/// <summary>
	/// Creates a VNode for an untyped component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, IVNode child);

	/// <summary>
	/// Creates a VNode for an untyped component with direct children (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, IVNode[] children);

	/// <summary>
	/// Creates a VNode for an untyped component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueChild child);

	/// <summary>
	/// Creates a VNode for an untyped component with named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueSlots slots);

	/// <summary>
	/// Creates a VNode for an untyped component with props and no slots/children.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props);

	/// <summary>
	/// Creates a VNode for an untyped component with props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props, IVNode child);

	/// <summary>
	/// Creates a VNode for an untyped component with props and direct children (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props, IVNode[] children);

	/// <summary>
	/// Creates a VNode for an untyped component with props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props, VueChild child);

	/// <summary>
	/// Creates a VNode for an untyped component with props and named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H(IVueComponent component, VueProps props, VueSlots slots);

	/// <summary>
	/// Creates a VNode for a typed-props component with typed props.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with a typed Vue object.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueObject<TProps> props)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, IVNode child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with direct children (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, IVNode[] children)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueChild child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueSlots slots)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props, IVNode child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed props and direct children (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props, IVNode[] children)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props, VueChild child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed Vue object props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueObject<TProps> props, IVNode child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed Vue object props and direct children (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueObject<TProps> props, IVNode[] children)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed Vue object props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueObject<TProps> props, VueChild child)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed props and named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, TProps props, VueSlots slots)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-props component with typed Vue object props and named slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps>(IVueComponent<TProps> component, VueObject<TProps> props, VueSlots slots)
		where TProps : VueProps;

	/// <summary>
	/// Creates a VNode for a typed-slots component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with direct children (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode[] children)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, VueChild child)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed-slots component with typed slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TSlots>(IVueSlotComponent<TSlots> component, TSlots slots)
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using direct children (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, IVNode[] children)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueChild child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using typed slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using typed props.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with props and slots contracts using a typed Vue object.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueObject<TProps> props)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed props and direct children (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode[] children)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, VueChild child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed Vue object props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueObject<TProps> props, IVNode child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed Vue object props and direct children (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueObject<TProps> props, IVNode[] children)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed Vue object props and direct child content (default slot sugar).
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueObject<TProps> props, VueChild child)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed props and typed slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

	/// <summary>
	/// Creates a VNode for a typed component with typed Vue object props and typed slots.
	/// </summary>
	[Description("@#h")]
	public extern static IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, VueObject<TProps> props, TSlots slots)
		where TProps : VueProps
		where TSlots : VueSlots;

}
