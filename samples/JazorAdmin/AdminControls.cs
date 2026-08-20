using ECMAScript;
using ECMAScript.TDesign;
using JazorAdmin.Features.Accounts;
using JazorAdmin.Features.Audit;
using JazorAdmin.Features.Identity;
using JazorAdmin.Features.Organizations;
using JazorAdmin.Features.Scheduling;
using JazorAdmin.Features.Settings;
using JazorAdmin.Features.Sso;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace JazorAdmin;

// Razor 标记无法在生成的泛型与非泛型 TDesign 绑定之间做重载决策；
// 桥接组件在 C# 侧固定泛型参数、转发未匹配属性，并保留官方组件行为。
// Bridges pin TDesign binding generics in C# and forward unmatched attributes.

[ECMAScriptModule("./components/admin-table")]
public class AdminTable<T> : ComponentBase, IVueComponent
{
    [Parameter]
    public T[]? Data { get; set; }

    [Parameter]
    public TPrimaryTableCol<T>[]? Columns { get; set; }

    [Parameter]
    public string RowKey { get; set; } = "Key";

    [Parameter]
    public bool? Loading { get; set; }

    [Parameter]
    public string? Empty { get; set; }

    [Parameter]
    public bool? Bordered { get; set; }

    [Parameter]
    public bool? Hover { get; set; }

    [Parameter]
    public TTableRowClassNameValue<T>? RowClassName { get; set; }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TTable<T>>(0);
        builder.AddComponentParameter(1, nameof(Data), Data);
        builder.AddComponentParameter(2, nameof(Columns), Columns);
        builder.AddComponentParameter(3, nameof(RowKey), RowKey);
        builder.AddComponentParameter(4, nameof(TTable<T>.LoadingValue), Loading);
        builder.AddComponentParameter(5, nameof(TTable<T>.EmptyValue), Empty);
        builder.AddComponentParameter(6, nameof(Bordered), Bordered);
        builder.AddComponentParameter(7, nameof(Hover), Hover);
        builder.AddComponentParameter(8, nameof(RowClassName), RowClassName);
        builder.CloseComponent();
    }
}

[ECMAScriptModule("./components/admin-input")]
public sealed class AdminInput : ComponentBase, IVueComponent
{
    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public bool? Readonly { get; set; }

    [Parameter]
    public bool? Disabled { get; set; }

    [Parameter]
    public bool? Clearable { get; set; }

    [Parameter]
    public TInputTypeValue? Type { get; set; }

    [Parameter]
    public EventCallback<string> OnChange { get; set; }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TInput<string>>(0);
        builder.AddComponentParameter(1, nameof(Value), Value);
        builder.AddComponentParameter(2, nameof(Placeholder), Placeholder);
        builder.AddComponentParameter(3, nameof(Readonly), Readonly);
        builder.AddComponentParameter(4, nameof(Disabled), Disabled);
        builder.AddComponentParameter(5, nameof(Clearable), Clearable);
        builder.AddComponentParameter(6, nameof(Type), Type);
        builder.AddComponentParameter(7, nameof(TInput<string>.OnChange),
            EventCallback.Factory.Create<string>(this, HandleChangeAsync));
        builder.CloseComponent();
    }

    private Task HandleChangeAsync(string value) => OnChange.InvokeAsync(value);
}

[ECMAScriptModule("./components/admin-textarea")]
public sealed class AdminTextarea : ComponentBase, IVueComponent
{
    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public bool? Readonly { get; set; }

    [Parameter]
    public EventCallback<string> OnChange { get; set; }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TTextarea>(0);
        builder.AddComponentParameter(1, nameof(TTextarea.Value), (TTextareaValue)Value);
        builder.AddComponentParameter(2, nameof(Placeholder), Placeholder);
        builder.AddComponentParameter(3, nameof(TTextarea.Readonly), Readonly);
        builder.AddComponentParameter(4, nameof(TTextarea.OnChange),
            EventCallback.Factory.Create<TTextareaValue>(this, HandleChangeAsync));
        builder.CloseComponent();
    }

    private Task HandleChangeAsync(TTextareaValue value)
        => OnChange.InvokeAsync(value.Value as string ?? string.Empty);
}

[ECMAScriptModule("./components/admin-form")]
public sealed class AdminForm : ComponentBase, IVueComponent
{
    [Parameter]
    public TFormLabelAlignValue? LabelAlign { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TForm<TJsonObject>>(0);
        builder.AddComponentParameter(1, nameof(TForm<TJsonObject>.LabelAlign), LabelAlign);
        builder.AddComponentParameter(2, nameof(TContentComponentBase.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}

[ECMAScriptModule("./components/admin-radio-group")]
public sealed class AdminRadioGroup : ComponentBase, IVueComponent
{
    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public TSizeEnum? Size { get; set; }

    [Parameter]
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    public EventCallback<string> OnChange { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TRadioGroup<string>>(0);
        builder.AddComponentParameter(1, nameof(TRadioGroup<string>.Value), Value);
        builder.AddComponentParameter(2, nameof(TRadioGroup<string>.Size), Size);
        builder.AddComponentParameter(3, nameof(TContentComponentBase.CssClass), CssClass);
        builder.AddComponentParameter(4, nameof(TRadioGroup<string>.OnChange),
            EventCallback.Factory.Create<string>(this, HandleChangeAsync));
        builder.AddComponentParameter(5, nameof(TContentComponentBase.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    private Task HandleChangeAsync(string value) => OnChange.InvokeAsync(value);
}

[ECMAScriptModule("./components/admin-radio-button")]
public sealed class AdminRadioButton : ComponentBase, IVueComponent
{
    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public string? LabelValue { get; set; }

    [Parameter]
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TRadioButton<string>>(0);
        builder.AddComponentParameter(1, nameof(TRadioButton<string>.Value), Value);
        builder.AddComponentParameter(2, nameof(TRadioButton<string>.LabelValue), LabelValue);
        builder.AddComponentParameter(3, nameof(TContentComponentBase.CssClass), CssClass);
        builder.AddComponentParameter(4, nameof(TContentComponentBase.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}

[ECMAScriptModule("./components/admin-toggle")]
public sealed class AdminToggle : ComponentBase, IVueComponent
{
    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public EventCallback<bool> OnChange { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<TSwitch<bool>>(0);
        builder.AddComponentParameter(1, nameof(TSwitch<bool>.Value), Value);
        builder.AddComponentParameter(2, nameof(TSwitch<bool>.OnChange),
            EventCallback.Factory.Create<bool>(this, HandleChangeAsync));
        builder.CloseComponent();
    }

    private Task HandleChangeAsync(bool value) => OnChange.InvokeAsync(value);
}

// Generic TypeInference helpers and authored generic BuildRenderTree methods are lowered by
// RazorVue after their builder symbols are normalized to the original method definition. These
// non-generic wrappers remain useful when a page wants a stable business-specific component
// name/module, but they are no longer required as a compiler workaround.
[ECMAScriptModule("./components/admin-table-setting")]
public sealed class AdminSettingTable : AdminTable<SettingView>;

[ECMAScriptModule("./components/admin-table-account")]
public sealed class AdminAccountTable : AdminTable<AccountResponse>;

[ECMAScriptModule("./components/admin-table-scope")]
public sealed class AdminScopeTable : AdminTable<ScopeView>;

[ECMAScriptModule("./components/admin-table-authorization")]
public sealed class AdminAuthorizationTable : AdminTable<AuthorizationView>;

[ECMAScriptModule("./components/admin-table-token")]
public sealed class AdminTokenTable : AdminTable<TokenView>;

[ECMAScriptModule("./components/admin-table-schedule")]
public sealed class AdminScheduleTable : AdminTable<ScheduleView>;

[ECMAScriptModule("./components/admin-table-schedule-run")]
public sealed class AdminScheduleRunTable : AdminTable<ScheduleRunView>;

[ECMAScriptModule("./components/admin-table-app")]
public sealed class AdminAppTable : AdminTable<AppView>;

[ECMAScriptModule("./components/admin-table-member")]
public sealed class AdminMemberTable : AdminTable<OrganizationMemberResponse>;

[ECMAScriptModule("./components/admin-table-resource-operation")]
public sealed class AdminResourceOperationTable : AdminTable<ResourceOperationResponse>;

[ECMAScriptModule("./components/admin-table-organization-summary")]
public sealed class AdminOrganizationSummaryTable : AdminTable<OrganizationSummary>;

[ECMAScriptModule("./components/admin-table-dashboard-role")]
public sealed class AdminDashboardRoleTable : AdminTable<DashboardRoleCell>;

[ECMAScriptModule("./components/admin-table-audit")]
public sealed class AdminAuditTable : AdminTable<AuditEventView>;
