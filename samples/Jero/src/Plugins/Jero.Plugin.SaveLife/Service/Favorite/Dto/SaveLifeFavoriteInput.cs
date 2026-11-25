// Jero 项目的版权、商标、专利和其他相关权利均受相应法律法规和许可证的要求。
namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeFavoriteInput : BasePageInput
{
    public string? ExternalUserId { get; set; }

    public string? ObjectType { get; set; }
}

public class ToggleSaveLifeMiniAppFavoriteInput
{
    [Required(ErrorMessage = "收藏对象类型不能为空")]
    public string ObjectType { get; set; } = string.Empty;

    [Required(ErrorMessage = "收藏对象Id不能为空")]
    public long ObjectId { get; set; }
}

public class SaveLifeMiniAppFavoriteOutput
{
    public string Id { get; set; } = string.Empty;

    public string ObjectType { get; set; } = string.Empty;

    public string ObjectId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Status { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreateTime { get; set; }
}

public class SaveLifeMiniAppFavoriteStateOutput
{
    public string ObjectType { get; set; } = string.Empty;

    public string ObjectId { get; set; } = string.Empty;

    public bool IsFavorite { get; set; }
}
