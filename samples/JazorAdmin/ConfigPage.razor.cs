// Loads and registers OpenIddict clients and scopes through the configuration API.
// 通过配置 API 加载和注册 OpenIddict 客户端与 scope。
using JazorAdmin.Features.Configuration;

namespace JazorAdmin;

[String]
public enum ConfigView
{
    [Description("@#clients")]
    Clients,

    [Description("@#scopes")]
    Scopes
}

[ECMAScriptModule("components/config.mjs")]
public partial class ConfigPage : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public ConfigView View { get; set; }

    private bool loading = true;
    private string? error;
    private OpenIdClientResponse[] clients = [];
    private OpenIdScopeResponse[] scopes = [];
    private string clientId = string.Empty;
    private string clientDisplayName = string.Empty;
    private string redirectUri = string.Empty;
    private string postLogoutUri = string.Empty;
    private string clientScopes = string.Empty;
    private string scopeName = string.Empty;
    private string scopeDisplayName = string.Empty;

    protected override void OnParametersSet()
    {
        // The configuration tab is a route-fed prop, so lifecycle-driven loading also refreshes
        // when the active tab changes. 配置页签由路由参数驱动，切换页签时需要在此重新加载。
        Load();
    }

    private void Load()
    {
        loading = true;
        error = null;
        if (View == ConfigView.Clients)
            ApiClient.GetClients().Then(ApplyClients);
        else
            ApiClient.GetScopes().Then(ApplyScopes);
    }

    private void ApplyClients(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? "Unable to load OIDC clients.";
            return;
        }

        clients = ApiClient.ToClients(outcome.Data);
    }

    private void ApplyScopes(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? "Unable to load OIDC scopes.";
            return;
        }

        scopes = ApiClient.ToScopes(outcome.Data);
    }

    private void CreateClient()
    {
        if (Text.Normalize(clientId) is null || Text.Normalize(clientDisplayName) is null ||
            Text.Normalize(redirectUri) is null || Text.Normalize(postLogoutUri) is null)
        {
            return;
        }

        ApiClient.CreateClient(
            clientId,
            clientDisplayName,
            [redirectUri],
            [postLogoutUri],
            Split(clientScopes)).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to register the OIDC client.";
                return;
            }

            clientId = string.Empty;
            clientDisplayName = string.Empty;
            redirectUri = string.Empty;
            postLogoutUri = string.Empty;
            clientScopes = string.Empty;
            Load();
        });
    }

    private void CreateScope()
    {
        if (Text.Normalize(scopeName) is null || Text.Normalize(scopeDisplayName) is null)
            return;

        ApiClient.CreateScope(scopeName, scopeDisplayName).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to create the OIDC scope.";
                return;
            }

            scopeName = string.Empty;
            scopeDisplayName = string.Empty;
            Load();
        });
    }

    private static string[] Split(string value)
        => value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Join(string[] values)
        => values.Length == 0 ? "-" : string.Join(", ", values);
}
