namespace advisor
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [AllowAnonymous]
    [Route("system")]
    public class SystemController : ControllerBase {
        [HttpGet("ping")]
        public IActionResult Ping() {
            return Ok("pong");
        }

        [HttpGet("headers")]
        public IActionResult Headers() {
            return new JsonResult(Request.Headers);
        }
    }
}
