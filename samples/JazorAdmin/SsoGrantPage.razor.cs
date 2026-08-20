using ECMAScript.TDesign;
using JazorAdmin.Features.Sso;
using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[String]
public enum GrantView
{
    [Description("@#authorizations")]
    Authorizations,

    [Description("@#tokens")]
    Tokens
}

[ECMAScriptModule("components/sso-grant")]
public partial class SsoGrantPage : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public GrantView View { get; set; }

    private bool loading = true;
    private string? error;
    private AuthorizationView[] authorizations = [];
    private TokenView[] tokens = [];

    // 两张运营表的列定义：首列承载 data-sso-authorization / data-sso-token 行锚点，
    // 撤销按钮按状态禁用，撤销动作仍由管理端状态迁移决定。
    private TPrimaryTableCol<AuthorizationView>[] AuthorizationColumns =>
    [
        new() { Title = (TPrimaryTableColTitle<AuthorizationView>)L("Application", "应用"), Cell = (TPrimaryTableColCell<AuthorizationView>)((RenderFragment<TPrimaryTableCellParams<AuthorizationView>>)(context => builder =>
            {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "data-sso-authorization", context.Row.Id);
        builder.AddContent(2, context.Row.ClientId ?? "-");
        builder.CloseElement();
            })) },
        new() { ColKey = "Subject", Title = (TPrimaryTableColTitle<AuthorizationView>)L("Subject", "主体") },
        new() { ColKey = "Type", Title = (TPrimaryTableColTitle<AuthorizationView>)L("Type", "类型") },
        new() { ColKey = "Status", Title = (TPrimaryTableColTitle<AuthorizationView>)L("Status", "状态") },
        new() { Title = (TPrimaryTableColTitle<AuthorizationView>)L("Scopes", "Scope"), Cell = (TPrimaryTableColCell<AuthorizationView>)((RenderFragment<TPrimaryTableCellParams<AuthorizationView>>)(context => builder =>
            {
        builder.AddContent(0, Join(context.Row.Scopes));
            })) },
        new() { Title = (TPrimaryTableColTitle<AuthorizationView>)L("Actions", "操作"), Cell = (TPrimaryTableColCell<AuthorizationView>)((RenderFragment<TPrimaryTableCellParams<AuthorizationView>>)(context => builder =>
            {
        builder.OpenComponent<TButton>(0);
        builder.AddComponentParameter(1, nameof(TButton.Variant), TButtonVariantValue.Text);
        builder.AddComponentParameter(2, nameof(TButton.Size), TSizeEnum.Small);
        builder.AddComponentParameter(3, nameof(TButton.Theme), TButtonThemeValue.Danger);
        builder.AddComponentParameter(4, nameof(TButton.Disabled), context.Row.Status == "revoked");
        builder.AddComponentParameter(5, "data-sso-command", "revoke-authorization");
        builder.AddComponentParameter(6, nameof(TButton.OnClick),
            EventCallback.Factory.Create(this, () => RevokeAuthorization(context.Row.Id)));
        builder.AddComponentParameter(7, nameof(TContentComponentBase.ChildContent),
            (RenderFragment)(child => child.AddContent(0, L("Revoke", "撤销"))));
        builder.CloseComponent();
            })) }
    ];

    private TPrimaryTableCol<TokenView>[] TokenColumns =>
    [
        new() { Title = (TPrimaryTableColTitle<TokenView>)L("Application", "应用"), Cell = (TPrimaryTableColCell<TokenView>)((RenderFragment<TPrimaryTableCellParams<TokenView>>)(context => builder =>
            {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "data-sso-token", context.Row.Id);
        builder.AddContent(2, context.Row.ClientId ?? "-");
        builder.CloseElement();
            })) },
        new() { ColKey = "Subject", Title = (TPrimaryTableColTitle<TokenView>)L("Subject", "主体") },
        new() { ColKey = "Type", Title = (TPrimaryTableColTitle<TokenView>)L("Type", "类型") },
        new() { ColKey = "Status", Title = (TPrimaryTableColTitle<TokenView>)L("Status", "状态") },
        new() { ColKey = "ExpiresAt", Title = (TPrimaryTableColTitle<TokenView>)L("Expires", "过期时间") },
        new() { Title = (TPrimaryTableColTitle<TokenView>)L("Actions", "操作"), Cell = (TPrimaryTableColCell<TokenView>)((RenderFragment<TPrimaryTableCellParams<TokenView>>)(context => builder =>
            {
        builder.OpenComponent<TButton>(0);
        builder.AddComponentParameter(1, nameof(TButton.Variant), TButtonVariantValue.Text);
        builder.AddComponentParameter(2, nameof(TButton.Size), TSizeEnum.Small);
        builder.AddComponentParameter(3, nameof(TButton.Theme), TButtonThemeValue.Danger);
        builder.AddComponentParameter(4, nameof(TButton.Disabled), context.Row.Status == "revoked");
        builder.AddComponentParameter(5, "data-sso-command", "revoke-token");
        builder.AddComponentParameter(6, nameof(TButton.OnClick),
            EventCallback.Factory.Create(this, () => RevokeToken(context.Row.Id)));
        builder.AddComponentParameter(7, nameof(TContentComponentBase.ChildContent),
            (RenderFragment)(child => child.AddContent(0, L("Revoke", "撤销"))));
        builder.CloseComponent();
            })) }
    ];

    protected override void OnParametersSet() => Load();

    private void Load()
    {
        loading = true;
        error = null;
        if (View == GrantView.Authorizations)
            ApiClient.GetAuthorizations().Then(ApplyAuthorizations);
        else
            ApiClient.GetTokens().Then(ApplyTokens);
    }

    private void ApplyAuthorizations(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load OpenID authorizations.", "无法加载 OpenID 授权记录。");
            return;
        }
        authorizations = ApiClient.ToAuthorizations(outcome.Data);
    }

    private void ApplyTokens(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load OpenID tokens.", "无法加载 OpenID 令牌。");
            return;
        }
        tokens = ApiClient.ToTokens(outcome.Data);
    }

    private void RevokeAuthorization(string id)
    {
        ApiClient.RevokeAuthorization(id).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to revoke the authorization.", "无法撤销该授权。");
                return;
            }
            Load();
        });
    }

    private void RevokeToken(string id)
    {
        ApiClient.RevokeToken(id).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to revoke the token.", "无法撤销该令牌。");
                return;
            }
            Load();
        });
    }

    private static string Join(string[] values)
        => values.Length == 0 ? "-" : string.Join(", ", values);
}
