using System;
using System.Collections.Generic;
using System.Threading;


namespace ThreadDataLab
{
	class Program
	{
		private const int NofThreads = 3;
		private const string TlsName = "tls";


		static void Main(string[] args)
		{
			Thread.AllocateNamedDataSlot(TlsName);

			var threadList = new List<Thread>();

			for (int i = 0; i < NofThreads; i++)
			{
				var newThread = new Thread(ThreadData.ThreadStaticDemo);
				threadList.Add(newThread);
				newThread.Start(i);
			}

			foreach (var thr in threadList)
			{
				thr.Join();
			}

			Thread.FreeNamedDataSlot(TlsName);
			Console.WriteLine("TLS deallocated");
		}
	}


	class ThreadData
	{
		[ThreadStatic]
		static int threadSpecificData;


		[ThreadStatic]
		static int mParameter;


		public static void ThreadStaticDemo(object data)
		{
			// Store the managed thread id for each thread in the static
			// variable.
			threadSpecificData = Thread.CurrentThread.ManagedThreadId;
			mParameter = (int) data;

			Thread.SetData(Thread.GetNamedDataSlot("tls"), data);

			// Allow other threads time to execute the same code, to show
			// that the static data is unique to each thread.
			Thread.Sleep(1000);

			// Display the static data.
			Console.WriteLine("Thread #{0}: thread ID now {1}, thread ID in static {2}, data {3}",
				mParameter, Thread.CurrentThread.ManagedThreadId, threadSpecificData,
				Thread.GetData(Thread.GetNamedDataSlot("tls")));
		}
	}
}
