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
        public override async Task<MessageReply> PostRepuesto(RepuestoDTO request, ServerCallContext context)
        {
            BusinessLogic session = BusinessLogic.GetInstance();
            //Console.WriteLine("Antes de crear el usuario con nombre {0}",request.Name);
            string message = await session.CreateRepuestoAsync(request.Name, request.Proveedor, request.Marca);
            return new MessageReply { Message = message };
        }

        public override Task<MessageReply> GetRepuesto(Id request, ServerCallContext context)
        {
            //BusinessLogic session = BusinessLogic.GetInstance();
            //Console.WriteLine("Antes de crear el usuario con nombre {0}", request.Name);
            //string message = session.CreateUser(request.Name);
            return Task.FromResult(new MessageReply { Message = "test get" });
        }

        public override Task<MessageReply> PutRepuesto(RepuestoDTO request, ServerCallContext context)
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

        //public override Task<MessageReply> GetRepuestos(_, ServerCallContext context)
        //{
        //    BusinessLogic session = BusinessLogic.GetInstance();
        //    Console.WriteLine("Antes de crear el usuario con nombre {0}", request.Name);
        //    string message = session.CreateUser(request.Name);
        //    return Task.FromResult(new MessageReply { Message = message });
        //}

    }
}