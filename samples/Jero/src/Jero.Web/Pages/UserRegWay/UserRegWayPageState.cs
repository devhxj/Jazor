using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.UserRegWay;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.UserRegWay;

[ECMAScriptModule("components/user-reg-way-page-state.js")]
public static class UserRegWayPageModule
{
    public static UserRegWayPageState Create() => new();

    public sealed class UserRegWayPageState : global::Jero.Web.Composition.IPageState<UserRegWayPageProps>
    {
        private UserRegWayView[] _items = [];
        private UserRegWayView[] _rows = [];
        private UserRegWayTenantOption[] _tenantItems = [];
        private UserRegWayAccountTypeOption[] _accountTypeOptions = DefaultAccountTypes();
        private string _tenantId = string.Empty;
        private string _keyword = string.Empty;
        private string _name = string.Empty;
        private string _field = "orderNo";
        private string _order = "ascending";
        private bool _loading;
        private bool _tenantLoading;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增注册方案";
        private bool _saving;
        private bool _dependencyLoading;
        private int _dependencyVersion;
        private int _dependencyPending;
        private string _editId = string.Empty;
        private string _editTenantId = string.Empty;
        private string _editName = string.Empty;
        private int _editAccountType = 777;
        private string _editRoleId = string.Empty;
        private string _editRoleName = string.Empty;
        private string _editOrgId = string.Empty;
        private string _editOrgName = string.Empty;
        private string _editPosId = string.Empty;
        private string _editPosName = string.Empty;
        private int _editOrderNo = 100;
        private string _editRemark = string.Empty;
        private UserRegWayNamedOption[] _roleOptions = [];
        private UserRegWayNamedOption[] _orgOptions = [];
        private UserRegWayNamedOption[] _posOptions = [];

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";
        private string _auditRemark = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysUserRegWay:add");
        private bool CanEdit => ApplicationState.HasPermission("sysUserRegWay:update");
        private bool CanDelete => ApplicationState.HasPermission("sysUserRegWay:delete");
        private bool HasActionColumn => CanEdit || CanDelete;
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            _tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadAccountTypes();
            LoadItems();
        }

        private void LoadTenants()
        {
            _tenantLoading = true;
            UserRegWayClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }

                _tenantItems = UserRegWayClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadAccountTypes()
        {
            UserRegWayClient.AccountTypes().Then(outcome =>
            {
                if (!outcome.Ok)
                    return;
                var loaded = UserRegWayClient.ReadAccountTypes(outcome.Data);
                if (loaded.Length > 0)
                    _accountTypeOptions = loaded;
            });
        }

        private void LoadItems()
        {
            _loading = true;
            UserRegWayClient.List(new UserRegWayListInput
            {
                Keyword = _keyword.Trim(),
                Name = _name.Trim(),
                TenantId = IsSuperAdmin ? _tenantId : DefaultTenantId,
                Field = _field,
                Order = _order,
                DescStr = "descending"
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载注册方案失败。";
                    _items = [];
                    _rows = [];
                    return;
                }

                _items = UserRegWayClient.ReadItems(outcome.Data);
                _rows = _items;
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载注册方案失败，请检查网络后重试。";
            });
        }

        private void Search() => LoadItems();

        private void ResetSearch()
        {
            _keyword = string.Empty;
            _name = string.Empty;
            _tenantId = IsSuperAdmin ? string.Empty : DefaultTenantId;
            _field = "orderNo";
            _order = "ascending";
            LoadItems();
        }

        private void Refresh()
        {
            if (IsSuperAdmin)
                LoadTenants();
            LoadAccountTypes();
            LoadItems();
        }

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
            LoadItems();
        }

        private void SortRows(string prop, string direction)
        {
            _field = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            _order = direction == "descending" ? "descending" : "ascending";
            LoadItems();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? _tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增注册方案");
                return;
            }

            _editDialogTitle = "新增注册方案";
            _editId = string.Empty;
            _editTenantId = targetTenantId;
            _editName = string.Empty;
            _editAccountType = _accountTypeOptions.Length == 0 ? 777 : _accountTypeOptions[0].Value;
            _editRoleId = string.Empty;
            _editRoleName = string.Empty;
            _editOrgId = string.Empty;
            _editOrgName = string.Empty;
            _editPosId = string.Empty;
            _editPosName = string.Empty;
            _editOrderNo = 100;
            _editRemark = string.Empty;
            _error = null;
            _editDialogVisible = true;
            LoadDependencies();
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _editDialogTitle = "编辑注册方案";
            _editId = row.Id;
            _editTenantId = row.TenantId.Length > 0 ? row.TenantId : DefaultTenantId;
            _editName = row.Name;
            _editAccountType = row.AccountType;
            _editRoleId = row.RoleId;
            _editRoleName = row.RoleName;
            _editOrgId = row.OrgId;
            _editOrgName = row.OrgName;
            _editPosId = row.PosId;
            _editPosName = row.PosName;
            _editOrderNo = row.OrderNo;
            _editRemark = row.Remark;
            _error = null;
            _editDialogVisible = true;
            LoadDependencies();
        }

        private void EditTenantChanged(string? value)
        {
            _editTenantId = value ?? string.Empty;
            _editRoleId = string.Empty;
            _editRoleName = string.Empty;
            _editOrgId = string.Empty;
            _editOrgName = string.Empty;
            _editPosId = string.Empty;
            _editPosName = string.Empty;
            LoadDependencies();
        }

        private void LoadDependencies()
        {
            var requestVersion = ++_dependencyVersion;
            _dependencyPending = 3;
            _dependencyLoading = true;
            _roleOptions = [];
            _orgOptions = [];
            _posOptions = [];

            UserRegWayClient.Roles().Then(outcome =>
            {
                if (requestVersion != _dependencyVersion)
                    return;
                if (outcome.Ok)
                    _roleOptions = EnsureOption(UserRegWayClient.ReadRoles(outcome.Data, _editTenantId), _editRoleId, _editRoleName);
                else
                    _error = outcome.Error ?? "加载角色失败。";
                CompleteDependency(requestVersion);
            }, _ =>
            {
                if (requestVersion != _dependencyVersion)
                    return;
                _error = "加载角色失败，请检查网络后重试。";
                _roleOptions = EnsureOption([], _editRoleId, _editRoleName);
                CompleteDependency(requestVersion);
            });

            UserRegWayClient.Organizations(_editTenantId).Then(outcome =>
            {
                if (requestVersion != _dependencyVersion)
                    return;
                if (outcome.Ok)
                    _orgOptions = EnsureOption(UserRegWayClient.ReadOrganizations(outcome.Data), _editOrgId, _editOrgName);
                else
                    _error = outcome.Error ?? "加载机构失败。";
                CompleteDependency(requestVersion);
            }, _ =>
            {
                if (requestVersion != _dependencyVersion)
                    return;
                _error = "加载机构失败，请检查网络后重试。";
                _orgOptions = EnsureOption([], _editOrgId, _editOrgName);
                CompleteDependency(requestVersion);
            });

            UserRegWayClient.Positions(_editTenantId).Then(outcome =>
            {
                if (requestVersion != _dependencyVersion)
                    return;
                if (outcome.Ok)
                    _posOptions = EnsureOption(UserRegWayClient.ReadPositions(outcome.Data, _editTenantId), _editPosId, _editPosName);
                else
                    _error = outcome.Error ?? "加载职位失败。";
                CompleteDependency(requestVersion);
            }, _ =>
            {
                if (requestVersion != _dependencyVersion)
                    return;
                _error = "加载职位失败，请检查网络后重试。";
                _posOptions = EnsureOption([], _editPosId, _editPosName);
                CompleteDependency(requestVersion);
            });
        }

        private void CompleteDependency(int requestVersion)
        {
            if (requestVersion != _dependencyVersion)
                return;
            _dependencyPending--;
            if (_dependencyPending <= 0)
                _dependencyLoading = false;
        }

        private void Save()
        {
            if (_saving || _dependencyLoading)
                return;

            var normalizedName = _editName.Trim();
            var normalizedRemark = _editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                _error = "方案名称不能为空。";
                return;
            }
            if (normalizedName.Length > 32)
            {
                _error = "方案名称不能超过32个字符。";
                return;
            }
            if (_editAccountType <= 0 || _editAccountType == 888 || _editAccountType == 999)
            {
                _error = "请选择可注册的账户类型。";
                return;
            }
            if (_editRoleId.Length == 0)
            {
                _error = "绑定角色不能为空。";
                return;
            }
            if (_editOrgId.Length == 0)
            {
                _error = "绑定机构不能为空。";
                return;
            }
            if (_editPosId.Length == 0)
            {
                _error = "绑定职位不能为空。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                _error = "备注不能超过128个字符。";
                return;
            }
            if (_editTenantId.Length == 0)
            {
                _error = "租户不能为空。";
                return;
            }

            var input = new UserRegWaySaveInput
            {
                Id = _editId,
                TenantId = _editTenantId,
                Name = normalizedName,
                AccountType = _editAccountType,
                RoleId = _editRoleId,
                OrgId = _editOrgId,
                PosId = _editPosId,
                OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                Remark = normalizedRemark
            };

            _saving = true;
            var request = _editId.Length == 0 ? UserRegWayClient.Add(input) : UserRegWayClient.Update(input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存注册方案失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "注册方案已新增" : "注册方案已更新");
                LoadItems();
            }, _ =>
            {
                _saving = false;
                _error = "保存注册方案失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            UserRegWayClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除注册方案失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("注册方案已删除");
                LoadItems();
            }, _ => _error = "删除注册方案失败，请检查网络后重试。");
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
            _auditRemark = DisplayValue(row.Remark);
            _auditDialogVisible = true;
        }

        private UserRegWayView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private string AccountTypeLabel(UserRegWayView row)
        {
            var value = row.AccountType;
            for (var index = 0; index < _accountTypeOptions.Length; index++)
                if (_accountTypeOptions[index].Value == value)
                    return _accountTypeOptions[index].Label;
            return value > 0 ? "类型" + value : "-";
        }

        private string AccountTypeTag(UserRegWayView row)
        {
            var value = row.AccountType;
            for (var index = 0; index < _accountTypeOptions.Length; index++)
            {
                if (_accountTypeOptions[index].Value != value)
                    continue;
                var tag = _accountTypeOptions[index].TagType;
                if (tag == "success") return "success";
                if (tag == "warning") return "warning";
                if (tag == "danger") return "danger";
                if (tag == "info") return "info";
                return "primary";
            }
            return value == 666 ? "success" : "primary";
        }

        private static UserRegWayNamedOption[] EnsureOption(
            UserRegWayNamedOption[] source,
            string id,
            string name)
        {
            if (id.Length == 0)
                return source;
            for (var index = 0; index < source.Length; index++)
                if (source[index].Id == id)
                    return source;

            var result = new UserRegWayNamedOption[source.Length + 1];
            for (var index = 0; index < source.Length; index++)
                result[index] = source[index];
            result[source.Length] = new UserRegWayNamedOption
            {
                Id = id,
                Name = name.Length == 0 ? "历史选项 (" + id + ")" : name
            };
            return result;
        }

        private static UserRegWayAccountTypeOption[] DefaultAccountTypes()
            =>
            [
                new UserRegWayAccountTypeOption { Value = 666, Label = "会员", TagType = "success" },
                new UserRegWayAccountTypeOption { Value = 777, Label = "普通账号", TagType = "primary" }
            ];

        private static string TenantLabel(UserRegWayTenantOption tenant)
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

        private ConfigurationOption[] AccountOptions()
        {
            var options = new ConfigurationOption[_accountTypeOptions.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(_accountTypeOptions[index].Value.ToString(), _accountTypeOptions[index].Label);
            return options;
        }

        private static ConfigurationOption[] NamedOptions(UserRegWayNamedOption[] items)
        {
            var options = new ConfigurationOption[items.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(items[index].Id, items[index].Name);
            return options;
        }

        private UserRegWayTableRow[] TableRows()
        {
            var result = new UserRegWayTableRow[_rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _rows[index];
                result[index] = new UserRegWayTableRow(row.Id, row.Name, row.OrgName, row.RoleName, row.PosName, row.OrderNo,
                    AccountTypeLabel(row), AccountTypeTag(row));
            }
            return result;
        }

        public UserRegWayPageProps CreateProps() => new()
        {
            TenantOptions = TenantOptions(),
            TenantChanged = TenantChanged,
            TenantId = _tenantId,
            EditTenantChanged = EditTenantChanged,
            EditTenantId = _editTenantId,
            RoleOptions = NamedOptions(_roleOptions),
            SetEditRoleId = value => _editRoleId = value,
            EditRoleId = _editRoleId,
            OrgOptions = NamedOptions(_orgOptions),
            SetEditOrgId = value => _editOrgId = value,
            EditOrgId = _editOrgId,
            PosOptions = NamedOptions(_posOptions),
            SetEditPosId = value => _editPosId = value,
            EditPosId = _editPosId,
            AccountTypeOptions = AccountOptions(),
            EditAccountType = _editAccountType.ToString(),
            SetEditAccountType = value => _editAccountType = (int)ECMAScript.Global.NumberValue(value),
            HasActionColumn = HasActionColumn,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            OpenAuditRow = OpenAudit,
            OpenEditRow = OpenEdit,
            DeleteRow = Delete,
            SortRows = SortRows,
            Rows = TableRows(),
            EditOrderNo = _editOrderNo,
            SetEditOrderNo = value => _editOrderNo = (int)(value ?? 100),
            SetKeyword = value => _keyword = value ?? "",
            SetName = value => _name = value ?? "",
            SetEditName = value => _editName = value ?? "",
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
            Error = _error,
            IsSuperAdmin = IsSuperAdmin,
            TenantLoading = _tenantLoading,
            Keyword = _keyword,
            Name = _name,
            Loading = _loading,
            CanAdd = CanAdd,
            EditDialogVisible = _editDialogVisible,
            EditDialogTitle = _editDialogTitle,
            Editing = _editId.Length > 0,
            DependencyLoading = _dependencyLoading,
            EditName = _editName,
            EditRemark = _editRemark,
            Saving = _saving,
            AuditDialogVisible = _auditDialogVisible,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            AuditRemark = _auditRemark,
        };
    }
}

public sealed record UserRegWayPageProps : VueProps
{
    public required ConfigurationOption[] TenantOptions { get; init; }
    public required Action<string> TenantChanged { get; init; }
    public required string TenantId { get; init; }
    public required Action<string> EditTenantChanged { get; init; }
    public required string EditTenantId { get; init; }
    public required ConfigurationOption[] RoleOptions { get; init; }
    public required Action<string> SetEditRoleId { get; init; }
    public required string EditRoleId { get; init; }
    public required ConfigurationOption[] OrgOptions { get; init; }
    public required Action<string> SetEditOrgId { get; init; }
    public required string EditOrgId { get; init; }
    public required ConfigurationOption[] PosOptions { get; init; }
    public required Action<string> SetEditPosId { get; init; }
    public required string EditPosId { get; init; }
    public required ConfigurationOption[] AccountTypeOptions { get; init; }
    public required string EditAccountType { get; init; }
    public required Action<string> SetEditAccountType { get; init; }
    public required bool HasActionColumn { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required Action<string> OpenAuditRow { get; init; }
    public required Action<string> OpenEditRow { get; init; }
    public required Action<string> DeleteRow { get; init; }
    public required Action<string, string> SortRows { get; init; }
    public required UserRegWayTableRow[] Rows { get; init; }
    public required int EditOrderNo { get; init; }
    public required Action<double?> SetEditOrderNo { get; init; }
    public required Action<string?> SetKeyword { get; init; }
    public required Action<string?> SetName { get; init; }
    public required Action<string?> SetEditName { get; init; }
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
    public required string? Error { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required bool TenantLoading { get; init; }
    public required string Keyword { get; init; }
    public required string Name { get; init; }
    public required bool Loading { get; init; }
    public required bool CanAdd { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required string EditDialogTitle { get; init; }
    public required bool Editing { get; init; }
    public required bool DependencyLoading { get; init; }
    public required string EditName { get; init; }
    public required string EditRemark { get; init; }
    public required bool Saving { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required string AuditRemark { get; init; }
}
