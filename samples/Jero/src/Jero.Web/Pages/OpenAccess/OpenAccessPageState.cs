using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.OpenAccess;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.OpenAccess;

[ECMAScriptModule("components/open-access-page-state.js")]
public static class OpenAccessPageModule
{
    public static OpenAccessPageState Create() => new();

    public sealed class OpenAccessPageState : global::Jero.Web.Composition.IPageState<OpenAccessPageProps>
    {
        private readonly OpenAccessMethodOption[] _methodOptions =
        [
            new() { Value = "0", Label = "GET" },
            new() { Value = "1", Label = "POST" },
            new() { Value = "2", Label = "PUT" },
            new() { Value = "3", Label = "DELETE" },
            new() { Value = "4", Label = "PATCH" },
            new() { Value = "5", Label = "HEAD" },
            new() { Value = "6", Label = "OPTIONS" },
            new() { Value = "7", Label = "TRACE" },
            new() { Value = "8", Label = "CONNECT" }
        ];

        private OpenAccessView[] _items = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _queryAccessKey = string.Empty;
        private bool _loading;
        private string? _error;

        private OpenAccessTenantOption[] _tenantOptions = [];
        private OpenAccessUserOption[] _userOptions = [];
        private OpenAccessView? _editOriginal;
        private bool _editDialogVisible;
        private string _editDialogTitle = "新增开放接口身份";
        private string _editAccessKey = string.Empty;
        private string _editAccessSecret = string.Empty;
        private string _editTenantId = string.Empty;
        private string _editUserId = string.Empty;
        private bool _tenantLoading;
        private bool _userLoading;
        private bool _secretLoading;
        private int _secretRequestVersion;
        private bool _dependencyFailed;
        private bool _saving;
        private int _editorVersion;
        private int _userRequestVersion;

        private bool _signatureDialogVisible;
        private string _signatureAccessKey = string.Empty;
        private string _signatureAccessSecret = string.Empty;
        private int _signatureMethod;
        private string _signatureUrl = string.Empty;
        private string _signatureTimestamp = string.Empty;
        private string _signatureNonce = string.Empty;
        private string _signatureResult = string.Empty;
        private bool _signatureGenerating;

        private bool _helpDialogVisible;
        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

        private bool CanAdd => ApplicationState.HasPermission("sysOpenAccess:add");
        private bool CanEdit => ApplicationState.HasPermission("sysOpenAccess:update");
        private bool CanDelete => ApplicationState.HasPermission("sysOpenAccess:delete");
        private bool EditorBusy => _tenantLoading || _userLoading || _secretLoading;

        public void Initialize()
            => LoadPage();

        private void LoadPage()
        {
            _loading = true;
            OpenAccessClient.Page(new OpenAccessPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                AccessKey = _queryAccessKey.Trim()
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载开放接口身份失败。";
                    _items = [];
                    _total = 0;
                    return;
                }

                _items = OpenAccessClient.ReadItems(outcome.Data);
                _total = OpenAccessClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载开放接口身份失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _queryAccessKey = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void ChangePage(double value)
        {
            _currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(double value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            _editorVersion++;
            _editOriginal = null;
            _editDialogTitle = "新增开放接口身份";
            _editAccessKey = string.Empty;
            _editAccessSecret = string.Empty;
            _editTenantId = string.Empty;
            _editUserId = string.Empty;
            _userOptions = [];
            _dependencyFailed = false;
            _editDialogVisible = true;
            _error = null;
            LoadTenants(_editorVersion, false);
            RequestSecret(_editorVersion);
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _editorVersion++;
            _secretRequestVersion++;
            _secretLoading = false;
            _editOriginal = row;
            _editDialogTitle = "编辑开放接口身份";
            _editAccessKey = row.AccessKey;
            _editAccessSecret = row.AccessSecret;
            _editTenantId = row.BindTenantId;
            _editUserId = row.BindUserId;
            _userOptions = [];
            _dependencyFailed = false;
            _editDialogVisible = true;
            _error = null;
            LoadTenants(_editorVersion, true);
        }

        private void LoadTenants(int requestVersion, bool loadBoundUsers)
        {
            if (_tenantOptions.Length > 0)
            {
                if (loadBoundUsers)
                    LoadUsers(_editTenantId, true);
                return;
            }

            _tenantLoading = true;
            OpenAccessClient.Tenants().Then(outcome =>
            {
                if (requestVersion != _editorVersion)
                    return;
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _dependencyFailed = true;
                    _error = outcome.Error ?? "加载租户列表失败。";
                    _tenantOptions = [];
                    return;
                }

                _tenantOptions = OpenAccessClient.ReadTenants(outcome.Data);
                if (loadBoundUsers)
                    LoadUsers(_editTenantId, true);
            }, _ =>
            {
                if (requestVersion != _editorVersion)
                    return;
                _tenantLoading = false;
                _dependencyFailed = true;
                _error = "加载租户列表失败，请检查网络后重试。";
            });
        }

        private void TenantChanged(string? value)
        {
            _editTenantId = value ?? string.Empty;
            _editUserId = string.Empty;
            _dependencyFailed = false;
            LoadUsers(_editTenantId, false);
        }

        private void LoadUsers(string tenantId, bool preserveSelection)
        {
            _userRequestVersion++;
            var requestVersion = _userRequestVersion;
            if (!preserveSelection)
                _editUserId = string.Empty;
            if (tenantId.Length == 0)
            {
                _userOptions = [];
                _userLoading = false;
                return;
            }

            _userLoading = true;
            OpenAccessClient.Users(tenantId).Then(outcome =>
            {
                if (requestVersion != _userRequestVersion)
                    return;
                _userLoading = false;
                if (!outcome.Ok)
                {
                    _dependencyFailed = true;
                    _error = outcome.Error ?? "加载租户用户失败。";
                    _userOptions = [];
                    return;
                }

                _userOptions = OpenAccessClient.ReadUsers(outcome.Data);
                if (!ContainsUser(_editUserId))
                    _editUserId = string.Empty;
            }, _ =>
            {
                if (requestVersion != _userRequestVersion)
                    return;
                _userLoading = false;
                _dependencyFailed = true;
                _error = "加载租户用户失败，请检查网络后重试。";
            });
        }

        private void RequestSecret(int requestVersion)
        {
            _secretRequestVersion++;
            var secretVersion = _secretRequestVersion;
            _secretLoading = true;
            OpenAccessClient.CreateSecret().Then(outcome =>
            {
                if (secretVersion != _secretRequestVersion)
                    return;
                _secretLoading = false;
                if (requestVersion != _editorVersion)
                    return;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "生成密钥失败。";
                    return;
                }

                _editAccessSecret = OpenAccessClient.ReadText(outcome.Data);
            }, _ =>
            {
                if (secretVersion != _secretRequestVersion)
                    return;
                _secretLoading = false;
                if (requestVersion != _editorVersion)
                    return;
                _error = "生成密钥失败，请检查网络后重试。";
            });
        }

        private void RegenerateSecret()
            => RequestSecret(_editorVersion);

        private void Save()
        {
            if (_saving || EditorBusy)
                return;

            var accessKey = _editAccessKey.Trim();
            var accessSecret = _editAccessSecret.Trim();
            if (accessKey.Length == 0)
            {
                _error = "身份标识不能为空。";
                return;
            }
            if (accessKey.Length > 128)
            {
                _error = "身份标识不能超过128个字符。";
                return;
            }
            if (accessSecret.Length == 0)
            {
                _error = "密钥不能为空。";
                return;
            }
            if (accessSecret.Length > 256)
            {
                _error = "密钥不能超过256个字符。";
                return;
            }
            if (_editTenantId.Length == 0)
            {
                _error = "请选择绑定租户。";
                return;
            }
            if (_editUserId.Length == 0)
            {
                _error = "请选择绑定用户。";
                return;
            }

            var input = new OpenAccessDraft
            {
                AccessKey = accessKey,
                AccessSecret = accessSecret,
                BindTenantId = _editTenantId,
                BindUserId = _editUserId
            };

            _saving = true;
            var request = _editOriginal is null
                ? OpenAccessClient.Add(input)
                : OpenAccessClient.Update(_editOriginal, input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存开放接口身份失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editOriginal is null ? "开放接口身份已新增" : "开放接口身份已更新");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存开放接口身份失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            OpenAccessClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除开放接口身份失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("开放接口身份已删除");
                LoadPage();
            }, _ => _error = "删除开放接口身份失败，请检查网络后重试。");
        }

        private void OpenSignature(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            _signatureAccessKey = row.AccessKey;
            _signatureAccessSecret = row.AccessSecret;
            _signatureMethod = 0;
            _signatureUrl = string.Empty;
            _signatureTimestamp = OpenAccessBrowserActions.CurrentTimestampSeconds();
            _signatureNonce = OpenAccessBrowserActions.CreateNonce();
            _signatureResult = string.Empty;
            _signatureDialogVisible = true;
            _error = null;
        }

        private void SignatureMethodChanged(string? value)
        {
            if (value == "1") _signatureMethod = 1;
            else if (value == "2") _signatureMethod = 2;
            else if (value == "3") _signatureMethod = 3;
            else if (value == "4") _signatureMethod = 4;
            else if (value == "5") _signatureMethod = 5;
            else if (value == "6") _signatureMethod = 6;
            else if (value == "7") _signatureMethod = 7;
            else if (value == "8") _signatureMethod = 8;
            else _signatureMethod = 0;
        }

        private void GenerateSignature()
        {
            if (_signatureGenerating)
                return;

            var url = _signatureUrl.Trim();
            var timestamp = _signatureTimestamp.Trim();
            var nonce = _signatureNonce.Trim();
            if (url.Length == 0 || timestamp.Length == 0 || nonce.Length == 0)
            {
                _error = "接口地址、时间戳和随机数不能为空。";
                _signatureResult = string.Empty;
                return;
            }

            _signatureGenerating = true;
            OpenAccessClient.GenerateSignature(new OpenAccessSignatureInput
            {
                AccessKey = _signatureAccessKey,
                AccessSecret = _signatureAccessSecret,
                Method = _signatureMethod,
                Url = url,
                Timestamp = timestamp,
                Nonce = nonce
            }).Then(outcome =>
            {
                _signatureGenerating = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "生成签名失败。";
                    _signatureResult = string.Empty;
                    return;
                }

                _signatureResult = OpenAccessClient.ReadText(outcome.Data);
                _error = null;
            }, _ =>
            {
                _signatureGenerating = false;
                _signatureResult = string.Empty;
                _error = "生成签名失败，请检查网络后重试。";
            });
        }

        private void CopySignature()
        {
            if (_signatureResult.Length == 0)
                return;
            OpenAccessBrowserActions.CopyText(_signatureResult);
            ElementPlus.ElMessageMembers.Success("签名已复制");
        }

        private void FillTimestamp()
            => _signatureTimestamp = OpenAccessBrowserActions.CurrentTimestampSeconds();

        private void FillNonce()
            => _signatureNonce = OpenAccessBrowserActions.CreateNonce();

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            _auditDialogTitle = "修改记录 · " + row.AccessKey;
            _auditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            _auditCreateTime = DisplayValue(row.CreateTime);
            _auditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            _auditUpdateTime = DisplayValue(row.UpdateTime);
            _auditDialogVisible = true;
        }

        private OpenAccessView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private bool ContainsUser(string id)
        {
            if (id.Length == 0)
                return false;
            for (var index = 0; index < _userOptions.Length; index++)
                if (_userOptions[index].Id == id)
                    return true;
            return false;
        }

        private OpenAccessTableRow[] TableRows()
        {
            var result = new OpenAccessTableRow[_items.Length];
            for (var index = 0; index < _items.Length; index++)
            {
                var row = _items[index];
                result[index] = new OpenAccessTableRow(row.Id, row.AccessKey, row.AccessSecret, row.BindUserAccount, row.BindTenantName);
            }
            return result;
        }

        private ConfigurationOption[] TenantChoices()
        {
            var result = new ConfigurationOption[_tenantOptions.Length];
            for (var index = 0; index < _tenantOptions.Length; index++)
                result[index] = new ConfigurationOption(_tenantOptions[index].Value, TenantLabel(_tenantOptions[index]));
            return result;
        }

        private ConfigurationOption[] UserChoices()
        {
            var result = new ConfigurationOption[_userOptions.Length];
            for (var index = 0; index < _userOptions.Length; index++)
                result[index] = new ConfigurationOption(_userOptions[index].Id, UserLabel(_userOptions[index]));
            return result;
        }

        private ConfigurationOption[] MethodChoices()
        {
            var result = new ConfigurationOption[_methodOptions.Length];
            for (var index = 0; index < _methodOptions.Length; index++)
                result[index] = new ConfigurationOption(_methodOptions[index].Value, _methodOptions[index].Label);
            return result;
        }

        private static string TenantLabel(OpenAccessTenantOption tenant)
            => tenant.Host.Length == 0 ? tenant.Label : tenant.Label + " (" + tenant.Host + ")";

        private static string UserLabel(OpenAccessUserOption user)
            => DisplayValue(user.Account) + " (" + DisplayValue(user.RealName) + ")";

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
            Loading = _loading,
            TenantLoading = _tenantLoading,
            UserLoading = _userLoading,
            SecretLoading = _secretLoading,
            EditorBusy = EditorBusy,
            DependencyFailed = _dependencyFailed,
            Saving = _saving,
            EditDialogVisible = _editDialogVisible,
            SignatureDialogVisible = _signatureDialogVisible,
            SignatureGenerating = _signatureGenerating,
            HelpDialogVisible = _helpDialogVisible,
            AuditDialogVisible = _auditDialogVisible,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            QueryAccessKey = _queryAccessKey,
            EditDialogTitle = _editDialogTitle,
            EditAccessKey = _editAccessKey,
            EditAccessSecret = _editAccessSecret,
            EditTenantId = _editTenantId,
            EditUserId = _editUserId,
            SignatureMethod = _signatureMethod.ToString(),
            SignatureAccessKey = _signatureAccessKey,
            SignatureAccessSecret = _signatureAccessSecret,
            SignatureUrl = _signatureUrl,
            SignatureTimestamp = _signatureTimestamp,
            SignatureNonce = _signatureNonce,
            SignatureResult = _signatureResult,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            Error = _error,
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetSignatureDialogVisible = value => _signatureDialogVisible = value,
            SetHelpDialogVisible = value => _helpDialogVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            SetQueryAccessKey = value => _queryAccessKey = value ?? "",
            SetEditAccessKey = value => _editAccessKey = value ?? "",
            SetEditAccessSecret = value => _editAccessSecret = value ?? "",
            SetSignatureUrl = value => _signatureUrl = value ?? "",
            SetSignatureTimestamp = value => _signatureTimestamp = value ?? "",
            SetSignatureNonce = value => _signatureNonce = value ?? "",
            SetEditUserId = value => _editUserId = value,
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
            DismissError = () => _error = null,
            CancelEdit = () => _editDialogVisible = false,
            OpenHelp = () => _helpDialogVisible = true,
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
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}
