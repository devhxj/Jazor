using Jero.Web.App.InfoSetting;
using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Auth;
using SmCrypto = Npm.SmCryptoV2.Exports;

namespace Jero.Web.App.Profile;

[ECMAScriptModule("components/profile-page-state.js")]
public static class ProfilePageModule
{
    public static ProfilePageState Create() => new();

    public sealed class ProfilePageState : global::Jero.Web.App.IPageState<ProfilePageProps>
    {
        private FileRef? avatarFile;
        private FileRef? signatureFile;

        private const string AvatarInputId = "profile-avatar-input";
        private const string SignatureInputId = "profile-signature-input";
        private const string EmptyImageDataUrl = "data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=";

        private readonly ProfileSettingItem[] securityItems =
        [
            new() { value = true, fieldName = "accountPassword", label = "账户密码", description = "当前密码强度：强" },
            new() { value = true, fieldName = "securityPhone", label = "密保手机", description = "已绑定手机：138****8293" },
            new() { value = true, fieldName = "securityQuestion", label = "密保问题", description = "未设置密保问题，密保问题可有效保护账户安全" },
            new() { value = true, fieldName = "securityEmail", label = "备用邮箱", description = "已绑定邮箱：ant***sign.com" },
            new() { value = false, fieldName = "securityMfa", label = "MFA 设备", description = "未绑定 MFA 设备，绑定后，可以进行二次确认" }
        ];

        private readonly ProfileSettingItem[] notificationItems =
        [
            new() { value = true, fieldName = "accountPassword", label = "账户密码", description = "其他用户的消息将以站内信的形式通知" },
            new() { value = true, fieldName = "systemMessage", label = "系统消息", description = "系统消息将以站内信的形式通知" },
            new() { value = true, fieldName = "todoTask", label = "待办任务", description = "待办任务将以站内信的形式通知" }
        ];

        private ProfileUser? profile;
        private string activeTab = "basic";
        private string editRealName = string.Empty;
        private string editIntroduction = string.Empty;
        private string passwordOld = string.Empty;
        private string passwordNew = string.Empty;
        private string passwordConfirm = string.Empty;
        private string? error;
        private bool loading;
        private bool saving;
        private bool changingPassword;
        private bool uploadingAvatar;
        private bool uploadingSignature;

        private bool CanBaseInfo => ApplicationState.HasPermission("sysUser:baseInfo");
        private bool CanChangePassword => ApplicationState.HasPermission("sysUser:changePwd");
        private bool CanUploadAvatar => ApplicationState.HasPermission("sysFile:uploadAvatar");
        private bool CanUploadSignature => ApplicationState.HasPermission("sysFile:uploadSignature");
        private string[] RoleIds => ApplicationState.User?.RoleIds ?? [];

        public void Initialize()
            => LoadProfile();

        private void LoadProfile()
        {
            loading = true;
            error = null;
            ProfileClient.BaseInfo().Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载个人信息失败。";
                    return;
                }

                var loaded = ProfileClient.ReadUser(outcome.Data);
                if (loaded is null)
                {
                    error = "个人信息不存在。";
                    return;
                }

                profile = loaded;
                editRealName = loaded.realName;
                editIntroduction = loaded.introduction ?? string.Empty;
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载个人信息失败，请检查网络后重试。";
            });
        }

        private void SelectTab(string value) => activeTab = value;

        private void SaveBaseInfo()
        {
            var current = profile;
            if (current is null || saving || !CanBaseInfo)
                return;

            var normalizedName = editRealName.Trim();
            if (normalizedName.Length == 0)
            {
                error = "姓名不能为空。";
                return;
            }
            if (normalizedName.Length > 32)
            {
                error = "姓名不能超过32个字符。";
                return;
            }
            if (editIntroduction.Length > 512)
            {
                error = "个人简介不能超过512个字符。";
                return;
            }

            var oldName = current.realName;
            var oldIntroduction = current.introduction;
            current.realName = normalizedName;
            current.introduction = editIntroduction;
            saving = true;
            ProfileClient.UpdateBaseInfo(current).Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    current.realName = oldName;
                    current.introduction = oldIntroduction;
                    error = outcome.Error ?? "保存个人信息失败。";
                    return;
                }

                error = null;
                ProfileStateChange.Notify(new ProfileShellChange { RealName = current.realName });
                ElementPlus.ElMessageMembers.Success("基本信息已更新");
            }, _ =>
            {
                saving = false;
                current.realName = oldName;
                current.introduction = oldIntroduction;
                error = "保存个人信息失败，请检查网络后重试。";
            });
        }

        private void ChangePassword()
        {
            if (changingPassword || !CanChangePassword)
                return;
            if (passwordOld.Length == 0)
            {
                error = "请输入旧密码。";
                return;
            }
            if (passwordNew.Length < 5)
            {
                error = "新密码至少需要5个字符。";
                return;
            }
            if (passwordNew != passwordConfirm)
            {
                error = "两次输入的密码不一致。";
                return;
            }
            if (passwordOld == passwordNew)
            {
                error = "新密码不能与旧密码相同。";
                return;
            }

            changingPassword = true;
            // 公钥可能随服务端配置变化，必须紧邻改密请求重新获取，不能复用页面初始化快照。
            AuthClient.LoginConfig().Then(configOutcome =>
            {
                if (!configOutcome.Ok)
                {
                    changingPassword = false;
                    error = configOutcome.Error ?? "加载密码加密配置失败。";
                    return;
                }

                var currentPublicKey = AuthClient.ReadPublicKey(configOutcome.Data);
                if (currentPublicKey.Length == 0)
                {
                    changingPassword = false;
                    error = "服务端未返回密码加密公钥。";
                    return;
                }

                // ChangePwd 与登录使用同一 SM2 C1C3C2 协议；两个字段都要单独加密。
                var encryptedOld = SmCrypto.Sm2.DoEncrypt(passwordOld, currentPublicKey, 1);
                var encryptedNew = SmCrypto.Sm2.DoEncrypt(passwordNew, currentPublicKey, 1);
                ProfileClient.ChangePassword(encryptedOld, encryptedNew).Then(outcome =>
                {
                    changingPassword = false;
                    if (!outcome.Ok)
                    {
                        error = outcome.Error ?? "修改密码失败。";
                        return;
                    }

                    passwordOld = string.Empty;
                    passwordNew = string.Empty;
                    passwordConfirm = string.Empty;
                    error = null;
                    ElementPlus.ElMessageMembers.Success("密码修改成功");
                }, _ =>
                {
                    changingPassword = false;
                    error = "修改密码失败，请检查网络后重试。";
                });
            }, _ =>
            {
                changingPassword = false;
                error = "加载密码加密配置失败，请检查网络后重试。";
            });
        }

        private void SelectAvatar()
        {
            // Snapshot the native File during the callback, before any async work.
            avatarFile = BrowserFileSelection.Snapshot(AvatarInputId);
        }

        private void UploadAvatar()
        {
            if (!CanUploadAvatar || uploadingAvatar)
                return;

            if (avatarFile is null)
            {
                error = "请先选择图片。";
                return;
            }

            uploadingAvatar = true;
            ProfileBrowserActions.UploadSelectedImage(avatarFile, "/api/sysFile/uploadAvatar").Then(outcome =>
            {
                uploadingAvatar = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "上传头像失败。";
                    return;
                }

                if (profile is not null)
                    profile.avatar = outcome.RawUrl;
                avatarFile = null;
                ProfileBrowserActions.ClearSelectedFile(AvatarInputId);
                error = null;
                ProfileStateChange.Notify(new ProfileShellChange { Avatar = outcome.Url });
                ElementPlus.ElMessageMembers.Success("头像已更新");
            }, _ =>
            {
                uploadingAvatar = false;
                error = "上传头像失败，请检查网络后重试。";
            });
        }

        private void SelectSignature()
        {
            // Snapshot the native File during the callback, before any async work.
            signatureFile = BrowserFileSelection.Snapshot(SignatureInputId);
        }

        private void UploadSignature()
        {
            if (!CanUploadSignature || uploadingSignature)
                return;

            if (signatureFile is null)
            {
                error = "请先选择图片。";
                return;
            }

            uploadingSignature = true;
            ProfileBrowserActions.UploadSelectedImage(signatureFile, "/api/sysFile/uploadSignature").Then(outcome =>
            {
                uploadingSignature = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "上传电子签名失败。";
                    return;
                }

                if (profile is not null)
                    profile.signature = outcome.RawUrl;
                signatureFile = null;
                ProfileBrowserActions.ClearSelectedFile(SignatureInputId);
                error = null;
                ElementPlus.ElMessageMembers.Success("电子签名已更新");
            }, _ =>
            {
                uploadingSignature = false;
                error = "上传电子签名失败，请检查网络后重试。";
            });
        }

        private void ToggleSecurity(int index)
            => securityItems[index].value = !securityItems[index].value;

        private void ToggleNotification(int index)
            => notificationItems[index].value = !notificationItems[index].value;

        private string DisplayAvatar()
        {
            var value = profile?.avatar ?? ApplicationState.User?.Avatar ?? string.Empty;
            return NormalizeImageUrl(value);
        }

        private string DisplaySignature()
            => NormalizeImageUrl(profile?.signature ?? string.Empty);

        private static string ImageSource(string value)
            => value.Length == 0 ? EmptyImageDataUrl : value;

        private string DisplayRealName()
        {
            var value = profile?.realName ?? ApplicationState.User?.RealName ?? string.Empty;
            return value.Length == 0 ? DisplayAccount() : value;
        }

        private string DisplayAccount()
            => profile?.account ?? ApplicationState.User?.Account ?? string.Empty;

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
            HasProfile = profile is not null,
            Account = profile?.account ?? "",
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
            ActiveTab = activeTab,
            EditRealName = editRealName,
            SetEditRealName = value => editRealName = value ?? "",
            EditIntroduction = editIntroduction,
            SetEditIntroduction = value => editIntroduction = value ?? "",
            PasswordOld = passwordOld,
            SetPasswordOld = value => passwordOld = value ?? "",
            PasswordNew = passwordNew,
            SetPasswordNew = value => passwordNew = value ?? "",
            PasswordConfirm = passwordConfirm,
            SetPasswordConfirm = value => passwordConfirm = value ?? "",
            Error = error,
            Loading = loading,
            Saving = saving,
            ChangingPassword = changingPassword,
            UploadingAvatar = uploadingAvatar,
            UploadingSignature = uploadingSignature,
            SaveBaseInfo = SaveBaseInfo,
            ChangePassword = ChangePassword,
            UploadAvatar = UploadAvatar,
            UploadSignature = UploadSignature,
            SelectAvatar = SelectAvatar,
            SelectSignature = SelectSignature,
            DismissError = () => error = null,
            SelectTab = SelectTab,
            ToggleSecurity = ToggleSecurity,
            ToggleNotification = ToggleNotification,
            RoleIds = RoleIds,
            SecurityItems = securityItems,
            NotificationItems = notificationItems,
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
