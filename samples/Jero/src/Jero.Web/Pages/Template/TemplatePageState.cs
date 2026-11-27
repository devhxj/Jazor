using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Template;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Template;

[ECMAScriptModule("components/template-page-state.js")]
public static class TemplatePageModule
{
    public static TemplatePageState Create() => new();

    public sealed class TemplatePageState : ITemplatePageState
    {
        private const string RichEditorId = "jero-template-rich-editor";

        private TemplateView[] _items = [];
        private TemplateView[] _rows = [];
        public string[] Groups { get; set; } = [];
        public TemplateTypeOption[] TypeOptions { get; set; } = DefaultTypeOptions();
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _name = string.Empty;
        private string _queryCode = string.Empty;
        private int _queryType;
        private string _groupName = string.Empty;
        private string _field = "orderNo";
        private string _order = "ascending";
        private bool _loading;
        private string? _error;

        public bool EditDialogVisible { get; set; }
        public string EditDialogTitle { get; set; } = "新增模板";
        public bool EditReadonly { get; set; }
        public bool Saving { get; set; }
        public bool Previewing { get; set; }
        public string SelectedTab { get; set; } = "basic";
        public string ContentMode { get; set; } = "rich";
        private string _editId = string.Empty;
        public string EditName { get; set; } = string.Empty;
        public string EditCode { get; set; } = string.Empty;
        public int EditType { get; set; } = 1;
        public string EditGroupName { get; set; } = string.Empty;
        public int EditOrderNo { get; set; } = 100;
        public string EditRemark { get; set; } = string.Empty;
        public string EditContent { get; set; } = string.Empty;
        public string RichEditorHtml { get; set; } = string.Empty;
        public string PreviewHtml { get; set; } = string.Empty;
        public TemplateRenderEntry[] RenderEntries { get; set; } = [];

        public bool AuditDialogVisible { get; set; }
        public string AuditDialogTitle { get; set; } = "修改记录";
        public string AuditCreateUser { get; set; } = "-";
        public string AuditCreateTime { get; set; } = "-";
        public string AuditUpdateUser { get; set; } = "-";
        public string AuditUpdateTime { get; set; } = "-";
        public string AuditRemark { get; set; } = "-";

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
            _loading = true;
            TemplateClient.Page(new TemplatePageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Field = _field,
                Order = _order,
                DescStr = "descending",
                Name = _name.Trim(),
                Code = _queryCode.Trim(),
                Type = _queryType > 0 ? _queryType : null,
                GroupName = _groupName.Trim()
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载模板失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }

                _items = TemplateClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = TemplateClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载模板失败，请检查网络后重试。";
            });
        }

        private void LoadGroups()
        {
            TemplateClient.Groups().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载模板分组失败。";
                    Groups = [];
                    return;
                }
                Groups = TemplateClient.ReadGroups(outcome.Data);
            }, _ => _error = "加载模板分组失败，请检查网络后重试。");
        }

        private void LoadTypes()
        {
            TemplateClient.Types().Then(outcome =>
            {
                if (!outcome.Ok)
                    return;
                var loaded = TemplateClient.ReadTypes(outcome.Data);
                if (loaded.Length > 0)
                    TypeOptions = loaded;
            });
        }

        private void DismissError() => _error = null;
        private void SetName(string value) => _name = value;
        private void SetQueryCode(string value) => _queryCode = value;
        private void SetQueryType(double value) => _queryType = (int)value;
        private void SetGroupName(string value) => _groupName = value;

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _name = string.Empty;
            _queryCode = string.Empty;
            _queryType = 0;
            _groupName = string.Empty;
            _currentPage = 1;
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
            _currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(double value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadPage();
        }

        private void Sort(string prop, string direction)
        {
            _field = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            _order = direction == "descending" ? "descending" : "ascending";
            LoadPage();
        }

        private void OpenCreate()
        {
            ResetDialog();
            EditDialogTitle = "新增模板";
            EditType = TypeOptions.Length == 0 ? 1 : TypeOptions[0].Value;
            EditDialogVisible = true;
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
            EditDialogTitle = readOnly ? "预览模板" : "编辑模板";
            EditReadonly = readOnly;
            _editId = row.Id;
            EditName = row.Name;
            EditCode = row.Code;
            EditType = row.Type;
            EditGroupName = row.GroupName;
            EditOrderNo = row.OrderNo;
            EditRemark = row.Remark;
            EditContent = row.Content;
            RichEditorHtml = row.Content;
            ContentMode = LooksLikeHtml(row.Content) ? "rich" : "plain";
            SyncRenderEntries(row.Content);
            EditDialogVisible = true;
        }

        private void ResetDialog()
        {
            EditReadonly = false;
            Saving = false;
            Previewing = false;
            SelectedTab = "basic";
            ContentMode = "rich";
            _editId = string.Empty;
            EditName = string.Empty;
            EditCode = string.Empty;
            EditType = 1;
            EditGroupName = string.Empty;
            EditOrderNo = 100;
            EditRemark = string.Empty;
            EditContent = string.Empty;
            RichEditorHtml = string.Empty;
            PreviewHtml = string.Empty;
            RenderEntries = [];
            _error = null;
        }

        public void Save()
        {
            if (Saving || EditReadonly)
                return;

            if (ContentMode == "rich")
                SyncRichContent();

            var normalizedName = EditName.Trim();
            var normalizedCode = EditCode.Trim();
            var normalizedGroup = EditGroupName.Trim();
            var normalizedContent = EditContent.Trim();
            var normalizedRemark = EditRemark.Trim();
            if (normalizedName.Length == 0)
            {
                SelectedTab = "basic";
                _error = "模板名称不能为空。";
                return;
            }
            if (normalizedName.Length > 128)
            {
                SelectedTab = "basic";
                _error = "模板名称不能超过128个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                SelectedTab = "basic";
                _error = "模板编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 128)
            {
                SelectedTab = "basic";
                _error = "模板编码不能超过128个字符。";
                return;
            }
            if (EditType <= 0)
            {
                SelectedTab = "basic";
                _error = "请选择模板类型。";
                return;
            }
            if (normalizedGroup.Length == 0)
            {
                SelectedTab = "basic";
                _error = "分组名称不能为空。";
                return;
            }
            if (normalizedGroup.Length > 32)
            {
                SelectedTab = "basic";
                _error = "分组名称不能超过32个字符。";
                return;
            }
            if (normalizedContent.Length == 0)
            {
                SelectedTab = "content";
                _error = "模板内容不能为空。";
                return;
            }
            if (normalizedRemark.Length > 128)
            {
                SelectedTab = "basic";
                _error = "备注不能超过128个字符。";
                return;
            }

            var input = new TemplateSaveInput
            {
                Id = _editId,
                Name = normalizedName,
                Code = normalizedCode,
                Type = EditType,
                GroupName = normalizedGroup,
                Content = normalizedContent,
                OrderNo = EditOrderNo < 0 ? 0 : EditOrderNo,
                Remark = normalizedRemark
            };

            Saving = true;
            var request = _editId.Length == 0 ? TemplateClient.Add(input) : TemplateClient.Update(input);
            request.Then(outcome =>
            {
                Saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存模板失败。";
                    return;
                }

                EditDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "模板已新增" : "模板已更新");
                LoadGroups();
                LoadPage();
            }, _ =>
            {
                Saving = false;
                _error = "保存模板失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            TemplateClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除模板失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("模板已删除");
                LoadGroups();
                LoadPage();
            }, _ => _error = "删除模板失败，请检查网络后重试。");
        }

        public void RenderPreview()
        {
            if (Previewing)
                return;
            if (ContentMode == "rich")
                SyncRichContent();

            var content = EditContent.Trim();
            if (content.Length == 0)
            {
                SelectedTab = "content";
                ElementPlus.ElMessageMembers.Warning("请先输入模板内容");
                return;
            }

            Previewing = true;
            TemplateClient.Render(content, BuildRenderData()).Then(outcome =>
            {
                Previewing = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "生成模板预览失败。";
                    return;
                }
                PreviewHtml = TemplateClient.ReadRenderedHtml(outcome.Data);
                SelectedTab = "content";
                _error = null;
            }, _ =>
            {
                Previewing = false;
                _error = "生成模板预览失败，请检查网络后重试。";
            });
        }

        private VueDictionary<string> BuildRenderData()
        {
            var data = new VueDictionary<string>();
            for (var index = 0; index < RenderEntries.Length; index++)
            {
                var key = RenderEntries[index].Key.Trim();
                if (key.Length > 0)
                    data[key] = RenderEntries[index].Value;
            }
            return data;
        }

        public void ContentChanged(string value)
        {
            EditContent = value;
            SyncRenderEntries(value);
        }

        public void ChangeContentMode(string mode)
        {
            if (mode == ContentMode)
                return;

            if (ContentMode == "rich")
                SyncRichContent();
            if (mode == "rich")
                RichEditorHtml = EditContent;
            ContentMode = mode == "plain" ? "plain" : "rich";
        }

        public void SyncRichContent()
        {
            EditContent = TemplateBrowserActions.ReadHtml(RichEditorId);
            SyncRenderEntries(EditContent);
        }

        public void RichCommand(string command)
        {
            if (EditReadonly)
                return;
            TemplateBrowserActions.ExecuteEditorCommand(RichEditorId, command);
            SyncRichContent();
        }

        public void RichBlock(string tag)
        {
            if (EditReadonly)
                return;
            TemplateBrowserActions.ExecuteEditorCommand(RichEditorId, "formatBlock", tag);
            SyncRichContent();
        }

        public void InsertParameter()
        {
            if (EditReadonly)
                return;

            const string token = "@(name)";
            if (ContentMode == "rich")
            {
                TemplateBrowserActions.InsertTextAtEnd(RichEditorId, token);
                SyncRichContent();
            }
            else
            {
                ContentChanged(EditContent + token);
            }
        }

        public void AddRenderEntry()
        {
            var next = new TemplateRenderEntry[RenderEntries.Length + 1];
            for (var index = 0; index < RenderEntries.Length; index++)
                next[index] = RenderEntries[index];
            next[RenderEntries.Length] = new TemplateRenderEntry();
            RenderEntries = next;
        }

        public void RemoveRenderEntry(int removeIndex)
        {
            if (removeIndex < 0 || removeIndex >= RenderEntries.Length)
                return;
            var next = new TemplateRenderEntry[RenderEntries.Length - 1];
            var target = 0;
            for (var index = 0; index < RenderEntries.Length; index++)
            {
                if (index == removeIndex)
                    continue;
                next[target] = RenderEntries[index];
                target++;
            }
            RenderEntries = next;
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

            var next = new TemplateRenderEntry[RenderEntries.Length + missing];
            for (var index = 0; index < RenderEntries.Length; index++)
                next[index] = RenderEntries[index];
            var target = RenderEntries.Length;
            for (var keyIndex = 0; keyIndex < keys.Length; keyIndex++)
            {
                if (HasRenderEntry(keys[keyIndex]))
                    continue;
                next[target] = new TemplateRenderEntry
                {
                    Key = keys[keyIndex],
                    Value = "参数" + (target + 1)
                };
                target++;
            }
            RenderEntries = next;
        }

        private bool HasRenderEntry(string key)
        {
            for (var index = 0; index < RenderEntries.Length; index++)
                if (RenderEntries[index].Key == key)
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
            AuditDialogTitle = "修改记录 · " + row.Name;
            AuditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            AuditCreateTime = DisplayValue(row.CreateTime);
            AuditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            AuditUpdateTime = DisplayValue(row.UpdateTime);
            AuditRemark = DisplayValue(row.Remark);
            AuditDialogVisible = true;
        }

        private TemplateView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
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
                new TemplateTypeOption { Value = 1, Label = "通知", TagType = "primary" },
                new TemplateTypeOption { Value = 2, Label = "短信", TagType = "warning" },
                new TemplateTypeOption { Value = 3, Label = "邮件", TagType = "success" },
                new TemplateTypeOption { Value = 4, Label = "微信", TagType = "success" },
                new TemplateTypeOption { Value = 5, Label = "钉钉", TagType = "info" },
                new TemplateTypeOption { Value = 7, Label = "企业微信", TagType = "primary" }
            ];

        public string RichEditorIdValue => RichEditorId;

        public TemplatePageProps CreateProps() => new()
        {
            Error = _error,
            Name = _name,
            QueryCode = _queryCode,
            QueryType = _queryType,
            GroupName = _groupName,
            TypeOptions = TypeOptions,
            Groups = Groups,
            Rows = _rows,
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanPreview = CanPreview,
            CanDelete = CanDelete,
            Loading = _loading,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
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
