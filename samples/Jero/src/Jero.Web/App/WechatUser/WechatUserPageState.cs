using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.WechatUser;

[ECMAScriptModule("components/wechat-user-page-state.js")]
public static class WechatUserPageModule
{
    public static WechatUserPageState Create() => new();

    public sealed class WechatUserPageState : IWechatUserContentState
    {
        private WechatUserView[] items = [];
        public WechatUserView[] rows { get; private set; } = [];
        private WechatUserPlatformOption[] platformOptions = DefaultPlatformOptions();
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string queryNickName = string.Empty;
        private string queryMobile = string.Empty;
        private bool loading;
        private string? error;
        public string deletingId { get; private set; } = string.Empty;

        private bool editDialogVisible;
        private bool saving;
        private WechatUserView? editOriginal;
        private string editNickName = string.Empty;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

        public bool CanEdit => ApplicationState.HasPermission("sysWechatUser:update");
        public bool CanDelete => ApplicationState.HasPermission("sysWechatUser:delete");
        public bool HasActionColumn => CanEdit || CanDelete;

        public void Initialize()
        {
            LoadPlatformOptions();
            LoadPage();
        }

        private void LoadPlatformOptions()
        {
            WechatUserClient.PlatformTypes().Then(outcome =>
            {
                if (!outcome.Ok)
                    return;

                var loaded = WechatUserClient.ReadPlatformOptions(outcome.Data);
                if (loaded.Length > 0)
                    platformOptions = loaded;
            }, _ => { });
        }

        private void LoadPage()
        {
            loading = true;
            WechatUserClient.Page(new WechatUserPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                nickName = queryNickName.Trim(),
                mobile = queryMobile.Trim()
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载三方账号失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }

                items = WechatUserClient.ReadItems(outcome.Data);
                rows = items;
                total = WechatUserClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载三方账号失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            queryNickName = string.Empty;
            queryMobile = string.Empty;
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

        public void OpenEdit(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || !CanEdit)
                return;

            editOriginal = row;
            editNickName = row.nickName;
            error = null;
            editDialogVisible = true;
        }

        private void Save()
        {
            if (saving)
                return;

            var row = editOriginal;
            var normalizedNickName = editNickName.Trim();
            if (row is null)
            {
                error = "当前三方账号不存在。";
                return;
            }
            if (normalizedNickName.Length == 0)
            {
                error = "昵称不能为空。";
                return;
            }
            if (normalizedNickName.Length > 64)
            {
                error = "昵称不能超过64个字符。";
                return;
            }

            // The backend updates an adapted entity; retain every non-editable binding and audit field.
            var request = new WechatUserSaveInput
            {
                id = row.id,
                userId = RequiredId(row.userId),
                platformType = row.platformType,
                openId = row.openId,
                sessionKey = Optional(row.sessionKey),
                unionId = Optional(row.unionId),
                nickName = normalizedNickName,
                avatar = Optional(row.avatar),
                mobile = Optional(row.mobile),
                sex = row.sex,
                language = Optional(row.language),
                city = Optional(row.city),
                province = Optional(row.province),
                country = Optional(row.country),
                accessToken = Optional(row.accessToken),
                refreshToken = Optional(row.refreshToken),
                expiresIn = row.expiresIn,
                scope = Optional(row.scope),
                createTime = row.createTime,
                createUserId = Optional(row.createUserId),
                createUserName = Optional(row.createUserName),
                updateTime = Optional(row.updateTime),
                updateUserId = Optional(row.updateUserId),
                updateUserName = Optional(row.updateUserName)
            };

            saving = true;
            WechatUserClient.Update(request).Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "更新三方账号失败。";
                    return;
                }

                editDialogVisible = false;
                editOriginal = null;
                error = null;
                ElementPlus.ElMessageMembers.Success("三方账号已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "更新三方账号失败，请检查网络后重试。";
            });
        }

        public void Delete(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || !CanDelete || deletingId.Length > 0)
                return;

            deletingId = row.id;
            WechatUserClient.Delete(row.id).Then(outcome =>
            {
                deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除三方账号失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("三方账号已删除");
                LoadPage();
            }, _ =>
            {
                deletingId = string.Empty;
                error = "删除三方账号失败，请检查网络后重试。";
            });
        }

        public void OpenAudit(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null)
                return;

            auditDialogTitle = "修改记录 · " + DisplayName(row);
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = DisplayValue(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = DisplayValue(row.updateTime);
            auditDialogVisible = true;
        }

        public string PlatformLabel(VueProps? row)
            => OptionLabel(RowInt(row, "platformType", 1));

        public string PlatformTag(VueProps? row)
            => OptionTag(RowInt(row, "platformType", 1));

        private static string GenderText(VueProps? row)
            => RowInt(row, "sex", 0) == 0 ? "男" : "女";

        private static string GenderTag(VueProps? row)
            => RowInt(row, "sex", 0) == 0 ? "primary" : "danger";

        private static string AvatarSource(VueProps? row)
            => RowString(row, "avatar");

        private static string DisplayRowValue(VueProps? row, string key)
            => DisplayValue(RowString(row, key));

        public bool IsDeletingRow(VueProps? row)
            => deletingId.Length > 0 && deletingId == RowString(row, "id");

        private WechatUserView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private string OptionLabel(int value)
        {
            for (var index = 0; index < platformOptions.Length; index++)
                if (platformOptions[index].value == value)
                    return platformOptions[index].label;
            return "平台" + value;
        }

        private string OptionTag(int value)
        {
            for (var index = 0; index < platformOptions.Length; index++)
            {
                if (platformOptions[index].value != value)
                    continue;
                var tag = platformOptions[index].tagType;
                if (tag == "success") return "success";
                if (tag == "warning") return "warning";
                if (tag == "danger") return "danger";
                if (tag == "info") return "info";
                return DefaultPlatformTag(value);
            }
            return DefaultPlatformTag(value);
        }

        private static string DefaultPlatformTag(int value)
        {
            if (value == 2) return "success";
            if (value == 3) return "info";
            if (value == 4) return "warning";
            if (value == 5) return "danger";
            return "primary";
        }

        private static WechatUserPlatformOption[] DefaultPlatformOptions()
            =>
            [
                new WechatUserPlatformOption { value = 1, label = "微信公众号", tagType = "primary" },
                new WechatUserPlatformOption { value = 2, label = "微信小程序", tagType = "success" },
                new WechatUserPlatformOption { value = 3, label = "QQ", tagType = "info" },
                new WechatUserPlatformOption { value = 4, label = "支付宝", tagType = "warning" },
                new WechatUserPlatformOption { value = 5, label = "Gitee", tagType = "danger" }
            ];

        private static string RowString(VueProps? row, string key)
        {
            if (row is null)
                return string.Empty;
            var value = Reflect.Get(row, key);
            if (value is null)
                return string.Empty;
            var result = StringValue(value);
            return result == "null" || result == "undefined" ? string.Empty : result;
        }

        private static int RowInt(VueProps? row, string key, int fallback)
        {
            if (row is null)
                return fallback;
            var value = Reflect.Get(row, key);
            return value is null ? fallback : (int)NumberValue(value);
        }

        private static string DisplayName(WechatUserView row)
        {
            if (row.nickName.Length > 0)
                return row.nickName;
            return DisplayValue(row.openId);
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

        private static string RequiredId(string value)
            => value.Length == 0 ? "0" : value;

        private static string? Optional(string value)
            => value.Length == 0 ? null : value;

        public string GenderTextForView(VueProps? row) => GenderText(row);
        public string GenderTagForView(VueProps? row) => GenderTag(row);
        public string AvatarSourceForView(VueProps? row) => AvatarSource(row);
        public string DisplayRowValueForView(VueProps? row, string key) => DisplayRowValue(row, key);

        public WechatUserPageProps CreateProps() => new()
        {
            SetQueryNickName = value => queryNickName = value,
            SetQueryMobile = value => queryMobile = value,
            SetEditDialogVisible = value => editDialogVisible = value,
            SetEditNickName = value => editNickName = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            DismissError = () => error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            CancelEdit = () => editDialogVisible = false,
            Save = Save,
            EditAvatar = editOriginal?.avatar ?? "",
            EditOpenId = editOriginal?.openId ?? "未绑定 OpenId",
            EditMobile = editOriginal?.mobile ?? "",
            EditUnionId = editOriginal?.unionId ?? "",
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            QueryNickName = queryNickName,
            QueryMobile = queryMobile,
            Loading = loading,
            EditDialogVisible = editDialogVisible,
            Saving = saving,
            EditNickName = editNickName,
            AuditDialogVisible = auditDialogVisible,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            Error = error,
        };
    }
}

public sealed record WechatUserPageProps : VueProps
{
    public required Action<string> SetQueryNickName { get; init; }
    public required Action<string> SetQueryMobile { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<string> SetEditNickName { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action DismissError { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action Save { get; init; }
    public required string EditAvatar { get; init; }
    public required string EditOpenId { get; init; }
    public required string EditMobile { get; init; }
    public required string EditUnionId { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required string QueryNickName { get; init; }
    public required string QueryMobile { get; init; }
    public required bool Loading { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool Saving { get; init; }
    public required string EditNickName { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required string? Error { get; init; }
}
