


using Furion.SpecificationDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Jero.Web.Controllers;

/// <summary>把 Furion 注册的 OpenAPI 分组映射为 Swagger 资源入口所需的名称和 URL。</summary>
[AllowAnonymous]
[ApiController]
public class SwaggerResourcesController : ControllerBase
{
    // 根地址和 /swagger 下的资源入口复用同一份分组列表。
    [HttpGet("/swagger-resources")]
    [HttpGet("/swagger/swagger-resources")]
    public IActionResult Get()
    {
        var resources = SpecificationDocumentBuilder
            .GetOpenApiGroups()
            .Where(groupInfo => !string.IsNullOrWhiteSpace(groupInfo.RouteTemplate))
            .Select(groupInfo => new
            {
                name = groupInfo.Title,
                url = NormalizeRoute(groupInfo.RouteTemplate)
            });

        return Ok(resources);
    }

    private static string NormalizeRoute(string routeTemplate)
    {
        return routeTemplate.StartsWith('/') ? routeTemplate : $"/{routeTemplate}";
    }
}
