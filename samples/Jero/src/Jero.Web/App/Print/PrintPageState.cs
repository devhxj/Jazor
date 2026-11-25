using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Print;

[ECMAScriptModule("components/print-page-state.js")]
public static class PrintPageModule
{
    public static PrintPageState Create(Func<IPromise<HiprintExports>> loadHiprint) => new(loadHiprint);

    public sealed class PrintPageState : global::Jero.Web.App.IPageState<PrintPageProps>
    {
        private readonly Func<IPromise<HiprintExports>> loadHiprint;

        public PrintPageState(Func<IPromise<HiprintExports>> loadHiprint) => this.loadHiprint = loadHiprint;

        private readonly double[] pageSizes = [10, 20, 50, 100];
        private readonly PrintProviderOption[] providerOptions =
        [
            new() { value = 0, label = "A设计" },
            new() { value = 1, label = "B设计" }
        ];
        private readonly PrintPaperOption[] paperOptions =
        [
            new() { type = "A3", width = 420, height = 296.6 },
            new() { type = "A4", width = 210, height = 296.6 },
            new() { type = "A5", width = 210, height = 147.6 },
            new() { type = "B3", width = 500, height = 352.6 },
            new() { type = "B4", width = 250, height = 352.6 },
            new() { type = "B5", width = 250, height = 175.6 },
            new() { type = "4R", width = 152, height = 102 },
            new() { type = "6R", width = 203, height = 152 }
        ];

        private PrintView[] items = [];
        private PrintView[] rows = [];
        private PrintTenantOption[] tenantItems = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string tenantId = string.Empty;
        private string name = string.Empty;
        private bool loading;
        private bool tenantLoading;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增打印模板";
        private bool saving;
        private PrintView? editing;
        private string editTenantId = string.Empty;
        private string editName = string.Empty;
        private int editOrderNo = 100;
        private int editStatus = 1;
        private int editPrintType = 1;
        private string editClientServiceAddress = string.Empty;
        private string editPrintParam = PrintShared.DefaultPrintParam;
        private string editRemark = string.Empty;
        private string editPrintDataDemo = PrintShared.DefaultPrintDataDemo;
        private string editTemplate = string.Empty;

        private HiprintDesignerSession? designer;
        private bool designerLoading;
        private int designerMountVersion;
        private string modeValue = "0";
        private string paperType = "A4";
        private double paperWidth = 210;
        private double paperHeight = 296.6;
        private double customPaperWidth = 220;
        private double customPaperHeight = 80;
        private double scaleValue = 1;
        private string designerTab = "properties";
        private bool jsonDialogVisible;
        private string jsonText = string.Empty;
        private bool previewDialogVisible;
        private bool printing;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysPrint:add");
        private bool CanEdit => ApplicationState.HasPermission("sysPrint:update");
        private bool CanDelete => ApplicationState.HasPermission("sysPrint:delete");
        private bool HasActionColumn => CanEdit || CanDelete;
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;
        private bool DesignerReady => designer is not null && !designerLoading;

        public void Initialize()
        {
            OnBeforeUnmount(ReleasePage);
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadPage();
        }

        private void ReleasePage()
        {
            CleanupDesigner();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            PrintClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }
                tenantItems = PrintClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            loading = true;
            PrintClient.Page(new PrintPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                name = name.Trim(),
                tenantId = IsSuperAdmin ? tenantId : string.Empty
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载打印模板失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }
                items = PrintClient.ReadItems(outcome.Data);
                rows = items;
                total = PrintClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载打印模板失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            tenantId = DefaultTenantId;
            name = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void TenantChanged(string? value)
        {
            tenantId = value ?? string.Empty;
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

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增打印模板");
                return;
            }

            editing = null;
            editDialogTitle = "新增打印模板";
            editTenantId = targetTenantId;
            editName = string.Empty;
            editOrderNo = 100;
            editStatus = 1;
            editPrintType = 1;
            editClientServiceAddress = string.Empty;
            editPrintParam = PrintShared.DefaultPrintParam;
            editRemark = string.Empty;
            editPrintDataDemo = PrintShared.DefaultPrintDataDemo;
            editTemplate = string.Empty;
            modeValue = "0";
            ResetDesignerControls();
            error = null;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            editing = row;
            editDialogTitle = "编辑打印模板";
            editTenantId = row.tenantId;
            editName = row.name;
            editOrderNo = row.orderNo;
            editStatus = row.status == 2 ? 2 : 1;
            editPrintType = row.printType == 2 ? 2 : 1;
            editClientServiceAddress = row.clientServiceAddress;
            editPrintParam = row.printParam.Length == 0 ? PrintShared.DefaultPrintParam : PrintShared.FormatJsonText(row.printParam);
            editRemark = row.remark;
            editPrintDataDemo = row.printDataDemo.Length == 0 ? PrintShared.DefaultPrintDataDemo : PrintShared.FormatJsonText(row.printDataDemo);
            editTemplate = row.template;
            modeValue = PrintShared.ReadTemplateModeIndex(row.template).ToString();
            ResetDesignerControls();
            error = null;
            editDialogVisible = true;
        }

        private void InitializeDesigner()
            => RebuildDesigner(editTemplate);

        private void RebuildDesigner(string template)
        {
            var version = ++designerMountVersion;
            designerLoading = true;
            loadHiprint().Then(exports =>
            {
                // Hiprint queues document.ready auto-connect. Disable it before returning the runtime.
                exports.DisableAutoConnect();
                return exports.Default;
            }).Then(runtime =>
            {
                // Dialog close/unmount invalidates this load before it can read or replace DOM.
                if (version != designerMountVersion || !editDialogVisible)
                    return;
                designer = PrintBrowserActions.CreateDesigner(runtime, ParseMode(), editClientServiceAddress, template);
                ApplyDesignerState();
                designerLoading = false;
                error = null;
            }).Catch(exception =>
            {
                if (version != designerMountVersion || !editDialogVisible)
                    return;
                designer = null;
                designerLoading = false;
                error = "打印设计器初始化失败：" + exception.Message;
            });
        }

        private void ModeChanged(string? value)
        {
            var next = value == "1" ? "1" : "0";
            if (next == modeValue)
                return;
            var template = designer is null ? editTemplate : PrintBrowserActions.GetTemplateJson(designer, ParseMode());
            modeValue = next;
            RebuildDesigner(template);
        }

        private void PaperChanged(string? value)
        {
            if (designer is null)
                return;
            var option = FindPaper(value ?? "A4");
            if (option is null)
                return;
            paperType = option.type;
            paperWidth = option.width;
            paperHeight = option.height;
            PrintBrowserActions.SetPaper(designer, paperWidth, paperHeight);
        }

        private void ApplyCustomPaper()
        {
            if (designer is null || customPaperWidth <= 0 || customPaperHeight <= 0)
            {
                ElementPlus.ElMessageMembers.Warning("自定义纸张宽高必须大于 0");
                return;
            }
            paperType = "other";
            paperWidth = customPaperWidth;
            paperHeight = customPaperHeight;
            PrintBrowserActions.SetPaper(designer, paperWidth, paperHeight);
        }

        private void ScaleChanged(double? value)
        {
            if (designer is null)
                return;
            scaleValue = PrintBrowserActions.SetScale(designer, value ?? 1);
        }

        private void RotatePaper()
        {
            if (designer is null)
                return;
            PrintBrowserActions.RotatePaper(designer);
            paperWidth = designer.PaperWidth;
            paperHeight = designer.PaperHeight;
            paperType = MatchPaper(paperWidth, paperHeight);
        }

        private void Align(string alignment)
        {
            if (designer is not null)
                PrintBrowserActions.Align(designer, alignment);
        }

        private void ClearPaper()
        {
            if (designer is not null)
                PrintBrowserActions.ClearPaper(designer);
        }

        private void FormatPrintData()
        {
            var outcome = PrintShared.ValidateJsonObjectText(editPrintDataDemo, "打印测试数据");
            if (!outcome.Ok)
            {
                ElementPlus.ElMessageMembers.Error(outcome.Error ?? "打印测试数据不是有效 JSON");
                return;
            }
            editPrintDataDemo = outcome.Value.Length == 0 ? PrintShared.DefaultPrintDataDemo : outcome.Value;
        }

        private void ShowTemplateJson()
        {
            if (designer is null)
                return;
            jsonText = PrintBrowserActions.GetTemplateJson(designer, ParseMode());
            jsonDialogVisible = true;
        }

        private void OpenPreview()
        {
            if (!EnsureDesignerHost() || !ValidatePrintJson())
                return;
            previewDialogVisible = true;
            RenderPreview();
        }

        private void RenderPreview()
        {
            if (designer is not null)
                PrintBrowserActions.RenderPreview(designer, editPrintDataDemo);
        }

        private void PrintDirectly()
        {
            if (printing || !EnsureDesignerHost() || !ValidatePrintJson() || designer is null)
                return;

            printing = true;
            try
            {
                if (editPrintType == 2)
                {
                    var clientError = PrintBrowserActions.PrintWithClient(
                        designer,
                        editPrintDataDemo,
                        editPrintParam,
                        () => printing = false,
                        message =>
                        {
                            printing = false;
                            ElementPlus.ElMessageMembers.Error("客户端打印失败：" + message);
                        });
                    if (clientError.Length > 0)
                    {
                        printing = false;
                        ElementPlus.ElMessageMembers.Error(clientError);
                    }
                    return;
                }

                PrintBrowserActions.PrintInBrowser(designer, editPrintDataDemo, () => printing = false);
            }
            catch (Exception exception)
            {
                printing = false;
                ElementPlus.ElMessageMembers.Error("打印失败：" + exception.Message);
            }
        }

        private void ExportPdf()
        {
            if (!EnsureDesignerHost() || !ValidatePrintJson() || designer is null)
                return;
            try
            {
                PrintBrowserActions.ExportPdf(designer, editPrintDataDemo);
            }
            catch (Exception exception)
            {
                ElementPlus.ElMessageMembers.Error("导出 PDF 失败：" + exception.Message);
            }
        }

        private bool EnsureDesignerHost()
        {
            if (designer is null)
            {
                ElementPlus.ElMessageMembers.Warning("打印设计器尚未完成初始化");
                return false;
            }
            var host = editClientServiceAddress.Trim();
            if (designer.Host == host)
                return true;

            var template = PrintBrowserActions.GetTemplateJson(designer, ParseMode());
            RebuildDesigner(template);
            return designer is not null;
        }

        private bool ValidatePrintJson()
        {
            var dataOutcome = PrintShared.ValidateJsonObjectText(editPrintDataDemo, "打印测试数据");
            if (!dataOutcome.Ok)
            {
                ElementPlus.ElMessageMembers.Error(dataOutcome.Error ?? "打印测试数据不是有效 JSON");
                return false;
            }
            var paramOutcome = PrintShared.ValidateJsonObjectText(editPrintParam, "打印参数");
            if (!paramOutcome.Ok)
            {
                ElementPlus.ElMessageMembers.Error(paramOutcome.Error ?? "打印参数不是有效 JSON");
                return false;
            }
            editPrintDataDemo = dataOutcome.Value.Length == 0 ? PrintShared.DefaultPrintDataDemo : dataOutcome.Value;
            editPrintParam = paramOutcome.Value.Length == 0 ? PrintShared.DefaultPrintParam : paramOutcome.Value;
            return true;
        }

        private void Save()
        {
            if (saving || designer is null)
                return;

            var normalizedName = editName.Trim();
            if (normalizedName.Length == 0)
            {
                error = "模板名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                error = "模板名称不能超过64个字符。";
                return;
            }
            if (editClientServiceAddress.Trim().Length > 128)
            {
                error = "客户端服务地址不能超过128个字符。";
                return;
            }
            if (editRemark.Trim().Length > 128)
            {
                error = "备注不能超过128个字符。";
                return;
            }
            if (!ValidatePrintJson())
                return;

            var templateOutcome = PrintShared.NormalizeTemplateJson(
                PrintBrowserActions.GetTemplateJson(designer, ParseMode()),
                ParseMode());
            if (!templateOutcome.Ok)
            {
                error = templateOutcome.Error ?? "打印模板无效。";
                return;
            }

            saving = true;
            var request = editing is null
                ? PrintClient.Add(new PrintAddInput
                {
                    tenantId = PrintShared.Optional(editTenantId),
                    name = normalizedName,
                    template = templateOutcome.Value,
                    printType = editPrintType == 2 ? 2 : 1,
                    clientServiceAddress = PrintShared.Optional(editClientServiceAddress),
                    printParam = editPrintParam,
                    orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                    status = editStatus == 2 ? 2 : 1,
                    remark = PrintShared.Optional(editRemark),
                    printDataDemo = editPrintDataDemo
                })
                : PrintClient.Update(new PrintUpdateInput
                {
                    id = editing.id,
                    tenantId = PrintShared.Optional(editTenantId),
                    name = normalizedName,
                    template = templateOutcome.Value,
                    printType = editPrintType == 2 ? 2 : 1,
                    clientServiceAddress = PrintShared.Optional(editClientServiceAddress),
                    printParam = editPrintParam,
                    orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                    status = editStatus == 2 ? 2 : 1,
                    remark = PrintShared.Optional(editRemark),
                    printDataDemo = editPrintDataDemo,
                    createTime = editing.createTime,
                    createUserId = PrintShared.Optional(editing.createUserId),
                    createUserName = PrintShared.Optional(editing.createUserName),
                    updateTime = PrintShared.Optional(editing.updateTime),
                    updateUserId = PrintShared.Optional(editing.updateUserId),
                    updateUserName = PrintShared.Optional(editing.updateUserName)
                });

            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存打印模板失败。";
                    return;
                }
                CloseEditor();
                error = null;
                ElementPlus.ElMessageMembers.Success(editing is null ? "打印模板已新增" : "打印模板已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存打印模板失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            PrintClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除打印模板失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("打印模板已删除");
                LoadPage();
            }, _ => error = "删除打印模板失败，请检查网络后重试。");
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
            auditDialogVisible = true;
        }

        private void CloseEditor()
        {
            editDialogVisible = false;
            previewDialogVisible = false;
            jsonDialogVisible = false;
            printing = false;
            CleanupDesigner();
        }

        private void CleanupDesigner()
        {
            ++designerMountVersion;
            PrintBrowserActions.ClearDesigner(designer);
            designer = null;
            designerLoading = false;
        }

        private void ResetDesignerControls()
        {
            CleanupDesigner();
            paperType = "A4";
            paperWidth = 210;
            paperHeight = 296.6;
            customPaperWidth = 220;
            customPaperHeight = 80;
            scaleValue = 1;
            designerTab = "properties";
            previewDialogVisible = false;
            jsonDialogVisible = false;
            printing = false;
        }

        private void ApplyDesignerState()
        {
            if (designer is null)
                return;
            paperWidth = designer.PaperWidth;
            paperHeight = designer.PaperHeight;
            customPaperWidth = paperWidth;
            customPaperHeight = paperHeight;
            scaleValue = designer.Scale;
            paperType = MatchPaper(paperWidth, paperHeight);
        }

        private int ParseMode() => modeValue == "1" ? 1 : 0;

        private PrintPaperOption? FindPaper(string type)
        {
            for (var index = 0; index < paperOptions.Length; index++)
                if (paperOptions[index].type == type)
                    return paperOptions[index];
            return null;
        }

        private string MatchPaper(double width, double height)
        {
            for (var index = 0; index < paperOptions.Length; index++)
                if (paperOptions[index].width == width && paperOptions[index].height == height)
                    return paperOptions[index].type;
            return "other";
        }

        private PrintView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private PrintTableRow[] TableRows()
        {
            var result = new PrintTableRow[rows.Length];
            for (var index = 0; index < rows.Length; index++)
            {
                var row = rows[index];
                result[index] = new PrintTableRow(row.id, row.name, row.printType, row.status, row.orderNo,
                    DisplayValue(row.clientServiceAddress ?? ""), DisplayValue(row.remark ?? ""));
            }
            return result;
        }

        private ConfigurationOption[] TenantOptionsForView()
        {
            var result = new ConfigurationOption[tenantItems.Length];
            for (var index = 0; index < tenantItems.Length; index++)
                result[index] = new ConfigurationOption(tenantItems[index].value, tenantItems[index].label);
            return result;
        }

        private ConfigurationOption[] ProviderOptionsForView()
        {
            var result = new ConfigurationOption[providerOptions.Length];
            for (var index = 0; index < providerOptions.Length; index++)
                result[index] = new ConfigurationOption(providerOptions[index].value.ToString(), providerOptions[index].label);
            return result;
        }

        private ConfigurationOption[] PaperOptionsForView()
        {
            var result = new ConfigurationOption[paperOptions.Length + 1];
            for (var index = 0; index < paperOptions.Length; index++)
                result[index] = new ConfigurationOption(paperOptions[index].type, paperOptions[index].type);
            result[paperOptions.Length] = new ConfigurationOption("other", "自定义");
            return result;
        }

        private static string DisplayUser(string name, string id)
        {
            if (name.Length > 0 && id.Length > 0)
                return name + " (" + id + ")";
            if (name.Length > 0)
                return name;
            return DisplayValue(id);
        }

        private static string DisplayValue(string value) => value.Length == 0 ? "-" : value;

        public PrintPageProps CreateProps() => new()
        {
            IsSuperAdmin = IsSuperAdmin,
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            HasActionColumn = HasActionColumn,
            DesignerReady = DesignerReady,
            Loading = loading,
            TenantLoading = tenantLoading,
            EditDialogVisible = editDialogVisible,
            Saving = saving,
            DesignerLoading = designerLoading,
            PreviewDialogVisible = previewDialogVisible,
            Printing = printing,
            JsonDialogVisible = jsonDialogVisible,
            AuditDialogVisible = auditDialogVisible,
            TenantId = tenantId,
            Name = name,
            EditDialogTitle = editDialogTitle,
            EditName = editName,
            EditClientServiceAddress = editClientServiceAddress,
            EditPrintParam = editPrintParam,
            EditRemark = editRemark,
            ModeValue = modeValue,
            PaperType = paperType,
            DesignerTab = designerTab,
            EditPrintDataDemo = editPrintDataDemo,
            JsonText = jsonText,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            EditOrderNo = editOrderNo,
            EditStatus = editStatus,
            EditPrintType = editPrintType,
            CustomPaperWidth = customPaperWidth,
            CustomPaperHeight = customPaperHeight,
            ScaleValue = scaleValue,
            Error = error,
            TenantOptions = TenantOptionsForView(),
            ProviderOptions = ProviderOptionsForView(),
            PaperOptions = PaperOptionsForView(),
            Rows = TableRows(),
            PageSizes = pageSizes,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPage = LoadPage,
            InitializeDesigner = InitializeDesigner,
            CleanupDesigner = CleanupDesigner,
            ApplyCustomPaper = ApplyCustomPaper,
            RotatePaper = RotatePaper,
            OpenPreview = OpenPreview,
            ClearPaper = ClearPaper,
            ShowTemplateJson = ShowTemplateJson,
            FormatPrintData = FormatPrintData,
            CloseEditor = CloseEditor,
            Save = Save,
            PrintDirectly = PrintDirectly,
            ExportPdf = ExportPdf,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            Delete = Delete,
            TenantChanged = TenantChanged,
            ModeChanged = ModeChanged,
            PaperChanged = PaperChanged,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            SetName = value => name = value ?? "",
            SetEditName = value => editName = value ?? "",
            SetEditClientServiceAddress = value => editClientServiceAddress = value ?? "",
            SetEditPrintParam = value => editPrintParam = value ?? "",
            SetEditRemark = value => editRemark = value ?? "",
            SetEditPrintDataDemo = value => editPrintDataDemo = value ?? "",
            SetEditOrderNo = value => editOrderNo = (int)(value ?? 100),
            SetEditStatus = value => editStatus = (int)value,
            SetEditPrintType = value => editPrintType = (int)value,
            SetCustomPaperWidth = value => customPaperWidth = value ?? 220,
            SetCustomPaperHeight = value => customPaperHeight = value ?? 80,
            ScaleChanged = ScaleChanged,
            DismissError = () => error = null,
            SetEditDialogVisible = value => { if (!value) CloseEditor(); },
            SetPreviewDialogVisible = value => previewDialogVisible = value,
            SetJsonDialogVisible = value => jsonDialogVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            ClosePreviewDialog = () => previewDialogVisible = false,
            CloseJsonDialog = () => jsonDialogVisible = false,
            AlignLeft = () => Align("left"),
            AlignVertical = () => Align("vertical"),
            AlignRight = () => Align("right"),
            AlignTop = () => Align("top"),
            AlignHorizontal = () => Align("horizontal"),
            AlignBottom = () => Align("bottom"),
            ShowProperties = () => designerTab = "properties",
            ShowData = () => designerTab = "data",
        };
    }
}

public sealed record PrintPageProps : VueProps
{
    public required bool IsSuperAdmin { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool HasActionColumn { get; init; }
    public required bool DesignerReady { get; init; }
    public required bool Loading { get; init; }
    public required bool TenantLoading { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool Saving { get; init; }
    public required bool DesignerLoading { get; init; }
    public required bool PreviewDialogVisible { get; init; }
    public required bool Printing { get; init; }
    public required bool JsonDialogVisible { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required string TenantId { get; init; }
    public required string Name { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditName { get; init; }
    public required string EditClientServiceAddress { get; init; }
    public required string EditPrintParam { get; init; }
    public required string EditRemark { get; init; }
    public required string ModeValue { get; init; }
    public required string PaperType { get; init; }
    public required string DesignerTab { get; init; }
    public required string EditPrintDataDemo { get; init; }
    public required string JsonText { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required int EditOrderNo { get; init; }
    public required int EditStatus { get; init; }
    public required int EditPrintType { get; init; }
    public required double CustomPaperWidth { get; init; }
    public required double CustomPaperHeight { get; init; }
    public required double ScaleValue { get; init; }
    public required string? Error { get; init; }
    public required ConfigurationOption[] TenantOptions { get; init; }
    public required ConfigurationOption[] ProviderOptions { get; init; }
    public required ConfigurationOption[] PaperOptions { get; init; }
    public required PrintTableRow[] Rows { get; init; }
    public required double[] PageSizes { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action LoadPage { get; init; }
    public required Action InitializeDesigner { get; init; }
    public required Action CleanupDesigner { get; init; }
    public required Action ApplyCustomPaper { get; init; }
    public required Action RotatePaper { get; init; }
    public required Action OpenPreview { get; init; }
    public required Action ClearPaper { get; init; }
    public required Action ShowTemplateJson { get; init; }
    public required Action FormatPrintData { get; init; }
    public required Action CloseEditor { get; init; }
    public required Action Save { get; init; }
    public required Action PrintDirectly { get; init; }
    public required Action ExportPdf { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string?> TenantChanged { get; init; }
    public required Action<string?> ModeChanged { get; init; }
    public required Action<string?> PaperChanged { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action<string?> SetName { get; init; }
    public required Action<string?> SetEditName { get; init; }
    public required Action<string?> SetEditClientServiceAddress { get; init; }
    public required Action<string?> SetEditPrintParam { get; init; }
    public required Action<string?> SetEditRemark { get; init; }
    public required Action<string?> SetEditPrintDataDemo { get; init; }
    public required Action<double?> SetEditOrderNo { get; init; }
    public required Action<double> SetEditStatus { get; init; }
    public required Action<double> SetEditPrintType { get; init; }
    public required Action<double?> SetCustomPaperWidth { get; init; }
    public required Action<double?> SetCustomPaperHeight { get; init; }
    public required Action<double?> ScaleChanged { get; init; }
    public required Action DismissError { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetPreviewDialogVisible { get; init; }
    public required Action<bool> SetJsonDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action ClosePreviewDialog { get; init; }
    public required Action CloseJsonDialog { get; init; }
    public required Action AlignLeft { get; init; }
    public required Action AlignVertical { get; init; }
    public required Action AlignRight { get; init; }
    public required Action AlignTop { get; init; }
    public required Action AlignHorizontal { get; init; }
    public required Action AlignBottom { get; init; }
    public required Action ShowProperties { get; init; }
    public required Action ShowData { get; init; }
}
