using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Region;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Region;

[ECMAScriptModule("components/region-page-state.js")]
public static class RegionPageModule
{
    public static RegionPageState Create() => new();

    public sealed class RegionPageState : global::Jero.Web.Composition.IPageState<RegionPageProps>
    {
        private readonly double[] _joltPageSizes = [10, 20, 50, 100];

        private RegionFlatView[] _treeRows = [];
        private RegionFlatView[] _visibleTreeRows = [];
        private string[] _expandedIds = [];
        private string _treeFilter = string.Empty;
        private string _selectedRegionId = "0";
        private string _treeExpandingId = string.Empty;
        private bool _treeLoading;
        private int _treeVersion;

        private RegionView[] _pageItems = [];
        private RegionFlatView[] _tableItems = [];
        private RegionFlatView[] _rows = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _queryName = string.Empty;
        private string _queryCode = string.Empty;
        private bool _listLoading;
        private int _listVersion;
        private string _deletingId = string.Empty;
        private bool _syncing;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增行政区域";
        private string _editParentNamePath = "顶级";
        private bool _saving;
        private string _editId = string.Empty;
        private string _editPid = "0";
        private string _editName = string.Empty;
        private string _editCode = string.Empty;
        private string _editCityCode = string.Empty;
        private int _editOrderNo = 100;
        private string _editRemark = string.Empty;
        private RegionView? _editOriginal;

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
            var requestVersion = ++_treeVersion;
            _treeLoading = true;
            _treeExpandingId = string.Empty;
            RegionClient.List("0").Then(outcome =>
            {
                if (requestVersion != _treeVersion)
                    return;
                _treeLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载行政区域树失败。";
                    _treeRows = [];
                    _visibleTreeRows = [];
                    return;
                }

                _expandedIds = [];
                _treeRows = BuildRootRows(RegionClient.ReadList(outcome.Data));
                ApplyTreeFilter();
            }, _ =>
            {
                if (requestVersion != _treeVersion)
                    return;
                _treeLoading = false;
                _error = "加载行政区域树失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            var requestVersion = ++_listVersion;
            _listLoading = true;
            RegionClient.Page(new RegionPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Pid = _selectedRegionId.Length == 0 ? "0" : _selectedRegionId,
                Name = _queryName.Trim(),
                Code = _queryCode.Trim()
            }).Then(outcome =>
            {
                if (requestVersion != _listVersion)
                    return;
                _listLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载行政区域列表失败。";
                    _pageItems = [];
                    _tableItems = [];
                    _rows = [];
                    _total = 0;
                    return;
                }

                _pageItems = RegionClient.ReadItems(outcome.Data);
                _tableItems = BuildPageRows(_pageItems, _selectedRegionId);
                _rows = _tableItems;
                _total = RegionClient.ReadTotal(outcome.Data);
            }, _ =>
            {
                if (requestVersion != _listVersion)
                    return;
                _listLoading = false;
                _error = "加载行政区域列表失败，请检查网络后重试。";
            });
        }

        private void RefreshAll()
        {
            _error = null;
            LoadTree();
            LoadPage();
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
            _selectedRegionId = "0";
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

            var result = new RegionFlatView[count];
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

        private bool IsTreeRowVisible(RegionFlatView row, string filter)
        {
            if (filter.Length > 0)
                return row.Name.ToLowerInvariant().Contains(filter) || HasMatchingDescendant(row.Id, filter);

            for (var index = 0; index < row.AncestorIds.Length; index++)
                if (!ContainsId(_expandedIds, row.AncestorIds[index]))
                    return false;
            return true;
        }

        private bool HasMatchingDescendant(string id, string filter)
        {
            for (var index = 0; index < _treeRows.Length; index++)
            {
                var candidate = _treeRows[index];
                if (ContainsId(candidate.AncestorIds, id) && candidate.Name.ToLowerInvariant().Contains(filter))
                    return true;
            }
            return false;
        }

        private void ToggleTree(RegionFlatView row)
        {
            if (_treeExpandingId.Length > 0)
                return;

            if (!row.ChildrenLoaded)
            {
                LoadChildren(row);
                return;
            }

            if (!row.HasChildren)
                return;
            _expandedIds = ContainsId(_expandedIds, row.Id)
                ? RemoveId(_expandedIds, row.Id)
                : AppendId(_expandedIds, row.Id);
            ApplyTreeFilter();
        }

        private void LoadChildren(RegionFlatView parent)
        {
            var requestVersion = _treeVersion;
            _treeExpandingId = parent.Id;
            RegionClient.List(parent.Id).Then(outcome =>
            {
                if (requestVersion != _treeVersion)
                    return;
                _treeExpandingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载下级行政区域失败。";
                    return;
                }

                var children = RegionClient.ReadList(outcome.Data);
                _treeRows = InsertChildren(_treeRows, parent, children);
                if (children.Length > 0 && !ContainsId(_expandedIds, parent.Id))
                    _expandedIds = AppendId(_expandedIds, parent.Id);
                if (children.Length == 0)
                    _expandedIds = RemoveId(_expandedIds, parent.Id);
                ApplyTreeFilter();
            }, _ =>
            {
                if (requestVersion != _treeVersion)
                    return;
                _treeExpandingId = string.Empty;
                _error = "加载下级行政区域失败，请检查网络后重试。";
            });
        }

        private void ExpandAll()
        {
            var count = 0;
            for (var index = 0; index < _treeRows.Length; index++)
                if (_treeRows[index].ChildrenLoaded && _treeRows[index].HasChildren)
                    count++;

            var result = new string[count];
            var target = 0;
            for (var index = 0; index < _treeRows.Length; index++)
            {
                if (!_treeRows[index].ChildrenLoaded || !_treeRows[index].HasChildren)
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
            _selectedRegionId = "0";
            _queryName = string.Empty;
            _queryCode = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void SelectTreeNode(RegionFlatView row)
        {
            _selectedRegionId = row.Id;
            _queryName = string.Empty;
            _queryCode = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            var parent = FindTreeRow(_selectedRegionId);
            _editDialogTitle = "新增行政区域";
            _editParentNamePath = parent?.PathName ?? "顶级";
            _editId = string.Empty;
            _editPid = parent?.Id ?? "0";
            _editName = string.Empty;
            _editCode = string.Empty;
            _editCityCode = string.Empty;
            _editOrderNo = 100;
            _editRemark = string.Empty;
            _editOriginal = null;
            _editDialogVisible = true;
            _error = null;
        }

        private void OpenEdit(VueProps? value)
        {
            var row = FindRegion(_pageItems, RowString(value, "id"));
            if (row is null || !CanEdit)
                return;

            _editDialogTitle = "编辑行政区域";
            _editParentNamePath = string.Empty;
            _editId = row.Id;
            _editPid = row.Pid.Length == 0 ? "0" : row.Pid;
            _editName = row.Name;
            _editCode = row.Code;
            _editCityCode = row.CityCode;
            _editOrderNo = row.OrderNo;
            _editRemark = row.Remark;
            _editOriginal = row;
            _editDialogVisible = true;
            _error = null;
        }

        private void Save()
        {
            if (_saving)
                return;

            var normalizedName = _editName.Trim();
            var normalizedCode = _editCode.Trim();
            var normalizedCityCode = _editCityCode.Trim();
            var normalizedRemark = _editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                _error = "行政名称不能为空。";
                return;
            }
            if (normalizedName.Length > 128)
            {
                _error = "行政名称不能超过128个字符。";
                return;
            }
            if (normalizedCode.Length != 6 && normalizedCode.Length != 9 && normalizedCode.Length != 12)
            {
                _error = "行政代码长度必须为6、9或12位。";
                return;
            }
            if (normalizedCityCode.Length > 6)
            {
                _error = "区号不能超过6个字符。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                _error = "备注不能超过128个字符。";
                return;
            }
            if (_editId.Length > 0 && _editOriginal is null)
            {
                _error = "原始行政区域快照不可用，请刷新列表后重试。";
                return;
            }

            var input = new RegionSaveInput
            {
                Pid = _editPid.Length == 0 ? "0" : _editPid,
                Name = normalizedName,
                Code = normalizedCode,
                CityCode = normalizedCityCode,
                OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                Remark = normalizedRemark
            };

            _saving = true;
            var isCreate = _editId.Length == 0;
            var request = isCreate
                ? RegionClient.Add(input)
                : RegionClient.Update(_editOriginal!, input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存行政区域失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(isCreate ? "行政区域已新增" : "行政区域已更新");
                RefreshAll();
            }, _ =>
            {
                _saving = false;
                _error = "保存行政区域失败，请检查网络后重试。";
            });
        }

        private void Delete(VueProps? value)
        {
            var row = FindRegion(_pageItems, RowString(value, "id"));
            if (row is null || !CanDelete || _deletingId.Length > 0 || _syncing)
                return;

            _deletingId = row.Id;
            RegionClient.Delete(row.Id).Then(outcome =>
            {
                _deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除行政区域失败。";
                    return;
                }

                if (_selectedRegionId == row.Id)
                    _selectedRegionId = "0";
                _currentPage = 1;
                _error = null;
                ElementPlus.ElMessageMembers.Success("行政区域及其全部下级节点已删除");
                RefreshAll();
            }, _ =>
            {
                _deletingId = string.Empty;
                _error = "删除行政区域失败，请检查网络后重试。";
            });
        }

        private void SyncRegions()
        {
            if (_syncing || _deletingId.Length > 0)
                return;

            _syncing = true;
            RegionClient.Sync().Then(outcome =>
            {
                _syncing = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "同步行政区域失败。";
                    return;
                }

                _selectedRegionId = "0";
                _queryName = string.Empty;
                _queryCode = string.Empty;
                _currentPage = 1;
                _expandedIds = [];
                _error = null;
                ElementPlus.ElMessageMembers.Success("行政区域同步完成");
                RefreshAll();
            }, _ =>
            {
                _syncing = false;
                _error = "同步行政区域失败，请检查网络后重试。";
            });
        }

        private RegionFlatView? FindTreeRow(string id)
        {
            for (var index = 0; index < _treeRows.Length; index++)
                if (_treeRows[index].Id == id)
                    return _treeRows[index];
            return null;
        }

        private static RegionView? FindRegion(RegionView[] source, string id)
        {
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                if (row.Id == id)
                    return row;
                var child = FindRegion(row.Children, id);
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
                count += CountNodes(nodes[index].Children);
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
            // 递归要返回下一处写入位置，让外层接着写。
            // 如果偏移只留在局部，生成的 JavaScript 数组会出现空洞。
            var current = offset;
            for (var index = 0; index < nodes.Length; index++)
            {
                var node = nodes[index];
                var path = parentPath.Length == 0 ? node.Name : parentPath + " / " + node.Name;
                target[current] = ToFlat(node, depth, path, ancestors);
                current++;
                current = FillRows(
                    node.Children,
                    AppendId(ancestors, node.Id),
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
            result[0] = ToFlat(parent, 0, parent.Name, []);
            FillChildrenByPid(source, selectedId, parent.Name, result, 1);
            return result;
        }

        private static int CountChildrenByPid(RegionView[] source, string pid)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                if (row.Id != pid && row.Pid == pid)
                    count++;
                count += CountChildrenByPid(row.Children, pid);
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
                if (row.Id != pid && row.Pid == pid)
                {
                    target[current] = ToFlat(row, 1, parentPath + " / " + row.Name, [pid]);
                    current++;
                }
                current = FillChildrenByPid(row.Children, pid, parentPath, target, current);
            }
            return current;
        }

        private static RegionFlatView ToFlat(RegionView node, int depth, string path, string[] ancestors)
            => new()
            {
                Id = node.Id,
                Pid = node.Pid,
                Name = node.Name,
                Code = node.Code,
                CityCode = node.CityCode,
                OrderNo = node.OrderNo,
                Remark = node.Remark,
                Depth = depth,
                HasChildren = node.Children.Length > 0,
                ChildrenLoaded = true,
                PathName = path,
                AncestorIds = ancestors
            };

        private static RegionFlatView[] BuildRootRows(RegionView[] roots)
        {
            var result = new RegionFlatView[roots.Length];
            for (var index = 0; index < roots.Length; index++)
                result[index] = ToLazyTreeRow(roots[index], 0, roots[index].Name, []);
            return result;
        }

        private static RegionFlatView[] InsertChildren(
            RegionFlatView[] source,
            RegionFlatView parent,
            RegionView[] children)
        {
            var parentIndex = -1;
            for (var index = 0; index < source.Length; index++)
                if (source[index].Id == parent.Id)
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
                    var childAncestors = AppendId(parent.AncestorIds, parent.Id);
                    for (var childIndex = 0; childIndex < children.Length; childIndex++)
                    {
                        var child = children[childIndex];
                        result[target] = ToLazyTreeRow(
                            child,
                            parent.Depth + 1,
                            parent.PathName + " / " + child.Name,
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
                Id = node.Id,
                Pid = node.Pid,
                Name = node.Name,
                Code = node.Code,
                CityCode = node.CityCode,
                OrderNo = node.OrderNo,
                Remark = node.Remark,
                Depth = depth,
                // list 接口不返回子节点计数；首次展开前保持可加载，空响应后再固化为叶子。
                HasChildren = true,
                ChildrenLoaded = false,
                PathName = path,
                AncestorIds = ancestors
            };

        private static RegionFlatView CopyTreeRow(RegionFlatView source, bool hasChildren, bool childrenLoaded)
            => new()
            {
                Id = source.Id,
                Pid = source.Pid,
                Name = source.Name,
                Code = source.Code,
                CityCode = source.CityCode,
                OrderNo = source.OrderNo,
                Remark = source.Remark,
                Depth = source.Depth,
                HasChildren = hasChildren,
                ChildrenLoaded = childrenLoaded,
                PathName = source.PathName,
                AncestorIds = source.AncestorIds
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
            var background = row.Id == selectedId ? "background:#ecf5ff;color:#409eff;" : string.Empty;
            return background + "display:flex;align-items:center;gap:4px;padding:5px 8px 5px " +
                   (8 + row.Depth * 18) + "px;border-radius:5px;";
        }

        private static string TreeToggleText(RegionFlatView row, string[] expanded, string loadingId)
        {
            if (row.Id == loadingId)
                return "…";
            if (row.ChildrenLoaded && !row.HasChildren)
                return "·";
            if (!row.ChildrenLoaded)
                return "+";
            return ContainsId(expanded, row.Id) ? "−" : "+";
        }

        private static string TreeToggleTitle(RegionFlatView row)
        {
            if (!row.ChildrenLoaded)
                return "加载下级区域";
            return row.HasChildren ? "展开或折叠" : "无下级区域";
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
            SetQueryName = v => _queryName = v ?? "",
            SetQueryCode = v => _queryCode = v ?? "",
            SetEditDialogVisible = v => _editDialogVisible = v == true,
            SetEditName = v => _editName = v ?? "",
            SetEditCode = v => _editCode = v ?? "",
            SetEditCityCode = v => _editCityCode = v ?? "",
            SetEditOrderNo = v => _editOrderNo = (int)(v ?? 100),
            SetEditRemark = v => _editRemark = v ?? "",
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            HasActionColumn = HasActionColumn,
            DeletingId = _deletingId,
            Syncing = _syncing,
            OpenEdit = id => OpenEdit(_rows.FirstOrDefault(row => row.Id == id)),
            Delete = id => Delete(_rows.FirstOrDefault(row => row.Id == id)),
            DismissError = () => _error = null,
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
            CancelEdit = () => _editDialogVisible = false,
            Save = Save,
            PageSizes = _joltPageSizes,
            VisibleTreeRows = _visibleTreeRows,
            ExpandedIds = _expandedIds,
            TreeFilter = _treeFilter,
            SelectedRegionId = _selectedRegionId,
            TreeExpandingId = _treeExpandingId,
            TreeLoading = _treeLoading,
            Rows = _rows,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            QueryName = _queryName,
            QueryCode = _queryCode,
            ListLoading = _listLoading,
            Error = _error,
            EditDialogVisible = _editDialogVisible,
            EditDialogTitle = _editDialogTitle,
            EditParentNamePath = _editParentNamePath,
            Saving = _saving,
            EditId = _editId,
            EditName = _editName,
            EditCode = _editCode,
            EditCityCode = _editCityCode,
            EditOrderNo = _editOrderNo,
            EditRemark = _editRemark,
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
