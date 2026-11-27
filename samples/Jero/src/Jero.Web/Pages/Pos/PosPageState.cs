using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Pos;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Pos;

[ECMAScriptModule("components/pos-page-state.js")]
public static class PosPageModule
{
    public static PosPageState Create() => new();

    public sealed class PosPageState : global::Jero.Web.Composition.IPageState<PosPageProps>
    {
        private PosView[] _items = [];
        private PosView[] _rows = [];
        private TenantOption[] _tenantItems = [];
        private string _tenantId = string.Empty;
        private string _name = string.Empty;
        private string _posCode = string.Empty;
        private bool _loading;
        private bool _tenantLoading;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增职位";
        private bool _saving;
        private string _editId = string.Empty;
        private string _editTenantId = string.Empty;
        private string _editName = string.Empty;
        private string _editCode = string.Empty;
        private int _editOrderNo = 100;
        private int _editStatus = 1;
        private string _editRemark = string.Empty;

        private bool _usersDialogVisible;
        private string _usersDialogTitle = "人员明细";
        private PosUserView[] _userRows = [];

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

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
            _tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadPositions();
        }

        private void LoadTenants()
        {
            _tenantLoading = true;
            PosClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }

                _tenantItems = PosClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPositions()
        {
            _loading = true;
            PosClient.List(new PosListInput
            {
                Name = _name.Trim(),
                Code = _posCode.Trim(),
                TenantId = IsSuperAdmin ? _tenantId : string.Empty
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载职位失败。";
                    _items = [];
                    _rows = [];
                    return;
                }

                _items = PosClient.ReadPositions(outcome.Data);
                _rows = _items;
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载职位失败，请检查网络后重试。";
            });
        }

        private void Search() => LoadPositions();

        private void ResetSearch()
        {
            _name = string.Empty;
            _posCode = string.Empty;
            LoadPositions();
        }

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
            LoadPositions();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? _tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增职位");
                return;
            }

            _editDialogTitle = "新增职位";
            _editId = string.Empty;
            _editTenantId = targetTenantId;
            _editName = string.Empty;
            _editCode = string.Empty;
            _editOrderNo = 100;
            _editStatus = 1;
            _editRemark = string.Empty;
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEditRow(row))
                return;

            _editDialogTitle = "编辑职位";
            PopulateForm(row, false);
        }

        private void OpenCopy(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            var targetTenantId = row.TenantId.Length > 0 ? row.TenantId : _tenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再复制职位");
                return;
            }

            _editDialogTitle = "复制职位";
            PopulateForm(row, true);
        }

        private void PopulateForm(PosView row, bool copy)
        {
            _editId = copy ? string.Empty : row.Id;
            _editTenantId = row.TenantId.Length > 0 ? row.TenantId : DefaultTenantId;
            _editName = copy ? string.Empty : row.Name;
            _editCode = copy ? string.Empty : row.Code;
            _editOrderNo = row.OrderNo;
            _editStatus = row.Status == 2 ? 2 : 1;
            _editRemark = row.Remark;
            _editDialogVisible = true;
        }

        private void Save()
        {
            if (_saving)
                return;

            var normalizedName = _editName.Trim();
            var normalizedCode = _editCode.Trim();
            var normalizedRemark = _editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                _error = "职位名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                _error = "职位名称不能超过64个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                _error = "职位编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 64)
            {
                _error = "职位编码不能超过64个字符。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                _error = "备注不能超过128个字符。";
                return;
            }
            if (IsSuperAdmin && _editTenantId.Length == 0)
            {
                _error = "超级管理员维护职位时必须选择租户。";
                return;
            }

            _saving = true;
            var input = new PosSaveInput
            {
                Id = _editId,
                TenantId = IsSuperAdmin ? _editTenantId : DefaultTenantId,
                Name = normalizedName,
                Code = normalizedCode,
                OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                Status = _editStatus == 2 ? 2 : 1,
                Remark = normalizedRemark
            };
            var request = _editId.Length == 0 ? PosClient.Add(input) : PosClient.Update(input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存职位失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "职位已新增" : "职位已更新");
                LoadPositions();
            }, _ =>
            {
                _saving = false;
                _error = "保存职位失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDeleteRow(row))
                return;

            PosClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除职位失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("职位已删除");
                LoadPositions();
            }, _ => _error = "删除职位失败，请检查网络后重试。");
        }

        private void OpenUsers(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            _usersDialogTitle = "人员明细 · " + row.Name;
            _userRows = row.UserList;
            _usersDialogVisible = true;
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            _auditDialogTitle = "修改记录 · " + row.Name;
            _auditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            _auditCreateTime = DisplayValue(row.CreateTime);
            _auditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            _auditUpdateTime = DisplayValue(row.UpdateTime);
            _auditDialogVisible = true;
        }

        private bool CanEditRow(PosView? row) => CanEdit && IsOwn(row);

        private bool CanDeleteRow(PosView? row)
            => CanDelete && IsOwn(row) && row is not null && row.UserList.Length == 0;

        private string EditDisabledReason(PosView row)
            => IsOwn(row) ? string.Empty : "仅创建者可编辑";

        private string DeleteDisabledReason(PosView row)
        {
            if (!IsOwn(row))
                return "仅创建者可删除";
            return row.UserList.Length > 0 ? "当前职位存在在职人员，不可删除" : string.Empty;
        }
        private bool IsOwn(PosView? row)
            => row is not null && (IsSuperAdmin || (CurrentUserId.Length > 0 && row.CreateUserId == CurrentUserId));

        private PosView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static string TenantLabel(TenantOption tenant)
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

        private ConfigurationOption[] TenantOptions()
        {
            var options = new ConfigurationOption[_tenantItems.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(_tenantItems[index].Value, TenantLabel(_tenantItems[index]));
            return options;
        }

        private PosTableRow[] TableRows()
        {
            var result = new PosTableRow[_rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _rows[index];
                result[index] = new PosTableRow(row.Id, row.Name, row.Code, row.OrderNo, row.Status, row.UserList.Length,
                    CanEditRow(row), CanDeleteRow(row), EditDisabledReason(row), DeleteDisabledReason(row));
            }
            return result;
        }

        private PosUserTableRow[] UserTableRows()
        {
            var result = new PosUserTableRow[_userRows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _userRows[index];
                result[index] = new PosUserTableRow(row.Id, row.Account, row.RealName);
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
            TenantId = _tenantId,
            TenantLoading = _tenantLoading,
            TenantChanged = TenantChanged,
            EditTenantId = _editTenantId,
            Editing = _editId.Length > 0,
            SetEditTenantId = value => _editTenantId = value,
            EditOrderNo = _editOrderNo,
            SetEditOrderNo = value => _editOrderNo = (int)(value ?? 100),
            EditStatus = _editStatus,
            SetEditStatus = value => _editStatus = (int)value,
            SetName = value => _name = value ?? "",
            SetPosCode = value => _posCode = value ?? "",
            SetEditName = value => _editName = value ?? "",
            SetEditCode = value => _editCode = value ?? "",
            SetEditRemark = value => _editRemark = value ?? "",
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetUsersDialogVisible = value => _usersDialogVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            DismissError = () => _error = null,
            CancelEdit = () => _editDialogVisible = false,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPositions = LoadPositions,
            Save = Save,
            Error = _error,
            Name = _name,
            PosCode = _posCode,
            Loading = _loading,
            Saving = _saving,
            IsSuperAdmin = IsSuperAdmin,
            EditDialogVisible = _editDialogVisible,
            EditDialogTitle = _editDialogTitle,
            EditName = _editName,
            EditCode = _editCode,
            EditRemark = _editRemark,
            UsersDialogVisible = _usersDialogVisible,
            UsersDialogTitle = _usersDialogTitle,
            AuditDialogVisible = _auditDialogVisible,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
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
