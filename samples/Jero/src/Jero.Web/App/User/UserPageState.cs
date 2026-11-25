using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.User;

[ECMAScriptModule("components/user-page-state.js")]
public static class UserPageModule
{
    public static UserPageState Create() => new();

    public sealed class UserPageState : global::Jero.Web.App.IPageState<UserPageProps>
    {
        private readonly double[] pageSizes = [10, 20, 50, 100];
        private UserView[] items = [];
        private UserView[] rows = [];
        private UserTenantOption[] tenantItems = [];
        private UserOrgOption[] orgTreeItems = [];
        private UserOrgOption[] editorOrgOptions = [];
        private UserNamedOption[] roleOptions = [];
        private UserNamedOption[] posOptions = [];
        private UserLanguageOption[] languageOptions = [];
        private UserEnumOption[] accountTypeOptions = DefaultAccountTypes();
        private UserEnumOption[] cardTypeOptions = DefaultCardTypes();
        private UserEnumOption[] cultureLevelOptions = DefaultCultureLevels();
        private UserEnumOption[] genderOptions = DefaultGenders();
        private UserEnumOption[] nationOptions = DefaultNations();

        private string tenantId = string.Empty;
        private string selectedOrgId = "-1";
        private string selectedOrgName = "全部机构";
        private string selectedOrgTenantId = string.Empty;
        private string account = string.Empty;
        private string realName = string.Empty;
        private string posName = string.Empty;
        private string phone = string.Empty;
        private int currentPage = 1;
        private int pageSize = 20;
        private int total;
        private bool loading;
        private bool tenantLoading;
        private bool orgLoading;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增账号";
        private string editTab = "basic";
        private bool saving;
        private bool dependencyLoading;
        private bool dependencyFailed;
        private int dependencyVersion;
        private int dependencyPending;
        private UserView? editOriginal;
        private UserDraft draft = NewDraft(string.Empty, string.Empty, string.Empty);
        private string[] editRoleIds = [];
        private UserExtOrgInput[] editExtOrganizations = [];

        private bool detailDialogVisible;
        private UserView? detailUser;

        private bool grantDialogVisible;
        private bool grantLoading;
        private bool grantFailed;
        private bool granting;
        private UserView? grantUser;
        private UserNamedOption[] grantRoleOptions = [];
        private string[] grantRoleIds = [];

        private bool passwordDialogVisible;
        private string resetPasswordValue = string.Empty;
        private string deletingId = string.Empty;
        private string statusLoadingId = string.Empty;
        private string resetLoadingId = string.Empty;
        private string unlockLoadingId = string.Empty;

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
        private bool CanEditDraftRoles => draft.id.Length == 0 ? CanAdd : CanEdit;

        public void Initialize()
        {
            tenantId = DefaultTenantId;
            languageOptions = DefaultLanguages(CurrentLangCode);
            if (IsSuperAdmin)
                LoadTenants();
            LoadPageAccountTypes();
            LoadOrganizations();
            LoadUsers();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            UserClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }

                tenantItems = UserClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
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
                    accountTypeOptions = loaded;
            });
        }

        private void LoadOrganizations()
        {
            orgLoading = true;
            var targetTenant = IsSuperAdmin ? tenantId : DefaultTenantId;
            UserClient.Organizations(targetTenant).Then(outcome =>
            {
                orgLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载组织机构失败。";
                    orgTreeItems = [];
                    return;
                }

                orgTreeItems = UserClient.ReadOrganizations(outcome.Data);
            }, _ =>
            {
                orgLoading = false;
                error = "加载组织机构失败，请检查网络后重试。";
            });
        }

        private void LoadUsers()
        {
            loading = true;
            UserClient.Page(new UserPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                account = account.Trim(),
                realName = realName.Trim(),
                posName = posName.Trim(),
                phone = phone.Trim(),
                orgId = selectedOrgId,
                tenantId = IsSuperAdmin ? tenantId : DefaultTenantId
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载账号失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }

                items = UserClient.ReadUsers(outcome.Data);
                rows = items;
                total = UserClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载账号失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadUsers();
        }

        private void ResetSearch()
        {
            account = string.Empty;
            realName = string.Empty;
            posName = string.Empty;
            phone = string.Empty;
            tenantId = DefaultTenantId;
            selectedOrgId = "-1";
            selectedOrgName = "全部机构";
            selectedOrgTenantId = string.Empty;
            currentPage = 1;
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
            tenantId = value ?? string.Empty;
            selectedOrgId = "-1";
            selectedOrgName = "全部机构";
            selectedOrgTenantId = string.Empty;
            currentPage = 1;
            LoadOrganizations();
            LoadUsers();
        }

        private void SelectOrganization(string id, string name, string orgTenantId)
        {
            selectedOrgId = id.Length == 0 ? "-1" : id;
            selectedOrgName = name.Length == 0 ? "全部机构" : name;
            selectedOrgTenantId = orgTenantId;
            if (IsSuperAdmin && orgTenantId.Length > 0)
                tenantId = orgTenantId;
            currentPage = 1;
            LoadUsers();
        }

        private void ChangePage(Number value)
        {
            currentPage = (int)value;
            LoadUsers();
        }

        private void ChangePageSize(Number value)
        {
            pageSize = (int)value;
            currentPage = 1;
            LoadUsers();
        }

        private void OpenCreate()
        {
            var targetTenantId = selectedOrgTenantId.Length > 0
                ? selectedOrgTenantId
                : IsSuperAdmin ? tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户或组织机构后再新增账号");
                return;
            }

            editDialogTitle = "新增账号";
            editOriginal = null;
            draft = NewDraft(targetTenantId, selectedOrgId == "-1" ? string.Empty : selectedOrgId, CurrentLangCode);
            editRoleIds = [];
            editExtOrganizations = [];
            editTab = "basic";
            editDialogVisible = true;
            error = null;
            LoadEditorDependencies(targetTenantId, string.Empty);
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            editDialogTitle = "编辑账号";
            editOriginal = row;
            draft = DraftFrom(row);
            editRoleIds = [];
            editExtOrganizations = [];
            editTab = "basic";
            editDialogVisible = true;
            error = null;
            LoadEditorDependencies(row.tenantId ?? DefaultTenantId, row.id);
        }

        private void OpenCopy(string id)
        {
            var row = Find(id);
            if (row is null || !CanAdd)
                return;

            var targetTenantId = row.tenantId ?? tenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户或组织机构后再复制账号");
                return;
            }

            editDialogTitle = "复制账号";
            editOriginal = null;
            draft = DraftCopyFrom(row);
            editRoleIds = [];
            editExtOrganizations = [];
            editTab = "basic";
            editDialogVisible = true;
            error = null;
            LoadEditorDependencies(targetTenantId, string.Empty);
        }

        private void LoadEditorDependencies(string targetTenantId, string userId)
        {
            // The dialog can switch users before its lookups finish. Each callback
            // checks this version before updating the options or the pending count.
            dependencyVersion++;
            var version = dependencyVersion;
            dependencyPending = userId.Length == 0 ? 9 : 11;
            dependencyLoading = true;
            dependencyFailed = false;
            editorOrgOptions = [];
            roleOptions = [];
            posOptions = [];
            languageOptions = DefaultLanguages(CurrentLangCode);

            UserClient.Organizations(targetTenantId).Then(outcome =>
            {
                if (version != dependencyVersion) return;
                if (outcome.Ok)
                    editorOrgOptions = UserClient.ReadOrganizations(outcome.Data);
                else
                    DependencyError(outcome.Error ?? "加载组织机构失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载组织机构失败。"));

            UserClient.Positions(targetTenantId).Then(outcome =>
            {
                if (version != dependencyVersion) return;
                if (outcome.Ok)
                {
                    posOptions = UserClient.ReadPositions(outcome.Data, targetTenantId);
                    posOptions = EnsureNamedOption(posOptions, draft.posId, editOriginal?.posName ?? string.Empty);
                }
                else
                    DependencyError(outcome.Error ?? "加载职位失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载职位失败。"));

            UserClient.Roles().Then(outcome =>
            {
                if (version != dependencyVersion) return;
                if (outcome.Ok)
                    roleOptions = UserClient.ReadRoles(outcome.Data, targetTenantId);
                else
                    DependencyError(outcome.Error ?? "加载角色失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载角色失败。"));

            UserClient.Languages().Then(outcome =>
            {
                if (version != dependencyVersion) return;
                if (outcome.Ok)
                {
                    var loaded = UserClient.ReadLanguages(outcome.Data);
                    languageOptions = loaded.Length > 0 ? loaded : DefaultLanguages(CurrentLangCode);
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
                if (version != dependencyVersion) return;
                if (outcome.Ok)
                    editRoleIds = CompactIds(UserClient.ReadIds(outcome.Data));
                else
                    DependencyError(outcome.Error ?? "加载账号角色失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载账号角色失败。"));

            UserClient.OwnExtOrganizations(userId).Then(outcome =>
            {
                if (version != dependencyVersion) return;
                if (outcome.Ok)
                    editExtOrganizations = UserClient.ReadExtOrganizations(outcome.Data);
                else
                    DependencyError(outcome.Error ?? "加载附属机构失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载附属机构失败。"));
        }

        private void LoadEditorEnum(int version, string code, int target)
        {
            UserClient.Dict(code).Then(outcome =>
            {
                if (version != dependencyVersion) return;
                if (outcome.Ok)
                {
                    var loaded = UserClient.ReadEnumOptions(outcome.Data, target != 4);
                    if (target == 0)
                    {
                        loaded = FilterAccountTypes(loaded);
                        if (loaded.Length > 0) accountTypeOptions = loaded;
                    }
                    else if (target == 1 && loaded.Length > 0) cardTypeOptions = loaded;
                    else if (target == 2 && loaded.Length > 0) cultureLevelOptions = loaded;
                    else if (target == 3 && loaded.Length > 0) genderOptions = loaded;
                    else if (target == 4 && loaded.Length > 0) nationOptions = loaded;
                }
                else
                    DependencyError(outcome.Error ?? "加载字典失败。");
                DependencyDone(version);
            }, _ => DependencyRejected(version, "加载字典失败。"));
        }

        private void DependencyRejected(int version, string message)
        {
            if (version != dependencyVersion)
                return;
            DependencyError(message + " 请检查网络后重试。");
            DependencyDone(version);
        }

        private void DependencyError(string message)
        {
            dependencyFailed = true;
            error = message;
        }

        private void DependencyDone(int version)
        {
            if (version != dependencyVersion)
                return;
            dependencyPending--;
            if (dependencyPending <= 0)
                dependencyLoading = false;
        }

        private void SelectEditTab(string tab) => editTab = tab;

        private void Save()
        {
            if (saving || dependencyLoading || dependencyFailed)
                return;
            if (!ValidateDraft())
                return;

            var input = BuildSaveInput();
            saving = true;
            var request = draft.id.Length == 0 ? UserClient.Add(input) : UserClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存账号失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(draft.id.Length == 0 ? "账号已新增" : "账号已更新");
                LoadUsers();
            }, _ =>
            {
                saving = false;
                error = "保存账号失败，请检查网络后重试。";
            });
        }

        private bool ValidateDraft()
        {
            draft.account = draft.account.Trim();
            draft.realName = draft.realName.Trim();
            draft.phone = draft.phone.Trim();
            draft.email = draft.email.Trim();
            draft.domainAccount = draft.domainAccount.Trim();
            draft.homepage = draft.homepage.Trim();
            draft.remark = draft.remark.Trim();

            if (draft.account.Length == 0 || draft.account.Length > 32)
                return FailValidation("账号名称不能为空且不能超过32个字符。", "basic");
            if (draft.realName.Length == 0 || draft.realName.Length > 32)
                return FailValidation("真实姓名不能为空且不能超过32个字符。", "basic");
            if (draft.phone.Length == 0 || draft.phone.Length > 16)
                return FailValidation("手机号码不能为空且不能超过16个字符。", "basic");
            if (draft.accountType <= 0)
                return FailValidation("请选择账号类型。", "basic");
            if (draft.langCode.Length == 0)
                return FailValidation("请选择语言。", "basic");
            if (draft.orgId.Length == 0)
                return FailValidation("请选择所属机构。", "basic");
            if (draft.posId.Length == 0)
                return FailValidation("请选择职位。", "basic");
            if (draft.email.Length > 64)
                return FailValidation("邮箱不能超过64个字符。", "basic");
            if (draft.homepage.Length > 512)
                return FailValidation("个性化首页不能超过512个字符。", "basic");
            if (draft.domainAccount.Length > 32)
                return FailValidation("域账号不能超过32个字符。", "binding");
            if (draft.remark.Length > 256)
                return FailValidation("备注不能超过256个字符。", "profile");
            if (IsSuperAdmin && draft.tenantId.Length == 0)
                return FailValidation("超级管理员维护账号时必须选择租户。", "basic");

            for (var index = 0; index < editExtOrganizations.Length; index++)
            {
                var item = editExtOrganizations[index];
                if (item.orgId.Length == 0 || item.posId.Length == 0)
                    return FailValidation("附属机构的机构和职位不能为空。", "basic");
            }
            return true;
        }

        private bool FailValidation(string message, string tab)
        {
            editTab = tab;
            error = message;
            return false;
        }

        private UserSaveInput BuildSaveInput()
        {
            var original = editOriginal;
            return new UserSaveInput
            {
                id = draft.id,
                tenantId = Optional(draft.tenantId),
                orgId = draft.orgId,
                createTime = original?.createTime ?? string.Empty,
                updateTime = original?.updateTime,
                createUserId = original?.createUserId,
                createUserName = original?.createUserName,
                updateUserId = original?.updateUserId,
                updateUserName = original?.updateUserName,
                account = draft.account,
                realName = draft.realName,
                nickName = Optional(draft.nickName),
                avatar = original?.avatar,
                sex = draft.sex,
                age = draft.age < 0 ? 0 : draft.age > 150 ? 150 : draft.age,
                birthday = OptionalDate(draft.birthday),
                nation = Optional(draft.nation),
                phone = Optional(draft.phone),
                cardType = draft.cardType,
                idCardNum = Optional(draft.idCardNum),
                email = Optional(draft.email),
                address = Optional(draft.address),
                cultureLevel = draft.cultureLevel,
                politicalOutlook = Optional(draft.politicalOutlook),
                college = Optional(draft.college),
                officePhone = Optional(draft.officePhone),
                emergencyContact = Optional(draft.emergencyContact),
                emergencyPhone = Optional(draft.emergencyPhone),
                emergencyAddress = Optional(draft.emergencyAddress),
                introduction = Optional(draft.introduction),
                orderNo = draft.orderNo < 0 ? 0 : draft.orderNo,
                status = draft.status == 2 ? 2 : 1,
                remark = Optional(draft.remark),
                accountType = draft.accountType,
                managerUserId = original?.managerUserId,
                posId = draft.posId,
                jobNum = Optional(draft.jobNum),
                posLevel = original?.posLevel,
                posTitle = original?.posTitle,
                expertise = original?.expertise,
                officeZone = original?.officeZone,
                office = original?.office,
                joinDate = OptionalDate(draft.joinDate),
                lastLoginIp = original?.lastLoginIp,
                lastLoginAddress = original?.lastLoginAddress,
                lastLoginTime = original?.lastLoginTime,
                lastLoginDevice = original?.lastLoginDevice,
                signature = original?.signature,
                langCode = draft.langCode,
                homepage = Optional(draft.homepage),
                domainAccount = Optional(draft.domainAccount),
                roleIdList = CompactIds(editRoleIds),
                extOrgIdList = NormalizeExtOrganizations(editExtOrganizations)
            };
        }

        private void AddExtOrganization()
        {
            var result = new UserExtOrgInput[editExtOrganizations.Length + 1];
            for (var index = 0; index < editExtOrganizations.Length; index++)
                result[index] = editExtOrganizations[index];
            result[editExtOrganizations.Length] = new UserExtOrgInput();
            editExtOrganizations = result;
        }

        private void RemoveExtOrganization(int removeIndex)
        {
            if (removeIndex < 0 || removeIndex >= editExtOrganizations.Length)
                return;
            var result = new UserExtOrgInput[editExtOrganizations.Length - 1];
            var target = 0;
            for (var index = 0; index < editExtOrganizations.Length; index++)
                if (index != removeIndex)
                    result[target++] = editExtOrganizations[index];
            editExtOrganizations = result;
        }

        private void SetExtOrgOrganization(int index, string? value)
        {
            if (index >= 0 && index < editExtOrganizations.Length)
                editExtOrganizations[index].orgId = value ?? string.Empty;
        }

        private void SetExtOrgPosition(int index, string? value)
        {
            if (index >= 0 && index < editExtOrganizations.Length)
                editExtOrganizations[index].posId = value ?? string.Empty;
        }

        private void ToggleEditRole(string roleId)
        {
            if (CanEditDraftRoles)
                editRoleIds = ToggleId(editRoleIds, roleId);
        }

        private void OpenDetail(string id)
        {
            var row = Find(id);
            if (row is null || !CanDetail)
                return;
            detailUser = row;
            detailDialogVisible = true;
        }

        private void OpenGrantRoles(string id)
        {
            var row = Find(id);
            if (row is null || !CanGrantRole)
                return;

            grantUser = row;
            grantRoleOptions = [];
            grantRoleIds = [];
            grantDialogVisible = true;
            grantLoading = true;
            grantFailed = false;
            var pending = 2;
            Action done = () =>
            {
                pending--;
                if (pending <= 0)
                    grantLoading = false;
            };

            UserClient.Roles().Then(outcome =>
            {
                if (outcome.Ok)
                    grantRoleOptions = UserClient.ReadRoles(outcome.Data, row.tenantId ?? string.Empty);
                else
                {
                    grantFailed = true;
                    error = outcome.Error ?? "加载角色失败。";
                }
                done();
            }, _ =>
            {
                grantFailed = true;
                error = "加载角色失败，请检查网络后重试。";
                done();
            });

            UserClient.OwnRoles(row.id).Then(outcome =>
            {
                if (outcome.Ok)
                    grantRoleIds = CompactIds(UserClient.ReadIds(outcome.Data));
                else
                {
                    grantFailed = true;
                    error = outcome.Error ?? "加载账号角色失败。";
                }
                done();
            }, _ =>
            {
                grantFailed = true;
                error = "加载账号角色失败，请检查网络后重试。";
                done();
            });
        }

        private void ToggleGrantRole(string roleId)
            => grantRoleIds = ToggleId(grantRoleIds, roleId);

        private void SaveGrantRoles()
        {
            var user = grantUser;
            if (user is null || granting || grantLoading || grantFailed)
                return;

            granting = true;
            UserClient.GrantRole(user.id, CompactIds(grantRoleIds)).Then(outcome =>
            {
                granting = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "授权角色失败。";
                    return;
                }

                grantDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("角色授权已更新");
                LoadUsers();
            }, _ =>
            {
                granting = false;
                error = "授权角色失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || deletingId.Length > 0)
                return;

            deletingId = row.id;
            UserClient.Delete(row.id).Then(outcome =>
            {
                deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除账号失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("账号已删除");
                LoadUsers();
            }, _ =>
            {
                deletingId = string.Empty;
                error = "删除账号失败，请检查网络后重试。";
            });
        }

        private void SetStatus(string id)
        {
            var row = Find(id);
            if (row is null || !CanSetStatus || statusLoadingId.Length > 0)
                return;

            var next = row.status == 1 ? 2 : 1;
            statusLoadingId = row.id;
            UserClient.SetStatus(row.id, next).Then(outcome =>
            {
                statusLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "设置账号状态失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success(next == 1 ? "账号已启用" : "账号已停用");
                LoadUsers();
            }, _ =>
            {
                statusLoadingId = string.Empty;
                error = "设置账号状态失败，请检查网络后重试。";
            });
        }

        private void ResetPassword(string id)
        {
            var row = Find(id);
            if (row is null || !CanResetPassword || resetLoadingId.Length > 0)
                return;

            resetLoadingId = row.id;
            UserClient.ResetPassword(row.id).Then(outcome =>
            {
                resetLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "重置密码失败。";
                    return;
                }

                resetPasswordValue = UserClient.ReadPassword(outcome.Data);
                passwordDialogVisible = true;
                error = null;
            }, _ =>
            {
                resetLoadingId = string.Empty;
                error = "重置密码失败，请检查网络后重试。";
            });
        }

        private void UnlockLogin(string id)
        {
            var row = Find(id);
            if (row is null || !CanUnlockLogin || unlockLoadingId.Length > 0)
                return;

            unlockLoadingId = row.id;
            UserClient.UnlockLogin(row.id).Then(outcome =>
            {
                unlockLoadingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "解除登录锁定失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("登录锁定已解除");
            }, _ =>
            {
                unlockLoadingId = string.Empty;
                error = "解除登录锁定失败，请检查网络后重试。";
            });
        }

        private UserView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private string AccountTypeLabel(VueProps? row)
        {
            var value = RowInt(row, "accountType", 0).ToString();
            for (var index = 0; index < accountTypeOptions.Length; index++)
                if (accountTypeOptions[index].value == value)
                    return accountTypeOptions[index].label;
            return value == "0" ? "-" : "类型" + value;
        }

        private string AccountTypeTag(VueProps? row)
        {
            var value = RowInt(row, "accountType", 0).ToString();
            for (var index = 0; index < accountTypeOptions.Length; index++)
            {
                if (accountTypeOptions[index].value != value)
                    continue;
                return TagType(accountTypeOptions[index].tagType);
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
            => tenant.host.Length == 0 ? tenant.label : tenant.label + " (" + tenant.host + ")";

        private static string OrgButtonClass(string selectedId, string id)
            => "user-org-item" + (selectedId == id ? " is-active" : string.Empty);

        private static string OrgIndent(UserOrgOption option)
            => "padding-left:" + (12 + option.depth * 18) + "px";

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
                    id = Optional(item.id),
                    userId = Optional(item.userId),
                    orgId = item.orgId,
                    posId = item.posId,
                    jobNum = Optional(item.jobNum),
                    posLevel = Optional(item.posLevel),
                    joinDate = OptionalDate(item.joinDate)
                };
            }
            return result;
        }

        private static UserNamedOption[] EnsureNamedOption(UserNamedOption[] source, string id, string name)
        {
            if (id.Length == 0)
                return source;
            for (var index = 0; index < source.Length; index++)
                if (source[index].id == id)
                    return source;

            var result = new UserNamedOption[source.Length + 1];
            for (var index = 0; index < source.Length; index++)
                result[index] = source[index];
            result[source.Length] = new UserNamedOption
            {
                id = id,
                name = name.Length == 0 ? "历史选项 (" + id + ")" : name
            };
            return result;
        }

        private UserEnumOption[] FilterAccountTypes(UserEnumOption[] source)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
            {
                var value = source[index].value;
                if (value != "999" && (value != "888" || IsSuperAdmin))
                    count++;
            }

            var result = new UserEnumOption[count];
            var target = 0;
            for (var index = 0; index < source.Length; index++)
            {
                var value = source[index].value;
                if (value == "999" || (value == "888" && !IsSuperAdmin))
                    continue;
                result[target++] = source[index];
            }
            return result;
        }

        private static UserEnumOption[] DefaultAccountTypes()
            =>
            [
                new UserEnumOption { value = "777", number = 777, label = "普通账号", tagType = "primary" },
                new UserEnumOption { value = "666", number = 666, label = "会员", tagType = "success" }
            ];

        private static UserEnumOption[] DefaultCardTypes()
            =>
            [
                new UserEnumOption { value = "0", number = 0, label = "身份证" },
                new UserEnumOption { value = "1", number = 1, label = "护照" }
            ];

        private static UserEnumOption[] DefaultCultureLevels()
            =>
            [
                new UserEnumOption { value = "0", number = 0, label = "其他" },
                new UserEnumOption { value = "5", number = 5, label = "技工学校" },
                new UserEnumOption { value = "9", number = 9, label = "大学专科" },
                new UserEnumOption { value = "10", number = 10, label = "大学本科" }
            ];

        private static UserEnumOption[] DefaultGenders()
            =>
            [
                new UserEnumOption { value = "1", number = 1, label = "男性" },
                new UserEnumOption { value = "2", number = 2, label = "女性" },
                new UserEnumOption { value = "0", number = 0, label = "未知" }
            ];

        private static UserEnumOption[] DefaultNations()
            =>
            [
                new UserEnumOption { value = "1", label = "汉族" },
                new UserEnumOption { value = "2", label = "壮族" },
                new UserEnumOption { value = "3", label = "满族" },
                new UserEnumOption { value = "4", label = "回族" }
            ];

        private static UserLanguageOption[] DefaultLanguages(string current)
            => [new UserLanguageOption { value = current, label = current }];

        private static UserDraft NewDraft(string targetTenantId, string targetOrgId, string langCode)
            => new()
            {
                tenantId = targetTenantId,
                orgId = targetOrgId,
                accountType = 777,
                birthday = "2000-01-01",
                cardType = 0,
                cultureLevel = 5,
                langCode = langCode.Length == 0 ? "zh-CN" : langCode,
                orderNo = 100,
                sex = 1,
                status = 1
            };

        private static UserDraft DraftFrom(UserView row)
            => new()
            {
                id = row.id,
                tenantId = row.tenantId ?? string.Empty,
                orgId = row.orgId,
                account = row.account,
                realName = row.realName,
                nickName = row.nickName ?? string.Empty,
                sex = row.sex,
                age = row.age,
                birthday = row.birthday ?? string.Empty,
                nation = row.nation ?? string.Empty,
                phone = row.phone ?? string.Empty,
                cardType = row.cardType,
                idCardNum = row.idCardNum ?? string.Empty,
                email = row.email ?? string.Empty,
                address = row.address ?? string.Empty,
                cultureLevel = row.cultureLevel,
                politicalOutlook = row.politicalOutlook ?? string.Empty,
                college = row.college ?? string.Empty,
                officePhone = row.officePhone ?? string.Empty,
                emergencyContact = row.emergencyContact ?? string.Empty,
                emergencyPhone = row.emergencyPhone ?? string.Empty,
                emergencyAddress = row.emergencyAddress ?? string.Empty,
                introduction = row.introduction ?? string.Empty,
                orderNo = row.orderNo,
                status = row.status,
                remark = row.remark ?? string.Empty,
                accountType = row.accountType,
                posId = row.posId,
                jobNum = row.jobNum ?? string.Empty,
                joinDate = row.joinDate ?? string.Empty,
                langCode = row.langCode,
                homepage = row.homepage ?? string.Empty,
                domainAccount = row.domainAccount
            };

        private static UserDraft DraftCopyFrom(UserView row)
        {
            var result = DraftFrom(row);
            result.id = string.Empty;
            result.account = string.Empty;
            result.phone = string.Empty;
            result.domainAccount = string.Empty;
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

        private bool IsDeleting(VueProps? row) => deletingId == RowString(row, "id");
        private bool IsChangingStatus(VueProps? row) => statusLoadingId == RowString(row, "id");
        private bool IsResetting(VueProps? row) => resetLoadingId == RowString(row, "id");
        private bool IsUnlocking(VueProps? row) => unlockLoadingId == RowString(row, "id");

        private UserTableRow[] TableRows()
        {
            var result = new UserTableRow[rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = rows[index];
                result[index] = new UserTableRow(
                    row.id,
                    row.account,
                    row.realName,
                    row.phone ?? string.Empty,
                    row.roleName,
                    row.orgName,
                    row.posName,
                    row.orderNo,
                    DisplayValue(row.updateTime),
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
            var result = new UserTreeRow[orgTreeItems.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = orgTreeItems[index];
                result[index] = new UserTreeRow(row.id, row.name, row.path, row.tenantId, row.depth, row.disabled);
            }
            return result;
        }

        private UserExtOrganizationRow[] EditExtOrganizationsView()
        {
            var result = new UserExtOrganizationRow[editExtOrganizations.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new UserExtOrganizationRow(editExtOrganizations[index].orgId, editExtOrganizations[index].posId);
            return result;
        }

        private UserRoleRow[] RoleOptionsView() => RoleRows(roleOptions);
        private UserRoleRow[] GrantRoleOptionsView() => RoleRows(grantRoleOptions);

        private static UserRoleRow[] RoleRows(UserNamedOption[] source)
        {
            var result = new UserRoleRow[source.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new UserRoleRow(source[index].id, source[index].name);
            return result;
        }

        private ConfigurationOption[] TenantOptionsView()
        {
            var result = new ConfigurationOption[tenantItems.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(tenantItems[index].value, TenantLabel(tenantItems[index]));
            return result;
        }

        private ConfigurationOption[] EditorOrganizationOptionsView()
        {
            var result = new ConfigurationOption[editorOrgOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(editorOrgOptions[index].id, editorOrgOptions[index].path);
            return result;
        }

        private ConfigurationOption[] PositionOptionsView()
        {
            var result = new ConfigurationOption[posOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(posOptions[index].id, posOptions[index].name);
            return result;
        }

        private ConfigurationOption[] LanguageOptionsView()
        {
            var result = new ConfigurationOption[languageOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(languageOptions[index].value, languageOptions[index].label);
            return result;
        }

        private ConfigurationOption[] AccountTypeOptionsView() => EnumOptions(accountTypeOptions);
        private ConfigurationOption[] CardTypeOptionsView() => EnumOptions(cardTypeOptions);
        private ConfigurationOption[] CultureLevelOptionsView() => EnumOptions(cultureLevelOptions);
        private ConfigurationOption[] GenderOptionsView() => EnumOptions(genderOptions);
        private ConfigurationOption[] NationOptionsView() => EnumOptions(nationOptions);

        private static ConfigurationOption[] EnumOptions(UserEnumOption[] source)
        {
            var result = new ConfigurationOption[source.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(source[index].value, source[index].label);
            return result;
        }

        private readonly ConfigurationOption[] StatusOptionsView =
        [
            new ConfigurationOption("1", "启用"),
            new ConfigurationOption("2", "停用")
        ];

        public UserPageProps CreateProps() => new()
        {
            TenantLoading = tenantLoading,
            TenantOptions = TenantOptionsView(),
            EditorOrganizationOptions = EditorOrganizationOptionsView(),
            PositionOptions = PositionOptionsView(),
            LanguageOptions = LanguageOptionsView(),
            NationOptions = NationOptionsView(),
            AccountTypeOptions = AccountTypeOptionsView(),
            CardTypeOptions = CardTypeOptionsView(),
            GenderOptions = GenderOptionsView(),
            CultureLevelOptions = CultureLevelOptionsView(),
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
            StatusLoadingId = statusLoadingId,
            ResetLoadingId = resetLoadingId,
            UnlockLoadingId = unlockLoadingId,
            DeletingId = deletingId,
            SetStatus = SetStatus,
            OpenDetail = OpenDetail,
            OpenEdit = OpenEdit,
            OpenGrantRoles = OpenGrantRoles,
            OpenCopy = OpenCopy,
            ResetPassword = ResetPassword,
            UnlockLogin = UnlockLogin,
            Delete = Delete,
            TenantChanged = TenantChanged,
            TenantId = tenantId,
            SetDraftAccountType = value => draft.accountType = value.Length == 0 ? 777 : (int)ECMAScript.Global.NumberValue(value),
            DraftAccountType = draft.accountType.ToString(),
            SetDraftLangCode = value => draft.langCode = value,
            DraftLangCode = draft.langCode,
            SetDraftStatus = value => draft.status = value.Length == 0 ? 1 : (int)ECMAScript.Global.NumberValue(value),
            StatusOptions = StatusOptionsView,
            DraftStatus = draft.status.ToString(),
            SetDraftOrgId = value => draft.orgId = value,
            DraftOrgId = draft.orgId,
            SetDraftPosId = value => draft.posId = value,
            DraftPosId = draft.posId,
            ChangeExtOrganization = SetExtOrgOrganization,
            ChangeExtPosition = SetExtOrgPosition,
            SetDraftCardType = value => draft.cardType = value.Length == 0 ? 0 : (int)ECMAScript.Global.NumberValue(value),
            DraftCardType = draft.cardType.ToString(),
            SetDraftSex = value => draft.sex = value.Length == 0 ? 1 : (int)ECMAScript.Global.NumberValue(value),
            DraftSex = draft.sex.ToString(),
            SetDraftNation = value => draft.nation = value,
            DraftNation = draft.nation,
            SetDraftCultureLevel = value => draft.cultureLevel = value.Length == 0 ? 5 : (int)ECMAScript.Global.NumberValue(value),
            DraftCultureLevel = draft.cultureLevel.ToString(),
            DraftOrderNo = draft.orderNo,
            SetDraftOrderNo = value => draft.orderNo = (int)(value ?? 100),
            DraftAge = draft.age,
            SetDraftAge = value => draft.age = (int)(value ?? 0),
            DraftJoinDate = draft.joinDate,
            SetDraftJoinDate = value => draft.joinDate = value,
            DraftBirthday = draft.birthday,
            SetDraftBirthday = value => draft.birthday = value,
            SetAccount = value => account = value ?? "",
            SetRealName = value => realName = value ?? "",
            SetPosName = value => posName = value ?? "",
            SetPhone = value => phone = value ?? "",
            SetDraftAccount = value => draft.account = value ?? "",
            SetDraftRealName = value => draft.realName = value ?? "",
            SetDraftNickName = value => draft.nickName = value ?? "",
            SetDraftPhone = value => draft.phone = value ?? "",
            SetDraftEmail = value => draft.email = value ?? "",
            SetDraftJobNum = value => draft.jobNum = value ?? "",
            SetDraftHomepage = value => draft.homepage = value ?? "",
            SetDraftIdCardNum = value => draft.idCardNum = value ?? "",
            SetDraftCollege = value => draft.college = value ?? "",
            SetDraftPoliticalOutlook = value => draft.politicalOutlook = value ?? "",
            SetDraftOfficePhone = value => draft.officePhone = value ?? "",
            SetDraftAddress = value => draft.address = value ?? "",
            SetDraftEmergencyContact = value => draft.emergencyContact = value ?? "",
            SetDraftEmergencyPhone = value => draft.emergencyPhone = value ?? "",
            SetDraftEmergencyAddress = value => draft.emergencyAddress = value ?? "",
            SetDraftIntroduction = value => draft.introduction = value ?? "",
            SetDraftRemark = value => draft.remark = value ?? "",
            SetDraftDomainAccount = value => draft.domainAccount = value ?? "",
            SetEditDialogVisible = value => editDialogVisible = value,
            SetDetailDialogVisible = value => detailDialogVisible = value,
            SetGrantDialogVisible = value => grantDialogVisible = value,
            SetPasswordDialogVisible = value => passwordDialogVisible = value,
            CloseEditDialogVisible = () => editDialogVisible = false,
            CloseGrantDialogVisible = () => grantDialogVisible = false,
            ClosePasswordDialogVisible = () => passwordDialogVisible = false,
            RemoveExtOrganization = RemoveExtOrganization,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            Refresh = Refresh,
            AddExtOrganization = AddExtOrganization,
            Save = Save,
            SaveGrantRoles = SaveGrantRoles,
            DismissError = () => error = null,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            Editing = draft.id.Length > 0,
            DraftAccount = draft.account,
            DraftRealName = draft.realName,
            DraftNickName = draft.nickName,
            DraftPhone = draft.phone,
            DraftEmail = draft.email,
            DraftJobNum = draft.jobNum,
            DraftHomepage = draft.homepage,
            DraftIdCardNum = draft.idCardNum,
            DraftCollege = draft.college,
            DraftPoliticalOutlook = draft.politicalOutlook,
            DraftOfficePhone = draft.officePhone,
            DraftAddress = draft.address,
            DraftEmergencyContact = draft.emergencyContact,
            DraftEmergencyPhone = draft.emergencyPhone,
            DraftEmergencyAddress = draft.emergencyAddress,
            DraftIntroduction = draft.introduction,
            DraftRemark = draft.remark,
            DraftDomainAccount = draft.domainAccount,
            DetailAccount = DisplayValue(detailUser?.account),
            DetailRealName = DisplayValue(detailUser?.realName),
            DetailNickName = DisplayValue(detailUser?.nickName),
            DetailPhone = DisplayValue(detailUser?.phone),
            DetailEmail = DisplayValue(detailUser?.email),
            DetailRoleName = DisplayValue(detailUser?.roleName),
            DetailOrgName = DisplayValue(detailUser?.orgName),
            DetailPosName = DisplayValue(detailUser?.posName),
            DetailJobNum = DisplayValue(detailUser?.jobNum),
            DetailLangCode = DisplayValue(detailUser?.langCode),
            DetailLastLoginIp = DisplayValue(detailUser?.lastLoginIp),
            DetailLastLoginAddress = DisplayValue(detailUser?.lastLoginAddress),
            DetailLastLoginTime = DisplayValue(detailUser?.lastLoginTime),
            DetailLastLoginDevice = DisplayValue(detailUser?.lastLoginDevice),
            DetailCreateTime = DisplayValue(detailUser?.createTime),
            DetailUpdateTime = DisplayValue(detailUser?.updateTime),
            DetailRemark = DisplayValue(detailUser?.remark),
            DetailCreateUserName = DisplayUser(detailUser?.createUserName, detailUser?.createUserId),
            DetailUpdateUserName = DisplayUser(detailUser?.updateUserName, detailUser?.updateUserId),
            DetailAccountType = detailUser is null ? "-" : detailUser.accountType.ToString(),
            DetailStatus = detailUser?.status == 1 ? "启用" : "停用",
            GrantAccount = DisplayValue(grantUser?.account),
            GrantRealName = DisplayValue(grantUser?.realName),
            Error = error,
            Account = account,
            RealName = realName,
            PosName = posName,
            Phone = phone,
            Loading = loading,
            OrgLoading = orgLoading,
            SelectedOrgId = selectedOrgId,
            SelectedOrgName = selectedOrgName,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            EditDialogVisible = editDialogVisible,
            EditDialogTitle = editDialogTitle,
            EditTab = editTab,
            Saving = saving,
            DependencyLoading = dependencyLoading,
            DependencyFailed = dependencyFailed,
            EditRoleIds = editRoleIds,
            GrantRoleIds = grantRoleIds,
            DetailDialogVisible = detailDialogVisible,
            GrantDialogVisible = grantDialogVisible,
            GrantLoading = grantLoading,
            Granting = granting,
            GrantFailed = grantFailed,
            PasswordDialogVisible = passwordDialogVisible,
            ResetPasswordValue = resetPasswordValue,
            CanEditDraftRoles = CanEditDraftRoles,
            IsSuperAdmin = IsSuperAdmin,
            OrgTreeItems = OrgTreeItemsView(),
            RoleOptions = RoleOptionsView(),
            GrantRoleOptions = GrantRoleOptionsView(),
            EditExtOrganizations = EditExtOrganizationsView(),
            PageSizes = pageSizes,
            SelectOrganization = SelectOrganization,
            SelectEditTab = SelectEditTab,
            ToggleEditRole = ToggleEditRole,
            ToggleGrantRole = ToggleGrantRole,
        };
    }
}

public sealed record UserPageProps : VueProps
{
    public required bool TenantLoading { get; init; }
    public required ConfigurationOption[] TenantOptions { get; init; }
    public required ConfigurationOption[] EditorOrganizationOptions { get; init; }
    public required ConfigurationOption[] PositionOptions { get; init; }
    public required ConfigurationOption[] LanguageOptions { get; init; }
    public required ConfigurationOption[] NationOptions { get; init; }
    public required ConfigurationOption[] AccountTypeOptions { get; init; }
    public required ConfigurationOption[] CardTypeOptions { get; init; }
    public required ConfigurationOption[] GenderOptions { get; init; }
    public required ConfigurationOption[] CultureLevelOptions { get; init; }
    public required UserTableRow[] Rows { get; init; }
    public required bool CanSetStatus { get; init; }
    public required bool CanDetail { get; init; }
    public required bool HasActionColumn { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanGrantRole { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanResetPassword { get; init; }
    public required bool CanUnlockLogin { get; init; }
    public required bool CanDelete { get; init; }
    public required string StatusLoadingId { get; init; }
    public required string ResetLoadingId { get; init; }
    public required string UnlockLoadingId { get; init; }
    public required string DeletingId { get; init; }
    public required Action<string> SetStatus { get; init; }
    public required Action<string> OpenDetail { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> OpenGrantRoles { get; init; }
    public required Action<string> OpenCopy { get; init; }
    public required Action<string> ResetPassword { get; init; }
    public required Action<string> UnlockLogin { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string> TenantChanged { get; init; }
    public required string TenantId { get; init; }
    public required Action<string> SetDraftAccountType { get; init; }
    public required string DraftAccountType { get; init; }
    public required Action<string> SetDraftLangCode { get; init; }
    public required string DraftLangCode { get; init; }
    public required Action<string> SetDraftStatus { get; init; }
    public required ConfigurationOption[] StatusOptions { get; init; }
    public required string DraftStatus { get; init; }
    public required Action<string> SetDraftOrgId { get; init; }
    public required string DraftOrgId { get; init; }
    public required Action<string> SetDraftPosId { get; init; }
    public required string DraftPosId { get; init; }
    public required Action<int, string?> ChangeExtOrganization { get; init; }
    public required Action<int, string?> ChangeExtPosition { get; init; }
    public required Action<string> SetDraftCardType { get; init; }
    public required string DraftCardType { get; init; }
    public required Action<string> SetDraftSex { get; init; }
    public required string DraftSex { get; init; }
    public required Action<string> SetDraftNation { get; init; }
    public required string DraftNation { get; init; }
    public required Action<string> SetDraftCultureLevel { get; init; }
    public required string DraftCultureLevel { get; init; }
    public required int DraftOrderNo { get; init; }
    public required Action<double?> SetDraftOrderNo { get; init; }
    public required int DraftAge { get; init; }
    public required Action<double?> SetDraftAge { get; init; }
    public required string DraftJoinDate { get; init; }
    public required Action<string> SetDraftJoinDate { get; init; }
    public required string DraftBirthday { get; init; }
    public required Action<string> SetDraftBirthday { get; init; }
    public required Action<string?> SetAccount { get; init; }
    public required Action<string?> SetRealName { get; init; }
    public required Action<string?> SetPosName { get; init; }
    public required Action<string?> SetPhone { get; init; }
    public required Action<string?> SetDraftAccount { get; init; }
    public required Action<string?> SetDraftRealName { get; init; }
    public required Action<string?> SetDraftNickName { get; init; }
    public required Action<string?> SetDraftPhone { get; init; }
    public required Action<string?> SetDraftEmail { get; init; }
    public required Action<string?> SetDraftJobNum { get; init; }
    public required Action<string?> SetDraftHomepage { get; init; }
    public required Action<string?> SetDraftIdCardNum { get; init; }
    public required Action<string?> SetDraftCollege { get; init; }
    public required Action<string?> SetDraftPoliticalOutlook { get; init; }
    public required Action<string?> SetDraftOfficePhone { get; init; }
    public required Action<string?> SetDraftAddress { get; init; }
    public required Action<string?> SetDraftEmergencyContact { get; init; }
    public required Action<string?> SetDraftEmergencyPhone { get; init; }
    public required Action<string?> SetDraftEmergencyAddress { get; init; }
    public required Action<string?> SetDraftIntroduction { get; init; }
    public required Action<string?> SetDraftRemark { get; init; }
    public required Action<string?> SetDraftDomainAccount { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetDetailDialogVisible { get; init; }
    public required Action<bool> SetGrantDialogVisible { get; init; }
    public required Action<bool> SetPasswordDialogVisible { get; init; }
    public required Action CloseEditDialogVisible { get; init; }
    public required Action CloseGrantDialogVisible { get; init; }
    public required Action ClosePasswordDialogVisible { get; init; }
    public required Action<int> RemoveExtOrganization { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action Refresh { get; init; }
    public required Action AddExtOrganization { get; init; }
    public required Action Save { get; init; }
    public required Action SaveGrantRoles { get; init; }
    public required Action DismissError { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required bool Editing { get; init; }
    public required string DraftAccount { get; init; }
    public required string DraftRealName { get; init; }
    public required string DraftNickName { get; init; }
    public required string DraftPhone { get; init; }
    public required string DraftEmail { get; init; }
    public required string DraftJobNum { get; init; }
    public required string DraftHomepage { get; init; }
    public required string DraftIdCardNum { get; init; }
    public required string DraftCollege { get; init; }
    public required string DraftPoliticalOutlook { get; init; }
    public required string DraftOfficePhone { get; init; }
    public required string DraftAddress { get; init; }
    public required string DraftEmergencyContact { get; init; }
    public required string DraftEmergencyPhone { get; init; }
    public required string DraftEmergencyAddress { get; init; }
    public required string DraftIntroduction { get; init; }
    public required string DraftRemark { get; init; }
    public required string DraftDomainAccount { get; init; }
    public required string DetailAccount { get; init; }
    public required string DetailRealName { get; init; }
    public required string DetailNickName { get; init; }
    public required string DetailPhone { get; init; }
    public required string DetailEmail { get; init; }
    public required string DetailRoleName { get; init; }
    public required string DetailOrgName { get; init; }
    public required string DetailPosName { get; init; }
    public required string DetailJobNum { get; init; }
    public required string DetailLangCode { get; init; }
    public required string DetailLastLoginIp { get; init; }
    public required string DetailLastLoginAddress { get; init; }
    public required string DetailLastLoginTime { get; init; }
    public required string DetailLastLoginDevice { get; init; }
    public required string DetailCreateTime { get; init; }
    public required string DetailUpdateTime { get; init; }
    public required string DetailRemark { get; init; }
    public required string DetailCreateUserName { get; init; }
    public required string DetailUpdateUserName { get; init; }
    public required string DetailAccountType { get; init; }
    public required string DetailStatus { get; init; }
    public required string GrantAccount { get; init; }
    public required string GrantRealName { get; init; }
    public required string? Error { get; init; }
    public required string Account { get; init; }
    public required string RealName { get; init; }
    public required string PosName { get; init; }
    public required string Phone { get; init; }
    public required bool Loading { get; init; }
    public required bool OrgLoading { get; init; }
    public required string SelectedOrgId { get; init; }
    public required string SelectedOrgName { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditTab { get; init; }
    public required bool Saving { get; init; }
    public required bool DependencyLoading { get; init; }
    public required bool DependencyFailed { get; init; }
    public required string[] EditRoleIds { get; init; }
    public required string[] GrantRoleIds { get; init; }
    public required bool DetailDialogVisible { get; init; }
    public required bool GrantDialogVisible { get; init; }
    public required bool GrantLoading { get; init; }
    public required bool Granting { get; init; }
    public required bool GrantFailed { get; init; }
    public required bool PasswordDialogVisible { get; init; }
    public required string ResetPasswordValue { get; init; }
    public required bool CanEditDraftRoles { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required UserTreeRow[] OrgTreeItems { get; init; }
    public required UserRoleRow[] RoleOptions { get; init; }
    public required UserRoleRow[] GrantRoleOptions { get; init; }
    public required UserExtOrganizationRow[] EditExtOrganizations { get; init; }
    public required double[] PageSizes { get; init; }
    public required Action<string, string, string> SelectOrganization { get; init; }
    public required Action<string> SelectEditTab { get; init; }
    public required Action<string> ToggleEditRole { get; init; }
    public required Action<string> ToggleGrantRole { get; init; }
}


public sealed record UserDraft : VueProps
{
    public string id { get; set; } = string.Empty;
    public string tenantId { get; set; } = string.Empty;
    public string orgId { get; set; } = string.Empty;
    public string account { get; set; } = string.Empty;
    public string realName { get; set; } = string.Empty;
    public string nickName { get; set; } = string.Empty;
    public int sex { get; set; } = 1;
    public int age { get; set; }
    public string birthday { get; set; } = string.Empty;
    public string nation { get; set; } = string.Empty;
    public string phone { get; set; } = string.Empty;
    public int cardType { get; set; }
    public string idCardNum { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string address { get; set; } = string.Empty;
    public int cultureLevel { get; set; } = 5;
    public string politicalOutlook { get; set; } = string.Empty;
    public string college { get; set; } = string.Empty;
    public string officePhone { get; set; } = string.Empty;
    public string emergencyContact { get; set; } = string.Empty;
    public string emergencyPhone { get; set; } = string.Empty;
    public string emergencyAddress { get; set; } = string.Empty;
    public string introduction { get; set; } = string.Empty;
    public int orderNo { get; set; } = 100;
    public int status { get; set; } = 1;
    public string remark { get; set; } = string.Empty;
    public int accountType { get; set; } = 777;
    public string posId { get; set; } = string.Empty;
    public string jobNum { get; set; } = string.Empty;
    public string joinDate { get; set; } = string.Empty;
    public string langCode { get; set; } = "zh-CN";
    public string homepage { get; set; } = string.Empty;
    public string domainAccount { get; set; } = string.Empty;

}
