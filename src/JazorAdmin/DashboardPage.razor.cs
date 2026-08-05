namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-dashboard-page")]
public partial class DashboardPage : AppComponentBase, IVueContainerComponent
{
    private const string StudyKras = "一项在 KRAS G12D 突变小细胞肺癌患者中比较 Setidegrasib 的研究";
    private const string StudyLiver = "在肝硬化患者中评估 feceleglipron 安全性的方案";
    private const string StudyTargeted = "贝那替尼联合治疗方案的有效性和安全性";
    private const string StudyMercury = "MERCURY 伊布利珠单抗研究";
    private const string StudyTt0113 = "TT0113 胶囊治疗早发性直肠癌";

    private string selectedStudy = StudyKras;

    [Parameter]
    public string? SelectedRowKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedRowKeyChanged { get; set; }

    [Parameter]
    public string[]? SelectedRowKeys { get; set; }

    [Parameter]
    public EventCallback<string[]> SelectedRowKeysChanged { get; set; }

    [Parameter]
    public string? SearchText { get; set; }

    [Parameter]
    public EventCallback<string> SearchTextChanged { get; set; }

    [Parameter]
    public int PageIndex { get; set; }

    [Parameter]
    public EventCallback<int> PageIndexChanged { get; set; }

    [Parameter]
    public string? SortColumnKey { get; set; }

    [Parameter]
    public EventCallback<string> SortColumnKeyChanged { get; set; }

    [Parameter]
    public bool SortDescending { get; set; }

    [Parameter]
    public EventCallback<bool> SortDescendingChanged { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    private string SelectedStudy => selectedStudy;

    private string GetStudyClass(string study)
        => string.Equals(study, selectedStudy, StringComparison.Ordinal)
            ? "jazor-admin-medical-dashboard__study is-selected"
            : "jazor-admin-medical-dashboard__study";

    private void SelectStudy(string study)
        => selectedStudy = study;

    private void ShowAllStudies()
        => selectedStudy = "全部招募项目";

    private void ShowApplications()
        => selectedStudy = "最近报名记录";
}
