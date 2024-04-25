using Microsoft.AspNetCore.Mvc;

namespace CDRSandbox.Controllers;

[ApiController]
[Route("[controller]")]
public class CdrController : ControllerBase
{
    public CdrController(ILogger<CdrController> logger)
    {
    }
    //[HttpGet(Name = "GetWeatherForecast")]

}