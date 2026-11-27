namespace Jero.Core;

/// <summary>
/// 枚举转字典
/// </summary>
[JobDetail("job_EnumToDictJob", Description = "枚举转字典", GroupName = "default", Concurrent = false)]
[PeriodSeconds(1, TriggerId = "trigger_EnumToDictJob", Description = "枚举转字典", MaxNumberOfRuns = 1, RunOnStart = true)]
public class EnumToDictJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private const string DefaultTagType = null;
    private const int OrderOffset = 10;

    public EnumToDictJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var originColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"【{DateTime.Now}】系统枚举转换字典");

        using var serviceScope = _scopeFactory.CreateScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<ISqlSugarClient>().CopyNew();

        var sysEnumService = serviceScope.ServiceProvider.GetRequiredService<SysEnumService>();
        var sysDictTypeList = GetDictByEnumType(sysEnumService.GetEnumTypeList());

        // 校验枚举类命名规范，字典相关功能中需要通过后缀判断是否为枚举类型
        Console.ForegroundColor = ConsoleColor.Red;
        foreach (var dictType in sysDictTypeList.Where(x => !x.Code.EndsWith("Enum")))
            Console.WriteLine($"【{DateTime.Now}】系统枚举转换字典的枚举类名称必须以Enum结尾: {dictType.Code} ({dictType.Name})");
        sysDictTypeList = sysDictTypeList.Where(x => x.Code.EndsWith("Enum")).ToList();

        await SyncEnumToDictInfoAsync(db, sysDictTypeList);

        Console.ForegroundColor = ConsoleColor.Yellow;
        try
        {
            await db.BeginTranAsync();
            var storageable1 = await db.Storageable(sysDictTypeList)
                .SplitUpdate(it => it.Any())
                .SplitInsert(_ => true)
                .ToStorageAsync();
            await storageable1.AsInsertable.ExecuteCommandAsync(stoppingToken);
            await storageable1.AsUpdateable.ExecuteCommandAsync(stoppingToken);

            Console.WriteLine($"【{DateTime.Now}】系统枚举类转字典类型数据: 插入{storageable1.InsertList.Count}条, 更新{storageable1.UpdateList.Count}条, 共{storageable1.TotalList.Count}条。");

            var storageable2 = await db.Storageable(sysDictTypeList.SelectMany(x => x.Children).ToList())
                .WhereColumns(it => new { it.DictTypeId, it.Value })
                .SplitUpdate(it => it.Any())
                .SplitInsert(_ => true)
                .ToStorageAsync();
            await storageable2.AsInsertable.ExecuteCommandAsync(stoppingToken);
            await storageable2.AsUpdateable.UpdateColumns(u => new
            {
                u.Label,
                u.Code,
                u.Value
            }).ExecuteCommandAsync(stoppingToken);

            Console.WriteLine($"【{DateTime.Now}】系统枚举项转字典值数据: 插入{storageable2.InsertList.Count}条, 更新{storageable2.UpdateList.Count}条, 共{storageable2.TotalList.Count}条。");

            await db.CommitTranAsync();
        }
        catch (Exception error)
        {
            await db.RollbackTranAsync();
            Log.Error($"系统枚举转换字典操作错误：{error.Message}\n堆栈跟踪：{error.StackTrace}", error);
            throw;
        }
        finally
        {
            Console.ForegroundColor = originColor;
        }
    }

    /// <summary>
    /// 用于同步枚举转字典数据
    /// </summary>
    /// <param name="db"></param>
    /// <param name="list"></param>
    private async Task SyncEnumToDictInfoAsync(SqlSugarClient db, List<SysDictType> list)
    {
        var codeList = list.Select(x => x.Code).ToList();
        foreach (var dbDictType in await db.Queryable<SysDictType>().ClearFilter().Where(x => codeList.Contains(x.Code)).ToListAsync() ?? new())
        {
            var enumDictType = list.First(x => x.Code == dbDictType.Code);
            if (enumDictType.Id == dbDictType.Id)
            {
                // 字典值表字段改变后每条字典值记录会多出一条，此处用于删除多余的字典值数据
                var dataValueList = enumDictType.Children.Select(e => e.Value).ToList();
                await db.Deleteable<SysDictData>().Where(x => x.DictTypeId == dbDictType.Id && !dataValueList.Contains(x.Value)).ExecuteCommandAsync();
                continue;
            }

            // 数据不一致则删除
            await db.Deleteable<SysDictData>().Where(x => x.DictTypeId == dbDictType.Id).ExecuteCommandAsync();
            await db.Deleteable<SysDictType>().Where(x => x.Id == dbDictType.Id).ExecuteCommandAsync();
            Console.WriteLine($"【{DateTime.Now}】删除字典数据: {dbDictType.Name}-{dbDictType.Code}");
        }
    }

    /// <summary>
    /// 枚举信息转字典
    /// </summary>
    /// <param name="enumTypeList"></param>
    /// <returns></returns>
    private List<SysDictType> GetDictByEnumType(List<EnumTypeOutput> enumTypeList)
    {
        var orderNo = 1;
        var list = new List<SysDictType>();
        foreach (var type in enumTypeList)
        {
            var dictType = new SysDictType
            {
                Id = 900000000000 + CommonUtil.GetFixedHashCode(GetPersistedEnumKey(type.TypeFullName)),
                SysFlag = YesNoEnum.Y,
                Code = type.TypeName,
                Name = type.TypeDescribe,
                Remark = type.TypeFullName
            };
            dictType.Children = type.EnumEntities.Select(x => new SysDictData
            {
                Id = dictType.Id + orderNo++,
                DictTypeId = dictType.Id,
                Code = x.Name,
                Label = x.Describe,
                Value = x.Value.ToString(),
                OrderNo = x.Value + OrderOffset,
                TagType = x.Theme != "" ? x.Theme : DefaultTagType
            }).ToList();
            list.Add(dictType);
        }
        return list;
    }

    // These modules moved assemblies and namespaces. Keep their existing dictionary IDs
    // so the synchronization job does not delete and recreate rows during upgrade.
    private static string GetPersistedEnumKey(string fullName) => fullName switch
    {
        "Jero.Application.Modules.ApprovalFlow.FlowTypeEnum" => "Jero.Plugin.ApprovalFlow.FlowTypeEnum",
        "Jero.Application.Modules.DingTalk.DingTalkConversationTypeEnum" => "Jero.Plugin.DingTalk.DingTalkConversationTypeEnum",
        "Jero.Application.Modules.GoView.GoViewProStateEnum" => "Jero.Plugin.GoView.GoViewProStateEnum",
        _ => fullName
    };
}
