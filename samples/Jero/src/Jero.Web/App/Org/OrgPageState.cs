using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Org;

[ECMAScriptModule("components/org-page-state.js")]
public static class OrgPageModule
{
    public static OrgPageState Create() => new();

    public sealed class OrgPageState : global::Jero.Web.App.IPageState<OrgPageProps>
    {
        private OrgView[] treeItems = [];
        private OrgFlatView[] treeRows = [];
        private OrgFlatView[] visibleTreeRows = [];
        private OrgView[] listItems = [];
        private OrgFlatView[] tableItems = [];
        private OrgFlatView[] rows = [];
        private OrgTenantOption[] tenantItems = [];
        private OrgTypeOption[] typeItems = [];
        private string[] expandedIds = [];
        private string treeFilter = string.Empty;
        private string selectedOrgId = "0";
        private string tenantId = string.Empty;
        private string name = string.Empty;
        private string code = string.Empty;
        private string type = string.Empty;
        private bool treeLoading;
        private bool listLoading;
        private bool tenantLoading;
        private int treeVersion;
        private int listVersion;
        private string deletingId = string.Empty;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增机构";
        private bool saving;
        private string editId = string.Empty;
        private string editTenantId = string.Empty;
        private string editPid = "0";
        private string editName = string.Empty;
        private string editCode = string.Empty;
        private int editLevel = 1;
        private string editType = string.Empty;
        private string editDirectorId = string.Empty;
        private int editOrderNo = 100;
        private int editStatus = 1;
        private string editRemark = string.Empty;
        private OrgParentOption[] parentOptions = [];
        private OrgFlatView? editOriginal;

        private bool detailDialogVisible;
        private string detailDialogTitle = "机构详情";
        private OrgFlatView? detailRow;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

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
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadTypes();
            RefreshAll();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            OrgClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }

                tenantItems = OrgClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadTypes()
        {
            OrgClient.Types().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载机构类型失败。";
                    typeItems = [];
                    return;
                }

                typeItems = OrgClient.ReadTypes(outcome.Data);
            }, _ => error = "加载机构类型失败，请检查网络后重试。");
        }

        private void LoadTree()
        {
            var requestVersion = ++treeVersion;
            var requestedTenant = IsSuperAdmin ? tenantId : DefaultTenantId;
            treeLoading = true;
            OrgClient.Tree(requestedTenant).Then(outcome =>
            {
                if (requestVersion != treeVersion)
                    return;
                treeLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载机构树失败。";
                    treeItems = [];
                    treeRows = [];
                    visibleTreeRows = [];
                    return;
                }

                treeItems = OrgClient.ReadOrganizations(outcome.Data);
                treeRows = Flatten(treeItems);
                ExpandAll();
                ApplyTreeFilter();
            }, _ =>
            {
                if (requestVersion != treeVersion)
                    return;
                treeLoading = false;
                error = "加载机构树失败，请检查网络后重试。";
            });
        }

        private void LoadList()
        {
            var requestVersion = ++listVersion;
            listLoading = true;
            OrgClient.List(new OrgListInput
            {
                id = selectedOrgId.Length == 0 ? "0" : selectedOrgId,
                name = name.Trim(),
                code = code.Trim(),
                type = type,
                tenantId = IsSuperAdmin ? tenantId : DefaultTenantId
            }).Then(outcome =>
            {
                if (requestVersion != listVersion)
                    return;
                listLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载机构列表失败。";
                    listItems = [];
                    tableItems = [];
                    rows = [];
                    return;
                }

                listItems = OrgClient.ReadOrganizations(outcome.Data);
                tableItems = Flatten(listItems);
                rows = tableItems;
            }, _ =>
            {
                if (requestVersion != listVersion)
                    return;
                listLoading = false;
                error = "加载机构列表失败，请检查网络后重试。";
            });
        }

        private void RefreshAll()
        {
            error = null;
            LoadTree();
            LoadList();
        }

        private void RefreshTree()
        {
            error = null;
            LoadTree();
        }

        private void Search()
        {
            error = null;
            LoadList();
        }

        private void ResetSearch()
        {
            name = string.Empty;
            code = string.Empty;
            type = string.Empty;
            selectedOrgId = "0";
            tenantId = DefaultTenantId;
            RefreshAll();
        }

        private void TenantChanged(string? value)
        {
            tenantId = value ?? string.Empty;
            selectedOrgId = "0";
            RefreshAll();
        }

        private void TreeFilterChanged(string value)
        {
            treeFilter = value;
            ApplyTreeFilter();
        }

        private void ApplyTreeFilter()
        {
            var filter = treeFilter.Trim().ToLowerInvariant();
            var count = 0;
            for (var index = 0; index < treeRows.Length; index++)
                if (IsTreeRowVisible(treeRows[index], filter))
                    count++;

            var result = new OrgFlatView[count];
            var target = 0;
            for (var index = 0; index < treeRows.Length; index++)
            {
                var row = treeRows[index];
                if (!IsTreeRowVisible(row, filter))
                    continue;
                result[target] = row;
                target++;
            }
            visibleTreeRows = result;
        }

        private bool IsTreeRowVisible(OrgFlatView row, string filter)
        {
            if (filter.Length > 0)
                return MatchesTreeFilter(row, filter) || HasMatchingDescendant(row.id, filter);

            for (var index = 0; index < row.ancestorIds.Length; index++)
                if (!ContainsId(expandedIds, row.ancestorIds[index]))
                    return false;
            return true;
        }

        private bool MatchesTreeFilter(OrgFlatView row, string filter)
            => row.name.ToLowerInvariant().Contains(filter);

        private bool HasMatchingDescendant(string id, string filter)
        {
            for (var index = 0; index < treeRows.Length; index++)
            {
                var candidate = treeRows[index];
                if (ContainsId(candidate.ancestorIds, id) && MatchesTreeFilter(candidate, filter))
                    return true;
            }
            return false;
        }

        private void ToggleTree(OrgFlatView row)
        {
            if (!row.hasChildren)
                return;

            if (ContainsId(expandedIds, row.id))
                expandedIds = RemoveId(expandedIds, row.id);
            else
                expandedIds = AppendId(expandedIds, row.id);
            ApplyTreeFilter();
        }

        private void ExpandAll()
        {
            var count = 0;
            for (var index = 0; index < treeRows.Length; index++)
                if (treeRows[index].hasChildren)
                    count++;

            var result = new string[count];
            var target = 0;
            for (var index = 0; index < treeRows.Length; index++)
            {
                if (!treeRows[index].hasChildren)
                    continue;
                result[target] = treeRows[index].id;
                target++;
            }
            expandedIds = result;
            ApplyTreeFilter();
        }

        private void CollapseAll()
        {
            expandedIds = [];
            ApplyTreeFilter();
        }

        private void SelectRoot()
        {
            selectedOrgId = "0";
            LoadList();
        }

        private void SelectTreeNode(OrgFlatView row)
        {
            if (row.disabled)
                return;
            selectedOrgId = row.id;
            LoadList();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增机构");
                return;
            }

            var parent = FindTree(selectedOrgId);
            editDialogTitle = parent is null ? "新增机构" : "新增下级机构";
            editId = string.Empty;
            editTenantId = targetTenantId;
            editPid = parent?.id ?? "0";
            editName = string.Empty;
            editCode = string.Empty;
            editLevel = parent is null ? 1 : parent.level + 1;
            editType = string.Empty;
            editDirectorId = string.Empty;
            editOrderNo = 100;
            editStatus = 1;
            editRemark = string.Empty;
            parentOptions = BuildParentOptions(string.Empty);
            editOriginal = null;
            editDialogVisible = true;
            error = null;
        }

        private void OpenCopy(string id)
        {
            var row = FindTable(id);
            if (row is null || !CanAdd)
                return;

            editDialogTitle = "复制机构";
            PopulateForm(row, true);
        }

        private void OpenEdit(string id)
        {
            var row = FindTable(id);
            if (row is null || !CanEdit)
                return;

            editDialogTitle = "编辑机构";
            PopulateForm(row, false);
        }

        private void PopulateForm(OrgFlatView row, bool copy)
        {
            editOriginal = copy ? null : row;
            editId = copy ? string.Empty : row.id;
            editTenantId = row.tenantId.Length > 0 ? row.tenantId : DefaultTenantId;
            editPid = row.pid.Length > 0 ? row.pid : "0";
            editName = copy ? string.Empty : row.name;
            editCode = row.code;
            editLevel = row.level < 1 ? 1 : row.level;
            editType = row.type;
            editDirectorId = row.directorId;
            editOrderNo = row.orderNo;
            editStatus = row.status == 2 ? 2 : 1;
            editRemark = row.remark;
            parentOptions = BuildParentOptions(editId);
            editDialogVisible = true;
            error = null;
        }

        private void ParentChanged(string? value)
        {
            editPid = value ?? "0";
            if (editPid.Length == 0)
                editPid = "0";
            var parent = FindTree(editPid);
            editLevel = parent is null ? 1 : parent.level + 1;
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
                error = "机构名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                error = "机构名称不能超过64个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                error = "机构编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 64)
            {
                error = "机构编码不能超过64个字符。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                error = "备注不能超过128个字符。";
                return;
            }
            if (IsSuperAdmin && editTenantId.Length == 0)
            {
                error = "超级管理员维护机构时必须选择租户。";
                return;
            }
            if (!IsSuperAdmin && (editPid.Length == 0 || editPid == "0"))
            {
                error = "请选择上级机构。";
                return;
            }
            if (!ParentAllowed(editPid))
            {
                error = "上级机构不能选择当前机构或其下级机构。";
                return;
            }
            if (editId.Length > 0 && editOriginal is null)
            {
                error = "原始机构快照不可用，请刷新列表后重试。";
                return;
            }

            var parent = FindTree(editPid);
            editLevel = parent is null ? 1 : parent.level + 1;
            var input = new OrgSaveInput
            {
                id = editId,
                tenantId = editTenantId,
                pid = editPid.Length == 0 ? "0" : editPid,
                name = normalizedName,
                code = normalizedCode,
                level = editLevel < 1 ? 1 : editLevel,
                type = editType,
                directorId = editDirectorId.Length == 0 ? null : editDirectorId,
                orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                status = editStatus == 2 ? 2 : 1,
                remark = normalizedRemark
            };

            saving = true;
            var request = editId.Length == 0
                ? OrgClient.Add(input)
                : OrgClient.Update(editOriginal!, input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存机构失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "机构已新增" : "机构已更新");
                RefreshAll();
            }, _ =>
            {
                saving = false;
                error = "保存机构失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = FindTable(id);
            if (row is null || !CanDelete || deletingId.Length > 0)
                return;

            deletingId = row.id;
            OrgClient.Delete(row.id).Then(outcome =>
            {
                deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除机构失败。";
                    return;
                }

                if (selectedOrgId == row.id)
                    selectedOrgId = "0";
                error = null;
                ElementPlus.ElMessageMembers.Success("机构及符合约束的下级机构已删除");
                RefreshAll();
            }, _ =>
            {
                deletingId = string.Empty;
                error = "删除机构失败，请检查网络后重试。";
            });
        }

        private void OpenDetail(string id)
        {
            var row = FindTable(id);
            if (row is null)
                return;
            detailRow = row;
            detailDialogTitle = "机构详情 · " + row.name;
            detailDialogVisible = true;
        }

        private void OpenAudit(string id)
        {
            var row = FindTable(id);
            if (row is null)
                return;

            auditDialogTitle = "修改记录 · " + row.name;
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = DisplayValue(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = DisplayValue(row.updateTime);
            auditDialogVisible = true;
        }

        private OrgParentOption[] BuildParentOptions(string currentId)
        {
            var count = IsSuperAdmin ? 1 : 0;
            for (var index = 0; index < treeRows.Length; index++)
                if (IsAllowedParent(treeRows[index], currentId, editTenantId))
                    count++;

            var result = new OrgParentOption[count];
            var target = 0;
            if (IsSuperAdmin)
            {
                result[target] = new OrgParentOption { id = "0", label = "顶级机构" };
                target++;
            }

            for (var index = 0; index < treeRows.Length; index++)
            {
                var row = treeRows[index];
                if (!IsAllowedParent(row, currentId, editTenantId))
                    continue;
                result[target] = new OrgParentOption
                {
                    id = row.id,
                    label = IndentLabel(row.depth, row.name)
                };
                target++;
            }
            return result;
        }

        private static bool IsAllowedParent(OrgFlatView row, string currentId, string targetTenantId)
        {
            if (row.disabled)
                return false;
            if (targetTenantId.Length > 0 && row.tenantId != targetTenantId)
                return false;
            if (currentId.Length == 0)
                return true;
            return row.id != currentId && !ContainsId(row.ancestorIds, currentId);
        }

        private bool ParentAllowed(string parentId)
        {
            if (parentId == "0")
                return IsSuperAdmin;
            for (var index = 0; index < parentOptions.Length; index++)
                if (parentOptions[index].id == parentId)
                    return true;
            return false;
        }

        private OrgFlatView? FindTree(string id)
        {
            for (var index = 0; index < treeRows.Length; index++)
                if (treeRows[index].id == id)
                    return treeRows[index];
            return null;
        }

        private OrgFlatView? FindTable(string id)
        {
            for (var index = 0; index < tableItems.Length; index++)
                if (tableItems[index].id == id)
                    return tableItems[index];
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
                count += CountNodes(nodes[index].children);
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
                    id = node.id,
                    tenantId = node.tenantId,
                    pid = node.pid,
                    name = node.name,
                    code = node.code,
                    level = node.level,
                    type = node.type,
                    directorId = node.directorId,
                    orderNo = node.orderNo,
                    status = node.status,
                    remark = node.remark,
                    disabled = node.disabled,
                    createTime = node.createTime,
                    createUserId = node.createUserId,
                    createUserName = node.createUserName,
                    updateTime = node.updateTime,
                    updateUserId = node.updateUserId,
                    updateUserName = node.updateUserName,
                    depth = depth,
                    hasChildren = node.children.Length > 0,
                    ancestorIds = ancestors
                };
                current++;
                current = FillRows(node.children, AppendId(ancestors, node.id), depth + 1, target, current);
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
            SetEditLevel = value => editLevel = (int)value,
            SetEditOrderNo = value => editOrderNo = (int)value,
            SetEditStatus = value => editStatus = (int)value,
            SetQueryName = value => name = value,
            SetQueryCode = value => code = value,
            SetQueryType = value => type = value,
            SetEditType = value => editType = value,
            SetEditDialogVisible = value => editDialogVisible = value,
            SetEditName = value => editName = value,
            SetEditCode = value => editCode = value,
            SetEditRemark = value => editRemark = value,
            SetDetailDialogVisible = value => detailDialogVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            DismissError = () => error = null,
            RefreshTree = RefreshTree,
            ExpandAll = ExpandAll,
            CollapseAll = CollapseAll,
            SelectRoot = SelectRoot,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            RefreshAll = RefreshAll,
            CancelEdit = () => editDialogVisible = false,
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
            VisibleTreeRows = visibleTreeRows,
            Rows = rows,
            TenantItems = tenantItems,
            TypeItems = typeItems,
            ExpandedIds = expandedIds,
            TreeFilter = treeFilter,
            SelectedOrgId = selectedOrgId,
            TenantId = tenantId,
            QueryName = name,
            QueryCode = code,
            QueryType = type,
            TreeLoading = treeLoading,
            ListLoading = listLoading,
            TenantLoading = tenantLoading,
            DeletingId = deletingId,
            Error = error,
            EditDialogVisible = editDialogVisible,
            EditDialogTitle = editDialogTitle,
            Saving = saving,
            EditPid = editPid,
            EditName = editName,
            EditCode = editCode,
            EditLevel = editLevel,
            EditType = editType,
            EditOrderNo = editOrderNo,
            EditStatus = editStatus,
            EditRemark = editRemark,
            ParentOptions = parentOptions,
            DetailDialogVisible = detailDialogVisible,
            DetailDialogTitle = detailDialogTitle,
            DetailRow = detailRow,
            AuditDialogVisible = auditDialogVisible,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
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
