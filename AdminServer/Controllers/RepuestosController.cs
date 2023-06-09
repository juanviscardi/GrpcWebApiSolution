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
        public async Task<ActionResult> DeleteRepuesto(int id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Repuesto.RepuestoClient client = new Repuesto.RepuestoClient(channel);
            var reply = await client.DeleteRepuestoAsync(new Id { Id_ = id });
            if(reply.Message == "No existe")
            {
                return NotFound();
            }
            return Ok($"Se ha eliminado el repuesto de ID {id}");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetRepuesto(int id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Repuesto.RepuestoClient client = new Repuesto.RepuestoClient(channel);

            RepuestoResponse repuestoResponse = await client.GetRepuestoAsync(new Id { Id_ = id });
            if (!repuestoResponse.Found)
            {
                return NotFound();
            }
            Common.Repuesto repuesto = new Common.Repuesto(
                repuestoResponse.Id,
                repuestoResponse.Name,
                repuestoResponse.Proveedor,
                repuestoResponse.Marca);
            return Ok(repuesto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutRepuesto(int id, RepuestoRequest repuestoRequest)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Repuesto.RepuestoClient client = new Repuesto.RepuestoClient(channel);
            RepuestoDTO repuesto = new RepuestoDTO
            {
                Id = id.ToString(),
                Name = repuestoRequest.Name,
                Proveedor = repuestoRequest.Proveedor,
                Marca = repuestoRequest.Marca
            };
            var reply = await client.PutRepuestoAsync(repuesto);
            if(reply.Message == "No existe")
            {
                return NotFound();
            }
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

        [HttpPatch("{id}")]
        public async Task<ActionResult> EliminarFoto(int id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            Repuesto.RepuestoClient client = new Repuesto.RepuestoClient(channel);
            var reply = await client.PatchRepuestoAsync(new Id { Id_ = id });
            if (reply.Message == "No existe")
            {
                return NotFound();
            }
            return Ok(reply.Message);
        }
    }
}
