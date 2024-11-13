using System.Drawing;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestSMSThrottleService.Services
{
    public class ClientEmulatorService
    {
        const int NUMBERS_PER_CLIENT = 10;
        TcpClient tcpClient = new TcpClient();

        byte[][] numbersPool = new byte[NUMBERS_PER_CLIENT][];

        public ClientEmulatorService(int clientId)
        {
            for (int i = 0; i < numbersPool.Length; i++)
            {
                numbersPool[i] = new byte[10];
//                string s = (new Random().Next()).ToString();
                string s = (clientId * 100 + i).ToString();
                s = s.Length<=10?s:s.Substring(0,10);
                System.Text.Encoding.Default.GetBytes(s, numbersPool[i]);
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
                    byte[] number = numbersPool[new Random().Next() % numbersPool.Length];
                    byte[] r = new byte[1];
                    tcpClient.GetStream().Write(number, 0, number.Length);
                    tcpClient.GetStream().Read(r, 0, 1);
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
                byte[] d = numbersPool[new Random().Next() % numbersPool.Length];
                byte[] r = new byte[1];
                await tcpClient.GetStream().WriteAsync(d, 0, d.Length);
                await tcpClient.GetStream().ReadAsync(r, 0, 1);
                if(cnt++%65536 == 0)
                    Console.WriteLine(r[0]);
            }
        }

        public void RunUsingThreadsViaDirectMethodCall(QuotaService q)
        {
            Thread thread = new Thread(() =>
            {
                int cnt = 0;
                string[] nums = new string[NUMBERS_PER_CLIENT];
                for (int i = 0; i < nums.Length; i++)
                {
                    nums[i] = i==0?"888":new Random().Next().ToString();
                }

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
