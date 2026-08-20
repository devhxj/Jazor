// Owns the editable projection of one OpenIddict application descriptor.
using ECMAScript.TDesign;
using JazorAdmin.Features.Sso;
using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("components/sso-app")]
public partial class SsoAppPage : AppComponentBase, IVueContainerComponent
{
    private bool loading = true;
    private string? error;
    private AppView[] applications = [];
    private string? selectedId;
    private string profile = "interactive";
    private string clientId = string.Empty;
    private string displayName = string.Empty;
    private string applicationType = "web";
    private string clientType = "public";
    private string consentType = "implicit";
    private bool requirePkce = true;
    private string redirectUris = string.Empty;
    private string postLogoutRedirectUris = string.Empty;
    private string scopeValues = "openid profile";
    private bool endpointAuthorization = true;
    private bool endpointEndSession = true;
    private bool endpointIntrospection;
    private bool endpointRevocation;
    private bool endpointToken = true;
    private bool grantAuthorizationCode = true;
    private bool grantClientCredentials;
    private bool grantRefreshToken = true;
    private bool responseCode = true;
    private string? issuedSecret;
    private bool deleteArmed;
    private int loadVersion;

    private bool IsNew => selectedId is null;

    private bool IsConfidential => clientType == "confidential";

    // TDesign 表格列：应用列承载 data-sso-application=ClientId 锚点，浏览器验证用它
    // 断言新建的 machine/API 应用行与一次性密钥展示。
    private TPrimaryTableCol<AppView>[] Columns =>
    [
        new() { Title = (TPrimaryTableColTitle<AppView>)L("Application", "应用"), Cell = (TPrimaryTableColCell<AppView>)((RenderFragment<TPrimaryTableCellParams<AppView>>)(context => builder =>
            {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "data-sso-application", context.Row.ClientId);
        builder.OpenElement(2, "strong");
        builder.AddContent(3, context.Row.DisplayName);
        builder.CloseElement();
        builder.OpenElement(4, "span");
        builder.AddContent(5, context.Row.ClientId);
        builder.CloseElement();
        builder.CloseElement();
            })) },
        new() { ColKey = "Profile", Title = (TPrimaryTableColTitle<AppView>)L("Profile", "配置") },
        new() { ColKey = "ClientType", Title = (TPrimaryTableColTitle<AppView>)L("Client type", "客户端类型") },
        new() { Title = (TPrimaryTableColTitle<AppView>)L("Actions", "操作"), Cell = (TPrimaryTableColCell<AppView>)((RenderFragment<TPrimaryTableCellParams<AppView>>)(context => builder =>
            {
        builder.OpenComponent<TButton>(0);
        builder.AddComponentParameter(1, nameof(TButton.Variant), TButtonVariantValue.Text);
        builder.AddComponentParameter(2, nameof(TButton.Size), TSizeEnum.Small);
        builder.AddComponentParameter(3, nameof(TButton.OnClick),
            EventCallback.Factory.Create(this, () => Select(context.Row)));
        builder.AddComponentParameter(4, nameof(TContentComponentBase.ChildContent),
            (RenderFragment)(child => child.AddContent(0, L("Manage", "管理"))));
        builder.CloseComponent();
            })) }
    ];

    private TTableRowClassNameValue<AppView> SelectedRowClassName
        => (TTableRowClassNameValueOption2<AppView>)SelectedRowClass;

    private TClassName SelectedRowClass(TRowClassNameParams<AppView> parameters)
        => parameters.Row.Id == selectedId ? (TClassName)"ja-table-row-selected" : (TClassName)string.Empty;

    protected override void OnInitialized() => Load();

    private void Load() => Reload(null);

    private void Reload(string? secret)
    {
        var requestVersion = ++loadVersion;
        loading = true;
        error = null;
        ApiClient.GetApps().Then(outcome =>
        {
            if (requestVersion != loadVersion)
                return;

            ApplyApps(outcome);
            issuedSecret = secret;
        });
    }

    private void ApplyApps(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load OpenID applications.", "无法加载 OpenID 应用。");
            return;
        }

        applications = ApiClient.ToApps(outcome.Data);
        if (selectedId is not null)
        {
            var selected = applications.FirstOrDefault(value => value.Id == selectedId);
            if (selected is not null)
            {
                Select(selected);
                return;
            }
        }
        if (applications.Length > 0)
            Select(applications[0]);
    }

    private void Select(AppView application)
    {
        selectedId = application.Id;
        profile = application.Profile;
        clientId = application.ClientId;
        displayName = application.DisplayName;
        applicationType = application.ApplicationType;
        clientType = application.ClientType;
        consentType = application.ConsentType;
        requirePkce = application.RequirePkce;
        redirectUris = JoinLines(application.RedirectUris);
        postLogoutRedirectUris = JoinLines(application.PostLogoutRedirectUris);
        scopeValues = Join(application.Scopes);
        endpointAuthorization = Has(application.Endpoints, "authorization");
        endpointEndSession = Has(application.Endpoints, "end_session");
        endpointIntrospection = Has(application.Endpoints, "introspection");
        endpointRevocation = Has(application.Endpoints, "revocation");
        endpointToken = Has(application.Endpoints, "token");
        grantAuthorizationCode = Has(application.GrantTypes, "authorization_code");
        grantClientCredentials = Has(application.GrantTypes, "client_credentials");
        grantRefreshToken = Has(application.GrantTypes, "refresh_token");
        responseCode = Has(application.ResponseTypes, "code");
        issuedSecret = null;
        deleteArmed = false;
    }

    private void NewInteractive()
    {
        Reset("interactive");
        applicationType = "web";
        clientType = "public";
        consentType = "implicit";
        requirePkce = true;
        endpointAuthorization = true;
        endpointEndSession = true;
        endpointToken = true;
        grantAuthorizationCode = true;
        grantRefreshToken = true;
        responseCode = true;
        scopeValues = "openid profile";
    }

    private void NewMachine()
    {
        Reset("machine");
        applicationType = "web";
        clientType = "confidential";
        consentType = "implicit";
        endpointToken = true;
        endpointRevocation = true;
        grantClientCredentials = true;
    }

    private void NewApi()
    {
        Reset("api");
        applicationType = "web";
        clientType = "confidential";
        consentType = "implicit";
        endpointIntrospection = true;
    }

    // 单选预设入口：仅在新建态由 profile 单选组触发，复用既有预设方法避免双份默认值。
    private void SelectProfile(string value)
    {
        if (selectedId is not null)
            return;

        switch (value)
        {
            case "machine":
                NewMachine();
                break;
            case "api":
                NewApi();
                break;
            default:
                NewInteractive();
                break;
        }
    }

    private void Reset(string value)
    {
        // A new preset is explicit user intent. Invalidate an older reload so its completion cannot
        // select an existing application again and replace the editor the user just opened.
        // 新建预设是明确的用户意图；失效旧的 reload，避免其完成后重新选中已有应用并覆盖刚打开的编辑器。
        loadVersion++;
        selectedId = null;
        profile = value;
        clientId = string.Empty;
        displayName = string.Empty;
        applicationType = "web";
        clientType = "public";
        consentType = "implicit";
        requirePkce = false;
        redirectUris = string.Empty;
        postLogoutRedirectUris = string.Empty;
        scopeValues = string.Empty;
        endpointAuthorization = false;
        endpointEndSession = false;
        endpointIntrospection = false;
        endpointRevocation = false;
        endpointToken = false;
        grantAuthorizationCode = false;
        grantClientCredentials = false;
        grantRefreshToken = false;
        responseCode = false;
        issuedSecret = null;
        deleteArmed = false;
        error = null;
    }

    private void Save()
    {
        error = null;
        var endpoints = BuildEndpoints();
        var grants = BuildGrants();
        var responses = responseCode ? new[] { "code" } : [];
        var redirects = SplitValues(redirectUris);
        var logoutUris = SplitValues(postLogoutRedirectUris);
        var scopes = SplitValues(scopeValues);

        if (selectedId is null)
        {
            ApiClient.CreateApp(new AppCreate(
                clientId,
                displayName,
                applicationType,
                clientType,
                consentType,
                requirePkce,
                redirects,
                logoutUris,
                endpoints,
                grants,
                responses,
                scopes)).Then(ApplyMutation);
        }
        else
        {
            ApiClient.UpdateApp(selectedId, new AppUpdate(
                displayName,
                applicationType,
                clientType,
                consentType,
                requirePkce,
                redirects,
                logoutUris,
                endpoints,
                grants,
                responses,
                scopes)).Then(ApplyMutation);
        }
    }

    private void ApplyMutation(ApiOutcome outcome)
    {
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to save the OpenID application.", "无法保存 OpenID 应用。");
            return;
        }

        var result = ApiClient.ToAppSaved(outcome.Data!);
        selectedId = result.App.Id;
        Reload(result.Secret);
    }

    private void RotateSecret()
    {
        if (selectedId is null)
            return;

        error = null;
        ApiClient.RotateAppSecret(selectedId).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to rotate the client secret.", "无法轮换客户端密钥。");
                return;
            }
            issuedSecret = ApiClient.ReadSecret(outcome.Data!);
        });
    }

    private void DeleteApp()
    {
        if (selectedId is null)
            return;
        if (!deleteArmed)
        {
            deleteArmed = true;
            return;
        }

        ApiClient.DeleteApp(selectedId).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to delete the OpenID application.", "无法删除 OpenID 应用。");
                return;
            }
            NewInteractive();
            Load();
        });
    }

    private string[] BuildEndpoints()
    {
        var values = new List<string>();
        if (endpointAuthorization) values.Add("authorization");
        if (endpointEndSession) values.Add("end_session");
        if (endpointIntrospection) values.Add("introspection");
        if (endpointRevocation) values.Add("revocation");
        if (endpointToken) values.Add("token");
        return values.ToArray();
    }

    private string[] BuildGrants()
    {
        var values = new List<string>();
        if (grantAuthorizationCode) values.Add("authorization_code");
        if (grantClientCredentials) values.Add("client_credentials");
        if (grantRefreshToken) values.Add("refresh_token");
        return values.ToArray();
    }

    private static bool Has(string[] values, string value)
        => values.Contains(value);

    private static string[] SplitValues(string value)
        => value.Split([' ', '\r', '\n', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Join(string[] values)
        => values.Length == 0 ? string.Empty : string.Join(" ", values);

    private static string JoinLines(string[] values)
        => values.Length == 0 ? string.Empty : string.Join("\n", values);
}
