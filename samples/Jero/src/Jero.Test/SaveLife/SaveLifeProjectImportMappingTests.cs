using System.Collections;
using System.Reflection;
using System.Text.Json;
using Magicodes.ExporterAndImporter.Core;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeProjectImportMappingTests
{
    private static readonly string[] V17TemplateHeaders =
    [
        "CDE登记号",
        "方案编号",
        "试验题目",
        "药物名称",
        "申办方",
        "试验阶段",
        "疾病领域",
        "状态",
        "目标例数",
        "参与者类型",
        "适应症",
        "入选标准",
        "排除标准",
        "参与者权益",
        "伦理委员会",
        "机构名称",
        "主要研究者",
        "省",
        "城市",
        "国家/地区",
    ];

    private static readonly string[] OptionalTemplateHeaders =
    [
        "项目描述",
        "已招募人数",
        "开始日期",
        "结束日期",
        "试验周期",
        "各参加单位信息",
    ];

    [Fact]
    public void ProjectImportMapping_WithV17ClinicalTrialColumns_BuildsTrial()
    {
        var input = new ImportSaveLifeProjectRowInput
        {
            CdeRegistrationNumber = "CTR20261793",
            ProtocolNumber = "HSK44459-301",
            Title = "评价HSK44459片在特发性肺纤维化患者中有效性和安全性的III期研究",
            DrugName = "HSK44459片",
            Sponsor = "四川海思科制药有限公司",
            ClinicalPhase = "III期",
            DiseaseArea = "呼吸系统疾病",
            Status = "进行中 尚未招募",
            TargetCaseCount = "394",
            ParticipantTypeText = "特发性肺纤维化",
            Indication = "特发性肺纤维化",
            InclusionCriteria = "年龄>=40岁",
            ExclusionCriteria = "存在有临床意义的气道阻塞",
            ParticipantBenefits = "受试者在试验期间享有知情同意、隐私保护等权益",
            EthicsCommittee = "经中国医学科学院北京协和医院药物临床试验伦理委员会批准",
            InstitutionNames = "中国医学科学院北京协和医院; 广州医科大学附属第一医院",
            PrincipalInvestigators = "徐作军; 韩茜",
            Provinces = "北京市; 广东省",
            Cities = "北京市; 广州市",
            Countries = "中国; 中国",
        };

        var trial = BuildTrialFromImportInput(input);

        Assert.Equal("CTR20261793", trial.CdeRegistrationNumber);
        Assert.Equal(394, trial.TargetCount);
        Assert.Equal("患者", trial.ParticipantType);
        Assert.Equal("recruiting", trial.Status);
        Assert.Equal("呼吸系统疾病", trial.DiseaseArea);
        Assert.Equal("特发性肺纤维化", trial.Indication);
        Assert.Contains("药物名称：HSK44459片", trial.Description);
        Assert.Contains("适应症：特发性肺纤维化", trial.Requirements);
        Assert.Contains("药物临床试验伦理委员会", trial.EthicsCommitteeInfo);

        using var sites = JsonDocument.Parse(trial.Sites!);
        Assert.Equal(2, sites.RootElement.GetArrayLength());
        Assert.Equal("中国医学科学院北京协和医院", sites.RootElement[0].GetProperty("institution_name").GetString());
        Assert.Equal("徐作军", sites.RootElement[0].GetProperty("principal_investigator").GetString());
        Assert.Equal("北京市", sites.RootElement[0].GetProperty("city").GetString());
    }

    [Fact]
    public void ProjectImportMapping_WithV17CsvHeaders_BuildsTrial()
    {
        var headers = V17TemplateHeaders.ToList();
        var cells = new List<string>
        {
            "CTR20261775",
            "ZPKLP.BE.HJ.ZK",
            "中国健康试验参与者空腹状态下单次口服佐匹克隆胶囊的生物等效性试验",
            "佐匹克隆胶囊",
            "天津华津制药有限公司",
            "生物等效性试验",
            "其他",
            "进行中 尚未招募",
            "32",
            "健康志愿者",
            "失眠症",
            "充分了解试验目的并签署知情同意书",
            "对本品及辅料中过敏者",
            "受试者在试验期间享有隐私保护、免费检查等权益",
            "经河北医科大学第一医院临床研究伦理审查委员会批准",
            "河北医科大学第一医院",
            "于静",
            "河北省",
            "石家庄市",
            "中国",
        };

        var trial = BuildTrialFromCells(headers, cells);

        Assert.Equal("CTR20261775", trial.CdeRegistrationNumber);
        Assert.Equal("健康志愿者", trial.ParticipantType);
        Assert.Equal("其他", trial.DiseaseArea);
        Assert.Equal("失眠症", trial.Indication);
        Assert.Equal(32, trial.TargetCount);
        Assert.Contains("药物名称：佐匹克隆胶囊", trial.Description);
        Assert.Contains("适应症：失眠症", trial.Requirements);
        Assert.Equal("石家庄市", trial.Location);
        Assert.Contains("河北医科大学第一医院", trial.Sites);

        using var sites = JsonDocument.Parse(trial.Sites!);
        Assert.Single(sites.RootElement.EnumerateArray());
        Assert.Equal("河北医科大学第一医院", sites.RootElement[0].GetProperty("institution_name").GetString());
        Assert.Equal("于静", sites.RootElement[0].GetProperty("principal_investigator").GetString());
        Assert.Equal("石家庄市", sites.RootElement[0].GetProperty("city").GetString());
    }

    [Fact]
    public async Task ProjectImportMapping_WithV17XlsxHeaders_BuildsTrial()
    {
        var file = BuildXlsxFormFile(
            V17TemplateHeaders,
            [
                "CTR20261775",
                "ZPKLP.BE.HJ.ZK",
                "中国健康试验参与者空腹状态下单次口服佐匹克隆胶囊的生物等效性试验",
                "佐匹克隆胶囊",
                "天津华津制药有限公司",
                "生物等效性试验",
                "其他",
                "进行中 尚未招募",
                "32",
                "健康志愿者",
                "失眠症",
                "充分了解试验目的并签署知情同意书",
                "对本品及辅料中过敏者",
                "受试者在试验期间享有隐私保护、免费检查等权益",
                "经河北医科大学第一医院临床研究伦理审查委员会批准",
                "河北医科大学第一医院",
                "于静",
                "河北省",
                "石家庄市",
                "中国",
            ]);

        var rows = await ReadProjectRowsFromXlsx(file);
        var trial = BuildTrialFromRow(rows.Single());

        Assert.Equal("CTR20261775", trial.CdeRegistrationNumber);
        Assert.Equal("健康志愿者", trial.ParticipantType);
        Assert.Equal(32, trial.TargetCount);
        Assert.Equal("石家庄市", trial.Location);
    }

    [Fact]
    public void ProjectImportMapping_WithTrialStatusHeader_PreservesCompletedStatus()
    {
        var trial = BuildTrialFromCells(
            ["CDE登记号", "试验题目", "方案编号", "试验状态"],
            ["CTR20260001", "已完成的临床试验", "SL-2026-001", "已完成"]);

        Assert.Equal("ended", trial.Status);
    }

    [Fact]
    public void ProjectImportMapping_WithTerminatedTrialStatus_MapsToEnded()
    {
        var trial = BuildTrialFromCells(
            ["CDE登记号", "试验题目", "方案编号", "试验状态"],
            ["CTR20260002", "主动终止的临床试验", "SL-2026-002", "主动终止"]);

        Assert.Equal("ended", trial.Status);
    }

    [Fact]
    public void ProjectImportMapping_WithMultilineCells_PreservesCompleteBusinessValues()
    {
        var trial = BuildTrialFromCells(
            ["CDE登记号", "试验题目", "方案编号", "申办方", "试验阶段", "入选标准"],
            [
                "CTR20260003",
                "第一行标题_x000D__x000D__x000D_\n \t\n第二行标题",
                "SL-2026-003\r\n\r\n修订版",
                "第一申办方\r\r\r\n 第二申办方",
                "I期\n\t\n扩展研究",
                "第一条标准_x000D_\n\n第二条标准\u0002补充",
            ]);

        Assert.Equal("第一行标题\n第二行标题", trial.Title);
        Assert.Equal("SL-2026-003\n修订版", trial.ProtocolNumber);
        Assert.Equal("第一申办方\n第二申办方", trial.Sponsor);
        Assert.Equal("I期\n扩展研究", trial.ClinicalPhase);
        Assert.Equal("第一条标准\n第二条标准 补充", trial.InclusionCriteria);
    }

    [Theory]
    [InlineData("冠心病项目", "呼吸系统疾病", "冠心病", "循环系统疾病")]
    [InlineData("匹伐他汀钙片研究", "消化系统疾病", "高胆固醇血症", "代谢系统与营养疾病")]
    [InlineData("阴道炎治疗研究", "消化系统疾病", "细菌性阴道病", "生殖系统疾病")]
    public void ProjectImportMapping_WithHighConfidenceDiseaseConflict_NormalizesDiseaseArea(
        string title,
        string sourceDiseaseArea,
        string indication,
        string expectedDiseaseArea)
    {
        var trial = BuildTrialFromImportInput(new ImportSaveLifeProjectRowInput
        {
            Title = title,
            DiseaseArea = sourceDiseaseArea,
            Indication = indication,
        });

        Assert.Equal(expectedDiseaseArea, trial.DiseaseArea);
        Assert.Equal(indication, trial.Indication);
    }

    [Fact]
    public void ProjectImportMapping_WithUnclassifiedIndication_DoesNotCopyIndicationToDiseaseArea()
    {
        var trial = BuildTrialFromImportInput(new ImportSaveLifeProjectRowInput
        {
            Title = "未分类疾病项目",
            Indication = "罕见未分类综合征",
        });

        Assert.Null(trial.DiseaseArea);
        Assert.Equal("罕见未分类综合征", trial.Indication);
    }

    [Fact]
    public async Task ProjectImportMapping_WithPrefaceRowsInXlsx_FindsHeaderRow()
    {
        var file = BuildXlsxFormFile(
            V17TemplateHeaders,
            [
                "CTR20261775",
                "ZPKLP.BE.HJ.ZK",
                "中国健康试验参与者空腹状态下单次口服佐匹克隆胶囊的生物等效性试验",
                "佐匹克隆胶囊",
                "天津华津制药有限公司",
                "生物等效性试验",
                "其他",
                "进行中 尚未招募",
                "32",
                "健康志愿者",
                "失眠症",
                "充分了解试验目的并签署知情同意书",
                "对本品及辅料中过敏者",
                "受试者在试验期间享有隐私保护、免费检查等权益",
                "经河北医科大学第一医院临床研究伦理审查委员会批准",
                "河北医科大学第一医院",
                "于静",
                "河北省",
                "石家庄市",
                "中国",
            ],
            prefaceRows: 2);

        var rows = await ReadProjectRowsFromXlsx(file);
        var trial = BuildTrialFromRow(rows.Single());

        Assert.Equal("CTR20261775", trial.CdeRegistrationNumber);
        Assert.Equal("中国健康试验参与者空腹状态下单次口服佐匹克隆胶囊的生物等效性试验", trial.Title);
    }

    [Fact]
    public async Task ProjectImportMapping_WithLegacyXlsxHeaders_BuildsTrial()
    {
        var file = BuildXlsxFormFile(
            [
                "试验题目",
                "方案编号",
                "CDE登记号",
                "项目描述",
                "申办方",
                "试验阶段",
                "疾病领域",
                "状态",
                "目标人数",
                "参与者类别",
                "入选标准",
                "受试者权益",
                "伦理委员会信息",
                "各参加单位信息",
            ],
            [
                "示例PD-1抑制剂治疗晚期肺癌III期临床试验",
                "SL-PD1-III-001",
                "CTR20260001",
                "多中心、随机、双盲临床研究",
                "示例生物医药有限公司",
                "III期",
                "肿瘤",
                "招募中",
                "200",
                "患者",
                "18-75岁；经病理确诊；签署知情同意书",
                "交通补贴及必要检查费用",
                "示例医院伦理委员会",
                "示例医院 / 张三 / 中国 / 浙江省 / 杭州",
            ]);

        var rows = await ReadProjectRowsFromXlsx(file);
        var trial = BuildTrialFromRow(rows.Single());

        Assert.Equal("CTR20260001", trial.CdeRegistrationNumber);
        Assert.Equal("患者", trial.ParticipantType);
        Assert.Equal(200, trial.TargetCount);
        Assert.Equal("示例医院 / 张三 / 中国 / 浙江省 / 杭州", trial.Sites);
    }

    [Fact]
    public void ProjectImportMapping_WithV17TextAliases_BuildsTrial()
    {
        var trial = BuildTrialFromText(
            """
            CDE登记号：CTR20261775
            方案编号：ZPKLP.BE.HJ.ZK
            试验题目：中国健康试验参与者空腹状态下单次口服佐匹克隆胶囊的生物等效性试验
            申办方：天津华津制药有限公司
            试验阶段：生物等效性试验
            疾病领域：其他
            状态：进行中 尚未招募
            目标例数：32
            参与者类型：健康志愿者
            入选标准：充分了解试验目的并签署知情同意书
            排除标准：对本品及辅料中过敏者
            参与者权益：受试者在试验期间享有隐私保护、免费检查等权益
            伦理委员会：经河北医科大学第一医院临床研究伦理审查委员会批准
            各参加单位信息：河北医科大学第一医院 / 于静 / 中国 / 河北省 / 石家庄市
            城市：石家庄市
            """);

        Assert.Equal("CTR20261775", trial.CdeRegistrationNumber);
        Assert.Equal(32, trial.TargetCount);
        Assert.Equal("健康志愿者", trial.ParticipantType);
        Assert.Equal("受试者在试验期间享有隐私保护、免费检查等权益", trial.Benefits);
        Assert.Contains("河北医科大学第一医院", trial.Sites);
        Assert.Equal("石家庄市", trial.Location);
    }

    [Fact]
    public void ProjectImportTemplate_ExportsV17ClinicalTrialHeaders()
    {
        var headers = typeof(SaveLifeProjectImportTemplateRow)
            .GetProperties()
            .Select(prop => prop.GetCustomAttribute<ExporterHeaderAttribute>())
            .Where(attr => attr is { IsIgnore: false })
            .OrderBy(attr => attr!.ColumnIndex)
            .Select(attr => attr!.DisplayName)
            .ToList();

        Assert.Equal(V17TemplateHeaders.Concat(OptionalTemplateHeaders), headers);
    }

    [Fact]
    public void ProjectImportBlock_WithV17Rows_IncludesRawAndDerivedFields()
    {
        var blockText = BuildProjectBlockFromCells(
            V17TemplateHeaders.ToList(),
            [
                "CTR20261775",
                "ZPKLP.BE.HJ.ZK",
                "中国健康试验参与者空腹状态下单次口服佐匹克隆胶囊的生物等效性试验",
                "佐匹克隆胶囊",
                "天津华津制药有限公司",
                "生物等效性试验",
                "其他",
                "进行中 尚未招募",
                "32",
                "健康志愿者",
                "失眠症",
                "充分了解试验目的并签署知情同意书",
                "对本品及辅料中过敏者",
                "受试者在试验期间享有隐私保护、免费检查等权益",
                "经河北医科大学第一医院临床研究伦理审查委员会批准",
                "河北医科大学第一医院",
                "于静",
                "河北省",
                "石家庄市",
                "中国",
            ]);

        Assert.Contains("药物名称：佐匹克隆胶囊", blockText);
        Assert.Contains("目标例数：32", blockText);
        Assert.Contains("参与者类型：健康志愿者", blockText);
        Assert.Contains("受试者类型：健康志愿者", blockText);
        Assert.Contains("招募要求：参与者类型：健康志愿者；适应症：失眠症", blockText);
        Assert.Contains("地点：石家庄市", blockText);
    }

    [Fact]
    public void ProjectImportSourceText_WithLargeRows_TruncatesBeforeMaxLength()
    {
        var headers = V17TemplateHeaders.ToList();
        var row = V17TemplateHeaders
            .Select(header => header switch
            {
                "CDE登记号" => "CTR20261775",
                "方案编号" => "ZPKLP.BE.HJ.ZK",
                "试验题目" => "中国健康试验参与者空腹状态下单次口服佐匹克隆胶囊的生物等效性试验",
                "入选标准" => new string('甲', 250_000),
                _ => "示例",
            })
            .ToList();

        var sourceText = BuildProjectSourceTextFromRows("large.xlsx", BuildProjectRowFromCells(headers, row));

        Assert.True(sourceText.Length <= 200_000);
        Assert.Contains("内容过长，已截断", sourceText);
    }

    [Fact]
    public void ProjectImportNumberKeys_WithCdeAndProtocol_UsesNormalizedCdeOnly()
    {
        var trial = new SaveLifeClinicalTrial
        {
            CdeRegistrationNumber = " CTR-2026-1775 ",
            ProtocolNumber = "HSK44459-301",
        };

        var keys = BuildProjectNumberKeys(trial);

        Assert.Equal(["cde:ctr20261775"], keys);
    }

    [Fact]
    public void ProjectImportNumberKeys_WithoutNumber_DoesNotUseTitleSponsorOrSites()
    {
        var trial = new SaveLifeClinicalTrial
        {
            Title = "示例PD-1治疗晚期肺癌研究",
            Sponsor = "示例生物医药有限公司",
            Sites = "示例医院",
        };

        var keys = BuildProjectNumberKeys(trial);

        Assert.Empty(keys);
    }

    [Fact]
    public void ProjectImportMatchKeys_WithCdeAndProtocol_PrefersCdeRegistrationNumber()
    {
        var trial = new SaveLifeClinicalTrial
        {
            CdeRegistrationNumber = "CTR20261775",
            ProtocolNumber = "SHARED-PROTOCOL-001",
        };

        Assert.Equal(["cde:ctr20261775"], BuildProjectMatchKeys(trial));
    }

    [Fact]
    public void ProjectImportMatchKeys_WithoutCdeRegistrationNumber_ReturnsNoMatchKey()
    {
        var trial = new SaveLifeClinicalTrial
        {
            ProtocolNumber = "SHARED-PROTOCOL-001",
        };

        Assert.Empty(BuildProjectMatchKeys(trial));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" - ")]
    public void ProjectImportRequiredCde_WithoutUsableValue_IsRejected(string cdeRegistrationNumber)
    {
        var trial = new SaveLifeClinicalTrial
        {
            CdeRegistrationNumber = cdeRegistrationNumber,
        };

        Assert.False(TryPrepareRequiredCdeRegistrationNumber(trial));
    }

    [Fact]
    public void ProjectImportRequiredCde_WithFormattedValue_IsNormalized()
    {
        var trial = new SaveLifeClinicalTrial
        {
            CdeRegistrationNumber = " ctr-2026-1775 ",
        };

        Assert.True(TryPrepareRequiredCdeRegistrationNumber(trial));
        Assert.Equal("CTR20261775", trial.CdeRegistrationNumber);
    }

    [Fact]
    public void ProjectImportMissingCdeSkipMessage_ExplainsThatTheRowWasSkipped()
    {
        var detail = BuildMissingCdeSkipMessage(7, "行");
        var resultMessage = BuildProjectActionResultMessage(
            totalCount: 1,
            insertedCount: 0,
            overwrittenCount: 0,
            skippedCount: 1,
            failedCount: 0,
            details: [detail]);

        Assert.Equal("第 7 行缺少 CDE 登记号，已跳过。", detail);
        Assert.Contains("跳过 1 条", resultMessage, StringComparison.Ordinal);
        Assert.Contains(detail, resultMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectImportHeader_RequiresCdeRegistrationNumberColumn()
    {
        Assert.True(IsProjectImportHeaderRow(["试验题目", "CDE登记号"]));
        Assert.False(IsProjectImportHeaderRow(["试验题目", "方案编号"]));
    }

    [Fact]
    public void AutoImportParser_KeepsRowsWithoutCdeForSkippedResultFeedback()
    {
        const string html = """
            <table>
              <tr><th>CDE登记号</th><th>试验题目</th><th>状态</th></tr>
              <tr><td></td><td>缺少登记号的临床试验项目</td><td>进行中</td></tr>
              <tr><td>CTR20260001</td><td>登记完整的临床试验项目</td><td>进行中</td></tr>
            </table>
            """;

        var trials = ParseClinicalTrialSearchHtml(html);

        Assert.Equal(2, trials.Count);
        Assert.Contains(trials, trial =>
            trial.Title == "缺少登记号的临床试验项目" &&
            string.IsNullOrWhiteSpace(trial.CdeRegistrationNumber));
    }

    [Fact]
    public void ProjectImportOverwrite_PreparesExistingIdAndPreservesOperationalFields()
    {
        var incoming = new SaveLifeClinicalTrial
        {
            Title = "覆盖后的项目",
            CdeRegistrationNumber = "CTR20261775",
            IsPinned = false,
            IsInKnowledgeBase = true,
            KnowledgeBaseError = "旧错误",
            KnowledgeBaseRetryCount = 3,
            KnowledgeBaseLastAttemptTime = new DateTime(2026, 7, 11),
            KnowledgeBaseNextRetryTime = new DateTime(2026, 7, 12),
            SortOrder = 100,
        };

        PrepareProjectOverwrite(incoming, 123);

        Assert.Equal(123, incoming.Id);
        Assert.False(incoming.IsInKnowledgeBase);
        Assert.Null(incoming.KnowledgeBaseError);
        Assert.Equal(0, incoming.KnowledgeBaseRetryCount);
        Assert.Null(incoming.KnowledgeBaseLastAttemptTime);
        Assert.Null(incoming.KnowledgeBaseNextRetryTime);
    }

    [Fact]
    public void ProjectImportNumberMatch_WhenNumbersPointToDifferentProjects_ReturnsEveryMatchedId()
    {
        var numberIndex = new Dictionary<string, HashSet<long>>(StringComparer.OrdinalIgnoreCase)
        {
            ["cde:ctr20261775"] = [101],
            ["protocol:hsk44459301"] = [202],
        };

        var matchedIds = FindProjectIdsByNumberKeys(
            numberIndex,
            ["cde:ctr20261775", "protocol:hsk44459301"]);

        Assert.Equal([101, 202], matchedIds);
    }

    [Fact]
    public void ProjectImportNumberIndex_WhenOverwriteChangesNumber_RemovesOldAssociation()
    {
        var numberIndex = new Dictionary<string, HashSet<long>>(StringComparer.OrdinalIgnoreCase)
        {
            ["cde:ctr20261775"] = [101],
            ["protocol:old001"] = [101],
        };
        var keysByProjectId = new Dictionary<long, HashSet<string>>
        {
            [101] = new(["cde:ctr20261775", "protocol:old001"], StringComparer.OrdinalIgnoreCase),
        };

        ReplaceProjectNumberKeys(
            numberIndex,
            keysByProjectId,
            101,
            ["cde:ctr20261775", "protocol:new001"]);

        Assert.DoesNotContain("protocol:old001", numberIndex.Keys);
        Assert.Equal([101], numberIndex["protocol:new001"]);
    }

    [Theory]
    [InlineData(5, 2, 2, 1)]
    [InlineData(3, 0, 3, 0)]
    public void ProjectImportFailureCount_DoesNotTreatSkippedRowsAsFailures(
        int totalCount,
        int successCount,
        int skippedCount,
        int expectedFailedCount)
    {
        Assert.Equal(
            expectedFailedCount,
            CalculateImportFailedCount(totalCount, successCount, skippedCount));
    }

    private static SaveLifeClinicalTrial BuildTrialFromImportInput(ImportSaveLifeProjectRowInput input)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var rowType = serviceType.GetNestedType("ProjectImportRow", BindingFlags.NonPublic)!;
        var fromImportInput = rowType.GetMethod("FromImportInput", BindingFlags.Public | BindingFlags.Static)!;
        var row = fromImportInput.Invoke(null, new object[] { 2, input })!;

        return BuildTrialFromRow(row);
    }

    private static SaveLifeClinicalTrial BuildTrialFromCells(List<string> headers, List<string> cells)
    {
        return BuildTrialFromRow(BuildProjectRowFromCells(headers, cells));
    }

    private static string BuildProjectBlockFromCells(List<string> headers, List<string> cells)
    {
        var row = BuildProjectRowFromCells(headers, cells);
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var buildBlock = serviceType.GetMethod("BuildProjectImportBlock", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (string)buildBlock.Invoke(null, new[] { row })!;
    }

    private static object BuildProjectRowFromCells(List<string> headers, List<string> cells)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var rowType = serviceType.GetNestedType("ProjectImportRow", BindingFlags.NonPublic)!;
        var fromCells = rowType.GetMethod("FromCells", BindingFlags.Public | BindingFlags.Static)!;
        return fromCells.Invoke(null, new object[] { 2, headers, cells })!;
    }

    private static string BuildProjectSourceTextFromRows(string fileName, params object[] rows)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var rowType = serviceType.GetNestedType("ProjectImportRow", BindingFlags.NonPublic)!;
        var listType = typeof(List<>).MakeGenericType(rowType);
        var typedRows = (IList)Activator.CreateInstance(listType)!;
        foreach (var row in rows)
        {
            typedRows.Add(row);
        }

        var buildSourceText = serviceType.GetMethod("BuildProjectFileSourceText", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (string)buildSourceText.Invoke(null, new object[] { fileName, typedRows })!;
    }

    private static List<string> BuildProjectNumberKeys(SaveLifeClinicalTrial trial)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var buildNumberKeys = serviceType.GetMethod("BuildProjectNumberKeys", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(buildNumberKeys);
        return ((IEnumerable<string>)buildNumberKeys.Invoke(null, new object[] { trial })!).ToList();
    }

    private static List<string> BuildProjectMatchKeys(SaveLifeClinicalTrial trial)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var buildMatchKeys = serviceType.GetMethod("BuildProjectMatchKeys", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(buildMatchKeys);
        return ((IEnumerable<string>)buildMatchKeys.Invoke(null, new object[] { trial })!).ToList();
    }

    private static bool TryPrepareRequiredCdeRegistrationNumber(SaveLifeClinicalTrial trial)
    {
        var method = typeof(SaveLifeProjectImportRecordService).GetMethod(
            "TryPrepareRequiredCdeRegistrationNumber",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (bool)method.Invoke(null, new object[] { trial })!;
    }

    private static string BuildMissingCdeSkipMessage(int position, string unit)
    {
        var method = typeof(SaveLifeProjectImportRecordService).GetMethod(
            "BuildMissingCdeSkipMessage",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (string)method.Invoke(null, new object[] { position, unit })!;
    }

    private static string BuildProjectActionResultMessage(
        int totalCount,
        int insertedCount,
        int overwrittenCount,
        int skippedCount,
        int failedCount,
        List<string> details)
    {
        var method = typeof(SaveLifeProjectImportRecordService).GetMethod(
            "BuildProjectActionResultMessage",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (string)method.Invoke(null,
        [
            totalCount,
            insertedCount,
            overwrittenCount,
            skippedCount,
            failedCount,
            details,
        ])!;
    }

    private static bool IsProjectImportHeaderRow(IReadOnlyCollection<string> headers)
    {
        var method = typeof(SaveLifeProjectImportRecordService).GetMethod(
            "IsProjectImportHeaderRow",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (bool)method.Invoke(null, new object[] { headers })!;
    }

    private static List<SaveLifeClinicalTrial> ParseClinicalTrialSearchHtml(string html)
    {
        var method = typeof(SaveLifeProjectImportRecordService).GetMethod(
            "ParseClinicalTrialSearchHtml",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (List<SaveLifeClinicalTrial>)method.Invoke(null, new object[] { html })!;
    }

    private static void PrepareProjectOverwrite(SaveLifeClinicalTrial trial, long existingId)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var prepareOverwrite = serviceType.GetMethod("PrepareProjectOverwrite", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(prepareOverwrite);
        prepareOverwrite.Invoke(null, new object[] { trial, existingId });
    }

    private static List<long> FindProjectIdsByNumberKeys(
        Dictionary<string, HashSet<long>> numberIndex,
        IEnumerable<string> numberKeys)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var findProjectIds = serviceType.GetMethod("FindProjectIdsByNumberKeys", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(findProjectIds);
        return ((IEnumerable<long>)findProjectIds.Invoke(null, new object[] { numberIndex, numberKeys })!).ToList();
    }

    private static void ReplaceProjectNumberKeys(
        Dictionary<string, HashSet<long>> numberIndex,
        Dictionary<long, HashSet<string>> keysByProjectId,
        long projectId,
        IEnumerable<string> numberKeys)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var replaceProjectNumberKeys = serviceType.GetMethod("ReplaceProjectNumberKeys", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(replaceProjectNumberKeys);
        replaceProjectNumberKeys.Invoke(null, new object[] { numberIndex, keysByProjectId, projectId, numberKeys });
    }

    private static int CalculateImportFailedCount(int totalCount, int successCount, int skippedCount)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var calculateFailedCount = serviceType.GetMethod("CalculateImportFailedCount", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(calculateFailedCount);
        return (int)calculateFailedCount.Invoke(null, new object[] { totalCount, successCount, skippedCount })!;
    }

    private static SaveLifeClinicalTrial BuildTrialFromRow(object row)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var buildProject = serviceType.GetMethod("BuildProjectFromImportRow", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (SaveLifeClinicalTrial)buildProject.Invoke(null, new[] { row })!;
    }

    private static SaveLifeClinicalTrial BuildTrialFromText(string text)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var parseBlock = serviceType.GetMethod("ParseTextBlock", BindingFlags.NonPublic | BindingFlags.Static)!;
        var block = parseBlock.Invoke(null, new object[] { text })!;

        var buildProject = serviceType.GetMethod("BuildProject", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (SaveLifeClinicalTrial)buildProject.Invoke(null, new[] { block })!;
    }

    private static FormFile BuildXlsxFormFile(
        IReadOnlyList<string> headers,
        IReadOnlyList<string> row,
        int prefaceRows = 0)
    {
        var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");
            for (var index = 1; index <= prefaceRows; index++)
            {
                worksheet.Cells[index, 1].Value = $"说明行 {index}";
            }

            var headerRow = prefaceRows + 1;
            var dataRow = prefaceRows + 2;
            for (var index = 0; index < headers.Count; index++)
            {
                worksheet.Cells[headerRow, index + 1].Value = headers[index];
            }

            for (var index = 0; index < row.Count; index++)
            {
                worksheet.Cells[dataRow, index + 1].Value = row[index];
            }

            package.Save();
        }

        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "file", "trial.xlsx");
    }

    private static async Task<List<object>> ReadProjectRowsFromXlsx(IFormFile file)
    {
        var serviceType = typeof(SaveLifeProjectImportRecordService);
        var readXlsx = serviceType.GetMethod("ReadProjectRowsFromXlsx", BindingFlags.NonPublic | BindingFlags.Static)!;
        var task = (Task)readXlsx.Invoke(null, new object[] { file })!;
        await task;

        var result = (IEnumerable)task.GetType().GetProperty("Result")!.GetValue(task)!;
        return result.Cast<object>().ToList();
    }
}
