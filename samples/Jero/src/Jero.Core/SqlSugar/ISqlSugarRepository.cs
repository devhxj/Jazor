namespace Jero.Core;

/// <summary>
/// 分表操作仓储接口
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISqlSugarRepository<T> : ISugarRepository, ISimpleClient<T> where T : class, new()
{
    /// <summary>
    /// 创建数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<bool> SplitTableInsertAsync(T input);

    /// <summary>
    /// 批量创建数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<bool> SplitTableInsertAsync(List<T> input);

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<bool> SplitTableUpdateAsync(T input);

    /// <summary>
    /// 批量更新数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<bool> SplitTableUpdateAsync(List<T> input);

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<bool> SplitTableDeleteableAsync(T input);

    /// <summary>
    /// 批量删除数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<bool> SplitTableDeleteableAsync(List<T> input);

    /// <summary>
    /// 获取第一条
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    Task<T> SplitTableGetFirstAsync(Expression<Func<T, bool>> whereExpression);

    /// <summary>
    /// 判断是否存在
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    Task<bool> SplitTableIsAnyAsync(Expression<Func<T, bool>> whereExpression);

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <returns></returns>
    Task<List<T>> SplitTableGetListAsync();

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    Task<List<T>> SplitTableGetListAsync(Expression<Func<T, bool>> whereExpression);

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="tableNames">表名</param>
    /// <returns></returns>
    Task<List<T>> SplitTableGetListAsync(Expression<Func<T, bool>> whereExpression, string[] tableNames);
}