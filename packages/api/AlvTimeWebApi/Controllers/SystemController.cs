using Microsoft.AspNetCore.Mvc;
using System;

namespace AlvTimeWebApi.Controllers
{
    [ApiController]
    public class SystemController : ControllerBase
    {
        [HttpGet("api/ping")]
        public ActionResult Ping()
        {
            return Ok(DateTime.Now);
        }

        [Obsolete]
        [HttpGet("api/throw")]
        public ActionResult Throw()
        {
            throw new Exception("An error occured. This is for test purpose only");
        }
    }
}
