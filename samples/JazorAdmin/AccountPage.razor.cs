// Loads and manages platform accounts through the typed administration API.
// 通过强类型管理 API 加载并维护平台账户。
using JazorAdmin.Features.Accounts;

namespace JazorAdmin;

[ECMAScriptModule("components/accounts.mjs")]
public partial class AccountPage : AppComponentBase, IVueContainerComponent
{
    private bool loading = true;
    private string? error;
    private AccountResponse[] accounts = [];
    private string email = string.Empty;
    private string displayName = string.Empty;
    private string password = string.Empty;
    private bool platformAdministrator;
    private string? selectedAccountId;
    private string newPassword = string.Empty;

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

    private void CreateAccount()
    {
        if (Text.Normalize(email) is null || Text.Normalize(displayName) is null || Text.Normalize(password) is null)
            return;

        ApiClient.CreateAccount(email, displayName, password, platformAdministrator).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to create the account.", "无法创建账户。");
                return;
            }

            email = string.Empty;
            displayName = string.Empty;
            password = string.Empty;
            platformAdministrator = false;
            Load();
        });
    }

    private void SelectAccount(AccountResponse account)
    {
        selectedAccountId = account.Id;
        newPassword = string.Empty;
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

    private void ResetPassword()
    {
        if (SelectedAccount is null || Text.Normalize(newPassword) is null)
            return;

        ApiClient.ResetAccountPassword(SelectedAccount.Id, newPassword).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to reset the account password.", "无法重置账户密码。");
                return;
            }

            newPassword = string.Empty;
        });
    }
}
