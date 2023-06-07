using System;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;

namespace AdminServer.Controllers
{
    [Route("admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private string grpcURL;

        static readonly ISettingsManager settingsMng = new SettingsManager();
        public AdminController()
        {
            AppContext.SetSwitch(
                  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            grpcURL = settingsMng.ReadSetting(ServerConfig.GrpcURL);
        }

        [HttpPost("mecanicos")]
        public async Task<ActionResult> PostUser([FromBody] MecanicoDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Admin.AdminClient client = new Admin.AdminClient(channel);
            var reply = await client.PostMecanicoAsync(user);
            return Ok(reply.Message);
        }
    }
}
