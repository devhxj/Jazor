using Jero.Web.App.Components;
using Jero.Web.App.Shell;

namespace Jero.Web.App.InfoSetting;

[ECMAScriptModule("components/info-setting-page-state.js")]
public static class InfoSettingPageModule
{
    public static InfoSettingPageState Create() => new();

    public sealed class InfoSettingPageState : global::Jero.Web.App.IPageState<InfoSettingPageProps>
    {
        private FileRef? logoFile;

        private const string LogoInputId = "info-setting-logo-input";
        private const string DefaultHomePath = "/dashboard/home";
        private const string EmptyImageDataUrl = "data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=";

        private InfoSettingWayOption[] wayOptions = [];
        private string storedLogo = string.Empty;
        private string previewLogo = string.Empty;
        private string selectedLogoFileName = string.Empty;
        private string title = string.Empty;
        private string viceTitle = string.Empty;
        private string viceDesc = string.Empty;
        private string watermark = string.Empty;
        private string copyright = string.Empty;
        private string icp = string.Empty;
        private string icpUrl = string.Empty;
        private string defaultHomePath = DefaultHomePath;
        private string regWayId = string.Empty;
        private int enableReg = 2;
        private int secondVer = 2;
        private int captcha = 2;
        private bool hideTenantForLogin;
        private bool loading;
        private bool readingLogo;
        private bool saving;
        private string? error;

        private bool RegistrationEnabled => enableReg == 1;
        private string CurrentLogo => previewLogo.Length > 0 ? previewLogo : NormalizeImageUrl(storedLogo);

        public void Initialize() => Load();

        private void Load()
        {
            loading = true;
            error = null;
            InfoSettingClient.GetSysInfo().Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载系统配置失败。";
                    return;
                }

                Apply(InfoSettingClient.ReadSysInfo(outcome.Data));
            }, _ =>
            {
                loading = false;
                error = "加载系统配置失败，请检查网络后重试。";
            });
        }

        private void Apply(InfoSettingView value)
        {
            storedLogo = value.logo;
            previewLogo = string.Empty;
            selectedLogoFileName = string.Empty;
            title = value.title;
            viceTitle = value.viceTitle;
            viceDesc = value.viceDesc;
            watermark = value.watermark;
            copyright = value.copyright;
            icp = value.icp;
            icpUrl = value.icpUrl;
            defaultHomePath = NormalizeLoadedHomePath(value.defaultHomePath);
            enableReg = value.enableReg == 1 ? 1 : 2;
            regWayId = value.regWayId;
            secondVer = value.secondVer == 1 ? 1 : 2;
            captcha = value.captcha == 1 ? 1 : 2;
            hideTenantForLogin = value.hideTenantForLogin;
            wayOptions = value.wayList;
            ApplicationState.UpdateRuntimeConfig(
                value.title,
                NormalizeImageUrl(value.logo),
                value.watermark,
                NormalizeLoadedHomePath(value.defaultHomePath));
            logoFile = null;
            InfoSettingBrowserActions.ClearSelectedLogo(LogoInputId);
            error = null;
        }

        private void SelectLogo()
        {
            // Snapshot the native File during the callback, before any async work.
            logoFile = BrowserFileSelection.Snapshot(LogoInputId);
        }

        private void PreviewSelectedLogo()
        {
            if (readingLogo || saving)
                return;

            readingLogo = true;
            InfoSettingBrowserActions.ReadSelectedLogo(logoFile).Then(selection =>
            {
                readingLogo = false;
                if (!selection.Ok)
                {
                    error = selection.Error ?? "读取 Logo 文件失败。";
                    return;
                }
                if (!selection.HasFile)
                {
                    error = "请先选择 Logo 图片。";
                    return;
                }

                previewLogo = selection.DataUrl;
                selectedLogoFileName = selection.FileName;
                error = null;
            }, _ =>
            {
                readingLogo = false;
                error = "读取 Logo 文件失败。";
            });
        }

        private void RestoreStoredLogo()
        {
            previewLogo = string.Empty;
            selectedLogoFileName = string.Empty;
            logoFile = null;
            InfoSettingBrowserActions.ClearSelectedLogo(LogoInputId);
        }

        private void EnableRegistrationChanged(double? value)
        {
            enableReg = (int)(value ?? 2);
            if (!RegistrationEnabled)
                regWayId = string.Empty;
        }

        private void RegistrationWayChanged(string? value)
            => regWayId = value ?? string.Empty;

        private void Save()
        {
            if (saving || loading || readingLogo || !Validate())
                return;

            saving = true;
            // Save consumes the selection captured by onchange, even without an explicit preview.
            InfoSettingBrowserActions.ReadSelectedLogo(logoFile).Then(selection =>
            {
                if (!selection.Ok)
                {
                    saving = false;
                    error = selection.Error ?? "读取 Logo 文件失败。";
                    return;
                }

                var input = BuildSaveInput(selection);
                InfoSettingClient.SaveSysInfo(input).Then(outcome =>
                {
                    saving = false;
                    if (!outcome.Ok)
                    {
                        error = outcome.Error ?? "保存系统配置失败。";
                        return;
                    }

                    error = null;
                    ElementPlus.ElMessageMembers.Success("系统配置已保存");
                    Load();
                }, _ =>
                {
                    saving = false;
                    error = "保存系统配置失败，请检查网络后重试。";
                });
            }, _ =>
            {
                saving = false;
                error = "读取 Logo 文件失败。";
            });
        }

        private InfoSettingSaveInput BuildSaveInput(InfoSettingLogoSelection selection)
            => new()
            {
                logoBase64 = selection.HasFile ? selection.DataUrl : string.Empty,
                logoFileName = selection.HasFile ? selection.FileName : string.Empty,
                watermark = watermark.Trim(),
                title = title.Trim(),
                viceTitle = viceTitle.Trim(),
                viceDesc = viceDesc.Trim(),
                copyright = copyright.Trim(),
                icp = icp.Trim(),
                icpUrl = icpUrl.Trim(),
                enableReg = RegistrationEnabled ? 1 : 2,
                // 这两个值仅透传读取快照；后端当前未开放保存逻辑，页面也不提供编辑入口。
                secondVer = secondVer,
                captcha = captcha,
                regWayId = RegistrationEnabled ? regWayId : "0",
                defaultHomePath = NormalizeHomePath(defaultHomePath)
            };

        private bool Validate()
        {
            var normalizedTitle = title.Trim();
            var normalizedViceTitle = viceTitle.Trim();
            var normalizedViceDesc = viceDesc.Trim();
            var normalizedCopyright = copyright.Trim();
            var normalizedIcp = icp.Trim();
            var normalizedIcpUrl = icpUrl.Trim();
            var normalizedWatermark = watermark.Trim();
            var normalizedHomePath = defaultHomePath.Trim();

            if (normalizedTitle.Length == 0 || normalizedTitle.Length > 32)
                return Fail(normalizedTitle.Length == 0 ? "系统主标题不能为空。" : "系统主标题不能超过32个字符。");
            if (normalizedViceTitle.Length == 0 || normalizedViceTitle.Length > 32)
                return Fail(normalizedViceTitle.Length == 0 ? "系统副标题不能为空。" : "系统副标题不能超过32个字符。");
            if (normalizedViceDesc.Length == 0 || normalizedViceDesc.Length > 64)
                return Fail(normalizedViceDesc.Length == 0 ? "系统描述不能为空。" : "系统描述不能超过64个字符。");
            if (normalizedWatermark.Length > 32)
                return Fail("水印内容不能超过32个字符。");
            if (normalizedCopyright.Length == 0 || normalizedCopyright.Length > 64)
                return Fail(normalizedCopyright.Length == 0 ? "版权说明不能为空。" : "版权说明不能超过64个字符。");
            if (normalizedIcp.Length == 0 || normalizedIcp.Length > 32)
                return Fail(normalizedIcp.Length == 0 ? "ICP备案号不能为空。" : "ICP备案号不能超过32个字符。");
            if (normalizedIcpUrl.Length == 0 || normalizedIcpUrl.Length > 32)
                return Fail(normalizedIcpUrl.Length == 0 ? "ICP 地址不能为空。" : "ICP 地址不能超过32个字符。");
            if (!IsValidHomePath(normalizedHomePath))
                return Fail("默认首页必须是以 / 开头的后台内部路由，不能包含空白、反斜杠或外部协议。");
            if (RegistrationEnabled && regWayId.Length == 0)
                return Fail("启用注册后必须选择默认注册方案。");

            error = null;
            return true;
        }

        private bool Fail(string message)
        {
            error = message;
            return false;
        }

        private static string NormalizeLoadedHomePath(string value)
            => IsValidHomePath(value.Trim()) ? value.Trim() : DefaultHomePath;

        private static string NormalizeHomePath(string value)
            => IsValidHomePath(value.Trim()) ? value.Trim() : DefaultHomePath;

        private static bool IsValidHomePath(string value)
        {
            if (value.Length == 0 || value.Length > 512 || !value.StartsWith('/') || value.StartsWith("//") || value.Contains("://") || value.Contains('\\'))
                return false;
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (char.IsWhiteSpace(character) || char.IsControl(character))
                    return false;
            }
            return true;
        }

        private static string NormalizeImageUrl(string value)
        {
            // Current seed path and bundled default asset differ; map only the known default.
            if (value == "/upload/logo.png")
                return "/images/logo.png";
            if (value.Length == 0 || value.StartsWith('/') || value.StartsWith("http://") || value.StartsWith("https://") || value.StartsWith("data:"))
                return value;
            return "/" + value;
        }

        private static string ImageSource(string value)
            => value.Length == 0 ? EmptyImageDataUrl : value;
        private ConfigurationOption[] RegistrationOptions()
        {
            var options = new ConfigurationOption[wayOptions.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(wayOptions[index].value, wayOptions[index].label);
            return options;
        }

        public InfoSettingPageProps CreateProps() => new()
        {
            LogoSource = ImageSource(CurrentLogo),
            CurrentLogo = CurrentLogo,
            DefaultHomePlaceholder = DefaultHomePath,
            SelectedLogoFileName = selectedLogoFileName,
            RegWayId = regWayId,
            EnableReg = enableReg,
            WayOptions = RegistrationOptions(),
            RegistrationEnabled = RegistrationEnabled,
            HideTenantForLogin = hideTenantForLogin,
            Loading = loading,
            Saving = saving,
            ReadingLogo = readingLogo,
            Error = error,
            Save = Save,
            PreviewSelectedLogo = PreviewSelectedLogo,
            RestoreStoredLogo = RestoreStoredLogo,
            DismissError = () => error = null,
            SetEnableReg = value => EnableRegistrationChanged(value),
            SetRegWayId = RegistrationWayChanged,
            Title = title,
            SetTitle = value => title = value ?? "",
            ViceTitle = viceTitle,
            SetViceTitle = value => viceTitle = value ?? "",
            ViceDesc = viceDesc,
            SetViceDesc = value => viceDesc = value ?? "",
            Watermark = watermark,
            SetWatermark = value => watermark = value ?? "",
            Copyright = copyright,
            SetCopyright = value => copyright = value ?? "",
            Icp = icp,
            SetIcp = value => icp = value ?? "",
            IcpUrl = icpUrl,
            SetIcpUrl = value => icpUrl = value ?? "",
            DefaultHomePath = defaultHomePath,
            SetDefaultHomePath = value => defaultHomePath = value ?? "",
            LogoInputId = LogoInputId,
            SelectLogo = SelectLogo,
        };
    }
}

public sealed record InfoSettingPageProps : VueProps
{
    public required string LogoSource { get; init; }
    public required string CurrentLogo { get; init; }
    public required string DefaultHomePlaceholder { get; init; }
    public required string SelectedLogoFileName { get; init; }
    public required string RegWayId { get; init; }
    public required int EnableReg { get; init; }
    public required ConfigurationOption[] WayOptions { get; init; }
    public required bool RegistrationEnabled { get; init; }
    public required bool HideTenantForLogin { get; init; }
    public required bool Loading { get; init; }
    public required bool Saving { get; init; }
    public required bool ReadingLogo { get; init; }
    public required string? Error { get; init; }
    public required Action Save { get; init; }
    public required Action PreviewSelectedLogo { get; init; }
    public required Action RestoreStoredLogo { get; init; }
    public required Action DismissError { get; init; }
    public required Action<double> SetEnableReg { get; init; }
    public required Action<string> SetRegWayId { get; init; }
    public required string Title { get; init; }
    public required Action<string?> SetTitle { get; init; }
    public required string ViceTitle { get; init; }
    public required Action<string?> SetViceTitle { get; init; }
    public required string ViceDesc { get; init; }
    public required Action<string?> SetViceDesc { get; init; }
    public required string Watermark { get; init; }
    public required Action<string?> SetWatermark { get; init; }
    public required string Copyright { get; init; }
    public required Action<string?> SetCopyright { get; init; }
    public required string Icp { get; init; }
    public required Action<string?> SetIcp { get; init; }
    public required string IcpUrl { get; init; }
    public required Action<string?> SetIcpUrl { get; init; }
    public required string DefaultHomePath { get; init; }
    public required Action<string?> SetDefaultHomePath { get; init; }
    public required string LogoInputId { get; init; }
    public required Action SelectLogo { get; init; }
}
