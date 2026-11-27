using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Auth;
using Jero.Web.Infrastructure.Api.Shell;
using Jero.Web.Infrastructure.Realtime;
using Jero.Web.Pages.Auth;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Shell;

[ECMAScriptModule("components/shell-frame-state.js")]
public static class ShellFrameState
{
    public static Frame Create(ShellRouter router) => Reactive(new Frame(router));

    public sealed class Frame
    {
        private readonly ShellRouter _router;

        public Frame(ShellRouter router) => this._router = router;

        public ShellState.ShellSnapshot State = ShellState.GetState();
        public bool Mobile;
        public bool Dark = Document.DocumentElement!.ClassList.Contains("dark");
        public string Lang = Window.LocalStorage.GetItem("jero-lang") ?? "zh-cn";
        public bool UserMenu;
        public bool Locked = Window.LocalStorage.GetItem(ShellState.LockKey) == "1";
        public bool LogoutConfirm;
        public bool LoggingOut;
        public string SelectedSection = "";
        public AppNavigationItem[] Tabs = [];
        public TabContext? Context;
        public AppUnreadNotice[] Notices = [];
        public bool NoticeOpen;
        public bool NoticeLoading;
        public string? NoticeError;
        public AppUnreadNotice? NoticeDetail;
        public bool DetailVisible;
        private AppUnreadNotice[] _duringLoad = [];
        private bool _markingNotice;
        private Action[] _unsubscribe = [];
        private MediaQueryList? _desktop;
        private HandleEventCallback? _keyListener;
        private HandleEventCallback? _desktopListener;

        public void Setup()
        {
            Watch(() => _router.Path(), (current, previous) => { RouteChanged(); SyncTabs(); });
            Watch(() => State.Navigation, (current, previous) => SyncTabs(), new VueWatchOptions { Immediate = true });
            Watch(() => State.Ready, (current, previous) => Start(), new VueWatchOptions { Immediate = true });
            WatchEffect(() => SetOverflow(Mobile || Locked ? "hidden" : ""));
            OnMounted(Mount);
            OnUnmounted(Unmount);
        }

        private void Mount()
        {
            var media = Window.MatchMedia("(min-width: 861px)");
            _desktop = media;
            _keyListener = evt => HandleKey((KeyboardEvent)evt);
            _desktopListener = _ =>
            {
                if (media.Matches)
                    CloseMobile();
            };
            Window.AddEventListener("keydown", _keyListener!, false);
            media.AddEventListener("change", _desktopListener!, false);
        }

        private static void SetOverflow(string value)
        {
            var style = Reflect.Get(Document.Body!, "style");
            Reflect.Set(style!, "overflow", value);
        }

        private void Unmount()
        {
            Window.RemoveEventListener("keydown", _keyListener!, false);
            _desktop?.RemoveEventListener("change", _desktopListener!, false);
            SetOverflow("");
            Stop();
        }

        public ShellNavigation CreateNavigation()
        {
            var route = Current();
            return new(Active(route), CreateTabs(route), CreateSections(), CreateMenus(route));
        }

        public ShellFrameProps CreateProps(string routeTitle, ShellNavigation navigation) => new()
        {
            BrandLogo = State.Brand.Logo,
            SectionTitle = SectionTitle(),
            Ready = State.Ready,
            UserInitialText = Initial(),
            UserDisplayName = Username(),
            UserMeta = UserMeta(),
            CurrentSection = navigation.Active?.SectionTitle ?? "",
            CurrentTitle = navigation.Active?.Title ?? NonEmpty(routeTitle, State.Brand.Title),
            UnreadNoticeCount = Notices.Length,
            UnreadNotices = Notices,
            Lang = Lang,
            IsDark = Dark,
            NoticePanelOpen = NoticeOpen,
            NoticeLoading = NoticeLoading,
            NoticeError = NoticeError,
            UserMenuOpen = UserMenu,
            LoggingOut = LoggingOut,
            ToggleMobileMenu = ToggleMobile,
            OpenHome = OpenHome,
            ToggleTheme = ToggleTheme,
            ToggleNoticePanel = ToggleNotice,
            LoadUnreadNotices = LoadNotices,
            OpenNoticeCenter = OpenNoticeCenter,
            ToggleUserMenu = ToggleUserMenu,
            OpenProfile = OpenProfile,
            RequestLockScreen = RequestLock,
            ShowLogoutConfirm = ShowLogoutConfirm,
            SwitchLang = SwitchLang,
            OpenNotice = OpenNotice,
            Mobile = Mobile,
            CloseMobile = CloseMobile,
            Tabs = navigation.Tabs,
            ContextStyle = ContextStyle(),
            ContextActions = ContextActions(),
            CloseContext = CloseContext,
            Sections = navigation.Sections,
            Menus = navigation.Menus,
            Watermark = State.User?.WatermarkText ?? "",
            Loading = State.Loading,
            Error = State.Error,
            DetailVisible = DetailVisible,
            SetDetailVisible = SetDetailVisible,
            NoticeTitle = NonEmpty(NoticeDetail?.Title, "消息详情"),
            NoticeContent = NoticeDetail?.Content ?? "",
            LogoutConfirm = LogoutConfirm,
            SetLogoutConfirm = SetLogoutConfirm,
            Logout = Logout,
        };

        public string Current() => _router.Path().Substring(1);
        private AppNavigationItem? Active(string route) => State.Navigation.FirstOrDefault(item => item.Route == route) ?? RouteRegistry.ResolveSupplemental(route);
        public string[] Sections() => State.Navigation.Select(item => item.SectionTitle).Distinct().ToArray();
        public string SectionTitle() => NonEmpty(SelectedSection, State.Brand.Title);
        public string Initial() => Username().Substring(0, 1);
        public string Username() => NonEmpty(State.User?.RealName, NonEmpty(State.User?.Account, "当前用户"));
        public string UserMeta()
        {
            var org = State.User?.OrgName ?? "";
            var position = State.User?.PosName ?? "";
            if (org.Length == 0)
                return NonEmpty(position, State.User?.Account ?? "");
            return position.Length == 0 ? org : org + " · " + position;
        }
        public void RouteChanged() => UserMenu = false;
        public void SyncTabs()
        {
            var item = Active(Current());
            var section = item?.SectionTitle ?? "";
            var all = Sections();
            if (section.Length > 0 || !all.Contains(SelectedSection))
                SelectedSection = NonEmpty(section, all.Length > 0 ? all[0] : "");
            if (item?.IsSupported == true && !Tabs.Any(tab => tab.Route == item.Route))
                Tabs = [.. Tabs, item];
        }

        public void OpenHome() => _router.Push(ShellState.ResolveHome());
        public void Go(string path) => _router.Push("/" + path);
        public void OpenMenu(AppNavigationItem item)
        {
            Mobile = false;
            if (item.IsExternal)
                Window.Open(item.ExternalUrl, "_blank");
            else if (item.IsSupported)
                Go(item.Route);
        }
        public void OpenContext(string path, double x, double y)
            => Context = new TabContext(path, Math.Max(0, Math.Min(x, Window.InnerWidth - 170)), Math.Max(0, Math.Min(y, Window.InnerHeight - 250)));

        public void CloseTabs(Func<AppNavigationItem, int, bool> match, string mode)
        {
            if (mode == "single" && Tabs.Length <= 1)
            {
                Context = null;
                return;
            }
            var old = Tabs;
            var active = FindTab(Current());
            List<AppNavigationItem> retained = [];
            for (int index = 0; index < old.Length; index++)
            {
                var item = old[index];
                if (item.IsAffix || !match(item, index))
                    retained.Add(item);
            }
            Tabs = retained.ToArray();
            if (mode == "all" && Tabs.Length > 0 && Tabs[0].Route != Current())
                Go(Tabs[0].Route);
            else if (active >= 0 && FindTab(Current()) < 0)
            {
                var nextIndex = mode == "group" ? FindTab(Context?.Path) : Math.Max(0, Math.Min(active - 1, Tabs.Length - 1));
                if (nextIndex >= 0 && nextIndex < Tabs.Length)
                    Go(Tabs[nextIndex].Route);
                else
                    OpenHome();
            }
            Context = null;
        }
        public int FindTab(string? path)
        {
            for (int index = 0; index < Tabs.Length; index++)
                if (Tabs[index].Route == path)
                    return index;
            return -1;
        }

        public void RefreshTab()
        {
            var path = Context?.Path;
            Context = null;
            if (!string.IsNullOrEmpty(path))
                Reflect.Set(Window, "location", _router.Resolve("/" + path));
        }
        public void ToggleTheme()
        {
            Dark = Document.DocumentElement!.ClassList.Toggle("dark");
            Window.LocalStorage.SetItem("jero-theme", Dark ? "dark" : "light");
        }
        public void SwitchLang(string value)
        {
            Lang = value;
            Window.LocalStorage.SetItem("jero-lang", value);
        }

        public void RequestLock()
        {
            Mobile = false;
            NoticeOpen = false;
            UserMenu = false;
            Window.LocalStorage.SetItem(ShellState.LockKey, "1");
            Window.LocalStorage.SetItem(ShellState.LockPasswordKey, "");
            Locked = true;
        }
        public void Logout()
        {
            if (LoggingOut)
                return;
            LogoutConfirm = false;
            LoggingOut = true;
            // 清理放在 finally 中，服务端无法访问时也能结束本地会话。
            AuthClient.Logout().Finally(() =>
            {
                ShellState.Clear();
                _router.Replace("/login");
            }).Catch(_ => { });
        }
        public void HandleKey(KeyboardEvent evt)
        {
            if (!Locked && evt.AltKey && evt.Code == "KeyL")
            {
                evt.PreventDefault();
                RequestLock();
            }
            else if (evt.AltKey && evt.Code == "KeyQ")
            {
                evt.PreventDefault();
                LogoutConfirm = true;
            }
            else if (evt.Key == "Escape")
            {
                Mobile = false;
                Context = null;
            }
        }

        public void LoadNotices()
        {
            if (NoticeLoading || !State.Ready)
                return;
            NoticeLoading = true;
            _duringLoad = [];
            AppNoticeClient.UnreadList().Then(outcome =>
            {
                if (outcome.Ok)
                {
                    var items = AppNoticeClient.ReadUnread(outcome.Data);
                    // 把请求等待期间收到的通知合并回列表。
                    for (int index = _duringLoad.Length - 1; index >= 0; index--)
                        items = Prepend(items, _duringLoad[index]);
                    Notices = items;
                    NoticeError = null;
                }
                else
                    NoticeError = outcome.Error ?? "加载未读通知失败。";
            }).Catch(_ => NoticeError = "加载未读通知失败，请稍后重试。").Finally(() =>
            {
                NoticeLoading = false;
                _duringLoad = [];
            });
        }

        public void OpenNotice(AppUnreadNotice notice)
        {
            NoticeOpen = false;
            NoticeDetail = notice;
            DetailVisible = true;
            if (string.IsNullOrEmpty(notice.Id) || _markingNotice)
                return;
            _markingNotice = true;
            AppNoticeClient.SetRead(notice.Id).Then(outcome =>
            {
                if (outcome.Ok)
                {
                    Notices = Notices.Where(item => item.Id != notice.Id).ToArray();
                    NoticeError = null;
                }
                else
                    NoticeError = outcome.Error ?? "标记通知已读失败。";
            }).Catch(_ => NoticeError = "标记通知已读失败，请稍后重试。").Finally(() => _markingNotice = false);
        }

        public void Start()
        {
            if (!State.Ready || _unsubscribe.Length > 0)
                return;
            _unsubscribe = [
                RealtimeState.SubscribePublicNotice(ReceiveNotice),
                RealtimeState.SubscribeConnectionStatus(status =>
                {
                    if (status == RealtimeConnectionStatus.Reconnecting)
                        ElementPlus.ElMessageMembers.Warning("在线用户通道重连中...");
                }),
                RealtimeState.SubscribeForceOffline(message =>
                {
                    ElementPlus.ElMessageMembers.Warning(message);
                    Logout();
                })
            ];
            RealtimeClient.Start().Catch(_ => { });
            LoadNotices();
        }
        public void Stop()
        {
            foreach (var release in _unsubscribe)
                release();
            RealtimeClient.Stop().Catch(_ => { });
        }
        private void ReceiveNotice(RealtimePublicNotice notice)
        {
            var row = new AppUnreadNotice
            {
                Id = notice.Id, Title = notice.Title, Content = notice.Content, Type = notice.Type,
                PublicUserName = notice.PublicUserName, PublicTime = NonEmpty(notice.PublicTime, notice.CreateTime)
            };
            Notices = Prepend(Notices, row);
            if (NoticeLoading)
                _duringLoad = Prepend(_duringLoad, row);
            NoticeError = null;
            ElementPlus.ElMessageMembers.Info("您有一条新的站内公告");
        }
        private static AppUnreadNotice[] Prepend(AppUnreadNotice[] items, AppUnreadNotice notice)
            => [notice, .. items.Where(item => notice.Id.Length == 0 || item.Id != notice.Id)];


        public void ToggleMobile() => Mobile = !Mobile;
        public void CloseMobile() => Mobile = false;
        public void ToggleNotice() => NoticeOpen = !NoticeOpen;
        public void ToggleUserMenu() => UserMenu = !UserMenu;
        public void ShowLogoutConfirm() => LogoutConfirm = true;
        public void SetLogoutConfirm(bool value) => LogoutConfirm = value;
        public void SetDetailVisible(bool value) => DetailVisible = value;
        public void Unlock() => Locked = false;
        public void CloseContext() => Context = null;
        public void OpenNoticeCenter() { NoticeOpen = false; Go("received-notice"); }
        public void OpenProfile() { UserMenu = false; Go("profile"); }

        private ShellTab[] CreateTabs(string route)
            => !State.Ready ? [] : Tabs.Select(tab => new ShellTab(
                tab.Route, tab.Title, tab.SectionTitle, tab.Route == route, tab.IsAffix,
                () => Go(tab.Route),
                () => CloseTabs((item, _) => item.Route == tab.Route, "single"),
                new ShellTabContextHandler(evt => {
                    var mouse = (MouseEvent)evt;
                    mouse.PreventDefault();
                    OpenContext(tab.Route, mouse.ClientX, mouse.ClientY);
                }))).ToArray();

        public string ContextStyle() => Context is null ? "" : "left:" + Context.X + "px;top:" + Context.Y + "px";

        public ShellContextAction[] ContextActions()
        {
            if (Context is null)
                return [];
            var selected = FindTab(Context.Path);
            return [
                new("刷新", false, RefreshTab),
                new("关闭", selected < 0 || Tabs[selected].IsAffix, () => CloseTabs((_, i) => i == selected, "single")),
                new("关闭左侧", !HasClosableTab(i => i < selected), () => CloseTabs((_, i) => i < selected, "group")),
                new("关闭右侧", !HasClosableTab(i => i > selected), () => CloseTabs((_, i) => i > selected, "group")),
                new("关闭其他", !HasClosableTab(i => i != selected), () => CloseTabs((_, i) => i != selected, "group")),
                new("关闭全部", !HasClosableTab(_ => true), () => CloseTabs((current, previous) => true, "all"))
            ];
        }

        private bool HasClosableTab(Func<int, bool> predicate)
        {
            for (int index = 0; index < Tabs.Length; index++)
                if (!Tabs[index].IsAffix && predicate(index))
                    return true;
            return false;
        }

        public ShellSection[] CreateSections() => Sections().Select(section => new ShellSection(
            section, section.Length == 0 ? "?" : section.Substring(0, 1), section == SelectedSection,
            () => SelectedSection = section)).ToArray();

        private ShellMenu[] CreateMenus(string route) => State.Navigation.Where(item => item.SectionTitle == SelectedSection)
            .Select(item => new ShellMenu(item.Id, item.Title, item.Route == route, item.IsExternal, item.IsSupported,
                () => OpenMenu(item))).ToArray();

        public int IconIndex(string title)
        {
            if (title.Contains("工作")) return 0;
            if (title.Contains("系统")) return 1;
            if (title.Contains("平台")) return 2;
            if (title.Contains("日志")) return 3;
            if (title.Contains("开发")) return 4;
            if (title.Contains("帮助")) return 5;
            return -1;
        }

        public static string NonEmpty(string? value, string fallback) => string.IsNullOrEmpty(value) ? fallback : value;
    }

    public sealed record ShellNavigation(AppNavigationItem? Active, ShellTab[] Tabs, ShellSection[] Sections, ShellMenu[] Menus);

    public sealed record ShellFrameProps : VueProps
    {
        public string BrandLogo { get; init; } = default!;
        public string SectionTitle { get; init; } = default!;
        public bool Ready { get; init; } = default!;
        public string UserInitialText { get; init; } = default!;
        public string UserDisplayName { get; init; } = default!;
        public string UserMeta { get; init; } = default!;
        public string CurrentSection { get; init; } = default!;
        public string CurrentTitle { get; init; } = default!;
        public int UnreadNoticeCount { get; init; } = default!;
        public AppUnreadNotice[] UnreadNotices { get; init; } = default!;
        public string Lang { get; init; } = default!;
        public bool IsDark { get; init; } = default!;
        public bool NoticePanelOpen { get; init; } = default!;
        public bool NoticeLoading { get; init; } = default!;
        public string? NoticeError { get; init; } = default!;
        public bool UserMenuOpen { get; init; } = default!;
        public bool LoggingOut { get; init; } = default!;
        public Action ToggleMobileMenu { get; init; } = default!;
        public Action OpenHome { get; init; } = default!;
        public Action ToggleTheme { get; init; } = default!;
        public Action ToggleNoticePanel { get; init; } = default!;
        public Action LoadUnreadNotices { get; init; } = default!;
        public Action OpenNoticeCenter { get; init; } = default!;
        public Action ToggleUserMenu { get; init; } = default!;
        public Action OpenProfile { get; init; } = default!;
        public Action RequestLockScreen { get; init; } = default!;
        public Action ShowLogoutConfirm { get; init; } = default!;
        public Action<string> SwitchLang { get; init; } = default!;
        public Action<AppUnreadNotice> OpenNotice { get; init; } = default!;
        public bool Mobile { get; init; } = default!;
        public Action CloseMobile { get; init; } = default!;
        public ShellTab[] Tabs { get; init; } = default!;
        public string ContextStyle { get; init; } = default!;
        public ShellContextAction[] ContextActions { get; init; } = default!;
        public Action CloseContext { get; init; } = default!;
        public ShellSection[] Sections { get; init; } = default!;
        public ShellMenu[] Menus { get; init; } = default!;
        public string Watermark { get; init; } = default!;
        public bool Loading { get; init; } = default!;
        public string? Error { get; init; } = default!;
        public bool DetailVisible { get; init; } = default!;
        public Action<bool> SetDetailVisible { get; init; } = default!;
        public string NoticeTitle { get; init; } = default!;
        public string NoticeContent { get; init; } = default!;
        public bool LogoutConfirm { get; init; } = default!;
        public Action<bool> SetLogoutConfirm { get; init; } = default!;
        public Action Logout { get; init; } = default!;
    }

    public sealed record ShellRouter(Func<string> Path, Action<string> Push, Action<string> Replace, Func<string, string> Resolve);
    public sealed record TabContext(string Path, double X, double Y);
}
