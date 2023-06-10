using Grpc.Core;
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
            if (repuesto == null)
            {
                RepuestoResponse notFoundResponse = new RepuestoResponse
                {
                    Found = false
                };
                return Task.FromResult(notFoundResponse);
            }
            RepuestoResponse repuestoResponse = new RepuestoResponse
            {
                Found = true,
                Id = repuesto.Id,
                Name = repuesto.Name,
                Proveedor = repuesto.Proveedor,
                Marca = repuesto.Marca
            };
            return Task.FromResult(repuestoResponse);
        }

        public override async Task<MessageReply> PutRepuesto(RepuestoDTO request, ServerCallContext context)
        {
            BusinessLogic session = BusinessLogic.GetInstance();
            string repuesta = await session.PutRepuestoAsync(request);
            return new MessageReply { Message = repuesta };
        }

        public override async Task<MessageReply> DeleteRepuesto(Id request, ServerCallContext context)
        {
            BusinessLogic session = BusinessLogic.GetInstance();
            string repuesta = await session.DeleteRepuestoAsync(request.Id_.ToString());
            return new MessageReply { Message = repuesta };
        }

        public override Task<ListRepuestos> GetRepuestos(Empty request, ServerCallContext context)
        {
            BusinessLogic session = BusinessLogic.GetInstance();
            List<Common.Repuesto> repuestos = session.GetRepuestos();

            ListRepuestos response = new ListRepuestos();
            response.Repuestos.AddRange(repuestos.Select(repuesto => new RepuestoDTO
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