namespace Jero.Core;

public static class SqlSugarFilterExtension
{
    /// <summary>
    /// 根据指定Attribute获取属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    private static List<string> GetPropertyNames<T>(this Type type) where T : Attribute
    {
        return type.GetProperties()
            .Where(p => p.CustomAttributes.Any(x => x.AttributeType == typeof(T)))
            .Select(x => x.Name).ToList();
    }

    /// <summary>
    /// 获取过滤表达式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="owners"></param>
    /// <returns></returns>
    public static LambdaExpression GetConditionExpression<T>(this Type type, List<long> owners) where T : Attribute
    {
        var fieldNames = type.GetPropertyNames<T>();
        ParameterExpression parameter = Expression.Parameter(type, "c");

        Expression right = Expression.Constant(false);
        ConstantExpression ownersCollection = Expression.Constant(owners);
        foreach (var fieldName in fieldNames)
        {
            var property = type.GetProperty(fieldName);
            if (property == null) continue;

            Expression memberExp = Expression.Property(parameter, property);

            // 如果属性是可为空的类型，则转换为其基础类型
            var baseType = Nullable.GetUnderlyingType(property.PropertyType);
            if (baseType != null) memberExp = Expression.Convert(memberExp, baseType);

            // 调用ownersCollection.Contains方法，检查是否包含属性值
            right = Expression.OrElse(Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Contains),
                new[] { memberExp.Type },
                ownersCollection,
                memberExp
            ), right);
        }
        return Expression.Lambda(right, parameter);
    }
}
