using Jero.Web.Application;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.Job;

[ECMAScript]
public interface IJobContentState
{
    void ApplyCronMacro(string value);
    bool Busy { get; }
    void CancelJob(VueProps? value);
    bool CanDelete { get; }
    bool CanEdit { get; }
    void ChangeRecordPage(Number value);
    void ChangeRecordPageSize(Number value);
    void ClearRecords();
    bool ClusterDialogVisible { get; set; }
    bool ClusterLoading { get; }
    JobClusterView[] ClusterRows { get; }
    string ClusterStatusTagForView(VueProps? value);
    string ClusterStatusTextForView(VueProps? value);
    string CreateTypeTagForView(VueProps? value);
    string CreateTypeTextForCode(int createType);
    string CreateTypeTextForRow(VueProps? value);
    void DeleteJob(VueProps? value);
    void DeleteTrigger(VueProps? value);
    string DisplayValueForView(string value);
    string EditHttpBody { get; set; }
    string EditHttpMethod { get; set; }
    bool EditHttpPropertiesValid { get; }
    string EditHttpRequestUri { get; set; }
    bool EditingJob { get; }
    bool EditJobConcurrent { get; set; }
    int EditJobCreateType { get; }
    string EditJobDescription { get; set; }
    string EditJobGroupName { get; set; }
    string EditJobId { get; set; }
    bool EditJobIncludeAnnotation { get; set; }
    string EditJobProperties { get; set; }
    string EditJobScriptCode { get; set; }
    string EditTriggerCron { get; set; }
    string EditTriggerDescription { get; set; }
    string EditTriggerEndTime { get; set; }
    string EditTriggerId { get; set; }
    string EditTriggerJobId { get; }
    string EditTriggerMaxNumberOfErrors { get; set; }
    string EditTriggerMaxNumberOfRuns { get; set; }
    string EditTriggerNumRetries { get; set; }
    string EditTriggerPeriod { get; set; }
    bool EditTriggerResetOnlyOnce { get; set; }
    string EditTriggerRetryTimeout { get; set; }
    bool EditTriggerRunOnStart { get; set; }
    bool EditTriggerStartNow { get; set; }
    string EditTriggerStartTime { get; set; }
    string EditTriggerType { get; set; }
    void FilterRecords(string? value);
    bool IsBusy(string key);
    string JobDialogTitle { get; }
    bool JobDialogVisible { get; set; }
    bool JobIsBuiltIn { get; }
    bool JobIsHttp { get; }
    bool JobIsScript { get; }
    string JobStatusTagForView(VueProps? value);
    string JobStatusTextForView(VueProps? value);
    void LoadClusters();
    void LoadRecords();
    void OpenCreateTrigger(VueProps? value);
    void OpenEditJob(VueProps? value);
    void OpenEditTrigger(VueProps? value);
    void OpenRecords(VueProps? value);
    double[] PageSizes { get; }
    void PauseJob(VueProps? value);
    void PauseTrigger(VueProps? value);
    bool RecordDialogVisible { get; set; }
    string RecordJobId { get; }
    bool RecordLoading { get; }
    int RecordPage { get; }
    int RecordPageSize { get; }
    JobTriggerRecordView[] RecordRows { get; }
    int RecordTotal { get; }
    string RecordTriggerId { get; }
    JobTriggerView[] RecordTriggerOptions { get; }
    bool RowBoolForView(VueProps? row, string key);
    JobView[] Rows { get; }
    string RowStringForView(VueProps? row, string key);
    void RunJob(VueProps? value);
    void SaveJob();
    void SaveTrigger();
    bool SavingJob { get; }
    bool SavingTrigger { get; }
    void StartJob(VueProps? value);
    void StartTrigger(VueProps? value);
    string TriggerDialogTitle { get; }
    bool TriggerDialogVisible { get; set; }
    bool TriggerIsCron { get; }
    JobTriggerView[] TriggerRows(VueProps? value);
    string TriggerStatusTagForView(VueProps? value);
    string TriggerStatusTextForRow(VueProps? value);
    string TriggerTypeTextForView(VueProps? value);
}
