using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.UserRegWay;

[ECMAScriptModule("components/user-reg-way-page-state.js")]
public static class UserRegWayPageModule
{
    public static UserRegWayPageState Create() => new();

    public sealed class UserRegWayPageState : global::Jero.Web.App.IPageState<UserRegWayPageProps>
    {
        private UserRegWayView[] items = [];
        private UserRegWayView[] rows = [];
        private UserRegWayTenantOption[] tenantItems = [];
        private UserRegWayAccountTypeOption[] accountTypeOptions = DefaultAccountTypes();
        private string tenantId = string.Empty;
        private string keyword = string.Empty;
        private string name = string.Empty;
        private string field = "orderNo";
        private string order = "ascending";
        private bool loading;
        private bool tenantLoading;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增注册方案";
        private bool saving;
        private bool dependencyLoading;
        private int dependencyVersion;
        private int dependencyPending;
        private string editId = string.Empty;
        private string editTenantId = string.Empty;
        private string editName = string.Empty;
        private int editAccountType = 777;
        private string editRoleId = string.Empty;
        private string editRoleName = string.Empty;
        private string editOrgId = string.Empty;
        private string editOrgName = string.Empty;
        private string editPosId = string.Empty;
        private string editPosName = string.Empty;
        private int editOrderNo = 100;
        private string editRemark = string.Empty;
        private UserRegWayNamedOption[] roleOptions = [];
        private UserRegWayNamedOption[] orgOptions = [];
        private UserRegWayNamedOption[] posOptions = [];

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";
        private string auditRemark = "-";

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
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadAccountTypes();
            LoadItems();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            UserRegWayClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }

                tenantItems = UserRegWayClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
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
                    accountTypeOptions = loaded;
            });
        }

        private void LoadItems()
        {
            loading = true;
            UserRegWayClient.List(new UserRegWayListInput
            {
                keyword = keyword.Trim(),
                name = name.Trim(),
                tenantId = IsSuperAdmin ? tenantId : DefaultTenantId,
                field = field,
                order = order,
                descStr = "descending"
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载注册方案失败。";
                    items = [];
                    rows = [];
                    return;
                }

                items = UserRegWayClient.ReadItems(outcome.Data);
                rows = items;
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载注册方案失败，请检查网络后重试。";
            });
        }

        private void Search() => LoadItems();

        private void ResetSearch()
        {
            keyword = string.Empty;
            name = string.Empty;
            tenantId = IsSuperAdmin ? string.Empty : DefaultTenantId;
            field = "orderNo";
            order = "ascending";
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
            tenantId = value ?? string.Empty;
            LoadItems();
        }

        private void SortRows(string prop, string direction)
        {
            field = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            order = direction == "descending" ? "descending" : "ascending";
            LoadItems();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增注册方案");
                return;
            }

            editDialogTitle = "新增注册方案";
            editId = string.Empty;
            editTenantId = targetTenantId;
            editName = string.Empty;
            editAccountType = accountTypeOptions.Length == 0 ? 777 : accountTypeOptions[0].value;
            editRoleId = string.Empty;
            editRoleName = string.Empty;
            editOrgId = string.Empty;
            editOrgName = string.Empty;
            editPosId = string.Empty;
            editPosName = string.Empty;
            editOrderNo = 100;
            editRemark = string.Empty;
            error = null;
            editDialogVisible = true;
            LoadDependencies();
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            editDialogTitle = "编辑注册方案";
            editId = row.id;
            editTenantId = row.tenantId.Length > 0 ? row.tenantId : DefaultTenantId;
            editName = row.name;
            editAccountType = row.accountType;
            editRoleId = row.roleId;
            editRoleName = row.roleName;
            editOrgId = row.orgId;
            editOrgName = row.orgName;
            editPosId = row.posId;
            editPosName = row.posName;
            editOrderNo = row.orderNo;
            editRemark = row.remark;
            error = null;
            editDialogVisible = true;
            LoadDependencies();
        }

        private void EditTenantChanged(string? value)
        {
            editTenantId = value ?? string.Empty;
            editRoleId = string.Empty;
            editRoleName = string.Empty;
            editOrgId = string.Empty;
            editOrgName = string.Empty;
            editPosId = string.Empty;
            editPosName = string.Empty;
            LoadDependencies();
        }

        private void LoadDependencies()
        {
            var requestVersion = ++dependencyVersion;
            dependencyPending = 3;
            dependencyLoading = true;
            roleOptions = [];
            orgOptions = [];
            posOptions = [];

            UserRegWayClient.Roles().Then(outcome =>
            {
                if (requestVersion != dependencyVersion)
                    return;
                if (outcome.Ok)
                    roleOptions = EnsureOption(UserRegWayClient.ReadRoles(outcome.Data, editTenantId), editRoleId, editRoleName);
                else
                    error = outcome.Error ?? "加载角色失败。";
                CompleteDependency(requestVersion);
            }, _ =>
            {
                if (requestVersion != dependencyVersion)
                    return;
                error = "加载角色失败，请检查网络后重试。";
                roleOptions = EnsureOption([], editRoleId, editRoleName);
                CompleteDependency(requestVersion);
            });

            UserRegWayClient.Organizations(editTenantId).Then(outcome =>
            {
                if (requestVersion != dependencyVersion)
                    return;
                if (outcome.Ok)
                    orgOptions = EnsureOption(UserRegWayClient.ReadOrganizations(outcome.Data), editOrgId, editOrgName);
                else
                    error = outcome.Error ?? "加载机构失败。";
                CompleteDependency(requestVersion);
            }, _ =>
            {
                if (requestVersion != dependencyVersion)
                    return;
                error = "加载机构失败，请检查网络后重试。";
                orgOptions = EnsureOption([], editOrgId, editOrgName);
                CompleteDependency(requestVersion);
            });

            UserRegWayClient.Positions(editTenantId).Then(outcome =>
            {
                if (requestVersion != dependencyVersion)
                    return;
                if (outcome.Ok)
                    posOptions = EnsureOption(UserRegWayClient.ReadPositions(outcome.Data, editTenantId), editPosId, editPosName);
                else
                    error = outcome.Error ?? "加载职位失败。";
                CompleteDependency(requestVersion);
            }, _ =>
            {
                if (requestVersion != dependencyVersion)
                    return;
                error = "加载职位失败，请检查网络后重试。";
                posOptions = EnsureOption([], editPosId, editPosName);
                CompleteDependency(requestVersion);
            });
        }

        private void CompleteDependency(int requestVersion)
        {
            if (requestVersion != dependencyVersion)
                return;
            dependencyPending--;
            if (dependencyPending <= 0)
                dependencyLoading = false;
        }

        private void Save()
        {
            if (saving || dependencyLoading)
                return;

            var normalizedName = editName.Trim();
            var normalizedRemark = editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                error = "方案名称不能为空。";
                return;
            }
            if (normalizedName.Length > 32)
            {
                error = "方案名称不能超过32个字符。";
                return;
            }
            if (editAccountType <= 0 || editAccountType == 888 || editAccountType == 999)
            {
                error = "请选择可注册的账户类型。";
                return;
            }
            if (editRoleId.Length == 0)
            {
                error = "绑定角色不能为空。";
                return;
            }
            if (editOrgId.Length == 0)
            {
                error = "绑定机构不能为空。";
                return;
            }
            if (editPosId.Length == 0)
            {
                error = "绑定职位不能为空。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                error = "备注不能超过128个字符。";
                return;
            }
            if (editTenantId.Length == 0)
            {
                error = "租户不能为空。";
                return;
            }

            var input = new UserRegWaySaveInput
            {
                id = editId,
                tenantId = editTenantId,
                name = normalizedName,
                accountType = editAccountType,
                roleId = editRoleId,
                orgId = editOrgId,
                posId = editPosId,
                orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                remark = normalizedRemark
            };

            saving = true;
            var request = editId.Length == 0 ? UserRegWayClient.Add(input) : UserRegWayClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存注册方案失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "注册方案已新增" : "注册方案已更新");
                LoadItems();
            }, _ =>
            {
                saving = false;
                error = "保存注册方案失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            UserRegWayClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除注册方案失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("注册方案已删除");
                LoadItems();
            }, _ => error = "删除注册方案失败，请检查网络后重试。");
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
            auditRemark = DisplayValue(row.remark);
            auditDialogVisible = true;
        }

        private UserRegWayView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private string AccountTypeLabel(UserRegWayView row)
        {
            var value = row.accountType;
            for (var index = 0; index < accountTypeOptions.Length; index++)
                if (accountTypeOptions[index].value == value)
                    return accountTypeOptions[index].label;
            return value > 0 ? "类型" + value : "-";
        }

        private string AccountTypeTag(UserRegWayView row)
        {
            var value = row.accountType;
            for (var index = 0; index < accountTypeOptions.Length; index++)
            {
                if (accountTypeOptions[index].value != value)
                    continue;
                var tag = accountTypeOptions[index].tagType;
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
                if (source[index].id == id)
                    return source;

            var result = new UserRegWayNamedOption[source.Length + 1];
            for (var index = 0; index < source.Length; index++)
                result[index] = source[index];
            result[source.Length] = new UserRegWayNamedOption
            {
                id = id,
                name = name.Length == 0 ? "历史选项 (" + id + ")" : name
            };
            return result;
        }

        private static UserRegWayAccountTypeOption[] DefaultAccountTypes()
            =>
            [
                new UserRegWayAccountTypeOption { value = 666, label = "会员", tagType = "success" },
                new UserRegWayAccountTypeOption { value = 777, label = "普通账号", tagType = "primary" }
            ];

        private static string TenantLabel(UserRegWayTenantOption tenant)
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

        private ConfigurationOption[] AccountOptions()
        {
            var options = new ConfigurationOption[accountTypeOptions.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(accountTypeOptions[index].value.ToString(), accountTypeOptions[index].label);
            return options;
        }

        private static ConfigurationOption[] NamedOptions(UserRegWayNamedOption[] items)
        {
            var options = new ConfigurationOption[items.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(items[index].id, items[index].name);
            return options;
        }

        private UserRegWayTableRow[] TableRows()
        {
            var result = new UserRegWayTableRow[rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = rows[index];
                result[index] = new UserRegWayTableRow(row.id, row.name, row.orgName, row.roleName, row.posName, row.orderNo,
                    AccountTypeLabel(row), AccountTypeTag(row));
            }
            return result;
        }

        public UserRegWayPageProps CreateProps() => new()
        {
            TenantOptions = TenantOptions(),
            TenantChanged = TenantChanged,
            TenantId = tenantId,
            EditTenantChanged = EditTenantChanged,
            EditTenantId = editTenantId,
            RoleOptions = NamedOptions(roleOptions),
            SetEditRoleId = value => editRoleId = value,
            EditRoleId = editRoleId,
            OrgOptions = NamedOptions(orgOptions),
            SetEditOrgId = value => editOrgId = value,
            EditOrgId = editOrgId,
            PosOptions = NamedOptions(posOptions),
            SetEditPosId = value => editPosId = value,
            EditPosId = editPosId,
            AccountTypeOptions = AccountOptions(),
            EditAccountType = editAccountType.ToString(),
            SetEditAccountType = value => editAccountType = (int)ECMAScript.Global.NumberValue(value),
            HasActionColumn = HasActionColumn,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            OpenAuditRow = OpenAudit,
            OpenEditRow = OpenEdit,
            DeleteRow = Delete,
            SortRows = SortRows,
            Rows = TableRows(),
            EditOrderNo = editOrderNo,
            SetEditOrderNo = value => editOrderNo = (int)(value ?? 100),
            SetKeyword = value => keyword = value ?? "",
            SetName = value => name = value ?? "",
            SetEditName = value => editName = value ?? "",
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
            Error = error,
            IsSuperAdmin = IsSuperAdmin,
            TenantLoading = tenantLoading,
            Keyword = keyword,
            Name = name,
            Loading = loading,
            CanAdd = CanAdd,
            EditDialogVisible = editDialogVisible,
            EditDialogTitle = editDialogTitle,
            Editing = editId.Length > 0,
            DependencyLoading = dependencyLoading,
            EditName = editName,
            EditRemark = editRemark,
            Saving = saving,
            AuditDialogVisible = auditDialogVisible,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            AuditRemark = auditRemark,
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
