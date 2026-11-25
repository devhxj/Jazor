using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.File;

[ECMAScriptModule("components/file-page-state.js")]
public static class FilePageModule
{
    public static FilePageState Create() => new();

    public sealed class FilePageState : IFilePageState
    {
        private FileRef? uploadFile;

        private const string UploadInputId = "system-file-upload-input";

        private FileView[] items = [];
        private FileView[] rows = [];
        private FileFolderView[] folders = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string fileName = string.Empty;
        private string startTime = string.Empty;
        private string endTime = string.Empty;
        private string folderKeyword = string.Empty;
        private string selectedFolderPath = string.Empty;
        private bool loading;
        private bool folderLoading;
        private string? error;
        private bool pageActive;

        public bool uploadDialogVisible { get; set; }
        public bool uploading { get; set; }
        public string uploadFilePath { get; set; } = string.Empty;
        public string uploadFileType { get; set; } = string.Empty;
        public string uploadVisibility { get; set; } = "private";

        private string previewingId = string.Empty;
        public bool previewDialogVisible { get; set; }
        public string previewKind { get; set; } = string.Empty;
        public string previewFileName { get; set; } = string.Empty;
        public string previewUrl { get; set; } = string.Empty;
        public Blob? previewBlob { get; set; }
        public int PreviewVersion { get; private set; }

        private string downloadingId = string.Empty;
        private string deletingId = string.Empty;

        public bool editDialogVisible { get; set; }
        public bool saving { get; set; }
        private FileView? editingFile;
        public string editFileName { get; set; } = string.Empty;
        public string editFileType { get; set; } = string.Empty;
        public string editFileAlias { get; set; } = string.Empty;
        public string editDataId { get; set; } = string.Empty;
        public string editVisibility { get; set; } = "private";

        public bool auditDialogVisible { get; set; }
        public string auditDialogTitle { get; set; } = "修改记录";
        public string auditCreateUser { get; set; } = "-";
        public string auditCreateTime { get; set; } = "-";
        public string auditUpdateUser { get; set; } = "-";
        public string auditUpdateTime { get; set; } = "-";

        private bool CanView => ApplicationState.HasPermission("sysFile:page");
        private bool CanUpload => ApplicationState.HasPermission("sysFile:uploadFile");
        private bool CanDownload => ApplicationState.HasPermission("sysFile:downloadFile");
        private bool CanDelete => ApplicationState.HasPermission("sysFile:delete");
        private bool CanEdit => ApplicationState.HasPermission("sysFile:update");

        public void Initialize()
        {
            pageActive = true;
            OnBeforeUnmount(ReleasePage);
            if (CanView)
                RefreshWorkspace();
        }

        private void ReleasePage()
        {
            pageActive = false;
            ReleasePreview();
        }

        private FilePageInput BuildPageInput()
        {
            var hasDateRange = startTime.Trim().Length > 0 && endTime.Trim().Length > 0;
            return new FilePageInput
            {
                page = currentPage,
                pageSize = pageSize,
                fileName = fileName.Trim(),
                filePath = selectedFolderPath,
                startTime = hasDateRange ? startTime.Trim() : null,
                endTime = hasDateRange ? endTime.Trim() : null
            };
        }

        private void LoadPage()
        {
            if (!CanView || loading)
                return;

            loading = true;
            FileClient.Page(BuildPageInput()).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载文件列表失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }

                items = FileClient.ReadItems(outcome.Data);
                rows = items;
                total = FileClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载文件列表失败，请检查网络后重试。";
            });
        }

        private void LoadFolders()
        {
            if (!CanView || folderLoading)
                return;

            folderLoading = true;
            FileClient.Folders().Then(outcome =>
            {
                folderLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载文件目录失败。";
                    folders = [];
                    return;
                }

                folders = FileClient.ReadFolders(outcome.Data);
                if (selectedFolderPath.Length > 0 && !HasFolder(selectedFolderPath))
                    selectedFolderPath = string.Empty;
                error = null;
            }, _ =>
            {
                folderLoading = false;
                error = "加载文件目录失败，请检查网络后重试。";
            });
        }

        private void RefreshWorkspace()
        {
            LoadFolders();
            LoadPage();
        }

        private void Search()
        {
            var hasStart = startTime.Trim().Length > 0;
            var hasEnd = endTime.Trim().Length > 0;
            if (hasStart != hasEnd)
            {
                error = "开始时间和结束时间需同时填写。";
                return;
            }

            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            fileName = string.Empty;
            startTime = string.Empty;
            endTime = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void SelectFolder(string path)
        {
            if (selectedFolderPath == path)
                return;
            selectedFolderPath = path;
            currentPage = 1;
            LoadPage();
        }

        private void ChangePage(double value)
        {
            currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(double value)
        {
            pageSize = (int)value;
            currentPage = 1;
            LoadPage();
        }

        private void DismissError() => error = null;
        private void SetFolderKeyword(string value) => folderKeyword = value;
        private void SetFileName(string value) => fileName = value;
        private void SetStartTime(string value) => startTime = value;
        private void SetEndTime(string value) => endTime = value;

        private void OpenUpload()
        {
            if (!CanUpload)
                return;
            ResetUpload();
            uploadFilePath = selectedFolderPath;
            uploadDialogVisible = true;
        }

        public void UploadDialogChanged(bool? value)
        {
            uploadDialogVisible = value == true;
            if (!uploadDialogVisible)
                ResetUpload();
        }

        private void ResetUpload()
        {
            uploading = false;
            uploadFilePath = string.Empty;
            uploadFileType = string.Empty;
            uploadVisibility = "private";
            uploadFile = null;
            FileBrowserActions.ClearSelectedFile(UploadInputId);
        }

        public void SelectUploadFile(FileRef? file)
        {
            // Snapshot the native File during the callback, before any async work.
            uploadFile = file;
        }

        public void SubmitUpload()
        {
            if (!CanUpload || uploading)
                return;
            if (uploadFile is null)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择要上传的文件");
                return;
            }

            uploading = true;
            FileBrowserActions.UploadSelectedFile(
                uploadFile,
                NormalizeFolder(uploadFilePath),
                uploadFileType.Trim(),
                uploadVisibility == "public").Then(outcome =>
            {
                uploading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "上传文件失败。";
                    return;
                }

                var uploaded = FileClient.ReadUploadedFile(outcome.Data);
                if (uploaded?.filePath is not null && uploaded.filePath.Length > 0)
                    selectedFolderPath = NormalizeFolder(uploaded.filePath);
                uploadDialogVisible = false;
                ResetUpload();
                currentPage = 1;
                error = null;
                ElementPlus.ElMessageMembers.Success("文件上传成功");
                RefreshWorkspace();
            }, _ =>
            {
                uploading = false;
                error = "上传文件失败，请检查网络后重试。";
            });
        }

        private void OpenPreview(string id)
        {
            var row = Find(id);
            if (row is null || !CanView || previewingId.Length > 0)
                return;

            var kind = PreviewKind(row.suffix);
            if (kind.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("当前文件格式暂不支持在线预览");
                return;
            }

            previewingId = row.id;
            FileBrowserActions.Preview(row.id).Then(outcome =>
            {
                previewingId = string.Empty;
                if (!outcome.Ok || outcome.Blob is null)
                {
                    error = outcome.Error ?? "文件预览失败。";
                    return;
                }
                if (!pageActive)
                    return;

                ReleasePreview();
                previewKind = kind;
                previewFileName = FileDisplayName(row);
                previewBlob = outcome.Blob;
                PreviewVersion++;
                if (kind == "image" || kind == "pdf")
                    previewUrl = FileBrowserActions.CreatePreviewUrl(outcome.Blob);
                previewDialogVisible = true;
                error = null;
            }, _ =>
            {
                previewingId = string.Empty;
                error = "文件预览失败，请检查网络后重试。";
            });
        }

        public void PreviewDialogChanged(bool? value)
        {
            previewDialogVisible = value == true;
            if (!previewDialogVisible)
                ReleasePreview();
        }

        private void ReleasePreview()
        {
            // Each preview owns its object URL until another preview replaces it or the dialog closes.
            FileBrowserActions.RevokePreviewUrl(previewUrl);
            previewDialogVisible = false;
            previewKind = string.Empty;
            previewFileName = string.Empty;
            previewUrl = string.Empty;
            previewBlob = null;
        }

        private static void PreviewRenderFailed()
            => ElementPlus.ElMessageMembers.Error("文件渲染失败，请尝试下载后查看");

        private void Download(string id)
        {
            var row = Find(id);
            if (row is null || !CanDownload || downloadingId.Length > 0)
                return;

            downloadingId = row.id;
            FileBrowserActions.Download(row.id, FileDisplayName(row)).Then(outcome =>
            {
                downloadingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "下载文件失败。";
                    return;
                }
                error = null;
            }, _ =>
            {
                downloadingId = string.Empty;
                error = "下载文件失败，请检查网络后重试。";
            });
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            editingFile = row;
            editFileName = row.fileName ?? string.Empty;
            editFileType = row.fileType ?? string.Empty;
            editFileAlias = row.fileAlias ?? string.Empty;
            editDataId = row.dataId ?? string.Empty;
            editVisibility = row.isPublic ? "public" : "private";
            editDialogVisible = true;
        }

        public void EditDialogChanged(bool? value)
        {
            editDialogVisible = value == true;
            if (!editDialogVisible)
                ResetEdit();
        }

        private void ResetEdit()
        {
            saving = false;
            editingFile = null;
            editFileName = string.Empty;
            editFileType = string.Empty;
            editFileAlias = string.Empty;
            editDataId = string.Empty;
            editVisibility = "private";
        }

        public void SaveEdit()
        {
            var source = editingFile;
            if (source is null || !CanEdit || saving)
                return;

            var normalizedName = editFileName.Trim();
            var normalizedDataId = editDataId.Trim();
            if (normalizedName.Length == 0)
            {
                error = "文件名称不能为空。";
                return;
            }
            if (normalizedDataId.Length > 0 && !new RegExp("^[1-9][0-9]*$").Test(normalizedDataId))
            {
                error = "业务数据 ID 必须为正整数。";
                return;
            }

            saving = true;
            FileClient.Update(BuildUpdateInput(source, normalizedName, normalizedDataId)).Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "更新文件信息失败。";
                    return;
                }

                editDialogVisible = false;
                ResetEdit();
                error = null;
                ElementPlus.ElMessageMembers.Success("文件信息已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "更新文件信息失败，请检查网络后重试。";
            });
        }

        private FileUpdateInput BuildUpdateInput(FileView source, string normalizedName, string normalizedDataId)
            => new()
            {
                // SysFileService.Update performs a full entity update. Keep every non-editable field
                // from the selected row; a partial payload would clear storage/audit metadata.
                id = source.id,
                createTime = source.createTime,
                updateTime = source.updateTime,
                createUserId = source.createUserId,
                createUserName = source.createUserName,
                updateUserId = source.updateUserId,
                updateUserName = source.updateUserName,
                orgId = source.orgId,
                tenantId = source.tenantId,
                provider = source.provider,
                bucketName = source.bucketName,
                fileName = normalizedName,
                suffix = source.suffix,
                filePath = source.filePath,
                sizeKb = source.sizeKb,
                sizeInfo = source.sizeInfo,
                url = source.url,
                fileMd5 = source.fileMd5,
                fileType = Optional(editFileType),
                fileAlias = Optional(editFileAlias),
                isPublic = editVisibility == "public",
                dataId = normalizedDataId.Length == 0 ? null : normalizedDataId
            };

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || deletingId.Length > 0)
                return;

            deletingId = row.id;
            FileClient.Delete(row.id).Then(outcome =>
            {
                deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除文件失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("文件已删除");
                RefreshWorkspace();
            }, _ =>
            {
                deletingId = string.Empty;
                error = "删除文件失败，请检查网络后重试。";
            });
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            auditDialogTitle = "修改记录 · " + FileDisplayName(row);
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = Display(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = Display(row.updateTime);
            auditDialogVisible = true;
        }

        private bool HasFolder(string path)
        {
            for (var index = 0; index < folders.Length; index++)
                if (folders[index].fullPath == path)
                    return true;
            return false;
        }

        private FileView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static string FileDisplayName(FileView row)
            => (row.fileName ?? row.fileAlias ?? "未命名文件") + NormalizeSuffix(row.suffix);

        private static string PreviewKind(string? suffix)
        {
            var value = NormalizeSuffix(suffix);
            if (value == ".jpg" || value == ".jpeg" || value == ".png" ||
                value == ".bmp" || value == ".gif" || value == ".webp")
                return "image";
            if (value == ".docx")
                return "docx";
            if (value == ".xls" || value == ".xlsx")
                return "excel";
            if (value == ".pdf")
                return "pdf";
            return string.Empty;
        }

        private static string NormalizeSuffix(string? suffix)
            => (suffix ?? string.Empty).Trim().ToLowerInvariant();

        private static string NormalizeFolder(string value)
        {
            var normalized = value.Trim().Replace("\\", "/");
            while (normalized.StartsWith('/'))
                normalized = normalized.Substring(1);
            while (normalized.EndsWith('/'))
                normalized = normalized.Substring(0, normalized.Length - 1);
            return normalized;
        }

        private static string Display(string? value)
            => string.IsNullOrEmpty(value) ? "-" : value;

        private static string DisplayUser(string? name, string? id)
        {
            var displayName = name ?? string.Empty;
            var displayId = id ?? string.Empty;
            if (displayName.Length > 0 && displayId.Length > 0)
                return displayName + " (" + displayId + ")";
            return displayName.Length > 0 ? displayName : Display(displayId);
        }

        private static string? Optional(string value)
        {
            var normalized = value.Trim();
            return normalized.Length == 0 ? null : normalized;
        }

        public string UploadInputIdValue => UploadInputId;
        public void PreviewRenderFailedForView() => PreviewRenderFailed();

        public FilePageProps CreateProps() => new()
        {
            Error = error,
            CanView = CanView,
            CanUpload = CanUpload,
            CanDownload = CanDownload,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            Loading = loading,
            FolderLoading = folderLoading,
            FolderKeyword = folderKeyword,
            SelectedFolderPath = selectedFolderPath,
            Folders = folders,
            Rows = rows,
            FileName = fileName,
            StartTime = startTime,
            EndTime = endTime,
            Total = total,
            CurrentPage = currentPage,
            PageSize = pageSize,
            PreviewingId = previewingId,
            DownloadingId = downloadingId,
            DeletingId = deletingId,
            RefreshWorkspace = RefreshWorkspace,
            DismissError = DismissError,
            SetFolderKeyword = SetFolderKeyword,
            SelectFolder = SelectFolder,
            SetFileName = SetFileName,
            SetStartTime = SetStartTime,
            SetEndTime = SetEndTime,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenUpload = OpenUpload,
            RefreshList = LoadPage,
            Preview = OpenPreview,
            Download = Download,
            Edit = OpenEdit,
            Delete = Delete,
            Audit = OpenAudit,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
        };
    }
}

