using ECMAScript.TDesign;
using JazorAdmin.Features.Audit;
using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("components/audit")]
public partial class AuditPage : AppComponentBase, IVueContainerComponent
{
    private sealed record AuditFilterDraft
    {
        public string From { get; set; } = string.Empty;

        public string To { get; set; } = string.Empty;

        public string Actor { get; set; } = string.Empty;

        public string Target { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;
    }

    private bool loading = true;
    private string? error;
    private AuditEventView[] events = [];
    private AuditFilterDraft Filters { get; set; } = NewFilters();

    private TPrimaryTableCol<AuditEventView>[] Columns =>
    [
        new() { Title = (TPrimaryTableColTitle<AuditEventView>)L("Time", "时间"), Cell = (TPrimaryTableColCell<AuditEventView>)((RenderFragment<TPrimaryTableCellParams<AuditEventView>>)(context => builder =>
            {
        builder.OpenElement(0, "time");
        builder.AddAttribute(1, "data-audit-event", context.Row.Id);
        builder.AddAttribute(2, "datetime", context.Row.OccurredAt);
        builder.AddContent(3, FormatTime(context.Row.OccurredAt));
        builder.CloseElement();
            })) },
        new() { Title = (TPrimaryTableColTitle<AuditEventView>)L("Actor", "操作者"), Cell = (TPrimaryTableColCell<AuditEventView>)((RenderFragment<TPrimaryTableCellParams<AuditEventView>>)(context => builder =>
            {
        builder.AddContent(0, context.Row.ActorName ?? context.Row.ActorId ?? "-");
            })) },
        new() { Title = (TPrimaryTableColTitle<AuditEventView>)L("Action", "操作"), Cell = (TPrimaryTableColCell<AuditEventView>)((RenderFragment<TPrimaryTableCellParams<AuditEventView>>)(context => builder =>
            {
        builder.OpenComponent<TTag>(0);
        builder.AddComponentParameter(1, nameof(TTag.Theme), ActionTheme(context.Row.Action));
        builder.AddComponentParameter(2, nameof(TContentComponentBase.ChildContent),
            (RenderFragment)(child => child.AddContent(0, context.Row.Action)));
        builder.CloseComponent();
            })) },
        new() { Title = (TPrimaryTableColTitle<AuditEventView>)L("Object", "对象"), Cell = (TPrimaryTableColCell<AuditEventView>)((RenderFragment<TPrimaryTableCellParams<AuditEventView>>)(context => builder =>
            {
        builder.OpenElement(0, "div");
        builder.OpenElement(1, "strong");
        builder.AddContent(2, context.Row.ObjectType);
        builder.CloseElement();
        builder.OpenElement(3, "small");
        builder.AddContent(4, context.Row.Summary ?? context.Row.ObjectId);
        builder.CloseElement();
        builder.CloseElement();
            })) }
    ];

    protected override void OnInitialized() => Load();

    private void Load()
    {
        loading = true;
        error = null;
        ApiClient.GetAudit(Filters.From, Filters.To, Filters.Actor, Filters.Target, Filters.Action).Then(Apply);
    }

    private void ApplyFilters(TSubmitContext<AuditFilterDraft> context)
        => Load();

    private void Apply(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load audit events.", "无法加载审计日志。");
            return;
        }

        events = ApiClient.ToAuditEvents(outcome.Data);
    }

    private void ResetFilters(TFormResetEventContext<AuditFilterDraft> context)
    {
        Filters = NewFilters();
        error = null;
        Load();
    }

    private static TTagThemeValue ActionTheme(string value)
        => value switch
        {
            "created" or "issued" or "granted" => TTagThemeValue.Success,
            "updated" => TTagThemeValue.Primary,
            "revoked" or "deleted" => TTagThemeValue.Danger,
            _ => TTagThemeValue.Default
        };

    // UTC is part of the audit contract. The table only trims the ISO payload for scanning and
    // keeps the full timestamp in datetime for copy/paste and browser tooling.
    // UTC 是审计契约的一部分；表格只为扫读截断 ISO 值，完整值仍保留在 datetime 属性。
    private static string FormatTime(string value)
        => value.Length <= 19 ? value : value.Substring(0, 19).Replace("T", " ") + " UTC";

    private static AuditFilterDraft NewFilters() => new()
    {
        From = string.Empty,
        To = string.Empty,
        Actor = string.Empty,
        Target = string.Empty,
        Action = string.Empty
    };
}
