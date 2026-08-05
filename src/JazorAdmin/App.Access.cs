using JazorAdmin.Frontend;
using JazorAdmin.Features.Identity;

namespace JazorAdmin;

public partial class App
{
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        // RazorVue maps this lifecycle hook to Vue onMounted. Constructors are not part of setup(),
        // so browser-only session recovery must start here. RazorVue 会映射到 Vue onMounted；构造函数不进入 setup，浏览器会话恢复只能在这里启动。
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
        JazorAdminApiClient.SignOut().Then(outcome =>
        {
            session = null;
            organizations = [];
            selectedOrganizationId = null;
            loginPassword = string.Empty;
            unlockPassword = string.Empty;
            accessError = null;
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

        JazorAdminApiClient.SignIn(loginAccount, loginPassword).Then(outcome =>
        {
            loginPassword = string.Empty;
            if (!outcome.Ok)
            {
                accessError = outcome.Error ?? "Sign-in failed.";
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

        JazorAdminApiClient.SignIn(loginAccount, unlockPassword).Then(outcome =>
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
        JazorAdminApiClient.GetSession().Then(ApplySession);
    }

    private void ApplySession(AdminApiOutcome outcome)
    {
        sessionRestoring = false;
        if (!outcome.Ok || outcome.Data is null)
        {
            session = null;
            organizations = [];
            selectedOrganizationId = null;
            if (!IsLoginPage && currentRoute.Path != "/lock")
                _ = router.Push("/login");
            return;
        }

        session = JazorAdminApiClient.ToSession(outcome.Data);
        loginAccount = session.Email;
        organizations = session.Organizations;
        selectedOrganizationId ??= organizations.FirstOrDefault()?.Id;
        accessError = null;
        if (IsLoginPage || currentRoute.Path == "/lock")
            _ = router.Push("/");
    }
}
