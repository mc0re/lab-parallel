using System;
using System.Threading;
using System.Threading.Tasks;

namespace CustomAwaiter
{
    class Program
    {
        private static AutoResetEvent mEvt = new AutoResetEvent(false);


        static void Main(string[] args)
        {
            Console.WriteLine("Sync");
            CustomAwaiter.RunAsync = false;
            MainMethod();

            Console.WriteLine("Async");
            CustomAwaiter.RunAsync = true;
            MainMethod();
        }


        private static void MainMethod()
        {
            Console.WriteLine("Start main method");
            DoWork();
            Console.WriteLine("DoWork returned");
            mEvt.WaitOne();
            Console.WriteLine("Event is triggered, main method exits");
        }

        private static async void DoWork()
        {
            var res = await DoWorkAsync();
            Console.WriteLine($"DoWorkAsync returned {res}");
            mEvt.Set();
        }


        private static CustomAwaitable DoWorkAsync()
        {
            return new CustomAwaitable();
        }
    }
}
