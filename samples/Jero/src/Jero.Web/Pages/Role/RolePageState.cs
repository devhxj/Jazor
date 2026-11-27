using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Role;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Role;

[ECMAScriptModule("components/role-page-state.js")]
public static class RolePageModule
{
    public static RolePageState Create() => new();

    public sealed class RolePageState : global::Jero.Web.Composition.IPageState<RolePageProps>
    {
        private RoleView[] _items = [];
        private RoleView[] _rows = [];
        private RoleTenantOption[] _tenantItems = [];
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _total;
        private string _tenantId = string.Empty;
        private string _queryName = string.Empty;
        private string _queryCode = string.Empty;
        private bool _loading;
        private bool _tenantLoading;
        private string _statusLoadingId = string.Empty;
        private string _deletingId = string.Empty;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增角色";
        private bool _saving;
        private RoleView? _editOriginal;
        private string _editId = string.Empty;
        private string _editTenantId = string.Empty;
        private string _editName = string.Empty;
        private string _editCode = string.Empty;
        private int _editOrderNo = 100;
        private int _editStatus = 1;
        private string _editRemark = string.Empty;
        private RoleTreeNode[] _menuNodes = [];
        private string[] _editMenuIds = [];
        private string[] _pendingMenuIds = [];
        private bool _menuTreeLoading;
        private bool _menuSelectionLoading;

        private bool _dataDialogVisible;
        private bool _dataSaving;
        private string _dataRoleId = string.Empty;
        private string _dataRoleName = string.Empty;
        private string _dataTenantId = string.Empty;
        private int _dataScope = 4;
        private RoleTreeNode[] _orgNodes = [];
        private string[] _dataOrgIds = [];
        private string _orgFilter = string.Empty;
        private bool _orgTreeLoading;
        private bool _orgSelectionLoading;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";
        private string _auditRemark = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysRole:add");
        private bool CanEdit => ApplicationState.HasPermission("sysRole:update");
        private bool CanDelete => ApplicationState.HasPermission("sysRole:delete");
        private bool CanGrantMenu => ApplicationState.HasPermission("sysRole:grantMenu");
        private bool CanGrantDataScope => ApplicationState.HasPermission("sysRole:grantDataScope");
        private bool CanSetStatus => ApplicationState.HasPermission("sysRole:setStatus");
        private bool HasActionColumn => CanGrantDataScope || CanEdit || CanDelete;
        private bool MenuBusy => _menuTreeLoading || _menuSelectionLoading;
        private bool OrgBusy => _orgTreeLoading || _orgSelectionLoading;
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
            RoleClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }

                _tenantItems = RoleClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            _loading = true;
            RoleClient.Page(new RolePageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Name = _queryName.Trim(),
                Code = _queryCode.Trim(),
                TenantId = IsSuperAdmin && _tenantId.Length > 0 ? _tenantId : "0"
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载角色失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }

                _items = RoleClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = RoleClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载角色失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _queryName = string.Empty;
            _queryCode = string.Empty;
            _tenantId = DefaultTenantId;
            _currentPage = 1;
            LoadPage();
        }

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
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增角色");
                return;
            }

            _editDialogTitle = "新增角色";
            _editOriginal = null;
            _editId = string.Empty;
            _editTenantId = targetTenantId;
            _editName = string.Empty;
            _editCode = string.Empty;
            _editOrderNo = 100;
            _editStatus = 1;
            _editRemark = string.Empty;
            _editMenuIds = [];
            _pendingMenuIds = [];
            _menuNodes = [];
            _editDialogVisible = true;
            LoadMenuAuthorization(false);
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _editDialogTitle = "编辑角色";
            _editOriginal = row;
            _editId = row.Id;
            _editTenantId = row.TenantId.Length == 0 ? DefaultTenantId : row.TenantId;
            _editName = row.Name;
            _editCode = row.Code;
            _editOrderNo = row.OrderNo;
            _editStatus = row.Status == 2 ? 2 : 1;
            _editRemark = row.Remark;
            _editMenuIds = [];
            _pendingMenuIds = [];
            _menuNodes = [];
            _editDialogVisible = true;
            LoadMenuAuthorization(true);
        }

        private void LoadMenuAuthorization(bool loadCurrentSelection)
        {
            _menuTreeLoading = false;
            _menuSelectionLoading = false;
            if (!CanGrantMenu)
                return;

            _menuTreeLoading = true;
            RoleClient.MenuTree(_editTenantId).Then(outcome =>
            {
                _menuTreeLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载菜单权限失败。";
                    _menuNodes = [];
                    return;
                }

                _menuNodes = RoleClient.ReadMenuTree(outcome.Data);
                _editMenuIds = ExpandMenuSelection(_menuNodes, _pendingMenuIds);
            }, _ =>
            {
                _menuTreeLoading = false;
                _error = "加载菜单权限失败，请检查网络后重试。";
            });

            if (!loadCurrentSelection || _editId.Length == 0)
                return;

            _menuSelectionLoading = true;
            RoleClient.OwnMenuList(_editId).Then(outcome =>
            {
                _menuSelectionLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载角色菜单失败。";
                    _editMenuIds = [];
                    return;
                }

                _pendingMenuIds = RoleClient.ReadIds(outcome.Data);
                _editMenuIds = ExpandMenuSelection(_menuNodes, _pendingMenuIds);
            }, _ =>
            {
                _menuSelectionLoading = false;
                _error = "加载角色菜单失败，请检查网络后重试。";
            });
        }

        private void ToggleMenuNode(string id)
        {
            if (MenuBusy)
                return;
            var select = !ContainsId(_editMenuIds, id);
            _editMenuIds = SetMenuSubtree(_menuNodes, _editMenuIds, id, select);
        }

        private void Save()
        {
            if (_saving || MenuBusy)
                return;

            var normalizedName = _editName.Trim();
            var normalizedCode = _editCode.Trim();
            var normalizedRemark = _editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                _error = "角色名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                _error = "角色名称不能超过64个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                _error = "角色编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 64)
            {
                _error = "角色编码不能超过64个字符。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                _error = "备注不能超过128个字符。";
                return;
            }
            if (IsSuperAdmin && _editTenantId.Length == 0)
            {
                _error = "超级管理员维护角色时必须选择租户。";
                return;
            }
            if (CanGrantMenu && _editMenuIds.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请至少选择一个菜单权限");
                return;
            }

            _saving = true;
            var menuIds = CanGrantMenu ? _editMenuIds : null;
            IPromise<ApiOutcome> request;
            if (_editId.Length == 0)
            {
                request = RoleClient.Add(new RoleAddInput
                {
                    TenantId = Optional(_editTenantId),
                    Name = normalizedName,
                    Code = normalizedCode,
                    OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                    DataScope = 4,
                    Remark = Optional(normalizedRemark),
                    Status = _editStatus == 2 ? 2 : 1,
                    MenuIdList = menuIds
                });
            }
            else
            {
                var original = _editOriginal;
                if (original is null)
                {
                    _saving = false;
                    _error = "当前角色不存在。";
                    return;
                }

                // UpdateRole 将输入映射为 SysRole，提交时要保留所选实体的全部标量字段。
                request = RoleClient.Update(new RoleUpdateInput
                {
                    Id = original.Id,
                    TenantId = Optional(original.TenantId),
                    Name = normalizedName,
                    Code = normalizedCode,
                    OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                    DataScope = original.DataScope,
                    Remark = Optional(normalizedRemark),
                    Status = _editStatus == 2 ? 2 : 1,
                    CreateTime = original.CreateTime,
                    CreateUserId = Optional(original.CreateUserId),
                    CreateUserName = Optional(original.CreateUserName),
                    UpdateTime = Optional(original.UpdateTime),
                    UpdateUserId = Optional(original.UpdateUserId),
                    UpdateUserName = Optional(original.UpdateUserName),
                    MenuIdList = menuIds
                });
            }

            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存角色失败。";
                    return;
                }

                _editDialogVisible = false;
                _editOriginal = null;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "角色已新增" : "角色已更新");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存角色失败，请检查网络后重试。";
            });
        }

        private void ToggleStatus(string id)
        {
            var row = Find(id);
            if (row is null || !CanSetStatus || _statusLoadingId.Length > 0)
                return;

            var nextStatus = row.Status == 1 ? 2 : 1;
            _statusLoadingId = row.Id;
            RoleClient.SetStatus(row.Id, nextStatus).Then(outcome =>
            {
                _statusLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "切换角色状态失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success(nextStatus == 1 ? "角色状态已切换为启用" : "角色状态已切换为停用");
                LoadPage();
            }, _ =>
            {
                _statusLoadingId = string.Empty;
                _error = "切换角色状态失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || _deletingId.Length > 0)
                return;

            _deletingId = row.Id;
            RoleClient.Delete(row.Id).Then(outcome =>
            {
                _deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除角色失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("角色已删除");
                LoadPage();
            }, _ =>
            {
                _deletingId = string.Empty;
                _error = "删除角色失败，请检查网络后重试。";
            });
        }

        private void OpenGrantData(string id)
        {
            var row = Find(id);
            if (row is null || !CanGrantDataScope)
                return;

            _dataRoleId = row.Id;
            _dataRoleName = row.Name;
            _dataTenantId = row.TenantId.Length == 0 ? DefaultTenantId : row.TenantId;
            _dataScope = row.DataScope is >= 1 and <= 5 ? row.DataScope : 4;
            _dataOrgIds = [];
            _orgNodes = [];
            _orgFilter = string.Empty;
            _dataDialogVisible = true;
            LoadOrgAuthorization();
        }

        private void LoadOrgAuthorization()
        {
            _orgTreeLoading = true;
            RoleClient.OrgTree(_dataTenantId).Then(outcome =>
            {
                _orgTreeLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载机构树失败。";
                    _orgNodes = [];
                    return;
                }

                _orgNodes = RoleClient.ReadOrgTree(outcome.Data);
            }, _ =>
            {
                _orgTreeLoading = false;
                _error = "加载机构树失败，请检查网络后重试。";
            });

            _orgSelectionLoading = true;
            RoleClient.OwnOrgList(_dataRoleId).Then(outcome =>
            {
                _orgSelectionLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载角色机构范围失败。";
                    _dataOrgIds = [];
                    return;
                }

                // 各机构独立勾选，恢复服务端返回的机构 ID 时无需展开父子关系。
                _dataOrgIds = RoleClient.ReadIds(outcome.Data);
            }, _ =>
            {
                _orgSelectionLoading = false;
                _error = "加载角色机构范围失败，请检查网络后重试。";
            });
        }

        private void ToggleOrgNode(string id)
        {
            if (OrgBusy)
                return;
            var node = FindTreeNode(_orgNodes, id);
            if (node is null || node.Disabled)
                return;
            _dataOrgIds = ContainsId(_dataOrgIds, id) ? RemoveId(_dataOrgIds, id) : AddId(_dataOrgIds, id);
        }

        private void SaveDataScope()
        {
            if (_dataSaving || OrgBusy)
                return;
            if (_dataRoleId.Length == 0)
            {
                _error = "当前角色不存在。";
                return;
            }
            if (_dataScope == 5 && _dataOrgIds.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("自定义数据范围至少选择一个机构");
                return;
            }

            _dataSaving = true;
            RoleClient.GrantDataScope(new RoleGrantDataInput
            {
                Id = _dataRoleId,
                DataScope = _dataScope,
                OrgIdList = _dataScope == 5 ? _dataOrgIds : []
            }).Then(outcome =>
            {
                _dataSaving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "更新数据范围失败。";
                    return;
                }

                _dataDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success("数据范围已更新");
                LoadPage();
            }, _ =>
            {
                _dataSaving = false;
                _error = "更新数据范围失败，请检查网络后重试。";
            });
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

        private RoleView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static RoleTreeNode? FindTreeNode(RoleTreeNode[] nodes, string id)
        {
            for (var index = 0; index < nodes.Length; index++)
                if (nodes[index].Id == id)
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
                if (node.Id != id && !IsDescendant(nodes, node, id))
                    continue;
                result = enabled ? AddId(result, node.Id) : RemoveId(result, node.Id);
            }
            return result;
        }

        private static string[] NormalizeMenuAncestors(RoleTreeNode[] nodes, string[] selected)
        {
            var result = selected;
            for (var index = nodes.Length - 1; index >= 0; index--)
            {
                var node = nodes[index];
                if (!node.HasChildren)
                    continue;

                var hasDirectChild = false;
                var allDirectChildrenSelected = true;
                for (var childIndex = 0; childIndex < nodes.Length; childIndex++)
                {
                    var child = nodes[childIndex];
                    if (child.ParentId != node.Id)
                        continue;
                    hasDirectChild = true;
                    if (!ContainsId(result, child.Id))
                        allDirectChildrenSelected = false;
                }

                result = hasDirectChild && allDirectChildrenSelected
                    ? AddId(result, node.Id)
                    : RemoveId(result, node.Id);
            }
            return result;
        }

        private static bool IsDescendant(RoleTreeNode[] nodes, RoleTreeNode node, string ancestorId)
        {
            var parentId = node.ParentId;
            while (parentId.Length > 0 && parentId != "0")
            {
                if (parentId == ancestorId)
                    return true;
                var parent = FindTreeNode(nodes, parentId);
                if (parent is null || parent.ParentId == parentId)
                    return false;
                parentId = parent.ParentId;
            }
            return false;
        }

        private static bool HasSelectedDescendant(RoleTreeNode[] nodes, string[] selected, string id)
        {
            for (var index = 0; index < nodes.Length; index++)
                if (ContainsId(selected, nodes[index].Id) && IsDescendant(nodes, nodes[index], id))
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
            var filter = _orgFilter.Trim();
            if (filter.Length == 0 || node.Label.Contains(filter))
                return true;
            for (var index = 0; index < _orgNodes.Length; index++)
                if (IsDescendant(_orgNodes, _orgNodes[index], node.Id) && _orgNodes[index].Label.Contains(filter))
                    return true;
            return false;
        }

        private string MenuCheckboxClass(RoleTreeNode node)
        {
            if (ContainsId(_editMenuIds, node.Id))
                return "role-tree__check is-checked";
            if (HasSelectedDescendant(_menuNodes, _editMenuIds, node.Id))
                return "role-tree__check is-partial";
            return "role-tree__check";
        }

        private string MenuCheckMark(RoleTreeNode node)
        {
            if (ContainsId(_editMenuIds, node.Id))
                return "✓";
            return HasSelectedDescendant(_menuNodes, _editMenuIds, node.Id) ? "−" : string.Empty;
        }

        private string MenuAriaChecked(RoleTreeNode node)
        {
            if (ContainsId(_editMenuIds, node.Id))
                return "true";
            return HasSelectedDescendant(_menuNodes, _editMenuIds, node.Id) ? "mixed" : "false";
        }

        private string OrgCheckboxClass(RoleTreeNode node)
            => ContainsId(_dataOrgIds, node.Id) ? "role-tree__check is-checked" : "role-tree__check";

        private string OrgCheckMark(RoleTreeNode node)
            => ContainsId(_dataOrgIds, node.Id) ? "✓" : string.Empty;

        private string OrgAriaChecked(RoleTreeNode node)
            => ContainsId(_dataOrgIds, node.Id) ? "true" : "false";

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
            SetEditStatus = value => _editStatus = (int)value,
            SetDataScope = value => _dataScope = (int)value,
            SetEditOrderNo = value => _editOrderNo = (int)value,
            SetQueryName = value => _queryName = value,
            SetQueryCode = value => _queryCode = value,
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetEditName = value => _editName = value,
            SetEditCode = value => _editCode = value,
            SetEditRemark = value => _editRemark = value,
            SetDataDialogVisible = value => _dataDialogVisible = value,
            SetOrgFilter = value => _orgFilter = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            DismissError = () => _error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPage = LoadPage,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            CancelEdit = () => _editDialogVisible = false,
            Save = Save,
            CancelData = () => _dataDialogVisible = false,
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
            Rows = _rows,
            TenantItems = _tenantItems,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            TenantId = _tenantId,
            QueryName = _queryName,
            QueryCode = _queryCode,
            Loading = _loading,
            TenantLoading = _tenantLoading,
            StatusLoadingId = _statusLoadingId,
            DeletingId = _deletingId,
            Error = _error,
            EditDialogVisible = _editDialogVisible,
            EditDialogTitle = _editDialogTitle,
            Saving = _saving,
            EditId = _editId,
            EditName = _editName,
            EditCode = _editCode,
            EditOrderNo = _editOrderNo,
            EditStatus = _editStatus,
            EditRemark = _editRemark,
            MenuNodes = _menuNodes,
            DataDialogVisible = _dataDialogVisible,
            DataSaving = _dataSaving,
            DataRoleName = _dataRoleName,
            DataScope = _dataScope,
            OrgNodes = _orgNodes,
            OrgFilter = _orgFilter,
            AuditDialogVisible = _auditDialogVisible,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            AuditRemark = _auditRemark,
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
