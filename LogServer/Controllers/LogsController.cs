using System;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using LogServer.LogProgram;
using Microsoft.AspNetCore.Mvc;

namespace LogServer.Controllers
{
    [Route("logs")]
    [ApiController]
    public class LogsController : ControllerBase
    {

        static readonly ISettingsManager settingsMng = new SettingsManager();
        public LogsController() { }

        [HttpGet]
        public async Task<ActionResult<List<Log>>> GetLogs(
            [FromQuery] string? contains = null,
            [FromQuery] string? from = null,
            [FromQuery] string? until = null,
            [FromQuery] string? status = null,
            [FromQuery] string? action = null,
            [FromQuery] string? userName = null)
        {
            BusinessLogic session = BusinessLogic.GetInstance();
            List<Log> logs = session.GetLogs(contains, from, until, status, action, userName);
            return Ok(logs);
        }
    }
}
