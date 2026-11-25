namespace Jero.Core;

public static class SqlSugarFilter
{
    /// <summary>
    /// 缓存全局查询过滤器（内存缓存）
    /// </summary>
    private static readonly ICache Cache = NewLife.Caching.Cache.Default;

    private static readonly SysOrgService SysOrgService = App.GetRequiredService<SysOrgService>();
    private static readonly SysCacheService SysCacheService = App.GetRequiredService<SysCacheService>();

    /// <summary>
    /// 删除用户机构缓存
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dbConfigId"></param>
    public static void DeleteUserOrgCache(long userId, string dbConfigId)
    {
        // 删除用户机构集合缓存
        SysCacheService.Remove($"{CacheConst.KeyUserOrg}{userId}");
        // 删除最大数据权限缓存
        SysCacheService.Remove($"{CacheConst.KeyRoleMaxDataScope}{userId}");
        // 用户权限缓存（按钮集合）
        SysCacheService.Remove($"{CacheConst.KeyUserButton}{userId}");
    }

    /// <summary>
    /// 删除自定义过滤器缓存
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dbConfigId"></param>
    public static void DeleteCustomCache(long userId, string dbConfigId)
    {
        // 删除自定义缓存——过滤器
        Cache.Remove($"db:{dbConfigId}:custom:{userId}");
    }

    /// <summary>
    /// 配置用户机构集合过滤器
    /// </summary>
    public static void SetOrgEntityFilter(SqlSugarScopeProvider db)
    {
        // 若仅本人数据，则直接返回
        var maxDataScope = SetDataScopeFilter(db);
        // 获取用户最大数据范围，如果是全部数据、仅本人，则跳过
        if (maxDataScope is 0 or (int)DataScopeEnum.Self or (int)DataScopeEnum.All) return;

        // 获取用户所属机构，保证同一作用域
        var orgIds = new List<long>();
        Scoped.Create((factory, scope) =>
        {
            var services = scope.ServiceProvider;
            orgIds = services.GetRequiredService<SysOrgService>().GetUserOrgIdList().GetAwaiter().GetResult();
        });
        if (orgIds == null || orgIds.Count == 0) return;

        //配置机构Id过滤器
        db.QueryFilter.AddTableFilter<IOrgIdFilter>(o => SqlFunc.ContainsArray(orgIds, o.OrgId));
    }

    /// <summary>
    /// 配置用户仅本人数据过滤器
    /// </summary>
    private static int SetDataScopeFilter(SqlSugarScopeProvider db)
    {
        var maxDataScope = (int)DataScopeEnum.All;

        long.TryParse(App.HttpContext?.User.FindFirst(ClaimConst.UserId)?.Value, out var userId);
        if (userId <= 0) return maxDataScope;

        // 获取用户最大数据范围---仅本人数据
        maxDataScope = App.GetRequiredService<SysCacheService>().Get<int>(CacheConst.KeyRoleMaxDataScope + userId);
        // 若为0则获取用户机构组织集合建立缓存
        if (maxDataScope == 0)
        {
            // 获取用户所属机构，保证同一作用域
            Scoped.Create((factory, scope) =>
            {
                SysOrgService.GetUserOrgIdList().GetAwaiter().GetResult();
                maxDataScope = SysCacheService.Get<int>(CacheConst.KeyRoleMaxDataScope + userId);
            });
        }
        if (maxDataScope != (int)DataScopeEnum.Self) return maxDataScope;

        // 配置用户数据范围缓存
        var cacheKey = $"db:{db.CurrentConnectionConfig.ConfigId}:dataScope:{userId}";
        var dataScopeFilter = Cache.Get<ConcurrentDictionary<Type, LambdaExpression>>(cacheKey);
        if (dataScopeFilter == null)
        {
            // 获取业务实体数据表
            var entityTypes = App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
                && (u.IsSubclassOf(typeof(EntityBaseOrg)) || u.IsSubclassOf(typeof(EntityBaseOrgDel))));
            if (!entityTypes.Any()) return maxDataScope;

            dataScopeFilter = new ConcurrentDictionary<Type, LambdaExpression>();
            foreach (var entityType in entityTypes)
            {
                // 排除非当前数据库实体
                var tAtt = entityType.GetCustomAttribute<TenantAttribute>();
                if ((tAtt != null && db.CurrentConnectionConfig.ConfigId.ToString() != tAtt.configId.ToString()))
                    continue;

                //var lambda = DynamicExpressionParser.ParseLambda(new[] {
                //    Expression.Parameter(entityType, "u") }, typeof(bool), $"u.{nameof(EntityBaseData.CreateUserId)}=@0", userId);
                var lambda = entityType.GetConditionExpression<OwnerUserAttribute>(new List<long> { userId });

                db.QueryFilter.AddTableFilter(entityType, lambda);
                dataScopeFilter.TryAdd(entityType, lambda);
            }
            Cache.Add(cacheKey, dataScopeFilter);
        }
        else
        {
            foreach (var filter in dataScopeFilter)
                db.QueryFilter.AddTableFilter(filter.Key, filter.Value);
        }
        return maxDataScope;
    }

    /// <summary>
    /// 配置自定义过滤器
    /// </summary>
    public static void SetCustomEntityFilter(SqlSugarScopeProvider db)
    {
        // 配置自定义缓存
        var userId = App.User?.FindFirst(ClaimConst.UserId)?.Value;
        var cacheKey = $"db:{db.CurrentConnectionConfig.ConfigId}:custom:{userId}";
        var tableFilterItemList = Cache.Get<List<TableFilterItem<object>>>(cacheKey);
        if (tableFilterItemList == null)
        {
            // 获取自定义实体过滤器
            var entityFilterTypes = App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
                && u.GetInterfaces().Any(i => i.HasImplementedRawGeneric(typeof(IEntityFilter))));
            if (!entityFilterTypes.Any()) return;

            var tableFilterItems = new List<TableFilterItem<object>>();
            foreach (var entityFilter in entityFilterTypes)
            {
                if (Activator.CreateInstance(entityFilter) is not IEntityFilter instance) continue;
                var entityFilters = instance.AddEntityFilter();

                foreach (var u in entityFilters)
                {
                    var tableFilterItem = (TableFilterItem<object>)u;
                    var typeProperty = tableFilterItem.GetType().GetProperty("type", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (typeProperty?.GetValue(tableFilterItem, null) is not Type entityType) continue;

                    // 排除非当前数据库实体
                    var tAtt = entityType.GetCustomAttribute<TenantAttribute>();
                    if ((tAtt != null && db.CurrentConnectionConfig.ConfigId.ToString() != tAtt.configId.ToString()) ||
                        (tAtt == null && db.CurrentConnectionConfig.ConfigId.ToString() != SqlSugarConst.MainConfigId))
                        continue;

                    tableFilterItems.Add(tableFilterItem);
                    db.QueryFilter.Add(tableFilterItem);
                }
            }
            Cache.Add(cacheKey, tableFilterItems);
        }
        else
        {
            tableFilterItemList.ForEach(u =>
            {
                db.QueryFilter.Add(u);
            });
        }
    }
}

/// <summary>
/// 自定义实体过滤器接口
/// </summary>
public interface IEntityFilter
{
    /// <summary>
    /// 实体过滤器
    /// </summary>
    /// <returns></returns>
    IEnumerable<TableFilterItem<object>> AddEntityFilter();
}

///// <summary>
///// 自定义业务实体过滤器示例
///// </summary>
//public class TestEntityFilter : IEntityFilter
//{
//    public IEnumerable<TableFilterItem<object>> AddEntityFilter()
//    {
//        // 构造自定义条件的过滤器
//        Expression<Func<SysUser, bool>> dynamicExpression = u => u.Remark.Contains("xxx");
//        var tableFilterItem = new TableFilterItem<object>(typeof(SysUser), dynamicExpression);

//        return new[]
//        {
//            tableFilterItem
//        };
//    }
//}
