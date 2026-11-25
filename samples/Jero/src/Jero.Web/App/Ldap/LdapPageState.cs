using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Http;

namespace Jero.Web.App.Ldap;

[ECMAScriptModule("components/ldap-page-state.js")]
public static class LdapPageModule
{
    public static LdapPageState Create() => new();

    public sealed class LdapPageState : global::Jero.Web.App.IPageState<LdapPageProps>
    {
        private LdapView[] items = [];
        private LdapView[] rows = [];
        private LdapTenantOption[] tenantItems = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string tenantId = string.Empty;
        private string keyword = string.Empty;
        private string host = string.Empty;
        private bool loading;
        private bool tenantLoading;
        private bool detailLoading;
        private bool actionLoading;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增 LDAP 配置";
        private bool saving;
        private string editId = string.Empty;
        private string editTenantId = string.Empty;
        private string editHost = string.Empty;
        private int editPort = 389;
        private string editBaseDn = string.Empty;
        private string editBindDn = string.Empty;
        private string editBindPass = string.Empty;
        private string preservedBindPass = string.Empty;
        private string editAuthFilter = "sAMAccountName=%s";
        private int editVersion = 3;
        private string editBindAttrAccount = "sAMAccountName";
        private string editBindAttrEmployeeId = "EmployeeId";
        private string editBindAttrCode = "objectGUID";
        private int editStatus = 1;
        private string editRemark = string.Empty;
        private string editCreateTime = string.Empty;
        private string editCreateUserId = string.Empty;
        private string editCreateUserName = string.Empty;
        private string editUpdateTime = string.Empty;
        private string editUpdateUserId = string.Empty;
        private string editUpdateUserName = string.Empty;
        private bool editIsDelete;
        private string editDeleteTime = string.Empty;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

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
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadPage();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            LdapClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }

                tenantItems = LdapClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            loading = true;
            LdapClient.Page(new LdapPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                keyword = keyword.Trim(),
                host = host.Trim(),
                tenantId = IsSuperAdmin ? tenantId : string.Empty
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载 LDAP 配置失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }

                items = LdapClient.ReadItems(outcome.Data);
                rows = items;
                total = LdapClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载 LDAP 配置失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            tenantId = DefaultTenantId;
            keyword = string.Empty;
            host = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void Refresh() => LoadPage();

        private void TenantChanged(string? value)
        {
            tenantId = value ?? string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void ChangePage(Number value)
        {
            currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(Number value)
        {
            pageSize = (int)value;
            currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增 LDAP 配置");
                return;
            }

            editDialogTitle = "新增 LDAP 配置";
            editId = string.Empty;
            editTenantId = targetTenantId;
            editHost = string.Empty;
            editPort = 389;
            editBaseDn = string.Empty;
            editBindDn = string.Empty;
            editBindPass = string.Empty;
            preservedBindPass = string.Empty;
            editAuthFilter = "sAMAccountName=%s";
            editVersion = 3;
            editBindAttrAccount = "sAMAccountName";
            editBindAttrEmployeeId = "EmployeeId";
            editBindAttrCode = "objectGUID";
            editStatus = 1;
            editRemark = string.Empty;
            editCreateTime = string.Empty;
            editCreateUserId = string.Empty;
            editCreateUserName = string.Empty;
            editUpdateTime = string.Empty;
            editUpdateUserId = string.Empty;
            editUpdateUserName = string.Empty;
            editIsDelete = false;
            editDeleteTime = string.Empty;
            error = null;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            detailLoading = true;
            LdapClient.Detail(row.id).Then(outcome =>
            {
                detailLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载 LDAP 配置详情失败。";
                    return;
                }

                var detail = LdapClient.ReadDetail(outcome.Data);
                editDialogTitle = "编辑 LDAP 配置";
                PopulateForm(detail);
                editDialogVisible = true;
            }, _ =>
            {
                detailLoading = false;
                error = "加载 LDAP 配置详情失败，请检查网络后重试。";
            });
        }

        private void PopulateForm(LdapView row)
        {
            editId = row.id;
            editTenantId = row.tenantId.Length == 0 ? DefaultTenantId : row.tenantId;
            editHost = row.host;
            editPort = row.port > 0 ? row.port : 389;
            editBaseDn = row.baseDn;
            editBindDn = row.bindDn;
            // The password box remains blank; the encrypted value is retained only for update.
            editBindPass = string.Empty;
            preservedBindPass = row.bindPass;
            editAuthFilter = row.authFilter.Length == 0 ? "sAMAccountName=%s" : row.authFilter;
            editVersion = row.version > 0 ? row.version : 3;
            editBindAttrAccount = row.bindAttrAccount.Length == 0 ? "sAMAccountName" : row.bindAttrAccount;
            editBindAttrEmployeeId = row.bindAttrEmployeeId.Length == 0 ? "EmployeeId" : row.bindAttrEmployeeId;
            editBindAttrCode = row.bindAttrCode.Length == 0 ? "objectGUID" : row.bindAttrCode;
            editStatus = row.status == 2 ? 2 : 1;
            editRemark = row.remark;
            editCreateTime = row.createTime;
            editCreateUserId = row.createUserId;
            editCreateUserName = row.createUserName;
            editUpdateTime = row.updateTime;
            editUpdateUserId = row.updateUserId;
            editUpdateUserName = row.updateUserName;
            editIsDelete = row.isDelete;
            editDeleteTime = row.deleteTime;
        }

        private void Save()
        {
            if (saving)
                return;

            var normalizedHost = editHost.Trim();
            var normalizedBaseDn = editBaseDn.Trim();
            var normalizedBindDn = editBindDn.Trim();
            var normalizedBindPass = editBindPass.Trim();
            var normalizedAuthFilter = editAuthFilter.Trim();
            var normalizedAccountAttr = editBindAttrAccount.Trim();
            var normalizedEmployeeAttr = editBindAttrEmployeeId.Trim();
            var normalizedCodeAttr = editBindAttrCode.Trim();
            var normalizedRemark = editRemark.Trim();

            if (normalizedHost.Length == 0 || normalizedHost.Length > 128)
            {
                error = "主机不能为空且不能超过128个字符。";
                return;
            }
            if (editPort < 1 || editPort > 65535)
            {
                error = "端口必须在1到65535之间。";
                return;
            }
            if (normalizedBaseDn.Length == 0 || normalizedBaseDn.Length > 128)
            {
                error = "用户搜索基准不能为空且不能超过128个字符。";
                return;
            }
            if (normalizedBindDn.Length == 0 || normalizedBindDn.Length > 128)
            {
                error = "绑定 DN 不能为空且不能超过128个字符。";
                return;
            }
            if (normalizedBindPass.Length == 0 && editId.Length == 0)
            {
                error = "新增 LDAP 配置时绑定密码不能为空。";
                return;
            }
            if (normalizedBindPass.Length > 512)
            {
                error = "绑定密码不能超过512个字符。";
                return;
            }
            if (normalizedAuthFilter.Length == 0 || normalizedAuthFilter.Length > 128 ||
                normalizedAccountAttr.Length == 0 || normalizedAccountAttr.Length > 32 ||
                normalizedEmployeeAttr.Length == 0 || normalizedEmployeeAttr.Length > 32 ||
                normalizedCodeAttr.Length == 0 || normalizedCodeAttr.Length > 64)
            {
                error = "LDAP 属性字段不能为空或超出长度限制。";
                return;
            }
            if (editVersion < 1 || editVersion > 10)
            {
                error = "LDAP 版本必须在1到10之间。";
                return;
            }
            if (IsSuperAdmin && editTenantId.Length == 0)
            {
                error = "超级管理员维护 LDAP 配置时必须选择租户。";
                return;
            }

            var input = new LdapSaveInput
            {
                id = editId,
                tenantId = IsSuperAdmin ? editTenantId : DefaultTenantId,
                host = normalizedHost,
                port = editPort,
                baseDn = normalizedBaseDn,
                bindDn = normalizedBindDn,
                bindPass = normalizedBindPass.Length > 0 ? normalizedBindPass : preservedBindPass,
                authFilter = normalizedAuthFilter,
                version = editVersion,
                bindAttrAccount = normalizedAccountAttr,
                bindAttrEmployeeId = normalizedEmployeeAttr,
                bindAttrCode = normalizedCodeAttr,
                status = editStatus == 2 ? 2 : 1,
                remark = normalizedRemark,
                createTime = editCreateTime,
                createUserId = Optional(editCreateUserId),
                createUserName = editCreateUserName,
                updateTime = Optional(editUpdateTime),
                updateUserId = Optional(editUpdateUserId),
                updateUserName = editUpdateUserName,
                isDelete = editIsDelete,
                deleteTime = Optional(editDeleteTime)
            };

            saving = true;
            var request = editId.Length == 0 ? LdapClient.Add(input) : LdapClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存 LDAP 配置失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "LDAP 配置已新增" : "LDAP 配置已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存 LDAP 配置失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || actionLoading)
                return;

            actionLoading = true;
            LdapClient.Delete(row.id).Then(outcome =>
            {
                actionLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除 LDAP 配置失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("LDAP 配置已删除");
                LoadPage();
            }, _ =>
            {
                actionLoading = false;
                error = "删除 LDAP 配置失败，请检查网络后重试。";
            });
        }

        private void SyncUser(string id)
            => Sync(id, true);

        private void SyncOrg(string id)
            => Sync(id, false);

        private void Sync(string id, bool users)
        {
            var row = Find(id);
            if (row is null || actionLoading)
                return;

            actionLoading = true;
            var request = users ? LdapClient.SyncUser(row.id) : LdapClient.SyncOrg(row.id);
            request.Then(outcome =>
            {
                actionLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? (users ? "同步域账户失败。" : "同步域组织失败。");
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success(users ? "域账户同步已完成" : "域组织架构同步已完成");
                LoadPage();
            }, _ =>
            {
                actionLoading = false;
                error = users ? "同步域账户失败，请检查网络后重试。" : "同步域组织失败，请检查网络后重试。";
            });
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            auditDialogTitle = "修改记录 · " + row.host;
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = DisplayValue(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = DisplayValue(row.updateTime);
            auditDialogVisible = true;
        }

        private LdapView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static string TenantLabel(LdapTenantOption tenant)
            => tenant.host.Length == 0 ? tenant.label : tenant.label + " (" + tenant.host + ")";

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
            var options = new ConfigurationOption[tenantItems.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(tenantItems[index].value, TenantLabel(tenantItems[index]));
            return options;
        }

        private readonly double[] JoltPageSizes = [10, 20, 50, 100, 200, 500];

        private LdapTableRow[] TableRows()
        {
            var result = new LdapTableRow[rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = rows[index];
                result[index] = new LdapTableRow(row.id, row.tenantId, row.host, row.port, row.baseDn, row.bindDn,
                    row.authFilter, row.version, row.status, row.bindPass.Length > 0);
            }
            return result;
        }

        public LdapPageProps CreateProps() => new()
        {
            TenantOptions = TenantOptions(),
            TenantChanged = TenantChanged,
            TenantId = tenantId,
            SetEditTenantId = value => editTenantId = value,
            EditTenantId = editTenantId,
            IsSuperAdmin = IsSuperAdmin,
            HasActionColumn = HasActionColumn,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            CanSyncUser = CanSyncUser,
            CanSyncOrg = CanSyncOrg,
            DetailLoading = detailLoading,
            ActionLoading = actionLoading,
            OpenAuditRow = OpenAudit,
            OpenEditRow = OpenEdit,
            DeleteRow = Delete,
            SyncUserRow = SyncUser,
            SyncOrgRow = SyncOrg,
            Rows = TableRows(),
            EditStatus = editStatus,
            SetEditStatus = value => editStatus = (int)value,
            EditPort = editPort,
            SetEditPort = value => editPort = (int)(value ?? 389),
            EditVersion = editVersion,
            SetEditVersion = value => editVersion = (int)(value ?? 3),
            SetKeyword = value => keyword = value ?? "",
            SetHost = value => host = value ?? "",
            SetEditHost = value => editHost = value ?? "",
            SetEditBaseDn = value => editBaseDn = value ?? "",
            SetEditAuthFilter = value => editAuthFilter = value ?? "",
            SetEditBindDn = value => editBindDn = value ?? "",
            SetEditBindPass = value => editBindPass = value ?? "",
            SetEditBindAttrAccount = value => editBindAttrAccount = value ?? "",
            SetEditBindAttrEmployeeId = value => editBindAttrEmployeeId = value ?? "",
            SetEditBindAttrCode = value => editBindAttrCode = value ?? "",
            SetEditRemark = value => editRemark = value ?? "",
            SetEditDialogVisible = value => editDialogVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            CancelEdit = () => editDialogVisible = false,
            DismissError = () => error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            Refresh = Refresh,
            Save = Save,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            Error = error,
            TenantLoading = tenantLoading,
            Keyword = keyword,
            Host = host,
            Loading = loading,
            CanAdd = CanAdd,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            EditDialogVisible = editDialogVisible,
            EditDialogTitle = editDialogTitle,
            Editing = editId.Length > 0,
            EditHost = editHost,
            EditBaseDn = editBaseDn,
            EditAuthFilter = editAuthFilter,
            EditBindDn = editBindDn,
            EditBindPass = editBindPass,
            EditBindAttrAccount = editBindAttrAccount,
            EditBindAttrEmployeeId = editBindAttrEmployeeId,
            EditBindAttrCode = editBindAttrCode,
            EditRemark = editRemark,
            Saving = saving,
            AuditDialogVisible = auditDialogVisible,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            PageSizes = JoltPageSizes,
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
