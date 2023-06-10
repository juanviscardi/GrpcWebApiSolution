﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;

namespace AdminServer.Controllers
{
    [Route("repuestos")]
    [ApiController]
    public class RepuestosController : ControllerBase
    {
        private string grpcURL;

        static readonly ISettingsManager settingsMng = new SettingsManager();
        public RepuestosController()
        {
            AppContext.SetSwitch(
                  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            grpcURL = settingsMng.ReadSetting(ServerConfig.GrpcURL);
        }

        [HttpPost]
        public async Task<ActionResult> PostRepuesto(RepuestoRequest repuesto)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Repuesto.RepuestoClient client = new Repuesto.RepuestoClient(channel);
            var reply = await client.PostRepuestoAsync(repuesto);
            return Ok(reply.Message);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRepuesto([FromRoute] Id id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Repuesto.RepuestoClient client = new Repuesto.RepuestoClient(channel);
            var reply = await client.DeleteRepuestoAsync(id);
            return Ok(reply.Message);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetRepuesto(int id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Repuesto.RepuestoClient client = new Repuesto.RepuestoClient(channel);

            RepuestoResponse repuesto = await client.GetRepuestoAsync(new Id{ Id_ = id });
            return Ok(repuesto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutRepuesto([FromRoute] Id id, RepuestoRequest repuesto)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Repuesto.RepuestoClient client = new Repuesto.RepuestoClient(channel);
            var reply = await client.PutRepuestoAsync(repuesto);
            return Ok(reply.Message);
        }

        [HttpGet]
        public async Task<ActionResult> GetRepuestos()
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Repuesto.RepuestoClient client = new Repuesto.RepuestoClient(channel);
            var request = new Empty();
            var reply = await client.GetRepuestosAsync(request);
            return Ok(reply.Repuestos);
        }
    }
}
