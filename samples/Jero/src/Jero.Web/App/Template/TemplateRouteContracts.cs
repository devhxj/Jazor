using System;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.App.Template;

[ECMAScript("@jero/runtime/components/template-page-state.js")]
public static class TemplatePageFactory
{
    [System.ComponentModel.Description("@#Create")]
    public extern static ITemplatePageState Create();
}

public sealed record TemplatePageProps : VueProps
{
    public required string? Error { get; init; }
    public required string Name { get; init; }
    public required string QueryCode { get; init; }
    public required int QueryType { get; init; }
    public required string GroupName { get; init; }
    public required TemplateTypeOption[] TypeOptions { get; init; }
    public required string[] Groups { get; init; }
    public required TemplateView[] Rows { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanPreview { get; init; }
    public required bool CanDelete { get; init; }
    public required bool Loading { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required Action DismissError { get; init; }
    public required Action<string> SetName { get; init; }
    public required Action<string> SetQueryCode { get; init; }
    public required Action<double> SetQueryType { get; init; }
    public required Action<string> SetGroupName { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action Refresh { get; init; }
    public required Action<string> Edit { get; init; }
    public required Action<string> Preview { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string> Audit { get; init; }
    public required Action<string, string> Sort { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
}

[ECMAScript]
public interface ITemplatePageState
{
    void Initialize();
    TemplatePageProps CreateProps();
    string RichEditorIdValue { get; }
    string[] groups { get; }
    TemplateTypeOption[] typeOptions { get; }
    bool editDialogVisible { get; set; }
    string editDialogTitle { get; }
    bool editReadonly { get; }
    bool saving { get; }
    bool previewing { get; }
    string selectedTab { get; set; }
    string contentMode { get; set; }
    string editName { get; set; }
    string editCode { get; set; }
    int editType { get; set; }
    string editGroupName { get; set; }
    int editOrderNo { get; set; }
    string editRemark { get; set; }
    string editContent { get; }
    string richEditorHtml { get; }
    string previewHtml { get; }
    TemplateRenderEntry[] renderEntries { get; }
    bool auditDialogVisible { get; set; }
    string auditDialogTitle { get; }
    string auditCreateUser { get; }
    string auditCreateTime { get; }
    string auditUpdateUser { get; }
    string auditUpdateTime { get; }
    string auditRemark { get; }
    bool CanPreview { get; }
    void Save();
    void RenderPreview();
    void ContentChanged(string value);
    void ChangeContentMode(string mode);
    void SyncRichContent();
    void RichCommand(string command);
    void RichBlock(string tag);
    void InsertParameter();
    void AddRenderEntry();
    void RemoveRenderEntry(int removeIndex);
}
