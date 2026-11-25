using Jero.Web.App.Components;
using Jero.Web.App.Shell;

namespace Jero.Web.App.Menu;

[ECMAScriptModule("components/menu-page-state.js")]
public static class MenuPageModule
{
    public static MenuPageState Create() => new();

    public sealed class MenuPageState : global::Jero.Web.App.IPageState<MenuPageProps>
    {
        private MenuView[] menuTree = [];
        private MenuFlatView[] visibleItems = [];
        private MenuFlatView[] parentItems = [];
        private MenuTenantOption[] tenantItems = [];
        private string[] expandedIds = [];
        private string tenantId = string.Empty;
        private string titleFilter = string.Empty;
        private string typeFilter = string.Empty;
        private bool loading;
        private bool tenantLoading;
        private string? error;
        private string statusLoadingId = string.Empty;

        private bool editDialogVisible;
        private bool formLoading;
        private bool saving;
        private string editDialogTitle = "新增菜单";
        private string pendingMode = string.Empty;
        private MenuView? pendingRow;
        private MenuView? editOriginal;
        private string editId = string.Empty;
        private string editTenantId = string.Empty;
        private string editPid = "0";
        private int editType = 2;
        private string editTitle = string.Empty;
        private string editName = string.Empty;
        private string editPath = string.Empty;
        private string editComponent = string.Empty;
        private string editIcon = string.Empty;
        private string editRedirect = string.Empty;
        private string editOutLink = string.Empty;
        private string editPermission = string.Empty;
        private int editOrderNo = 100;
        private int editStatus = 1;
        private bool editIsHide;
        private bool editIsKeepAlive = true;
        private bool editIsAffix;
        private bool editIsIframe;
        private string editRemark = string.Empty;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysMenu:add");
        private bool CanEdit => ApplicationState.HasPermission("sysMenu:update");
        private bool CanDelete => ApplicationState.HasPermission("sysMenu:delete");
        private bool CanSetStatus => ApplicationState.HasPermission("sysMenu:setStatus");
        private bool HasActionColumn => CanAdd || CanEdit || CanDelete;
        private bool IsRouteType => editType == 1 || editType == 2;
        private bool IsButtonType => editType == 3;
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadMenus();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            MenuClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    tenantItems = [];
                    error = outcome.Error ?? "加载租户失败。";
                    return;
                }

                tenantItems = MenuClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadMenus()
        {
            loading = true;
            MenuClient.List(new MenuListInput
            {
                title = titleFilter.Trim(),
                type = FilterType(),
                tenantId = IsSuperAdmin ? tenantId : string.Empty
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    menuTree = [];
                    visibleItems = [];
                    error = outcome.Error ?? "加载菜单失败。";
                    return;
                }

                menuTree = MenuClient.ReadMenus(outcome.Data);
                expandedIds = CollectExpandableIds(menuTree);
                RebuildVisibleItems();
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载菜单失败，请检查网络后重试。";
            });
        }

        private void Search() => LoadMenus();

        private void ResetSearch()
        {
            tenantId = DefaultTenantId;
            titleFilter = string.Empty;
            typeFilter = string.Empty;
            LoadMenus();
        }

        private void TenantChanged(string? value)
        {
            tenantId = value ?? string.Empty;
            LoadMenus();
        }

        private void TypeFilterChanged(string? value)
            => typeFilter = value ?? string.Empty;

        private int FilterType()
        {
            if (typeFilter == "1") return 1;
            if (typeFilter == "2") return 2;
            if (typeFilter == "3") return 3;
            return 0;
        }

        private void ToggleExpanded(string id)
        {
            var row = FindVisibleMenu(id);
            if (row is null)
                return;

            if (row.children.Length == 0)
                return;

            if (IsExpanded(row.id))
                expandedIds = RemoveId(expandedIds, row.id);
            else
                expandedIds = AddId(expandedIds, row.id);
            RebuildVisibleItems();
        }

        private void OpenCreate()
        {
            if (formLoading)
                return;

            var targetTenantId = IsSuperAdmin ? tenantId : DefaultTenantId;
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

            if (!CanEdit || formLoading)
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

            if (!CanAdd || formLoading)
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
            pendingMode = mode;
            pendingRow = row;
            editTenantId = targetTenantId;
            formLoading = true;

            // The list may currently be filtered. Reload the complete tenant tree so parent
            // selection never loses valid ancestors because of a search condition.
            MenuClient.List(new MenuListInput { tenantId = IsSuperAdmin ? targetTenantId : string.Empty }).Then(outcome =>
            {
                formLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载上级菜单失败。";
                    return;
                }

                parentItems = FlattenAll(MenuClient.ReadMenus(outcome.Data));
                OpenPendingForm();
            }, _ =>
            {
                formLoading = false;
                error = "加载上级菜单失败，请检查网络后重试。";
            });
        }

        private void OpenPendingForm()
        {
            if (pendingMode == "create" || pendingRow is null)
            {
                editDialogTitle = "新增菜单";
                ResetForm();
            }
            else
            {
                var copy = pendingMode == "copy";
                editDialogTitle = copy ? "复制菜单" : "编辑菜单";
                PopulateForm(pendingRow, copy);
            }

            pendingMode = string.Empty;
            pendingRow = null;
            editDialogVisible = true;
        }

        private void ResetForm()
        {
            editOriginal = null;
            editId = string.Empty;
            editPid = "0";
            editType = 2;
            editTitle = string.Empty;
            editName = string.Empty;
            editPath = string.Empty;
            editComponent = string.Empty;
            editIcon = string.Empty;
            editRedirect = string.Empty;
            editOutLink = string.Empty;
            editPermission = string.Empty;
            editOrderNo = 100;
            editStatus = 1;
            editIsHide = false;
            editIsKeepAlive = true;
            editIsAffix = false;
            editIsIframe = false;
            editRemark = string.Empty;
        }

        private void PopulateForm(MenuView row, bool copy)
        {
            editOriginal = copy ? null : row;
            editId = copy ? string.Empty : row.id;
            editTenantId = ResolveRowTenant(row);
            editPid = row.pid.Length == 0 ? "0" : row.pid;
            editType = NormalizeType(row.type);
            editTitle = copy ? string.Empty : row.title;
            editName = row.name;
            editPath = row.path;
            editComponent = row.component;
            editIcon = row.icon;
            editRedirect = row.redirect;
            editOutLink = row.outLink;
            editPermission = row.permission;
            editOrderNo = row.orderNo;
            editStatus = row.status == 2 ? 2 : 1;
            editIsHide = row.isHide;
            editIsKeepAlive = row.isKeepAlive;
            editIsAffix = row.isAffix;
            editIsIframe = row.isIframe;
            editRemark = row.remark;
        }

        private void ChangeEditType(int value)
            => editType = NormalizeType(value);

        private void Save()
        {
            if (saving)
                return;

            var normalizedTitle = editTitle.Trim();
            var normalizedName = editName.Trim();
            var normalizedPath = editPath.Trim();
            var normalizedComponent = editComponent.Trim();
            var normalizedIcon = editIcon.Trim();
            var normalizedRedirect = editRedirect.Trim();
            var normalizedOutLink = editOutLink.Trim();
            var normalizedPermission = editPermission.Trim();
            var normalizedRemark = editRemark.Trim();

            if (normalizedTitle.Length == 0)
            {
                error = "菜单名称不能为空。";
                return;
            }
            if (normalizedTitle.Length > 64)
            {
                error = "菜单名称不能超过64个字符。";
                return;
            }
            if (normalizedName.Length > 64 || normalizedPath.Length > 128 ||
                normalizedComponent.Length > 128 || normalizedIcon.Length > 128 ||
                normalizedRedirect.Length > 128 || normalizedPermission.Length > 128)
            {
                error = "路由名称不能超过64个字符，路径、组件、图标、重定向和权限标识不能超过128个字符。";
                return;
            }
            if (normalizedOutLink.Length > 256 || normalizedRemark.Length > 256)
            {
                error = "链接地址和备注不能超过256个字符。";
                return;
            }
            if (IsButtonType && normalizedPermission.Length == 0)
            {
                error = "按钮类型必须填写权限标识。";
                return;
            }
            if (IsButtonType && !normalizedPermission.Contains(':'))
            {
                error = "按钮权限标识必须包含冒号。";
                return;
            }
            if (IsSuperAdmin && editTenantId.Length == 0)
            {
                error = "超级管理员维护菜单时必须选择租户。";
                return;
            }

            var routeType = IsRouteType;
            var input = new MenuSaveInput
            {
                id = editId,
                pid = editPid.Length == 0 ? "0" : editPid,
                type = NormalizeType(editType),
                title = normalizedTitle,
                name = routeType ? normalizedName : string.Empty,
                path = routeType ? normalizedPath : string.Empty,
                component = routeType ? normalizedComponent : string.Empty,
                icon = routeType ? normalizedIcon : string.Empty,
                redirect = routeType ? normalizedRedirect : string.Empty,
                outLink = routeType ? normalizedOutLink : string.Empty,
                permission = routeType ? string.Empty : normalizedPermission,
                orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                status = editStatus == 2 ? 2 : 1,
                isHide = routeType && editIsHide,
                isKeepAlive = routeType && editIsKeepAlive,
                isAffix = routeType && editIsAffix,
                isIframe = routeType && editIsIframe,
                remark = normalizedRemark,
                tenantId = IsSuperAdmin ? editTenantId : DefaultTenantId
            };

            if (editId.Length > 0 && editOriginal is null)
            {
                error = "缺少菜单原始快照，请重新打开编辑窗口。";
                return;
            }

            saving = true;
            var request = editId.Length == 0
                ? MenuClient.Add(input)
                : MenuClient.Update(editOriginal!, input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存菜单失败。";
                    return;
                }

                editDialogVisible = false;
                editOriginal = null;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "菜单已新增" : "菜单已更新");
                LoadMenus();
            }, _ =>
            {
                saving = false;
                error = "保存菜单失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = FindVisibleMenu(id);
            if (row is null)
                return;

            if (!CanDelete)
                return;

            MenuClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除菜单失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("菜单已删除");
                LoadMenus();
            }, _ => error = "删除菜单失败，请检查网络后重试。");
        }

        private void ToggleStatus(string id)
        {
            var row = FindVisibleMenu(id);
            if (row is null)
                return;

            if (!CanSetStatus || statusLoadingId.Length > 0)
                return;

            var nextStatus = row.status == 1 ? 2 : 1;
            statusLoadingId = row.id;
            MenuClient.SetStatus(row.id, nextStatus).Then(outcome =>
            {
                statusLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "切换菜单状态失败。";
                    RebuildVisibleItems();
                    return;
                }

                row.status = nextStatus;
                error = null;
                ElementPlus.ElMessageMembers.Success(nextStatus == 1 ? "菜单状态已切换为启用" : "菜单状态已切换为停用");
                RebuildVisibleItems();
            }, _ =>
            {
                statusLoadingId = string.Empty;
                error = "切换菜单状态失败，请检查网络后重试。";
                RebuildVisibleItems();
            });
        }

        private string ResolveRowTenant(MenuView row)
        {
            if (row.tenantId.Length > 0)
                return row.tenantId;
            if (IsSuperAdmin && tenantId.Length > 0)
                return tenantId;
            return DefaultTenantId;
        }

        private void RebuildVisibleItems()
        {
            var count = CountVisible(menuTree);
            var result = new MenuFlatView[count];
            FillVisible(menuTree, 0, result, 0);
            visibleItems = result;
        }

        private int CountVisible(MenuView[] source)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
            {
                count += 1;
                if (source[index].children.Length > 0 && IsExpanded(source[index].id))
                    count += CountVisible(source[index].children);
            }
            return count;
        }

        private int FillVisible(MenuView[] source, int depth, MenuFlatView[] target, int writeIndex)
        {
            var current = writeIndex;
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                target[current] = new MenuFlatView { menu = row, depth = depth };
                current += 1;
                if (row.children.Length > 0 && IsExpanded(row.id))
                    current = FillVisible(row.children, depth + 1, target, current);
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
                count += CountAll(source[index].children);
            return count;
        }

        private static int FillAll(MenuView[] source, int depth, MenuFlatView[] target, int writeIndex)
        {
            var current = writeIndex;
            for (var index = 0; index < source.Length; index++)
            {
                target[current] = new MenuFlatView { menu = source[index], depth = depth };
                current += 1;
                current = FillAll(source[index].children, depth + 1, target, current);
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
                if (source[index].children.Length > 0)
                    count += 1;
                count += CountExpandable(source[index].children);
            }
            return count;
        }

        private static int FillExpandableIds(MenuView[] source, string[] target, int writeIndex)
        {
            var current = writeIndex;
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index].children.Length > 0)
                {
                    target[current] = source[index].id;
                    current += 1;
                }
                current = FillExpandableIds(source[index].children, target, current);
            }
            return current;
        }

        private bool IsExpanded(string id)
        {
            for (var index = 0; index < expandedIds.Length; index++)
                if (expandedIds[index] == id)
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
            var rows = new MenuTableRow[visibleItems.Length];
            for (var index = 0; index < visibleItems.Length; index++)
            {
                var flat = visibleItems[index];
                var row = flat.menu;
                var hasChildren = row.children.Length > 0;
                var expanded = IsExpanded(row.id);
                rows[index] = new MenuTableRow(
                    row.id, row.title, row.type, MenuTypeText(row.type),
                    DisplayValue(row.path), DisplayValue(row.component), DisplayValue(row.permission),
                    row.orderNo, row.status, hasChildren, expanded,
                    "padding-left:" + (flat.depth * 22) + "px",
                    !hasChildren ? "" : expanded ? "−" : "+",
                    ResolveMenuIcon(row.icon), MenuIconGlyph(row.icon));
            }
            return rows;
        }

        private ConfigurationOption[] TenantOptions()
        {
            var options = new ConfigurationOption[tenantItems.Length];
            for (var index = 0; index < tenantItems.Length; index++)
            {
                var tenant = tenantItems[index];
                options[index] = new ConfigurationOption(tenant.value, TenantLabel(tenant));
            }
            return options;
        }

        private ConfigurationOption[] ParentOptions()
        {
            var options = new ConfigurationOption[parentItems.Length + 1];
            options[0] = new ConfigurationOption("0", "顶级菜单");
            for (var index = 0; index < parentItems.Length; index++)
            {
                var parent = parentItems[index];
                options[index + 1] = new ConfigurationOption(parent.menu.id, ParentLabel(parent));
            }
            return options;
        }

        private MenuView? FindVisibleMenu(string id)
        {
            for (var index = 0; index < visibleItems.Length; index++)
            {
                if (visibleItems[index].menu.id == id)
                    return visibleItems[index].menu;
            }
            return null;
        }

        private static string ParentLabel(MenuFlatView row)
        {
            var prefix = string.Empty;
            for (var index = 0; index < row.depth; index++)
                prefix += "　";
            return prefix + row.menu.title + "（" + MenuTypeText(row.menu.type) + "）";
        }

        private static string DisplayValue(string value) => value.Length == 0 ? "-" : value;

        private static string TenantLabel(MenuTenantOption tenant)
            => tenant.host.Length == 0 ? tenant.label : tenant.label + " (" + tenant.host + ")";

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
            Loading = loading,
            FormLoading = formLoading,
            TenantLoading = tenantLoading,
            EditDialogVisible = editDialogVisible,
            Saving = saving,
            EditIsHide = editIsHide,
            EditIsKeepAlive = editIsKeepAlive,
            EditIsAffix = editIsAffix,
            EditIsIframe = editIsIframe,
            TenantId = tenantId,
            TitleFilter = titleFilter,
            TypeFilter = typeFilter,
            EditTenantId = editTenantId,
            EditPid = editPid,
            EditDialogTitle = editDialogTitle,
            EditTitle = editTitle,
            EditName = editName,
            EditPath = editPath,
            EditComponent = editComponent,
            EditIcon = editIcon,
            EditRedirect = editRedirect,
            EditOutLink = editOutLink,
            EditPermission = editPermission,
            EditRemark = editRemark,
            EditType = editType,
            EditStatus = editStatus,
            EditOrderNo = editOrderNo,
            Error = error,
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
            SetTitleFilter = value => titleFilter = value ?? "",
            SetEditTitle = value => editTitle = value ?? "",
            SetEditName = value => editName = value ?? "",
            SetEditPath = value => editPath = value ?? "",
            SetEditComponent = value => editComponent = value ?? "",
            SetEditIcon = value => editIcon = value ?? "",
            SetEditRedirect = value => editRedirect = value ?? "",
            SetEditOutLink = value => editOutLink = value ?? "",
            SetEditPermission = value => editPermission = value ?? "",
            SetEditRemark = value => editRemark = value ?? "",
            SetEditPid = value => editPid = string.IsNullOrEmpty(value) ? "0" : value,
            SetEditDialogVisible = value => editDialogVisible = value,
            ChangeEditType = value => ChangeEditType((int)value),
            SetEditStatus = value => editStatus = (int)value,
            SetEditOrderNo = value => editOrderNo = (int)(value ?? 100),
            ToggleEditIsHide = () => editIsHide = !editIsHide,
            ToggleEditIsKeepAlive = () => editIsKeepAlive = !editIsKeepAlive,
            ToggleEditIsAffix = () => editIsAffix = !editIsAffix,
            ToggleEditIsIframe = () => editIsIframe = !editIsIframe,
            DismissError = () => error = null,
            CancelEdit = () => editDialogVisible = false,
            StatusChanging = statusLoadingId.Length > 0,
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
