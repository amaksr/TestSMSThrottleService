using System.Drawing;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestSMSThrottleService.Services
{
    public class ClientEmulatorService
    {
        TcpClient tcpClient = new TcpClient();

        byte[][] data = new byte[128][];

        public ClientEmulatorService()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new byte[10];
                string s = (new Random().Next()).ToString();
                System.Text.Encoding.Default.GetBytes(s, data[i]);
            }
        }

        public async void RunUsingThreadsViaTCP()
        {
            await Task.Run(() => tcpClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345)));
            Thread thread = new Thread(() =>
            {
                int cnt = 0;
                while (true)
                {
                    byte[] d = data[new Random().Next() % data.Length];
                    tcpClient.GetStream().Write(d, 0, d.Length);
                    tcpClient.GetStream().Read(d, 0, 1);
                    if (cnt++ % 65536 == 0)
                        Console.WriteLine((char)d[0]);
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
                byte[] d = data[new Random().Next() % data.Length];
                await tcpClient.GetStream().WriteAsync(d, 0, d.Length);
                await tcpClient.GetStream().ReadAsync(d, 0, 1);
                if(cnt++%65536 == 0)
                    Console.WriteLine(d[0]);
            }
        }

        public void RunUsingThreadsViaDirectMethodCall(QuotaService q)
        {
            Thread thread = new Thread(() =>
            {
                int cnt = 0;
                string[] nums = new string[128];
                for(int i=0; i<nums.Length; i++)
                    nums[i] = new Random().Next().ToString();

                while (true)
                {
                    q.CountAndCheck(nums[new Random().Next() % nums.Length]);
                    //if (cnt++ % 65536 == 0)
                    //    Console.WriteLine((char)data[0]);
                }
            });
            thread.Start();
        }
    }
}
