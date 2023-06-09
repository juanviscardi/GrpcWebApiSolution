using System;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LogServer.Controllers
{
    [Route("logs")]
    [ApiController]
    public class LogsController : ControllerBase
    {

        static readonly ISettingsManager settingsMng = new SettingsManager();
        public LogsController() {}


        [HttpGet]
        public async Task<ActionResult<List<Log>>> GetLogs()
        {
            Log mockLog1 = new Log("Creado con exito", Action.Create);
            Log mockLog2 = new Log("Creado con exito", Action.Create);
            List<Log> logs = new List<Log>();
            logs.Add(mockLog1);
            logs.Add(mockLog2);
            return Ok(logs);
        }
    }
}
