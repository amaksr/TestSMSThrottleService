using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestSMSThrottleService.Services
{
    public class TcpServerService
    {
        private readonly Services.QuotaService _quotaService;

        static TcpListener? tcpServer;

        static List<TcpClient> tcpClients = [];

        public TcpServerService(Services.QuotaService quotaService)
        {
            _quotaService = quotaService;

            if (tcpServer == null)
            {
                tcpServer = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));
                tcpServer.Start();
                Task.Run(ServerListenerTask);
            }
        }

        async void ServerListenerTask()
        {
            if (tcpServer == null)
            {
                Console.WriteLine("TcpServer is down");
                return;
            }
            TcpClient client = await tcpServer.AcceptTcpClientAsync();

            int idx = tcpClients.FindIndex((c) => !c.Connected);
            if(idx >= 0)
                tcpClients[idx] = client;
            else
                tcpClients.Add(client);
            Console.WriteLine("TcpServer client " + tcpClients.Count + " connected");
            _ = Task.Run(() => { TcpConnectionTask(client); });
            _ = Task.Run(ServerListenerTask); // handle next incoming connection
        }

        async void TcpConnectionTask(TcpClient tcpClient)
        {
            NetworkStream stream = tcpClient.GetStream();
//            Console.WriteLine("Waiting for input data");
            byte[] buffer = new byte[1024];
            var size = await stream.ReadAsync(buffer, 0, buffer.Length);
            if(size == 0)
            {
                tcpClient.Close();
                return;
            }
//            Console.WriteLine("Recieved " + size + " bytes");
            string s = System.Text.Encoding.Default.GetString(buffer).TrimEnd((Char)0).ReplaceLineEndings("");
            if (!string.IsNullOrEmpty(s))
            {
//                Console.WriteLine("Recieved string: " + s);
                bool quotaExists = _quotaService.CountAndCheck(s);
                byte[] obuffer = new byte[1];
                System.Text.Encoding.Default.GetBytes(quotaExists?"1":"0", obuffer);
                await stream.WriteAsync(obuffer);
            }
            _ = Task.Run(() => { TcpConnectionTask(tcpClient); });
        }
    }
}
