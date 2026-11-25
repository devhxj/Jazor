


using System.Text.Json;
namespace Jero.Core;

/// <summary>
/// Sqlsugar 动态查询扩展方法
/// </summary>
public static class SqlSugarExtension
{
    public static ISugarQueryable<T> SearchBy<T>(this ISugarQueryable<T> queryable, BaseFilter? filter)
    {
        if (filter is null) return queryable;

        return queryable.SearchByKeyword(filter.Keyword)
                .AdvancedSearch(filter.Search)
                .AdvancedFilter(filter.Filter);
    }

    public static ISugarQueryable<T> SearchByKeyword<T>(this ISugarQueryable<T> queryable, string? keyword)
    {
        return queryable.AdvancedSearch(new Search { Keyword = keyword });
    }

    public static ISugarQueryable<T> AdvancedSearch<T>(this ISugarQueryable<T> queryable, Search? search)
    {
        if (!string.IsNullOrWhiteSpace(search?.Keyword))
        {
            var paramExpr = Expression.Parameter(typeof(T));

            Expression right = Expression.Constant(false);

            if (search.Fields?.Any() is true)
            {
                foreach (string field in search.Fields)
                {
                    MemberExpression propertyExpr = GetPropertyExpression(field, paramExpr);

                    var left = AddSearchPropertyByKeyword<T>(propertyExpr, search.Keyword);

                    right = Expression.Or(left, right);
                }
            }
            else
            {
                var properties = typeof(T).GetProperties()
                         .Where(prop => Nullable.GetUnderlyingType(prop.PropertyType) == null
                             && !prop.PropertyType.IsEnum
                             && Type.GetTypeCode(prop.PropertyType) != TypeCode.Object);

                foreach (var property in properties)
                {
                    var propertyExpr = Expression.Property(paramExpr, property);

                    var left = AddSearchPropertyByKeyword<T>(propertyExpr, search.Keyword);

                    right = Expression.Or(left, right);
                }
            }

            var lambda = Expression.Lambda<Func<T, bool>>(right, paramExpr);

            return queryable.Where(lambda);
        }

        return queryable;
    }

    public static ISugarQueryable<T> AdvancedFilter<T>(this ISugarQueryable<T> queryable, Filter? filter)
    {
        if (filter is not null)
        {
            var parameter = Expression.Parameter(typeof(T));

            Expression binaryExpresioFilter;

            if (filter.Logic.HasValue)
            {
                if (filter.Filters is null) throw new ArgumentException("The Filters attribute is required when declaring a logic");
                binaryExpresioFilter = CreateFilterExpression(filter.Logic.Value, filter.Filters, parameter);
            }
            else
            {
                var filterValid = GetValidFilter(filter);
                binaryExpresioFilter = CreateFilterExpression(filterValid.Field, filterValid.Operator, filterValid.Value, parameter);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(binaryExpresioFilter, parameter);

            return queryable.Where(lambda);
        }
        return queryable;
    }

    private static Expression CombineFilter(
           FilterLogicEnum filterLogic,
           Expression bExpresionBase,
           Expression bExpresion)
    {
        return filterLogic switch
        {
            FilterLogicEnum.And => Expression.AndAlso(bExpresionBase, bExpresion),
            FilterLogicEnum.Or => Expression.OrElse(bExpresionBase, bExpresion),
            FilterLogicEnum.Xor => Expression.ExclusiveOr(bExpresionBase, bExpresion),
            _ => throw new ArgumentException("FilterLogic is not valid.", nameof(filterLogic)),
        };
    }

    private static (string Field, FilterOperatorEnum Operator, object? Value) GetValidFilter(Filter filter)
    {
        if (string.IsNullOrEmpty(filter.Field)) throw new ArgumentException("The field attribute is required when declaring a filter");
        if (filter.Operator is null) throw new ArgumentException("The Operator attribute is required when declaring a filter");
        return (filter.Field, filter.Operator.Value, filter.Value);
    }

    private static Expression CreateFilterExpression(
        FilterLogicEnum filterLogic,
        IEnumerable<Filter> filters,
        ParameterExpression parameter)
    {
        Expression? filterExpression = null;

        foreach (var filter in filters)
        {
            Expression bExpresionFilter;

            if (filter.Logic.HasValue)
            {
                if (filter.Filters is null) throw new ArgumentException("The Filters attribute is required when declaring a logic");
                bExpresionFilter = CreateFilterExpression(filter.Logic.Value, filter.Filters, parameter);
            }
            else
            {
                var filterValid = GetValidFilter(filter);
                bExpresionFilter = CreateFilterExpression(filterValid.Field, filterValid.Operator, filterValid.Value, parameter);
            }

            filterExpression = filterExpression is null ? bExpresionFilter : CombineFilter(filterLogic, filterExpression, bExpresionFilter);
        }

        return filterExpression ?? throw new ArgumentException("The Filters attribute must contain at least one valid filter");
    }

    private static Expression CreateFilterExpression(
        string field,
        FilterOperatorEnum filterOperator,
        object? value,
        ParameterExpression parameter)
    {
        var propertyExpresion = GetPropertyExpression(field, parameter);
        var valueExpresion = GeValuetExpression(field, value, propertyExpresion.Type);
        return CreateFilterExpression(propertyExpresion, valueExpresion, filterOperator);
    }

    private static Expression CreateFilterExpression(
        MemberExpression memberExpression,
        ConstantExpression constantExpression,
        FilterOperatorEnum filterOperator)
    {
        return filterOperator switch
        {
            FilterOperatorEnum.EQ => Expression.Equal(memberExpression, constantExpression),
            FilterOperatorEnum.NEQ => Expression.NotEqual(memberExpression, constantExpression),
            FilterOperatorEnum.LT => Expression.LessThan(memberExpression, constantExpression),
            FilterOperatorEnum.LTE => Expression.LessThanOrEqual(memberExpression, constantExpression),
            FilterOperatorEnum.GT => Expression.GreaterThan(memberExpression, constantExpression),
            FilterOperatorEnum.GTE => Expression.GreaterThanOrEqual(memberExpression, constantExpression),
            FilterOperatorEnum.Contains => Expression.Call(memberExpression, nameof(FilterOperatorEnum.Contains), null, constantExpression),
            FilterOperatorEnum.StartsWith => Expression.Call(memberExpression, nameof(FilterOperatorEnum.StartsWith), null, constantExpression),
            FilterOperatorEnum.EndsWith => Expression.Call(memberExpression, nameof(FilterOperatorEnum.EndsWith), null, constantExpression),
            _ => throw new ArgumentException("Filter Operator is not valid."),
        };
    }

    private static string? GetStringFromJsonElement(object? value)
    {
        if (value is JsonElement jsonElement) return jsonElement.GetString();
        if (value is string text) return text;
        return value?.ToString();
    }

    private static ConstantExpression GeValuetExpression(
          string field,
          object? value,
          Type propertyType)
    {
        if (value == null) return Expression.Constant(null, propertyType);

        if (propertyType.IsEnum)
        {
            string? stringEnum = GetStringFromJsonElement(value);

            if (!Enum.TryParse(propertyType, stringEnum, true, out object? valueparsed)) throw new ArgumentException(string.Format("Value {0} is not valid for {1}", value, field));

            return Expression.Constant(valueparsed, propertyType);
        }
        if (propertyType == typeof(long) || propertyType == typeof(long?))
        {
            string? stringLong = GetStringFromJsonElement(value);

            if (!long.TryParse(stringLong, out long valueparsed)) throw new ArgumentException(string.Format("Value {0} is not valid for {1}", value, field));

            return Expression.Constant(valueparsed, propertyType);
        }

        if (propertyType == typeof(Guid))
        {
            string? stringGuid = GetStringFromJsonElement(value);

            if (!Guid.TryParse(stringGuid, out Guid valueparsed)) throw new ArgumentException(string.Format("Value {0} is not valid for {1}", value, field));

            return Expression.Constant(valueparsed, propertyType);
        }

        if (propertyType == typeof(string))
        {
            string? text = GetStringFromJsonElement(value);

            return Expression.Constant(text, propertyType);
        }

        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
        {
            string? text = GetStringFromJsonElement(value);
            return Expression.Constant(ChangeType(text, propertyType), propertyType);
        }

        var rawValue = value is JsonElement jsonElement ? jsonElement.GetRawText() : Convert.ToString(value);
        return Expression.Constant(ChangeType(rawValue, propertyType), propertyType);
    }

    private static dynamic? ChangeType(object? value, Type conversion)
    {
        var t = conversion;

        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            if (value == null)
            {
                return null;
            }

            t = Nullable.GetUnderlyingType(t) ?? t;
        }

        return Convert.ChangeType(value, t);
    }

    private static MemberExpression GetPropertyExpression(
            string propertyName,
            ParameterExpression parameter)
    {
        Expression propertyExpression = parameter;
        foreach (string member in propertyName.Split('.'))
        {
            propertyExpression = Expression.PropertyOrField(propertyExpression, member);
        }

        return (MemberExpression)propertyExpression;
    }

    private static Expression AddSearchPropertyByKeyword<T>(
          Expression propertyExpr,
            string keyword,
            FilterOperatorEnum operatorSearch = FilterOperatorEnum.Contains)
    {
        if (propertyExpr is not MemberExpression memberExpr || memberExpr.Member is not PropertyInfo property)
        {
            throw new ArgumentException("propertyExpr must be a property expression.", nameof(propertyExpr));
        }

        ConstantExpression constant = Expression.Constant(keyword);

        MethodInfo? method = operatorSearch switch
        {
            FilterOperatorEnum.Contains => typeof(string).GetMethod(nameof(FilterOperatorEnum.Contains), new Type[] { typeof(string) }),
            FilterOperatorEnum.StartsWith => typeof(string).GetMethod(nameof(FilterOperatorEnum.StartsWith), new Type[] { typeof(string) }),
            FilterOperatorEnum.EndsWith => typeof(string).GetMethod(nameof(FilterOperatorEnum.EndsWith), new Type[] { typeof(string) }),
            _ => throw new ArgumentException("Filter Operator is not valid."),
        };
        if (method is null) throw new MissingMethodException(typeof(string).FullName, operatorSearch.ToString());

        Expression selectorExpr =
               property.PropertyType == typeof(string)
                   ? Expression.Coalesce(propertyExpr, Expression.Constant(string.Empty))
                   : Expression.Condition(
                       Expression.Equal(Expression.Convert(propertyExpr, typeof(object)), Expression.Constant(null, typeof(object))),
                       Expression.Constant(string.Empty),
                       Expression.Call(propertyExpr, "ToString", null, null));

        return Expression.Call(selectorExpr, method, constant);
    }

    #region 视图操作

    /// <summary>
    /// 获取映射SQL语句, 用于创建视图
    /// </summary>
    /// <param name="queryable"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string ToMappedSqlString<T>(this ISugarQueryable<T> queryable) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        // 获取实体映射信息
        var entityInfo = queryable.Context.EntityMaintenance.GetEntityInfo<T>();
        if (entityInfo?.Columns == null || entityInfo.Columns.Count == 0) return queryable.ToSqlString();

        // 构建需要替换的字段名映射（只处理实际有差异的字段）
        var nameMap = entityInfo.Columns
            .Where(c => !string.Equals(c.PropertyName, c.DbColumnName, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(k => k.PropertyName.ToLower(), v => v.DbColumnName, StringComparer.OrdinalIgnoreCase);
        if (nameMap.Count == 0) return queryable.ToSqlString();

        // 预编译正则表达式提升性能
        var sql = queryable.ToSqlString();
        foreach (var kv in nameMap)
        {
            sql = Regex.Replace(sql, $@"\b{kv.Key}\b", kv.Value ?? kv.Key, RegexOptions.IgnoreCase | RegexOptions.Compiled); // 单词边界匹配
        }
        return sql;
    }

    #endregion 视图操作

    /// <summary>
    /// 列表转换为树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">列表数据</param>
    /// <param name="childrenSelector">设置子节点列表。例如：item => item.Children</param>
    /// <param name="parentIdSelector">设置元素的父级 Id。例如：item => item.ParentId</param>
    /// <param name="rootParentId">根节点的父级 Id，默认为 0 </param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IEnumerable<T> ToTree<T>(
       this IEnumerable<T> source,
       Func<T, List<T>> childrenSelector,
       Func<T, long> parentIdSelector,
       int rootParentId)
       where T : class
    {
        var lookup = source.ToLookup(parentIdSelector);
        List<T> BuildTree(long parentId)
        {
            return lookup[parentId].Select(item =>
            {
                var children = BuildTree(GetId(item));
                childrenSelector(item).Clear();
                childrenSelector(item).AddRange(children);
                return item;
            }).ToList();
        }

        // 需要提供获取Id的方法，可以用反射或者自己传参数
        long GetId(T item)
        {
            var prop = typeof(T).GetProperty("Id");
            if (prop == null) throw new Exception("没有找到Id属性");
            return prop.GetValue(item) is long id ? id : throw new Exception("Id属性值为空或不是long类型");
        }

        return BuildTree(rootParentId);
    }
}
