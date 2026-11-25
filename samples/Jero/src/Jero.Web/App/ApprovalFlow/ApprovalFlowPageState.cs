using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.ApprovalFlow;

[ECMAScriptModule("components/approval-flow-page-state.js")]
public static class ApprovalFlowPageModule
{
    public static ApprovalFlowPageState Create() => new();

    public sealed class ApprovalFlowPageState : global::Jero.Web.App.IPageState<ApprovalFlowPageProps>
    {
        private const int EditRequest = 1;
        private const int DetailRequest = 2;
        private const int FormRequest = 3;
        private const int DesignerRequest = 4;
        private readonly double[] pageSizes = [10, 20, 50, 100];
        private readonly ApprovalOperationOption[] operationOptions =
        [
            new() { value = "add", label = "新增" },
            new() { value = "update", label = "更新" },
            new() { value = "delete", label = "删除" },
            new() { value = "select", label = "查询" },
            new() { value = "export", label = "导出" }
        ];

        private ApprovalFlowView[] items = [];
        private ApprovalFlowView[] rows = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string queryKeyword = string.Empty;
        private string queryCode = string.Empty;
        private string queryName = string.Empty;
        private string queryRemark = string.Empty;
        private bool loading;
        private string? error;

        private bool editDialogVisible;
        private bool editLoading;
        private bool saving;
        private string editDialogTitle = "新增审批流";
        private ApprovalFlowView? editing;
        private string editCode = string.Empty;
        private string editName = string.Empty;
        private string editRemark = string.Empty;

        private bool detailDialogVisible;
        private bool detailLoading;
        private ApprovalFlowView? detail;
        private ApprovalFormConfig detailForm = new();
        private ApprovalFlowGraph detailGraph = new();
        private ApprovalFlowSummary detailSummary = new();
        private string detailParseError = string.Empty;
        private string detailFormJson = "暂无表单 JSON";
        private string detailFlowJson = "暂无流程 JSON";

        private bool formDialogVisible;
        private bool formLoading;
        private bool formSaving;
        private bool tableLoading;
        private ApprovalFlowView? formSource;
        private ApprovalDatabaseOption[] databaseOptions = [];
        private ApprovalTableOption[] tableOptions = [];
        private string[] routeOptions = [];
        private string originalRoute = string.Empty;
        private string formConfigId = string.Empty;
        private string formTableName = string.Empty;
        private string formEntityName = string.Empty;
        private string formTypeName = string.Empty;

        private bool designerDialogVisible;
        private bool designerLoading;
        private bool designerSaving;
        private ApprovalFlowView? designerSource;
        private ApprovalFlowGraph designerGraph = ApprovalFlowShared.CreateDefaultGraph();
        private ApprovalFlowSummary designerSummary = new();
        private ApprovalFlowDesignerSession? designer;
        private string designerWarning = string.Empty;
        private string designerError = string.Empty;
        private bool selectionMode;
        private bool canUndo;
        private bool canRedo;
        private ApprovalFlowSelection selected = new();
        private string selectedText = string.Empty;
        private bool graphJsonVisible;
        private string graphJsonText = string.Empty;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

        private bool contractDialogVisible;
        private string contractDialogTitle = "流程运行契约";
        private string contractDialogText = string.Empty;
        private string contractFlowName = string.Empty;

        private bool disposed;
        private int pageGeneration;
        private int editGeneration;
        private int detailGeneration;
        private int formGeneration;
        private int designerGeneration;

        private string FormRoutePreview => ApprovalFlowShared.PreviewRoute(formEntityName, formTypeName);
        private bool FormRouteConflict => HasRouteConflict(FormRoutePreview);

        public void Initialize()
        {
            disposed = false;
            // Release owned browser resources before Vue removes this page DOM.
            OnBeforeUnmount(Dispose);
            LoadPage();
        }

        public void Dispose()
        {
            DisposePage();
        }

        private void DisposePage()
        {
            if (disposed)
                return;
            disposed = true;
            InvalidatePendingWork();
            CleanupDesigner();
        }

        private void InvalidatePendingWork()
        {
            pageGeneration++;
            editGeneration++;
            detailGeneration++;
            formGeneration++;
            designerGeneration++;
            editDialogVisible = false;
            detailDialogVisible = false;
            formDialogVisible = false;
            designerDialogVisible = false;
            graphJsonVisible = false;
        }

        private void LoadPage()
        {
            if (disposed)
                return;
            var generation = ++pageGeneration;
            loading = true;
            ApprovalFlowClient.Page(new ApprovalFlowPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                keyword = queryKeyword.Trim(),
                code = queryCode.Trim(),
                name = queryName.Trim(),
                remark = queryRemark.Trim()
            }).Then(outcome =>
            {
                if (disposed || generation != pageGeneration)
                    return;
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载审批流程失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }
                items = ApprovalFlowClient.ReadItems(outcome.Data);
                rows = items;
                total = ApprovalFlowClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                if (disposed || generation != pageGeneration)
                    return;
                loading = false;
                error = "加载审批流程失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            queryKeyword = string.Empty;
            queryCode = string.Empty;
            queryName = string.Empty;
            queryRemark = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void ChangePage(Number value)
        {
            currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(Number value)
        {
            pageSize = (int)value;
            currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            editGeneration++;
            editing = null;
            editDialogTitle = "新增审批流";
            editCode = string.Empty;
            editName = string.Empty;
            editRemark = string.Empty;
            editLoading = false;
            error = null;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            if (id.Length == 0)
                return;
            var generation = ++editGeneration;
            editDialogTitle = "编辑审批流";
            editDialogVisible = true;
            editLoading = true;
            LoadDetail(id, EditRequest, generation, loaded =>
            {
                editLoading = false;
                editing = loaded;
                editCode = loaded.code;
                editName = loaded.name;
                editRemark = loaded.remark;
            }, "加载审批流详情失败。");
        }

        private void CloseEditDialog()
        {
            editGeneration++;
            editDialogVisible = false;
            editLoading = false;
        }

        private void SaveBasic()
        {
            if (saving || editLoading)
                return;
            var name = editName.Trim();
            var code = editCode.Trim();
            var remark = editRemark.Trim();
            if (name.Length == 0)
            {
                error = "名称不能为空。";
                return;
            }
            if (name.Length > 32)
            {
                error = "名称不能超过32个字符。";
                return;
            }
            if (code.Length > 32)
            {
                error = "编号不能超过32个字符。";
                return;
            }
            if (remark.Length > 256)
            {
                error = "备注不能超过256个字符。";
                return;
            }

            var creating = editing is null;
            var request = creating
                ? ApprovalFlowClient.Add(new ApprovalFlowAddInput
                {
                    code = ApprovalFlowShared.Optional(code),
                    name = name,
                    formJson = null,
                    flowJson = null,
                    remark = ApprovalFlowShared.Optional(remark),
                    isDelete = false
                })
                : ApprovalFlowClient.Update(BuildUpdate(
                    editing!,
                    code,
                    name,
                    editing!.formJson,
                    editing!.flowJson,
                    remark));

            var generation = editGeneration;
            saving = true;
            request.Then(outcome =>
            {
                if (disposed || generation != editGeneration || !editDialogVisible)
                    return;
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存审批流失败。";
                    return;
                }
                CloseEditDialog();
                error = null;
                ElementPlus.ElMessageMembers.Success(creating ? "审批流已新增" : "审批流基础信息已更新");
                LoadPage();
            }, _ =>
            {
                if (disposed || generation != editGeneration || !editDialogVisible)
                    return;
                saving = false;
                error = "保存审批流失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            if (id.Length == 0)
                return;
            ApprovalFlowClient.Delete(id).Then(outcome =>
            {
                if (disposed)
                    return;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除审批流失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("审批流已删除");
                LoadPage();
            }, _ =>
            {
                if (!disposed)
                    error = "删除审批流失败，请检查网络后重试。";
            });
        }

        private void OpenDetail(string id)
        {
            if (id.Length == 0)
                return;
            var generation = ++detailGeneration;
            detailDialogVisible = true;
            detailLoading = true;
            detailParseError = string.Empty;
            LoadDetail(id, DetailRequest, generation, loaded =>
            {
                detailLoading = false;
                detail = loaded;
                detailFormJson = ApprovalFlowShared.FormatJson(loaded.formJson, "暂无表单 JSON");
                detailFlowJson = ApprovalFlowShared.FormatJson(loaded.flowJson, "暂无流程 JSON");
                try
                {
                    detailForm = ApprovalFlowShared.ParseForm(loaded.formJson);
                }
                catch (Exception exception)
                {
                    detailForm = new ApprovalFormConfig();
                    detailParseError = exception.Message;
                }
                try
                {
                    detailGraph = ApprovalFlowShared.ParseGraph(loaded.flowJson);
                    detailSummary = ApprovalFlowShared.Summarize(detailGraph);
                }
                catch (Exception exception)
                {
                    detailGraph = new ApprovalFlowGraph();
                    detailSummary = new ApprovalFlowSummary();
                    detailParseError = detailParseError.Length == 0 ? exception.Message : detailParseError + "；" + exception.Message;
                }
            }, "加载审批流详情失败。");
        }

        private void CloseDetailDialog()
        {
            detailGeneration++;
            detailDialogVisible = false;
            detailLoading = false;
        }

        private void OpenFormConfig(string id)
        {
            if (id.Length == 0)
                return;
            var generation = ++formGeneration;
            formDialogVisible = true;
            formLoading = true;
            formSource = null;
            databaseOptions = [];
            tableOptions = [];
            routeOptions = [];
            LoadDetail(id, FormRequest, generation, loaded =>
            {
                formSource = loaded;
                try
                {
                    var config = ApprovalFlowShared.ParseForm(loaded.formJson);
                    formConfigId = config.configId;
                    formTableName = config.tableName;
                    formEntityName = config.entityName;
                    formTypeName = config.typeName;
                    originalRoute = config.route;
                }
                catch (Exception exception)
                {
                    formConfigId = string.Empty;
                    formTableName = string.Empty;
                    formEntityName = string.Empty;
                    formTypeName = string.Empty;
                    originalRoute = string.Empty;
                    error = exception.Message;
                }
                LoadFormDependencies(generation);
            }, "加载审批表单配置失败。");
        }

        private void CloseFormDialog()
        {
            formGeneration++;
            formDialogVisible = false;
            formLoading = false;
            tableLoading = false;
        }

        private void LoadFormDependencies(int generation)
        {
            ApprovalFlowClient.Databases().Then(databaseOutcome =>
            {
                if (!IsRequestCurrent(FormRequest, generation))
                    return;
                if (!databaseOutcome.Ok)
                {
                    formLoading = false;
                    error = databaseOutcome.Error ?? "加载库定位器失败。";
                    return;
                }
                databaseOptions = ApprovalFlowClient.ReadDatabases(databaseOutcome.Data);
                ApprovalFlowClient.FormRoutes().Then(routeOutcome =>
                {
                    if (!IsRequestCurrent(FormRequest, generation))
                        return;
                    if (!routeOutcome.Ok)
                    {
                        formLoading = false;
                        error = routeOutcome.Error ?? "加载审批路由失败。";
                        return;
                    }
                    routeOptions = ApprovalFlowClient.ReadStrings(routeOutcome.Data);
                    if (formConfigId.Length == 0)
                    {
                        formLoading = false;
                        return;
                    }
                    LoadTables(formConfigId, true, generation);
                }, _ =>
                {
                    if (!IsRequestCurrent(FormRequest, generation))
                        return;
                    formLoading = false;
                    error = "加载审批路由失败，请检查网络后重试。";
                });
            }, _ =>
            {
                if (!IsRequestCurrent(FormRequest, generation))
                    return;
                formLoading = false;
                error = "加载库定位器失败，请检查网络后重试。";
            });
        }

        private void FormConfigChanged(string? value)
        {
            formConfigId = value ?? string.Empty;
            formTableName = string.Empty;
            formEntityName = string.Empty;
            tableOptions = [];
            if (formConfigId.Length > 0)
                LoadTables(formConfigId, false, formGeneration);
        }

        private void LoadTables(string configId, bool finishFormLoading, int generation)
        {
            tableLoading = true;
            ApprovalFlowClient.Tables(configId).Then(outcome =>
            {
                if (!IsRequestCurrent(FormRequest, generation))
                    return;
                tableLoading = false;
                if (finishFormLoading)
                    formLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载数据库表失败。";
                    tableOptions = [];
                    return;
                }
                tableOptions = ApprovalFlowClient.ReadTables(outcome.Data);
            }, _ =>
            {
                if (!IsRequestCurrent(FormRequest, generation))
                    return;
                tableLoading = false;
                if (finishFormLoading)
                    formLoading = false;
                error = "加载数据库表失败，请检查网络后重试。";
            });
        }

        private void FormTableChanged(string? value)
        {
            formTableName = value ?? string.Empty;
            for (var index = 0; index < tableOptions.Length; index++)
            {
                if (tableOptions[index].tableName != formTableName)
                    continue;
                if (tableOptions[index].entityName.Length > 0)
                    formEntityName = tableOptions[index].entityName;
                return;
            }
        }

        private void SaveFormConfig()
        {
            if (formSaving || formLoading || formSource is null)
                return;
            if (FormRouteConflict)
            {
                error = "表单路由【" + FormRoutePreview + "】已被其他审批流占用。";
                return;
            }

            string formJson;
            try
            {
                formJson = ApprovalFlowShared.StringifyForm(new ApprovalFormConfig
                {
                    configId = formConfigId,
                    tableName = formTableName,
                    entityName = formEntityName,
                    typeName = formTypeName,
                    route = FormRoutePreview
                });
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return;
            }

            formSaving = true;
            var source = formSource;
            var generation = formGeneration;
            ApprovalFlowClient.Update(BuildUpdate(source, source.code, source.name, formJson, source.flowJson, source.remark)).Then(outcome =>
            {
                if (disposed || generation != formGeneration || !formDialogVisible)
                    return;
                formSaving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存审批表单配置失败。";
                    return;
                }
                CloseFormDialog();
                error = null;
                ElementPlus.ElMessageMembers.Success("审批表单配置已保存");
                LoadPage();
            }, _ =>
            {
                if (disposed || generation != formGeneration || !formDialogVisible)
                    return;
                formSaving = false;
                error = "保存审批表单配置失败，请检查网络后重试。";
            });
        }

        private void OpenDesigner(string id)
        {
            if (id.Length == 0)
                return;
            var generation = ++designerGeneration;
            CleanupDesigner();
            designerDialogVisible = true;
            designerLoading = true;
            designerWarning = string.Empty;
            designerError = string.Empty;
            LoadDetail(id, DesignerRequest, generation, loaded =>
            {
                designerSource = loaded;
                try
                {
                    designerGraph = ApprovalFlowShared.ParseGraph(loaded.flowJson);
                }
                catch (Exception exception)
                {
                    designerGraph = ApprovalFlowShared.CreateDefaultGraph();
                    designerWarning = "旧流程 JSON 无法解析，已载入默认模板：" + exception.Message;
                }
                designerSummary = ApprovalFlowShared.Summarize(designerGraph);
                Window.SetTimeout(
                    (TimerHandler)(Delegate)(Action)(() => InitializeDesigner(generation)),
                    0,
                    []);
            }, "加载审批流程设计数据失败。");
        }

        private void InitializeDesigner(int generation)
        {
            if (!IsRequestCurrent(DesignerRequest, generation) || designerSource is null)
                return;
            try
            {
                CleanupDesigner();
                designer = ApprovalFlowDesignerBrowserActions.Create(
                    designerGraph,
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
                designerLoading = false;
                designerError = string.Empty;
                DesignerGraphChanged();
            }
            catch (Exception exception)
            {
                designer = null;
                designerLoading = false;
                designerError = "流程设计器初始化失败：" + exception.Message;
            }
        }

        private void DesignerSelectionChanged(ApprovalFlowSelection value)
        {
            selected = value;
            selectedText = value.text;
        }

        private void DesignerHistoryChanged(bool undo, bool redo)
        {
            canUndo = undo;
            canRedo = redo;
        }

        private void DesignerGraphChanged()
        {
            if (designer is null)
                return;
            try
            {
                designerGraph = ApprovalFlowShared.NormalizeGraph(ApprovalFlowDesignerBrowserActions.GetGraph(designer));
                designerSummary = ApprovalFlowShared.Summarize(designerGraph);
                if (selected.id.Length > 0)
                    DesignerSelectionChanged(ApprovalFlowDesignerBrowserActions.ReadSelection(designer, selected.id));
            }
            catch
            {
                // LogicFlow 拖拽期间会出现短暂中间态，保存时再执行严格结构校验。
            }
        }

        private void ToggleSelectionMode()
        {
            if (designer is null)
                return;
            selectionMode = !selectionMode;
            ApprovalFlowDesignerBrowserActions.SetSelectionMode(designer, selectionMode);
        }

        private void ApplySelectedText()
        {
            if (designer is null || selected.id.Length == 0)
                return;
            ApprovalFlowDesignerBrowserActions.UpdateText(designer, selected.id, selectedText);
            DesignerGraphChanged();
        }

        private void DeleteSelected()
        {
            if (designer is null || selected.id.Length == 0)
                return;
            ApprovalFlowDesignerBrowserActions.DeleteElement(designer, selected.id);
            DesignerSelectionChanged(new ApprovalFlowSelection());
            DesignerGraphChanged();
        }

        private void FocusSelected()
        {
            if (designer is not null && selected.id.Length > 0)
                ApprovalFlowDesignerBrowserActions.Focus(designer, selected.id);
        }

        private void InlineEditSelected()
        {
            if (designer is not null && selected.id.Length > 0)
                ApprovalFlowDesignerBrowserActions.EditText(designer, selected.id);
        }

        private void FitDesigner()
        {
            if (designer is not null)
                ApprovalFlowDesignerBrowserActions.Fit(designer);
        }

        private void CenterDesigner()
        {
            if (designer is not null)
                ApprovalFlowDesignerBrowserActions.Center(designer);
        }

        private void ZoomInDesigner()
        {
            if (designer is not null)
                ApprovalFlowDesignerBrowserActions.ZoomIn(designer);
        }

        private void ZoomOutDesigner()
        {
            if (designer is not null)
                ApprovalFlowDesignerBrowserActions.ZoomOut(designer);
        }

        private void ResetDesignerZoom()
        {
            if (designer is not null)
                ApprovalFlowDesignerBrowserActions.ResetZoom(designer);
        }

        private void UndoDesigner()
        {
            if (designer is not null)
                ApprovalFlowDesignerBrowserActions.Undo(designer);
        }

        private void RedoDesigner()
        {
            if (designer is not null)
                ApprovalFlowDesignerBrowserActions.Redo(designer);
        }

        private void ResetDesignerGraph()
        {
            if (designer is null)
                return;
            designerGraph = ApprovalFlowShared.CreateDefaultGraph();
            ApprovalFlowDesignerBrowserActions.Render(designer, designerGraph);
            FitDesigner();
            DesignerSelectionChanged(new ApprovalFlowSelection());
            DesignerGraphChanged();
        }

        private void ClearDesignerGraph()
        {
            if (designer is null)
                return;
            ApprovalFlowDesignerBrowserActions.Clear(designer);
            designerGraph = new ApprovalFlowGraph();
            DesignerSelectionChanged(new ApprovalFlowSelection());
            DesignerGraphChanged();
        }

        private void ShowGraphJson()
        {
            if (designer is null)
                return;
            try
            {
                graphJsonText = JSON.Stringify(
                    ApprovalFlowShared.NormalizeGraph(ApprovalFlowDesignerBrowserActions.GetGraph(designer)),
                    (object?)null,
                    (Number)2) ?? "{}";
                graphJsonVisible = true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }
        }

        private void SaveDesigner()
        {
            if (designerSaving || designerLoading || designer is null || designerSource is null)
                return;
            string flowJson;
            try
            {
                flowJson = ApprovalFlowShared.StringifyGraph(ApprovalFlowDesignerBrowserActions.GetGraph(designer));
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return;
            }

            designerSaving = true;
            var source = designerSource;
            var generation = designerGeneration;
            ApprovalFlowClient.Update(BuildUpdate(source, source.code, source.name, source.formJson, flowJson, source.remark)).Then(outcome =>
            {
                if (disposed || generation != designerGeneration || !designerDialogVisible)
                    return;
                designerSaving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存审批流程失败。";
                    return;
                }
                CloseDesigner();
                error = null;
                ElementPlus.ElMessageMembers.Success("审批流程已保存");
                LoadPage();
            }, _ =>
            {
                if (disposed || generation != designerGeneration || !designerDialogVisible)
                    return;
                designerSaving = false;
                error = "保存审批流程失败，请检查网络后重试。";
            });
        }

        private void CloseDesigner()
        {
            designerGeneration++;
            designerDialogVisible = false;
            graphJsonVisible = false;
            CleanupDesigner();
        }

        private void CleanupDesigner()
        {
            ApprovalFlowDesignerBrowserActions.Destroy(designer);
            designer = null;
            designerLoading = false;
            selectionMode = false;
            canUndo = false;
            canRedo = false;
            selected = new ApprovalFlowSelection();
            selectedText = string.Empty;
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;
            auditDialogTitle = "修改记录 · " + DisplayValue(row.name);
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = DisplayValue(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = DisplayValue(row.updateTime);
            auditDialogVisible = true;
        }

        private void OpenPublishContract(string id)
        {
            contractFlowName = Find(id)?.name ?? string.Empty;
            contractDialogTitle = "发布与状态契约";
            contractDialogText = "当前 ApprovalFlowService 没有发布或状态切换端点，Update DTO 也不接收 status。页面保留状态展示入口，但不会发明无效请求。";
            contractDialogVisible = true;
        }

        private void OpenRecordContract(string id)
        {
            contractFlowName = Find(id)?.name ?? string.Empty;
            contractDialogTitle = "审批记录入口";
            contractDialogText = "中间件会写入 ApprovalFlowRecord 与 ApprovalFormRecord，但当前插件没有记录分页、详情或按流程查询 API。记录入口在此明确显示服务端契约缺口。";
            contractDialogVisible = true;
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
                    error = outcome.Error ?? fallbackError;
                    editLoading = false;
                    detailLoading = false;
                    formLoading = false;
                    designerLoading = false;
                    return;
                }
                var loaded = ApprovalFlowClient.ReadDetail(outcome.Data);
                if (loaded is null)
                {
                    error = "审批流详情不存在或已被删除。";
                    editLoading = false;
                    detailLoading = false;
                    formLoading = false;
                    designerLoading = false;
                    return;
                }
                error = null;
                completed(loaded);
            }, _ =>
            {
                if (!IsRequestCurrent(requestKind, generation))
                    return;
                error = fallbackError;
                editLoading = false;
                detailLoading = false;
                formLoading = false;
                designerLoading = false;
            });
        }

        private bool IsRequestCurrent(int requestKind, int generation)
        {
            if (disposed)
                return false;
            if (requestKind == EditRequest)
                return editDialogVisible && generation == editGeneration;
            if (requestKind == DetailRequest)
                return detailDialogVisible && generation == detailGeneration;
            if (requestKind == FormRequest)
                return formDialogVisible && generation == formGeneration;
            if (requestKind == DesignerRequest)
                return designerDialogVisible && generation == designerGeneration;
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
                id = source.id,
                code = ApprovalFlowShared.Optional(code),
                name = name.Trim(),
                formJson = ApprovalFlowShared.Optional(formJson),
                flowJson = ApprovalFlowShared.Optional(flowJson),
                remark = ApprovalFlowShared.Optional(remark),
                isDelete = source.isDelete,
                createTime = ApprovalFlowShared.Optional(source.createTime),
                createUserId = ApprovalFlowShared.Optional(source.createUserId),
                createUserName = ApprovalFlowShared.Optional(source.createUserName),
                createOrgId = ApprovalFlowShared.Optional(source.createOrgId),
                createOrgName = ApprovalFlowShared.Optional(source.createOrgName),
                updateTime = ApprovalFlowShared.Optional(source.updateTime),
                updateUserId = ApprovalFlowShared.Optional(source.updateUserId),
                updateUserName = ApprovalFlowShared.Optional(source.updateUserName)
            };

        private bool HasRouteConflict(string route)
        {
            if (route.Length == 0)
                return false;
            var count = 0;
            for (var index = 0; index < routeOptions.Length; index++)
                if (routeOptions[index] == route)
                    count++;
            return count > (route == originalRoute ? 1 : 0);
        }

        private string RowFormRoute(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null)
                return "-";
            try
            {
                var route = ApprovalFlowShared.ParseForm(row.formJson).route;
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
                var summary = ApprovalFlowShared.SummarizeText(row.flowJson);
                return summary.nodeCount + " 节点 / " + summary.edgeCount + " 连线";
            }
            catch
            {
                return "JSON 异常";
            }
        }

        private string RowStatus(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || row.status < 0)
                return "详情可见";
            return row.status == 1 ? "启用" : row.status == 2 ? "停用" : "状态 " + row.status;
        }

        private string RowStatusTag(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || row.status < 0)
                return "info";
            return row.status == 1 ? "success" : "warning";
        }

        private string DetailStatus()
        {
            if (detail is null || detail.status < 0)
                return "未由当前详情响应提供";
            return detail.status == 1 ? "启用" : detail.status == 2 ? "停用" : "状态 " + detail.status;
        }

        private ApprovalFlowView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
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
            => value.dbNickName.Length == 0 ? value.configId : value.dbNickName + " (" + value.configId + ")";

        private static string TableLabel(ApprovalTableOption value)
            => value.tableComment.Length == 0 ? value.tableName : value.tableName + " [" + value.tableComment + "]";

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
            var result = new ConfigurationOption[databaseOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(databaseOptions[index].configId, DatabaseLabel(databaseOptions[index]));
            return result;
        }

        private ConfigurationOption[] TableOptionsView()
        {
            var result = new ConfigurationOption[tableOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(tableOptions[index].tableName, TableLabel(tableOptions[index]));
            return result;
        }

        private ConfigurationOption[] OperationOptionsView()
        {
            var result = new ConfigurationOption[operationOptions.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var item = operationOptions[index];
                result[index] = new ConfigurationOption(item.value, item.label + " (" + item.value + ")");
            }
            return result;
        }

        private ApprovalFlowNodeRow[] DetailNodesView()
        {
            var result = new ApprovalFlowNodeRow[detailGraph.nodes.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var node = detailGraph.nodes[index];
                result[index] = new ApprovalFlowNodeRow(node.id, node.text?.value ?? ApprovalFlowShared.NodeTypeLabel(node.type), node.x, node.y);
            }
            return result;
        }

        private ApprovalFlowTableRow[] TableRows()
        {
            var result = new ApprovalFlowTableRow[rows.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = rows[index];
                var tag = row.status < 0 ? "info" : row.status == 1 ? "success" : "warning";
                result[index] = new ApprovalFlowTableRow(row.id, row.name, row.code, row.remark ?? string.Empty,
                    RowStatus(row), tag, RowFormRoute(row), RowFlowSummary(row));
            }
            return result;
        }

        public ApprovalFlowPageProps CreateProps() => new()
        {
            QueryKeyword = queryKeyword,
            QueryCode = queryCode,
            QueryName = queryName,
            QueryRemark = queryRemark,
            Error = error,
            Loading = loading,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            PageSizes = pageSizes,
            SetQueryKeyword = value => queryKeyword = value ?? "",
            SetQueryCode = value => queryCode = value ?? "",
            SetQueryName = value => queryName = value ?? "",
            SetQueryRemark = value => queryRemark = value ?? "",
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
            DismissError = () => error = null,
            Rows = TableRows(),
            DatabaseOptions = DatabaseOptionsView(),
            FormConfigChanged = FormConfigChanged,
            TableOptions = TableOptionsView(),
            FormTableChanged = FormTableChanged,
            OperationOptions = OperationOptionsView(),
            SetFormTypeName = value => formTypeName = value,
            SetEditDialogVisible = visible => { if (!visible) CloseEditDialog(); },
            SetFormDialogVisible = visible => { if (!visible) CloseFormDialog(); },
            SetDesignerVisible = visible => { if (!visible) CloseDesigner(); },
            SetDetailDialogVisible = visible => { if (!visible) CloseDetailDialog(); },
            SetGraphJsonVisible = value => graphJsonVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            SetContractDialogVisible = value => contractDialogVisible = value,
            SetEditCode = value => editCode = value ?? "",
            SetEditName = value => editName = value ?? "",
            SetEditRemark = value => editRemark = value ?? "",
            SetFormEntityName = value => formEntityName = value ?? "",
            SetSelectedText = value => selectedText = value ?? "",
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
            CloseContractDialog = () => contractDialogVisible = false,
            DetailCodeText = DisplayValue(detail?.code ?? ""),
            DetailNameText = DisplayValue(detail?.name ?? ""),
            DetailRemarkText = DisplayValue(detail?.remark ?? ""),
            DetailCreateTimeText = DisplayValue(detail?.createTime ?? ""),
            DetailUpdateTimeText = DisplayValue(detail?.updateTime ?? ""),
            DetailFormConfigIdText = DisplayValue(detailForm.configId),
            DetailFormTableNameText = DisplayValue(detailForm.tableName),
            DetailFormEntityNameText = DisplayValue(detailForm.entityName),
            DetailFormTypeNameText = DisplayValue(detailForm.typeName),
            DetailFormRouteText = DisplayValue(detailForm.route),
            ContractFlowNameText = DisplayValue(contractFlowName),
            DetailCreateUserNameText = DisplayUser(detail?.createUserName ?? "", detail?.createUserId ?? ""),
            DetailUpdateUserNameText = DisplayUser(detail?.updateUserName ?? "", detail?.updateUserId ?? ""),
            DetailStatusText = DetailStatus(),
            DesignerNodeCount = designerSummary.nodeCount,
            DesignerEdgeCount = designerSummary.edgeCount,
            DesignerStartCount = designerSummary.startCount,
            DesignerEndCount = designerSummary.endCount,
            DesignerUserCount = designerSummary.userCount,
            DesignerServiceCount = designerSummary.serviceCount,
            DesignerGatewayCount = designerSummary.gatewayCount,
            DesignerIssues = designerSummary.issues,
            DetailNodeCount = detailSummary.nodeCount,
            DetailEdgeCount = detailSummary.edgeCount,
            DetailNodes = DetailNodesView(),
            SelectedId = selected.id,
            SelectedKind = selected.kind,
            SelectedSourceNodeId = selected.sourceNodeId,
            SelectedTargetNodeId = selected.targetNodeId,
            SelectedX = selected.x,
            SelectedY = selected.y,
            CanvasId = ApprovalFlowDesignerBrowserActions.CanvasId,
            DesignerReady = designer is not null,
            SelectedTypeLabel = selected.kind == "edge" ? "流程连线" : ApprovalFlowShared.NodeTypeLabel(selected.type),
            EditDialogVisible = editDialogVisible,
            EditDialogTitle = editDialogTitle,
            EditCode = editCode,
            EditName = editName,
            EditRemark = editRemark,
            EditLoading = editLoading,
            Saving = saving,
            FormDialogVisible = formDialogVisible,
            FormConfigId = formConfigId,
            FormTableName = formTableName,
            FormEntityName = formEntityName,
            FormTypeName = formTypeName,
            FormLoading = formLoading,
            TableLoading = tableLoading,
            FormSaving = formSaving,
            FormRouteConflict = FormRouteConflict,
            FormRoutePreview = FormRoutePreview,
            DesignerDialogVisible = designerDialogVisible,
            SelectionMode = selectionMode,
            CanUndo = canUndo,
            CanRedo = canRedo,
            DesignerWarning = designerWarning,
            DesignerError = designerError,
            DesignerLoading = designerLoading,
            DesignerSaving = designerSaving,
            SelectedText = selectedText,
            GraphJsonVisible = graphJsonVisible,
            GraphJsonText = graphJsonText,
            DetailDialogVisible = detailDialogVisible,
            DetailParseError = detailParseError,
            DetailFormJson = detailFormJson,
            DetailFlowJson = detailFlowJson,
            DetailLoading = detailLoading,
            AuditDialogVisible = auditDialogVisible,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            ContractDialogVisible = contractDialogVisible,
            ContractDialogTitle = contractDialogTitle,
            ContractDialogText = contractDialogText,
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
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
}
