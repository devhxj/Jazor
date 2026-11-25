using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.OpenAccess;

[ECMAScriptModule("components/open-access-page-state.js")]
public static class OpenAccessPageModule
{
    public static OpenAccessPageState Create() => new();

    public sealed class OpenAccessPageState : global::Jero.Web.App.IPageState<OpenAccessPageProps>
    {
        private readonly OpenAccessMethodOption[] methodOptions =
        [
            new() { value = "0", label = "GET" },
            new() { value = "1", label = "POST" },
            new() { value = "2", label = "PUT" },
            new() { value = "3", label = "DELETE" },
            new() { value = "4", label = "PATCH" },
            new() { value = "5", label = "HEAD" },
            new() { value = "6", label = "OPTIONS" },
            new() { value = "7", label = "TRACE" },
            new() { value = "8", label = "CONNECT" }
        ];

        private OpenAccessView[] items = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string queryAccessKey = string.Empty;
        private bool loading;
        private string? error;

        private OpenAccessTenantOption[] tenantOptions = [];
        private OpenAccessUserOption[] userOptions = [];
        private OpenAccessView? editOriginal;
        private bool editDialogVisible;
        private string editDialogTitle = "新增开放接口身份";
        private string editAccessKey = string.Empty;
        private string editAccessSecret = string.Empty;
        private string editTenantId = string.Empty;
        private string editUserId = string.Empty;
        private bool tenantLoading;
        private bool userLoading;
        private bool secretLoading;
        private int secretRequestVersion;
        private bool dependencyFailed;
        private bool saving;
        private int editorVersion;
        private int userRequestVersion;

        private bool signatureDialogVisible;
        private string signatureAccessKey = string.Empty;
        private string signatureAccessSecret = string.Empty;
        private int signatureMethod;
        private string signatureUrl = string.Empty;
        private string signatureTimestamp = string.Empty;
        private string signatureNonce = string.Empty;
        private string signatureResult = string.Empty;
        private bool signatureGenerating;

        private bool helpDialogVisible;
        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

        private bool CanAdd => ApplicationState.HasPermission("sysOpenAccess:add");
        private bool CanEdit => ApplicationState.HasPermission("sysOpenAccess:update");
        private bool CanDelete => ApplicationState.HasPermission("sysOpenAccess:delete");
        private bool EditorBusy => tenantLoading || userLoading || secretLoading;

        public void Initialize()
            => LoadPage();

        private void LoadPage()
        {
            loading = true;
            OpenAccessClient.Page(new OpenAccessPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                accessKey = queryAccessKey.Trim()
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载开放接口身份失败。";
                    items = [];
                    total = 0;
                    return;
                }

                items = OpenAccessClient.ReadItems(outcome.Data);
                total = OpenAccessClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载开放接口身份失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            queryAccessKey = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void ChangePage(double value)
        {
            currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(double value)
        {
            pageSize = (int)value;
            currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            editorVersion++;
            editOriginal = null;
            editDialogTitle = "新增开放接口身份";
            editAccessKey = string.Empty;
            editAccessSecret = string.Empty;
            editTenantId = string.Empty;
            editUserId = string.Empty;
            userOptions = [];
            dependencyFailed = false;
            editDialogVisible = true;
            error = null;
            LoadTenants(editorVersion, false);
            RequestSecret(editorVersion);
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            editorVersion++;
            secretRequestVersion++;
            secretLoading = false;
            editOriginal = row;
            editDialogTitle = "编辑开放接口身份";
            editAccessKey = row.accessKey;
            editAccessSecret = row.accessSecret;
            editTenantId = row.bindTenantId;
            editUserId = row.bindUserId;
            userOptions = [];
            dependencyFailed = false;
            editDialogVisible = true;
            error = null;
            LoadTenants(editorVersion, true);
        }

        private void LoadTenants(int requestVersion, bool loadBoundUsers)
        {
            if (tenantOptions.Length > 0)
            {
                if (loadBoundUsers)
                    LoadUsers(editTenantId, true);
                return;
            }

            tenantLoading = true;
            OpenAccessClient.Tenants().Then(outcome =>
            {
                if (requestVersion != editorVersion)
                    return;
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    dependencyFailed = true;
                    error = outcome.Error ?? "加载租户列表失败。";
                    tenantOptions = [];
                    return;
                }

                tenantOptions = OpenAccessClient.ReadTenants(outcome.Data);
                if (loadBoundUsers)
                    LoadUsers(editTenantId, true);
            }, _ =>
            {
                if (requestVersion != editorVersion)
                    return;
                tenantLoading = false;
                dependencyFailed = true;
                error = "加载租户列表失败，请检查网络后重试。";
            });
        }

        private void TenantChanged(string? value)
        {
            editTenantId = value ?? string.Empty;
            editUserId = string.Empty;
            dependencyFailed = false;
            LoadUsers(editTenantId, false);
        }

        private void LoadUsers(string tenantId, bool preserveSelection)
        {
            userRequestVersion++;
            var requestVersion = userRequestVersion;
            if (!preserveSelection)
                editUserId = string.Empty;
            if (tenantId.Length == 0)
            {
                userOptions = [];
                userLoading = false;
                return;
            }

            userLoading = true;
            OpenAccessClient.Users(tenantId).Then(outcome =>
            {
                if (requestVersion != userRequestVersion)
                    return;
                userLoading = false;
                if (!outcome.Ok)
                {
                    dependencyFailed = true;
                    error = outcome.Error ?? "加载租户用户失败。";
                    userOptions = [];
                    return;
                }

                userOptions = OpenAccessClient.ReadUsers(outcome.Data);
                if (!ContainsUser(editUserId))
                    editUserId = string.Empty;
            }, _ =>
            {
                if (requestVersion != userRequestVersion)
                    return;
                userLoading = false;
                dependencyFailed = true;
                error = "加载租户用户失败，请检查网络后重试。";
            });
        }

        private void RequestSecret(int requestVersion)
        {
            secretRequestVersion++;
            var secretVersion = secretRequestVersion;
            secretLoading = true;
            OpenAccessClient.CreateSecret().Then(outcome =>
            {
                if (secretVersion != secretRequestVersion)
                    return;
                secretLoading = false;
                if (requestVersion != editorVersion)
                    return;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "生成密钥失败。";
                    return;
                }

                editAccessSecret = OpenAccessClient.ReadText(outcome.Data);
            }, _ =>
            {
                if (secretVersion != secretRequestVersion)
                    return;
                secretLoading = false;
                if (requestVersion != editorVersion)
                    return;
                error = "生成密钥失败，请检查网络后重试。";
            });
        }

        private void RegenerateSecret()
            => RequestSecret(editorVersion);

        private void Save()
        {
            if (saving || EditorBusy)
                return;

            var accessKey = editAccessKey.Trim();
            var accessSecret = editAccessSecret.Trim();
            if (accessKey.Length == 0)
            {
                error = "身份标识不能为空。";
                return;
            }
            if (accessKey.Length > 128)
            {
                error = "身份标识不能超过128个字符。";
                return;
            }
            if (accessSecret.Length == 0)
            {
                error = "密钥不能为空。";
                return;
            }
            if (accessSecret.Length > 256)
            {
                error = "密钥不能超过256个字符。";
                return;
            }
            if (editTenantId.Length == 0)
            {
                error = "请选择绑定租户。";
                return;
            }
            if (editUserId.Length == 0)
            {
                error = "请选择绑定用户。";
                return;
            }

            var input = new OpenAccessDraft
            {
                accessKey = accessKey,
                accessSecret = accessSecret,
                bindTenantId = editTenantId,
                bindUserId = editUserId
            };

            saving = true;
            var request = editOriginal is null
                ? OpenAccessClient.Add(input)
                : OpenAccessClient.Update(editOriginal, input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存开放接口身份失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editOriginal is null ? "开放接口身份已新增" : "开放接口身份已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存开放接口身份失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            OpenAccessClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除开放接口身份失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("开放接口身份已删除");
                LoadPage();
            }, _ => error = "删除开放接口身份失败，请检查网络后重试。");
        }

        private void OpenSignature(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            signatureAccessKey = row.accessKey;
            signatureAccessSecret = row.accessSecret;
            signatureMethod = 0;
            signatureUrl = string.Empty;
            signatureTimestamp = OpenAccessBrowserActions.CurrentTimestampSeconds();
            signatureNonce = OpenAccessBrowserActions.CreateNonce();
            signatureResult = string.Empty;
            signatureDialogVisible = true;
            error = null;
        }

        private void SignatureMethodChanged(string? value)
        {
            if (value == "1") signatureMethod = 1;
            else if (value == "2") signatureMethod = 2;
            else if (value == "3") signatureMethod = 3;
            else if (value == "4") signatureMethod = 4;
            else if (value == "5") signatureMethod = 5;
            else if (value == "6") signatureMethod = 6;
            else if (value == "7") signatureMethod = 7;
            else if (value == "8") signatureMethod = 8;
            else signatureMethod = 0;
        }

        private void GenerateSignature()
        {
            if (signatureGenerating)
                return;

            var url = signatureUrl.Trim();
            var timestamp = signatureTimestamp.Trim();
            var nonce = signatureNonce.Trim();
            if (url.Length == 0 || timestamp.Length == 0 || nonce.Length == 0)
            {
                error = "接口地址、时间戳和随机数不能为空。";
                signatureResult = string.Empty;
                return;
            }

            signatureGenerating = true;
            OpenAccessClient.GenerateSignature(new OpenAccessSignatureInput
            {
                accessKey = signatureAccessKey,
                accessSecret = signatureAccessSecret,
                method = signatureMethod,
                url = url,
                timestamp = timestamp,
                nonce = nonce
            }).Then(outcome =>
            {
                signatureGenerating = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "生成签名失败。";
                    signatureResult = string.Empty;
                    return;
                }

                signatureResult = OpenAccessClient.ReadText(outcome.Data);
                error = null;
            }, _ =>
            {
                signatureGenerating = false;
                signatureResult = string.Empty;
                error = "生成签名失败，请检查网络后重试。";
            });
        }

        private void CopySignature()
        {
            if (signatureResult.Length == 0)
                return;
            OpenAccessBrowserActions.CopyText(signatureResult);
            ElementPlus.ElMessageMembers.Success("签名已复制");
        }

        private void FillTimestamp()
            => signatureTimestamp = OpenAccessBrowserActions.CurrentTimestampSeconds();

        private void FillNonce()
            => signatureNonce = OpenAccessBrowserActions.CreateNonce();

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            auditDialogTitle = "修改记录 · " + row.accessKey;
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = DisplayValue(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = DisplayValue(row.updateTime);
            auditDialogVisible = true;
        }

        private OpenAccessView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private bool ContainsUser(string id)
        {
            if (id.Length == 0)
                return false;
            for (var index = 0; index < userOptions.Length; index++)
                if (userOptions[index].id == id)
                    return true;
            return false;
        }

        private OpenAccessTableRow[] TableRows()
        {
            var result = new OpenAccessTableRow[items.Length];
            for (var index = 0; index < items.Length; index++)
            {
                var row = items[index];
                result[index] = new OpenAccessTableRow(row.id, row.accessKey, row.accessSecret, row.bindUserAccount, row.bindTenantName);
            }
            return result;
        }

        private ConfigurationOption[] TenantChoices()
        {
            var result = new ConfigurationOption[tenantOptions.Length];
            for (var index = 0; index < tenantOptions.Length; index++)
                result[index] = new ConfigurationOption(tenantOptions[index].value, TenantLabel(tenantOptions[index]));
            return result;
        }

        private ConfigurationOption[] UserChoices()
        {
            var result = new ConfigurationOption[userOptions.Length];
            for (var index = 0; index < userOptions.Length; index++)
                result[index] = new ConfigurationOption(userOptions[index].id, UserLabel(userOptions[index]));
            return result;
        }

        private ConfigurationOption[] MethodChoices()
        {
            var result = new ConfigurationOption[methodOptions.Length];
            for (var index = 0; index < methodOptions.Length; index++)
                result[index] = new ConfigurationOption(methodOptions[index].value, methodOptions[index].label);
            return result;
        }

        private static string TenantLabel(OpenAccessTenantOption tenant)
            => tenant.host.Length == 0 ? tenant.label : tenant.label + " (" + tenant.host + ")";

        private static string UserLabel(OpenAccessUserOption user)
            => DisplayValue(user.account) + " (" + DisplayValue(user.realName) + ")";

        private static string DisplayUser(string name, string id)
        {
            if (name.Length > 0 && id.Length > 0)
                return name + " (" + id + ")";
            if (name.Length > 0)
                return name;
            return DisplayValue(id);
        }

        private static string DisplayValue(string? value)
            => string.IsNullOrEmpty(value) ? "-" : value!;

        public OpenAccessPageProps CreateProps() => new()
        {
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            Loading = loading,
            TenantLoading = tenantLoading,
            UserLoading = userLoading,
            SecretLoading = secretLoading,
            EditorBusy = EditorBusy,
            DependencyFailed = dependencyFailed,
            Saving = saving,
            EditDialogVisible = editDialogVisible,
            SignatureDialogVisible = signatureDialogVisible,
            SignatureGenerating = signatureGenerating,
            HelpDialogVisible = helpDialogVisible,
            AuditDialogVisible = auditDialogVisible,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            QueryAccessKey = queryAccessKey,
            EditDialogTitle = editDialogTitle,
            EditAccessKey = editAccessKey,
            EditAccessSecret = editAccessSecret,
            EditTenantId = editTenantId,
            EditUserId = editUserId,
            SignatureMethod = signatureMethod.ToString(),
            SignatureAccessKey = signatureAccessKey,
            SignatureAccessSecret = signatureAccessSecret,
            SignatureUrl = signatureUrl,
            SignatureTimestamp = signatureTimestamp,
            SignatureNonce = signatureNonce,
            SignatureResult = signatureResult,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            Error = error,
            SetEditDialogVisible = value => editDialogVisible = value,
            SetSignatureDialogVisible = value => signatureDialogVisible = value,
            SetHelpDialogVisible = value => helpDialogVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            SetQueryAccessKey = value => queryAccessKey = value ?? "",
            SetEditAccessKey = value => editAccessKey = value ?? "",
            SetEditAccessSecret = value => editAccessSecret = value ?? "",
            SetSignatureUrl = value => signatureUrl = value ?? "",
            SetSignatureTimestamp = value => signatureTimestamp = value ?? "",
            SetSignatureNonce = value => signatureNonce = value ?? "",
            SetEditUserId = value => editUserId = value,
            Rows = TableRows(),
            TenantOptions = TenantChoices(),
            UserOptions = UserChoices(),
            MethodOptions = MethodChoices(),
            Search = Search,
            ResetSearch = ResetSearch,
            LoadPage = LoadPage,
            OpenCreate = OpenCreate,
            Save = Save,
            RegenerateSecret = RegenerateSecret,
            FillTimestamp = FillTimestamp,
            FillNonce = FillNonce,
            CopySignature = CopySignature,
            GenerateSignature = GenerateSignature,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            Delete = Delete,
            OpenSignature = OpenSignature,
            TenantChanged = TenantChanged,
            SignatureMethodChanged = SignatureMethodChanged,
            DismissError = () => error = null,
            CancelEdit = () => editDialogVisible = false,
            OpenHelp = () => helpDialogVisible = true,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
        };
    }
}

public sealed record OpenAccessPageProps : VueProps
{
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool Loading { get; init; }
    public required bool TenantLoading { get; init; }
    public required bool UserLoading { get; init; }
    public required bool SecretLoading { get; init; }
    public required bool EditorBusy { get; init; }
    public required bool DependencyFailed { get; init; }
    public required bool Saving { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool SignatureDialogVisible { get; init; }
    public required bool SignatureGenerating { get; init; }
    public required bool HelpDialogVisible { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required string QueryAccessKey { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditAccessKey { get; init; }
    public required string EditAccessSecret { get; init; }
    public required string EditTenantId { get; init; }
    public required string EditUserId { get; init; }
    public required string SignatureMethod { get; init; }
    public required string SignatureAccessKey { get; init; }
    public required string SignatureAccessSecret { get; init; }
    public required string SignatureUrl { get; init; }
    public required string SignatureTimestamp { get; init; }
    public required string SignatureNonce { get; init; }
    public required string SignatureResult { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required string? Error { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetSignatureDialogVisible { get; init; }
    public required Action<bool> SetHelpDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action<string?> SetQueryAccessKey { get; init; }
    public required Action<string?> SetEditAccessKey { get; init; }
    public required Action<string?> SetEditAccessSecret { get; init; }
    public required Action<string?> SetSignatureUrl { get; init; }
    public required Action<string?> SetSignatureTimestamp { get; init; }
    public required Action<string?> SetSignatureNonce { get; init; }
    public required Action<string> SetEditUserId { get; init; }
    public required OpenAccessTableRow[] Rows { get; init; }
    public required ConfigurationOption[] TenantOptions { get; init; }
    public required ConfigurationOption[] UserOptions { get; init; }
    public required ConfigurationOption[] MethodOptions { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action LoadPage { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action Save { get; init; }
    public required Action RegenerateSecret { get; init; }
    public required Action FillTimestamp { get; init; }
    public required Action FillNonce { get; init; }
    public required Action CopySignature { get; init; }
    public required Action GenerateSignature { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string> OpenSignature { get; init; }
    public required Action<string> TenantChanged { get; init; }
    public required Action<string> SignatureMethodChanged { get; init; }
    public required Action DismissError { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action OpenHelp { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
}


public sealed record OpenAccessMethodOption
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
}
