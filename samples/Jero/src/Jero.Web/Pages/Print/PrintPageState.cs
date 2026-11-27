using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Print;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Print;

[ECMAScriptModule("components/print-page-state.js")]
public static class PrintPageModule
{
    public static PrintPageState Create() => new();

    public static IPromise<HiprintExports> LoadHiprint()
    {
        var runtime = Import<HiprintExports>("vue-plugin-hiprint/dist/vue-plugin-hiprint.js");
        // 设计器会测量控件尺寸，须等 CSS 加载后再初始化。
        return Promise.All(new IPromise[] { runtime, Import("vue-plugin-hiprint/dist/print-lock.css") })
            .Then<HiprintExports>(() => runtime);
    }

    public sealed class PrintPageState : global::Jero.Web.Composition.IPageState<PrintPageProps>
    {
        private readonly double[] _pageSizes = [10, 20, 50, 100];
        private readonly PrintProviderOption[] _providerOptions =
        [
            new() { Value = 0, Label = "A设计" },
            new() { Value = 1, Label = "B设计" }
        ];
        private readonly PrintPaperOption[] _paperOptions =
        [
            new() { Type = "A3", Width = 420, Height = 296.6 },
            new() { Type = "A4", Width = 210, Height = 296.6 },
            new() { Type = "A5", Width = 210, Height = 147.6 },
            new() { Type = "B3", Width = 500, Height = 352.6 },
            new() { Type = "B4", Width = 250, Height = 352.6 },
            new() { Type = "B5", Width = 250, Height = 175.6 },
            new() { Type = "4R", Width = 152, Height = 102 },
            new() { Type = "6R", Width = 203, Height = 152 }
        ];

        private PrintView[] _items = [];
        private PrintView[] _rows = [];
        private PrintTenantOption[] _tenantItems = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _tenantId = string.Empty;
        private string _name = string.Empty;
        private bool _loading;
        private bool _tenantLoading;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增打印模板";
        private bool _saving;
        private PrintView? _editing;
        private string _editTenantId = string.Empty;
        private string _editName = string.Empty;
        private int _editOrderNo = 100;
        private int _editStatus = 1;
        private int _editPrintType = 1;
        private string _editClientServiceAddress = string.Empty;
        private string _editPrintParam = PrintShared.DefaultPrintParam;
        private string _editRemark = string.Empty;
        private string _editPrintDataDemo = PrintShared.DefaultPrintDataDemo;
        private string _editTemplate = string.Empty;

        private HiprintDesignerSession? _designer;
        private bool _designerLoading;
        private int _designerMountVersion;
        private string _modeValue = "0";
        private string _paperType = "A4";
        private double _paperWidth = 210;
        private double _paperHeight = 296.6;
        private double _customPaperWidth = 220;
        private double _customPaperHeight = 80;
        private double _scaleValue = 1;
        private string _designerTab = "properties";
        private bool _jsonDialogVisible;
        private string _jsonText = string.Empty;
        private bool _previewDialogVisible;
        private bool _printing;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysPrint:add");
        private bool CanEdit => ApplicationState.HasPermission("sysPrint:update");
        private bool CanDelete => ApplicationState.HasPermission("sysPrint:delete");
        private bool HasActionColumn => CanEdit || CanDelete;
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;
        private bool DesignerReady => _designer is not null && !_designerLoading;

        public void Initialize()
        {
            OnBeforeUnmount(ReleasePage);
            _tenantId = DefaultTenantId;
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
            _tenantLoading = true;
            PrintClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }
                _tenantItems = PrintClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            _loading = true;
            PrintClient.Page(new PrintPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Name = _name.Trim(),
                TenantId = IsSuperAdmin ? _tenantId : string.Empty
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载打印模板失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }
                _items = PrintClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = PrintClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载打印模板失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _tenantId = DefaultTenantId;
            _name = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
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

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? _tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增打印模板");
                return;
            }

            _editing = null;
            _editDialogTitle = "新增打印模板";
            _editTenantId = targetTenantId;
            _editName = string.Empty;
            _editOrderNo = 100;
            _editStatus = 1;
            _editPrintType = 1;
            _editClientServiceAddress = string.Empty;
            _editPrintParam = PrintShared.DefaultPrintParam;
            _editRemark = string.Empty;
            _editPrintDataDemo = PrintShared.DefaultPrintDataDemo;
            _editTemplate = string.Empty;
            _modeValue = "0";
            ResetDesignerControls();
            _error = null;
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _editing = row;
            _editDialogTitle = "编辑打印模板";
            _editTenantId = row.TenantId;
            _editName = row.Name;
            _editOrderNo = row.OrderNo;
            _editStatus = row.Status == 2 ? 2 : 1;
            _editPrintType = row.PrintType == 2 ? 2 : 1;
            _editClientServiceAddress = row.ClientServiceAddress;
            _editPrintParam = row.PrintParam.Length == 0 ? PrintShared.DefaultPrintParam : PrintShared.FormatJsonText(row.PrintParam);
            _editRemark = row.Remark;
            _editPrintDataDemo = row.PrintDataDemo.Length == 0 ? PrintShared.DefaultPrintDataDemo : PrintShared.FormatJsonText(row.PrintDataDemo);
            _editTemplate = row.Template;
            _modeValue = PrintShared.ReadTemplateModeIndex(row.Template).ToString();
            ResetDesignerControls();
            _error = null;
            _editDialogVisible = true;
        }

        private void InitializeDesigner()
            => RebuildDesigner(_editTemplate);

        private void RebuildDesigner(string template)
        {
            var version = ++_designerMountVersion;
            _designerLoading = true;
            LoadHiprint().Then(exports =>
            {
                // Hiprint 会在 document.ready 自动连接，在交出运行时对象前关闭这个行为。
                exports.DisableAutoConnect();
                return exports.Default;
            }).Then(runtime =>
            {
                // 关闭弹窗或卸载页面会作废这次加载，迟到的回调不能读写 DOM。
                if (version != _designerMountVersion || !_editDialogVisible)
                    return;
                _designer = PrintBrowserActions.CreateDesigner(runtime, ParseMode(), _editClientServiceAddress, template);
                ApplyDesignerState();
                _designerLoading = false;
                _error = null;
            }).Catch(exception =>
            {
                if (version != _designerMountVersion || !_editDialogVisible)
                    return;
                _designer = null;
                _designerLoading = false;
                _error = "打印设计器初始化失败：" + exception.Message;
            });
        }

        private void ModeChanged(string? value)
        {
            var next = value == "1" ? "1" : "0";
            if (next == _modeValue)
                return;
            var template = _designer is null ? _editTemplate : PrintBrowserActions.GetTemplateJson(_designer, ParseMode());
            _modeValue = next;
            RebuildDesigner(template);
        }

        private void PaperChanged(string? value)
        {
            if (_designer is null)
                return;
            var option = FindPaper(value ?? "A4");
            if (option is null)
                return;
            _paperType = option.Type;
            _paperWidth = option.Width;
            _paperHeight = option.Height;
            PrintBrowserActions.SetPaper(_designer, _paperWidth, _paperHeight);
        }

        private void ApplyCustomPaper()
        {
            if (_designer is null || _customPaperWidth <= 0 || _customPaperHeight <= 0)
            {
                ElementPlus.ElMessageMembers.Warning("自定义纸张宽高必须大于 0");
                return;
            }
            _paperType = "other";
            _paperWidth = _customPaperWidth;
            _paperHeight = _customPaperHeight;
            PrintBrowserActions.SetPaper(_designer, _paperWidth, _paperHeight);
        }

        private void ScaleChanged(double? value)
        {
            if (_designer is null)
                return;
            _scaleValue = PrintBrowserActions.SetScale(_designer, value ?? 1);
        }

        private void RotatePaper()
        {
            if (_designer is null)
                return;
            PrintBrowserActions.RotatePaper(_designer);
            _paperWidth = _designer.PaperWidth;
            _paperHeight = _designer.PaperHeight;
            _paperType = MatchPaper(_paperWidth, _paperHeight);
        }

        private void Align(string alignment)
        {
            if (_designer is not null)
                PrintBrowserActions.Align(_designer, alignment);
        }

        private void ClearPaper()
        {
            if (_designer is not null)
                PrintBrowserActions.ClearPaper(_designer);
        }

        private void FormatPrintData()
        {
            var outcome = PrintShared.ValidateJsonObjectText(_editPrintDataDemo, "打印测试数据");
            if (!outcome.Ok)
            {
                ElementPlus.ElMessageMembers.Error(outcome.Error ?? "打印测试数据不是有效 JSON");
                return;
            }
            _editPrintDataDemo = outcome.Value.Length == 0 ? PrintShared.DefaultPrintDataDemo : outcome.Value;
        }

        private void ShowTemplateJson()
        {
            if (_designer is null)
                return;
            _jsonText = PrintBrowserActions.GetTemplateJson(_designer, ParseMode());
            _jsonDialogVisible = true;
        }

        private void OpenPreview()
        {
            if (!EnsureDesignerHost() || !ValidatePrintJson())
                return;
            _previewDialogVisible = true;
            RenderPreview();
        }

        private void RenderPreview()
        {
            if (_designer is not null)
                PrintBrowserActions.RenderPreview(_designer, _editPrintDataDemo);
        }

        private void PrintDirectly()
        {
            if (_printing || !EnsureDesignerHost() || !ValidatePrintJson() || _designer is null)
                return;

            _printing = true;
            try
            {
                if (_editPrintType == 2)
                {
                    var clientError = PrintBrowserActions.PrintWithClient(
                        _designer,
                        _editPrintDataDemo,
                        _editPrintParam,
                        () => _printing = false,
                        message =>
                        {
                            _printing = false;
                            ElementPlus.ElMessageMembers.Error("客户端打印失败：" + message);
                        });
                    if (clientError.Length > 0)
                    {
                        _printing = false;
                        ElementPlus.ElMessageMembers.Error(clientError);
                    }
                    return;
                }

                PrintBrowserActions.PrintInBrowser(_designer, _editPrintDataDemo, () => _printing = false);
            }
            catch (Exception exception)
            {
                _printing = false;
                ElementPlus.ElMessageMembers.Error("打印失败：" + exception.Message);
            }
        }

        private void ExportPdf()
        {
            if (!EnsureDesignerHost() || !ValidatePrintJson() || _designer is null)
                return;
            try
            {
                PrintBrowserActions.ExportPdf(_designer, _editPrintDataDemo);
            }
            catch (Exception exception)
            {
                ElementPlus.ElMessageMembers.Error("导出 PDF 失败：" + exception.Message);
            }
        }

        private bool EnsureDesignerHost()
        {
            if (_designer is null)
            {
                ElementPlus.ElMessageMembers.Warning("打印设计器尚未完成初始化");
                return false;
            }
            var host = _editClientServiceAddress.Trim();
            if (_designer.Host == host)
                return true;

            var template = PrintBrowserActions.GetTemplateJson(_designer, ParseMode());
            RebuildDesigner(template);
            return _designer is not null;
        }

        private bool ValidatePrintJson()
        {
            var dataOutcome = PrintShared.ValidateJsonObjectText(_editPrintDataDemo, "打印测试数据");
            if (!dataOutcome.Ok)
            {
                ElementPlus.ElMessageMembers.Error(dataOutcome.Error ?? "打印测试数据不是有效 JSON");
                return false;
            }
            var paramOutcome = PrintShared.ValidateJsonObjectText(_editPrintParam, "打印参数");
            if (!paramOutcome.Ok)
            {
                ElementPlus.ElMessageMembers.Error(paramOutcome.Error ?? "打印参数不是有效 JSON");
                return false;
            }
            _editPrintDataDemo = dataOutcome.Value.Length == 0 ? PrintShared.DefaultPrintDataDemo : dataOutcome.Value;
            _editPrintParam = paramOutcome.Value.Length == 0 ? PrintShared.DefaultPrintParam : paramOutcome.Value;
            return true;
        }

        private void Save()
        {
            if (_saving || _designer is null)
                return;

            var normalizedName = _editName.Trim();
            if (normalizedName.Length == 0)
            {
                _error = "模板名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                _error = "模板名称不能超过64个字符。";
                return;
            }
            if (_editClientServiceAddress.Trim().Length > 128)
            {
                _error = "客户端服务地址不能超过128个字符。";
                return;
            }
            if (_editRemark.Trim().Length > 128)
            {
                _error = "备注不能超过128个字符。";
                return;
            }
            if (!ValidatePrintJson())
                return;

            var templateOutcome = PrintShared.NormalizeTemplateJson(
                PrintBrowserActions.GetTemplateJson(_designer, ParseMode()),
                ParseMode());
            if (!templateOutcome.Ok)
            {
                _error = templateOutcome.Error ?? "打印模板无效。";
                return;
            }

            _saving = true;
            var request = _editing is null
                ? PrintClient.Add(new PrintAddInput
                {
                    TenantId = PrintShared.Optional(_editTenantId),
                    Name = normalizedName,
                    Template = templateOutcome.Value,
                    PrintType = _editPrintType == 2 ? 2 : 1,
                    ClientServiceAddress = PrintShared.Optional(_editClientServiceAddress),
                    PrintParam = _editPrintParam,
                    OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                    Status = _editStatus == 2 ? 2 : 1,
                    Remark = PrintShared.Optional(_editRemark),
                    PrintDataDemo = _editPrintDataDemo
                })
                : PrintClient.Update(new PrintUpdateInput
                {
                    Id = _editing.Id,
                    TenantId = PrintShared.Optional(_editTenantId),
                    Name = normalizedName,
                    Template = templateOutcome.Value,
                    PrintType = _editPrintType == 2 ? 2 : 1,
                    ClientServiceAddress = PrintShared.Optional(_editClientServiceAddress),
                    PrintParam = _editPrintParam,
                    OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                    Status = _editStatus == 2 ? 2 : 1,
                    Remark = PrintShared.Optional(_editRemark),
                    PrintDataDemo = _editPrintDataDemo,
                    CreateTime = _editing.CreateTime,
                    CreateUserId = PrintShared.Optional(_editing.CreateUserId),
                    CreateUserName = PrintShared.Optional(_editing.CreateUserName),
                    UpdateTime = PrintShared.Optional(_editing.UpdateTime),
                    UpdateUserId = PrintShared.Optional(_editing.UpdateUserId),
                    UpdateUserName = PrintShared.Optional(_editing.UpdateUserName)
                });

            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存打印模板失败。";
                    return;
                }
                CloseEditor();
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editing is null ? "打印模板已新增" : "打印模板已更新");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存打印模板失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            PrintClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除打印模板失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("打印模板已删除");
                LoadPage();
            }, _ => _error = "删除打印模板失败，请检查网络后重试。");
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;
            _auditDialogTitle = "修改记录 · " + row.Name;
            _auditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            _auditCreateTime = DisplayValue(row.CreateTime);
            _auditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            _auditUpdateTime = DisplayValue(row.UpdateTime);
            _auditDialogVisible = true;
        }

        private void CloseEditor()
        {
            _editDialogVisible = false;
            _previewDialogVisible = false;
            _jsonDialogVisible = false;
            _printing = false;
            CleanupDesigner();
        }

        private void CleanupDesigner()
        {
            ++_designerMountVersion;
            PrintBrowserActions.ClearDesigner(_designer);
            _designer = null;
            _designerLoading = false;
        }

        private void ResetDesignerControls()
        {
            CleanupDesigner();
            _paperType = "A4";
            _paperWidth = 210;
            _paperHeight = 296.6;
            _customPaperWidth = 220;
            _customPaperHeight = 80;
            _scaleValue = 1;
            _designerTab = "properties";
            _previewDialogVisible = false;
            _jsonDialogVisible = false;
            _printing = false;
        }

        private void ApplyDesignerState()
        {
            if (_designer is null)
                return;
            _paperWidth = _designer.PaperWidth;
            _paperHeight = _designer.PaperHeight;
            _customPaperWidth = _paperWidth;
            _customPaperHeight = _paperHeight;
            _scaleValue = _designer.Scale;
            _paperType = MatchPaper(_paperWidth, _paperHeight);
        }

        private int ParseMode() => _modeValue == "1" ? 1 : 0;

        private PrintPaperOption? FindPaper(string type)
        {
            for (var index = 0; index < _paperOptions.Length; index++)
                if (_paperOptions[index].Type == type)
                    return _paperOptions[index];
            return null;
        }

        private string MatchPaper(double width, double height)
        {
            for (var index = 0; index < _paperOptions.Length; index++)
                if (_paperOptions[index].Width == width && _paperOptions[index].Height == height)
                    return _paperOptions[index].Type;
            return "other";
        }

        private PrintView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private PrintTableRow[] TableRows()
        {
            var result = new PrintTableRow[_rows.Length];
            for (var index = 0; index < _rows.Length; index++)
            {
                var row = _rows[index];
                result[index] = new PrintTableRow(row.Id, row.Name, row.PrintType, row.Status, row.OrderNo,
                    DisplayValue(row.ClientServiceAddress ?? ""), DisplayValue(row.Remark ?? ""));
            }
            return result;
        }

        private ConfigurationOption[] TenantOptionsForView()
        {
            var result = new ConfigurationOption[_tenantItems.Length];
            for (var index = 0; index < _tenantItems.Length; index++)
                result[index] = new ConfigurationOption(_tenantItems[index].Value, _tenantItems[index].Label);
            return result;
        }

        private ConfigurationOption[] ProviderOptionsForView()
        {
            var result = new ConfigurationOption[_providerOptions.Length];
            for (var index = 0; index < _providerOptions.Length; index++)
                result[index] = new ConfigurationOption(_providerOptions[index].Value.ToString(), _providerOptions[index].Label);
            return result;
        }

        private ConfigurationOption[] PaperOptionsForView()
        {
            var result = new ConfigurationOption[_paperOptions.Length + 1];
            for (var index = 0; index < _paperOptions.Length; index++)
                result[index] = new ConfigurationOption(_paperOptions[index].Type, _paperOptions[index].Type);
            result[_paperOptions.Length] = new ConfigurationOption("other", "自定义");
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
            Loading = _loading,
            TenantLoading = _tenantLoading,
            EditDialogVisible = _editDialogVisible,
            Saving = _saving,
            DesignerLoading = _designerLoading,
            PreviewDialogVisible = _previewDialogVisible,
            Printing = _printing,
            JsonDialogVisible = _jsonDialogVisible,
            AuditDialogVisible = _auditDialogVisible,
            TenantId = _tenantId,
            Name = _name,
            EditDialogTitle = _editDialogTitle,
            EditName = _editName,
            EditClientServiceAddress = _editClientServiceAddress,
            EditPrintParam = _editPrintParam,
            EditRemark = _editRemark,
            ModeValue = _modeValue,
            PaperType = _paperType,
            DesignerTab = _designerTab,
            EditPrintDataDemo = _editPrintDataDemo,
            JsonText = _jsonText,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            EditOrderNo = _editOrderNo,
            EditStatus = _editStatus,
            EditPrintType = _editPrintType,
            CustomPaperWidth = _customPaperWidth,
            CustomPaperHeight = _customPaperHeight,
            ScaleValue = _scaleValue,
            Error = _error,
            TenantOptions = TenantOptionsForView(),
            ProviderOptions = ProviderOptionsForView(),
            PaperOptions = PaperOptionsForView(),
            Rows = TableRows(),
            PageSizes = _pageSizes,
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
            SetName = value => _name = value ?? "",
            SetEditName = value => _editName = value ?? "",
            SetEditClientServiceAddress = value => _editClientServiceAddress = value ?? "",
            SetEditPrintParam = value => _editPrintParam = value ?? "",
            SetEditRemark = value => _editRemark = value ?? "",
            SetEditPrintDataDemo = value => _editPrintDataDemo = value ?? "",
            SetEditOrderNo = value => _editOrderNo = (int)(value ?? 100),
            SetEditStatus = value => _editStatus = (int)value,
            SetEditPrintType = value => _editPrintType = (int)value,
            SetCustomPaperWidth = value => _customPaperWidth = value ?? 220,
            SetCustomPaperHeight = value => _customPaperHeight = value ?? 80,
            ScaleChanged = ScaleChanged,
            DismissError = () => _error = null,
            SetEditDialogVisible = value => { if (!value) CloseEditor(); },
            SetPreviewDialogVisible = value => _previewDialogVisible = value,
            SetJsonDialogVisible = value => _jsonDialogVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            ClosePreviewDialog = () => _previewDialogVisible = false,
            CloseJsonDialog = () => _jsonDialogVisible = false,
            AlignLeft = () => Align("left"),
            AlignVertical = () => Align("vertical"),
            AlignRight = () => Align("right"),
            AlignTop = () => Align("top"),
            AlignHorizontal = () => Align("horizontal"),
            AlignBottom = () => Align("bottom"),
            ShowProperties = () => _designerTab = "properties",
            ShowData = () => _designerTab = "data",
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
