using JazorAdmin.Features.Identity;
using ECMAScript.VueDataUi;
using static ECMAScript.VueRoute;

namespace JazorAdmin;

/// <summary>
/// Executable TDesign Starter pages. The page inventory is intentionally kept in one component
/// so every route uses the same typed table, form and feedback contracts.
/// 可执行的 TDesign Starter 页面；所有模板共享强类型表格、表单和反馈组件契约。
/// </summary>
[ECMAScriptModule("./components/starter-page")]
public partial class StarterPage : AppComponentBase, IVueContainerComponent
{
    private sealed record StarterContractDraft
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "main";
        public string Payment { get; set; } = "receive";
        public string Company { get; set; } = string.Empty;
        public string Employee { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string SigningDate { get; set; } = string.Empty;
        public string EffectiveDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    private sealed record StarterFilterDraft
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    private sealed record StarterCardDraft
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = "Application";
    }

    private sealed record StarterStepDraft
    {
        public string ContractName { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = "main";
        public string InvoiceTitle { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Consignee { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    private sealed record StarterLoginDraft
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    private readonly Router router = UseRouter();

    [Parameter]
    public string Template { get; set; } = string.Empty;

    [Parameter]
    public SessionResponse? Session { get; set; }

    private readonly StarterContractDraft contractDraft = new();
    private readonly StarterFilterDraft filterDraft = new();
    private readonly StarterCardDraft cardDraft = new();
    private readonly StarterLoginDraft loginDraft = new();
    private readonly StarterStepDraft stepDraft = new();

    private const int PageSize = 5;
    private StarterRow[] rows = CreateRows();
    private StarterRow[] cardRows = CreateRows();
    private StarterNotice[] notices = CreateNotices();
    private TUploadFile[] attachments = [];
    private string query = string.Empty;
    private string appliedCardSearch = string.Empty;
    private string treeFilter = string.Empty;
    private string[] selectedCodes = ["PRJ-2026-001"];
    private string selectedCode = "PRJ-2026-001";
    private string selectedTree = "Project A";
    private string selectedUserTab = "visit";
    private string selectedNoticeTab = "all";
    private string selectedProductTab = "quarter";
    private int listPage = 1;
    private int cardPage = 1;
    private int currentStep = 1;
    private bool submitted;
    private string stepError = string.Empty;
    private bool advancedApproved;
    private bool advancedDialogVisible;
    private bool deployDialogVisible;
    private bool cardDialogVisible;
    private bool isRegistering;
    private StarterRow[] deleteCandidates = [];
    private StarterRow? cardDeleteCandidate;

    private static readonly TPrimaryTableCol<StarterRow>[] TableColumns =
    [
        new() { ColKey = "Name", Title = (TPrimaryTableColTitle<StarterRow>)"Project" },
        new() { ColKey = "Status", Title = (TPrimaryTableColTitle<StarterRow>)"Status" },
        new() { ColKey = "Code", Title = (TPrimaryTableColTitle<StarterRow>)"Code" },
        new() { ColKey = "Group", Title = (TPrimaryTableColTitle<StarterRow>)"Group" },
        new() { ColKey = "UpdatedAt", Title = (TPrimaryTableColTitle<StarterRow>)"Updated" },
        new() { ColKey = "Owner", Title = (TPrimaryTableColTitle<StarterRow>)"Owner" }
    ];

    private static readonly TPrimaryTableCol<StarterRow>[] BaseTableColumns =
    [
        new() { Type = TPrimaryTableColType.Multiple },
        .. TableColumns
    ];

    private TTableSelectedRowKeysValueItem<StarterRow>[] SelectedRowKeys
        => selectedCodes.Select(code => (TTableSelectedRowKeysValueItem<StarterRow>)code).ToArray();

    private TSelectOption[] ContractTypeOptions =>
    [
        Option("main", L("Main contract", "主合同")),
        Option("supplement", L("Supplementary contract", "补充合同"))
    ];

    private TSelectOption[] PaymentTypeOptions =>
    [
        Option("receive", L("Receive", "收款")),
        Option("pay", L("Pay", "付款"))
    ];

    private TSelectOption[] CompanyOptions =>
    [
        Option("Jazor Technology", "Jazor Technology"),
        Option("North Clinic", "North Clinic")
    ];

    private TSelectOption[] EmployeeOptions =>
    [
        Option("Zhang San", "Zhang San"),
        Option("Li Si", "Li Si"),
        Option("Wang Wu", "Wang Wu")
    ];

    private TSelectOption[] InvoiceTypeOptions =>
    [
        Option("main", L("Main invoice", "主发票")),
        Option("supplement", L("Supplementary invoice", "补充发票"))
    ];

    private VueUiVerticalBarDatasetItem[] StarterTrendItems =>
    [
        new() { Name = "Mon", Value = 42, Color = "#0052d9" },
        new() { Name = "Tue", Value = 68, Color = "#0052d9" },
        new() { Name = "Wed", Value = 54, Color = "#0052d9" },
        new() { Name = "Thu", Value = 88, Color = "#0052d9" },
        new() { Name = "Fri", Value = 74, Color = "#0052d9" },
        new() { Name = "Sat", Value = 91, Color = "#0052d9" },
        new() { Name = "Sun", Value = 79, Color = "#0052d9" }
    ];

    private VueUiVerticalBarDatasetItem[] DeploymentTrendItems =>
    [
        new() { Name = "Mon", Value = 35, Color = "#0052d9" },
        new() { Name = "Tue", Value = 66, Color = "#0052d9" },
        new() { Name = "Wed", Value = 78, Color = "#0052d9" },
        new() { Name = "Thu", Value = 92, Color = "#0052d9" }
    ];

    private VueUiVerticalBarDatasetItem[] WarningTrendItems =>
    [
        new() { Name = "Mon", Value = 24, Color = "#ed7b2f" },
        new() { Name = "Tue", Value = 48, Color = "#ed7b2f" },
        new() { Name = "Wed", Value = 12, Color = "#ed7b2f" }
    ];

    private VueUiSparklineDatasetItem[] UserVisitItems =>
    [
        new() { Period = "Mon", Value = 48 },
        new() { Period = "Tue", Value = 72 },
        new() { Period = "Wed", Value = 60 },
        new() { Period = "Thu", Value = 86 }
    ];

    private static readonly VueUiVerticalBarConfig StarterTrendConfig = new() { Responsive = true };
    private static readonly VueUiSparklineConfig UserVisitConfig = new() { Responsive = true, Type = VueUiSparklineType.Line };

    private VueUiDonutDatasetItem[] StarterDistributionItems =>
    [
        new() { Name = L("Direct", "直接访问"), Values = [45], Color = "#0052d9" },
        new() { Name = L("Search", "搜索"), Values = [32], Color = "#00a870" },
        new() { Name = L("Referral", "推荐"), Values = [23], Color = "#edb105" }
    ];

    private static readonly VueUiDonutConfig StarterDistributionConfig = new() { Responsive = true };

    private string AdvancedStatusText
        => advancedApproved ? L("Approved", "已审核") : L("Pending review", "待审核");

    private TTagThemeValue AdvancedStatusTheme
        => advancedApproved ? TTagThemeValue.Success : TTagThemeValue.Warning;

    private StarterRow[] FilteredRows
        => FilterRows(rows, query);

    private StarterRow[] VisibleRows
        => Page(FilteredRows, listPage);

    private int VisibleRowTotal
        => FilteredRows.Length;

    private StarterRow[] FilteredCardRows
        => FilterRows(cardRows, appliedCardSearch);

    private StarterRow[] VisibleCardRows
        => Page(FilteredCardRows, cardPage);

    private int VisibleCardTotal
        => FilteredCardRows.Length;

    private StarterRow[] FilteredContractRows
        => rows.Where(row =>
            (string.IsNullOrWhiteSpace(filterDraft.Name) || row.Name.Contains(filterDraft.Name, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(filterDraft.Status) || row.Status == filterDraft.Status) &&
            (string.IsNullOrWhiteSpace(filterDraft.Code) || row.Code.Contains(filterDraft.Code, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(filterDraft.Type) || ContractTypeKey(row) == filterDraft.Type)).ToArray();

    private StarterRow[] TreeRows
        => VisibleRows.Where(row => selectedTree switch
        {
            "Project B" => row.Group is "Frontend" or "Identity",
            "Archive" => row.Status == "Completed",
            _ => row.Group is "Platform" or "Security"
        }).ToArray();

    private string[] FilteredTrees
        => string.IsNullOrWhiteSpace(treeFilter)
            ? new[] { "Project A", "Project B", "Archive" }
            : new[] { "Project A", "Project B", "Archive" }.Where(value => value.Contains(treeFilter, StringComparison.OrdinalIgnoreCase)).ToArray();

    private static StarterRow[] Page(StarterRow[] source, int page)
        => source.Skip(Math.Max(0, page - 1) * PageSize).Take(PageSize).ToArray();

    private static TSelectOption Option(string value, string label)
        => (TSelectOption)new TdOptionProps
        {
            Value = (TdOptionPropsValue)value,
            Label = label
        };

    private static TSelectValue<TSelectOption> SelectValue(string value)
        => (TSelectValue<TSelectOption>)value;

    private static TDatePickerValueValue DateValue(string value)
        => (TDatePickerValueValue)(TDateValue)value;

    private bool IsResult => Template.StartsWith("result-", StringComparison.Ordinal);

    protected override void OnParametersSet()
    {
        if (!IsSupportedTemplate(Template))
            throw new Error("Unsupported Starter template: " + Template);
    }

    private static bool IsSupportedTemplate(string template)
        => template is "dashboard-base" or "dashboard-detail" or "list-base" or "list-card" or "list-filter" or "list-tree" or
            "form-base" or "form-step" or "detail-base" or "detail-advanced" or "detail-deploy" or "detail-secondary" or
            "result-success" or "result-fail" or "result-network" or "result-403" or "result-404" or "result-500" or
            "result-browser" or "result-maintenance" or "user" or "login";

    private TdStepItemProps[] StepOptions =>
    [
        new() { Value = (TdStepItemPropsValue)"1", Title = (TdStepItemPropsTitle)L("Contract", "合同信息"), Content = (TdStepItemPropsContent)L("Select a contract", "选择合同") },
        new() { Value = (TdStepItemPropsValue)"2", Title = (TdStepItemPropsTitle)L("Invoice", "发票信息"), Content = (TdStepItemPropsContent)L("Invoice details", "填写发票") },
        new() { Value = (TdStepItemPropsValue)"3", Title = (TdStepItemPropsTitle)L("Delivery", "收货信息"), Content = (TdStepItemPropsContent)L("Delivery details", "填写收货") },
        new() { Value = (TdStepItemPropsValue)"4", Title = (TdStepItemPropsTitle)L("Complete", "完成"), Content = (TdStepItemPropsContent)L("Review result", "查看结果") }
    ];

    private static StarterRow[] CreateRows() =>
    [
        new("PRJ-2026-001", "Jazor identity center", "Platform", "In progress", "2026-08-07", "Zhang San"),
        new("PRJ-2026-002", "TDesign resource package", "Frontend", "Completed", "2026-08-06", "Li Si"),
        new("PRJ-2026-003", "Organization access review", "Security", "In progress", "2026-08-05", "Wang Wu"),
        new("PRJ-2026-004", "Quartz operations", "Platform", "Pending", "2026-08-04", "Zhao Liu"),
        new("PRJ-2026-005", "OpenID application migration", "Identity", "Completed", "2026-08-03", "Chen Qi")
    ];

    private static StarterNotice[] CreateNotices() =>
    [
        new("release", "Release approval completed", "Deployment pipeline is ready for the production window.", "2026-08-07 10:30", true, "success"),
        new("policy", "Security policy changed", "A new authorization policy requires review.", "2026-08-07 09:42", true, "warning"),
        new("schedule", "Scheduled job finished", "OpenIddict cleanup completed without errors.", "2026-08-06 18:16", false, "info")
    ];

    private static StarterRow[] FilterRows(StarterRow[] source, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return source;

        return source.Where(row =>
            row.Name.Contains(value, StringComparison.OrdinalIgnoreCase) ||
            row.Code.Contains(value, StringComparison.OrdinalIgnoreCase) ||
            row.Owner.Contains(value, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    private void SelectRow(TRowEventContext<StarterRow> context)
        => selectedCode = context.Row.Code;

    private void SelectRows(TTableSelectChangeEventSelectedRowKeysItem<StarterRow>[] keys)
    {
        var values = new List<string>();
        foreach (var key in keys)
        {
            if (key.Value is string code)
                values.Add(code);
        }

        selectedCodes = values.ToArray();
        selectedCode = values.Count == 0 ? string.Empty : values[0];
    }

    private void ChangeListQuery(string value)
    {
        query = value;
        listPage = 1;
    }

    private void OpenBaseContract()
        => Navigate("/starter/form/base");

    private void OpenBaseDetail()
        => Navigate("/starter/detail/base");

    private void Navigate(string path)
        => _ = router.Push((RouteLocationRaw)path);

    private void ApplyCardSearch()
    {
        appliedCardSearch = query;
        cardPage = 1;
    }

    private void ChangeCardQuery(string value)
    {
        query = value;
        cardPage = 1;
    }

    private void SortCards()
        => cardRows = cardRows.OrderBy(row => row.Name).ToArray();

    private void OpenCardDialog()
    {
        cardDraft.Name = string.Empty;
        cardDraft.Description = string.Empty;
        cardDraft.Type = "Application";
        cardDialogVisible = true;
    }

    private void CloseCardDialog()
        => cardDialogVisible = false;

    private void SaveCard(TSubmitContext<StarterCardDraft> context)
    {
        if (string.IsNullOrWhiteSpace(cardDraft.Name))
            return;

        cardRows = cardRows.Concat(
        [
            new StarterRow(
                "PRJ-NEW-" + (cardRows.Length + 1),
                cardDraft.Name,
                cardDraft.Type,
                "Pending",
                "2026-08-07",
                Session?.DisplayName ?? "Administrator")
        ]).ToArray();
        submitted = true;
        cardDialogVisible = false;
    }

    private void RequestCardDelete(StarterRow row)
        => cardDeleteCandidate = row;

    private void CancelCardDelete()
        => cardDeleteCandidate = null;

    private void ConfirmCardDelete()
    {
        if (cardDeleteCandidate is null)
            return;

        cardRows = cardRows.Where(row => row.Code != cardDeleteCandidate.Code).ToArray();
        cardDeleteCandidate = null;
    }

    private void RequestDelete(StarterRow row)
        => deleteCandidates = [row];

    private void RequestDeleteSelected()
    {
        deleteCandidates = rows.Where(item => selectedCodes.Contains(item.Code)).ToArray();
        if (deleteCandidates.Length == 0 && selectedCode.Length > 0)
        {
            var row = rows.FirstOrDefault(item => item.Code == selectedCode);
            if (row is not null)
                deleteCandidates = [row];
        }
    }

    private void CancelDelete()
        => deleteCandidates = [];

    private void ConfirmDelete()
    {
        if (deleteCandidates.Length == 0)
            return;

        var codes = deleteCandidates.Select(row => row.Code).ToArray();
        rows = rows.Where(row => !codes.Contains(row.Code)).ToArray();
        selectedCodes = selectedCodes.Where(code => !codes.Contains(code)).ToArray();
        selectedCode = selectedCodes.FirstOrDefault() ?? string.Empty;
        listPage = Math.Min(listPage, Math.Max(1, (int)Math.Ceiling(FilteredRows.Length / (double)PageSize)));
        deleteCandidates = [];
    }

    private void ChangePage(Number page)
        => listPage = Math.Clamp((int)page, 1, Math.Max(1, (int)Math.Ceiling(VisibleRowTotal / (double)PageSize)));

    private void ChangeCardPage(Number page)
        => cardPage = Math.Clamp((int)page, 1, Math.Max(1, (int)Math.Ceiling(VisibleCardTotal / (double)PageSize)));

    private void SelectTree(TTreeClickEventContext<string> context)
        => selectedTree = context.Node.Data;

    private void ApplyContractFilters(TSubmitContext<StarterFilterDraft> context)
    {
        listPage = 1;
        submitted = true;
    }

    private void ResetContractFilters(TFormResetEventContext<StarterFilterDraft> context)
    {
        filterDraft.Name = string.Empty;
        filterDraft.Status = string.Empty;
        filterDraft.Code = string.Empty;
        filterDraft.Type = string.Empty;
        submitted = false;
        listPage = 1;
    }

    private void SubmitBaseForm(TSubmitContext<StarterContractDraft> context)
        => submitted = true;

    private void ResetBaseForm(TFormResetEventContext<StarterContractDraft> context)
    {
        contractDraft.Name = string.Empty;
        contractDraft.Type = "main";
        contractDraft.Payment = "receive";
        contractDraft.Company = string.Empty;
        contractDraft.Employee = string.Empty;
        contractDraft.Amount = string.Empty;
        contractDraft.SigningDate = string.Empty;
        contractDraft.EffectiveDate = string.Empty;
        contractDraft.EndDate = string.Empty;
        contractDraft.Remark = string.Empty;
        attachments = [];
        submitted = false;
    }

    private void SubmitStep(TSubmitContext<StarterStepDraft> context)
    {
        stepError = currentStep switch
        {
            1 when string.IsNullOrWhiteSpace(stepDraft.ContractName) => L("Select a contract before continuing.", "请先选择合同。"),
            2 when string.IsNullOrWhiteSpace(stepDraft.InvoiceTitle) || string.IsNullOrWhiteSpace(stepDraft.TaxNumber) => L("Complete the invoice details before continuing.", "请先填写完整的发票信息。"),
            3 when string.IsNullOrWhiteSpace(stepDraft.Consignee) || string.IsNullOrWhiteSpace(stepDraft.MobileNumber) || string.IsNullOrWhiteSpace(stepDraft.Address) => L("Complete the delivery details before submitting.", "请先填写完整的收货信息。"),
            _ => string.Empty
        };
        if (stepError.Length > 0)
        {
            submitted = false;
            return;
        }

        submitted = true;
        if (currentStep < 4)
            currentStep++;
    }

    private void ResetStep(TFormResetEventContext<StarterStepDraft> context)
    {
        currentStep = 1;
        stepError = string.Empty;
        stepDraft.ContractName = string.Empty;
        stepDraft.InvoiceType = "main";
        stepDraft.InvoiceTitle = string.Empty;
        stepDraft.TaxNumber = string.Empty;
        stepDraft.Consignee = string.Empty;
        stepDraft.MobileNumber = string.Empty;
        stepDraft.Address = string.Empty;
        submitted = false;
    }

    private void PreviousStep()
    {
        submitted = false;
        stepError = string.Empty;
        if (currentStep > 1)
            currentStep--;
    }

    private void RestartStepForm()
    {
        currentStep = 1;
        submitted = false;
        stepError = string.Empty;
    }

    private void ChangeStep(TStepsChangeEventCurrent value)
    {
        if (value.Value is Number number)
            currentStep = Math.Clamp((int)number, 1, 4);
    }

    private void UpdateAttachments(TUploadFile[] files)
        => attachments = files;

    private void OpenAdvancedDialog()
        => advancedDialogVisible = true;

    private void ApproveAdvanced()
    {
        advancedApproved = true;
        submitted = true;
    }

    private void CloseAdvancedDialog()
        => advancedDialogVisible = false;

    private void OpenDeployDialog()
        => deployDialogVisible = true;

    private void CloseDeployDialog()
        => deployDialogVisible = false;

    private void SelectNoticeTab(TTabValue value)
    {
        if (value.Value is string tab)
            selectedNoticeTab = tab;
    }

    private void SelectProductTab(TTabValue value)
    {
        if (value.Value is string tab)
            selectedProductTab = tab;
    }

    private StarterNotice[] VisibleNotices => selectedNoticeTab switch
    {
        "unread" => notices.Where(notice => notice.Unread).ToArray(),
        "read" => notices.Where(notice => !notice.Unread).ToArray(),
        _ => notices
    };

    private void ToggleNotice(StarterNotice notice)
        => notices = notices.Select(item => item.Id == notice.Id ? item with { Unread = !item.Unread } : item).ToArray();

    private void RemoveNotice(StarterNotice notice)
        => notices = notices.Where(item => item.Id != notice.Id).ToArray();

    private void SelectUserTab(TTabValue value)
    {
        if (value.Value is string tab)
            selectedUserTab = tab;
    }

    private void ToggleLoginMode()
    {
        isRegistering = !isRegistering;
        submitted = false;
        loginDraft.Error = string.Empty;
    }

    private void SubmitStarterLogin(TSubmitContext<StarterLoginDraft> context)
    {
        loginDraft.Error = string.Empty;
        submitted = true;
    }

    private void SubmitStarterRegistration(TSubmitContext<StarterLoginDraft> context)
    {
        if (loginDraft.Password != loginDraft.ConfirmPassword)
        {
            loginDraft.Error = L("Passwords do not match.", "两次密码不一致。");
            submitted = false;
            return;
        }

        loginDraft.Error = string.Empty;
        submitted = true;
    }

    private TTagThemeValue StatusTheme(string status)
        => status switch
        {
            "Completed" => TTagThemeValue.Success,
            "Pending" => TTagThemeValue.Warning,
            _ => TTagThemeValue.Primary
        };

    private static TTabValue Tab(string value)
        => (TTabValue)value;

    private static TStepsCurrentValue StepCurrent(int value)
        => (TStepsCurrentValue)(Number)value;

    private string ContractType(StarterRow row)
        => ContractTypeKey(row) == "main" ? L("Main contract", "主合同") : L("Supplementary contract", "补充合同");

    private static string ContractTypeKey(StarterRow row)
        => row.Code.EndsWith("001", StringComparison.Ordinal) || row.Code.EndsWith("004", StringComparison.Ordinal) ? "main" : "supplement";

    private string PaymentType(StarterRow row)
        => row.Code.EndsWith("002", StringComparison.Ordinal) || row.Code.EndsWith("005", StringComparison.Ordinal) ? L("Receive", "收款") : L("Pay", "付款");

    private static string ContractAmount(StarterRow row)
        => row.Code.EndsWith("001", StringComparison.Ordinal) ? "¥ 5,000,000" : row.Code.EndsWith("002", StringComparison.Ordinal) ? "¥ 278,821" : "¥ 109,824";

    private StarterResult GetResult() => Template switch
    {
        "result-success" => new("check-circle", null, TTagThemeValue.Success, L("Submission successful", "提交成功"), L("The operation has been submitted. You can continue with the next task.", "操作已提交，您可以继续处理下一项任务。"), L("Back to dashboard", "返回首页"), "/starter/dashboard/base", L("View progress", "查看进度"), "/starter/detail/advanced"),
        "result-fail" => new("error-circle", null, TTagThemeValue.Danger, L("Submission failed", "提交失败"), L("Please check the entered information and submit again.", "请检查已填写的信息后重新提交。"), L("Modify", "返回修改"), "/starter/form/base", L("Back", "返回"), "/starter/dashboard/base"),
        "result-network" => new(string.Empty, "/brand/starter-assets-result-wifi.svg", TTagThemeValue.Warning, L("Network error", "网络异常"), L("The service is temporarily unavailable. Try again later.", "服务暂时不可用，请稍后重试。"), L("Reload", "重新加载"), "/starter/result/network-error", L("Back", "返回"), "/starter/dashboard/base"),
        "result-403" => new(string.Empty, "/brand/starter-assets-result-403.svg", TTagThemeValue.Danger, "403 Forbidden", L("Your current account does not have access to this resource.", "当前账号没有访问该资源的权限。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        "result-404" => new(string.Empty, "/brand/starter-assets-result-404.svg", TTagThemeValue.Warning, "404 Not Found", L("The requested page does not exist or has moved.", "请求的页面不存在或已被移动。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        "result-500" => new(string.Empty, "/brand/starter-assets-result-500.svg", TTagThemeValue.Danger, "500 Internal Server Error", L("The server could not finish this request.", "服务器未能完成本次请求。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        "result-browser" => new(string.Empty, "/brand/starter-assets-result-ie.svg", TTagThemeValue.Warning, L("Browser incompatible", "浏览器不兼容"), L("Use a current browser to access this workspace.", "请使用当前版本的浏览器访问本工作区。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        "result-maintenance" => new(string.Empty, "/brand/starter-assets-result-maintenance.svg", TTagThemeValue.Primary, L("Service maintenance", "系统维护中"), L("The service is being maintained and will return shortly.", "系统正在维护，将在短时间内恢复。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        _ => throw new Error("Unsupported Starter result template: " + Template)
    };
}

public sealed record StarterRow(string Code, string Name, string Group, string Status, string UpdatedAt, string Owner);

public sealed record StarterNotice(string Id, string Title, string Content, string Date, bool Unread, string Kind);

public sealed record StarterResult(
    string Icon,
    string? Asset,
    TTagThemeValue Theme,
    string Title,
    string Description,
    string PrimaryLabel,
    string PrimaryTarget,
    string? SecondaryLabel,
    string SecondaryTarget);
