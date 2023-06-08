using Common;
using Common.Interfaces;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace ClientProgram
{
    class Program
    {
        static readonly ISettingsManager SettingsMgr = new SettingsManager();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Client starting...");
            var clientIpEndPoint = new IPEndPoint(
                IPAddress.Parse(SettingsMgr.ReadSetting(ClientConfig.ClientIpConfigKey)),
                int.Parse(SettingsMgr.ReadSetting(ClientConfig.ClientPortConfigKey)));
            var tcpClient = new TcpClient(clientIpEndPoint);
            Console.WriteLine("Trying to connect to server");

            await tcpClient.ConnectAsync(
                IPAddress.Parse(SettingsMgr.ReadSetting(ClientConfig.ServerIpConfigKey)),
                int.Parse(SettingsMgr.ReadSetting(ClientConfig.SeverPortConfigKey))).ConfigureAwait(false);
            var keepConnection = true;

            await using (var networkStream = tcpClient.GetStream())
            {
                while (keepConnection)
                {
                    var word = string.Empty;
                    while (string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word))
                    {
                        Console.WriteLine("Write a message for the server");
                        word = Console.ReadLine();
                    }
                    byte[] data = Encoding.UTF8.GetBytes(word);
                    byte[] dataLength = BitConverter.GetBytes(data.Length);
                    await networkStream.WriteAsync(dataLength, 0, ProtocolSpecification.fixedLength).ConfigureAwait(false);
                    await networkStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                    if (word.Equals("exit"))
                    {
                        keepConnection = false;
                    }
                }
            }

            tcpClient.Close();
        }
    }
}
