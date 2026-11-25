using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Template;

[ECMAScriptModule("components/template-page-state.js")]
public static class TemplatePageModule
{
    public static TemplatePageState Create() => new();

    public sealed class TemplatePageState : ITemplatePageState
    {
        private const string RichEditorId = "jero-template-rich-editor";

        private TemplateView[] items = [];
        private TemplateView[] rows = [];
        public string[] groups { get; set; } = [];
        public TemplateTypeOption[] typeOptions { get; set; } = DefaultTypeOptions();
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string name = string.Empty;
        private string queryCode = string.Empty;
        private int queryType;
        private string groupName = string.Empty;
        private string field = "orderNo";
        private string order = "ascending";
        private bool loading;
        private string? error;

        public bool editDialogVisible { get; set; }
        public string editDialogTitle { get; set; } = "新增模板";
        public bool editReadonly { get; set; }
        public bool saving { get; set; }
        public bool previewing { get; set; }
        public string selectedTab { get; set; } = "basic";
        public string contentMode { get; set; } = "rich";
        private string editId = string.Empty;
        public string editName { get; set; } = string.Empty;
        public string editCode { get; set; } = string.Empty;
        public int editType { get; set; } = 1;
        public string editGroupName { get; set; } = string.Empty;
        public int editOrderNo { get; set; } = 100;
        public string editRemark { get; set; } = string.Empty;
        public string editContent { get; set; } = string.Empty;
        public string richEditorHtml { get; set; } = string.Empty;
        public string previewHtml { get; set; } = string.Empty;
        public TemplateRenderEntry[] renderEntries { get; set; } = [];

        public bool auditDialogVisible { get; set; }
        public string auditDialogTitle { get; set; } = "修改记录";
        public string auditCreateUser { get; set; } = "-";
        public string auditCreateTime { get; set; } = "-";
        public string auditUpdateUser { get; set; } = "-";
        public string auditUpdateTime { get; set; } = "-";
        public string auditRemark { get; set; } = "-";

        private bool CanAdd => ApplicationState.HasPermission("sysTemplate:add");
        private bool CanEdit => ApplicationState.HasPermission("sysTemplate:update");
        private bool CanDelete => ApplicationState.HasPermission("sysTemplate:delete");
        public bool CanPreview => ApplicationState.HasPermission("sysTemplate:preview");

        public void Initialize()
        {
            LoadGroups();
            LoadTypes();
            LoadPage();
        }

        private void LoadPage()
        {
            loading = true;
            TemplateClient.Page(new TemplatePageInput
            {
                page = currentPage,
                pageSize = pageSize,
                field = field,
                order = order,
                descStr = "descending",
                name = name.Trim(),
                code = queryCode.Trim(),
                type = queryType > 0 ? queryType : null,
                groupName = groupName.Trim()
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载模板失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }

                items = TemplateClient.ReadItems(outcome.Data);
                rows = items;
                total = TemplateClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载模板失败，请检查网络后重试。";
            });
        }

        private void LoadGroups()
        {
            TemplateClient.Groups().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载模板分组失败。";
                    groups = [];
                    return;
                }
                groups = TemplateClient.ReadGroups(outcome.Data);
            }, _ => error = "加载模板分组失败，请检查网络后重试。");
        }

        private void LoadTypes()
        {
            TemplateClient.Types().Then(outcome =>
            {
                if (!outcome.Ok)
                    return;
                var loaded = TemplateClient.ReadTypes(outcome.Data);
                if (loaded.Length > 0)
                    typeOptions = loaded;
            });
        }

        private void DismissError() => error = null;
        private void SetName(string value) => name = value;
        private void SetQueryCode(string value) => queryCode = value;
        private void SetQueryType(double value) => queryType = (int)value;
        private void SetGroupName(string value) => groupName = value;

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            name = string.Empty;
            queryCode = string.Empty;
            queryType = 0;
            groupName = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void Refresh()
        {
            LoadGroups();
            LoadTypes();
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

        private void Sort(string prop, string direction)
        {
            field = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            order = direction == "descending" ? "descending" : "ascending";
            LoadPage();
        }

        private void OpenCreate()
        {
            ResetDialog();
            editDialogTitle = "新增模板";
            editType = typeOptions.Length == 0 ? 1 : typeOptions[0].value;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;
            PopulateDialog(row, false);
        }

        private void OpenPreview(string id)
        {
            var row = Find(id);
            if (row is null || !CanPreview)
                return;
            PopulateDialog(row, true);
        }

        private void PopulateDialog(TemplateView row, bool readOnly)
        {
            ResetDialog();
            editDialogTitle = readOnly ? "预览模板" : "编辑模板";
            editReadonly = readOnly;
            editId = row.id;
            editName = row.name;
            editCode = row.code;
            editType = row.type;
            editGroupName = row.groupName;
            editOrderNo = row.orderNo;
            editRemark = row.remark;
            editContent = row.content;
            richEditorHtml = row.content;
            contentMode = LooksLikeHtml(row.content) ? "rich" : "plain";
            SyncRenderEntries(row.content);
            editDialogVisible = true;
        }

        private void ResetDialog()
        {
            editReadonly = false;
            saving = false;
            previewing = false;
            selectedTab = "basic";
            contentMode = "rich";
            editId = string.Empty;
            editName = string.Empty;
            editCode = string.Empty;
            editType = 1;
            editGroupName = string.Empty;
            editOrderNo = 100;
            editRemark = string.Empty;
            editContent = string.Empty;
            richEditorHtml = string.Empty;
            previewHtml = string.Empty;
            renderEntries = [];
            error = null;
        }

        public void Save()
        {
            if (saving || editReadonly)
                return;

            if (contentMode == "rich")
                SyncRichContent();

            var normalizedName = editName.Trim();
            var normalizedCode = editCode.Trim();
            var normalizedGroup = editGroupName.Trim();
            var normalizedContent = editContent.Trim();
            var normalizedRemark = editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                selectedTab = "basic";
                error = "模板名称不能为空。";
                return;
            }
            if (normalizedName.Length > 128)
            {
                selectedTab = "basic";
                error = "模板名称不能超过128个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                selectedTab = "basic";
                error = "模板编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 128)
            {
                selectedTab = "basic";
                error = "模板编码不能超过128个字符。";
                return;
            }
            if (editType <= 0)
            {
                selectedTab = "basic";
                error = "请选择模板类型。";
                return;
            }
            if (normalizedGroup.Length == 0)
            {
                selectedTab = "basic";
                error = "分组名称不能为空。";
                return;
            }
            if (normalizedGroup.Length > 32)
            {
                selectedTab = "basic";
                error = "分组名称不能超过32个字符。";
                return;
            }
            if (normalizedContent.Length == 0)
            {
                selectedTab = "content";
                error = "模板内容不能为空。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                selectedTab = "basic";
                error = "备注不能超过128个字符。";
                return;
            }

            var input = new TemplateSaveInput
            {
                id = editId,
                name = normalizedName,
                code = normalizedCode,
                type = editType,
                groupName = normalizedGroup,
                content = normalizedContent,
                orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                remark = normalizedRemark
            };

            saving = true;
            var request = editId.Length == 0 ? TemplateClient.Add(input) : TemplateClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存模板失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "模板已新增" : "模板已更新");
                LoadGroups();
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存模板失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            TemplateClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除模板失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("模板已删除");
                LoadGroups();
                LoadPage();
            }, _ => error = "删除模板失败，请检查网络后重试。");
        }

        public void RenderPreview()
        {
            if (previewing)
                return;
            if (contentMode == "rich")
                SyncRichContent();

            var content = editContent.Trim();
            if (content.Length == 0)
            {
                selectedTab = "content";
                ElementPlus.ElMessageMembers.Warning("请先输入模板内容");
                return;
            }

            previewing = true;
            TemplateClient.Render(content, BuildRenderData()).Then(outcome =>
            {
                previewing = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "生成模板预览失败。";
                    return;
                }
                previewHtml = TemplateClient.ReadRenderedHtml(outcome.Data);
                selectedTab = "content";
                error = null;
            }, _ =>
            {
                previewing = false;
                error = "生成模板预览失败，请检查网络后重试。";
            });
        }

        private VueDictionary<string> BuildRenderData()
        {
            var data = new VueDictionary<string>();
            for (var index = 0; index < renderEntries.Length; index++)
            {
                var key = renderEntries[index].key.Trim();
                if (key.Length > 0)
                    data[key] = renderEntries[index].value;
            }
            return data;
        }

        public void ContentChanged(string value)
        {
            editContent = value;
            SyncRenderEntries(value);
        }

        public void ChangeContentMode(string mode)
        {
            if (mode == contentMode)
                return;

            if (contentMode == "rich")
                SyncRichContent();
            if (mode == "rich")
                richEditorHtml = editContent;
            contentMode = mode == "plain" ? "plain" : "rich";
        }

        public void SyncRichContent()
        {
            editContent = TemplateBrowserActions.ReadHtml(RichEditorId);
            SyncRenderEntries(editContent);
        }

        public void RichCommand(string command)
        {
            if (editReadonly)
                return;
            TemplateBrowserActions.ExecuteEditorCommand(RichEditorId, command);
            SyncRichContent();
        }

        public void RichBlock(string tag)
        {
            if (editReadonly)
                return;
            TemplateBrowserActions.ExecuteEditorCommand(RichEditorId, "formatBlock", tag);
            SyncRichContent();
        }

        public void InsertParameter()
        {
            if (editReadonly)
                return;

            const string token = "@(name)";
            if (contentMode == "rich")
            {
                TemplateBrowserActions.InsertTextAtEnd(RichEditorId, token);
                SyncRichContent();
            }
            else
            {
                ContentChanged(editContent + token);
            }
        }

        public void AddRenderEntry()
        {
            var next = new TemplateRenderEntry[renderEntries.Length + 1];
            for (var index = 0; index < renderEntries.Length; index++)
                next[index] = renderEntries[index];
            next[renderEntries.Length] = new TemplateRenderEntry();
            renderEntries = next;
        }

        public void RemoveRenderEntry(int removeIndex)
        {
            if (removeIndex < 0 || removeIndex >= renderEntries.Length)
                return;
            var next = new TemplateRenderEntry[renderEntries.Length - 1];
            var target = 0;
            for (var index = 0; index < renderEntries.Length; index++)
            {
                if (index == removeIndex)
                    continue;
                next[target] = renderEntries[index];
                target++;
            }
            renderEntries = next;
        }

        private void SyncRenderEntries(string content)
        {
            var keys = ExtractParameterKeys(content);
            var missing = 0;
            for (var keyIndex = 0; keyIndex < keys.Length; keyIndex++)
                if (!HasRenderEntry(keys[keyIndex]))
                    missing++;
            if (missing == 0)
                return;

            var next = new TemplateRenderEntry[renderEntries.Length + missing];
            for (var index = 0; index < renderEntries.Length; index++)
                next[index] = renderEntries[index];
            var target = renderEntries.Length;
            for (var keyIndex = 0; keyIndex < keys.Length; keyIndex++)
            {
                if (HasRenderEntry(keys[keyIndex]))
                    continue;
                next[target] = new TemplateRenderEntry
                {
                    key = keys[keyIndex],
                    value = "参数" + (target + 1)
                };
                target++;
            }
            renderEntries = next;
        }

        private bool HasRenderEntry(string key)
        {
            for (var index = 0; index < renderEntries.Length; index++)
                if (renderEntries[index].key == key)
                    return true;
            return false;
        }

        private static string[] ExtractParameterKeys(string content)
        {
            var keys = new string[CountParameters(content)];
            var count = 0;
            var searchIndex = 0;
            while (searchIndex < content.Length)
            {
                var start = content.IndexOf("@(", searchIndex);
                if (start < 0)
                    break;
                var end = content.IndexOf(")", start + 2);
                if (end < 0)
                    break;
                var key = content.Substring(start + 2, end - start - 2).Trim();
                if (key.Length > 0 && !Contains(keys, count, key))
                {
                    keys[count] = key;
                    count++;
                }
                searchIndex = end + 1;
            }

            if (count == keys.Length)
                return keys;
            var compact = new string[count];
            for (var index = 0; index < count; index++)
                compact[index] = keys[index];
            return compact;
        }

        private static int CountParameters(string content)
        {
            var count = 0;
            var searchIndex = 0;
            while (searchIndex < content.Length)
            {
                var start = content.IndexOf("@(", searchIndex);
                if (start < 0)
                    break;
                var end = content.IndexOf(")", start + 2);
                if (end < 0)
                    break;
                count++;
                searchIndex = end + 1;
            }
            return count;
        }

        private static bool Contains(string[] values, int count, string key)
        {
            for (var index = 0; index < count; index++)
                if (values[index] == key)
                    return true;
            return false;
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;
            auditDialogTitle = "修改记录 · " + row.name;
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = DisplayValue(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = DisplayValue(row.updateTime);
            auditRemark = DisplayValue(row.remark);
            auditDialogVisible = true;
        }

        private TemplateView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static bool LooksLikeHtml(string content)
            => content.IndexOf("<") >= 0 && content.IndexOf(">") > content.IndexOf("<");

        private static string DisplayUser(string name, string id)
        {
            if (name.Length > 0 && id.Length > 0)
                return name + " (" + id + ")";
            if (name.Length > 0)
                return name;
            return DisplayValue(id);
        }

        private static string DisplayValue(string value)
            => value.Length == 0 ? "-" : value;

        private static TemplateTypeOption[] DefaultTypeOptions()
            =>
            [
                new TemplateTypeOption { value = 1, label = "通知", tagType = "primary" },
                new TemplateTypeOption { value = 2, label = "短信", tagType = "warning" },
                new TemplateTypeOption { value = 3, label = "邮件", tagType = "success" },
                new TemplateTypeOption { value = 4, label = "微信", tagType = "success" },
                new TemplateTypeOption { value = 5, label = "钉钉", tagType = "info" },
                new TemplateTypeOption { value = 7, label = "企业微信", tagType = "primary" }
            ];

        public string RichEditorIdValue => RichEditorId;

        public TemplatePageProps CreateProps() => new()
        {
            Error = error,
            Name = name,
            QueryCode = queryCode,
            QueryType = queryType,
            GroupName = groupName,
            TypeOptions = typeOptions,
            Groups = groups,
            Rows = rows,
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanPreview = CanPreview,
            CanDelete = CanDelete,
            Loading = loading,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            DismissError = DismissError,
            SetName = SetName,
            SetQueryCode = SetQueryCode,
            SetQueryType = SetQueryType,
            SetGroupName = SetGroupName,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            Refresh = Refresh,
            Edit = OpenEdit,
            Preview = OpenPreview,
            Delete = Delete,
            Audit = OpenAudit,
            Sort = Sort,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
        };
    }
}

