namespace Jero.Core;

/// <summary>
/// 视图实体接口
/// </summary>
public interface ISqlSugarView
{
    /// <summary>
    /// 获取视图查询sql语句
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public string GetQueryableSqlString(SqlSugarScopeProvider db);
}