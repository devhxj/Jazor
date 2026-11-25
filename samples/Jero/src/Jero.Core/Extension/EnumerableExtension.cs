namespace Jero.Core;

/// <summary>
/// 数据集合拓展类
/// </summary>
public static class EnumerableExtension
{
    private static readonly ConcurrentDictionary<string, PropertyInfo> PropertyCache = new();

    /// <summary>
    /// 查询有父子关系的数据集
    /// </summary>
    /// <param name="list">数据集</param>
    /// <param name="idExpression">主键ID字段</param>
    /// <param name="parentIdExpression">父级字段</param>
    /// <param name="topParentIdValue">顶级节点父级字段值</param>
    /// <param name="isContainOneself">是否包含顶级节点本身</param>
    /// <returns></returns>
    public static IEnumerable<T> ToChildList<T, P>(this IEnumerable<T> list,
        Expression<Func<T, P>> idExpression,
        Expression<Func<T, P>> parentIdExpression,
        object topParentIdValue,
        bool isContainOneself = true)
    {
        if (list == null || !list.Any()) return Enumerable.Empty<T>();

        var propId = GetPropertyInfo(idExpression);
        var propParentId = GetPropertyInfo(parentIdExpression);

        // 查找所有顶级节点
        var topNodes = list.Where(item => Equals(propId.GetValue(item), topParentIdValue)).ToList();

        return TraverseHierarchy(list, propId, propParentId, topNodes, isContainOneself);
    }

    /// <summary>
    /// 查询有父子关系的数据集
    /// </summary>
    /// <param name="list">数据集</param>
    /// <param name="idExpression">主键ID字段</param>
    /// <param name="parentIdExpression">父级字段</param>
    /// <param name="topLevelPredicate">顶级节点的选择条件</param>
    /// <param name="isContainOneself">是否包含顶级节点本身</param>
    /// <returns></returns>
    public static IEnumerable<T> ToChildList<T, P>(this IEnumerable<T> list,
        Expression<Func<T, P>> idExpression,
        Expression<Func<T, P>> parentIdExpression,
        Expression<Func<T, bool>> topLevelPredicate,
        bool isContainOneself = true)
    {
        if (list == null || !list.Any()) return Enumerable.Empty<T>();

        // 获取顶级节点
        var topNodes = list.Where(topLevelPredicate.Compile()).ToList();

        if (!topNodes.Any()) return Enumerable.Empty<T>();

        var idPropertyInfo = GetPropertyInfo(idExpression);
        var parentPropertyInfo = GetPropertyInfo(parentIdExpression);

        return TraverseHierarchy(list, idPropertyInfo, parentPropertyInfo, topNodes, isContainOneself);
    }

    /// <summary>
    /// 辅助方法，从表达式中提取属性信息并使用临时缓存
    /// </summary>
    private static PropertyInfo GetPropertyInfo<T, P>(Expression<Func<T, P>> expression)
    {
        // 使用 ConcurrentDictionary 确保线程安全
        return PropertyCache.GetOrAdd(typeof(T).FullName + "." + ((MemberExpression)expression.Body).Member.Name, k =>
        {
            if (expression.Body is UnaryExpression { Operand: MemberExpression member }) return (PropertyInfo)member.Member;
            if (expression.Body is MemberExpression memberExpression) return (PropertyInfo)memberExpression.Member;
            throw Oops.Oh("表达式必须是一个属性访问: " + expression);
        });
    }

    /// <summary>
    /// 使用队列遍历层级结构
    /// </summary>
    private static IEnumerable<T> TraverseHierarchy<T>(IEnumerable<T> list,
        PropertyInfo idPropertyInfo,
        PropertyInfo parentPropertyInfo,
        List<T> topNodes,
        bool isContainOneself)
    {
        var queue = new Queue<T>(topNodes);
        var result = new HashSet<T>(topNodes);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            var children = list.Where(item => Equals(parentPropertyInfo.GetValue(item), idPropertyInfo.GetValue(currentNode))).ToList();
            children.Where(child => result.Add(child)).ForEach(child => queue.Enqueue(child));
        }
        if (isContainOneself) return result;

        // 如果不需要包含顶级节点本身，则移除它们
        topNodes.ForEach(e => result.Remove(e));

        return result;
    }
}