using System;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.App.File;

[ECMAScript("@jero/runtime/components/file-page-state.js")]
public static class FilePageFactory
{
    [System.ComponentModel.Description("@#Create")]
    public extern static IFilePageState Create();
}

public sealed record FilePageProps : VueProps
{
    public required string? Error { get; init; }
    public required bool CanView { get; init; }
    public required bool CanUpload { get; init; }
    public required bool CanDownload { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool Loading { get; init; }
    public required bool FolderLoading { get; init; }
    public required string FolderKeyword { get; init; }
    public required string SelectedFolderPath { get; init; }
    public required FileFolderView[] Folders { get; init; }
    public required FileView[] Rows { get; init; }
    public required string FileName { get; init; }
    public required string StartTime { get; init; }
    public required string EndTime { get; init; }
    public required int Total { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required string PreviewingId { get; init; }
    public required string DownloadingId { get; init; }
    public required string DeletingId { get; init; }
    public required Action RefreshWorkspace { get; init; }
    public required Action DismissError { get; init; }
    public required Action<string> SetFolderKeyword { get; init; }
    public required Action<string> SelectFolder { get; init; }
    public required Action<string> SetFileName { get; init; }
    public required Action<string> SetStartTime { get; init; }
    public required Action<string> SetEndTime { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenUpload { get; init; }
    public required Action RefreshList { get; init; }
    public required Action<string> Preview { get; init; }
    public required Action<string> Download { get; init; }
    public required Action<string> Edit { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string> Audit { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
}

[ECMAScript("/File/office-loaders.mjs")]
public static class FileOfficeLoaders
{
    [System.ComponentModel.Description("@#LoadDocx")]
    public extern static IPromise<IVueComponent> LoadDocx();
    [System.ComponentModel.Description("@#LoadExcel")]
    public extern static IPromise<IVueComponent> LoadExcel();
}

public sealed record FileOfficeProps(Blob src, Action onError) : VueProps;

[ECMAScript]
public interface IFilePageState
{
    void Initialize();
    FilePageProps CreateProps();
    string UploadInputIdValue { get; }
    bool uploadDialogVisible { get; }
    bool uploading { get; }
    string uploadFilePath { get; set; }
    string uploadFileType { get; set; }
    string uploadVisibility { get; set; }
    bool previewDialogVisible { get; }
    string previewKind { get; set; }
    string previewFileName { get; }
    string previewUrl { get; }
    Blob? previewBlob { get; }
    int PreviewVersion { get; }
    bool editDialogVisible { get; }
    bool saving { get; }
    string editFileName { get; set; }
    string editFileType { get; set; }
    string editFileAlias { get; set; }
    string editDataId { get; set; }
    string editVisibility { get; set; }
    bool auditDialogVisible { get; set; }
    string auditDialogTitle { get; }
    string auditCreateUser { get; }
    string auditCreateTime { get; }
    string auditUpdateUser { get; }
    string auditUpdateTime { get; }
    void UploadDialogChanged(bool? value);
    void SelectUploadFile(FileRef? file);
    void SubmitUpload();
    void PreviewDialogChanged(bool? value);
    void PreviewRenderFailedForView();
    void EditDialogChanged(bool? value);
    void SaveEdit();
}
