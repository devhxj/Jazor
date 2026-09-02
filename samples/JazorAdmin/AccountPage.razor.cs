// Loads and manages platform accounts through the typed administration API.
// 通过强类型管理 API 加载并维护平台账户。
using ECMAScript.TDesign;
using JazorAdmin.Features.Accounts;
using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("components/accounts.mjs")]
public partial class AccountPage : AppComponentBase, IVueContainerComponent
{
    private sealed record AccountDraft
    {
        public string Email { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool PlatformAdministrator { get; set; }
    }

    private sealed record PasswordDraft
    {
        public string Value { get; set; } = string.Empty;
    }

    private bool loading = true;
    private string? error;
    private AccountResponse[] accounts = [];
    private string? selectedAccountId;
    private AccountDraft Draft { get; set; } = NewDraft();
    private PasswordDraft Password { get; set; } = NewPasswordDraft();

    private TFormRules<AccountDraft> DraftRules { get; } = new()
    {
        ["email"] =
        [
            new TFormRule { Required = true, Message = "Enter an email address." }
        ],
        ["displayName"] =
        [
            new TFormRule { Required = true, Message = "Enter a display name." }
        ],
        ["password"] =
        [
            new TFormRule { Required = true, Message = "Enter an initial password." }
        ]
    };

    private TFormRules<PasswordDraft> PasswordRules { get; } = new()
    {
        ["password"] =
        [
            new TFormRule { Required = true, Message = "Enter a new password." }
        ]
    };

    // TDesign 表格列：组合单元格走 Cell 渲染片段，行数据经 C# 成员访问保持类型安全。
    private TPrimaryTableCol<AccountResponse>[] Columns =>
    [
        new() { Title = (TPrimaryTableColTitle<AccountResponse>)L("Account", "账户"), Cell = (TPrimaryTableColCell<AccountResponse>)((RenderFragment<TPrimaryTableCellParams<AccountResponse>>)(context => builder =>
            {
        builder.OpenElement(0, "div");
        builder.OpenElement(1, "strong");
        builder.AddContent(2, context.Row.DisplayName);
        builder.CloseElement();
        builder.OpenElement(3, "span");
        builder.AddContent(4, context.Row.Email);
        builder.CloseElement();
        builder.CloseElement();
            })) },
        new() { Title = (TPrimaryTableColTitle<AccountResponse>)L("Platform role", "平台角色"), Cell = (TPrimaryTableColCell<AccountResponse>)((RenderFragment<TPrimaryTableCellParams<AccountResponse>>)(context => builder =>
            {
        builder.AddContent(0, context.Row.PlatformAdministrator
            ? L("Platform administrator", "平台管理员")
            : L("Organization account", "组织账户"));
            })) },
        new() { Title = (TPrimaryTableColTitle<AccountResponse>)L("State", "状态"), Cell = (TPrimaryTableColCell<AccountResponse>)((RenderFragment<TPrimaryTableCellParams<AccountResponse>>)(context => builder =>
            {
        builder.AddContent(0, context.Row.Enabled ? L("Enabled", "已启用") : L("Disabled", "已禁用"));
            })) },
        new() { Title = (TPrimaryTableColTitle<AccountResponse>)L("Actions", "操作"), Cell = (TPrimaryTableColCell<AccountResponse>)((RenderFragment<TPrimaryTableCellParams<AccountResponse>>)(context => builder =>
            {
        builder.OpenComponent<TButton>(0);
        builder.AddComponentParameter(1, nameof(TButton.Variant), TButtonVariantValue.Text);
        builder.AddComponentParameter(2, nameof(TButton.Size), TSizeEnum.Small);
        builder.AddComponentParameter(3, "data-account-command", "select");
        builder.AddComponentParameter(4, nameof(TButton.OnClick),
            EventCallback.Factory.Create(this, () => SelectAccount(context.Row)));
        builder.AddComponentParameter(5, nameof(TContentComponentBase.ChildContent),
            (RenderFragment)(child => child.AddContent(0, L("Manage", "管理"))));
        builder.CloseComponent();

        builder.OpenComponent<TButton>(6);
        builder.AddComponentParameter(7, nameof(TButton.Variant), TButtonVariantValue.Text);
        builder.AddComponentParameter(8, nameof(TButton.Size), TSizeEnum.Small);
        builder.AddComponentParameter(9, "data-account-command", "set-enabled");
        builder.AddComponentParameter(10, nameof(TButton.OnClick),
            EventCallback.Factory.Create(this, () => SetEnabled(context.Row)));
        builder.AddComponentParameter(11, nameof(TContentComponentBase.ChildContent),
            (RenderFragment)(child => child.AddContent(0, context.Row.Enabled ? L("Disable", "禁用") : L("Enable", "启用"))));
        builder.CloseComponent();
            })) }
    ];

    private TTableRowClassNameValue<AccountResponse> SelectedRowClassName
        => (TTableRowClassNameValueOption2<AccountResponse>)SelectedRowClass;

    private TClassName SelectedRowClass(TRowClassNameParams<AccountResponse> parameters)
        => parameters.Row.Id == selectedAccountId ? (TClassName)"ja-table-row-selected" : (TClassName)string.Empty;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            Load();
    }

    private AccountResponse? SelectedAccount
        => accounts.FirstOrDefault(account => account.Id == selectedAccountId);

    private void Load()
    {
        loading = true;
        error = null;
        ApiClient.GetAccounts().Then(ApplyAccounts);
    }

    private void ApplyAccounts(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load accounts.", "无法加载账户。");
            return;
        }

        accounts = ApiClient.ToAccounts(outcome.Data);
    }

    private void CreateAccount(TSubmitContext<AccountDraft> context)
    {
        if (Text.Normalize(Draft.Email) is null ||
            Text.Normalize(Draft.DisplayName) is null ||
            Text.Normalize(Draft.Password) is null)
            return;

        ApiClient.CreateAccount(
            Draft.Email,
            Draft.DisplayName,
            Draft.Password,
            Draft.PlatformAdministrator).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to create the account.", "无法创建账户。");
                return;
            }

            Draft = NewDraft();
            Load();
        });
    }

    private void ResetDraft(TFormResetEventContext<AccountDraft> context)
    {
        Draft = NewDraft();
        error = null;
    }

    private void SelectAccount(AccountResponse account)
    {
        selectedAccountId = account.Id;
        Password = NewPasswordDraft();
    }

    private void SetEnabled(AccountResponse account)
    {
        ApiClient.SetAccountEnabled(account.Id, !account.Enabled).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to update the account state.", "无法更新账户状态。");
                return;
            }

            Load();
        });
    }

    private void ResetPassword(TSubmitContext<PasswordDraft> context)
    {
        if (SelectedAccount is null || Text.Normalize(Password.Value) is null)
            return;

        ApiClient.ResetAccountPassword(SelectedAccount.Id, Password.Value).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to reset the account password.", "无法重置账户密码。");
                return;
            }

            Password = NewPasswordDraft();
        });
    }

    private void ResetPasswordDraft(TFormResetEventContext<PasswordDraft> context)
    {
        Password = NewPasswordDraft();
        error = null;
    }

    private static AccountDraft NewDraft() => new()
    {
        Email = string.Empty,
        DisplayName = string.Empty,
        Password = string.Empty,
        PlatformAdministrator = false
    };

    private static PasswordDraft NewPasswordDraft() => new()
    {
        Value = string.Empty
    };
}
