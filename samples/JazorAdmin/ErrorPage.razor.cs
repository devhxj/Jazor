namespace JazorAdmin;

[String]
public enum ErrorKind
{
    [Description("@#not-found")]
    NotFound,

    [Description("@#internal-server-error")]
    InternalServerError
}

[ECMAScriptModule("./components/error")]
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

    // 成员位置包装：text.mjs 只导出成员函数，标记内直接限定 Text 会触发 phantom
    // 类名导入并导致浏览器模块链接失败。
    private string? NormalizedTitle
        => Text.Normalize(Title);

    private string? NormalizedDescription
        => Text.Normalize(Description);

    private string? NormalizedActionText
        => Text.Normalize(ActionText);

    private VueClassValue RootCssClass
        => BuildCssClass("ja-error", GetKindCssClass(Kind));

    private Task OnAction()
        => Action.InvokeAsync();

    private static string GetKindCssClass(ErrorKind kind)
        => kind == ErrorKind.InternalServerError
            ? "ja-error--internal-server-error"
            : "ja-error--not-found";
}
