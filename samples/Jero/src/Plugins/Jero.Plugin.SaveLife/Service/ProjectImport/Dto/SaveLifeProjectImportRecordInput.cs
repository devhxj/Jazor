


using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeProjectImportRecordInput : BasePageInput
{
    public string? ImportType { get; set; }

    public string? InputMode { get; set; }

    public string? Status { get; set; }

    public long? ProjectId { get; set; }
}

public class SaveLifeProjectImportRecordBaseInput
{
    public virtual string? ImportType { get; set; }

    public virtual string? InputMode { get; set; }

    public virtual string Status { get; set; } = "pending";

    public virtual long? ProjectId { get; set; }

    public virtual string? ProjectTitle { get; set; }

    public virtual string? SourceText { get; set; }

    public virtual string? ResultMessage { get; set; }

    public virtual int TotalCount { get; set; }

    public virtual int SuccessCount { get; set; }

    public virtual int FailedCount { get; set; }
}

public class AddSaveLifeProjectImportRecordInput : SaveLifeProjectImportRecordBaseInput
{
}

public class UpdateSaveLifeProjectImportRecordInput : SaveLifeProjectImportRecordBaseInput
{
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}

public class DeleteSaveLifeProjectImportRecordInput : BaseIdInput
{
}

public class QueryByIdSaveLifeProjectImportRecordInput : DeleteSaveLifeProjectImportRecordInput
{
}

public class ImportSaveLifeProjectTextInput
{
    [Required(ErrorMessage = "导入内容不能为空")]
    public string SourceText { get; set; } = string.Empty;

    public string? InputMode { get; set; } = "text";
}

public class ImportSaveLifeProjectFileInput
{
    [Required(ErrorMessage = "导入文件不能为空")]
    public IFormFile File { get; set; } = default!;

    public bool OverwriteExisting { get; set; } = false;
}

[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportSaveLifeProjectRowInput : BaseImportInput
{
    [ImporterHeader(Name = "试验题目")]
    [ExporterHeader("试验题目", ColumnIndex = 3, Format = "", Width = 42, IsBold = true)]
    public string? Title { get; set; }

    [ImporterHeader(Name = "药物名称")]
    [ExporterHeader("药物名称", ColumnIndex = 4, Format = "", Width = 24, IsBold = true)]
    public string? DrugName { get; set; }

    [ImporterHeader(Name = "方案编号")]
    [ExporterHeader("方案编号", ColumnIndex = 2, Format = "", Width = 22, IsBold = true)]
    public string? ProtocolNumber { get; set; }

    [ImporterHeader(Name = "CDE登记号")]
    [ExporterHeader("CDE登记号", ColumnIndex = 1, Format = "", Width = 20, IsBold = true)]
    public string? CdeRegistrationNumber { get; set; }

    [ImporterHeader(Name = "项目描述")]
    [ExporterHeader("项目描述", ColumnIndex = 21, Format = "", Width = 36, IsBold = true)]
    public string? Description { get; set; }

    [ImporterHeader(Name = "申办方")]
    [ExporterHeader("申办方", ColumnIndex = 5, Format = "", Width = 28, IsBold = true)]
    public string? Sponsor { get; set; }

    [ImporterHeader(Name = "试验阶段")]
    [ExporterHeader("试验阶段", ColumnIndex = 6, Format = "", Width = 16, IsBold = true)]
    public string? ClinicalPhase { get; set; }

    [ImporterHeader(Name = "疾病领域")]
    [ExporterHeader("疾病领域", ColumnIndex = 7, Format = "", Width = 18, IsBold = true)]
    public string? DiseaseArea { get; set; }

    [ImporterHeader(Name = "状态")]
    [ExporterHeader("状态", ColumnIndex = 8, Format = "", Width = 14, IsBold = true)]
    public string? Status { get; set; }

    [ImporterHeader(Name = "目标人数")]
    [ExporterHeader(IsIgnore = true)]
    public string? TargetCount { get; set; }

    [ImporterHeader(Name = "目标例数")]
    [ExporterHeader("目标例数", ColumnIndex = 9, Format = "", Width = 14, IsBold = true)]
    public string? TargetCaseCount { get; set; }

    [ImporterHeader(Name = "已招募人数")]
    [ExporterHeader("已招募人数", ColumnIndex = 22, Format = "", Width = 14, IsBold = true)]
    public string? EnrolledCount { get; set; }

    [ImporterHeader(Name = "开始日期")]
    [ExporterHeader("开始日期", ColumnIndex = 23, Format = "yyyy-mm-dd", Width = 16, IsBold = true)]
    public string? StartDate { get; set; }

    [ImporterHeader(Name = "结束日期")]
    [ExporterHeader("结束日期", ColumnIndex = 24, Format = "yyyy-mm-dd", Width = 16, IsBold = true)]
    public string? EndDate { get; set; }

    [ImporterHeader(Name = "参与者类别")]
    [ExporterHeader(IsIgnore = true)]
    public string? ParticipantType { get; set; }

    [ImporterHeader(Name = "参与者类型")]
    [ExporterHeader("参与者类型", ColumnIndex = 10, Format = "", Width = 18, IsBold = true)]
    public string? ParticipantTypeText { get; set; }

    [ImporterHeader(Name = "适应症")]
    [ExporterHeader("适应症", ColumnIndex = 11, Format = "", Width = 28, IsBold = true)]
    public string? Indication { get; set; }

    [ImporterHeader(Name = "入选标准")]
    [ExporterHeader("入选标准", ColumnIndex = 12, Format = "", Width = 36, IsBold = true)]
    public string? InclusionCriteria { get; set; }

    [ImporterHeader(Name = "排除标准")]
    [ExporterHeader("排除标准", ColumnIndex = 13, Format = "", Width = 36, IsBold = true)]
    public string? ExclusionCriteria { get; set; }

    [ImporterHeader(Name = "试验周期")]
    [ExporterHeader("试验周期", ColumnIndex = 25, Format = "", Width = 18, IsBold = true)]
    public string? TrialDuration { get; set; }

    [ImporterHeader(Name = "受试者权益")]
    [ExporterHeader(IsIgnore = true)]
    public string? Benefits { get; set; }

    [ImporterHeader(Name = "参与者权益")]
    [ExporterHeader("参与者权益", ColumnIndex = 14, Format = "", Width = 28, IsBold = true)]
    public string? ParticipantBenefits { get; set; }

    [ImporterHeader(Name = "伦理委员会信息")]
    [ExporterHeader(IsIgnore = true)]
    public string? EthicsCommitteeInfo { get; set; }

    [ImporterHeader(Name = "伦理委员会")]
    [ExporterHeader("伦理委员会", ColumnIndex = 15, Format = "", Width = 30, IsBold = true)]
    public string? EthicsCommittee { get; set; }

    [ImporterHeader(Name = "各参加单位信息")]
    [ExporterHeader("各参加单位信息", ColumnIndex = 26, Format = "", Width = 44, IsBold = true)]
    public string? Sites { get; set; }

    [ImporterHeader(Name = "机构名称")]
    [ExporterHeader("机构名称", ColumnIndex = 16, Format = "", Width = 44, IsBold = true)]
    public string? InstitutionNames { get; set; }

    [ImporterHeader(Name = "主要研究者")]
    [ExporterHeader("主要研究者", ColumnIndex = 17, Format = "", Width = 30, IsBold = true)]
    public string? PrincipalInvestigators { get; set; }

    [ImporterHeader(Name = "省")]
    [ExporterHeader("省", ColumnIndex = 18, Format = "", Width = 24, IsBold = true)]
    public string? Provinces { get; set; }

    [ImporterHeader(Name = "城市")]
    [ExporterHeader("城市", ColumnIndex = 19, Format = "", Width = 24, IsBold = true)]
    public string? Cities { get; set; }

    [ImporterHeader(Name = "国家/地区")]
    [ExporterHeader("国家/地区", ColumnIndex = 20, Format = "", Width = 18, IsBold = true)]
    public string? Countries { get; set; }
}

public class SaveLifeProjectImportTemplateRow : ImportSaveLifeProjectRowInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; } = string.Empty;
}

public class ImportSaveLifeKnowledgeTextInput
{
    [Required(ErrorMessage = "导入内容不能为空")]
    public string SourceText { get; set; } = string.Empty;

    public string? InputMode { get; set; } = "text";
}

public class ImportSaveLifeKnowledgeFileInput
{
    [Required(ErrorMessage = "导入文件不能为空")]
    public IFormFile File { get; set; } = default!;
}

[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportSaveLifeKnowledgeRowInput : BaseImportInput
{
    [ImporterHeader(Name = "标题")]
    [ExporterHeader("标题", ColumnIndex = 1, Format = "", Width = 36, IsBold = true)]
    public string? Title { get; set; }

    [ImporterHeader(Name = "分类名称")]
    [ExporterHeader("分类名称", ColumnIndex = 2, Format = "", Width = 18, IsBold = true)]
    public string? CategoryName { get; set; }

    [ImporterHeader(Name = "疾病名称")]
    [ExporterHeader("疾病名称", ColumnIndex = 3, Format = "", Width = 18, IsBold = true)]
    public string? DiseaseName { get; set; }

    [ImporterHeader(Name = "作者")]
    [ExporterHeader("作者", ColumnIndex = 4, Format = "", Width = 16, IsBold = true)]
    public string? Author { get; set; }

    [ImporterHeader(Name = "标签（逗号分隔）")]
    [ExporterHeader("标签（逗号分隔）", ColumnIndex = 5, Format = "", Width = 28, IsBold = true)]
    public string? Tags { get; set; }

    [ImporterHeader(Name = "封面图URL")]
    [ExporterHeader("封面图URL", ColumnIndex = 6, Format = "", Width = 36, IsBold = true)]
    public string? ImageUrl { get; set; }

    [ImporterHeader(Name = "摘要")]
    [ExporterHeader("摘要", ColumnIndex = 7, Format = "", Width = 42, IsBold = true)]
    public string? Summary { get; set; }

    [ImporterHeader(Name = "正文内容")]
    [ExporterHeader("正文内容", ColumnIndex = 8, Format = "", Width = 60, IsBold = true)]
    public string? Content { get; set; }
}

public class SaveLifeKnowledgeImportTemplateRow : ImportSaveLifeKnowledgeRowInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; } = string.Empty;
}

public class SaveLifeTextImportResult
{
    public long RecordId { get; set; }

    public int TotalCount { get; set; }

    public int SuccessCount { get; set; }

    public int FailedCount { get; set; }

    public int SkippedCount { get; set; }

    public string ResultMessage { get; set; } = string.Empty;
}

public class AutoImportSaveLifeProjectInput
{
    public string? Keyword { get; set; }

    public string? Status { get; set; } = "进行中";

    [Range(1, 10, ErrorMessage = "页码范围为 1-10")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "每页条数范围为 1-50")]
    public int PageSize { get; set; } = 20;

    [Range(1, 100, ErrorMessage = "单次导入上限范围为 1-100")]
    public int ImportLimit { get; set; } = 20;
}
