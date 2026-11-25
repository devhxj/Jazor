


using System.Text.Json;
namespace Jero.Core.Service;

/// <summary>
/// 翻译服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 100, Description = "翻译服务")]
public partial class SysLangTextService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysLangText> _sysLangTextRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SysLangTextCacheService _sysLangTextCacheService;

    public SysLangTextService(
        SqlSugarRepository<SysLangText> sysLangTextRep,
        SysLangTextCacheService sysLangTextCacheService,
        ISqlSugarClient sqlSugarClient)
    {
        _sysLangTextRep = sysLangTextRep;
        _sqlSugarClient = sqlSugarClient;
        _sysLangTextCacheService = sysLangTextCacheService;
    }

    /// <summary>
    /// 分页查询翻译表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询翻译表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<SysLangTextOutput>> Page(PageSysLangTextInput input)
    {
        var keyword = input.Keyword?.Trim() ?? string.Empty;
        var entityName = input.EntityName?.Trim() ?? string.Empty;
        var fieldName = input.FieldName?.Trim() ?? string.Empty;
        var langCode = input.LangCode?.Trim() ?? string.Empty;
        var content = input.Content?.Trim() ?? string.Empty;

        var query = _sysLangTextRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u => (!string.IsNullOrWhiteSpace(u.EntityName) && u.EntityName.Contains(keyword)) || (!string.IsNullOrWhiteSpace(u.FieldName) && u.FieldName.Contains(keyword)) || (!string.IsNullOrWhiteSpace(u.LangCode) && u.LangCode.Contains(keyword)) || (!string.IsNullOrWhiteSpace(u.Content) && u.Content.Contains(keyword)))
            .WhereIF(!string.IsNullOrWhiteSpace(entityName), u => !string.IsNullOrWhiteSpace(u.EntityName) && u.EntityName.Contains(entityName))
            .WhereIF(!string.IsNullOrWhiteSpace(fieldName), u => !string.IsNullOrWhiteSpace(u.FieldName) && u.FieldName.Contains(fieldName))
            .WhereIF(!string.IsNullOrWhiteSpace(langCode), u => !string.IsNullOrWhiteSpace(u.LangCode) && u.LangCode.Contains(langCode))
            .WhereIF(!string.IsNullOrWhiteSpace(content), u => !string.IsNullOrWhiteSpace(u.Content) && u.Content.Contains(content))
            .WhereIF(input.EntityId != null, u => u.EntityId == input.EntityId)
            .Select<SysLangTextOutput>();
        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    [DisplayName("获取翻译表")]
    [ApiDescriptionSettings(Name = "List"), HttpPost]
    public async Task<List<SysLangTextOutput>> List(ListSysLangTextInput input)
    {
        var query = _sysLangTextRep.AsQueryable()
            .Where(u => u.EntityName == input.EntityName.Trim() && u.FieldName == input.FieldName.Trim() && u.EntityId == input.EntityId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.LangCode), u => u.LangCode == input.LangCode.Trim())
            .Select<SysLangTextOutput>();
        return await query.ToListAsync();
    }

    /// <summary>
    /// 获取翻译表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取翻译表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<SysLangText> Detail([FromQuery] QueryByIdSysLangTextInput input)
    {
        return await _sysLangTextRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加翻译表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加翻译表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddSysLangTextInput input)
    {
        var entity = input.Adapt<SysLangText>();
        return await _sysLangTextRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新翻译表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新翻译表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateSysLangTextInput input)
    {
        var entity = input.Adapt<SysLangText>();
        await _sysLangTextRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
        _sysLangTextCacheService.UpdateCache(entity.EntityName, entity.FieldName, entity.EntityId, entity.LangCode, entity.Content);
    }

    /// <summary>
    /// 删除翻译表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除翻译表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteSysLangTextInput input)
    {
        var entity = await _sysLangTextRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

        await _sysLangTextRep.DeleteAsync(entity);   //真删除
        _sysLangTextCacheService.DeleteCache(entity.EntityName, entity.FieldName, entity.EntityId, entity.LangCode);
    }

    /// <summary>
    /// 批量删除翻译表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除翻译表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task BatchDelete([Required(ErrorMessage = "主键列表不能为空")] List<DeleteSysLangTextInput> input)
    {
        var exp = Expressionable.Create<SysLangText>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _sysLangTextRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        await _sysLangTextRep.DeleteAsync(list);   //真删除
        foreach (var item in list)
        {
            _sysLangTextCacheService.DeleteCache(item.EntityName, item.FieldName, item.EntityId, item.LangCode);
        }
    }

    private static readonly object _sysLangTextBatchSaveLock = new object();

    /// <summary>
    /// 批量保存翻译表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量保存翻译表")]
    [ApiDescriptionSettings(Name = "BatchSave"), HttpPost]
    public void BatchSave([Required(ErrorMessage = "列表不能为空")] List<ImportSysLangTextInput> input)
    {
        lock (_sysLangTextBatchSaveLock)
        {
            // 校验并过滤必填基本类型为null的字段
            var rows = input.Where(x =>
            {
                if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                if (x.EntityId == null)
                {
                    x.Error = "所属实体ID不能为空";
                    return false;
                }
                return true;
            }).Adapt<List<SysLangText>>();

            var storageable = _sysLangTextRep.Context.Storageable(rows)
                .SplitError(it => string.IsNullOrWhiteSpace(it.Item.EntityName), "所属实体名不能为空")
                .SplitError(it => it.Item.EntityName?.Length > 255, "所属实体名长度不能超过255个字符")
                .SplitError(it => string.IsNullOrWhiteSpace(it.Item.FieldName), "字段名不能为空")
                .SplitError(it => it.Item.FieldName?.Length > 255, "字段名长度不能超过255个字符")
                .SplitError(it => string.IsNullOrWhiteSpace(it.Item.LangCode), "语言代码不能为空")
                .SplitError(it => it.Item.LangCode?.Length > 255, "语言代码长度不能超过255个字符")
                .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Content), "翻译内容不能为空")
                .WhereColumns(it => new { it.EntityId, it.EntityName, it.FieldName, it.LangCode })
                .SplitInsert(it => it.NotAny())
                .SplitUpdate(it => it.Any())
                .ToStorage();

            storageable.AsInsertable.ExecuteCommand();// 不存在插入
            storageable.AsUpdateable.UpdateColumns(it => new
            {
                it.EntityName,
                it.EntityId,
                it.FieldName,
                it.LangCode,
                it.Content,
            }).ExecuteCommand();// 存在更新
            foreach (var item in rows)
            {
                _sysLangTextCacheService.DeleteCache(item.EntityName, item.FieldName, item.EntityId, item.LangCode);
            }
            if (storageable.ErrorList.Any())
            {
                throw Oops.Oh($"处理过程中出现以下错误：{string.Join("；", storageable.ErrorList.Distinct())}");
            }
        }
    }

    /// <summary>
    /// 导出翻译表记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出翻译表记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageSysLangTextInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportSysLangTextOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "翻译表导出记录");
    }

    /// <summary>
    /// 下载翻译表数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载翻译表数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportSysLangTextOutput>(), "翻译表导入模板");
    }

    private static readonly object _sysLangTextImportLock = new object();

    /// <summary>
    /// 导入翻译表记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入翻译表记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_sysLangTextImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportSysLangTextInput, SysLangText>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x =>
                    {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.EntityId == null)
                        {
                            x.Error = "所属实体ID不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<SysLangText>>();

                    var storageable = _sysLangTextRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.EntityName), "所属实体名不能为空")
                        .SplitError(it => it.Item.EntityName?.Length > 255, "所属实体名长度不能超过255个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.FieldName), "字段名不能为空")
                        .SplitError(it => it.Item.FieldName?.Length > 255, "字段名长度不能超过255个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.LangCode), "语言代码不能为空")
                        .SplitError(it => it.Item.LangCode?.Length > 255, "语言代码长度不能超过255个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Content), "翻译内容不能为空")
                        .SplitError(it => it.Item.Content?.Length > 255, "翻译内容长度不能超过255个字符")
                        .WhereColumns(it => new { it.EntityId, it.EntityName, it.FieldName, it.LangCode })
                        .SplitInsert(it => it.NotAny())
                        .SplitUpdate(it => it.Any())
                        .ToStorage();

                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.EntityName,
                        it.EntityId,
                        it.FieldName,
                        it.LangCode,
                        it.Content,
                    }).ExecuteCommand();// 存在更新

                    foreach (var item in rows)
                    {
                        _sysLangTextCacheService.DeleteCache(item.EntityName, item.FieldName, item.EntityId, item.LangCode);
                    }
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });

            return stream;
        }
    }

    /// <summary>
    /// DEEPSEEK 翻译接口
    /// </summary>
    /// <returns></returns>
    [DisplayName("DEEPSEEK 翻译接口")]
    [ApiDescriptionSettings(Name = "AiTranslateText"), HttpPost]
    public async Task<string> AiTranslateText(AiTranslateTextInput input)
    {
        // 需要先把DeepSeek.example复制改名为DeepSeek.json文件，添加你的 API KEY
        var deepSeekOptions = App.GetConfig<DeepSeekOptions>("DeepSeekSettings", true);
        if (deepSeekOptions == null)
        {
            throw new InvalidOperationException("DeepSeek.json文件 未定义");
        }
        if (string.IsNullOrEmpty(deepSeekOptions.ApiKey))
        {
            throw new InvalidOperationException("环境变量 DEEPSEEK_API_KEY 未定义");
        }

        using (HttpClient client = new HttpClient())
        {
            // 构建请求头
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {deepSeekOptions.ApiKey}");

            // 构建系统提示词
            string systemPrompt = BuildSystemPrompt(deepSeekOptions.SourceLang, input.TargetLang);

            // 构建请求体
            var requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = input.OriginalText }
                },
                temperature = 0.3,
                max_tokens = 2000
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 发送请求
            HttpResponseMessage response = await client.PostAsync(deepSeekOptions.ApiUrl, content);

            // 处理响应
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, ResponseJsonOptions);
                string errorMsg = errorResponse?.error?.message ?? $"HTTP {response.StatusCode}: {response.ReasonPhrase}";
                throw new HttpRequestException($"翻译API返回错误：{errorMsg}");
            }

            // 解析有效响应
            var result = JsonSerializer.Deserialize<TranslationResponse>(responseBody, ResponseJsonOptions);

            var translatedContent = result?.choices is { Length: > 0 }
                ? result.choices[0]?.message?.content
                : null;
            if (string.IsNullOrWhiteSpace(translatedContent))
            {
                throw new InvalidOperationException("API返回无效的翻译结果");
            }

            return translatedContent.Trim();
        }
    }

    private static readonly JsonSerializerOptions ResponseJsonOptions = new() { PropertyNameCaseInsensitive = true };

    // JSON 响应模型
    private class TranslationResponse
    {
        public Choice[] choices { get; set; } = [];
    }

    private class Choice
    {
        public Message? message { get; set; }
    }

    private class Message
    {
        public string content { get; set; } = string.Empty;
    }

    private class ErrorResponse
    {
        public ErrorInfo? error { get; set; }
    }

    private class ErrorInfo
    {
        public string message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 生成提示词
    /// </summary>
    /// <param name="sourceLang"></param>
    /// <param name="targetLang"></param>
    /// <returns></returns>
    private static string BuildSystemPrompt(string sourceLang, string targetLang)
    {
        return $@"作为企业软件系统专业翻译，严格遵守以下铁律：

■ 核心原则
1. 严格逐符号翻译（{sourceLang}→{targetLang}）
2. 禁止添加/删除/改写任何内容
3. 保持批量翻译的编号格式

■ 符号保留规则
! 所有符号必须原样保留：
• 编程符号：\${{ }} <% %> @ # & |
• UI占位符：{{0}} %s [ ]
• 货币单位：¥100.00 kg cm²
• 中文符号：【 】 《 》 ：

■ 中文符号位置规范
# 三级处理机制：
1. 成对符号必须保持完整结构：
   ✓ 正确：【Warning】Text
   ✗ 禁止：Warning【 】Text

2. 独立符号位置：
   • 优先句尾 → Text】?
   • 次选句首 → 】Text?
   • 禁止句中 → Text】Text?

3. 跨字符串符号处理：
   • 前段含【时 → 保留在段尾（""Synchronize【""）
   • 后段含】时 → 保留在段首（""】authorization data?""）
   • 符号后接字母时添加空格：】 Authorization

■ 语法规范
• 外文 → 被动语态（""Item was created""）
• 中文 → 主动语态（""已创建项目""）
• 禁止推测上下文（只翻译当前字符串内容）

■ 错误预防（绝对禁止）
✗ 将中文符号改为西式符号（】→]）
✗ 移动非中文符号位置
✗ 添加原文不存在的内容
✗ 合并/拆分原始字符串

■ 批量处理
▸ 严格保持原始JSON结构
▸ 语言键名精确匹配（zh-cn/en/it等）";
    }
}
