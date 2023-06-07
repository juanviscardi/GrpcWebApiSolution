using Grpc.Core;
using GrpcMainServer.ServerProgram;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcMainServer { 
    public class AdminService : Admin.AdminBase
    {
        public override Task<MessageReply> PostMecanico(MecanicoDTO request, ServerCallContext context)
        {
            BusinessLogic session = BusinessLogic.GetInstance();
            Console.WriteLine("Antes de crear el usuario con nombre {0}",request.Name);
            string message = session.CreateUser(request.Name);
            return Task.FromResult(new MessageReply { Message = message });
        }

    }
}