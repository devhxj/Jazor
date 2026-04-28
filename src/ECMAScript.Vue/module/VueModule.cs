namespace ECMAScript.Vue;

[ECMAScriptModule("vue")]
public static class Vue
{
    [ECMAScriptName("createApp")]
    public extern static VueApp CreateApp(object rootComponent);

    [ECMAScriptName("createApp")]
    public extern static VueApp CreateApp(object rootComponent, object rootProps);

    [ECMAScriptName("createSSRApp")]
    public extern static VueApp CreateSsrApp(object rootComponent);

    [ECMAScriptName("createSSRApp")]
    public extern static VueApp CreateSsrApp(object rootComponent, object rootProps);

    [ECMAScriptName("defineComponent")]
    public extern static object DefineComponent(object options);

    [ECMAScriptName("h")]
    public extern static object H(string type);

    [ECMAScriptName("h")]
    public extern static object H(string type, object props);

    [ECMAScriptName("h")]
    public extern static object H(string type, object props, object children);

    [ECMAScriptName("h")]
    public extern static object H(object component);

    [ECMAScriptName("h")]
    public extern static object H(object component, object props);

    [ECMAScriptName("h")]
    public extern static object H(object component, object props, object children);

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
    public extern object Mount(string selector);

    [ECMAScriptName("mount")]
    public extern object Mount(Element container);

    [ECMAScriptName("unmount")]
    public extern void Unmount();

    [ECMAScriptName("use")]
    public extern VueApp Use(object plugin);

    [ECMAScriptName("use")]
    public extern VueApp Use(object plugin, object options);

    [ECMAScriptName("component")]
    public extern VueApp Component(string name, object component);

    [ECMAScriptName("component")]
    public extern object Component(string name);

    [ECMAScriptName("directive")]
    public extern VueApp Directive(string name, object directive);

    [ECMAScriptName("directive")]
    public extern object Directive(string name);

    [ECMAScriptName("provide")]
    public extern VueApp Provide(string key, object value);
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
