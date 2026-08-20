using ECMAScript.TDesign;
using JazorAdmin.Features.Settings;
using Microsoft.AspNetCore.Components;

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
    private int loadVersion;

    private bool IsNew => selectedKey is null;

    // TDesign 表格列在 C# 侧组装：标题走 string 分支，组合单元格走 Cell 渲染片段，
    // 行数据经 TPrimaryTableCellParams.Row 以 C# 成员访问，避免依赖 JS 侧键名。
    private TPrimaryTableCol<SettingView>[] Columns =>
    [
        new()
        {
            Title = (TPrimaryTableColTitle<SettingView>)L("Setting", "配置项"),
            Cell = (TPrimaryTableColCell<SettingView>)((RenderFragment<TPrimaryTableCellParams<SettingView>>)(context => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "data-setting-key", context.Row.Key);
                builder.OpenElement(2, "strong");
                builder.AddContent(3, context.Row.Label);
                builder.CloseElement();
                builder.OpenElement(4, "span");
                builder.AddContent(5, context.Row.Key);
                builder.CloseElement();
                builder.CloseElement();
            }))
        },
        new() { ColKey = "Group", Title = (TPrimaryTableColTitle<SettingView>)L("Group", "分组") },
        new()
        {
            Title = (TPrimaryTableColTitle<SettingView>)L("Type", "类型"),
            Cell = (TPrimaryTableColCell<SettingView>)((RenderFragment<TPrimaryTableCellParams<SettingView>>)(context => builder =>
            {
                builder.OpenElement(0, "code");
                builder.AddContent(1, context.Row.Kind);
                builder.CloseElement();
            }))
        },
        new() { Title = (TPrimaryTableColTitle<SettingView>)L("Actions", "操作"), Cell = (TPrimaryTableColCell<SettingView>)((RenderFragment<TPrimaryTableCellParams<SettingView>>)(context => builder =>
        {
            builder.OpenComponent<TButton>(0);
            builder.AddComponentParameter(1, nameof(TButton.Variant), TButtonVariantValue.Text);
            builder.AddComponentParameter(2, nameof(TButton.Size), TSizeEnum.Small);
            builder.AddComponentParameter(3, "data-setting-command", "edit");
            builder.AddComponentParameter(4, nameof(TButton.OnClick),
                EventCallback.Factory.Create(this, () => Select(context.Row)));
            builder.AddComponentParameter(5, nameof(TContentComponentBase.ChildContent),
                (RenderFragment)(child => child.AddContent(0, L("Edit", "编辑"))));
            builder.CloseComponent();
        })) }
    ];

    private TTableRowClassNameValue<SettingView> SelectedRowClassName
        => (TTableRowClassNameValueOption2<SettingView>)SelectedRowClass;

    private TClassName SelectedRowClass(TRowClassNameParams<SettingView> parameters)
        => parameters.Row.Key == selectedKey ? (TClassName)"ja-table-row-selected" : (TClassName)string.Empty;

    protected override void OnInitialized() => Load();

    private void Load()
    {
        var requestVersion = ++loadVersion;
        loading = true;
        error = null;
        ApiClient.GetSettings().Then(outcome =>
        {
            // Create/delete can issue a newer refresh before an older response resolves.
            // 忽略过期响应，避免旧配置快照在写操作后覆盖最新表格状态。
            if (requestVersion != loadVersion)
                return;

            ApplySettings(outcome);
        });
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
        // Opening a new editor is explicit user intent, so an in-flight load must not reselect
        // an older row after the form has been reset.
        // 用户明确切换到新建态时，失效旧请求，避免其回写并重新选中旧配置。
        loadVersion++;
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
