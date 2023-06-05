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

        [HttpPost("users")]
        public async Task<ActionResult> PostUser([FromBody] UserDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new Admin.AdminClient(channel);
            var reply = await client.PostUserAsync(user);
            return Ok(reply.Message);
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser([FromRoute] int id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            client = new Admin.AdminClient(channel);
            var reply = await client.DeleteUserAsync(new Id { Id_ = id });
            return Ok(reply.Message);
        }


    }
}
