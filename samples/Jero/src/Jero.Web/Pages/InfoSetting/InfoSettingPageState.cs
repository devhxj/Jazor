using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.InfoSetting;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;

namespace Jero.Web.Pages.InfoSetting;

[ECMAScriptModule("components/info-setting-page-state.js")]
public static class InfoSettingPageModule
{
    public static InfoSettingPageState Create() => new();

    public sealed class InfoSettingPageState : global::Jero.Web.Composition.IPageState<InfoSettingPageProps>
    {
        private FileRef? _logoFile;

        private const string LogoInputId = "info-setting-logo-input";
        private const string DefaultHomePath = "/dashboard/home";
        private const string EmptyImageDataUrl = "data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=";

        private InfoSettingWayOption[] _wayOptions = [];
        private string _storedLogo = string.Empty;
        private string _previewLogo = string.Empty;
        private string _selectedLogoFileName = string.Empty;
        private string _title = string.Empty;
        private string _viceTitle = string.Empty;
        private string _viceDesc = string.Empty;
        private string _watermark = string.Empty;
        private string _copyright = string.Empty;
        private string _icp = string.Empty;
        private string _icpUrl = string.Empty;
        private string _defaultHomePath = DefaultHomePath;
        private string _regWayId = string.Empty;
        private int _enableReg = 2;
        private int _secondVer = 2;
        private int _captcha = 2;
        private bool _hideTenantForLogin;
        private bool _loading;
        private bool _readingLogo;
        private bool _saving;
        private string? _error;

        private bool RegistrationEnabled => _enableReg == 1;
        private string CurrentLogo => _previewLogo.Length > 0 ? _previewLogo : NormalizeImageUrl(_storedLogo);

        public void Initialize() => Load();

        private void Load()
        {
            _loading = true;
            _error = null;
            InfoSettingClient.GetSysInfo().Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载系统配置失败。";
                    return;
                }

                Apply(InfoSettingClient.ReadSysInfo(outcome.Data));
            }, _ =>
            {
                _loading = false;
                _error = "加载系统配置失败，请检查网络后重试。";
            });
        }

        private void Apply(InfoSettingView value)
        {
            _storedLogo = value.Logo;
            _previewLogo = string.Empty;
            _selectedLogoFileName = string.Empty;
            _title = value.Title;
            _viceTitle = value.ViceTitle;
            _viceDesc = value.ViceDesc;
            _watermark = value.Watermark;
            _copyright = value.Copyright;
            _icp = value.Icp;
            _icpUrl = value.IcpUrl;
            _defaultHomePath = NormalizeLoadedHomePath(value.DefaultHomePath);
            _enableReg = value.EnableReg == 1 ? 1 : 2;
            _regWayId = value.RegWayId;
            _secondVer = value.SecondVer == 1 ? 1 : 2;
            _captcha = value.Captcha == 1 ? 1 : 2;
            _hideTenantForLogin = value.HideTenantForLogin;
            _wayOptions = value.WayList;
            ApplicationState.UpdateRuntimeConfig(
                value.Title,
                NormalizeImageUrl(value.Logo),
                value.Watermark,
                NormalizeLoadedHomePath(value.DefaultHomePath));
            _logoFile = null;
            InfoSettingBrowserActions.ClearSelectedLogo(LogoInputId);
            _error = null;
        }

        private void SelectLogo()
        {
            // 在事件回调中取出原生 File，再开始异步操作。
            _logoFile = BrowserFileSelection.Snapshot(LogoInputId);
        }

        private void PreviewSelectedLogo()
        {
            if (_readingLogo || _saving)
                return;

            _readingLogo = true;
            InfoSettingBrowserActions.ReadSelectedLogo(_logoFile).Then(selection =>
            {
                _readingLogo = false;
                if (!selection.Ok)
                {
                    _error = selection.Error ?? "读取 Logo 文件失败。";
                    return;
                }
                if (!selection.HasFile)
                {
                    _error = "请先选择 Logo 图片。";
                    return;
                }

                _previewLogo = selection.DataUrl;
                _selectedLogoFileName = selection.FileName;
                _error = null;
            }, _ =>
            {
                _readingLogo = false;
                _error = "读取 Logo 文件失败。";
            });
        }

        private void RestoreStoredLogo()
        {
            _previewLogo = string.Empty;
            _selectedLogoFileName = string.Empty;
            _logoFile = null;
            InfoSettingBrowserActions.ClearSelectedLogo(LogoInputId);
        }

        private void EnableRegistrationChanged(double? value)
        {
            _enableReg = (int)(value ?? 2);
            if (!RegistrationEnabled)
                _regWayId = string.Empty;
        }

        private void RegistrationWayChanged(string? value)
            => _regWayId = value ?? string.Empty;

        private void Save()
        {
            if (_saving || _loading || _readingLogo || !Validate())
                return;

            _saving = true;
            // 保存使用 onchange 回调中取得的文件，即使用户没有预览。
            InfoSettingBrowserActions.ReadSelectedLogo(_logoFile).Then(selection =>
            {
                if (!selection.Ok)
                {
                    _saving = false;
                    _error = selection.Error ?? "读取 Logo 文件失败。";
                    return;
                }

                var input = BuildSaveInput(selection);
                InfoSettingClient.SaveSysInfo(input).Then(outcome =>
                {
                    _saving = false;
                    if (!outcome.Ok)
                    {
                        _error = outcome.Error ?? "保存系统配置失败。";
                        return;
                    }

                    _error = null;
                    ElementPlus.ElMessageMembers.Success("系统配置已保存");
                    Load();
                }, _ =>
                {
                    _saving = false;
                    _error = "保存系统配置失败，请检查网络后重试。";
                });
            }, _ =>
            {
                _saving = false;
                _error = "读取 Logo 文件失败。";
            });
        }

        private InfoSettingSaveInput BuildSaveInput(InfoSettingLogoSelection selection)
            => new()
            {
                LogoBase64 = selection.HasFile ? selection.DataUrl : string.Empty,
                LogoFileName = selection.HasFile ? selection.FileName : string.Empty,
                Watermark = _watermark.Trim(),
                Title = _title.Trim(),
                ViceTitle = _viceTitle.Trim(),
                ViceDesc = _viceDesc.Trim(),
                Copyright = _copyright.Trim(),
                Icp = _icp.Trim(),
                IcpUrl = _icpUrl.Trim(),
                EnableReg = RegistrationEnabled ? 1 : 2,
                // 这两个值仅透传读取快照；后端当前未开放保存逻辑，页面也不提供编辑入口。
                SecondVer = _secondVer,
                Captcha = _captcha,
                RegWayId = RegistrationEnabled ? _regWayId : "0",
                DefaultHomePath = NormalizeHomePath(_defaultHomePath)
            };

        private bool Validate()
        {
            var normalizedTitle = _title.Trim();
            var normalizedViceTitle = _viceTitle.Trim();
            var normalizedViceDesc = _viceDesc.Trim();
            var normalizedCopyright = _copyright.Trim();
            var normalizedIcp = _icp.Trim();
            var normalizedIcpUrl = _icpUrl.Trim();
            var normalizedWatermark = _watermark.Trim();
            var normalizedHomePath = _defaultHomePath.Trim();

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
            if (RegistrationEnabled && _regWayId.Length == 0)
                return Fail("启用注册后必须选择默认注册方案。");

            _error = null;
            return true;
        }

        private bool Fail(string message)
        {
            _error = message;
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
            // 种子数据中的默认路径与打包后的资源路径不同，只转换这条已知的默认路径。
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
            var options = new ConfigurationOption[_wayOptions.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(_wayOptions[index].Value, _wayOptions[index].Label);
            return options;
        }

        public InfoSettingPageProps CreateProps() => new()
        {
            LogoSource = ImageSource(CurrentLogo),
            CurrentLogo = CurrentLogo,
            DefaultHomePlaceholder = DefaultHomePath,
            SelectedLogoFileName = _selectedLogoFileName,
            RegWayId = _regWayId,
            EnableReg = _enableReg,
            WayOptions = RegistrationOptions(),
            RegistrationEnabled = RegistrationEnabled,
            HideTenantForLogin = _hideTenantForLogin,
            Loading = _loading,
            Saving = _saving,
            ReadingLogo = _readingLogo,
            Error = _error,
            Save = Save,
            PreviewSelectedLogo = PreviewSelectedLogo,
            RestoreStoredLogo = RestoreStoredLogo,
            DismissError = () => _error = null,
            SetEnableReg = value => EnableRegistrationChanged(value),
            SetRegWayId = RegistrationWayChanged,
            Title = _title,
            SetTitle = value => _title = value ?? "",
            ViceTitle = _viceTitle,
            SetViceTitle = value => _viceTitle = value ?? "",
            ViceDesc = _viceDesc,
            SetViceDesc = value => _viceDesc = value ?? "",
            Watermark = _watermark,
            SetWatermark = value => _watermark = value ?? "",
            Copyright = _copyright,
            SetCopyright = value => _copyright = value ?? "",
            Icp = _icp,
            SetIcp = value => _icp = value ?? "",
            IcpUrl = _icpUrl,
            SetIcpUrl = value => _icpUrl = value ?? "",
            DefaultHomePath = _defaultHomePath,
            SetDefaultHomePath = value => _defaultHomePath = value ?? "",
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
