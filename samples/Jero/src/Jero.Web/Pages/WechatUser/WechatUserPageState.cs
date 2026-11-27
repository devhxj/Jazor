using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.WechatUser;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.WechatUser;

[ECMAScriptModule("components/wechat-user-page-state.js")]
public static class WechatUserPageModule
{
    public static WechatUserPageState Create() => new();

    public sealed class WechatUserPageState : IWechatUserContentState
    {
        private WechatUserView[] _items = [];
        public WechatUserView[] Rows { get; private set; } = [];
        private WechatUserPlatformOption[] _platformOptions = DefaultPlatformOptions();
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _queryNickName = string.Empty;
        private string _queryMobile = string.Empty;
        private bool _loading;
        private string? _error;
        public string DeletingId { get; private set; } = string.Empty;

        private bool _editDialogVisible;
        private bool _saving;
        private WechatUserView? _editOriginal;
        private string _editNickName = string.Empty;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

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
                    _platformOptions = loaded;
            }, _ => { });
        }

        private void LoadPage()
        {
            _loading = true;
            WechatUserClient.Page(new WechatUserPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                NickName = _queryNickName.Trim(),
                Mobile = _queryMobile.Trim()
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载三方账号失败。";
                    _items = [];
                    Rows = [];
                    _total = 0;
                    return;
                }

                _items = WechatUserClient.ReadItems(outcome.Data);
                Rows = _items;
                _total = WechatUserClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载三方账号失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _queryNickName = string.Empty;
            _queryMobile = string.Empty;
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

        public void OpenEdit(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || !CanEdit)
                return;

            _editOriginal = row;
            _editNickName = row.NickName;
            _error = null;
            _editDialogVisible = true;
        }

        private void Save()
        {
            if (_saving)
                return;

            var row = _editOriginal;
            var normalizedNickName = _editNickName.Trim();
            if (row is null)
            {
                _error = "当前三方账号不存在。";
                return;
            }
            if (normalizedNickName.Length == 0)
            {
                _error = "昵称不能为空。";
                return;
            }
            if (normalizedNickName.Length > 64)
            {
                _error = "昵称不能超过64个字符。";
                return;
            }

            // 后端将输入映射为实体后更新，提交时要保留不可编辑的绑定信息和审计字段。
            var request = new WechatUserSaveInput
            {
                Id = row.Id,
                UserId = RequiredId(row.UserId),
                PlatformType = row.PlatformType,
                OpenId = row.OpenId,
                SessionKey = Optional(row.SessionKey),
                UnionId = Optional(row.UnionId),
                NickName = normalizedNickName,
                Avatar = Optional(row.Avatar),
                Mobile = Optional(row.Mobile),
                Sex = row.Sex,
                Language = Optional(row.Language),
                City = Optional(row.City),
                Province = Optional(row.Province),
                Country = Optional(row.Country),
                AccessToken = Optional(row.AccessToken),
                RefreshToken = Optional(row.RefreshToken),
                ExpiresIn = row.ExpiresIn,
                Scope = Optional(row.Scope),
                CreateTime = row.CreateTime,
                CreateUserId = Optional(row.CreateUserId),
                CreateUserName = Optional(row.CreateUserName),
                UpdateTime = Optional(row.UpdateTime),
                UpdateUserId = Optional(row.UpdateUserId),
                UpdateUserName = Optional(row.UpdateUserName)
            };

            _saving = true;
            WechatUserClient.Update(request).Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "更新三方账号失败。";
                    return;
                }

                _editDialogVisible = false;
                _editOriginal = null;
                _error = null;
                ElementPlus.ElMessageMembers.Success("三方账号已更新");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "更新三方账号失败，请检查网络后重试。";
            });
        }

        public void Delete(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || !CanDelete || DeletingId.Length > 0)
                return;

            DeletingId = row.Id;
            WechatUserClient.Delete(row.Id).Then(outcome =>
            {
                DeletingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除三方账号失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("三方账号已删除");
                LoadPage();
            }, _ =>
            {
                DeletingId = string.Empty;
                _error = "删除三方账号失败，请检查网络后重试。";
            });
        }

        public void OpenAudit(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null)
                return;

            _auditDialogTitle = "修改记录 · " + DisplayName(row);
            _auditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            _auditCreateTime = DisplayValue(row.CreateTime);
            _auditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            _auditUpdateTime = DisplayValue(row.UpdateTime);
            _auditDialogVisible = true;
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
            => DeletingId.Length > 0 && DeletingId == RowString(row, "id");

        private WechatUserView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private string OptionLabel(int value)
        {
            for (var index = 0; index < _platformOptions.Length; index++)
                if (_platformOptions[index].Value == value)
                    return _platformOptions[index].Label;
            return "平台" + value;
        }

        private string OptionTag(int value)
        {
            for (var index = 0; index < _platformOptions.Length; index++)
            {
                if (_platformOptions[index].Value != value)
                    continue;
                var tag = _platformOptions[index].TagType;
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
                new WechatUserPlatformOption { Value = 1, Label = "微信公众号", TagType = "primary" },
                new WechatUserPlatformOption { Value = 2, Label = "微信小程序", TagType = "success" },
                new WechatUserPlatformOption { Value = 3, Label = "QQ", TagType = "info" },
                new WechatUserPlatformOption { Value = 4, Label = "支付宝", TagType = "warning" },
                new WechatUserPlatformOption { Value = 5, Label = "Gitee", TagType = "danger" }
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
            if (row.NickName.Length > 0)
                return row.NickName;
            return DisplayValue(row.OpenId);
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
            SetQueryNickName = value => _queryNickName = value,
            SetQueryMobile = value => _queryMobile = value,
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetEditNickName = value => _editNickName = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            DismissError = () => _error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            CancelEdit = () => _editDialogVisible = false,
            Save = Save,
            EditAvatar = _editOriginal?.Avatar ?? "",
            EditOpenId = _editOriginal?.OpenId ?? "未绑定 OpenId",
            EditMobile = _editOriginal?.Mobile ?? "",
            EditUnionId = _editOriginal?.UnionId ?? "",
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            QueryNickName = _queryNickName,
            QueryMobile = _queryMobile,
            Loading = _loading,
            EditDialogVisible = _editDialogVisible,
            Saving = _saving,
            EditNickName = _editNickName,
            AuditDialogVisible = _auditDialogVisible,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            Error = _error,
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
