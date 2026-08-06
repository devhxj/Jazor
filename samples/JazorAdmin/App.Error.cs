namespace JazorAdmin;

public partial class App
{
    private bool IsInternalErrorPage => currentRoute.Path == Routes.InternalErrorPath;

    private void ReturnHome()
        => _ = router.Push("/");
}
