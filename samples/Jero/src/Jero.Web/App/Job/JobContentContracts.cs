using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.App.Job;

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
    bool clusterDialogVisible { get; set; }
    bool clusterLoading { get; }
    JobClusterView[] clusterRows { get; }
    string ClusterStatusTagForView(VueProps? value);
    string ClusterStatusTextForView(VueProps? value);
    string CreateTypeTagForView(VueProps? value);
    string CreateTypeTextForCode(int createType);
    string CreateTypeTextForRow(VueProps? value);
    void DeleteJob(VueProps? value);
    void DeleteTrigger(VueProps? value);
    string DisplayValueForView(string value);
    string editHttpBody { get; set; }
    string editHttpMethod { get; set; }
    bool editHttpPropertiesValid { get; }
    string editHttpRequestUri { get; set; }
    bool EditingJob { get; }
    bool editJobConcurrent { get; set; }
    int editJobCreateType { get; }
    string editJobDescription { get; set; }
    string editJobGroupName { get; set; }
    string editJobId { get; set; }
    bool editJobIncludeAnnotation { get; set; }
    string editJobProperties { get; set; }
    string editJobScriptCode { get; set; }
    string editTriggerCron { get; set; }
    string editTriggerDescription { get; set; }
    string editTriggerEndTime { get; set; }
    string editTriggerId { get; set; }
    string editTriggerJobId { get; }
    string editTriggerMaxNumberOfErrors { get; set; }
    string editTriggerMaxNumberOfRuns { get; set; }
    string editTriggerNumRetries { get; set; }
    string editTriggerPeriod { get; set; }
    bool editTriggerResetOnlyOnce { get; set; }
    string editTriggerRetryTimeout { get; set; }
    bool editTriggerRunOnStart { get; set; }
    bool editTriggerStartNow { get; set; }
    string editTriggerStartTime { get; set; }
    string editTriggerType { get; set; }
    void FilterRecords(string? value);
    bool IsBusy(string key);
    string JobDialogTitle { get; }
    bool jobDialogVisible { get; set; }
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
    double[] pageSizes { get; }
    void PauseJob(VueProps? value);
    void PauseTrigger(VueProps? value);
    bool recordDialogVisible { get; set; }
    string recordJobId { get; }
    bool recordLoading { get; }
    int recordPage { get; }
    int recordPageSize { get; }
    JobTriggerRecordView[] recordRows { get; }
    int recordTotal { get; }
    string recordTriggerId { get; }
    JobTriggerView[] recordTriggerOptions { get; }
    bool RowBoolForView(VueProps? row, string key);
    JobView[] rows { get; }
    string RowStringForView(VueProps? row, string key);
    void RunJob(VueProps? value);
    void SaveJob();
    void SaveTrigger();
    bool savingJob { get; }
    bool savingTrigger { get; }
    void StartJob(VueProps? value);
    void StartTrigger(VueProps? value);
    string TriggerDialogTitle { get; }
    bool triggerDialogVisible { get; set; }
    bool TriggerIsCron { get; }
    JobTriggerView[] TriggerRows(VueProps? value);
    string TriggerStatusTagForView(VueProps? value);
    string TriggerStatusTextForRow(VueProps? value);
    string TriggerTypeTextForView(VueProps? value);
}
