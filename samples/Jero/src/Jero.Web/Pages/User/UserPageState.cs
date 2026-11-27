using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.User;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.User;

[ECMAScriptModule("components/user-page-state.js")]
public static class UserPageModule
{
    public static UserPageState Create() => new();

    public sealed class UserPageState : global::Jero.Web.Composition.IPageState<UserPageProps>
    {
        private readonly double[] _pageSizes = [10, 20, 50, 100];
        private UserView[] _items = [];
        private UserView[] _rows = [];
        private UserTenantOption[] _tenantItems = [];
        private UserOrgOption[] _orgTreeItems = [];
        private UserOrgOption[] _editorOrgOptions = [];
        private UserNamedOption[] _roleOptions = [];
        private UserNamedOption[] _posOptions = [];
        private UserLanguageOption[] _languageOptions = [];
        private UserEnumOption[] _accountTypeOptions = DefaultAccountTypes();
        private UserEnumOption[] _cardTypeOptions = DefaultCardTypes();
        private UserEnumOption[] _cultureLevelOptions = DefaultCultureLevels();
        private UserEnumOption[] _genderOptions = DefaultGenders();
        private UserEnumOption[] _nationOptions = DefaultNations();

        private string _tenantId = string.Empty;
        private string _selectedOrgId = "-1";
        private string _selectedOrgName = "全部机构";
        private string _selectedOrgTenantId = string.Empty;
        private string _account = string.Empty;
        private string _realName = string.Empty;
        private string _posName = string.Empty;
        private string _phone = string.Empty;
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _total;
        private bool _loading;
        private bool _tenantLoading;
        private bool _orgLoading;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增账号";
        private string _editTab = "basic";
        private bool _saving;
        private bool _dependencyLoading;
        private bool _dependencyFailed;
        private int _dependencyVersion;
        private int _dependencyPending;
        private UserView? _editOriginal;
        private UserDraft _draft = NewDraft(string.Empty, string.Empty, string.Empty);
        private string[] _editRoleIds = [];
        private UserExtOrgInput[] _editExtOrganizations = [];

        private bool _detailDialogVisible;
        private UserView? _detailUser;

        private bool _grantDialogVisible;
        private bool _grantLoading;
        private bool _grantFailed;
        private bool _granting;
        private UserView? _grantUser;
        private UserNamedOption[] _grantRoleOptions = [];
        private string[] _grantRoleIds = [];

        private bool _passwordDialogVisible;
        private string _resetPasswordValue = string.Empty;
        private string _deletingId = string.Empty;
        private string _statusLoadingId = string.Empty;
        private string _resetLoadingId = string.Empty;
        private string _unlockLoadingId = string.Empty;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;
        private string CurrentLangCode => ApplicationState.User?.LangCode ?? "zh-CN";
        private bool CanDetail => ApplicationState.HasPermission("sysUser:detail");
        private bool CanAdd => ApplicationState.HasPermission("sysUser:add");
        private bool CanEdit => ApplicationState.HasPermission("sysUser:update");
        private bool CanDelete => ApplicationState.HasPermission("sysUser:delete");
        private bool CanGrantRole => ApplicationState.HasPermission("sysUser:grantRole");
        private bool CanResetPassword => ApplicationState.HasPermission("sysUser:resetPwd");
        private bool CanSetStatus => ApplicationState.HasPermission("sysUser:setStatus");
        private bool CanUnlockLogin => ApplicationState.HasPermission("sysUser:unlockLogin");
        private bool HasActionColumn => CanDetail || CanAdd || CanEdit || CanDelete || CanGrantRole || CanResetPassword || CanUnlockLogin;
        private bool CanEditDraftRoles => _draft.Id.Length == 0 ? CanAdd : CanEdit;

        public void Initialize()
        {
            _tenantId = DefaultTenantId;
            _languageOptions = DefaultLanguages(CurrentLangCode);
            if (IsSuperAdmin)
                LoadTenants();
            LoadPageAccountTypes();
            LoadOrganizations();
            LoadUsers();
        }

        private void LoadTenants()
        {
            _tenantLoading = true;
            UserClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }

                _tenantItems = UserClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPageAccountTypes()
        {
            UserClient.Dict("AccountTypeEnum").Then(outcome =>
            {
                if (!outcome.Ok)
                    return;
                var loaded = FilterAccountTypes(UserClient.ReadEnumOptions(outcome.Data, true));
                if (loaded.Length > 0)
                    _accountTypeOptions = loaded;
            });
        }

        private void LoadOrganizations()
        {
            _orgLoading = true;
            var targetTenant = IsSuperAdmin ? _tenantId : DefaultTenantId;
            UserClient.Organizations(targetTenant).Then(outcome =>
            {
                _orgLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载组织机构失败。";
                    _orgTreeItems = [];
                    return;
                }

                _orgTreeItems = UserClient.ReadOrganizations(outcome.Data);
            }, _ =>
            {
                _orgLoading = false;
                _error = "加载组织机构失败，请检查网络后重试。";
            });
        }

        private void LoadUsers()
        {
            _loading = true;
            UserClient.Page(new UserPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Account = _account.Trim(),
                RealName = _realName.Trim(),
                PosName = _posName.Trim(),
                Phone = _phone.Trim(),
                OrgId = _selectedOrgId,
                TenantId = IsSuperAdmin ? _tenantId : DefaultTenantId
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载账号失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }

                _items = UserClient.ReadUsers(outcome.Data);
                _rows = _items;
                _total = UserClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载账号失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            _currentPage = 1;
            LoadUsers();
        }

        private void ResetSearch()
        {
            _account = string.Empty;
            _realName = string.Empty;
            _posName = string.Empty;
            _phone = string.Empty;
            _tenantId = DefaultTenantId;
            _selectedOrgId = "-1";
            _selectedOrgName = "全部机构";
            _selectedOrgTenantId = string.Empty;
            _currentPage = 1;
            LoadOrganizations();
            LoadUsers();
        }

        private void Refresh()
        {
            if (IsSuperAdmin)
                LoadTenants();
            LoadPageAccountTypes();
            LoadOrganizations();
            LoadUsers();
        }

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
            _selectedOrgId = "-1";
            _selectedOrgName = "全部机构";
            _selectedOrgTenantId = string.Empty;
            _currentPage = 1;
            LoadOrganizations();
            LoadUsers();
        }

        private void SelectOrganization(string id, string name, string orgTenantId)
        {
            _selectedOrgId = id.Length == 0 ? "-1" : id;
            _selectedOrgName = name.Length == 0 ? "全部机构" : name;
            _selectedOrgTenantId = orgTenantId;
            if (IsSuperAdmin && orgTenantId.Length > 0)
                _tenantId = orgTenantId;
            _currentPage = 1;
            LoadUsers();
        }

        private void ChangePage(Number value)
        {
            _currentPage = (int)value;
            LoadUsers();
        }

        private void ChangePageSize(Number value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadUsers();
        }

        private void OpenCreate()
        {
            var targetTenantId = _selectedOrgTenantId.Length > 0
                ? _selectedOrgTenantId
                : IsSuperAdmin ? _tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户或组织机构后再新增账号");
                return;
            }

            _editDialogTitle = "新增账号";
            _editOriginal = null;
            _draft = NewDraft(targetTenantId, _selectedOrgId == "-1" ? string.Empty : _selectedOrgId, CurrentLangCode);
            _editRoleIds = [];
            _editExtOrganizations = [];
            _editTab = "basic";
            _editDialogVisible = true;
            _error = null;
            LoadEditorDependencies(targetTenantId, string.Empty);
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _editDialogTitle = "编辑账号";
            _editOriginal = row;
            _draft = DraftFrom(row);
            _editRoleIds = [];
            _editExtOrganizations = [];
            _editTab = "basic";
            _editDialogVisible = true;
            _error = null;
            LoadEditorDependencies(row.TenantId ?? DefaultTenantId, row.Id);
        }

        private void OpenCopy(string id)
        {
            var row = Find(id);
            if (row is null || !CanAdd)
                return;

            var targetTenantId = row.TenantId ?? _tenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户或组织机构后再复制账号");
                return;
            }

            _editDialogTitle = "复制账号";
            _editOriginal = null;
            _draft = DraftCopyFrom(row);
            _editRoleIds = [];
            _editExtOrganizations = [];
            _editTab = "basic";
            _editDialogVisible = true;
            _error = null;
            LoadEditorDependencies(targetTenantId, string.Empty);
        }

        private void LoadEditorDependencies(string targetTenantId, string userId)
        {
            // 同一弹窗可在查询完成前切换用户；只有当前版本可更新选项或减少待完成计数。
            _dependencyVersion++;
            var version = _dependencyVersion;
            _dependencyPending = userId.Length == 0 ? 9 : 11;
            _dependencyLoading = true;
            _dependencyFailed = false;
            _editorOrgOptions = [];
            _roleOptions = [];
            _posOptions = [];
            _languageOptions = DefaultLanguages(CurrentLangCode);

            UserClient.Organizations(targetTenantId).Then(outcome =>
            {
                if (version != _dependencyVersion) return;
                if (outcome.Ok)
                    _editorOrgOptions = UserClient.ReadOrganizations(outcome.Data);
                else
                    DependencyError(outcome.Error ?? "加载组织机构失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载组织机构失败。"));

            UserClient.Positions(targetTenantId).Then(outcome =>
            {
                if (version != _dependencyVersion) return;
                if (outcome.Ok)
                {
                    _posOptions = UserClient.ReadPositions(outcome.Data, targetTenantId);
                    _posOptions = EnsureNamedOption(_posOptions, _draft.PosId, _editOriginal?.PosName ?? string.Empty);
                }
                else
                    DependencyError(outcome.Error ?? "加载职位失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载职位失败。"));

            UserClient.Roles().Then(outcome =>
            {
                if (version != _dependencyVersion) return;
                if (outcome.Ok)
                    _roleOptions = UserClient.ReadRoles(outcome.Data, targetTenantId);
                else
                    DependencyError(outcome.Error ?? "加载角色失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载角色失败。"));

            UserClient.Languages().Then(outcome =>
            {
                if (version != _dependencyVersion) return;
                if (outcome.Ok)
                {
                    var loaded = UserClient.ReadLanguages(outcome.Data);
                    _languageOptions = loaded.Length > 0 ? loaded : DefaultLanguages(CurrentLangCode);
                }
                else
                    DependencyError(outcome.Error ?? "加载语言失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载语言失败。"));

            LoadEditorEnum(version, "AccountTypeEnum", 0);
            LoadEditorEnum(version, "CardTypeEnum", 1);
            LoadEditorEnum(version, "CultureLevelEnum", 2);
            LoadEditorEnum(version, "GenderEnum", 3);
            LoadEditorEnum(version, "NationEnum", 4);

            if (userId.Length == 0)
                return;

            UserClient.OwnRoles(userId).Then(outcome =>
            {
                if (version != _dependencyVersion) return;
                if (outcome.Ok)
                    _editRoleIds = CompactIds(UserClient.ReadIds(outcome.Data));
                else
                    DependencyError(outcome.Error ?? "加载账号角色失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载账号角色失败。"));

            UserClient.OwnExtOrganizations(userId).Then(outcome =>
            {
                if (version != _dependencyVersion) return;
                if (outcome.Ok)
                    _editExtOrganizations = UserClient.ReadExtOrganizations(outcome.Data);
                else
                    DependencyError(outcome.Error ?? "加载附属机构失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载附属机构失败。"));
        }

        private void LoadEditorEnum(int version, string code, int target)
        {
            UserClient.Dict(code).Then(outcome =>
            {
                if (version != _dependencyVersion) return;
                if (outcome.Ok)
                {
                    var loaded = UserClient.ReadEnumOptions(outcome.Data, target != 4);
                    if (target == 0)
                    {
                        loaded = FilterAccountTypes(loaded);
                        if (loaded.Length > 0) _accountTypeOptions = loaded;
                    }
                    else if (target == 1 && loaded.Length > 0) _cardTypeOptions = loaded;
                    else if (target == 2 && loaded.Length > 0) _cultureLevelOptions = loaded;
                    else if (target == 3 && loaded.Length > 0) _genderOptions = loaded;
                    else if (target == 4 && loaded.Length > 0) _nationOptions = loaded;
                }
                else
                    DependencyError(outcome.Error ?? "加载字典失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载字典失败。"));
        }

        private void DependencyRejected(int version, string message)
        {
            if (version != _dependencyVersion)
                return;
            DependencyError(message + " 请检查网络后重试。");
            DependencyDone(version);
        }

        private void DependencyError(string message)
        {
            _dependencyFailed = true;
            _error = message;
        }

        private void DependencyDone(int version)
        {
            if (version != _dependencyVersion)
                return;
            _dependencyPending--;
            if (_dependencyPending <= 0)
                _dependencyLoading = false;
        }

        private void SelectEditTab(string tab) => _editTab = tab;

        private void Save()
        {
            if (_saving || _dependencyLoading || _dependencyFailed)
                return;
            if (!ValidateDraft())
                return;

            var input = BuildSaveInput();
            _saving = true;
            var request = _draft.Id.Length == 0 ? UserClient.Add(input) : UserClient.Update(input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存账号失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_draft.Id.Length == 0 ? "账号已新增" : "账号已更新");
                LoadUsers();
            }, _ =>
            {
                _saving = false;
                _error = "保存账号失败，请检查网络后重试。";
            });
        }

        private bool ValidateDraft()
        {
            _draft.Account = _draft.Account.Trim();
            _draft.RealName = _draft.RealName.Trim();
            _draft.Phone = _draft.Phone.Trim();
            _draft.Email = _draft.Email.Trim();
            _draft.DomainAccount = _draft.DomainAccount.Trim();
            _draft.Homepage = _draft.Homepage.Trim();
            _draft.Remark = _draft.Remark.Trim();

            if (_draft.Account.Length == 0 || _draft.Account.Length > 32)
                return FailValidation("账号名称不能为空且不能超过32个字符。", "basic");
            if (_draft.RealName.Length == 0 || _draft.RealName.Length > 32)
                return FailValidation("真实姓名不能为空且不能超过32个字符。", "basic");
            if (_draft.Phone.Length == 0 || _draft.Phone.Length > 16)
                return FailValidation("手机号码不能为空且不能超过16个字符。", "basic");
            if (_draft.AccountType <= 0)
                return FailValidation("请选择账号类型。", "basic");
            if (_draft.LangCode.Length == 0)
                return FailValidation("请选择语言。", "basic");
            if (_draft.OrgId.Length == 0)
                return FailValidation("请选择所属机构。", "basic");
            if (_draft.PosId.Length == 0)
                return FailValidation("请选择职位。", "basic");
            if (_draft.Email.Length > 64)
                return FailValidation("邮箱不能超过64个字符。", "basic");
            if (_draft.Homepage.Length > 512)
                return FailValidation("个性化首页不能超过512个字符。", "basic");
            if (_draft.DomainAccount.Length > 32)
                return FailValidation("域账号不能超过32个字符。", "binding");
            if (_draft.Remark.Length > 256)
                return FailValidation("备注不能超过256个字符。", "profile");
            if (IsSuperAdmin && _draft.TenantId.Length == 0)
                return FailValidation("超级管理员维护账号时必须选择租户。", "basic");

            for (var index = 0; index < _editExtOrganizations.Length; index++)
            {
                var item = _editExtOrganizations[index];
                if (item.OrgId.Length == 0 || item.PosId.Length == 0)
                    return FailValidation("附属机构的机构和职位不能为空。", "basic");
            }
            return true;
        }

        private bool FailValidation(string message, string tab)
        {
            _editTab = tab;
            _error = message;
            return false;
        }

        private UserSaveInput BuildSaveInput()
        {
            var original = _editOriginal;
            return new UserSaveInput
            {
                Id = _draft.Id,
                TenantId = Optional(_draft.TenantId),
                OrgId = _draft.OrgId,
                CreateTime = original?.CreateTime ?? string.Empty,
                UpdateTime = original?.UpdateTime,
                CreateUserId = original?.CreateUserId,
                CreateUserName = original?.CreateUserName,
                UpdateUserId = original?.UpdateUserId,
                UpdateUserName = original?.UpdateUserName,
                Account = _draft.Account,
                RealName = _draft.RealName,
                NickName = Optional(_draft.NickName),
                Avatar = original?.Avatar,
                Sex = _draft.Sex,
                Age = _draft.Age < 0 ? 0 : _draft.Age > 150 ? 150 : _draft.Age,
                Birthday = OptionalDate(_draft.Birthday),
                Nation = Optional(_draft.Nation),
                Phone = Optional(_draft.Phone),
                CardType = _draft.CardType,
                IdCardNum = Optional(_draft.IdCardNum),
                Email = Optional(_draft.Email),
                Address = Optional(_draft.Address),
                CultureLevel = _draft.CultureLevel,
                PoliticalOutlook = Optional(_draft.PoliticalOutlook),
                College = Optional(_draft.College),
                OfficePhone = Optional(_draft.OfficePhone),
                EmergencyContact = Optional(_draft.EmergencyContact),
                EmergencyPhone = Optional(_draft.EmergencyPhone),
                EmergencyAddress = Optional(_draft.EmergencyAddress),
                Introduction = Optional(_draft.Introduction),
                OrderNo = _draft.OrderNo < 0 ? 0 : _draft.OrderNo,
                Status = _draft.Status == 2 ? 2 : 1,
                Remark = Optional(_draft.Remark),
                AccountType = _draft.AccountType,
                ManagerUserId = original?.ManagerUserId,
                PosId = _draft.PosId,
                JobNum = Optional(_draft.JobNum),
                PosLevel = original?.PosLevel,
                PosTitle = original?.PosTitle,
                Expertise = original?.Expertise,
                OfficeZone = original?.OfficeZone,
                Office = original?.Office,
                JoinDate = OptionalDate(_draft.JoinDate),
                LastLoginIp = original?.LastLoginIp,
                LastLoginAddress = original?.LastLoginAddress,
                LastLoginTime = original?.LastLoginTime,
                LastLoginDevice = original?.LastLoginDevice,
                Signature = original?.Signature,
                LangCode = _draft.LangCode,
                Homepage = Optional(_draft.Homepage),
                DomainAccount = Optional(_draft.DomainAccount),
                RoleIdList = CompactIds(_editRoleIds),
                ExtOrgIdList = NormalizeExtOrganizations(_editExtOrganizations)
            };
        }

        private void AddExtOrganization()
        {
            var result = new UserExtOrgInput[_editExtOrganizations.Length + 1];
            for (var index = 0; index < _editExtOrganizations.Length; index++)
                result[index] = _editExtOrganizations[index];
            result[_editExtOrganizations.Length] = new UserExtOrgInput();
            _editExtOrganizations = result;
        }

        private void RemoveExtOrganization(int removeIndex)
        {
            if (removeIndex < 0 || removeIndex >= _editExtOrganizations.Length)
                return;
            var result = new UserExtOrgInput[_editExtOrganizations.Length - 1];
            var target = 0;
            for (var index = 0; index < _editExtOrganizations.Length; index++)
                if (index != removeIndex)
                    result[target++] = _editExtOrganizations[index];
            _editExtOrganizations = result;
        }

        private void SetExtOrgOrganization(int index, string? value)
        {
            if (index >= 0 && index < _editExtOrganizations.Length)
                _editExtOrganizations[index].OrgId = value ?? string.Empty;
        }

        private void SetExtOrgPosition(int index, string? value)
        {
            if (index >= 0 && index < _editExtOrganizations.Length)
                _editExtOrganizations[index].PosId = value ?? string.Empty;
        }

        private void ToggleEditRole(string roleId)
        {
            if (CanEditDraftRoles)
                _editRoleIds = ToggleId(_editRoleIds, roleId);
        }

        private void OpenDetail(string id)
        {
            var row = Find(id);
            if (row is null || !CanDetail)
                return;
            _detailUser = row;
            _detailDialogVisible = true;
        }

        private void OpenGrantRoles(string id)
        {
            var row = Find(id);
            if (row is null || !CanGrantRole)
                return;

            _grantUser = row;
            _grantRoleOptions = [];
            _grantRoleIds = [];
            _grantDialogVisible = true;
            _grantLoading = true;
            _grantFailed = false;
            var pending = 2;
            Action done = () =>
            {
                pending--;
                if (pending <= 0)
                    _grantLoading = false;
            };

            UserClient.Roles().Then(outcome =>
            {
                if (outcome.Ok)
                    _grantRoleOptions = UserClient.ReadRoles(outcome.Data, row.TenantId ?? string.Empty);
                else
                {
                    _grantFailed = true;
                    _error = outcome.Error ?? "加载角色失败。";
                }
                done();
            }, _ =>
            {
                _grantFailed = true;
                _error = "加载角色失败，请检查网络后重试。";
                done();
            });

            UserClient.OwnRoles(row.Id).Then(outcome =>
            {
                if (outcome.Ok)
                    _grantRoleIds = CompactIds(UserClient.ReadIds(outcome.Data));
                else
                {
                    _grantFailed = true;
                    _error = outcome.Error ?? "加载账号角色失败。";
                }
                done();
            }, _ =>
            {
                _grantFailed = true;
                _error = "加载账号角色失败，请检查网络后重试。";
                done();
            });
        }

        private void ToggleGrantRole(string roleId)
            => _grantRoleIds = ToggleId(_grantRoleIds, roleId);

        private void SaveGrantRoles()
        {
            var user = _grantUser;
            if (user is null || _granting || _grantLoading || _grantFailed)
                return;

            _granting = true;
            UserClient.GrantRole(user.Id, CompactIds(_grantRoleIds)).Then(outcome =>
            {
                _granting = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "授权角色失败。";
                    return;
                }

                _grantDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success("角色授权已更新");
                LoadUsers();
            }, _ =>
            {
                _granting = false;
                _error = "授权角色失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || _deletingId.Length > 0)
                return;

            _deletingId = row.Id;
            UserClient.Delete(row.Id).Then(outcome =>
            {
                _deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除账号失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("账号已删除");
                LoadUsers();
            }, _ =>
            {
                _deletingId = string.Empty;
                _error = "删除账号失败，请检查网络后重试。";
            });
        }

        private void SetStatus(string id)
        {
            var row = Find(id);
            if (row is null || !CanSetStatus || _statusLoadingId.Length > 0)
                return;

            var next = row.Status == 1 ? 2 : 1;
            _statusLoadingId = row.Id;
            UserClient.SetStatus(row.Id, next).Then(outcome =>
            {
                _statusLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "设置账号状态失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success(next == 1 ? "账号已启用" : "账号已停用");
                LoadUsers();
            }, _ =>
            {
                _statusLoadingId = string.Empty;
                _error = "设置账号状态失败，请检查网络后重试。";
            });
        }

        private void ResetPassword(string id)
        {
            var row = Find(id);
            if (row is null || !CanResetPassword || _resetLoadingId.Length > 0)
                return;

            _resetLoadingId = row.Id;
            UserClient.ResetPassword(row.Id).Then(outcome =>
            {
                _resetLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "重置密码失败。";
                    return;
                }

                _resetPasswordValue = UserClient.ReadPassword(outcome.Data);
                _passwordDialogVisible = true;
                _error = null;
            }, _ =>
            {
                _resetLoadingId = string.Empty;
                _error = "重置密码失败，请检查网络后重试。";
            });
        }

        private void UnlockLogin(string id)
        {
            var row = Find(id);
            if (row is null || !CanUnlockLogin || _unlockLoadingId.Length > 0)
                return;

            _unlockLoadingId = row.Id;
            UserClient.UnlockLogin(row.Id).Then(outcome =>
            {
                _unlockLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "解除登录锁定失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("登录锁定已解除");
            }, _ =>
            {
                _unlockLoadingId = string.Empty;
                _error = "解除登录锁定失败，请检查网络后重试。";
            });
        }

        private UserView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private string AccountTypeLabel(VueProps? row)
        {
            var value = RowInt(row, "accountType", 0).ToString();
            for (var index = 0; index < _accountTypeOptions.Length; index++)
                if (_accountTypeOptions[index].Value == value)
                    return _accountTypeOptions[index].Label;
            return value == "0" ? "-" : "类型" + value;
        }

        private string AccountTypeTag(VueProps? row)
        {
            var value = RowInt(row, "accountType", 0).ToString();
            for (var index = 0; index < _accountTypeOptions.Length; index++)
            {
                if (_accountTypeOptions[index].Value != value)
                    continue;
                return TagType(_accountTypeOptions[index].TagType);
            }
            return value == "666" ? "success" : "primary";
        }

        private static string TagType(string value)
            => value == "success" ? "success"
                : value == "warning" ? "warning"
                : value == "danger" ? "danger"
                : value == "info" ? "info"
                : "primary";

        private static string StatusText(VueProps? row)
            => RowInt(row, "status", 1) == 1 ? "启用" : "停用";

        private static string StatusTag(VueProps? row)
            => RowInt(row, "status", 1) == 1 ? "success" : "info";

        private static string StatusAction(VueProps? row)
            => RowInt(row, "status", 1) == 1 ? "停用" : "启用";

        private static string StatusConfirm(VueProps? row)
            => "确定" + StatusAction(row) + "账号【" + RowString(row, "account") + "】吗？";

        private static string DisplayName(VueProps? row)
        {
            var value = RowString(row, "nickName");
            if (value.Length == 0) value = RowString(row, "realName");
            if (value.Length == 0) value = RowString(row, "account");
            return value;
        }

        private static string AvatarSource(VueProps? row)
        {
            var value = RowString(row, "avatar");
            if (value.Length == 0 || value.StartsWith('/') || value.StartsWith("http://") || value.StartsWith("https://"))
                return value;
            return "/" + value;
        }

        private static string DisplayValue(string? value)
            => string.IsNullOrWhiteSpace(value) ? "-" : value;

        private static string DisplayUser(string? name, string? id)
        {
            var actualName = name ?? string.Empty;
            var actualId = id ?? string.Empty;
            if (actualName.Length > 0 && actualId.Length > 0)
                return actualName + " (" + actualId + ")";
            return actualName.Length > 0 ? actualName : DisplayValue(actualId);
        }

        private static string TenantLabel(UserTenantOption tenant)
            => tenant.Host.Length == 0 ? tenant.Label : tenant.Label + " (" + tenant.Host + ")";

        private static string OrgButtonClass(string selectedId, string id)
            => "user-org-item" + (selectedId == id ? " is-active" : string.Empty);

        private static string OrgIndent(UserOrgOption option)
            => "padding-left:" + (12 + option.Depth * 18) + "px";

        private static string RoleButtonClass(string[] selected, string id)
            => "user-role-option" + (ContainsId(selected, id) ? " is-selected" : string.Empty);

        private static string PanelStyle(string active, string panel)
            => active == panel ? string.Empty : "display:none";

        private static bool ContainsId(string[] source, string id)
        {
            for (var index = 0; index < source.Length; index++)
                if (source[index] == id)
                    return true;
            return false;
        }

        private static string[] ToggleId(string[] source, string id)
        {
            if (ContainsId(source, id))
            {
                var result = new string[source.Length - 1];
                var target = 0;
                for (var index = 0; index < source.Length; index++)
                    if (source[index] != id)
                        result[target++] = source[index];
                return result;
            }

            var expanded = new string[source.Length + 1];
            for (var index = 0; index < source.Length; index++)
                expanded[index] = source[index];
            expanded[source.Length] = id;
            return expanded;
        }

        private static string[] CompactIds(string[] source)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
                if (source[index].Length > 0 && !ContainsBefore(source, index, source[index]))
                    count++;

            var result = new string[count];
            var target = 0;
            for (var index = 0; index < source.Length; index++)
                if (source[index].Length > 0 && !ContainsBefore(source, index, source[index]))
                    result[target++] = source[index];
            return result;
        }

        private static bool ContainsBefore(string[] source, int end, string id)
        {
            for (var index = 0; index < end; index++)
                if (source[index] == id)
                    return true;
            return false;
        }

        private static UserExtOrgInput[] NormalizeExtOrganizations(UserExtOrgInput[] source)
        {
            var result = new UserExtOrgInput[source.Length];
            for (var index = 0; index < source.Length; index++)
            {
                var item = source[index];
                result[index] = new UserExtOrgInput
                {
                    Id = Optional(item.Id),
                    UserId = Optional(item.UserId),
                    OrgId = item.OrgId,
                    PosId = item.PosId,
                    JobNum = Optional(item.JobNum),
                    PosLevel = Optional(item.PosLevel),
                    JoinDate = OptionalDate(item.JoinDate)
                };
            }
            return result;
        }

        private static UserNamedOption[] EnsureNamedOption(UserNamedOption[] source, string id, string name)
        {
            if (id.Length == 0)
                return source;
            for (var index = 0; index < source.Length; index++)
                if (source[index].Id == id)
                    return source;

            var result = new UserNamedOption[source.Length + 1];
            for (var index = 0; index < source.Length; index++)
                result[index] = source[index];
            result[source.Length] = new UserNamedOption
            {
                Id = id,
                Name = name.Length == 0 ? "历史选项 (" + id + ")" : name
            };
            return result;
        }

        private UserEnumOption[] FilterAccountTypes(UserEnumOption[] source)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
            {
                var value = source[index].Value;
                if (value != "999" && (value != "888" || IsSuperAdmin))
                    count++;
            }

            var result = new UserEnumOption[count];
            var target = 0;
            for (var index = 0; index < source.Length; index++)
            {
                var value = source[index].Value;
                if (value == "999" || (value == "888" && !IsSuperAdmin))
                    continue;
                result[target++] = source[index];
            }
            return result;
        }

        private static UserEnumOption[] DefaultAccountTypes()
            =>
            [
                new UserEnumOption { Value = "777", Number = 777, Label = "普通账号", TagType = "primary" },
                new UserEnumOption { Value = "666", Number = 666, Label = "会员", TagType = "success" }
            ];

        private static UserEnumOption[] DefaultCardTypes()
            =>
            [
                new UserEnumOption { Value = "0", Number = 0, Label = "身份证" },
                new UserEnumOption { Value = "1", Number = 1, Label = "护照" }
            ];

        private static UserEnumOption[] DefaultCultureLevels()
            =>
            [
                new UserEnumOption { Value = "0", Number = 0, Label = "其他" },
                new UserEnumOption { Value = "5", Number = 5, Label = "技工学校" },
                new UserEnumOption { Value = "9", Number = 9, Label = "大学专科" },
                new UserEnumOption { Value = "10", Number = 10, Label = "大学本科" }
            ];

        private static UserEnumOption[] DefaultGenders()
            =>
            [
                new UserEnumOption { Value = "1", Number = 1, Label = "男性" },
                new UserEnumOption { Value = "2", Number = 2, Label = "女性" },
                new UserEnumOption { Value = "0", Number = 0, Label = "未知" }
            ];

        private static UserEnumOption[] DefaultNations()
            =>
            [
                new UserEnumOption { Value = "1", Label = "汉族" },
                new UserEnumOption { Value = "2", Label = "壮族" },
                new UserEnumOption { Value = "3", Label = "满族" },
                new UserEnumOption { Value = "4", Label = "回族" }
            ];

        private static UserLanguageOption[] DefaultLanguages(string current)
            => [new UserLanguageOption { Value = current, Label = current }];

        private static UserDraft NewDraft(string targetTenantId, string targetOrgId, string langCode)
            => new()
            {
                TenantId = targetTenantId,
                OrgId = targetOrgId,
                AccountType = 777,
                Birthday = "2000-01-01",
                CardType = 0,
                CultureLevel = 5,
                LangCode = langCode.Length == 0 ? "zh-CN" : langCode,
                OrderNo = 100,
                Sex = 1,
                Status = 1
            };

        private static UserDraft DraftFrom(UserView row)
            => new()
            {
                Id = row.Id,
                TenantId = row.TenantId ?? string.Empty,
                OrgId = row.OrgId,
                Account = row.Account,
                RealName = row.RealName,
                NickName = row.NickName ?? string.Empty,
                Sex = row.Sex,
                Age = row.Age,
                Birthday = row.Birthday ?? string.Empty,
                Nation = row.Nation ?? string.Empty,
                Phone = row.Phone ?? string.Empty,
                CardType = row.CardType,
                IdCardNum = row.IdCardNum ?? string.Empty,
                Email = row.Email ?? string.Empty,
                Address = row.Address ?? string.Empty,
                CultureLevel = row.CultureLevel,
                PoliticalOutlook = row.PoliticalOutlook ?? string.Empty,
                College = row.College ?? string.Empty,
                OfficePhone = row.OfficePhone ?? string.Empty,
                EmergencyContact = row.EmergencyContact ?? string.Empty,
                EmergencyPhone = row.EmergencyPhone ?? string.Empty,
                EmergencyAddress = row.EmergencyAddress ?? string.Empty,
                Introduction = row.Introduction ?? string.Empty,
                OrderNo = row.OrderNo,
                Status = row.Status,
                Remark = row.Remark ?? string.Empty,
                AccountType = row.AccountType,
                PosId = row.PosId,
                JobNum = row.JobNum ?? string.Empty,
                JoinDate = row.JoinDate ?? string.Empty,
                LangCode = row.LangCode,
                Homepage = row.Homepage ?? string.Empty,
                DomainAccount = row.DomainAccount
            };

        private static UserDraft DraftCopyFrom(UserView row)
        {
            var result = DraftFrom(row);
            result.Id = string.Empty;
            result.Account = string.Empty;
            result.Phone = string.Empty;
            result.DomainAccount = string.Empty;
            return result;
        }

        private static string? Optional(string? value)
        {
            var text = value?.Trim() ?? string.Empty;
            return text.Length == 0 ? null : text;
        }

        private static string? OptionalDate(string? value)
        {
            var text = Optional(value);
            if (text is null)
                return null;
            return text.Length > 10 ? text.Substring(0, 10) : text;
        }

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

        private static int RowInt(VueProps? row, string key, int fallback)
        {
            if (row is null)
                return fallback;
            var value = Reflect.Get(row, key);
            return value is null ? fallback : (int)NumberValue(value);
        }

        private bool IsDeleting(VueProps? row) => _deletingId == RowString(row, "id");
        private bool IsChangingStatus(VueProps? row) => _statusLoadingId == RowString(row, "id");
        private bool IsResetting(VueProps? row) => _resetLoadingId == RowString(row, "id");
        private bool IsUnlocking(VueProps? row) => _unlockLoadingId == RowString(row, "id");

        private UserTableRow[] TableRows()
        {
            var result = new UserTableRow[_rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _rows[index];
                result[index] = new UserTableRow(
                    row.Id,
                    row.Account,
                    row.RealName,
                    row.Phone ?? string.Empty,
                    row.RoleName,
                    row.OrgName,
                    row.PosName,
                    row.OrderNo,
                    DisplayValue(row.UpdateTime),
                    AvatarSource(row),
                    AccountTypeTag(row),
                    AccountTypeLabel(row),
                    StatusTag(row),
                    StatusText(row),
                    StatusConfirm(row),
                    StatusAction(row),
                    IsChangingStatus(row),
                    IsResetting(row),
                    IsUnlocking(row),
                    IsDeleting(row));
            }
            return result;
        }

        private UserTreeRow[] OrgTreeItemsView()
        {
            var result = new UserTreeRow[_orgTreeItems.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _orgTreeItems[index];
                result[index] = new UserTreeRow(row.Id, row.Name, row.Path, row.TenantId, row.Depth, row.Disabled);
            }
            return result;
        }

        private UserExtOrganizationRow[] EditExtOrganizationsView()
        {
            var result = new UserExtOrganizationRow[_editExtOrganizations.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new UserExtOrganizationRow(_editExtOrganizations[index].OrgId, _editExtOrganizations[index].PosId);
            return result;
        }

        private UserRoleRow[] RoleOptionsView() => RoleRows(_roleOptions);
        private UserRoleRow[] GrantRoleOptionsView() => RoleRows(_grantRoleOptions);

        private static UserRoleRow[] RoleRows(UserNamedOption[] source)
        {
            var result = new UserRoleRow[source.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new UserRoleRow(source[index].Id, source[index].Name);
            return result;
        }

        private ConfigurationOption[] TenantOptionsView()
        {
            var result = new ConfigurationOption[_tenantItems.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(_tenantItems[index].Value, TenantLabel(_tenantItems[index]));
            return result;
        }

        private ConfigurationOption[] EditorOrganizationOptionsView()
        {
            var result = new ConfigurationOption[_editorOrgOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(_editorOrgOptions[index].Id, _editorOrgOptions[index].Path);
            return result;
        }

        private ConfigurationOption[] PositionOptionsView()
        {
            var result = new ConfigurationOption[_posOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(_posOptions[index].Id, _posOptions[index].Name);
            return result;
        }

        private ConfigurationOption[] LanguageOptionsView()
        {
            var result = new ConfigurationOption[_languageOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(_languageOptions[index].Value, _languageOptions[index].Label);
            return result;
        }

        private ConfigurationOption[] AccountTypeOptionsView() => EnumOptions(_accountTypeOptions);
        private ConfigurationOption[] CardTypeOptionsView() => EnumOptions(_cardTypeOptions);
        private ConfigurationOption[] CultureLevelOptionsView() => EnumOptions(_cultureLevelOptions);
        private ConfigurationOption[] GenderOptionsView() => EnumOptions(_genderOptions);
        private ConfigurationOption[] NationOptionsView() => EnumOptions(_nationOptions);

        private static ConfigurationOption[] EnumOptions(UserEnumOption[] source)
        {
            var result = new ConfigurationOption[source.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(source[index].Value, source[index].Label);
            return result;
        }

        private readonly ConfigurationOption[] _statusOptionsView =
        [
            new ConfigurationOption("1", "启用"),
            new ConfigurationOption("2", "停用")
        ];

        public UserPageProps CreateProps() => new(new UserPageModel
        {
            Draft = _draft,
            Table = new UserTableModel
            {
                Rows = TableRows(),
                CanSetStatus = CanSetStatus,
                CanDetail = CanDetail,
                HasActionColumn = HasActionColumn,
                CanEdit = CanEdit,
                CanGrantRole = CanGrantRole,
                CanAdd = CanAdd,
                CanResetPassword = CanResetPassword,
                CanUnlockLogin = CanUnlockLogin,
                CanDelete = CanDelete,
                StatusLoadingId = _statusLoadingId,
                ResetLoadingId = _resetLoadingId,
                UnlockLoadingId = _unlockLoadingId,
                DeletingId = _deletingId,
                SetStatus = SetStatus,
                OpenDetail = OpenDetail,
                OpenEdit = OpenEdit,
                OpenGrantRoles = OpenGrantRoles,
                OpenCopy = OpenCopy,
                ResetPassword = ResetPassword,
                UnlockLogin = UnlockLogin,
                Delete = Delete,
            },
            TenantLoading = _tenantLoading,
            TenantOptions = TenantOptionsView(),
            EditorOrganizationOptions = EditorOrganizationOptionsView(),
            PositionOptions = PositionOptionsView(),
            LanguageOptions = LanguageOptionsView(),
            NationOptions = NationOptionsView(),
            AccountTypeOptions = AccountTypeOptionsView(),
            CardTypeOptions = CardTypeOptionsView(),
            GenderOptions = GenderOptionsView(),
            CultureLevelOptions = CultureLevelOptionsView(),
            TenantChanged = TenantChanged,
            TenantId = _tenantId,
            SetDraftAccountType = value => _draft.AccountType = value.Length == 0 ? 777 : (int)ECMAScript.Global.NumberValue(value),
            DraftAccountType = _draft.AccountType.ToString(),
            SetDraftStatus = value => _draft.Status = value.Length == 0 ? 1 : (int)ECMAScript.Global.NumberValue(value),
            StatusOptions = _statusOptionsView,
            DraftStatus = _draft.Status.ToString(),
            ChangeExtOrganization = SetExtOrgOrganization,
            ChangeExtPosition = SetExtOrgPosition,
            SetDraftCardType = value => _draft.CardType = value.Length == 0 ? 0 : (int)ECMAScript.Global.NumberValue(value),
            DraftCardType = _draft.CardType.ToString(),
            SetDraftSex = value => _draft.Sex = value.Length == 0 ? 1 : (int)ECMAScript.Global.NumberValue(value),
            DraftSex = _draft.Sex.ToString(),
            SetDraftCultureLevel = value => _draft.CultureLevel = value.Length == 0 ? 5 : (int)ECMAScript.Global.NumberValue(value),
            DraftCultureLevel = _draft.CultureLevel.ToString(),
            SetDraftOrderNo = value => _draft.OrderNo = (int)(value ?? 100),
            SetDraftAge = value => _draft.Age = (int)(value ?? 0),
            SetAccount = value => _account = value ?? "",
            SetRealName = value => _realName = value ?? "",
            SetPosName = value => _posName = value ?? "",
            SetPhone = value => _phone = value ?? "",
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetDetailDialogVisible = value => _detailDialogVisible = value,
            SetGrantDialogVisible = value => _grantDialogVisible = value,
            SetPasswordDialogVisible = value => _passwordDialogVisible = value,
            CloseEditDialogVisible = () => _editDialogVisible = false,
            CloseGrantDialogVisible = () => _grantDialogVisible = false,
            ClosePasswordDialogVisible = () => _passwordDialogVisible = false,
            RemoveExtOrganization = RemoveExtOrganization,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            Refresh = Refresh,
            AddExtOrganization = AddExtOrganization,
            Save = Save,
            SaveGrantRoles = SaveGrantRoles,
            DismissError = () => _error = null,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            Editing = _draft.Id.Length > 0,
            DetailAccount = DisplayValue(_detailUser?.Account),
            DetailRealName = DisplayValue(_detailUser?.RealName),
            DetailNickName = DisplayValue(_detailUser?.NickName),
            DetailPhone = DisplayValue(_detailUser?.Phone),
            DetailEmail = DisplayValue(_detailUser?.Email),
            DetailRoleName = DisplayValue(_detailUser?.RoleName),
            DetailOrgName = DisplayValue(_detailUser?.OrgName),
            DetailPosName = DisplayValue(_detailUser?.PosName),
            DetailJobNum = DisplayValue(_detailUser?.JobNum),
            DetailLangCode = DisplayValue(_detailUser?.LangCode),
            DetailLastLoginIp = DisplayValue(_detailUser?.LastLoginIp),
            DetailLastLoginAddress = DisplayValue(_detailUser?.LastLoginAddress),
            DetailLastLoginTime = DisplayValue(_detailUser?.LastLoginTime),
            DetailLastLoginDevice = DisplayValue(_detailUser?.LastLoginDevice),
            DetailCreateTime = DisplayValue(_detailUser?.CreateTime),
            DetailUpdateTime = DisplayValue(_detailUser?.UpdateTime),
            DetailRemark = DisplayValue(_detailUser?.Remark),
            DetailCreateUserName = DisplayUser(_detailUser?.CreateUserName, _detailUser?.CreateUserId),
            DetailUpdateUserName = DisplayUser(_detailUser?.UpdateUserName, _detailUser?.UpdateUserId),
            DetailAccountType = _detailUser is null ? "-" : _detailUser.AccountType.ToString(),
            DetailStatus = _detailUser?.Status == 1 ? "启用" : "停用",
            GrantAccount = DisplayValue(_grantUser?.Account),
            GrantRealName = DisplayValue(_grantUser?.RealName),
            Error = _error,
            Account = _account,
            RealName = _realName,
            PosName = _posName,
            Phone = _phone,
            Loading = _loading,
            OrgLoading = _orgLoading,
            SelectedOrgId = _selectedOrgId,
            SelectedOrgName = _selectedOrgName,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            EditDialogVisible = _editDialogVisible,
            EditDialogTitle = _editDialogTitle,
            EditTab = _editTab,
            Saving = _saving,
            DependencyLoading = _dependencyLoading,
            DependencyFailed = _dependencyFailed,
            EditRoleIds = _editRoleIds,
            GrantRoleIds = _grantRoleIds,
            DetailDialogVisible = _detailDialogVisible,
            GrantDialogVisible = _grantDialogVisible,
            GrantLoading = _grantLoading,
            Granting = _granting,
            GrantFailed = _grantFailed,
            PasswordDialogVisible = _passwordDialogVisible,
            ResetPasswordValue = _resetPasswordValue,
            CanEditDraftRoles = CanEditDraftRoles,
            IsSuperAdmin = IsSuperAdmin,
            OrgTreeItems = OrgTreeItemsView(),
            RoleOptions = RoleOptionsView(),
            GrantRoleOptions = GrantRoleOptionsView(),
            EditExtOrganizations = EditExtOrganizationsView(),
            PageSizes = _pageSizes,
            SelectOrganization = SelectOrganization,
            SelectEditTab = SelectEditTab,
            ToggleEditRole = ToggleEditRole,
            ToggleGrantRole = ToggleGrantRole,
        });
    }
}

public sealed record UserPageProps(UserPageModel Model) : VueProps;
