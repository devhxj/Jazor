using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Auth;
using Jero.Web.Infrastructure.Api.Profile;
using Jero.Web.Pages.InfoSetting;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Pages.Auth;
using SmCrypto = Npm.SmCryptoV2.Exports;

namespace Jero.Web.Pages.Profile;

[ECMAScriptModule("components/profile-page-state.js")]
public static class ProfilePageModule
{
    public static ProfilePageState Create() => new();

    public sealed class ProfilePageState : global::Jero.Web.Composition.IPageState<ProfilePageProps>
    {
        private FileRef? _avatarFile;
        private FileRef? _signatureFile;

        private const string AvatarInputId = "profile-avatar-input";
        private const string SignatureInputId = "profile-signature-input";
        private const string EmptyImageDataUrl = "data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=";

        private readonly ProfileSettingItem[] _securityItems =
        [
            new() { Value = true, FieldName = "accountPassword", Label = "账户密码", Description = "当前密码强度：强" },
            new() { Value = true, FieldName = "securityPhone", Label = "密保手机", Description = "已绑定手机：138****8293" },
            new() { Value = true, FieldName = "securityQuestion", Label = "密保问题", Description = "未设置密保问题，密保问题可有效保护账户安全" },
            new() { Value = true, FieldName = "securityEmail", Label = "备用邮箱", Description = "已绑定邮箱：ant***sign.com" },
            new() { Value = false, FieldName = "securityMfa", Label = "MFA 设备", Description = "未绑定 MFA 设备，绑定后，可以进行二次确认" }
        ];

        private readonly ProfileSettingItem[] _notificationItems =
        [
            new() { Value = true, FieldName = "accountPassword", Label = "账户密码", Description = "其他用户的消息将以站内信的形式通知" },
            new() { Value = true, FieldName = "systemMessage", Label = "系统消息", Description = "系统消息将以站内信的形式通知" },
            new() { Value = true, FieldName = "todoTask", Label = "待办任务", Description = "待办任务将以站内信的形式通知" }
        ];

        private ProfileUser? _profile;
        private string _activeTab = "basic";
        private string _editRealName = string.Empty;
        private string _editIntroduction = string.Empty;
        private string _passwordOld = string.Empty;
        private string _passwordNew = string.Empty;
        private string _passwordConfirm = string.Empty;
        private string? _error;
        private bool _loading;
        private bool _saving;
        private bool _changingPassword;
        private bool _uploadingAvatar;
        private bool _uploadingSignature;

        private bool CanBaseInfo => ApplicationState.HasPermission("sysUser:baseInfo");
        private bool CanChangePassword => ApplicationState.HasPermission("sysUser:changePwd");
        private bool CanUploadAvatar => ApplicationState.HasPermission("sysFile:uploadAvatar");
        private bool CanUploadSignature => ApplicationState.HasPermission("sysFile:uploadSignature");
        private string[] RoleIds => ApplicationState.User?.RoleIds ?? [];

        public void Initialize()
            => LoadProfile();

        private void LoadProfile()
        {
            _loading = true;
            _error = null;
            ProfileClient.BaseInfo().Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载个人信息失败。";
                    return;
                }

                var loaded = ProfileClient.ReadUser(outcome.Data);
                if (loaded is null)
                {
                    _error = "个人信息不存在。";
                    return;
                }

                _profile = loaded;
                _editRealName = loaded.RealName;
                _editIntroduction = loaded.Introduction ?? string.Empty;
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载个人信息失败，请检查网络后重试。";
            });
        }

        private void SelectTab(string value) => _activeTab = value;

        private void SaveBaseInfo()
        {
            var current = _profile;
            if (current is null || _saving || !CanBaseInfo)
                return;

            var normalizedName = _editRealName.Trim();
            if (normalizedName.Length == 0)
            {
                _error = "姓名不能为空。";
                return;
            }
            if (normalizedName.Length > 32)
            {
                _error = "姓名不能超过32个字符。";
                return;
            }
            if (_editIntroduction.Length > 512)
            {
                _error = "个人简介不能超过512个字符。";
                return;
            }

            var oldName = current.RealName;
            var oldIntroduction = current.Introduction;
            current.RealName = normalizedName;
            current.Introduction = _editIntroduction;
            _saving = true;
            ProfileClient.UpdateBaseInfo(current).Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    current.RealName = oldName;
                    current.Introduction = oldIntroduction;
                    _error = outcome.Error ?? "保存个人信息失败。";
                    return;
                }

                _error = null;
                ProfileStateChange.Notify(new ProfileShellChange { RealName = current.RealName });
                ElementPlus.ElMessageMembers.Success("基本信息已更新");
            }, _ =>
            {
                _saving = false;
                current.RealName = oldName;
                current.Introduction = oldIntroduction;
                _error = "保存个人信息失败，请检查网络后重试。";
            });
        }

        private void ChangePassword()
        {
            if (_changingPassword || !CanChangePassword)
                return;
            if (_passwordOld.Length == 0)
            {
                _error = "请输入旧密码。";
                return;
            }
            if (_passwordNew.Length < 5)
            {
                _error = "新密码至少需要5个字符。";
                return;
            }
            if (_passwordNew != _passwordConfirm)
            {
                _error = "两次输入的密码不一致。";
                return;
            }
            if (_passwordOld == _passwordNew)
            {
                _error = "新密码不能与旧密码相同。";
                return;
            }

            _changingPassword = true;
            // 公钥可能随服务端配置变化，必须紧邻改密请求重新获取，不能复用页面初始化快照。
            AuthClient.LoginConfig().Then(configOutcome =>
            {
                if (!configOutcome.Ok)
                {
                    _changingPassword = false;
                    _error = configOutcome.Error ?? "加载密码加密配置失败。";
                    return;
                }

                var currentPublicKey = AuthClient.ReadPublicKey(configOutcome.Data);
                if (currentPublicKey.Length == 0)
                {
                    _changingPassword = false;
                    _error = "服务端未返回密码加密公钥。";
                    return;
                }

                // ChangePwd 与登录使用同一 SM2 C1C3C2 协议；两个字段都要单独加密。
                var encryptedOld = SmCrypto.Sm2.DoEncrypt(_passwordOld, currentPublicKey, 1);
                var encryptedNew = SmCrypto.Sm2.DoEncrypt(_passwordNew, currentPublicKey, 1);
                ProfileClient.ChangePassword(encryptedOld, encryptedNew).Then(outcome =>
                {
                    _changingPassword = false;
                    if (!outcome.Ok)
                    {
                        _error = outcome.Error ?? "修改密码失败。";
                        return;
                    }

                    _passwordOld = string.Empty;
                    _passwordNew = string.Empty;
                    _passwordConfirm = string.Empty;
                    _error = null;
                    ElementPlus.ElMessageMembers.Success("密码修改成功");
                }, _ =>
                {
                    _changingPassword = false;
                    _error = "修改密码失败，请检查网络后重试。";
                });
            }, _ =>
            {
                _changingPassword = false;
                _error = "加载密码加密配置失败，请检查网络后重试。";
            });
        }

        private void SelectAvatar()
        {
            // 在事件回调中取出原生 File，再开始异步操作。
            _avatarFile = BrowserFileSelection.Snapshot(AvatarInputId);
        }

        private void UploadAvatar()
        {
            if (!CanUploadAvatar || _uploadingAvatar)
                return;

            if (_avatarFile is null)
            {
                _error = "请先选择图片。";
                return;
            }

            _uploadingAvatar = true;
            ProfileBrowserActions.UploadSelectedImage(_avatarFile, "/api/sysFile/uploadAvatar").Then(outcome =>
            {
                _uploadingAvatar = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "上传头像失败。";
                    return;
                }

                if (_profile is not null)
                    _profile.Avatar = outcome.RawUrl;
                _avatarFile = null;
                ProfileBrowserActions.ClearSelectedFile(AvatarInputId);
                _error = null;
                ProfileStateChange.Notify(new ProfileShellChange { Avatar = outcome.Url });
                ElementPlus.ElMessageMembers.Success("头像已更新");
            }, _ =>
            {
                _uploadingAvatar = false;
                _error = "上传头像失败，请检查网络后重试。";
            });
        }

        private void SelectSignature()
        {
            // 在事件回调中取出原生 File，再开始异步操作。
            _signatureFile = BrowserFileSelection.Snapshot(SignatureInputId);
        }

        private void UploadSignature()
        {
            if (!CanUploadSignature || _uploadingSignature)
                return;

            if (_signatureFile is null)
            {
                _error = "请先选择图片。";
                return;
            }

            _uploadingSignature = true;
            ProfileBrowserActions.UploadSelectedImage(_signatureFile, "/api/sysFile/uploadSignature").Then(outcome =>
            {
                _uploadingSignature = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "上传电子签名失败。";
                    return;
                }

                if (_profile is not null)
                    _profile.Signature = outcome.RawUrl;
                _signatureFile = null;
                ProfileBrowserActions.ClearSelectedFile(SignatureInputId);
                _error = null;
                ElementPlus.ElMessageMembers.Success("电子签名已更新");
            }, _ =>
            {
                _uploadingSignature = false;
                _error = "上传电子签名失败，请检查网络后重试。";
            });
        }

        private void ToggleSecurity(int index)
            => _securityItems[index].Value = !_securityItems[index].Value;

        private void ToggleNotification(int index)
            => _notificationItems[index].Value = !_notificationItems[index].Value;

        private string DisplayAvatar()
        {
            var value = _profile?.Avatar ?? ApplicationState.User?.Avatar ?? string.Empty;
            return NormalizeImageUrl(value);
        }

        private string DisplaySignature()
            => NormalizeImageUrl(_profile?.Signature ?? string.Empty);

        private static string ImageSource(string value)
            => value.Length == 0 ? EmptyImageDataUrl : value;

        private string DisplayRealName()
        {
            var value = _profile?.RealName ?? ApplicationState.User?.RealName ?? string.Empty;
            return value.Length == 0 ? DisplayAccount() : value;
        }

        private string DisplayAccount()
            => _profile?.Account ?? ApplicationState.User?.Account ?? string.Empty;

        private string AvatarInitial()
        {
            var name = DisplayRealName();
            return name.Length == 0 ? "U" : name.Substring(0, 1).ToUpperInvariant();
        }

        private static string RoleLabel(string role)
            => role == "super" ? "管理员" : role == "user" ? "用户" : role == "test" ? "测试" : role;

        private static string NormalizeImageUrl(string value)
        {
            if (value.Length == 0 || value.StartsWith('/') || value.StartsWith("http://") || value.StartsWith("https://"))
                return value;
            return "/" + value;
        }

        public ProfilePageProps CreateProps() => new()
        {
            HasProfile = _profile is not null,
            Account = _profile?.Account ?? "",
            DisplayRealName = DisplayRealName(),
            DisplayAccount = DisplayAccount(),
            AvatarInitial = AvatarInitial(),
            DisplayAvatar = DisplayAvatar(),
            DisplaySignature = DisplaySignature(),
            AvatarSource = ImageSource(DisplayAvatar()),
            SignatureSource = ImageSource(DisplaySignature()),
            AvatarInputId = AvatarInputId,
            SignatureInputId = SignatureInputId,
            CanBaseInfo = CanBaseInfo,
            CanChangePassword = CanChangePassword,
            CanUploadAvatar = CanUploadAvatar,
            CanUploadSignature = CanUploadSignature,
            ActiveTab = _activeTab,
            EditRealName = _editRealName,
            SetEditRealName = value => _editRealName = value ?? "",
            EditIntroduction = _editIntroduction,
            SetEditIntroduction = value => _editIntroduction = value ?? "",
            PasswordOld = _passwordOld,
            SetPasswordOld = value => _passwordOld = value ?? "",
            PasswordNew = _passwordNew,
            SetPasswordNew = value => _passwordNew = value ?? "",
            PasswordConfirm = _passwordConfirm,
            SetPasswordConfirm = value => _passwordConfirm = value ?? "",
            Error = _error,
            Loading = _loading,
            Saving = _saving,
            ChangingPassword = _changingPassword,
            UploadingAvatar = _uploadingAvatar,
            UploadingSignature = _uploadingSignature,
            SaveBaseInfo = SaveBaseInfo,
            ChangePassword = ChangePassword,
            UploadAvatar = UploadAvatar,
            UploadSignature = UploadSignature,
            SelectAvatar = SelectAvatar,
            SelectSignature = SelectSignature,
            DismissError = () => _error = null,
            SelectTab = SelectTab,
            ToggleSecurity = ToggleSecurity,
            ToggleNotification = ToggleNotification,
            RoleIds = RoleIds,
            SecurityItems = _securityItems,
            NotificationItems = _notificationItems,
        };
    }
}

public sealed record ProfilePageProps : VueProps
{
    public required bool HasProfile { get; init; }
    public required string Account { get; init; }
    public required string DisplayRealName { get; init; }
    public required string DisplayAccount { get; init; }
    public required string AvatarInitial { get; init; }
    public required string DisplayAvatar { get; init; }
    public required string DisplaySignature { get; init; }
    public required string AvatarSource { get; init; }
    public required string SignatureSource { get; init; }
    public required string AvatarInputId { get; init; }
    public required string SignatureInputId { get; init; }
    public required bool CanBaseInfo { get; init; }
    public required bool CanChangePassword { get; init; }
    public required bool CanUploadAvatar { get; init; }
    public required bool CanUploadSignature { get; init; }
    public required string ActiveTab { get; init; }
    public required string EditRealName { get; init; }
    public required Action<string?> SetEditRealName { get; init; }
    public required string EditIntroduction { get; init; }
    public required Action<string?> SetEditIntroduction { get; init; }
    public required string PasswordOld { get; init; }
    public required Action<string?> SetPasswordOld { get; init; }
    public required string PasswordNew { get; init; }
    public required Action<string?> SetPasswordNew { get; init; }
    public required string PasswordConfirm { get; init; }
    public required Action<string?> SetPasswordConfirm { get; init; }
    public required string? Error { get; init; }
    public required bool Loading { get; init; }
    public required bool Saving { get; init; }
    public required bool ChangingPassword { get; init; }
    public required bool UploadingAvatar { get; init; }
    public required bool UploadingSignature { get; init; }
    public required Action SaveBaseInfo { get; init; }
    public required Action ChangePassword { get; init; }
    public required Action UploadAvatar { get; init; }
    public required Action UploadSignature { get; init; }
    public required Action SelectAvatar { get; init; }
    public required Action SelectSignature { get; init; }
    public required Action DismissError { get; init; }
    public required Action<string> SelectTab { get; init; }
    public required Action<int> ToggleSecurity { get; init; }
    public required Action<int> ToggleNotification { get; init; }
    public required string[] RoleIds { get; init; }
    public required ProfileSettingItem[] SecurityItems { get; init; }
    public required ProfileSettingItem[] NotificationItems { get; init; }
}
