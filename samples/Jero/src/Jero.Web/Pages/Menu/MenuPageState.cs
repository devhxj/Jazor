using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Menu;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;

namespace Jero.Web.Pages.Menu;

[ECMAScriptModule("components/menu-page-state.js")]
public static class MenuPageModule
{
    public static MenuPageState Create() => new();

    public sealed class MenuPageState : global::Jero.Web.Composition.IPageState<MenuPageProps>
    {
        private MenuView[] _menuTree = [];
        private MenuFlatView[] _visibleItems = [];
        private MenuFlatView[] _parentItems = [];
        private MenuTenantOption[] _tenantItems = [];
        private string[] _expandedIds = [];
        private string _tenantId = string.Empty;
        private string _titleFilter = string.Empty;
        private string _typeFilter = string.Empty;
        private bool _loading;
        private bool _tenantLoading;
        private string? _error;
        private string _statusLoadingId = string.Empty;

        private bool _editDialogVisible;
        private bool _formLoading;
        private bool _saving;
        private string _editDialogTitle = "新增菜单";
        private string _pendingMode = string.Empty;
        private MenuView? _pendingRow;
        private MenuView? _editOriginal;
        private string _editId = string.Empty;
        private string _editTenantId = string.Empty;
        private string _editPid = "0";
        private int _editType = 2;
        private string _editTitle = string.Empty;
        private string _editName = string.Empty;
        private string _editPath = string.Empty;
        private string _editComponent = string.Empty;
        private string _editIcon = string.Empty;
        private string _editRedirect = string.Empty;
        private string _editOutLink = string.Empty;
        private string _editPermission = string.Empty;
        private int _editOrderNo = 100;
        private int _editStatus = 1;
        private bool _editIsHide;
        private bool _editIsKeepAlive = true;
        private bool _editIsAffix;
        private bool _editIsIframe;
        private string _editRemark = string.Empty;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysMenu:add");
        private bool CanEdit => ApplicationState.HasPermission("sysMenu:update");
        private bool CanDelete => ApplicationState.HasPermission("sysMenu:delete");
        private bool CanSetStatus => ApplicationState.HasPermission("sysMenu:setStatus");
        private bool HasActionColumn => CanAdd || CanEdit || CanDelete;
        private bool IsRouteType => _editType == 1 || _editType == 2;
        private bool IsButtonType => _editType == 3;
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            _tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadMenus();
        }

        private void LoadTenants()
        {
            _tenantLoading = true;
            MenuClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _tenantItems = [];
                    _error = outcome.Error ?? "加载租户失败。";
                    return;
                }

                _tenantItems = MenuClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadMenus()
        {
            _loading = true;
            MenuClient.List(new MenuListInput
            {
                Title = _titleFilter.Trim(),
                Type = FilterType(),
                TenantId = IsSuperAdmin ? _tenantId : string.Empty
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _menuTree = [];
                    _visibleItems = [];
                    _error = outcome.Error ?? "加载菜单失败。";
                    return;
                }

                _menuTree = MenuClient.ReadMenus(outcome.Data);
                _expandedIds = CollectExpandableIds(_menuTree);
                RebuildVisibleItems();
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载菜单失败，请检查网络后重试。";
            });
        }

        private void Search() => LoadMenus();

        private void ResetSearch()
        {
            _tenantId = DefaultTenantId;
            _titleFilter = string.Empty;
            _typeFilter = string.Empty;
            LoadMenus();
        }

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
            LoadMenus();
        }

        private void TypeFilterChanged(string? value)
            => _typeFilter = value ?? string.Empty;

        private int FilterType()
        {
            if (_typeFilter == "1") return 1;
            if (_typeFilter == "2") return 2;
            if (_typeFilter == "3") return 3;
            return 0;
        }

        private void ToggleExpanded(string id)
        {
            var row = FindVisibleMenu(id);
            if (row is null)
                return;

            if (row.Children.Length == 0)
                return;

            if (IsExpanded(row.Id))
                _expandedIds = RemoveId(_expandedIds, row.Id);
            else
                _expandedIds = AddId(_expandedIds, row.Id);
            RebuildVisibleItems();
        }

        private void OpenCreate()
        {
            if (_formLoading)
                return;

            var targetTenantId = IsSuperAdmin ? _tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增菜单");
                return;
            }

            BeginOpenForm("create", null, targetTenantId);
        }

        private void OpenEdit(string id)
        {
            var row = FindVisibleMenu(id);
            if (row is null)
                return;

            if (!CanEdit || _formLoading)
                return;

            var targetTenantId = ResolveRowTenant(row);
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再编辑菜单");
                return;
            }

            BeginOpenForm("edit", row, targetTenantId);
        }

        private void OpenCopy(string id)
        {
            var row = FindVisibleMenu(id);
            if (row is null)
                return;

            if (!CanAdd || _formLoading)
                return;

            var targetTenantId = ResolveRowTenant(row);
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再复制菜单");
                return;
            }

            BeginOpenForm("copy", row, targetTenantId);
        }

        private void BeginOpenForm(string mode, MenuView? row, string targetTenantId)
        {
            _pendingMode = mode;
            _pendingRow = row;
            _editTenantId = targetTenantId;
            _formLoading = true;

            // 列表可能已按条件筛选。重新加载完整的租户菜单树，
            // 确保选择父级时能看到所有可用的祖先节点。
            MenuClient.List(new MenuListInput { TenantId = IsSuperAdmin ? targetTenantId : string.Empty }).Then(outcome =>
            {
                _formLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载上级菜单失败。";
                    return;
                }

                _parentItems = FlattenAll(MenuClient.ReadMenus(outcome.Data));
                OpenPendingForm();
            }, _ =>
            {
                _formLoading = false;
                _error = "加载上级菜单失败，请检查网络后重试。";
            });
        }

        private void OpenPendingForm()
        {
            if (_pendingMode == "create" || _pendingRow is null)
            {
                _editDialogTitle = "新增菜单";
                ResetForm();
            }
            else
            {
                var copy = _pendingMode == "copy";
                _editDialogTitle = copy ? "复制菜单" : "编辑菜单";
                PopulateForm(_pendingRow, copy);
            }

            _pendingMode = string.Empty;
            _pendingRow = null;
            _editDialogVisible = true;
        }

        private void ResetForm()
        {
            _editOriginal = null;
            _editId = string.Empty;
            _editPid = "0";
            _editType = 2;
            _editTitle = string.Empty;
            _editName = string.Empty;
            _editPath = string.Empty;
            _editComponent = string.Empty;
            _editIcon = string.Empty;
            _editRedirect = string.Empty;
            _editOutLink = string.Empty;
            _editPermission = string.Empty;
            _editOrderNo = 100;
            _editStatus = 1;
            _editIsHide = false;
            _editIsKeepAlive = true;
            _editIsAffix = false;
            _editIsIframe = false;
            _editRemark = string.Empty;
        }

        private void PopulateForm(MenuView row, bool copy)
        {
            _editOriginal = copy ? null : row;
            _editId = copy ? string.Empty : row.Id;
            _editTenantId = ResolveRowTenant(row);
            _editPid = row.Pid.Length == 0 ? "0" : row.Pid;
            _editType = NormalizeType(row.Type);
            _editTitle = copy ? string.Empty : row.Title;
            _editName = row.Name;
            _editPath = row.Path;
            _editComponent = row.Component;
            _editIcon = row.Icon;
            _editRedirect = row.Redirect;
            _editOutLink = row.OutLink;
            _editPermission = row.Permission;
            _editOrderNo = row.OrderNo;
            _editStatus = row.Status == 2 ? 2 : 1;
            _editIsHide = row.IsHide;
            _editIsKeepAlive = row.IsKeepAlive;
            _editIsAffix = row.IsAffix;
            _editIsIframe = row.IsIframe;
            _editRemark = row.Remark;
        }

        private void ChangeEditType(int value)
            => _editType = NormalizeType(value);

        private void Save()
        {
            if (_saving)
                return;

            var normalizedTitle = _editTitle.Trim();
            var normalizedName = _editName.Trim();
            var normalizedPath = _editPath.Trim();
            var normalizedComponent = _editComponent.Trim();
            var normalizedIcon = _editIcon.Trim();
            var normalizedRedirect = _editRedirect.Trim();
            var normalizedOutLink = _editOutLink.Trim();
            var normalizedPermission = _editPermission.Trim();
            var normalizedRemark = _editRemark.Trim();

            if (normalizedTitle.Length == 0)
            {
                _error = "菜单名称不能为空。";
                return;
            }
            if (normalizedTitle.Length > 64)
            {
                _error = "菜单名称不能超过64个字符。";
                return;
            }
            if (normalizedName.Length > 64 || normalizedPath.Length > 128 ||
                normalizedComponent.Length > 128 || normalizedIcon.Length > 128 ||
                normalizedRedirect.Length > 128 || normalizedPermission.Length > 128)
            {
                _error = "路由名称不能超过64个字符，路径、组件、图标、重定向和权限标识不能超过128个字符。";
                return;
            }
            if (normalizedOutLink.Length > 256 || normalizedRemark.Length > 256)
            {
                _error = "链接地址和备注不能超过256个字符。";
                return;
            }
            if (IsButtonType && normalizedPermission.Length == 0)
            {
                _error = "按钮类型必须填写权限标识。";
                return;
            }
            if (IsButtonType && !normalizedPermission.Contains(':'))
            {
                _error = "按钮权限标识必须包含冒号。";
                return;
            }
            if (IsSuperAdmin && _editTenantId.Length == 0)
            {
                _error = "超级管理员维护菜单时必须选择租户。";
                return;
            }

            var routeType = IsRouteType;
            var input = new MenuSaveInput
            {
                Id = _editId,
                Pid = _editPid.Length == 0 ? "0" : _editPid,
                Type = NormalizeType(_editType),
                Title = normalizedTitle,
                Name = routeType ? normalizedName : string.Empty,
                Path = routeType ? normalizedPath : string.Empty,
                Component = routeType ? normalizedComponent : string.Empty,
                Icon = routeType ? normalizedIcon : string.Empty,
                Redirect = routeType ? normalizedRedirect : string.Empty,
                OutLink = routeType ? normalizedOutLink : string.Empty,
                Permission = routeType ? string.Empty : normalizedPermission,
                OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                Status = _editStatus == 2 ? 2 : 1,
                IsHide = routeType && _editIsHide,
                IsKeepAlive = routeType && _editIsKeepAlive,
                IsAffix = routeType && _editIsAffix,
                IsIframe = routeType && _editIsIframe,
                Remark = normalizedRemark,
                TenantId = IsSuperAdmin ? _editTenantId : DefaultTenantId
            };

            if (_editId.Length > 0 && _editOriginal is null)
            {
                _error = "缺少菜单原始快照，请重新打开编辑窗口。";
                return;
            }

            _saving = true;
            var request = _editId.Length == 0
                ? MenuClient.Add(input)
                : MenuClient.Update(_editOriginal!, input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存菜单失败。";
                    return;
                }

                _editDialogVisible = false;
                _editOriginal = null;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "菜单已新增" : "菜单已更新");
                LoadMenus();
            }, _ =>
            {
                _saving = false;
                _error = "保存菜单失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = FindVisibleMenu(id);
            if (row is null)
                return;

            if (!CanDelete)
                return;

            MenuClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除菜单失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("菜单已删除");
                LoadMenus();
            }, _ => _error = "删除菜单失败，请检查网络后重试。");
        }

        private void ToggleStatus(string id)
        {
            var row = FindVisibleMenu(id);
            if (row is null)
                return;

            if (!CanSetStatus || _statusLoadingId.Length > 0)
                return;

            var nextStatus = row.Status == 1 ? 2 : 1;
            _statusLoadingId = row.Id;
            MenuClient.SetStatus(row.Id, nextStatus).Then(outcome =>
            {
                _statusLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "切换菜单状态失败。";
                    RebuildVisibleItems();
                    return;
                }

                row.Status = nextStatus;
                _error = null;
                ElementPlus.ElMessageMembers.Success(nextStatus == 1 ? "菜单状态已切换为启用" : "菜单状态已切换为停用");
                RebuildVisibleItems();
            }, _ =>
            {
                _statusLoadingId = string.Empty;
                _error = "切换菜单状态失败，请检查网络后重试。";
                RebuildVisibleItems();
            });
        }

        private string ResolveRowTenant(MenuView row)
        {
            if (row.TenantId.Length > 0)
                return row.TenantId;
            if (IsSuperAdmin && _tenantId.Length > 0)
                return _tenantId;
            return DefaultTenantId;
        }

        private void RebuildVisibleItems()
        {
            var count = CountVisible(_menuTree);
            var result = new MenuFlatView[count];
            FillVisible(_menuTree, 0, result, 0);
            _visibleItems = result;
        }

        private int CountVisible(MenuView[] source)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
            {
                count += 1;
                if (source[index].Children.Length > 0 && IsExpanded(source[index].Id))
                    count += CountVisible(source[index].Children);
            }
            return count;
        }

        private int FillVisible(MenuView[] source, int depth, MenuFlatView[] target, int writeIndex)
        {
            var current = writeIndex;
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                target[current] = new MenuFlatView { Menu = row, Depth = depth };
                current += 1;
                if (row.Children.Length > 0 && IsExpanded(row.Id))
                    current = FillVisible(row.Children, depth + 1, target, current);
            }
            return current;
        }

        private static MenuFlatView[] FlattenAll(MenuView[] source)
        {
            var result = new MenuFlatView[CountAll(source)];
            FillAll(source, 0, result, 0);
            return result;
        }

        private static int CountAll(MenuView[] source)
        {
            var count = source.Length;
            for (var index = 0; index < source.Length; index++)
                count += CountAll(source[index].Children);
            return count;
        }

        private static int FillAll(MenuView[] source, int depth, MenuFlatView[] target, int writeIndex)
        {
            var current = writeIndex;
            for (var index = 0; index < source.Length; index++)
            {
                target[current] = new MenuFlatView { Menu = source[index], Depth = depth };
                current += 1;
                current = FillAll(source[index].Children, depth + 1, target, current);
            }
            return current;
        }

        private static string[] CollectExpandableIds(MenuView[] source)
        {
            var result = new string[CountExpandable(source)];
            FillExpandableIds(source, result, 0);
            return result;
        }

        private static int CountExpandable(MenuView[] source)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index].Children.Length > 0)
                    count += 1;
                count += CountExpandable(source[index].Children);
            }
            return count;
        }

        private static int FillExpandableIds(MenuView[] source, string[] target, int writeIndex)
        {
            var current = writeIndex;
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index].Children.Length > 0)
                {
                    target[current] = source[index].Id;
                    current += 1;
                }
                current = FillExpandableIds(source[index].Children, target, current);
            }
            return current;
        }

        private bool IsExpanded(string id)
        {
            for (var index = 0; index < _expandedIds.Length; index++)
                if (_expandedIds[index] == id)
                    return true;
            return false;
        }

        private static string[] AddId(string[] source, string id)
        {
            var result = new string[source.Length + 1];
            for (var index = 0; index < source.Length; index++)
                result[index] = source[index];
            result[source.Length] = id;
            return result;
        }

        private static string[] RemoveId(string[] source, string id)
        {
            var result = new string[source.Length - 1];
            var targetIndex = 0;
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index] == id)
                    continue;
                result[targetIndex] = source[index];
                targetIndex += 1;
            }
            return result;
        }

        private static int NormalizeType(int value)
            => value == 1 || value == 3 ? value : 2;

        private static string MenuTypeText(int value)
        {
            if (value == 1) return "目录";
            if (value == 2) return "菜单";
            if (value == 3) return "按钮";
            return "未分类";
        }

        private static string ResolveMenuIcon(string icon)
        {
            if (icon.Length == 0) return "lucide:circle";
            if (icon.Contains(':')) return icon;
            if (icon == "ele-Bell") return "lucide:bell";
            if (icon == "ele-Collection") return "lucide:book-open";
            if (icon == "ele-Document") return "lucide:file-text";
            if (icon == "ele-DocumentCopy") return "lucide:copy";
            if (icon == "ele-Help") return "lucide:shield-check";
            if (icon == "ele-HomeFilled") return "lucide:house";
            if (icon == "ele-Menu") return "lucide:menu";
            if (icon == "ele-OfficeBuilding") return "lucide:building-2";
            if (icon == "ele-Operation") return "lucide:settings-2";
            if (icon == "ele-Setting") return "lucide:settings";
            if (icon == "ele-User") return "lucide:user-round";
            return "lucide:circle";
        }

        private static string MenuIconGlyph(string icon)
        {
            var resolved = ResolveMenuIcon(icon);
            if (resolved == "lucide:bell") return "铃";
            if (resolved == "lucide:book-open") return "册";
            if (resolved == "lucide:file-text") return "文";
            if (resolved == "lucide:copy") return "复";
            if (resolved == "lucide:shield-check") return "盾";
            if (resolved == "lucide:house") return "⌂";
            if (resolved == "lucide:menu") return "☰";
            if (resolved == "lucide:building-2") return "楼";
            if (resolved == "lucide:settings-2" || resolved == "lucide:settings") return "⚙";
            if (resolved == "lucide:user-round") return "人";
            return "○";
        }

        private MenuTableRow[] TableRows()
        {
            var rows = new MenuTableRow[_visibleItems.Length];
            for (var index = 0; index < _visibleItems.Length; index++)
            {
                var flat = _visibleItems[index];
                var row = flat.Menu;
                var hasChildren = row.Children.Length > 0;
                var expanded = IsExpanded(row.Id);
                rows[index] = new MenuTableRow(
                    row.Id, row.Title, row.Type, MenuTypeText(row.Type),
                    DisplayValue(row.Path), DisplayValue(row.Component), DisplayValue(row.Permission),
                    row.OrderNo, row.Status, hasChildren, expanded,
                    "padding-left:" + (flat.Depth * 22) + "px",
                    !hasChildren ? "" : expanded ? "−" : "+",
                    ResolveMenuIcon(row.Icon), MenuIconGlyph(row.Icon));
            }
            return rows;
        }

        private ConfigurationOption[] TenantOptions()
        {
            var options = new ConfigurationOption[_tenantItems.Length];
            for (var index = 0; index < _tenantItems.Length; index++)
            {
                var tenant = _tenantItems[index];
                options[index] = new ConfigurationOption(tenant.Value, TenantLabel(tenant));
            }
            return options;
        }

        private ConfigurationOption[] ParentOptions()
        {
            var options = new ConfigurationOption[_parentItems.Length + 1];
            options[0] = new ConfigurationOption("0", "顶级菜单");
            for (var index = 0; index < _parentItems.Length; index++)
            {
                var parent = _parentItems[index];
                options[index + 1] = new ConfigurationOption(parent.Menu.Id, ParentLabel(parent));
            }
            return options;
        }

        private MenuView? FindVisibleMenu(string id)
        {
            for (var index = 0; index < _visibleItems.Length; index++)
            {
                if (_visibleItems[index].Menu.Id == id)
                    return _visibleItems[index].Menu;
            }
            return null;
        }

        private static string ParentLabel(MenuFlatView row)
        {
            var prefix = string.Empty;
            for (var index = 0; index < row.Depth; index++)
                prefix += "　";
            return prefix + row.Menu.Title + "（" + MenuTypeText(row.Menu.Type) + "）";
        }

        private static string DisplayValue(string value) => value.Length == 0 ? "-" : value;

        private static string TenantLabel(MenuTenantOption tenant)
            => tenant.Host.Length == 0 ? tenant.Label : tenant.Label + " (" + tenant.Host + ")";

        public MenuPageProps CreateProps() => new()
        {
            IsSuperAdmin = IsSuperAdmin,
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            CanSetStatus = CanSetStatus,
            HasActionColumn = HasActionColumn,
            IsRouteType = IsRouteType,
            IsButtonType = IsButtonType,
            Loading = _loading,
            FormLoading = _formLoading,
            TenantLoading = _tenantLoading,
            EditDialogVisible = _editDialogVisible,
            Saving = _saving,
            EditIsHide = _editIsHide,
            EditIsKeepAlive = _editIsKeepAlive,
            EditIsAffix = _editIsAffix,
            EditIsIframe = _editIsIframe,
            TenantId = _tenantId,
            TitleFilter = _titleFilter,
            TypeFilter = _typeFilter,
            EditTenantId = _editTenantId,
            EditPid = _editPid,
            EditDialogTitle = _editDialogTitle,
            EditTitle = _editTitle,
            EditName = _editName,
            EditPath = _editPath,
            EditComponent = _editComponent,
            EditIcon = _editIcon,
            EditRedirect = _editRedirect,
            EditOutLink = _editOutLink,
            EditPermission = _editPermission,
            EditRemark = _editRemark,
            EditType = _editType,
            EditStatus = _editStatus,
            EditOrderNo = _editOrderNo,
            Error = _error,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadMenus = LoadMenus,
            Save = Save,
            ToggleExpanded = id => ToggleExpanded(id),
            OpenEdit = id => OpenEdit(id),
            OpenCopy = id => OpenCopy(id),
            Delete = id => Delete(id),
            ToggleStatus = id => ToggleStatus(id),
            TenantChanged = TenantChanged,
            TypeFilterChanged = TypeFilterChanged,
            SetTitleFilter = value => _titleFilter = value ?? "",
            SetEditTitle = value => _editTitle = value ?? "",
            SetEditName = value => _editName = value ?? "",
            SetEditPath = value => _editPath = value ?? "",
            SetEditComponent = value => _editComponent = value ?? "",
            SetEditIcon = value => _editIcon = value ?? "",
            SetEditRedirect = value => _editRedirect = value ?? "",
            SetEditOutLink = value => _editOutLink = value ?? "",
            SetEditPermission = value => _editPermission = value ?? "",
            SetEditRemark = value => _editRemark = value ?? "",
            SetEditPid = value => _editPid = string.IsNullOrEmpty(value) ? "0" : value,
            SetEditDialogVisible = value => _editDialogVisible = value,
            ChangeEditType = value => ChangeEditType((int)value),
            SetEditStatus = value => _editStatus = (int)value,
            SetEditOrderNo = value => _editOrderNo = (int)(value ?? 100),
            ToggleEditIsHide = () => _editIsHide = !_editIsHide,
            ToggleEditIsKeepAlive = () => _editIsKeepAlive = !_editIsKeepAlive,
            ToggleEditIsAffix = () => _editIsAffix = !_editIsAffix,
            ToggleEditIsIframe = () => _editIsIframe = !_editIsIframe,
            DismissError = () => _error = null,
            CancelEdit = () => _editDialogVisible = false,
            StatusChanging = _statusLoadingId.Length > 0,
            Rows = TableRows(),
            TenantOptions = TenantOptions(),
            ParentOptions = ParentOptions(),
        };
    }
}

public sealed record MenuPageProps : VueProps
{
    public required bool IsSuperAdmin { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool CanSetStatus { get; init; }
    public required bool HasActionColumn { get; init; }
    public required bool IsRouteType { get; init; }
    public required bool IsButtonType { get; init; }
    public required bool Loading { get; init; }
    public required bool FormLoading { get; init; }
    public required bool TenantLoading { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool Saving { get; init; }
    public required bool EditIsHide { get; init; }
    public required bool EditIsKeepAlive { get; init; }
    public required bool EditIsAffix { get; init; }
    public required bool EditIsIframe { get; init; }
    public required string TenantId { get; init; }
    public required string TitleFilter { get; init; }
    public required string TypeFilter { get; init; }
    public required string EditTenantId { get; init; }
    public required string EditPid { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditTitle { get; init; }
    public required string EditName { get; init; }
    public required string EditPath { get; init; }
    public required string EditComponent { get; init; }
    public required string EditIcon { get; init; }
    public required string EditRedirect { get; init; }
    public required string EditOutLink { get; init; }
    public required string EditPermission { get; init; }
    public required string EditRemark { get; init; }
    public required int EditType { get; init; }
    public required int EditStatus { get; init; }
    public required int EditOrderNo { get; init; }
    public required string? Error { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action LoadMenus { get; init; }
    public required Action Save { get; init; }
    public required Action<string> ToggleExpanded { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> OpenCopy { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string> ToggleStatus { get; init; }
    public required Action<string?> TenantChanged { get; init; }
    public required Action<string?> TypeFilterChanged { get; init; }
    public required Action<string?> SetTitleFilter { get; init; }
    public required Action<string?> SetEditTitle { get; init; }
    public required Action<string?> SetEditName { get; init; }
    public required Action<string?> SetEditPath { get; init; }
    public required Action<string?> SetEditComponent { get; init; }
    public required Action<string?> SetEditIcon { get; init; }
    public required Action<string?> SetEditRedirect { get; init; }
    public required Action<string?> SetEditOutLink { get; init; }
    public required Action<string?> SetEditPermission { get; init; }
    public required Action<string?> SetEditRemark { get; init; }
    public required Action<string?> SetEditPid { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<double> ChangeEditType { get; init; }
    public required Action<double> SetEditStatus { get; init; }
    public required Action<double?> SetEditOrderNo { get; init; }
    public required Action ToggleEditIsHide { get; init; }
    public required Action ToggleEditIsKeepAlive { get; init; }
    public required Action ToggleEditIsAffix { get; init; }
    public required Action ToggleEditIsIframe { get; init; }
    public required Action DismissError { get; init; }
    public required Action CancelEdit { get; init; }
    public required bool StatusChanging { get; init; }
    public required MenuTableRow[] Rows { get; init; }
    public required ConfigurationOption[] TenantOptions { get; init; }
    public required ConfigurationOption[] ParentOptions { get; init; }
}
