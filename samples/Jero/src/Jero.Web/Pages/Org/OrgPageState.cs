using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Org;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Org;

[ECMAScriptModule("components/org-page-state.js")]
public static class OrgPageModule
{
    public static OrgPageState Create() => new();

    public sealed class OrgPageState : global::Jero.Web.Composition.IPageState<OrgPageProps>
    {
        private OrgView[] _treeItems = [];
        private OrgFlatView[] _treeRows = [];
        private OrgFlatView[] _visibleTreeRows = [];
        private OrgView[] _listItems = [];
        private OrgFlatView[] _tableItems = [];
        private OrgFlatView[] _rows = [];
        private OrgTenantOption[] _tenantItems = [];
        private OrgTypeOption[] _typeItems = [];
        private string[] _expandedIds = [];
        private string _treeFilter = string.Empty;
        private string _selectedOrgId = "0";
        private string _tenantId = string.Empty;
        private string _name = string.Empty;
        private string _code = string.Empty;
        private string _type = string.Empty;
        private bool _treeLoading;
        private bool _listLoading;
        private bool _tenantLoading;
        private int _treeVersion;
        private int _listVersion;
        private string _deletingId = string.Empty;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增机构";
        private bool _saving;
        private string _editId = string.Empty;
        private string _editTenantId = string.Empty;
        private string _editPid = "0";
        private string _editName = string.Empty;
        private string _editCode = string.Empty;
        private int _editLevel = 1;
        private string _editType = string.Empty;
        private string _editDirectorId = string.Empty;
        private int _editOrderNo = 100;
        private int _editStatus = 1;
        private string _editRemark = string.Empty;
        private OrgParentOption[] _parentOptions = [];
        private OrgFlatView? _editOriginal;

        private bool _detailDialogVisible;
        private string _detailDialogTitle = "机构详情";
        private OrgFlatView? _detailRow;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysOrg:add");
        private bool CanEdit => ApplicationState.HasPermission("sysOrg:update");
        private bool CanDelete => ApplicationState.HasPermission("sysOrg:delete");
        private bool HasActionColumn => CanAdd || CanEdit || CanDelete;
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            _tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadTypes();
            RefreshAll();
        }

        private void LoadTenants()
        {
            _tenantLoading = true;
            OrgClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }

                _tenantItems = OrgClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadTypes()
        {
            OrgClient.Types().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载机构类型失败。";
                    _typeItems = [];
                    return;
                }

                _typeItems = OrgClient.ReadTypes(outcome.Data);
            }, _ => _error = "加载机构类型失败，请检查网络后重试。");
        }

        private void LoadTree()
        {
            var requestVersion = ++_treeVersion;
            var requestedTenant = IsSuperAdmin ? _tenantId : DefaultTenantId;
            _treeLoading = true;
            OrgClient.Tree(requestedTenant).Then(outcome =>
            {
                if (requestVersion != _treeVersion)
                    return;
                _treeLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载机构树失败。";
                    _treeItems = [];
                    _treeRows = [];
                    _visibleTreeRows = [];
                    return;
                }

                _treeItems = OrgClient.ReadOrganizations(outcome.Data);
                _treeRows = Flatten(_treeItems);
                ExpandAll();
                ApplyTreeFilter();
            }, _ =>
            {
                if (requestVersion != _treeVersion)
                    return;
                _treeLoading = false;
                _error = "加载机构树失败，请检查网络后重试。";
            });
        }

        private void LoadList()
        {
            var requestVersion = ++_listVersion;
            _listLoading = true;
            OrgClient.List(new OrgListInput
            {
                Id = _selectedOrgId.Length == 0 ? "0" : _selectedOrgId,
                Name = _name.Trim(),
                Code = _code.Trim(),
                Type = _type,
                TenantId = IsSuperAdmin ? _tenantId : DefaultTenantId
            }).Then(outcome =>
            {
                if (requestVersion != _listVersion)
                    return;
                _listLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载机构列表失败。";
                    _listItems = [];
                    _tableItems = [];
                    _rows = [];
                    return;
                }

                _listItems = OrgClient.ReadOrganizations(outcome.Data);
                _tableItems = Flatten(_listItems);
                _rows = _tableItems;
            }, _ =>
            {
                if (requestVersion != _listVersion)
                    return;
                _listLoading = false;
                _error = "加载机构列表失败，请检查网络后重试。";
            });
        }

        private void RefreshAll()
        {
            _error = null;
            LoadTree();
            LoadList();
        }

        private void RefreshTree()
        {
            _error = null;
            LoadTree();
        }

        private void Search()
        {
            _error = null;
            LoadList();
        }

        private void ResetSearch()
        {
            _name = string.Empty;
            _code = string.Empty;
            _type = string.Empty;
            _selectedOrgId = "0";
            _tenantId = DefaultTenantId;
            RefreshAll();
        }

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
            _selectedOrgId = "0";
            RefreshAll();
        }

        private void TreeFilterChanged(string value)
        {
            _treeFilter = value;
            ApplyTreeFilter();
        }

        private void ApplyTreeFilter()
        {
            var filter = _treeFilter.Trim().ToLowerInvariant();
            var count = 0;
            for (var index = 0; index < _treeRows.Length; index++)
                if (IsTreeRowVisible(_treeRows[index], filter))
                    count++;

            var result = new OrgFlatView[count];
            var target = 0;
            for (var index = 0; index < _treeRows.Length; index++)
            {
                var row = _treeRows[index];
                if (!IsTreeRowVisible(row, filter))
                    continue;
                result[target] = row;
                target++;
            }
            _visibleTreeRows = result;
        }

        private bool IsTreeRowVisible(OrgFlatView row, string filter)
        {
            if (filter.Length > 0)
                return MatchesTreeFilter(row, filter) || HasMatchingDescendant(row.Id, filter);

            for (var index = 0; index < row.AncestorIds.Length; index++)
                if (!ContainsId(_expandedIds, row.AncestorIds[index]))
                    return false;
            return true;
        }

        private bool MatchesTreeFilter(OrgFlatView row, string filter)
            => row.Name.ToLowerInvariant().Contains(filter);

        private bool HasMatchingDescendant(string id, string filter)
        {
            for (var index = 0; index < _treeRows.Length; index++)
            {
                var candidate = _treeRows[index];
                if (ContainsId(candidate.AncestorIds, id) && MatchesTreeFilter(candidate, filter))
                    return true;
            }
            return false;
        }

        private void ToggleTree(OrgFlatView row)
        {
            if (!row.HasChildren)
                return;

            if (ContainsId(_expandedIds, row.Id))
                _expandedIds = RemoveId(_expandedIds, row.Id);
            else
                _expandedIds = AppendId(_expandedIds, row.Id);
            ApplyTreeFilter();
        }

        private void ExpandAll()
        {
            var count = 0;
            for (var index = 0; index < _treeRows.Length; index++)
                if (_treeRows[index].HasChildren)
                    count++;

            var result = new string[count];
            var target = 0;
            for (var index = 0; index < _treeRows.Length; index++)
            {
                if (!_treeRows[index].HasChildren)
                    continue;
                result[target] = _treeRows[index].Id;
                target++;
            }
            _expandedIds = result;
            ApplyTreeFilter();
        }

        private void CollapseAll()
        {
            _expandedIds = [];
            ApplyTreeFilter();
        }

        private void SelectRoot()
        {
            _selectedOrgId = "0";
            LoadList();
        }

        private void SelectTreeNode(OrgFlatView row)
        {
            if (row.Disabled)
                return;
            _selectedOrgId = row.Id;
            LoadList();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? _tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增机构");
                return;
            }

            var parent = FindTree(_selectedOrgId);
            _editDialogTitle = parent is null ? "新增机构" : "新增下级机构";
            _editId = string.Empty;
            _editTenantId = targetTenantId;
            _editPid = parent?.Id ?? "0";
            _editName = string.Empty;
            _editCode = string.Empty;
            _editLevel = parent is null ? 1 : parent.Level + 1;
            _editType = string.Empty;
            _editDirectorId = string.Empty;
            _editOrderNo = 100;
            _editStatus = 1;
            _editRemark = string.Empty;
            _parentOptions = BuildParentOptions(string.Empty);
            _editOriginal = null;
            _editDialogVisible = true;
            _error = null;
        }

        private void OpenCopy(string id)
        {
            var row = FindTable(id);
            if (row is null || !CanAdd)
                return;

            _editDialogTitle = "复制机构";
            PopulateForm(row, true);
        }

        private void OpenEdit(string id)
        {
            var row = FindTable(id);
            if (row is null || !CanEdit)
                return;

            _editDialogTitle = "编辑机构";
            PopulateForm(row, false);
        }

        private void PopulateForm(OrgFlatView row, bool copy)
        {
            _editOriginal = copy ? null : row;
            _editId = copy ? string.Empty : row.Id;
            _editTenantId = row.TenantId.Length > 0 ? row.TenantId : DefaultTenantId;
            _editPid = row.Pid.Length > 0 ? row.Pid : "0";
            _editName = copy ? string.Empty : row.Name;
            _editCode = row.Code;
            _editLevel = row.Level < 1 ? 1 : row.Level;
            _editType = row.Type;
            _editDirectorId = row.DirectorId;
            _editOrderNo = row.OrderNo;
            _editStatus = row.Status == 2 ? 2 : 1;
            _editRemark = row.Remark;
            _parentOptions = BuildParentOptions(_editId);
            _editDialogVisible = true;
            _error = null;
        }

        private void ParentChanged(string? value)
        {
            _editPid = value ?? "0";
            if (_editPid.Length == 0)
                _editPid = "0";
            var parent = FindTree(_editPid);
            _editLevel = parent is null ? 1 : parent.Level + 1;
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
                _error = "机构名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                _error = "机构名称不能超过64个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                _error = "机构编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 64)
            {
                _error = "机构编码不能超过64个字符。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                _error = "备注不能超过128个字符。";
                return;
            }
            if (IsSuperAdmin && _editTenantId.Length == 0)
            {
                _error = "超级管理员维护机构时必须选择租户。";
                return;
            }
            if (!IsSuperAdmin && (_editPid.Length == 0 || _editPid == "0"))
            {
                _error = "请选择上级机构。";
                return;
            }
            if (!ParentAllowed(_editPid))
            {
                _error = "上级机构不能选择当前机构或其下级机构。";
                return;
            }
            if (_editId.Length > 0 && _editOriginal is null)
            {
                _error = "原始机构快照不可用，请刷新列表后重试。";
                return;
            }

            var parent = FindTree(_editPid);
            _editLevel = parent is null ? 1 : parent.Level + 1;
            var input = new OrgSaveInput
            {
                Id = _editId,
                TenantId = _editTenantId,
                Pid = _editPid.Length == 0 ? "0" : _editPid,
                Name = normalizedName,
                Code = normalizedCode,
                Level = _editLevel < 1 ? 1 : _editLevel,
                Type = _editType,
                DirectorId = _editDirectorId.Length == 0 ? null : _editDirectorId,
                OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                Status = _editStatus == 2 ? 2 : 1,
                Remark = normalizedRemark
            };

            _saving = true;
            var request = _editId.Length == 0
                ? OrgClient.Add(input)
                : OrgClient.Update(_editOriginal!, input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存机构失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "机构已新增" : "机构已更新");
                RefreshAll();
            }, _ =>
            {
                _saving = false;
                _error = "保存机构失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = FindTable(id);
            if (row is null || !CanDelete || _deletingId.Length > 0)
                return;

            _deletingId = row.Id;
            OrgClient.Delete(row.Id).Then(outcome =>
            {
                _deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除机构失败。";
                    return;
                }

                if (_selectedOrgId == row.Id)
                    _selectedOrgId = "0";
                _error = null;
                ElementPlus.ElMessageMembers.Success("机构及符合约束的下级机构已删除");
                RefreshAll();
            }, _ =>
            {
                _deletingId = string.Empty;
                _error = "删除机构失败，请检查网络后重试。";
            });
        }

        private void OpenDetail(string id)
        {
            var row = FindTable(id);
            if (row is null)
                return;
            _detailRow = row;
            _detailDialogTitle = "机构详情 · " + row.Name;
            _detailDialogVisible = true;
        }

        private void OpenAudit(string id)
        {
            var row = FindTable(id);
            if (row is null)
                return;

            _auditDialogTitle = "修改记录 · " + row.Name;
            _auditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            _auditCreateTime = DisplayValue(row.CreateTime);
            _auditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            _auditUpdateTime = DisplayValue(row.UpdateTime);
            _auditDialogVisible = true;
        }

        private OrgParentOption[] BuildParentOptions(string currentId)
        {
            var count = IsSuperAdmin ? 1 : 0;
            for (var index = 0; index < _treeRows.Length; index++)
                if (IsAllowedParent(_treeRows[index], currentId, _editTenantId))
                    count++;

            var result = new OrgParentOption[count];
            var target = 0;
            if (IsSuperAdmin)
            {
                result[target] = new OrgParentOption { Id = "0", Label = "顶级机构" };
                target++;
            }

            for (var index = 0; index < _treeRows.Length; index++)
            {
                var row = _treeRows[index];
                if (!IsAllowedParent(row, currentId, _editTenantId))
                    continue;
                result[target] = new OrgParentOption
                {
                    Id = row.Id,
                    Label = IndentLabel(row.Depth, row.Name)
                };
                target++;
            }
            return result;
        }

        private static bool IsAllowedParent(OrgFlatView row, string currentId, string targetTenantId)
        {
            if (row.Disabled)
                return false;
            if (targetTenantId.Length > 0 && row.TenantId != targetTenantId)
                return false;
            if (currentId.Length == 0)
                return true;
            return row.Id != currentId && !ContainsId(row.AncestorIds, currentId);
        }

        private bool ParentAllowed(string parentId)
        {
            if (parentId == "0")
                return IsSuperAdmin;
            for (var index = 0; index < _parentOptions.Length; index++)
                if (_parentOptions[index].Id == parentId)
                    return true;
            return false;
        }

        private OrgFlatView? FindTree(string id)
        {
            for (var index = 0; index < _treeRows.Length; index++)
                if (_treeRows[index].Id == id)
                    return _treeRows[index];
            return null;
        }

        private OrgFlatView? FindTable(string id)
        {
            for (var index = 0; index < _tableItems.Length; index++)
                if (_tableItems[index].Id == id)
                    return _tableItems[index];
            return null;
        }

        private static OrgFlatView[] Flatten(OrgView[] roots)
        {
            var result = new OrgFlatView[CountNodes(roots)];
            FillRows(roots, [], 0, result, 0);
            return result;
        }

        private static int CountNodes(OrgView[] nodes)
        {
            var count = nodes.Length;
            for (var index = 0; index < nodes.Length; index++)
                count += CountNodes(nodes[index].Children);
            return count;
        }

        private static int FillRows(
            OrgView[] nodes,
            string[] ancestors,
            int depth,
            OrgFlatView[] target,
            int offset)
        {
            var current = offset;
            for (var index = 0; index < nodes.Length; index++)
            {
                var node = nodes[index];
                target[current] = new OrgFlatView
                {
                    Id = node.Id,
                    TenantId = node.TenantId,
                    Pid = node.Pid,
                    Name = node.Name,
                    Code = node.Code,
                    Level = node.Level,
                    Type = node.Type,
                    DirectorId = node.DirectorId,
                    OrderNo = node.OrderNo,
                    Status = node.Status,
                    Remark = node.Remark,
                    Disabled = node.Disabled,
                    CreateTime = node.CreateTime,
                    CreateUserId = node.CreateUserId,
                    CreateUserName = node.CreateUserName,
                    UpdateTime = node.UpdateTime,
                    UpdateUserId = node.UpdateUserId,
                    UpdateUserName = node.UpdateUserName,
                    Depth = depth,
                    HasChildren = node.Children.Length > 0,
                    AncestorIds = ancestors
                };
                current++;
                current = FillRows(node.Children, AppendId(ancestors, node.Id), depth + 1, target, current);
            }
            return current;
        }

        private static string[] AppendId(string[] values, string id)
        {
            var result = new string[values.Length + 1];
            for (var index = 0; index < values.Length; index++)
                result[index] = values[index];
            result[values.Length] = id;
            return result;
        }

        private static string[] RemoveId(string[] values, string id)
        {
            var count = 0;
            for (var index = 0; index < values.Length; index++)
                if (values[index] != id)
                    count++;

            var result = new string[count];
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

        private static bool ContainsId(string[] values, string id)
        {
            for (var index = 0; index < values.Length; index++)
                if (values[index] == id)
                    return true;
            return false;
        }

        private static string IndentLabel(int depth, string name)
        {
            var prefix = string.Empty;
            for (var index = 0; index < depth; index++)
                prefix += "— ";
            return prefix + name;
        }

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

        public OrgPageProps CreateProps() => new()
        {
            SetEditLevel = value => _editLevel = (int)value,
            SetEditOrderNo = value => _editOrderNo = (int)value,
            SetEditStatus = value => _editStatus = (int)value,
            SetQueryName = value => _name = value,
            SetQueryCode = value => _code = value,
            SetQueryType = value => _type = value,
            SetEditType = value => _editType = value,
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetEditName = value => _editName = value,
            SetEditCode = value => _editCode = value,
            SetEditRemark = value => _editRemark = value,
            SetDetailDialogVisible = value => _detailDialogVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            DismissError = () => _error = null,
            RefreshTree = RefreshTree,
            ExpandAll = ExpandAll,
            CollapseAll = CollapseAll,
            SelectRoot = SelectRoot,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            RefreshAll = RefreshAll,
            CancelEdit = () => _editDialogVisible = false,
            Save = Save,
            TenantChanged = TenantChanged,
            ParentChanged = ParentChanged,
            TreeFilterChanged = TreeFilterChanged,
            OpenDetail = OpenDetail,
            OpenAudit = OpenAudit,
            OpenCopy = OpenCopy,
            OpenEdit = OpenEdit,
            Delete = Delete,
            ToggleTree = ToggleTree,
            SelectTreeNode = SelectTreeNode,
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            HasActionColumn = HasActionColumn,
            IsSuperAdmin = IsSuperAdmin,
            VisibleTreeRows = _visibleTreeRows,
            Rows = _rows,
            TenantItems = _tenantItems,
            TypeItems = _typeItems,
            ExpandedIds = _expandedIds,
            TreeFilter = _treeFilter,
            SelectedOrgId = _selectedOrgId,
            TenantId = _tenantId,
            QueryName = _name,
            QueryCode = _code,
            QueryType = _type,
            TreeLoading = _treeLoading,
            ListLoading = _listLoading,
            TenantLoading = _tenantLoading,
            DeletingId = _deletingId,
            Error = _error,
            EditDialogVisible = _editDialogVisible,
            EditDialogTitle = _editDialogTitle,
            Saving = _saving,
            EditPid = _editPid,
            EditName = _editName,
            EditCode = _editCode,
            EditLevel = _editLevel,
            EditType = _editType,
            EditOrderNo = _editOrderNo,
            EditStatus = _editStatus,
            EditRemark = _editRemark,
            ParentOptions = _parentOptions,
            DetailDialogVisible = _detailDialogVisible,
            DetailDialogTitle = _detailDialogTitle,
            DetailRow = _detailRow,
            AuditDialogVisible = _auditDialogVisible,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
        };
    }
}

public sealed record OrgPageProps : VueProps
{
    public required Action<double> SetEditLevel { get; init; }
    public required Action<double> SetEditOrderNo { get; init; }
    public required Action<double> SetEditStatus { get; init; }
    public required Action<string> SetQueryName { get; init; }
    public required Action<string> SetQueryCode { get; init; }
    public required Action<string> SetQueryType { get; init; }
    public required Action<string> SetEditType { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<string> SetEditName { get; init; }
    public required Action<string> SetEditCode { get; init; }
    public required Action<string> SetEditRemark { get; init; }
    public required Action<bool> SetDetailDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action DismissError { get; init; }
    public required Action RefreshTree { get; init; }
    public required Action ExpandAll { get; init; }
    public required Action CollapseAll { get; init; }
    public required Action SelectRoot { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action RefreshAll { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action Save { get; init; }
    public required Action<string> TenantChanged { get; init; }
    public required Action<string> ParentChanged { get; init; }
    public required Action<string> TreeFilterChanged { get; init; }
    public required Action<string> OpenDetail { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenCopy { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<OrgFlatView> ToggleTree { get; init; }
    public required Action<OrgFlatView> SelectTreeNode { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool HasActionColumn { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required OrgFlatView[] VisibleTreeRows { get; init; }
    public required OrgFlatView[] Rows { get; init; }
    public required OrgTenantOption[] TenantItems { get; init; }
    public required OrgTypeOption[] TypeItems { get; init; }
    public required string[] ExpandedIds { get; init; }
    public required string TreeFilter { get; init; }
    public required string SelectedOrgId { get; init; }
    public required string TenantId { get; init; }
    public required string QueryName { get; init; }
    public required string QueryCode { get; init; }
    public required string QueryType { get; init; }
    public required bool TreeLoading { get; init; }
    public required bool ListLoading { get; init; }
    public required bool TenantLoading { get; init; }
    public required string DeletingId { get; init; }
    public required string? Error { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required string EditDialogTitle { get; init; }
    public required bool Saving { get; init; }
    public required string EditPid { get; init; }
    public required string EditName { get; init; }
    public required string EditCode { get; init; }
    public required int EditLevel { get; init; }
    public required string EditType { get; init; }
    public required int EditOrderNo { get; init; }
    public required int EditStatus { get; init; }
    public required string EditRemark { get; init; }
    public required OrgParentOption[] ParentOptions { get; init; }
    public required bool DetailDialogVisible { get; init; }
    public required string DetailDialogTitle { get; init; }
    public required OrgFlatView? DetailRow { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
}
