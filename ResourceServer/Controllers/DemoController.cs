using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ResourceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {

        [HttpGet("public-data")]
        public IActionResult GetPublicData()
        {
            var publicData = new
            {
                Message = "This is public data accessible without authentication."
            };
            return Ok(publicData);
        }
        [Authorize]
        [HttpGet("protected-data")]
        public IActionResult GetProtectedData()
        {
            var protectedData = new
            {
                Message = "This is protected data accessible only with a valid JWT token."
            };
            return Ok(protectedData);
        }
    }
}
