using Microsoft.AspNetCore.Mvc;

namespace Alvtime.Adminpanel.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController(IConfiguration configuration) : ControllerBase
{
    [HttpGet("apiURL")]
    public ActionResult<string> GetApiURL()
    {
        return configuration["ApiSettings:BaseUrl"]!;
    }
}