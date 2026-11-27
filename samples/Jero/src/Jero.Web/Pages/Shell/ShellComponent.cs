using System.ComponentModel;
using static ECMAScript.Global;
using static ECMAScript.Vue;
using static ECMAScript.VueRoute;

namespace Jero.Web.Pages.Shell;

[ECMAScriptModule("components/shell-component.js")]
public static class ShellComponent
{
    public static IVueComponent Create()
    {
        var icons = new[]
        {
            ShellIcons.LayoutDashboard, ShellIcons.Settings, ShellIcons.Building2,
            ShellIcons.ScrollText, ShellIcons.Terminal, ShellIcons.BookOpen
        };
        var lockView = DefineComponent(new VueComponentOptions<ShellLockProps>
        {
            Props = (string[])["avatar", "username", "unlock", "logout"],
            Setup = (props, _) =>
            {
                var state = LockOverlayState.Create();
                state.Setup(props.Unlock);
                return () => H(LockView.Component, state.CreateProps(props.Avatar, props.Username, props.Logout));
            }
        });
        return DefineComponent(new VueComponentOptions
        {
            Setup = () =>
            {
                var route = UseRoute();
                var router = UseRouter();
                var frame = ShellFrameState.Create(new ShellFrameState.ShellRouter(
                    () => route.Path,
                    path => router.Push(path),
                    path => router.Replace(path),
                    path => router.Resolve(path).Href));
                frame.Setup();
                // 账号菜单、主题和未读数都会触发外壳渲染。导航只在路由、菜单、分组或标签变化时重新计算。
                var navigation = Computed(frame.CreateNavigation);

                var routeSlots = new RouterViewSlots
                {
                    // 查询参数变化时，重新挂载匹配到的路由组件。
                    Default = scope =>
                    {
                        var component = scope.Component ?? null;
                        return component is null ? [] : [CloneVNode(component, new ShellViewKey(route.FullPath))];
                    }
                };
                var frameSlots = new ShellSlots
                {
                    Route = () => [H(RouterView, routeSlots)],
                    Lock = () => frame.Locked
                        ? [H(lockView, new ShellLockProps(frame.State.User?.Avatar ?? "", frame.Username(), frame.Unlock, frame.Logout))]
                        : [],
                    TabIcon = props =>
                    {
                        var index = frame.IconIndex(props.Section);
                        return index < 0 ? [] : [H(icons[index], new ShellIconProps(13))];
                    },
                    SectionIcon = props =>
                    {
                        var index = frame.IconIndex(props.Title);
                        return index < 0
                            ? [H("span", new ShellGlyphProps(), props.Initial)]
                            : [H(icons[index], new ShellIconProps(18))];
                    }
                };
                return () => H(ShellConfig.Provider, new ShellLocaleProps(frame.Lang == "en" ? ShellEnglish.Locale : ShellChinese.Locale),
                    new ShellConfigSlots
                    {
                        Content = () => [H(ShellView.Component, frame.CreateProps(StringValue(route.Meta["title"] ?? (RouteMetaValue)""), navigation.Value), frameSlots)]
                    });
            }
        });
    }
}

public sealed record ShellViewKey([property: Description("@#key")] string Key) : VueProps;
public sealed record ShellIconProps([property: Description("@#size")] int Size) : VueProps;
public sealed record ShellLocaleProps([property: Description("@#locale")] IShellLocale Locale) : VueProps;
public sealed record ShellLockProps(
    [property: Description("@#avatar")] string Avatar,
    [property: Description("@#username")] string Username,
    [property: Description("@#unlock")] Action Unlock,
    [property: Description("@#logout")] Action Logout) : VueProps;
public sealed record ShellGlyphProps : VueProps
{
    [Description("@#class")]
    public string Class { get; init; } = "jero-shell__rail-glyph";
}
public sealed record ShellConfigSlots : VueSlots
{
    [Description("@#default")]
    public required Func<IVNode[]> Content { get; init; }
}
public sealed record ShellSlots : VueSlots
{
    [Description("@#route")]
    public required Func<IVNode[]> Route { get; init; }

    [Description("@#lock")]
    public required Func<IVNode[]> Lock { get; init; }

    [Description("@#tabIcon")]
    public required Func<ShellTabIconScope, IVNode[]> TabIcon { get; init; }

    [Description("@#sectionIcon")]
    public required Func<ShellSectionIconScope, IVNode[]> SectionIcon { get; init; }
}

[ECMAScript]
public sealed record ShellTabIconScope([property: ECMAScriptName("section")] string Section);
[ECMAScript]
public sealed record ShellSectionIconScope(
    [property: Description("@#title")] string Title,
    [property: Description("@#initial")] string Initial);
[ECMAScript]
public interface IShellLocale;

#pragma warning disable CS0626 // Vue and its dependencies own the JavaScript exports.
[ECMAScript("/Pages/Shell/ShellFrame.jazor")]
[Style("/Pages/Shell/shell.css")]
internal static class ShellView
{
    [Description("@#default")]
    public extern static IVueComponent Component { get; }
}

[ECMAScript("/Pages/Shell/LockOverlay.jazor")]
internal static class LockView
{
    [Description("@#default")]
    public extern static IVueComponent Component { get; }
}

[ECMAScript("element-plus")]
[Style("element-plus/dist/index.css")]
internal static class ShellConfig
{
    [Description("@#ElConfigProvider")]
    public extern static IVueComponent Provider { get; }
}

[ECMAScript("element-plus/es/locale/lang/en")]
internal static class ShellEnglish
{
    [Description("@#default")]
    public extern static IShellLocale Locale { get; }
}

[ECMAScript("element-plus/es/locale/lang/zh-cn")]
internal static class ShellChinese
{
    [Description("@#default")]
    public extern static IShellLocale Locale { get; }
}

[ECMAScript("lucide-vue-next")]
internal static class ShellIcons
{
    [Description("@#LayoutDashboard")]
    public extern static IVueComponent LayoutDashboard { get; }
    [Description("@#Settings")]
    public extern static IVueComponent Settings { get; }
    [Description("@#Building2")]
    public extern static IVueComponent Building2 { get; }
    [Description("@#ScrollText")]
    public extern static IVueComponent ScrollText { get; }
    [Description("@#Terminal")]
    public extern static IVueComponent Terminal { get; }
    [Description("@#BookOpen")]
    public extern static IVueComponent BookOpen { get; }
}
