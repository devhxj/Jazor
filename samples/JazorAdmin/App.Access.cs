namespace JazorAdmin;

public partial class App
{
    protected override void OnInitialized()
    {
        // Vue setup can read local storage before the first VNode is created. Restoring visual
        // state here avoids replacing the mounted session placeholder with a different slot tree.
        RestoreStarterStyleConfig();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        // RazorVue maps this lifecycle hook to Vue onMounted. Constructors are not part of setup(),
        // so browser-only session recovery starts only after the root is mounted.
        RestoreSession();
    }

    private bool IsLoginPage => currentRoute.Path == "/login";

    private void OpenLockScreen()
    {
        unlockPassword = string.Empty;
        accessError = null;
        _ = router.Push("/lock");
    }

    private void SignOut()
    {
        ApiClient.SignOut().Then(outcome =>
        {
            session = null;
            organizations = [];
            selectedOrganizationId = null;
            notificationsOpen = false;
            notificationsLoading = false;
            notifications = [];
            notificationRequestVersion++;
            loginPassword = string.Empty;
            loginCaptcha = string.Empty;
            unlockPassword = string.Empty;
            accessError = null;
            RefreshCaptcha();
            _ = router.Push("/login");
        });
    }

    private void SignIn()
    {
        if (Text.Normalize(loginAccount) is null || Text.Normalize(loginPassword) is null)
        {
            accessError = Localization.Get(Language, TextKey.LoginRequired);
            return;
        }

        if (Text.Normalize(loginCaptcha) is null || loginCaptchaId is null)
        {
            accessError = Localization.Get(Language, TextKey.VerificationCodeRequired);
            return;
        }

        ApiClient.SignIn(loginAccount, loginPassword, loginCaptchaId, loginCaptcha).Then(outcome =>
        {
            loginPassword = string.Empty;
            loginCaptcha = string.Empty;
            if (!outcome.Ok)
            {
                accessError = outcome.Error ?? "Sign-in failed.";
                RefreshCaptcha();
                return;
            }

            RestoreSession();
        });
    }

    private void Unlock()
    {
        if (Text.Normalize(unlockPassword) is null)
        {
            accessError = Localization.Get(Language, TextKey.UnlockRequired);
            return;
        }

        ApiClient.SignIn(loginAccount, unlockPassword).Then(outcome =>
        {
            unlockPassword = string.Empty;
            if (!outcome.Ok)
            {
                accessError = outcome.Error ?? "Unlock failed.";
                return;
            }

            RestoreSession();
        });
    }

    private void RestoreSession()
    {
        ApiClient.GetSession().Then(ApplySession);
    }

    private void ApplySession(ApiOutcome outcome)
    {
        sessionRestoring = false;
        if (!outcome.Ok || outcome.Data is null)
        {
            session = null;
            organizations = [];
            selectedOrganizationId = null;
            notificationsOpen = false;
            notificationsLoading = false;
            notifications = [];
            notificationRequestVersion++;
            RefreshCaptcha();
            if (!IsLoginPage && currentRoute.Path != "/lock")
                _ = router.Push("/login");
            return;
        }

        session = ApiClient.ToSession(outcome.Data);
        loginAccount = session.Email;
        organizations = session.Organizations;
        selectedOrganizationId ??= organizations.FirstOrDefault()?.Id;
        accessError = null;
        // The bell badge is meaningful before its panel opens, so hydrate it once for the
        // authenticated session. Opening the panel still refreshes the operational feed.
        // 会话恢复后先加载一次，铃铛才能在展开前显示未处理通知数量。
        LoadNotifications();
        if (IsLoginPage || currentRoute.Path == "/lock")
            _ = router.Push("/");
    }

    private void RefreshCaptcha()
    {
        loginCaptcha = string.Empty;
        loginCaptchaId = null;
        loginCaptchaImageUrl = null;
        var requestVersion = ++captchaRequestVersion;
        ApiClient.GetCaptcha().Then(outcome =>
        {
            if (requestVersion != captchaRequestVersion)
                return;

            if (!outcome.Ok || outcome.Data is null)
            {
                accessError = outcome.Error ?? "Verification code could not be loaded.";
                return;
            }

            var captcha = ApiClient.ToCaptchaChallenge(outcome.Data);
            loginCaptchaId = captcha.Id;
            loginCaptchaImageUrl = captcha.ImageUrl;
        });
    }
}
