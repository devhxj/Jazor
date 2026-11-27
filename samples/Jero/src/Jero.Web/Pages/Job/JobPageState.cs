using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Job;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Job;

[ECMAScriptModule("components/job-page-state.js")]
public static class JobPageModule
{
    public static JobPageState Create() => new();

    public sealed class JobPageState : IJobContentState, Jero.Web.Composition.IPageState<JobPageProps>
    {
        public double[] PageSizes { get; private set; } = [10, 20, 50, 100];

        private JobView[] _items = [];
        public JobView[] Rows { get; private set; } = [];
        private string[] _groupOptions = [];
        private string _queryJobId = string.Empty;
        private string _queryGroupName = string.Empty;
        private string _queryDescription = string.Empty;
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private bool _loading;
        private bool _loadingGroups;
        private string _busyKey = string.Empty;
        private string? _error;

        public bool JobDialogVisible { get; set; }
        public bool SavingJob { get; private set; }
        private string _editJobDbId = string.Empty;
        public string EditJobId { get; set; } = string.Empty;
        public string EditJobGroupName { get; set; } = "default";
        private string _editJobType = string.Empty;
        private string _editJobAssemblyName = string.Empty;
        public string EditJobDescription { get; set; } = string.Empty;
        public string EditJobProperties { get; set; } = string.Empty;
        private string _editJobUpdatedTime = string.Empty;
        public string EditJobScriptCode { get; set; } = JobClient.DefaultScript;
        public int EditJobCreateType { get; private set; } = 1;
        public bool EditJobConcurrent { get; set; } = true;
        public bool EditJobIncludeAnnotation { get; set; } = true;
        public bool EditHttpPropertiesValid { get; private set; } = true;
        public string EditHttpRequestUri { get; set; } = string.Empty;
        public string EditHttpMethod { get; set; } = "GET";
        public string EditHttpBody { get; set; } = string.Empty;

        public bool TriggerDialogVisible { get; set; }
        public bool SavingTrigger { get; private set; }
        private string _editTriggerDbId = string.Empty;
        public string EditTriggerJobId { get; private set; } = string.Empty;
        public string EditTriggerId { get; set; } = string.Empty;
        public string EditTriggerType { get; set; } = JobClient.PeriodTrigger;
        private string _editTriggerAssemblyName = "Furion.Pure";
        public string EditTriggerPeriod { get; set; } = "1000";
        public string EditTriggerCron { get; set; } = string.Empty;
        public string EditTriggerDescription { get; set; } = string.Empty;
        private int _editTriggerStatus = 1;
        public string EditTriggerStartTime { get; set; } = string.Empty;
        public string EditTriggerEndTime { get; set; } = string.Empty;
        private string _editTriggerLastRunTime = string.Empty;
        private string _editTriggerNextRunTime = string.Empty;
        private string _editTriggerNumberOfRuns = "0";
        public string EditTriggerMaxNumberOfRuns { get; set; } = "0";
        private string _editTriggerNumberOfErrors = "0";
        public string EditTriggerMaxNumberOfErrors { get; set; } = "0";
        public string EditTriggerNumRetries { get; set; } = "0";
        public string EditTriggerRetryTimeout { get; set; } = "1000";
        public bool EditTriggerStartNow { get; set; } = true;
        public bool EditTriggerRunOnStart { get; set; } = true;
        public bool EditTriggerResetOnlyOnce { get; set; } = true;
        private string _editTriggerUpdatedTime = string.Empty;

        public bool RecordDialogVisible { get; set; }
        public bool RecordLoading { get; private set; }
        private JobTriggerRecordView[] _recordItems = [];
        public JobTriggerRecordView[] RecordRows { get; private set; } = [];
        public string RecordJobId { get; private set; } = string.Empty;
        public string RecordTriggerId { get; private set; } = string.Empty;
        public JobTriggerView[] RecordTriggerOptions { get; private set; } = [];
        public int RecordPage { get; private set; } = 1;
        public int RecordPageSize { get; private set; } = 50;
        public int RecordTotal { get; private set; }

        public bool ClusterDialogVisible { get; set; }
        public bool ClusterLoading { get; private set; }
        private JobClusterView[] _clusterItems = [];
        public JobClusterView[] ClusterRows { get; private set; } = [];

        private bool CanAdd => ApplicationState.HasPermission("sysJob:addJobDetail");
        public bool CanEdit => ApplicationState.HasPermission("sysJob:updateJobDetail");
        public bool CanDelete => ApplicationState.HasPermission("sysJob:deleteJobDetail");
        public bool Busy => _busyKey.Length > 0 || SavingJob || SavingTrigger;
        public bool EditingJob => _editJobDbId.Length > 0;
        public bool JobIsScript => EditJobCreateType == 1;
        public bool JobIsHttp => EditJobCreateType == 2;
        public bool JobIsBuiltIn => EditJobCreateType == 0;
        public bool TriggerIsCron => EditTriggerType == JobClient.CronTrigger;
        public string JobDialogTitle => EditingJob ? "编辑作业" : "添加作业";
        public string TriggerDialogTitle => _editTriggerDbId.Length > 0 ? "编辑触发器" : "添加触发器";

        private readonly JeroNavigation _navigation = new();

        public void Initialize()
        {
            LoadPage();
            LoadGroups();
        }

        private void LoadPage()
        {
            if (_loading)
                return;
            _loading = true;
            JobClient.Page(new JobPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                JobId = _queryJobId.Trim(),
                GroupName = _queryGroupName.Trim(),
                Description = _queryDescription.Trim()
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _items = [];
                    Rows = [];
                    _total = 0;
                    _error = outcome.Error ?? "加载任务调度列表失败。";
                    return;
                }

                _items = JobClient.ReadJobs(outcome.Data);
                Rows = _items;
                _total = JobClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _items = [];
                Rows = [];
                _total = 0;
                _error = "加载任务调度列表失败，请检查网络后重试。";
            });
        }

        private void LoadGroups()
        {
            if (_loadingGroups)
                return;
            _loadingGroups = true;
            JobClient.Groups().Then(outcome =>
            {
                _loadingGroups = false;
                if (outcome.Ok)
                    _groupOptions = JobClient.ReadGroups(outcome.Data);
            }, _ => _loadingGroups = false);
        }

        private void RefreshAll()
        {
            LoadPage();
            LoadGroups();
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _queryJobId = string.Empty;
            _queryGroupName = string.Empty;
            _queryDescription = string.Empty;
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

        private void OpenCreateJob()
        {
            if (!CanAdd)
                return;
            _editJobDbId = string.Empty;
            EditJobId = string.Empty;
            EditJobGroupName = "default";
            _editJobType = string.Empty;
            _editJobAssemblyName = string.Empty;
            EditJobDescription = string.Empty;
            EditJobProperties = string.Empty;
            _editJobUpdatedTime = string.Empty;
            EditJobScriptCode = JobClient.DefaultScript;
            EditJobCreateType = 1;
            EditJobConcurrent = true;
            EditJobIncludeAnnotation = true;
            EditHttpPropertiesValid = true;
            EditHttpRequestUri = string.Empty;
            EditHttpMethod = "GET";
            EditHttpBody = string.Empty;
            _error = null;
            JobDialogVisible = true;
        }

        public void OpenEditJob(VueProps? value)
        {
            if (!CanEdit)
                return;
            var item = FindJob(RowString(value, "jobId"));
            if (item is null)
                return;

            _editJobDbId = item.Id;
            EditJobId = item.JobId;
            EditJobGroupName = item.GroupName.Length == 0 ? "default" : item.GroupName;
            _editJobType = item.JobType;
            _editJobAssemblyName = item.AssemblyName;
            EditJobDescription = item.Description;
            EditJobProperties = item.Properties;
            _editJobUpdatedTime = item.UpdatedTime;
            EditJobScriptCode = item.ScriptCode.Length == 0 ? JobClient.DefaultScript : item.ScriptCode;
            EditJobCreateType = item.CreateType;
            EditJobConcurrent = item.Concurrent;
            EditJobIncludeAnnotation = item.IncludeAnnotation;

            var http = JobClient.ParseHttpProperties(item.Properties);
            EditHttpPropertiesValid = http.Valid;
            EditHttpRequestUri = http.RequestUri;
            EditHttpMethod = http.Method;
            EditHttpBody = http.Body;
            _error = null;
            JobDialogVisible = true;
        }

        public void SaveJob()
        {
            if (SavingJob || (EditingJob ? !CanEdit : !CanAdd))
                return;

            var jobId = EditJobId.Trim();
            var groupName = EditJobGroupName.Trim();
            var description = EditJobDescription.Trim();
            if (jobId.Length < 2 || jobId.Length > 64)
            {
                _error = "作业编号长度必须为 2 至 64 个字符。";
                return;
            }
            if (groupName.Length == 0 || groupName.Length > 128)
            {
                _error = "组名称不能为空且不能超过 128 个字符。";
                return;
            }
            if (description.Length > 128)
            {
                _error = "描述信息不能超过 128 个字符。";
                return;
            }
            if (JobIsScript && EditJobScriptCode.Trim().Length == 0)
            {
                _error = "脚本代码不能为空。";
                return;
            }
            if (JobIsHttp && EditHttpRequestUri.Trim().Length == 0)
            {
                _error = "HTTP 作业请求地址不能为空。";
                return;
            }

            var properties = JobIsHttp
                ? JobClient.BuildHttpProperties(new JobHttpDraft
                {
                    Valid = true,
                    RequestUri = EditHttpRequestUri,
                    Method = EditHttpMethod,
                    Body = EditHttpBody
                })
                : EditJobProperties.Trim();

            var input = new JobDetailSaveInput
            {
                Id = _editJobDbId,
                JobId = jobId,
                GroupName = groupName,
                JobType = _editJobType.Trim(),
                AssemblyName = _editJobAssemblyName.Trim(),
                Description = description,
                Concurrent = EditJobConcurrent,
                IncludeAnnotation = EditJobIncludeAnnotation,
                Properties = properties,
                UpdatedTime = _editJobUpdatedTime,
                CreateType = EditJobCreateType,
                ScriptCode = JobIsScript ? EditJobScriptCode : string.Empty
            };

            SavingJob = true;
            var request = EditingJob ? JobClient.UpdateDetail(input) : JobClient.AddDetail(input);
            request.Then(outcome =>
            {
                SavingJob = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存作业失败。";
                    return;
                }

                JobDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(EditingJob ? "作业已更新" : "作业已新增");
                LoadPage();
                LoadGroups();
            }, _ =>
            {
                SavingJob = false;
                _error = "保存作业失败，请检查网络后重试。";
            });
        }

        public void DeleteJob(VueProps? value)
        {
            var item = FindJob(RowString(value, "jobId"));
            if (!CanDelete || item is null || !BeginAction("delete-job:" + item.JobId))
                return;
            CompleteActionAndReloadGroups(JobClient.DeleteDetail(item.JobId), "作业已删除");
        }

        public void OpenCreateTrigger(VueProps? value)
        {
            var job = FindJob(RowString(value, "jobId"));
            if (job is null)
                return;

            _editTriggerDbId = string.Empty;
            EditTriggerJobId = job.JobId;
            EditTriggerId = string.Empty;
            EditTriggerType = JobClient.PeriodTrigger;
            _editTriggerAssemblyName = "Furion.Pure";
            EditTriggerPeriod = "1000";
            EditTriggerCron = string.Empty;
            EditTriggerDescription = string.Empty;
            _editTriggerStatus = 1;
            EditTriggerStartTime = string.Empty;
            EditTriggerEndTime = string.Empty;
            _editTriggerLastRunTime = string.Empty;
            _editTriggerNextRunTime = string.Empty;
            _editTriggerNumberOfRuns = "0";
            EditTriggerMaxNumberOfRuns = "0";
            _editTriggerNumberOfErrors = "0";
            EditTriggerMaxNumberOfErrors = "0";
            EditTriggerNumRetries = "0";
            EditTriggerRetryTimeout = "1000";
            EditTriggerStartNow = true;
            EditTriggerRunOnStart = true;
            EditTriggerResetOnlyOnce = true;
            _editTriggerUpdatedTime = string.Empty;
            _error = null;
            TriggerDialogVisible = true;
        }

        public void OpenEditTrigger(VueProps? value)
        {
            var trigger = FindTrigger(RowString(value, "jobId"), RowString(value, "triggerId"));
            if (trigger is null)
                return;

            var args = JobClient.ParseTriggerArgs(trigger.TriggerType, trigger.Args);
            _editTriggerDbId = trigger.Id;
            EditTriggerJobId = trigger.JobId;
            EditTriggerId = trigger.TriggerId;
            EditTriggerType = trigger.TriggerType.Length == 0 ? JobClient.PeriodTrigger : trigger.TriggerType;
            _editTriggerAssemblyName = trigger.AssemblyName.Length == 0 ? "Furion.Pure" : trigger.AssemblyName;
            EditTriggerPeriod = args.Period;
            EditTriggerCron = args.Cron;
            EditTriggerDescription = trigger.Description;
            _editTriggerStatus = trigger.Status;
            EditTriggerStartTime = trigger.StartTime;
            EditTriggerEndTime = trigger.EndTime;
            _editTriggerLastRunTime = trigger.LastRunTime;
            _editTriggerNextRunTime = trigger.NextRunTime;
            _editTriggerNumberOfRuns = ValueOrZero(trigger.NumberOfRuns);
            EditTriggerMaxNumberOfRuns = ValueOrZero(trigger.MaxNumberOfRuns);
            _editTriggerNumberOfErrors = ValueOrZero(trigger.NumberOfErrors);
            EditTriggerMaxNumberOfErrors = ValueOrZero(trigger.MaxNumberOfErrors);
            EditTriggerNumRetries = trigger.NumRetries.ToString();
            EditTriggerRetryTimeout = trigger.RetryTimeout.ToString();
            EditTriggerStartNow = trigger.StartNow;
            EditTriggerRunOnStart = trigger.RunOnStart;
            EditTriggerResetOnlyOnce = trigger.ResetOnlyOnce;
            _editTriggerUpdatedTime = trigger.UpdatedTime;
            _error = null;
            TriggerDialogVisible = true;
        }

        public void ApplyCronMacro(string value)
        {
            if (value.Length > 0)
                EditTriggerCron = value;
        }

        public void SaveTrigger()
        {
            if (SavingTrigger)
                return;

            var triggerId = EditTriggerId.Trim();
            if (EditTriggerJobId.Trim().Length < 2)
            {
                _error = "作业编号不能为空。";
                return;
            }
            if (triggerId.Length < 2 || triggerId.Length > 64)
            {
                _error = "触发器编号长度必须为 2 至 64 个字符。";
                return;
            }
            if (EditTriggerDescription.Trim().Length > 128)
            {
                _error = "触发器描述不能超过 128 个字符。";
                return;
            }
            if (TriggerIsCron && EditTriggerCron.Trim().Length == 0)
            {
                _error = "Cron 表达式不能为空。";
                return;
            }

            var period = PositiveWhole(EditTriggerPeriod);
            var numberOfRuns = NonNegativeLongText(_editTriggerNumberOfRuns);
            var maxRuns = NonNegativeLongText(EditTriggerMaxNumberOfRuns);
            var numberOfErrors = NonNegativeLongText(_editTriggerNumberOfErrors);
            var maxErrors = NonNegativeLongText(EditTriggerMaxNumberOfErrors);
            var retries = NonNegativeInt(EditTriggerNumRetries);
            var retryTimeout = NonNegativeInt(EditTriggerRetryTimeout);
            if (!TriggerIsCron && (period is null || period.Value < 1000))
            {
                _error = "间隔时间必须是至少 1000 毫秒的整数。";
                return;
            }
            if (numberOfRuns is null || maxRuns is null || numberOfErrors is null || maxErrors is null || retries is null || retryTimeout is null)
            {
                _error = "触发次数、错误次数和重试配置必须是非负整数。";
                return;
            }

            var input = new JobTriggerSaveInput
            {
                Id = _editTriggerDbId,
                TriggerId = triggerId,
                JobId = EditTriggerJobId.Trim(),
                TriggerType = EditTriggerType,
                AssemblyName = _editTriggerAssemblyName.Trim(),
                Args = JobClient.BuildTriggerArgs(EditTriggerType, EditTriggerPeriod, EditTriggerCron),
                Description = EditTriggerDescription.Trim(),
                Status = _editTriggerStatus,
                StartTime = Optional(EditTriggerStartTime),
                EndTime = Optional(EditTriggerEndTime),
                LastRunTime = Optional(_editTriggerLastRunTime),
                NextRunTime = Optional(_editTriggerNextRunTime),
                NumberOfRuns = numberOfRuns,
                MaxNumberOfRuns = maxRuns,
                NumberOfErrors = numberOfErrors,
                MaxNumberOfErrors = maxErrors,
                NumRetries = retries.Value,
                RetryTimeout = retryTimeout.Value,
                StartNow = EditTriggerStartNow,
                RunOnStart = EditTriggerRunOnStart,
                ResetOnlyOnce = EditTriggerResetOnlyOnce,
                UpdatedTime = Optional(_editTriggerUpdatedTime)
            };

            SavingTrigger = true;
            var request = _editTriggerDbId.Length > 0 ? JobClient.UpdateTrigger(input) : JobClient.AddTrigger(input);
            request.Then(outcome =>
            {
                SavingTrigger = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存触发器失败。";
                    return;
                }

                TriggerDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editTriggerDbId.Length > 0 ? "触发器已更新" : "触发器已新增");
                LoadPage();
            }, _ =>
            {
                SavingTrigger = false;
                _error = "保存触发器失败，请检查网络后重试。";
            });
        }

        public void DeleteTrigger(VueProps? value)
        {
            var trigger = FindTrigger(RowString(value, "jobId"), RowString(value, "triggerId"));
            if (trigger is null || !BeginAction("delete-trigger:" + trigger.JobId + ":" + trigger.TriggerId))
                return;
            CompleteAction(JobClient.DeleteTrigger(trigger.JobId, trigger.TriggerId), "触发器已删除");
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
            _busyKey = key;
            _error = null;
            return true;
        }

        private void CompleteAction(IPromise<ApiOutcome> request, string successMessage)
        {
            request.Then(outcome =>
            {
                _busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "任务调度操作失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success(successMessage);
                LoadPage();
            }, _ =>
            {
                _busyKey = string.Empty;
                _error = "任务调度操作失败，请检查网络后重试。";
            });
        }

        private void CompleteActionAndReloadGroups(IPromise<ApiOutcome> request, string successMessage)
        {
            request.Then(outcome =>
            {
                _busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "任务调度操作失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success(successMessage);
                LoadPage();
                LoadGroups();
            }, _ =>
            {
                _busyKey = string.Empty;
                _error = "任务调度操作失败，请检查网络后重试。";
            });
        }

        public void OpenRecords(VueProps? value)
        {
            var job = FindJob(RowString(value, "jobId"));
            if (job is null)
                return;
            RecordJobId = job.JobId;
            RecordTriggerId = string.Empty;
            RecordTriggerOptions = job.Triggers;
            RecordPage = 1;
            RecordPageSize = 50;
            _recordItems = [];
            RecordRows = [];
            RecordTotal = 0;
            RecordDialogVisible = true;
            LoadRecords();
        }

        public void LoadRecords()
        {
            if (RecordLoading || RecordJobId.Length == 0)
                return;
            RecordLoading = true;
            JobClient.TriggerRecords(new JobTriggerRecordPageInput
            {
                Page = RecordPage,
                PageSize = RecordPageSize,
                JobId = RecordJobId,
                TriggerId = RecordTriggerId.Trim()
            }).Then(outcome =>
            {
                RecordLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载触发器运行记录失败。";
                    return;
                }
                _recordItems = JobClient.ReadRecords(outcome.Data);
                RecordRows = _recordItems;
                RecordTotal = JobClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                RecordLoading = false;
                _error = "加载触发器运行记录失败，请检查网络后重试。";
            });
        }

        public void FilterRecords(string? value)
        {
            RecordTriggerId = value ?? string.Empty;
            RecordPage = 1;
            LoadRecords();
        }

        public void ChangeRecordPage(Number value)
        {
            RecordPage = (int)value;
            LoadRecords();
        }

        public void ChangeRecordPageSize(Number value)
        {
            RecordPageSize = (int)value;
            RecordPage = 1;
            LoadRecords();
        }

        public void ClearRecords()
        {
            if (RecordLoading || !BeginAction("clear-records"))
                return;
            RecordLoading = true;
            JobClient.ClearRecords().Then(outcome =>
            {
                _busyKey = string.Empty;
                RecordLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "清空运行记录失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("运行记录已清空");
                LoadRecords();
            }, _ =>
            {
                _busyKey = string.Empty;
                RecordLoading = false;
                _error = "清空运行记录失败，请检查网络后重试。";
            });
        }

        private void OpenClusters()
        {
            ClusterDialogVisible = true;
            LoadClusters();
        }

        public void LoadClusters()
        {
            if (ClusterLoading)
                return;
            ClusterLoading = true;
            JobClient.Clusters().Then(outcome =>
            {
                ClusterLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载作业集群失败。";
                    return;
                }
                _clusterItems = JobClient.ReadClusters(outcome.Data);
                ClusterRows = _clusterItems;
                _error = null;
            }, _ =>
            {
                ClusterLoading = false;
                _error = "加载作业集群失败，请检查网络后重试。";
            });
        }

        private void OpenDashboard()
            => _navigation.NavigateTo(_navigation.BaseUri + "job/dashboard");

        private JobView? FindJob(string jobId)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].JobId == jobId)
                    return _items[index];
            return null;
        }

        private JobTriggerView? FindTrigger(string jobId, string triggerId)
        {
            var job = FindJob(jobId);
            if (job is null)
                return null;
            for (var index = 0; index < job.Triggers.Length; index++)
                if (job.Triggers[index].TriggerId == triggerId)
                    return job.Triggers[index];
            return null;
        }

        public JobTriggerView[] TriggerRows(VueProps? value)
            => FindJob(RowString(value, "jobId"))?.Triggers ?? [];

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

            // 浏览器端传输 long 值时使用十进制字符串。
            // 服务端配置的 Furion 转换器接受字符串，保留超出 JavaScript 安全范围的整数精度。
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
            => _busyKey == key;

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
            SetQueryGroupName = value => _queryGroupName = value,
            SetQueryJobId = value => _queryJobId = value,
            SetQueryDescription = value => _queryDescription = value,
            RefreshAll = RefreshAll,
            DismissError = () => _error = null,
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
            Loading = _loading,
            Error = _error,
            QueryJobId = _queryJobId,
            QueryDescription = _queryDescription,
            QueryGroupName = _queryGroupName,
            LoadingGroups = _loadingGroups,
            GroupOptions = _groupOptions,
            Total = _total,
            ClusterLoading = ClusterLoading,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
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
