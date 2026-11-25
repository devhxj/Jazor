using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Auth;
using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Tenant;

[ECMAScriptModule("components/tenant-page-state.js")]
public static class TenantPageModule
{
    public static TenantPageState Create() => new();

    public sealed class TenantPageState : global::Jero.Web.App.IPageState<TenantPageProps>
    {
        private readonly JeroNavigation Navigation = new();

        private const string DefaultTenantId = "1300000000001";
        private const string LogoInputId = "jero-tenant-logo-input";
        private readonly double[] pageSizes = [10, 20, 50, 100];

        private TenantView[] items = [];
        private TenantView[] rows = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string queryName = string.Empty;
        private string queryPhone = string.Empty;
        private bool loading;
        private string busyId = string.Empty;
        private string busyAction = string.Empty;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增租户";
        private string editTab = "basic";
        private bool saving;
        private bool logoReading;
        private TenantView? editOriginal;
        private string editId = string.Empty;
        private string editName = string.Empty;
        private string editAdminAccount = string.Empty;
        private string editPhone = string.Empty;
        private string editEmail = string.Empty;
        private string editHost = string.Empty;
        private int editTenantType;
        private int editDbType;
        private string editConnection = string.Empty;
        private string editSlaveConnections = string.Empty;
        private int editOrderNo = 100;
        private int editStatus = 1;
        private string editRemark = string.Empty;
        private string editLogo = string.Empty;
        private string editLogoPreview = string.Empty;
        private string editLogoBase64 = string.Empty;
        private string editLogoFileName = string.Empty;
        private string editTitle = string.Empty;
        private string editViceTitle = string.Empty;
        private string editViceDesc = string.Empty;
        private string editWatermark = string.Empty;
        private string editCopyright = string.Empty;
        private string editIcp = string.Empty;
        private string editIcpUrl = string.Empty;

        private bool menuDialogVisible;
        private bool menuTreeLoading;
        private bool menuSelectionLoading;
        private bool menuSaving;
        private string menuTenantId = string.Empty;
        private string menuTenantName = string.Empty;
        private TenantMenuNode[] menuNodes = [];
        private string[] selectedMenuIds = [];
        private string[] pendingMenuIds = [];
        private int menuRequestVersion;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";
        private string auditRemark = "-";

        private bool passwordDialogVisible;
        private string passwordTenantName = string.Empty;
        private string resetPassword = string.Empty;

        private bool CanAdd => ApplicationState.HasPermission("sysTenant:add");
        private bool CanEdit => ApplicationState.HasPermission("sysTenant:update");
        private bool CanDelete => ApplicationState.HasPermission("sysTenant:delete");
        private bool CanGrantMenu => ApplicationState.HasPermission("sysTenant:grantMenu");
        private bool CanResetPassword => ApplicationState.HasPermission("sysTenant:resetPwd");
        private bool CanCreateDb => ApplicationState.HasPermission("sysTenant:createDb");
        private bool CanSetStatus => ApplicationState.HasPermission("sysTenant:setStatus");
        private bool CanSyncGrantMenu => ApplicationState.HasPermission("sysTenant:syncGrantMenu");
        private bool CanChangeTenant => ApplicationState.HasPermission("sysTenant:changeTenant");
        private bool CanGoTenant => ApplicationState.HasPermission("sysTenant:goTenant");
        private bool HasActionColumn => CanCreateDb || CanEdit || CanDelete || CanGrantMenu || CanResetPassword ||
            CanSyncGrantMenu || CanChangeTenant || CanGoTenant;
        private bool MenuBusy => menuTreeLoading || menuSelectionLoading;
        private bool SiteEnabled => editHost.Trim().Length > 0;

        public void Initialize() => LoadPage();

        private void LoadPage()
        {
            loading = true;
            TenantClient.Page(new TenantPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                name = queryName.Trim(),
                phone = queryPhone.Trim()
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }

                items = TenantClient.ReadItems(outcome.Data);
                rows = items;
                total = TenantClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载租户失败，请检查网络后重试。";
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
            queryPhone = string.Empty;
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
            editDialogTitle = "新增租户";
            editTab = "basic";
            editOriginal = null;
            editId = string.Empty;
            editName = string.Empty;
            editAdminAccount = string.Empty;
            editPhone = string.Empty;
            editEmail = string.Empty;
            editHost = string.Empty;
            editTenantType = 0;
            editDbType = 0;
            editConnection = string.Empty;
            editSlaveConnections = string.Empty;
            editOrderNo = 100;
            editStatus = 1;
            editRemark = string.Empty;
            editLogo = string.Empty;
            editLogoPreview = string.Empty;
            editLogoBase64 = string.Empty;
            editLogoFileName = string.Empty;
            editTitle = string.Empty;
            editViceTitle = string.Empty;
            editViceDesc = string.Empty;
            editWatermark = string.Empty;
            editCopyright = "Copyright © 2026-present Jero All rights reserved.";
            editIcp = string.Empty;
            editIcpUrl = string.Empty;
            TenantBrowserActions.ClearSelectedLogo(LogoInputId);
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            editDialogTitle = "编辑租户";
            editTab = "basic";
            editOriginal = row;
            editId = row.id;
            editName = row.name;
            editAdminAccount = row.adminAccount;
            editPhone = row.phone;
            editEmail = row.email;
            editHost = row.host;
            editTenantType = row.tenantType;
            editDbType = row.dbType;
            editConnection = row.connection;
            editSlaveConnections = row.slaveConnections;
            editOrderNo = row.orderNo;
            editStatus = row.status == 2 ? 2 : 1;
            editRemark = row.remark;
            editLogo = row.logo;
            editLogoPreview = NormalizeImageUrl(row.logo);
            editLogoBase64 = string.Empty;
            editLogoFileName = string.Empty;
            editTitle = row.title;
            editViceTitle = row.viceTitle;
            editViceDesc = row.viceDesc;
            editWatermark = row.watermark;
            editCopyright = row.copyright;
            editIcp = row.icp;
            editIcpUrl = row.icpUrl;
            TenantBrowserActions.ClearSelectedLogo(LogoInputId);
            editDialogVisible = true;
        }

        private void ReadSelectedLogo()
        {
            if (logoReading)
                return;
            logoReading = true;
            TenantBrowserActions.ReadSelectedLogo(LogoInputId).Then(outcome =>
            {
                logoReading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "Logo 图片读取失败。";
                    return;
                }

                editLogoFileName = outcome.FileName;
                editLogoBase64 = outcome.DataUrl;
                editLogoPreview = outcome.DataUrl;
                error = null;
            }, _ =>
            {
                logoReading = false;
                error = "Logo 图片读取失败。";
            });
        }

        private void ClearLogoSelection()
        {
            editLogoBase64 = string.Empty;
            editLogoFileName = string.Empty;
            editLogoPreview = NormalizeImageUrl(editLogo);
            TenantBrowserActions.ClearSelectedLogo(LogoInputId);
        }

        private void Save()
        {
            if (saving || logoReading)
                return;

            var normalizedName = editName.Trim();
            var normalizedAccount = editAdminAccount.Trim();
            var normalizedPhone = editPhone.Trim();
            var normalizedHost = editHost.Trim().ToLowerCase();
            if (normalizedName.Length < 2)
            {
                error = "租户名称不能少于2个字符。";
                return;
            }
            if (normalizedAccount.Length < 3)
            {
                error = "租管账号不能少于3个字符。";
                return;
            }
            if (normalizedPhone.Length == 0)
            {
                error = "联系电话不能为空。";
                return;
            }
            if (editTenantType == 1 && editConnection.Trim().Length == 0)
            {
                error = "库隔离租户必须填写数据库连接。";
                return;
            }
            if (normalizedHost.Length > 0 &&
                (editTitle.Trim().Length == 0 || editViceTitle.Trim().Length == 0 ||
                 editViceDesc.Trim().Length == 0 || editCopyright.Trim().Length == 0))
            {
                error = "启用站点域名时，标题、副标题、描述和版权信息不能为空。";
                editTab = "site";
                return;
            }
            if (normalizedHost.Length > 0 && editLogo.Length == 0 && editLogoBase64.Length == 0)
            {
                error = "启用站点域名时必须上传 Logo。";
                editTab = "site";
                return;
            }
            if (editIcp.Trim().Length > 0 && editIcpUrl.Trim().Length == 0)
            {
                error = "填写备案号后，ICP 地址不能为空。";
                editTab = "site";
                return;
            }

            var original = editOriginal;
            var input = new TenantSaveInput
            {
                id = original?.id ?? string.Empty,
                userId = original?.userId ?? string.Empty,
                orgId = original?.orgId ?? string.Empty,
                host = Optional(normalizedHost),
                tenantType = editTenantType == 1 ? 1 : 0,
                dbType = editDbType,
                connection = Optional(editConnection.Trim()),
                configId = Optional(original?.configId ?? string.Empty),
                slaveConnections = Optional(editSlaveConnections.Trim()),
                enableReg = original?.enableReg,
                regWayId = original?.regWayId,
                logo = Optional(editLogo),
                logoBase64 = Optional(editLogoBase64),
                logoFileName = Optional(editLogoFileName),
                title = Optional(editTitle.Trim()),
                viceTitle = Optional(editViceTitle.Trim()),
                viceDesc = Optional(editViceDesc.Trim()),
                watermark = Optional(editWatermark.Trim()),
                copyright = Optional(editCopyright.Trim()),
                icp = Optional(editIcp.Trim()),
                icpUrl = Optional(editIcpUrl.Trim()),
                orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                remark = Optional(editRemark.Trim()),
                status = editStatus == 2 ? 2 : 1,
                createTime = original?.createTime ?? string.Empty,
                createUserId = original?.createUserId,
                createUserName = original?.createUserName,
                updateTime = original?.updateTime,
                updateUserId = original?.updateUserId,
                updateUserName = original?.updateUserName,
                name = normalizedName,
                adminAccount = normalizedAccount,
                email = editEmail.Trim(),
                phone = normalizedPhone
            };

            var isCreate = editId.Length == 0;
            if (!isCreate && original is null)
            {
                error = "当前租户不存在。";
                return;
            }

            saving = true;
            var request = isCreate ? TenantClient.Add(input) : TenantClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存租户失败。";
                    return;
                }

                editDialogVisible = false;
                editOriginal = null;
                error = null;
                ElementPlus.ElMessageMembers.Success(isCreate ? "租户已新增" : "租户已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存租户失败，请检查网络后重试。";
            });
        }

        private void ToggleStatus(string id)
        {
            var row = Find(id);
            if (row is null || !CanSetStatus || Busy)
                return;
            var nextStatus = row.status == 1 ? 2 : 1;
            RunAction(row, "status", TenantClient.SetStatus(row.id, nextStatus),
                nextStatus == 1 ? "租户状态已切换为启用" : "租户状态已切换为停用", true);
        }

        private void CreateDatabase(string id)
        {
            var row = Find(id);
            if (row is null || !CanCreateDb || Busy)
                return;
            RunAction(row, "database", TenantClient.CreateDatabase(row.id), "租户数据库已创建或更新", false);
        }

        private void SyncGrantMenu(string id)
        {
            var row = Find(id);
            if (row is null || !CanSyncGrantMenu || Busy)
                return;
            RunAction(row, "sync", TenantClient.SyncGrantMenu(row.id), "授权数据已同步", false);
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || row.id == DefaultTenantId || Busy)
                return;
            RunAction(row, "delete", TenantClient.Delete(row.id), "租户已删除", true);
        }

        private void ResetPassword(string id)
        {
            var row = Find(id);
            if (row is null || row.userId.Length == 0 || !CanResetPassword || Busy)
                return;

            busyId = row.id;
            busyAction = "password";
            TenantClient.ResetPassword(row.userId).Then(outcome =>
            {
                ClearBusy();
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "重置租户管理员密码失败。";
                    return;
                }

                resetPassword = TenantClient.ReadPassword(outcome.Data);
                passwordTenantName = row.name;
                passwordDialogVisible = true;
                error = null;
            }, _ =>
            {
                ClearBusy();
                error = "重置租户管理员密码失败，请检查网络后重试。";
            });
        }

        private void ChangeTenant(string id) => SwitchTenant(id, false);

        private void GoTenant(string id) => SwitchTenant(id, true);

        private void SwitchTenant(string id, bool asTenantAdmin)
        {
            var row = Find(id);
            if (row is null || Busy || (asTenantAdmin ? !CanGoTenant : !CanChangeTenant))
                return;

            busyId = row.id;
            busyAction = asTenantAdmin ? "go" : "change";
            var request = asTenantAdmin ? TenantClient.GoTenant(row.id) : TenantClient.ChangeTenant(row.id);
            request.Then(outcome =>
            {
                ClearBusy();
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "切换租户上下文失败。";
                    return;
                }

                var login = TenantClient.ReadLoginResult(outcome.Data);
                if (login.accessToken.Length == 0 || login.refreshToken.Length == 0)
                {
                    error = "未获取到新的登录令牌，无法切换租户上下文。";
                    return;
                }

                BrowserSessionStore.SetTokens(login.accessToken, login.refreshToken);
                ApplicationState.Clear();
                var homeRoute = RouteRegistry.ResolveHome(login.homepage);
                Navigation.NavigateTo(Navigation.BaseUri + homeRoute);
            }, _ =>
            {
                ClearBusy();
                error = "切换租户上下文失败，请检查网络后重试。";
            });
        }

        private void RunAction(TenantView row, string action, IPromise<ApiOutcome> request, string success, bool refresh)
        {
            busyId = row.id;
            busyAction = action;
            request.Then(outcome =>
            {
                ClearBusy();
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "租户操作失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success(success);
                if (refresh)
                    LoadPage();
            }, _ =>
            {
                ClearBusy();
                error = "租户操作失败，请检查网络后重试。";
            });
        }

        private void OpenGrantMenu(string id)
        {
            var row = Find(id);
            if (row is null || !CanGrantMenu)
                return;

            menuTenantId = row.id;
            menuTenantName = row.name;
            menuNodes = [];
            selectedMenuIds = [];
            pendingMenuIds = [];
            menuDialogVisible = true;
            menuRequestVersion++;
            LoadMenuAuthorization(menuRequestVersion, row.id);
        }

        private void LoadMenuAuthorization(int requestVersion, string tenantId)
        {
            menuTreeLoading = true;
            menuSelectionLoading = true;
            TenantClient.MenuTree().Then(outcome =>
            {
                if (requestVersion != menuRequestVersion || tenantId != menuTenantId)
                    return;
                menuTreeLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载菜单树失败。";
                    menuNodes = [];
                    return;
                }
                menuNodes = TenantClient.ReadMenuTree(outcome.Data);
                selectedMenuIds = ExpandMenuSelection(menuNodes, pendingMenuIds);
            }, _ =>
            {
                if (requestVersion != menuRequestVersion || tenantId != menuTenantId)
                    return;
                menuTreeLoading = false;
                error = "加载菜单树失败，请检查网络后重试。";
            });

            TenantClient.OwnMenuList(tenantId).Then(outcome =>
            {
                if (requestVersion != menuRequestVersion || tenantId != menuTenantId)
                    return;
                menuSelectionLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户菜单失败。";
                    selectedMenuIds = [];
                    return;
                }
                pendingMenuIds = TenantClient.ReadIds(outcome.Data);
                selectedMenuIds = ExpandMenuSelection(menuNodes, pendingMenuIds);
            }, _ =>
            {
                if (requestVersion != menuRequestVersion || tenantId != menuTenantId)
                    return;
                menuSelectionLoading = false;
                error = "加载租户菜单失败，请检查网络后重试。";
            });
        }

        private void ToggleMenuNode(string id)
        {
            if (MenuBusy)
                return;
            selectedMenuIds = SetMenuSubtree(menuNodes, selectedMenuIds, id, !ContainsId(selectedMenuIds, id));
        }

        private void SaveMenu()
        {
            if (menuSaving || MenuBusy || menuTenantId.Length == 0)
                return;

            menuSaving = true;
            TenantClient.GrantMenu(menuTenantId, selectedMenuIds).Then(outcome =>
            {
                menuSaving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "更新租户菜单授权失败。";
                    return;
                }
                menuDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("租户菜单授权已更新");
            }, _ =>
            {
                menuSaving = false;
                error = "更新租户菜单授权失败，请检查网络后重试。";
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

        private TenantView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private void ClearBusy()
        {
            busyId = string.Empty;
            busyAction = string.Empty;
        }

        private bool Busy => busyId.Length > 0;

        private bool IsBusy(VueProps? row, string action)
            => busyId == RowString(row, "id") && busyAction == action;

        private static string[] ExpandMenuSelection(TenantMenuNode[] nodes, string[] selected)
        {
            string[] result = [];
            for (var index = 0; index < selected.Length; index++)
                result = ApplyMenuSubtree(nodes, result, selected[index], true);
            return NormalizeMenuAncestors(nodes, result);
        }

        private static string[] SetMenuSubtree(TenantMenuNode[] nodes, string[] selected, string id, bool enabled)
            => NormalizeMenuAncestors(nodes, ApplyMenuSubtree(nodes, selected, id, enabled));

        private static string[] ApplyMenuSubtree(TenantMenuNode[] nodes, string[] selected, string id, bool enabled)
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

        private static string[] NormalizeMenuAncestors(TenantMenuNode[] nodes, string[] selected)
        {
            var result = selected;
            for (var index = nodes.Length - 1; index >= 0; index--)
            {
                var node = nodes[index];
                if (!node.hasChildren)
                    continue;
                var hasChild = false;
                var allSelected = true;
                for (var childIndex = 0; childIndex < nodes.Length; childIndex++)
                {
                    if (nodes[childIndex].parentId != node.id)
                        continue;
                    hasChild = true;
                    if (!ContainsId(result, nodes[childIndex].id))
                        allSelected = false;
                }
                result = hasChild && allSelected ? AddId(result, node.id) : RemoveId(result, node.id);
            }
            return result;
        }

        private static bool IsDescendant(TenantMenuNode[] nodes, TenantMenuNode node, string ancestorId)
        {
            var parentId = node.parentId;
            while (parentId.Length > 0 && parentId != "0")
            {
                if (parentId == ancestorId)
                    return true;
                var parent = FindMenuNode(nodes, parentId);
                if (parent is null || parent.parentId == parentId)
                    return false;
                parentId = parent.parentId;
            }
            return false;
        }

        private static TenantMenuNode? FindMenuNode(TenantMenuNode[] nodes, string id)
        {
            for (var index = 0; index < nodes.Length; index++)
                if (nodes[index].id == id)
                    return nodes[index];
            return null;
        }

        private static bool HasSelectedDescendant(TenantMenuNode[] nodes, string[] selected, string id)
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
                result[target++] = values[index];
            }
            return result;
        }

        private string MenuCheckboxClass(TenantMenuNode node)
        {
            if (ContainsId(selectedMenuIds, node.id))
                return "tenant-tree__check is-checked";
            if (HasSelectedDescendant(menuNodes, selectedMenuIds, node.id))
                return "tenant-tree__check is-partial";
            return "tenant-tree__check";
        }

        private string MenuCheckMark(TenantMenuNode node)
        {
            if (ContainsId(selectedMenuIds, node.id))
                return "✓";
            return HasSelectedDescendant(menuNodes, selectedMenuIds, node.id) ? "−" : string.Empty;
        }

        private string MenuAriaChecked(TenantMenuNode node)
        {
            if (ContainsId(selectedMenuIds, node.id))
                return "true";
            return HasSelectedDescendant(menuNodes, selectedMenuIds, node.id) ? "mixed" : "false";
        }

        private static string TreeRowStyle(TenantMenuNode node)
            => "padding-left:" + (node.depth * 22 + 6) + "px";

        private static string RowString(VueProps? row, string key)
        {
            if (row is null)
                return string.Empty;
            var value = Reflect.Get(row, key);
            if (value is null)
                return string.Empty;
            var text = StringValue(value);
            return text == "null" || text == "undefined" ? string.Empty : text;
        }

        private static string TenantLogoUrl(VueProps? row)
            => NormalizeImageUrl(RowString(row, "logo"));

        private static string NormalizeImageUrl(string value)
        {
            if (value.Length == 0)
                return string.Empty;
            if (value.StartsWith("data:") || value.StartsWith("http://") || value.StartsWith("https://"))
                return value;

            // Seed data still references /upload/logo.png, but the shipped default asset is
            // /images/logo.png. Keep every custom upload path unchanged.
            if (value == "/upload/logo.png")
                return "/images/logo.png";
            return value.StartsWith('/') ? value : "/" + value;
        }

        private static int RowInt(VueProps? row, string key, int fallback)
        {
            if (row is null)
                return fallback;
            var value = Reflect.Get(row, key);
            return value is null ? fallback : (int)NumberValue(value);
        }

        private static string StatusText(VueProps? row)
            => RowInt(row, "status", 1) == 1 ? "启用" : "停用";

        private static string StatusTag(VueProps? row)
            => RowInt(row, "status", 1) == 1 ? "success" : "info";

        private static string StatusActionText(VueProps? row)
            => RowInt(row, "status", 1) == 1 ? "停用" : "启用";

        private static string TenantTypeText(VueProps? row)
            => RowInt(row, "tenantType", 0) == 1 ? "库隔离" : "ID隔离";

        private static string TenantTypeTag(VueProps? row)
            => RowInt(row, "tenantType", 0) == 1 ? "warning" : "success";

        private static string DbTypeText(VueProps? row)
            => DbTypeText(RowInt(row, "dbType", 0));

        private static string DbTypeText(int value)
        {
            string[] labels = ["MySql", "SqlServer", "Sqlite", "Oracle", "PostgreSQL", "Dm", "Kdbndp", "Oscar",
                "MySqlConnector", "Access", "OpenGauss", "QuestDB", "HG", "ClickHouse", "GBase", "Odbc",
                "OceanBaseForOracle", "TDengine", "GaussDB", "OceanBase", "Tidb", "Vastbase", "PolarDB", "Doris"];
            if (value == 900)
                return "Custom";
            return value >= 0 && value < labels.Length ? labels[value] : "未设置";
        }

        private static string DisplayUser(string? name, string? id)
        {
            var displayName = name ?? string.Empty;
            var displayId = id ?? string.Empty;
            if (displayName.Length > 0 && displayId.Length > 0)
                return displayName + " (" + displayId + ")";
            return displayName.Length > 0 ? displayName : DisplayValue(displayId);
        }

        private static string DisplayValue(string? value)
            => string.IsNullOrEmpty(value) ? "-" : value!;

        private static string? Optional(string value)
            => value.Length == 0 ? null : value;


        private TenantTableRow[] TableRows()
        {
            var result = new TenantTableRow[rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = rows[index];
                result[index] = new TenantTableRow(
                    row.id,
                    row.name,
                    row.adminAccount,
                    row.phone ?? string.Empty,
                    row.title ?? string.Empty,
                    row.viceTitle ?? string.Empty,
                    row.host ?? string.Empty,
                    row.configId ?? string.Empty,
                    row.icp ?? string.Empty,
                    row.watermark ?? string.Empty,
                    row.copyright ?? string.Empty,
                    row.connection ?? string.Empty,
                    row.slaveConnections ?? string.Empty,
                    TenantLogoUrl(row),
                    DbTypeText(row),
                    TenantTypeText(row),
                    TenantTypeTag(row),
                    StatusText(row),
                    StatusTag(row),
                    StatusActionText(row));
            }
            return result;
        }

        private TenantMenuRow[] MenuRows()
        {
            var result = new TenantMenuRow[menuNodes.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var node = menuNodes[index];
                result[index] = new TenantMenuRow(node.id, node.label, node.path, node.depth, TreeRowStyle(node),
                    MenuCheckboxClass(node), MenuAriaChecked(node), MenuCheckMark(node));
            }
            return result;
        }

        private readonly ConfigurationOption[] DbOptions =
        [
            new ConfigurationOption("0", "MySql"),
            new ConfigurationOption("1", "SqlServer"),
            new ConfigurationOption("2", "Sqlite"),
            new ConfigurationOption("3", "Oracle"),
            new ConfigurationOption("4", "PostgreSQL"),
            new ConfigurationOption("5", "Dm"),
            new ConfigurationOption("6", "Kdbndp"),
            new ConfigurationOption("7", "Oscar"),
            new ConfigurationOption("8", "MySqlConnector"),
            new ConfigurationOption("9", "Access"),
            new ConfigurationOption("10", "OpenGauss"),
            new ConfigurationOption("11", "QuestDB"),
            new ConfigurationOption("12", "HG"),
            new ConfigurationOption("13", "ClickHouse"),
            new ConfigurationOption("14", "GBase"),
            new ConfigurationOption("15", "Odbc"),
            new ConfigurationOption("16", "OceanBaseForOracle"),
            new ConfigurationOption("17", "TDengine"),
            new ConfigurationOption("18", "GaussDB"),
            new ConfigurationOption("19", "OceanBase"),
            new ConfigurationOption("20", "Tidb"),
            new ConfigurationOption("21", "Vastbase"),
            new ConfigurationOption("22", "PolarDB"),
            new ConfigurationOption("23", "Doris"),
            new ConfigurationOption("900", "Custom")
        ];

        public TenantPageProps CreateProps() => new()
        {
            Rows = TableRows(),
            CanSetStatus = CanSetStatus,
            HasActionColumn = HasActionColumn,
            CanCreateDb = CanCreateDb,
            CanGrantMenu = CanGrantMenu,
            CanSyncGrantMenu = CanSyncGrantMenu,
            CanGoTenant = CanGoTenant,
            CanChangeTenant = CanChangeTenant,
            CanResetPassword = CanResetPassword,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            Busy = Busy,
            BusyId = busyId,
            BusyAction = busyAction,
            DefaultTenantId = DefaultTenantId,
            ToggleStatus = ToggleStatus,
            OpenAudit = OpenAudit,
            CreateDatabase = CreateDatabase,
            OpenGrantMenu = OpenGrantMenu,
            SyncGrantMenu = SyncGrantMenu,
            GoTenant = GoTenant,
            ChangeTenant = ChangeTenant,
            ResetPassword = ResetPassword,
            OpenEdit = OpenEdit,
            Delete = Delete,
            EditDbType = editDbType.ToString(),
            DbOptions = DbOptions,
            SetEditDbType = value => editDbType = (int)ECMAScript.Global.NumberValue(value),
            EditLogoFileNameText = DisplayValue(editLogoFileName),
            ResetPasswordText = DisplayValue(resetPassword),
            ToggleMenuNode = ToggleMenuNode,
            ReadSelectedLogo = ReadSelectedLogo,
            MenuNodes = MenuRows(),
            EditTenantType = editTenantType,
            SetEditTenantType = value => editTenantType = (int)value,
            EditStatus = editStatus,
            SetEditStatus = value => editStatus = (int)value,
            EditOrderNo = editOrderNo,
            SetEditOrderNo = value => editOrderNo = (int)(value ?? 100),
            SetQueryName = value => queryName = value ?? "",
            SetQueryPhone = value => queryPhone = value ?? "",
            SetEditTab = value => editTab = value ?? "basic",
            SetEditName = value => editName = value ?? "",
            SetEditAdminAccount = value => editAdminAccount = value ?? "",
            SetEditPhone = value => editPhone = value ?? "",
            SetEditEmail = value => editEmail = value ?? "",
            SetEditHost = value => editHost = value ?? "",
            SetEditRemark = value => editRemark = value ?? "",
            SetEditConnection = value => editConnection = value ?? "",
            SetEditSlaveConnections = value => editSlaveConnections = value ?? "",
            SetEditTitle = value => editTitle = value ?? "",
            SetEditViceTitle = value => editViceTitle = value ?? "",
            SetEditViceDesc = value => editViceDesc = value ?? "",
            SetEditCopyright = value => editCopyright = value ?? "",
            SetEditIcp = value => editIcp = value ?? "",
            SetEditIcpUrl = value => editIcpUrl = value ?? "",
            SetEditWatermark = value => editWatermark = value ?? "",
            SetEditDialogVisible = value => editDialogVisible = value,
            SetMenuDialogVisible = value => menuDialogVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            SetPasswordDialogVisible = value => passwordDialogVisible = value,
            CloseEditDialogVisible = () => editDialogVisible = false,
            CloseMenuDialogVisible = () => menuDialogVisible = false,
            ClosePasswordDialogVisible = () => passwordDialogVisible = false,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPage = LoadPage,
            ClearLogoSelection = ClearLogoSelection,
            Save = Save,
            DismissError = () => error = null,
            SaveMenu = SaveMenu,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            Editing = editId.Length > 0,
            Error = error,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            PageSizes = pageSizes,
            Loading = loading,
            Saving = saving,
            LogoReading = logoReading,
            MenuSaving = menuSaving,
            EditDialogVisible = editDialogVisible,
            MenuDialogVisible = menuDialogVisible,
            AuditDialogVisible = auditDialogVisible,
            PasswordDialogVisible = passwordDialogVisible,
            CanAdd = CanAdd,
            MenuBusy = MenuBusy,
            SiteEnabled = SiteEnabled,
            QueryName = queryName,
            QueryPhone = queryPhone,
            EditDialogTitle = editDialogTitle,
            EditTab = editTab,
            EditName = editName,
            EditAdminAccount = editAdminAccount,
            EditPhone = editPhone,
            EditEmail = editEmail,
            EditHost = editHost,
            EditRemark = editRemark,
            EditConnection = editConnection,
            EditSlaveConnections = editSlaveConnections,
            EditLogoPreview = editLogoPreview,
            EditLogoBase64 = editLogoBase64,
            EditTitle = editTitle,
            EditViceTitle = editViceTitle,
            EditViceDesc = editViceDesc,
            EditCopyright = editCopyright,
            EditIcp = editIcp,
            EditIcpUrl = editIcpUrl,
            EditWatermark = editWatermark,
            MenuTenantName = menuTenantName,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            AuditRemark = auditRemark,
            PasswordTenantName = passwordTenantName,
            LogoInputId = LogoInputId,
        };
    }
}

public sealed record TenantPageProps : VueProps
{
    public required TenantTableRow[] Rows { get; init; }
    public required bool CanSetStatus { get; init; }
    public required bool HasActionColumn { get; init; }
    public required bool CanCreateDb { get; init; }
    public required bool CanGrantMenu { get; init; }
    public required bool CanSyncGrantMenu { get; init; }
    public required bool CanGoTenant { get; init; }
    public required bool CanChangeTenant { get; init; }
    public required bool CanResetPassword { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool Busy { get; init; }
    public required string BusyId { get; init; }
    public required string BusyAction { get; init; }
    public required string DefaultTenantId { get; init; }
    public required Action<string> ToggleStatus { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> CreateDatabase { get; init; }
    public required Action<string> OpenGrantMenu { get; init; }
    public required Action<string> SyncGrantMenu { get; init; }
    public required Action<string> GoTenant { get; init; }
    public required Action<string> ChangeTenant { get; init; }
    public required Action<string> ResetPassword { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Delete { get; init; }
    public required string EditDbType { get; init; }
    public required ConfigurationOption[] DbOptions { get; init; }
    public required Action<string> SetEditDbType { get; init; }
    public required string EditLogoFileNameText { get; init; }
    public required string ResetPasswordText { get; init; }
    public required Action<string> ToggleMenuNode { get; init; }
    public required Action ReadSelectedLogo { get; init; }
    public required TenantMenuRow[] MenuNodes { get; init; }
    public required int EditTenantType { get; init; }
    public required Action<double> SetEditTenantType { get; init; }
    public required int EditStatus { get; init; }
    public required Action<double> SetEditStatus { get; init; }
    public required int EditOrderNo { get; init; }
    public required Action<double?> SetEditOrderNo { get; init; }
    public required Action<string?> SetQueryName { get; init; }
    public required Action<string?> SetQueryPhone { get; init; }
    public required Action<string?> SetEditTab { get; init; }
    public required Action<string?> SetEditName { get; init; }
    public required Action<string?> SetEditAdminAccount { get; init; }
    public required Action<string?> SetEditPhone { get; init; }
    public required Action<string?> SetEditEmail { get; init; }
    public required Action<string?> SetEditHost { get; init; }
    public required Action<string?> SetEditRemark { get; init; }
    public required Action<string?> SetEditConnection { get; init; }
    public required Action<string?> SetEditSlaveConnections { get; init; }
    public required Action<string?> SetEditTitle { get; init; }
    public required Action<string?> SetEditViceTitle { get; init; }
    public required Action<string?> SetEditViceDesc { get; init; }
    public required Action<string?> SetEditCopyright { get; init; }
    public required Action<string?> SetEditIcp { get; init; }
    public required Action<string?> SetEditIcpUrl { get; init; }
    public required Action<string?> SetEditWatermark { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetMenuDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action<bool> SetPasswordDialogVisible { get; init; }
    public required Action CloseEditDialogVisible { get; init; }
    public required Action CloseMenuDialogVisible { get; init; }
    public required Action ClosePasswordDialogVisible { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action LoadPage { get; init; }
    public required Action ClearLogoSelection { get; init; }
    public required Action Save { get; init; }
    public required Action DismissError { get; init; }
    public required Action SaveMenu { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required bool Editing { get; init; }
    public required string? Error { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required double[] PageSizes { get; init; }
    public required bool Loading { get; init; }
    public required bool Saving { get; init; }
    public required bool LogoReading { get; init; }
    public required bool MenuSaving { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool MenuDialogVisible { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required bool PasswordDialogVisible { get; init; }
    public required bool CanAdd { get; init; }
    public required bool MenuBusy { get; init; }
    public required bool SiteEnabled { get; init; }
    public required string QueryName { get; init; }
    public required string QueryPhone { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditTab { get; init; }
    public required string EditName { get; init; }
    public required string EditAdminAccount { get; init; }
    public required string EditPhone { get; init; }
    public required string EditEmail { get; init; }
    public required string EditHost { get; init; }
    public required string EditRemark { get; init; }
    public required string EditConnection { get; init; }
    public required string EditSlaveConnections { get; init; }
    public required string EditLogoPreview { get; init; }
    public required string EditLogoBase64 { get; init; }
    public required string EditTitle { get; init; }
    public required string EditViceTitle { get; init; }
    public required string EditViceDesc { get; init; }
    public required string EditCopyright { get; init; }
    public required string EditIcp { get; init; }
    public required string EditIcpUrl { get; init; }
    public required string EditWatermark { get; init; }
    public required string MenuTenantName { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required string AuditRemark { get; init; }
    public required string PasswordTenantName { get; init; }
    public required string LogoInputId { get; init; }
}
