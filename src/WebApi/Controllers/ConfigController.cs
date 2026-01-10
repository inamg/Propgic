using Microsoft.AspNetCore.Mvc;

namespace Propgic.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("google-maps-key")]
    public IActionResult GetGoogleMapsApiKey()
    {
        var apiKey = _configuration["GoogleMaps:ApiKey"] ?? "";
        return Ok(new { apiKey });
    }
}
