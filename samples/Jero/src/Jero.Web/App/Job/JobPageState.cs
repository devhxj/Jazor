using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Job;

[ECMAScriptModule("components/job-page-state.js")]
public static class JobPageModule
{
    public static JobPageState Create() => new();

    public sealed class JobPageState : IJobContentState
    {
        public double[] pageSizes { get; private set; } = [10, 20, 50, 100];

        private JobView[] items = [];
        public JobView[] rows { get; private set; } = [];
        private string[] groupOptions = [];
        private string queryJobId = string.Empty;
        private string queryGroupName = string.Empty;
        private string queryDescription = string.Empty;
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private bool loading;
        private bool loadingGroups;
        private string busyKey = string.Empty;
        private string? error;

        public bool jobDialogVisible { get; set; }
        public bool savingJob { get; private set; }
        private string editJobDbId = string.Empty;
        public string editJobId { get; set; } = string.Empty;
        public string editJobGroupName { get; set; } = "default";
        private string editJobType = string.Empty;
        private string editJobAssemblyName = string.Empty;
        public string editJobDescription { get; set; } = string.Empty;
        public string editJobProperties { get; set; } = string.Empty;
        private string editJobUpdatedTime = string.Empty;
        public string editJobScriptCode { get; set; } = JobClient.DefaultScript;
        public int editJobCreateType { get; private set; } = 1;
        public bool editJobConcurrent { get; set; } = true;
        public bool editJobIncludeAnnotation { get; set; } = true;
        public bool editHttpPropertiesValid { get; private set; } = true;
        public string editHttpRequestUri { get; set; } = string.Empty;
        public string editHttpMethod { get; set; } = "GET";
        public string editHttpBody { get; set; } = string.Empty;

        public bool triggerDialogVisible { get; set; }
        public bool savingTrigger { get; private set; }
        private string editTriggerDbId = string.Empty;
        public string editTriggerJobId { get; private set; } = string.Empty;
        public string editTriggerId { get; set; } = string.Empty;
        public string editTriggerType { get; set; } = JobClient.PeriodTrigger;
        private string editTriggerAssemblyName = "Furion.Pure";
        public string editTriggerPeriod { get; set; } = "1000";
        public string editTriggerCron { get; set; } = string.Empty;
        public string editTriggerDescription { get; set; } = string.Empty;
        private int editTriggerStatus = 1;
        public string editTriggerStartTime { get; set; } = string.Empty;
        public string editTriggerEndTime { get; set; } = string.Empty;
        private string editTriggerLastRunTime = string.Empty;
        private string editTriggerNextRunTime = string.Empty;
        private string editTriggerNumberOfRuns = "0";
        public string editTriggerMaxNumberOfRuns { get; set; } = "0";
        private string editTriggerNumberOfErrors = "0";
        public string editTriggerMaxNumberOfErrors { get; set; } = "0";
        public string editTriggerNumRetries { get; set; } = "0";
        public string editTriggerRetryTimeout { get; set; } = "1000";
        public bool editTriggerStartNow { get; set; } = true;
        public bool editTriggerRunOnStart { get; set; } = true;
        public bool editTriggerResetOnlyOnce { get; set; } = true;
        private string editTriggerUpdatedTime = string.Empty;

        public bool recordDialogVisible { get; set; }
        public bool recordLoading { get; private set; }
        private JobTriggerRecordView[] recordItems = [];
        public JobTriggerRecordView[] recordRows { get; private set; } = [];
        public string recordJobId { get; private set; } = string.Empty;
        public string recordTriggerId { get; private set; } = string.Empty;
        public JobTriggerView[] recordTriggerOptions { get; private set; } = [];
        public int recordPage { get; private set; } = 1;
        public int recordPageSize { get; private set; } = 50;
        public int recordTotal { get; private set; }

        public bool clusterDialogVisible { get; set; }
        public bool clusterLoading { get; private set; }
        private JobClusterView[] clusterItems = [];
        public JobClusterView[] clusterRows { get; private set; } = [];

        private bool CanAdd => ApplicationState.HasPermission("sysJob:addJobDetail");
        public bool CanEdit => ApplicationState.HasPermission("sysJob:updateJobDetail");
        public bool CanDelete => ApplicationState.HasPermission("sysJob:deleteJobDetail");
        public bool Busy => busyKey.Length > 0 || savingJob || savingTrigger;
        public bool EditingJob => editJobDbId.Length > 0;
        public bool JobIsScript => editJobCreateType == 1;
        public bool JobIsHttp => editJobCreateType == 2;
        public bool JobIsBuiltIn => editJobCreateType == 0;
        public bool TriggerIsCron => editTriggerType == JobClient.CronTrigger;
        public string JobDialogTitle => EditingJob ? "编辑作业" : "添加作业";
        public string TriggerDialogTitle => editTriggerDbId.Length > 0 ? "编辑触发器" : "添加触发器";

        private readonly JeroNavigation Navigation = new();

        public void Initialize()
        {
            LoadPage();
            LoadGroups();
        }

        private void LoadPage()
        {
            if (loading)
                return;
            loading = true;
            JobClient.Page(new JobPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                jobId = queryJobId.Trim(),
                groupName = queryGroupName.Trim(),
                description = queryDescription.Trim()
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    items = [];
                    rows = [];
                    total = 0;
                    error = outcome.Error ?? "加载任务调度列表失败。";
                    return;
                }

                items = JobClient.ReadJobs(outcome.Data);
                rows = items;
                total = JobClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                items = [];
                rows = [];
                total = 0;
                error = "加载任务调度列表失败，请检查网络后重试。";
            });
        }

        private void LoadGroups()
        {
            if (loadingGroups)
                return;
            loadingGroups = true;
            JobClient.Groups().Then(outcome =>
            {
                loadingGroups = false;
                if (outcome.Ok)
                    groupOptions = JobClient.ReadGroups(outcome.Data);
            }, _ => loadingGroups = false);
        }

        private void RefreshAll()
        {
            LoadPage();
            LoadGroups();
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            queryJobId = string.Empty;
            queryGroupName = string.Empty;
            queryDescription = string.Empty;
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

        private void OpenCreateJob()
        {
            if (!CanAdd)
                return;
            editJobDbId = string.Empty;
            editJobId = string.Empty;
            editJobGroupName = "default";
            editJobType = string.Empty;
            editJobAssemblyName = string.Empty;
            editJobDescription = string.Empty;
            editJobProperties = string.Empty;
            editJobUpdatedTime = string.Empty;
            editJobScriptCode = JobClient.DefaultScript;
            editJobCreateType = 1;
            editJobConcurrent = true;
            editJobIncludeAnnotation = true;
            editHttpPropertiesValid = true;
            editHttpRequestUri = string.Empty;
            editHttpMethod = "GET";
            editHttpBody = string.Empty;
            error = null;
            jobDialogVisible = true;
        }

        public void OpenEditJob(VueProps? value)
        {
            if (!CanEdit)
                return;
            var item = FindJob(RowString(value, "jobId"));
            if (item is null)
                return;

            editJobDbId = item.id;
            editJobId = item.jobId;
            editJobGroupName = item.groupName.Length == 0 ? "default" : item.groupName;
            editJobType = item.jobType;
            editJobAssemblyName = item.assemblyName;
            editJobDescription = item.description;
            editJobProperties = item.properties;
            editJobUpdatedTime = item.updatedTime;
            editJobScriptCode = item.scriptCode.Length == 0 ? JobClient.DefaultScript : item.scriptCode;
            editJobCreateType = item.createType;
            editJobConcurrent = item.concurrent;
            editJobIncludeAnnotation = item.includeAnnotation;

            var http = JobClient.ParseHttpProperties(item.properties);
            editHttpPropertiesValid = http.valid;
            editHttpRequestUri = http.requestUri;
            editHttpMethod = http.method;
            editHttpBody = http.body;
            error = null;
            jobDialogVisible = true;
        }

        public void SaveJob()
        {
            if (savingJob || (EditingJob ? !CanEdit : !CanAdd))
                return;

            var jobId = editJobId.Trim();
            var groupName = editJobGroupName.Trim();
            var description = editJobDescription.Trim();
            if (jobId.Length < 2 || jobId.Length > 64)
            {
                error = "作业编号长度必须为 2 至 64 个字符。";
                return;
            }
            if (groupName.Length == 0 || groupName.Length > 128)
            {
                error = "组名称不能为空且不能超过 128 个字符。";
                return;
            }
            if (description.Length > 128)
            {
                error = "描述信息不能超过 128 个字符。";
                return;
            }
            if (JobIsScript && editJobScriptCode.Trim().Length == 0)
            {
                error = "脚本代码不能为空。";
                return;
            }
            if (JobIsHttp && editHttpRequestUri.Trim().Length == 0)
            {
                error = "HTTP 作业请求地址不能为空。";
                return;
            }

            var properties = JobIsHttp
                ? JobClient.BuildHttpProperties(new JobHttpDraft
                {
                    valid = true,
                    requestUri = editHttpRequestUri,
                    method = editHttpMethod,
                    body = editHttpBody
                })
                : editJobProperties.Trim();

            var input = new JobDetailSaveInput
            {
                id = editJobDbId,
                jobId = jobId,
                groupName = groupName,
                jobType = editJobType.Trim(),
                assemblyName = editJobAssemblyName.Trim(),
                description = description,
                concurrent = editJobConcurrent,
                includeAnnotation = editJobIncludeAnnotation,
                properties = properties,
                updatedTime = editJobUpdatedTime,
                createType = editJobCreateType,
                scriptCode = JobIsScript ? editJobScriptCode : string.Empty
            };

            savingJob = true;
            var request = EditingJob ? JobClient.UpdateDetail(input) : JobClient.AddDetail(input);
            request.Then(outcome =>
            {
                savingJob = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存作业失败。";
                    return;
                }

                jobDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(EditingJob ? "作业已更新" : "作业已新增");
                LoadPage();
                LoadGroups();
            }, _ =>
            {
                savingJob = false;
                error = "保存作业失败，请检查网络后重试。";
            });
        }

        public void DeleteJob(VueProps? value)
        {
            var item = FindJob(RowString(value, "jobId"));
            if (!CanDelete || item is null || !BeginAction("delete-job:" + item.jobId))
                return;
            CompleteActionAndReloadGroups(JobClient.DeleteDetail(item.jobId), "作业已删除");
        }

        public void OpenCreateTrigger(VueProps? value)
        {
            var job = FindJob(RowString(value, "jobId"));
            if (job is null)
                return;

            editTriggerDbId = string.Empty;
            editTriggerJobId = job.jobId;
            editTriggerId = string.Empty;
            editTriggerType = JobClient.PeriodTrigger;
            editTriggerAssemblyName = "Furion.Pure";
            editTriggerPeriod = "1000";
            editTriggerCron = string.Empty;
            editTriggerDescription = string.Empty;
            editTriggerStatus = 1;
            editTriggerStartTime = string.Empty;
            editTriggerEndTime = string.Empty;
            editTriggerLastRunTime = string.Empty;
            editTriggerNextRunTime = string.Empty;
            editTriggerNumberOfRuns = "0";
            editTriggerMaxNumberOfRuns = "0";
            editTriggerNumberOfErrors = "0";
            editTriggerMaxNumberOfErrors = "0";
            editTriggerNumRetries = "0";
            editTriggerRetryTimeout = "1000";
            editTriggerStartNow = true;
            editTriggerRunOnStart = true;
            editTriggerResetOnlyOnce = true;
            editTriggerUpdatedTime = string.Empty;
            error = null;
            triggerDialogVisible = true;
        }

        public void OpenEditTrigger(VueProps? value)
        {
            var trigger = FindTrigger(RowString(value, "jobId"), RowString(value, "triggerId"));
            if (trigger is null)
                return;

            var args = JobClient.ParseTriggerArgs(trigger.triggerType, trigger.args);
            editTriggerDbId = trigger.id;
            editTriggerJobId = trigger.jobId;
            editTriggerId = trigger.triggerId;
            editTriggerType = trigger.triggerType.Length == 0 ? JobClient.PeriodTrigger : trigger.triggerType;
            editTriggerAssemblyName = trigger.assemblyName.Length == 0 ? "Furion.Pure" : trigger.assemblyName;
            editTriggerPeriod = args.period;
            editTriggerCron = args.cron;
            editTriggerDescription = trigger.description;
            editTriggerStatus = trigger.status;
            editTriggerStartTime = trigger.startTime;
            editTriggerEndTime = trigger.endTime;
            editTriggerLastRunTime = trigger.lastRunTime;
            editTriggerNextRunTime = trigger.nextRunTime;
            editTriggerNumberOfRuns = ValueOrZero(trigger.numberOfRuns);
            editTriggerMaxNumberOfRuns = ValueOrZero(trigger.maxNumberOfRuns);
            editTriggerNumberOfErrors = ValueOrZero(trigger.numberOfErrors);
            editTriggerMaxNumberOfErrors = ValueOrZero(trigger.maxNumberOfErrors);
            editTriggerNumRetries = trigger.numRetries.ToString();
            editTriggerRetryTimeout = trigger.retryTimeout.ToString();
            editTriggerStartNow = trigger.startNow;
            editTriggerRunOnStart = trigger.runOnStart;
            editTriggerResetOnlyOnce = trigger.resetOnlyOnce;
            editTriggerUpdatedTime = trigger.updatedTime;
            error = null;
            triggerDialogVisible = true;
        }

        public void ApplyCronMacro(string value)
        {
            if (value.Length > 0)
                editTriggerCron = value;
        }

        public void SaveTrigger()
        {
            if (savingTrigger)
                return;

            var triggerId = editTriggerId.Trim();
            if (editTriggerJobId.Trim().Length < 2)
            {
                error = "作业编号不能为空。";
                return;
            }
            if (triggerId.Length < 2 || triggerId.Length > 64)
            {
                error = "触发器编号长度必须为 2 至 64 个字符。";
                return;
            }
            if (editTriggerDescription.Trim().Length > 128)
            {
                error = "触发器描述不能超过 128 个字符。";
                return;
            }
            if (TriggerIsCron && editTriggerCron.Trim().Length == 0)
            {
                error = "Cron 表达式不能为空。";
                return;
            }

            var period = PositiveWhole(editTriggerPeriod);
            var numberOfRuns = NonNegativeLongText(editTriggerNumberOfRuns);
            var maxRuns = NonNegativeLongText(editTriggerMaxNumberOfRuns);
            var numberOfErrors = NonNegativeLongText(editTriggerNumberOfErrors);
            var maxErrors = NonNegativeLongText(editTriggerMaxNumberOfErrors);
            var retries = NonNegativeInt(editTriggerNumRetries);
            var retryTimeout = NonNegativeInt(editTriggerRetryTimeout);
            if (!TriggerIsCron && (period is null || period.Value < 1000))
            {
                error = "间隔时间必须是至少 1000 毫秒的整数。";
                return;
            }
            if (numberOfRuns is null || maxRuns is null || numberOfErrors is null || maxErrors is null || retries is null || retryTimeout is null)
            {
                error = "触发次数、错误次数和重试配置必须是非负整数。";
                return;
            }

            var input = new JobTriggerSaveInput
            {
                id = editTriggerDbId,
                triggerId = triggerId,
                jobId = editTriggerJobId.Trim(),
                triggerType = editTriggerType,
                assemblyName = editTriggerAssemblyName.Trim(),
                args = JobClient.BuildTriggerArgs(editTriggerType, editTriggerPeriod, editTriggerCron),
                description = editTriggerDescription.Trim(),
                status = editTriggerStatus,
                startTime = Optional(editTriggerStartTime),
                endTime = Optional(editTriggerEndTime),
                lastRunTime = Optional(editTriggerLastRunTime),
                nextRunTime = Optional(editTriggerNextRunTime),
                numberOfRuns = numberOfRuns,
                maxNumberOfRuns = maxRuns,
                numberOfErrors = numberOfErrors,
                maxNumberOfErrors = maxErrors,
                numRetries = retries.Value,
                retryTimeout = retryTimeout.Value,
                startNow = editTriggerStartNow,
                runOnStart = editTriggerRunOnStart,
                resetOnlyOnce = editTriggerResetOnlyOnce,
                updatedTime = Optional(editTriggerUpdatedTime)
            };

            savingTrigger = true;
            var request = editTriggerDbId.Length > 0 ? JobClient.UpdateTrigger(input) : JobClient.AddTrigger(input);
            request.Then(outcome =>
            {
                savingTrigger = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存触发器失败。";
                    return;
                }

                triggerDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editTriggerDbId.Length > 0 ? "触发器已更新" : "触发器已新增");
                LoadPage();
            }, _ =>
            {
                savingTrigger = false;
                error = "保存触发器失败，请检查网络后重试。";
            });
        }

        public void DeleteTrigger(VueProps? value)
        {
            var trigger = FindTrigger(RowString(value, "jobId"), RowString(value, "triggerId"));
            if (trigger is null || !BeginAction("delete-trigger:" + trigger.jobId + ":" + trigger.triggerId))
                return;
            CompleteAction(JobClient.DeleteTrigger(trigger.jobId, trigger.triggerId), "触发器已删除");
        }

        private void StartAll()
        {
            if (!BeginAction("start-all")) return;
            CompleteAction(JobClient.StartAll(), "已启动所有作业");
        }

        private void PauseAll()
        {
            if (!BeginAction("pause-all")) return;
            CompleteAction(JobClient.PauseAll(), "已暂停所有作业");
        }

        private void CancelSleep()
        {
            if (!BeginAction("cancel-sleep")) return;
            CompleteAction(JobClient.CancelSleep(), "已强制唤醒作业调度器");
        }

        private void PersistAll()
        {
            if (!BeginAction("persist-all")) return;
            CompleteAction(JobClient.PersistAll(), "已触发所有作业持久化");
        }

        public void RunJob(VueProps? value)
        {
            var jobId = RowString(value, "jobId");
            if (jobId.Length == 0 || !BeginAction("run-job:" + jobId)) return;
            CompleteAction(JobClient.RunJob(jobId), "作业已执行");
        }

        public void StartJob(VueProps? value)
        {
            var jobId = RowString(value, "jobId");
            if (jobId.Length == 0 || !BeginAction("start-job:" + jobId)) return;
            CompleteAction(JobClient.StartJob(jobId), "作业已启动");
        }

        public void PauseJob(VueProps? value)
        {
            var jobId = RowString(value, "jobId");
            if (jobId.Length == 0 || !BeginAction("pause-job:" + jobId)) return;
            CompleteAction(JobClient.PauseJob(jobId), "作业已暂停");
        }

        public void CancelJob(VueProps? value)
        {
            var jobId = RowString(value, "jobId");
            if (jobId.Length == 0 || !BeginAction("cancel-job:" + jobId)) return;
            CompleteAction(JobClient.CancelJob(jobId), "作业已取消");
        }

        public void StartTrigger(VueProps? value)
        {
            var jobId = RowString(value, "jobId");
            var triggerId = RowString(value, "triggerId");
            if (jobId.Length == 0 || triggerId.Length == 0 || !BeginAction("start-trigger:" + jobId + ":" + triggerId)) return;
            CompleteAction(JobClient.StartTrigger(jobId, triggerId), "触发器已启动");
        }

        public void PauseTrigger(VueProps? value)
        {
            var jobId = RowString(value, "jobId");
            var triggerId = RowString(value, "triggerId");
            if (jobId.Length == 0 || triggerId.Length == 0 || !BeginAction("pause-trigger:" + jobId + ":" + triggerId)) return;
            CompleteAction(JobClient.PauseTrigger(jobId, triggerId), "触发器已暂停");
        }

        private bool BeginAction(string key)
        {
            if (Busy)
                return false;
            busyKey = key;
            error = null;
            return true;
        }

        private void CompleteAction(IPromise<ApiOutcome> request, string successMessage)
        {
            request.Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "任务调度操作失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success(successMessage);
                LoadPage();
            }, _ =>
            {
                busyKey = string.Empty;
                error = "任务调度操作失败，请检查网络后重试。";
            });
        }

        private void CompleteActionAndReloadGroups(IPromise<ApiOutcome> request, string successMessage)
        {
            request.Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "任务调度操作失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success(successMessage);
                LoadPage();
                LoadGroups();
            }, _ =>
            {
                busyKey = string.Empty;
                error = "任务调度操作失败，请检查网络后重试。";
            });
        }

        public void OpenRecords(VueProps? value)
        {
            var job = FindJob(RowString(value, "jobId"));
            if (job is null)
                return;
            recordJobId = job.jobId;
            recordTriggerId = string.Empty;
            recordTriggerOptions = job.triggers;
            recordPage = 1;
            recordPageSize = 50;
            recordItems = [];
            recordRows = [];
            recordTotal = 0;
            recordDialogVisible = true;
            LoadRecords();
        }

        public void LoadRecords()
        {
            if (recordLoading || recordJobId.Length == 0)
                return;
            recordLoading = true;
            JobClient.TriggerRecords(new JobTriggerRecordPageInput
            {
                page = recordPage,
                pageSize = recordPageSize,
                jobId = recordJobId,
                triggerId = recordTriggerId.Trim()
            }).Then(outcome =>
            {
                recordLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载触发器运行记录失败。";
                    return;
                }
                recordItems = JobClient.ReadRecords(outcome.Data);
                recordRows = recordItems;
                recordTotal = JobClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                recordLoading = false;
                error = "加载触发器运行记录失败，请检查网络后重试。";
            });
        }

        public void FilterRecords(string? value)
        {
            recordTriggerId = value ?? string.Empty;
            recordPage = 1;
            LoadRecords();
        }

        public void ChangeRecordPage(Number value)
        {
            recordPage = (int)value;
            LoadRecords();
        }

        public void ChangeRecordPageSize(Number value)
        {
            recordPageSize = (int)value;
            recordPage = 1;
            LoadRecords();
        }

        public void ClearRecords()
        {
            if (recordLoading || !BeginAction("clear-records"))
                return;
            recordLoading = true;
            JobClient.ClearRecords().Then(outcome =>
            {
                busyKey = string.Empty;
                recordLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "清空运行记录失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("运行记录已清空");
                LoadRecords();
            }, _ =>
            {
                busyKey = string.Empty;
                recordLoading = false;
                error = "清空运行记录失败，请检查网络后重试。";
            });
        }

        private void OpenClusters()
        {
            clusterDialogVisible = true;
            LoadClusters();
        }

        public void LoadClusters()
        {
            if (clusterLoading)
                return;
            clusterLoading = true;
            JobClient.Clusters().Then(outcome =>
            {
                clusterLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载作业集群失败。";
                    return;
                }
                clusterItems = JobClient.ReadClusters(outcome.Data);
                clusterRows = clusterItems;
                error = null;
            }, _ =>
            {
                clusterLoading = false;
                error = "加载作业集群失败，请检查网络后重试。";
            });
        }

        private void OpenDashboard()
            => Navigation.NavigateTo(Navigation.BaseUri + "job/dashboard");

        private JobView? FindJob(string jobId)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].jobId == jobId)
                    return items[index];
            return null;
        }

        private JobTriggerView? FindTrigger(string jobId, string triggerId)
        {
            var job = FindJob(jobId);
            if (job is null)
                return null;
            for (var index = 0; index < job.triggers.Length; index++)
                if (job.triggers[index].triggerId == triggerId)
                    return job.triggers[index];
            return null;
        }

        public JobTriggerView[] TriggerRows(VueProps? value)
            => FindJob(RowString(value, "jobId"))?.triggers ?? [];

        private static string JobStatusText(VueProps? value)
        {
            var text = RowString(value, "jobStatusText");
            if (text.Length > 0)
                return text;
            var status = RowString(value, "jobStatus");
            if (status == "running") return "运行中";
            if (status == "paused") return "已暂停";
            if (status == "started") return "已启动";
            if (status == "stopped") return "已停止";
            if (status == "notLoaded") return "未加载";
            if (status == "notStarted") return "未启动";
            if (status == "noTrigger") return "无触发器";
            if (status == "abnormal") return "异常";
            return "未知";
        }

        private static string JobStatusTag(VueProps? value)
        {
            var status = RowString(value, "jobStatus");
            if (status == "running" || status == "started") return "success";
            if (status == "paused" || status == "abnormal") return "danger";
            if (status == "notLoaded" || status == "notStarted") return "warning";
            return "info";
        }

        private static string CreateTypeText(VueProps? value)
            => CreateTypeText(RowInt(value, "createType", 0));

        private static string CreateTypeText(int createType)
            => createType == 1 ? "脚本" : createType == 2 ? "HTTP请求" : "内置";

        private static string CreateTypeTag(VueProps? value)
        {
            var createType = RowInt(value, "createType", 0);
            return createType == 1 ? "warning" : createType == 2 ? "success" : "info";
        }

        private static string TriggerTypeText(VueProps? value)
            => RowString(value, "triggerType") == JobClient.CronTrigger ? "Cron" : "间隔";

        private static string TriggerStatusText(VueProps? value)
            => TriggerStatusText(RowInt(value, "status", 1));

        private static string TriggerStatusText(int status)
        {
            if (status == 0) return "积压";
            if (status == 1) return "就绪";
            if (status == 2) return "正在运行";
            if (status == 3) return "暂停";
            if (status == 4) return "阻塞";
            if (status == 5) return "由失败进入就绪";
            if (status == 6) return "归档";
            if (status == 7) return "崩溃";
            if (status == 8) return "超限";
            if (status == 9) return "无触发时间";
            if (status == 10) return "未启动";
            if (status == 11) return "未知作业触发器";
            if (status == 12) return "未知作业处理程序";
            return "未知";
        }

        private static string TriggerStatusTag(VueProps? value)
        {
            var status = RowInt(value, "status", 1);
            if (status == 2) return "success";
            if (status == 1) return "info";
            if (status == 0 || status == 5 || status == 10) return "warning";
            return "danger";
        }

        private static string ClusterStatusText(VueProps? value)
        {
            var status = RowInt(value, "status", 0);
            return status == 1 ? "工作中" : status == 2 ? "等待唤醒" : "宕机";
        }

        private static string ClusterStatusTag(VueProps? value)
        {
            var status = RowInt(value, "status", 0);
            return status == 1 ? "success" : status == 2 ? "warning" : "danger";
        }

        private static string DisplayValue(string value)
            => value.Length == 0 ? "-" : value;

        private static string ValueOrZero(string value)
            => value.Length == 0 ? "0" : value;

        private static string? Optional(string value)
        {
            var normalized = value.Trim();
            return normalized.Length == 0 ? null : normalized;
        }

        private static double? PositiveWhole(string value)
        {
            var normalized = value.Trim();
            if (normalized.Length == 0)
                return null;
            var number = (double)NumberValue(normalized);
            return IsFinite(number) && number > 0 && number % 1 == 0 ? number : null;
        }

        private static double? NonNegativeWhole(string value)
        {
            var normalized = value.Trim();
            if (normalized.Length == 0)
                return null;
            var number = (double)NumberValue(normalized);
            return IsFinite(number) && number >= 0 && number % 1 == 0 ? number : null;
        }

        private static string? NonNegativeLongText(string value)
        {
            var normalized = value.Trim();
            if (normalized.Length == 0)
                return null;

            for (var index = 0; index < normalized.Length; index++)
                if (normalized[index] < '0' || normalized[index] > '9')
                    return null;

            var firstDigit = 0;
            while (firstDigit < normalized.Length - 1 && normalized[firstDigit] == '0')
                firstDigit++;
            var canonical = normalized.Substring(firstDigit);
            if (canonical.Length > 19)
                return null;

            // Keep long values as decimal text across the browser boundary. Furion's configured
            // The server long converter accepts string tokens and preserves integers beyond JavaScript safe precision.
            const string maxLong = "9223372036854775807";
            if (canonical.Length == maxLong.Length)
            {
                for (var index = 0; index < canonical.Length; index++)
                {
                    if (canonical[index] > maxLong[index])
                        return null;
                    if (canonical[index] < maxLong[index])
                        break;
                }
            }

            return canonical;
        }

        private static int? NonNegativeInt(string value)
        {
            var number = NonNegativeWhole(value);
            return number is null || number.Value > 2147483647 ? null : (int)number.Value;
        }

        public bool IsBusy(string key)
            => busyKey == key;

        private static string RowString(VueProps? row, string key)
        {
            if (row is null)
                return string.Empty;
            var value = Reflect.Get(row, key);
            if (value is null)
                return string.Empty;
            var text = StringValue(value);
            return text == "null" || text == "undefined" ? string.Empty : text;
        }

        private static int RowInt(VueProps? row, string key, int fallback)
        {
            if (row is null)
                return fallback;
            var value = Reflect.Get(row, key);
            return value is null ? fallback : (int)NumberValue(value);
        }

        private static bool RowBool(VueProps? row, string key)
        {
            if (row is null)
                return false;
            var value = Reflect.Get(row, key);
            return value is not null && BooleanValue(value);
        }

        public string JobStatusTextForView(VueProps? value) => JobStatusText(value);
        public string JobStatusTagForView(VueProps? value) => JobStatusTag(value);
        public string CreateTypeTextForRow(VueProps? value) => CreateTypeText(value);
        public string CreateTypeTextForCode(int createType) => CreateTypeText(createType);
        public string CreateTypeTagForView(VueProps? value) => CreateTypeTag(value);
        public string TriggerTypeTextForView(VueProps? value) => TriggerTypeText(value);
        public string TriggerStatusTextForRow(VueProps? value) => TriggerStatusText(value);
        public string TriggerStatusTagForView(VueProps? value) => TriggerStatusTag(value);
        public string ClusterStatusTextForView(VueProps? value) => ClusterStatusText(value);
        public string ClusterStatusTagForView(VueProps? value) => ClusterStatusTag(value);
        public string DisplayValueForView(string value) => DisplayValue(value);
        public string RowStringForView(VueProps? row, string key) => RowString(row, key);
        public bool RowBoolForView(VueProps? row, string key) => RowBool(row, key);

        public JobPageProps CreateProps() => new()
        {
            SetQueryGroupName = value => queryGroupName = value,
            SetQueryJobId = value => queryJobId = value,
            SetQueryDescription = value => queryDescription = value,
            RefreshAll = RefreshAll,
            DismissError = () => error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreateJob = OpenCreateJob,
            StartAll = StartAll,
            PauseAll = PauseAll,
            CancelSleep = CancelSleep,
            PersistAll = PersistAll,
            OpenClusters = OpenClusters,
            OpenDashboard = OpenDashboard,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            StartAllBusy = IsBusy("start-all"),
            PauseAllBusy = IsBusy("pause-all"),
            CancelSleepBusy = IsBusy("cancel-sleep"),
            PersistAllBusy = IsBusy("persist-all"),
            Loading = loading,
            Error = error,
            QueryJobId = queryJobId,
            QueryDescription = queryDescription,
            QueryGroupName = queryGroupName,
            LoadingGroups = loadingGroups,
            GroupOptions = groupOptions,
            Total = total,
            ClusterLoading = clusterLoading,
            CurrentPage = currentPage,
            PageSize = pageSize,
            CanAdd = CanAdd,
            Busy = Busy,
        };
    }
}

public sealed record JobPageProps : VueProps
{
    public required Action<string> SetQueryGroupName { get; init; }
    public required Action<string> SetQueryJobId { get; init; }
    public required Action<string> SetQueryDescription { get; init; }
    public required Action RefreshAll { get; init; }
    public required Action DismissError { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreateJob { get; init; }
    public required Action StartAll { get; init; }
    public required Action PauseAll { get; init; }
    public required Action CancelSleep { get; init; }
    public required Action PersistAll { get; init; }
    public required Action OpenClusters { get; init; }
    public required Action OpenDashboard { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required bool StartAllBusy { get; init; }
    public required bool PauseAllBusy { get; init; }
    public required bool CancelSleepBusy { get; init; }
    public required bool PersistAllBusy { get; init; }
    public required bool Loading { get; init; }
    public required string? Error { get; init; }
    public required string QueryJobId { get; init; }
    public required string QueryDescription { get; init; }
    public required string QueryGroupName { get; init; }
    public required bool LoadingGroups { get; init; }
    public required string[] GroupOptions { get; init; }
    public required int Total { get; init; }
    public required bool ClusterLoading { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required bool CanAdd { get; init; }
    public required bool Busy { get; init; }
}
