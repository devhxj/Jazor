using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Pos;

[ECMAScriptModule("components/pos-page-state.js")]
public static class PosPageModule
{
    public static PosPageState Create() => new();

    public sealed class PosPageState : global::Jero.Web.App.IPageState<PosPageProps>
    {
        private PosView[] items = [];
        private PosView[] rows = [];
        private TenantOption[] tenantItems = [];
        private string tenantId = string.Empty;
        private string name = string.Empty;
        private string posCode = string.Empty;
        private bool loading;
        private bool tenantLoading;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增职位";
        private bool saving;
        private string editId = string.Empty;
        private string editTenantId = string.Empty;
        private string editName = string.Empty;
        private string editCode = string.Empty;
        private int editOrderNo = 100;
        private int editStatus = 1;
        private string editRemark = string.Empty;

        private bool usersDialogVisible;
        private string usersDialogTitle = "人员明细";
        private PosUserView[] userRows = [];

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysPos:add");
        private bool CanEdit => ApplicationState.HasPermission("sysPos:update");
        private bool CanDelete => ApplicationState.HasPermission("sysPos:delete");
        private bool HasActionColumn => CanAdd || CanEdit || CanDelete;
        private string CurrentUserId => ApplicationState.User?.Id ?? string.Empty;
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadPositions();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            PosClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }

                tenantItems = PosClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPositions()
        {
            loading = true;
            PosClient.List(new PosListInput
            {
                name = name.Trim(),
                code = posCode.Trim(),
                tenantId = IsSuperAdmin ? tenantId : string.Empty
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载职位失败。";
                    items = [];
                    rows = [];
                    return;
                }

                items = PosClient.ReadPositions(outcome.Data);
                rows = items;
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载职位失败，请检查网络后重试。";
            });
        }

        private void Search() => LoadPositions();

        private void ResetSearch()
        {
            name = string.Empty;
            posCode = string.Empty;
            LoadPositions();
        }

        private void TenantChanged(string? value)
        {
            tenantId = value ?? string.Empty;
            LoadPositions();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增职位");
                return;
            }

            editDialogTitle = "新增职位";
            editId = string.Empty;
            editTenantId = targetTenantId;
            editName = string.Empty;
            editCode = string.Empty;
            editOrderNo = 100;
            editStatus = 1;
            editRemark = string.Empty;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEditRow(row))
                return;

            editDialogTitle = "编辑职位";
            PopulateForm(row, false);
        }

        private void OpenCopy(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            var targetTenantId = row.tenantId.Length > 0 ? row.tenantId : tenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再复制职位");
                return;
            }

            editDialogTitle = "复制职位";
            PopulateForm(row, true);
        }

        private void PopulateForm(PosView row, bool copy)
        {
            editId = copy ? string.Empty : row.id;
            editTenantId = row.tenantId.Length > 0 ? row.tenantId : DefaultTenantId;
            editName = copy ? string.Empty : row.name;
            editCode = copy ? string.Empty : row.code;
            editOrderNo = row.orderNo;
            editStatus = row.status == 2 ? 2 : 1;
            editRemark = row.remark;
            editDialogVisible = true;
        }

        private void Save()
        {
            if (saving)
                return;

            var normalizedName = editName.Trim();
            var normalizedCode = editCode.Trim();
            var normalizedRemark = editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                error = "职位名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                error = "职位名称不能超过64个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                error = "职位编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 64)
            {
                error = "职位编码不能超过64个字符。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                error = "备注不能超过128个字符。";
                return;
            }
            if (IsSuperAdmin && editTenantId.Length == 0)
            {
                error = "超级管理员维护职位时必须选择租户。";
                return;
            }

            saving = true;
            var input = new PosSaveInput
            {
                id = editId,
                tenantId = IsSuperAdmin ? editTenantId : DefaultTenantId,
                name = normalizedName,
                code = normalizedCode,
                orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                status = editStatus == 2 ? 2 : 1,
                remark = normalizedRemark
            };
            var request = editId.Length == 0 ? PosClient.Add(input) : PosClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存职位失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "职位已新增" : "职位已更新");
                LoadPositions();
            }, _ =>
            {
                saving = false;
                error = "保存职位失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDeleteRow(row))
                return;

            PosClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除职位失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("职位已删除");
                LoadPositions();
            }, _ => error = "删除职位失败，请检查网络后重试。");
        }

        private void OpenUsers(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            usersDialogTitle = "人员明细 · " + row.name;
            userRows = row.userList;
            usersDialogVisible = true;
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            auditDialogTitle = "修改记录 · " + row.name;
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = DisplayValue(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = DisplayValue(row.updateTime);
            auditDialogVisible = true;
        }

        private bool CanEditRow(PosView? row) => CanEdit && IsOwn(row);

        private bool CanDeleteRow(PosView? row)
            => CanDelete && IsOwn(row) && row is not null && row.userList.Length == 0;

        private string EditDisabledReason(PosView row)
            => IsOwn(row) ? string.Empty : "仅创建者可编辑";

        private string DeleteDisabledReason(PosView row)
        {
            if (!IsOwn(row))
                return "仅创建者可删除";
            return row.userList.Length > 0 ? "当前职位存在在职人员，不可删除" : string.Empty;
        }
        private bool IsOwn(PosView? row)
            => row is not null && (IsSuperAdmin || (CurrentUserId.Length > 0 && row.createUserId == CurrentUserId));

        private PosView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static string TenantLabel(TenantOption tenant)
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

        private ConfigurationOption[] TenantOptions()
        {
            var options = new ConfigurationOption[tenantItems.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(tenantItems[index].value, TenantLabel(tenantItems[index]));
            return options;
        }

        private PosTableRow[] TableRows()
        {
            var result = new PosTableRow[rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = rows[index];
                result[index] = new PosTableRow(row.id, row.name, row.code, row.orderNo, row.status, row.userList.Length,
                    CanEditRow(row), CanDeleteRow(row), EditDisabledReason(row), DeleteDisabledReason(row));
            }
            return result;
        }

        private PosUserTableRow[] UserTableRows()
        {
            var result = new PosUserTableRow[userRows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = userRows[index];
                result[index] = new PosUserTableRow(row.id, row.account, row.realName);
            }
            return result;
        }

        public PosPageProps CreateProps() => new()
        {
            Rows = TableRows(),
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            HasActionColumn = HasActionColumn,
            OpenUsers = OpenUsers,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            OpenCopy = OpenCopy,
            Delete = Delete,
            UserRows = UserTableRows(),
            TenantOptions = TenantOptions(),
            TenantId = tenantId,
            TenantLoading = tenantLoading,
            TenantChanged = TenantChanged,
            EditTenantId = editTenantId,
            Editing = editId.Length > 0,
            SetEditTenantId = value => editTenantId = value,
            EditOrderNo = editOrderNo,
            SetEditOrderNo = value => editOrderNo = (int)(value ?? 100),
            EditStatus = editStatus,
            SetEditStatus = value => editStatus = (int)value,
            SetName = value => name = value ?? "",
            SetPosCode = value => posCode = value ?? "",
            SetEditName = value => editName = value ?? "",
            SetEditCode = value => editCode = value ?? "",
            SetEditRemark = value => editRemark = value ?? "",
            SetEditDialogVisible = value => editDialogVisible = value,
            SetUsersDialogVisible = value => usersDialogVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            DismissError = () => error = null,
            CancelEdit = () => editDialogVisible = false,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPositions = LoadPositions,
            Save = Save,
            Error = error,
            Name = name,
            PosCode = posCode,
            Loading = loading,
            Saving = saving,
            IsSuperAdmin = IsSuperAdmin,
            EditDialogVisible = editDialogVisible,
            EditDialogTitle = editDialogTitle,
            EditName = editName,
            EditCode = editCode,
            EditRemark = editRemark,
            UsersDialogVisible = usersDialogVisible,
            UsersDialogTitle = usersDialogTitle,
            AuditDialogVisible = auditDialogVisible,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
        };
    }
}

public sealed record PosPageProps : VueProps
{
    public required PosTableRow[] Rows { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool HasActionColumn { get; init; }
    public required Action<string> OpenUsers { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> OpenCopy { get; init; }
    public required Action<string> Delete { get; init; }
    public required PosUserTableRow[] UserRows { get; init; }
    public required ConfigurationOption[] TenantOptions { get; init; }
    public required string TenantId { get; init; }
    public required bool TenantLoading { get; init; }
    public required Action<string> TenantChanged { get; init; }
    public required string EditTenantId { get; init; }
    public required bool Editing { get; init; }
    public required Action<string> SetEditTenantId { get; init; }
    public required int EditOrderNo { get; init; }
    public required Action<double?> SetEditOrderNo { get; init; }
    public required int EditStatus { get; init; }
    public required Action<double> SetEditStatus { get; init; }
    public required Action<string?> SetName { get; init; }
    public required Action<string?> SetPosCode { get; init; }
    public required Action<string?> SetEditName { get; init; }
    public required Action<string?> SetEditCode { get; init; }
    public required Action<string?> SetEditRemark { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetUsersDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action DismissError { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action LoadPositions { get; init; }
    public required Action Save { get; init; }
    public required string? Error { get; init; }
    public required string Name { get; init; }
    public required string PosCode { get; init; }
    public required bool Loading { get; init; }
    public required bool Saving { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditName { get; init; }
    public required string EditCode { get; init; }
    public required string EditRemark { get; init; }
    public required bool UsersDialogVisible { get; init; }
    public required string UsersDialogTitle { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
}
