// Loads and manages platform accounts through the typed administration API.
// 通过强类型管理 API 加载并维护平台账户。
using JazorAdmin.Frontend;
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
        JazorAdminApiClient.GetAccounts().Then(ApplyAccounts);
    }

    private void ApplyAccounts(AdminApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? "Unable to load accounts.";
            return;
        }

        accounts = JazorAdminApiClient.ToAccounts(outcome.Data);
    }

    private void CreateAccount()
    {
        if (Text.Normalize(email) is null || Text.Normalize(displayName) is null || Text.Normalize(password) is null)
            return;

        JazorAdminApiClient.CreateAccount(email, displayName, password, platformAdministrator).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to create the account.";
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
        JazorAdminApiClient.SetAccountEnabled(account.Id, !account.Enabled).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to update the account state.";
                return;
            }

            Load();
        });
    }

    private void ResetPassword()
    {
        if (SelectedAccount is null || Text.Normalize(newPassword) is null)
            return;

        JazorAdminApiClient.ResetAccountPassword(SelectedAccount.Id, newPassword).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to reset the account password.";
                return;
            }

            newPassword = string.Empty;
        });
    }
}
