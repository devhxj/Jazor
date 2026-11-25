using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jero.Core.Service;

/// <summary>
/// APIJSON服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class APIJSONService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient _db;
    private readonly IdentityService _identityService;
    private readonly TableMapper _tableMapper;
    private readonly SelectTable _selectTable;

    public APIJSONService(ISqlSugarClient db,
        IdentityService identityService,
        TableMapper tableMapper)
    {
        _db = db;
        _tableMapper = tableMapper;
        _identityService = identityService;
        _selectTable = new SelectTable(_identityService, _tableMapper, _db);
    }

    /// <summary>
    /// 统一查询入口 🔖
    /// </summary>
    /// <param name="jobject"></param>
    /// <remarks>参数：{"[]":{"SYSLOGOP":{}}}</remarks>
    /// <returns></returns>
    [HttpPost("get")]
    [DisplayName("APIJSON统一查询")]
    public JsonObject Query([FromBody] JsonObject jobject)
    {
        var database = jobject["@database"]?.ToString();
        if (!string.IsNullOrEmpty(database))
        {
            // 设置数据库
            var provider = _db.AsTenant().GetConnectionScope(database);
            jobject.Remove("@database");
            return new SelectTable(_identityService, _tableMapper, provider).Query(jobject);
        }
        return _selectTable.Query(jobject);
    }

    /// <summary>
    /// 查询 🔖
    /// </summary>
    /// <param name="table"></param>
    /// <param name="jobject"></param>
    /// <returns></returns>
    [HttpPost("get/{table}")]
    [DisplayName("APIJSON查询")]
    public JsonObject QueryByTable([FromRoute] string table, [FromBody] JsonObject jobject)
    {
        var ht = new JsonObject
        {
            { table + "[]", jobject }
        };

        // 自动添加总计数量
        if (ApiJsonValue.ReadInt(jobject["query"]) is not (null or 0) && !jobject.ContainsKey("total@"))
            ht.Add("total@", "");

        // 每页最大1000条数据
        if (ApiJsonValue.ReadInt(jobject["count"]) is > 1000)
            throw Oops.Bah("count分页数量最大不能超过1000");

        jobject.Remove("@debug");

        var hasTableKey = false;
        var ignoreConditions = new List<string> { "page", "count", "query" };
        var tableConditions = new JsonObject(); // 表的其它查询条件，比如过滤、字段等
        foreach (var item in jobject)
        {
            if (item.Key.Equals(table, StringComparison.CurrentCultureIgnoreCase))
            {
                hasTableKey = true;
                break;
            }
            if (!ignoreConditions.Contains(item.Key.ToLower()))
                tableConditions.Add(item.Key, item.Value?.DeepClone());
        }

        foreach (var removeKey in tableConditions)
        {
            jobject.Remove(removeKey.Key);
        }

        if (!hasTableKey)
            jobject.Add(table, tableConditions);

        return Query(ht);
    }

    /// <summary>
    /// 新增 🔖
    /// </summary>
    /// <param name="tables">表对象或数组，若没有传Id则后端生成Id</param>
    /// <returns></returns>
    [HttpPost("add")]
    [DisplayName("APIJSON新增")]
    [UnitOfWork]
    public JsonObject Add([FromBody] JsonObject tables)
    {
        var ht = new JsonObject();
        foreach (var table in tables)
        {
            var talbeName = table.Key.Trim();
            var role = _identityService.GetRole();
            if (role.Insert?.Table == null)
                throw Oops.Bah("insert权限未配置");
            if (!role.Insert.Table.Contains(talbeName, StringComparer.CurrentCultureIgnoreCase))
                throw Oops.Bah($"没权限添加{talbeName}");

            JsonNode? result;
            // 批量插入
            if (table.Value is JsonArray array)
            {
                var ids = new List<object>();
                foreach (var record in array)
                {
                    var cols = record as JsonObject ?? throw Oops.Bah($"{talbeName}批量新增记录必须是对象");
                    var id = _selectTable.InsertSingle(talbeName, cols, role);
                    ids.Add(id);
                }
                result = JsonSerializer.SerializeToNode(new { id = ids, count = ids.Count });
            }
            // 单条插入
            else
            {
                var cols = table.Value as JsonObject ?? throw Oops.Bah($"{talbeName}新增记录必须是对象");
                var id = _selectTable.InsertSingle(talbeName, cols, role);
                result = JsonSerializer.SerializeToNode(new { id });
            }
            ht.Add(talbeName, result);
        }
        return ht;
    }

    /// <summary>
    /// 更新（只支持Id作为条件） 🔖
    /// </summary>
    /// <param name="tables">支持多表、多Id批量更新</param>
    /// <returns></returns>
    [HttpPost("update")]
    [DisplayName("APIJSON更新")]
    [UnitOfWork]
    public JsonObject Edit([FromBody] JsonObject tables)
    {
        var ht = new JsonObject();
        foreach (var table in tables)
        {
            var tableName = table.Key.Trim();
            var role = _identityService.GetRole();
            var records = table.Value ?? throw Oops.Bah($"{tableName}更新记录不能为空");
            var count = _selectTable.UpdateSingleTable(tableName, records, role);
            ht.Add(tableName, JsonSerializer.SerializeToNode(new { count }));
        }
        return ht;
    }

    /// <summary>
    /// 删除（支持非Id条件、支持批量） 🔖
    /// </summary>
    /// <param name="tables"></param>
    /// <returns></returns>
    [HttpPost("delete")]
    [DisplayName("APIJSON删除")]
    [UnitOfWork]
    public JsonObject Delete([FromBody] JsonObject tables)
    {
        var ht = new JsonObject();
        var role = _identityService.GetRole();
        foreach (var table in tables)
        {
            var talbeName = table.Key.Trim();
            if (role.Delete == null || role.Delete.Table == null)
                throw Oops.Bah("delete权限未配置");
            if (!role.Delete.Table.Contains(talbeName, StringComparer.CurrentCultureIgnoreCase))
                throw Oops.Bah($"没权限删除{talbeName}");
            //if (!value.ContainsKey("id"))
            //    throw Oops.Bah("未传主键id");

            if (table.Value is not JsonObject value)
                throw Oops.Bah($"{talbeName}删除条件必须是 JSON 对象");
            var sb = new StringBuilder(100);
            var parameters = new List<SugarParameter>();
            foreach (var f in value)
            {
                if (f.Value is JsonArray array)
                {
                    sb.Append($"{f.Key} in (@{f.Key}) and ");
                    var paraArray = FuncList.TransJsonArrayToSugarPara(array);
                    parameters.Add(new SugarParameter($"@{f.Key}", paraArray));
                }
                else
                {
                    sb.Append($"{f.Key}=@{f.Key} and ");
                    parameters.Add(new SugarParameter($"@{f.Key}", FuncList.TransJsonObjectToSugarPara(f.Value)));
                }
            }
            if (!parameters.Any())
                throw Oops.Bah("请输入删除条件");

            var whereSql = sb.ToString();
            if (whereSql.EndsWith(" and ", StringComparison.Ordinal))
                whereSql = whereSql[..^5];
            var count = _db.Deleteable<object>().AS(talbeName).Where(whereSql, parameters).ExecuteCommand(); // 无实体删除
            value.Add("count", count); // 命中数量
            ht.Add(talbeName, value);
        }
        return ht;
    }
}
