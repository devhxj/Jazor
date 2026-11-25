namespace Jero.Core.Service;

/// <summary>
/// 系统作业任务服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 320, Description = "作业任务")]
public class SysJobService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysJobDetail> _sysJobDetailRep;
    private readonly SqlSugarRepository<SysJobTrigger> _sysJobTriggerRep;
    private readonly SqlSugarRepository<SysJobTriggerRecord> _sysJobTriggerRecordRep;
    private readonly SqlSugarRepository<SysJobCluster> _sysJobClusterRep;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly DynamicJobCompiler _dynamicJobCompiler;

    public SysJobService(SqlSugarRepository<SysJobDetail> sysJobDetailRep,
        SqlSugarRepository<SysJobTrigger> sysJobTriggerRep,
        SqlSugarRepository<SysJobTriggerRecord> sysJobTriggerRecordRep,
        SqlSugarRepository<SysJobCluster> sysJobClusterRep,
        ISchedulerFactory schedulerFactory,
        DynamicJobCompiler dynamicJobCompiler)
    {
        _sysJobDetailRep = sysJobDetailRep;
        _sysJobTriggerRep = sysJobTriggerRep;
        _sysJobTriggerRecordRep = sysJobTriggerRecordRep;
        _sysJobClusterRep = sysJobClusterRep;
        _schedulerFactory = schedulerFactory;
        _dynamicJobCompiler = dynamicJobCompiler;
    }

    /// <summary>
    /// 获取作业分页列表 ⏰
    /// </summary>
    [DisplayName("获取作业分页列表")]
    public async Task<SqlSugarPagedList<JobDetailOutput>> PageJobDetail(PageJobDetailInput input)
    {
        var jobId = input.JobId?.Trim() ?? string.Empty;
        var groupName = input.GroupName?.Trim() ?? string.Empty;
        var description = input.Description?.Trim() ?? string.Empty;

        var jobDetails = await _sysJobDetailRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(jobId), u => !string.IsNullOrWhiteSpace(u.JobId) && u.JobId.Contains(jobId))
            .WhereIF(!string.IsNullOrWhiteSpace(groupName), u => !string.IsNullOrWhiteSpace(u.GroupName) && u.GroupName.Contains(groupName))
            .WhereIF(!string.IsNullOrWhiteSpace(description), u => !string.IsNullOrWhiteSpace(u.Description) && u.Description.Contains(description))
            .Select(d => new JobDetailOutput
            {
                JobDetail = d,
            }).ToPagedListAsync(input.Page, input.PageSize);
        await _sysJobDetailRep.AsSugarClient().ThenMapperAsync(jobDetails.Items, async u =>
        {
            u.JobTriggers = await _sysJobTriggerRep.GetListAsync(t => t.JobId == u.JobDetail.JobId);
        });

        // 提取中括号里面的参数值
        var rgx = new Regex(@"(?i)(?<=\[)(.*)(?=\])");
        foreach (var job in jobDetails.Items)
        {
            foreach (var jobTrigger in job.JobTriggers)
            {
                jobTrigger.Args = rgx.Match(jobTrigger.Args ?? "").Value;
            }
        }
        ResolveJobRuntimeStatuses(jobDetails.Items);
        return jobDetails;
    }

    /// <summary>
    /// 获取任务看板聚合数据 ⏰
    /// </summary>
    [DisplayName("获取任务看板聚合数据")]
    public async Task<JobDashboardOutput> GetDashboard()
    {
        var abnormalStatuses = new[]
        {
            (TriggerStatus)4,
            (TriggerStatus)5,
            (TriggerStatus)7,
            (TriggerStatus)8,
            (TriggerStatus)11,
            (TriggerStatus)12
        };

        var clusterRows = await _sysJobClusterRep.AsQueryable()
            .OrderByDescending(u => u.UpdatedTime)
            .OrderByDescending(u => u.Id)
            .ToListAsync();

        var recentlyUpdatedJobs = await _sysJobDetailRep.AsQueryable()
            .OrderByDescending(u => u.UpdatedTime)
            .OrderByDescending(u => u.Id)
            .Take(10)
            .Select(d => new JobDetailOutput
            {
                JobDetail = d,
            })
            .ToListAsync();
        ResolveJobRuntimeStatuses(recentlyUpdatedJobs);

        return new JobDashboardOutput
        {
            TotalJobs = await _sysJobDetailRep.AsQueryable().CountAsync(),
            TotalTriggers = await _sysJobTriggerRep.AsQueryable().CountAsync(),
            RunningTriggerCount = await _sysJobTriggerRep.AsQueryable().CountAsync(u => u.Status == (TriggerStatus)2),
            PausedTriggerCount = await _sysJobTriggerRep.AsQueryable().CountAsync(u => u.Status == (TriggerStatus)3),
            AbnormalTriggerCount = await _sysJobTriggerRep.AsQueryable().CountAsync(u => abnormalStatuses.Contains(u.Status)),
            ClusterWorkingCount = clusterRows.Count(u => u.Status == (ClusterStatus)1),
            ClusterWaitingCount = clusterRows.Count(u => u.Status == (ClusterStatus)2),
            GroupRows = await BuildDashboardGroupRows(),
            ClusterRows = clusterRows,
            RecentlyUpdatedJobs = recentlyUpdatedJobs
        };
    }

    /// <summary>
    /// 获取作业组名称集合 ⏰
    /// </summary>
    [DisplayName("获取作业组名称集合")]
    public async Task<List<string>> ListJobGroup()
    {
        var groupNames = await _sysJobDetailRep.AsQueryable()
            .Where(e => !string.IsNullOrWhiteSpace(e.GroupName))
            .Distinct()
            .Select(e => e.GroupName)
            .ToListAsync();
        var result = new List<string>();
        foreach (var name in groupNames)
        {
            if (!string.IsNullOrWhiteSpace(name)) result.Add(name);
        }

        return result;
    }

    /// <summary>
    /// 添加作业 ⏰
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "AddJobDetail"), HttpPost]
    [DisplayName("添加作业")]
    public async Task AddJobDetail(AddJobDetailInput input)
    {
        var isExist = await _sysJobDetailRep.IsAnyAsync(u => u.JobId == input.JobId && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1006);

        // 动态创建作业
        Type jobType;
        switch (input.CreateType)
        {
            case JobCreateTypeEnum.Script when string.IsNullOrEmpty(input.ScriptCode):
                throw Oops.Oh(ErrorCodeEnum.D1701);
            case JobCreateTypeEnum.Script:
                {
                    jobType = DynamicJobCompiler.BuildJob(input.ScriptCode);

                    if (jobType.GetCustomAttributes(typeof(JobDetailAttribute)).FirstOrDefault() is not JobDetailAttribute jobDetailAttribute)
                        throw Oops.Oh(ErrorCodeEnum.D1702);
                    if (jobDetailAttribute.JobId != input.JobId)
                        throw Oops.Oh(ErrorCodeEnum.D1703);
                    break;
                }
            //todo:hxj
            //case JobCreateTypeEnum.Http:
            //    jobType = typeof(HttpJob);
            //    break;

            default:
                throw new NotSupportedException();
        }

        _schedulerFactory.AddJob(JobBuilder.Create(jobType).LoadFrom(input.Adapt<SysJobDetail>()).SetJobType(jobType));

        // 延迟一下等待持久化写入，再执行其他字段的更新
        await Task.Delay(500);
        await _sysJobDetailRep.AsUpdateable()
            .SetColumns(u => new SysJobDetail { CreateType = input.CreateType, ScriptCode = input.ScriptCode })
            .Where(u => u.JobId == input.JobId).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新作业 ⏰
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "UpdateJobDetail"), HttpPost]
    [DisplayName("更新作业")]
    public async Task UpdateJobDetail(UpdateJobDetailInput input)
    {
        var isExist = await _sysJobDetailRep.IsAnyAsync(u => u.JobId == input.JobId && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1006);

        var sysJobDetail = await _sysJobDetailRep.GetFirstAsync(u => u.Id == input.Id);
        if (sysJobDetail.JobId != input.JobId) throw Oops.Oh(ErrorCodeEnum.D1704);

        var scheduler = _schedulerFactory.GetJob(sysJobDetail.JobId);
        var oldScriptCode = sysJobDetail.ScriptCode; // 旧脚本代码
        input.Adapt(sysJobDetail);

        if (input.CreateType == JobCreateTypeEnum.Script)
        {
            if (string.IsNullOrEmpty(input.ScriptCode)) throw Oops.Oh(ErrorCodeEnum.D1701);

            if (input.ScriptCode != oldScriptCode)
            {
                // 动态创建作业
                var jobType = DynamicJobCompiler.BuildJob(input.ScriptCode);

                if (jobType.GetCustomAttributes(typeof(JobDetailAttribute)).FirstOrDefault() is not JobDetailAttribute jobDetailAttribute)
                    throw Oops.Oh(ErrorCodeEnum.D1702);
                if (jobDetailAttribute.JobId != input.JobId) throw Oops.Oh(ErrorCodeEnum.D1703);

                scheduler?.UpdateDetail(JobBuilder.Create(jobType).LoadFrom(sysJobDetail).SetJobType(jobType));
            }
        }
        else
        {
            scheduler?.UpdateDetail(scheduler.GetJobBuilder().LoadFrom(sysJobDetail));
        }

        // Tip: 假如这次更新有变更了 JobId，变更 JobId 后触发的持久化更新执行，会由于找不到 JobId 而更新不到数据
        // 延迟一下等待持久化写入，再执行其他字段的更新
        await Task.Delay(500);
        await _sysJobDetailRep.UpdateAsync(sysJobDetail);
    }

    /// <summary>
    /// 删除作业 ⏰
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "DeleteJobDetail"), HttpPost]
    [DisplayName("删除作业")]
    public async Task DeleteJobDetail(DeleteJobDetailInput input)
    {
        _schedulerFactory.RemoveJob(input.JobId);

        // 如果 _schedulerFactory 中不存在 JodId，则无法触发持久化，下面的代码确保作业和触发器能被删除
        await _sysJobDetailRep.DeleteAsync(u => u.JobId == input.JobId);
        await _sysJobTriggerRep.DeleteAsync(u => u.JobId == input.JobId);
    }

    /// <summary>
    /// 获取触发器列表 ⏰
    /// </summary>
    [DisplayName("获取触发器列表")]
    public async Task<List<SysJobTrigger>> GetJobTriggerList([FromQuery] JobDetailInput input)
    {
        return await _sysJobTriggerRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.JobId), u => u.JobId.Contains(input.JobId))
            .ToListAsync();
    }

    /// <summary>
    /// 添加触发器 ⏰
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "AddJobTrigger"), HttpPost]
    [DisplayName("添加触发器")]
    public async Task AddJobTrigger(AddJobTriggerInput input)
    {
        var isExist = await _sysJobTriggerRep.IsAnyAsync(u => u.TriggerId == input.TriggerId && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1006);

        var jobTrigger = input.Adapt<SysJobTrigger>();
        jobTrigger.Args = "[" + jobTrigger.Args + "]";

        var scheduler = _schedulerFactory.GetJob(input.JobId);
        scheduler?.AddTrigger(Triggers.Create(input.AssemblyName, input.TriggerType).LoadFrom(jobTrigger));
    }

    /// <summary>
    /// 更新触发器 ⏰
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "UpdateJobTrigger"), HttpPost]
    [DisplayName("更新触发器")]
    public async Task UpdateJobTrigger(UpdateJobTriggerInput input)
    {
        var isExist = await _sysJobTriggerRep.IsAnyAsync(u => u.TriggerId == input.TriggerId && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1006);

        var jobTrigger = input.Adapt<SysJobTrigger>();
        if (jobTrigger.EndTime.HasValue && jobTrigger.EndTime.Value.Year < 1901)
        {
            jobTrigger.EndTime = null;
        }
        if (jobTrigger.StartTime.HasValue && jobTrigger.StartTime.Value.Year < 1901)
        {
            jobTrigger.StartTime = null;
        }
        jobTrigger.Args = "[" + jobTrigger.Args + "]";

        var scheduler = _schedulerFactory.GetJob(input.JobId);
        scheduler?.UpdateTrigger(Triggers.Create(input.AssemblyName, input.TriggerType).LoadFrom(jobTrigger));
    }

    /// <summary>
    /// 删除触发器 ⏰
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "DeleteJobTrigger"), HttpPost]
    [DisplayName("删除触发器")]
    public async Task DeleteJobTrigger(DeleteJobTriggerInput input)
    {
        var scheduler = _schedulerFactory.GetJob(input.JobId);
        scheduler?.RemoveTrigger(input.TriggerId);

        // 如果 _schedulerFactory 中不存在 JodId，则无法触发持久化，下行代码确保触发器能被删除
        await _sysJobTriggerRep.DeleteAsync(u => u.JobId == input.JobId && u.TriggerId == input.TriggerId);
    }

    /// <summary>
    /// 暂停所有作业 ⏰
    /// </summary>
    /// <returns></returns>
    [DisplayName("暂停所有作业")]
    public void PauseAllJob()
    {
        _schedulerFactory.PauseAll();
    }

    /// <summary>
    /// 启动所有作业 ⏰
    /// </summary>
    /// <returns></returns>
    [DisplayName("启动所有作业")]
    public void StartAllJob()
    {
        _schedulerFactory.StartAll();
    }

    /// <summary>
    /// 暂停作业 ⏰
    /// </summary>
    [DisplayName("暂停作业")]
    public void PauseJob(JobDetailInput input)
    {
        _schedulerFactory.TryPauseJob(input.JobId, out _);
    }

    /// <summary>
    /// 启动作业 ⏰
    /// </summary>
    [DisplayName("启动作业")]
    public void StartJob(JobDetailInput input)
    {
        _schedulerFactory.TryStartJob(input.JobId, out _);
    }

    /// <summary>
    /// 取消作业 ⏰
    /// </summary>
    [DisplayName("取消作业")]
    public void CancelJob(JobDetailInput input)
    {
        _schedulerFactory.TryCancelJob(input.JobId, out _);
    }

    /// <summary>
    /// 执行作业 ⏰
    /// </summary>
    /// <param name="input"></param>
    [DisplayName("执行作业")]
    public void RunJob(JobDetailInput input)
    {
        if (_schedulerFactory.TryRunJob(input.JobId, out _) != ScheduleResult.Succeed) throw Oops.Oh(ErrorCodeEnum.D1705);
    }

    /// <summary>
    /// 暂停触发器 ⏰
    /// </summary>
    [DisplayName("暂停触发器")]
    public void PauseTrigger(JobTriggerInput input)
    {
        var scheduler = _schedulerFactory.GetJob(input.JobId);
        scheduler?.PauseTrigger(input.TriggerId);
    }

    /// <summary>
    /// 启动触发器 ⏰
    /// </summary>
    [DisplayName("启动触发器")]
    public void StartTrigger(JobTriggerInput input)
    {
        var scheduler = _schedulerFactory.GetJob(input.JobId);
        scheduler?.StartTrigger(input.TriggerId);
    }

    /// <summary>
    /// 强制唤醒作业调度器 ⏰
    /// </summary>
    [DisplayName("强制唤醒作业调度器")]
    public void CancelSleep()
    {
        _schedulerFactory.CancelSleep();
    }

    /// <summary>
    /// 强制触发所有作业持久化 ⏰
    /// </summary>
    [DisplayName("强制触发所有作业持久化")]
    public void PersistAll()
    {
        _schedulerFactory.PersistAll();
    }

    /// <summary>
    /// 获取集群列表 ⏰
    /// </summary>
    [DisplayName("获取集群列表")]
    public async Task<List<SysJobCluster>> GetJobClusterList()
    {
        return await _sysJobClusterRep.GetListAsync();
    }

    /// <summary>
    /// 获取作业触发器运行记录分页列表 ⏰
    /// </summary>
    [DisplayName("获取作业触发器运行记录分页列表")]
    public async Task<SqlSugarPagedList<SysJobTriggerRecord>> PageJobTriggerRecord(PageJobTriggerRecordInput input)
    {
        return await _sysJobTriggerRecordRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.JobId), u => u.JobId == input.JobId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.TriggerId), u => u.TriggerId == input.TriggerId)
            .OrderByDescending(u => u.Id)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 清空作业触发器运行记录 🔖
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "ClearJobTriggerRecord"), HttpPost]
    [DisplayName("清空作业触发器运行记录")]
    public void ClearJobTriggerRecord()
    {
        // Job records use Snowflake ids rather than a database identity column.
        // A SQLite TRUNCATE implementation may still try to update sqlite_sequence,
        // which does not exist for this table.
        _sysJobTriggerRecordRep.AsSugarClient().Deleteable<SysJobTriggerRecord>().ExecuteCommand();
    }

    /// <summary>
    /// 清空不保留的作业触发器运行记录 🔖
    /// </summary>
    /// <returns></returns>
    [NonAction]
    [DisplayName("清空过期的作业触发器运行记录")]
    public async Task ClearExpireJobTriggerRecord(SysJobTriggerRecord input)
    {
        int keepRecords = 30;//保留记录条数
        // 使用CopyNew()创建新的数据库连接实例，避免连接冲突
        var db = _sysJobTriggerRecordRep.AsSugarClient().CopyNew();
        var keepIds = await db.Queryable<SysJobTriggerRecord>()
            .Where(u => u.JobId == input.JobId && u.TriggerId == input.TriggerId)
            .OrderByDescending(u => u.Id)
            .Take(keepRecords)
            .Select(u => u.Id)
            .ToListAsync();

        if (keepIds.Count < keepRecords)
            return;

        var oldestKeptId = keepIds.Min();
        await db.Deleteable<SysJobTriggerRecord>()
            .Where(u => u.JobId == input.JobId && u.TriggerId == input.TriggerId)
            .Where(u => u.Id < oldestKeptId)
            .ExecuteCommandAsync();
    }

    private async Task<List<JobDashboardGroupOutput>> BuildDashboardGroupRows()
    {
        var jobs = await _sysJobDetailRep.AsQueryable()
            .Select(u => new JobDashboardJobGroupSource
            {
                JobId = u.JobId,
                GroupName = u.GroupName
            })
            .ToListAsync();
        var triggers = await _sysJobTriggerRep.AsQueryable()
            .Select(u => new JobDashboardTriggerGroupSource
            {
                JobId = u.JobId,
                Status = u.Status
            })
            .ToListAsync();
        var jobGroupMap = jobs
            .GroupBy(u => u.JobId)
            .ToDictionary(
                u => u.Key,
                u => NormalizeDashboardGroupName(u.FirstOrDefault()?.GroupName));
        var triggerRows = triggers
            .GroupBy(u => jobGroupMap.TryGetValue(u.JobId, out var groupName) ? groupName : "default")
            .ToDictionary(
                u => u.Key,
                u => new
                {
                    TriggerCount = u.Count(),
                    RunningTriggerCount = u.Count(t => t.Status == (TriggerStatus)2)
                });

        return jobs
            .GroupBy(u => NormalizeDashboardGroupName(u.GroupName))
            .Select(u =>
            {
                triggerRows.TryGetValue(u.Key, out var triggerRow);
                return new JobDashboardGroupOutput
                {
                    GroupName = u.Key,
                    JobCount = u.Count(),
                    TriggerCount = triggerRow?.TriggerCount ?? 0,
                    RunningTriggerCount = triggerRow?.RunningTriggerCount ?? 0
                };
            })
            .OrderByDescending(u => u.JobCount)
            .ThenBy(u => u.GroupName)
            .ToList();
    }

    private static string NormalizeDashboardGroupName(string? groupName)
    {
        return string.IsNullOrWhiteSpace(groupName) ? "default" : groupName.Trim();
    }

    private void ResolveJobRuntimeStatuses(IEnumerable<JobDetailOutput> jobs)
    {
        foreach (var job in jobs)
        {
            var scheduler = _schedulerFactory.GetJob(job.JobDetail.JobId);
            var triggerStatuses = scheduler?.GetTriggers().Select(u => u.Status)
                ?? job.JobTriggers.Select(u => u.Status);
            var status = JobRuntimeStatusResolver.Resolve(scheduler is not null, triggerStatuses);

            job.JobStatus = status.Value;
            job.JobStatusText = status.Text;
        }
    }

    private sealed class JobDashboardJobGroupSource
    {
        public string JobId { get; set; } = string.Empty;

        public string? GroupName { get; set; }
    }

    private sealed class JobDashboardTriggerGroupSource
    {
        public string JobId { get; set; } = string.Empty;

        public TriggerStatus Status { get; set; }
    }
}
