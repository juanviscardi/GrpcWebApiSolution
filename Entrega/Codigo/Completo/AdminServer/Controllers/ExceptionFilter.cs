using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AdminServer.Controllers
{
    public class ExceptionFilter : Attribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            int statusCode;

            string errorMessage = context.Exception.Message;

            if (context.Exception is Grpc.Core.RpcException e)
            {
                if (e.Status.StatusCode == StatusCode.AlreadyExists) { statusCode = 400; } else { statusCode = 500; }
                errorMessage = e.Status.Detail;
            }
            else
            {
                statusCode = 500;
            }

            MessageReply message = new MessageReply() { Message = errorMessage };
            context.Result = new ObjectResult(message)
            {
                StatusCode = statusCode,
            };
        }
    }
}
