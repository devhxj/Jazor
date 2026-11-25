


/*
 *━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 *  文件名称：BaiDuTranslationService
 *  创建时间：2025年03月25日 星期二 20:54:04
 *  创 建 者:莫闻啼
 *━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 *  功能描述:
 *     调用百度翻译Api接口在线翻译,在DeBug模式下生成前端i18n Ts翻译key value,需要先维护对应目录下的zh-CN.ts,对比对应语言包下不存在的key,将value进行翻译并新增到对应语言包文件中
 *
 *━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 */

using System.Security.Cryptography;
namespace Jero.Core;

/// <summary>
/// 百度翻译
/// </summary>
[ApiDescriptionSettings("Extend", Module = "Extend", Order = 200)]
public class BaiDuTranslationService : IDynamicApiController, ITransient
{
    // http远程请求
    private readonly IHttpRemoteService _httpRemoteService;

    /// <summary>
    /// 百度翻译appId
    /// </summary>
    private static readonly string _appId = "xxxxxxxxxxx";

    /// <summary>
    /// 百度翻译appKey
    /// </summary>
    private static readonly string _appKey = "xxxxxxxxxxx";

    /// <summary>
    /// 百度翻译api地址
    /// </summary>
    private static readonly string _baseUrl = "https://fanyi-api.baidu.com/api/trans/vip/translate?";

    // 语言映射字典
    private static readonly Dictionary<string, string> langMap = new Dictionary<string, string>
    {
        ["en"] = "en",
        ["de"] = "de",
        ["fi"] = "fin",
        ["es"] = "spa",
        ["fr"] = "fra",
        ["it"] = "it",
        ["ja"] = "jp",
        ["ko"] = "kor",
        ["no"] = "nor",
        ["pl"] = "pl",
        ["pt"] = "pt",
        ["ru"] = "ru",
        ["th"] = "th",
        ["id"] = "id",
        ["ms"] = "may",
        ["vi"] = "vie",
        ["zh-HK"] = "yue",
        ["zh-TW"] = "cht"
    };

    /// <summary>
    /// 初始化一个<see cref="BaiDuTranslationService"/>类型的新实例.
    /// </summary>
    /// <param name="httpRemoteService"></param>
    public BaiDuTranslationService(IHttpRemoteService httpRemoteService)
    {
        _httpRemoteService = httpRemoteService;
    }

    /// <summary>
    /// 百度在线翻译
    /// </summary>
    /// <param name="from">翻译源语种</param>
    /// <param name="to">翻译目标语种</param>
    /// <param name="content">文本内容</param>
    ///<remarks>
    ///源语种和目标语种支持：
    ///zh:简体中文
    ///cht:繁體中文(台灣)
    ///yue:繁體中文(香港)
    ///en:英语
    ///de:德语
    ///spa:西班牙语
    ///fin:芬兰语
    ///fra:法语
    ///it:意大利语
    ///jp:日语
    ///kor:韩语
    ///nor:挪威语
    ///pl:波兰语
    ///pt:葡萄牙语
    ///ru:俄语
    ///th:泰语
    ///id:印度尼西亚语
    ///may:马来西亚
    ///vie:越南语
    ///
    ///更多语种请查看：https://api.fanyi.baidu.com/doc/21
    /// </remarks>
    /// <returns>翻译后的文本内容</returns>
    [DisplayName("百度在线翻译")]
    [HttpGet]
    public async Task<BaiDuTranslationResult> Translation([FromQuery][Required] string from, [FromQuery][Required] string to, [FromQuery][Required] string content)
    {
        // 标准版API授权只能翻译基础18语言，201种需要企业尊享版支持见百度api 文档
        Random rd = new Random();
        string salt = rd.Next(100000).ToString();
        // 改成您的密钥
        string secretKey = _appKey;
        string sign = EncryptString(_appId + content + salt + secretKey);
        string url = $"{_baseUrl}q={HttpUtility.UrlEncode(content)}&from={from}&to={to}&appid={_appId}&salt={salt}&sign={sign}";
        var res = await _httpRemoteService.GetAsAsync<BaiDuTranslationResult>(url) ?? throw Oops.Bah("翻译失败,百度接口响应为空");

        if (!string.Equals(res.error_code, "0", StringComparison.Ordinal))
        {
            throw Oops.Bah($"翻译失败,错误码:{res.error_code},错误信息:{res.error_msg}");
        }

        if (res.trans_result.Count == 0) throw Oops.Bah("翻译失败,百度接口未返回翻译结果");

        return res;
    }

#if DEBUG

    /// <summary>
    /// 生成前端页面i18n文件
    /// </summary>
    [DisplayName("生成前端页面i18n文件")]
    [HttpPost]
    public async Task GeneratePageI18nFile()
    {
        try
        {
            // 获取基础路径
            var i18nPath = ResolveI18nPath("Web", "src", "i18n", "pages", "systemMenu");

            // 读取基础语言文件
            var dic = await ReadBaseLanguageFile(i18nPath);

            if (dic.Count == 0)
            {
                throw Oops.Bah("未查询到属性定义,不能生成");
            }

            // 并行处理所有语言文件
            var files = Directory.GetFiles(i18nPath, "*.ts").Where(f => !f.EndsWith("zh-CN.ts")).ToList();

            foreach (var file in files)
            {
                var langCode = Path.GetFileNameWithoutExtension(file);
                var langDic = await ReadLanguageFile(file);

                // 查询出没有生成的键值对

                // Linq查询
                // var notGen = dic.Where(kv => !langDic.ContainsKey(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                // 转换为 HashSet 提升性能
                var langDicKey = new HashSet<string>(langDic.Keys);
                var notGen = dic.Where(kv => !langDicKey.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);

                // 没有未生成的跳出
                if (notGen.Count == 0)
                {
                    Console.WriteLine($"{langCode,-6} 语言包:{langDic.Count}/共:{dic.Count} 已全部生成,无需再次生成");
                    continue;
                }

                var str = string.Empty;
                Console.WriteLine($"{langCode,-6}开始生成语言包,未生成:{notGen.Count}/已生成:{langDic.Count}/共{dic.Count}");
                foreach (var gen in notGen)
                {
                    try
                    {
                        if (!langMap.TryGetValue(langCode, out var targetLang))
                        {
                            continue;
                        }

                        var result = await Translation("zh", targetLang, $"{gen.Value}");

                        if (!string.Equals(result.error_code, "0", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        var translationValue = result.trans_result[0].Dst;
                        LogTranslationProgress(gen.Key, gen.Value, translationValue, ConsoleColor.DarkMagenta);

                        // 如果翻译结果为空字符串不追加
                        if (string.IsNullOrEmpty(translationValue))
                        {
                            continue;
                        }

                        // 如果翻译结果包含"'" 法语意大利语常出现  在"'"前加转义符
                        if (translationValue.Contains("'"))
                        {
                            translationValue = translationValue.Replace("'", "\\'");
                        }

                        str += ($"        {gen.Key}: '{translationValue}',{Environment.NewLine}");
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                    }
                }

                if (str.Length > 0)
                {
                    str = str.TrimStart();
                    await FileHelper.InsertsStringAtSpecifiedLocationInFile(file, str, '}', 2, false);
                }
            }
        }
        catch (Exception e)
        {
            throw Oops.Bah(e.Message);
        }
    }

    /// <summary>
    /// 生成前端菜单i18n文件
    /// </summary>
    [DisplayName("生成前端菜单i18n文件")]
    [HttpPost]
    public async Task GenerateMenuI18nFile()
    {
        try
        {
            // 获取基础路径
            var i18nPath = ResolveI18nPath("Web", "src", "i18n", "menu");

            // 读取基础语言文件
            var dic = await ReadBaseLanguageFile(i18nPath);

            if (dic.Count == 0)
            {
                throw Oops.Bah("未查询到属性定义,不能生成");
            }

            // 并行处理所有语言文件
            var files = Directory.GetFiles(i18nPath, "*.ts").Where(f => !f.EndsWith("zh-CN.ts")).ToList();

            foreach (var file in files)
            {
                var langCode = Path.GetFileNameWithoutExtension(file);
                var langDic = await ReadLanguageFile(file);

                // 查询出没有生成的键值对

                // Linq查询
                // var notGen = dic.Where(kv => !langDic.ContainsKey(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                // 转换为 HashSet 提升性能
                var langDicKey = new HashSet<string>(langDic.Keys);
                var notGen = dic.Where(kv => !langDicKey.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);

                // 没有未生成的跳出
                if (notGen.Count == 0)
                {
                    Console.WriteLine($"{langCode,-6} 语言包:{langDic.Count}/共:{dic.Count} 已全部生成,无需再次生成");
                    continue;
                }

                var str = string.Empty;
                Console.WriteLine($"{langCode,-6}开始生成语言包,未生成:{notGen.Count}/已生成:{langDic.Count}/共{dic.Count}");
                foreach (var gen in notGen)
                {
                    try
                    {
                        if (!langMap.TryGetValue(langCode, out var targetLang))
                        {
                            continue;
                        }

                        var result = await Translation("zh", targetLang, $"{gen.Value}");

                        if (!string.Equals(result.error_code, "0", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        var translationValue = result.trans_result[0].Dst;
                        LogTranslationProgress(gen.Key, gen.Value, translationValue, ConsoleColor.DarkMagenta);

                        // 如果翻译结果为空字符串不追加
                        if (string.IsNullOrEmpty(translationValue))
                        {
                            continue;
                        }

                        // 如果翻译结果包含"'" 法语意大利语常出现  在"'"前加转义符
                        if (translationValue.Contains("'"))
                        {
                            translationValue = translationValue.Replace("'", "\\'");
                        }

                        str += ($"        {gen.Key}: '{translationValue}',{Environment.NewLine}");
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                    }
                }

                if (str.Length > 0)
                {
                    str = str.TrimStart();
                    await FileHelper.InsertsStringAtSpecifiedLocationInFile(file, str, '}', 2, false);
                }
            }
        }
        catch (Exception e)
        {
            throw Oops.Bah(e.Message);
        }
    }

    #region 辅助方法

    private static async Task<Dictionary<string, string>> ReadBaseLanguageFile(string i18nPath)
    {
        var baseFile = Path.Combine(i18nPath, "zh-CN.ts");
        if (!File.Exists(baseFile))
        {
            throw Oops.Bah("【zh-CN.ts】文件未找到");
        }

        var dic = new Dictionary<string, string>();
        using var reader = new StreamReader(baseFile, Encoding.UTF8);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (line.Contains('{') || line.Contains('}')) continue;

            var cleanLine = line.Trim().TrimEnd(',').Replace("'", "");
            var parts = cleanLine.Split(new[] { ':' }, 2);
            if (parts.Length == 2) dic[parts[0].Trim()] = parts[1].Trim();
        }

        reader.Close();
        return dic;
    }

    private static string ResolveI18nPath(params string[] paths)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 6; i++)
        {
            directory = directory.Parent ?? throw Oops.Bah("无法定位前端 i18n 目录");
        }

        return Path.Combine(new[] { directory.FullName }.Concat(paths).ToArray());
    }

    private static async Task<Dictionary<string, string>> ReadLanguageFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw Oops.Bah($"【{filePath.Split('/').Last()}】文件未找到");
        }

        var dic = new Dictionary<string, string>();
        using var reader = new StreamReader(filePath, Encoding.UTF8);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (line.Contains('{') || line.Contains('}')) continue;

            var cleanLine = line.Trim().TrimEnd(',').Replace("'", "");
            var parts = cleanLine.Split(new[] { ':' }, 2);
            if (parts.Length == 2) dic[parts[0].Trim()] = parts[1].Trim();
        }

        reader.Close();
        return dic;
    }

    private static void LogTranslationProgress(string key, string value, string res, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"翻译属性: {key,-32}值: {value,-64}结果: {res}");
        Console.ResetColor();
    }

    private static void LogError(Exception e)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"{e.Message}");
        Console.ResetColor();
    }

    #endregion 辅助方法

#endif

    // 计算MD5值
    [NonAction]
    private static string EncryptString(string str)
    {
        MD5 md5 = MD5.Create();
        // 将字符串转换成字节数组
        byte[] byteOld = Encoding.UTF8.GetBytes(str);
        // 调用加密方法
        byte[] byteNew = md5.ComputeHash(byteOld);
        // 将加密结果转换为字符串
        StringBuilder sb = new StringBuilder();
        foreach (byte b in byteNew)
        {
            // 将字节转换成16进制表示的字符串，
            sb.Append(b.ToString("x2"));
        }

        // 返回加密的字符串
        return sb.ToString();
    }
}
