namespace JazorAdmin;

public partial class App
{
    private bool IsLoginPage => currentRoute.Path == "/login";

    private void OpenLockScreen()
    {
        unlockPassword = string.Empty;
        accessError = null;
        _ = router.Push("/lock");
    }

    private void SignOut()
    {
        loginPassword = string.Empty;
        unlockPassword = string.Empty;
        accessError = null;
        _ = router.Push("/login");
    }

    private void SignIn()
    {
        if (Text.Normalize(loginAccount) is null || Text.Normalize(loginPassword) is null)
        {
            accessError = Localization.Get(Language, TextKey.LoginRequired);
            return;
        }

        accessError = null;
        loginPassword = string.Empty;
        _ = router.Push("/");
    }

    private void Unlock()
    {
        if (Text.Normalize(unlockPassword) is null)
        {
            accessError = Localization.Get(Language, TextKey.UnlockRequired);
            return;
        }

        accessError = null;
        unlockPassword = string.Empty;
        _ = router.Push("/");
    }
}
