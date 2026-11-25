


using Furion.SpecificationDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Jero.Web.Controllers;

[AllowAnonymous]
[ApiController]
public class SwaggerResourcesController : ControllerBase
{
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
