using System.Net;
using System.Net.Sockets;

namespace TestSMSThrottleService.Services
{
    public class ClientEmulatorService
    {
        TcpClient tcpClient = new TcpClient();

        byte[] data = new byte[10];

        public ClientEmulatorService()
        {
            string s = (new Random().Next()).ToString();
            System.Text.Encoding.Default.GetBytes(s, data);
        }

        public async void RunUsingThreadsViaTCP()
        {
            await Task.Run(() => tcpClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345)));
            Thread thread = new Thread(() =>
            {
                int cnt = 0;
                while (true)
                {
                    tcpClient.GetStream().Write(data, 0, data.Length);
                    tcpClient.GetStream().Read(data, 0, 1);
                    if (cnt++ % 65536 == 0)
                        Console.WriteLine((char)data[0]);
                }
            });

            thread.Start();
        }

        public async void RunUsingTasksViaTCP()
        {
            await Task.Run(() => tcpClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345)));
            int cnt = 0;
            while(true)
            {
                await tcpClient.GetStream().WriteAsync(data, 0, data.Length);
                await tcpClient.GetStream().ReadAsync(data, 0, 1);
                if(cnt++%65536 == 0)
                    Console.WriteLine(data[0]);
            }
        }

        public void RunUsingThreadsViaDirectMethodCall(QuotaService q)
        {
            Thread thread = new Thread(() =>
            {
                string s = (new Random().Next()).ToString();
                int cnt = 0;
                while (true)
                {
                    q.CountAndCheck(s);
                    //if (cnt++ % 65536 == 0)
                    //    Console.WriteLine((char)data[0]);
                }
            });
            thread.Start();
        }
    }
}
