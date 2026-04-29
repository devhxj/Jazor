namespace ECMAScript.Vue;

[ECMAScriptModule("vue")]
public abstract class VuePlugin
{
    protected VuePlugin()
    {
    }
}

[ECMAScriptModule("vue")]
public abstract class VueComponent
{
    protected VueComponent()
    {
    }
}

[ECMAScriptModule("vue")]
public abstract class VueDirective
{
    protected VueDirective()
    {
    }
}

[ECMAScriptModule]
[Description("@#")]
public abstract record VueOptionsBag;

[ECMAScriptModule]
[Description("@#")]
public abstract record VueComponentDefinition : VueOptionsBag;

[ECMAScriptModule]
[Description("@#VueComponentOptions")]
public sealed record VueComponentOptions : VueComponentDefinition
{
    [Description("@#name")]
    public string? Name { get; init; }

    [Description("@#components")]
    public VueComponentRegistry? Components { get; init; }

    [Description("@#directives")]
    public VueDirectiveRegistry? Directives { get; init; }

    [Description("@#render")]
    public Func<VueVNode>? Render { get; init; }
}

[ECMAScriptModule]
[Description("@#")]
public abstract record VueProps : VueOptionsBag;

[ECMAScriptModule]
[Description("@#")]
public abstract record VuePluginOptions : VueOptionsBag;

[ECMAScriptModule]
[Description("@#")]
public abstract record VueComponentRegistry : VueOptionsBag;

[ECMAScriptModule]
[Description("@#")]
public abstract record VueDirectiveRegistry : VueOptionsBag;

[ECMAScriptModule("vue")]
public sealed class VueVNode
{
    private VueVNode()
    {
    }
}

[ECMAScriptModule("vue")]
public sealed class VueComponentPublicInstance
{
    private VueComponentPublicInstance()
    {
    }
}

[ECMAScriptModule("vue")]
public static class Vue
{
    [ECMAScriptName("createApp")]
    public extern static VueApp CreateApp(VueComponent rootComponent);

    [ECMAScriptName("createApp")]
    public extern static VueApp CreateApp(VueComponent rootComponent, VueProps rootProps);

    [ECMAScriptName("createSSRApp")]
    public extern static VueApp CreateSsrApp(VueComponent rootComponent);

    [ECMAScriptName("createSSRApp")]
    public extern static VueApp CreateSsrApp(VueComponent rootComponent, VueProps rootProps);

    [ECMAScriptName("defineComponent")]
    public extern static VueComponent DefineComponent(VueComponentDefinition options);

    [ECMAScriptName("h")]
    public extern static VueVNode H(string type);

    [ECMAScriptName("h")]
    public extern static VueVNode H(string type, Either<string, Number, bool, VueVNode, VueVNode[]> children);

    [ECMAScriptName("h")]
    public extern static VueVNode H(string type, VueProps props);

    [ECMAScriptName("h")]
    public extern static VueVNode H(string type, VueProps props, Either<string, Number, bool, VueVNode, VueVNode[]> children);

    [ECMAScriptName("h")]
    public extern static VueVNode H(VueComponent component);

    [ECMAScriptName("h")]
    public extern static VueVNode H(VueComponent component, Either<string, Number, bool, VueVNode, VueVNode[]> children);

    [ECMAScriptName("h")]
    public extern static VueVNode H(VueComponent component, VueProps props);

    [ECMAScriptName("h")]
    public extern static VueVNode H(VueComponent component, VueProps props, Either<string, Number, bool, VueVNode, VueVNode[]> children);

    [ECMAScriptName("reactive")]
    public extern static T Reactive<T>(T value) where T : class;

    [ECMAScriptName("readonly")]
    public extern static T Readonly<T>(T value) where T : class;

    [ECMAScriptName("ref")]
    public extern static VueRef<T> Ref<T>(T value);

    [ECMAScriptName("shallowRef")]
    public extern static VueRef<T> ShallowRef<T>(T value);

    [ECMAScriptName("computed")]
    public extern static VueReadonlyRef<T> Computed<T>(Func<T> getter);

    [ECMAScriptName("watch")]
    public extern static VueWatchHandle Watch<T>(Func<T> source, Action<T, T> callback);

    [ECMAScriptName("watchEffect")]
    public extern static VueWatchHandle WatchEffect(Action effect);

    [ECMAScriptName("nextTick")]
    public extern static PromiseResult NextTick();

    [ECMAScriptName("onMounted")]
    public extern static void OnMounted(Action callback);

    [ECMAScriptName("onUnmounted")]
    public extern static void OnUnmounted(Action callback);

    [ECMAScriptName("onUpdated")]
    public extern static void OnUpdated(Action callback);
}

[ECMAScriptModule("vue")]
public class VueApp
{
    [ECMAScriptName("mount")]
    public extern VueComponentPublicInstance Mount(string selector);

    [ECMAScriptName("mount")]
    public extern VueComponentPublicInstance Mount(Element container);

    [ECMAScriptName("unmount")]
    public extern void Unmount();

    [ECMAScriptName("use")]
    public extern VueApp Use(VuePlugin plugin);

    [ECMAScriptName("use")]
    public extern VueApp Use(VuePlugin plugin, VuePluginOptions options);

    [ECMAScriptName("component")]
    public extern VueApp Component(string name, VueComponent component);

    [ECMAScriptName("component")]
    public extern VueComponent Component(string name);

    [ECMAScriptName("directive")]
    public extern VueApp Directive(string name, VueDirective directive);

    [ECMAScriptName("directive")]
    public extern VueDirective Directive(string name);

    [ECMAScriptName("provide")]
    public extern VueApp Provide<TValue>(string key, TValue value);
}

[ECMAScriptModule("vue")]
public class VueRef<T>
{
    [ECMAScriptName("value")]
    public extern T Value { get; set; }
}

[ECMAScriptModule("vue")]
public class VueReadonlyRef<T>
{
    [ECMAScriptName("value")]
    public extern T Value { get; }
}

public delegate void VueWatchHandle();

public delegate void VueEventHandler<T>(T value);
