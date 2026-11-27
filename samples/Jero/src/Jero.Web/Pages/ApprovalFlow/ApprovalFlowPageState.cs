using Jero.Web.Infrastructure.Api.ApprovalFlow;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Pages.ApprovalFlow;

[ECMAScriptModule("components/approval-flow-page-state.js")]
public static class ApprovalFlowPageModule
{
    public static ApprovalFlowPageState Create() => new();

    public sealed class ApprovalFlowPageState : global::Jero.Web.Composition.IPageState<ApprovalFlowPageProps>
    {
        private const int EditRequest = 1;
        private const int DetailRequest = 2;
        private const int FormRequest = 3;
        private const int DesignerRequest = 4;
        private readonly double[] _pageSizes = [10, 20, 50, 100];
        private readonly ApprovalOperationOption[] _operationOptions =
        [
            new() { Value = "add", Label = "新增" },
            new() { Value = "update", Label = "更新" },
            new() { Value = "delete", Label = "删除" },
            new() { Value = "select", Label = "查询" },
            new() { Value = "export", Label = "导出" }
        ];

        private ApprovalFlowView[] _items = [];
        private ApprovalFlowView[] _rows = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _queryKeyword = string.Empty;
        private string _queryCode = string.Empty;
        private string _queryName = string.Empty;
        private string _queryRemark = string.Empty;
        private bool _loading;
        private string? _error;

        private bool _editDialogVisible;
        private bool _editLoading;
        private bool _saving;
        private string _editDialogTitle = "新增审批流";
        private ApprovalFlowView? _editing;
        private string _editCode = string.Empty;
        private string _editName = string.Empty;
        private string _editRemark = string.Empty;

        private bool _detailDialogVisible;
        private bool _detailLoading;
        private ApprovalFlowView? _detail;
        private ApprovalFormConfig _detailForm = new();
        private ApprovalFlowGraph _detailGraph = new();
        private ApprovalFlowSummary _detailSummary = new();
        private string _detailParseError = string.Empty;
        private string _detailFormJson = "暂无表单 JSON";
        private string _detailFlowJson = "暂无流程 JSON";

        private bool _formDialogVisible;
        private bool _formLoading;
        private bool _formSaving;
        private bool _tableLoading;
        private ApprovalFlowView? _formSource;
        private ApprovalDatabaseOption[] _databaseOptions = [];
        private ApprovalTableOption[] _tableOptions = [];
        private string[] _routeOptions = [];
        private string _originalRoute = string.Empty;
        private string _formConfigId = string.Empty;
        private string _formTableName = string.Empty;
        private string _formEntityName = string.Empty;
        private string _formTypeName = string.Empty;

        private bool _designerDialogVisible;
        private bool _designerLoading;
        private bool _designerSaving;
        private ApprovalFlowView? _designerSource;
        private ApprovalFlowGraph _designerGraph = ApprovalFlowShared.CreateDefaultGraph();
        private ApprovalFlowSummary _designerSummary = new();
        private ApprovalFlowDesignerSession? _designer;
        private string _designerWarning = string.Empty;
        private string _designerError = string.Empty;
        private bool _selectionMode;
        private bool _canUndo;
        private bool _canRedo;
        private ApprovalFlowSelection _selected = new();
        private string _selectedText = string.Empty;
        private bool _graphJsonVisible;
        private string _graphJsonText = string.Empty;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

        private bool _contractDialogVisible;
        private string _contractDialogTitle = "流程运行契约";
        private string _contractDialogText = string.Empty;
        private string _contractFlowName = string.Empty;

        private bool _disposed;
        private int _pageGeneration;
        private int _editGeneration;
        private int _detailGeneration;
        private int _formGeneration;
        private int _designerGeneration;

        private string FormRoutePreview => ApprovalFlowShared.PreviewRoute(_formEntityName, _formTypeName);
        private bool FormRouteConflict => HasRouteConflict(FormRoutePreview);

        public void Initialize()
        {
            _disposed = false;
            // Vue 移除页面 DOM 之前，先释放这个页面持有的浏览器资源。
            OnBeforeUnmount(Dispose);
            LoadPage();
        }

        public void Dispose()
        {
            DisposePage();
        }

        private void DisposePage()
        {
            if (_disposed)
                return;
            _disposed = true;
            InvalidatePendingWork();
            CleanupDesigner();
        }

        private void InvalidatePendingWork()
        {
            _pageGeneration++;
            _editGeneration++;
            _detailGeneration++;
            _formGeneration++;
            _designerGeneration++;
            _editDialogVisible = false;
            _detailDialogVisible = false;
            _formDialogVisible = false;
            _designerDialogVisible = false;
            _graphJsonVisible = false;
        }

        private void LoadPage()
        {
            if (_disposed)
                return;
            var generation = ++_pageGeneration;
            _loading = true;
            ApprovalFlowClient.Page(new ApprovalFlowPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Keyword = _queryKeyword.Trim(),
                Code = _queryCode.Trim(),
                Name = _queryName.Trim(),
                Remark = _queryRemark.Trim()
            }).Then(outcome =>
            {
                if (_disposed || generation != _pageGeneration)
                    return;
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载审批流程失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }
                _items = ApprovalFlowClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = ApprovalFlowClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                if (_disposed || generation != _pageGeneration)
                    return;
                _loading = false;
                _error = "加载审批流程失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _queryKeyword = string.Empty;
            _queryCode = string.Empty;
            _queryName = string.Empty;
            _queryRemark = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void ChangePage(Number value)
        {
            _currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(Number value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            _editGeneration++;
            _editing = null;
            _editDialogTitle = "新增审批流";
            _editCode = string.Empty;
            _editName = string.Empty;
            _editRemark = string.Empty;
            _editLoading = false;
            _error = null;
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            if (id.Length == 0)
                return;
            var generation = ++_editGeneration;
            _editDialogTitle = "编辑审批流";
            _editDialogVisible = true;
            _editLoading = true;
            LoadDetail(id, EditRequest, generation, loaded =>
            {
                _editLoading = false;
                _editing = loaded;
                _editCode = loaded.Code;
                _editName = loaded.Name;
                _editRemark = loaded.Remark;
            }, "加载审批流详情失败。");
        }

        private void CloseEditDialog()
        {
            _editGeneration++;
            _editDialogVisible = false;
            _editLoading = false;
        }

        private void SaveBasic()
        {
            if (_saving || _editLoading)
                return;
            var name = _editName.Trim();
            var code = _editCode.Trim();
            var remark = _editRemark.Trim();
            if (name.Length == 0)
            {
                _error = "名称不能为空。";
                return;
            }
            if (name.Length > 32)
            {
                _error = "名称不能超过32个字符。";
                return;
            }
            if (code.Length > 32)
            {
                _error = "编号不能超过32个字符。";
                return;
            }
            if (remark.Length > 256)
            {
                _error = "备注不能超过256个字符。";
                return;
            }

            var creating = _editing is null;
            var request = creating
                ? ApprovalFlowClient.Add(new ApprovalFlowAddInput
                {
                    Code = ApprovalFlowShared.Optional(code),
                    Name = name,
                    FormJson = null,
                    FlowJson = null,
                    Remark = ApprovalFlowShared.Optional(remark),
                    IsDelete = false
                })
                : ApprovalFlowClient.Update(BuildUpdate(
                    _editing!,
                    code,
                    name,
                    _editing!.FormJson,
                    _editing!.FlowJson,
                    remark));

            var generation = _editGeneration;
            _saving = true;
            request.Then(outcome =>
            {
                if (_disposed || generation != _editGeneration || !_editDialogVisible)
                    return;
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存审批流失败。";
                    return;
                }
                CloseEditDialog();
                _error = null;
                ElementPlus.ElMessageMembers.Success(creating ? "审批流已新增" : "审批流基础信息已更新");
                LoadPage();
            }, _ =>
            {
                if (_disposed || generation != _editGeneration || !_editDialogVisible)
                    return;
                _saving = false;
                _error = "保存审批流失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            if (id.Length == 0)
                return;
            ApprovalFlowClient.Delete(id).Then(outcome =>
            {
                if (_disposed)
                    return;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除审批流失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("审批流已删除");
                LoadPage();
            }, _ =>
            {
                if (!_disposed)
                    _error = "删除审批流失败，请检查网络后重试。";
            });
        }

        private void OpenDetail(string id)
        {
            if (id.Length == 0)
                return;
            var generation = ++_detailGeneration;
            _detailDialogVisible = true;
            _detailLoading = true;
            _detailParseError = string.Empty;
            LoadDetail(id, DetailRequest, generation, loaded =>
            {
                _detailLoading = false;
                _detail = loaded;
                _detailFormJson = ApprovalFlowShared.FormatJson(loaded.FormJson, "暂无表单 JSON");
                _detailFlowJson = ApprovalFlowShared.FormatJson(loaded.FlowJson, "暂无流程 JSON");
                try
                {
                    _detailForm = ApprovalFlowShared.ParseForm(loaded.FormJson);
                }
                catch (Exception exception)
                {
                    _detailForm = new ApprovalFormConfig();
                    _detailParseError = exception.Message;
                }
                try
                {
                    _detailGraph = ApprovalFlowShared.ParseGraph(loaded.FlowJson);
                    _detailSummary = ApprovalFlowShared.Summarize(_detailGraph);
                }
                catch (Exception exception)
                {
                    _detailGraph = new ApprovalFlowGraph();
                    _detailSummary = new ApprovalFlowSummary();
                    _detailParseError = _detailParseError.Length == 0 ? exception.Message : _detailParseError + "；" + exception.Message;
                }
            }, "加载审批流详情失败。");
        }

        private void CloseDetailDialog()
        {
            _detailGeneration++;
            _detailDialogVisible = false;
            _detailLoading = false;
        }

        private void OpenFormConfig(string id)
        {
            if (id.Length == 0)
                return;
            var generation = ++_formGeneration;
            _formDialogVisible = true;
            _formLoading = true;
            _formSource = null;
            _databaseOptions = [];
            _tableOptions = [];
            _routeOptions = [];
            LoadDetail(id, FormRequest, generation, loaded =>
            {
                _formSource = loaded;
                try
                {
                    var config = ApprovalFlowShared.ParseForm(loaded.FormJson);
                    _formConfigId = config.ConfigId;
                    _formTableName = config.TableName;
                    _formEntityName = config.EntityName;
                    _formTypeName = config.TypeName;
                    _originalRoute = config.Route;
                }
                catch (Exception exception)
                {
                    _formConfigId = string.Empty;
                    _formTableName = string.Empty;
                    _formEntityName = string.Empty;
                    _formTypeName = string.Empty;
                    _originalRoute = string.Empty;
                    _error = exception.Message;
                }
                LoadFormDependencies(generation);
            }, "加载审批表单配置失败。");
        }

        private void CloseFormDialog()
        {
            _formGeneration++;
            _formDialogVisible = false;
            _formLoading = false;
            _tableLoading = false;
        }

        private void LoadFormDependencies(int generation)
        {
            ApprovalFlowClient.Databases().Then(databaseOutcome =>
            {
                if (!IsRequestCurrent(FormRequest, generation))
                    return;
                if (!databaseOutcome.Ok)
                {
                    _formLoading = false;
                    _error = databaseOutcome.Error ?? "加载库定位器失败。";
                    return;
                }
                _databaseOptions = ApprovalFlowClient.ReadDatabases(databaseOutcome.Data);
                ApprovalFlowClient.FormRoutes().Then(routeOutcome =>
                {
                    if (!IsRequestCurrent(FormRequest, generation))
                        return;
                    if (!routeOutcome.Ok)
                    {
                        _formLoading = false;
                        _error = routeOutcome.Error ?? "加载审批路由失败。";
                        return;
                    }
                    _routeOptions = ApprovalFlowClient.ReadStrings(routeOutcome.Data);
                    if (_formConfigId.Length == 0)
                    {
                        _formLoading = false;
                        return;
                    }
                    LoadTables(_formConfigId, true, generation);
                }, _ =>
                {
                    if (!IsRequestCurrent(FormRequest, generation))
                        return;
                    _formLoading = false;
                    _error = "加载审批路由失败，请检查网络后重试。";
                });
            }, _ =>
            {
                if (!IsRequestCurrent(FormRequest, generation))
                    return;
                _formLoading = false;
                _error = "加载库定位器失败，请检查网络后重试。";
            });
        }

        private void FormConfigChanged(string? value)
        {
            _formConfigId = value ?? string.Empty;
            _formTableName = string.Empty;
            _formEntityName = string.Empty;
            _tableOptions = [];
            if (_formConfigId.Length > 0)
                LoadTables(_formConfigId, false, _formGeneration);
        }

        private void LoadTables(string configId, bool finishFormLoading, int generation)
        {
            _tableLoading = true;
            ApprovalFlowClient.Tables(configId).Then(outcome =>
            {
                if (!IsRequestCurrent(FormRequest, generation))
                    return;
                _tableLoading = false;
                if (finishFormLoading)
                    _formLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载数据库表失败。";
                    _tableOptions = [];
                    return;
                }
                _tableOptions = ApprovalFlowClient.ReadTables(outcome.Data);
            }, _ =>
            {
                if (!IsRequestCurrent(FormRequest, generation))
                    return;
                _tableLoading = false;
                if (finishFormLoading)
                    _formLoading = false;
                _error = "加载数据库表失败，请检查网络后重试。";
            });
        }

        private void FormTableChanged(string? value)
        {
            _formTableName = value ?? string.Empty;
            for (var index = 0; index < _tableOptions.Length; index++)
            {
                if (_tableOptions[index].TableName != _formTableName)
                    continue;
                if (_tableOptions[index].EntityName.Length > 0)
                    _formEntityName = _tableOptions[index].EntityName;
                return;
            }
        }

        private void SaveFormConfig()
        {
            if (_formSaving || _formLoading || _formSource is null)
                return;
            if (FormRouteConflict)
            {
                _error = "表单路由【" + FormRoutePreview + "】已被其他审批流占用。";
                return;
            }

            string formJson;
            try
            {
                formJson = ApprovalFlowShared.StringifyForm(new ApprovalFormConfig
                {
                    ConfigId = _formConfigId,
                    TableName = _formTableName,
                    EntityName = _formEntityName,
                    TypeName = _formTypeName,
                    Route = FormRoutePreview
                });
            }
            catch (Exception exception)
            {
                _error = exception.Message;
                return;
            }

            _formSaving = true;
            var source = _formSource;
            var generation = _formGeneration;
            ApprovalFlowClient.Update(BuildUpdate(source, source.Code, source.Name, formJson, source.FlowJson, source.Remark)).Then(outcome =>
            {
                if (_disposed || generation != _formGeneration || !_formDialogVisible)
                    return;
                _formSaving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存审批表单配置失败。";
                    return;
                }
                CloseFormDialog();
                _error = null;
                ElementPlus.ElMessageMembers.Success("审批表单配置已保存");
                LoadPage();
            }, _ =>
            {
                if (_disposed || generation != _formGeneration || !_formDialogVisible)
                    return;
                _formSaving = false;
                _error = "保存审批表单配置失败，请检查网络后重试。";
            });
        }

        private void OpenDesigner(string id)
        {
            if (id.Length == 0)
                return;
            var generation = ++_designerGeneration;
            CleanupDesigner();
            _designerDialogVisible = true;
            _designerLoading = true;
            _designerWarning = string.Empty;
            _designerError = string.Empty;
            LoadDetail(id, DesignerRequest, generation, loaded =>
            {
                _designerSource = loaded;
                try
                {
                    _designerGraph = ApprovalFlowShared.ParseGraph(loaded.FlowJson);
                }
                catch (Exception exception)
                {
                    _designerGraph = ApprovalFlowShared.CreateDefaultGraph();
                    _designerWarning = "旧流程 JSON 无法解析，已载入默认模板：" + exception.Message;
                }
                _designerSummary = ApprovalFlowShared.Summarize(_designerGraph);
                Window.SetTimeout(
                    (TimerHandler)(Delegate)(Action)(() => InitializeDesigner(generation)),
                    0,
                    []);
            }, "加载审批流程设计数据失败。");
        }

        private void InitializeDesigner(int generation)
        {
            if (!IsRequestCurrent(DesignerRequest, generation) || _designerSource is null)
                return;
            try
            {
                CleanupDesigner();
                _designer = ApprovalFlowDesignerBrowserActions.Create(
                    _designerGraph,
                    value =>
                    {
                        if (IsRequestCurrent(DesignerRequest, generation))
                            DesignerSelectionChanged(value);
                    },
                    (undo, redo) =>
                    {
                        if (IsRequestCurrent(DesignerRequest, generation))
                            DesignerHistoryChanged(undo, redo);
                    },
                    () =>
                    {
                        if (IsRequestCurrent(DesignerRequest, generation))
                            DesignerGraphChanged();
                    });
                _designerLoading = false;
                _designerError = string.Empty;
                DesignerGraphChanged();
            }
            catch (Exception exception)
            {
                _designer = null;
                _designerLoading = false;
                _designerError = "流程设计器初始化失败：" + exception.Message;
            }
        }

        private void DesignerSelectionChanged(ApprovalFlowSelection value)
        {
            _selected = value;
            _selectedText = value.Text;
        }

        private void DesignerHistoryChanged(bool undo, bool redo)
        {
            _canUndo = undo;
            _canRedo = redo;
        }

        private void DesignerGraphChanged()
        {
            if (_designer is null)
                return;
            try
            {
                _designerGraph = ApprovalFlowShared.NormalizeGraph(ApprovalFlowDesignerBrowserActions.GetGraph(_designer));
                _designerSummary = ApprovalFlowShared.Summarize(_designerGraph);
                if (_selected.Id.Length > 0)
                    DesignerSelectionChanged(ApprovalFlowDesignerBrowserActions.ReadSelection(_designer, _selected.Id));
            }
            catch
            {
                // LogicFlow 拖拽期间会出现短暂中间态，保存时再执行严格结构校验。
            }
        }

        private void ToggleSelectionMode()
        {
            if (_designer is null)
                return;
            _selectionMode = !_selectionMode;
            ApprovalFlowDesignerBrowserActions.SetSelectionMode(_designer, _selectionMode);
        }

        private void ApplySelectedText()
        {
            if (_designer is null || _selected.Id.Length == 0)
                return;
            ApprovalFlowDesignerBrowserActions.UpdateText(_designer, _selected.Id, _selectedText);
            DesignerGraphChanged();
        }

        private void DeleteSelected()
        {
            if (_designer is null || _selected.Id.Length == 0)
                return;
            ApprovalFlowDesignerBrowserActions.DeleteElement(_designer, _selected.Id);
            DesignerSelectionChanged(new ApprovalFlowSelection());
            DesignerGraphChanged();
        }

        private void FocusSelected()
        {
            if (_designer is not null && _selected.Id.Length > 0)
                ApprovalFlowDesignerBrowserActions.Focus(_designer, _selected.Id);
        }

        private void InlineEditSelected()
        {
            if (_designer is not null && _selected.Id.Length > 0)
                ApprovalFlowDesignerBrowserActions.EditText(_designer, _selected.Id);
        }

        private void FitDesigner()
        {
            if (_designer is not null)
                ApprovalFlowDesignerBrowserActions.Fit(_designer);
        }

        private void CenterDesigner()
        {
            if (_designer is not null)
                ApprovalFlowDesignerBrowserActions.Center(_designer);
        }

        private void ZoomInDesigner()
        {
            if (_designer is not null)
                ApprovalFlowDesignerBrowserActions.ZoomIn(_designer);
        }

        private void ZoomOutDesigner()
        {
            if (_designer is not null)
                ApprovalFlowDesignerBrowserActions.ZoomOut(_designer);
        }

        private void ResetDesignerZoom()
        {
            if (_designer is not null)
                ApprovalFlowDesignerBrowserActions.ResetZoom(_designer);
        }

        private void UndoDesigner()
        {
            if (_designer is not null)
                ApprovalFlowDesignerBrowserActions.Undo(_designer);
        }

        private void RedoDesigner()
        {
            if (_designer is not null)
                ApprovalFlowDesignerBrowserActions.Redo(_designer);
        }

        private void ResetDesignerGraph()
        {
            if (_designer is null)
                return;
            _designerGraph = ApprovalFlowShared.CreateDefaultGraph();
            ApprovalFlowDesignerBrowserActions.Render(_designer, _designerGraph);
            FitDesigner();
            DesignerSelectionChanged(new ApprovalFlowSelection());
            DesignerGraphChanged();
        }

        private void ClearDesignerGraph()
        {
            if (_designer is null)
                return;
            ApprovalFlowDesignerBrowserActions.Clear(_designer);
            _designerGraph = new ApprovalFlowGraph();
            DesignerSelectionChanged(new ApprovalFlowSelection());
            DesignerGraphChanged();
        }

        private void ShowGraphJson()
        {
            if (_designer is null)
                return;
            try
            {
                _graphJsonText = JSON.Stringify(
                    ApprovalFlowShared.NormalizeGraph(ApprovalFlowDesignerBrowserActions.GetGraph(_designer)),
                    (object?)null,
                    (Number)2) ?? "{}";
                _graphJsonVisible = true;
            }
            catch (Exception exception)
            {
                _error = exception.Message;
            }
        }

        private void SaveDesigner()
        {
            if (_designerSaving || _designerLoading || _designer is null || _designerSource is null)
                return;
            string flowJson;
            try
            {
                flowJson = ApprovalFlowShared.StringifyGraph(ApprovalFlowDesignerBrowserActions.GetGraph(_designer));
            }
            catch (Exception exception)
            {
                _error = exception.Message;
                return;
            }

            _designerSaving = true;
            var source = _designerSource;
            var generation = _designerGeneration;
            ApprovalFlowClient.Update(BuildUpdate(source, source.Code, source.Name, source.FormJson, flowJson, source.Remark)).Then(outcome =>
            {
                if (_disposed || generation != _designerGeneration || !_designerDialogVisible)
                    return;
                _designerSaving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存审批流程失败。";
                    return;
                }
                CloseDesigner();
                _error = null;
                ElementPlus.ElMessageMembers.Success("审批流程已保存");
                LoadPage();
            }, _ =>
            {
                if (_disposed || generation != _designerGeneration || !_designerDialogVisible)
                    return;
                _designerSaving = false;
                _error = "保存审批流程失败，请检查网络后重试。";
            });
        }

        private void CloseDesigner()
        {
            _designerGeneration++;
            _designerDialogVisible = false;
            _graphJsonVisible = false;
            CleanupDesigner();
        }

        private void CleanupDesigner()
        {
            ApprovalFlowDesignerBrowserActions.Destroy(_designer);
            _designer = null;
            _designerLoading = false;
            _selectionMode = false;
            _canUndo = false;
            _canRedo = false;
            _selected = new ApprovalFlowSelection();
            _selectedText = string.Empty;
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;
            _auditDialogTitle = "修改记录 · " + DisplayValue(row.Name);
            _auditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            _auditCreateTime = DisplayValue(row.CreateTime);
            _auditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            _auditUpdateTime = DisplayValue(row.UpdateTime);
            _auditDialogVisible = true;
        }

        private void OpenPublishContract(string id)
        {
            _contractFlowName = Find(id)?.Name ?? string.Empty;
            _contractDialogTitle = "发布与状态契约";
            _contractDialogText = "当前 ApprovalFlowService 没有发布或状态切换端点，Update DTO 也不接收 status。页面保留状态展示入口，但不会发明无效请求。";
            _contractDialogVisible = true;
        }

        private void OpenRecordContract(string id)
        {
            _contractFlowName = Find(id)?.Name ?? string.Empty;
            _contractDialogTitle = "审批记录入口";
            _contractDialogText = "中间件会写入 ApprovalFlowRecord 与 ApprovalFormRecord，但当前插件没有记录分页、详情或按流程查询 API。记录入口在此明确显示服务端契约缺口。";
            _contractDialogVisible = true;
        }

        private void LoadDetail(
            string id,
            int requestKind,
            int generation,
            Action<ApprovalFlowView> completed,
            string fallbackError)
        {
            ApprovalFlowClient.Detail(id).Then(outcome =>
            {
                if (!IsRequestCurrent(requestKind, generation))
                    return;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? fallbackError;
                    _editLoading = false;
                    _detailLoading = false;
                    _formLoading = false;
                    _designerLoading = false;
                    return;
                }
                var loaded = ApprovalFlowClient.ReadDetail(outcome.Data);
                if (loaded is null)
                {
                    _error = "审批流详情不存在或已被删除。";
                    _editLoading = false;
                    _detailLoading = false;
                    _formLoading = false;
                    _designerLoading = false;
                    return;
                }
                _error = null;
                completed(loaded);
            }, _ =>
            {
                if (!IsRequestCurrent(requestKind, generation))
                    return;
                _error = fallbackError;
                _editLoading = false;
                _detailLoading = false;
                _formLoading = false;
                _designerLoading = false;
            });
        }

        private bool IsRequestCurrent(int requestKind, int generation)
        {
            if (_disposed)
                return false;
            if (requestKind == EditRequest)
                return _editDialogVisible && generation == _editGeneration;
            if (requestKind == DetailRequest)
                return _detailDialogVisible && generation == _detailGeneration;
            if (requestKind == FormRequest)
                return _formDialogVisible && generation == _formGeneration;
            if (requestKind == DesignerRequest)
                return _designerDialogVisible && generation == _designerGeneration;
            return false;
        }

        private static ApprovalFlowUpdateInput BuildUpdate(
            ApprovalFlowView source,
            string code,
            string name,
            string formJson,
            string flowJson,
            string remark)
            => new()
            {
                Id = source.Id,
                Code = ApprovalFlowShared.Optional(code),
                Name = name.Trim(),
                FormJson = ApprovalFlowShared.Optional(formJson),
                FlowJson = ApprovalFlowShared.Optional(flowJson),
                Remark = ApprovalFlowShared.Optional(remark),
                IsDelete = source.IsDelete,
                CreateTime = ApprovalFlowShared.Optional(source.CreateTime),
                CreateUserId = ApprovalFlowShared.Optional(source.CreateUserId),
                CreateUserName = ApprovalFlowShared.Optional(source.CreateUserName),
                CreateOrgId = ApprovalFlowShared.Optional(source.CreateOrgId),
                CreateOrgName = ApprovalFlowShared.Optional(source.CreateOrgName),
                UpdateTime = ApprovalFlowShared.Optional(source.UpdateTime),
                UpdateUserId = ApprovalFlowShared.Optional(source.UpdateUserId),
                UpdateUserName = ApprovalFlowShared.Optional(source.UpdateUserName)
            };

        private bool HasRouteConflict(string route)
        {
            if (route.Length == 0)
                return false;
            var count = 0;
            for (var index = 0; index < _routeOptions.Length; index++)
                if (_routeOptions[index] == route)
                    count++;
            return count > (route == _originalRoute ? 1 : 0);
        }

        private string RowFormRoute(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null)
                return "-";
            try
            {
                var route = ApprovalFlowShared.ParseForm(row.FormJson).Route;
                return route.Length == 0 ? "未配置" : route;
            }
            catch
            {
                return "JSON 异常";
            }
        }

        private string RowFlowSummary(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null)
                return "-";
            try
            {
                var summary = ApprovalFlowShared.SummarizeText(row.FlowJson);
                return summary.NodeCount + " 节点 / " + summary.EdgeCount + " 连线";
            }
            catch
            {
                return "JSON 异常";
            }
        }

        private string RowStatus(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || row.Status < 0)
                return "详情可见";
            return row.Status == 1 ? "启用" : row.Status == 2 ? "停用" : "状态 " + row.Status;
        }

        private string RowStatusTag(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || row.Status < 0)
                return "info";
            return row.Status == 1 ? "success" : "warning";
        }

        private string DetailStatus()
        {
            if (_detail is null || _detail.Status < 0)
                return "未由当前详情响应提供";
            return _detail.Status == 1 ? "启用" : _detail.Status == 2 ? "停用" : "状态 " + _detail.Status;
        }

        private ApprovalFlowView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static string RowString(VueProps? row, string key)
        {
            if (row is null)
                return string.Empty;
            var value = Reflect.Get(row, key);
            return value is null ? string.Empty : StringValue(value);
        }

        private static string DatabaseLabel(ApprovalDatabaseOption value)
            => value.DbNickName.Length == 0 ? value.ConfigId : value.DbNickName + " (" + value.ConfigId + ")";

        private static string TableLabel(ApprovalTableOption value)
            => value.TableComment.Length == 0 ? value.TableName : value.TableName + " [" + value.TableComment + "]";

        private static string DisplayUser(string name, string id)
        {
            if (name.Length > 0 && id.Length > 0)
                return name + " (" + id + ")";
            if (name.Length > 0)
                return name;
            return DisplayValue(id);
        }

        private static string DisplayValue(string value) => value.Length == 0 ? "-" : value;

        private ConfigurationOption[] DatabaseOptionsView()
        {
            var result = new ConfigurationOption[_databaseOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(_databaseOptions[index].ConfigId, DatabaseLabel(_databaseOptions[index]));
            return result;
        }

        private ConfigurationOption[] TableOptionsView()
        {
            var result = new ConfigurationOption[_tableOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(_tableOptions[index].TableName, TableLabel(_tableOptions[index]));
            return result;
        }

        private ConfigurationOption[] OperationOptionsView()
        {
            var result = new ConfigurationOption[_operationOptions.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var item = _operationOptions[index];
                result[index] = new ConfigurationOption(item.Value, item.Label + " (" + item.Value + ")");
            }
            return result;
        }

        private ApprovalFlowNodeRow[] DetailNodesView()
        {
            var result = new ApprovalFlowNodeRow[_detailGraph.Nodes.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var node = _detailGraph.Nodes[index];
                result[index] = new ApprovalFlowNodeRow(node.Id, node.Text?.Value ?? ApprovalFlowShared.NodeTypeLabel(node.Type), node.X, node.Y);
            }
            return result;
        }

        private ApprovalFlowTableRow[] TableRows()
        {
            var result = new ApprovalFlowTableRow[_rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _rows[index];
                var tag = row.Status < 0 ? "info" : row.Status == 1 ? "success" : "warning";
                result[index] = new ApprovalFlowTableRow(row.Id, row.Name, row.Code, row.Remark ?? string.Empty,
                    RowStatus(row), tag, RowFormRoute(row), RowFlowSummary(row));
            }
            return result;
        }

        public ApprovalFlowPageProps CreateProps() => new()
        {
            QueryKeyword = _queryKeyword,
            QueryCode = _queryCode,
            QueryName = _queryName,
            QueryRemark = _queryRemark,
            Error = _error,
            Loading = _loading,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            PageSizes = _pageSizes,
            SetQueryKeyword = value => _queryKeyword = value ?? "",
            SetQueryCode = value => _queryCode = value ?? "",
            SetQueryName = value => _queryName = value ?? "",
            SetQueryRemark = value => _queryRemark = value ?? "",
            OpenAudit = OpenAudit,
            OpenDetail = OpenDetail,
            OpenEdit = OpenEdit,
            OpenFormConfig = OpenFormConfig,
            OpenDesigner = OpenDesigner,
            OpenPublishContract = OpenPublishContract,
            OpenRecordContract = OpenRecordContract,
            Delete = Delete,
            Search = Search,
            ResetSearch = ResetSearch,
            LoadPage = LoadPage,
            OpenCreate = OpenCreate,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            DismissError = () => _error = null,
            Rows = TableRows(),
            DatabaseOptions = DatabaseOptionsView(),
            FormConfigChanged = FormConfigChanged,
            TableOptions = TableOptionsView(),
            FormTableChanged = FormTableChanged,
            OperationOptions = OperationOptionsView(),
            SetFormTypeName = value => _formTypeName = value,
            SetEditDialogVisible = visible => { if (!visible) CloseEditDialog(); },
            SetFormDialogVisible = visible => { if (!visible) CloseFormDialog(); },
            SetDesignerVisible = visible => { if (!visible) CloseDesigner(); },
            SetDetailDialogVisible = visible => { if (!visible) CloseDetailDialog(); },
            SetGraphJsonVisible = value => _graphJsonVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            SetContractDialogVisible = value => _contractDialogVisible = value,
            SetEditCode = value => _editCode = value ?? "",
            SetEditName = value => _editName = value ?? "",
            SetEditRemark = value => _editRemark = value ?? "",
            SetFormEntityName = value => _formEntityName = value ?? "",
            SetSelectedText = value => _selectedText = value ?? "",
            CloseEditDialog = CloseEditDialog,
            SaveBasic = SaveBasic,
            CloseFormDialog = CloseFormDialog,
            SaveFormConfig = SaveFormConfig,
            FitDesigner = FitDesigner,
            CenterDesigner = CenterDesigner,
            ZoomInDesigner = ZoomInDesigner,
            ZoomOutDesigner = ZoomOutDesigner,
            ResetDesignerZoom = ResetDesignerZoom,
            UndoDesigner = UndoDesigner,
            RedoDesigner = RedoDesigner,
            ToggleSelectionMode = ToggleSelectionMode,
            ShowGraphJson = ShowGraphJson,
            ApplySelectedText = ApplySelectedText,
            InlineEditSelected = InlineEditSelected,
            FocusSelected = FocusSelected,
            CloseDesigner = CloseDesigner,
            SaveDesigner = SaveDesigner,
            CloseDetailDialog = CloseDetailDialog,
            ResetDesignerGraph = ResetDesignerGraph,
            ClearDesignerGraph = ClearDesignerGraph,
            DeleteSelected = DeleteSelected,
            CleanupDesigner = CleanupDesigner,
            CloseContractDialog = () => _contractDialogVisible = false,
            DetailCodeText = DisplayValue(_detail?.Code ?? ""),
            DetailNameText = DisplayValue(_detail?.Name ?? ""),
            DetailRemarkText = DisplayValue(_detail?.Remark ?? ""),
            DetailCreateTimeText = DisplayValue(_detail?.CreateTime ?? ""),
            DetailUpdateTimeText = DisplayValue(_detail?.UpdateTime ?? ""),
            DetailFormConfigIdText = DisplayValue(_detailForm.ConfigId),
            DetailFormTableNameText = DisplayValue(_detailForm.TableName),
            DetailFormEntityNameText = DisplayValue(_detailForm.EntityName),
            DetailFormTypeNameText = DisplayValue(_detailForm.TypeName),
            DetailFormRouteText = DisplayValue(_detailForm.Route),
            ContractFlowNameText = DisplayValue(_contractFlowName),
            DetailCreateUserNameText = DisplayUser(_detail?.CreateUserName ?? "", _detail?.CreateUserId ?? ""),
            DetailUpdateUserNameText = DisplayUser(_detail?.UpdateUserName ?? "", _detail?.UpdateUserId ?? ""),
            DetailStatusText = DetailStatus(),
            DesignerNodeCount = _designerSummary.NodeCount,
            DesignerEdgeCount = _designerSummary.EdgeCount,
            DesignerStartCount = _designerSummary.StartCount,
            DesignerEndCount = _designerSummary.EndCount,
            DesignerUserCount = _designerSummary.UserCount,
            DesignerServiceCount = _designerSummary.ServiceCount,
            DesignerGatewayCount = _designerSummary.GatewayCount,
            DesignerIssues = _designerSummary.Issues,
            DetailNodeCount = _detailSummary.NodeCount,
            DetailEdgeCount = _detailSummary.EdgeCount,
            DetailNodes = DetailNodesView(),
            SelectedId = _selected.Id,
            SelectedKind = _selected.Kind,
            SelectedSourceNodeId = _selected.SourceNodeId,
            SelectedTargetNodeId = _selected.TargetNodeId,
            SelectedX = _selected.X,
            SelectedY = _selected.Y,
            CanvasId = ApprovalFlowDesignerBrowserActions.CanvasId,
            DesignerReady = _designer is not null,
            SelectedTypeLabel = _selected.Kind == "edge" ? "流程连线" : ApprovalFlowShared.NodeTypeLabel(_selected.Type),
            EditDialogVisible = _editDialogVisible,
            EditDialogTitle = _editDialogTitle,
            EditCode = _editCode,
            EditName = _editName,
            EditRemark = _editRemark,
            EditLoading = _editLoading,
            Saving = _saving,
            FormDialogVisible = _formDialogVisible,
            FormConfigId = _formConfigId,
            FormTableName = _formTableName,
            FormEntityName = _formEntityName,
            FormTypeName = _formTypeName,
            FormLoading = _formLoading,
            TableLoading = _tableLoading,
            FormSaving = _formSaving,
            FormRouteConflict = FormRouteConflict,
            FormRoutePreview = FormRoutePreview,
            DesignerDialogVisible = _designerDialogVisible,
            SelectionMode = _selectionMode,
            CanUndo = _canUndo,
            CanRedo = _canRedo,
            DesignerWarning = _designerWarning,
            DesignerError = _designerError,
            DesignerLoading = _designerLoading,
            DesignerSaving = _designerSaving,
            SelectedText = _selectedText,
            GraphJsonVisible = _graphJsonVisible,
            GraphJsonText = _graphJsonText,
            DetailDialogVisible = _detailDialogVisible,
            DetailParseError = _detailParseError,
            DetailFormJson = _detailFormJson,
            DetailFlowJson = _detailFlowJson,
            DetailLoading = _detailLoading,
            AuditDialogVisible = _auditDialogVisible,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            ContractDialogVisible = _contractDialogVisible,
            ContractDialogTitle = _contractDialogTitle,
            ContractDialogText = _contractDialogText,
        };
    }
}

public sealed record ApprovalFlowPageProps : VueProps
{
    public required string QueryKeyword { get; init; }
    public required string QueryCode { get; init; }
    public required string QueryName { get; init; }
    public required string QueryRemark { get; init; }
    public required string? Error { get; init; }
    public required bool Loading { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required double[] PageSizes { get; init; }
    public required Action<string?> SetQueryKeyword { get; init; }
    public required Action<string?> SetQueryCode { get; init; }
    public required Action<string?> SetQueryName { get; init; }
    public required Action<string?> SetQueryRemark { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenDetail { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> OpenFormConfig { get; init; }
    public required Action<string> OpenDesigner { get; init; }
    public required Action<string> OpenPublishContract { get; init; }
    public required Action<string> OpenRecordContract { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action LoadPage { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action DismissError { get; init; }
    public required ApprovalFlowTableRow[] Rows { get; init; }
    public required ConfigurationOption[] DatabaseOptions { get; init; }
    public required Action<string> FormConfigChanged { get; init; }
    public required ConfigurationOption[] TableOptions { get; init; }
    public required Action<string> FormTableChanged { get; init; }
    public required ConfigurationOption[] OperationOptions { get; init; }
    public required Action<string> SetFormTypeName { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetFormDialogVisible { get; init; }
    public required Action<bool> SetDesignerVisible { get; init; }
    public required Action<bool> SetDetailDialogVisible { get; init; }
    public required Action<bool> SetGraphJsonVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action<bool> SetContractDialogVisible { get; init; }
    public required Action<string?> SetEditCode { get; init; }
    public required Action<string?> SetEditName { get; init; }
    public required Action<string?> SetEditRemark { get; init; }
    public required Action<string?> SetFormEntityName { get; init; }
    public required Action<string?> SetSelectedText { get; init; }
    public required Action CloseEditDialog { get; init; }
    public required Action SaveBasic { get; init; }
    public required Action CloseFormDialog { get; init; }
    public required Action SaveFormConfig { get; init; }
    public required Action FitDesigner { get; init; }
    public required Action CenterDesigner { get; init; }
    public required Action ZoomInDesigner { get; init; }
    public required Action ZoomOutDesigner { get; init; }
    public required Action ResetDesignerZoom { get; init; }
    public required Action UndoDesigner { get; init; }
    public required Action RedoDesigner { get; init; }
    public required Action ToggleSelectionMode { get; init; }
    public required Action ShowGraphJson { get; init; }
    public required Action ApplySelectedText { get; init; }
    public required Action InlineEditSelected { get; init; }
    public required Action FocusSelected { get; init; }
    public required Action CloseDesigner { get; init; }
    public required Action SaveDesigner { get; init; }
    public required Action CloseDetailDialog { get; init; }
    public required Action ResetDesignerGraph { get; init; }
    public required Action ClearDesignerGraph { get; init; }
    public required Action DeleteSelected { get; init; }
    public required Action CleanupDesigner { get; init; }
    public required Action CloseContractDialog { get; init; }
    public required string DetailCodeText { get; init; }
    public required string DetailNameText { get; init; }
    public required string DetailRemarkText { get; init; }
    public required string DetailCreateTimeText { get; init; }
    public required string DetailUpdateTimeText { get; init; }
    public required string DetailFormConfigIdText { get; init; }
    public required string DetailFormTableNameText { get; init; }
    public required string DetailFormEntityNameText { get; init; }
    public required string DetailFormTypeNameText { get; init; }
    public required string DetailFormRouteText { get; init; }
    public required string ContractFlowNameText { get; init; }
    public required string DetailCreateUserNameText { get; init; }
    public required string DetailUpdateUserNameText { get; init; }
    public required string DetailStatusText { get; init; }
    public required int DesignerNodeCount { get; init; }
    public required int DesignerEdgeCount { get; init; }
    public required int DesignerStartCount { get; init; }
    public required int DesignerEndCount { get; init; }
    public required int DesignerUserCount { get; init; }
    public required int DesignerServiceCount { get; init; }
    public required int DesignerGatewayCount { get; init; }
    public required string[] DesignerIssues { get; init; }
    public required int DetailNodeCount { get; init; }
    public required int DetailEdgeCount { get; init; }
    public required ApprovalFlowNodeRow[] DetailNodes { get; init; }
    public required string SelectedId { get; init; }
    public required string SelectedKind { get; init; }
    public required string SelectedSourceNodeId { get; init; }
    public required string SelectedTargetNodeId { get; init; }
    public required string SelectedX { get; init; }
    public required string SelectedY { get; init; }
    public required string CanvasId { get; init; }
    public required bool DesignerReady { get; init; }
    public required string SelectedTypeLabel { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditCode { get; init; }
    public required string EditName { get; init; }
    public required string EditRemark { get; init; }
    public required bool EditLoading { get; init; }
    public required bool Saving { get; init; }
    public required bool FormDialogVisible { get; init; }
    public required string FormConfigId { get; init; }
    public required string FormTableName { get; init; }
    public required string FormEntityName { get; init; }
    public required string FormTypeName { get; init; }
    public required bool FormLoading { get; init; }
    public required bool TableLoading { get; init; }
    public required bool FormSaving { get; init; }
    public required bool FormRouteConflict { get; init; }
    public required string FormRoutePreview { get; init; }
    public required bool DesignerDialogVisible { get; init; }
    public required bool SelectionMode { get; init; }
    public required bool CanUndo { get; init; }
    public required bool CanRedo { get; init; }
    public required string DesignerWarning { get; init; }
    public required string DesignerError { get; init; }
    public required bool DesignerLoading { get; init; }
    public required bool DesignerSaving { get; init; }
    public required string SelectedText { get; init; }
    public required bool GraphJsonVisible { get; init; }
    public required string GraphJsonText { get; init; }
    public required bool DetailDialogVisible { get; init; }
    public required string DetailParseError { get; init; }
    public required string DetailFormJson { get; init; }
    public required string DetailFlowJson { get; init; }
    public required bool DetailLoading { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required bool ContractDialogVisible { get; init; }
    public required string ContractDialogTitle { get; init; }
    public required string ContractDialogText { get; init; }
}


public sealed record ApprovalOperationOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}
