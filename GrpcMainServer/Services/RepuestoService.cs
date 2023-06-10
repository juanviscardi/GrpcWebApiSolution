﻿using Grpc.Core;
using GrpcMainServer.ServerProgram;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcMainServer { 
    public class RepuestoService : Repuesto.RepuestoBase
    {
        public override async Task<MessageReply> PostRepuesto(RepuestoRequest request, ServerCallContext context)
        {
            BusinessLogic session = BusinessLogic.GetInstance();
            //Console.WriteLine("Antes de crear el usuario con nombre {0}",request.Name);
            string message = await session.CreateRepuestoAsync(request.Name, request.Proveedor, request.Marca);
            return new MessageReply { Message = message };
        }

        public override Task<RepuestoResponse> GetRepuesto(Id request, ServerCallContext context)
        {
            BusinessLogic session = BusinessLogic.GetInstance();
            Common.Repuesto repuesto = session.GetRepuestoById(request.Id_.ToString());
            RepuestoResponse repuestoResponse = new RepuestoResponse
            {
                Id = repuesto.Id,
                Name = repuesto.Name,
                Proveedor = repuesto.Proveedor,
                Marca = repuesto.Marca
            };
            return Task.FromResult(repuestoResponse);
        }

        public override Task<MessageReply> PutRepuesto(RepuestoRequest request, ServerCallContext context)
        {
            //BusinessLogic session = BusinessLogic.GetInstance();
            //Console.WriteLine("Antes de crear el usuario con nombre {0}", request.Name);
            //string message = session.CreateUser(request.Name);
            return Task.FromResult(new MessageReply { Message = "test put" });
        }

        public override Task<MessageReply> DeleteRepuesto(Id request, ServerCallContext context)
        {
            //BusinessLogic session = BusinessLogic.GetInstance();
            //Console.WriteLine("Antes de crear el usuario con nombre {0}", request.Name);
            //string message = session.CreateUser(request.Name);
            return Task.FromResult(new MessageReply { Message = "test delete" });
        }

        public override Task<ListRepuestos> GetRepuestos(Empty request, ServerCallContext context)
        {
            BusinessLogic session = BusinessLogic.GetInstance();
            List<Common.Repuesto> repuestos = session.GetRepuestos();

            ListRepuestos response = new ListRepuestos();
            response.Repuestos.AddRange(repuestos.Select(repuesto => new RepuestoResponse
            {
                Id = repuesto.Id,
                Name = repuesto.Name,
                Proveedor = repuesto.Proveedor,
                Marca = repuesto.Marca
            }));

            return Task.FromResult(response);
        }

    }
}