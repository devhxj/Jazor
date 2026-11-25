using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Role;

[ECMAScriptModule("components/role-page-state.js")]
public static class RolePageModule
{
    public static RolePageState Create() => new();

    public sealed class RolePageState : global::Jero.Web.App.IPageState<RolePageProps>
    {
        private RoleView[] items = [];
        private RoleView[] rows = [];
        private RoleTenantOption[] tenantItems = [];
        private int currentPage = 1;
        private int pageSize = 20;
        private int total;
        private string tenantId = string.Empty;
        private string queryName = string.Empty;
        private string queryCode = string.Empty;
        private bool loading;
        private bool tenantLoading;
        private string statusLoadingId = string.Empty;
        private string deletingId = string.Empty;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增角色";
        private bool saving;
        private RoleView? editOriginal;
        private string editId = string.Empty;
        private string editTenantId = string.Empty;
        private string editName = string.Empty;
        private string editCode = string.Empty;
        private int editOrderNo = 100;
        private int editStatus = 1;
        private string editRemark = string.Empty;
        private RoleTreeNode[] menuNodes = [];
        private string[] editMenuIds = [];
        private string[] pendingMenuIds = [];
        private bool menuTreeLoading;
        private bool menuSelectionLoading;

        private bool dataDialogVisible;
        private bool dataSaving;
        private string dataRoleId = string.Empty;
        private string dataRoleName = string.Empty;
        private string dataTenantId = string.Empty;
        private int dataScope = 4;
        private RoleTreeNode[] orgNodes = [];
        private string[] dataOrgIds = [];
        private string orgFilter = string.Empty;
        private bool orgTreeLoading;
        private bool orgSelectionLoading;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";
        private string auditRemark = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysRole:add");
        private bool CanEdit => ApplicationState.HasPermission("sysRole:update");
        private bool CanDelete => ApplicationState.HasPermission("sysRole:delete");
        private bool CanGrantMenu => ApplicationState.HasPermission("sysRole:grantMenu");
        private bool CanGrantDataScope => ApplicationState.HasPermission("sysRole:grantDataScope");
        private bool CanSetStatus => ApplicationState.HasPermission("sysRole:setStatus");
        private bool HasActionColumn => CanGrantDataScope || CanEdit || CanDelete;
        private bool MenuBusy => menuTreeLoading || menuSelectionLoading;
        private bool OrgBusy => orgTreeLoading || orgSelectionLoading;
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
            RoleClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }

                tenantItems = RoleClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            loading = true;
            RoleClient.Page(new RolePageInput
            {
                page = currentPage,
                pageSize = pageSize,
                name = queryName.Trim(),
                code = queryCode.Trim(),
                tenantId = IsSuperAdmin && tenantId.Length > 0 ? tenantId : "0"
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载角色失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }

                items = RoleClient.ReadItems(outcome.Data);
                rows = items;
                total = RoleClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载角色失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            queryName = string.Empty;
            queryCode = string.Empty;
            tenantId = DefaultTenantId;
            currentPage = 1;
            LoadPage();
        }

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
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增角色");
                return;
            }

            editDialogTitle = "新增角色";
            editOriginal = null;
            editId = string.Empty;
            editTenantId = targetTenantId;
            editName = string.Empty;
            editCode = string.Empty;
            editOrderNo = 100;
            editStatus = 1;
            editRemark = string.Empty;
            editMenuIds = [];
            pendingMenuIds = [];
            menuNodes = [];
            editDialogVisible = true;
            LoadMenuAuthorization(false);
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            editDialogTitle = "编辑角色";
            editOriginal = row;
            editId = row.id;
            editTenantId = row.tenantId.Length == 0 ? DefaultTenantId : row.tenantId;
            editName = row.name;
            editCode = row.code;
            editOrderNo = row.orderNo;
            editStatus = row.status == 2 ? 2 : 1;
            editRemark = row.remark;
            editMenuIds = [];
            pendingMenuIds = [];
            menuNodes = [];
            editDialogVisible = true;
            LoadMenuAuthorization(true);
        }

        private void LoadMenuAuthorization(bool loadCurrentSelection)
        {
            menuTreeLoading = false;
            menuSelectionLoading = false;
            if (!CanGrantMenu)
                return;

            menuTreeLoading = true;
            RoleClient.MenuTree(editTenantId).Then(outcome =>
            {
                menuTreeLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载菜单权限失败。";
                    menuNodes = [];
                    return;
                }

                menuNodes = RoleClient.ReadMenuTree(outcome.Data);
                editMenuIds = ExpandMenuSelection(menuNodes, pendingMenuIds);
            }, _ =>
            {
                menuTreeLoading = false;
                error = "加载菜单权限失败，请检查网络后重试。";
            });

            if (!loadCurrentSelection || editId.Length == 0)
                return;

            menuSelectionLoading = true;
            RoleClient.OwnMenuList(editId).Then(outcome =>
            {
                menuSelectionLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载角色菜单失败。";
                    editMenuIds = [];
                    return;
                }

                pendingMenuIds = RoleClient.ReadIds(outcome.Data);
                editMenuIds = ExpandMenuSelection(menuNodes, pendingMenuIds);
            }, _ =>
            {
                menuSelectionLoading = false;
                error = "加载角色菜单失败，请检查网络后重试。";
            });
        }

        private void ToggleMenuNode(string id)
        {
            if (MenuBusy)
                return;
            var select = !ContainsId(editMenuIds, id);
            editMenuIds = SetMenuSubtree(menuNodes, editMenuIds, id, select);
        }

        private void Save()
        {
            if (saving || MenuBusy)
                return;

            var normalizedName = editName.Trim();
            var normalizedCode = editCode.Trim();
            var normalizedRemark = editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                error = "角色名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                error = "角色名称不能超过64个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                error = "角色编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 64)
            {
                error = "角色编码不能超过64个字符。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                error = "备注不能超过128个字符。";
                return;
            }
            if (IsSuperAdmin && editTenantId.Length == 0)
            {
                error = "超级管理员维护角色时必须选择租户。";
                return;
            }
            if (CanGrantMenu && editMenuIds.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请至少选择一个菜单权限");
                return;
            }

            saving = true;
            var menuIds = CanGrantMenu ? editMenuIds : null;
            IPromise<ApiOutcome> request;
            if (editId.Length == 0)
            {
                request = RoleClient.Add(new RoleAddInput
                {
                    tenantId = Optional(editTenantId),
                    name = normalizedName,
                    code = normalizedCode,
                    orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                    dataScope = 4,
                    remark = Optional(normalizedRemark),
                    status = editStatus == 2 ? 2 : 1,
                    menuIdList = menuIds
                });
            }
            else
            {
                var original = editOriginal;
                if (original is null)
                {
                    saving = false;
                    error = "当前角色不存在。";
                    return;
                }

                // UpdateRole adapts the input to SysRole; retain the selected entity's full scalar snapshot.
                request = RoleClient.Update(new RoleUpdateInput
                {
                    id = original.id,
                    tenantId = Optional(original.tenantId),
                    name = normalizedName,
                    code = normalizedCode,
                    orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                    dataScope = original.dataScope,
                    remark = Optional(normalizedRemark),
                    status = editStatus == 2 ? 2 : 1,
                    createTime = original.createTime,
                    createUserId = Optional(original.createUserId),
                    createUserName = Optional(original.createUserName),
                    updateTime = Optional(original.updateTime),
                    updateUserId = Optional(original.updateUserId),
                    updateUserName = Optional(original.updateUserName),
                    menuIdList = menuIds
                });
            }

            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存角色失败。";
                    return;
                }

                editDialogVisible = false;
                editOriginal = null;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "角色已新增" : "角色已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存角色失败，请检查网络后重试。";
            });
        }

        private void ToggleStatus(string id)
        {
            var row = Find(id);
            if (row is null || !CanSetStatus || statusLoadingId.Length > 0)
                return;

            var nextStatus = row.status == 1 ? 2 : 1;
            statusLoadingId = row.id;
            RoleClient.SetStatus(row.id, nextStatus).Then(outcome =>
            {
                statusLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "切换角色状态失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success(nextStatus == 1 ? "角色状态已切换为启用" : "角色状态已切换为停用");
                LoadPage();
            }, _ =>
            {
                statusLoadingId = string.Empty;
                error = "切换角色状态失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || deletingId.Length > 0)
                return;

            deletingId = row.id;
            RoleClient.Delete(row.id).Then(outcome =>
            {
                deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除角色失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("角色已删除");
                LoadPage();
            }, _ =>
            {
                deletingId = string.Empty;
                error = "删除角色失败，请检查网络后重试。";
            });
        }

        private void OpenGrantData(string id)
        {
            var row = Find(id);
            if (row is null || !CanGrantDataScope)
                return;

            dataRoleId = row.id;
            dataRoleName = row.name;
            dataTenantId = row.tenantId.Length == 0 ? DefaultTenantId : row.tenantId;
            dataScope = row.dataScope is >= 1 and <= 5 ? row.dataScope : 4;
            dataOrgIds = [];
            orgNodes = [];
            orgFilter = string.Empty;
            dataDialogVisible = true;
            LoadOrgAuthorization();
        }

        private void LoadOrgAuthorization()
        {
            orgTreeLoading = true;
            RoleClient.OrgTree(dataTenantId).Then(outcome =>
            {
                orgTreeLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载机构树失败。";
                    orgNodes = [];
                    return;
                }

                orgNodes = RoleClient.ReadOrgTree(outcome.Data);
            }, _ =>
            {
                orgTreeLoading = false;
                error = "加载机构树失败，请检查网络后重试。";
            });

            orgSelectionLoading = true;
            RoleClient.OwnOrgList(dataRoleId).Then(outcome =>
            {
                orgSelectionLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载角色机构范围失败。";
                    dataOrgIds = [];
                    return;
                }

                // The legacy OrgTree uses check-strictly, so each organization remains independent.
                dataOrgIds = RoleClient.ReadIds(outcome.Data);
            }, _ =>
            {
                orgSelectionLoading = false;
                error = "加载角色机构范围失败，请检查网络后重试。";
            });
        }

        private void ToggleOrgNode(string id)
        {
            if (OrgBusy)
                return;
            var node = FindTreeNode(orgNodes, id);
            if (node is null || node.disabled)
                return;
            dataOrgIds = ContainsId(dataOrgIds, id) ? RemoveId(dataOrgIds, id) : AddId(dataOrgIds, id);
        }

        private void SaveDataScope()
        {
            if (dataSaving || OrgBusy)
                return;
            if (dataRoleId.Length == 0)
            {
                error = "当前角色不存在。";
                return;
            }
            if (dataScope == 5 && dataOrgIds.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("自定义数据范围至少选择一个机构");
                return;
            }

            dataSaving = true;
            RoleClient.GrantDataScope(new RoleGrantDataInput
            {
                id = dataRoleId,
                dataScope = dataScope,
                orgIdList = dataScope == 5 ? dataOrgIds : []
            }).Then(outcome =>
            {
                dataSaving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "更新数据范围失败。";
                    return;
                }

                dataDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("数据范围已更新");
                LoadPage();
            }, _ =>
            {
                dataSaving = false;
                error = "更新数据范围失败，请检查网络后重试。";
            });
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

        private RoleView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static RoleTreeNode? FindTreeNode(RoleTreeNode[] nodes, string id)
        {
            for (var index = 0; index < nodes.Length; index++)
                if (nodes[index].id == id)
                    return nodes[index];
            return null;
        }

        private static string[] ExpandMenuSelection(RoleTreeNode[] nodes, string[] selected)
        {
            string[] result = [];
            for (var index = 0; index < selected.Length; index++)
                result = ApplyMenuSubtree(nodes, result, selected[index], true);
            return NormalizeMenuAncestors(nodes, result);
        }

        private static string[] SetMenuSubtree(RoleTreeNode[] nodes, string[] selected, string id, bool enabled)
            => NormalizeMenuAncestors(nodes, ApplyMenuSubtree(nodes, selected, id, enabled));

        private static string[] ApplyMenuSubtree(RoleTreeNode[] nodes, string[] selected, string id, bool enabled)
        {
            var result = selected;
            for (var index = 0; index < nodes.Length; index++)
            {
                var node = nodes[index];
                if (node.id != id && !IsDescendant(nodes, node, id))
                    continue;
                result = enabled ? AddId(result, node.id) : RemoveId(result, node.id);
            }
            return result;
        }

        private static string[] NormalizeMenuAncestors(RoleTreeNode[] nodes, string[] selected)
        {
            var result = selected;
            for (var index = nodes.Length - 1; index >= 0; index--)
            {
                var node = nodes[index];
                if (!node.hasChildren)
                    continue;

                var hasDirectChild = false;
                var allDirectChildrenSelected = true;
                for (var childIndex = 0; childIndex < nodes.Length; childIndex++)
                {
                    var child = nodes[childIndex];
                    if (child.parentId != node.id)
                        continue;
                    hasDirectChild = true;
                    if (!ContainsId(result, child.id))
                        allDirectChildrenSelected = false;
                }

                result = hasDirectChild && allDirectChildrenSelected
                    ? AddId(result, node.id)
                    : RemoveId(result, node.id);
            }
            return result;
        }

        private static bool IsDescendant(RoleTreeNode[] nodes, RoleTreeNode node, string ancestorId)
        {
            var parentId = node.parentId;
            while (parentId.Length > 0 && parentId != "0")
            {
                if (parentId == ancestorId)
                    return true;
                var parent = FindTreeNode(nodes, parentId);
                if (parent is null || parent.parentId == parentId)
                    return false;
                parentId = parent.parentId;
            }
            return false;
        }

        private static bool HasSelectedDescendant(RoleTreeNode[] nodes, string[] selected, string id)
        {
            for (var index = 0; index < nodes.Length; index++)
                if (ContainsId(selected, nodes[index].id) && IsDescendant(nodes, nodes[index], id))
                    return true;
            return false;
        }

        private static bool ContainsId(string[] values, string id)
        {
            for (var index = 0; index < values.Length; index++)
                if (values[index] == id)
                    return true;
            return false;
        }

        private static string[] AddId(string[] values, string id)
        {
            if (id.Length == 0 || ContainsId(values, id))
                return values;
            var result = new string[values.Length + 1];
            for (var index = 0; index < values.Length; index++)
                result[index] = values[index];
            result[values.Length] = id;
            return result;
        }

        private static string[] RemoveId(string[] values, string id)
        {
            if (!ContainsId(values, id))
                return values;
            var result = new string[values.Length - 1];
            var target = 0;
            for (var index = 0; index < values.Length; index++)
            {
                if (values[index] == id)
                    continue;
                result[target] = values[index];
                target++;
            }
            return result;
        }

        private bool OrgNodeVisible(RoleTreeNode node)
        {
            var filter = orgFilter.Trim();
            if (filter.Length == 0 || node.label.Contains(filter))
                return true;
            for (var index = 0; index < orgNodes.Length; index++)
                if (IsDescendant(orgNodes, orgNodes[index], node.id) && orgNodes[index].label.Contains(filter))
                    return true;
            return false;
        }

        private string MenuCheckboxClass(RoleTreeNode node)
        {
            if (ContainsId(editMenuIds, node.id))
                return "role-tree__check is-checked";
            if (HasSelectedDescendant(menuNodes, editMenuIds, node.id))
                return "role-tree__check is-partial";
            return "role-tree__check";
        }

        private string MenuCheckMark(RoleTreeNode node)
        {
            if (ContainsId(editMenuIds, node.id))
                return "✓";
            return HasSelectedDescendant(menuNodes, editMenuIds, node.id) ? "−" : string.Empty;
        }

        private string MenuAriaChecked(RoleTreeNode node)
        {
            if (ContainsId(editMenuIds, node.id))
                return "true";
            return HasSelectedDescendant(menuNodes, editMenuIds, node.id) ? "mixed" : "false";
        }

        private string OrgCheckboxClass(RoleTreeNode node)
            => ContainsId(dataOrgIds, node.id) ? "role-tree__check is-checked" : "role-tree__check";

        private string OrgCheckMark(RoleTreeNode node)
            => ContainsId(dataOrgIds, node.id) ? "✓" : string.Empty;

        private string OrgAriaChecked(RoleTreeNode node)
            => ContainsId(dataOrgIds, node.id) ? "true" : "false";

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

        public RolePageProps CreateProps() => new()
        {
            SetEditStatus = value => editStatus = (int)value,
            SetDataScope = value => dataScope = (int)value,
            SetEditOrderNo = value => editOrderNo = (int)value,
            SetQueryName = value => queryName = value,
            SetQueryCode = value => queryCode = value,
            SetEditDialogVisible = value => editDialogVisible = value,
            SetEditName = value => editName = value,
            SetEditCode = value => editCode = value,
            SetEditRemark = value => editRemark = value,
            SetDataDialogVisible = value => dataDialogVisible = value,
            SetOrgFilter = value => orgFilter = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            DismissError = () => error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPage = LoadPage,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            CancelEdit = () => editDialogVisible = false,
            Save = Save,
            CancelData = () => dataDialogVisible = false,
            SaveDataScope = SaveDataScope,
            OrgNodeVisible = OrgNodeVisible,
            MenuCheckboxClass = MenuCheckboxClass,
            MenuCheckMark = MenuCheckMark,
            MenuAriaChecked = MenuAriaChecked,
            OrgCheckboxClass = OrgCheckboxClass,
            OrgCheckMark = OrgCheckMark,
            OrgAriaChecked = OrgAriaChecked,
            ToggleMenuNode = ToggleMenuNode,
            ToggleOrgNode = ToggleOrgNode,
            TenantChanged = TenantChanged,
            ToggleStatus = ToggleStatus,
            OpenAudit = OpenAudit,
            OpenGrantData = OpenGrantData,
            OpenEdit = OpenEdit,
            Delete = Delete,
            Rows = rows,
            TenantItems = tenantItems,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            TenantId = tenantId,
            QueryName = queryName,
            QueryCode = queryCode,
            Loading = loading,
            TenantLoading = tenantLoading,
            StatusLoadingId = statusLoadingId,
            DeletingId = deletingId,
            Error = error,
            EditDialogVisible = editDialogVisible,
            EditDialogTitle = editDialogTitle,
            Saving = saving,
            EditId = editId,
            EditName = editName,
            EditCode = editCode,
            EditOrderNo = editOrderNo,
            EditStatus = editStatus,
            EditRemark = editRemark,
            MenuNodes = menuNodes,
            DataDialogVisible = dataDialogVisible,
            DataSaving = dataSaving,
            DataRoleName = dataRoleName,
            DataScope = dataScope,
            OrgNodes = orgNodes,
            OrgFilter = orgFilter,
            AuditDialogVisible = auditDialogVisible,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            AuditRemark = auditRemark,
            IsSuperAdmin = IsSuperAdmin,
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            CanGrantMenu = CanGrantMenu,
            CanGrantDataScope = CanGrantDataScope,
            CanSetStatus = CanSetStatus,
            HasActionColumn = HasActionColumn,
            MenuBusy = MenuBusy,
            OrgBusy = OrgBusy,
        };
    }
}

public sealed record RolePageProps : VueProps
{
    public required Action<double> SetEditStatus { get; init; }
    public required Action<double> SetDataScope { get; init; }
    public required Action<double> SetEditOrderNo { get; init; }
    public required Action<string> SetQueryName { get; init; }
    public required Action<string> SetQueryCode { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<string> SetEditName { get; init; }
    public required Action<string> SetEditCode { get; init; }
    public required Action<string> SetEditRemark { get; init; }
    public required Action<bool> SetDataDialogVisible { get; init; }
    public required Action<string> SetOrgFilter { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action DismissError { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action LoadPage { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action Save { get; init; }
    public required Action CancelData { get; init; }
    public required Action SaveDataScope { get; init; }
    public required Func<RoleTreeNode, bool> OrgNodeVisible { get; init; }
    public required Func<RoleTreeNode, string> MenuCheckboxClass { get; init; }
    public required Func<RoleTreeNode, string> MenuCheckMark { get; init; }
    public required Func<RoleTreeNode, string> MenuAriaChecked { get; init; }
    public required Func<RoleTreeNode, string> OrgCheckboxClass { get; init; }
    public required Func<RoleTreeNode, string> OrgCheckMark { get; init; }
    public required Func<RoleTreeNode, string> OrgAriaChecked { get; init; }
    public required Action<string> ToggleMenuNode { get; init; }
    public required Action<string> ToggleOrgNode { get; init; }
    public required Action<string> TenantChanged { get; init; }
    public required Action<string> ToggleStatus { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenGrantData { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Delete { get; init; }
    public required RoleView[] Rows { get; init; }
    public required RoleTenantOption[] TenantItems { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required string TenantId { get; init; }
    public required string QueryName { get; init; }
    public required string QueryCode { get; init; }
    public required bool Loading { get; init; }
    public required bool TenantLoading { get; init; }
    public required string StatusLoadingId { get; init; }
    public required string DeletingId { get; init; }
    public required string? Error { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required string EditDialogTitle { get; init; }
    public required bool Saving { get; init; }
    public required string EditId { get; init; }
    public required string EditName { get; init; }
    public required string EditCode { get; init; }
    public required int EditOrderNo { get; init; }
    public required int EditStatus { get; init; }
    public required string EditRemark { get; init; }
    public required RoleTreeNode[] MenuNodes { get; init; }
    public required bool DataDialogVisible { get; init; }
    public required bool DataSaving { get; init; }
    public required string DataRoleName { get; init; }
    public required int DataScope { get; init; }
    public required RoleTreeNode[] OrgNodes { get; init; }
    public required string OrgFilter { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required string AuditRemark { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool CanGrantMenu { get; init; }
    public required bool CanGrantDataScope { get; init; }
    public required bool CanSetStatus { get; init; }
    public required bool HasActionColumn { get; init; }
    public required bool MenuBusy { get; init; }
    public required bool OrgBusy { get; init; }
}
