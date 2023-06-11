using LogServer.LogProgram;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace LogServer
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Thread receiverThread = new Thread(Receiver);
            receiverThread.Start();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static void Receiver()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            BusinessLogic businessLogic = new BusinessLogic();
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "logs",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Esta l�nea es necesario para hacer el reparto justo.
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    var body = ea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                    businessLogic.AddLog(message);
                    Console.WriteLine(" [x] Done");
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(queue: "logs",
                    autoAck: false,
                    consumer: consumer);
                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}