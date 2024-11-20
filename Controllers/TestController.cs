using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("/api/test")]
    public IActionResult Test()
    {
        return Ok(new { Message = "Kind reagards!" });
    }
}

