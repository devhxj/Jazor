using JazorAdmin.Features.Sso;

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
            error = outcome.Error ?? "Unable to load OpenID authorizations.";
            return;
        }
        authorizations = ApiClient.ToAuthorizations(outcome.Data);
    }

    private void ApplyTokens(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? "Unable to load OpenID tokens.";
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
                error = outcome.Error ?? "Unable to revoke the authorization.";
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
                error = outcome.Error ?? "Unable to revoke the token.";
                return;
            }
            Load();
        });
    }

    private static string Join(string[] values)
        => values.Length == 0 ? "-" : string.Join(", ", values);
}
