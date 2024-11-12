using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Text;

namespace TestSMSThrottleService.Services
{
    public class QuotaService
    {
        const int MAX_TOTAL_COUNTER = 10;
        const int MAX_PER_NUM_COUNTER = 5;

        const int COUNTER_RESET_INTERVAL = 5000;

        static int totalCounter = 0;
        static long intervalTotalCounter = 0;
        static int accountCounter = 0;
        static Dictionary<string, int> perNumberCounters = [];

        static Task? resetTask; // This task is to reset counters every second

        public QuotaService()
        {

            //if (resetTask == null)
            //{
            //    Console.WriteLine("Start ResetTask");
            //    resetTask = Task.Run(ResetTask);
            //}
            Thread resetThread = new Thread(() =>
            {
                long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                while (true)
                {
                    Thread.Sleep(10);
                    long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (end - start > COUNTER_RESET_INTERVAL)
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(Status().Value) + ", Resetting counters. " + DateTime.Now.ToString());
                        Monitor.Enter(perNumberCounters);
                        accountCounter = 0;
                        intervalTotalCounter = 0;
                        perNumberCounters.Clear();
                        Monitor.Exit(perNumberCounters);
                        start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }
                }
            } );
            resetThread.Start();
        }

        public async void ResetTask() 
        {
            await Task.Delay(COUNTER_RESET_INTERVAL);
            Console.WriteLine(JsonConvert.SerializeObject(Status().Value) + ", Resetting counters. "+ DateTime.Now.ToString());
            Monitor.Enter(perNumberCounters);
            accountCounter = 0;
            intervalTotalCounter = 0;
            perNumberCounters.Clear();
            Monitor.Exit(perNumberCounters);
            _ = Task.Run(ResetTask);
        }
        public bool CountAndCheck(string number)
        {
            totalCounter++;
            intervalTotalCounter++;
            if (Monitor.TryEnter(perNumberCounters, 1000))
            {
                try
                {
                    if (accountCounter >= MAX_TOTAL_COUNTER)
                        return false;

                    int numCcounter = 0;
                    if (perNumberCounters.ContainsKey(number))
                        numCcounter = perNumberCounters[number];

                    if (numCcounter >= MAX_PER_NUM_COUNTER)
                        return false;

                    accountCounter++;
                    numCcounter++;

                    perNumberCounters[number] = numCcounter;

                    return true;
                }
                finally
                {
                    Monitor.Exit(perNumberCounters);
                }
            }
            else
                return false;
        }

        public JsonResult Status()
        {
            JsonResult res = new(new object[] { intervalTotalCounter*1000/COUNTER_RESET_INTERVAL, accountCounter/*, perNumberCounters*/ });
            return res;
        }
    }
}
