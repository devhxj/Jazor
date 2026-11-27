using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Tenant;
using Jero.Web.Infrastructure.Storage;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Pages.Auth;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Tenant;

[ECMAScriptModule("components/tenant-page-state.js")]
public static class TenantPageModule
{
    public static TenantPageState Create() => new();

    public sealed class TenantPageState : global::Jero.Web.Composition.IPageState<TenantPageProps>
    {
        private readonly JeroNavigation _navigation = new();

        private const string DefaultTenantId = "1300000000001";
        private const string LogoInputId = "jero-tenant-logo-input";
        private readonly double[] _pageSizes = [10, 20, 50, 100];

        private TenantView[] _items = [];
        private TenantView[] _rows = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _queryName = string.Empty;
        private string _queryPhone = string.Empty;
        private bool _loading;
        private string _busyId = string.Empty;
        private string _busyAction = string.Empty;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增租户";
        private string _editTab = "basic";
        private bool _saving;
        private bool _logoReading;
        private TenantView? _editOriginal;
        private string _editId = string.Empty;
        private string _editName = string.Empty;
        private string _editAdminAccount = string.Empty;
        private string _editPhone = string.Empty;
        private string _editEmail = string.Empty;
        private string _editHost = string.Empty;
        private int _editTenantType;
        private int _editDbType;
        private string _editConnection = string.Empty;
        private string _editSlaveConnections = string.Empty;
        private int _editOrderNo = 100;
        private int _editStatus = 1;
        private string _editRemark = string.Empty;
        private string _editLogo = string.Empty;
        private string _editLogoPreview = string.Empty;
        private string _editLogoBase64 = string.Empty;
        private string _editLogoFileName = string.Empty;
        private string _editTitle = string.Empty;
        private string _editViceTitle = string.Empty;
        private string _editViceDesc = string.Empty;
        private string _editWatermark = string.Empty;
        private string _editCopyright = string.Empty;
        private string _editIcp = string.Empty;
        private string _editIcpUrl = string.Empty;

        private bool _menuDialogVisible;
        private bool _menuTreeLoading;
        private bool _menuSelectionLoading;
        private bool _menuSaving;
        private string _menuTenantId = string.Empty;
        private string _menuTenantName = string.Empty;
        private TenantMenuNode[] _menuNodes = [];
        private string[] _selectedMenuIds = [];
        private string[] _pendingMenuIds = [];
        private int _menuRequestVersion;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";
        private string _auditRemark = "-";

        private bool _passwordDialogVisible;
        private string _passwordTenantName = string.Empty;
        private string _resetPassword = string.Empty;

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
        private bool MenuBusy => _menuTreeLoading || _menuSelectionLoading;
        private bool SiteEnabled => _editHost.Trim().Length > 0;

        public void Initialize() => LoadPage();

        private void LoadPage()
        {
            _loading = true;
            TenantClient.Page(new TenantPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Name = _queryName.Trim(),
                Phone = _queryPhone.Trim()
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }

                _items = TenantClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = TenantClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载租户失败，请检查网络后重试。";
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
            _queryPhone = string.Empty;
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
            _editDialogTitle = "新增租户";
            _editTab = "basic";
            _editOriginal = null;
            _editId = string.Empty;
            _editName = string.Empty;
            _editAdminAccount = string.Empty;
            _editPhone = string.Empty;
            _editEmail = string.Empty;
            _editHost = string.Empty;
            _editTenantType = 0;
            _editDbType = 0;
            _editConnection = string.Empty;
            _editSlaveConnections = string.Empty;
            _editOrderNo = 100;
            _editStatus = 1;
            _editRemark = string.Empty;
            _editLogo = string.Empty;
            _editLogoPreview = string.Empty;
            _editLogoBase64 = string.Empty;
            _editLogoFileName = string.Empty;
            _editTitle = string.Empty;
            _editViceTitle = string.Empty;
            _editViceDesc = string.Empty;
            _editWatermark = string.Empty;
            _editCopyright = "Copyright © 2026-present Jero All rights reserved.";
            _editIcp = string.Empty;
            _editIcpUrl = string.Empty;
            TenantBrowserActions.ClearSelectedLogo(LogoInputId);
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _editDialogTitle = "编辑租户";
            _editTab = "basic";
            _editOriginal = row;
            _editId = row.Id;
            _editName = row.Name;
            _editAdminAccount = row.AdminAccount;
            _editPhone = row.Phone;
            _editEmail = row.Email;
            _editHost = row.Host;
            _editTenantType = row.TenantType;
            _editDbType = row.DbType;
            _editConnection = row.Connection;
            _editSlaveConnections = row.SlaveConnections;
            _editOrderNo = row.OrderNo;
            _editStatus = row.Status == 2 ? 2 : 1;
            _editRemark = row.Remark;
            _editLogo = row.Logo;
            _editLogoPreview = NormalizeImageUrl(row.Logo);
            _editLogoBase64 = string.Empty;
            _editLogoFileName = string.Empty;
            _editTitle = row.Title;
            _editViceTitle = row.ViceTitle;
            _editViceDesc = row.ViceDesc;
            _editWatermark = row.Watermark;
            _editCopyright = row.Copyright;
            _editIcp = row.Icp;
            _editIcpUrl = row.IcpUrl;
            TenantBrowserActions.ClearSelectedLogo(LogoInputId);
            _editDialogVisible = true;
        }

        private void ReadSelectedLogo()
        {
            if (_logoReading)
                return;
            _logoReading = true;
            TenantBrowserActions.ReadSelectedLogo(LogoInputId).Then(outcome =>
            {
                _logoReading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "Logo 图片读取失败。";
                    return;
                }

                _editLogoFileName = outcome.FileName;
                _editLogoBase64 = outcome.DataUrl;
                _editLogoPreview = outcome.DataUrl;
                _error = null;
            }, _ =>
            {
                _logoReading = false;
                _error = "Logo 图片读取失败。";
            });
        }

        private void ClearLogoSelection()
        {
            _editLogoBase64 = string.Empty;
            _editLogoFileName = string.Empty;
            _editLogoPreview = NormalizeImageUrl(_editLogo);
            TenantBrowserActions.ClearSelectedLogo(LogoInputId);
        }

        private void Save()
        {
            if (_saving || _logoReading)
                return;

            var normalizedName = _editName.Trim();
            var normalizedAccount = _editAdminAccount.Trim();
            var normalizedPhone = _editPhone.Trim();
            var normalizedHost = _editHost.Trim().ToLowerCase();
            if (normalizedName.Length < 2)
            {
                _error = "租户名称不能少于2个字符。";
                return;
            }
            if (normalizedAccount.Length < 3)
            {
                _error = "租管账号不能少于3个字符。";
                return;
            }
            if (normalizedPhone.Length == 0)
            {
                _error = "联系电话不能为空。";
                return;
            }
            if (_editTenantType == 1 && _editConnection.Trim().Length == 0)
            {
                _error = "库隔离租户必须填写数据库连接。";
                return;
            }
            if (normalizedHost.Length > 0 &&
                (_editTitle.Trim().Length == 0 || _editViceTitle.Trim().Length == 0 ||
                 _editViceDesc.Trim().Length == 0 || _editCopyright.Trim().Length == 0))
            {
                _error = "启用站点域名时，标题、副标题、描述和版权信息不能为空。";
                _editTab = "site";
                return;
            }
            if (normalizedHost.Length > 0 && _editLogo.Length == 0 && _editLogoBase64.Length == 0)
            {
                _error = "启用站点域名时必须上传 Logo。";
                _editTab = "site";
                return;
            }
            if (_editIcp.Trim().Length > 0 && _editIcpUrl.Trim().Length == 0)
            {
                _error = "填写备案号后，ICP 地址不能为空。";
                _editTab = "site";
                return;
            }

            var original = _editOriginal;
            var input = new TenantSaveInput
            {
                Id = original?.Id ?? string.Empty,
                UserId = original?.UserId ?? string.Empty,
                OrgId = original?.OrgId ?? string.Empty,
                Host = Optional(normalizedHost),
                TenantType = _editTenantType == 1 ? 1 : 0,
                DbType = _editDbType,
                Connection = Optional(_editConnection.Trim()),
                ConfigId = Optional(original?.ConfigId ?? string.Empty),
                SlaveConnections = Optional(_editSlaveConnections.Trim()),
                EnableReg = original?.EnableReg,
                RegWayId = original?.RegWayId,
                Logo = Optional(_editLogo),
                LogoBase64 = Optional(_editLogoBase64),
                LogoFileName = Optional(_editLogoFileName),
                Title = Optional(_editTitle.Trim()),
                ViceTitle = Optional(_editViceTitle.Trim()),
                ViceDesc = Optional(_editViceDesc.Trim()),
                Watermark = Optional(_editWatermark.Trim()),
                Copyright = Optional(_editCopyright.Trim()),
                Icp = Optional(_editIcp.Trim()),
                IcpUrl = Optional(_editIcpUrl.Trim()),
                OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                Remark = Optional(_editRemark.Trim()),
                Status = _editStatus == 2 ? 2 : 1,
                CreateTime = original?.CreateTime ?? string.Empty,
                CreateUserId = original?.CreateUserId,
                CreateUserName = original?.CreateUserName,
                UpdateTime = original?.UpdateTime,
                UpdateUserId = original?.UpdateUserId,
                UpdateUserName = original?.UpdateUserName,
                Name = normalizedName,
                AdminAccount = normalizedAccount,
                Email = _editEmail.Trim(),
                Phone = normalizedPhone
            };

            var isCreate = _editId.Length == 0;
            if (!isCreate && original is null)
            {
                _error = "当前租户不存在。";
                return;
            }

            _saving = true;
            var request = isCreate ? TenantClient.Add(input) : TenantClient.Update(input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存租户失败。";
                    return;
                }

                _editDialogVisible = false;
                _editOriginal = null;
                _error = null;
                ElementPlus.ElMessageMembers.Success(isCreate ? "租户已新增" : "租户已更新");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存租户失败，请检查网络后重试。";
            });
        }

        private void ToggleStatus(string id)
        {
            var row = Find(id);
            if (row is null || !CanSetStatus || Busy)
                return;
            var nextStatus = row.Status == 1 ? 2 : 1;
            RunAction(row, "status", TenantClient.SetStatus(row.Id, nextStatus),
                nextStatus == 1 ? "租户状态已切换为启用" : "租户状态已切换为停用", true);
        }

        private void CreateDatabase(string id)
        {
            var row = Find(id);
            if (row is null || !CanCreateDb || Busy)
                return;
            RunAction(row, "database", TenantClient.CreateDatabase(row.Id), "租户数据库已创建或更新", false);
        }

        private void SyncGrantMenu(string id)
        {
            var row = Find(id);
            if (row is null || !CanSyncGrantMenu || Busy)
                return;
            RunAction(row, "sync", TenantClient.SyncGrantMenu(row.Id), "授权数据已同步", false);
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || row.Id == DefaultTenantId || Busy)
                return;
            RunAction(row, "delete", TenantClient.Delete(row.Id), "租户已删除", true);
        }

        private void ResetPassword(string id)
        {
            var row = Find(id);
            if (row is null || row.UserId.Length == 0 || !CanResetPassword || Busy)
                return;

            _busyId = row.Id;
            _busyAction = "password";
            TenantClient.ResetPassword(row.UserId).Then(outcome =>
            {
                ClearBusy();
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "重置租户管理员密码失败。";
                    return;
                }

                _resetPassword = TenantClient.ReadPassword(outcome.Data);
                _passwordTenantName = row.Name;
                _passwordDialogVisible = true;
                _error = null;
            }, _ =>
            {
                ClearBusy();
                _error = "重置租户管理员密码失败，请检查网络后重试。";
            });
        }

        private void ChangeTenant(string id) => SwitchTenant(id, false);

        private void GoTenant(string id) => SwitchTenant(id, true);

        private void SwitchTenant(string id, bool asTenantAdmin)
        {
            var row = Find(id);
            if (row is null || Busy || (asTenantAdmin ? !CanGoTenant : !CanChangeTenant))
                return;

            _busyId = row.Id;
            _busyAction = asTenantAdmin ? "go" : "change";
            var request = asTenantAdmin ? TenantClient.GoTenant(row.Id) : TenantClient.ChangeTenant(row.Id);
            request.Then(outcome =>
            {
                ClearBusy();
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "切换租户上下文失败。";
                    return;
                }

                var login = TenantClient.ReadLoginResult(outcome.Data);
                if (login.AccessToken.Length == 0 || login.RefreshToken.Length == 0)
                {
                    _error = "未获取到新的登录令牌，无法切换租户上下文。";
                    return;
                }

                BrowserSessionStore.SetTokens(login.AccessToken, login.RefreshToken);
                ApplicationState.Clear();
                var homeRoute = RouteRegistry.ResolveHome(login.Homepage);
                _navigation.NavigateTo(_navigation.BaseUri + homeRoute);
            }, _ =>
            {
                ClearBusy();
                _error = "切换租户上下文失败，请检查网络后重试。";
            });
        }

        private void RunAction(TenantView row, string action, IPromise<ApiOutcome> request, string success, bool refresh)
        {
            _busyId = row.Id;
            _busyAction = action;
            request.Then(outcome =>
            {
                ClearBusy();
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "租户操作失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success(success);
                if (refresh)
                    LoadPage();
            }, _ =>
            {
                ClearBusy();
                _error = "租户操作失败，请检查网络后重试。";
            });
        }

        private void OpenGrantMenu(string id)
        {
            var row = Find(id);
            if (row is null || !CanGrantMenu)
                return;

            _menuTenantId = row.Id;
            _menuTenantName = row.Name;
            _menuNodes = [];
            _selectedMenuIds = [];
            _pendingMenuIds = [];
            _menuDialogVisible = true;
            _menuRequestVersion++;
            LoadMenuAuthorization(_menuRequestVersion, row.Id);
        }

        private void LoadMenuAuthorization(int requestVersion, string tenantId)
        {
            _menuTreeLoading = true;
            _menuSelectionLoading = true;
            TenantClient.MenuTree().Then(outcome =>
            {
                if (requestVersion != _menuRequestVersion || tenantId != _menuTenantId)
                    return;
                _menuTreeLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载菜单树失败。";
                    _menuNodes = [];
                    return;
                }
                _menuNodes = TenantClient.ReadMenuTree(outcome.Data);
                _selectedMenuIds = ExpandMenuSelection(_menuNodes, _pendingMenuIds);
            }, _ =>
            {
                if (requestVersion != _menuRequestVersion || tenantId != _menuTenantId)
                    return;
                _menuTreeLoading = false;
                _error = "加载菜单树失败，请检查网络后重试。";
            });

            TenantClient.OwnMenuList(tenantId).Then(outcome =>
            {
                if (requestVersion != _menuRequestVersion || tenantId != _menuTenantId)
                    return;
                _menuSelectionLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户菜单失败。";
                    _selectedMenuIds = [];
                    return;
                }
                _pendingMenuIds = TenantClient.ReadIds(outcome.Data);
                _selectedMenuIds = ExpandMenuSelection(_menuNodes, _pendingMenuIds);
            }, _ =>
            {
                if (requestVersion != _menuRequestVersion || tenantId != _menuTenantId)
                    return;
                _menuSelectionLoading = false;
                _error = "加载租户菜单失败，请检查网络后重试。";
            });
        }

        private void ToggleMenuNode(string id)
        {
            if (MenuBusy)
                return;
            _selectedMenuIds = SetMenuSubtree(_menuNodes, _selectedMenuIds, id, !ContainsId(_selectedMenuIds, id));
        }

        private void SaveMenu()
        {
            if (_menuSaving || MenuBusy || _menuTenantId.Length == 0)
                return;

            _menuSaving = true;
            TenantClient.GrantMenu(_menuTenantId, _selectedMenuIds).Then(outcome =>
            {
                _menuSaving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "更新租户菜单授权失败。";
                    return;
                }
                _menuDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success("租户菜单授权已更新");
            }, _ =>
            {
                _menuSaving = false;
                _error = "更新租户菜单授权失败，请检查网络后重试。";
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

        private TenantView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private void ClearBusy()
        {
            _busyId = string.Empty;
            _busyAction = string.Empty;
        }

        private bool Busy => _busyId.Length > 0;

        private bool IsBusy(VueProps? row, string action)
            => _busyId == RowString(row, "id") && _busyAction == action;

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
                if (node.Id != id && !IsDescendant(nodes, node, id))
                    continue;
                result = enabled ? AddId(result, node.Id) : RemoveId(result, node.Id);
            }
            return result;
        }

        private static string[] NormalizeMenuAncestors(TenantMenuNode[] nodes, string[] selected)
        {
            var result = selected;
            for (var index = nodes.Length - 1; index >= 0; index--)
            {
                var node = nodes[index];
                if (!node.HasChildren)
                    continue;
                var hasChild = false;
                var allSelected = true;
                for (var childIndex = 0; childIndex < nodes.Length; childIndex++)
                {
                    if (nodes[childIndex].ParentId != node.Id)
                        continue;
                    hasChild = true;
                    if (!ContainsId(result, nodes[childIndex].Id))
                        allSelected = false;
                }
                result = hasChild && allSelected ? AddId(result, node.Id) : RemoveId(result, node.Id);
            }
            return result;
        }

        private static bool IsDescendant(TenantMenuNode[] nodes, TenantMenuNode node, string ancestorId)
        {
            var parentId = node.ParentId;
            while (parentId.Length > 0 && parentId != "0")
            {
                if (parentId == ancestorId)
                    return true;
                var parent = FindMenuNode(nodes, parentId);
                if (parent is null || parent.ParentId == parentId)
                    return false;
                parentId = parent.ParentId;
            }
            return false;
        }

        private static TenantMenuNode? FindMenuNode(TenantMenuNode[] nodes, string id)
        {
            for (var index = 0; index < nodes.Length; index++)
                if (nodes[index].Id == id)
                    return nodes[index];
            return null;
        }

        private static bool HasSelectedDescendant(TenantMenuNode[] nodes, string[] selected, string id)
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
                result[target++] = values[index];
            }
            return result;
        }

        private string MenuCheckboxClass(TenantMenuNode node)
        {
            if (ContainsId(_selectedMenuIds, node.Id))
                return "tenant-tree__check is-checked";
            if (HasSelectedDescendant(_menuNodes, _selectedMenuIds, node.Id))
                return "tenant-tree__check is-partial";
            return "tenant-tree__check";
        }

        private string MenuCheckMark(TenantMenuNode node)
        {
            if (ContainsId(_selectedMenuIds, node.Id))
                return "✓";
            return HasSelectedDescendant(_menuNodes, _selectedMenuIds, node.Id) ? "−" : string.Empty;
        }

        private string MenuAriaChecked(TenantMenuNode node)
        {
            if (ContainsId(_selectedMenuIds, node.Id))
                return "true";
            return HasSelectedDescendant(_menuNodes, _selectedMenuIds, node.Id) ? "mixed" : "false";
        }

        private static string TreeRowStyle(TenantMenuNode node)
            => "padding-left:" + (node.Depth * 22 + 6) + "px";

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

            // 种子数据仍指向 /upload/logo.png，打包的默认图标在 /images/logo.png。
            // 自定义上传路径应按原样使用。
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
            var result = new TenantTableRow[_rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _rows[index];
                result[index] = new TenantTableRow(
                    row.Id,
                    row.Name,
                    row.AdminAccount,
                    row.Phone ?? string.Empty,
                    row.Title ?? string.Empty,
                    row.ViceTitle ?? string.Empty,
                    row.Host ?? string.Empty,
                    row.ConfigId ?? string.Empty,
                    row.Icp ?? string.Empty,
                    row.Watermark ?? string.Empty,
                    row.Copyright ?? string.Empty,
                    row.Connection ?? string.Empty,
                    row.SlaveConnections ?? string.Empty,
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
            var result = new TenantMenuRow[_menuNodes.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var node = _menuNodes[index];
                result[index] = new TenantMenuRow(node.Id, node.Label, node.Path, node.Depth, TreeRowStyle(node),
                    MenuCheckboxClass(node), MenuAriaChecked(node), MenuCheckMark(node));
            }
            return result;
        }

        private readonly ConfigurationOption[] _dbOptions =
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
            BusyId = _busyId,
            BusyAction = _busyAction,
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
            EditDbType = _editDbType.ToString(),
            DbOptions = _dbOptions,
            SetEditDbType = value => _editDbType = (int)ECMAScript.Global.NumberValue(value),
            EditLogoFileNameText = DisplayValue(_editLogoFileName),
            ResetPasswordText = DisplayValue(_resetPassword),
            ToggleMenuNode = ToggleMenuNode,
            ReadSelectedLogo = ReadSelectedLogo,
            MenuNodes = MenuRows(),
            EditTenantType = _editTenantType,
            SetEditTenantType = value => _editTenantType = (int)value,
            EditStatus = _editStatus,
            SetEditStatus = value => _editStatus = (int)value,
            EditOrderNo = _editOrderNo,
            SetEditOrderNo = value => _editOrderNo = (int)(value ?? 100),
            SetQueryName = value => _queryName = value ?? "",
            SetQueryPhone = value => _queryPhone = value ?? "",
            SetEditTab = value => _editTab = value ?? "basic",
            SetEditName = value => _editName = value ?? "",
            SetEditAdminAccount = value => _editAdminAccount = value ?? "",
            SetEditPhone = value => _editPhone = value ?? "",
            SetEditEmail = value => _editEmail = value ?? "",
            SetEditHost = value => _editHost = value ?? "",
            SetEditRemark = value => _editRemark = value ?? "",
            SetEditConnection = value => _editConnection = value ?? "",
            SetEditSlaveConnections = value => _editSlaveConnections = value ?? "",
            SetEditTitle = value => _editTitle = value ?? "",
            SetEditViceTitle = value => _editViceTitle = value ?? "",
            SetEditViceDesc = value => _editViceDesc = value ?? "",
            SetEditCopyright = value => _editCopyright = value ?? "",
            SetEditIcp = value => _editIcp = value ?? "",
            SetEditIcpUrl = value => _editIcpUrl = value ?? "",
            SetEditWatermark = value => _editWatermark = value ?? "",
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetMenuDialogVisible = value => _menuDialogVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            SetPasswordDialogVisible = value => _passwordDialogVisible = value,
            CloseEditDialogVisible = () => _editDialogVisible = false,
            CloseMenuDialogVisible = () => _menuDialogVisible = false,
            ClosePasswordDialogVisible = () => _passwordDialogVisible = false,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPage = LoadPage,
            ClearLogoSelection = ClearLogoSelection,
            Save = Save,
            DismissError = () => _error = null,
            SaveMenu = SaveMenu,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            Editing = _editId.Length > 0,
            Error = _error,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            PageSizes = _pageSizes,
            Loading = _loading,
            Saving = _saving,
            LogoReading = _logoReading,
            MenuSaving = _menuSaving,
            EditDialogVisible = _editDialogVisible,
            MenuDialogVisible = _menuDialogVisible,
            AuditDialogVisible = _auditDialogVisible,
            PasswordDialogVisible = _passwordDialogVisible,
            CanAdd = CanAdd,
            MenuBusy = MenuBusy,
            SiteEnabled = SiteEnabled,
            QueryName = _queryName,
            QueryPhone = _queryPhone,
            EditDialogTitle = _editDialogTitle,
            EditTab = _editTab,
            EditName = _editName,
            EditAdminAccount = _editAdminAccount,
            EditPhone = _editPhone,
            EditEmail = _editEmail,
            EditHost = _editHost,
            EditRemark = _editRemark,
            EditConnection = _editConnection,
            EditSlaveConnections = _editSlaveConnections,
            EditLogoPreview = _editLogoPreview,
            EditLogoBase64 = _editLogoBase64,
            EditTitle = _editTitle,
            EditViceTitle = _editViceTitle,
            EditViceDesc = _editViceDesc,
            EditCopyright = _editCopyright,
            EditIcp = _editIcp,
            EditIcpUrl = _editIcpUrl,
            EditWatermark = _editWatermark,
            MenuTenantName = _menuTenantName,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            AuditRemark = _auditRemark,
            PasswordTenantName = _passwordTenantName,
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
