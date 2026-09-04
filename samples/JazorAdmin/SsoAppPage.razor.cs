// Owns the editable projection of one OpenIddict application descriptor.
using ECMAScript.TDesign;
using JazorAdmin.Features.Sso;
using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("components/sso-app")]
public partial class SsoAppPage : AppComponentBase, IVueContainerComponent
{
    private sealed record AppDraft
    {
        public string ClientId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string ApplicationType { get; set; } = "web";

        public string ClientType { get; set; } = "public";

        public string ConsentType { get; set; } = "implicit";

        public TTextareaValue RedirectUris { get; set; } = string.Empty;

        public TTextareaValue PostLogoutRedirectUris { get; set; } = string.Empty;

        public string ScopeValues { get; set; } = string.Empty;

        public bool EndpointAuthorization { get; set; }

        public bool EndpointToken { get; set; }

        public bool EndpointEndSession { get; set; }

        public bool EndpointIntrospection { get; set; }

        public bool EndpointRevocation { get; set; }

        public bool GrantAuthorizationCode { get; set; }

        public bool GrantClientCredentials { get; set; }

        public bool GrantRefreshToken { get; set; }

        public bool ResponseCode { get; set; }

        public bool RequirePkce { get; set; }
    }

    private bool loading = true;
    private string? error;
    private AppView[] applications = [];
    private string? selectedId;
    private string profile = "interactive";
    private AppDraft Draft { get; set; } = NewDraft();
    private string? issuedSecret;
    private bool deleteArmed;
    private int loadVersion;

    private bool IsNew => selectedId is null;

    private bool IsConfidential => Draft.ClientType == "confidential";

    private TFormRules<AppDraft> DraftRules { get; } = new()
    {
        ["clientId"] =
        [
            new TFormRule { Required = true, Message = "Enter a client ID." }
        ],
        ["displayName"] =
        [
            new TFormRule { Required = true, Message = "Enter a display name." }
        ],
        ["applicationType"] =
        [
            new TFormRule { Required = true, Message = "Select an application type." }
        ],
        ["clientType"] =
        [
            new TFormRule { Required = true, Message = "Select a client type." }
        ],
        ["consentType"] =
        [
            new TFormRule { Required = true, Message = "Select a consent type." }
        ]
    };

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
        Draft = new AppDraft
        {
            ClientId = application.ClientId,
            DisplayName = application.DisplayName,
            ApplicationType = application.ApplicationType,
            ClientType = application.ClientType,
            ConsentType = application.ConsentType,
            RequirePkce = application.RequirePkce,
            RedirectUris = JoinLines(application.RedirectUris),
            PostLogoutRedirectUris = JoinLines(application.PostLogoutRedirectUris),
            ScopeValues = Join(application.Scopes),
            EndpointAuthorization = Has(application.Endpoints, "authorization"),
            EndpointEndSession = Has(application.Endpoints, "end_session"),
            EndpointIntrospection = Has(application.Endpoints, "introspection"),
            EndpointRevocation = Has(application.Endpoints, "revocation"),
            EndpointToken = Has(application.Endpoints, "token"),
            GrantAuthorizationCode = Has(application.GrantTypes, "authorization_code"),
            GrantClientCredentials = Has(application.GrantTypes, "client_credentials"),
            GrantRefreshToken = Has(application.GrantTypes, "refresh_token"),
            ResponseCode = Has(application.ResponseTypes, "code")
        };
        issuedSecret = null;
        deleteArmed = false;
    }

    private void NewInteractive()
    {
        Reset("interactive");
        Draft.ApplicationType = "web";
        Draft.ClientType = "public";
        Draft.ConsentType = "implicit";
        Draft.RequirePkce = true;
        Draft.EndpointAuthorization = true;
        Draft.EndpointEndSession = true;
        Draft.EndpointToken = true;
        Draft.GrantAuthorizationCode = true;
        Draft.GrantRefreshToken = true;
        Draft.ResponseCode = true;
        Draft.ScopeValues = "openid profile";
    }

    private void NewMachine()
    {
        Reset("machine");
        Draft.ApplicationType = "web";
        Draft.ClientType = "confidential";
        Draft.ConsentType = "implicit";
        Draft.EndpointToken = true;
        Draft.EndpointRevocation = true;
        Draft.GrantClientCredentials = true;
    }

    private void NewApi()
    {
        Reset("api");
        Draft.ApplicationType = "web";
        Draft.ClientType = "confidential";
        Draft.ConsentType = "implicit";
        Draft.EndpointIntrospection = true;
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
        Draft = NewDraft();
        issuedSecret = null;
        deleteArmed = false;
        error = null;
    }

    private void Save(TSubmitContext<AppDraft> context)
    {
        error = null;
        var endpoints = BuildEndpoints();
        var grants = BuildGrants();
        var responses = Draft.ResponseCode ? new[] { "code" } : [];
        if (Draft.RedirectUris is not string redirectsText || Draft.PostLogoutRedirectUris is not string logoutUrisText)
        {
            error = L("Redirect URI values must be text.", "回调地址必须是文本。");
            return;
        }

        var redirects = SplitValues(redirectsText);
        var logoutUris = SplitValues(logoutUrisText);
        var scopes = SplitValues(Draft.ScopeValues);

        if (selectedId is null)
        {
            ApiClient.CreateApp(new AppCreate(
                Draft.ClientId,
                Draft.DisplayName,
                Draft.ApplicationType,
                Draft.ClientType,
                Draft.ConsentType,
                Draft.RequirePkce,
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
                Draft.DisplayName,
                Draft.ApplicationType,
                Draft.ClientType,
                Draft.ConsentType,
                Draft.RequirePkce,
                redirects,
                logoutUris,
                endpoints,
                grants,
                responses,
                scopes)).Then(ApplyMutation);
        }
    }

    private void ResetDraft(TFormResetEventContext<AppDraft> context)
    {
        var selected = applications.FirstOrDefault(application => application.Id == selectedId);
        if (selected is not null)
        {
            Select(selected);
        }
        else
        {
            switch (profile)
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

        deleteArmed = false;
        error = null;
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
        if (Draft.EndpointAuthorization) values.Add("authorization");
        if (Draft.EndpointEndSession) values.Add("end_session");
        if (Draft.EndpointIntrospection) values.Add("introspection");
        if (Draft.EndpointRevocation) values.Add("revocation");
        if (Draft.EndpointToken) values.Add("token");
        return values.ToArray();
    }

    private string[] BuildGrants()
    {
        var values = new List<string>();
        if (Draft.GrantAuthorizationCode) values.Add("authorization_code");
        if (Draft.GrantClientCredentials) values.Add("client_credentials");
        if (Draft.GrantRefreshToken) values.Add("refresh_token");
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

    private static AppDraft NewDraft() => new();
}
