using static ECMAScript.Global;

namespace Jero.Web.Pages.Shell;

[ECMAScriptModule("components/lock-overlay-state.js")]
public static class LockOverlayState
{
    public static LockViewState Create() => Reactive(new LockViewState());

    public sealed class LockViewState
    {
        public string Password = "";
        public string Error = "";
        public bool ShowForm;
        public Date Now = new();
        private int _timer;

        public void Setup(Action unlock)
        {
            _onUnlock = unlock;
            OnMounted(Mount);
            OnUnmounted(Unmount);
        }

        private Action? _onUnlock;
        private void OnSubmit(EventRef evt)
        {
            evt.PreventDefault();
            if (Submit())
                _onUnlock?.Invoke();
        }

        private void OnKeydown(EventRef evt)
        {
            if (((KeyboardEvent)evt).Key == "Escape")
            {
                evt.PreventDefault();
                Hide();
            }
        }

        public LockOverlayProps CreateProps(string avatar, string username, Action logout) => new()
        {
            Avatar = avatar,
            Username = username,
            Initial = username.Length == 0 ? "" : username.Substring(0, 1),
            Hour = Hour(),
            Minute = Minute(),
            Date = Now.ToLocaleDateString("zh-CN", new Intl.DateTimeFormatOptions(
                Weekday: Intl.LongShortNarrow.Long,
                Year: Intl.NumericTwoDigit.Numeric,
                Month: Intl.LongShortNarrow.Long,
                Day: Intl.NumericTwoDigit.Numeric)),
            ShowForm = ShowForm,
            Error = Error,
            Password = Password,
            PasswordInputId = "jero-lock-password",
            FormHandlers = new LockFormHandlers(OnSubmit, OnKeydown),
            Show = Show,
            Hide = Hide,
            Logout = logout,
            UpdatePassword = value => Password = value
        };

        public void Mount() => _timer = Window.SetInterval((TimerHandler)(Delegate)(Action)(() => Now = new Date()), 1000, []);
        public void Unmount() => Window.ClearInterval(_timer);
        public string Hour() => TwoDigits((int)Now.GetHours());
        public string Minute() => TwoDigits((int)Now.GetMinutes());

        public void Show()
        {
            ShowForm = true;
            NextTick(() => (Document.GetElementById("jero-lock-password") as HTMLInputElement)?.Focus());
        }

        public void Hide()
        {
            ShowForm = false;
            Password = "";
            Error = "";
        }

        public bool Submit()
        {
            if (Password != (Window.LocalStorage.GetItem(ShellState.LockPasswordKey) ?? ""))
            {
                Error = "锁屏密码不正确。";
                return false;
            }
            Window.LocalStorage.RemoveItem(ShellState.LockKey);
            Window.LocalStorage.RemoveItem(ShellState.LockPasswordKey);
            return true;
        }

        private static string TwoDigits(int number) => number < 10 ? "0" + number : number.ToString();
    }

    public sealed record LockOverlayProps : VueProps
    {
        public string Avatar { get; init; } = "";
        public string Username { get; init; } = "";
        public string Initial { get; init; } = "";
        public string Hour { get; init; } = "";
        public string Minute { get; init; } = "";
        public string Date { get; init; } = "";
        public bool ShowForm { get; init; }
        public string Error { get; init; } = "";
        public string Password { get; init; } = "";
        public string PasswordInputId { get; init; } = "";
        public LockFormHandlers FormHandlers { get; init; } = default!;
        public Action Show { get; init; } = default!;
        public Action Hide { get; init; } = default!;
        public Action Logout { get; init; } = default!;
        public Action<string> UpdatePassword { get; init; } = default!;
    }
}

