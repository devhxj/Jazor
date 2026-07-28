namespace JazorAdmin;

[String]
public enum ErrorKind
{
    [Description("@#not-found")]
    NotFound,

    [Description("@#internal-server-error")]
    InternalServerError
}

[ECMAScriptModule("./components/jazor-admin-error-page")]
public partial class ErrorPage : AppComponentBase, IVueComponent
{
    [Parameter]
    public ErrorKind Kind { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? ActionText { get; set; }

    [Parameter]
    public EventCallback Action { get; set; }

    private string ErrorCode
        => Kind == ErrorKind.InternalServerError ? "500" : "404";

    private VueClassValue RootCssClass
        => BuildCssClass("jazor-admin-error", GetKindCssClass(Kind));

    private Task OnAction()
        => Action.InvokeAsync();

    private static string GetKindCssClass(ErrorKind kind)
        => kind == ErrorKind.InternalServerError
            ? "jazor-admin-error--internal-server-error"
            : "jazor-admin-error--not-found";
}
