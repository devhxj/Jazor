using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.File;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.File;

[ECMAScriptModule("components/file-page-state.js")]
public static class FilePageModule
{
    public static FilePageState Create() => new();

    public sealed class FilePageState : IFilePageState
    {
        private FileRef? _uploadFile;

        private const string UploadInputId = "system-file-upload-input";

        private FileView[] _items = [];
        private FileView[] _rows = [];
        private FileFolderView[] _folders = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _fileName = string.Empty;
        private string _startTime = string.Empty;
        private string _endTime = string.Empty;
        private string _folderKeyword = string.Empty;
        private string _selectedFolderPath = string.Empty;
        private bool _loading;
        private bool _folderLoading;
        private string? _error;
        private bool _pageActive;

        public bool UploadDialogVisible { get; set; }
        public bool Uploading { get; set; }
        public string UploadFilePath { get; set; } = string.Empty;
        public string UploadFileType { get; set; } = string.Empty;
        public string UploadVisibility { get; set; } = "private";

        private string _previewingId = string.Empty;
        public bool PreviewDialogVisible { get; set; }
        public string PreviewKind { get; set; } = string.Empty;
        public string PreviewFileName { get; set; } = string.Empty;
        public string PreviewUrl { get; set; } = string.Empty;
        public Blob? PreviewBlob { get; set; }
        public int PreviewVersion { get; private set; }

        private string _downloadingId = string.Empty;
        private string _deletingId = string.Empty;

        public bool EditDialogVisible { get; set; }
        public bool Saving { get; set; }
        private FileView? _editingFile;
        public string EditFileName { get; set; } = string.Empty;
        public string EditFileType { get; set; } = string.Empty;
        public string EditFileAlias { get; set; } = string.Empty;
        public string EditDataId { get; set; } = string.Empty;
        public string EditVisibility { get; set; } = "private";

        public bool AuditDialogVisible { get; set; }
        public string AuditDialogTitle { get; set; } = "修改记录";
        public string AuditCreateUser { get; set; } = "-";
        public string AuditCreateTime { get; set; } = "-";
        public string AuditUpdateUser { get; set; } = "-";
        public string AuditUpdateTime { get; set; } = "-";

        private bool CanView => ApplicationState.HasPermission("sysFile:page");
        private bool CanUpload => ApplicationState.HasPermission("sysFile:uploadFile");
        private bool CanDownload => ApplicationState.HasPermission("sysFile:downloadFile");
        private bool CanDelete => ApplicationState.HasPermission("sysFile:delete");
        private bool CanEdit => ApplicationState.HasPermission("sysFile:update");

        public void Initialize()
        {
            _pageActive = true;
            OnBeforeUnmount(ReleasePage);
            if (CanView)
                RefreshWorkspace();
        }

        private void ReleasePage()
        {
            _pageActive = false;
            ReleasePreview();
        }

        private FilePageInput BuildPageInput()
        {
            var hasDateRange = _startTime.Trim().Length > 0 && _endTime.Trim().Length > 0;
            return new FilePageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                FileName = _fileName.Trim(),
                FilePath = _selectedFolderPath,
                StartTime = hasDateRange ? _startTime.Trim() : null,
                EndTime = hasDateRange ? _endTime.Trim() : null
            };
        }

        private void LoadPage()
        {
            if (!CanView || _loading)
                return;

            _loading = true;
            FileClient.Page(BuildPageInput()).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载文件列表失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }

                _items = FileClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = FileClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载文件列表失败，请检查网络后重试。";
            });
        }

        private void LoadFolders()
        {
            if (!CanView || _folderLoading)
                return;

            _folderLoading = true;
            FileClient.Folders().Then(outcome =>
            {
                _folderLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载文件目录失败。";
                    _folders = [];
                    return;
                }

                _folders = FileClient.ReadFolders(outcome.Data);
                if (_selectedFolderPath.Length > 0 && !HasFolder(_selectedFolderPath))
                    _selectedFolderPath = string.Empty;
                _error = null;
            }, _ =>
            {
                _folderLoading = false;
                _error = "加载文件目录失败，请检查网络后重试。";
            });
        }

        private void RefreshWorkspace()
        {
            LoadFolders();
            LoadPage();
        }

        private void Search()
        {
            var hasStart = _startTime.Trim().Length > 0;
            var hasEnd = _endTime.Trim().Length > 0;
            if (hasStart != hasEnd)
            {
                _error = "开始时间和结束时间需同时填写。";
                return;
            }

            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _fileName = string.Empty;
            _startTime = string.Empty;
            _endTime = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void SelectFolder(string path)
        {
            if (_selectedFolderPath == path)
                return;
            _selectedFolderPath = path;
            _currentPage = 1;
            LoadPage();
        }

        private void ChangePage(double value)
        {
            _currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(double value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadPage();
        }

        private void DismissError() => _error = null;
        private void SetFolderKeyword(string value) => _folderKeyword = value;
        private void SetFileName(string value) => _fileName = value;
        private void SetStartTime(string value) => _startTime = value;
        private void SetEndTime(string value) => _endTime = value;

        private void OpenUpload()
        {
            if (!CanUpload)
                return;
            ResetUpload();
            UploadFilePath = _selectedFolderPath;
            UploadDialogVisible = true;
        }

        public void UploadDialogChanged(bool? value)
        {
            UploadDialogVisible = value == true;
            if (!UploadDialogVisible)
                ResetUpload();
        }

        private void ResetUpload()
        {
            Uploading = false;
            UploadFilePath = string.Empty;
            UploadFileType = string.Empty;
            UploadVisibility = "private";
            _uploadFile = null;
            FileBrowserActions.ClearSelectedFile(UploadInputId);
        }

        public void SelectUploadFile(FileRef? file)
        {
            // 在选择回调中保存 File，异步上传期间用户可能已经切换了选择。
            _uploadFile = file;
        }

        public void SubmitUpload()
        {
            if (!CanUpload || Uploading)
                return;
            if (_uploadFile is null)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择要上传的文件");
                return;
            }

            Uploading = true;
            FileBrowserActions.UploadSelectedFile(
                _uploadFile,
                NormalizeFolder(UploadFilePath),
                UploadFileType.Trim(),
                UploadVisibility == "public").Then(outcome =>
            {
                Uploading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "上传文件失败。";
                    return;
                }

                var uploaded = FileClient.ReadUploadedFile(outcome.Data);
                if (uploaded?.FilePath is not null && uploaded.FilePath.Length > 0)
                    _selectedFolderPath = NormalizeFolder(uploaded.FilePath);
                UploadDialogVisible = false;
                ResetUpload();
                _currentPage = 1;
                _error = null;
                ElementPlus.ElMessageMembers.Success("文件上传成功");
                RefreshWorkspace();
            }, _ =>
            {
                Uploading = false;
                _error = "上传文件失败，请检查网络后重试。";
            });
        }

        private void OpenPreview(string id)
        {
            var row = Find(id);
            if (row is null || !CanView || _previewingId.Length > 0)
                return;

            var kind = ResolvePreviewKind(row.Suffix);
            if (kind.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("当前文件格式暂不支持在线预览");
                return;
            }

            _previewingId = row.Id;
            FileBrowserActions.Preview(row.Id).Then(outcome =>
            {
                _previewingId = string.Empty;
                if (!outcome.Ok || outcome.Blob is null)
                {
                    _error = outcome.Error ?? "文件预览失败。";
                    return;
                }
                if (!_pageActive)
                    return;

                ReleasePreview();
                PreviewKind = kind;
                PreviewFileName = FileDisplayName(row);
                PreviewBlob = outcome.Blob;
                PreviewVersion++;
                if (kind == "image" || kind == "pdf")
                    PreviewUrl = FileBrowserActions.CreatePreviewUrl(outcome.Blob);
                PreviewDialogVisible = true;
                _error = null;
            }, _ =>
            {
                _previewingId = string.Empty;
                _error = "文件预览失败，请检查网络后重试。";
            });
        }

        public void PreviewDialogChanged(bool? value)
        {
            PreviewDialogVisible = value == true;
            if (!PreviewDialogVisible)
                ReleasePreview();
        }

        private void ReleasePreview()
        {
            // 预览切换前先释放上一个文件的 URL，关闭弹窗时也要释放当前 URL。
            FileBrowserActions.RevokePreviewUrl(PreviewUrl);
            PreviewDialogVisible = false;
            PreviewKind = string.Empty;
            PreviewFileName = string.Empty;
            PreviewUrl = string.Empty;
            PreviewBlob = null;
        }

        private static void PreviewRenderFailed()
            => ElementPlus.ElMessageMembers.Error("文件渲染失败，请尝试下载后查看");

        private void Download(string id)
        {
            var row = Find(id);
            if (row is null || !CanDownload || _downloadingId.Length > 0)
                return;

            _downloadingId = row.Id;
            FileBrowserActions.Download(row.Id, FileDisplayName(row)).Then(outcome =>
            {
                _downloadingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "下载文件失败。";
                    return;
                }
                _error = null;
            }, _ =>
            {
                _downloadingId = string.Empty;
                _error = "下载文件失败，请检查网络后重试。";
            });
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _editingFile = row;
            EditFileName = row.FileName ?? string.Empty;
            EditFileType = row.FileType ?? string.Empty;
            EditFileAlias = row.FileAlias ?? string.Empty;
            EditDataId = row.DataId ?? string.Empty;
            EditVisibility = row.IsPublic ? "public" : "private";
            EditDialogVisible = true;
        }

        public void EditDialogChanged(bool? value)
        {
            EditDialogVisible = value == true;
            if (!EditDialogVisible)
                ResetEdit();
        }

        private void ResetEdit()
        {
            Saving = false;
            _editingFile = null;
            EditFileName = string.Empty;
            EditFileType = string.Empty;
            EditFileAlias = string.Empty;
            EditDataId = string.Empty;
            EditVisibility = "private";
        }

        public void SaveEdit()
        {
            var source = _editingFile;
            if (source is null || !CanEdit || Saving)
                return;

            var normalizedName = EditFileName.Trim();
            var normalizedDataId = EditDataId.Trim();
            if (normalizedName.Length == 0)
            {
                _error = "文件名称不能为空。";
                return;
            }
            if (normalizedDataId.Length > 0 && !new RegExp("^[1-9][0-9]*$").Test(normalizedDataId))
            {
                _error = "业务数据 ID 必须为正整数。";
                return;
            }

            Saving = true;
            FileClient.Update(BuildUpdateInput(source, normalizedName, normalizedDataId)).Then(outcome =>
            {
                Saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "更新文件信息失败。";
                    return;
                }

                EditDialogVisible = false;
                ResetEdit();
                _error = null;
                ElementPlus.ElMessageMembers.Success("文件信息已更新");
                LoadPage();
            }, _ =>
            {
                Saving = false;
                _error = "更新文件信息失败，请检查网络后重试。";
            });
        }

        private FileUpdateInput BuildUpdateInput(FileView source, string normalizedName, string normalizedDataId)
            => new()
            {
                // 服务端按完整实体更新，表单以外的存储和审计字段也要从原记录带回。
                Id = source.Id,
                CreateTime = source.CreateTime,
                UpdateTime = source.UpdateTime,
                CreateUserId = source.CreateUserId,
                CreateUserName = source.CreateUserName,
                UpdateUserId = source.UpdateUserId,
                UpdateUserName = source.UpdateUserName,
                OrgId = source.OrgId,
                TenantId = source.TenantId,
                Provider = source.Provider,
                BucketName = source.BucketName,
                FileName = normalizedName,
                Suffix = source.Suffix,
                FilePath = source.FilePath,
                SizeKb = source.SizeKb,
                SizeInfo = source.SizeInfo,
                Url = source.Url,
                FileMd5 = source.FileMd5,
                FileType = Optional(EditFileType),
                FileAlias = Optional(EditFileAlias),
                IsPublic = EditVisibility == "public",
                DataId = normalizedDataId.Length == 0 ? null : normalizedDataId
            };

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || _deletingId.Length > 0)
                return;

            _deletingId = row.Id;
            FileClient.Delete(row.Id).Then(outcome =>
            {
                _deletingId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除文件失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("文件已删除");
                RefreshWorkspace();
            }, _ =>
            {
                _deletingId = string.Empty;
                _error = "删除文件失败，请检查网络后重试。";
            });
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            AuditDialogTitle = "修改记录 · " + FileDisplayName(row);
            AuditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            AuditCreateTime = Display(row.CreateTime);
            AuditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            AuditUpdateTime = Display(row.UpdateTime);
            AuditDialogVisible = true;
        }

        private bool HasFolder(string path)
        {
            for (var index = 0; index < _folders.Length; index++)
                if (_folders[index].FullPath == path)
                    return true;
            return false;
        }

        private FileView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static string FileDisplayName(FileView row)
            => (row.FileName ?? row.FileAlias ?? "未命名文件") + NormalizeSuffix(row.Suffix);

        private static string ResolvePreviewKind(string? suffix)
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
            Error = _error,
            CanView = CanView,
            CanUpload = CanUpload,
            CanDownload = CanDownload,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            Loading = _loading,
            FolderLoading = _folderLoading,
            FolderKeyword = _folderKeyword,
            SelectedFolderPath = _selectedFolderPath,
            Folders = _folders,
            Rows = _rows,
            FileName = _fileName,
            StartTime = _startTime,
            EndTime = _endTime,
            Total = _total,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            PreviewingId = _previewingId,
            DownloadingId = _downloadingId,
            DeletingId = _deletingId,
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
