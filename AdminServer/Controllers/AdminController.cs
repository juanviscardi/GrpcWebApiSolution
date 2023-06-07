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

        private Admin.AdminClient client;
        private string grpcURL;
       

        static readonly ISettingsManager SettingsMgr = new SettingsManager();
        public AdminController()
        {
           

            AppContext.SetSwitch(
                  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            grpcURL = SettingsMgr.ReadSetting(ServerConfig.GrpcURL);
            grpcURL = "https://localhost:7157";
        }

        [HttpPost("mecanicos")]
        public async Task<ActionResult> PostUser([FromBody] MecanicoDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new Admin.AdminClient(channel);
            var reply = await client.PostMecanicoAsync(user);
            return Ok(reply.Message);
        }
    }
}
