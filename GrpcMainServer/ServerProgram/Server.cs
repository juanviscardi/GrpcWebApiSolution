using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Hosting.Server;



namespace GrpcSGrpcMainServer.ServerProgram
{
    public class Server
    {
        public const int maxClientsInQ = 100;
        public bool acceptingConnections;
        private readonly TcpListener tcpListener;


        public Server(string serverIpAddress, string serverPort)
        {

            var ipEndPoint = new IPEndPoint(
                 IPAddress.Parse(serverIpAddress), int.Parse(serverPort));

             tcpListener = new TcpListener(ipEndPoint);
            acceptingConnections = true;
        }

        public async Task StartReceivingConnections()
        {

            Console.WriteLine("######### Server Tcp Iniciado y aceptando conexiones ###########");
            tcpListener.Start(100);

            while (acceptingConnections)
            {
                try
                {
                    var tcpClientSocket = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    var task = Task.Run(async () => await HandleClient(tcpClientSocket).ConfigureAwait(false));
                }
                catch (SocketException)
                {
                    Console.WriteLine("Server no longer accepts requests");
                    acceptingConnections = false;
                }

            }


        }

        private static async Task HandleClient(TcpClient tcpClientSocket)
        {
            var isClientConnected = true;
            try
            {
                using (var networkStream = tcpClientSocket.GetStream())
                {
                    while (isClientConnected)
                    {
                        var dataLength = new byte[ProtocolSpecification.fixedLength];
                        int totalReceived = 0;
                        while (totalReceived < ProtocolSpecification.fixedLength)
                        {
                            var received = await networkStream.ReadAsync(dataLength, totalReceived, ProtocolSpecification.fixedLength - totalReceived).ConfigureAwait(false);
                            if (received == 0)
                            {
                                throw new SocketException();
                            }
                            totalReceived += received;
                        }
                        var length = BitConverter.ToInt32(dataLength, 0);
                        var data = new byte[length];
                        totalReceived = 0;
                        while (totalReceived < length)
                        {
                            int received = await networkStream.ReadAsync(data, totalReceived, length - totalReceived).ConfigureAwait(false);
                            if (received == 0)
                            {
                                throw new SocketException();
                            }
                            totalReceived += received;
                        }
                        var word = Encoding.UTF8.GetString(data);
                        if (word.Equals("exit"))
                        {
                            isClientConnected = false;
                            Console.WriteLine("Client is leaving");
                        }
                        else
                        {
                            Console.WriteLine("Client says: " + word);
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"The client connection was interrupted - Exception {e.Message}");
            }
        }


    }
}
