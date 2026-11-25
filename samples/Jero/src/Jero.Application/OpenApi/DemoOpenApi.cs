namespace Jero.Application;

/// <summary>
/// 示例开放接口
/// </summary>
[ApiDescriptionSettings("开放接口", Name = "Demo", Order = 100)]
[Authorize(AuthenticationSchemes = SignatureAuthenticationDefaults.AuthenticationScheme)]
public class DemoOpenApi : IDynamicApiController
{
    private readonly UserManager _userManager;

    public DemoOpenApi(UserManager userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("helloWord")]
    public Task<string> HelloWord()
    {
        return Task.FromResult($"Hello word. {_userManager.Account}");
    }
}