using System;
using System.Threading;

namespace TaskParallel
{
	class Program
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.Name = "Main";

			Console.WriteLine("--- new Task.Start ---");
			TaskStart.Run();

			Console.WriteLine("--- Task.Run ---");
			TaskRun.Run();

			Console.WriteLine("--- Task.Factory.StartNew ---");
			DefaultTaskFactory.Run(10);

			Console.WriteLine("--- Task<TResult>.Factory.StartNew ---");
			DefaultTaskFactoryWithResult.Run(1, 100, 1000);

			Console.WriteLine("--- Task.ContinueWith ---");
			TaskContinue.Run(100);

			Console.WriteLine("--- Detached inner task ---");
			DetachedTask.Run();

			Console.WriteLine("--- Attached inner task ---");
			AttachedTask.Run(10);

			Console.WriteLine("--- Task.WaitAll ---");
			TaskWaitAll.Run(3);

			Console.WriteLine("--- Cancel a task ---");
			CancelTask.Run();

			Console.WriteLine("--- AsyncLocal ---");
			AsyncLocalData.Run();
		}
	}
}
