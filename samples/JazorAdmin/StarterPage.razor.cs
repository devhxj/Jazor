using JazorAdmin.Features.Identity;
using static ECMAScript.VueRoute;

namespace JazorAdmin;

/// <summary>
/// Hosts the non-domain TDesign Starter routes. These are executable page reproductions, not
/// screenshots: search, step progression, form feedback and notification state remain live.
/// </summary>
[ECMAScriptModule("components/starter-page.mjs")]
public partial class StarterPage : AppComponentBase, IVueContainerComponent
{
    private readonly Router router = UseRouter();

    [Parameter]
    public string Template { get; set; } = string.Empty;

    [Parameter]
    public SessionResponse? Session { get; set; }

    private string query = string.Empty;
    private string filterName = string.Empty;
    private string filterStatus = string.Empty;
    private string filterCode = string.Empty;
    private string filterType = string.Empty;
    private string activeFilterName = string.Empty;
    private string activeFilterStatus = string.Empty;
    private string activeFilterCode = string.Empty;
    private string activeFilterType = string.Empty;
    private string treeFilter = string.Empty;
    private string contractName = string.Empty;
    private string contractParty = string.Empty;
    private string contractType = "main";
    private string paymentType = "receive";
    private string contractAmount = string.Empty;
    private string employee = string.Empty;
    private string remark = string.Empty;
    private string invoiceTitle = string.Empty;
    private string taxNumber = string.Empty;
    private string invoiceAddress = string.Empty;
    private string bank = string.Empty;
    private string bankAccount = string.Empty;
    private string contactEmail = string.Empty;
    private string consignee = string.Empty;
    private string mobileNumber = string.Empty;
    private string deliveryAddress = string.Empty;
    private string fullAddress = string.Empty;
    private string appliedCardSearch = string.Empty;
    private string cardName = string.Empty;
    private string cardDescription = string.Empty;
    private string cardType = "0";
    private string selectedUserTab = "visit";
    private string starterEmail = string.Empty;
    private string starterPassword = string.Empty;
    private string starterConfirmPassword = string.Empty;
    private string selectedTree = "Project A";
    private string selectedNoticeTab = "all";
    private int currentStep = 1;
    private bool submitted;
    private bool advancedDialogVisible;
    private bool deployDialogVisible;
    private bool cardDialogVisible;
    private bool isRegistering;
    private StarterRow? cardDeleteCandidate;
    private string[] selectedCodes = ["PRJ-2026-001", "PRJ-2026-002"];
    private StarterRow? deleteCandidate;
    private StarterNotice[] notices =
    [
        new("release", "Release approval completed", "Deployment pipeline is ready for the production window.", "2026-08-07 10:30", true, "success"),
        new("policy", "Security policy changed", "A new authorization policy requires review.", "2026-08-07 09:42", true, "warning"),
        new("schedule", "Scheduled job finished", "OpenIddict cleanup completed without errors.", "2026-08-06 18:16", false, "info")
    ];

    private static StarterRow[] CreateRows() =>
        [
            new("PRJ-2026-001", "Jazor identity center", "Platform", "In progress", "2026-08-07", "Zhang San"),
            new("PRJ-2026-002", "TDesign resource package", "Frontend", "Completed", "2026-08-06", "Li Si"),
            new("PRJ-2026-003", "Organization access review", "Security", "In progress", "2026-08-05", "Wang Wu"),
            new("PRJ-2026-004", "Quartz operations", "Platform", "Pending", "2026-08-04", "Zhao Liu"),
            new("PRJ-2026-005", "OpenID application migration", "Identity", "Completed", "2026-08-03", "Chen Qi")
        ];

    private static readonly string[] TreeNames = ["Project A", "Project B", "Archive"];

    // State is materialized as one JavaScript object literal. Field initializers must not read
    // sibling fields (`state` does not exist yet), so each mutable view receives a fresh array.
    private StarterRow[] rows = CreateRows();
    private StarterRow[] cardRows = CreateRows();

    private IEnumerable<StarterRow> VisibleRows => string.IsNullOrWhiteSpace(query)
        ? rows
        : rows.Where(row =>
            row.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            row.Code.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            row.Owner.Contains(query, StringComparison.OrdinalIgnoreCase));

    private IEnumerable<StarterRow> VisibleCardRows => string.IsNullOrWhiteSpace(appliedCardSearch)
        ? cardRows
        : cardRows.Where(row =>
            row.Name.Contains(appliedCardSearch, StringComparison.OrdinalIgnoreCase) ||
            row.Code.Contains(appliedCardSearch, StringComparison.OrdinalIgnoreCase));

    private IEnumerable<StarterRow> FilteredContractRows => rows.Where(row =>
        (string.IsNullOrWhiteSpace(activeFilterName) || row.Name.Contains(activeFilterName, StringComparison.OrdinalIgnoreCase)) &&
        (string.IsNullOrWhiteSpace(activeFilterStatus) || row.Status == activeFilterStatus) &&
        (string.IsNullOrWhiteSpace(activeFilterCode) || row.Code.Contains(activeFilterCode, StringComparison.OrdinalIgnoreCase)) &&
        (string.IsNullOrWhiteSpace(activeFilterType) || ContractTypeKey(row) == activeFilterType));

    private IEnumerable<string> FilteredTrees =>
        string.IsNullOrWhiteSpace(treeFilter)
            ? TreeNames
            : TreeNames.Where(tree =>
                tree.Contains(treeFilter, StringComparison.OrdinalIgnoreCase));

    private IEnumerable<StarterRow> TreeRows => VisibleRows.Where(row => selectedTree switch
    {
        "Project B" => row.Group == "Frontend" || row.Group == "Identity",
        "Archive" => row.Status == "Completed",
        _ => row.Group == "Platform" || row.Group == "Security"
    });

    private bool AreVisibleRowsSelected => VisibleRows.Any() && VisibleRows.All(row => IsSelected(row.Code));

    private IEnumerable<StarterNotice> VisibleNotices => selectedNoticeTab switch
    {
        "unread" => notices.Where(notice => notice.Unread),
        "read" => notices.Where(notice => !notice.Unread),
        _ => notices
    };

    private bool IsResult => Template.StartsWith("result-", StringComparison.Ordinal);

    private void SubmitDemoForm()
    {
        submitted = true;
        if (Template == "form-step" && currentStep < 4)
            currentStep++;
    }

    private void PreviousStep()
    {
        submitted = false;
        if (currentStep > 1)
            currentStep--;
    }

    private void NextStep()
    {
        submitted = false;
        if (currentStep < 4)
            currentStep++;
    }

    private void RestartStepForm()
    {
        currentStep = 1;
        submitted = false;
    }

    private void SetReceivePayment() => paymentType = "receive";

    private void SetPayPayment() => paymentType = "pay";

    private void OpenCardDialog()
    {
        cardName = string.Empty;
        cardDescription = string.Empty;
        cardType = "0";
        cardDialogVisible = true;
    }

    private void CloseCardDialog() => cardDialogVisible = false;

    private void ApplyCardSearch() => appliedCardSearch = query;

    private void SortCards() => cardRows = cardRows.OrderBy(row => row.Name).ToArray();

    private void SaveCard()
    {
        if (string.IsNullOrWhiteSpace(cardName))
            return;

        cardRows = cardRows.Concat(new[] { new StarterRow(
            "PRJ-NEW-" + (cardRows.Length + 1),
            cardName,
            cardType == "0" ? "Application" : "Service",
            "Pending",
            "2026-08-07",
            Session?.DisplayName ?? "Administrator") }).ToArray();
        submitted = true;
        cardDialogVisible = false;
    }

    private void RequestCardDelete(StarterRow row) => cardDeleteCandidate = row;

    private void CancelCardDelete() => cardDeleteCandidate = null;

    private void ConfirmCardDelete()
    {
        if (cardDeleteCandidate is null)
            return;

        cardRows = cardRows.Where(row => row.Code != cardDeleteCandidate.Code).ToArray();
        cardDeleteCandidate = null;
    }

    private void ShowUserContent() => selectedUserTab = "content";

    private void ShowUserVisits() => selectedUserTab = "visit";

    private void ShowUserActivity() => selectedUserTab = "activity";

    private void ToggleLoginMode()
    {
        isRegistering = !isRegistering;
        submitted = false;
    }

    private void SubmitStarterLogin() => submitted = true;

    private void SubmitStarterRegistration()
    {
        if (starterPassword != starterConfirmPassword)
            return;

        submitted = true;
    }

    private void OpenAdvancedDialog() => advancedDialogVisible = true;

    private void CloseAdvancedDialog() => advancedDialogVisible = false;

    private void OpenDeployDialog() => deployDialogVisible = true;

    private void CloseDeployDialog() => deployDialogVisible = false;

    private void SelectTree(string tree) => selectedTree = tree;

    private string TreeLabel(string tree) => tree switch
    {
        "Archive" => L("Archive", "归档"),
        "Project B" => L("Project B", "项目 B"),
        _ => L("Project A", "项目 A")
    };

    private bool IsSelected(string code) => selectedCodes.Contains(code);

    private void ToggleSelection(string code)
    {
        selectedCodes = IsSelected(code)
            ? selectedCodes.Where(item => item != code).ToArray()
            : selectedCodes.Concat(new[] { code }).ToArray();
    }

    private void ToggleVisibleSelection()
    {
        var visible = VisibleRows.Select(row => row.Code).ToArray();
        if (visible.All(IsSelected))
            selectedCodes = selectedCodes.Where(code => !visible.Contains(code)).ToArray();
        else
            selectedCodes = selectedCodes.Concat(visible.Where(code => !IsSelected(code))).ToArray();
    }

    private void ApplyContractFilters()
    {
        activeFilterName = filterName;
        activeFilterStatus = filterStatus;
        activeFilterCode = filterCode;
        activeFilterType = filterType;
    }

    private void ResetContractFilters()
    {
        filterName = string.Empty;
        filterStatus = string.Empty;
        filterCode = string.Empty;
        filterType = string.Empty;
        ApplyContractFilters();
    }

    private string StatusClass(string status) => status switch
    {
        "Completed" => "is-success",
        "Pending" => "is-warning",
        _ => "is-processing"
    };

    private string ContractType(StarterRow row) => row.Code.EndsWith("001", StringComparison.Ordinal) || row.Code.EndsWith("004", StringComparison.Ordinal)
        ? L("Main contract", "主合同")
        : L("Supplementary contract", "补充合同");

    private string ContractTypeKey(StarterRow row) => row.Code.EndsWith("001", StringComparison.Ordinal) || row.Code.EndsWith("004", StringComparison.Ordinal)
        ? "main"
        : "supplement";

    private string PaymentType(StarterRow row) => row.Code.EndsWith("002", StringComparison.Ordinal) || row.Code.EndsWith("005", StringComparison.Ordinal)
        ? L("Receive", "收款")
        : L("Pay", "付款");

    private string ContractAmount(StarterRow row) => row.Code.EndsWith("001", StringComparison.Ordinal) ? "¥ 5,000,000" : row.Code.EndsWith("002", StringComparison.Ordinal) ? "¥ 278,821" : "¥ 109,824";

    private void OpenBaseContract() => Navigate("/starter/form/base");

    private void OpenBaseDetail() => Navigate("/starter/detail/base");

    private void RequestDelete(StarterRow row) => deleteCandidate = row;

    private void CancelDelete() => deleteCandidate = null;

    private void ConfirmDelete()
    {
        if (deleteCandidate is null)
            return;

        rows = rows.Where(row => row.Code != deleteCandidate.Code).ToArray();
        selectedCodes = selectedCodes.Where(code => code != deleteCandidate.Code).ToArray();
        deleteCandidate = null;
    }

    private void SelectAllNotices() => selectedNoticeTab = "all";

    private void SelectUnreadNotices() => selectedNoticeTab = "unread";

    private void SelectReadNotices() => selectedNoticeTab = "read";

    private void ToggleNotice(StarterNotice notice)
    {
        notices = notices.Select(item => item.Id == notice.Id ? item with { Unread = !item.Unread } : item).ToArray();
    }

    private void RemoveNotice(StarterNotice notice)
    {
        notices = notices.Where(item => item.Id != notice.Id).ToArray();
    }

    private void Navigate(string path)
    {
        _ = router.Push((RouteLocationRaw)path);
    }

    private StarterResult GetResult() => Template switch
    {
        "result-success" => new("check-circle", null, "success", L("Submission successful", "提交成功"), L("The operation has been submitted. You can continue with the next task.", "操作已提交，您可以继续处理下一项任务。"), L("Back to dashboard", "返回首页"), "/starter/dashboard/base", L("View progress", "查看进度"), "/starter/detail/advanced"),
        "result-fail" => new("error-circle", null, "fail", L("Submission failed", "提交失败"), L("Please check the entered information and submit again.", "请检查已填写的信息后重新提交。"), L("Modify", "返回修改"), "/starter/form/base", L("Back", "返回"), "/starter/dashboard/base"),
        "result-network" => new(string.Empty, "/brand/starter-assets-result-wifi.svg", "illustrated", L("Network error", "网络异常"), L("The service is temporarily unavailable. Try again later.", "服务暂时不可用，请稍后重试。"), L("Reload", "重新加载"), "/starter/result/network-error", L("Back", "返回"), "/starter/dashboard/base"),
        "result-403" => new(string.Empty, "/brand/starter-assets-result-403.svg", "illustrated", "403 Forbidden", L("Your current account does not have access to this resource.", "当前账号没有访问该资源的权限。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        "result-404" => new(string.Empty, "/brand/starter-assets-result-404.svg", "illustrated", "404 Not Found", L("The requested page does not exist or has moved.", "请求的页面不存在或已被移动。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        "result-500" => new(string.Empty, "/brand/starter-assets-result-500.svg", "illustrated", "500 Internal Server Error", L("The server could not finish this request.", "服务器未能完成本次请求。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        "result-browser" => new(string.Empty, "/brand/starter-assets-result-ie.svg", "illustrated", L("Browser incompatible", "浏览器不兼容"), L("Use a current browser to access this workspace.", "请使用当前版本的浏览器访问本工作区。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        "result-maintenance" => new(string.Empty, "/brand/starter-assets-result-maintenance.svg", "illustrated", L("Service maintenance", "系统维护中"), L("The service is being maintained and will return shortly.", "系统正在维护，将在短时间内恢复。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty),
        _ => new(string.Empty, "/brand/starter-assets-result-404.svg", "illustrated", "404 Not Found", L("The requested page does not exist or has moved.", "请求的页面不存在或已被移动。"), L("Back", "返回"), "/starter/dashboard/base", null, string.Empty)
    };

    private sealed record StarterRow(string Code, string Name, string Group, string Status, string UpdatedAt, string Owner);

    private sealed record StarterNotice(string Id, string Title, string Content, string Date, bool Unread, string Kind);

    private sealed record StarterResult(
        string Icon,
        string? Asset,
        string Kind,
        string Title,
        string Description,
        string PrimaryLabel,
        string PrimaryTarget,
        string? SecondaryLabel,
        string SecondaryTarget);
}
