using JazorAdmin.Features.Settings;

namespace JazorAdmin;

[ECMAScriptModule("components/settings")]
public partial class SettingsPage : AppComponentBase, IVueContainerComponent
{
    private bool loading = true;
    private string? error;
    private SettingView[] settings = [];
    private string? selectedKey;
    private string key = string.Empty;
    private string group = "general";
    private string label = string.Empty;
    private string description = string.Empty;
    private string kind = "text";
    private string value = string.Empty;
    private bool deleteArmed;

    private bool IsNew => selectedKey is null;

    protected override void OnInitialized() => Load();

    private void Load()
    {
        loading = true;
        error = null;
        ApiClient.GetSettings().Then(ApplySettings);
    }

    private void ApplySettings(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load system settings.", "无法加载系统配置。");
            return;
        }

        settings = ApiClient.ToSettings(outcome.Data);
        if (selectedKey is not null)
        {
            var selected = settings.FirstOrDefault(item => item.Key == selectedKey);
            if (selected is not null)
            {
                Select(selected);
                return;
            }
        }
        if (settings.Length > 0)
            Select(settings[0]);
    }

    private void Select(SettingView setting)
    {
        selectedKey = setting.Key;
        key = setting.Key;
        group = setting.Group;
        label = setting.Label;
        description = setting.Description ?? string.Empty;
        kind = setting.Kind;
        value = setting.Value;
        deleteArmed = false;
    }

    private void NewSetting()
    {
        selectedKey = null;
        key = string.Empty;
        group = "general";
        label = string.Empty;
        description = string.Empty;
        kind = "text";
        value = string.Empty;
        deleteArmed = false;
        error = null;
    }

    private void Save()
    {
        if (selectedKey is null)
        {
            ApiClient.CreateSetting(new SettingCreate(key, group, label, description, kind, value)).Then(ApplySaved);
        }
        else
        {
            ApiClient.UpdateSetting(selectedKey, new SettingUpdate(group, label, description, kind, value)).Then(ApplySaved);
        }
    }

    private void ApplySaved(ApiOutcome outcome)
    {
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to save the system setting.", "无法保存系统配置。");
            return;
        }
        Load();
    }

    private void DeleteSetting()
    {
        if (selectedKey is null)
            return;
        if (!deleteArmed)
        {
            deleteArmed = true;
            return;
        }

        ApiClient.DeleteSetting(selectedKey).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to delete the system setting.", "无法删除系统配置。");
                return;
            }
            NewSetting();
            Load();
        });
    }
}
