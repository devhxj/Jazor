using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Ldap;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Infrastructure.Http;

namespace Jero.Web.Pages.Ldap;

[ECMAScriptModule("components/ldap-page-state.js")]
public static class LdapPageModule
{
    public static LdapPageState Create() => new();

    public sealed class LdapPageState : global::Jero.Web.Composition.IPageState<LdapPageProps>
    {
        private LdapView[] _items = [];
        private LdapView[] _rows = [];
        private LdapTenantOption[] _tenantItems = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _tenantId = string.Empty;
        private string _keyword = string.Empty;
        private string _host = string.Empty;
        private bool _loading;
        private bool _tenantLoading;
        private bool _detailLoading;
        private bool _actionLoading;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增 LDAP 配置";
        private bool _saving;
        private string _editId = string.Empty;
        private string _editTenantId = string.Empty;
        private string _editHost = string.Empty;
        private int _editPort = 389;
        private string _editBaseDn = string.Empty;
        private string _editBindDn = string.Empty;
        private string _editBindPass = string.Empty;
        private string _preservedBindPass = string.Empty;
        private string _editAuthFilter = "sAMAccountName=%s";
        private int _editVersion = 3;
        private string _editBindAttrAccount = "sAMAccountName";
        private string _editBindAttrEmployeeId = "EmployeeId";
        private string _editBindAttrCode = "objectGUID";
        private int _editStatus = 1;
        private string _editRemark = string.Empty;
        private string _editCreateTime = string.Empty;
        private string _editCreateUserId = string.Empty;
        private string _editCreateUserName = string.Empty;
        private string _editUpdateTime = string.Empty;
        private string _editUpdateUserId = string.Empty;
        private string _editUpdateUserName = string.Empty;
        private bool _editIsDelete;
        private string _editDeleteTime = string.Empty;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysLdap:add");
        private bool CanEdit => ApplicationState.HasPermission("sysLdap:update");
        private bool CanDelete => ApplicationState.HasPermission("sysLdap:delete");
        private bool CanSyncUser => ApplicationState.HasPermission("sysLdap:syncUser");
        private bool CanSyncOrg => ApplicationState.HasPermission("sysLdap:syncOrg");
        private bool HasActionColumn => CanEdit || CanDelete || CanSyncUser || CanSyncOrg;
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            _tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadPage();
        }

        private void LoadTenants()
        {
            _tenantLoading = true;
            LdapClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }

                _tenantItems = LdapClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            _loading = true;
            LdapClient.Page(new LdapPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Keyword = _keyword.Trim(),
                Host = _host.Trim(),
                TenantId = IsSuperAdmin ? _tenantId : string.Empty
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载 LDAP 配置失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }

                _items = LdapClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = LdapClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载 LDAP 配置失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _tenantId = DefaultTenantId;
            _keyword = string.Empty;
            _host = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void Refresh() => LoadPage();

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void ChangePage(Number value)
        {
            _currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(Number value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? _tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增 LDAP 配置");
                return;
            }

            _editDialogTitle = "新增 LDAP 配置";
            _editId = string.Empty;
            _editTenantId = targetTenantId;
            _editHost = string.Empty;
            _editPort = 389;
            _editBaseDn = string.Empty;
            _editBindDn = string.Empty;
            _editBindPass = string.Empty;
            _preservedBindPass = string.Empty;
            _editAuthFilter = "sAMAccountName=%s";
            _editVersion = 3;
            _editBindAttrAccount = "sAMAccountName";
            _editBindAttrEmployeeId = "EmployeeId";
            _editBindAttrCode = "objectGUID";
            _editStatus = 1;
            _editRemark = string.Empty;
            _editCreateTime = string.Empty;
            _editCreateUserId = string.Empty;
            _editCreateUserName = string.Empty;
            _editUpdateTime = string.Empty;
            _editUpdateUserId = string.Empty;
            _editUpdateUserName = string.Empty;
            _editIsDelete = false;
            _editDeleteTime = string.Empty;
            _error = null;
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _detailLoading = true;
            LdapClient.Detail(row.Id).Then(outcome =>
            {
                _detailLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载 LDAP 配置详情失败。";
                    return;
                }

                var detail = LdapClient.ReadDetail(outcome.Data);
                _editDialogTitle = "编辑 LDAP 配置";
                PopulateForm(detail);
                _editDialogVisible = true;
            }, _ =>
            {
                _detailLoading = false;
                _error = "加载 LDAP 配置详情失败，请检查网络后重试。";
            });
        }

        private void PopulateForm(LdapView row)
        {
            _editId = row.Id;
            _editTenantId = row.TenantId.Length == 0 ? DefaultTenantId : row.TenantId;
            _editHost = row.Host;
            _editPort = row.Port > 0 ? row.Port : 389;
            _editBaseDn = row.BaseDn;
            _editBindDn = row.BindDn;
            // 密码输入框保持为空，已有密文仅用于提交更新。
            _editBindPass = string.Empty;
            _preservedBindPass = row.BindPass;
            _editAuthFilter = row.AuthFilter.Length == 0 ? "sAMAccountName=%s" : row.AuthFilter;
            _editVersion = row.Version > 0 ? row.Version : 3;
            _editBindAttrAccount = row.BindAttrAccount.Length == 0 ? "sAMAccountName" : row.BindAttrAccount;
            _editBindAttrEmployeeId = row.BindAttrEmployeeId.Length == 0 ? "EmployeeId" : row.BindAttrEmployeeId;
            _editBindAttrCode = row.BindAttrCode.Length == 0 ? "objectGUID" : row.BindAttrCode;
            _editStatus = row.Status == 2 ? 2 : 1;
            _editRemark = row.Remark;
            _editCreateTime = row.CreateTime;
            _editCreateUserId = row.CreateUserId;
            _editCreateUserName = row.CreateUserName;
            _editUpdateTime = row.UpdateTime;
            _editUpdateUserId = row.UpdateUserId;
            _editUpdateUserName = row.UpdateUserName;
            _editIsDelete = row.IsDelete;
            _editDeleteTime = row.DeleteTime;
        }

        private void Save()
        {
            if (_saving)
                return;

            var normalizedHost = _editHost.Trim();
            var normalizedBaseDn = _editBaseDn.Trim();
            var normalizedBindDn = _editBindDn.Trim();
            var normalizedBindPass = _editBindPass.Trim();
            var normalizedAuthFilter = _editAuthFilter.Trim();
            var normalizedAccountAttr = _editBindAttrAccount.Trim();
            var normalizedEmployeeAttr = _editBindAttrEmployeeId.Trim();
            var normalizedCodeAttr = _editBindAttrCode.Trim();
            var normalizedRemark = _editRemark.Trim();

            if (normalizedHost.Length == 0 || normalizedHost.Length > 128)
            {
                _error = "主机不能为空且不能超过128个字符。";
                return;
            }
            if (_editPort < 1 || _editPort > 65535)
            {
                _error = "端口必须在1到65535之间。";
                return;
            }
            if (normalizedBaseDn.Length == 0 || normalizedBaseDn.Length > 128)
            {
                _error = "用户搜索基准不能为空且不能超过128个字符。";
                return;
            }
            if (normalizedBindDn.Length == 0 || normalizedBindDn.Length > 128)
            {
                _error = "绑定 DN 不能为空且不能超过128个字符。";
                return;
            }
            if (normalizedBindPass.Length == 0 && _editId.Length == 0)
            {
                _error = "新增 LDAP 配置时绑定密码不能为空。";
                return;
            }
            if (normalizedBindPass.Length > 512)
            {
                _error = "绑定密码不能超过512个字符。";
                return;
            }
            if (normalizedAuthFilter.Length == 0 || normalizedAuthFilter.Length > 128 ||
                normalizedAccountAttr.Length == 0 || normalizedAccountAttr.Length > 32 ||
                normalizedEmployeeAttr.Length == 0 || normalizedEmployeeAttr.Length > 32 ||
                normalizedCodeAttr.Length == 0 || normalizedCodeAttr.Length > 64)
            {
                _error = "LDAP 属性字段不能为空或超出长度限制。";
                return;
            }
            if (_editVersion < 1 || _editVersion > 10)
            {
                _error = "LDAP 版本必须在1到10之间。";
                return;
            }
            if (IsSuperAdmin && _editTenantId.Length == 0)
            {
                _error = "超级管理员维护 LDAP 配置时必须选择租户。";
                return;
            }

            var input = new LdapSaveInput
            {
                Id = _editId,
                TenantId = IsSuperAdmin ? _editTenantId : DefaultTenantId,
                Host = normalizedHost,
                Port = _editPort,
                BaseDn = normalizedBaseDn,
                BindDn = normalizedBindDn,
                BindPass = normalizedBindPass.Length > 0 ? normalizedBindPass : _preservedBindPass,
                AuthFilter = normalizedAuthFilter,
                Version = _editVersion,
                BindAttrAccount = normalizedAccountAttr,
                BindAttrEmployeeId = normalizedEmployeeAttr,
                BindAttrCode = normalizedCodeAttr,
                Status = _editStatus == 2 ? 2 : 1,
                Remark = normalizedRemark,
                CreateTime = _editCreateTime,
                CreateUserId = Optional(_editCreateUserId),
                CreateUserName = _editCreateUserName,
                UpdateTime = Optional(_editUpdateTime),
                UpdateUserId = Optional(_editUpdateUserId),
                UpdateUserName = _editUpdateUserName,
                IsDelete = _editIsDelete,
                DeleteTime = Optional(_editDeleteTime)
            };

            _saving = true;
            var request = _editId.Length == 0 ? LdapClient.Add(input) : LdapClient.Update(input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存 LDAP 配置失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "LDAP 配置已新增" : "LDAP 配置已更新");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存 LDAP 配置失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || _actionLoading)
                return;

            _actionLoading = true;
            LdapClient.Delete(row.Id).Then(outcome =>
            {
                _actionLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除 LDAP 配置失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("LDAP 配置已删除");
                LoadPage();
            }, _ =>
            {
                _actionLoading = false;
                _error = "删除 LDAP 配置失败，请检查网络后重试。";
            });
        }

        private void SyncUser(string id)
            => Sync(id, true);

        private void SyncOrg(string id)
            => Sync(id, false);

        private void Sync(string id, bool users)
        {
            var row = Find(id);
            if (row is null || _actionLoading)
                return;

            _actionLoading = true;
            var request = users ? LdapClient.SyncUser(row.Id) : LdapClient.SyncOrg(row.Id);
            request.Then(outcome =>
            {
                _actionLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? (users ? "同步域账户失败。" : "同步域组织失败。");
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success(users ? "域账户同步已完成" : "域组织架构同步已完成");
                LoadPage();
            }, _ =>
            {
                _actionLoading = false;
                _error = users ? "同步域账户失败，请检查网络后重试。" : "同步域组织失败，请检查网络后重试。";
            });
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            _auditDialogTitle = "修改记录 · " + row.Host;
            _auditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            _auditCreateTime = DisplayValue(row.CreateTime);
            _auditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            _auditUpdateTime = DisplayValue(row.UpdateTime);
            _auditDialogVisible = true;
        }

        private LdapView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static string TenantLabel(LdapTenantOption tenant)
            => tenant.Host.Length == 0 ? tenant.Label : tenant.Label + " (" + tenant.Host + ")";

        private static string DisplayUser(string name, string id)
        {
            if (name.Length > 0 && id.Length > 0)
                return name + " (" + id + ")";
            if (name.Length > 0)
                return name;
            return DisplayValue(id);
        }

        private static string DisplayValue(string value)
            => value.Length == 0 ? "-" : value;

        private static string? Optional(string value)
            => value.Length == 0 ? null : value;

        private ConfigurationOption[] TenantOptions()
        {
            var options = new ConfigurationOption[_tenantItems.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(_tenantItems[index].Value, TenantLabel(_tenantItems[index]));
            return options;
        }

        private readonly double[] _joltPageSizes = [10, 20, 50, 100, 200, 500];

        private LdapTableRow[] TableRows()
        {
            var result = new LdapTableRow[_rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _rows[index];
                result[index] = new LdapTableRow(row.Id, row.TenantId, row.Host, row.Port, row.BaseDn, row.BindDn,
                    row.AuthFilter, row.Version, row.Status, row.BindPass.Length > 0);
            }
            return result;
        }

        public LdapPageProps CreateProps() => new()
        {
            TenantOptions = TenantOptions(),
            TenantChanged = TenantChanged,
            TenantId = _tenantId,
            SetEditTenantId = value => _editTenantId = value,
            EditTenantId = _editTenantId,
            IsSuperAdmin = IsSuperAdmin,
            HasActionColumn = HasActionColumn,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            CanSyncUser = CanSyncUser,
            CanSyncOrg = CanSyncOrg,
            DetailLoading = _detailLoading,
            ActionLoading = _actionLoading,
            OpenAuditRow = OpenAudit,
            OpenEditRow = OpenEdit,
            DeleteRow = Delete,
            SyncUserRow = SyncUser,
            SyncOrgRow = SyncOrg,
            Rows = TableRows(),
            EditStatus = _editStatus,
            SetEditStatus = value => _editStatus = (int)value,
            EditPort = _editPort,
            SetEditPort = value => _editPort = (int)(value ?? 389),
            EditVersion = _editVersion,
            SetEditVersion = value => _editVersion = (int)(value ?? 3),
            SetKeyword = value => _keyword = value ?? "",
            SetHost = value => _host = value ?? "",
            SetEditHost = value => _editHost = value ?? "",
            SetEditBaseDn = value => _editBaseDn = value ?? "",
            SetEditAuthFilter = value => _editAuthFilter = value ?? "",
            SetEditBindDn = value => _editBindDn = value ?? "",
            SetEditBindPass = value => _editBindPass = value ?? "",
            SetEditBindAttrAccount = value => _editBindAttrAccount = value ?? "",
            SetEditBindAttrEmployeeId = value => _editBindAttrEmployeeId = value ?? "",
            SetEditBindAttrCode = value => _editBindAttrCode = value ?? "",
            SetEditRemark = value => _editRemark = value ?? "",
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            CancelEdit = () => _editDialogVisible = false,
            DismissError = () => _error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            Refresh = Refresh,
            Save = Save,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            Error = _error,
            TenantLoading = _tenantLoading,
            Keyword = _keyword,
            Host = _host,
            Loading = _loading,
            CanAdd = CanAdd,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            EditDialogVisible = _editDialogVisible,
            EditDialogTitle = _editDialogTitle,
            Editing = _editId.Length > 0,
            EditHost = _editHost,
            EditBaseDn = _editBaseDn,
            EditAuthFilter = _editAuthFilter,
            EditBindDn = _editBindDn,
            EditBindPass = _editBindPass,
            EditBindAttrAccount = _editBindAttrAccount,
            EditBindAttrEmployeeId = _editBindAttrEmployeeId,
            EditBindAttrCode = _editBindAttrCode,
            EditRemark = _editRemark,
            Saving = _saving,
            AuditDialogVisible = _auditDialogVisible,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            PageSizes = _joltPageSizes,
        };
    }
}

public sealed record LdapPageProps : VueProps
{
    public required ConfigurationOption[] TenantOptions { get; init; }
    public required Action<string> TenantChanged { get; init; }
    public required string TenantId { get; init; }
    public required Action<string> SetEditTenantId { get; init; }
    public required string EditTenantId { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required bool HasActionColumn { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool CanSyncUser { get; init; }
    public required bool CanSyncOrg { get; init; }
    public required bool DetailLoading { get; init; }
    public required bool ActionLoading { get; init; }
    public required Action<string> OpenAuditRow { get; init; }
    public required Action<string> OpenEditRow { get; init; }
    public required Action<string> DeleteRow { get; init; }
    public required Action<string> SyncUserRow { get; init; }
    public required Action<string> SyncOrgRow { get; init; }
    public required LdapTableRow[] Rows { get; init; }
    public required int EditStatus { get; init; }
    public required Action<double> SetEditStatus { get; init; }
    public required int EditPort { get; init; }
    public required Action<double?> SetEditPort { get; init; }
    public required int EditVersion { get; init; }
    public required Action<double?> SetEditVersion { get; init; }
    public required Action<string?> SetKeyword { get; init; }
    public required Action<string?> SetHost { get; init; }
    public required Action<string?> SetEditHost { get; init; }
    public required Action<string?> SetEditBaseDn { get; init; }
    public required Action<string?> SetEditAuthFilter { get; init; }
    public required Action<string?> SetEditBindDn { get; init; }
    public required Action<string?> SetEditBindPass { get; init; }
    public required Action<string?> SetEditBindAttrAccount { get; init; }
    public required Action<string?> SetEditBindAttrEmployeeId { get; init; }
    public required Action<string?> SetEditBindAttrCode { get; init; }
    public required Action<string?> SetEditRemark { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action DismissError { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action Refresh { get; init; }
    public required Action Save { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required string? Error { get; init; }
    public required bool TenantLoading { get; init; }
    public required string Keyword { get; init; }
    public required string Host { get; init; }
    public required bool Loading { get; init; }
    public required bool CanAdd { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required string EditDialogTitle { get; init; }
    public required bool Editing { get; init; }
    public required string EditHost { get; init; }
    public required string EditBaseDn { get; init; }
    public required string EditAuthFilter { get; init; }
    public required string EditBindDn { get; init; }
    public required string EditBindPass { get; init; }
    public required string EditBindAttrAccount { get; init; }
    public required string EditBindAttrEmployeeId { get; init; }
    public required string EditBindAttrCode { get; init; }
    public required string EditRemark { get; init; }
    public required bool Saving { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required double[] PageSizes { get; init; }
}
