namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeDiseaseKnowledgeInput : BasePageInput
{
    public long? CategoryId { get; set; }

    public string? ContentCategory { get; set; }

    public string? DiseaseName { get; set; }

    public bool? IsPublished { get; set; }
}

public class SaveLifeDiseaseKnowledgeBaseInput
{
    public virtual long? CategoryId { get; set; }

    public virtual string? ContentCategory { get; set; } = SaveLifeKnowledgeContentCategory.Science;

    [Required(ErrorMessage = "文章标题不能为空")]
    public virtual string Title { get; set; } = string.Empty;

    public virtual string? Content { get; set; }

    public virtual string? Summary { get; set; }

    public virtual string? ImageUrl { get; set; }

    public virtual string? Tags { get; set; }

    public virtual string? DiseaseName { get; set; }

    public virtual int ViewCount { get; set; }

    public virtual int LikeCount { get; set; }

    public virtual string? Author { get; set; }

    public virtual bool IsPublished { get; set; }

    public virtual int SortOrder { get; set; } = 100;
}

public class AddSaveLifeDiseaseKnowledgeInput : SaveLifeDiseaseKnowledgeBaseInput
{
}

public class UpdateSaveLifeDiseaseKnowledgeInput : SaveLifeDiseaseKnowledgeBaseInput
{
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}

public class DeleteSaveLifeDiseaseKnowledgeInput : BaseIdInput
{
}

public class QueryByIdSaveLifeDiseaseKnowledgeInput : DeleteSaveLifeDiseaseKnowledgeInput
{
}

public class PublishSaveLifeDiseaseKnowledgeInput : BaseIdInput
{
    public bool IsPublished { get; set; }
}

public class UpdateSaveLifeDiseaseKnowledgeContentInput : BaseIdInput
{
    public string? Content { get; set; }
}

public class UploadSaveLifeKnowledgeMediaInput
{
    [Required(ErrorMessage = "媒体文件不能为空")]
    public IFormFile File { get; set; } = default!;

    public long? KnowledgeId { get; set; }
}

public class SaveLifeKnowledgeMediaOutput
{
    public string Id { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? FileSuffix { get; set; }

    public string? ContentType { get; set; }

    public string MediaType { get; set; } = "image";

    public long SizeKb { get; set; }

    public string? SizeInfo { get; set; }
}

public class SaveLifeKnowledgeReadHistoryInput : BasePageInput
{
}
