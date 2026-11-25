using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Region;

[ECMAScriptModule("components/region-page-state.js")]
public static class RegionPageModule
{
    public static RegionPageState Create() => new();

    public sealed class RegionPageState : global::Jero.Web.App.IPageState<RegionPageProps>
    {
        private readonly double[] JoltPageSizes = [10, 20, 50, 100];

        private RegionFlatView[] treeRows = [];
        private RegionFlatView[] visibleTreeRows = [];
        private string[] expandedIds = [];
        private string treeFilter = string.Empty;
        private string selectedRegionId = "0";
        private string treeExpandingId = string.Empty;
        private bool treeLoading;
        private int treeVersion;

        private RegionView[] pageItems = [];
        private RegionFlatView[] tableItems = [];
        private RegionFlatView[] rows = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string queryName = string.Empty;
        private string queryCode = string.Empty;
        private bool listLoading;
        private int listVersion;
        private string deletingId = string.Empty;
        private bool syncing;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增行政区域";
        private string editParentNamePath = "顶级";
        private bool saving;
        private string editId = string.Empty;
        private string editPid = "0";
        private string editName = string.Empty;
        private string editCode = string.Empty;
        private string editCityCode = string.Empty;
        private int editOrderNo = 100;
        private string editRemark = string.Empty;
        private RegionView? editOriginal;

        private bool CanAdd => ApplicationState.HasPermission("sysRegion:add");
        private bool CanEdit => ApplicationState.HasPermission("sysRegion:update");
        private bool CanDelete => ApplicationState.HasPermission("sysRegion:delete");
        private bool CanSync => ApplicationState.HasPermission("sysRegion:sync");
        private bool HasActionColumn => CanEdit || CanDelete;

        public void Initialize()
        {
            LoadTree();
            LoadPage();
        }

        private void LoadTree()
        {
            var requestVersion = ++treeVersion;
            treeLoading = true;
            treeExpandingId = string.Empty;
            RegionClient.List("0").Then(outcome =>
            {
                if (requestVersion != treeVersion)
                    return;
                treeLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载行政区域树失败。";
                    treeRows = [];
                    visibleTreeRows = [];
                    return;
                }

                expandedIds = [];
                treeRows = BuildRootRows(RegionClient.ReadList(outcome.Data));
                ApplyTreeFilter();
            }, _ =>
            {
                if (requestVersion != treeVersion)
                    return;
                treeLoading = false;
                error = "加载行政区域树失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            var requestVersion = ++listVersion;
            listLoading = true;
            RegionClient.Page(new RegionPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                pid = selectedRegionId.Length == 0 ? "0" : selectedRegionId,
                name = queryName.Trim(),
                code = queryCode.Trim()
            }).Then(outcome =>
            {
                if (requestVersion != listVersion)
                    return;
                listLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载行政区域列表失败。";
                    pageItems = [];
                    tableItems = [];
                    rows = [];
                    total = 0;
                    return;
                }

                pageItems = RegionClient.ReadItems(outcome.Data);
                tableItems = BuildPageRows(pageItems, selectedRegionId);
                rows = tableItems;
                total = RegionClient.ReadTotal(outcome.Data);
            }, _ =>
            {
                if (requestVersion != listVersion)
                    return;
                listLoading = false;
                error = "加载行政区域列表失败，请检查网络后重试。";
            });
        }

        private void RefreshAll()
        {
            error = null;
            LoadTree();
            LoadPage();
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
            selectedRegionId = "0";
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

            var result = new RegionFlatView[count];
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

        private bool IsTreeRowVisible(RegionFlatView row, string filter)
        {
            if (filter.Length > 0)
                return row.name.ToLowerInvariant().Contains(filter) || HasMatchingDescendant(row.id, filter);

            for (var index = 0; index < row.ancestorIds.Length; index++)
                if (!ContainsId(expandedIds, row.ancestorIds[index]))
                    return false;
            return true;
        }

        private bool HasMatchingDescendant(string id, string filter)
        {
            for (var index = 0; index < treeRows.Length; index++)
            {
                var candidate = treeRows[index];
                if (ContainsId(candidate.ancestorIds, id) && candidate.name.ToLowerInvariant().Contains(filter))
                    return true;
            }
            return false;
        }

        private void ToggleTree(RegionFlatView row)
        {
            if (treeExpandingId.Length > 0)
                return;

            if (!row.childrenLoaded)
            {
                LoadChildren(row);
                return;
            }

            if (!row.hasChildren)
                return;
            expandedIds = ContainsId(expandedIds, row.id)
                ? RemoveId(expandedIds, row.id)
                : AppendId(expandedIds, row.id);
            ApplyTreeFilter();
        }

        private void LoadChildren(RegionFlatView parent)
        {
            var requestVersion = treeVersion;
            treeExpandingId = parent.id;
            RegionClient.List(parent.id).Then(outcome =>
            {
                if (requestVersion != treeVersion)
                    return;
                treeExpandingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载下级行政区域失败。";
                    return;
                }

                var children = RegionClient.ReadList(outcome.Data);
                treeRows = InsertChildren(treeRows, parent, children);
                if (children.Length > 0 && !ContainsId(expandedIds, parent.id))
                    expandedIds = AppendId(expandedIds, parent.id);
                if (children.Length == 0)
                    expandedIds = RemoveId(expandedIds, parent.id);
                ApplyTreeFilter();
            }, _ =>
            {
                if (requestVersion != treeVersion)
                    return;
                treeExpandingId = string.Empty;
                error = "加载下级行政区域失败，请检查网络后重试。";
            });
        }

        private void ExpandAll()
        {
            var count = 0;
            for (var index = 0; index < treeRows.Length; index++)
                if (treeRows[index].childrenLoaded && treeRows[index].hasChildren)
                    count++;

            var result = new string[count];
            var target = 0;
            for (var index = 0; index < treeRows.Length; index++)
            {
                if (!treeRows[index].childrenLoaded || !treeRows[index].hasChildren)
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
            selectedRegionId = "0";
            queryName = string.Empty;
            queryCode = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void SelectTreeNode(RegionFlatView row)
        {
            selectedRegionId = row.id;
            queryName = string.Empty;
            queryCode = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            var parent = FindTreeRow(selectedRegionId);
            editDialogTitle = "新增行政区域";
            editParentNamePath = parent?.pathName ?? "顶级";
            editId = string.Empty;
            editPid = parent?.id ?? "0";
            editName = string.Empty;
            editCode = string.Empty;
            editCityCode = string.Empty;
            editOrderNo = 100;
            editRemark = string.Empty;
            editOriginal = null;
            editDialogVisible = true;
            error = null;
        }

        private void OpenEdit(VueProps? value)
        {
            var row = FindRegion(pageItems, RowString(value, "id"));
            if (row is null || !CanEdit)
                return;

            editDialogTitle = "编辑行政区域";
            editParentNamePath = string.Empty;
            editId = row.id;
            editPid = row.pid.Length == 0 ? "0" : row.pid;
            editName = row.name;
            editCode = row.code;
            editCityCode = row.cityCode;
            editOrderNo = row.orderNo;
            editRemark = row.remark;
            editOriginal = row;
            editDialogVisible = true;
            error = null;
        }

        private void Save()
        {
            if (saving)
                return;

            var normalizedName = editName.Trim();
            var normalizedCode = editCode.Trim();
            var normalizedCityCode = editCityCode.Trim();
            var normalizedRemark = editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                error = "行政名称不能为空。";
                return;
            }
            if (normalizedName.Length > 128)
            {
                error = "行政名称不能超过128个字符。";
                return;
            }
            if (normalizedCode.Length != 6 && normalizedCode.Length != 9 && normalizedCode.Length != 12)
            {
                error = "行政代码长度必须为6、9或12位。";
                return;
            }
            if (normalizedCityCode.Length > 6)
            {
                error = "区号不能超过6个字符。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                error = "备注不能超过128个字符。";
                return;
            }
            if (editId.Length > 0 && editOriginal is null)
            {
                error = "原始行政区域快照不可用，请刷新列表后重试。";
                return;
            }

            var input = new RegionSaveInput
            {
                pid = editPid.Length == 0 ? "0" : editPid,
                name = normalizedName,
                code = normalizedCode,
                cityCode = normalizedCityCode,
                orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                remark = normalizedRemark
            };

            saving = true;
            var isCreate = editId.Length == 0;
            var request = isCreate
                ? RegionClient.Add(input)
                : RegionClient.Update(editOriginal!, input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存行政区域失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(isCreate ? "行政区域已新增" : "行政区域已更新");
                RefreshAll();
            }, _ =>
            {
                saving = false;
                error = "保存行政区域失败，请检查网络后重试。";
            });
        }

        private void Delete(VueProps? value)
        {
            var row = FindRegion(pageItems, RowString(value, "id"));
            if (row is null || !CanDelete || deletingId.Length > 0 || syncing)
                return;

            deletingId = row.id;
            RegionClient.Delete(row.id).Then(outcome =>
            {
                deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除行政区域失败。";
                    return;
                }

                if (selectedRegionId == row.id)
                    selectedRegionId = "0";
                currentPage = 1;
                error = null;
                ElementPlus.ElMessageMembers.Success("行政区域及其全部下级节点已删除");
                RefreshAll();
            }, _ =>
            {
                deletingId = string.Empty;
                error = "删除行政区域失败，请检查网络后重试。";
            });
        }

        private void SyncRegions()
        {
            if (syncing || deletingId.Length > 0)
                return;

            syncing = true;
            RegionClient.Sync().Then(outcome =>
            {
                syncing = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "同步行政区域失败。";
                    return;
                }

                selectedRegionId = "0";
                queryName = string.Empty;
                queryCode = string.Empty;
                currentPage = 1;
                expandedIds = [];
                error = null;
                ElementPlus.ElMessageMembers.Success("行政区域同步完成");
                RefreshAll();
            }, _ =>
            {
                syncing = false;
                error = "同步行政区域失败，请检查网络后重试。";
            });
        }

        private RegionFlatView? FindTreeRow(string id)
        {
            for (var index = 0; index < treeRows.Length; index++)
                if (treeRows[index].id == id)
                    return treeRows[index];
            return null;
        }

        private static RegionView? FindRegion(RegionView[] source, string id)
        {
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                if (row.id == id)
                    return row;
                var child = FindRegion(row.children, id);
                if (child is not null)
                    return child;
            }
            return null;
        }

        private static RegionFlatView[] Flatten(RegionView[] roots)
        {
            var result = new RegionFlatView[CountNodes(roots)];
            FillRows(roots, [], string.Empty, 0, result, 0);
            return result;
        }

        private static int CountNodes(RegionView[] nodes)
        {
            var count = nodes.Length;
            for (var index = 0; index < nodes.Length; index++)
                count += CountNodes(nodes[index].children);
            return count;
        }

        private static int FillRows(
            RegionView[] nodes,
            string[] ancestors,
            string parentPath,
            int depth,
            RegionFlatView[] target,
            int offset)
        {
            // Returning the next offset keeps recursive writes dense; a local offset copy used
            // without propagation leaves undefined holes in the emitted JavaScript array.
            var current = offset;
            for (var index = 0; index < nodes.Length; index++)
            {
                var node = nodes[index];
                var path = parentPath.Length == 0 ? node.name : parentPath + " / " + node.name;
                target[current] = ToFlat(node, depth, path, ancestors);
                current++;
                current = FillRows(
                    node.children,
                    AppendId(ancestors, node.id),
                    path,
                    depth + 1,
                    target,
                    current);
            }
            return current;
        }

        private static RegionFlatView[] BuildPageRows(RegionView[] source, string selectedId)
        {
            if (selectedId.Length == 0 || selectedId == "0")
                return Flatten(source);

            var parent = FindRegion(source, selectedId);
            if (parent is null)
                return Flatten(source);

            var childCount = CountChildrenByPid(source, selectedId);
            var result = new RegionFlatView[childCount + 1];
            result[0] = ToFlat(parent, 0, parent.name, []);
            FillChildrenByPid(source, selectedId, parent.name, result, 1);
            return result;
        }

        private static int CountChildrenByPid(RegionView[] source, string pid)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                if (row.id != pid && row.pid == pid)
                    count++;
                count += CountChildrenByPid(row.children, pid);
            }
            return count;
        }

        private static int FillChildrenByPid(
            RegionView[] source,
            string pid,
            string parentPath,
            RegionFlatView[] target,
            int offset)
        {
            var current = offset;
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                if (row.id != pid && row.pid == pid)
                {
                    target[current] = ToFlat(row, 1, parentPath + " / " + row.name, [pid]);
                    current++;
                }
                current = FillChildrenByPid(row.children, pid, parentPath, target, current);
            }
            return current;
        }

        private static RegionFlatView ToFlat(RegionView node, int depth, string path, string[] ancestors)
            => new()
            {
                id = node.id,
                pid = node.pid,
                name = node.name,
                code = node.code,
                cityCode = node.cityCode,
                orderNo = node.orderNo,
                remark = node.remark,
                depth = depth,
                hasChildren = node.children.Length > 0,
                childrenLoaded = true,
                pathName = path,
                ancestorIds = ancestors
            };

        private static RegionFlatView[] BuildRootRows(RegionView[] roots)
        {
            var result = new RegionFlatView[roots.Length];
            for (var index = 0; index < roots.Length; index++)
                result[index] = ToLazyTreeRow(roots[index], 0, roots[index].name, []);
            return result;
        }

        private static RegionFlatView[] InsertChildren(
            RegionFlatView[] source,
            RegionFlatView parent,
            RegionView[] children)
        {
            var parentIndex = -1;
            for (var index = 0; index < source.Length; index++)
                if (source[index].id == parent.id)
                    parentIndex = index;
            if (parentIndex < 0)
                return source;

            var result = new RegionFlatView[source.Length + children.Length];
            var target = 0;
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                if (index == parentIndex)
                {
                    result[target] = CopyTreeRow(row, children.Length > 0, true);
                    target++;
                    var childAncestors = AppendId(parent.ancestorIds, parent.id);
                    for (var childIndex = 0; childIndex < children.Length; childIndex++)
                    {
                        var child = children[childIndex];
                        result[target] = ToLazyTreeRow(
                            child,
                            parent.depth + 1,
                            parent.pathName + " / " + child.name,
                            childAncestors);
                        target++;
                    }
                    continue;
                }

                result[target] = row;
                target++;
            }
            return result;
        }

        private static RegionFlatView ToLazyTreeRow(
            RegionView node,
            int depth,
            string path,
            string[] ancestors)
            => new()
            {
                id = node.id,
                pid = node.pid,
                name = node.name,
                code = node.code,
                cityCode = node.cityCode,
                orderNo = node.orderNo,
                remark = node.remark,
                depth = depth,
                // list 接口不返回子节点计数；首次展开前保持可加载，空响应后再固化为叶子。
                hasChildren = true,
                childrenLoaded = false,
                pathName = path,
                ancestorIds = ancestors
            };

        private static RegionFlatView CopyTreeRow(RegionFlatView source, bool hasChildren, bool childrenLoaded)
            => new()
            {
                id = source.id,
                pid = source.pid,
                name = source.name,
                code = source.code,
                cityCode = source.cityCode,
                orderNo = source.orderNo,
                remark = source.remark,
                depth = source.depth,
                hasChildren = hasChildren,
                childrenLoaded = childrenLoaded,
                pathName = source.pathName,
                ancestorIds = source.ancestorIds
            };

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

        private static string RowString(VueProps? row, string key)
        {
            if (row is null)
                return string.Empty;
            var value = Reflect.Get(row, key);
            return value is null ? string.Empty : StringValue(value);
        }

        private static int RowInt(VueProps? row, string key, int fallback)
        {
            if (row is null)
                return fallback;
            var value = Reflect.Get(row, key);
            return value is null ? fallback : (int)NumberValue(value);
        }

        private static string TreeRowStyle(RegionFlatView row, string selectedId)
        {
            var background = row.id == selectedId ? "background:#ecf5ff;color:#409eff;" : string.Empty;
            return background + "display:flex;align-items:center;gap:4px;padding:5px 8px 5px " +
                   (8 + row.depth * 18) + "px;border-radius:5px;";
        }

        private static string TreeToggleText(RegionFlatView row, string[] expanded, string loadingId)
        {
            if (row.id == loadingId)
                return "…";
            if (row.childrenLoaded && !row.hasChildren)
                return "·";
            if (!row.childrenLoaded)
                return "+";
            return ContainsId(expanded, row.id) ? "−" : "+";
        }

        private static string TreeToggleTitle(RegionFlatView row)
        {
            if (!row.childrenLoaded)
                return "加载下级区域";
            return row.hasChildren ? "展开或折叠" : "无下级区域";
        }

        private static string TableNameStyle(VueProps? row)
            => "display:inline-block;padding-left:" + (RowInt(row, "depth", 0) * 20) + "px;";

        private static string DisplayValue(string value)
            => value.Length == 0 ? "-" : value;

        private static string DeleteTitle(VueProps? row)
            => "确定删除行政区域【" + RowString(row, "name") + "】吗？该操作会同时删除其所有下级节点。";

        public RegionPageProps CreateProps() => new()
        {
            TreeFilterChanged = value => TreeFilterChanged(value ?? ""),
            SetQueryName = v => queryName = v ?? "",
            SetQueryCode = v => queryCode = v ?? "",
            SetEditDialogVisible = v => editDialogVisible = v == true,
            SetEditName = v => editName = v ?? "",
            SetEditCode = v => editCode = v ?? "",
            SetEditCityCode = v => editCityCode = v ?? "",
            SetEditOrderNo = v => editOrderNo = (int)(v ?? 100),
            SetEditRemark = v => editRemark = v ?? "",
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            HasActionColumn = HasActionColumn,
            DeletingId = deletingId,
            Syncing = syncing,
            OpenEdit = id => OpenEdit(rows.FirstOrDefault(row => row.id == id)),
            Delete = id => Delete(rows.FirstOrDefault(row => row.id == id)),
            DismissError = () => error = null,
            LoadTree = LoadTree,
            ExpandAll = ExpandAll,
            CollapseAll = CollapseAll,
            SelectRoot = SelectRoot,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            SyncRegions = SyncRegions,
            RefreshAll = RefreshAll,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            CancelEdit = () => editDialogVisible = false,
            Save = Save,
            PageSizes = JoltPageSizes,
            VisibleTreeRows = visibleTreeRows,
            ExpandedIds = expandedIds,
            TreeFilter = treeFilter,
            SelectedRegionId = selectedRegionId,
            TreeExpandingId = treeExpandingId,
            TreeLoading = treeLoading,
            Rows = rows,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            QueryName = queryName,
            QueryCode = queryCode,
            ListLoading = listLoading,
            Error = error,
            EditDialogVisible = editDialogVisible,
            EditDialogTitle = editDialogTitle,
            EditParentNamePath = editParentNamePath,
            Saving = saving,
            EditId = editId,
            EditName = editName,
            EditCode = editCode,
            EditCityCode = editCityCode,
            EditOrderNo = editOrderNo,
            EditRemark = editRemark,
            CanAdd = CanAdd,
            CanSync = CanSync,
            ToggleTree = ToggleTree,
            SelectTreeNode = SelectTreeNode,
        };
    }
}

public sealed record RegionPageProps : VueProps
{
    public required Action<string?> TreeFilterChanged { get; init; }
    public required Action<string?> SetQueryName { get; init; }
    public required Action<string?> SetQueryCode { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<string?> SetEditName { get; init; }
    public required Action<string?> SetEditCode { get; init; }
    public required Action<string?> SetEditCityCode { get; init; }
    public required Action<double?> SetEditOrderNo { get; init; }
    public required Action<string?> SetEditRemark { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool HasActionColumn { get; init; }
    public required string DeletingId { get; init; }
    public required bool Syncing { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action DismissError { get; init; }
    public required Action LoadTree { get; init; }
    public required Action ExpandAll { get; init; }
    public required Action CollapseAll { get; init; }
    public required Action SelectRoot { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action SyncRegions { get; init; }
    public required Action RefreshAll { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action Save { get; init; }
    public required double[] PageSizes { get; init; }
    public required RegionFlatView[] VisibleTreeRows { get; init; }
    public required string[] ExpandedIds { get; init; }
    public required string TreeFilter { get; init; }
    public required string SelectedRegionId { get; init; }
    public required string TreeExpandingId { get; init; }
    public required bool TreeLoading { get; init; }
    public required RegionFlatView[] Rows { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required string QueryName { get; init; }
    public required string QueryCode { get; init; }
    public required bool ListLoading { get; init; }
    public required string? Error { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditParentNamePath { get; init; }
    public required bool Saving { get; init; }
    public required string EditId { get; init; }
    public required string EditName { get; init; }
    public required string EditCode { get; init; }
    public required string EditCityCode { get; init; }
    public required int EditOrderNo { get; init; }
    public required string EditRemark { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanSync { get; init; }
    public required Action<RegionFlatView> ToggleTree { get; init; }
    public required Action<RegionFlatView> SelectTreeNode { get; init; }
}
